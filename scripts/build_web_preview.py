import csv
import json
import re
from pathlib import Path

with open('web-preview/kutaisi_all_routes.json', encoding='utf-8') as f:
    all_k_routes = json.load(f)

print(f'Loaded {len(all_k_routes)} Kutaisi patterns for build')
all_k_routes_json = json.dumps(all_k_routes, ensure_ascii=False)

# Read the same schematic geometry the WinForms map uses.
map_source = Path('GeorgiaTransitIntelligence/Mapping/GeoMapControl.cs').read_text(encoding='utf-8')
geometry = Path('GeorgiaTransitIntelligence/Services/GeometryService.cs').read_text(encoding='utf-8')
coordinate_pair = r"\((-?\d+\.\d+),\s*(-?\d+\.\d+)\)"

def coordinates(source):
    return [[float(lat), float(lon)] for lat, lon in re.findall(coordinate_pair, source)]

with open('data/kutaisi-municipal/route_stops.csv', encoding='utf-8-sig', newline='') as stop_file:
    stops = {
        row['StopID']: [float(row['Latitude']), float(row['Longitude']), row['StopName']]
        for row in csv.DictReader(stop_file)
    }

desktop_map = {
    'outline': coordinates(map_source.split('var georgiaPolygon', 1)[1].split('};', 1)[0]),
    'highway': coordinates(geometry.split('E60HighwayCorridor =', 1)[1].split('};', 1)[0]),
    'branches': [
        coordinates(map_source.split(f'var {name} =', 1)[1].split('.Select', 1)[0])
        for name in ('zugdidiPath', 'telaviPath', 'southPath')
    ],
    'cities': [
        [float(lat), float(lon), name]
        for name, lat, lon in re.findall(r'\{ "([^"]+)", ' + coordinate_pair, geometry)
    ],
    'stops': list(stops.values()),
}

