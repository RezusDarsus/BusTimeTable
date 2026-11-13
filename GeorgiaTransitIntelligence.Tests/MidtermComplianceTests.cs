using System.Text;
using GeorgiaTransitIntelligence.Data;
using GeorgiaTransitIntelligence.Models;
using GeorgiaTransitIntelligence.Services;
using Xunit;

namespace GeorgiaTransitIntelligence.Tests
{
    public class MidtermComplianceTests
    {
        [Fact]
        public void Requirement2_CalculatedProperty_WorksCorrectly()
        {
            var expressRoute = new BusRoute
            {
                RouteNumber = "GEO-101",
                Origin = "Tbilisi",
                Destination = "Batumi",
                DepartureTime = new TimeSpan(8, 0, 0),
                DurationMinutes = 330,
                Price = 40.00m,
                IsExpress = true
            };

            var regularRoute = new BusRoute
            {
                RouteNumber = "GEO-104",
                Origin = "Tbilisi",
                Destination = "Kutaisi",
                DepartureTime = new TimeSpan(9, 0, 0),
                DurationMinutes = 240,
                Price = 18.00m,
                IsExpress = false
            };

            Assert.Equal("Express", expressRoute.ServiceType);
            Assert.Equal("Regular", regularRoute.ServiceType);
            Assert.Equal("08:00", expressRoute.DepartureTimeString);
            Assert.Equal("13:30", expressRoute.EstimatedArrivalTime);
            Assert.Equal("5h 30m", expressRoute.DurationDisplay);
        }

        [Fact]
        public void Requirement2_EstimatedArrival_HandlesMidnightRollover()
        {
            var nightRoute = new BusRoute
            {
                RouteNumber = "NIGHT-1",
                Origin = "Batumi",
                Destination = "Tbilisi",
                DepartureTime = new TimeSpan(22, 30, 0),
                DurationMinutes = 180, // 3 hours -> 01:30 next day
                IsExpress = true
            };

            Assert.Equal(new TimeSpan(1, 30, 0), nightRoute.EstimatedArrival);
            Assert.Equal("01:30", nightRoute.EstimatedArrivalTime);
        }

        [Fact]
        public void Requirement3_SeedData_ContainsAtLeast15Rows()
        {
            var repo = new BusRouteRepository();
            int count = repo.CountAll();
            Assert.True(count >= 15, $"Seed data contains {count} rows, expected at least 15.");
        }

        [Fact]
        public void Requirement4And5_Search_SearchesOriginDestinationAndNumber()
        {
            var repo = new BusRouteRepository();

            var kutaisiResults = repo.Search("Kutaisi", "All");
            Assert.NotEmpty(kutaisiResults);
            Assert.True(kutaisiResults.All(r =>
                r.Origin.Contains("Kutaisi", StringComparison.OrdinalIgnoreCase) ||
                r.Destination.Contains("Kutaisi", StringComparison.OrdinalIgnoreCase)));

            var tbilisiResults = repo.Search("Tbilisi", "All");
            Assert.NotEmpty(tbilisiResults);
            Assert.True(tbilisiResults.Count >= 3);

            var emptySearch = repo.Search("", "All");
            Assert.Equal(repo.CountAll(), emptySearch.Count);

            var noMatch = repo.Search("NonExistentCityXYZ", "All");
            Assert.Empty(noMatch);
        }

        [Fact]
        public void Requirement5_SqlInjection_IsHandledSafely()
        {
            var repo = new BusRouteRepository();
            string maliciousInput = "' OR 1=1 --";

            var results = repo.Search(maliciousInput, "All");
            // Parameterized query treats this string as literal text, finding 0 rows without SQL error
            Assert.Empty(results);
        }

        [Fact]
        public void Requirement9_FilterAndSearch_WorkTogether()
        {
            var repo = new BusRouteRepository();

            var expressKutaisi = repo.Search("Kutaisi", "Express");
            Assert.NotEmpty(expressKutaisi);
            Assert.True(expressKutaisi.All(r => r.IsExpress));

            var regularKutaisi = repo.Search("Kutaisi", "Regular");
            Assert.NotEmpty(regularKutaisi);
            Assert.True(regularKutaisi.All(r => !r.IsExpress));
        }

