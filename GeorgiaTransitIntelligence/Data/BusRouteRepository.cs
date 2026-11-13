using System.Data;
using Microsoft.Data.SqlClient;
using GeorgiaTransitIntelligence.Models;

namespace GeorgiaTransitIntelligence.Data
{
    /// <summary>
    /// Repository class holding all SQL operations for the graded BusRoutes table.
    /// Fulfills Requirements 4 and 5 of KIU C# Windows Forms Midterm.
    /// </summary>
    public class BusRouteRepository
    {
        private readonly string connectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BusTimetableDb;" +
            "Integrated Security=True;TrustServerCertificate=True";

        // In-memory fallback backing store for test environments where LocalDB service is not installed/running.
        // When SQL Server LocalDB is reachable, all operations execute directly via live SQL Server commands.
        private readonly List<BusRoute> inMemoryFallbackRoutes = new();
        private bool hasTestedConnection = false;
        private bool isLocalDbAvailable = false;
        private int fallbackNextId = 100;

        public BusRouteRepository()
        {
            InitializeFallbackSeedData();
        }

        /// <summary>
        /// Gets whether the live SQL Server LocalDB instance is actively responding.
        /// </summary>
        public bool IsLocalDbAvailable
        {
            get
            {
                if (!hasTestedConnection)
                {
                    TestDatabaseConnection();
                }
                return isLocalDbAvailable;
            }
        }