html_template = r'''<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Georgia Transit Intelligence — National Transit & Marshrutka Portal</title>
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
  <style>
    :root {
      --navy: #14203c;
      --navy-light: #1e2e54;
      --accent-blue: #0f6cbd;
      --accent-purple: #8e24aa;
      --accent-cyan: #0891b2;
      --accent-orange: #ea580c;
      --accent-red: #dc2626;
      --accent-green: #16a34a;
      --bg: #f4f6fa;
      --card-bg: #ffffff;
      --border: #d0d5dd;
      --text-dark: #1b1b1b;
      --text-muted: #667085;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; }
    body { background: var(--bg); color: var(--text-dark); display: flex; flex-direction: column; height: 100vh; overflow: hidden; }

    /* Header */
    header { background: var(--navy); color: white; padding: 10px 24px; display: flex; justify-content: space-between; align-items: center; box-shadow: 0 2px 8px rgba(0,0,0,0.25); z-index: 100; }
    .brand h1 { font-size: 1.25rem; font-weight: 700; letter-spacing: 0.5px; }
    .brand p { font-size: 0.8rem; color: #b4cde6; }
    .header-badges { display: flex; gap: 10px; align-items: center; }
    .student-badge { background: #28395e; color: #f0f4ff; padding: 5px 14px; border-radius: 999px; font-size: 0.82rem; font-weight: 600; border: 1px solid #3d5280; }
    .download-btn { background: #107c10; color: white; text-decoration: none; padding: 6px 14px; border-radius: 6px; font-size: 0.82rem; font-weight: 600; transition: background 0.2s; display: flex; align-items: center; gap: 6px; }
    .download-btn:hover { background: #0b5a0b; }

    /* Navigation Tabs */
    nav.tabs { background: #1b2b48; border-bottom: 1px solid #324462; display: flex; padding: 0 20px; }
    nav.tabs button { background: none; border: none; padding: 12px 18px; font-size: 0.92rem; font-weight: 600; color: #cfdbec; cursor: pointer; border-bottom: 3px solid transparent; transition: all 0.2s; }
    nav.tabs button.active { color: white; background: #263d61; border-bottom-color: #78bbff; }
    nav.tabs button:hover { color: white; background: #263d61; }

    /* Main Container */
    main { flex: 1; position: relative; overflow: hidden; display: flex; flex-direction: column; }
    .tab-content { display: none; width: 100%; height: 100%; overflow: auto; padding: 18px 22px; }
    .tab-content.active { display: flex; flex-direction: column; }

    /* Timetable Manager (CRUD) */
    .crud-toolbar { background: white; border: 1px solid var(--border); border-radius: 8px; padding: 12px 16px; display: flex; justify-content: space-between; align-items: center; margin-bottom: 14px; flex-wrap: wrap; gap: 12px; }
    .filter-group { display: flex; align-items: center; gap: 10px; }
    .filter-group input, .filter-group select { padding: 7px 12px; border: 1px solid var(--border); border-radius: 6px; font-size: 0.9rem; outline: none; }
    .filter-group input:focus, .filter-group select:focus { border-color: var(--accent-blue); }
    .action-group { display: flex; gap: 8px; }
    .btn { padding: 7px 14px; border-radius: 6px; font-size: 0.88rem; font-weight: 600; cursor: pointer; border: 1px solid transparent; transition: all 0.2s; display: inline-flex; align-items: center; gap: 5px; }
    .btn-primary { background: var(--accent-blue); color: white; }
    .btn-primary:hover { background: #0c5696; }
    .btn-success { background: #16a34a; color: white; }
    .btn-success:hover { background: #15803d; }
    .btn-danger { background: #fee2e2; color: #dc2626; border-color: #fca5a5; }
    .btn-danger:hover { background: #fecaca; }
    .btn-secondary { background: #f3f4f6; color: #374151; border-color: #d1d5db; }
    .btn-secondary:hover { background: #e5e7eb; }

    /* Grid Table */
    .table-container { background: white; border: 1px solid var(--border); border-radius: 8px; overflow: auto; flex: 1; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
    table { width: 100%; border-collapse: collapse; font-size: 0.88rem; }
    th { background: #f8fafc; color: var(--navy); font-weight: 600; text-align: left; padding: 10px 14px; border-bottom: 2px solid var(--border); position: sticky; top: 0; }
    td { padding: 9px 14px; border-bottom: 1px solid #edf2f7; }
    tr:hover { background: #f8fafc; cursor: pointer; }
    tr.selected { background: #e0f2fe !important; }
    tr.express-row { background: #f0f7ff; }
    .badge { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: 700; }
    .badge-express { background: #e0e7ff; color: #3730a3; }
    .badge-regular { background: #f3f4f6; color: #4b5563; }
    .status-bar { padding: 8px 12px; font-size: 0.82rem; color: var(--text-muted); background: white; border: 1px solid var(--border); border-radius: 6px; margin-top: 10px; display: flex; justify-content: space-between; }

    /* Map Tab */
    #tab-simulation { padding: 16px 24px 24px; gap: 12px; min-height: 0; background: #f3f6fa; }
    .simulation-heading { display: flex; align-items: center; justify-content: space-between; gap: 12px; color: #183036; }
    .simulation-heading h2 { font-size: clamp(1.28rem, 2.3vw, 1.8rem); font-weight: 650; letter-spacing: -.035em; }
    .clock-display { color: #144d97; font-size: .86rem; font-weight: 650; font-variant-numeric: tabular-nums; white-space: nowrap; }
    .sim-bar { display: flex; flex-wrap: wrap; gap: 8px 16px; }
    .sim-controls { display: flex; align-items: center; flex-wrap: wrap; gap: 8px; }
    .sim-controls button, .sim-controls select { min-height: 34px; padding: 5px 10px; border-radius: 6px; font-size: .82rem; font-weight: 600; cursor: pointer; }
    .sim-controls select { border: 1px solid #d7e1ed; background: white; color: #183036; max-width: 100%; }
    .sim-controls label { font-size: .8rem; color: #435b60; }
    #btnYandex[aria-pressed="true"], #btnBasemap[aria-pressed="true"] { background: #144d97; color: #fff; }
    #tab-simulation button:active { translate: 0 1px; }
    #tab-simulation :focus-visible { outline: 2px solid #144d97; outline-offset: 2px; }

    /* Operator Filter Strip */
    .operator-filter-strip { background: #f3f6fa; color: #183036; padding: 8px 16px; display: flex; gap: 8px; align-items: center; z-index: 999; border-top: 1px solid #d7e1ed; overflow-x: auto; flex-wrap: wrap; }
    .operator-filter-strip span { font-size: 0.8rem; color: #94a3b8; font-weight: 600; margin-right: 4px; }
    .op-pill { border: none; padding: 4px 10px; border-radius: 999px; font-size: 0.78rem; font-weight: 700; cursor: pointer; transition: all 0.2s; opacity: 0.75; display: inline-flex; align-items: center; gap: 6px; }
    .op-pill:hover, .op-pill.active { opacity: 1; transform: translateY(-1px); box-shadow: 0 2px 6px rgba(0,0,0,0.3); }
    .op-pill-all { background: #475569; color: white; }
    .op-pill-marsh { background: #ea580c; color: white; border: 1px solid #f97316; }
    .op-pill-muni { background: #0284c7; color: white; }
    .op-pill-citybus { background: #8e24aa; color: white; }
    .op-pill-omnibus { background: #1e3a8a; color: #facc15; border: 1px solid #3b82f6; }
    .op-pill-geobus { background: #0891b2; color: white; }
    .op-pill-metro { background: #dc2626; color: white; }

    .map-wrapper { flex: 1; min-height: 260px; position: relative; display: flex; overflow: hidden; border: 1px solid #d7e1ed; border-radius: 14px; }
    .map-canvas { position: relative; flex: 1; min-height: 0; overflow: hidden; }
    #map { position: absolute; inset: 0; background: #deedef; }
    #yandex-map { position: absolute; inset: 0; }
    .map-canvas.yandex-active #map { background: transparent; clip-path: inset(0 0 26px 0); }
    #yandex-config { margin: auto; max-width: min(440px, 94vw); padding: 24px; border: 1px solid #d7e1ed; border-radius: 12px; color: #183036; }
    #yandex-config::backdrop { background: #14203c80; }
    #yandex-config form { display: grid; gap: 12px; }
    #yandex-config input { width: 100%; padding: 8px; border: 1px solid #d7e1ed; border-radius: 6px; }
    #yandex-config .action-group { justify-content: flex-end; }
    #yandex-status:not(:empty) { padding: 6px 10px; color: #8b310c; font-size: .85rem; }
    .map-estimate, .map-status { position: absolute; z-index: 500; pointer-events: none; padding: 4px 8px; font-size: .82rem; font-weight: 700; }
    .map-estimate { top: 16px; left: 16px; background: white; color: #244944; border-radius: 5px; }
    .map-legend { position: absolute; z-index: 500; top: 44px; left: 16px; display: flex; flex-wrap: wrap; gap: 5px 12px; max-width: 55%; background: white; border-radius: 5px; padding: 5px 8px; color: #253955; font-size: .72rem; pointer-events: none; }
    .map-legend span { display: inline-flex; gap: 4px; align-items: center; }
    .map-legend span::before { content: ''; width: 8px; height: 8px; border-radius: 50%; background: var(--marker); }
    .map-status { bottom: 22px; left: 12px; background: white; border: 1px solid #c8c8c8; }
    .map-city { display: flex; align-items: center; gap: 5px; color: #171f32; white-space: nowrap; font-weight: 700; }
    .map-city::before { content: ''; flex: 0 0 8px; height: 8px; border-radius: 50%; background: #172440; }
    .inspector-card { position: absolute; top: 16px; right: 16px; z-index: 550; width: min(300px, 42%); max-height: calc(100% - 58px); border: 1px solid #d9e2dd; border-radius: 12px; background: #fff; padding: 16px; overflow: auto; overflow-wrap: anywhere; font-size: .88rem; box-shadow: 0 6px 18px #18303620; }
    .inspector-title { color: #111827; font-size: 1.08rem; margin-bottom: 14px; }

    .sim-bar .btn-primary { background: #144d97; }
    .vehicle-cluster { display: grid; place-items: center; background: #144d97; color: white; border: 2px solid white; border-radius: 50%; font-weight: 700; }
    #map.grouped .municipal-vehicle { visibility: hidden; }
    .inspector-card.is-empty .vehicle-details { display: none; }
    .inspector-card:not(.is-empty) .inspector-empty { display: none; }
    .sim-controls .btn-secondary { background: #e6efec; color: #183036; }
    .sim-controls .btn-secondary:hover { background: #d5e5df; }
    .sim-filters { background: #fff; color: #183036; border: 1px solid #d7e1ed; border-radius: 8px; }
    .sim-filters summary { cursor: pointer; padding: 7px 12px; font-size: .8rem; font-weight: 600; }
    .sim-filter-controls { padding: 6px 18px; }
    @media (max-width: 720px) {
      #tab-simulation { padding: 12px; gap: 8px; }
      .simulation-heading { flex-wrap: wrap; }
      .map-wrapper { display: grid; grid-template-rows: minmax(175px, 1fr) auto; }
      .inspector-card { position: static; width: 100%; max-height: 180px; border-radius: 0; padding: 10px 14px; }
      header { flex-wrap: wrap; gap: 8px; padding: 10px 14px; }
      .header-badges { flex-wrap: wrap; }
      .student-badge { display: none; }
      nav.tabs { overflow-x: auto; padding: 0; }
      nav.tabs button { flex: 0 0 auto; }
      .map-estimate { font-size: .64rem; }
    }
    .inspector-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; padding-bottom: 6px; border-bottom: 1px solid #edf2f7; }
    .inspector-header h4 { font-size: 1.02rem; color: var(--navy); display: flex; align-items: center; gap: 6px; }
    .op-badge { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 0.72rem; font-weight: 800; text-transform: uppercase; }
    .inspector-meta { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin: 8px 0; background: #f8fafc; padding: 10px; border-radius: 6px; border: 1px solid #e2e8f0; }
    .meta-item { display: flex; flex-direction: column; }
    .meta-label { font-size: 0.7rem; color: var(--text-muted); text-transform: uppercase; font-weight: 600; }
    .meta-val { font-size: 0.85rem; font-weight: 700; color: var(--navy); }
    .progress-bar-wrap { background: #e2e8f0; height: 8px; border-radius: 4px; overflow: hidden; margin: 8px 0; }
    .progress-bar-fill { height: 100%; background: var(--accent-blue); width: 0%; transition: width 0.3s; }
    .features-tag-list { display: flex; flex-wrap: wrap; gap: 4px; margin-top: 8px; }
    .feat-tag { background: #e0f2fe; color: #0369a1; font-size: 0.72rem; padding: 2px 6px; border-radius: 4px; font-weight: 600; }

    /* Custom Leaflet Vehicle Markers - Hardware Accelerated & Zero Lag */
    .leaflet-marker-icon.vehicle-marker-icon {
      transition: transform 0.05s linear !important;
      will-change: transform;
      pointer-events: auto;
    }
    .leaflet-marker-icon.traveler-icon {
      transition: transform 0.06s linear !important;
      will-change: transform;
      pointer-events: auto;
    }
    #map .bus-marker-badge { width: 24px; height: 24px; min-width: 0; padding: 0; justify-content: center; border-radius: 50%; font-size: 9px; border: 1.5px solid white; }
    #map .bus-marker-badge .heading-arrow { display: none; }
    .vehicle-marker-icon { display: flex; align-items: center; justify-content: center; }
    .bus-marker-badge {
      border-radius: 14px;
      padding: 2px 7px;
      color: white;
      font-weight: 800;
      font-size: 10px;
      box-shadow: 0 2px 6px rgba(0,0,0,0.35);
      border: 1.5px solid white;
      display: flex;
      align-items: center;
      gap: 4px;
      white-space: nowrap;
      cursor: pointer;
      user-select: none;
      transition: filter 0.15s, box-shadow 0.15s;
    }
    .bus-marker-badge:hover {
      filter: brightness(1.2);
      box-shadow: 0 4px 12px rgba(0,0,0,0.55);
      z-index: 9999 !important;
    }
    .heading-arrow {
      width: 0;
      height: 0;
      border-left: 4px solid transparent;
      border-right: 4px solid transparent;
      border-bottom: 7px solid currentColor;
      display: inline-block;
      transition: transform 0.12s ease-out;
      transform-origin: 50% 60%;
    }

    /* High-Definition Transit Line & Dashed Intersection Styling */
    @keyframes dash-flow {
      to { stroke-dashoffset: -32; }
    }
    .dashed-intersection-line {
      stroke-dasharray: 8, 8 !important;
      animation: dash-flow 2s linear infinite;
    }
    .dashed-transfer-link {
      stroke-dasharray: 5, 5 !important;
      animation: dash-flow 1.2s linear infinite;
    }
    .intersection-halo-ring {
      stroke-dasharray: 4, 4 !important;
      animation: dash-flow 3s linear infinite;
    }

    /* Journey Planner */
    .planner-layout { display: grid; grid-template-columns: 340px 1fr; gap: 20px; height: 100%; }
    .planner-sidebar { background: white; border: 1px solid var(--border); border-radius: 8px; padding: 18px; display: flex; flex-direction: column; gap: 14px; }
    .form-group label { display: block; font-size: 0.82rem; font-weight: 600; color: var(--navy); margin-bottom: 4px; }
    .form-group select, .form-group input { width: 100%; padding: 8px 10px; border: 1px solid var(--border); border-radius: 6px; font-size: 0.88rem; }
    .planner-results { background: white; border: 1px solid var(--border); border-radius: 8px; padding: 18px; display: flex; flex-direction: column; gap: 14px; overflow: auto; }
    .itinerary-card { border: 1px solid var(--border); border-radius: 8px; padding: 14px; margin-bottom: 12px; cursor: pointer; transition: all 0.2s; }
    .itinerary-card:hover { border-color: var(--accent-blue); background: #f8fafc; }
    .itinerary-card.active { border-color: var(--accent-blue); box-shadow: 0 0 0 1px var(--accent-blue); background: #f0f7ff; }
    .itinerary-header { display: flex; justify-content: space-between; align-items: baseline; font-weight: 700; margin-bottom: 8px; }
    .leg-step { display: flex; gap: 10px; align-items: center; margin: 6px 0; font-size: 0.85rem; color: #475467; }
    .leg-icon { width: 22px; height: 22px; border-radius: 50%; display: grid; place-items: center; font-size: 0.7rem; font-weight: 700; color: white; flex: none; }
    .icon-walk { background: #16a34a; }
    .icon-muni { background: #0284c7; }
    .icon-inter { background: #8e24aa; }
    .icon-marsh { background: #ea580c; }
    .quick-chip { background: #f1f5f9; border: 1px solid #cbd5e1; border-radius: 999px; padding: 3px 9px; font-size: 0.74rem; color: #334155; cursor: pointer; transition: all 0.15s; user-select: none; }
    .quick-chip:hover { background: #e0f2fe; border-color: #38bdf8; color: #0369a1; }
    .journey-notice { background: #fef3c7; border: 1px solid #fde047; color: #92400e; padding: 10px 14px; border-radius: 6px; font-size: 0.83rem; margin-bottom: 12px; display: flex; align-items: flex-start; gap: 8px; line-height: 1.4; }
    .journey-hud { position: absolute; bottom: 20px; left: 50%; transform: translateX(-50%); background: rgba(15, 23, 42, 0.94); backdrop-filter: blur(8px); color: white; padding: 10px 20px; border-radius: 10px; box-shadow: 0 6px 24px rgba(0,0,0,0.4); z-index: 1000; font-size: 0.85rem; border: 1px solid #334155; display: flex; align-items: center; gap: 14px; max-width: 90%; }

    /* Modal */
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: none; place-items: center; z-index: 2000; }
    .modal-overlay.open { display: grid; }
    .modal { background: white; border-radius: 8px; width: 440px; max-width: 90%; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.25); }
    .modal-header { background: var(--navy); color: white; padding: 14px 18px; }
    .modal-body { padding: 18px; display: flex; flex-direction: column; gap: 12px; }
    .modal-footer { padding: 12px 18px; background: #f8fafc; border-top: 1px solid #edf2f7; display: flex; justify-content: flex-end; gap: 8px; }
  </style>
</head>
<body>

  <!-- Top Banner -->
  <header>
    <div class="brand">
      <h1>GEORGIA TRANSIT INTELLIGENCE</h1>
      <p>National Bus Network &amp; Timetable Control Center</p>
    </div>
    <div class="header-badges">
      <span class="student-badge">Student: Rezo Darsavelidze</span>
      <span class="student-badge">Topic 13: BusRoutes</span>
      <a href="./Midterm_Darsavelidze_Rezo.zip" class="download-btn" download>📥 Download Midterm ZIP</a>
    </div>
  </header>

  <!-- Navigation Tabs -->
  <nav class="tabs">
    <button class="active" onclick="switchTab('tab-crud')">Timetable Manager</button>
    <button onclick="switchTab('tab-planner')">Georgia Journey Planner</button>
    <button onclick="switchTab('tab-simulation')">National Live Simulation</button>
    <button onclick="switchTab('tab-explorer')">Network Explorer</button>
    <button onclick="switchTab('tab-marshrutka')">Marshrutka (Route Taxi) Directory</button>
  </nav>

  <!-- Main Workspaces -->
  <main>
    <!-- TAB 1: Timetable Manager (University Graded CRUD) -->
    <section id="tab-crud" class="tab-content active">
      <div class="crud-toolbar">
        <div class="filter-group">
          <label><b>Search:</b></label>
          <input type="text" id="searchInput" placeholder="Search origin, destination, route..." oninput="handleSearchFilter()">
          <label><b>Filter:</b></label>
          <select id="filterSelect" onchange="handleSearchFilter()">
            <option value="All">All Services</option>
            <option value="Express">Express Only</option>
            <option value="Regular">Regular Only</option>
          </select>
        </div>
        <div class="action-group">
          <button class="btn btn-primary" onclick="openAddModal()">+ Add Route</button>
          <button class="btn btn-secondary" onclick="openEditModal()">Edit Selected</button>
          <button class="btn btn-danger" onclick="deleteSelected()">Delete Selected</button>
          <button class="btn btn-success" onclick="exportCsv()">Export CSV (+3 Bonus)</button>
        </div>
      </div>

      <div class="table-container">
        <table id="routesTable">
          <thead>
            <tr>
              <th>Route Number</th>
              <th>Origin Station</th>
              <th>Destination Station</th>
              <th>Departure Time</th>
              <th>Duration</th>
              <th>Price (GEL)</th>
              <th>Service Type</th>
              <th>Est. Arrival</th>
            </tr>
          </thead>
          <tbody id="routesTableBody">
            <!-- Populated via JavaScript -->
          </tbody>
        </table>
      </div>

      <div class="status-bar">
        <span id="crudStatusText">Ready. 18 bus routes loaded.</span>
        <span style="color:#107c10; font-weight:600;">Database: BusTimetableDb (dbo.BusRoutes)</span>
      </div>
    </section>

    <!-- TAB 2: Journey Planner -->
    <section id="tab-planner" class="tab-content">
      <div class="planner-layout">
        <div class="planner-sidebar">
          <h3>Plan Journey</h3>
          <div class="form-group">
            <label>Origin Location / Address / Stop:</label>
            <div style="position:relative;">
              <input type="text" id="plannerOrigin" list="originDatalist" value="Tabukashvili Street 217, Kutaisi" placeholder="Enter street, landmark, or stop..." oninput="calculateJourney()" style="width:100%; padding:8px 32px 8px 10px; border:1px solid var(--border); border-radius:6px; font-size:0.88rem;">
              <button type="button" onclick="setOriginQuick('Tabukashvili Street 217, Kutaisi')" title="Reset to Tabukashvili 217" style="position:absolute; right:6px; top:6px; border:none; background:none; cursor:pointer; font-size:1.05rem;">📍</button>
            </div>
            <datalist id="originDatalist">
              <option value="Tabukashvili Street 217, Kutaisi">
              <option value="Kutaisi International University (KIU)">
              <option value="Kutaisi (Red Bridge - წითელი ხიდი)">
              <option value="Kutaisi (Chavchavadze Omnibus Terminal)">
              <option value="Bagrati Cathedral, Kutaisi">
              <option value="Nikea Street 10, Kutaisi">
              <option value="Tsereteli State University, Kutaisi">
              <option value="Kutaisi International Airport (KUT)">
              <option value="Tbilisi (Didube Terminal)">
              <option value="Batumi (Central / New Station)">
            </datalist>
            <!-- Quick Suggestion Chips -->
            <div style="display:flex; flex-wrap:wrap; gap:5px; margin-top:8px;">
              <span class="quick-chip" onclick="setOriginQuick('Tabukashvili Street 217, Kutaisi')">📍 Tabukashvili St 217</span>
              <span class="quick-chip" onclick="setOriginQuick('Kutaisi International University (KIU)')">📍 KIU Campus</span>
              <span class="quick-chip" onclick="setOriginQuick('Bagrati Cathedral, Kutaisi')">📍 Bagrati</span>
              <span class="quick-chip" onclick="setOriginQuick('Kutaisi (Red Bridge)')">📍 Red Bridge</span>
              <span class="quick-chip" onclick="setOriginQuick('Kutaisi (Chavchavadze Omnibus Terminal)')">📍 Chavchavadze / Omnibus</span>
            </div>
          </div>
          <div class="form-group">
            <label>Destination City / Station:</label>
            <select id="plannerDest" onchange="calculateJourney()">
              <option value="Tbilisi" selected>Tbilisi (Didube / Ortachala / Station Sq)</option>
              <option value="Batumi">Batumi (Central / New Station)</option>
              <option value="Zugdidi">Zugdidi (Bus Station)</option>
              <option value="Poti">Poti (Central / Sea Port)</option>
              <option value="Kazbegi">Stepantsminda (Kazbegi)</option>
              <option value="Mestia">Mestia (Svaneti)</option>
              <option value="Tskaltubo">Tskaltubo (Spa Resort)</option>
              <option value="Borjomi">Borjomi (Park Station)</option>
              <option value="Gori">Gori (Fortress / Center)</option>
              <option value="Telavi">Telavi (Kakheti)</option>
            </select>
          </div>
          <div class="form-group">
            <label>Earliest Departure:</label>
            <input type="time" id="plannerTime" value="08:00" onchange="calculateJourney()">
          </div>
          <div class="form-group">
            <label>Optimization:</label>
            <select id="plannerOpt" onchange="calculateJourney()">
              <option value="fastest">Fastest Door-to-Door Arrival</option>
              <option value="cheapest">Cheapest Known Total Fare</option>
              <option value="fewest">Fewest Station Transfers</option>
            </select>
          </div>
          <div class="form-group" style="background:#f8fafc; padding:8px 10px; border-radius:6px; border:1px solid #e2e8f0; font-size:0.83rem;">
            <div style="font-weight:700; margin-bottom:5px; color:#334155;">Allowed Modes:</div>
            <label style="display:flex; align-items:center; gap:6px; margin-bottom:3px; cursor:pointer;">
              <input type="checkbox" id="chkMuniBus" checked onchange="calculateJourney()"> Kutaisi Municipal Bus (e.g. Bus #20)
            </label>
            <label style="display:flex; align-items:center; gap:6px; margin-bottom:3px; cursor:pointer;">
              <input type="checkbox" id="chkIntercityCoach" checked onchange="calculateJourney()"> Intercity Coach (Omnibus, Citybus)
            </label>
            <label style="display:flex; align-items:center; gap:6px; margin-bottom:3px; cursor:pointer;">
              <input type="checkbox" id="chkMarshrutka" checked onchange="calculateJourney()"> Regional Route Taxi (სამარშრუტო ტაქსი)
            </label>
          </div>
          <button class="btn btn-primary" style="margin-top:4px; width:100%; padding:10px;" onclick="calculateJourney()">Find Multimodal Journeys</button>
          <button class="btn btn-secondary" style="width:100%;" onclick="simulateJourneyOnMap(0)">▶ Simulate On Live Map</button>
        </div>

        <div class="planner-results" id="plannerResultsArea">
          <h3>Viable Itineraries</h3>
          <div id="itinerariesList">
            <!-- Populated via JS -->
          </div>
        </div>
      </div>
    </section>

    <!-- TAB 3: Map Simulation -->
    <section id="tab-simulation" class="tab-content">
      <div class="simulation-heading">
        <h2>Vehicle simulation</h2>
        <div class="clock-display" id="simClockText">Georgia time</div>
      </div>
      <div class="sim-bar">
        <div class="sim-controls" aria-label="Playback">
          <button id="btnPlayPause" class="btn btn-primary" onclick="toggleSim()">⏸ Pause</button>
          <button class="btn btn-secondary" onclick="resetSim()">Reset Time</button>
          <label for="simSpeedSelect">Speed</label>
          <select id="simSpeedSelect" onchange="changeSpeed()">
            <option value="1" selected>1x Real-time</option>
            <option value="5">5x Fast</option>
            <option value="15">15x Rapid</option>
            <option value="60">60x Warp</option>
          </select>
        </div>
        <div class="sim-controls" aria-label="Map view">
          <button class="btn btn-secondary" onclick="map.zoomIn()" aria-label="Zoom in">+</button>
          <button class="btn btn-secondary" onclick="map.zoomOut()" aria-label="Zoom out">−</button>
          <button class="btn btn-secondary" onclick="zoomGeorgia()">Georgia View</button>
          <button class="btn btn-primary" onclick="zoomKutaisi()">Kutaisi City</button>
          <button id="btnYandex" class="btn btn-secondary" aria-pressed="false" onclick="selectYandex()">Yandex Maps</button>
          <button id="btnBasemap" class="btn btn-secondary" aria-pressed="false" onclick="toggleStreetMap()">Street map</button>

        </div>
      </div>
      <details class="sim-filters">
        <summary>Map filters</summary>
        <div class="sim-controls sim-filter-controls">
          <button class="btn btn-secondary" onclick="zoomRikoti()">Rikoti Pass View</button>
          <!-- Marshrutka / Route Taxi Quick Filter -->
          <span style="font-size:0.85rem; color:#f97316; margin-left:6px;"><b>Route Taxi:</b></span>
          <select id="marshrutkaFilterSelect" style="max-width:210px; padding:5px 8px; font-weight:600; background:#fff7ed; border:1px solid #fdba74;" onchange="handleMarshrutkaFilter()">
            <option value="ALL">All 25 Marshrutka Routes</option>
            <!-- Generated dynamically via JS -->
          </select>

          <!-- Kutaisi Line Quick Filter -->
          <span style="font-size:0.85rem; color:#b4cde6; margin-left:6px;"><b>Kutaisi Line:</b></span>
          <select id="kutaisiLineFilterSelect" style="max-width:190px; padding:5px 8px; font-weight:600;" onchange="handleKutaisiLineFilter()">
            <option value="ALL">All 22 Kutaisi Municipal Lines</option>
            <!-- Generated dynamically via JS -->
          </select>

          <!-- Intersections Dashed Toggle Button -->
          <button id="btnDashedIntersections" class="btn btn-secondary" onclick="toggleDashedIntersections()" style="background:#0f172a; border:1px solid #facc15; color:#facc15; font-weight:700; padding:5px 10px; margin-left:4px;">
            Show Intersections
          </button>
        </div>
      <!-- Operator Filter Strip -->
      <div class="operator-filter-strip">
        <button class="op-pill op-pill-all active" onclick="filterOperator('ALL')">All Transit</button>
        <button class="op-pill op-pill-marsh" onclick="filterOperator('Route Taxi')">Marshrutka / Route Taxi</button>
        <button class="op-pill op-pill-muni" onclick="filterOperator('Kutaisi Municipal')">Kutaisi City Bus</button>
        <button class="op-pill op-pill-citybus" onclick="filterOperator('Citybus Georgia')">Citybus</button>
        <button class="op-pill op-pill-omnibus" onclick="filterOperator('Omnibus Express')">Omnibus</button>
        <button class="op-pill op-pill-geobus" onclick="filterOperator('Georgian Bus')">Georgian Bus</button>
        <button class="op-pill op-pill-metro" onclick="filterOperator('Metro Georgia')">Metro</button>
      </div>

      </details>
      <p id="yandex-status" role="status"></p>
      <dialog id="yandex-config" aria-labelledby="yandex-title">
        <form id="yandex-form">
          <h3 id="yandex-title">Connect Yandex Maps</h3>
          <p>Use a <a href="https://developer.tech.yandex.com/" target="_blank" rel="noopener">Yandex Maps JavaScript API key</a> that allows this localhost hostname.</p>
          <label for="yandex-key">API key</label>
          <input id="yandex-key" type="password" required autocomplete="off">
          <div class="action-group">
            <button type="button" class="btn btn-secondary" onclick="document.getElementById('yandex-config').close()">Cancel</button>
            <button class="btn btn-primary">Connect</button>
          </div>
        </form>
      </dialog>
      <div class="map-wrapper">
        <div class="map-canvas">
          <div id="yandex-map" hidden></div>
          <div id="map" aria-label="Georgia transit simulation map"></div>
          <div class="map-estimate">Estimated vehicle positions</div>
          <div class="map-legend" aria-label="Vehicle types">
            <span style="--marker:#0369a1">Municipal bus</span>
            <span style="--marker:#6d28a7">Intercity coach</span>
            <span style="--marker:#c2410c">Marshrutka</span>
          </div>
          <div class="map-status" id="activeFleetPill"></div>
        </div>

        <!-- Vehicle Inspector Card -->
        <aside class="inspector-card is-empty" id="vehicleInspector">
          <h3 class="inspector-title">Vehicle Inspector</h3>
          <p class="inspector-empty">Select a vehicle to see its route and estimated schedule.</p>
          <div class="vehicle-details">
          <div class="inspector-header">
            <h4><span id="inspBadge" class="op-badge" style="background:#ea580c; color:white;">ROUTE TAXI</span> <span id="inspId">MR-OK-01</span></h4>
          </div>
          <p id="inspOperator" style="font-weight:700; color:var(--navy); margin-bottom:2px;">Marshrutka (Route Taxi / სამარშრუტო ტაქსი)</p>
          <p id="inspModel" style="color:var(--text-muted); font-size:0.8rem; margin-bottom:6px;">Mercedes-Benz Sprinter 519 CDI Minibus (18-seater)</p>

          <p id="inspRoute" style="font-size:0.88rem; font-weight:600; color:#1e2e54;">Kutaisi (Chavchavadze) → Tbilisi (Didube / Okriba)</p>
          <p id="inspCorridor" style="font-size:0.8rem; color:#475569; margin:4px 0;">Corridor: <b>Highway E60 (Rikoti Tunnel Pass)</b></p>

          <div class="progress-bar-wrap">
            <div class="progress-bar-fill" id="inspProgressBar" style="width: 45%;"></div>
          </div>

          <div class="inspector-meta">
            <div class="meta-item">
              <span class="meta-label">Progress</span>
              <span class="meta-val" id="inspProgressText">45% along route</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">Speed & Heading</span>
              <span class="meta-val" id="inspSpeed">78 km/h • 255° WSW</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">Scheduled Dep</span>
              <span class="meta-val" id="inspDepTime">08:00</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">Est. Arrival</span>
              <span class="meta-val" id="inspArrTime">11:40</span>
            </div>
          </div>

          <p id="inspNextStop" style="font-size:0.82rem; color:#0f6cbd; font-weight:600; margin-top:4px;">Next Landmark: Surami Pass Entrance (~12 min)</p>

          </div>
        </aside>
      </div>
    </section>

    <!-- TAB 4: Network Explorer -->
    <section id="tab-explorer" class="tab-content">
      <!-- FULL 22 KUTAISI MUNICIPAL ROUTES DIRECTORY -->
      <div style="background:white; border:1px solid var(--border); border-radius:8px; padding:18px; margin-bottom:16px;">
        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:12px;">
          <div>
            <h3>All 22 Kutaisi Municipal Bus Routes & Timetables</h3>
            <p style="color:var(--text-muted); font-size:0.85rem;">Official Kutaisi City Hall public transport network (44 directions, 450 verified stops, 182,291 timetable entries):</p>
          </div>
          <input type="text" id="kutaisiDirSearch" placeholder="Search line number or street..." style="padding:7px 12px; border:1px solid var(--border); border-radius:6px; font-size:0.88rem; width:260px;" oninput="renderKutaisiDirectoryTable()">
        </div>

        <div style="max-height: 380px; overflow-y: auto; border: 1px solid var(--border); border-radius: 6px;">
          <table>
            <thead>
              <tr>
                <th>Line</th>
                <th>Route Name (ქართულად)</th>
                <th>Direction 0 (Forward Terminals)</th>
                <th>Direction 1 (Return Terminals)</th>
                <th>Total Stops</th>
                <th>Headway / Hours</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody id="kutaisiDirectoryBody">
              <!-- Populated dynamically via JS with all 22 routes -->
            </tbody>
          </table>
        </div>
      </div>

      <div style="background:white; border:1px solid var(--border); border-radius:8px; padding:18px; margin-bottom:16px;">
        <h3>Smart Departure Countdown Board</h3>
        <p style="color:var(--text-muted); font-size:0.85rem; margin-bottom:10px;">Select a major Georgian transport hub to see live departure countdowns:</p>
        <div style="display:flex; gap:12px; margin-bottom:14px;">
          <select id="stationSelect" style="padding:6px 12px; border-radius:6px; border:1px solid var(--border); font-size:0.9rem;" onchange="updateDepartureBoard()">
            <option value="kutaisi-chavchavadze">Kutaisi - Chavchavadze Bus Terminal</option>
            <option value="kutaisi-redbridge">Kutaisi - Red Bridge (წითელი ხიდი)</option>
            <option value="tbilisi-didube">Tbilisi - Didube Terminal</option>
            <option value="batumi-central">Batumi - Central Bus Station</option>
          </select>
        </div>
        <table id="departureBoardTable">
          <thead>
            <tr>
              <th>Service / Route</th>
              <th>Destination</th>
              <th>Operator</th>
              <th>Scheduled Time</th>
              <th>Countdown</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody id="departureBoardBody">
            <!-- Populated via JS -->
          </tbody>
        </table>
      </div>
    </section>

    <!-- TAB 5: Marshrutka (Route Taxi) Directory -->
    <section id="tab-marshrutka" class="tab-content">
      <div style="background:white; border:1px solid var(--border); border-radius:8px; padding:18px; margin-bottom:16px;">
        <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:12px; flex-wrap:wrap; gap:12px;">
          <div>
            <h3>National Marshrutka (Fixed-Route Taxicab / სამარშრუტო ტაქსი) Directory</h3>
            <p style="color:var(--text-muted); font-size:0.85rem; margin-top:4px;">
              <b>Terminology & Transit Architecture:</b> In English, a <i>Marshrutka</i> is properly classified as a <b>Fixed-Route Taxicab (Route Taxi / Shared Minibus)</b>. Operating along designated national corridors with flexible hail/flag-down boarding.
            </p>
          </div>
          <input type="text" id="marshDirSearch" placeholder="Search route, city, carrier..." style="padding:7px 12px; border:1px solid var(--border); border-radius:6px; font-size:0.88rem; width:260px;" oninput="renderMarshrutkaDirectoryTable()">
        </div>

        <div style="max-height: 480px; overflow-y: auto; border: 1px solid var(--border); border-radius: 6px;">
          <table>
            <thead>
              <tr>
                <th>Service Code</th>
                <th>Route Corridor (Origin → Destination)</th>
                <th>Operator / Carrier</th>
                <th>Fleet Type</th>
                <th>Frequency / Headway</th>
                <th>Fare (GEL)</th>
                <th>Departure Platform</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody id="marshrutkaDirectoryBody">
              <!-- Populated via JS -->
            </tbody>
          </table>
        </div>
      </div>
    </section>
  </main>

  <!-- Add/Edit Modal Dialog -->
  <div class="modal-overlay" id="routeModal">
    <div class="modal">
      <div class="modal-header">
        <h3 id="modalTitle">Add Bus Route</h3>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label>Route Number:</label>
          <input type="text" id="mRouteNumber" placeholder="e.g. GEO-101 or 34">
        </div>
        <div class="form-group">
          <label>Origin Station:</label>
          <input type="text" id="mOrigin" placeholder="e.g. Kutaisi (Chavchavadze)">
        </div>
        <div class="form-group">
          <label>Destination Station:</label>
          <input type="text" id="mDestination" placeholder="e.g. Batumi (Central)">
        </div>
        <div class="form-group">
          <label>Departure Time:</label>
          <input type="time" id="mDepartureTime" value="08:30">
        </div>
        <div class="form-group">
          <label>Duration (Minutes):</label>
          <input type="number" id="mDuration" min="1" max="1440" value="120">
        </div>
        <div class="form-group">
          <label>Price (GEL):</label>
          <input type="number" id="mPrice" step="0.5" min="0" value="15.00">
        </div>
        <div class="form-group">
          <label>
            <input type="checkbox" id="mIsExpress"> Express Service
          </label>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn btn-secondary" onclick="closeModal()">Cancel</button>
        <button class="btn btn-primary" onclick="saveRouteModal()">Save</button>
      </div>
    </div>
  </div>

  <script>
    // Initial 18 realistic seed records matching database.sql
    let routes = [
      { id: 1, routeNumber: 'GEO-101', origin: 'Tbilisi (Ortachala)', destination: 'Batumi (New Station)', depTime: '07:00', duration: 330, price: 40.00, isExpress: true },
      { id: 2, routeNumber: 'GEO-102', origin: 'Tbilisi (Didube)', destination: 'Kutaisi (Chavchavadze)', depTime: '08:00', duration: 220, price: 20.00, isExpress: true },
      { id: 3, routeNumber: 'GEO-103', origin: 'Kutaisi (Chavchavadze)', destination: 'Batumi (Central)', depTime: '08:30', duration: 140, price: 15.00, isExpress: true },
      { id: 4, routeNumber: 'GEO-104', origin: 'Tbilisi (Ortachala)', destination: 'Kutaisi (Chavchavadze)', depTime: '09:00', duration: 240, price: 18.00, isExpress: false },
      { id: 5, routeNumber: 'GEO-105', origin: 'Batumi (Central)', destination: 'Kutaisi (Chavchavadze)', depTime: '09:15', duration: 150, price: 15.00, isExpress: false },
      { id: 6, routeNumber: 'GEO-106', origin: 'Kutaisi (Chavchavadze)', destination: 'Tbilisi (Didube)', depTime: '10:00', duration: 215, price: 20.00, isExpress: true },
      { id: 7, routeNumber: 'GEO-107', origin: 'Tbilisi (Didube)', destination: 'Zugdidi (Bus Station)', depTime: '10:30', duration: 360, price: 30.00, isExpress: false },
      { id: 8, routeNumber: 'GEO-108', origin: 'Kutaisi (Chavchavadze)', destination: 'Zugdidi (Bus Station)', depTime: '11:00', duration: 120, price: 12.00, isExpress: true },
      { id: 9, routeNumber: 'GEO-109', origin: 'Tbilisi (Ortachala)', destination: 'Telavi (Central)', depTime: '11:30', duration: 130, price: 14.00, isExpress: false },
      { id: 10, routeNumber: 'GEO-110', origin: 'Kutaisi (Chavchavadze)', destination: 'Poti (Bus Station)', depTime: '12:00', duration: 95, price: 10.00, isExpress: false },
      { id: 11, routeNumber: 'GEO-111', origin: 'Tbilisi (Didube)', destination: 'Akhaltsikhe (Central)', depTime: '13:00', duration: 190, price: 18.00, isExpress: false },
      { id: 12, routeNumber: 'GEO-112', origin: 'Batumi (Central)', destination: 'Tbilisi (Ortachala)', depTime: '14:00', duration: 340, price: 40.00, isExpress: true },
      { id: 13, routeNumber: 'GEO-113', origin: 'Kutaisi (Chavchavadze)', destination: 'Gori (Comfort Bus)', depTime: '14:30', duration: 150, price: 16.00, isExpress: false },
      { id: 14, routeNumber: 'GEO-114', origin: 'Tbilisi (Didube)', destination: 'Kazbegi (Stepantsminda)', depTime: '15:00', duration: 180, price: 17.00, isExpress: false },
      { id: 15, routeNumber: 'GEO-115', origin: 'Kutaisi Airport (KUT)', destination: 'Tbilisi (Pushkin Sq)', depTime: '16:00', duration: 240, price: 25.00, isExpress: true },
      { id: 16, routeNumber: 'GEO-116', origin: 'Kutaisi Airport (KUT)', destination: 'Batumi (Radisson)', depTime: '16:30', duration: 130, price: 20.00, isExpress: true },
      { id: 17, routeNumber: 'GEO-117', origin: 'Tbilisi (Didube)', destination: 'Borjomi (Park Station)', depTime: '17:15', duration: 150, price: 15.00, isExpress: false },
      { id: 18, routeNumber: 'GEO-118', origin: 'Kutaisi (Chavchavadze)', destination: 'Mestia (Seti Square)', depTime: '07:30', duration: 310, price: 35.00, isExpress: false }
    ];

    let selectedRowId = null;
    let editMode = false;
    let nextId = 19;

    function calcEstArrival(dep, duration) {
      let [h, m] = dep.split(':').map(Number);
      let totalM = (h * 60 + m + duration) % 1440;
      let arrH = Math.floor(totalM / 60);
      let arrM = totalM % 60;
      return `${String(arrH).padStart(2,'0')}:${String(arrM).padStart(2,'0')}`;
    }

    function renderRoutesTable(filtered) {
      const tbody = document.getElementById('routesTableBody');
      tbody.innerHTML = '';
      filtered.forEach(r => {
        const tr = document.createElement('tr');
        if (r.id === selectedRowId) tr.classList.add('selected');
        if (r.isExpress) tr.classList.add('express-row');
        tr.onclick = () => selectRow(r.id);
        tr.ondblclick = () => openEditModal();

        tr.innerHTML = `
          <td><b>${r.routeNumber}</b></td>
          <td>${r.origin}</td>
          <td>${r.destination}</td>
          <td>${r.depTime}</td>
          <td>${Math.floor(r.duration/60)}h ${String(r.duration%60).padStart(2,'0')}m</td>
          <td>${r.price.toFixed(2)} ₾</td>
          <td><span class="badge ${r.isExpress ? 'badge-express' : 'badge-regular'}">${r.isExpress ? 'Express' : 'Regular'}</span></td>
          <td>${calcEstArrival(r.depTime, r.duration)}</td>
        `;
        tbody.appendChild(tr);
      });

      document.getElementById('crudStatusText').innerText = `Showing ${filtered.length} of ${routes.length} bus routes.`;
    }

    function selectRow(id) {
      selectedRowId = id;
      handleSearchFilter();
    }

    function handleSearchFilter() {
      const query = document.getElementById('searchInput').value.trim().toLowerCase();
      const filter = document.getElementById('filterSelect').value;

      let result = routes.filter(r => {
        let matchesSearch = !query || r.origin.toLowerCase().includes(query) || r.destination.toLowerCase().includes(query) || r.routeNumber.toLowerCase().includes(query);
        let matchesFilter = filter === 'All' || (filter === 'Express' && r.isExpress) || (filter === 'Regular' && !r.isExpress);
        return matchesSearch && matchesFilter;
      });
      renderRoutesTable(result);
    }

    function openAddModal() {
      editMode = false;
      document.getElementById('modalTitle').innerText = 'Add New Bus Route';
      document.getElementById('mRouteNumber').value = '';
      document.getElementById('mOrigin').value = '';
      document.getElementById('mDestination').value = '';
      document.getElementById('mDepartureTime').value = '08:00';
      document.getElementById('mDuration').value = '120';
      document.getElementById('mPrice').value = '15.00';
      document.getElementById('mIsExpress').checked = false;
      document.getElementById('routeModal').classList.add('open');
    }

    function openEditModal() {
      if (!selectedRowId) {
        alert('Please select a route from the table first.');
        return;
      }
      const r = routes.find(x => x.id === selectedRowId);
      if (!r) return;
      editMode = true;
      document.getElementById('modalTitle').innerText = `Edit Route ${r.routeNumber}`;
      document.getElementById('mRouteNumber').value = r.routeNumber;
      document.getElementById('mOrigin').value = r.origin;
      document.getElementById('mDestination').value = r.destination;
      document.getElementById('mDepartureTime').value = r.depTime;
      document.getElementById('mDuration').value = r.duration;
      document.getElementById('mPrice').value = r.price;
      document.getElementById('mIsExpress').checked = r.isExpress;
      document.getElementById('routeModal').classList.add('open');
    }

    function closeModal() {
      document.getElementById('routeModal').classList.remove('open');
    }

    function saveRouteModal() {
      const num = document.getElementById('mRouteNumber').value.trim();
      const orig = document.getElementById('mOrigin').value.trim();
      const dest = document.getElementById('mDestination').value.trim();
      const time = document.getElementById('mDepartureTime').value;
      const dur = parseInt(document.getElementById('mDuration').value);
      const pr = parseFloat(document.getElementById('mPrice').value);
      const exp = document.getElementById('mIsExpress').checked;

      if (!num || !orig || !dest) {
        alert('Validation Error: All text fields are required.');
        return;
      }
      if (orig.toLowerCase() === dest.toLowerCase()) {
        alert('Validation Error: Origin and Destination cannot be identical.');
        return;
      }
      if (dur <= 0 || isNaN(dur)) {
        alert('Validation Error: Duration must be at least 1 minute.');
        return;
      }
      if (pr < 0 || isNaN(pr)) {
        alert('Validation Error: Price cannot be negative.');
        return;
      }

      if (editMode) {
        const item = routes.find(x => x.id === selectedRowId);
        if (item) {
          item.routeNumber = num;
          item.origin = orig;
          item.destination = dest;
          item.depTime = time;
          item.duration = dur;
          item.price = pr;
          item.isExpress = exp;
        }
      } else {
        routes.unshift({
          id: nextId++,
          routeNumber: num,
          origin: orig,
          destination: dest,
          depTime: time,
          duration: dur,
          price: pr,
          isExpress: exp
        });
        selectedRowId = routes[0].id;
      }

      closeModal();
      handleSearchFilter();
    }

    function deleteSelected() {
      if (!selectedRowId) {
        alert('Please select a route from the table first.');
        return;
      }
      const r = routes.find(x => x.id === selectedRowId);
      if (!r) return;

      if (confirm(`Are you sure you want to delete route '${r.routeNumber}' from ${r.origin} to ${r.destination}?`)) {
        routes = routes.filter(x => x.id !== selectedRowId);
        selectedRowId = null;
        handleSearchFilter();
      }
    }

    function exportCsv() {
      let csv = '\uFEFFRoute Number,Origin,Destination,Departure Time,Duration,Price (GEL),Service Type,Estimated Arrival\n';
      const query = document.getElementById('searchInput').value.trim().toLowerCase();
      const filter = document.getElementById('filterSelect').value;

      let filtered = routes.filter(r => {
        let matchesSearch = !query || r.origin.toLowerCase().includes(query) || r.destination.toLowerCase().includes(query) || r.routeNumber.toLowerCase().includes(query);
        let matchesFilter = filter === 'All' || (filter === 'Express' && r.isExpress) || (filter === 'Regular' && !r.isExpress);
        return matchesSearch && matchesFilter;
      });

      filtered.forEach(r => {
        let esc = s => `"${String(s).replace(/"/g, '""')}"`;
        csv += `${esc(r.routeNumber)},${esc(r.origin)},${esc(r.destination)},${r.depTime},${r.duration},${r.price.toFixed(2)},${r.isExpress ? 'Express' : 'Regular'},${calcEstArrival(r.depTime, r.duration)}\n`;
      });

      const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = 'BusRoutes_Timetable_Export.csv';
      link.click();
    }

    // Tabs
    function switchTab(tabId) {
      document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
      document.querySelectorAll('nav.tabs button').forEach(el => el.classList.remove('active'));
      document.getElementById(tabId).classList.add('active');
      document.querySelector(`nav.tabs button[onclick="switchTab('${tabId}')"]`)?.classList.add('active');

      if (tabId === 'tab-simulation') {
        setTimeout(() => {
          map.invalidateSize();
          if (!streetMapInitialized && /^https?:$/.test(location.protocol)) {
            streetMapInitialized = true;
            toggleStreetMap();
          }
        }, 200);
      }
      if (tabId === 'tab-explorer') {
        renderKutaisiDirectoryTable();
        updateDepartureBoard();
      }
      if (tabId === 'tab-marshrutka') {
        renderMarshrutkaDirectoryTable();
      }
    }

    // ==========================================
    // LEAFLET MAP & COMPREHENSIVE ROAD GEOMETRIES
    // ==========================================
    let map;
    let simRunning = true;
    let simSpeed = 1;
    let municipalCluster;
    let streetMapInitialized = false;
    let yandexMap;
    let yandexReady;
    let yandexKey = '';
    let yandexActive = false;

    function syncYandexView() {
      if (!yandexActive) return;
      const {lat, lng} = map.getCenter();
      yandexMap.setLocation({center: [lng, lat], zoom: map.getZoom(), duration: 0});
    }

    function leaveYandex() {
      if (!yandexActive) return;
      yandexActive = false;
      yandexMap.destroy();
      yandexMap = undefined;
      document.querySelector('.map-canvas').classList.remove('yandex-active');
      document.getElementById('yandex-map').hidden = true;
      document.getElementById('btnYandex').setAttribute('aria-pressed', 'false');
      schematicBase.addTo(map);
    }

    function loadYandex(key) {
      if (yandexReady && key === yandexKey) return yandexReady;
      const script = document.createElement('script');
      script.src = 'https://api-maps.yandex.ru/v3/?' + new URLSearchParams({apikey: key, lang: 'en_US'});
      const ready = new Promise((resolve, reject) => {
        script.onload = async () => {
          try {
            const api = window.ymaps3;
            await api.ready;


            resolve(api);
          } catch (error) {
            reject(error);
          }
        };
        script.onerror = () => reject(new Error("SDK request failed"));
      });
      yandexKey = key;
      yandexReady = new Promise((resolve, reject) => {
        const timeout = setTimeout(() => reject(new Error("SDK initialization timed out")), 15000);
        ready.then(resolve, reject).finally(() => clearTimeout(timeout));
      }).catch(error => {
        script.remove();
        yandexReady = undefined;
        yandexKey = '';
        throw error;
      });
      script.referrerPolicy = 'strict-origin-when-cross-origin';
      document.head.append(script);
      return yandexReady;
    }

    async function selectYandex(key) {
      const button = document.getElementById('btnYandex');
      const status = document.getElementById('yandex-status');
      if (yandexActive) {
        leaveYandex();
        return;
      }
      if (!/^https?:$/.test(location.protocol)) {
        status.textContent = 'Open the preview through localhost to connect Yandex Maps.';
        return;
      }
      key ||= yandexKey;
      if (!key) {
        document.getElementById('yandex-config').showModal();
        return;
      }
      button.disabled = true;
      status.textContent = 'Connecting to Yandex Maps…';
      const container = document.getElementById('yandex-map');
      try {
        const api = await loadYandex(key);
        if (!document.getElementById('tab-simulation').classList.contains('active')) return;
        const {YMap, YMapDefaultSchemeLayer} = api;
        const {lat, lng} = map.getCenter();
        container.hidden = false;
        yandexMap = new YMap(container, {
          location: {center: [lng, lat], zoom: map.getZoom()},
          behaviors: [], projection: api.projections.sphericalMercator
        });
        yandexMap.addChild(new YMapDefaultSchemeLayer());
        yandexActive = true;
        streetTiles.remove();
        schematicBase.remove();
        document.querySelector('.map-canvas').classList.add('yandex-active');
        document.getElementById('btnBasemap').setAttribute('aria-pressed', 'false');
        button.setAttribute('aria-pressed', 'true');
        status.textContent = '';
      } catch (error) {
        yandexMap?.destroy();
        yandexMap = undefined;
        container.hidden = true;
        yandexReady = undefined;
        yandexKey = '';
        status.textContent = 'Yandex Maps could not load (' + (['SDK request failed', 'SDK initialization timed out'].includes(error?.message) ? error.message : error?.name ?? 'SDK error') + '). Check your key and localhost restriction.';
      } finally {
        button.disabled = false;
        if (status.textContent === 'Connecting to Yandex Maps…') status.textContent = '';
      }
    }

    document.getElementById('yandex-form').addEventListener('submit', event => {
      event.preventDefault();
      const input = document.getElementById('yandex-key');
      const key = input.value.trim();
      if (!key) return;
      input.value = '';
      document.getElementById('yandex-config').close();
      selectYandex(key);
    });

    function toggleStreetMap() {
      leaveYandex();
      document.getElementById('yandex-status').textContent = '';
      const button = document.getElementById('btnBasemap');
      const showStreets = !map.hasLayer(streetTiles);
      if (showStreets) streetTiles.addTo(map);
      else {
        streetTiles.remove();
        schematicBase.addTo(map);
      }
      button.setAttribute('aria-pressed', String(showStreets));
    }
    let simTime = new Date();
    let currentOperatorFilter = 'ALL';
    let currentKutaisiLineFilter = 'ALL';
    let currentMarshrutkaFilter = 'ALL';
    let allVehicleSims = [];
    let kutaisiRoutePolylines = {};
    let highlightedRouteLayer = null;
    let stopMarkersGroup = null;

    // ALL 44 Verified Kutaisi Municipal Route Patterns (22 Routes)
    const KUTAISI_ALL_PATTERNS = ''' + all_k_routes_json + ''';

    // Highway E60 Primary Spine
    const e60FullCorridor = [
      [41.6750, 44.8350, 'Tbilisi Ortachala'],
      [41.7210, 44.7990, 'Tbilisi Central'],
      [41.7508, 44.7788, 'Tbilisi Didube Terminal'],
      [41.8020, 44.7550, 'Zahesi'],
      [41.8410, 44.7150, 'Mtskheta (Jvari)'],
      [41.9180, 44.7170, 'Natakhtari'],
      [41.9450, 44.5700, 'Ksani / Mukhrani'],
      [41.9950, 44.4050, 'Igoeti Interchange'],
      [42.0120, 44.2550, 'Nadarbazevi'],
      [41.9842, 44.1158, 'Gori E60 Bypass'],
      [42.0250, 43.9550, 'Ruisi / Urbnisi'],
      [42.0200, 43.8900, 'Kareli Junction'],
      [42.0350, 43.8200, 'Agara E60'],
      [41.9942, 43.5999, 'Khashuri Central'],
      [42.0250, 43.5500, 'Surami Pass Entrance'],
      [42.0420, 43.5150, 'Chumateleti'],
      [42.0550, 43.4850, 'Rikoti Tunnel / Mountain Pass'],
      [42.0720, 43.3750, 'Khevi'],
      [42.0950, 43.2350, 'Ubisa Monastery'],
      [42.1050, 43.0850, 'Shorapani'],
      [42.1100, 43.0333, 'Zestafoni E60'],
      [42.1550, 42.9200, 'Argveta'],
      [42.1950, 42.8550, 'Simoneti'],
      [42.2250, 42.7500, 'Rioni Bridge'],
      [42.2578, 42.6678, 'Kutaisi Chavchavadze Terminal'],
      [42.1800, 42.4820, 'Kutaisi International Airport (KUT)'],
      [42.1626, 42.3417, 'Samtredia Junction'],
      [42.1050, 42.2050, 'Sajavakho'],
      [42.0880, 42.0350, 'Lanchkhuti'],
      [42.0620, 41.8750, 'Supsa'],
      [42.0450, 41.7450, 'Grigoleti Coast'],
      [41.9917, 41.7761, 'Ureki Beach'],
      [41.9250, 41.7700, 'Shekvetili'],
      [41.8850, 41.7850, 'Natanebi'],
      [41.8111, 41.7750, 'Kobuleti Central'],
      [41.7580, 41.7500, 'Tsikhisdziri (Petra)'],
      [41.7220, 41.7280, 'Chakvi'],
      [41.6920, 41.7050, 'Mtsvane Kontskhi'],
      [41.6720, 41.6920, 'Makhinjauri'],
      [41.6580, 41.6770, 'Batumi Central Station'],
      [41.6434, 41.6399, 'Batumi Old Town']
    ];

    // Branch corridors
    const s1ZugdidiCorridor = [
      [42.1626, 42.3417, 'Samtredia'],
      [42.2050, 42.2050, 'Abasha'],
      [42.2680, 42.0670, 'Senaki Central'],
      [42.3180, 41.9050, 'Khobi'],
      [42.4150, 41.8850, 'Khamiskuri'],
      [42.5088, 41.8709, 'Zugdidi Central Bus Station']
    ];

    const s12PotiCorridor = [
      [42.0450, 41.7450, 'Grigoleti'],
      [42.1150, 41.7050, 'Maltakva'],
      [42.1462, 41.6720, 'Poti Central Station']
    ];

    const s8AkhaltsikheCorridor = [
      [41.9942, 43.5999, 'Khashuri'],
      [41.9250, 43.4900, 'Akhaldaba'],
      [41.8398, 43.3850, 'Borjomi Park Station'],
      [41.8250, 43.3450, 'Likani'],
      [41.7250, 43.1650, 'Atskuri'],
      [41.6390, 42.9826, 'Akhaltsikhe Central']
    ];

    const s3KazbegiCorridor = [
      [41.8410, 44.7150, 'Mtskheta'],
      [41.9180, 44.7170, 'Natakhtari'],
      [42.1350, 44.7700, 'Zhinvali Reservoir'],
      [42.1630, 44.7030, 'Ananuri Fortress'],
      [42.3500, 44.6900, 'Pasanauri'],
      [42.4770, 44.4850, 'Gudauri Ski Resort'],
      [42.5050, 44.4550, 'Jvari Pass (2,379m)'],
      [42.5550, 44.5050, 'Kobi'],
      [42.6567, 44.6433, 'Stepantsminda (Kazbegi)']
    ];

    const s5TelaviCorridor = [
      [41.6750, 44.8350, 'Tbilisi Ortachala'],
      [41.7150, 45.1850, 'Sartichala'],
      [41.7330, 45.3330, 'Sagarejo'],
      [41.6950, 45.7450, 'Bakurtsikhe'],
      [41.7450, 45.8000, 'Gurjaani'],
      [41.9198, 45.4731, 'Telavi Central']
    ];

    const svanetiCorridor = [
      [42.5088, 41.8709, 'Zugdidi Central'],
      [42.7150, 42.0250, 'Jvari / Enguri Dam'],
      [42.9150, 42.1850, 'Khaishi (Svaneti)'],
      [42.9950, 42.4350, 'Pari'],
      [43.0440, 42.7297, 'Mestia (Seti Square)']
    ];

    // Regional Marshrutka (Route Taxi) Branch Corridors
    const tskaltuboCorridor = [
      [42.2578, 42.6678, 'Kutaisi Chavchavadze Terminal'],
      [42.2850, 42.6350, 'Gvishtibi Junction'],
      [42.3260, 42.6000, 'Tskaltubo Central Spa Park']
    ];

    const tkibuliCorridor = [
      [42.2578, 42.6678, 'Kutaisi Chavchavadze Terminal'],
      [42.2800, 42.7500, 'Gelati Junction'],
      [42.3200, 42.8800, 'Orpiri'],
      [42.3500, 42.9900, 'Tkibuli Central Station']
    ];

    const chiaturaCorridor = [
      [42.1100, 43.0333, 'Zestafoni E60'],
      [42.1900, 43.1500, 'Terjola Turn'],
      [42.2900, 43.2800, 'Chiatura Cableways'],
      [42.3400, 43.4050, 'Sachkhere Central']
    ];

    const martviliCorridor = [
      [42.1626, 42.3417, 'Samtredia Junction'],
      [42.2800, 42.3600, 'Khoni Center'],
      [42.4150, 42.3800, 'Martvili Canyon Terminal']
    ];

    const baghdatiCorridor = [
      [42.2578, 42.6678, 'Kutaisi Chavchavadze Terminal'],
      [42.1600, 42.7500, 'Vartsikhe Reservoirs'],
      [42.0650, 42.8250, 'Baghdati Central'],
      [41.9100, 42.7500, 'Sairme Mineral Springs']
    ];

    const vaniCorridor = [
      [42.2578, 42.6678, 'Kutaisi Chavchavadze Terminal'],
      [42.1626, 42.3417, 'Samtredia Turn'],
      [42.0850, 42.5100, 'Vani Archaeological Reserve']
    ];

    const ambrolauriCorridor = [
      [42.3500, 42.9900, 'Tkibuli'],
      [42.3900, 43.0400, 'Nakerala Mountain Pass (1,218m)'],
      [42.4400, 43.0600, 'Shaori Reservoir'],
      [42.5180, 43.1480, 'Ambrolauri (Racha)']
    ];

    const sighnaghiCorridor = [
      [41.6750, 44.8350, 'Tbilisi Ortachala'],
      [41.7330, 45.3330, 'Sagarejo'],
      [41.6950, 45.7450, 'Bakurtsikhe Junction'],
      [41.6210, 45.9220, 'Sighnaghi (City of Love)']
    ];

    // Helper functions for spatial interpolation
    function getDistanceKm(lat1, lon1, lat2, lon2) {
      const R = 6371;
      const dLat = (lat2 - lat1) * Math.PI / 180;
      const dLon = (lon2 - lon1) * Math.PI / 180;
      const a = Math.sin(dLat/2) * Math.sin(dLat/2) +
                Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
                Math.sin(dLon/2) * Math.sin(dLon/2);
      const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
      return R * c;
    }

    function calculateBearing(lat1, lon1, lat2, lon2) {
      const y = Math.sin((lon2 - lon1) * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180);
      const x = Math.cos(lat1 * Math.PI / 180) * Math.sin(lat2 * Math.PI / 180) -
                Math.sin(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.cos((lon2 - lon1) * Math.PI / 180);
      let brng = Math.atan2(y, x) * 180 / Math.PI;
      return (brng + 360) % 360;
    }

    // Precompute segment distances, cumulative distances, and bearings once for zero-overhead fast runtime interpolation
    function preparePolyline(pts) {
      if (!pts || pts.length === 0) return { pts: [], dists: [], cumDists: [0], totalDist: 0, bearings: [] };
      let dists = [];
      let cumDists = [0];
      let bearings = [];
      let totalDist = 0;
      for (let i = 0; i < pts.length - 1; i++) {
        let p1 = pts[i];
        let p2 = pts[i + 1];
        let d = getDistanceKm(p1[0], p1[1], p2[0], p2[1]);
        dists.push(d);
        totalDist += d;
        cumDists.push(totalDist);
        bearings.push(calculateBearing(p1[0], p1[1], p2[0], p2[1]));
      }
      return { pts: pts, dists: dists, cumDists: cumDists, totalDist: totalDist, bearings: bearings };
    }

    function interpolateAlongPrepared(prep, fraction) {
      if (!prep || !prep.pts || prep.pts.length === 0) {
        return { lat: 42.26, lng: 42.70, heading: 0, nextStop: '', totalKm: 0, currentKm: 0 };
      }
      if (prep.pts.length === 1 || prep.totalDist === 0) {
        return { lat: prep.pts[0][0], lng: prep.pts[0][1], heading: 0, nextStop: prep.pts[0][2] || '', totalKm: 1, currentKm: 0 };
      }

      let targetDist = Math.max(0, Math.min(1, fraction)) * prep.totalDist;
      let cum = prep.cumDists;
      let i = 0;
      while (i < cum.length - 2 && cum[i + 1] < targetDist) {
        i++;
      }
      let segDist = prep.dists[i];
      let segFrac = segDist > 0 ? (targetDist - cum[i]) / segDist : 0;
      segFrac = Math.max(0, Math.min(1, segFrac));

      let p1 = prep.pts[i];
      let p2 = prep.pts[i + 1];
      let lat = p1[0] + segFrac * (p2[0] - p1[0]);
      let lng = p1[1] + segFrac * (p2[1] - p1[1]);
      let heading = prep.bearings[i] || 0;
      let nextStopName = p2[2] || ('Landmark #' + (i + 1));

      return {
        lat: lat,
        lng: lng,
        heading: heading,
        nextStop: nextStopName,
        totalKm: prep.totalDist,
        currentKm: targetDist
      };
    }

    function interpolateAlongPolyline(pts, fraction) {
      let prep = preparePolyline(pts);
      return interpolateAlongPrepared(prep, fraction);
    }

    function sliceCorridor(fullPath, fromIdx, toIdx) {
      if (fromIdx <= toIdx) {
        return fullPath.slice(fromIdx, toIdx + 1);
      } else {
        return fullPath.slice(toIdx, fromIdx + 1).reverse();
      }
    }

    // Build the complete fleet of 85 vehicles: 25 Marshrutkas (Route Taxis) + 42 Kutaisi Municipal Lines + 18 Intercity Coaches
    function buildVehiclesFleet() {
      const e60Pts = e60FullCorridor;

      // Operator Profiles
      const OP_MARSH = {
        name: 'Marshrutka (Route Taxi / სამარშრუტო ტაქსი)',
        badge: 'ROUTE TAXI',
        color: '#ea580c',
        textColor: '#ffffff',
        model: 'Mercedes-Benz Sprinter 519 CDI (18-seater)',
        features: ['Fixed-Route Shared Minibus', 'High Frequency (20-40 min)', 'Flexible Boarding', 'Luggage Compartment']
      };
      const OP_MUNI = {
        name: 'Kutaisi Municipal Transport',
        badge: 'KUTAISI BUS',
        color: '#0284c7',
        textColor: '#ffffff',
        model: 'Isuzu Novociti Life Low-Floor Bus',
        features: ['0.60 GEL Fare', 'Wheelchair Ramp', 'Contactless Tap', 'AVL Monitored']
      };
      const OP_CITYBUS = {
        name: 'Citybus Georgia',
        badge: 'CITYBUS',
        color: '#8e24aa',
        textColor: '#ffffff',
        model: 'Neoplan Tourliner Luxury Coach',
        features: ['Free Wi-Fi', '220V Outlets', 'Reclining Leather', 'AC', 'Express Non-Stop']
      };
      const OP_OMNIBUS = {
        name: 'Omnibus Express',
        badge: 'OMNIBUS',
        color: '#1e3a8a',
        textColor: '#facc15',
        model: 'Setra ComfortClass S 515 HD',
        features: ['Airport Direct', 'Flight Delay Guarantee', 'WizzAir Partner', 'USB Charger']
      };
      const OP_GEOBUS = {
        name: 'Georgian Bus',
        badge: 'GEO BUS',
        color: '#0891b2',
        textColor: '#ffffff',
        model: 'Scania Touring Airport Shuttle',
        features: ['Official Airport Carrier', 'Free 32kg Baggage', 'Direct Terminal Gate', 'AC']
      };
      const OP_METRO = {
        name: 'Metro Georgia',
        badge: 'METRO',
        color: '#dc2626',
        textColor: '#ffffff',
        model: 'TEMSA Maraton Coach',
        features: ['Individual Screen TV', 'Hot Tea & Coffee', 'High Comfort Seat', 'Steward Service']
      };

      const fleet = [
        // --- 1. FULL REGIONAL MARSHRUTKA (ROUTE TAXI) NETWORK (25 ROUTED TAXIS) ---
        { id: 'MR-OK-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Tbilisi (Didube / Okriba)', corridor: 'Highway E60 (Rikoti Tunnel Pass)', path: sliceCorridor(e60Pts, 24, 2), t: 0.40, speed: 0.00065, nominalSpeed: 78, depTime: '08:00', arrTime: '11:40', isMarsh: true, marshCarrier: 'LTD Okriba (Route Taxi)' },
        { id: 'MR-OK-02', op: OP_MARSH, route: 'Tbilisi (Didube / Okriba) → Kutaisi (Chavchavadze)', corridor: 'Highway E60 (Gori - Khashuri)', path: sliceCorridor(e60Pts, 2, 24), t: 0.65, speed: 0.00062, nominalSpeed: 80, depTime: '09:20', arrTime: '13:00', isMarsh: true, marshCarrier: 'LTD Okriba (Route Taxi)' },
        { id: 'MR-IM-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Batumi (Central / Mayakovsky)', corridor: 'Highway E60 (Samtredia - Kobuleti)', path: sliceCorridor(e60Pts, 24, 39), t: 0.52, speed: 0.00085, nominalSpeed: 76, depTime: '08:30', arrTime: '11:00', isMarsh: true, marshCarrier: 'LTD Imertrans (Route Taxi)' },
        { id: 'MR-IM-02', op: OP_MARSH, route: 'Batumi (Mayakovsky) → Kutaisi (Chavchavadze)', corridor: 'Highway E60 (Ureki Coast - Lanchkhuti)', path: sliceCorridor(e60Pts, 39, 24), t: 0.28, speed: 0.00080, nominalSpeed: 75, depTime: '10:00', arrTime: '12:30', isMarsh: true, marshCarrier: 'LTD Imertrans (Route Taxi)' },
        { id: 'MR-POT-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Poti Central Bus Station', corridor: 'Highway E60 + S12 Corridor', path: sliceCorridor(e60Pts, 24, 30).concat(s12PotiCorridor.slice(1)), t: 0.48, speed: 0.0011, nominalSpeed: 74, depTime: '09:00', arrTime: '10:35', isMarsh: true, marshCarrier: 'LTD Sprinter Express (Route Taxi)' },
        { id: 'MR-POT-02', op: OP_MARSH, route: 'Poti Central Bus Station → Kutaisi (Chavchavadze)', corridor: 'Highway S12 + E60 Corridor', path: s12PotiCorridor.slice().reverse().concat(sliceCorridor(e60Pts, 30, 24)), t: 0.72, speed: 0.0010, nominalSpeed: 72, depTime: '11:15', arrTime: '12:50', isMarsh: true, marshCarrier: 'LTD Sprinter Express (Route Taxi)' },
        { id: 'MR-ZUG-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Zugdidi Central Station', corridor: 'Highway E60 + S1 Corridor', path: sliceCorridor(e60Pts, 24, 26).concat(s1ZugdidiCorridor.slice(1)), t: 0.38, speed: 0.00095, nominalSpeed: 76, depTime: '08:45', arrTime: '10:45', isMarsh: true, marshCarrier: 'Regional Route Taxi Hub' },
        { id: 'MR-ZUG-02', op: OP_MARSH, route: 'Zugdidi Central Station → Kutaisi (Chavchavadze)', corridor: 'Highway S1 + E60 Corridor', path: s1ZugdidiCorridor.slice().reverse().concat(sliceCorridor(e60Pts, 26, 24)), t: 0.60, speed: 0.00090, nominalSpeed: 75, depTime: '12:00', arrTime: '14:00', isMarsh: true, marshCarrier: 'Regional Route Taxi Hub' },
        { id: 'MR-TSK-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) ↔ Tskaltubo Spa Resort', corridor: 'Kutaisi - Tskaltubo Route Taxi #30', path: tskaltuboCorridor, t: 0.55, speed: 0.0035, nominalSpeed: 60, depTime: '12:00', arrTime: '12:25', isMarsh: true, marshCarrier: 'Tskaltubo Local Route Taxi #30' },
        { id: 'MR-SAM-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) ↔ Samtredia Central Hub', corridor: 'Highway E60 Samtredia Branch', path: sliceCorridor(e60Pts, 24, 26), t: 0.32, speed: 0.0022, nominalSpeed: 70, depTime: '11:50', arrTime: '12:30', isMarsh: true, marshCarrier: 'Imereti Intercity Route Taxi' },
        { id: 'MR-TKI-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) ↔ Tkibuli Station', corridor: 'Gelati - Orpiri - Tkibuli Road', path: tkibuliCorridor, t: 0.45, speed: 0.0025, nominalSpeed: 65, depTime: '11:40', arrTime: '12:35', isMarsh: true, marshCarrier: 'Tkibuli Regional Route Taxi' },
        { id: 'MR-CHI-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Chiatura / Sachkhere', corridor: 'Zestafoni - Terjola - Chiatura Road', path: sliceCorridor(e60Pts, 24, 20).concat(chiaturaCorridor.slice(1)), t: 0.62, speed: 0.0012, nominalSpeed: 68, depTime: '10:30', arrTime: '12:15', isMarsh: true, marshCarrier: 'Chiatura Cable City Minibus' },
        { id: 'MR-BAG-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) ↔ Baghdati / Sairme Springs', corridor: 'Vartsikhe - Baghdati - Sairme Valley', path: baghdatiCorridor, t: 0.35, speed: 0.0020, nominalSpeed: 62, depTime: '11:30', arrTime: '12:45', isMarsh: true, marshCarrier: 'Sairme Spa Resort Route Taxi' },
        { id: 'MR-VAN-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) ↔ Vani Reserve', corridor: 'Rioni Valley - Vani Archaeological Road', path: vaniCorridor, t: 0.70, speed: 0.0022, nominalSpeed: 65, depTime: '11:45', arrTime: '12:40', isMarsh: true, marshCarrier: 'Vani District Route Taxi' },
        { id: 'MR-KHO-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Khoni / Martvili Canyon', corridor: 'Samtredia - Khoni - Martvili Road', path: sliceCorridor(e60Pts, 24, 26).concat(martviliCorridor.slice(1)), t: 0.50, speed: 0.0014, nominalSpeed: 68, depTime: '10:00', arrTime: '11:30', isMarsh: true, marshCarrier: 'Martvili Canyon Route Taxi' },
        { id: 'MR-AMB-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Ambrolauri (High Racha)', corridor: 'Tkibuli - Nakerala Pass - Shaori Lake', path: tkibuliCorridor.concat(ambrolauriCorridor.slice(1)), t: 0.42, speed: 0.0011, nominalSpeed: 55, depTime: '09:30', arrTime: '12:15', isMarsh: true, marshCarrier: 'Racha Mountain Route Taxi' },
        { id: 'MR-GOR-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) ↔ Gori (Comfort Bus)', corridor: 'Highway E60 (Rikoti - Khashuri - Gori)', path: sliceCorridor(e60Pts, 24, 9), t: 0.48, speed: 0.00075, nominalSpeed: 75, depTime: '08:30', arrTime: '11:00', isMarsh: true, marshCarrier: 'Gori Central Minibus Service' },
        { id: 'MR-MES-01', op: OP_MARSH, route: 'Kutaisi (Chavchavadze) → Mestia (Seti Square)', corridor: 'E60 + S1 + Svaneti Mountain Road', path: sliceCorridor(e60Pts, 24, 26).concat(s1ZugdidiCorridor.slice(1)).concat(svanetiCorridor.slice(1)), t: 0.62, speed: 0.00045, nominalSpeed: 54, depTime: '07:30', arrTime: '13:00', isMarsh: true, marshCarrier: 'Mestia Public Mountain Minibus' },
        { id: 'MR-KAZ-01', op: OP_MARSH, route: 'Tbilisi (Didube) ↔ Stepantsminda (Kazbegi)', corridor: 'Highway S3 (Georgian Military Road)', path: s3KazbegiCorridor, t: 0.65, speed: 0.00070, nominalSpeed: 62, depTime: '08:45', arrTime: '11:45', isMarsh: true, marshCarrier: 'Kazbegi Mountain Route Taxi' },
        { id: 'MR-GUD-01', op: OP_MARSH, route: 'Tbilisi (Didube) ↔ Gudauri Ski Resort', corridor: 'Highway S3 (Ananuri - Pasanauri - Gudauri)', path: s3KazbegiCorridor.slice(0, 6), t: 0.40, speed: 0.00085, nominalSpeed: 65, depTime: '09:15', arrTime: '11:30', isMarsh: true, marshCarrier: 'Gudauri Ski Express Minibus' },
        { id: 'MR-BOR-01', op: OP_MARSH, route: 'Tbilisi (Didube) ↔ Borjomi (Park Station)', corridor: 'Highway E60 + S8 Corridor', path: sliceCorridor(e60Pts, 2, 13).concat(s8AkhaltsikheCorridor.slice(1, 3)), t: 0.58, speed: 0.00075, nominalSpeed: 72, depTime: '09:45', arrTime: '12:15', isMarsh: true, marshCarrier: 'Borjomi Mineral Springs Minibus' },
        { id: 'MR-TEL-01', op: OP_MARSH, route: 'Tbilisi (Ortachala / Navtlughi) ↔ Telavi', corridor: 'Highway S5 (Kakheti Wine Road)', path: s5TelaviCorridor, t: 0.45, speed: 0.00088, nominalSpeed: 70, depTime: '10:00', arrTime: '12:10', isMarsh: true, marshCarrier: 'Kakheti Regional Route Taxi' },
        { id: 'MR-SIG-01', op: OP_MARSH, route: 'Tbilisi (Samgori) ↔ Sighnaghi (City of Love)', corridor: 'Highway S5 + Sighnaghi Branch', path: sighnaghiCorridor, t: 0.50, speed: 0.00092, nominalSpeed: 68, depTime: '10:15', arrTime: '12:25', isMarsh: true, marshCarrier: 'Sighnaghi Tourism Route Taxi' },
        { id: 'MR-SVN-01', op: OP_MARSH, route: 'Zugdidi Central ↔ Mestia (Seti Square)', corridor: 'Enguri Dam - Khaishi - Mestia Gorge', path: svanetiCorridor, t: 0.30, speed: 0.00060, nominalSpeed: 52, depTime: '12:00', arrTime: '16:30', isMarsh: true, marshCarrier: 'Svaneti Trans Minibus' },
        { id: 'MR-ADJ-01', op: OP_MARSH, route: 'Batumi (Old Town) ↔ Kobuleti / Sarpi Border', corridor: 'Black Sea Coastal Highway', path: sliceCorridor(e60Pts, 34, 40), t: 0.55, speed: 0.0016, nominalSpeed: 65, depTime: '12:15', arrTime: '13:00', isMarsh: true, marshCarrier: 'Adjara Coastal Route Taxi' },

        // --- 2. INTERCITY LUXURY COACHES (18 VEHICLES) ---
        // Citybus Georgia
        { id: 'CB-101', op: OP_CITYBUS, route: 'Tbilisi (Ortachala) → Batumi (New Station)', corridor: 'Highway E60 (Rikoti Tunnel Pass)', path: sliceCorridor(e60Pts, 0, 40), t: 0.42, speed: 0.00032, nominalSpeed: 86, depTime: '07:00', arrTime: '12:30' },
        { id: 'CB-102', op: OP_CITYBUS, route: 'Batumi (New Station) → Tbilisi (Ortachala)', corridor: 'Highway E60 (Zestafoni Section)', path: sliceCorridor(e60Pts, 40, 0), t: 0.58, speed: 0.00030, nominalSpeed: 84, depTime: '08:30', arrTime: '14:00' },
        { id: 'CB-103', op: OP_CITYBUS, route: 'Tbilisi (Didube) → Kutaisi (Chavchavadze)', corridor: 'Highway E60 (Gori - Ruisi Bypass)', path: sliceCorridor(e60Pts, 2, 24), t: 0.35, speed: 0.00045, nominalSpeed: 88, depTime: '09:00', arrTime: '12:40' },
        { id: 'CB-104', op: OP_CITYBUS, route: 'Kutaisi (Chavchavadze) → Tbilisi (Didube)', corridor: 'Highway E60 (Khashuri - Agara)', path: sliceCorridor(e60Pts, 24, 2), t: 0.65, speed: 0.00042, nominalSpeed: 82, depTime: '10:15', arrTime: '13:50' },
        { id: 'CB-105', op: OP_CITYBUS, route: 'Kutaisi (Chavchavadze) → Batumi (Central)', corridor: 'Highway E60 (Guria - Ureki Coast)', path: sliceCorridor(e60Pts, 24, 39), t: 0.70, speed: 0.00055, nominalSpeed: 85, depTime: '11:00', arrTime: '13:20' },
        { id: 'CB-106', op: OP_CITYBUS, route: 'Batumi (Central) → Kutaisi (Chavchavadze)', corridor: 'Highway E60 (Kobuleti - Shekvetili)', path: sliceCorridor(e60Pts, 39, 24), t: 0.28, speed: 0.00052, nominalSpeed: 82, depTime: '12:30', arrTime: '14:50' },
        { id: 'CB-107', op: OP_CITYBUS, route: 'Tbilisi (Ortachala) → Kobuleti Central', corridor: 'Highway E60 (Surami Mountain Curves)', path: sliceCorridor(e60Pts, 0, 34), t: 0.48, speed: 0.00034, nominalSpeed: 86, depTime: '13:00', arrTime: '18:15' },

        // Omnibus Express
        { id: 'OB-201', op: OP_OMNIBUS, route: 'Kutaisi Airport (KUT) → Tbilisi (Pushkin Sq)', corridor: 'Highway E60 (Rikoti Pass - Ubisa)', path: sliceCorridor(e60Pts, 25, 0), t: 0.52, speed: 0.00035, nominalSpeed: 88, depTime: '08:15', arrTime: '12:15' },
        { id: 'OB-202', op: OP_OMNIBUS, route: 'Tbilisi (Pushkin Sq) → Kutaisi Airport (KUT)', corridor: 'Highway E60 (Igoeti - Gori Highway)', path: sliceCorridor(e60Pts, 0, 25), t: 0.25, speed: 0.00036, nominalSpeed: 87, depTime: '09:30', arrTime: '13:30' },
        { id: 'OB-203', op: OP_OMNIBUS, route: 'Kutaisi Airport (KUT) → Batumi (Radisson Blu)', corridor: 'Highway E60 (Samtredia - Lanchkhuti)', path: sliceCorridor(e60Pts, 25, 39), t: 0.38, speed: 0.00060, nominalSpeed: 85, depTime: '10:00', arrTime: '12:10' },
        { id: 'OB-204', op: OP_OMNIBUS, route: 'Batumi (Radisson Blu) → Kutaisi Airport (KUT)', corridor: 'Highway E60 (Chakvi - Tsikhisdziri)', path: sliceCorridor(e60Pts, 39, 25), t: 0.62, speed: 0.00058, nominalSpeed: 83, depTime: '11:30', arrTime: '13:40' },
        { id: 'OB-205', op: OP_OMNIBUS, route: 'Tbilisi (Didube) → Zugdidi (Central)', corridor: 'E60 + S1 Highway Corridor', path: sliceCorridor(e60Pts, 2, 26).concat(s1ZugdidiCorridor.slice(1)), t: 0.60, speed: 0.00028, nominalSpeed: 84, depTime: '08:00', arrTime: '14:00' },
        { id: 'OB-206', op: OP_OMNIBUS, route: 'Zugdidi (Central) → Tbilisi (Didube)', corridor: 'S1 + E60 Highway Corridor', path: s1ZugdidiCorridor.slice().reverse().concat(sliceCorridor(e60Pts, 26, 2)), t: 0.30, speed: 0.00027, nominalSpeed: 85, depTime: '12:00', arrTime: '18:00' },

        // Georgian Bus
        { id: 'GB-301', op: OP_GEOBUS, route: 'Kutaisi Airport (KUT) → Tbilisi (Liberty Sq)', corridor: 'Highway E60 (Zestafoni - Shorapani)', path: sliceCorridor(e60Pts, 25, 0), t: 0.38, speed: 0.00036, nominalSpeed: 84, depTime: '07:30', arrTime: '11:30' },
        { id: 'GB-302', op: OP_GEOBUS, route: 'Tbilisi (Liberty Sq) → Kutaisi Airport (KUT)', corridor: 'Highway E60 (Nadarbazevi - Igoeti)', path: sliceCorridor(e60Pts, 0, 25), t: 0.20, speed: 0.00035, nominalSpeed: 85, depTime: '10:45', arrTime: '14:45' },
        { id: 'GB-303', op: OP_GEOBUS, route: 'Kutaisi Airport (KUT) → Batumi (Old Town)', corridor: 'Highway E60 (Shekvetili - Ureki)', path: sliceCorridor(e60Pts, 25, 40), t: 0.55, speed: 0.00058, nominalSpeed: 82, depTime: '09:15', arrTime: '11:25' },
        { id: 'GB-304', op: OP_GEOBUS, route: 'Batumi (Old Town) → Kutaisi Airport (KUT)', corridor: 'Highway E60 (Grigoleti - Supsa)', path: sliceCorridor(e60Pts, 40, 25), t: 0.45, speed: 0.00056, nominalSpeed: 83, depTime: '12:15', arrTime: '14:25' },
        { id: 'GB-305', op: OP_GEOBUS, route: 'Kutaisi (Chavchavadze) → Kopitnari Airport', corridor: 'Highway E60 (Kutaisi Airport Express)', path: sliceCorridor(e60Pts, 24, 25), t: 0.70, speed: 0.00180, nominalSpeed: 75, depTime: '13:00', arrTime: '13:30' },

        // Metro Georgia
        { id: 'MG-401', op: OP_METRO, route: 'Tbilisi (Ortachala) → Batumi (Central)', corridor: 'Highway E60 (Chumateleti - Rikoti)', path: sliceCorridor(e60Pts, 0, 39), t: 0.44, speed: 0.00031, nominalSpeed: 85, depTime: '07:45', arrTime: '13:15' },
        { id: 'MG-402', op: OP_METRO, route: 'Batumi (Central) → Tbilisi (Ortachala)', corridor: 'Highway E60 (Simoneti - Argveta)', path: sliceCorridor(e60Pts, 39, 0), t: 0.54, speed: 0.00030, nominalSpeed: 86, depTime: '09:00', arrTime: '14:30' },
        { id: 'MG-403', op: OP_METRO, route: 'Tbilisi (Ortachala) → Zugdidi (Bus Station)', corridor: 'E60 + S1 Highway Corridor', path: sliceCorridor(e60Pts, 0, 26).concat(s1ZugdidiCorridor.slice(1)), t: 0.72, speed: 0.00026, nominalSpeed: 84, depTime: '10:00', arrTime: '16:00' },
        { id: 'MG-404', op: OP_METRO, route: 'Zugdidi (Bus Station) → Tbilisi (Ortachala)', corridor: 'S1 + E60 Highway Corridor', path: s1ZugdidiCorridor.slice().reverse().concat(sliceCorridor(e60Pts, 26, 0)), t: 0.22, speed: 0.00025, nominalSpeed: 83, depTime: '11:30', arrTime: '17:30' },
        { id: 'MG-405', op: OP_METRO, route: 'Batumi (Central) → Akhaltsikhe (Central)', corridor: 'E60 + S8 Highway Corridor', path: sliceCorridor(e60Pts, 39, 13).concat(s8AkhaltsikheCorridor.slice(1)), t: 0.62, speed: 0.00032, nominalSpeed: 78, depTime: '08:30', arrTime: '14:30' },
        { id: 'MG-406', op: OP_METRO, route: 'Akhaltsikhe (Central) → Batumi (Central)', corridor: 'S8 + E60 Highway Corridor', path: s8AkhaltsikheCorridor.slice().reverse().concat(sliceCorridor(e60Pts, 13, 39)), t: 0.35, speed: 0.00031, nominalSpeed: 76, depTime: '12:00', arrTime: '18:00' }
      ];

      // --- 3. ALL 22 KUTAISI MUNICIPAL BUS LINES (42 ACTIVE BUSES) ---
      const kutaisiRoutesList = [
        '1L', '1R', '3', '4', '5', '6', '7', '10', '19', '20', '25', '31', '41',
        '110', '111', '112', '114', '118', '126', '133', '137', '141'
      ];

      kutaisiRoutesList.forEach((rNo, idx) => {
        let p0 = KUTAISI_ALL_PATTERNS[rNo + '_0'];
        let p1 = KUTAISI_ALL_PATTERNS[rNo + '_1'] || p0;

        if (p0 && p0.stops && p0.stops.length >= 2) {
          let t0 = ((idx * 0.17 + 0.12) % 0.85);
          fleet.push({
            id: `KB-${rNo}-A`,
            op: OP_MUNI,
            route: `Line ${rNo}: ${p0.name} (Dir 0)`,
            corridor: p0.dir_text || `Kutaisi Municipal Line ${rNo}`,
            path: p0.stops,
            t: t0,
            speed: 0.0020 + (idx % 4) * 0.0003,
            nominalSpeed: 32 + (idx % 6),
            depTime: '11:45',
            arrTime: '12:25',
            routeNo: rNo
          });
        }

        if (p1 && p1.stops && p1.stops.length >= 2) {
          let t1 = ((idx * 0.19 + 0.55) % 0.85);
          fleet.push({
            id: `KB-${rNo}-B`,
            op: OP_MUNI,
            route: `Line ${rNo}: ${p1.name} (Return)`,
            corridor: p1.dir_text || `Kutaisi Municipal Line ${rNo} Return`,
            path: p1.stops,
            t: t1,
            speed: 0.0022 + (idx % 3) * 0.0002,
            nominalSpeed: 30 + (idx % 5),
            depTime: '11:55',
            arrTime: '12:35',
            routeNo: rNo
          });
        }
      });

      return fleet;
    }

    // Initialize Map
    function initMap() {
      const geography = __DESKTOP_MAP__;
      map = L.map('map', { zoomControl: false, zoomAnimation: false, zoomSnap: 0.5, minZoom: 6, maxZoom: 17 })
        .setView([42.15, 43.30], 8);
      const canvas = document.querySelector('.map-canvas');
      new ResizeObserver(() => {
        if (canvas.clientWidth && canvas.clientHeight) map.invalidateSize({ pan: false });
      }).observe(canvas);

      window.schematicBase = L.layerGroup().addTo(map);
      map.createPane('schematicLand').style.zIndex = 250;
      L.polygon(geography.outline, {
        pane: 'schematicLand',
        color: '#cbdbd5', weight: 1, fillColor: '#f7f9f2', fillOpacity: 1, interactive: false
      }).addTo(window.schematicBase);

      window.transitCorridorsGroup = L.layerGroup().addTo(map);
      window.intersectionHubsGroup = L.layerGroup();
      window.kutaisiLinesGroup = L.layerGroup().addTo(map);
      L.polyline(geography.highway, { color: '#5b8f85', weight: 5.5 }).addTo(window.transitCorridorsGroup);
      L.polyline(geography.highway, { color: '#74a999', weight: 4 }).addTo(window.transitCorridorsGroup);
      geography.branches.forEach(branch => {
        L.polyline(branch, { color: '#d9e2dd', weight: 2 }).addTo(window.schematicBase);
      });

      const majorCities = new Set(['Tbilisi', 'Kutaisi', 'Batumi', 'Zugdidi', 'Poti']);
      const cityLabels = geography.cities.map(([lat, lon, name]) => ({
        name,
        marker: L.marker([lat, lon], {
          interactive: false,
          icon: L.divIcon({ className: 'map-city', html: name, iconSize: [8, 16], iconAnchor: [4, 8] })
        })
      }));
      const municipalStops = L.layerGroup(geography.stops.map(([lat, lon, name]) =>
        L.circleMarker([lat, lon], { radius: 4, color: '#fff', weight: 1, fillColor: '#1e88e5', fillOpacity: .75 })
          .bindTooltip(() => {
            const label = document.createElement('span');
            label.textContent = name;
            return label;
          })
      ));
      function updateMapDetail() {
        const zoom = map.getZoom();
        cityLabels.forEach(({ name, marker }) => {
          const show = majorCities.has(name) || (zoom >= 9.5 && (!name.includes('(') || zoom >= 11.5));
          if (show) marker.addTo(window.schematicBase);
          else marker.remove();
        });
        if (zoom >= 12) municipalStops.addTo(map);
        else municipalStops.remove();
        updateMapStatus();
      }
      municipalCluster = L.marker([42.2662, 42.7180], { title: 'Zoom to Kutaisi vehicles' }).on('click', zoomKutaisi);
      map.on('zoomend', updateMapDetail);
      map.on('move zoom resize', syncYandexView);
      window.streetTiles = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        referrerPolicy: 'strict-origin-when-cross-origin',
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
      });
      streetTiles.on('load', () => {
        if (!map.hasLayer(streetTiles)) return;
        if (streetTiles.getContainer()?.querySelector('.leaflet-tile-loaded')) {
          schematicBase.remove();
        } else {
          schematicBase.addTo(map);
        }
      });
      if (location.protocol === 'file:') {
        const button = document.getElementById('btnBasemap');
        button.disabled = true;
        button.title = 'Open the preview through localhost to use the street map.';
      }

      // Group for stop markers and route polylines
      stopMarkersGroup = L.layerGroup().addTo(map);

      // Draw all 22 Kutaisi municipal lines on the map with clear visual hierarchy
      const colorPalette = [
        '#0284c7', '#0ea5e9', '#38bdf8', '#0284c7', '#0369a1', '#2563eb',
        '#1d4ed8', '#1e40af', '#0284c7', '#0891b2', '#06b6d4', '#22d3ee',
        '#059669', '#10b981', '#34d399', '#7c3aed', '#8b5cf6', '#a855f7',
        '#c026d3', '#d946ef', '#db2777', '#f43f5e'
      ];

      let colorIdx = 0;
      Object.keys(KUTAISI_ALL_PATTERNS).forEach(k => {
        let p = KUTAISI_ALL_PATTERNS[k];
        if (p.stops && p.stops.length >= 2) {
          let linePts = p.stops.map(st => [st[0], st[1]]);
          let polyColor = colorPalette[colorIdx % colorPalette.length];
          colorIdx++;

          let isReturnOrCross = k.endsWith('_1');

          // Subtle casing so overlapping streets do not blur together
          L.polyline(linePts, {
            color: '#0f172a',
            weight: 4.5,
            opacity: 0.45,
            lineCap: 'round',
            lineJoin: 'round'
          }).addTo(window.kutaisiLinesGroup);

          // Line core: Inbound/intersecting return lines are rendered DASHED
          let poly = L.polyline(linePts, {
            color: polyColor,
            weight: 3,
            opacity: isReturnOrCross ? 0.75 : 0.85,
            dashArray: isReturnOrCross ? '6, 6' : null,
            className: isReturnOrCross ? 'dashed-intersection-line' : '',
            lineCap: 'round',
            lineJoin: 'round'
          }).addTo(window.kutaisiLinesGroup);

          poly.bindTooltip(`<b>Line ${p.route}:</b> ${p.name}<br><small>${p.dir_text} ${isReturnOrCross ? '(Dashed Return Leg)' : '(Outbound)'}</small>`, { sticky: true });
          kutaisiRoutePolylines[k] = poly;
        }
      });

      // Populate Kutaisi Line Filter Dropdown with ALL 22 Routes
      const lineSelect = document.getElementById('kutaisiLineFilterSelect');
      const uniqueRoutes = [
        '1L', '1R', '3', '4', '5', '6', '7', '10', '19', '20', '25', '31', '41',
        '110', '111', '112', '114', '118', '126', '133', '137', '141'
      ];

      uniqueRoutes.forEach(r => {
        let p = KUTAISI_ALL_PATTERNS[r + '_0'] || KUTAISI_ALL_PATTERNS[r + '_1'];
        let opt = document.createElement('option');
        opt.value = r;
        opt.innerText = `Line ${r} — ${p ? p.name : 'Kutaisi Bus'}`;
        lineSelect.appendChild(opt);
      });

      // Populate Marshrutka Quick Filter Dropdown
      const marshSelect = document.getElementById('marshrutkaFilterSelect');
      const marshRoutesList = [
        { code: 'MR-OK-01', label: 'Kutaisi ↔ Tbilisi (Didube / Okriba)' },
        { code: 'MR-IM-01', label: 'Kutaisi ↔ Batumi (Mayakovsky Hub)' },
        { code: 'MR-POT-01', label: 'Kutaisi ↔ Poti Central' },
        { code: 'MR-ZUG-01', label: 'Kutaisi ↔ Zugdidi Central' },
        { code: 'MR-TSK-01', label: 'Kutaisi ↔ Tskaltubo Spa Resort (#30)' },
        { code: 'MR-SAM-01', label: 'Kutaisi ↔ Samtredia Junction' },
        { code: 'MR-TKI-01', label: 'Kutaisi ↔ Tkibuli (Racha Road)' },
        { code: 'MR-CHI-01', label: 'Kutaisi ↔ Chiatura / Sachkhere' },
        { code: 'MR-BAG-01', label: 'Kutaisi ↔ Baghdati / Sairme' },
        { code: 'MR-VAN-01', label: 'Kutaisi ↔ Vani Reserve' },
        { code: 'MR-KHO-01', label: 'Kutaisi ↔ Khoni / Martvili Canyon' },
        { code: 'MR-AMB-01', label: 'Kutaisi ↔ Ambrolauri (High Racha)' },
        { code: 'MR-GOR-01', label: 'Kutaisi ↔ Gori (Comfort Bus)' },
        { code: 'MR-MES-01', label: 'Kutaisi ↔ Mestia (Svaneti Minibus)' },
        { code: 'MR-KAZ-01', label: 'Tbilisi ↔ Stepantsminda (Kazbegi)' },
        { code: 'MR-GUD-01', label: 'Tbilisi ↔ Gudauri Ski Resort' },
        { code: 'MR-BOR-01', label: 'Tbilisi ↔ Borjomi / Bakuriani' },
        { code: 'MR-TEL-01', label: 'Tbilisi ↔ Telavi (Kakheti Wine)' },
        { code: 'MR-SIG-01', label: 'Tbilisi ↔ Sighnaghi' }
      ];

      marshRoutesList.forEach(m => {
        let opt = document.createElement('option');
        opt.value = m.code;
        opt.innerText = m.label;
        marshSelect.appendChild(opt);
      });

      // Major Transit Intersections & Interchange Hubs
      const majorHubs = [
        { name: 'Tbilisi Didube Intermodal Hub', lat: 41.7508, lon: 44.7788, desc: 'Major Northern Gateway · Metro Line 1 · Okriba & Kazbegi Terminals', lines: 'Highway E60, S3, Marshrutkas, Tbilisi Metro' },
        { name: 'Tbilisi Ortachala International Station', lat: 41.6750, lon: 44.8350, desc: 'Eastern Gateway · Kakheti & Southern Intercity Hub', lines: 'Highway E60, S5 Kakheti, Metro Georgia' },
        { name: 'Kutaisi Chavchavadze Central Hub', lat: 42.2578, lon: 42.6678, desc: 'Central Transit Hub · Omnibus Platform · 22 Municipal Lines · Regional Route Taxis', lines: 'Highway E60, All 22 Kutaisi City Buses, 25 Marshrutkas' },
        { name: 'Kutaisi Red Bridge (წითელი ხიდი)', lat: 42.2712, lon: 42.6996, desc: 'Rioni River Crossing · Central Historic Interchange', lines: 'Lines 1L, 1R, 4, 10, 19, 111, 114' },
        { name: 'Samtredia Junction (E60 / S1)', lat: 42.1626, lon: 42.3417, desc: 'Major Western Highway Interchange (Zugdidi / Batumi Split)', lines: 'Highway E60, S1 Zugdidi, Samegrelo Marshrutkas' },
        { name: 'Grigoleti Coast Junction (E60 / S12)', lat: 42.0450, lon: 41.7450, desc: 'Black Sea Highway Interchange (Poti Seaport / Adjara Coast Split)', lines: 'Highway E60, S12 Poti Corridor' },
        { name: 'Khashuri Central Junction (E60 / S8)', lat: 41.9942, lon: 43.5999, desc: 'Shida Kartli Highway Interchange (Borjomi / Akhaltsikhe Split)', lines: 'Highway E60, S8 Samtskhe-Javakheti Corridor' },
        { name: 'Mtskheta Junction (E60 / S3)', lat: 41.8410, lon: 44.7150, desc: 'Georgian Military Highway Interchange (Kazbegi / Stepantsminda Split)', lines: 'Highway E60, S3 Military Highway' },
        { name: 'Zestafoni Interchange (E60 / Chiatura)', lat: 42.1080, lon: 43.0450, desc: 'Imereti Mining & Industrial Highway Interchange', lines: 'Highway E60, Chiatura / Sachkhere Road' },
        { name: 'Rikoti Mountain Pass Interchange', lat: 42.0550, lon: 43.4850, desc: 'Continental Divide Interchange (East / West Georgia)', lines: 'Highway E60 Highway Tunnel Bypass' },
        { name: 'Gori Highway Interchange', lat: 41.9842, lon: 44.1158, desc: 'Shida Kartli Regional Transit Interchange', lines: 'Highway E60, Shida Kartli Route Taxis' },
        { name: 'Batumi Central Station Interchange', lat: 41.6580, lon: 41.6770, desc: 'Adjara Transit Terminal · Intercity Coaches · Black Sea Fleet', lines: 'Highway E60, Citybus, Omnibus, Metro GEO' },
        { name: 'Kutaisi International Airport (KUT)', lat: 42.1800, lon: 42.4820, desc: 'Air-Rail-Bus Intermodal Terminal', lines: 'Highway E60, Georgian Bus, Omnibus Airport Express' },
        { name: 'Zugdidi Central Station', lat: 42.5088, lon: 41.8709, desc: 'Samegrelo-Svaneti Transit Gateway', lines: 'S1, Svaneti Mountain Route, Regional Taxis' },
        { name: 'Stepantsminda (Kazbegi) Terminal', lat: 42.6567, lon: 44.6433, desc: 'Greater Caucasus Mountain Hub', lines: 'S3, Kazbegi Route Taxis' }
      ];

      majorHubs.forEach(h => {
        // Outer pulsing dashed orbit ring indicating transit intersection
        L.circleMarker([h.lat, h.lon], {
          radius: 12,
          color: '#facc15',
          weight: 2.2,
          dashArray: '4, 4',
          className: 'intersection-halo-ring',
          fillColor: '#fef08a',
          fillOpacity: 0.22
        }).addTo(window.intersectionHubsGroup);

        // Core intersection hub dot
        L.circleMarker([h.lat, h.lon], {
          radius: 5.5,
          color: '#0f172a',
          weight: 2,
          fillColor: '#ffffff',
          fillOpacity: 1
        }).addTo(window.intersectionHubsGroup).bindPopup(`
          <div style="font-family:sans-serif; min-width:210px;">
            <div style="display:flex; align-items:center; gap:6px; margin-bottom:4px;">
              <span style="background:#0f172a; color:#facc15; font-size:10px; font-weight:800; padding:2px 6px; border-radius:4px; border:1px dashed #facc15;">🔀 INTERSECTION</span>
              <span style="font-size:11px; color:#16a34a; font-weight:700;">● ACTIVE</span>
            </div>
            <b style="font-size:0.95rem; color:#0f172a;">${h.name}</b><br>
            <span style="font-size:0.8rem; color:#475569;">${h.desc}</span>
            <div style="margin-top:6px; font-size:0.75rem; background:#f8fafc; border:1px solid #e2e8f0; border-radius:4px; padding:4px 6px;">
              <b style="color:#0284c7;">Intersecting Lines:</b> ${h.lines}
            </div>
          </div>
        `);
      });

      window.dashedIntersectionsEnabled = false;
      window.toggleDashedIntersections = function() {
        window.dashedIntersectionsEnabled = !window.dashedIntersectionsEnabled;
        const btn = document.getElementById('btnDashedIntersections');
        if (btn) {
          if (window.dashedIntersectionsEnabled) {
            btn.innerHTML = '🔀 Intersections: Dashed ON';
            btn.style.color = '#facc15';
            btn.style.borderColor = '#facc15';
            if (window.intersectionHubsGroup && !map.hasLayer(window.intersectionHubsGroup)) map.addLayer(window.intersectionHubsGroup);
          } else {
            btn.innerHTML = '🔀 Intersections: Dashed OFF';
            btn.style.color = '#94a3b8';
            btn.style.borderColor = '#475569';
            if (window.intersectionHubsGroup && map.hasLayer(window.intersectionHubsGroup)) map.removeLayer(window.intersectionHubsGroup);
          }
        }
      };

      function getVehicleArrowEl(v) {
        if (v.arrowEl && v.arrowEl.isConnected) return v.arrowEl;
        if (!v.marker) return null;
        let el = v.marker.getElement();
        if (el) {
          v.arrowEl = el.querySelector('.heading-arrow');
          return v.arrowEl;
        }
        return null;
      }

      // Build and register vehicles
      const fleetData = buildVehiclesFleet();

      fleetData.forEach(v => {
        v.preparedPath = preparePolyline(v.path);
        let pos = interpolateAlongPrepared(v.preparedPath, v.t);

        let iconLabel = v.routeNo ? `#${v.routeNo}` : (v.isMarsh ? "MRS" : v.op.badge.slice(0, 3));

        let customIcon = L.divIcon({
          className: v.routeNo ? 'vehicle-marker-icon municipal-vehicle' : 'vehicle-marker-icon',
          html: `
            <div class="bus-marker-badge" style="background:${v.routeNo ? '#0369a1' : v.isMarsh ? '#c2410c' : '#6d28a7'}; color:white;">
              <span class="heading-arrow" style="transform: rotate(${Math.round(pos.heading)}deg);"></span>
              <span>${iconLabel}</span>
            </div>
          `,
          iconSize: [25, 25],
          iconAnchor: [12, 12]
        });

        let marker = L.marker([pos.lat, pos.lng], { icon: customIcon }).addTo(map);

        marker.on('click', () => {
          showVehicleInspector(v);
        });

        v.marker = marker;
        v.currentHeading = pos.heading;
        v.lastHeading = Math.round(pos.heading);
        v.currentLat = pos.lat;
        v.currentLng = pos.lng;
        v.nextStopName = pos.nextStop;
        v.totalKm = pos.totalKm;
        v.currentKm = pos.currentKm;
        allVehicleSims.push(v);
      });

      updateMapDetail();
      updateSimClock();

      // Animation ticker and elapsed-time clock
      let simTickCounter = 0;
      let lastTick = performance.now();
      setInterval(() => {
        const now = performance.now();
        const elapsed = now - lastTick;
        lastTick = now;
        if (!simRunning) return;
        simTickCounter++;

        simTime = new Date(simTime.getTime() + elapsed * simSpeed);
        if (simTickCounter % 4 === 0) updateSimClock();

        allVehicleSims.forEach(v => {
          v.t += v.speed * (simSpeed / 25);
          if (v.t > 1.0) v.t = 0.0;

          // Skip DOM operations completely if vehicle is filtered out
          if (!map.hasLayer(v.marker)) {
            if (window.inspectedVehicleId === v.id && (simTickCounter % 4 === 0)) {
              let pos = interpolateAlongPrepared(v.preparedPath, v.t);
              v.currentLat = pos.lat;
              v.currentLng = pos.lng;
              v.currentHeading = pos.heading;
              v.nextStopName = pos.nextStop;
              v.totalKm = pos.totalKm;
              v.currentKm = pos.currentKm;
              updateInspectorDynamic(v);
            }
            return;
          }

          let pos = interpolateAlongPrepared(v.preparedPath, v.t);
          v.currentLat = pos.lat;
          v.currentLng = pos.lng;
          v.currentHeading = pos.heading;
          v.nextStopName = pos.nextStop;
          v.totalKm = pos.totalKm;
          v.currentKm = pos.currentKm;

          // Smooth GPU hardware-accelerated movement via Leaflet setLatLng & CSS transition
          v.marker.setLatLng([pos.lat, pos.lng]);

          // Update heading rotation on live element without rebuilding DOM
          let arrow = getVehicleArrowEl(v);
          if (arrow) {
            let roundH = Math.round(pos.heading);
            if (v.lastHeading !== roundH) {
              arrow.style.transform = `rotate(${roundH}deg)`;
              v.lastHeading = roundH;
            }
          }

          if (window.inspectedVehicleId === v.id && (simTickCounter % 3 === 0)) {
            updateInspectorDynamic(v);
          }
        });
      }, 50);
    }

    function showVehicleInspector(v) {
      document.getElementById('vehicleInspector').classList.remove('is-empty');
      window.inspectedVehicleId = v.id;
      const b = document.getElementById('inspBadge');
      b.innerText = v.routeNo ? `LINE #${v.routeNo}` : (v.isMarsh ? 'ROUTE TAXI' : v.op.badge);
      b.style.background = v.op.color;
      b.style.color = v.op.textColor;

      document.getElementById('inspId').innerText = v.id;
      document.getElementById('inspOperator').innerText = v.marshCarrier ? v.marshCarrier : v.op.name;
      document.getElementById('inspModel').innerText = v.op.model;
      document.getElementById('inspRoute').innerText = v.route;
      document.getElementById('inspCorridor').innerHTML = `Corridor / Road: <b>${v.corridor}</b>`;
      document.getElementById('inspDepTime').innerText = v.depTime;
      document.getElementById('inspArrTime').innerText = v.arrTime;

      updateInspectorDynamic(v);
    }

    function updateInspectorDynamic(v) {
      let pct = Math.round(v.t * 100);
      document.getElementById('inspProgressBar').style.width = pct + '%';
      document.getElementById('inspProgressText').innerText = `${pct}% along route (${v.currentKm.toFixed(1)} / ${v.totalKm.toFixed(1)} km)`;
      document.getElementById('inspSpeed').innerText = `${v.nominalSpeed} km/h • Bearing ${Math.round(v.currentHeading)}°`;
      document.getElementById('inspNextStop').innerText = `Next Landmark: ${v.nextStopName}`;
    }

    // Operator Filtering
    function filterOperator(opKey) {
      currentOperatorFilter = opKey;

      document.querySelectorAll('.op-pill').forEach(btn => btn.classList.remove('active'));
      event.target.classList.add('active');

      applyVisibilityFilters();
    }

    // Marshrutka Route Taxi Quick Filter
    function handleMarshrutkaFilter() {
      currentMarshrutkaFilter = document.getElementById('marshrutkaFilterSelect').value;
      currentKutaisiLineFilter = 'ALL';
      document.getElementById('kutaisiLineFilterSelect').value = 'ALL';

      applyVisibilityFilters();

      if (currentMarshrutkaFilter !== 'ALL') {
        let v = allVehicleSims.find(x => x.id === currentMarshrutkaFilter);
        if (v && v.path && v.path.length >= 2) {
          if (highlightedRouteLayer) {
            map.removeLayer(highlightedRouteLayer);
            highlightedRouteLayer = null;
          }
          stopMarkersGroup.clearLayers();

          let latLngs = v.path.map(st => [st[0], st[1]]);
          highlightedRouteLayer = L.layerGroup().addTo(map);

          // Casing
          L.polyline(latLngs, {
            color: '#0f172a',
            weight: 8.5,
            opacity: 0.9,
            lineCap: 'round',
            lineJoin: 'round'
          }).addTo(highlightedRouteLayer);

          // Vibrant Orange Route Taxi Core
          L.polyline(latLngs, {
            color: '#f97316',
            weight: 5,
            opacity: 1,
            lineCap: 'round',
            lineJoin: 'round'
          }).addTo(highlightedRouteLayer);

          // Animated dashed flow along highway
          L.polyline(latLngs, {
            color: '#fef08a',
            weight: 2,
            dashArray: '6, 6',
            className: 'dashed-intersection-line',
            lineCap: 'round'
          }).addTo(highlightedRouteLayer);

          map.fitBounds(L.polyline(latLngs).getBounds(), { padding: [40, 40] });
          showVehicleInspector(v);
        }
      } else {
        if (highlightedRouteLayer) {
          map.removeLayer(highlightedRouteLayer);
          highlightedRouteLayer = null;
        }
      }
    }

    // Kutaisi Municipal Line Selector
    function handleKutaisiLineFilter() {
      currentKutaisiLineFilter = document.getElementById('kutaisiLineFilterSelect').value;
      currentMarshrutkaFilter = 'ALL';
      document.getElementById('marshrutkaFilterSelect').value = 'ALL';

      applyVisibilityFilters();

      if (currentKutaisiLineFilter !== 'ALL') {
        highlightKutaisiRoute(currentKutaisiLineFilter);
      } else {
        if (highlightedRouteLayer) {
          map.removeLayer(highlightedRouteLayer);
          highlightedRouteLayer = null;
        }
        stopMarkersGroup.clearLayers();
      }
    }

    function highlightKutaisiRoute(rNo) {
      if (highlightedRouteLayer) {
        map.removeLayer(highlightedRouteLayer);
        highlightedRouteLayer = null;
      }
      stopMarkersGroup.clearLayers();

      let p = KUTAISI_ALL_PATTERNS[rNo + '_0'] || KUTAISI_ALL_PATTERNS[rNo + '_1'];
      if (!p || !p.stops || p.stops.length === 0) return;

      let latLngs = p.stops.map(st => [st[0], st[1]]);
      highlightedRouteLayer = L.layerGroup().addTo(map);

      // 1. Dark casing for the selected primary route
      L.polyline(latLngs, {
        color: '#0f172a',
        weight: 9,
        opacity: 0.9,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(highlightedRouteLayer);

      // 2. Glowing yellow primary line
      L.polyline(latLngs, {
        color: '#facc15',
        weight: 5.5,
        opacity: 1,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(highlightedRouteLayer);

      // 3. Dynamically find all intersecting routes in Kutaisi and render them as DASHED lines!
      let myStops = p.stops;
      let intersectingRoutes = new Set();
      Object.keys(KUTAISI_ALL_PATTERNS).forEach(k => {
        if (k.startsWith(rNo + '_')) return;
        let other = KUTAISI_ALL_PATTERNS[k];
        if (!other.stops || other.stops.length < 2) return;
        let crosses = other.stops.some(oSt => {
          return myStops.some(mSt => Math.abs(oSt[0] - mSt[0]) < 0.0018 && Math.abs(oSt[1] - mSt[1]) < 0.0025);
        });
        if (crosses && !intersectingRoutes.has(other.route)) {
          intersectingRoutes.add(other.route);
          let oPts = other.stops.map(st => [st[0], st[1]]);
          // Render intersecting route with dashed styling!
          L.polyline(oPts, {
            color: '#38bdf8',
            weight: 3.2,
            opacity: 0.7,
            dashArray: '6, 8',
            className: 'dashed-intersection-line',
            lineCap: 'round'
          }).addTo(highlightedRouteLayer).bindTooltip(`<b>🔀 Intersecting Route:</b> Line ${other.route} (${other.name})`, { sticky: true });
        }
      });

      // 4. Render stops along the route with intersection highlights
      p.stops.forEach((st, idx) => {
        L.circleMarker([st[0], st[1]], {
          radius: 5,
          color: '#0f172a',
          fillColor: '#ffffff',
          fillOpacity: 1,
          weight: 2.5
        }).addTo(stopMarkersGroup).bindPopup(`<b>Stop #${idx + 1}: ${st[2]}</b><br>Kutaisi Municipal Line ${rNo}`);
      });

      map.fitBounds(L.polyline(latLngs).getBounds(), { padding: [40, 40] });

      let bus = allVehicleSims.find(x => x.routeNo === rNo);
      if (bus) showVehicleInspector(bus);
    }

    function applyVisibilityFilters() {
      let visibleCount = 0;
      allVehicleSims.forEach(v => {
        let matchOp = (currentOperatorFilter === 'ALL') ||
                      (currentOperatorFilter === 'Route Taxi' && v.isMarsh) ||
                      (v.op.name.toLowerCase().includes(currentOperatorFilter.toLowerCase()));

        let matchLine = (currentKutaisiLineFilter === 'ALL') || (v.routeNo === currentKutaisiLineFilter);
        let matchMarsh = (currentMarshrutkaFilter === 'ALL') || (v.id === currentMarshrutkaFilter);

        if (matchOp && matchLine && matchMarsh) {
          if (!map.hasLayer(v.marker)) map.addLayer(v.marker);
          visibleCount++;
        } else {
          if (map.hasLayer(v.marker)) map.removeLayer(v.marker);
        }
      });

      updateMapStatus(visibleCount);
    }

    function updateMapStatus(vehicleCount = allVehicleSims.filter(vehicle => map.hasLayer(vehicle.marker)).length) {
      const grouped = map.getZoom() < 10
        ? allVehicleSims.filter(vehicle => vehicle.routeNo && map.hasLayer(vehicle.marker)).length
        : 0;
      map.getContainer().classList.toggle('grouped', grouped > 0);
      if (grouped > 0) {
        municipalCluster.setIcon(L.divIcon({
          className: 'vehicle-cluster', html: String(grouped), iconSize: [34, 34], iconAnchor: [17, 17]
        })).addTo(map);
      } else {
        municipalCluster.remove();
      }
      document.getElementById('activeFleetPill').textContent =
        `Zoom: ${map.getZoom().toFixed(1)}x | Simulated vehicles: ${vehicleCount} | Municipal stops: ${__STOP_COUNT__}`;
    }

    const georgiaClock = new Intl.DateTimeFormat('en-GB', {
      timeZone: 'Asia/Tbilisi', hour: '2-digit', minute: '2-digit', second: '2-digit'
    });

    function updateSimClock() {
      document.getElementById('simClockText').textContent = `${georgiaClock.format(simTime)} · Georgia time`;
    }

    function toggleSim() {
      simRunning = !simRunning;
      document.getElementById('btnPlayPause').innerText = simRunning ? '⏸ Pause' : '▶ Play';
    }

    function resetSim() {
      simTime = new Date();
      simRunning = true;
      document.getElementById('btnPlayPause').textContent = '⏸ Pause';
      document.getElementById('simSpeedSelect').value = '1';
      changeSpeed();
      updateSimClock();
    }

    function changeSpeed() {
      simSpeed = parseInt(document.getElementById('simSpeedSelect').value);
    }

    function zoomGeorgia() {
      map.setView([42.15, 43.30], 8);
    }

    function zoomKutaisi() {
      map.setView([42.2662, 42.7180], 13);
    }

    function zoomRikoti() {
      map.setView([42.0550, 43.4850], 11);
    }

    function setOriginQuick(val) {
      const input = document.getElementById('plannerOrigin');
      if (input) {
        input.value = val;
        calculateJourney();
      }
    }

    // 1. Bus 20 real street stops (Line 20: Tabukashvili 205 -> Chavchavadze Omnibus Terminal)
    const BUS_20_STREET_STOPS = [
      [42.231467, 42.671027, 'რ.თაბუკაშვილის ქუჩა N205'],
      [42.233305, 42.672214, 'რ. თაბუკაშვილის ქუჩა N183'],
      [42.234389, 42.673052, 'რ.თაბუკაშვილის ქუჩა N183-B'],
      [42.236550, 42.674310, 'რ.თაბუკაშვილის ქუჩა N159'],
      [42.239768, 42.676468, 'რ.თაბუკაშვილი ქუჩა N119'],
      [42.241550, 42.677800, 'რ.თაბუკაშვილის ქუჩა N111'],
      [42.244200, 42.678900, 'რ.თაბუკაშვილის ქუჩა N77'],
      [42.247500, 42.679500, 'რ. თაბუკაშვილის ქუჩა N65'],
      [42.249800, 42.678500, 'რ. თაბუკაშვილის ქუჩა N29'],
      [42.252000, 42.676500, 'რ. თაბუკაშვილის ქუჩა N5'],
      [42.253800, 42.674500, '"შენგელია არენა"'],
      [42.255500, 42.673000, 'ი.ჭავჭავაძის გამზირი N42'],
      [42.256710, 42.671950, 'ცენტრალური ავტოსადგური (Omnibus Terminal)']
    ];

    // 2. Bus 25 real street stops (Line 25: KIU Campus -> Akhalgazrdoba -> Chavchavadze Omnibus Terminal)
    const BUS_25_KIU_STOPS = [
      [42.214444, 42.710544, '"ქუთაისი საერთაშორისო უნივერსიტეტი"'],
      [42.231520, 42.712003, 'ახალგაზრდობის გამზირი N21'],
      [42.234197, 42.712145, 'ახალგაზრდობის გამზირი N19'],
      [42.237176, 42.712483, 'ახალგაზრდობის  გამზირი N21'],
      [42.240353, 42.712870, 'ახალგაზრდობის გამზირი N23'],
      [42.245579, 42.712096, 'ახალგაზრდობის გამზირი N12'],
      [42.246939, 42.709153, 'ა. წერეთლის ქუჩა N220 "გ"'],
      [42.248861, 42.694689, 'ი. ჭავჭავაძის გამზირი N6'],
      [42.249537, 42.689783, 'ი.ჭავჭავაძის გამზირი 8/4'],
      [42.250108, 42.686989, 'ი. ჭავჭავაძის  გამზირი N12'],
      [42.252205, 42.682146, '"შენგელია არენა"'],
      [42.254359, 42.677322, 'ი.ჭავჭავაძის  გამზირი N42'],
      [42.256710, 42.671950, 'ცენტრალური ავტოსადგური (Omnibus Terminal)']
    ];

    // 3. Bus 10 real street stops (Line 10: Red Bridge -> Aghmashenebeli -> Chavchavadze Omnibus Terminal)
    const BUS_10_REDBRIDGE_STOPS = [
      [42.271179, 42.699622, 'წითელი ხიდი'],
      [42.269795, 42.698001, 'შ.რუსთაველის გამზირი N10'],
      [42.267410, 42.695415, 'ნ. დუმბაძის ქუჩა N2'],
      [42.262238, 42.692997, '"ნინოშვილის ბაღი"'],
      [42.261752, 42.692533, 'აღმაშენებელის გამზირი N58 "ა"'],
      [42.261841, 42.686667, 'აღმაშენებლის გამზირი N82'],
      [42.261864, 42.684345, 'აღმაშენებლის გამზირი N93'],
      [42.261867, 42.681681, 'აღმაშენებლის გამზირი N118'],
      [42.261947, 42.679497, 'აღმაშენებლის გამზირი N132'],
      [42.262012, 42.675552, 'აღმაშენებლის გამზირი N144'],
      [42.261453, 42.672982, 'ზ. გამსახურდიას გამზირი N28'],
      [42.260460, 42.671381, 'ზ.გამსახურდიას გამზირი N32'],
      [42.258788, 42.668539, 'ზ.გამსახურდიას  გამზირი N44'],
      [42.257891, 42.667855, 'ი.ჭავჭავაძის  გამზირი N63 (Omnibus Terminal)']
    ];

    // 4. Line 111 real stops for Bagrati Cathedral
    const BUS_111_BAGRATI_STOPS = [
      [42.277712, 42.704495, 'ბაგრატის ტაძარი'],
      [42.276968, 42.703012, 'ბაგრატის ქუჩა N23'],
      [42.270910, 42.699605, 'წითელი ხიდი'],
      [42.262238, 42.692997, '"ნინოშვილის ბაღი"'],
      [42.257891, 42.667855, 'ი.ჭავჭავაძის გამზირი N63 (Omnibus Terminal)']
    ];

    // Real Highway E60 corridor from Kutaisi Omnibus Terminal to Tbilisi Didube Terminal
    const HIGHWAY_KUTAISI_TBILISI = [
      [42.25671, 42.67195], // Kutaisi Chavchavadze Omnibus Terminal
      [42.2510, 42.7100],   // Kutaisi East Bypass
      [42.2150, 42.8250],   // Argveta Interchange
      [42.1100, 43.0333],   // Zestafoni Bypass
      [42.0850, 43.1500],   // Shorapani
      [42.0620, 43.2750],   // Dzirula
      [42.0520, 43.4350],   // Ubisa
      [42.0550, 43.4850],   // Rikoti Pass Tunnel (850m)
      [42.0250, 43.5450],   // Surami Bypass
      [41.9942, 43.5999],   // Khashuri Highway Junction
      [41.9842, 43.8200],   // Agara
      [41.9842, 44.1158],   // Gori Highway Interchange
      [41.9950, 44.4050],   // Igoeti
      [41.9450, 44.5700],   // Ksani / Mukhrani Junction
      [41.8410, 44.7150],   // Mtskheta Bypass
      [41.8020, 44.7550],   // Zahesi Gateway
      [41.7508, 44.7788]    // Tbilisi Didube Bus Terminal
    ];

    let currentResolvedOrigin = null;

    function resolveOriginLocation(origStr) {
      const s = (origStr || '').toLowerCase();

      // Check KIU (Kutaisi International University)
      if (s.includes('kiu') || s.includes('საერთაშორისო') || s.includes('უნივერსიტეტ') || s.includes('campus')) {
        return {
          id: 'kiu',
          name: 'Kutaisi International University (KIU)',
          coords: [42.21400, 42.71030],
          stopName: 'Stop #1: "ქუთაისი საერთაშორისო უნივერსიტეტი"',
          stopCoords: [42.214444, 42.710544],
          walkMeters: 80,
          walkMinutes: 1,
          lineNum: '25',
          lineName: 'ავტომშენებელი-ახალგაზრდობის გამზირი (Line 25)',
          lineStops: BUS_25_KIU_STOPS,
          hubStopName: 'ცენტრალური ავტოსადგური / Chavchavadze Omnibus Terminal',
          muniRideMins: 20,
          transferMins: 10,
          fareMuni: 0.40,
          notice: '<b>Origin: Kutaisi International University (KIU Campus)</b><br>Direct campus transit via <b>Kutaisi Municipal Bus Line 25</b> from campus stop (<b>"ქუთაისი საერთაშორისო უნივერსიტეტი"</b>) directly along Akhalgazrdoba & Chavchavadze Ave to <b>Chavchavadze Central Bus Station / Omnibus Terminal</b> (12 stops, 20 min).'
        };
      }

      // Check Tabukashvili Street 217
      if (s.includes('tabukashvili') || s.includes('თაბუკაშვილ') || s.includes('217')) {
        return {
          id: 'tabukashvili',
          name: 'Tabukashvili Street 217, Kutaisi',
          coords: [42.23020, 42.67050],
          stopName: 'Stop #0: რ.თაბუკაშვილის ქუჩა N205',
          stopCoords: [42.231467, 42.671027],
          walkMeters: 160,
          walkMinutes: 2,
          lineNum: '20',
          lineName: 'თაბუკაშვილი-წითელი ხიდი (Line 20)',
          lineStops: BUS_20_STREET_STOPS,
          hubStopName: 'ცენტრალური ავტოსადგური / Chavchavadze Omnibus Terminal',
          muniRideMins: 17,
          transferMins: 13,
          fareMuni: 0.40,
          notice: '<b>Exact Location: Tabukashvili Street 217, Kutaisi</b><br>Walk 160m (~2 min) to nearest municipal stop (<b>Stop #0: რ.თაბუკაშვილის ქუჩა N205</b>), board <b>Kutaisi Municipal Bus Line 20</b> to <b>Chavchavadze Central Bus Station / Omnibus Terminal</b> (12 stops, 17 min).'
        };
      }

      // Check Red Bridge
      if (s.includes('red bridge') || s.includes('წითელი') || s.includes('ხიდი')) {
        return {
          id: 'redbridge',
          name: 'Kutaisi (Red Bridge - წითელი ხიდი)',
          coords: [42.27118, 42.69962],
          stopName: 'Stop #0: წითელი ხიდი',
          stopCoords: [42.271179, 42.699622],
          walkMeters: 40,
          walkMinutes: 1,
          lineNum: '10',
          lineName: 'სულხან-საბა (Line 10)',
          lineStops: BUS_10_REDBRIDGE_STOPS,
          hubStopName: 'ი.ჭავჭავაძის გამზირი N63 / Central Bus Station',
          muniRideMins: 18,
          transferMins: 12,
          fareMuni: 0.40,
          notice: '<b>Origin: Red Bridge (წითელი ხიდი)</b><br>Board <b>Kutaisi Municipal Bus Line 10</b> at Red Bridge directly along Aghmashenebeli & Chavchavadze to <b>Chavchavadze Central Bus Station / Omnibus Terminal</b> (13 stops, 18 min).'
        };
      }

      // Check Bagrati Cathedral
      if (s.includes('bagrati') || s.includes('ბაგრატ')) {
        return {
          id: 'bagrati',
          name: 'Bagrati Cathedral, Kutaisi (ბაგრატი)',
          coords: [42.27771, 42.70449],
          stopName: 'Stop #3: ბაგრატის ტაძარი',
          stopCoords: [42.277712, 42.704495],
          walkMeters: 50,
          walkMinutes: 1,
          lineNum: '111',
          lineName: 'ჩანჩიბაძე-ბაგრატის ტაძარი (Line 111)',
          lineStops: BUS_111_BAGRATI_STOPS,
          hubStopName: 'ცენტრალური ავტოსადგური / Chavchavadze Omnibus Terminal',
          muniRideMins: 22,
          transferMins: 12,
          fareMuni: 0.40,
          notice: '<b>Origin: Bagrati Cathedral (ბაგრატის ტაძარი)</b><br>Board <b>Line 111</b> at Bagrati Cathedral stop, connecting to Central Hub.'
        };
      }

      // Check Chavchavadze / Omnibus Terminal
      if (s.includes('chavchavadze') || s.includes('ჭავჭავაძ') || s.includes('omnibus') || s.includes('ავტოსადგურ')) {
        return {
          id: 'chavchavadze',
          name: 'Kutaisi (Chavchavadze Omnibus Terminal)',
          coords: [42.25671, 42.67195],
          stopName: 'ცენტრალური ავტოსადგური (Direct Boarding)',
          stopCoords: [42.25671, 42.67195],
          walkMeters: 0,
          walkMinutes: 0,
          lineNum: null,
          lineName: 'Direct Terminal Departure',
          lineStops: [],
          hubStopName: 'ცენტრალური ავტოსადგური / Chavchavadze Omnibus Terminal',
          muniRideMins: 0,
          transferMins: 0,
          fareMuni: 0.00,
          notice: '<b>Origin: Chavchavadze Central Bus Station / Omnibus Terminal</b><br>Direct intercity departure from Omnibus Terminal platform. Zero municipal transit or walking needed.'
        };
      }

      // Default: Kutaisi Center / General
      return {
        id: 'general',
        name: origStr || 'Kutaisi Central',
        coords: [42.26620, 42.71800],
        stopName: 'Nearest Municipal Stop (Line 1L/10)',
        stopCoords: [42.267410, 42.695415],
        walkMeters: 280,
        walkMinutes: 4,
        lineNum: '10',
        lineName: 'Kutaisi Municipal Network',
        lineStops: BUS_10_REDBRIDGE_STOPS,
        hubStopName: 'ცენტრალური ავტოსადგური / Chavchavadze Omnibus Terminal',
        muniRideMins: 16,
        transferMins: 12,
        fareMuni: 0.40,
        notice: `<b>Origin: ${origStr}</b><br>Walk 280m (~4 min) to nearest municipal stop, board connecting bus to <b>Chavchavadze Central Bus Station / Omnibus Terminal</b>.`
      };
    }

    // Journey Planner calculation
    function calculateJourney() {
      const origInput = document.getElementById('plannerOrigin');
      const orig = (origInput ? origInput.value : '').trim();
      const destSelect = document.getElementById('plannerDest');
      const dest = destSelect ? destSelect.value : 'Tbilisi';
      const container = document.getElementById('itinerariesList');
      if (!container) return;

      const origInfo = resolveOriginLocation(orig);
      currentResolvedOrigin = origInfo;

      const isDirectTerminal = !origInfo.lineNum;

      const coachDurationMins = 210; // 3h 30m
      const coachFare = 20.00;
      const marshDurationMins = 205; // 3h 25m
      const marshFare = 15.00;
      const citybusDurationMins = 220; // 3h 40m
      const citybusFare = 18.00;

      // Recommended itinerary metrics
      const recTotalMins = origInfo.walkMinutes + origInfo.muniRideMins + origInfo.transferMins + coachDurationMins;
      const recHours = Math.floor(recTotalMins / 60);
      const recMins = recTotalMins % 60;
      const recTotalFare = (origInfo.fareMuni + coachFare).toFixed(2);

      // Marshrutka itinerary metrics
      const marshTotalMins = origInfo.walkMinutes + origInfo.muniRideMins + 8 + marshDurationMins;
      const marshHours = Math.floor(marshTotalMins / 60);
      const marshMins = marshTotalMins % 60;
      const marshTotalFare = (origInfo.fareMuni + marshFare).toFixed(2);

      // Citybus metrics
      const cityTotalMins = origInfo.walkMinutes + origInfo.muniRideMins + 30 + citybusDurationMins;
      const cityHours = Math.floor(cityTotalMins / 60);
      const cityMins = cityTotalMins % 60;
      const cityTotalFare = (origInfo.fareMuni + citybusFare).toFixed(2);

      container.innerHTML = `
        <div class="journey-notice">
          <span style="font-size:1.3rem;">📍</span>
          <div>${origInfo.notice}</div>
        </div>

        <!-- ITINERARY 1: Recommended Coach -->
        <div class="itinerary-card active">
          <div class="itinerary-header">
            <div>
              <span style="font-size:1.02rem; color:var(--navy);">⭐ Recommended: ${isDirectTerminal ? 'Direct Omnibus Express Coach' : `Walk + Kutaisi Bus #${origInfo.lineNum} + Omnibus Express`}</span>
              <div style="font-size:0.78rem; color:#64748b; margin-top:2px;">Origin: ${origInfo.name} ➔ Destination: ${dest} (Didube Terminal)</div>
            </div>
            <div style="text-align:right;">
              <span style="color:#0f6cbd; font-size:1.05rem; font-weight:800;">${recHours}h ${recMins}m | ${recTotalFare} ₾</span>
              <div style="font-size:0.75rem; color:#16a34a; font-weight:600;">🚶 ${origInfo.walkMeters}m walk · 🔄 ${isDirectTerminal ? 0 : 1} transfer</div>
            </div>
          </div>

          ${isDirectTerminal ? '' : `
          <div class="leg-step">
            <span class="leg-icon icon-walk">W</span>
            <div>
              <b>Walk ${origInfo.walkMeters} meters (~${origInfo.walkMinutes} min)</b> from ${origInfo.name} to nearest transit stop:
              <span style="color:#0284c7; font-weight:600;">${origInfo.stopName}</span> (Departs 08:04 AM)
            </div>
          </div>

          <div class="leg-step">
            <span class="leg-icon icon-muni">${origInfo.lineNum}</span>
            <div>
              Board <b>Kutaisi Municipal Bus #${origInfo.lineNum}</b> (${origInfo.lineName}) at 08:05 AM —
              Ride ${origInfo.lineStops.length - 1} intermediate stops to
              <b>${origInfo.hubStopName}</b> (Arrive: 08:${5 + origInfo.muniRideMins < 10 ? '0' : ''}${5 + origInfo.muniRideMins} AM · ${origInfo.muniRideMins} min ride · <b>${origInfo.fareMuni.toFixed(2)} ₾</b>)
            </div>
          </div>

          <div class="leg-step">
            <span class="leg-icon" style="background:#475569;">🔄</span>
            <div>
              <b>Station Transfer Buffer (${origInfo.transferMins} mins):</b> Walk across platform to Omnibus Express Departure Bay #2. Restroom & ticket verification available.
            </div>
          </div>
          `}

          <div class="leg-step">
            <span class="leg-icon icon-inter">🚌</span>
            <div>
              Board <b>Omnibus Express Coach</b> (Setra S 515 HD ComfortClass with AC & Wi-Fi) departing at <b>08:35 AM</b> —
              Highway E60 express transit via Zestaponi, Rikoti Pass Tunnel, Surami bypass, Khashuri, Gori to <b>${dest} (Didube Terminal)</b> (Arrive: <b>12:05 PM</b> · 3h 30m · <b>20.00 ₾</b>)
            </div>
          </div>

          <div class="leg-step">
            <span class="leg-icon" style="background:#dc2626;">🏁</span>
            <div>
              <b>Arrive at ${dest} (Didube Bus Terminal):</b> 12:05 PM (Total Duration: <b>${recHours}h ${recMins}m</b> · Total Fare: <b>${recTotalFare} ₾</b>)
            </div>
          </div>

          <div style="margin-top:10px; display:flex; justify-content:flex-end;">
            <button class="btn btn-primary" style="padding:6px 14px; font-size:0.82rem;" onclick="simulateJourneyOnMap(0)">▶ Simulate This Journey On Live Map</button>
          </div>
        </div>

        <!-- ITINERARY 2: Frequent Route Taxi (Marshrutka) -->
        <div class="itinerary-card">
          <div class="itinerary-header">
            <div>
              <span style="font-size:1.02rem; color:var(--navy);">⚡ Fastest / High Frequency: ${isDirectTerminal ? 'Direct LTD Okriba Marshrutka' : `Walk + Bus #${origInfo.lineNum} + LTD Okriba Marshrutka`}</span>
              <div style="font-size:0.78rem; color:#64748b; margin-top:2px;">Origin: ${origInfo.name} ➔ ${dest} (Platform 3)</div>
            </div>
            <div style="text-align:right;">
              <span style="color:#ea580c; font-size:1.05rem; font-weight:800;">${marshHours}h ${marshMins}m | ${marshTotalFare} ₾</span>
              <div style="font-size:0.75rem; color:#16a34a; font-weight:600;">🚶 ${origInfo.walkMeters}m walk · 🔄 ${isDirectTerminal ? 0 : 1} transfer</div>
            </div>
          </div>

          ${isDirectTerminal ? '' : `
          <div class="leg-step">
            <span class="leg-icon icon-walk">W</span>
            <div><b>Walk ${origInfo.walkMeters}m</b> to ${origInfo.stopName}</div>
          </div>
          <div class="leg-step">
            <span class="leg-icon icon-muni">${origInfo.lineNum}</span>
            <div>Board <b>Kutaisi Bus #${origInfo.lineNum}</b> to ${origInfo.hubStopName} (${origInfo.fareMuni.toFixed(2)} ₾)</div>
          </div>
          `}
          <div class="leg-step">
            <span class="leg-icon icon-marsh">M</span>
            <div>
              Transfer (8 min buffer) to <b>LTD Okriba Mercedes-Benz Sprinter 519 CDI (Route Taxi M-101 / სამარშრუტო ტაქსი)</b> at Platform 5 —
              Depart <b>08:30 AM</b> directly to ${dest} (Arrive <b>11:55 AM</b> · 3h 25m · <b>15.00 ₾</b> · Departs every 20-30 min)
            </div>
          </div>
          <div class="leg-step">
            <span class="leg-icon" style="background:#dc2626;">🏁</span>
            <div><b>Arrive at ${dest} (Didube Minibus Hub):</b> 11:55 AM (Total: <b>${marshHours}h ${marshMins}m</b> · Total Fare: <b>${marshTotalFare} ₾</b>)</div>
          </div>

          <div style="margin-top:10px; display:flex; justify-content:flex-end;">
            <button class="btn btn-secondary" style="padding:6px 14px; font-size:0.82rem;" onclick="simulateJourneyOnMap(1)">▶ Simulate Marshrutka On Live Map</button>
          </div>
        </div>

        <!-- ITINERARY 3: Budget Coach via Citybus Georgia -->
        <div class="itinerary-card">
          <div class="itinerary-header">
            <div>
              <span style="font-size:1.02rem; color:var(--navy);">💰 Budget Coach: ${isDirectTerminal ? 'Direct Citybus Georgia Express' : `Walk + Bus #${origInfo.lineNum} + Citybus Georgia Express`}</span>
              <div style="font-size:0.78rem; color:#64748b; margin-top:2px;">Origin: ${origInfo.name} ➔ ${dest}</div>
            </div>
            <div style="text-align:right;">
              <span style="color:#8e24aa; font-size:1.05rem; font-weight:800;">${cityHours}h ${cityMins}m | ${cityTotalFare} ₾</span>
              <div style="font-size:0.75rem; color:#16a34a; font-weight:600;">🚶 ${origInfo.walkMeters}m walk · 🔄 ${isDirectTerminal ? 0 : 1} transfer</div>
            </div>
          </div>

          ${isDirectTerminal ? '' : `
          <div class="leg-step">
            <span class="leg-icon icon-walk">W</span>
            <div><b>Walk ${origInfo.walkMeters}m</b> to ${origInfo.stopName}</div>
          </div>
          <div class="leg-step">
            <span class="leg-icon icon-muni">${origInfo.lineNum}</span>
            <div>Board <b>Kutaisi Bus #${origInfo.lineNum}</b> to ${origInfo.hubStopName} (${origInfo.fareMuni.toFixed(2)} ₾)</div>
          </div>
          `}
          <div class="leg-step">
            <span class="leg-icon" style="background:#8e24aa;">C</span>
            <div>
              Transfer (30 min buffer) to <b>Citybus Georgia (GEO-101) MAN Lion's Coach</b> departing at <b>09:00 AM</b> —
              Arrive in ${dest} at <b>12:40 PM</b> (3h 40m · <b>18.00 ₾</b>)
            </div>
          </div>
          <div style="margin-top:10px; display:flex; justify-content:flex-end;">
            <button class="btn btn-secondary" style="padding:6px 14px; font-size:0.82rem;" onclick="simulateJourneyOnMap(2)">▶ Simulate Citybus On Live Map</button>
          </div>
        </div>

        <!-- ITINERARY 4: Direct Taxi / Rideshare to Terminal + Omnibus Express -->
        <div class="itinerary-card">
          <div class="itinerary-header">
            <div>
              <span style="font-size:1.02rem; color:var(--navy);">🚕 Door-to-Door Rideshare: Bolt / Taxi to Omnibus Terminal + Omnibus Express</span>
              <div style="font-size:0.78rem; color:#64748b; margin-top:2px;">Zero walking · Direct pickup at ${origInfo.name}</div>
            </div>
            <div style="text-align:right;">
              <span style="color:#1e3a8a; font-size:1.05rem; font-weight:800;">3h 45m | 25.00 ₾</span>
              <div style="font-size:0.75rem; color:#64748b; font-weight:600;">🚶 0m walk · 🔄 1 transfer</div>
            </div>
          </div>

          <div class="leg-step">
            <span class="leg-icon" style="background:#eab308; color:#1e293b;">🚕</span>
            <div>On-demand Bolt / City Taxi pickup directly at ${origInfo.name} at 08:18 AM ➔ Chavchavadze Omnibus Terminal (8 min ride · <b>5.00 ₾</b>)</div>
          </div>
          <div class="leg-step">
            <span class="leg-icon icon-inter">🚌</span>
            <div>Board <b>Omnibus Express Coach</b> at 08:35 AM directly to ${dest} (Arrive 12:05 PM · <b>20.00 ₾</b>)</div>
          </div>
          <div style="margin-top:10px; display:flex; justify-content:flex-end;">
            <button class="btn btn-secondary" style="padding:6px 14px; font-size:0.82rem;" onclick="simulateJourneyOnMap(3)">▶ Simulate Rideshare On Live Map</button>
          </div>
        </div>
      `;
    }

    function simulateJourneyOnMap(idx) {
      switchTab('tab-simulation');
      const tabs = document.querySelectorAll('nav.tabs button');
      tabs.forEach((b, i) => b.classList.toggle('active', i === 2));

      stopJourneySim();

      const origInfo = currentResolvedOrigin || resolveOriginLocation(document.getElementById('plannerOrigin')?.value);

      window.journeySimLayer = L.featureGroup().addTo(map);

      // 1. Origin Marker
      const origIcon = L.divIcon({
        className: 'orig-pin',
        html: `<div style="background:#16a34a; color:white; border-radius:999px; padding:3px 9px; font-size:11px; font-weight:800; border:2px solid white; box-shadow:0 2px 8px rgba(0,0,0,0.4); white-space:nowrap;">📍 You: ${origInfo.name}</div>`,
        iconSize: [180, 26],
        iconAnchor: [90, 13]
      });
      L.marker(origInfo.coords, { icon: origIcon }).addTo(window.journeySimLayer)
        .bindPopup(`<b>📍 Your Exact Location:</b> ${origInfo.name}<br><small>Start of Multimodal Journey</small>`);

      // 2. Walking Path & Bus Stop (if walking required)
      if (origInfo.walkMeters > 0) {
        // Casing
        L.polyline([origInfo.coords, origInfo.stopCoords], {
          color: '#431407',
          weight: 6,
          opacity: 0.85,
          lineCap: 'round'
        }).addTo(window.journeySimLayer);

        // Dashed pedestrian path
        L.polyline([origInfo.coords, origInfo.stopCoords], {
          color: '#ea580c',
          weight: 3.5,
          dashArray: '6, 6',
          className: 'dashed-intersection-line',
          opacity: 1,
          lineCap: 'round'
        }).addTo(window.journeySimLayer).bindTooltip(`<b>🚶 Pedestrian Walkway:</b> ${origInfo.walkMeters}m to bus stop`, { sticky: true });

        const busStopIcon = L.divIcon({
          className: 'stop-pin',
          html: `<div style="background:#0284c7; color:white; border-radius:999px; padding:2px 7px; font-size:10px; font-weight:800; border:2px solid white; box-shadow:0 2px 6px rgba(0,0,0,0.3); white-space:nowrap;">🚏 ${origInfo.stopName}</div>`,
          iconSize: [230, 24],
          iconAnchor: [115, 12]
        });
        L.marker(origInfo.stopCoords, { icon: busStopIcon }).addTo(window.journeySimLayer)
          .bindPopup(`<b>🚏 Transit Stop:</b> ${origInfo.stopName}<br>Walk Distance: ${origInfo.walkMeters}m (~${origInfo.walkMinutes} min)<br><b>Connecting Line:</b> Kutaisi Municipal Bus Line ${origInfo.lineNum}`);
      }

      // 3. Municipal Bus Street Route (if applicable)
      if (origInfo.lineStops && origInfo.lineStops.length > 0) {
        const lineCoords = origInfo.lineStops.map(s => [s[0], s[1]]);
        // Casing
        L.polyline(lineCoords, {
          color: '#082f49',
          weight: 8,
          opacity: 0.85,
          lineCap: 'round',
          lineJoin: 'round'
        }).addTo(window.journeySimLayer);

        // Core line
        L.polyline(lineCoords, {
          color: '#0284c7',
          weight: 5,
          opacity: 0.95,
          lineCap: 'round',
          lineJoin: 'round'
        }).addTo(window.journeySimLayer).bindTooltip(`<b>🚌 Transit Leg:</b> Kutaisi Municipal Bus #${origInfo.lineNum}`, { sticky: true });
      }

      // 4. Omnibus Central Terminal Hub & Dashed Transfer Link
      const hubIcon = L.divIcon({
        className: 'hub-pin',
        html: '<div style="background:#1e3a8a; color:#facc15; border-radius:999px; padding:3px 9px; font-size:11px; font-weight:800; border:2px solid white; box-shadow:0 3px 8px rgba(0,0,0,0.4); white-space:nowrap;">🔀 Transfer Hub: Omnibus Terminal</div>',
        iconSize: [210, 26],
        iconAnchor: [105, 13]
      });
      L.marker([42.25671, 42.67195], { icon: hubIcon }).addTo(window.journeySimLayer)
        .bindPopup('<b>🔀 Transfer Intersection:</b> ცენტრალური ავტოსადგური / Chavchavadze Omnibus Terminal<br>Transfer buffer: <b>' + origInfo.transferMins + ' min</b> to <b>Omnibus Express Coach</b>');

      // Transfer interchange dashed link between arriving bus stop and departure bay
      if (origInfo.lineStops && origInfo.lineStops.length > 0) {
        let lastStop = origInfo.lineStops[origInfo.lineStops.length - 1];
        L.polyline([[lastStop[0], lastStop[1]], [42.25671, 42.67195]], {
          color: '#facc15',
          weight: 5,
          dashArray: '5, 5',
          className: 'dashed-transfer-link',
          lineCap: 'round'
        }).addTo(window.journeySimLayer).bindTooltip('<b>🔀 Platform Transfer Interchange (Dashed Link)</b>', { sticky: true });
      }

      // 5. Highway E60 Corridor to Destination with casing and dashed track
      L.polyline(HIGHWAY_KUTAISI_TBILISI, {
        color: '#0f172a',
        weight: 8,
        opacity: 0.9,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(window.journeySimLayer);

      L.polyline(HIGHWAY_KUTAISI_TBILISI, {
        color: idx === 1 ? '#ea580c' : '#8e24aa',
        weight: 5,
        opacity: 0.95,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(window.journeySimLayer).bindTooltip(idx === 1 ? '<b>🚐 Regional Route Taxi:</b> Highway E60 Express' : '<b>🚍 Omnibus Express Coach:</b> Highway E60 Express', { sticky: true });

      L.polyline(HIGHWAY_KUTAISI_TBILISI, {
        color: '#facc15',
        weight: 2,
        dashArray: '8, 8',
        className: 'dashed-intersection-line',
        lineCap: 'round'
      }).addTo(window.journeySimLayer);

      // 6. Destination Marker
      const destIcon = L.divIcon({
        className: 'dest-pin',
        html: '<div style="background:#dc2626; color:white; border-radius:999px; padding:3px 8px; font-size:11px; font-weight:800; border:2px solid white; box-shadow:0 3px 8px rgba(0,0,0,0.4); white-space:nowrap;">🏁 Tbilisi Didube Terminal</div>',
        iconSize: [160, 26],
        iconAnchor: [80, 13]
      });
      L.marker([41.7508, 44.7788], { icon: destIcon }).addTo(window.journeySimLayer)
        .bindPopup('<b>🏁 Destination:</b> Tbilisi (Didube Bus Terminal)<br>Connecting to Didube Metro Station');

      // Fit map bounds
      map.fitBounds(window.journeySimLayer.getBounds(), { padding: [50, 50] });

      // Add HUD
      const hud = document.createElement('div');
      hud.id = 'journeyHud';
      hud.className = 'journey-hud';
      hud.innerHTML = `
        <div style="font-weight:700; color:#38bdf8;">🗺️ Live Route Simulation:</div>
        <div id="hudStageText" style="color:#e2e8f0; font-size:0.86rem; font-weight:600;">
          🚶 Starting journey from ${origInfo.name}...
        </div>
        <button onclick="stopJourneySim()" style="background:#475569; color:white; border:none; padding:4px 10px; border-radius:4px; font-size:0.75rem; cursor:pointer;">Exit</button>
      `;
      document.getElementById('tab-simulation').appendChild(hud);

      // Animated traveler path - smoothly subdivided so movement glides without lag
      const rawSteps = [];
      if (origInfo.walkMeters > 0) {
        rawSteps.push({ pt: origInfo.coords, mode: 'walk', label: `🚶 Step 1: Walking from ${origInfo.name} (${origInfo.walkMeters}m)...` });
        rawSteps.push({ pt: origInfo.stopCoords, mode: 'muni', label: `🚏 Step 2: Arrived at ${origInfo.stopName}. Boarding Line ${origInfo.lineNum}...` });
      } else {
        rawSteps.push({ pt: origInfo.coords, mode: 'transfer', label: `🏢 Starting directly at ${origInfo.name}...` });
      }

      if (origInfo.lineStops && origInfo.lineStops.length > 0) {
        origInfo.lineStops.slice(1).forEach(s => {
          rawSteps.push({ pt: [s[0], s[1]], mode: 'muni', label: `🚌 Step 2: Riding Bus #${origInfo.lineNum} past ${s[2]}...` });
        });
      }

      rawSteps.push({ pt: [42.25671, 42.67195], mode: 'transfer', label: `🔄 Step 3: Transfer buffer (${origInfo.transferMins} min) at Omnibus Terminal Platform 2...` });

      HIGHWAY_KUTAISI_TBILISI.slice(1).forEach(pt => {
        rawSteps.push({ pt: pt, mode: 'coach', label: '🚍 Step 4: Express transit along Highway E60 to Tbilisi...' });
      });

      // Subdivide coarse steps into smooth continuous sub-steps (~160 points total)
      const simPoints = [];
      for (let i = 0; i < rawSteps.length; i++) {
        if (i === 0) {
          simPoints.push(rawSteps[0]);
          continue;
        }
        let prev = rawSteps[i - 1];
        let curr = rawSteps[i];
        let p1 = prev.pt;
        let p2 = curr.pt;
        let dLat = p2[0] - p1[0];
        let dLng = p2[1] - p1[1];
        let dist = Math.hypot(dLat, dLng);
        let maxStep = curr.mode === 'walk' ? 0.0006 : (curr.mode === 'muni' ? 0.002 : 0.012);
        let numSub = Math.max(1, Math.ceil(dist / maxStep));
        for (let s = 1; s <= numSub; s++) {
          let frac = s / numSub;
          simPoints.push({
            pt: [p1[0] + frac * dLat, p1[1] + frac * dLng],
            mode: curr.mode,
            label: curr.label
          });
        }
      }

      let sIdx = 0;
      const travelerIcon = L.divIcon({
        className: 'traveler-icon',
        html: `<div id="travelerBadge" style="background:#ea580c; color:white; font-weight:800; font-size:11px; padding:3px 8px; border-radius:999px; border:2px solid white; box-shadow:0 3px 10px rgba(0,0,0,0.5); white-space:nowrap; transition: background 0.2s;">🚶 Traveler</div>`,
        iconSize: [110, 26],
        iconAnchor: [55, 13]
      });
      const travelerMarker = L.marker(simPoints[0].pt, { icon: travelerIcon, zIndexOffset: 10000 }).addTo(window.journeySimLayer);

      window.journeySimTimer = setInterval(() => {
        sIdx++;
        if (sIdx >= simPoints.length) {
          sIdx = simPoints.length - 1;
          clearInterval(window.journeySimTimer);
          const hudText = document.getElementById('hudStageText');
          if (hudText) hudText.innerHTML = '🎉 <span style="color:#4ade80;">Arrived at Tbilisi Didube Bus Terminal! Journey Complete.</span>';
          const badge = document.getElementById('travelerBadge');
          if (badge) { badge.style.background = '#22c55e'; badge.innerHTML = '🏁 Arrived!'; }
          return;
        }

        const step = simPoints[sIdx];
        travelerMarker.setLatLng(step.pt);
        const hudText = document.getElementById('hudStageText');
        if (hudText && hudText.innerText !== step.label) hudText.innerText = step.label;

        const badge = document.getElementById('travelerBadge');
        if (badge) {
          if (step.mode === 'walk' && badge.innerText !== '🚶 Walking') {
            badge.style.background = '#ea580c'; badge.innerHTML = '🚶 Walking';
          } else if (step.mode === 'muni' && !badge.innerText.includes('Bus')) {
            badge.style.background = '#0284c7'; badge.innerHTML = `🚌 Bus #${origInfo.lineNum}`;
          } else if (step.mode === 'transfer' && !badge.innerText.includes('Transfer')) {
            badge.style.background = '#eab308'; badge.style.color = '#1e293b'; badge.innerHTML = '🔄 Transfer Hub';
          } else if (step.mode === 'coach' && !badge.innerText.includes('Express') && !badge.innerText.includes('Marshrutka')) {
            badge.style.background = idx === 1 ? '#ea580c' : '#8e24aa'; badge.style.color = 'white';
            badge.innerHTML = idx === 1 ? '🚐 Okriba Marshrutka' : '🚍 Omnibus Express';
          }
        }
      }, 60);
    }

    function stopJourneySim() {
      if (window.journeySimLayer) {
        map.removeLayer(window.journeySimLayer);
        window.journeySimLayer = null;
      }
      if (window.journeySimTimer) {
        clearInterval(window.journeySimTimer);
        window.journeySimTimer = null;
      }
      const hud = document.getElementById('journeyHud');
      if (hud) hud.remove();
    }

    // Render Tab 4 Kutaisi Directory Table
    function renderKutaisiDirectoryTable() {
      const tbody = document.getElementById('kutaisiDirectoryBody');
      const query = (document.getElementById('kutaisiDirSearch').value || '').trim().toLowerCase();
      tbody.innerHTML = '';

      const uniqueRoutes = [
        '1L', '1R', '3', '4', '5', '6', '7', '10', '19', '20', '25', '31', '41',
        '110', '111', '112', '114', '118', '126', '133', '137', '141'
      ];

      uniqueRoutes.forEach(r => {
        let p0 = KUTAISI_ALL_PATTERNS[r + '_0'] || { name: 'Kutaisi Bus', dir_text: 'Central', stops: [] };
        let p1 = KUTAISI_ALL_PATTERNS[r + '_1'] || { name: 'Kutaisi Bus', dir_text: 'Return', stops: [] };

        let match = !query || r.toLowerCase().includes(query) || p0.name.toLowerCase().includes(query) || p0.dir_text.toLowerCase().includes(query) || p1.dir_text.toLowerCase().includes(query);
        if (!match) return;

        let tr = document.createElement('tr');
        tr.innerHTML = `
          <td><span class="badge" style="background:#0284c7; color:white; font-size:0.85rem;">Line ${r}</span></td>
          <td><b>${p0.name}</b></td>
          <td style="font-size:0.82rem; color:#334155;">${p0.dir_text || 'Forward'}</td>
          <td style="font-size:0.82rem; color:#334155;">${p1.dir_text || 'Return'}</td>
          <td><b>${(p0.stops ? p0.stops.length : 0) + (p1.stops ? p1.stops.length : 0)} stops</b></td>
          <td><span style="color:#16a34a; font-weight:600;">Every 15-20 min</span> (07:00 - 21:30)</td>
          <td><button class="btn btn-primary" style="padding:4px 10px; font-size:0.75rem;" onclick="viewKutaisiLineOnMap('${r}')">Map View</button></td>
        `;
        tbody.appendChild(tr);
      });
    }

    function viewKutaisiLineOnMap(rNo) {
      switchTab('tab-simulation');
      document.querySelector('nav.tabs button:nth-child(3)').classList.add('active');
      document.querySelector('nav.tabs button:nth-child(4)').classList.remove('active');

      document.getElementById('kutaisiLineFilterSelect').value = rNo;
      handleKutaisiLineFilter();
    }

    // Render Tab 5 Marshrutka Directory Table
    function renderMarshrutkaDirectoryTable() {
      const tbody = document.getElementById('marshrutkaDirectoryBody');
      const query = (document.getElementById('marshDirSearch').value || '').trim().toLowerCase();
      tbody.innerHTML = '';

      const marshList = [
        { id: 'MR-OK-01', route: 'Kutaisi (Chavchavadze) ↔ Tbilisi (Didube / Okriba)', carrier: 'LTD Okriba', fleet: 'Mercedes Sprinter 519 CDI', freq: 'Every 30–40 min (08:00–18:00)', fare: 20.00, bay: 'Bay 1 (Okriba)' },
        { id: 'MR-IM-01', route: 'Kutaisi (Chavchavadze) ↔ Batumi (Mayakovsky)', carrier: 'LTD Imertrans', fleet: 'Mercedes Sprinter 316 CDI', freq: 'Every 30 min (08:30–19:00)', fare: 15.00, bay: 'Bay 2 (Batumi)' },
        { id: 'MR-POT-01', route: 'Kutaisi (Chavchavadze) ↔ Poti Central', carrier: 'LTD Sprinter Express', fleet: 'Ford Transit / Sprinter', freq: 'Every 45 min (08:00–18:30)', fare: 10.00, bay: 'Bay 4 (Poti)' },
        { id: 'MR-ZUG-01', route: 'Kutaisi (Chavchavadze) ↔ Zugdidi Central', carrier: 'Regional Minibus Hub', fleet: 'Mercedes Sprinter', freq: 'Every 40 min (08:15–18:00)', fare: 12.00, bay: 'Bay 5 (Zugdidi)' },
        { id: 'MR-TSK-01', route: 'Kutaisi (Chavchavadze) ↔ Tskaltubo Spa Resort (#30)', carrier: 'Tskaltubo Local Carrier', fleet: 'Mercedes Sprinter', freq: 'Every 15 min (07:30–20:30)', fare: 2.00, bay: 'Local Platform A' },
        { id: 'MR-SAM-01', route: 'Kutaisi (Chavchavadze) ↔ Samtredia Junction', carrier: 'Imereti Regional Trans', fleet: 'Mercedes Sprinter', freq: 'Every 20 min (07:00–20:00)', fare: 3.00, bay: 'Local Platform B' },
        { id: 'MR-TKI-01', route: 'Kutaisi (Chavchavadze) ↔ Tkibuli Central', carrier: 'Tkibuli Minibus Trans', fleet: 'Mercedes Sprinter', freq: 'Every 45 min (08:00–18:00)', fare: 5.00, bay: 'Bay 7 (Tkibuli)' },
        { id: 'MR-CHI-01', route: 'Kutaisi (Chavchavadze) ↔ Chiatura / Sachkhere', carrier: 'Chiatura Cable City Trans', fleet: 'Mercedes Sprinter', freq: 'Every 40 min (08:30–17:30)', fare: 8.00, bay: 'Bay 8 (Chiatura)' },
        { id: 'MR-BAG-01', route: 'Kutaisi (Chavchavadze) ↔ Baghdati / Sairme Springs', carrier: 'Sairme Spa Express', fleet: 'Mercedes Sprinter', freq: 'Hourly (09:00–17:00)', fare: 6.00, bay: 'Bay 9 (Sairme)' },
        { id: 'MR-VAN-01', route: 'Kutaisi (Chavchavadze) ↔ Vani Reserve', carrier: 'Vani Municipal Hub', fleet: 'Mercedes Sprinter', freq: 'Hourly (08:30–17:30)', fare: 5.00, bay: 'Bay 10 (Vani)' },
        { id: 'MR-KHO-01', route: 'Kutaisi (Chavchavadze) ↔ Khoni / Martvili Canyon', carrier: 'Martvili Canyon Trans', fleet: 'Mercedes Sprinter', freq: 'Hourly (09:00–18:00)', fare: 6.50, bay: 'Bay 11 (Martvili)' },
        { id: 'MR-AMB-01', route: 'Kutaisi (Chavchavadze) ↔ Ambrolauri (High Racha)', carrier: 'Racha Mountain Minibus', fleet: 'Mercedes Sprinter 4x4', freq: '3 Daily (09:30, 13:00, 16:00)', fare: 15.00, bay: 'Mountain Bay M1' },
        { id: 'MR-MES-01', route: 'Kutaisi (Chavchavadze) ↔ Mestia (Svaneti)', carrier: 'Mestia Mountain Minibus', fleet: 'Mercedes Sprinter 4x4', freq: '2 Daily (07:30, 12:00)', fare: 35.00, bay: 'Mountain Bay M2' },
        { id: 'MR-KAZ-01', route: 'Tbilisi (Didube) ↔ Stepantsminda (Kazbegi)', carrier: 'Kazbegi Mountain Minibus', fleet: 'Mercedes Sprinter', freq: 'Hourly (08:00–19:00)', fare: 17.00, bay: 'Didube North Bay' },
        { id: 'MR-GUD-01', route: 'Tbilisi (Didube) ↔ Gudauri Ski Resort', carrier: 'Gudauri Ski Shuttle', fleet: 'Mercedes Sprinter', freq: 'Every 45 min (08:30–18:00)', fare: 15.00, bay: 'Didube Bay 12' },
        { id: 'MR-BOR-01', route: 'Tbilisi (Didube) ↔ Borjomi / Bakuriani', carrier: 'Borjomi Mineral Trans', fleet: 'Mercedes Sprinter', freq: 'Every 30 min (08:00–19:00)', fare: 15.00, bay: 'Didube Bay 6' },
        { id: 'MR-TEL-01', route: 'Tbilisi (Ortachala / Navtlughi) ↔ Telavi', carrier: 'Kakheti Wine Trans', fleet: 'Mercedes Sprinter', freq: 'Every 30 min (08:00–18:30)', fare: 14.00, bay: 'Navtlughi Terminal' },
        { id: 'MR-SIG-01', route: 'Tbilisi (Samgori) ↔ Sighnaghi (City of Love)', carrier: 'Sighnaghi Tourist Minibus', fleet: 'Mercedes Sprinter', freq: 'Every 40 min (09:00–18:00)', fare: 12.00, bay: 'Samgori Bay 3' }
      ];

      marshList.forEach(m => {
        let match = !query || m.id.toLowerCase().includes(query) || m.route.toLowerCase().includes(query) || m.carrier.toLowerCase().includes(query);
        if (!match) return;

        let tr = document.createElement('tr');
        tr.innerHTML = `
          <td><span class="badge" style="background:#ea580c; color:white; font-size:0.8rem;">${m.id}</span></td>
          <td><b>${m.route}</b></td>
          <td><span style="color:#0f6cbd; font-weight:600;">${m.carrier}</span></td>
          <td style="font-size:0.82rem; color:#475569;">${m.fleet}</td>
          <td><span style="color:#16a34a; font-weight:600;">${m.freq}</span></td>
          <td><b>${m.fare.toFixed(2)} ₾</b></td>
          <td style="font-size:0.82rem;">${m.bay}</td>
          <td><button class="btn btn-primary" style="padding:4px 10px; font-size:0.75rem; background:#ea580c;" onclick="viewMarshrutkaOnMap('${m.id}')">Map View</button></td>
        `;
        tbody.appendChild(tr);
      });
    }

    function viewMarshrutkaOnMap(mId) {
      switchTab('tab-simulation');
      document.querySelector('nav.tabs button:nth-child(3)').classList.add('active');
      document.querySelector('nav.tabs button:nth-child(5)').classList.remove('active');

      document.getElementById('marshrutkaFilterSelect').value = mId;
      handleMarshrutkaFilter();
    }

    // Departure Board
    function updateDepartureBoard() {
      const tbody = document.getElementById('departureBoardBody');
      const st = document.getElementById('stationSelect').value;

      if (st.includes('chavchavadze')) {
        tbody.innerHTML = `
          <tr><td><b>Route Taxi #MR-OK-01</b></td><td>Tbilisi (Didube / Okriba)</td><td><span class="op-badge" style="background:#ea580c; color:white;">LTD Okriba (Route Taxi)</span></td><td>08:00</td><td style="color:#16a34a; font-weight:700;">Boarding Platform 1</td><td>Departing in 4 min</td></tr>
          <tr><td><b>Route Taxi #MR-IM-01</b></td><td>Batumi (Mayakovsky Hub)</td><td><span class="op-badge" style="background:#ea580c; color:white;">LTD Imertrans (Route Taxi)</span></td><td>08:30</td><td style="color:#16a34a; font-weight:700;">In 12 min</td><td>Gate 2</td></tr>
          <tr><td><b>Citybus GEO-103</b></td><td>Batumi Central</td><td><span class="op-badge" style="background:#8e24aa; color:white;">Citybus Georgia</span></td><td>08:30</td><td style="color:#16a34a; font-weight:700;">In 12 min</td><td>Platform 2</td></tr>
          <tr><td><b>Kutaisi Bus #10</b></td><td>სულხან-საბას გამზირი N71</td><td><span class="op-badge" style="background:#0284c7; color:white;">Kutaisi Municipal</span></td><td>08:35</td><td style="color:#16a34a; font-weight:700;">In 17 min</td><td>Approaching Platform A</td></tr>
          <tr><td><b>Route Taxi #MR-TSK-01</b></td><td>Tskaltubo Spa Resort</td><td><span class="op-badge" style="background:#ea580c; color:white;">Route Taxi #30</span></td><td>08:40</td><td style="color:#16a34a; font-weight:700;">In 22 min</td><td>Local Platform A</td></tr>
          <tr><td><b>Omnibus #203</b></td><td>Batumi (Radisson)</td><td><span class="op-badge" style="background:#1e3a8a; color:#facc15;">Omnibus Express</span></td><td>10:00</td><td>In 1h 42m</td><td>Platform 3</td></tr>
        `;
      } else if (st.includes('redbridge')) {
        tbody.innerHTML = `
          <tr><td><b>Kutaisi Bus #10</b></td><td>სულხან-საბას გამზირი</td><td><span class="op-badge" style="background:#0284c7; color:white;">Kutaisi Municipal</span></td><td>08:30</td><td style="color:#16a34a; font-weight:700;">In 5 min</td><td>Red Bridge Pier</td></tr>
          <tr><td><b>Kutaisi Bus #1L</b></td><td>ცენტრალური ბეჭედი</td><td><span class="op-badge" style="background:#0284c7; color:white;">Kutaisi Municipal</span></td><td>08:38</td><td style="color:#16a34a; font-weight:700;">In 13 min</td><td>On Time</td></tr>
          <tr><td><b>Kutaisi Bus #111</b></td><td>ბაგრატის ტაძარი</td><td><span class="op-badge" style="background:#0284c7; color:white;">Kutaisi Municipal</span></td><td>08:45</td><td>In 20 min</td><td>Scheduled</td></tr>
        `;
      } else if (st.includes('didube')) {
        tbody.innerHTML = `
          <tr><td><b>Route Taxi #MR-OK-02</b></td><td>Kutaisi (Chavchavadze)</td><td><span class="op-badge" style="background:#ea580c; color:white;">LTD Okriba (Route Taxi)</span></td><td>09:20</td><td style="color:#16a34a; font-weight:700;">In 8 min</td><td>Okriba Bay 1</td></tr>
          <tr><td><b>Route Taxi #MR-KAZ-01</b></td><td>Stepantsminda (Kazbegi)</td><td><span class="op-badge" style="background:#ea580c; color:white;">Kazbegi Route Taxi</span></td><td>08:45</td><td style="color:#16a34a; font-weight:700;">In 25 min</td><td>Didube North Bay</td></tr>
          <tr><td><b>Citybus GEO-102</b></td><td>Kutaisi (Chavchavadze)</td><td><span class="op-badge" style="background:#8e24aa; color:white;">Citybus Georgia</span></td><td>08:00</td><td style="color:#16a34a; font-weight:700;">Boarding Now</td><td>Platform 1</td></tr>
          <tr><td><b>Omnibus #202</b></td><td>Kutaisi Airport (KUT)</td><td><span class="op-badge" style="background:#1e3a8a; color:#facc15;">Omnibus Express</span></td><td>09:30</td><td>In 1h 10m</td><td>Platform 2</td></tr>
        `;
      } else {
        tbody.innerHTML = `
          <tr><td><b>Route Taxi #MR-IM-02</b></td><td>Kutaisi (Chavchavadze)</td><td><span class="op-badge" style="background:#ea580c; color:white;">LTD Imertrans (Route Taxi)</span></td><td>10:00</td><td style="color:#16a34a; font-weight:700;">In 15 min</td><td>Mayakovsky Bay</td></tr>
          <tr><td><b>Citybus GEO-101</b></td><td>Tbilisi (Ortachala)</td><td><span class="op-badge" style="background:#8e24aa; color:white;">Citybus Georgia</span></td><td>08:30</td><td style="color:#16a34a; font-weight:700;">In 10 min</td><td>Bay 1</td></tr>
          <tr><td><b>Metro GEO-402</b></td><td>Tbilisi (Ortachala)</td><td><span class="op-badge" style="background:#dc2626; color:white;">Metro Georgia</span></td><td>09:00</td><td style="color:#16a34a; font-weight:700;">In 40 min</td><td>Bay 3</td></tr>
        `;
      }
    }

    // Initialization
    window.onload = () => {
      renderRoutesTable(routes);
      calculateJourney();
      initMap();
    };
  </script>
</body>
</html>
'''

html_template = html_template.replace('__DESKTOP_MAP__', json.dumps(desktop_map, ensure_ascii=False))
html_template = html_template.replace('__STOP_COUNT__', str(len(stops)))

with open('web-preview/index.html', 'w', encoding='utf-8') as f:
    f.write(html_template)

print(f'Generated web-preview/index.html successfully with ALL 25 Marshrutka (Route Taxi) lines!')
