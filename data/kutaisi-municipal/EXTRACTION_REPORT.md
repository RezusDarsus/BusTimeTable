# Kutaisi municipal bus timetable extraction report

- **Extracted:** 2026-11-13
- **Source:** [Kutaisi Transport public timetable API](https://kutaisi-bus.msplus.ge/#/schedule).
- **App version observed:** 3.1.3
- **Mode:** municipal city buses (`BUS`); intercity records are excluded.
- **Schedule type:** published timetable; live arrivals were not used.
- **Service dates returned:** 2026-11-13, 2026-11-14, 2026-11-15, 2026-11-16, 2026-11-17, 2026-11-18, 2026-11-19.

## Counts

| Measure | Count |
|---|---:|
| Bus routes | 22 |
| Route patterns/directions | 44 |
| Route + direction ID combinations | 44 |
| Unique stop IDs | 450 |
| Route-pattern-stop combinations | 847 |
| Timetable records | 182291 |
| Verified timetable records | 182291 |
| Stop/day checks | 5929 |
| Checks with published times | 5929 |
| Checks without returned schedule | 0 |
| Route-stop rows missing coordinates | 0 |

## Coverage by weekday

| Day | Time records | Stop/day checks | Published schedule checks | No schedule group returned |
|---|---:|---:|---:|---:|
| Monday | 27693 | 847 | 847 | 0 |
| Tuesday | 27693 | 847 | 847 | 0 |
| Wednesday | 27693 | 847 | 847 | 0 |
| Thursday | 27693 | 847 | 847 | 0 |
| Friday | 27693 | 847 | 847 | 0 |
| Saturday | 21933 | 847 | 847 | 0 |
| Sunday | 21893 | 847 | 847 | 0 |

`coverage.csv` records every route-pattern-stop/day check. Days without a returned schedule group are marked unavailable, not inferred as no-service.

## Holidays

The public schedule response exposed weekly day ranges and service dates, but no distinct public-holiday day type. No holiday schedule was inferred.

## Files

- `timetable.csv`: flat row-per-time export used by the Windows Forms app.
- `route_patterns.csv`: route direction and variation IDs, endpoints, and headsigns.
- `route_stops.csv`: ordered stops, IDs, stop codes, Georgian names, and coordinates.
- `coverage.csv`: one status row per route-pattern-stop/day check.
- `timetable.json`: normalized JSON; `recordColumns` defines each compact timetable row and metadata/stop/pattern records supply the joined fields.
- `seed.sql`: normalized SQL Server tables for routes, patterns, stops, day coverage, and timetable entries.

## Remaining gaps

- 0 route-pattern-stop/day checks have no published times in the API response; see `coverage.csv`.
- Coordinates were returned for every route-pattern-stop row.

## Validation

- Enumerated the BUS routes from the public app route list.
- Queried each route for patterns covering the current week, then queried each pattern's stops and schedule.
- Cross-checked route 3, stop code 244 against the open app screen: Friday 08:00 and 08:30 match the API.
- Compared route-stop order/IDs with timetable stop order/IDs; differences are listed above.
- Collapsed exact duplicate timetable rows by route, pattern, direction, stop position/ID, weekday, and time.
- Source paths: `/pis-gateway/api/v3/routes` and `/pis-gateway/api/v3/routes/{routeId}/schedule?patternSuffix=…&locale=ka`.
