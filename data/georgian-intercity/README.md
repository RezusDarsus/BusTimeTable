# Georgian Intercity Bus Routes — research snapshot

**Research date:** 2026-11-13  
**Use:** Importable, single-table schedule observations plus a much broader route/operator coverage inventory. This is a source snapshot, not a live timetable.

## Deliverables

- `georgian-intercity-routes.xlsx` — Summary, Services, Route Coverage, Operators, Sources and Destinations sheets.
- `georgian-intercity-routes.json` — same data, provenance and limitations in structured form.
- `georgian-intercity-services.sql` — SQL Server CREATE TABLE and seed INSERT for a generic flat services table.

## Coverage counts

| Measure | Count |
|---|---:|
| Named transport companies/operators discovered | 14 |
| Georgian domestic/regional providers among them | 9 |
| International operators listed by the Tbilisi terminal | 5 |
| Destination nodes listed by the regional route directory | 146 |
| Destination nodes in this bundle (including airports and cross-border destinations) | 168 |
| Source-backed service records (timed departures and published frequency patterns) | 585 |
| Primary/institutional source records | 194 |
| Third-party line-detail schedule records (not operator-confirmed) | 391 |
| Other secondary-source schedule records | 0 |
| Service rows missing fare | 130 |
| Service rows missing arrival time | 526 |
| Service rows missing duration | 507 |
| Service rows missing both origin and arrival stations | 367 |
| Service rows missing both operating days and dates | 385 |
| Exact duplicate schedule tuples | 0 |
| Explicit departure-time records | 520 |
| Published frequency-pattern records, not expanded into guessed trips | 65 |
| Synthetic service records | 0 |
| Confirmed directional OD pairs (route evidence, any named operator) | 74 |
| Total directional OD pairs when route-directory leads are included | 266 |
| Regional directory line entries retained | 249 |
| Individual route-detail pages fetched successfully | 249 |
| Line pages with fare / departure section | 127 / 156 |
| Unique OD pairs among those directory lines | 229 |

The route directory advertises 246 lines in its heading; its currently accessible HTML yielded 249 unique line links. All linked detail pages were fetched successfully. Pages with fares and departure sections are represented as direct-time rows or frequency patterns with individual page URLs, but remain third-party, community-submitted observations rather than operator-verified active trips. Every index link also remains in Route Coverage. Directional OD counts deduplicate the same origin/destination pair while retaining separate operator rows and route variants in the workbook.

## Operators found

Domestic and regional names: Citybus Georgia, Metro Georgia, Omnibus Express, Georgian Bus, LTD Okriba, LTD Imertrans, LTD Sprinter, Rustavi Intertrans LLC and Tetritskaro Transport Company Ltd. (this name appears only in community route descriptions and is not independently verified). Metro also appears on the terminal list for Istanbul and Izmir. Five foreign-carrier names appear in terminal listings: Luks Karadeniz, Luks Muradoglu, Grand Tours, Green Express and Global Trans. TKT.GE/TRE.GE and Obilet are booking platforms, not carriers. ClassTour was named in a provider FAQ but no route could be tied to it. Station/dispatcher labels and ‘carrier not identified’ are not counted as companies. Easy Way's public schedule is for parcel delivery, not passenger transport.

## Data rules

- Every row in **Services** has a public source URL and an explicit evidence status. Operator competition is kept in separate rows.
- Exact departure times are separate rows. Source frequency windows are kept in `SchedulePattern` rows; the interval is not expanded into synthetic departures.
- Opposite directions are entered only when a source lists them. A return is not inferred from a one-way route.
- Arrival day offset and duration minutes are calculated only when a source lists both departure and arrival times. Approximate ranges remain text.
- Missing fare, station, duration, days or dates are null. Airport, regional and cross-border routes are flagged in route coverage.
- “Source-backed” means the value appears in a cited public source. It does not guarantee that an old or undated listing still operates today. Historical observations are labeled.
- No synthetic records were created.

## Important limitations

