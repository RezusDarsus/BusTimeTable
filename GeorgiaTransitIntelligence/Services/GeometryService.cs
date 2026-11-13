using System.Drawing;

namespace GeorgiaTransitIntelligence.Services
{
    /// <summary>
    /// Geographic calculations, road polylines, and spatial interpolation for Georgian transport.
    /// Fulfills Section 10 and 11 of Master Codex with real road curves and highway alignments.
    /// </summary>
    public static class GeometryService
    {
        // Verified coordinates of major Georgian transport nodes and cities
        public static readonly Dictionary<string, (double Lat, double Lon)> KnownCities = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Tbilisi", (41.6938, 44.8015) },
            { "Tbilisi (Ortachala)", (41.6750, 44.8350) },
            { "Tbilisi (Didube)", (41.7508, 44.7788) },
            { "Tbilisi (Pushkin Sq)", (41.6975, 44.8008) },
            { "Kutaisi", (42.2662, 42.7180) },
            { "Kutaisi (Chavchavadze)", (42.2578, 42.6678) },
            { "Kutaisi (Red Bridge)", (42.2712, 42.6996) },
            { "Kutaisi Airport (KUT)", (42.1800, 42.4820) },
            { "Tabukashvili Street 217", (42.2748, 42.6845) },
            { "Tabukashvili", (42.2748, 42.6845) },
            { "Kutaisi International University (KIU)", (42.2382, 42.7480) },
            { "KIU", (42.2382, 42.7480) },
            { "Bagrati Cathedral", (42.2772, 42.7042) },
            { "Gelati Monastery", (42.2964, 42.7683) },
            { "Nikea", (42.2420, 42.6910) },
            { "White Bridge", (42.2690, 42.7010) },
            { "Tsereteli University", (42.2660, 42.7080) },
            { "Omnibus Terminal", (42.2578, 42.6678) },
            { "Batumi", (41.6434, 41.6399) },
            { "Batumi (Central)", (41.6580, 41.6770) },
            { "Batumi (New Station)", (41.6610, 41.6810) },
            { "Batumi (Radisson)", (41.6520, 41.6380) },
            { "Zugdidi", (42.5088, 41.8709) },
            { "Zugdidi (Bus Station)", (42.5120, 41.8740) },
            { "Poti", (42.1462, 41.6720) },
            { "Poti (Bus Station)", (42.1490, 41.6750) },
            { "Gori", (41.9842, 44.1158) },
            { "Gori (Comfort Bus)", (41.9810, 44.1120) },
            { "Khashuri", (41.9942, 43.5999) },
            { "Zestafoni", (42.1100, 43.0333) },
            { "Samtredia", (42.1626, 42.3417) },
            { "Akhaltsikhe", (41.6390, 42.9826) },
            { "Akhaltsikhe (Central)", (41.6410, 42.9850) },
            { "Telavi", (41.9198, 45.4731) },
            { "Telavi (Central)", (41.9210, 45.4750) },
            { "Borjomi", (41.8398, 43.3850) },
            { "Borjomi (Park Station)", (41.8410, 43.3870) },
            { "Mestia", (43.0440, 42.7297) },
            { "Mestia (Seti Square)", (43.0450, 42.7310) },
            { "Kazbegi", (42.6567, 44.6433) },
            { "Kazbegi (Stepantsminda)", (42.6570, 44.6440) },
            { "Ureki", (41.9917, 41.7761) },
            { "Kobuleti", (41.8111, 41.7750) }
        };

        // Real curving E60 Highway corridor tracing actual road bends through Georgia:
        // Tbilisi -> Didube -> Mtskheta -> Ksani -> Igoeti -> Gori -> Khashuri -> Surami Pass -> Rikoti Tunnel -> Ubisa -> Zestafoni -> Kutaisi -> Samtredia -> Lanchkhuti -> Ureki -> Kobuleti -> Batumi
        public static readonly List<(double Lat, double Lon)> E60HighwayCorridor = new()
        {
            (41.6750, 44.8350), // Tbilisi Ortachala
            (41.7210, 44.7990), // Tbilisi Central Station
            (41.7508, 44.7788), // Tbilisi Didube Terminal
            (41.8020, 44.7550), // Zahesi
            (41.8410, 44.7150), // Mtskheta (Jvari View)
            (41.9180, 44.7170), // Natakhtari
            (41.9450, 44.5700), // Ksani / Mukhrani turn
            (41.9950, 44.4050), // Igoeti
            (42.0120, 44.2550), // Nadarbazevi
            (41.9842, 44.1158), // Gori E60 Interchange
            (42.0250, 43.9550), // Ruisi / Urbnisi
            (42.0200, 43.8900), // Kareli junction
            (42.0350, 43.8200), // Agara
            (41.9942, 43.5999), // Khashuri Central
            (42.0250, 43.5500), // Surami (Pass entrance)
            (42.0420, 43.5150), // Chumateleti
            (42.0550, 43.4850), // Rikoti Tunnel
            (42.0720, 43.3750), // Khevi
            (42.0950, 43.2350), // Ubisa Monastery
            (42.1050, 43.0850), // Shorapani
            (42.1100, 43.0333), // Zestafoni E60
            (42.1550, 42.9200), // Argveta
            (42.1950, 42.8550), // Simoneti
            (42.2250, 42.7500), // Rioni River Bridge
            (42.2578, 42.6678), // Kutaisi Chavchavadze Bus Terminal
            (42.1800, 42.4820), // Kopitnari (Kutaisi International Airport KUT)
            (42.1626, 42.3417), // Samtredia Junction
            (42.1050, 42.2050), // Sajavakho
            (42.0880, 42.0350), // Lanchkhuti (Guria)
            (42.0620, 41.8750), // Supsa
            (42.0450, 41.7450), // Grigoleti (Black Sea Coast)
            (41.9917, 41.7761), // Ureki Beach
            (41.9250, 41.7700), // Shekvetili
            (41.8850, 41.7850), // Natanebi
            (41.8111, 41.7750), // Kobuleti Central
            (41.7580, 41.7500), // Tsikhisdziri (Petra Fortress)
            (41.7220, 41.7280), // Chakvi
            (41.6920, 41.7050), // Mtsvane Kontskhi (Botanical Garden)
            (41.6720, 41.6920), // Makhinjauri
            (41.6580, 41.6770), // Batumi Central Station
            (41.6434, 41.6399)  // Batumi Old City Center
        };

