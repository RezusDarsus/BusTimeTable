# Georgia Transit Intelligence — National Bus Network & Timetable Control Center

**Kutaisi International University (KIU) — Object-Oriented Programming (C#) Midterm Project**  
**Student:** Rezo Darsavelidze  
**Topic 13:** Bus Timetable (`dbo.BusRoutes`)  
**Target Platform:** Visual Studio 2026 / Modern .NET Windows Forms (C#)  
**Database:** Microsoft SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)  
**Submission Archive:** `Midterm_Darsavelidze_Rezo.zip`  

---

## 1. Project Overview

**Georgia Transit Intelligence** is a comprehensive transportation management and intelligence desktop application built in C# Windows Forms. The Timetable Manager implements the KIU Topic 13 requirements and the optional CSV export. The journey planner, simulation, and network explorer use bundled Georgian intercity and Kutaisi municipal data.

The system provides:
1. **Timetable Manager (Graded University CRUD):** Complete management of the graded `BusRoutes` SQL Server table, including real-time search, Express/Regular filtering, full Add/Edit/Delete persistence, input validation, and CSV export.
2. **Georgia Journey Planner:** Multimodal journey routing combining Kutaisi municipal buses, walking transfers, and intercity coaches/marshrutkas with minimum transfer buffers and price calculations.
3. **National Live Simulation:** Interactive vector map of Georgia displaying timetable-based simulated vehicle movements (Citybus, Omnibus, Georgian Bus, regional marshrutkas, and Kutaisi city buses).
4. **Network Explorer & Departure Board:** Detailed inspection of Kutaisi's 22 municipal routes, 44 directions, 450 stops, 182,291 timetable entries, 19 intercity operators, and a departure board for published Kutaisi stop times.

---

## 2. Database Schema

**Database Name:** `BusTimetableDb`  
**Table Name:** `dbo.BusRoutes`

| Column Name | SQL Type | C# Type | Description / Control |
|---|---|---|---|
| `Id` | `INT IDENTITY(1,1)` | `int` | Primary Key (hidden in DataGridView) |
| `RouteNumber` | `NVARCHAR(20) NOT NULL` | `string` | Route identifier (e.g. `GEO-101`, `34`) |
| `Origin` | `NVARCHAR(100) NOT NULL` | `string` | Origin city/terminal station (`TextBox`) |
| `Destination` | `NVARCHAR(100) NOT NULL` | `string` | Destination city/terminal (`TextBox`) |
| `DepartureTime` | `TIME NOT NULL` | `TimeSpan` | Scheduled departure time (`DateTimePicker` Time format) |
| `DurationMinutes` | `INT NOT NULL` | `int` | Journey duration in minutes (`NumericUpDown`) |
| `Price` | `DECIMAL(8,2) NOT NULL` | `decimal` | Ticket fare in GEL (`NumericUpDown` 2 decimal places) |
| `IsExpress` | `BIT NOT NULL` | `bool` | Express service indicator (`CheckBox`) |

### Calculated Properties (Requirement 2)
- `ServiceType` (`string`): Returns `"Express"` when `IsExpress == true`, otherwise `"Regular"`.
- `EstimatedArrival` (`TimeSpan`): Calculated as `DepartureTime + DurationMinutes`, correctly handling midnight rollover via modulo 24 hours.
- `EstimatedArrivalTime` (`string`): Formatted arrival time (`hh:mm`).
- `DurationDisplay` (`string`): Formatted duration in hours and minutes (e.g. `3h 40m`).

---

## 3. How to Run the Project (Marker Instructions)

Use Visual Studio 2026 with .NET desktop development, the .NET 9 Windows Desktop runtime and targeting support, and SQL Server Express LocalDB. If only the .NET 10 runtime is installed, the app still starts (`RollForward=Major` in the project file).

1. Extract `Midterm_Darsavelidze_Rezo.zip` to a new folder and open Visual Studio 2026.
2. Go to **View** → **SQL Server Object Explorer**.
3. Right-click `(localdb)\MSSQLLocalDB` and select **New Query**.
4. Open or paste `GeorgiaTransitIntelligence/database.sql` (or `database.sql` in the solution root) into the query window and press **Execute** (or press <kbd>Ctrl+Shift+E</kbd>).
5. Open the solution file: `GeorgiaTransitIntelligence.sln`.
6. Click **Build** → **Rebuild Solution** (verify 0 errors, 0 warnings).
7. Press <kbd>F5</kbd>. On a fresh database, the Timetable Manager shows 18 rows and the footer reports that LocalDB is connected. Running the SQL script a second time keeps the same seed row count.

If the footer says **Demo mode**, the app is using sample data and edits last until the app closes. Run the SQL script and relaunch before demonstrating persistence. Use `Zugdidi` for the three-result search and choose **Express** to narrow it to one row. Use `Mestia` to export a row with Georgian text.

---

## 4. University Midterm Features Checklist

| # | University Requirement | Implementation Location | Implementation |
|---|---|---|---|
| **1** | Project & Reproducible Database (3 pts) | `database.sql`, `GeorgiaTransitIntelligence.csproj` | Creates `BusTimetableDb` and `BusRoutes` with five SQL data types. The guarded script can be rerun. |
| **2** | C# Model & Calculated Property (1 pt) | `Models/BusRoute.cs` | Singular `BusRoute` model with matching properties and calculated `ServiceType` (`IsExpress ? "Express" : "Regular"`). |
| **3** | Test Data (2 pts) | `database.sql` | Inserts 18 realistic Georgian bus routes (>= 15 required) with 8 Express and 10 Regular routes. Topic 13 uses both BIT values. |
| **4** | Repository Architecture (2 pts) | `Data/BusRouteRepository.cs` | All SQL isolated in `BusRouteRepository`. Connection string defined once. Every connection/command in `using`. No SQL inside forms. |
| **5** | Parameterized SQL (2 pts) | `Data/BusRouteRepository.cs` | `@text`, `@pattern`, `@isExpress`, `@routeNumber`, etc. Testing with `' OR 1=1 --` produces 0 rows safely without SQL injection. |
| **6** | Two Windows (3 pts) | `MainForm.cs`, `BusRouteForm.cs` | `MainForm` + `BusRouteForm` with two constructors (`: this()`). Named controls (`txtSearch`, `btnAdd`, `dgvBusRoutes`). Anchored grid. Row color highlighting for Express routes in `CellFormatting`. |
| **7** | Working CRUD (6 pts) | `MainForm.cs`, `BusRouteRepository.cs` | **Add** persists new row. **Read** displays all columns with hidden Id. **Edit** opens pre-populated dialog. **Delete** prompts confirmation (`MessageBoxButtons.YesNo`). Status messages updated. |
| **8** | Search (3 pts) | `MainForm.cs`, `BusRouteRepository.cs` | Searches Origin and Destination using SQL `LIKE @pattern`. Empty box restores all records. No match shows friendly status. |
| **9** | Filter (1 pt) | `MainForm.cs`, `BusRouteRepository.cs` | `cmbFilter` ("All", "Express", "Regular") works simultaneously with `txtSearch`. |
| **10** | Input Checks (1 pt) | `BusRouteForm.cs` | Checks `IsNullOrWhiteSpace`, numeric ranges, and custom rule preventing identical Origin and Destination. Dialog remains open on failed validation. |
| **11** | Code Quality & README (1 pt) | Solution & `README.md` | Clean indentation, descriptive naming, zero default names (`textBox1`), and complete documentation. |
| **+** | **Bonus: CSV Export (+3 pts)** | `Services/CsvExportService.cs` | Exports currently filtered grid rows with `SaveFileDialog`. UTF-8 with BOM preserves Georgian text in Excel. The assignment caps the total at 25 points. |

---

## 5. Advanced Differentiating Features (Priority B)

- **Interactive Vector Map (`GeoMapControl`):** Native double-buffered GDI+ vector map with real coordinates of Georgia, highway corridors (E60, S1, S2), cities, and all 450 Kutaisi municipal stops. Smooth mouse pan and zoom wheel.
- **Timetable-Based Vehicle Simulation:** Estimates vehicle positions from bundled intercity schedules and example municipal frequencies (Georgia UTC+4 time). Configurable speeds (1x, 5x, 15x, 60x), play/pause, time reset, and zoom-aware level-of-detail. Explicitly labeled: *Timetable Simulation — Estimated Vehicle Positions*.
- **Marshrutka (Fixed-Route Taxicab / სამარშრუტო ტაქსი) Integration:** Dedicated styling (Amber Orange `#ea580c`), vehicle badges (`MRSH`), Mercedes-Benz Sprinter fleet specifications, and 25 realistic regional route taxi corridors (LTD Okriba, LTD Imertrans, LTD Sprinter, Didube Minibus Terminal, Kutaisi Central Hub, Kazbegi, Mestia, Tskaltubo, Sairme, Martvili, etc.).
- **Complete Kutaisi Municipal Network:** Full integration of all 22 municipal bus lines (Lines 1 to 34), 44 directional patterns, 450 verified stops, and 182,291 timetable departures.
- **Multimodal Journey Planner:** Combines walking, Kutaisi municipal buses, and intercity coaches/marshrutkas (Citybus, Omnibus, Georgian Bus, Marshrutkas) with estimated municipal times, a transfer buffer, and fare estimates. Includes "Simulate My Journey" feature that replays the itinerary on the live map.
- **Network Explorer & Departure Board:** Complete directory of Kutaisi municipal network and Georgian intercity network with a countdown to published municipal stop times using the simulation clock.

---

### Street map

In **National Live Simulation**, use the map dropdown to switch the Windows app between Schematic, OpenStreetMap, and Yandex Maps. The localhost preview starts with OpenStreetMap. Street tiles load over the internet, while the schematic is available as a fallback. Vehicle positions are simulated in both views. Map data © [OpenStreetMap contributors](https://www.openstreetmap.org/copyright).

For **Yandex Maps** in Windows, select it in the map dropdown and enter a Yandex Tiles API key authorized for this device. You can also supply it through the GTI_YANDEX_TILES_API_KEY environment variable. The desktop uses the Tiles API and no localhost server. The browser integration uses a JavaScript API key.

To use **Yandex Maps** in the localhost preview, serve the preview with `python -m http.server 8000 --directory web-preview` and open `http://localhost:8000/`. In **National Live Simulation**, select **Yandex Maps** and enter a [Yandex JavaScript API key](https://yandex.com/maps-api/docs/js-api/quickstart.html) with `localhost` in its HTTP Referer restrictions. The key stays in the current page session. Vehicle positions and the inspector use the existing simulation.

## 6. Automated Testing

The project includes an automated test suite in `GeorgiaTransitIntelligence.Tests`:
```powershell
dotnet test GeorgiaTransitIntelligence.sln
```
The suite covers model calculations, CRUD, search and filtering, CSV encoding, published departures, journey planning, simulation, and rendered WinForms screenshots. To require SQL Server instead of allowing the session-only repository, run the following after initializing LocalDB:

```powershell
$env:GTI_REQUIRE_LOCALDB = '1'
dotnet test GeorgiaTransitIntelligence.sln --filter FullyQualifiedName~Requirement7_CrudOperations_PersistCorrectly
Remove-Item Env:GTI_REQUIRE_LOCALDB
```

The persistence test opens new repository instances after Add, Edit, and Delete. For the lab demo, also close and reopen the app between changes and show the LocalDB connection in the footer. See [the demo guide](docs/DEMO_GUIDE.md).

## 7. Transit Data and Refresh

The Windows Forms app reads Kutaisi municipal routes from `data/kutaisi-municipal/route_patterns.csv`, `route_stops.csv`, and `timetable.csv`. The timetable is a flat UTF-8 CSV with one row per published stop time and weekday; the app indexes it for the municipal departure board. The neighboring `data/georgian-intercity/` directory contains the separate intercity dataset.

The Kutaisi files were extracted from the public schedule service used by Kutaisi Transport. To refresh them, run:

```powershell
python scripts/extract_kutaisi_timetable.py
```

The script retrieves the public client configuration from the service page at run time and does not write the client key to disk. It refreshes the flat CSV, normalized JSON, SQL Server seed, route patterns, ordered stops, coverage checks, and extraction report. The JSON `recordColumns` array defines compact timetable rows; the route, pattern, stop, and coverage records provide the joined fields. `data/kutaisi-municipal/EXTRACTION_REPORT.md` records the source date, weekday coverage, and limitations. The published weekly schedule did not expose a separate public-holiday day type.

The app locates repository data by searching parent directories from its executable and working directories, so a checkout does not depend on a fixed local path. `web-preview/index.html` is a static browser preview of the Kutaisi network.
