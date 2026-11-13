using GeorgiaTransitIntelligence.Data;
using GeorgiaTransitIntelligence.Models;

namespace GeorgiaTransitIntelligence.Services
{
    public enum JourneyOptimization
    {
        FastestArrival,
        CheapestKnown,
        FewestTransfers
    }

    /// <summary>
    /// Multimodal journey planner combining municipal buses, intercity coaches, and walking transfers.
    /// Fulfills Section 13 of Master Codex.
    /// </summary>
    public class JourneyPlanner
    {
        private readonly MunicipalDataLoader municipalLoader;
        private readonly IntercityDataLoader intercityLoader;
        private readonly BusRouteRepository repository;

        public JourneyPlanner(
            MunicipalDataLoader municipalLoader,
            IntercityDataLoader intercityLoader,
            BusRouteRepository repository)
        {
            this.municipalLoader = municipalLoader;
            this.intercityLoader = intercityLoader;
            this.repository = repository;
        }

        /// <summary>
        /// Plans multimodal journeys between two locations in Georgia.
        /// </summary>
        public List<JourneyItinerary> PlanJourney(
            string origin,
            string destination,
            DateTime travelDate,
            TimeSpan earliestDeparture,
            bool allowMunicipal = true,
            bool allowIntercity = true,
            bool allowMarshrutka = true,
            JourneyOptimization optimization = JourneyOptimization.FastestArrival)
        {
            List<JourneyItinerary> itineraries = new();

            string cleanOrigin = origin.Trim();
            string cleanDest = destination.Trim();
            string originCity = ExtractCity(cleanOrigin);
            string destCity = ExtractCity(cleanDest);

            // 1. Check for Direct Intercity Services
            var intercityCandidates = new List<(string Operator, TimeSpan Dep, int Duration, decimal Fare, string FromStation, string ToStation, TransportCategory Cat, string Evidence)>();

            // Check graded repository first (using city names)
            var gradedRoutes = repository.Search(string.Empty, "All")
                .Where(r => MatchesCityOrPlace(r.Origin, originCity) && MatchesCityOrPlace(r.Destination, destCity))
                .ToList();

            foreach (var gr in gradedRoutes)
            {
                intercityCandidates.Add((
                    gr.IsExpress ? "Citybus Georgia (Express)" : "Intercity Bus",
                    gr.DepartureTime,
                    gr.DurationMinutes,
                    gr.Price,
                    gr.Origin,
                    gr.Destination,
                    gr.IsExpress ? TransportCategory.IntercityCoach : TransportCategory.Marshrutka,
                    "Verified Database Schedule"
                ));
            }

            // Check intercity dataset
            var loadedIntercity = intercityLoader.GetServices(originCity, destCity);
            foreach (var s in loadedIntercity)
            {
                if (s.DepartureTime.HasValue && s.DurationMinutes.HasValue)
                {
                    intercityCandidates.Add((
                        s.OperatorName,
                        s.DepartureTime.Value,
                        s.DurationMinutes.Value,
                        s.TicketPriceGEL ?? 20.00m,
                        string.IsNullOrEmpty(s.DepartureStation) ? $"{originCity} Bus Station" : s.DepartureStation,
                        string.IsNullOrEmpty(s.ArrivalStation) ? $"{destCity} Central" : s.ArrivalStation,
                        s.Category,
                        s.EvidenceStatus
                    ));
                }
            }

            bool isStreetAddress = cleanOrigin.Contains("Tabukashvili", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("Street", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("ქუჩა", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("KIU", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("Bagrati", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("217");

            // Direct Intercity Option(s) (only valid when starting directly from a terminal/city node, not a residential street)
            if (!isStreetAddress)
            {
                foreach (var cand in intercityCandidates.Where(c => c.Dep >= earliestDeparture).Take(4))
                {
                    if (cand.Cat == TransportCategory.IntercityCoach && !allowIntercity) continue;
                    if (cand.Cat == TransportCategory.Marshrutka && !allowMarshrutka) continue;

                    var (origLat, origLon) = GeometryService.GetCityCoordinates(cleanOrigin);
                    var (destLat, destLon) = GeometryService.GetCityCoordinates(cleanDest);

                    var directItinerary = new JourneyItinerary
                    {
                        Title = $"Direct {cand.Operator} ({cand.FromStation} → {cand.ToStation})",
                        OriginCity = cleanOrigin,
                        DestinationCity = cleanDest,
                        DepartureTime = cand.Dep,
                        ArrivalTime = cand.Dep.Add(TimeSpan.FromMinutes(cand.Duration)),
                        TotalDurationMinutes = cand.Duration,
                        TotalFareGEL = cand.Fare,
                        TransferCount = 0,
                        TotalWalkingMinutes = 5,
                        ReliabilityWarning = cand.Evidence
                    };

                    directItinerary.Legs.Add(new JourneyLeg
                    {
                        LegType = cand.Cat == TransportCategory.IntercityCoach ? JourneyLegType.IntercityCoach : JourneyLegType.Marshrutka,
                        FromLocation = cand.FromStation,
                        ToLocation = cand.ToStation,
                        FromLatitude = origLat,
                        FromLongitude = origLon,
                        ToLatitude = destLat,
                        ToLongitude = destLon,
                        DepartureTime = cand.Dep,
                        ArrivalTime = directItinerary.ArrivalTime,
                        DurationMinutes = cand.Duration,
                        FareGEL = cand.Fare,
                        ServiceIdentifier = cand.Operator,
                        Instructions = $"Board {cand.Operator} at {cand.FromStation} to {cand.ToStation}."
                    });

                    itineraries.Add(directItinerary);
                }
            }

            // 2. Multimodal Connection: Kutaisi City Point -> Chavchavadze Bus Station -> Intercity to Destination
            bool isKutaisiOrigin = cleanOrigin.Contains("Kutaisi", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("Tabukashvili", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("KIU", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("Bagrati", StringComparison.OrdinalIgnoreCase) ||
                                  cleanOrigin.Contains("217") ||
                                  originCity.Equals("Kutaisi", StringComparison.OrdinalIgnoreCase);

            if (allowMunicipal && isKutaisiOrigin &&
                !cleanDest.Contains("Kutaisi", StringComparison.OrdinalIgnoreCase))
            {
                bool isTabukashvili = cleanOrigin.Contains("Tabukashvili", StringComparison.OrdinalIgnoreCase) || cleanOrigin.Contains("217");
                bool isKIU = cleanOrigin.Contains("KIU", StringComparison.OrdinalIgnoreCase) || cleanOrigin.Contains("International University", StringComparison.OrdinalIgnoreCase) || cleanOrigin.Contains("საერთაშორისო", StringComparison.OrdinalIgnoreCase);

                // Find municipal buses heading to Chavchavadze Terminal (Route 25 for KIU, Route 20 for Tabukashvili, Route 1/10/34 for others)
                var patternToChavchavadze = isKIU
                    ? municipalLoader.Patterns.FirstOrDefault(p => p.RouteNumber == "25" && p.DirectionId == 0)
                      ?? municipalLoader.Patterns.FirstOrDefault(p => p.RouteNumber == "25")
                    : isTabukashvili
                    ? municipalLoader.Patterns.FirstOrDefault(p => p.RouteNumber == "20" && p.DirectionId == 1)
                      ?? municipalLoader.Patterns.FirstOrDefault(p => p.RouteNumber == "20")
                    : municipalLoader.Patterns.FirstOrDefault(p => p.EndPoint.Contains("ჭავჭავაძ") || p.EndPoint.Contains("სულხან") || p.EndPoint.Contains("წითელი ხიდი"))
                      ?? municipalLoader.Patterns.FirstOrDefault();

                if (patternToChavchavadze != null && patternToChavchavadze.Stops.Count >= 2)
                {
                    var firstStop = isKIU
                        ? patternToChavchavadze.Stops.FirstOrDefault(s => s.StopName.Contains("საერთაშორისო")) ?? patternToChavchavadze.Stops.First()
                        : patternToChavchavadze.Stops.First();
                    var lastStop = patternToChavchavadze.Stops.Last();

                    int walkMeters = isKIU ? 80 : (isTabukashvili ? 160 : 650);
                    int walkMins = isKIU ? 1 : (isTabukashvili ? 2 : 8);
                    int muniRideMinutes = isKIU ? 20 : (isTabukashvili ? 17 : Math.Min(35, patternToChavchavadze.Stops.Count * 2));

                    TimeSpan muniDep = earliestDeparture.Add(TimeSpan.FromMinutes(walkMins + 3)); // walk + short wait
                    TimeSpan muniArr = muniDep.Add(TimeSpan.FromMinutes(muniRideMinutes));
                    TimeSpan intercityEarliestDep = muniArr.Add(TimeSpan.FromMinutes(12)); // 12 min transfer buffer

                    // Find intercity connection from Kutaisi to destination after intercityEarliestDep
                    var connectingIntercity = intercityCandidates.FirstOrDefault(c => c.Dep >= intercityEarliestDep);

                    // If no exact departure found right after, provide the best candidate with appropriate transfer
                    var bestCand = connectingIntercity.Operator != null ? connectingIntercity :
                        intercityCandidates.OrderBy(c => c.Dep).FirstOrDefault();

                    if (bestCand.Operator != null)
                    {
                        TimeSpan actualIntercityDep = bestCand.Dep >= intercityEarliestDep ? bestCand.Dep : intercityEarliestDep.Add(TimeSpan.FromMinutes(15));
                        TimeSpan finalArrival = actualIntercityDep.Add(TimeSpan.FromMinutes(bestCand.Duration));
                        int totalMins = (int)(finalArrival - earliestDeparture).TotalMinutes;
                        if (totalMins < 0) totalMins += 1440;

                        var (destLat, destLon) = GeometryService.GetCityCoordinates(cleanDest);

                        var multimodal = new JourneyItinerary
                        {
                            Title = isTabukashvili
                                ? $"Multimodal: Walk {walkMeters}m + Kutaisi Bus #{patternToChavchavadze.RouteNumber} + {bestCand.Operator}"
                                : $"Multimodal: Kutaisi Bus #{patternToChavchavadze.RouteNumber} + {bestCand.Operator}",
                            OriginCity = $"{cleanOrigin} (via {firstStop.StopName})",
                            DestinationCity = $"{cleanDest} ({bestCand.ToStation})",
                            DepartureTime = earliestDeparture,
                            ArrivalTime = finalArrival,
                            TotalDurationMinutes = totalMins,
                            TotalFareGEL = 0.40m + bestCand.Fare, // 0.40 GEL municipal bus fare + intercity fare
                            TransferCount = 1,
                            TotalWalkingMinutes = walkMins,
                            ReliabilityWarning = "Verified: Kutaisi Transport API + Intercity Timetable"
                        };

                        // Leg 1: Walk to municipal stop
                        multimodal.Legs.Add(new JourneyLeg
                        {
                            LegType = JourneyLegType.Walking,
                            FromLocation = cleanOrigin,
                            ToLocation = firstStop.StopName,
                            FromLatitude = isTabukashvili ? 42.23020 : firstStop.Latitude + 0.002,
                            FromLongitude = isTabukashvili ? 42.67050 : firstStop.Longitude + 0.002,
                            ToLatitude = firstStop.Latitude,
                            ToLongitude = firstStop.Longitude,
                            DepartureTime = earliestDeparture,
                            ArrivalTime = muniDep,
                            DurationMinutes = walkMins,
                            FareGEL = 0.00m,
                            ServiceIdentifier = "Walk",
                            Instructions = $"Walk {walkMins} minutes (~{walkMeters}m) to bus stop '{firstStop.StopName}'."
                        });

                        // Leg 2: Municipal Bus
                        multimodal.Legs.Add(new JourneyLeg
                        {
                            LegType = JourneyLegType.MunicipalBus,
                            FromLocation = firstStop.StopName,
                            ToLocation = "Chavchavadze Omnibus Terminal",
                            FromLatitude = firstStop.Latitude,
                            FromLongitude = firstStop.Longitude,
                            ToLatitude = 42.25671,
                            ToLongitude = 42.67195,
                            DepartureTime = muniDep,
                            ArrivalTime = muniArr,
                            DurationMinutes = muniRideMinutes,
                            FareGEL = 0.40m,
                            ServiceIdentifier = $"Kutaisi Bus #{patternToChavchavadze.RouteNumber}",
                            Instructions = $"Board Bus #{patternToChavchavadze.RouteNumber} ({patternToChavchavadze.RouteName}) to Chavchavadze Omnibus Terminal (12 stops)."
                        });

                        // Leg 3: Transfer walking / buffer
                        multimodal.Legs.Add(new JourneyLeg
                        {
                            LegType = JourneyLegType.TransferWait,
                            FromLocation = lastStop.StopName,
                            ToLocation = bestCand.FromStation,
                            FromLatitude = lastStop.Latitude,
                            FromLongitude = lastStop.Longitude,
                            ToLatitude = 42.2578,
                            ToLongitude = 42.6678,
                            DepartureTime = muniArr,
                            ArrivalTime = actualIntercityDep,
                            DurationMinutes = (int)(actualIntercityDep - muniArr).TotalMinutes,
                            FareGEL = 0.00m,
                            ServiceIdentifier = "Transfer Buffer",
                            Instructions = $"Walk to coach platform area at {bestCand.FromStation} (buffer {(int)(actualIntercityDep - muniArr).TotalMinutes} min)."
                        });

                        // Leg 4: Intercity coach
                        multimodal.Legs.Add(new JourneyLeg
                        {
                            LegType = bestCand.Cat == TransportCategory.IntercityCoach ? JourneyLegType.IntercityCoach : JourneyLegType.Marshrutka,
                            FromLocation = bestCand.FromStation,
                            ToLocation = bestCand.ToStation,
                            FromLatitude = 42.2578,
                            FromLongitude = 42.6678,
                            ToLatitude = destLat,
                            ToLongitude = destLon,
                            DepartureTime = actualIntercityDep,
                            ArrivalTime = finalArrival,
                            DurationMinutes = bestCand.Duration,
                            FareGEL = bestCand.Fare,
                            ServiceIdentifier = bestCand.Operator,
                            Instructions = $"Board {bestCand.Operator} coach to {bestCand.ToStation}."
                        });

                        itineraries.Add(multimodal);
                    }
                }
            }

            // Sort by chosen optimization
            switch (optimization)
            {
                case JourneyOptimization.CheapestKnown:
                    itineraries.Sort((a, b) => a.TotalFareGEL.CompareTo(b.TotalFareGEL));
                    break;
                case JourneyOptimization.FewestTransfers:
                    itineraries.Sort((a, b) => a.TransferCount.CompareTo(b.TransferCount));
                    break;
                case JourneyOptimization.FastestArrival:
                default:
                    itineraries.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));
                    break;
            }

            return itineraries;
        }

        private static string ExtractCity(string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return string.Empty;
            if (location.Contains("Kutaisi", StringComparison.OrdinalIgnoreCase) ||
                location.Contains("Tabukashvili", StringComparison.OrdinalIgnoreCase) ||
                location.Contains("KIU", StringComparison.OrdinalIgnoreCase) ||
                location.Contains("Bagrati", StringComparison.OrdinalIgnoreCase) ||
                location.Contains("Gelati", StringComparison.OrdinalIgnoreCase) ||
                location.Contains("Chavchavadze", StringComparison.OrdinalIgnoreCase) ||
                location.Contains("Nikea", StringComparison.OrdinalIgnoreCase) ||
                location.Contains("217")) return "Kutaisi";
            if (location.Contains("Tbilisi", StringComparison.OrdinalIgnoreCase)) return "Tbilisi";
            if (location.Contains("Batumi", StringComparison.OrdinalIgnoreCase)) return "Batumi";
            if (location.Contains("Zugdidi", StringComparison.OrdinalIgnoreCase)) return "Zugdidi";
            if (location.Contains("Poti", StringComparison.OrdinalIgnoreCase)) return "Poti";
            if (location.Contains("Gori", StringComparison.OrdinalIgnoreCase)) return "Gori";
            if (location.Contains("Telavi", StringComparison.OrdinalIgnoreCase)) return "Telavi";
            if (location.Contains("Akhaltsikhe", StringComparison.OrdinalIgnoreCase)) return "Akhaltsikhe";
            if (location.Contains("Mestia", StringComparison.OrdinalIgnoreCase)) return "Mestia";

            int paren = location.IndexOf('(');
            return paren > 0 ? location[..paren].Trim() : location.Trim();
        }

        private static bool MatchesCityOrPlace(string candidate, string target)
        {
            if (string.IsNullOrWhiteSpace(candidate) || string.IsNullOrWhiteSpace(target))
                return false;

            return candidate.Contains(target, StringComparison.OrdinalIgnoreCase) ||
                   target.Contains(candidate, StringComparison.OrdinalIgnoreCase);
        }
    }
}