        // Highway S1 (Samtredia -> Zugdidi)
        public static readonly List<(double Lat, double Lon)> S1ZugdidiCorridor = new()
        {
            (42.1626, 42.3417), // Samtredia
            (42.2050, 42.2050), // Abasha
            (42.2680, 42.0670), // Senaki
            (42.3180, 41.9050), // Khobi
            (42.4150, 41.8850), // Khamiskuri
            (42.5088, 41.8709)  // Zugdidi Central Bus Station
        };

        // Highway S12 (Grigoleti -> Poti)
        public static readonly List<(double Lat, double Lon)> S12PotiCorridor = new()
        {
            (42.0450, 41.7450), // Grigoleti
            (42.1150, 41.7050), // Maltakva
            (42.1462, 41.6720)  // Poti Central Station
        };

        // Highway S8 (Khashuri -> Borjomi -> Akhaltsikhe)
        public static readonly List<(double Lat, double Lon)> S8AkhaltsikheCorridor = new()
        {
            (41.9942, 43.5999), // Khashuri
            (41.9250, 43.4900), // Akhaldaba
            (41.8398, 43.3850), // Borjomi
            (41.8250, 43.3450), // Likani
            (41.7250, 43.1650), // Atskuri
            (41.6390, 42.9826)  // Akhaltsikhe
        };

        // Highway S5 (Tbilisi -> Sagarejo -> Telavi)
        public static readonly List<(double Lat, double Lon)> S5TelaviCorridor = new()
        {
            (41.6750, 44.8350), // Tbilisi Ortachala
            (41.7150, 45.1850), // Sartichala
            (41.7330, 45.3330), // Sagarejo
            (41.6950, 45.7450), // Bakurtsikhe
            (41.7450, 45.8000), // Gurjaani
            (41.9198, 45.4731)  // Telavi
        };

        // Highway S3 (Mtskheta -> Gudauri -> Kazbegi)
        public static readonly List<(double Lat, double Lon)> S3KazbegiCorridor = new()
        {
            (41.8410, 44.7150), // Mtskheta
            (42.1350, 44.7700), // Zhinvali Reservoir
            (42.1630, 44.7030), // Ananuri Fortress
            (42.3500, 44.6900), // Pasanauri
            (42.4770, 44.4850), // Gudauri
            (42.5050, 44.4550), // Jvari Pass (2,379m)
            (42.5550, 44.5050), // Kobi
            (42.6567, 44.6433)  // Stepantsminda (Kazbegi)
        };

