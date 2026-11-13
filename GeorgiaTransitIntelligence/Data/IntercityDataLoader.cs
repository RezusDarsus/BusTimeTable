using System.Text.Json;
using GeorgiaTransitIntelligence.Models;

namespace GeorgiaTransitIntelligence.Data
{
    /// <summary>
    /// Loads and indexes Georgia nationwide intercity transit services, operators, and terminals.
    /// Fulfills Section 9 of Master Codex.
    /// </summary>
    public class IntercityDataLoader
    {
        private readonly List<IntercityService> services = new();
        private readonly List<TransportOperator> operators = new();
        private readonly List<string> destinations = new();
        private readonly Dictionary<string, List<IntercityService>> servicesByOrigin = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<(string, string), List<IntercityService>> servicesByPair = new();

        public IReadOnlyList<IntercityService> Services => services;
        public IReadOnlyList<TransportOperator> Operators => operators;
        public IReadOnlyList<string> Destinations => destinations;

        public IntercityDataLoader()
        {
            LoadIntercityData();
        }

        private void LoadIntercityData()
        {
            string jsonPath = DataPathResolver.ResolvePath("data/georgian-intercity/georgian-intercity-routes.json");
            if (!File.Exists(jsonPath)) return;

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                using JsonDocument doc = JsonDocument.Parse(jsonText);
                var root = doc.RootElement;

                // 1. Parse Operators
                if (root.TryGetProperty("Operators", out var operatorsElem) && operatorsElem.ValueKind == JsonValueKind.Array)
                {
                    int opId = 1;
                    foreach (var op in operatorsElem.EnumerateArray())
                    {
                        string name = op.GetProperty("OperatorName").GetString() ?? string.Empty;
                        string catStr = op.TryGetProperty("OperatorCategory", out var c) ? c.GetString() ?? "" : "";
                        string summary = op.TryGetProperty("ProfileSummary", out var s) ? s.GetString() ?? "" : "";

                        TransportCategory cat = TransportCategory.IntercityCoach;
                        string color = "#7B1FA2"; // Deep purple for intercity

                        if (name.Contains("Citybus", StringComparison.OrdinalIgnoreCase))
                        {
                            color = "#8E24AA";
                            cat = TransportCategory.IntercityCoach;
                        }
                        else if (name.Contains("Omnibus", StringComparison.OrdinalIgnoreCase))
                        {
                            color = "#3949AB";
                            cat = TransportCategory.IntercityCoach;
                        }
                        else if (name.Contains("Georgian Bus", StringComparison.OrdinalIgnoreCase))
                        {
                            color = "#00ACC1";
                            cat = TransportCategory.IntercityCoach;
                        }
                        else if (name.Contains("Marshrutka", StringComparison.OrdinalIgnoreCase) ||
                                 catStr.Contains("minibus", StringComparison.OrdinalIgnoreCase) ||
                                 name.Contains("Sprinter", StringComparison.OrdinalIgnoreCase) ||
                                 name.Contains("Imertrans", StringComparison.OrdinalIgnoreCase))
                        {
                            color = "#FB8C00";
                            cat = TransportCategory.Marshrutka;
                        }

                        operators.Add(new TransportOperator
                        {
                            OperatorId = opId++,
                            Name = name,
                            Category = cat,
                            ColorHex = color,
                            CoverageSummary = summary,
                            IsVerified = !summary.Contains("unverified", StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }

                // 2. Parse Destinations
                if (root.TryGetProperty("Destinations", out var destsElem) && destsElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var d in destsElem.EnumerateArray())
                    {
                        string destName = d.GetProperty("Destination").GetString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(destName) && !destinations.Contains(destName))
                        {
                            destinations.Add(destName);
                        }
                    }
                    destinations.Sort();
                }

                // 3. Parse Services
                if (root.TryGetProperty("Services", out var servicesElem) && servicesElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var s in servicesElem.EnumerateArray())
                    {
                        int id = s.GetProperty("ServiceId").GetInt32();
                        string opName = s.TryGetProperty("OperatorName", out var op) ? op.GetString() ?? "Unspecified Operator" : "Unspecified";
                        string fromCity = s.TryGetProperty("DepartureCity", out var dc) ? dc.GetString() ?? "" : "";
                        string toCity = s.TryGetProperty("ArrivalCity", out var ac) ? ac.GetString() ?? "" : "";
                        string depTimeStr = s.TryGetProperty("DepartureTime", out var dt) ? dt.GetString() ?? "" : "";
                        string arrTimeStr = s.TryGetProperty("ArrivalTime", out var at) ? at.GetString() ?? "" : "";
                        int? duration = s.TryGetProperty("DurationMinutes", out var dm) && dm.ValueKind == JsonValueKind.Number ? dm.GetInt32() : null;
                        decimal? price = s.TryGetProperty("TicketPriceGEL", out var tp) && tp.ValueKind == JsonValueKind.Number ? tp.GetDecimal() : null;
                        string fromStation = s.TryGetProperty("DepartureStation", out var ds) ? ds.GetString() ?? "" : "";
                        string toStation = s.TryGetProperty("ArrivalStation", out var arrs) ? arrs.GetString() ?? "" : "";
                        string evidence = s.TryGetProperty("EvidenceStatus", out var ev) ? ev.GetString() ?? "" : "";
                        string pattern = s.TryGetProperty("SchedulePattern", out var sp) ? sp.GetString() ?? "" : "";
                        string recType = s.TryGetProperty("ScheduleRecordType", out var rt) ? rt.GetString() ?? "" : "";
                        string srcTitle = s.TryGetProperty("SourceTitle", out var st) ? st.GetString() ?? "" : "";
                        string srcUrl = s.TryGetProperty("SourceUrl", out var su) ? su.GetString() ?? "" : "";

                        TimeSpan? depTime = null;
                        if (TimeSpan.TryParse(depTimeStr, out TimeSpan parsedDep))
                        {
                            depTime = parsedDep;
                        }

                        TimeSpan? arrTime = null;
                        if (TimeSpan.TryParse(arrTimeStr, out TimeSpan parsedArr))
                        {
                            arrTime = parsedArr;
                        }

                        TransportCategory category = TransportCategory.IntercityCoach;
                        if (opName.Contains("Marshrutka", StringComparison.OrdinalIgnoreCase) ||
                            opName.Contains("Sprinter", StringComparison.OrdinalIgnoreCase) ||
                            opName.Contains("Imertrans", StringComparison.OrdinalIgnoreCase) ||
                            opName.Contains("Okriba", StringComparison.OrdinalIgnoreCase) ||
                            opName.Contains("Minibus", StringComparison.OrdinalIgnoreCase) ||
                            opName.Contains("Taxi", StringComparison.OrdinalIgnoreCase) ||
                            evidence.Contains("marshrutka", StringComparison.OrdinalIgnoreCase))
                        {
                            category = TransportCategory.Marshrutka;
                        }

                        var service = new IntercityService
                        {
                            ServiceId = id,
                            OperatorName = opName,
                            Category = category,
                            DepartureCity = fromCity,
                            ArrivalCity = toCity,
                            DepartureTime = depTime,
                            ArrivalTime = arrTime,
                            DurationMinutes = duration,
                            TicketPriceGEL = price,
                            DepartureStation = fromStation,
                            ArrivalStation = toStation,
                            SchedulePattern = pattern,
                            ScheduleRecordType = recType,
                            EvidenceStatus = evidence,
                            SourceTitle = srcTitle,
                            SourceUrl = srcUrl
                        };

                        services.Add(service);

                        // Index by Origin
                        if (!servicesByOrigin.TryGetValue(fromCity, out var byOrigList))
                        {
                            byOrigList = new List<IntercityService>();
                            servicesByOrigin[fromCity] = byOrigList;
                        }
                        byOrigList.Add(service);

                        // Index by OD pair
                        var pairKey = (fromCity.Trim().ToLowerInvariant(), toCity.Trim().ToLowerInvariant());
                        if (!servicesByPair.TryGetValue(pairKey, out var byPairList))
                        {
                            byPairList = new List<IntercityService>();
                            servicesByPair[pairKey] = byPairList;
                        }
                        byPairList.Add(service);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load intercity data: {ex.Message}");
            }
        }

        public List<IntercityService> GetServices(string originCity, string destinationCity)
        {
            var key = (originCity.Trim().ToLowerInvariant(), destinationCity.Trim().ToLowerInvariant());
            if (servicesByPair.TryGetValue(key, out var list))
            {
                return list;
            }
            return new List<IntercityService>();
        }

        public List<IntercityService> GetServicesFromOrigin(string originCity)
        {
            if (servicesByOrigin.TryGetValue(originCity, out var list))
            {
                return list;
            }
            return new List<IntercityService>();
        }
    }
}
