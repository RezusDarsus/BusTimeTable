# 5-Minute University Demonstration Guide

**Project:** Georgia Transit Intelligence — C# Windows Forms Midterm  
**Student:** Rezo Darsavelidze  
**Student Topic:** Topic 13 — Bus Timetable (`dbo.BusRoutes`)  
**Assessment Target:** 25 points maximum, including any CSV bonus
**Submission Package:** `Midterm_Darsavelidze_Rezo.zip`  

---

## ⏱ Minute 1: Introduction & Timetable Manager

1. **Launch the application:**
   - Press <kbd>F5</kbd> in Visual Studio 2026. The application opens directly to the **Timetable Manager** tab. Check that the footer says **LocalDB [Live Connected]** before starting the graded demo.
2. **Explain the Architecture:**
   - *"This application manages a single SQL Server table, `dbo.BusRoutes`, in `BusTimetableDb` using Microsoft SQL Server LocalDB and `Microsoft.Data.SqlClient`."*
   - *"The grid displays 18 realistic routes with columns formatted properly, and the internal database `Id` column is hidden."*
3. **Demonstrate Search & Filter Together (Requirements 8 & 9):**
   - In the search box `txtSearch`, type `Zugdidi`.
   - Observe the grid update in real-time to show three matching routes in the fresh database.
   - Now in `cmbFilter`, select **Express**.
   - Observe the list narrow to one Express route, `GEO-108`.
   - Clear the search box and set filter back to **All** to restore all 18 routes.
4. **Demonstrate SQL Injection Protection (Requirement 5):**
   - Type `' OR 1=1 --` in the search box.
   - Point out to the marker: *"Because all user inputs reach SQL Server via parameterized queries (`command.Parameters.AddWithValue`), this malicious input is treated as a literal search string, returning 0 rows safely without error."*

---

## ⏱ Minute 2: CRUD Operations (Requirement 7)

1. **Add Route (Create):**
   - Click **+ Add Route** button.
   - The `BusRouteForm` opens in Add mode.
   - Fill in:
     - Route Number: `GEO-200`
     - Origin: `Kutaisi (Chavchavadze)`
     - Destination: `Kobuleti (Beach)`
     - Departure Time: `11:45`
     - Duration: `110`
     - Price: `14.50`
     - Express: Checked
   - Click **Save**.
   - Point out the new row and the `lblStatus` confirmation. Close and reopen the app and search for `GEO-200` to show that it persisted.
2. **Edit Route (Update):**
   - Double-click the row for `GEO-200` (or select it and click **Edit**).
   - Point out: The dialog opens **pre-populated** with all existing values.
   - Change Destination to `Kobuleti (Central Station)` and Price to `16.00`.
   - Click **Save**.
   - Point out the updated row and status. Reopen the app and search for `GEO-200` to confirm the saved destination and price.
3. **Cancel Verification:**
   - Double-click `GEO-200` again.
   - Change Price to `99.00`, but click **Cancel** (or press <kbd>Esc</kbd>).
   - Point out: No changes were saved.
4. **Delete Route (Delete):**
   - Select `GEO-200` and click **Delete**.
   - A confirmation dialog appears: *"Are you sure you want to delete route 'GEO-200'...?"*
   - Click **No** → Nothing is deleted.
   - Click **Delete** again and click **Yes** → Reopen the app and search for `GEO-200` to confirm the row is gone.

---

## ⏱ Minute 3: Code Quality, Validation & Bonus CSV Export

1. **Input Validation (Requirement 10):**
   - Click **+ Add Route**.
   - Enter a route number, leave Origin blank, and click **Add Route**. A warning dialog appears: *"Origin station cannot be empty."* The dialog stays open with user data intact.
   - Enter `Kutaisi` for both Origin and Destination and click **Save**. A warning dialog appears: *"Origin and destination cannot be identical..."*
2. **Explain Model & Calculated Property (Requirement 2):**
   - In Visual Studio, show `BusRoute.cs`:
     ```csharp
     public string ServiceType => IsExpress ? "Express" : "Regular";
     ```
   - Explain: *"This is a calculated get-only property that is computed on the fly from `IsExpress` without existing as a separate database column."*
   - Show `dgvBusRoutes_CellFormatting`: Express routes are highlighted with a soft blue tint (`Color.FromArgb(238, 244, 255)`).
3. **Bonus CSV Export (+3 Points):**
   - Type `Mestia` in the search box to find the row containing `სეტის მოედანი`.
   - Click **Export CSV**.
   - Choose a filename (e.g. `Tbilisi_Routes.csv`) and save.
   - Open the file in Notepad or Excel: show that only the filtered rows were exported, headers are included, and Georgian characters (`სეტის მოედანი`) render cleanly due to UTF-8 with BOM.

---

## ⏱ Minute 4: Interactive National Simulation Map

1. Click the **National Live Simulation** tab.
2. Show the interactive map:
   - *"This is a custom double-buffered vector map of Georgia."*
   - Drag with the mouse to pan; use the mouse wheel to zoom in and out.
   - Click **Georgia View** to fit the country.
   - Observe moving simulated vehicle markers:
     - Blue markers: Kutaisi municipal city buses
     - Purple markers: Intercity express coaches (Citybus Georgia, Omnibus)
     - Orange markers: Regional marshrutkas
3. Click any moving vehicle:
   - The **Vehicle Inspector** on the right displays: Operator, Route Number, Scheduled Departure, Estimated Arrival, and Next Stop.
   - Emphasize: *"The clock uses Georgia time. The map estimates vehicle positions using timetable entries and sample municipal frequencies."*
4. Click **Kutaisi City**:
   - The map zooms into Kutaisi, displaying detailed municipal routes, 450 municipal bus stops, and city buses navigating between stops.

---

## ⏱ Minute 5: Multimodal Journey Planner & Network Explorer

1. Click the **Georgia Journey Planner** tab.
2. Select:
   - Origin: `Kutaisi (Red Bridge)`
   - Destination: `Batumi`
   - Departure Time: `08:00`
3. Click **Find Multimodal Journeys**:
   - Open a multimodal result and explain the estimated walking and municipal legs, the transfer buffer, and the intercity departure. The result includes a reliability note.
4. Click **▶ Simulate My Journey on Map**:
   - The map automatically loads the journey path, highlights the route in purple, sets the simulation clock to departure, and begins the animated journey!
5. Briefly show **Network Explorer**:
   - Browse Kutaisi municipal bus patterns, stop sequences with GPS coordinates, intercity services, and the Smart Departure Board.