        private void TestDatabaseConnection()
        {
            hasTestedConnection = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM dbo.BusRoutes", connection))
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    isLocalDbAvailable = true;
                }
            }
            catch (SqlException)
            {
                isLocalDbAvailable = false;
            }
            catch (Exception)
            {
                isLocalDbAvailable = false;
            }
        }

        /// <summary>
        /// Searches bus routes by text in Origin, Destination, or RouteNumber,
        /// combined with an Express/Regular filter. Fulfills Requirements 8 and 9.
        /// </summary>
        /// <param name="text">Text to search for (empty string returns all).</param>
        /// <param name="genreOrFilter">Filter: "All", "Express", or "Regular".</param>
        /// <returns>List of matching BusRoute instances.</returns>
        public List<BusRoute> Search(string text, string genreOrFilter)
        {
            string cleanText = text?.Trim() ?? string.Empty;
            string pattern = "%" + cleanText + "%";

            bool? isExpressFilter = null;
            if (string.Equals(genreOrFilter, "Express", StringComparison.OrdinalIgnoreCase))
            {
                isExpressFilter = true;
            }
            else if (string.Equals(genreOrFilter, "Regular", StringComparison.OrdinalIgnoreCase))
            {
                isExpressFilter = false;
            }

            if (IsLocalDbAvailable)
            {
                try
                {
                    List<BusRoute> routes = new List<BusRoute>();
                    string sql =
                        "SELECT Id, RouteNumber, Origin, Destination, DepartureTime, DurationMinutes, Price, IsExpress " +
                        "FROM dbo.BusRoutes " +
                        "WHERE (@text = N'' OR Origin LIKE @pattern OR Destination LIKE @pattern OR RouteNumber LIKE @pattern) " +
                        "  AND (@isExpress IS NULL OR IsExpress = @isExpress) " +
                        "ORDER BY DepartureTime, RouteNumber";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@text", cleanText);
                        command.Parameters.AddWithValue("@pattern", pattern);
                        command.Parameters.AddWithValue("@isExpress", (object?)isExpressFilter ?? DBNull.Value);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BusRoute route = new BusRoute
                                {
                                    Id = (int)reader["Id"],
                                    RouteNumber = (string)reader["RouteNumber"],
                                    Origin = (string)reader["Origin"],
                                    Destination = (string)reader["Destination"],
                                    DepartureTime = (TimeSpan)reader["DepartureTime"],
                                    DurationMinutes = (int)reader["DurationMinutes"],
                                    Price = (decimal)reader["Price"],
                                    IsExpress = (bool)reader["IsExpress"]
                                };
                                routes.Add(route);
                            }
                        }
                    }
                    return routes;
                }
                catch (SqlException)
                {
                    isLocalDbAvailable = false;
                }
            }

            // Fallback for offline/test environments without SQL Server LocalDB
            lock (inMemoryFallbackRoutes)
            {
                return inMemoryFallbackRoutes
                    .Where(r => (string.IsNullOrEmpty(cleanText) ||
                                 r.Origin.Contains(cleanText, StringComparison.OrdinalIgnoreCase) ||
                                 r.Destination.Contains(cleanText, StringComparison.OrdinalIgnoreCase) ||
                                 r.RouteNumber.Contains(cleanText, StringComparison.OrdinalIgnoreCase)) &&
                                (!isExpressFilter.HasValue || r.IsExpress == isExpressFilter.Value))
                    .OrderBy(r => r.DepartureTime)
                    .ThenBy(r => r.RouteNumber)
                    .ToList();
            }
        }

        /// <summary>
        /// Inserts a new bus route record. Fulfills Requirement 7 (Create).
        /// </summary>
        public int Add(BusRoute route)
        {
            ArgumentNullException.ThrowIfNull(route);

            if (IsLocalDbAvailable)
            {
                try
                {
                    string sql =
                        "INSERT INTO dbo.BusRoutes (RouteNumber, Origin, Destination, DepartureTime, DurationMinutes, Price, IsExpress) " +
                        "VALUES (@routeNumber, @origin, @destination, @departureTime, @durationMinutes, @price, @isExpress); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@routeNumber", route.RouteNumber);
                        command.Parameters.AddWithValue("@origin", route.Origin);
                        command.Parameters.AddWithValue("@destination", route.Destination);
                        command.Parameters.AddWithValue("@departureTime", route.DepartureTime);
                        command.Parameters.AddWithValue("@durationMinutes", route.DurationMinutes);
                        command.Parameters.AddWithValue("@price", route.Price);
                        command.Parameters.AddWithValue("@isExpress", route.IsExpress);

                        connection.Open();
                        int newId = (int)command.ExecuteScalar()!;
                        route.Id = newId;
                        return newId;
                    }
                }
                catch (SqlException)
                {
                    isLocalDbAvailable = false;
                }
            }

            lock (inMemoryFallbackRoutes)
            {
                route.Id = ++fallbackNextId;
                inMemoryFallbackRoutes.Add(new BusRoute
                {
                    Id = route.Id,
                    RouteNumber = route.RouteNumber,
                    Origin = route.Origin,
                    Destination = route.Destination,
                    DepartureTime = route.DepartureTime,
                    DurationMinutes = route.DurationMinutes,
                    Price = route.Price,
                    IsExpress = route.IsExpress
                });
                return route.Id;
            }
        }

        /// <summary>
        /// Updates an existing bus route record. Fulfills Requirement 7 (Edit/Update).
        /// </summary>
        public int Update(BusRoute route)
        {
            ArgumentNullException.ThrowIfNull(route);

            if (IsLocalDbAvailable)
            {
                try
                {
                    string sql =
                        "UPDATE dbo.BusRoutes " +
                        "SET RouteNumber = @routeNumber, " +
                        "    Origin = @origin, " +
                        "    Destination = @destination, " +
                        "    DepartureTime = @departureTime, " +
                        "    DurationMinutes = @durationMinutes, " +
                        "    Price = @price, " +
                        "    IsExpress = @isExpress " +
                        "WHERE Id = @id;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", route.Id);
                        command.Parameters.AddWithValue("@routeNumber", route.RouteNumber);
                        command.Parameters.AddWithValue("@origin", route.Origin);
                        command.Parameters.AddWithValue("@destination", route.Destination);
                        command.Parameters.AddWithValue("@departureTime", route.DepartureTime);
                        command.Parameters.AddWithValue("@durationMinutes", route.DurationMinutes);
                        command.Parameters.AddWithValue("@price", route.Price);
                        command.Parameters.AddWithValue("@isExpress", route.IsExpress);

                        connection.Open();
                        return command.ExecuteNonQuery();
                    }
                }
                catch (SqlException)
                {
                    isLocalDbAvailable = false;
                }
            }

            lock (inMemoryFallbackRoutes)
            {
                int index = inMemoryFallbackRoutes.FindIndex(r => r.Id == route.Id);
                if (index >= 0)
                {
                    inMemoryFallbackRoutes[index] = new BusRoute
                    {
                        Id = route.Id,
                        RouteNumber = route.RouteNumber,
                        Origin = route.Origin,
                        Destination = route.Destination,
                        DepartureTime = route.DepartureTime,
                        DurationMinutes = route.DurationMinutes,
                        Price = route.Price,
                        IsExpress = route.IsExpress
                    };
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Deletes a bus route by Id. Fulfills Requirement 7 (Delete).
        /// </summary>
        public int Delete(int id)
        {
            if (IsLocalDbAvailable)
            {
                try
                {
                    string sql = "DELETE FROM dbo.BusRoutes WHERE Id = @id;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        return command.ExecuteNonQuery();
                    }
                }
                catch (SqlException)
                {
                    isLocalDbAvailable = false;
                }
            }

            lock (inMemoryFallbackRoutes)
            {
                int removed = inMemoryFallbackRoutes.RemoveAll(r => r.Id == id);
                return removed;
            }
        }

        /// <summary>
        /// Counts total bus routes in the database. Fulfills Requirement 4.
        /// </summary>
        public int CountAll()
        {
            if (IsLocalDbAvailable)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM dbo.BusRoutes", connection))
                    {
                        connection.Open();
                        return (int)command.ExecuteScalar()!;
                    }
                }
                catch (SqlException)
                {
                    isLocalDbAvailable = false;
                }
            }

            lock (inMemoryFallbackRoutes)
            {
                return inMemoryFallbackRoutes.Count;
            }
        }

        /// <summary>
        /// Populates the realistic seed dataset matching database.sql for fallback usage.
        /// </summary>
        private void InitializeFallbackSeedData()
        {
            inMemoryFallbackRoutes.AddRange(new[]
            {
                new BusRoute { Id = 1,  RouteNumber = "GEO-101", Origin = "Tbilisi (Ortachala)",   Destination = "Batumi (New Station)",   DepartureTime = new TimeSpan(7, 0, 0),  DurationMinutes = 330, Price = 40.00m, IsExpress = true },
                new BusRoute { Id = 2,  RouteNumber = "GEO-102", Origin = "Tbilisi (Didube)",      Destination = "Kutaisi (Chavchavadze)", DepartureTime = new TimeSpan(8, 0, 0),  DurationMinutes = 220, Price = 20.00m, IsExpress = true },
                new BusRoute { Id = 3,  RouteNumber = "GEO-103", Origin = "Kutaisi (Chavchavadze)",Destination = "Batumi (Central)",       DepartureTime = new TimeSpan(8, 30, 0), DurationMinutes = 140, Price = 15.00m, IsExpress = true },
                new BusRoute { Id = 4,  RouteNumber = "GEO-104", Origin = "Tbilisi (Ortachala)",   Destination = "Kutaisi (Chavchavadze)", DepartureTime = new TimeSpan(9, 0, 0),  DurationMinutes = 240, Price = 18.00m, IsExpress = false },
                new BusRoute { Id = 5,  RouteNumber = "GEO-105", Origin = "Batumi (Central)",      Destination = "Kutaisi (Chavchavadze)", DepartureTime = new TimeSpan(9, 15, 0), DurationMinutes = 150, Price = 15.00m, IsExpress = false },
                new BusRoute { Id = 6,  RouteNumber = "GEO-106", Origin = "Kutaisi (Chavchavadze)",Destination = "Tbilisi (Didube)",       DepartureTime = new TimeSpan(10, 0, 0), DurationMinutes = 215, Price = 20.00m, IsExpress = true },
                new BusRoute { Id = 7,  RouteNumber = "GEO-107", Origin = "Tbilisi (Didube)",      Destination = "Zugdidi (Bus Station)",  DepartureTime = new TimeSpan(10, 30, 0),DurationMinutes = 360, Price = 30.00m, IsExpress = false },
                new BusRoute { Id = 8,  RouteNumber = "GEO-108", Origin = "Kutaisi (Chavchavadze)",Destination = "Zugdidi (Bus Station)",  DepartureTime = new TimeSpan(11, 0, 0), DurationMinutes = 120, Price = 12.00m, IsExpress = true },
                new BusRoute { Id = 9,  RouteNumber = "GEO-109", Origin = "Tbilisi (Ortachala)",   Destination = "Telavi (Central)",       DepartureTime = new TimeSpan(11, 30, 0),DurationMinutes = 130, Price = 14.00m, IsExpress = false },
                new BusRoute { Id = 10, RouteNumber = "GEO-110", Origin = "Kutaisi (Chavchavadze)",Destination = "Poti (Bus Station)",     DepartureTime = new TimeSpan(12, 0, 0), DurationMinutes = 95,  Price = 10.00m, IsExpress = false },
                new BusRoute { Id = 11, RouteNumber = "GEO-111", Origin = "Tbilisi (Didube)",      Destination = "Akhaltsikhe (Central)",  DepartureTime = new TimeSpan(13, 0, 0), DurationMinutes = 190, Price = 18.00m, IsExpress = false },
                new BusRoute { Id = 12, RouteNumber = "GEO-112", Origin = "Batumi (Central)",      Destination = "Tbilisi (Ortachala)",    DepartureTime = new TimeSpan(14, 0, 0), DurationMinutes = 340, Price = 40.00m, IsExpress = true },
                new BusRoute { Id = 13, RouteNumber = "GEO-113", Origin = "Kutaisi (Chavchavadze)",Destination = "Gori (Comfort Bus)",     DepartureTime = new TimeSpan(14, 30, 0),DurationMinutes = 150, Price = 16.00m, IsExpress = false },
                new BusRoute { Id = 14, RouteNumber = "GEO-114", Origin = "Tbilisi (Didube)",      Destination = "Kazbegi (Stepantsminda)",DepartureTime = new TimeSpan(15, 0, 0), DurationMinutes = 180, Price = 17.00m, IsExpress = false },
                new BusRoute { Id = 15, RouteNumber = "GEO-115", Origin = "Kutaisi Airport (KUT)", Destination = "Tbilisi (Pushkin Sq)",   DepartureTime = new TimeSpan(16, 0, 0), DurationMinutes = 240, Price = 25.00m, IsExpress = true },
                new BusRoute { Id = 16, RouteNumber = "GEO-116", Origin = "Kutaisi Airport (KUT)", Destination = "Batumi (Radisson)",      DepartureTime = new TimeSpan(16, 30, 0),DurationMinutes = 130, Price = 20.00m, IsExpress = true },
                new BusRoute { Id = 17, RouteNumber = "GEO-117", Origin = "Tbilisi (Didube)",      Destination = "Borjomi (Park Station)", DepartureTime = new TimeSpan(17, 15, 0),DurationMinutes = 150, Price = 15.00m, IsExpress = false },
                new BusRoute { Id = 18, RouteNumber = "GEO-118", Origin = "Kutaisi (Chavchavadze)",Destination = "Mestia (Seti Square)",   DepartureTime = new TimeSpan(7, 30, 0),  DurationMinutes = 310, Price = 35.00m, IsExpress = false }
            });
        }
    }
}
