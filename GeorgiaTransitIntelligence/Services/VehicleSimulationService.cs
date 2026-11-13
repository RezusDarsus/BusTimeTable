using GeorgiaTransitIntelligence.Data;
using GeorgiaTransitIntelligence.Models;

namespace GeorgiaTransitIntelligence.Services
{
    /// <summary>
    /// Simulates vehicle movements across Georgia based on published timetables.
    /// Fulfills Section 11 of Master Codex: "Timetable-Based Simulation — Estimated Vehicle Positions".
    /// </summary>
    public class VehicleSimulationService
    {
        private readonly MunicipalDataLoader municipalLoader;
        private readonly IntercityDataLoader intercityLoader;
        private readonly BusRouteRepository repository;

        // Simulation clock state
        private DateTime simulatedDateTime;
        private bool isPaused = false;
        private double timeMultiplier = 1.0; // 1x, 5x, 15x, 60x
        private readonly object lockObj = new();

        public DateTime CurrentSimulatedTime
        {
            get { lock (lockObj) return simulatedDateTime; }
            set { lock (lockObj) simulatedDateTime = value; }
        }

        public bool IsPaused
        {
            get { lock (lockObj) return isPaused; }
            set { lock (lockObj) isPaused = value; }
        }

        public double TimeMultiplier
        {
            get { lock (lockObj) return timeMultiplier; }
            set { lock (lockObj) timeMultiplier = Math.Clamp(value, 1.0, 120.0); }
        }

        public VehicleSimulationService(
            MunicipalDataLoader municipalLoader,
            IntercityDataLoader intercityLoader,
            BusRouteRepository repository)
        {
            this.municipalLoader = municipalLoader;
            this.intercityLoader = intercityLoader;
            this.repository = repository;

            // Initialize clock to current time in Georgia (UTC+4)
            simulatedDateTime = DateTime.UtcNow.AddHours(4);
        }

        /// <summary>
        /// Advances the simulation clock by deltaSeconds * timeMultiplier.
        /// </summary>
        public void Tick(double deltaSeconds)
        {
            lock (lockObj)
            {
                if (!isPaused)
                {
                    simulatedDateTime = simulatedDateTime.AddSeconds(deltaSeconds * timeMultiplier);
                }
            }
        }

        public void ResetToRealTime()
        {
            lock (lockObj)
            {
                simulatedDateTime = DateTime.UtcNow.AddHours(4);
                timeMultiplier = 1.0;
                isPaused = false;
            }
        }