        [Fact]
        public void Requirement7_CrudOperations_PersistCorrectly()
        {
            var repo = new BusRouteRepository();
            int initialCount = repo.CountAll();

            // 1. Create
            var newRoute = new BusRoute
            {
                RouteNumber = "TEST-999",
                Origin = "Kutaisi (Test)",
                Destination = "Batumi (Test)",
                DepartureTime = new TimeSpan(14, 0, 0),
                DurationMinutes = 120,
                Price = 16.50m,
                IsExpress = true
            };

            int newId = repo.Add(newRoute);
            Assert.True(newId > 0);
            Assert.Equal(initialCount + 1, repo.CountAll());

            // 2. Read
            var found = repo.Search("TEST-999", "All");
            Assert.Single(found);
            Assert.Equal("Kutaisi (Test)", found[0].Origin);

            // 3. Update
            var toUpdate = found[0];
            toUpdate.Destination = "Batumi (Updated)";
            toUpdate.Price = 19.00m;
            int updateResult = repo.Update(toUpdate);
            Assert.Equal(1, updateResult);

            var updatedSearch = repo.Search("TEST-999", "All");
            Assert.Single(updatedSearch);
            Assert.Equal("Batumi (Updated)", updatedSearch[0].Destination);
            Assert.Equal(19.00m, updatedSearch[0].Price);

            // 4. Delete
            int deleteResult = repo.Delete(toUpdate.Id);
            Assert.Equal(1, deleteResult);
            Assert.Equal(initialCount, repo.CountAll());
        }