- The solution's graded Windows Forms CRUD module uses `BusTimetableDb.dbo.BusRoutes`. The intercity dataset remains separate: the app reads `georgian-intercity-routes.json`, while `georgian-intercity-services.sql` is an optional seed for `dbo.GeorgianIntercityBusServices` and does not insert intercity records into `dbo.BusRoutes`.
- The supplied ticket.ge URL redirects to TRE.GE. TRE provider profiles expose current destination networks, but its dynamic date search does not provide a full all-date timetable in static page text. Its route links are recorded as leads where carrier/service identity is unresolved.
- Citybus, Metro Georgia and Georgian Bus have useful dated route listings, but not every route/date variation could be exhaustively queried. Metro and Georgian Bus examples include past dates; dates are retained per row.
- The terminal's undated schedule snapshot lists Citybus Tbilisi–Batumi at 07:30, while the operator route page lists 08:00. The terminal row is retained with a conflict warning; use the operator page for operational confirmation.
- Okriba's official schedule lists times under its Kutaisi branch but does not label the destination; Kutaisi Travel identifies Okriba's Kutaisi–Tbilisi service and fare but publishes a different frequency window. Both observations and the mismatch are retained. Kutaisi Travel provides other detailed regional times/fare windows but does not state an effective date. The Mestia municipal PDF is also undated. Confirm these schedules before passenger-facing publication.
- Rustavi's 2026 municipal budget gives an aggregate five-line/130-bus-per-day figure, not time-by-time departures. No per-trip timetable was inferred.
- Tbilisi Central lists international operators and starting prices, but trip-level current dates and schedules were not obtained. Cross-border rows are route leads, not a complete verified timetable.
- Public dynamic booking forms did not render a reliable all-date listing in the accessible page text. No CAPTCHA, authentication or access restriction was bypassed.

## Source register

- [Citybus Georgia official site](https://citybus.ge/en) and its route schedule pages linked per service row.
- [Metro Georgia Tbilisi–Batumi](https://metrogeorgia.ge/otobus-bileti/TIFLIS-BATUM), [Batumi–Tbilisi](https://metrogeorgia.ge/otobus-bileti/BATUM-TIFLIS), and [official FAQ](https://www.metrogeorgia.ge/sss).
- [Georgian Bus timetable](https://georgianbus.com/en/page/timetable) and [routes/FAQ](https://georgianbus.com/en/frequently-asked-questions).
- [TKT.GE Omnibus route page](https://tkt.ge/en/omnibus), [ticket.ge (redirects to TRE.GE)](https://ticket.ge/), [TRE.GE City Bus](https://tre.ge/en/citybus), [OmniBus](https://tre.ge/en/omnibus) and [Georgian Bus](https://tre.ge/en/georgianbus) provider profiles.
- [Tbilisi Central schedule](https://busterminal.ge/en/schedule/), [destinations](https://busterminal.ge/en/destinations/) and [transport partners](https://busterminal.ge/en/partners/).
- [Okriba official schedule](https://okriba.ge/en/branches), [Kutaisi regional transport schedule](https://kutaisi.travel/en/transport-schedule-en/) and [Kutaisi Airport public transport](https://kutaisiairport.ge/transport/).
- [Mestia Municipality schedule PDF](https://mestia.gov.ge/sites/default/files/sakalaktashoriso_transporti.pdf) and [ProSvaneti schedule guide](https://prosvaneti.ge/en/blog/marshrutka-to-mestia).
- [Georgia Marshrutka route directory](https://marshrutka.info/lines) and [city directory](https://marshrutka.info/cities). [Easy Way parcel delivery zones](https://www.easyway.ge/en/schedule) were checked and excluded as non-passenger service.
- [National Parks transport information for Borjomi](https://www.nationalparks.ge/en/master/transportation/borjomi-kharagaulinp), [Rustavi Municipality](https://rustavi.gov.ge/sachiro-informacia/) and its [2026–2029 budget](https://rustavi.gov.ge/wp-content/uploads/2025/11/3-2026-%E1%83%AC%E1%83%9A%E1%83%98%E1%83%A1-%E1%83%9E%E1%83%A0%E1%83%98%E1%83%9D%E1%83%A0%E1%83%98%E1%83%A2%E1%83%94%E1%83%A2%E1%83%94%E1%83%91%E1%83%98%E1%83%A1-%E1%83%93%E1%83%9D%E1%83%99%E1%83%A3%E1%83%9B%E1%83%94%E1%83%9c%E1%83%A2%E1%83%98.pdf).

## SQL Server integration

The SQL seed creates one table, `dbo.GeorgianIntercityBusServices`, with nullable times/fares and in-row provenance. It is compatible with a simple flat CRUD workflow, but it is **not** an insert into the absent application's table. Keep `EvidenceStatus` in the app if you import these rows so historical, undated, and secondary observations remain distinguishable.
