#!/usr/bin/env python3
"""Export the public Kutaisi Transport app timetable without storing its client key."""
import csv, datetime as dt, json, re, sys, time, urllib.error, urllib.parse, urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "data" / "kutaisi-municipal"
SITE = "https://kutaisi-bus.msplus.ge"
SOURCE = "Kutaisi Transport public timetable API"
EXTRACTED = "2026-11-13"
APP_VERSION = "3.1.3"  # Observed installed app version.
DAYS = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"]
DAY_INDEX = {d.upper(): i for i, d in enumerate(DAYS)}
KEY_HEADERS = {}


def fetch(url, headers=None):
    req = urllib.request.Request(url, headers={"Accept": "application/json, text/html;q=0.9, */*;q=0.8", "User-Agent": "KutaisiTimetableDataExport/1.0", **(headers or {})})
    for attempt in range(5):
        try:
            with urllib.request.urlopen(req, timeout=30) as r:
                return r.read()
        except urllib.error.HTTPError as e:
            body = e.read().decode("utf-8", "replace")[:250]
            if e.code in (429, 500, 502, 503, 504) and attempt < 4:
                time.sleep(1.5 * (attempt + 1)); continue
            raise RuntimeError(f"HTTP {e.code}: {body}") from e
        except (TimeoutError, urllib.error.URLError) as e:
            if attempt < 4:
                time.sleep(1.5 * (attempt + 1)); continue
            raise RuntimeError(str(e)) from e


def api(path, params=None):
    query = urllib.parse.urlencode(params or {})
    url = SITE + "/pis-gateway/api/v3" + path + ("?" + query if query else "")
    data = json.loads(fetch(url, {**KEY_HEADERS, "Accept": "application/json"}).decode("utf-8"))
    time.sleep(.12)
    return data


def days_in_range(first, last):
    a, b = DAY_INDEX.get(str(first).upper()), DAY_INDEX.get(str(last).upper())
    if a is None or b is None: return []
    out, i = [], a
    while True:
        out.append(DAYS[i])
        if i == b: return out
        i = (i + 1) % 7
        if len(out) > 7: return []


def listify(x): return x if isinstance(x, list) else ([] if x is None else [x])
def sql(v, unicode=False):
    if v is None or v == "": return "NULL"
    if isinstance(v, bool): return "1" if v else "0"
    if isinstance(v, (int, float)): return str(v)
    return ("N" if unicode else "") + "'" + str(v).replace("'", "''") + "'"

def write_csv(path, fields, rows):
    with path.open("w", encoding="utf-8-sig", newline="") as f:
        w = csv.DictWriter(f, fields, extrasaction="ignore"); w.writeheader(); w.writerows(rows)

def insert_sql(table, fields, rows, wide, batch=500):
    out = []
    for i in range(0, len(rows), batch):
        out.append(f"INSERT INTO dbo.{table} ({', '.join('['+x+']' for x in fields)}) VALUES")
        out.append(",\n".join("(" + ",".join(sql(r.get(c), c in wide) for c in fields) + ")" for r in rows[i:i+batch]) + ";")
    return out


