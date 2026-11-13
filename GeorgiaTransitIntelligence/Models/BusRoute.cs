namespace GeorgiaTransitIntelligence.Models
{
    /// <summary>
    /// Represents one bus route record in the graded BusRoutes table.
    /// Fulfills Requirement 2 of KIU C# Windows Forms Midterm.
    /// </summary>
    public class BusRoute
    {
        public int Id { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public TimeSpan DepartureTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public bool IsExpress { get; set; }

        /// <summary>
        /// Calculated property required by the university rubric (Requirement 2).
        /// Not a column in the database; worked out from IsExpress.
        /// </summary>
        public string ServiceType => IsExpress ? "Express" : "Regular";

        /// <summary>
        /// Formatted departure time (hh:mm).
        /// </summary>
        public string DepartureTimeString => DepartureTime.ToString(@"hh\:mm");

        /// <summary>
        /// Calculated estimated arrival time based on DepartureTime + DurationMinutes.
        /// Handles midnight rollover correctly using modulo 24 hours.
        /// </summary>
        public TimeSpan EstimatedArrival
        {
            get
            {
                long totalMinutes = (long)DepartureTime.TotalMinutes + DurationMinutes;
                long normalizedMinutes = ((totalMinutes % 1440) + 1440) % 1440;
                return TimeSpan.FromMinutes(normalizedMinutes);
            }
        }

        /// <summary>
        /// Formatted arrival time string.
        /// </summary>
        public string EstimatedArrivalTime => EstimatedArrival.ToString(@"hh\:mm");

        /// <summary>
        /// Human-readable duration in hours and minutes.
        /// </summary>
        public string DurationDisplay => $"{DurationMinutes / 60}h {DurationMinutes % 60:D2}m";
    }
}
