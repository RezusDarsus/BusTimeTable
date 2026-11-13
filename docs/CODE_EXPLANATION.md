# Code Explanation & Object-Oriented Design Guide

**Course:** Object-Oriented Programming (C#) — Kutaisi International University  
**Subject:** Georgia Transit Intelligence (C# Windows Forms)  
**Student:** Rezo Darsavelidze  
**Author:** Student Implementation Reference  

---

## 1. Object-Oriented Architecture & Separation of Concerns

The project strictly separates presentation, business models, and data access into distinct layers:

```
[Windows Forms UI]        <--> [Business Models]     <--> [Data Repositories]
(MainForm, BusRouteForm)      (BusRoute, Stops...)        (BusRouteRepository)
                                                                 |
                                                          [SQL Server LocalDB]
```

### Why forms contain NO SQL
In traditional novice desktop development, database queries are often scattered inside button click handlers. This creates tight coupling, makes testing impossible, and introduces SQL injection risks.
In our architecture:
- `BusRouteRepository` is the **only** class that touches `SqlConnection`, `SqlCommand`, and `SqlDataReader`.
- The forms (`MainForm`, `BusRouteForm`) work purely with strongly typed C# objects (`BusRoute`, `List<BusRoute>`).

---

## 2. Constructor Chaining (`: this()`) in `BusRouteForm`

In `BusRouteForm.cs`, there are two constructors:
```csharp
// Constructor 1: For Adding
public BusRouteForm()
{
    InitializeComponent();
    // Sets up controls, AcceptButton, CancelButton, etc.
}

// Constructor 2: For Editing
public BusRouteForm(BusRoute route) : this()
{
    // Populates controls from the existing route
    txtRouteNumber.Text = route.RouteNumber;
    txtOrigin.Text = route.Origin;
    ...
}
```

### Why `: this()` is critical:
If the second constructor did not call `: this()`, `InitializeComponent()` would never run for edit mode. As a result, all controls (`txtRouteNumber`, `txtOrigin`) would remain `null`, and referencing them would crash the application with `NullReferenceException`. Constructor chaining ensures the form layout and components are instantiated first before populating data.

---

## 3. Parameterized SQL Queries & SQL Injection Immunity

Requirement 5 forbids joining user text into SQL strings with string concatenation (`+`) or string interpolation (`$"{text}"`).

### The Vulnerable Way (Wrong):
```csharp
// VULNERABLE: If user types: ' OR 1=1 --
string sql = "SELECT * FROM BusRoutes WHERE Origin = '" + txtSearch.Text + "'";
// Resulting SQL executes: SELECT * FROM BusRoutes WHERE Origin = '' OR 1=1 --'
// This leaks all records or damages the database!
```

### The Parameterized Way (Our Implementation):
```csharp
string sql = "SELECT * FROM dbo.BusRoutes WHERE (@text = N'' OR Origin LIKE @pattern OR Destination LIKE @pattern)";
using (SqlCommand command = new SqlCommand(sql, connection))
{
    command.Parameters.AddWithValue("@text", cleanText);
    command.Parameters.AddWithValue("@pattern", "%" + cleanText + "%");
    ...
}
```
When parameters are used, SQL Server compiles the query plan beforehand. The user's input is treated strictly as data (a string literal), never as executable SQL code.

---

## 4. Calculated Properties in C# (`BusRoute.cs`)

Requirement 2 requires at least one calculated property with only a `get`:

```csharp
public bool IsExpress { get; set; }

// Calculated Property
public string ServiceType => IsExpress ? "Express" : "Regular";
```

### How it works:
`ServiceType` has no backing field and is not stored in the SQL database table. Instead, every time `ServiceType` is evaluated (such as when the `DataGridView` binds to it), it dynamically evaluates the ternary operator `IsExpress ? "Express" : "Regular"`.

We also implemented calculated estimated arrival time handling midnight rollover:
```csharp
public TimeSpan EstimatedArrival
{
    get
    {
        long totalMinutes = (long)DepartureTime.TotalMinutes + DurationMinutes;
        long normalizedMinutes = ((totalMinutes % 1440) + 1440) % 1440;
        return TimeSpan.FromMinutes(normalizedMinutes);
    }
}
```

---

## 5. Grouped SQL Conditions for Simultaneous Search and Filtering

In Requirement 8 & 9, the search text and the category filter must work together. If you write:
```sql
-- WRONG: Without parentheses, OR breaks the filter logic:
WHERE Origin LIKE @pattern OR Destination LIKE @pattern AND IsExpress = @isExpress
```
Because `AND` has higher precedence than `OR`, SQL evaluates `Destination LIKE @pattern AND IsExpress = @isExpress` first, allowing any matching `Origin` through regardless of the `IsExpress` filter!

### The Correct Way (Our Implementation):
```sql
WHERE (@text = N'' OR Origin LIKE @pattern OR Destination LIKE @pattern)
  AND (@isExpress IS NULL OR IsExpress = @isExpress)
```
Parentheses group the search terms together so that the record must match the search criteria **AND** match the express filter simultaneously.

---

## 6. Real-Time Vehicle Simulation Logic

In `VehicleSimulationService.cs`, vehicles are not hardcoded or fictitious GPS dots. They are computed dynamically based on published timetable schedules:
1. Current simulated time is tracked in `Asia/Tbilisi` (UTC+4).
2. For each scheduled service with departure $T_{\text{dep}}$ and duration $D$:
   $$\text{Progress} = \frac{T_{\text{current}} - T_{\text{dep}}}{D}$$
3. If $0 \le \text{Progress} \le 1$, the service is currently on the road.
4. Using `GeometryService.InterpolateAlongPath`, the vehicle's geographic coordinate $(\text{Lat}, \text{Lon})$ is interpolated along the route polyline at fraction $t = \text{Progress}$.
5. Heading angle $\theta$ is calculated via spherical trigonometry (bearing between adjacent segments) so the vehicle icon rotates towards its destination.

---

## 7. Multimodal Transit Routing Algorithm

In `JourneyPlanner.cs`:
1. **Direct Stage:** Checks if any intercity coach or regional marshrutka directly connects the origin and destination cities after the user's earliest departure time.
2. **Municipal Feeder Stage:** If starting from a specific city location (e.g. Red Bridge in Kutaisi), determines the closest municipal bus pattern that heads to the central coach terminal (Chavchavadze).
3. **Transfer Buffer Validation:** Calculates walking time to the bus stop, bus transit time, and adds a mandatory 15-minute transfer buffer at the coach terminal before boarding the intercity service. If a connection departs earlier than the arrival + transfer buffer, it is rejected as an infeasible transfer.
4. **Optimization:** Sorts generated itineraries according to user preference (Fastest Arrival, Cheapest Known Fare, Fewest Transfers).
