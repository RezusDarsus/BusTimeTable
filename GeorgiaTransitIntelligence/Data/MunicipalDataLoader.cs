using System.Globalization;
using GeorgiaTransitIntelligence.Models;

namespace GeorgiaTransitIntelligence.Data
{
    /// <summary>
    /// Loads and indexes Kutaisi municipal bus routes, stops, and timetable departures.
    /// Fulfills Section 8 of Master Codex.
    /// </summary>
    public class MunicipalDataLoader
    {
        private readonly List<MunicipalPattern> patterns = new();
        private readonly Dictionary<string, MunicipalStop> uniqueStops = new();
        private readonly Dictionary<(string RouteNumber, int DirectionId), MunicipalPattern> patternLookup = new();
        private readonly Dictionary<string, List<ScheduledDeparture>> stopDeparturesLookup = new(); // Key: StopId_Weekday
        private readonly List<ScheduledDeparture> allDepartures = new();
        private volatile bool isTimetableLoaded = false;
        private readonly object lockObj = new();

        public IReadOnlyList<MunicipalPattern> Patterns => patterns;
        public IReadOnlyDictionary<string, MunicipalStop> UniqueStops => uniqueStops;
        public int TotalStopCount => uniqueStops.Count;
        public int TotalPatternCount => patterns.Count;
        public int TotalTimetableRecordsCount => allDepartures.Count;
        public bool IsTimetableLoaded => isTimetableLoaded;

        public MunicipalDataLoader()
        {
            LoadPatternsAndStops();
        }

        private void LoadPatternsAndStops()
        {
            string patternsPath = DataPathResolver.ResolvePath("data/kutaisi-municipal/route_patterns.csv");
            string stopsPath = DataPathResolver.ResolvePath("data/kutaisi-municipal/route_stops.csv");

            if (!File.Exists(patternsPath) || !File.Exists(stopsPath))
            {
                return;
            }

            // 1. Read route patterns
            using (var reader = new StreamReader(patternsPath))
            {
                string? header = reader.ReadLine();
                while (reader.ReadLine() is { } line)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = SplitCsvLine(line);
                    if (parts.Count < 11) continue;

                    var pattern = new MunicipalPattern
                    {
                        RouteNumber = parts[0].Trim(),
                        RouteId = parts[1].Trim(),
                        RouteName = parts[2].Trim(),
                        PatternSuffix = parts[4].Trim(),
                        DirectionId = int.TryParse(parts[5].Trim(), out int dir) ? dir : 0,
                        DirectionDescription = parts[6].Trim(),
                        StartPoint = parts[7].Trim(),
                        EndPoint = parts[8].Trim(),
                        FirstStopId = parts[9].Trim(),
                        LastStopId = parts[10].Trim(),
                        HeadSign = parts.Count > 11 ? parts[11].Trim() : string.Empty
                    };

                    patterns.Add(pattern);
                    patternLookup[(pattern.RouteNumber, pattern.DirectionId)] = pattern;
                }
            }

            // 2. Read route stops
            using (var reader = new StreamReader(stopsPath))
            {
                string? header = reader.ReadLine();
                while (reader.ReadLine() is { } line)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = SplitCsvLine(line);
                    if (parts.Count < 13) continue;

                    string routeNum = parts[0].Trim();
                    int dirId = int.TryParse(parts[4].Trim(), out int d) ? d : 0;
                    int stopOrder = int.TryParse(parts[6].Trim(), out int ord) ? ord : 0;
                    string stopId = parts[7].Trim();
                    string stopCode = parts[8].Trim();
                    string stopName = parts[9].Trim();
                    double.TryParse(parts[10].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double lat);
                    double.TryParse(parts[11].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double lon);
                    string vehicleMode = parts[12].Trim();

                    var stop = new MunicipalStop
                    {
                        StopId = stopId,
                        StopCode = stopCode,
                        StopName = stopName,
                        Latitude = lat,
                        Longitude = lon,
                        StopOrder = stopOrder,
                        VehicleMode = vehicleMode
                    };

                    if (!uniqueStops.ContainsKey(stopId))
                    {
                        uniqueStops[stopId] = stop;
                    }

                    if (patternLookup.TryGetValue((routeNum, dirId), out var pattern))
                    {
                        pattern.Stops.Add(stop);
                    }
                }
            }

            // Sort stops in each pattern by stop order
            foreach (var p in patterns)
            {
                p.Stops.Sort((a, b) => a.StopOrder.CompareTo(b.StopOrder));
            }
        }

        /// <summary>
        /// Loads the complete Kutaisi municipal timetable departures asynchronously.
        /// Parses the 182,291 departure records across 7 weekdays into indexed memory structures.
        /// </summary>
        public async Task LoadTimetableAsync(IProgress<int>? progress = null)
        {
            if (isTimetableLoaded) return;

            string timetablePath = DataPathResolver.ResolvePath("data/kutaisi-municipal/timetable.csv");
            if (!File.Exists(timetablePath)) return;

            await Task.Run(() =>
            {
                lock (lockObj)
                {
                    if (isTimetableLoaded) return;

                    using var reader = new StreamReader(timetablePath);
                    string? header = reader.ReadLine();
                    int lineCount = 0;

                    while (reader.ReadLine() is { } line)
                    {
                        lineCount++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = SplitCsvLine(line);
                        if (parts.Count < 14) continue;

                        string routeNumber = parts[0].Trim();
                        int dirId = int.TryParse(parts[5].Trim(), out int d) ? d : 0;
                        string stopId = parts[8].Trim();
                        string weekday = parts[11].Trim();
                        string timeStr = parts[13].Trim();

                        if (TimeSpan.TryParse(timeStr, out TimeSpan depTime))
                        {
                            var dep = new ScheduledDeparture
                            {
                                RouteNumber = routeNumber,
                                DirectionId = dirId,
                                StopId = stopId,
                                Weekday = weekday,
                                DepartureTime = depTime
                            };

                            allDepartures.Add(dep);

                            string key = $"{stopId}_{weekday}";
                            if (!stopDeparturesLookup.TryGetValue(key, out var list))
                            {
                                list = new List<ScheduledDeparture>();
                                stopDeparturesLookup[key] = list;
                            }
                            list.Add(dep);
                        }

                        if (lineCount % 25000 == 0)
                        {
                            progress?.Report(lineCount);
                        }
                    }

                    // Sort departures for each stop key
                    foreach (var pair in stopDeparturesLookup)
                    {
                        pair.Value.Sort((a, b) => a.DepartureTime.CompareTo(b.DepartureTime));
                    }

                    isTimetableLoaded = true;
                }
            });
        }

        /// <summary>
        /// Gets next scheduled departures from a given stop on a specific weekday after the specified time.
        /// </summary>
        public List<ScheduledDeparture> GetNextDepartures(string stopId, string weekday, TimeSpan afterTime, int limit = 8)
        {
            if (!isTimetableLoaded) return new List<ScheduledDeparture>();

            string key = $"{stopId}_{weekday}";
            return stopDeparturesLookup.TryGetValue(key, out var list)
                ? list.Where(departure => departure.DepartureTime >= afterTime).Take(limit).ToList()
                : new List<ScheduledDeparture>();
        }

        public MunicipalPattern? GetPattern(string routeNumber, int directionId)
        {
            patternLookup.TryGetValue((routeNumber, directionId), out var p);
            return p;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            int start = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(line.Substring(start, i - start).Trim(' ', '"'));
                    start = i + 1;
                }
            }
            if (start <= line.Length)
            {
                result.Add(line.Substring(start).Trim(' ', '"'));
            }
            return result;
        }
    }
}