        [Fact]
        public void Bonus_CsvExport_PreservesUnicodeAndFormat()
        {
            var routes = new List<BusRoute>
            {
                new BusRoute
                {
                    RouteNumber = "GEO-1",
                    Origin = "ქუთაისი (ჭავჭავაძე)",
                    Destination = "თბილისი (დიდუბე)",
                    DepartureTime = new TimeSpan(8, 0, 0),
                    DurationMinutes = 210,
                    Price = 20.00m,
                    IsExpress = true
                },
                new BusRoute
                {
                    RouteNumber = "GEO-2",
                    Origin = "ბათუმი, ცენტრალური",
                    Destination = "ქუთაისი \"ცენტრი\"",
                    DepartureTime = new TimeSpan(10, 30, 0),
                    DurationMinutes = 150,
                    Price = 15.00m,
                    IsExpress = false
                }
            };

            string tempFile = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.csv");

            try
            {
                int exported = CsvExportService.ExportToCsv(routes, tempFile);
                Assert.Equal(2, exported);
                Assert.True(File.Exists(tempFile));

                byte[] bytes = File.ReadAllBytes(tempFile);
                // Verify UTF-8 BOM (0xEF, 0xBB, 0xBF)
                Assert.True(bytes.Length >= 3);
                Assert.Equal(0xEF, bytes[0]);
                Assert.Equal(0xBB, bytes[1]);
                Assert.Equal(0xBF, bytes[2]);

                string text = File.ReadAllText(tempFile, Encoding.UTF8);
                Assert.Contains("ქუთაისი", text);
                Assert.Contains("თბილისი", text);
                Assert.Contains("ბათუმი", text);
                Assert.Contains("\"ბათუმი, ცენტრალური\"", text); // Escaped comma
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void MunicipalDataLoader_LoadsRoutesAndStops()
        {
            var loader = new MunicipalDataLoader();
            Assert.True(loader.TotalPatternCount >= 40, $"Loaded {loader.TotalPatternCount} patterns");
            Assert.True(loader.TotalStopCount >= 400, $"Loaded {loader.TotalStopCount} unique stops");

            var stop = loader.UniqueStops.Values.First();
            Assert.False(string.IsNullOrEmpty(stop.StopName));
            Assert.True(stop.Latitude > 42.0 && stop.Latitude < 43.0);
            Assert.True(stop.Longitude > 42.0 && stop.Longitude < 43.5);
        }

        [Fact]
        public void IntercityDataLoader_LoadsServicesAndOperators()
        {
            var loader = new IntercityDataLoader();
            Assert.NotEmpty(loader.Operators);
            Assert.NotEmpty(loader.Services);
            Assert.NotEmpty(loader.Destinations);

            var tbilisiBatumi = loader.GetServices("Tbilisi", "Batumi");
            Assert.NotEmpty(tbilisiBatumi);
        }

        [Fact]
        public void JourneyPlanner_FindsMultimodalJourney()
        {
            var repo = new BusRouteRepository();
            var muni = new MunicipalDataLoader();
            var inter = new IntercityDataLoader();
            var planner = new JourneyPlanner(muni, inter, repo);

            var results = planner.PlanJourney(
                "Kutaisi (Red Bridge)",
                "Batumi",
                DateTime.Today,
                new TimeSpan(8, 0, 0),
                allowMunicipal: true,
                allowIntercity: true,
                allowMarshrutka: true);

            Assert.NotEmpty(results);
            var best = results[0];
            Assert.True(best.TotalDurationMinutes > 0);
            Assert.True(best.TotalFareGEL > 0);
            Assert.NotEmpty(best.Legs);
        }

        [Fact]
        public void JourneyPlanner_TabukashviliStreet217ToTbilisi_CalculatesWalkAndBus20()
        {
            var repo = new BusRouteRepository();
            var muni = new MunicipalDataLoader();
            var inter = new IntercityDataLoader();
            var planner = new JourneyPlanner(muni, inter, repo);

            var results = planner.PlanJourney(
                "Tabukashvili Street 217, Kutaisi",
                "Tbilisi",
                DateTime.Today,
                new TimeSpan(8, 0, 0),
                allowMunicipal: true,
                allowIntercity: true,
                allowMarshrutka: true);

            Assert.NotEmpty(results);
            var best = results[0];
            Assert.True(best.TotalWalkingMinutes > 0);
            Assert.True(best.TotalDurationMinutes > 180);
            Assert.True(best.TotalFareGEL > 0);
            Assert.NotEmpty(best.Legs);
            Assert.Contains(best.Legs, l => l.LegType == JourneyLegType.Walking);
            Assert.Contains(best.Legs, l => l.LegType == JourneyLegType.MunicipalBus);
        }

        [Fact]
        public void JourneyPlanner_KIUToTbilisi_CalculatesBus25AndWalk()
        {
            var repo = new BusRouteRepository();
            var muni = new MunicipalDataLoader();
            var inter = new IntercityDataLoader();
            var planner = new JourneyPlanner(muni, inter, repo);

            var results = planner.PlanJourney(
                "Kutaisi International University (KIU)",
                "Tbilisi",
                DateTime.Today,
                new TimeSpan(8, 0, 0),
                allowMunicipal: true,
                allowIntercity: true,
                allowMarshrutka: true);

            Assert.NotEmpty(results);
            var best = results[0];
            Assert.True(best.TotalWalkingMinutes > 0);
            Assert.True(best.TotalDurationMinutes > 180);
            Assert.True(best.TotalFareGEL > 0);
            Assert.NotEmpty(best.Legs);
            Assert.Contains(best.Legs, l => l.LegType == JourneyLegType.Walking);
            Assert.Contains(best.Legs, l => l.LegType == JourneyLegType.MunicipalBus);
        }

        [Fact]
        public void VehicleSimulationService_ClockAndTick_Work()
        {
            var repo = new BusRouteRepository();
            var muni = new MunicipalDataLoader();
            var inter = new IntercityDataLoader();
            var sim = new VehicleSimulationService(muni, inter, repo);

            var initialTime = sim.CurrentSimulatedTime;
            sim.Tick(1.0); // 1 second
            Assert.True(sim.CurrentSimulatedTime > initialTime);

            sim.IsPaused = true;
            var pausedTime = sim.CurrentSimulatedTime;
            sim.Tick(5.0);
            Assert.Equal(pausedTime, sim.CurrentSimulatedTime);
        }
    }
}