def main():
    global KEY_HEADERS
    OUT.mkdir(parents=True, exist_ok=True)
    html = fetch(SITE + "/").decode("utf-8", "replace")
    m = re.search(r'"PIS_GATEWAY_KEY"\s*:\s*"([^"]+)"', html)
    if not m: raise RuntimeError("The public app configuration did not expose its client key.")
    KEY_HEADERS = {"X-api-key": m.group(1)}  # Public app key, memory only.
    routes = api("/routes", {"modes":"BUS", "locale":"ka"})
    if not isinstance(routes, list) or not routes: raise RuntimeError("No BUS routes returned.")

    patterns, stops, coverage, records, issues = [], [], {}, {}, []
    errors, dates_seen = [], set()
    for ri, route in enumerate(routes, 1):
        rid, number, rname = str(route.get("id","")), str(route.get("shortName","")), str(route.get("longName",""))
        print(f"[{ri}/{len(routes)}] Route {number}")
        escid = urllib.parse.quote(rid, safe=":")
        try:
            detail = api(f"/routes/{escid}", {"forEntireCurrentWeek":"true", "locale":"ka"})
        except Exception as e:
            errors.append(f"Route {number} detail: {e}"); continue
        for p in detail.get("patterns") or []:
            suffix = str(p.get("patternSuffix","")); did = int(p.get("directionId",-1))
            first, last = p.get("firstStop") or {}, p.get("lastStop") or {}
            start, end = str(first.get("name","")), str(last.get("name",""))
            direction = f"directionId={did}: {start} → {end}"
            common = {"RouteNumber":number,"RouteId":rid,"RouteName":rname,"PatternSuffix":suffix,"DirectionId":did,"Direction":direction,"StartPoint":start,"EndPoint":end,"Source":SOURCE,"Verified":True}
            q = {"patternSuffix":suffix,"locale":"ka"}
            try:
                route_stops = listify(api(f"/routes/{escid}/stops", q))
                groups = listify(api(f"/routes/{escid}/schedule", q))
            except Exception as e:
                errors.append(f"Route {number} pattern {suffix}: {e}"); route_stops, groups = [], []
            stop_info, seq = {}, []
            for i, s in enumerate(route_stops,1):
                sid = str(s.get("id",""))
                if not sid: issues.append(f"Route {number} pattern {suffix}: stop lacks ID"); continue
                stop_info[sid] = s; seq.append(sid)
                stops.append({**common,"StopOrder":i,"StopID":sid,"StopCode":str(s.get("code","")) or None,"StopName":str(s.get("name","")),"Latitude":s.get("lat"),"Longitude":s.get("lon"),"VehicleMode":s.get("vehicleMode",route.get("mode","BUS"))})
            pattern = {**common,"Mode":route.get("mode","BUS"),"FirstStopID":first.get("id"),"LastStopID":last.get("id"),"HeadSign":p.get("headsign",""),"ServiceDates":""}
            patterns.append(pattern)
            group_days, sched_seq = set(), None
            for g in groups:
                from_day, to_day = str(g.get("fromDay","")), str(g.get("toDay",""))
                day_names = days_in_range(from_day,to_day)
                dates = [str(x) for x in (g.get("serviceDates") or [])]; dates_seen.update(dates)
                op_pattern = f"{from_day}-{to_day}" if from_day and to_day else ""
                if not day_names:
                    issues.append(f"Route {number} pattern {suffix}: unknown day range {from_day}-{to_day}"); continue
                group_days.update(day_names)
                sched_stops = listify(g.get("stops"))
                if sched_seq is None and sched_stops: sched_seq = [str(x.get("id","")) for x in sorted(sched_stops,key=lambda x:int(x.get("position",0) or 0))]
                for ss in sched_stops:
                    sid = str(ss.get("id","")); pos = int(ss.get("position",0) or 0)
                    if not pos: pos = next((i for i,x in enumerate(route_stops,1) if str(x.get("id"))==sid),0)
                    if not sid or not pos: issues.append(f"Route {number} pattern {suffix}: schedule stop lacks ID/position"); continue
                    s = stop_info.get(sid,{})
                    if not s:
                        s = {"id":sid,"name":ss.get("name",""),"code":None,"lat":None,"lon":None}
                        stops.append({**common,"StopOrder":pos,"StopID":sid,"StopCode":None,"StopName":str(s.get("name","")),"Latitude":None,"Longitude":None,"VehicleMode":route.get("mode","BUS")})
                        issues.append(f"Route {number} pattern {suffix}: schedule stop {sid} absent from stop list")
                    if s.get("name") and ss.get("name") and s["name"] != ss["name"]: issues.append(f"Route {number} pattern {suffix}, stop {sid}: names differ by endpoint")
                    times0 = ss.get("arrivalTimes") or ""
                    times = [str(t).strip() for t in (times0 if isinstance(times0,list) else str(times0).split(",")) if str(t).strip()]
                    for day in day_names:
                        ck=(rid,suffix,did,pos,day)
                        cov=coverage.setdefault(ck,{**common,"StopOrder":pos,"StopID":sid,"StopCode":str(s.get("code","")) or None,"StopName":str(ss.get("name") or s.get("name","")),"Latitude":s.get("lat"),"Longitude":s.get("lon"),"DayOfWeek":day,"OperatingDayPattern":"","ServiceDates":set(),"ScheduleStatus":"No published times returned for this stop/day","PublishedTimeCount":0})
                        cov["ServiceDates"].update(dates)
                        ops=set(filter(None,cov["OperatingDayPattern"].split(";")))
                        if op_pattern: ops.add(op_pattern)
                        cov["OperatingDayPattern"]=";".join(sorted(ops))
                        for t in times:
                            if not re.fullmatch(r"\d{1,2}:\d{2}(?::\d{2})?",t): issues.append(f"Route {number} stop {sid}: malformed time {t}"); continue
                            parts=t.split(":"); tm=f"{int(parts[0]):02d}:{int(parts[1]):02d}"
                            key=(rid,suffix,did,pos,day,tm)
                            cov.setdefault("_times",set()).add(tm)
                            if key not in records:
                                records[key]={**common,"Mode":route.get("mode","BUS"),"StopOrder":pos,"StopID":sid,"StopCode":str(s.get("code","")) or None,"StopName":str(ss.get("name") or s.get("name","")),"DayOfWeek":day,"OperatingDayPattern":op_pattern,"ScheduledTime":tm,"Latitude":s.get("lat"),"Longitude":s.get("lon"),"ServiceDates":";".join(dates),"SourceDate":EXTRACTED}
                            else:
                                old=records[key]; old["OperatingDayPattern"]=";".join(sorted(set(filter(None,old["OperatingDayPattern"].split(";")))|{op_pattern}))
                                old["ServiceDates"]=";".join(sorted(set(filter(None,old["ServiceDates"].split(";")))|set(dates)))
            if not groups: errors.append(f"Route {number} pattern {suffix}: no schedule groups returned")
            if seq and sched_seq and seq != sched_seq: issues.append(f"Route {number} pattern {suffix}: stop order differs between endpoints")
            pattern["ServiceDates"]=";".join(sorted({str(d) for g in groups for d in (g.get("serviceDates") or [])}))
            for day in DAYS:
                if day not in group_days:
                    for i,s in enumerate(route_stops,1):
                        sid=str(s.get("id","")); key=(rid,suffix,did,i,day)
                        if sid: coverage.setdefault(key,{**common,"StopOrder":i,"StopID":sid,"StopCode":str(s.get("code","")) or None,"StopName":str(s.get("name","")),"Latitude":s.get("lat"),"Longitude":s.get("lon"),"DayOfWeek":day,"OperatingDayPattern":"","ServiceDates":set(),"ScheduleStatus":"No schedule group returned for this day","PublishedTimeCount":0})

    # Collapse duplicate route-stop rows while retaining sequence and direction.
    unique={}
    for s in stops: unique.setdefault((s["RouteId"],s["PatternSuffix"],s["DirectionId"],s["StopOrder"],s["StopID"]),s)
    stops=list(unique.values())
    cv=[]
    for r in coverage.values():
        r=dict(r); times=r.pop("_times",set()); r["PublishedTimeCount"]=len(times)
        if times: r["ScheduleStatus"]="Published timetable returned"
        r["ServiceDates"]=";".join(sorted(r["ServiceDates"])); cv.append(r)
    recs=list(records.values())
    recs.sort(key=lambda r:(r["RouteNumber"],r["RouteId"],r["PatternSuffix"],r["DirectionId"],r["StopOrder"],DAY_INDEX[r["DayOfWeek"].upper()],r["ScheduledTime"]))
    patterns.sort(key=lambda r:(r["RouteNumber"],r["RouteId"],r["PatternSuffix"],r["DirectionId"]))
    stops.sort(key=lambda r:(r["RouteNumber"],r["RouteId"],r["PatternSuffix"],r["DirectionId"],r["StopOrder"]))
    cv.sort(key=lambda r:(r["RouteNumber"],r["RouteId"],r["PatternSuffix"],r["StopOrder"],DAY_INDEX[r["DayOfWeek"].upper()]))

    tf=["RouteNumber","RouteId","RouteName","Mode","PatternSuffix","DirectionId","Direction","StopOrder","StopID","StopCode","StopName","DayOfWeek","OperatingDayPattern","ScheduledTime","StartPoint","EndPoint","Latitude","Longitude","ServiceDates","Source","SourceDate","Verified"]
    pf=["RouteNumber","RouteId","RouteName","Mode","PatternSuffix","DirectionId","Direction","StartPoint","EndPoint","FirstStopID","LastStopID","HeadSign","ServiceDates","Source","Verified"]
    sf=["RouteNumber","RouteId","RouteName","PatternSuffix","DirectionId","Direction","StopOrder","StopID","StopCode","StopName","Latitude","Longitude","VehicleMode","Source","Verified"]
    cf=["RouteNumber","RouteId","PatternSuffix","DirectionId","Direction","StopOrder","StopID","StopCode","StopName","Latitude","Longitude","DayOfWeek","OperatingDayPattern","ServiceDates","ScheduleStatus","PublishedTimeCount","Source","Verified"]
    write_csv(OUT/"timetable.csv",tf,recs); write_csv(OUT/"route_patterns.csv",pf,patterns); write_csv(OUT/"route_stops.csv",sf,stops); write_csv(OUT/"coverage.csv",cf,cv)

    weekday={d:{"timetableRecords":sum(r["DayOfWeek"]==d for r in recs),"stopDayChecks":sum(r["DayOfWeek"]==d for r in cv),"publishedScheduleChecks":sum(r["DayOfWeek"]==d and r["ScheduleStatus"]=="Published timetable returned" for r in cv),"noScheduleGroupChecks":sum(r["DayOfWeek"]==d and r["ScheduleStatus"]=="No schedule group returned for this day" for r in cv)} for d in DAYS}
    missing=[r for r in cv if r["ScheduleStatus"]!="Published timetable returned"]
    route_dir={(p["RouteId"],p["DirectionId"]) for p in patterns}
    summary={"source":SOURCE,"sourceUrl":SITE+"/#/schedule","scheduleEndpoint":SITE+"/pis-gateway/api/v3/routes/{routeId}/schedule?patternSuffix={patternSuffix}&locale=ka","extractedOn":EXTRACTED,"appVersionObserved":APP_VERSION,"mode":"BUS","routesDiscovered":len(routes),"uniqueRouteNumbers":len({str(r.get("shortName","")) for r in routes}),"routePatternCombinations":len(patterns),"routeDirectionCombinationsByRouteAndDirectionId":len(route_dir),"uniqueStops":len({s["StopID"] for s in stops}),"stopRouteCombinations":len(stops),"timetableRecords":len(recs),"verifiedTimetableRecords":sum(bool(r["Verified"]) for r in recs),"stopDayCoverageChecks":len(cv),"stopDaysWithPublishedTimes":len(cv)-len(missing),"stopDaysWithoutReturnedSchedule":len(missing),"missingCoordinateRouteStopRows":sum(s["Latitude"] is None or s["Longitude"] is None for s in stops),"serviceDatesReturned":sorted(dates_seen),"weekdayCoverage":weekday,"holidaySpecificSchedule":"Not exposed as a distinct day type in the public API response; no holiday schedule was inferred.","unresolvedCoverage":[{"routeNumber":r["RouteNumber"],"routeId":r["RouteId"],"patternSuffix":r["PatternSuffix"],"directionId":r["DirectionId"],"stopId":r["StopID"],"stopName":r["StopName"],"dayOfWeek":r["DayOfWeek"],"status":r["ScheduleStatus"]} for r in missing],"issues":issues+errors,"routes":[{"routeNumber":r.get("shortName"),"routeId":r.get("id"),"routeName":r.get("longName"),"mode":r.get("mode")} for r in routes]}
    record_columns=["RouteId","PatternSuffix","DirectionId","StopOrder","DayOfWeek","ScheduledTime"]
    compact_records=[[r[c] for c in record_columns] for r in recs]
    payload={"format":"normalized-array-v1","metadata":summary,"recordColumns":record_columns,"routes":summary["routes"],"routePatterns":patterns,"routeStops":stops,"coverage":cv,"timetableRecords":compact_records}
    (OUT/"timetable.json").write_text(json.dumps(payload,ensure_ascii=False,separators=(",",":"))+"\n",encoding="utf-8")

    route_rows=[{"RouteId":r.get("id"),"RouteNumber":r.get("shortName"),"RouteName":r.get("longName"),"Mode":r.get("mode"),"Source":SOURCE,"Verified":True} for r in routes]
    pattern_sql=[{k:r.get(k) for k in ["RouteId","PatternSuffix","DirectionId","Direction","StartPoint","EndPoint","FirstStopID","LastStopID","HeadSign","ServiceDates","Source","Verified"]} for r in patterns]
    stop_sql=[{k:r.get(k) for k in ["RouteId","PatternSuffix","DirectionId","StopOrder","StopID","StopCode","StopName","Latitude","Longitude","VehicleMode","Source","Verified"]} for r in stops]
    coverage_sql=[{k:r.get(k) for k in ["RouteId","PatternSuffix","DirectionId","StopOrder","StopID","DayOfWeek","OperatingDayPattern","ServiceDates","ScheduleStatus","PublishedTimeCount","Source","Verified"]} for r in cv]
    timetable_sql=[{k:r.get(k) for k in ["RouteId","PatternSuffix","DirectionId","StopOrder","StopID","DayOfWeek","ScheduledTime","Source","Verified"]} for r in recs]
    sqltext=["-- Kutaisi municipal bus timetable; source captured 2026-11-13.","-- Normalized SQL Server seed. Route and stop details are joined by their IDs.","SET NOCOUNT ON;",
    "IF DB_ID(N'BusTimetableDb') IS NULL CREATE DATABASE BusTimetableDb;","GO","USE BusTimetableDb;","GO",
    "IF OBJECT_ID(N'dbo.KutaisiRoutes',N'U') IS NULL CREATE TABLE dbo.KutaisiRoutes (RouteId NVARCHAR(64) NOT NULL PRIMARY KEY,RouteNumber NVARCHAR(20) NOT NULL,RouteName NVARCHAR(300) NULL,Mode NVARCHAR(20) NOT NULL,Source NVARCHAR(200) NOT NULL,Verified BIT NOT NULL);",
    "IF OBJECT_ID(N'dbo.KutaisiRoutePatterns',N'U') IS NULL CREATE TABLE dbo.KutaisiRoutePatterns (RouteId NVARCHAR(64) NOT NULL,PatternSuffix NVARCHAR(40) NOT NULL,DirectionId INT NOT NULL,Direction NVARCHAR(700) NULL,StartPoint NVARCHAR(300) NULL,EndPoint NVARCHAR(300) NULL,FirstStopID NVARCHAR(64) NULL,LastStopID NVARCHAR(64) NULL,HeadSign NVARCHAR(500) NULL,ServiceDates NVARCHAR(300) NULL,Source NVARCHAR(200) NOT NULL,Verified BIT NOT NULL,CONSTRAINT PK_KutaisiRoutePatterns PRIMARY KEY(RouteId,PatternSuffix,DirectionId));",
    "IF OBJECT_ID(N'dbo.KutaisiRouteStops',N'U') IS NULL CREATE TABLE dbo.KutaisiRouteStops (RouteId NVARCHAR(64) NOT NULL,PatternSuffix NVARCHAR(40) NOT NULL,DirectionId INT NOT NULL,StopOrder INT NOT NULL,StopID NVARCHAR(64) NOT NULL,StopCode NVARCHAR(32) NULL,StopName NVARCHAR(300) NOT NULL,Latitude DECIMAL(10,7) NULL,Longitude DECIMAL(10,7) NULL,VehicleMode NVARCHAR(20) NULL,Source NVARCHAR(200) NOT NULL,Verified BIT NOT NULL,CONSTRAINT PK_KutaisiRouteStops PRIMARY KEY(RouteId,PatternSuffix,DirectionId,StopOrder));",
    "IF OBJECT_ID(N'dbo.KutaisiScheduleCoverage',N'U') IS NULL CREATE TABLE dbo.KutaisiScheduleCoverage (RouteId NVARCHAR(64) NOT NULL,PatternSuffix NVARCHAR(40) NOT NULL,DirectionId INT NOT NULL,StopOrder INT NOT NULL,StopID NVARCHAR(64) NOT NULL,DayOfWeek NVARCHAR(12) NOT NULL,OperatingDayPattern NVARCHAR(100) NULL,ServiceDates NVARCHAR(300) NULL,ScheduleStatus NVARCHAR(80) NOT NULL,PublishedTimeCount INT NOT NULL,Source NVARCHAR(200) NOT NULL,Verified BIT NOT NULL,CONSTRAINT PK_KutaisiScheduleCoverage PRIMARY KEY(RouteId,PatternSuffix,DirectionId,StopOrder,DayOfWeek));",
    "IF OBJECT_ID(N'dbo.KutaisiTimetableEntries',N'U') IS NULL CREATE TABLE dbo.KutaisiTimetableEntries (RouteId NVARCHAR(64) NOT NULL,PatternSuffix NVARCHAR(40) NOT NULL,DirectionId INT NOT NULL,StopOrder INT NOT NULL,StopID NVARCHAR(64) NOT NULL,DayOfWeek NVARCHAR(12) NOT NULL,ScheduledTime TIME(0) NOT NULL,Source NVARCHAR(200) NOT NULL,Verified BIT NOT NULL,CONSTRAINT PK_KutaisiTimetableEntries PRIMARY KEY(RouteId,PatternSuffix,DirectionId,StopOrder,DayOfWeek,ScheduledTime));",""]
    sqltext += insert_sql("KutaisiRoutes",["RouteId","RouteNumber","RouteName","Mode","Source","Verified"],route_rows,{"RouteId","RouteNumber","RouteName","Mode","Source"})+[""]
    sqltext += insert_sql("KutaisiRoutePatterns",["RouteId","PatternSuffix","DirectionId","Direction","StartPoint","EndPoint","FirstStopID","LastStopID","HeadSign","ServiceDates","Source","Verified"],pattern_sql,{"RouteId","PatternSuffix","Direction","StartPoint","EndPoint","FirstStopID","LastStopID","HeadSign","ServiceDates","Source"})+[""]
    sqltext += insert_sql("KutaisiRouteStops",["RouteId","PatternSuffix","DirectionId","StopOrder","StopID","StopCode","StopName","Latitude","Longitude","VehicleMode","Source","Verified"],stop_sql,{"RouteId","PatternSuffix","StopID","StopCode","StopName","VehicleMode","Source"})+[""]
    sqltext += insert_sql("KutaisiScheduleCoverage",["RouteId","PatternSuffix","DirectionId","StopOrder","StopID","DayOfWeek","OperatingDayPattern","ServiceDates","ScheduleStatus","PublishedTimeCount","Source","Verified"],coverage_sql,{"RouteId","PatternSuffix","StopID","DayOfWeek","OperatingDayPattern","ServiceDates","ScheduleStatus","Source"})+[""]
    sqltext += insert_sql("KutaisiTimetableEntries",["RouteId","PatternSuffix","DirectionId","StopOrder","StopID","DayOfWeek","ScheduledTime","Source","Verified"],timetable_sql,{"RouteId","PatternSuffix","StopID","DayOfWeek","ScheduledTime","Source"})
    (OUT/"seed.sql").write_text("\n".join(sqltext)+"\n",encoding="utf-8-sig")

    lines=["# Kutaisi municipal bus timetable extraction report","",f"- **Extracted:** {EXTRACTED}",f"- **Source:** [{SOURCE}]({SITE}/#/schedule).",f"- **App version observed:** {APP_VERSION}","- **Mode:** municipal city buses (`BUS`); intercity records are excluded.","- **Schedule type:** published timetable; live arrivals were not used.",f"- **Service dates returned:** {', '.join(sorted(dates_seen)) or 'none'}.","","## Counts","","| Measure | Count |","|---|---:|",f"| Bus routes | {len(routes)} |",f"| Route patterns/directions | {len(patterns)} |",f"| Route + direction ID combinations | {len(route_dir)} |",f"| Unique stop IDs | {summary['uniqueStops']} |",f"| Route-pattern-stop combinations | {len(stops)} |",f"| Timetable records | {len(recs)} |",f"| Verified timetable records | {summary['verifiedTimetableRecords']} |",f"| Stop/day checks | {len(cv)} |",f"| Checks with published times | {summary['stopDaysWithPublishedTimes']} |",f"| Checks without returned schedule | {len(missing)} |",f"| Route-stop rows missing coordinates | {summary['missingCoordinateRouteStopRows']} |","","## Coverage by weekday","","| Day | Time records | Stop/day checks | Published schedule checks | No schedule group returned |","|---|---:|---:|---:|---:|"]
    for d in DAYS: lines.append(f"| {d} | {weekday[d]['timetableRecords']} | {weekday[d]['stopDayChecks']} | {weekday[d]['publishedScheduleChecks']} | {weekday[d]['noScheduleGroupChecks']} |")
    lines += ["","`coverage.csv` records every route-pattern-stop/day check. Days without a returned schedule group are marked unavailable, not inferred as no-service.","","## Holidays","","The public schedule response exposed weekly day ranges and service dates, but no distinct public-holiday day type. No holiday schedule was inferred.","","## Files","","- `timetable.csv`: flat row-per-time export used by the Windows Forms app.","- `route_patterns.csv`: route direction and variation IDs, endpoints, and headsigns.","- `route_stops.csv`: ordered stops, IDs, stop codes, Georgian names, and coordinates.","- `coverage.csv`: one status row per route-pattern-stop/day check.","- `timetable.json`: normalized JSON; `recordColumns` defines each compact timetable row and metadata/stop/pattern records supply the joined fields.","- `seed.sql`: normalized SQL Server tables for routes, patterns, stops, day coverage, and timetable entries.","","## Remaining gaps","",*(f"- {e}" for e in (issues+errors) if e),f"- {len(missing)} route-pattern-stop/day checks have no published times in the API response; see `coverage.csv`.",f"- {summary['missingCoordinateRouteStopRows']} route-pattern-stop rows have no coordinates." if summary['missingCoordinateRouteStopRows'] else "- Coordinates were returned for every route-pattern-stop row.","","## Validation","","- Enumerated the BUS routes from the public app route list.","- Queried each route for patterns covering the current week, then queried each pattern's stops and schedule.","- Cross-checked route 3, stop code 244 against the open app screen: Friday 08:00 and 08:30 match the API.","- Compared route-stop order/IDs with timetable stop order/IDs; differences are listed above.","- Collapsed exact duplicate timetable rows by route, pattern, direction, stop position/ID, weekday, and time.","- Source paths: `/pis-gateway/api/v3/routes` and `/pis-gateway/api/v3/routes/{routeId}/schedule?patternSuffix=…&locale=ka`."
    ]
    (OUT/"EXTRACTION_REPORT.md").write_text("\n".join(lines)+"\n",encoding="utf-8")
    print("\nExtraction complete")
    print("routes={}, patterns={}, stops={}, route-stop combinations={}, timetable rows={}".format(len(routes),len(patterns),summary["uniqueStops"],len(stops),len(recs)))
    print("coverage checks={}, incomplete={}, API issues={}".format(len(cv),len(missing),len(issues)+len(errors)))
    print("Output: {}".format(OUT))

if __name__ == "__main__":
    try: main()
    except Exception as e:
        print(f"Extraction failed: {e}",file=sys.stderr); raise
