namespace GeorgiaTransitIntelligence.Models
{
    public enum TransportCategory
    {
        MunicipalBus,
        IntercityCoach,
        Marshrutka
    }

    public class MunicipalStop
    {
        public string StopId { get; set; } = string.Empty;
        public string StopCode { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int StopOrder { get; set; }
        public string VehicleMode { get; set; } = "BUS";
    }

    public class MunicipalPattern
    {
        public string RouteNumber { get; set; } = string.Empty;
        public string RouteId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string PatternSuffix { get; set; } = string.Empty;
        public int DirectionId { get; set; }
        public string DirectionDescription { get; set; } = string.Empty;
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public string HeadSign { get; set; } = string.Empty;
        public string FirstStopId { get; set; } = string.Empty;
        public string LastStopId { get; set; } = string.Empty;
        public List<MunicipalStop> Stops { get; set; } = new();
    }

    public class ScheduledDeparture
    {
        public string RouteNumber { get; set; } = string.Empty;
        public int DirectionId { get; set; }
        public string StopId { get; set; } = string.Empty;
        public string Weekday { get; set; } = string.Empty;
        public TimeSpan DepartureTime { get; set; }
    }

    public class TransportOperator
    {
        public int OperatorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TransportCategory Category { get; set; }
        public string ColorHex { get; set; } = "#1E88E5";
        public string FleetDescription { get; set; } = string.Empty;
        public string CoverageSummary { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }

    public class IntercityService
    {
        public int ServiceId { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public TransportCategory Category { get; set; } = TransportCategory.IntercityCoach;
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public TimeSpan? DepartureTime { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? TicketPriceGEL { get; set; }
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public string SchedulePattern { get; set; } = string.Empty;
        public string ScheduleRecordType { get; set; } = string.Empty;
        public string EvidenceStatus { get; set; } = string.Empty;
        public string SourceTitle { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
    }

    public class VehiclePosition
    {
        public string VehicleId { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string RouteNumber { get; set; } = string.Empty;
        public TransportCategory Category { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public double HeadingDegrees { get; set; }
        public TimeSpan ScheduledDeparture { get; set; }
        public TimeSpan EstimatedArrival { get; set; }
        public double ProgressFraction { get; set; } // 0.0 to 1.0
        public string NextStopName { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = "En Route (Simulated)";
        public bool IsSimulated => true;
    }

    public enum JourneyLegType
    {
        Walking,
        MunicipalBus,
        IntercityCoach,
        Marshrutka,
        TransferWait
    }

    public class JourneyLeg
    {
        public JourneyLegType LegType { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public double FromLatitude { get; set; }
        public double FromLongitude { get; set; }
        public double ToLatitude { get; set; }
        public double ToLongitude { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal? FareGEL { get; set; }
        public string ServiceIdentifier { get; set; } = string.Empty; // e.g. "Kutaisi Bus 1" or "Citybus Georgia"
        public string Instructions { get; set; } = string.Empty;
    }

    public class JourneyItinerary
    {
        public string ItineraryId { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Title { get; set; } = string.Empty;
        public string OriginCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public int TotalDurationMinutes { get; set; }
        public decimal TotalFareGEL { get; set; }
        public int TransferCount { get; set; }
        public int TotalWalkingMinutes { get; set; }
        public List<JourneyLeg> Legs { get; set; } = new();
        public string ReliabilityWarning { get; set; } = string.Empty;

        public string SummaryText =>
            $"{DepartureTime:hh\\:mm} → {ArrivalTime:hh\\:mm} ({TotalDurationMinutes / 60}h {TotalDurationMinutes % 60:D2}m) | {TransferCount} transfer(s) | {TotalFareGEL:F2} ₾";
    }
}