        /// <summary>
        /// Generates the list of currently active vehicles based on the simulated time and map zoom level.
        /// Zoom levels:
        /// - zoom &lt; 10: Nationwide (Intercity coaches and marshrutkas only)
        /// - 10 &lt;= zoom &lt; 13: Regional (Intercity + active regional routes)
        /// - zoom &gt;= 13: City level (Kutaisi municipal buses + local intercity)
        /// </summary>
        public List<VehiclePosition> GetActiveVehicles(double zoomLevel, double viewCenterLat, double viewCenterLon)
        {
            DateTime current;
            lock (lockObj) current = simulatedDateTime;

            TimeSpan timeOfDay = current.TimeOfDay;
            string weekday = current.DayOfWeek.ToString(); // Monday, Tuesday...

            List<VehiclePosition> activeVehicles = new();

            // 1. Intercity services simulation (Citybus, Omnibus, Georgian Bus, Marshrutkas)
            // Visible at country and regional zoom, or when near the viewport
            var intercityServices = intercityLoader.Services
                .Where(s => s.DepartureTime.HasValue && s.DurationMinutes.HasValue && s.DurationMinutes.Value > 0)
                .ToList();

            // Also include graded BusRoutes
            var gradedRoutes = repository.Search(string.Empty, "All");
            foreach (var gr in gradedRoutes)
            {
                TimeSpan dep = gr.DepartureTime;
                TimeSpan arr = dep.Add(TimeSpan.FromMinutes(gr.DurationMinutes));

                if (IsActiveInterval(timeOfDay, dep, arr, out double progress))
                {
                    var (fromLat, fromLon) = GeometryService.GetCityCoordinates(gr.Origin);
                    var (toLat, toLon) = GeometryService.GetCityCoordinates(gr.Destination);

                    var corridor = GeometryService.BuildHighwayCorridor(fromLat, fromLon, toLat, toLon);
                    var (curLat, curLon, heading) = GeometryService.InterpolateAlongPath(corridor, progress);

                    string opName = gr.RouteNumber switch
                    {
                        "GEO-101" or "GEO-102" or "GEO-103" or "GEO-106" or "GEO-112" => "Citybus Georgia",
                        "GEO-115" => "Omnibus Express",
                        "GEO-116" => "Georgian Bus",
                        "GEO-104" or "GEO-107" => "Metro Georgia",
                        _ => gr.IsExpress ? "Citybus Georgia" : "Regional Marshrutka"
                    };

                    activeVehicles.Add(new VehiclePosition
                    {
                        VehicleId = $"INTERCITY-G-{gr.Id}",
                        OperatorName = opName,
                        RouteNumber = gr.RouteNumber,
                        Category = gr.IsExpress ? TransportCategory.IntercityCoach : TransportCategory.Marshrutka,
                        Origin = gr.Origin,
                        Destination = gr.Destination,
                        CurrentLatitude = curLat,
                        CurrentLongitude = curLon,
                        HeadingDegrees = heading,
                        ScheduledDeparture = dep,
                        EstimatedArrival = arr,
                        ProgressFraction = progress,
                        NextStopName = progress > 0.8 ? gr.Destination : "Highway Corridor E60",
                        StatusDescription = $"En Route ({progress * 100:F0}%)"
                    });
                }
            }

            foreach (var s in intercityServices)
            {
                TimeSpan dep = s.DepartureTime!.Value;
                int duration = s.DurationMinutes ?? 180;
                TimeSpan arr = dep.Add(TimeSpan.FromMinutes(duration));

                if (IsActiveInterval(timeOfDay, dep, arr, out double progress))
                {
                    var (fromLat, fromLon) = GeometryService.GetCityCoordinates(s.DepartureCity);
                    var (toLat, toLon) = GeometryService.GetCityCoordinates(s.ArrivalCity);

                    var corridor = GeometryService.BuildHighwayCorridor(fromLat, fromLon, toLat, toLon);
                    var (curLat, curLon, heading) = GeometryService.InterpolateAlongPath(corridor, progress);

                    activeVehicles.Add(new VehiclePosition
                    {
                        VehicleId = $"INTERCITY-S-{s.ServiceId}",
                        OperatorName = s.OperatorName,
                        RouteNumber = string.IsNullOrEmpty(s.RouteCode) ? $"{s.DepartureCity[..3]}-{s.ArrivalCity[..3]}" : s.RouteCode,
                        Category = s.Category,
                        Origin = s.DepartureCity,
                        Destination = s.ArrivalCity,
                        CurrentLatitude = curLat,
                        CurrentLongitude = curLon,
                        HeadingDegrees = heading,
                        ScheduledDeparture = dep,
                        EstimatedArrival = arr,
                        ProgressFraction = progress,
                        NextStopName = progress > 0.85 ? s.ArrivalCity : "Intermediate Station",
                        StatusDescription = $"Simulated Progress ({progress * 100:F0}%)"
                    });
                }
            }

            // 2. Kutaisi Municipal Buses Simulation
            // Shown when zoom >= 11 or when viewing Kutaisi area (Lat 42.2 to 42.3, Lon 42.6 to 42.8)
            bool isViewingKutaisi = Math.Abs(viewCenterLat - 42.26) < 0.25 && Math.Abs(viewCenterLon - 42.70) < 0.35;

            if (zoomLevel >= 11.5 || isViewingKutaisi)
            {
                // For each municipal pattern, generate active trips based on timetable or pattern headway
                foreach (var pattern in municipalLoader.Patterns)
                {
                    if (pattern.Stops.Count < 2) continue;

                    // Typical headway for Kutaisi buses: every 15-25 minutes between 07:00 and 21:00
                    int intervalMinutes = (pattern.RouteNumber == "1" || pattern.RouteNumber == "10") ? 15 : 20;
                    int tripDurationMinutes = Math.Max(20, pattern.Stops.Count * 2);

                    TimeSpan serviceStart = TimeSpan.FromHours(7.0);
                    TimeSpan serviceEnd = TimeSpan.FromHours(21.5);

                    if (timeOfDay < serviceStart || timeOfDay > serviceEnd) continue;

                    // Compute trip instances that could be active right now
                    double minutesSinceStart = (timeOfDay - serviceStart).TotalMinutes;
                    int currentTripIndex = (int)(minutesSinceStart / intervalMinutes);

                    for (int offset = -2; offset <= 0; offset++)
                    {
                        int tripIdx = currentTripIndex + offset;
                        if (tripIdx < 0) continue;

                        TimeSpan tripDep = serviceStart.Add(TimeSpan.FromMinutes(tripIdx * intervalMinutes));
                        TimeSpan tripArr = tripDep.Add(TimeSpan.FromMinutes(tripDurationMinutes));

                        if (IsActiveInterval(timeOfDay, tripDep, tripArr, out double progress))
                        {
                            var stopPoints = pattern.Stops.Select(st => (st.Latitude, st.Longitude)).ToList();
                            var (lat, lon, heading) = GeometryService.InterpolateAlongPath(stopPoints, progress);

                            int nextStopIdx = (int)Math.Min(pattern.Stops.Count - 1, Math.Floor(progress * pattern.Stops.Count));
                            string nextStop = pattern.Stops[nextStopIdx].StopName;

                            activeVehicles.Add(new VehiclePosition
                            {
                                VehicleId = $"KUT-BUS-{pattern.RouteNumber}-{pattern.DirectionId}-{tripIdx}",
                                OperatorName = "Kutaisi Municipal Transport",
                                RouteNumber = pattern.RouteNumber,
                                Category = TransportCategory.MunicipalBus,
                                Origin = pattern.StartPoint,
                                Destination = pattern.EndPoint,
                                CurrentLatitude = lat,
                                CurrentLongitude = lon,
                                HeadingDegrees = heading,
                                ScheduledDeparture = tripDep,
                                EstimatedArrival = tripArr,
                                ProgressFraction = progress,
                                NextStopName = nextStop,
                                StatusDescription = $"Approaching {nextStop} (Simulated)"
                            });
                        }
                    }
                }
            }

            return activeVehicles;
        }

        private static bool IsActiveInterval(TimeSpan current, TimeSpan dep, TimeSpan arr, out double progress)
        {
            progress = 0.0;
            double currentMins = current.TotalMinutes;
            double depMins = dep.TotalMinutes;
            double arrMins = arr.TotalMinutes;

            // Handle overnight crossing
            if (arrMins < depMins)
            {
                arrMins += 1440.0;
                if (currentMins < depMins) currentMins += 1440.0;
            }

            if (currentMins >= depMins && currentMins <= arrMins)
            {
                double duration = arrMins - depMins;
                progress = duration > 0 ? (currentMins - depMins) / duration : 0.0;
                progress = Math.Clamp(progress, 0.0, 1.0);
                return true;
            }

            return false;
        }

        private static List<(double Lat, double Lon)> GetCorridorPath(
            double fromLat, double fromLon, double toLat, double toLon)
        {
            return GeometryService.BuildHighwayCorridor(fromLat, fromLon, toLat, toLon);
        }
    }
}