        public static List<(double Lat, double Lon)> BuildHighwayCorridor(double fromLat, double fromLon, double toLat, double toLon)
        {
            var list = new List<(double Lat, double Lon)> { (fromLat, fromLon) };

            bool toZugdidi = CalculateDistanceKm(toLat, toLon, 42.5088, 41.8709) < 35;
            bool toPoti = CalculateDistanceKm(toLat, toLon, 42.1462, 41.6720) < 25;
            bool toSouth = CalculateDistanceKm(toLat, toLon, 41.6390, 42.9826) < 35 || CalculateDistanceKm(toLat, toLon, 41.8398, 43.3850) < 25;
            bool toKazbegi = CalculateDistanceKm(toLat, toLon, 42.6567, 44.6433) < 35;
            bool toTelavi = CalculateDistanceKm(toLat, toLon, 41.9198, 45.4731) < 35;

            bool fromKazbegi = CalculateDistanceKm(fromLat, fromLon, 42.6567, 44.6433) < 35;
            bool fromTelavi = CalculateDistanceKm(fromLat, fromLon, 41.9198, 45.4731) < 35;

            if (toTelavi)
            {
                list.AddRange(S5TelaviCorridor);
            }
            else if (fromTelavi)
            {
                for (int i = S5TelaviCorridor.Count - 1; i >= 0; i--) list.Add(S5TelaviCorridor[i]);
            }
            else if (toKazbegi)
            {
                list.AddRange(S3KazbegiCorridor);
            }
            else if (fromKazbegi)
            {
                for (int i = S3KazbegiCorridor.Count - 1; i >= 0; i--) list.Add(S3KazbegiCorridor[i]);
            }
            else
            {
                int closestFromIdx = 0;
                double minDistFrom = double.MaxValue;
                int closestToIdx = 0;
                double minDistTo = double.MaxValue;

                for (int i = 0; i < E60HighwayCorridor.Count; i++)
                {
                    var pt = E60HighwayCorridor[i];
                    double d1 = CalculateDistanceKm(fromLat, fromLon, pt.Lat, pt.Lon);
                    if (d1 < minDistFrom) { minDistFrom = d1; closestFromIdx = i; }
                    double d2 = CalculateDistanceKm(toLat, toLon, pt.Lat, pt.Lon);
                    if (d2 < minDistTo) { minDistTo = d2; closestToIdx = i; }
                }

                if (closestFromIdx <= closestToIdx)
                {
                    for (int i = closestFromIdx; i <= closestToIdx; i++) list.Add(E60HighwayCorridor[i]);
                }
                else
                {
                    for (int i = closestFromIdx; i >= closestToIdx; i--) list.Add(E60HighwayCorridor[i]);
                }

                if (toZugdidi) list.AddRange(S1ZugdidiCorridor);
                if (toPoti) list.AddRange(S12PotiCorridor);
                if (toSouth) list.AddRange(S8AkhaltsikheCorridor);
            }

            list.Add((toLat, toLon));
            return list;
        }

        public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public static int EstimateWalkingMinutes(double distanceKm)
        {
            double minutes = (distanceKm / 4.8) * 60.0;
            return Math.Max(1, (int)Math.Ceiling(minutes));
        }

        public static (double Lat, double Lon) GetCityCoordinates(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return (42.2662, 42.7180);

            string clean = cityName.Trim();

            if (KnownCities.TryGetValue(clean, out var coords))
                return coords;

            foreach (var kvp in KnownCities)
            {
                if (clean.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Contains(clean, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return (42.2662, 42.7180);
        }

        public static (double Lat, double Lon, double Heading) InterpolateAlongPath(
            IReadOnlyList<(double Lat, double Lon)> points, double progressFraction)
        {
            if (points == null || points.Count == 0)
                return (42.2662, 42.7180, 0);

            if (points.Count == 1 || progressFraction <= 0.0)
                return (points[0].Lat, points[0].Lon, 0);

            if (progressFraction >= 1.0)
            {
                int last = points.Count - 1;
                double heading = CalculateBearing(points[last - 1].Lat, points[last - 1].Lon, points[last].Lat, points[last].Lon);
                return (points[last].Lat, points[last].Lon, heading);
            }

            double[] segmentDistances = new double[points.Count - 1];
            double totalDistance = 0.0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                double d = CalculateDistanceKm(points[i].Lat, points[i].Lon, points[i + 1].Lat, points[i + 1].Lon);
                segmentDistances[i] = d;
                totalDistance += d;
            }

            if (totalDistance <= 0.0001)
                return (points[0].Lat, points[0].Lon, 0);

            double targetDistance = progressFraction * totalDistance;
            double accumulated = 0.0;

            for (int i = 0; i < segmentDistances.Length; i++)
            {
                if (accumulated + segmentDistances[i] >= targetDistance || i == segmentDistances.Length - 1)
                {
                    double segDist = segmentDistances[i];
                    double segFraction = segDist > 0 ? (targetDistance - accumulated) / segDist : 0;
                    segFraction = Math.Clamp(segFraction, 0.0, 1.0);

                    double lat = points[i].Lat + segFraction * (points[i + 1].Lat - points[i].Lat);
                    double lon = points[i].Lon + segFraction * (points[i + 1].Lon - points[i].Lon);
                    double heading = CalculateBearing(points[i].Lat, points[i].Lon, points[i + 1].Lat, points[i + 1].Lon);

                    return (lat, lon, heading);
                }
                accumulated += segmentDistances[i];
            }

            int endIdx = points.Count - 1;
            return (points[endIdx].Lat, points[endIdx].Lon, 0);
        }

        public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
        {
            double rLat1 = ToRadians(lat1);
            double rLat2 = ToRadians(lat2);
            double dLon = ToRadians(lon2 - lon1);

            double y = Math.Sin(dLon) * Math.Cos(rLat2);
            double x = Math.Cos(rLat1) * Math.Sin(rLat2) - Math.Sin(rLat1) * Math.Cos(rLat2) * Math.Cos(dLon);
            double brng = Math.Atan2(y, x);

            return (ToDegrees(brng) + 360.0) % 360.0;
        }

        public static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
        public static double ToDegrees(double radians) => radians * (180.0 / Math.PI);
    }
}
