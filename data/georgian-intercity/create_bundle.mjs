import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { SpreadsheetFile, Workbook } from "@oai/artifact-tool";

const outDir = path.dirname(fileURLToPath(import.meta.url));
const checkedOn = "2026-11-13";
const sources = [
  ["citybus-home", "Citybus Georgia official fares and popular routes", "https://citybus.ge/en", "Official operator", "Current fare list; does not show all date-specific departures."],
  ["citybus-tb-bt-08", "Citybus Tbilisi–Batumi CT0801 schedule", "https://citybus.ge/en/bus/route/number-15166", "Official operator route page", "Stop-by-stop listing; date applicability not shown in the accessible text."],
  ["citybus-tb-bt-09", "Citybus Tbilisi–Batumi CT0901 schedule", "https://citybus.ge/en/bus/route/number-19556", "Official operator route page", "Dates shown: 11–20 November; year interpreted as 2026 from research date."],
  ["citybus-tb-bt-16", "Citybus Tbilisi–Batumi CT1601 schedule", "https://citybus.ge/en/bus/route/number-15076", "Official operator route page", "Dates shown: 11–17 and 19–21 November; year interpreted as 2026."],
  ["citybus-tb-bt-18", "Citybus Tbilisi Didube–Batumi CT1801 schedule", "https://citybus.ge/en/bus/route/number-18173", "Official operator route page", "Dates shown: 12–21 November; year interpreted as 2026."],
  ["citybus-bt-tb-01", "Citybus Batumi–Tbilisi CT0102 schedule", "https://citybus.ge/en/bus/route/number-19558", "Official operator route page", "Departure and stops available; date range not shown in accessible extract."],
  ["citybus-bt-tb-08", "Citybus Batumi–Tbilisi CT0802 schedule", "https://citybus.ge/en/bus/route/number-15069", "Official operator route page", "Summary indexed exact stop times; date range not shown."],
  ["citybus-bt-tb-09", "Citybus Batumi–Tbilisi CT0902 schedule", "https://citybus.ge/en/bus/route/number-19557", "Official operator route page", "Dates shown include 10 December and later holiday dates; year inferred from current listing."],
  ["citybus-bt-tb-10", "Citybus Batumi–Tbilisi CT1002 schedule", "https://citybus.ge/en/bus/route/number-16468", "Official operator route page", "Dates shown: 12–21 November; year interpreted as 2026."],
  ["citybus-bt-tb-16", "Citybus Batumi–Tbilisi CT1602 schedule", "https://citybus.ge/en/bus/route/number-17785", "Official operator route page", "Dates shown: 12–21 November; year interpreted as 2026."],
  ["citybus-bt-tb-17", "Citybus Batumi–Tbilisi CT1702 Express schedule", "https://citybus.ge/en/bus/route/number-15803", "Official operator route page", "Stop-by-stop listing; date applicability not shown in accessible extract."],
  ["citybus-tb-sarpi-01", "Citybus Tbilisi–Sarpi CT0101 schedule", "https://citybus.ge/en/bus/route/number-15074", "Official operator route page", "Dates shown: 12–21 November; year interpreted as 2026."],
  ["citybus-tb-kobuleti", "Citybus Tbilisi–Kobuleti booking results", "https://citybus.ge/en/Tbilisi/Kobuleti", "Official operator booking page", "Date-specific result shown for 19 October; same coach continues to Batumi."],
  ["citybus-tb-ureki", "Citybus Tbilisi–Ureki booking results", "https://citybus.ge/en/Tbilisi/Ureki", "Official operator booking page", "Date-specific result shown for 15 October; same coach continues to Batumi."],
  ["metro-tb-bt", "Metro Georgia Tbilisi–Batumi schedule", "https://metrogeorgia.ge/otobus-bileti/TIFLIS-BATUM", "Official operator booking listing", "Exact fares and a dated 11 September 2026 sample; historical at research date."],
  ["metro-bt-tb", "Metro Georgia Batumi–Tbilisi schedule", "https://metrogeorgia.ge/otobus-bileti/BATUM-TIFLIS", "Official operator booking listing", "Exact fares and a dated 6 November 2026 sample; historical at research date."],
  ["metro-faq", "Metro Georgia FAQ and station information", "https://www.metrogeorgia.ge/sss", "Official operator", "Route duration and terminal locations."],
  ["gb-timetable", "Georgian Bus timetable", "https://georgianbus.com/en/page/timetable", "Official operator", "Flight-linked dated sample timetables; search index exposes September and October 2026 examples."],
  ["gb-faq", "Georgian Bus routes, flight adjustment and stations FAQ", "https://georgianbus.com/en/frequently-asked-questions", "Official operator", "Confirms route pairs, seasonal Gudauri service, and station details; times depend on flights."],
  ["gb-price-secondary", "Kutaisi airport transfer prices and durations", "https://thegeorgianguide.com/practical-info/kutaisi-airport-transfers/", "Secondary guide", "Used only for a 30 GEL airport-transfer fare; not the timetable authority."],
  ["tkt-omnibus", "Omnibus route network on TKT.GE", "https://tkt.ge/en/omnibus", "Booking platform", "Route pairs and approximate durations; no dated departure list exposed."],
  ["tkt-faq", "TKT.GE transport providers FAQ", "https://tkt.ge/en/transport", "Booking platform", "Names providers including Metro Georgia, Omnibus, Georgian Bus, ClassTour, Ortachala and Kutaisi stations; route attribution is incomplete."],
  ["ticket-ge-home", "User-provided ticket.ge entry point", "https://ticket.ge/", "Booking platform", "Redirects to TRE.GE; the current landing page has a dynamic bus search and provider/destination summaries, but the all-date search results are not exposed in static text."],
  ["tre-home", "TRE.GE / former TKT.GE public platform", "https://tre.ge/en", "Booking platform", "User-provided ticket.ge URL redirects to TRE.GE; page mixes transport modes and dynamic booking content."],
  ["tre-citybus", "TRE.GE City Bus provider page", "https://tre.ge/en/citybus", "Booking platform / operator profile", "Lists Tbilisi, Kutaisi, Batumi, Ureki, Kobuleti and Sarpi; states the Tbilisi–Batumi service has daily departures and earliest Tbilisi departure 08:00. Dynamic date search was not captured."],
  ["tre-omnibus", "TRE.GE OmniBus provider page", "https://tre.ge/en/omnibus", "Booking platform / operator profile", "Lists Tbilisi, Batumi, Zugdidi and Kutaisi Airport; exact dated departures were not exposed in static page text."],
  ["tre-georgianbus", "TRE.GE Georgian Bus provider page", "https://tre.ge/en/georgianbus", "Booking platform / operator profile", "Lists Tbilisi, Batumi, Kutaisi Airport, Kobuleti and Gudauri; the dynamic date search was not captured."],
  ["tre-zugdidi", "TRE.GE Tbilisi–Zugdidi destination page", "https://tre.ge/en/destination/tbilisi-zugdidi", "Booking platform", "Mixed bus/train page; accessible text does not expose bus-specific trips."],
  ["terminal-schedule", "Tbilisi Central Bus Terminal schedule", "https://busterminal.ge/en/schedule/", "Terminal listing", "Undated schedule snapshot with times and an operator column; several rows conflict with operator pages or identify a ticket vendor rather than carrier."],
  ["terminal-destinations", "Tbilisi Central Bus Terminal destinations and partners", "https://busterminal.ge/en/destinations/", "Terminal listing", "Published route leads, starting fares, estimated durations and international carrier names."],
  ["kutaisi-schedule", "Kutaisi Travel transport schedule", "https://kutaisi.travel/en/transport-schedule-en/", "Regional tourism portal", "Detailed regional minibus providers, fares, exact departures and frequency windows; in-page effective date is not stated."],
  ["mestia-municipal", "Mestia Municipality intercity minibus PDF", "https://mestia.gov.ge/sites/default/files/sakalaktashoriso_transporti.pdf", "Municipal source", "Daily Mestia-origin schedules, fares and contact; effective date is not stated."],
  ["mestia-guide", "ProSvaneti Mestia marshrutka guide", "https://prosvaneti.ge/en/blog/marshrutka-to-mestia", "Secondary guide", "Updated August 2026; lists departure examples and approximate durations; operator names and fares often absent."],
  ["nationalparks-borjomi", "Borjomi–Kharagauli National Park transport information", "https://www.nationalparks.ge/en/master/transportation/borjomi-kharagaulinp", "Official public agency visitor information", "Daily Tbilisi–Borjomi minibus service window and route duration; no operator or exact frequency per departure."],
  ["rustavi-info", "Rustavi Municipality public transport information", "https://rustavi.gov.ge/sachiro-informacia/", "Municipal source", "Identifies Intertrans on the Tbilisi–Rustavi corridor; detailed schedule absent."],
  ["rustavi-budget", "Rustavi 2026–2029 municipal budget", "https://rustavi.gov.ge/wp-content/uploads/2025/11/3-2026-%E1%83%AC%E1%83%9A%E1%83%98%E1%83%A1-%E1%83%9E%E1%83%A0%E1%83%98%E1%83%9D%E1%83%A0%E1%83%98%E1%83%A2%E1%83%94%E1%83%A2%E1%83%94%E1%83%91%E1%83%98%E1%83%A1-%E1%83%93%E1%83%9D%E1%83%99%E1%83%A3%E1%83%9B%E1%83%94%E1%83%9C%E1%83%A2%E1%83%98.pdf", "Municipal source", "Mentions five Tbilisi–Rustavi route lines and aggregate 130 bus movements per day; no trip-level times."],
  ["marshrutka-lines", "Georgian Marshrutka route directory", "https://marshrutka.info/lines", "Third-party route directory", "Current HTML exposes 249 line links although page heading says 246; carrier and live schedule completeness are not established."],
  ["marshrutka-cities", "Georgian Marshrutka city directory", "https://marshrutka.info/cities", "Third-party route directory", "Lists 146 place nodes; this is a directory count, not proof of an active scheduled service at each node."],
  ["marshrutka-line-details", "Georgian Marshrutka individual route-detail pages", "https://marshrutka.info/lines", "Third-party route directory", "All 249 linked line pages were fetched on the research date. Pages are community-submitted, may be stale or internally inconsistent, and rarely name the carrier; their schedule observations are not operator-verified."],
  ["marshrutka-line-operators", "Georgian Marshrutka line-operator index", "https://marshrutka.info/line-operators", "Third-party route directory", "The public operator index exposed no named operators; individual line descriptions mention Tetritskaro Transport Company Ltd on the Tbilisi–Manglisi corridor."],
  ["easyway-delivery", "Easy Way delivery-zone schedule (excluded from passenger carriers)", "https://www.easyway.ge/en/schedule", "Courier / parcel service", "Lists parcel pickup and delivery zones/days; no scheduled passenger bus services were identified."],
  ["okriba-home", "Okriba official site and bus terminal", "https://okriba.ge/en", "Official provider/terminal", "Confirms Okriba identity and terminal role; top destination links are not trip-specific."],
  ["okriba-schedule", "Okriba official schedule page", "https://okriba.ge/en/branches", "Official provider schedule listing", "Lists departure times under the Kutaisi branch but does not name the destination beside those times; route pairing is corroborated by Kutaisi Travel, whose frequency window differs."],
  ["terminal-partners", "Tbilisi Central Bus Terminal transport partners", "https://busterminal.ge/en/partners/", "Terminal partner directory", "Names domestic coach and international operators and destination coverage; no per-trip timetable."],
  ["kutaisi-airport-transport", "Kutaisi International Airport public transport information", "https://kutaisiairport.ge/transport/", "Official airport route listing", "States that airport minibuses run to Tbilisi, Batumi, Zugdidi, Mestia, Kazbegi and other directions, with timetables arranged around flights; does not identify every carrier or exact trip."],
  ["omnibus-social", "Omnibus Express post mirrored by FindGlocal", "https://www.findglocal.com/GE/Tbilisi/Omnibus-Express", "Social post mirror", "Mentions Tbilisi–Zugdidi departures; route dates/applicability not clear in the accessible mirror."],
  ["didube-guide", "Didube Bus Station Guide 2026", "https://thegeorgianguide.com/practical-info/didube-bus-station/", "Secondary guide", "Current general route/fare ranges; no named carrier or trip-level booking schedule."],
  ["akhaltsikhe-guide", "Tbilisi–Akhaltsikhe minibus guide", "https://en.tbilisitrip.com/from-tbilisi-to-akhaltsikhe/", "Secondary guide", "Approximate hourly window, fare and duration; schedule described as fill-up based and unstable."],
  ["yandex-zugdidi", "Zugdidi station route map", "https://yandex.com.ge/maps/122069/zugdidi/stops/4925615606/", "Map directory", "Route presence only; time snapshot is not treated as a timetable."],
];
const sourceMap = Object.fromEntries(sources.map(([id, title, url, type, limitations]) => [id, { SourceId: id, SourceTitle: title, SourceUrl: url, SourceType: type, Limitations: limitations, CheckedOn: checkedOn }]));
const serviceRows = [];
const routeRows = [];
const routeKeyMap = new Map();
const toMinutes = (v) => { if (!v) return null; const [h,m] = v.split(":").map(Number); return h*60+m; };
const fmtTime = (v) => { if (!v) return null; const [h,m] = v.split(":").map(Number); return `${String(h).padStart(2,"0")}:${String(m).padStart(2,"0")}`; };
function addRoute(from, to, operator, status, x={}) {
  const key = `${operator}|${from}|${to}|${x.RouteVariant || ""}`;
  if (routeKeyMap.has(key)) {
    const r = routeRows[routeKeyMap.get(key)];
    if (x.SourceUrl && !r.SourceUrl.split(" | ").includes(x.SourceUrl)) r.SourceUrl += ` | ${x.SourceUrl}`;
    if (x.ServiceIds) r.ServiceIds = [...new Set([...r.ServiceIds, ...x.ServiceIds])];
    if (x.SourceTitle && !r.SourceTitle.split(" | ").includes(x.SourceTitle)) r.SourceTitle += ` | ${x.SourceTitle}`;
    if (x.Notes && !r.Notes.includes(x.Notes)) r.Notes = [r.Notes, x.Notes].filter(Boolean).join("; ");
    if (x.ScheduleEvidence && (!r.ScheduleEvidence || r.ScheduleEvidence === "Not provided in route index")) r.ScheduleEvidence = x.ScheduleEvidence;
    if (x.TicketPriceGEL != null && r.TicketPriceGEL == null) r.TicketPriceGEL = x.TicketPriceGEL;
    if (x.JourneyDurationText && !r.JourneyDurationText) r.JourneyDurationText = x.JourneyDurationText;
    if (x.CountAsConfirmedRoute === true) r.CountAsConfirmedRoute = true;
    return r;
  }
  const r = {
    RouteCoverageId: routeRows.length + 1,
    DepartureCity: from,
    ArrivalCity: to,
    OperatorName: operator,
    RouteVariant: x.RouteVariant || null,
    CoverageType: x.CoverageType || "Published route listing",
    CoverageStatus: status,
    CountAsConfirmedRoute: x.CountAsConfirmedRoute ?? true,
    ScheduleEvidence: x.ScheduleEvidence || "None captured",
    TicketPriceGEL: x.TicketPriceGEL ?? null,
    JourneyDurationText: x.JourneyDurationText || null,
    ServiceIds: x.ServiceIds || [],
    SourceTitle: x.SourceTitle || "",
    SourceUrl: x.SourceUrl || "",
    SourceCheckedDate: checkedOn,
    Notes: x.Notes || "",
  };
  routeKeyMap.set(key, routeRows.length);
  routeRows.push(r);
  return r;
}
function addService(x) {
  const dep = fmtTime(x.DepartureTime);
  const arr = fmtTime(x.ArrivalTime);
  let duration = x.DurationMinutes ?? null;
  let arrivalOffset = x.ArrivalDayOffset ?? null;
  if (dep && arr && duration == null) {
    let delta = toMinutes(arr) - toMinutes(dep);
    arrivalOffset = delta < 0 ? 1 : 0;
    if (delta < 0) delta += 1440;
    duration = delta;
  }
  const s = {
    ServiceId: serviceRows.length + 1,
    RouteCode: x.RouteCode || null,
    OperatorName: x.OperatorName,
    DepartureCity: x.DepartureCity,
    ArrivalCity: x.ArrivalCity,
    DepartureTime: dep,
    ArrivalTime: arr,
    ArrivalDayOffset: arrivalOffset,
    DurationMinutes: duration,
    JourneyDurationText: x.JourneyDurationText || (duration != null ? `${Math.floor(duration/60)}h ${duration%60}m (from listed stop times)` : null),
    TicketPriceGEL: x.TicketPriceGEL ?? null,
    OperatingDays: x.OperatingDays || null,
    OperatingDates: x.OperatingDates || null,
    DepartureStation: x.DepartureStation || null,
    ArrivalStation: x.ArrivalStation || null,
    ViaStops: x.ViaStops || null,
    SchedulePattern: x.SchedulePattern || null,
    ScheduleRecordType: x.SchedulePattern ? "Published frequency pattern" : "Published departure",
    EvidenceStatus: x.EvidenceStatus,
    SourceTitle: x.SourceTitle || sourceMap[x.SourceId]?.SourceTitle || "",
    SourceUrl: x.SourceUrl || sourceMap[x.SourceId]?.SourceUrl || "",
    PriceSourceUrl: x.PriceSourceUrl || sourceMap[x.PriceSourceId || x.SourceId]?.SourceUrl || null,
    SupportingSourceUrl: x.SupportingSourceUrl || sourceMap[x.SupportingSourceId]?.SourceUrl || null,
    SourceCheckedDate: checkedOn,
    Notes: x.Notes || "",
    Synthetic: false,
  };
  serviceRows.push(s);
  const status = s.SchedulePattern ? "Published frequency window" : "Published timed service";
  addRoute(s.DepartureCity, s.ArrivalCity, s.OperatorName, status, {
    RouteVariant: x.RouteVariant,
    CoverageType: s.ScheduleRecordType,
    ScheduleEvidence: dep ? `${dep}${arr ? `–${arr}` : ""}` : s.SchedulePattern,
    TicketPriceGEL: s.TicketPriceGEL,
    JourneyDurationText: s.JourneyDurationText,
    ServiceIds: [s.ServiceId],
    SourceTitle: s.SourceTitle,
    SourceUrl: s.SourceUrl,
    CountAsConfirmedRoute: x.CountAsConfirmedRoute ?? true,
    Notes: s.Notes,
  });
  return s;
}
function timed(operator, from, to, times, defaults={}) {
  for (const dep of times) addService({ ...defaults, OperatorName: operator, DepartureCity: from, ArrivalCity: to, DepartureTime: dep });
}
function pattern(operator, from, to, schedule, defaults={}) {
  addService({ ...defaults, OperatorName: operator, DepartureCity: from, ArrivalCity: to, SchedulePattern: schedule });
}

// Okriba publishes exact times under its Kutaisi branch. Kutaisi Travel identifies Okriba's
// Kutaisi–Tbilisi service and GEL 20 fare, but its frequency window differs; retain the conflict.
for (const dep of ["08:00","08:40","09:20","10:00","10:40","11:20","12:00","12:40","13:20","14:00","14:40","15:20","16:00","16:40","17:20","18:00","18:40","19:20","20:00"]) addService({
  OperatorName:"LTD Okriba", DepartureCity:"Kutaisi", ArrivalCity:"Tbilisi", DepartureTime:dep,
  TicketPriceGEL:20, PriceSourceId:"kutaisi-schedule", DepartureStation:"Okriba Kutaisi branch; Kutaisi Travel lists Chavchavadze Ave. 67",
  SourceId:"okriba-schedule", EvidenceStatus:"Official time list under Kutaisi branch; destination and fare paired from Kutaisi Travel; discrepancy unresolved",
  Notes:"Okriba's official page lists these Kutaisi-branch departure times but does not label the destination. Kutaisi Travel identifies LTD Okriba on Kutaisi–Tbilisi and lists GEL 20, with a different 06:50–19:00 frequency window; verify destination and current operating pattern before use."
});

// Citybus Georgia: exact stop-level trips. Each direction is independently sourced.
const cityVia = "Gori → Terjola/Kutaisi → Kopitnari Airport → Lanchkhuti → Ureki → Kobuleti";
const cityBusStation = "Tbilisi Bus Station ‘Ortachala’, 5 Dimitri Gulia St";
const batumiBusStation = "Batumi Bus Station, 11 Gogol Street";
const cbFare = { TicketPriceGEL: 45, PriceSourceId: "citybus-home" };
const cbDates = [
  ["08:00","13:35","Citybus CT0801","citybus-tb-bt-08",null,"08:00 Tbilisi; 09:00 Gori; 11:40 Kopitnari; 12:50 Ureki; 13:10 Kobuleti; 13:35 Batumi."],
  ["09:00","15:00","Citybus CT0901","citybus-tb-bt-09","2026-11-11 through 2026-11-20","09:00 Tbilisi; 10:00 Gori; 12:40 Kutaisi; 13:00 Kopitnari; 14:15 Ureki; 14:35 Kobuleti; 15:00 Batumi."],
  ["16:00","21:55","Citybus CT1601","citybus-tb-bt-16","2026-11-11 to 17 and 19 to 21","16:00 Tbilisi; 17:00 Gori; 19:30 Kutaisi; 19:45 Kopitnari; 21:00 Ureki; 21:35 Kobuleti; 21:55 Batumi."],
  ["18:00","23:30","Citybus CT1801 Didube","citybus-tb-bt-18","2026-11-12 through 2026-11-21","18:00 Didube; Gori; Terjola; Kutaisi; Kopitnari; Lanchkhuti; Ureki; Kobuleti; 23:30 Batumi."],
];
for (const [dep,arr,code,src,dates,via] of cbDates) addService({ OperatorName:"Citybus Georgia", DepartureCity:"Tbilisi", ArrivalCity:"Batumi", DepartureTime:dep, ArrivalTime:arr, RouteCode:code, TicketPriceGEL:45, PriceSourceId:"citybus-home", OperatingDays:src==="citybus-tb-bt-08"?"Daily (TRE provider page)":null, OperatingDates:dates, DepartureStation:dep==="18:00"?"Tbilisi Didube (Okriba), 151 Akaki Tsereteli St":"Tbilisi Ortachala, 5 Dimitri Gulia St", ArrivalStation:batumiBusStation, ViaStops:via, SourceId:src, SupportingSourceId:src==="citybus-tb-bt-08"?"tre-citybus":null, EvidenceStatus:dates?"Official dated booking timetable":"Official operator route listing; date not stated; TRE says daily", Notes:"Citybus homepage publishes 45 GEL for Tbilisi–Batumi. TRE's City Bus provider page states daily departures and 08:00 is the earliest departure. Duration is calculated from the route page's published first/last stop times." });
const reverseCb = [
  ["01:00","06:55","CT0102","citybus-bt-tb-01",null,"Batumi; Batumi railway station; Kobuleti; Ureki; Lanchkhuti; Kopitnari; Kutaisi; Terjola; Gori; Tbilisi."],
  ["08:00","13:30","CT0802","citybus-bt-tb-08",null,"Batumi; Kobuleti; Ureki; Kutaisi Airport stop; Gori; Tbilisi."],
  ["09:00","14:55","CT0902","citybus-bt-tb-09","2026-12-10; 2026-12-23 through 2027-01-01","Batumi; Kobuleti; Ureki; Kopitnari; Kutaisi; Gori; Tbilisi."],
  ["10:00","15:55","CT1002","citybus-bt-tb-10","2026-11-12 through 2026-11-21","Batumi; Kobuleti; Ureki; Lanchkhuti; Kopitnari; Kutaisi; Terjola; Gori; Didube."],
  ["16:00","21:55","CT1602","citybus-bt-tb-16","2026-11-12 through 2026-11-21","Batumi; Kobuleti; Ureki; Kopitnari; Kutaisi; Gori; Tbilisi."],
  ["17:00","23:00","CT1702 Express","citybus-bt-tb-17",null,"Batumi; Kobuleti; Ureki; Kopitnari; Gori; Tbilisi."],
];
for (const [dep,arr,code,src,dates,via] of reverseCb) addService({ OperatorName:"Citybus Georgia", DepartureCity:"Batumi", ArrivalCity:"Tbilisi", DepartureTime:dep, ArrivalTime:arr, RouteCode:code, TicketPriceGEL:45, PriceSourceId:"citybus-home", OperatingDates:dates, DepartureStation:batumiBusStation, ArrivalStation:cityBusStation, ViaStops:via, SourceId:src, EvidenceStatus:dates?"Official dated booking timetable":"Official operator route listing; date not stated", Notes:"Citybus homepage publishes 45 GEL for Batumi–Tbilisi. Duration is calculated from the published first/last stop times." });
addService({ OperatorName:"Citybus Georgia", DepartureCity:"Tbilisi", ArrivalCity:"Sarpi", DepartureTime:"01:00", ArrivalTime:"07:15", RouteCode:"CT0101", TicketPriceGEL:55, PriceSourceId:"citybus-home", OperatingDates:"2026-11-12 through 2026-11-21", DepartureStation:cityBusStation, ArrivalStation:"Sarpi customs checkpoint (GE–TR)", ViaStops:"Gori 02:00; Terjola 04:00; Kutaisi 04:30; Kopitnari 04:45; Lanchkhuti 05:30; Ureki 05:50; Kobuleti 06:10; Batumi 06:30; Batumi Airport 06:55; Gonio 07:05; Kvariati 07:10", SourceId:"citybus-tb-sarpi-01", EvidenceStatus:"Official dated booking timetable", Notes:"Price 55 GEL from Citybus official popular-routes list. Arrival shown as 07:15." });
// Bookable short-distance destinations on the same coach; kept as separate OD services.
for (const [to,dep,arr,code,src,date,price] of [
  ["Kobuleti","16:00","21:35","CT1601 Tbilisi–Batumi segment","citybus-tb-kobuleti","2026-10-19",45],
  ["Kobuleti","18:00","23:00","CT1801 Tbilisi Didube–Batumi segment","citybus-tb-kobuleti","2026-10-19",45],
  ["Ureki","18:00","22:40","CT1801 Tbilisi Didube–Batumi segment","citybus-tb-ureki","2026-10-15",40],
]) addService({ OperatorName:"Citybus Georgia", DepartureCity:"Tbilisi", ArrivalCity:to, DepartureTime:dep, ArrivalTime:arr, RouteCode:code, TicketPriceGEL:price, PriceSourceId:"citybus-home", OperatingDates:date, DepartureStation:dep==="18:00"?"Tbilisi Didube (Okriba), platform 5":cityBusStation, ArrivalStation:to==="Kobuleti"?"Kobuleti Railway Station":"Ureki Railway Station", ViaStops:"Same coach continues to Batumi", SourceId:src, EvidenceStatus:"Official date-specific booking result (historical sample)", Notes:"Destination is an intermediate stop on a through coach; distinct OD ticket/route listing, not an extra physical bus departure." });

// Metro Georgia: exact dated sample timetable rows (historical observations, not live guarantees).
const metroTb = [
  ["01:00","05:45"],["06:30","12:00"],["08:00","14:00"],["09:00","15:00"],["10:00","16:00"],["12:00","17:45"],["13:30","19:15"],["15:00","20:30"],["17:00","22:15"],["18:30","02:00"],["21:00","02:40"],["23:00","04:30"],["23:59","05:30"],
];
for (const [dep,arr] of metroTb) addService({ OperatorName:"Metro Georgia", DepartureCity:"Tbilisi", ArrivalCity:"Batumi", DepartureTime:dep, ArrivalTime:arr, TicketPriceGEL:45, OperatingDates:"2026-09-11 (dated sample)", DepartureStation:"Tbilisi, Gulia St. 1 / Ortachala terminal", ArrivalStation:"Batumi New International Bus Station, N. Gogol St. 11", SourceId:"metro-tb-bt", EvidenceStatus:"Official dated timetable (historical sample)", Notes:"45 GEL published on Metro Georgia booking listing. Duration calculated from listed times; overnight arrival day is recorded." });
for (const [dep,arr] of [["12:00","17:30"],["15:00","21:00"],["18:00","23:55"],["20:00","02:00"],["23:59","06:00"]]) addService({ OperatorName:"Metro Georgia", DepartureCity:"Batumi", ArrivalCity:"Tbilisi", DepartureTime:dep, ArrivalTime:arr, TicketPriceGEL:45, OperatingDates:"2026-11-06 (dated sample)", DepartureStation:"Batumi New International Bus Station, N. Gogol St. 11", ArrivalStation:"Tbilisi, Gulia St. 1 / Ortachala terminal", SourceId:"metro-bt-tb", EvidenceStatus:"Official dated timetable (historical sample)", Notes:"45 GEL published on Metro Georgia booking listing. Duration calculated from listed times; overnight arrival day is recorded." });

// Georgian Bus: flight-linked airport services plus one dated city-to-city schedule sample.
const airportLegs = [
  ["Tbilisi","Kutaisi International Airport","2026-09-19",[["04:30","08:30"],["07:30","11:30"],["08:30","12:30"],["16:00","20:00"],["21:30","01:30"],["23:00","03:00"]]],
  ["Tbilisi","Kutaisi International Airport","2026-09-20",[["06:00","10:00"],["08:30","12:30"],["12:00","16:00"],["15:00","19:00"],["16:00","20:00"],["23:00","03:00"]]],
  ["Kutaisi International Airport","Tbilisi","2026-09-19",[["01:30","05:30"],["02:05","06:05"],["11:45","15:45"],["15:05","19:05"],["23:00","03:00"]]],
  ["Kutaisi International Airport","Tbilisi","2026-09-20",[["01:00","05:00"],["01:45","05:45"],["04:30","08:30"],["12:45","16:45"],["14:50","18:50"],["15:25","19:25"],["18:30","22:30"],["22:00","02:00"],["23:15","03:15"]]],
];
for (const [from,to,date,legs] of airportLegs) for (const [dep,arr] of legs) addService({ OperatorName:"Georgian Bus", DepartureCity:from, ArrivalCity:to, DepartureTime:dep, ArrivalTime:arr, TicketPriceGEL:30, PriceSourceId:"gb-price-secondary", OperatingDates:`${date} (flight-linked dated sample)`, DepartureStation:from==="Tbilisi"?"Tbilisi Station Square, Tsotne Dadiani side":"Kutaisi International Airport arrivals hall", ArrivalStation:to==="Tbilisi"?"Tbilisi Station Square, Tsotne Dadiani side":"Kutaisi International Airport arrivals hall", SourceId:"gb-timetable", EvidenceStatus:"Official dated flight-linked timetable; fare from secondary guide", Notes:"Schedule is tied to flight operations and may change with flights. 30 GEL fare is secondary corroboration, not shown in the operator timetable." });
addService({ OperatorName:"Georgian Bus", DepartureCity:"Tbilisi", ArrivalCity:"Batumi", DepartureTime:"09:00", ArrivalTime:"14:30", OperatingDays:"Daily in listed sample", OperatingDates:"2026-10-26 through 2026-11-01 (historical sample)", TicketPriceGEL:null, DepartureStation:"Tbilisi Station Square, Tsotne Dadiani side", ArrivalStation:"Batumi New Bus Station, 11 Giorgi Tsabadze St., platform 14", SourceId:"gb-timetable", EvidenceStatus:"Official dated timetable (historical sample)", Notes:"The timetable page lists a daily 09:00–14:30 Tbilisi–Batumi trip in this date window. Fare was not visible." });

// Kutaisi regional bus/minibus timetables. Frequency windows remain one pattern row; no invented interval expansions.
const ktStation="Kutaisi, Chavchavadze Ave. 67";
pattern("LTD Okriba","Kutaisi","Tbilisi","06:50–19:00 every 40 minutes",{TicketPriceGEL:20,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Return window is separately listed as 06:50–20:00 every 40 minutes; no individual departures expanded."});
pattern("LTD Okriba","Tbilisi","Kutaisi","06:50–20:00 every 40 minutes",{TicketPriceGEL:20,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Return station is marked TBC by source."});
addService({OperatorName:"LTD Imertrans",DepartureCity:"Kutaisi",ArrivalCity:"Tbilisi",DepartureTime:"06:00",TicketPriceGEL:20,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated",Notes:"The source also lists departures 07:00–21:00 every 30 minutes; kept as a separate frequency-pattern row."});
pattern("LTD Imertrans","Kutaisi","Tbilisi","07:00–21:00 every 30 minutes",{TicketPriceGEL:20,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
addService({OperatorName:"LTD Imertrans",DepartureCity:"Tbilisi",ArrivalCity:"Kutaisi",DepartureTime:"07:00",TicketPriceGEL:20,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact return departure; page effective date not stated",Notes:"Source return window also lists 08:00–10:00 every 30 minutes; station at Tbilisi is marked TBC."});
pattern("LTD Imertrans","Tbilisi","Kutaisi","08:00–10:00 every 30 minutes",{TicketPriceGEL:20,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Return station is marked TBC by source."});
for (const [from,to,pat] of [["Kutaisi","Batumi","06:30–19:30 every 50 minutes"],["Batumi","Kutaisi","06:30–19:30 every 50 minutes"]]) pattern("LTD Imertrans",from,to,pat,{TicketPriceGEL:15,OperatingDays:"Daily",DepartureStation:from==="Kutaisi"?ktStation:null,ArrivalStation:to==="Kutaisi"?ktStation:null,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Source lists the same window for both directions."});
for (const t of ["10:50","11:55","13:45","17:50"]) addService({OperatorName:"LTD Sprinter",DepartureCity:"Kutaisi",ArrivalCity:"Zugdidi",DepartureTime:t,TicketPriceGEL:10,OperatingDays:"Daily",DepartureStation:ktStation,ViaStops:"Senaki (source heading: Kutaisi–Senaki, Zugdidi)",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated",Notes:"Fare and times are listed for the combined Senaki/Zugdidi line; return is TBC."});
pattern("LTD Sprinter","Kutaisi","Batumi","08:00–18:00 every 1 hour",{TicketPriceGEL:15,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Reverse timetable is TBC in source."});
addService({OperatorName:"LTD Sprinter",DepartureCity:"Kutaisi",ArrivalCity:"Mestia",DepartureTime:"10:00",TicketPriceGEL:35,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated",Notes:"Source lists return time 08:00; return station is TBC."});
addService({OperatorName:"LTD Sprinter",DepartureCity:"Mestia",ArrivalCity:"Kutaisi",DepartureTime:"08:00",TicketPriceGEL:35,OperatingDays:"Daily",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact return time; page effective date not stated",Notes:"Reverse direction represented from the source return-time field; departure station is not stated."});
for (const t of ["11:00","15:00","16:15","17:30"]) addService({OperatorName:"LTD Sprinter",DepartureCity:"Kutaisi",ArrivalCity:"Ozurgeti",DepartureTime:t,TicketPriceGEL:12,OperatingDays:"Daily",DepartureStation:ktStation,ViaStops:"Chokhatauri (source heading: Kutaisi–Chokhatauri–Ozurgeti)",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated",Notes:"Reverse timetable is TBC in source."});
for (const t of ["11:00","16:00"]) addService({OperatorName:"LTD Sprinter",DepartureCity:"Kutaisi",ArrivalCity:"Ambrolauri",DepartureTime:t,TicketPriceGEL:15,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated"});
addService({OperatorName:"LTD Sprinter",DepartureCity:"Ambrolauri",ArrivalCity:"Kutaisi",DepartureTime:"09:30",TicketPriceGEL:15,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact return time; page effective date not stated"});
addService({OperatorName:"LTD Sprinter",DepartureCity:"Kutaisi",ArrivalCity:"Oni",DepartureTime:"16:00",TicketPriceGEL:20,OperatingDays:"Daily",DepartureStation:ktStation,ViaStops:"Ambrolauri",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated"});
addService({OperatorName:"LTD Sprinter",DepartureCity:"Oni",ArrivalCity:"Kutaisi",DepartureTime:"09:00",TicketPriceGEL:20,OperatingDays:"Daily",ArrivalStation:ktStation,ViaStops:"Ambrolauri",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact return time; page effective date not stated"});
pattern("Carrier not named — Kutaisi Central Bus Station service","Kutaisi","Zugdidi","06:30–18:20 every 30 minutes",{TicketPriceGEL:10,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Source labels station/dispatcher, not a carrier; return timetable TBC."});
pattern("Carrier not named — Kutaisi Central Bus Station service","Kutaisi","Martvili","07:00–18:20 every 60 minutes",{TicketPriceGEL:6,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Source labels station/dispatcher, not a carrier; return timetable TBC."});
addService({OperatorName:"Carrier not named — Kutaisi Central Bus Station service",DepartureCity:"Kutaisi",ArrivalCity:"Abastumani",DepartureTime:"11:30",TicketPriceGEL:20,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated",Notes:"Source labels station/dispatcher, not a carrier; return timetable TBC."});
pattern("LTD Sprinter","Kutaisi","Samtredia","07:30–18:30 every 20–30 minutes",{TicketPriceGEL:3,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
pattern("LTD Sprinter","Samtredia","Kutaisi","08:00–18:00 every 30 minutes",{TicketPriceGEL:3,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
for (const [operator,from,to,pat,note] of [
  ["Carrier not named — Kutaisi Central Bus Station service","Kutaisi","Tskaltubo","N34: 07:15–19:00 every 15 minutes","Source says route N34; Kutaisi stop Chavchavadze Ave. 67."],
  ["Carrier not named — Kutaisi Central Bus Station service","Tskaltubo","Kutaisi","N34: 06:30–19:00 every 15 minutes","Return stop: Tskaltubo, Avliani St. 12."],
  ["LTD Sprinter","Kutaisi","Tskaltubo","N44: 07:50–19:30 every 20 minutes","Route N44; effective date not stated."],
  ["LTD Sprinter","Tskaltubo","Kutaisi","N44: 07:00–19:45 every 20 minutes","Route N44; effective date not stated."],
  ["Carrier not named — N30 Kutaisi minibus","Kutaisi","Tskaltubo","N30: 07:15–19:00 every 20 minutes","Source labels operator/station TBC; departure near Red Bridge."],
  ["Carrier not named — N30 Kutaisi minibus","Tskaltubo","Kutaisi","N30: 07:15–18:30 every 20 minutes","Source labels operator/station TBC."],
]) pattern(operator,from,to,pat,{TicketPriceGEL:2,OperatingDays:"Daily",DepartureStation:from==="Kutaisi"?ktStation:null,ArrivalStation:to==="Kutaisi"?ktStation:null,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:note});
for (const t of ["11:45","15:00","17:00"]) addService({OperatorName:"Carrier not named — Kutaisi Central Bus Station service",DepartureCity:"Kutaisi",ArrivalCity:"Sachkhere",DepartureTime:t,TicketPriceGEL:10,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated"});
for (const t of ["07:50","09:30","11:00","13:00","15:30"]) addService({OperatorName:"Carrier not named — Kutaisi Central Bus Station service",DepartureCity:"Sachkhere",ArrivalCity:"Kutaisi",DepartureTime:t,TicketPriceGEL:10,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact return departure; page effective date not stated"});
for (const t of ["07:00","12:40","14:30"]) addService({OperatorName:"Carrier not named — Kutaisi Central Bus Station service",DepartureCity:"Kutaisi",ArrivalCity:"Kharagauli",DepartureTime:t,TicketPriceGEL:8,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated"});
for (const t of ["07:50","15:00"]) addService({OperatorName:"Carrier not named — Kutaisi Central Bus Station service",DepartureCity:"Kharagauli",ArrivalCity:"Kutaisi",DepartureTime:t,TicketPriceGEL:8,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact return departure; page effective date not stated"});
pattern("Carrier not named — Kutaisi Central Bus Station service","Kutaisi","Baghdati","07:00–19:20 every 60 minutes",{TicketPriceGEL:3,OperatingDays:"Frequency depends on passenger demand",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
pattern("Carrier not named — Kutaisi Central Bus Station service","Baghdati","Kutaisi","06:45–18:40 every 20 minutes",{TicketPriceGEL:3,OperatingDays:"Frequency depends on passenger demand",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
pattern("Carrier not named — Kutaisi Central Bus Station service","Kutaisi","Vani","07:00–19:25 every 60 minutes",{TicketPriceGEL:4,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
pattern("Carrier not named — Kutaisi Central Bus Station service","Vani","Kutaisi","06:50–18:10 every 40 minutes",{TicketPriceGEL:4,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
for (const t of ["09:00","10:00","10:30","11:20","12:20","14:00","14:20","15:30","17:20","18:00","18:50"]) addService({OperatorName:"Carrier not named — Kutaisi Central Bus Station service",DepartureCity:"Kutaisi",ArrivalCity:"Terjola",DepartureTime:t,TicketPriceGEL:3,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated"});
pattern("Carrier not named — Kutaisi Central Bus Station service","Terjola","Kutaisi","07:30–18:10 every 30 minutes",{TicketPriceGEL:3,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated"});
for (const t of ["08:00","11:00","13:00","14:00","15:00","16:00","16:40"]) addService({OperatorName:"Carrier not named — Kutaisi Central Bus Station service",DepartureCity:"Kutaisi",ArrivalCity:"Tkibuli",DepartureTime:t,TicketPriceGEL:5,OperatingDays:"Daily",DepartureStation:"Kutaisi, King Tamar St. 5",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated"});
pattern("Carrier not named — Kutaisi Central Bus Station service","Tkibuli","Kutaisi","08:00–12:00 hourly; additional departures 15:00 and 17:00",{TicketPriceGEL:5,OperatingDays:"Daily",ArrivalStation:"Kutaisi, King Tamar St. 5",SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency/pattern; page effective date not stated"});
pattern("Carrier not named — Khoni route","Kutaisi","Khoni","07:30–19:30 every 30 minutes",{TicketPriceGEL:3,OperatingDays:"Daily",DepartureStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Carrier/station is blank in source; return window is 07:00–19:00 every 30 minutes."});
pattern("Carrier not named — Khoni route","Khoni","Kutaisi","07:00–19:00 every 30 minutes",{TicketPriceGEL:3,OperatingDays:"Daily",ArrivalStation:ktStation,SourceId:"kutaisi-schedule",EvidenceStatus:"Published frequency; page effective date not stated",Notes:"Carrier/station is blank in source."});
for (const t of ["07:30","11:00","14:00","16:00","18:00"]) addService({OperatorName:"Carrier not named — Gelati minibus",DepartureCity:"Kutaisi",ArrivalCity:"Gelati Monastery",DepartureTime:t,TicketPriceGEL:2,OperatingDays:"Daily",DepartureStation:"Behind Measkishvili Theatre, Kutaisi",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact departure; page effective date not stated",Notes:"Local/tourist regional service; included separately from intercity routes."});
for (const t of ["08:00","11:30","14:30","16:30"]) addService({OperatorName:"Carrier not named — Gelati minibus",DepartureCity:"Gelati Monastery",ArrivalCity:"Kutaisi",DepartureTime:t,TicketPriceGEL:2,OperatingDays:"Daily",ArrivalStation:"Kutaisi, behind Measkishvili Theatre",SourceId:"kutaisi-schedule",EvidenceStatus:"Published exact return departure; page effective date not stated",Notes:"Local/tourist regional service; included separately from intercity routes."});

// Mestia departures from municipal and current guide sources.
for (const [from,to,dep,fare,station,note] of [
  ["Mestia","Tbilisi","08:00",50,"Mestia station; arrival Tbilisi Station Square","Municipal table lists daily once; reverse Tbilisi departure not given."],
  ["Mestia","Tbilisi","08:00",50,"Mestia station; arrival Tbilisi Samgori","Separate Samgori route line in municipal table."],
  ["Mestia","Kutaisi","08:00",40,"Mestia station","Municipal table lists daily once; reverse time not given there."],
  ["Mestia","Zugdidi","08:00",35,"Mestia station","Daily; paired return departures from Zugdidi at 12:00 and 15:00."],
  ["Mestia","Zugdidi","12:00",35,"Mestia station","Daily; paired return departures from Zugdidi at 12:00 and 15:00."],
  ["Zugdidi","Mestia","12:00",35,"Zugdidi station","Daily; return column in municipal table."],
  ["Zugdidi","Mestia","15:00",35,"Zugdidi station","Daily; return column in municipal table."],
  ["Mestia","Batumi","08:00",50,"Mestia station","Daily once; paired return from Batumi at 15:00."],
  ["Batumi","Mestia","15:00",50,"Batumi station","Daily; return column in municipal table."],
]) addService({OperatorName:"Mestia public minibus (carrier not identified)",DepartureCity:from,ArrivalCity:to,DepartureTime:dep,TicketPriceGEL:fare,OperatingDays:"Daily",DepartureStation:station,SourceId:"mestia-municipal",EvidenceStatus:"Municipal timetable; effective date not stated",Notes:note});
for (const [from,to,dep,duration,note] of [
  ["Tbilisi","Mestia","07:00","8–10 h","Departure listed from Samgori; carrier not named. Guide warns vehicles may leave early when full."],
  ["Tbilisi","Mestia","07:30","8–10 h","Departure listed from Central Railway Station; carrier not named. Guide warns vehicles may leave early when full."],
  ["Kutaisi","Mestia","07:30","5–6 h","Secondary guide; carrier not named. Kutaisi municipal listing separately gives Sprinter at 10:00."],
  ["Zugdidi","Mestia","12:00","3–4 h","Secondary guide; carrier not named; municipal schedule lists same return time."],
  ["Zugdidi","Mestia","15:00","3–4 h","Secondary guide; carrier not named; municipal schedule lists same return time."],
]) addService({OperatorName:"Carrier not named — Mestia marshrutka listing",DepartureCity:from,ArrivalCity:to,DepartureTime:dep,JourneyDurationText:duration,OperatingDays:"Daily (guide description)",SourceId:"mestia-guide",EvidenceStatus:"Secondary guide schedule example",Notes:note});

// Tbilisi Central terminal snapshot; preserve the carrier uncertainty and conflict flags.
const terminalRows = [
  ["06:00","Batumi","Omnibus Express","5–6 h",null,"Omnibus is shown in terminal operator column."],
  ["06:30","Kutaisi","Metro Georgia","3–4 h",null,"Terminal listing; no arrival time or fare."],
  ["07:00","Telavi","Carrier unresolved — TKT.GE listing","2 h",15,"TKT.GE is shown as ticket link/vendor, not a carrier."],
  ["07:30","Batumi","Citybus Georgia","5–6 h",null,"Terminal snapshot differs from Citybus route page's 08:00 departure; unresolved secondary listing."],
  ["08:00","Zugdidi","Omnibus Express","5–6 h",null,"Terminal listing; no arrival time or fare."],
  ["08:30","Gori","Metro Georgia",null,null,"Terminal listing; no operator timetable independently found."],
  ["09:00","Yerevan","Carrier unresolved — TKT.GE listing","5 h",35,"Cross-border listing; terminal source names TKT.GE as ticket link/vendor."],
  ["09:30","Mestia","Omnibus Express",null,null,"Terminal listing; exact official Omnibus timetable was not accessible."],
  ["10:00","Kutaisi","Citybus Georgia","3–4 h",30,"Terminal snapshot; official fare page lists Tbilisi–Kutaisi at 30 GEL."],
  ["10:30","Batumi","Metro Georgia","5–6 h",null,"Terminal listing; no arrival time or fare."],
  ["11:00","Kvareli","Carrier unresolved — TKT.GE listing","2.1 h",15,"TKT.GE is shown as ticket link/vendor, not a carrier."],
  ["12:00","Zugdidi","Citybus Georgia","5–6 h",null,"Terminal listing; Citybus destination fare/time not exposed."],
  ["14:00","Batumi","Omnibus Express","5–6 h",null,"Terminal listing; no arrival time or fare."],
  ["16:00","Kutaisi","Metro Georgia","3–4 h",null,"Terminal listing; no arrival time or fare."],
  ["18:00","Yerevan","Carrier unresolved — TKT.GE listing","5 h",35,"Cross-border listing; ticket vendor, not verified carrier."],
  ["20:00","Istanbul","Carrier unresolved — terminal listing","18 h",120,"Cross-border route listing; company is not identified for this departure."],
];
for (const [dep,to,op,duration,fare,note] of terminalRows) addService({OperatorName:op,DepartureCity:"Tbilisi",ArrivalCity:to,DepartureTime:dep,TicketPriceGEL:fare,JourneyDurationText:duration,DepartureStation:"Tbilisi Central Bus Terminal (address: 5 Gulia St.)",SourceId:"terminal-schedule",EvidenceStatus:"Undated terminal schedule snapshot; carrier/time not independently confirmed",Notes:note});

// Undated Omnibus Express mirrored announcement; retained as lower-confidence schedule evidence.
for (const dep of ["10:00","12:00","23:45"]) addService({OperatorName:"Omnibus Express",DepartureCity:"Tbilisi",ArrivalCity:"Zugdidi",DepartureTime:dep,OperatingDates:"Announcement mirror dated 2026-08-16; applicability unclear",DepartureStation:"Tbilisi (station not stated)",SourceId:"omnibus-social",EvidenceStatus:"Secondary mirror of operator announcement; current applicability unconfirmed",Notes:"Departure time copied from a mirrored social post. Do not treat as currently bookable without operator confirmation."});

// Route listings from independently identified operators and terminals.
const rr=(from,to,op,status,x={})=>addRoute(from,to,op,status,{...x,CountAsConfirmedRoute:x.CountAsConfirmedRoute??true});
for (const [from,to,fare,dur] of [
  ["Tbilisi","Batumi",45,"5–6 h"],["Batumi","Tbilisi",45,"5–6 h"],
  ["Tbilisi","Kutaisi",30,null],["Kutaisi","Tbilisi",30,null],
  ["Tbilisi","Sarpi",55,null],["Sarpi","Tbilisi",55,null],
  ["Tbilisi","Ureki",40,null],["Ureki","Tbilisi",40,null],
  ["Tbilisi","Kobuleti",45,null],["Kobuleti","Tbilisi",45,null],
]) rr(from,to,"Citybus Georgia","Official operator route and fare listing",{CoverageType:"Direct route listing",TicketPriceGEL:fare,JourneyDurationText:dur,SourceTitle:sourceMap["citybus-home"].SourceTitle,SourceUrl:sourceMap["citybus-home"].SourceUrl});
for (const [from,to] of [["Tbilisi","Kutaisi International Airport"],["Kutaisi International Airport","Tbilisi"],["Batumi","Kutaisi International Airport"],["Kutaisi International Airport","Batumi"],["Gudauri","Kutaisi International Airport"],["Kutaisi International Airport","Gudauri"],["Tbilisi","Batumi"],["Batumi","Tbilisi"],["Tbilisi","Kutaisi"],["Kutaisi","Tbilisi"]]) rr(from,to,"Georgian Bus","Route pair confirmed in operator FAQ; dated schedules captured only for some routes",{CoverageType:"Official route listing",SourceTitle:sourceMap["gb-faq"].SourceTitle,SourceUrl:sourceMap["gb-faq"].SourceUrl,Notes:(from.includes("Gudauri")||to.includes("Gudauri"))?"Winter season only, per operator FAQ.":"Flight-linked schedules may change with airport flight times."});
for (const [from,to,dur] of [["Tbilisi","Kutaisi International Airport","4 h"],["Kutaisi International Airport","Tbilisi","4 h"],["Batumi","Kutaisi International Airport","3 h"],["Kutaisi International Airport","Batumi","3 h"],["Tbilisi","Batumi","6 h"],["Tbilisi","Zugdidi","6 h"],["Kutaisi International Airport","Batumi","3 h"],["Kutaisi International Airport","Tbilisi","4 h"]]) rr(from,to,"Omnibus Express","Route pair listed on TKT.GE; exact timetables not exposed",{CoverageType:"Booking-platform route listing",JourneyDurationText:dur,SourceTitle:sourceMap["tkt-omnibus"].SourceTitle,SourceUrl:sourceMap["tkt-omnibus"].SourceUrl,Notes:"Approximate duration from TKT.GE Omnibus page; duplicated same OD merged."});
for (const [from,to,op] of [["Tbilisi","Batumi","Metro Georgia"],["Batumi","Tbilisi","Metro Georgia"],["Tbilisi","Kutaisi","Metro Georgia"],["Tbilisi","Zugdidi","Metro Georgia"],["Tbilisi","Kutaisi International Airport","Metro Georgia"],["Kutaisi International Airport","Tbilisi","Metro Georgia"],["Batumi","Kutaisi International Airport","Metro Georgia"],["Kutaisi International Airport","Batumi","Metro Georgia"]]) rr(from,to,op,"Route listed by terminal/TKT sources; detailed schedule coverage incomplete",{CoverageType:"Terminal or platform route listing",JourneyDurationText:(from==="Tbilisi"&&to==="Batumi")||from==="Batumi"&&to==="Tbilisi"?"5–6 h":null,SourceTitle:sourceMap["terminal-destinations"].SourceTitle,SourceUrl:sourceMap["terminal-destinations"].SourceUrl,Notes:"Direction independently listed only where shown in sources; not all pairs are operator schedules."});
for (const [from,to,op,dur] of [["Tbilisi","Batumi","Citybus Georgia","6 h"],["Tbilisi","Kutaisi International Airport","Omnibus Express","4 h"],["Batumi","Kutaisi International Airport","Omnibus Express","3 h"],["Tbilisi","Zugdidi","Omnibus Express","6 h"],["Kutaisi International Airport","Batumi","Omnibus Express","3 h"],["Kutaisi International Airport","Tbilisi","Omnibus Express","4 h"]]) rr(from,to,op,"Route listing on Omnibus booking page",{CoverageType:"Booking-platform route listing",JourneyDurationText:dur,SourceTitle:sourceMap["tkt-omnibus"].SourceTitle,SourceUrl:sourceMap["tkt-omnibus"].SourceUrl});
for (const [to,dur,fare] of [["Batumi","5–6 h",35],["Kutaisi","3–4 h",18],["Zugdidi","5–6 h",30],["Telavi","2 h",15],["Gurjaani","1.3 h",10],["Kvareli","2.1 h",15],["Yerevan","5 h",35],["Istanbul","18 h",120]]) {
  const operators=to==="Batumi"?["Metro Georgia","Omnibus Express","Citybus Georgia"]:to==="Kutaisi"?["Omnibus Express","Citybus Georgia","Metro Georgia"]:to==="Zugdidi"?["Omnibus Express","Metro Georgia"]:to==="Istanbul"?["Luks Karadeniz","Metro Georgia","Obilet (platform)"]:to==="Yerevan"?["Carrier unresolved — TKT.GE listing"]:["Carrier unresolved — TKT.GE listing"];
  for(const op of operators) rr("Tbilisi",to,op,"Tbilisi Central destination listing; no full dated timetable on page",{CoverageType:to==="Istanbul"||to==="Yerevan"?"Cross-border terminal route lead":"Terminal destination listing",JourneyDurationText:dur,TicketPriceGEL:fare,SourceTitle:sourceMap["terminal-destinations"].SourceTitle,SourceUrl:sourceMap["terminal-destinations"].SourceUrl,Notes:to==="Istanbul"||to==="Yerevan"?"Cross-border route; operator association is from terminal listing, not a departure-specific timetable.":"Terminal starting fare and approximate duration; not operator-specific."});
}
rr("Tbilisi","Izmir","Metro Georgia","Terminal partner listing names Metro Georgia and Izmir; no dated coach trip captured",{CoverageType:"Cross-border terminal route lead",CountAsConfirmedRoute:false,SourceTitle:sourceMap["terminal-partners"].SourceTitle,SourceUrl:sourceMap["terminal-partners"].SourceUrl,Notes:"Terminal partner page lists Izmir under Metro Georgia; it does not show a dated departure or direction-specific schedule."});
rr("Tbilisi","Trabzon","Luks Karadeniz","Terminal partner listing names Trabzon; no dated coach trip captured",{CoverageType:"Cross-border terminal route lead",CountAsConfirmedRoute:false,SourceTitle:sourceMap["terminal-partners"].SourceTitle,SourceUrl:sourceMap["terminal-partners"].SourceUrl,Notes:"Terminal partner page lists Trabzon under Luks Karadeniz; it does not show a dated departure or direction-specific schedule."});
// Kutaisi Travel source routes without duplicating service records.
const kutaisiListings=[
 ["Kutaisi","Senaki","LTD Sprinter","Published route, return time TBC"],["Senaki","Kutaisi","Carrier not identified","Return direction not available in Kutaisi source; directory lead only"],
 ["Kutaisi","Zugdidi","LTD Sprinter","Scheduled exact departures; return time TBC"],["Kutaisi","Batumi","LTD Sprinter","Published frequency; reverse TBC"],["Batumi","Kutaisi","LTD Sprinter","Return timetable TBC"],
 ["Kutaisi","Chokhatauri","LTD Sprinter","Intermediate stop on Kutaisi–Ozurgeti line"],["Kutaisi","Ozurgeti","LTD Sprinter","Scheduled exact departures; return TBC"],
 ["Kutaisi","Ambrolauri","LTD Sprinter","Scheduled exact departures"],["Ambrolauri","Kutaisi","LTD Sprinter","Scheduled exact return departure"],
 ["Kutaisi","Oni","LTD Sprinter","Scheduled via Ambrolauri"],["Oni","Kutaisi","LTD Sprinter","Scheduled return via Ambrolauri"],
 ["Kutaisi","Martvili","Carrier not named — Kutaisi Central Bus Station service","Published frequency; carrier not identified"],["Kutaisi","Abastumani","Carrier not named — Kutaisi Central Bus Station service","Published exact departure; carrier not identified"],
 ["Kutaisi","Samtredia","LTD Sprinter","Published frequency"],["Samtredia","Kutaisi","LTD Sprinter","Published return frequency"],
 ["Kutaisi","Sachkhere","Carrier not named — Kutaisi Central Bus Station service","Published exact departures; station/dispatcher named, carrier unknown"],["Sachkhere","Kutaisi","Carrier not named — Kutaisi Central Bus Station service","Published exact return departures"],
 ["Kutaisi","Kharagauli","Carrier not named — Kutaisi Central Bus Station service","Published exact departures"],["Kharagauli","Kutaisi","Carrier not named — Kutaisi Central Bus Station service","Published exact return departures"],
 ["Kutaisi","Baghdati","Carrier not named — Kutaisi Central Bus Station service","Published frequency; demand-sensitive"],["Baghdati","Kutaisi","Carrier not named — Kutaisi Central Bus Station service","Published frequency; demand-sensitive"],
 ["Kutaisi","Vani","Carrier not named — Kutaisi Central Bus Station service","Published frequency"],["Vani","Kutaisi","Carrier not named — Kutaisi Central Bus Station service","Published return frequency"],
 ["Kutaisi","Terjola","Carrier not named — Kutaisi Central Bus Station service","Published exact departures"],["Terjola","Kutaisi","Carrier not named — Kutaisi Central Bus Station service","Published return frequency"],
 ["Kutaisi","Tkibuli","Carrier not named — Kutaisi Central Bus Station service","Published exact departures"],["Tkibuli","Kutaisi","Carrier not named — Kutaisi Central Bus Station service","Published return pattern"],
 ["Kutaisi","Khoni","Carrier not named — Khoni route","Published frequency; carrier not named"],["Khoni","Kutaisi","Carrier not named — Khoni route","Published return frequency"],
 ["Kutaisi","Gelati Monastery","Carrier not named — Gelati minibus","Local/tourist regional route"],["Gelati Monastery","Kutaisi","Carrier not named — Gelati minibus","Local/tourist regional return route"],
];
for(const [from,to,op,status] of kutaisiListings) rr(from,to,op,status,{CoverageType:"Regional minibus listing",SourceTitle:sourceMap["kutaisi-schedule"].SourceTitle,SourceUrl:sourceMap["kutaisi-schedule"].SourceUrl,Notes:"Page does not state a current effective date; frequency windows are not expanded."});
// Official public-agency/municipal route evidence.
rr("Tbilisi","Borjomi","Carrier not identified — Didube minibus service","Daily service window 08:00–18:00 published by national parks agency",{CoverageType:"Official visitor transport listing",ScheduleEvidence:"08:00–18:00 daily (window, individual departures not stated)",JourneyDurationText:"About 2 h 15 min",SourceTitle:sourceMap["nationalparks-borjomi"].SourceTitle,SourceUrl:sourceMap["nationalparks-borjomi"].SourceUrl,Notes:"Source gives Tbilisi Didube departure window; do not infer exact hourly services."});
rr("Tbilisi","Akhaltsikhe","Carrier not identified — Didube/Okriba minibus","Secondary guide reports roughly hourly 08:00–18:00; schedule described as fill-up based",{CoverageType:"Secondary route and schedule estimate",ScheduleEvidence:"Approx. hourly 08:00–18:00",TicketPriceGEL:15,JourneyDurationText:"3–4 h",SourceTitle:sourceMap["akhaltsikhe-guide"].SourceTitle,SourceUrl:sourceMap["akhaltsikhe-guide"].SourceUrl,Notes:"Secondary travel guide; no operator, return listing or exact trip times."});
rr("Tbilisi","Rustavi","Rustavi Intertrans LLC","Municipal source identifies corridor; budget reports 5 route lines and aggregate 130 buses/day",{CoverageType:"Municipal route listing",ScheduleEvidence:"Aggregate 5 lines / 130 buses per day (not per direction or line)",SourceTitle:sourceMap["rustavi-info"].SourceTitle,SourceUrl:sourceMap["rustavi-info"].SourceUrl,Notes:"No trip-level schedule or current fare located."});
rr("Rustavi","Tbilisi","Rustavi Intertrans LLC","Municipal source identifies corridor; budget reports 5 route lines and aggregate 130 buses/day",{CoverageType:"Municipal route listing",ScheduleEvidence:"Aggregate 5 lines / 130 buses per day (not per direction or line)",SourceTitle:sourceMap["rustavi-budget"].SourceTitle,SourceUrl:sourceMap["rustavi-budget"].SourceUrl,Notes:"Reverse corridor is included from municipal description; no direction-specific trip times found."});
for(const [from,to] of [["Tbilisi","Samtredia"],["Samtredia","Tbilisi"],["Tbilisi","Zugdidi"],["Zugdidi","Tbilisi"],["Tbilisi","Poti"],["Poti","Tbilisi"],["Tbilisi","Senaki"],["Senaki","Tbilisi"],["Tbilisi","Kobuleti"],["Kobuleti","Tbilisi"],["Tbilisi","Ureki"],["Ureki","Tbilisi"]]) rr(from,to,"TRE.GE route-search lead (carrier unresolved)","Platform destination link found; bus provider and dated bus result not verified",{CoverageType:"Booking-platform route lead",CountAsConfirmedRoute:false,SourceTitle:sourceMap["tre-georgianbus"].SourceTitle,SourceUrl:sourceMap["tre-georgianbus"].SourceUrl,Notes:"TRE destination hub links this city pair, but the linked page may show railway/mixed-mode data. Not counted as a verified bus service."});

// Current TRE provider profiles confirm route-network endpoints, but do not expose dated trips here.
for(const [from,to,operator,sourceId,note] of [
  ["Tbilisi","Kutaisi","Citybus Georgia","tre-citybus","TRE City Bus profile lists both cities in its intercity network; no dated city-pair departure captured."],
  ["Kutaisi","Tbilisi","Citybus Georgia","tre-citybus","TRE City Bus profile lists routes to and from these network cities; no dated return departure captured."],
  ["Tbilisi","Ureki","Citybus Georgia","tre-citybus","Provider-network listing only; date-specific schedule not captured."],
  ["Ureki","Tbilisi","Citybus Georgia","tre-citybus","Provider-network listing only; date-specific return schedule not captured."],
  ["Tbilisi","Kobuleti","Citybus Georgia","tre-citybus","Provider-network listing only; date-specific schedule not captured."],
  ["Kobuleti","Tbilisi","Citybus Georgia","tre-citybus","Provider-network listing only; date-specific return schedule not captured."],
  ["Tbilisi","Sarpi","Citybus Georgia","tre-citybus","Provider page lists Sarpi; an official Tbilisi–Sarpi date-specific route page is separately recorded."],
  ["Sarpi","Tbilisi","Citybus Georgia","tre-citybus","Provider-network listing only; date-specific return schedule not captured."],
  ["Tbilisi","Batumi","Omnibus Express","tre-omnibus","TRE OmniBus profile lists routes to and from Tbilisi and Batumi; no dated trip list captured."],
  ["Batumi","Tbilisi","Omnibus Express","tre-omnibus","TRE OmniBus profile lists routes to and from Tbilisi and Batumi; no dated trip list captured."],
  ["Tbilisi","Zugdidi","Omnibus Express","tre-omnibus","TRE OmniBus profile lists routes to and from Tbilisi and Zugdidi; no dated trip list captured."],
  ["Zugdidi","Tbilisi","Omnibus Express","tre-omnibus","TRE OmniBus profile lists routes to and from Tbilisi and Zugdidi; no dated return trip list captured."],
  ["Tbilisi","Kutaisi International Airport","Omnibus Express","tre-omnibus","TRE profile describes Kutaisi Airport transfers; route pair is also listed on TKT.GE."],
  ["Kutaisi International Airport","Tbilisi","Omnibus Express","tre-omnibus","TRE profile describes Kutaisi Airport transfers; route pair is also listed on TKT.GE."],
  ["Batumi","Kutaisi International Airport","Omnibus Express","tkt-omnibus","TKT.GE route list names this direction; dated departure not captured."],
  ["Kutaisi International Airport","Batumi","Omnibus Express","tkt-omnibus","TKT.GE route list names this direction; dated return not captured."],
  ["Tbilisi","Batumi","Georgian Bus","tre-georgianbus","TRE Georgian Bus profile lists both cities; dated timetable sample is separately recorded."],
  ["Batumi","Tbilisi","Georgian Bus","tre-georgianbus","TRE Georgian Bus profile lists routes to and from its destinations; no dated return timetable captured."],
  ["Tbilisi","Kobuleti","Georgian Bus","tre-georgianbus","Provider-network listing only; date-specific schedule not captured."],
  ["Kobuleti","Tbilisi","Georgian Bus","tre-georgianbus","Provider-network listing only; date-specific return schedule not captured."],
  ["Tbilisi","Gudauri","Georgian Bus","tre-georgianbus","Provider-network listing only; seasonal dates and departure times not captured on the provider profile."],
  ["Gudauri","Tbilisi","Georgian Bus","tre-georgianbus","Provider-network listing only; seasonal dates and return times not captured."],
  ["Kutaisi International Airport","Gudauri","Georgian Bus","gb-faq","Official FAQ lists this airport/Gudauri connection as winter service; no dates or timetable captured."],
  ["Gudauri","Kutaisi International Airport","Georgian Bus","gb-faq","Official FAQ lists this airport/Gudauri connection as winter service; no dates or timetable captured."],
]) rr(from,to,operator,"Provider route-network listing; dated departures not captured",{CoverageType:"Operator profile route-network listing",ScheduleEvidence:note,SourceTitle:sourceMap[sourceId].SourceTitle,SourceUrl:sourceMap[sourceId].SourceUrl,Notes:note});

// Citybus publishes directional fares on its own site; route-specific dated schedules are not available for every pair.
for(const [from,to,fare] of [["Tbilisi","Batumi",45],["Batumi","Tbilisi",45],["Tbilisi","Sarpi",55],["Sarpi","Tbilisi",55],["Tbilisi","Kutaisi",30],["Kutaisi","Tbilisi",30],["Tbilisi","Ureki",40],["Ureki","Tbilisi",40],["Tbilisi","Kobuleti",45],["Kobuleti","Tbilisi",45]]) rr(from,to,"Citybus Georgia","Operator route and fare published; not every direction has a dated trip captured",{CoverageType:"Operator-published route and fare listing",ScheduleEvidence:"Citybus public route list; dated schedule availability varies by pair",TicketPriceGEL:fare,SourceTitle:`${sourceMap["citybus-home"].SourceTitle} | ${sourceMap["tre-citybus"].SourceTitle}`,SourceUrl:`${sourceMap["citybus-home"].SourceUrl} | ${sourceMap["tre-citybus"].SourceUrl}`,Notes:`Citybus lists this direction and a ${fare} GEL fare; no dated trip listing is available here for every route. Follow the direction-specific timetable URL where present.`});

// Kutaisi Airport publishes route destinations and flight-linked timing policy but no carrier-specific trip rows.
for(const to of ["Tbilisi","Batumi","Zugdidi","Mestia","Stepantsminda/Kazbegi"]) rr("Kutaisi International Airport",to,"Carrier not identified — Kutaisi Airport minibus","Official airport route and flight-schedule information; exact trip/price and carrier not published",{CoverageType:"Official airport route-network listing",ScheduleEvidence:"Airport says minibuses run to these destinations and schedules follow flight times; exact trips are not listed",CountAsConfirmedRoute:true,SourceTitle:sourceMap["kutaisi-airport-transport"].SourceTitle,SourceUrl:sourceMap["kutaisi-airport-transport"].SourceUrl,Notes:"Airport information page names this destination direction and says timetables are issued according to flight schedules; carrier, fare and individual departure time are not given."});

// Regional route-directory lines: all currently exposed directional listing entries, carrier/timetable not guaranteed.
const directoryLines = JSON.parse(await fs.readFile(path.join(outDir,".regional-route-leads.json"),"utf8"));
const cityDirectory = JSON.parse(await fs.readFile(path.join(outDir,".regional-cities.json"),"utf8"));
const removeGeorgianParenthetical = (s) => s.replace(/\s*\([^)]*[\u10A0-\u10FF][^)]*\)/gu,"").trim();
const directoryCity = (s) => s.replace(/^\s*N\d+\s+/i,"").replace(/\s*\([^)]*\)/g,"").replace(/\s+Nikoloz$/i,"").trim();
const lineSegments = (label) => label.split(/\s+-\s+/).map(s=>s.trim());
const endpointOf = (s) => directoryCity(s);
const stationFromSegment = (s) => {
  const names=[...s.matchAll(/\(([^)]*)\)/g)].map(m=>m[1].trim());
  return names.find(n=>n && !/[\u10A0-\u10FF]/u.test(n)) || null;
};
for (const l of directoryLines) {
  const seg = lineSegments(l.RouteLabel);
  const from = endpointOf(seg[0] || l.RouteLabel);
  const to = endpointOf(seg.at(-1) || l.RouteLabel);
  const via = seg.length > 2 ? seg.slice(1,-1).map(endpointOf).join(" → ") : "";
  addRoute(from,to,"Carrier not identified — Georgian Marshrutka route directory","Third-party directory route line; operator, active status, fares and timetable not independently verified",{
    RouteVariant:l.RouteLabel,
    CoverageType:"Third-party regional route-directory lead",
    CountAsConfirmedRoute:false,
    ScheduleEvidence:"Not provided in route index",
    SourceTitle:sourceMap["marshrutka-lines"].SourceTitle,
    SourceUrl:l.SourceUrl,
    Notes:`Listed route: ${l.RouteLabel}${via?`; via: ${via}`:""}. Route directory page currently links this line; availability and direction not confirmed with carrier.`,
  });
}

// Read all accessible individual line pages. Exact listed departures become individual rows;
// frequency windows stay as a single pattern row and are never expanded into guessed trips.
const routeDetails = JSON.parse(await fs.readFile(path.join(outDir,".regional-route-detail-pages.json"),"utf8"));
const namedDirectoryOperators = new Map();
function sourceSection(text, heading, nextHeading) {
  const re = new RegExp(`${heading}\\s*:\\s*([\\s\\S]*?)(?=${nextHeading}\\s*:|Sources\\s*:|Propose changes|Please describe|$)`,"i");
  return text.match(re)?.[1]?.trim() || "";
}
function routeDays(text) {
  const dayText=text.match(/(?:Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)(?:\s*[-–]\s*(?:Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday))?(?:\s*,?\s*(?:Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday))*/i)?.[0];
  if(dayText) return dayText.replace(/\s+/g," ").trim();
  if(/except\s+Sunday/i.test(text)) return "Daily except Sunday (directory wording)";
  if(/Jun(?:e)?\s*[-–]\s*Aug(?:ust)?|summer|in season|high season|seasonal/i.test(text)) return "Seasonal; dates not fully specified";
  return null;
}
function scheduleIsFrequency(text) {
  return /\b(every|hourly|frequency|times? daily|when full|leave when|departures? are in the morning|from\s+\d{1,2}\s*(?:am|pm)\s+to)\b/i.test(text);
}
function normalizeHour(raw) {
  const m=raw.trim().match(/^(\d{1,2})(?::(\d{2}))?\s*(am|pm)?$/i);
  if(!m) return null;
  let h=Number(m[1]), minute=Number(m[2]||0), ap=m[3]?.toLowerCase();
  if(ap){ if(h===12) h=0; if(ap==="pm") h+=12; }
  if(h>23 || minute>59) return null;
  return `${String(h).padStart(2,"0")}:${String(minute).padStart(2,"0")}`;
}
function explicitTimes(text) {
  const times=[];
  for(const m of text.matchAll(/\b([01]?\d|2[0-3]):([0-5]\d)\b/g)) times.push(`${String(Number(m[1])).padStart(2,"0")}:${m[2]}`);
  return [...new Set(times)];
}
for (const page of routeDetails) {
  if(page.Status!==200) continue;
  const seg=lineSegments(page.RouteLabel);
  if(seg.length<2) continue;
  const from=endpointOf(seg[0]), to=endpointOf(seg.at(-1));
  if(!from || !to) continue;
  const fareSection=sourceSection(page.Text,"Price","Route");
  const fareMatch=fareSection.match(/\b(\d+(?:[.,]\d+)?)\s*GEL\b/i);
  const fare=fareMatch?Number(fareMatch[1].replace(",",".")):null;
  const schedule=sourceSection(page.Text,"Departures","Description");
  if(!schedule) continue;
  const times=explicitTimes(schedule);
  const isPattern=scheduleIsFrequency(schedule) || times.length===0;
  const intermediate=seg.slice(1,-1).map(endpointOf).filter(Boolean).join(" → ") || null;
  const departureStation=stationFromSegment(seg[0]);
  const description=sourceSection(page.Text,"Description","Sources");
  const fullSchedule=[schedule,description].filter(Boolean).join(" | ");
  let operator="Carrier not identified — Georgian Marshrutka route directory";
  if(/Tetritskaro Transport Company/i.test(page.Text)) {
    operator="Tetritskaro Transport Company Ltd.";
    namedDirectoryOperators.set(operator,true);
  }
  const namedDayPairs=[...schedule.matchAll(/Departure\s+(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)\s+(\d{1,2}:\d{2})/gi)];
  const specialDayRows=namedDayPairs.map(m=>({day:m[1],time:m[2]}));
  const notes=[
    `Individual route page: ${page.SourceUrl}`,
    intermediate?`Intermediate listed places: ${intermediate}`:null,
    description?`Directory description: ${description}`:null,
    /\b(?:2019|2020|2021|2022|2023|2024|2025)\b/.test(fullSchedule)?"Page explicitly references older schedule information; current applicability is unverified.":null,
    "Third-party community directory; carrier did not confirm this service unless the row names an explicitly reported operator."
  ].filter(Boolean).join(" ");
  const base={
    OperatorName:operator,DepartureCity:from,ArrivalCity:to,TicketPriceGEL:fare,
    OperatingDays:routeDays(fullSchedule),DepartureStation:departureStation,
    ViaStops:intermediate,SourceId:"marshrutka-line-details",SourceUrl:page.SourceUrl,
    SourceTitle:`Georgian Marshrutka line — ${page.RouteLabel}`,
    PriceSourceUrl:fare==null?null:page.SourceUrl,EvidenceStatus:"Third-party route detail; user-submitted/secondary schedule, operator and current service not verified",
    Notes:notes,RouteVariant:page.RouteLabel,CountAsConfirmedRoute:false
  };
  if(isPattern){
    const raw=fullSchedule.replace(/\s+/g," ").trim();
    addService({...base,SchedulePattern:raw.slice(0,250),OperatingDays:routeDays(fullSchedule)});
    continue;
  }
  const rows=specialDayRows.length?specialDayRows.map(x=>({time:normalizeHour(x.time),day:x.day})):times.map(time=>({time,day:routeDays(fullSchedule)}));
  for(const row of rows){
    if(!row.time) continue;
    const duplicate=serviceRows.find(s=>s.DepartureCity.toLowerCase()===from.toLowerCase()&&s.ArrivalCity.toLowerCase()===to.toLowerCase()&&s.DepartureTime===row.time&&s.TicketPriceGEL===fare&&s.SchedulePattern==null);
    if(duplicate){
      const urls=(duplicate.SupportingSourceUrl||"").split(" | ").filter(Boolean);
      if(!urls.includes(page.SourceUrl)) duplicate.SupportingSourceUrl=[...urls,page.SourceUrl].join(" | ");
      duplicate.Notes=[duplicate.Notes,`Matching time/fare also appears in third-party route page ${page.SourceUrl}; likely duplicate, retained as corroboration only.`].filter(Boolean).join(" ");
      continue;
    }
    addService({...base,DepartureTime:row.time,OperatingDays:row.day||base.OperatingDays});
  }
}

const operators = [
  ["Citybus Georgia","Domestic intercity bus operator","Georgia","Confirmed operator; official schedule pages accessible","Tbilisi–Batumi, Batumi–Tbilisi, Sarpi, Kutaisi, Ureki, Kobuleti; some stop-level services","Official fares and dated/undated stop-level schedule pages."],
  ["Metro Georgia","Domestic and international coach operator","Georgia and cross-border","Confirmed operator; official dated booking samples accessible","Tbilisi–Batumi both directions; terminal route leads to Kutaisi, Zugdidi and international Istanbul/Izmir","Only one dated direction sample from September and one from November captured; other route schedules unresolved."],
  ["Omnibus Express","Domestic intercity and airport-transfer operator","Georgia","Confirmed operator; TKT route listings, terminal times and social mirror","Tbilisi–Batumi, Tbilisi–Zugdidi, Tbilisi/Batumi–Kutaisi Airport, Kutaisi Airport–Tbilisi/Batumi","Official operator site was not machine-readable in accessible source; exact schedule coverage remains partial."],
  ["Georgian Bus","Intercity and airport-transfer operator","Georgia","Confirmed operator; official FAQ and dated timetable samples","Tbilisi/Kutaisi Airport, Batumi/Kutaisi Airport, Tbilisi/Batumi, Tbilisi/Kutaisi; seasonal Gudauri–airport","Airport times are flight-linked; full date-by-date timetable and current fare were not captured."],
  ["LTD Okriba","Regional minibus provider","Imereti / Tbilisi","Named in regional timetable; operator website not found","Kutaisi–Tbilisi and return","Frequency windows and GEL 20 fare; page effective date unspecified."],
  ["LTD Imertrans","Regional minibus provider","Imereti / Tbilisi / Adjara","Named in regional timetable; operator website not found","Kutaisi–Tbilisi, Tbilisi–Kutaisi, Kutaisi–Batumi and return","Some exact departures and frequency windows; date unspecified."],
  ["LTD Sprinter","Regional minibus provider","Western Georgia / Svaneti","Named in regional timetable; operator website not found","Kutaisi–Batumi, Zugdidi, Senaki, Mestia, Ozurgeti, Ambrolauri, Oni, Samtredia, Tskaltubo","Many route/time entries are published by Kutaisi Travel; effective date unspecified."],
  ["Rustavi Intertrans LLC","Municipal intercity corridor operator","Tbilisi–Rustavi","Named by Rustavi Municipality; detailed public timetable not found","Tbilisi–Rustavi corridor (five municipal route lines per budget)","Aggregate budget figure only; do not allocate the reported 130 bus movements to individual directions or lines."],
  ["Tetritskaro Transport Company Ltd.","Regional minibus company name reported by route directory","Tbilisi–Manglisi / Tetritskaro area","Named in two community route descriptions; no separate official company or schedule confirmation located","Tbilisi–Manglisi and Manglisi–Tbilisi; related Tetritsqaro corridor lead","Counted as a discovered company name, but carrier identity and service are not independently verified. Exact route observations remain third-party directory data."],
  ["Luks Karadeniz","International coach operator","Georgia–Turkey","Named on Tbilisi Central terminal destination and partner pages","Tbilisi–Istanbul, Ankara, Antalya and Trabzon leads","No accessible trip-level schedule captured."],
  ["Luks Muradoglu","International coach operator","Georgia–Ukraine","Named on Tbilisi Central terminal destination page","Tbilisi–Kyiv lead","No accessible trip-level schedule captured."],
  ["Grand Tours","International coach operator","Georgia–Greece","Named on Tbilisi Central terminal destination page","Tbilisi–Thessaloniki lead","No accessible trip-level schedule captured."],
  ["Green Express","International coach operator","Georgia–Greece","Named on Tbilisi Central terminal destination page","Tbilisi–Athens lead","No accessible trip-level schedule captured."],
  ["Global Trans","International coach operator","Georgia–Russia","Named on Tbilisi Central terminal destination page","Tbilisi–Saint Petersburg lead","No accessible trip-level schedule captured."],
  ["ClassTour","Provider label / identity unresolved","Unknown","Named in TKT transport-provider FAQ; no route attribution verified","None confirmed","Not counted as a discovered transport company until a service/route can be tied to it."],
  ["Carrier not named — Kutaisi Central Bus Station service","Station/dispatcher label, not verified carrier","Kutaisi and Imereti","Several regional schedule rows label the station as provider","Kutaisi–Zugdidi, Martvili, Abastumani, Sachkhere, Kharagauli, Baghdati, Vani, Terjola, Tkibuli","The source does not identify the vehicle operator; not counted as a transport company."],
  ["TRE.GE / TKT.GE","Ticketing platform, not transport operator","Georgia and cross-border listings","Platform verified; ticket.ge redirects to tre.ge","Route links and provider names vary by page","Several pages mix rail and bus data; no carrier attribution from generic platform links."],
  ["Obilet","Ticketing platform, not transport operator","Cross-border listing","Named as a booking partner on terminal page","Tbilisi–Istanbul route listing","Not counted as a carrier."],
  ["Carrier not identified — Georgian Marshrutka route directory","Unidentified service label, not a company","Georgia","249 individual route pages are publicly linked; no general carrier register exposed","Community route-detail pages with a fare and/or timetable","Not counted as a transport company. Availability and carrier identity vary by line; details may be stale."],
];
const operatorRows = operators.map(([OperatorName,OperatorType,GeographicScope,DiscoveryStatus,RoutesFound,Notes],i)=>({OperatorId:i+1,OperatorName,OperatorType,GeographicScope,DiscoveryStatus,RoutesFound,CountedAsTransportCompany:i<14,Notes,SourceCheckedDate:checkedOn}));

// Destination directory includes 146 nodes; append nodes visible in schedule and international route listings.
const destinationMap = new Map();
for (const c of cityDirectory) {
  const name=removeGeorgianParenthetical(c.CityLabel);
  destinationMap.set(name,{Destination:name,OriginalLabel:c.CityLabel,DirectorySource:"Georgian Marshrutka city directory",SourceUrl:c.SourceUrl,Scope:"Regional city/town node listed in directory"});
}
const extraDestinations=[
 ["Kutaisi International Airport","Kopitnari / airport","Flight-linked and bus route listings"],["Batumi Airport","Batumi airport stop","Citybus stop-level timetable"],["Gonio","Gonio","Citybus stop-level timetable"],["Kvariati","Kvariati","Citybus stop-level timetable"],["Istanbul","Istanbul","Cross-border terminal listing"],["Izmir","Izmir","Cross-border terminal partner listing"],["Trabzon","Trabzon","Cross-border terminal partner listing"],["Yerevan","Yerevan","Cross-border terminal listing"],["Ankara","Ankara","Cross-border terminal directory listing"],["Antalya","Antalya","Cross-border terminal directory listing"],["Kyiv","Kyiv","Cross-border terminal directory listing"],["Thessaloniki","Thessaloniki","Cross-border terminal directory listing"],["Athens","Athens","Cross-border terminal directory listing"],["Saint Petersburg","Saint Petersburg","Cross-border terminal directory listing"],
];
for(const [name,label,scope] of extraDestinations) if(!destinationMap.has(name)) destinationMap.set(name,{Destination:name,OriginalLabel:label,DirectorySource:scope,SourceUrl:sourceMap["terminal-destinations"].SourceUrl,Scope:"Airport or international destination"});
for(const s of serviceRows) for(const name of [s.DepartureCity,s.ArrivalCity]) if(name&&!destinationMap.has(name)) destinationMap.set(name,{Destination:name,OriginalLabel:name,DirectorySource:"Schedule observation",SourceUrl:s.SourceUrl,Scope:"Service origin/destination"});
for(const r of routeRows) for(const name of [r.DepartureCity,r.ArrivalCity]) if(name&&!destinationMap.has(name)) destinationMap.set(name,{Destination:name,OriginalLabel:name,DirectorySource:"Route coverage listing",SourceUrl:r.SourceUrl?.split(" | ")[0]||"",Scope:"Published route or route lead endpoint"});
const destinationRows=[...destinationMap.values()].sort((a,b)=>a.Destination.localeCompare(b.Destination)).map((x,i)=>({DestinationId:i+1,...x}));

const explicitTimed = serviceRows.filter(s=>s.DepartureTime).length;
const patternRows = serviceRows.filter(s=>s.SchedulePattern).length;
const scheduleTuple=(s)=>[s.OperatorName,s.DepartureCity,s.ArrivalCity,s.DepartureTime,s.OperatingDays,s.OperatingDates,s.TicketPriceGEL,s.DepartureStation,s.ArrivalStation,s.SchedulePattern].join("|");
const scheduleTupleCounts=new Map();
for(const s of serviceRows) scheduleTupleCounts.set(scheduleTuple(s),(scheduleTupleCounts.get(scheduleTuple(s))||0)+1);
const duplicateScheduleTuples=[...scheduleTupleCounts.values()].reduce((n,c)=>n+Math.max(0,c-1),0);
const verifiedOd = new Set(routeRows.filter(r=>r.CountAsConfirmedRoute).map(r=>`${r.DepartureCity}→${r.ArrivalCity}`));
const allOd = new Set(routeRows.map(r=>`${r.DepartureCity}→${r.ArrivalCity}`));
const routeDirOd = new Set(routeRows.filter(r=>r.CoverageType==="Third-party regional route-directory lead").map(r=>`${r.DepartureCity}→${r.ArrivalCity}`));
const stats = {
  researchAsOf:checkedOn,
  transportCompaniesDiscovered:14,
  domesticOrRegionalTransportProviders:9,
  internationalOperatorsListed:5,
  destinationNodesInRegionalDirectory:cityDirectory.length,
  destinationNodesInBundle:destinationRows.length,
  scheduledServiceRecords:serviceRows.length,
  explicitTimedDepartureRecords:explicitTimed,
  frequencyPatternRecords:patternRows,
  serviceRecordsMissingFare:serviceRows.filter(s=>s.TicketPriceGEL==null).length,
  serviceRecordsMissingArrivalTime:serviceRows.filter(s=>!s.ArrivalTime).length,
  serviceRecordsMissingDuration:serviceRows.filter(s=>s.DurationMinutes==null&&!s.JourneyDurationText).length,
  serviceRecordsMissingBothStations:serviceRows.filter(s=>!s.DepartureStation&&!s.ArrivalStation).length,
  serviceRecordsMissingDaysAndDates:serviceRows.filter(s=>!s.OperatingDays&&!s.OperatingDates).length,
  duplicateScheduleTupleRows:duplicateScheduleTuples,
  sourceBackedServiceRecords:serviceRows.filter(s=>!s.Synthetic).length,
  primaryOrInstitutionalSourceRecords:serviceRows.filter(s=>!s.SourceUrl.includes("marshrutka.info/line/")&&!/tbilisitrip|thegeorgianguide|wander-lush/i.test(s.SourceUrl)).length,
  thirdPartyLineDirectoryServiceRecords:serviceRows.filter(s=>s.SourceUrl.includes("marshrutka.info/line/")).length,
  otherSecondarySourceRecords:serviceRows.filter(s=>/tbilisitrip|thegeorgianguide|wander-lush/i.test(s.SourceUrl)).length,
  syntheticServiceRecords:serviceRows.filter(s=>s.Synthetic).length,
  confirmedDirectionalOdPairs:[...verifiedOd].length,
  totalDirectionalOdPairsIncludingRouteLeads:[...allOd].length,
  regionalRouteDirectoryLineRecords:directoryLines.length,
  regionalRouteDirectoryDetailPagesFetched:routeDetails.filter(p=>p.Status===200).length,
  regionalRouteDetailPagesWithFare:routeDetails.filter(p=>/Price\s*:/i.test(p.Text)).length,
  regionalRouteDetailPagesWithDepartureSection:routeDetails.filter(p=>/Departures\s*:/i.test(p.Text)).length,
  regionalRouteDirectoryUniqueDirectionalOdPairs:[...routeDirOd].length,
  routeCoverageRows:routeRows.length,
};

const schema = [
  ["ServiceId","int identity primary key","Row identifier"],["RouteCode","nvarchar(100) null","Operator trip/route code if published"],["OperatorName","nvarchar(200) not null","Carrier name or explicit unresolved provider label"],["DepartureCity","nvarchar(150) not null","Origin"],["ArrivalCity","nvarchar(150) not null","Destination"],["DepartureTime","time null","Local Georgia time; null when only a frequency window is published"],["ArrivalTime","time null","Local Georgia time, if listed"],["ArrivalDayOffset","tinyint null","0 same calendar day; 1 next day, calculated from listed clocks"],["DurationMinutes","int null","Calculated from listed departure/arrival clocks when both exist"],["JourneyDurationText","nvarchar(80) null","Range/estimate or duration calculation note"],["TicketPriceGEL","decimal(8,2) null","Per ticket in GEL; null when source does not publish an amount"],["OperatingDays","nvarchar(200) null","Days/frequency context copied from source"],["OperatingDates","nvarchar(250) null","Date range/sample exactly or cautiously normalized from source"],["DepartureStation","nvarchar(250) null","Origin boarding place"],["ArrivalStation","nvarchar(250) null","Destination stop/station"],["ViaStops","nvarchar(max) null","Published intermediate stops, in route order where available"],["SchedulePattern","nvarchar(250) null","Published frequency window; not expanded into invented trips"],["ScheduleRecordType","nvarchar(50) not null","Published departure or frequency pattern"],["EvidenceStatus","nvarchar(250) not null","Source quality, date applicability and verification limits"],["SourceTitle","nvarchar(250) not null","Primary source record title"],["SourceUrl","nvarchar(1000) not null","Direct URL for schedule evidence"],["PriceSourceUrl","nvarchar(1000) null","Source for fare if distinct"],["SupportingSourceUrl","nvarchar(1000) null","Additional context, such as secondary fare corroboration"],["SourceCheckedDate","date not null","Research date"],["Notes","nvarchar(max) null","Material limitations and clarifying context"]
];
const bundle={DatasetTitle:"Georgian Intercity Bus Routes — Source-backed collection",SchemaVersion:"1.0",ResearchAsOf:checkedOn,Scope:"Georgian domestic intercity and regional bus/minibus routes, airport shuttles, and selected cross-border terminal listings. Schedule observations are a dated/undated research snapshot, not a live feed.",Statistics:stats,SqlServerSingleTableSchema:schema.map(([Name,SqlType,Meaning])=>({Name,SqlType,Meaning})),Services:serviceRows,RouteCoverage:routeRows,Operators:operatorRows,Sources:Object.values(sourceMap),Destinations:destinationRows,Limitations:[
  "No C# solution/project, SQL Server connection details, or existing table schema were present in the workspace; the live database was not modified. The SQL export defines a generic flat table for review/import.",
  "The supplied ticket.ge URL redirects to TRE.GE. Current TRE provider profiles expose operator route networks, but dynamic date searches do not expose the full all-date timetable; train/bus results may mix on destination pages.",
  "Citybus, Metro Georgia and Georgian Bus publish dated samples, but not every date and route combination was exhaustively queried. Historical sample schedules remain labeled as such.",
  "The Tbilisi terminal schedule is undated and includes a 07:30 Citybus Batumi listing that conflicts with the operator's 08:00 route page. It is retained with an explicit conflict warning.",
  "Kutaisi Travel publishes detailed regional times/fares, but the timetable page does not state an effective date. Frequency windows are stored as patterns and not expanded into individual invented departures.",
  "Marshrutka.info exposes 249 route links in current HTML while its visible heading says 246, and lists 146 city/town nodes. These are third-party route leads; active operation, operator identity and trip schedules are unverified.",
  "Mestia municipality's one-page schedule has no effective date. ProSvaneti is a secondary guide. Rustavi budget figures are aggregate and cannot be allocated to individual trip rows.",
  "International route listings from Tbilisi Central are not full departure schedules; cross-border operator names are included for discovery, but actual carrier/date availability was not verified.",
  "No CAPTCHA, login, or access restriction was bypassed. Some booking interfaces are dynamic and did not expose results in the readable public page extract.",
  "No synthetic service/test records were created. Missing ticket prices, times, durations, stations, days or dates remain null rather than being estimated unless a source explicitly supplied a range.",
]};

await fs.writeFile(path.join(outDir,"georgian-intercity-routes.json"),JSON.stringify(bundle,null,2)+"\n","utf8");

// SQL Server seed: one flat table. Status/provenance columns travel with each record.
const sqlType = Object.fromEntries(schema.map(([n,t])=>[n,t.split(" ")[0]]));
const sqlColumns=schema.map(([n])=>n);
const q=(v)=>v==null?"NULL":typeof v==="number"?String(v):`N'${String(v).replace(/'/g,"''")}'`;
const sql=[
  "/* Generic SQL Server single-table import. This is not mapped to an existing app schema. */",
  "IF OBJECT_ID(N'dbo.GeorgianIntercityBusServices', N'U') IS NULL",
  "BEGIN",
  "  CREATE TABLE dbo.GeorgianIntercityBusServices (",
  "    ServiceId int IDENTITY(1,1) NOT NULL PRIMARY KEY,",
  ...schema.slice(1).map(([n,t])=>{
    const baseType=t.split(/\s+/)[0];
    const nullable=/\bnull\b/.test(t) && !/\bnot null\b/.test(t);
    return `    ${n} ${baseType}${nullable?" NULL":" NOT NULL"}${n===schema.at(-1)[0]?"":","}`;
  }),
  "  );",
  "END;",
  "GO",
  "/* All inserted rows are source-backed schedule observations/patterns; no synthetic rows. */",
  "INSERT INTO dbo.GeorgianIntercityBusServices ("+sqlColumns.slice(1).join(", ")+") VALUES",
  serviceRows.map(s=>"("+sqlColumns.slice(1).map(c=>{
    if(c==="DepartureTime"||c==="ArrivalTime") return s[c]?`CONVERT(time(0), ${q(s[c])})`:"NULL";
    if(c==="SourceCheckedDate") return `CONVERT(date, ${q(s[c])})`;
    return q(s[c]);
  }).join(", ")+")").join(",\n"),
  ";",
].join("\n");
await fs.writeFile(path.join(outDir,"georgian-intercity-services.sql"),sql+"\n","utf8");

const readme=`# Georgian Intercity Bus Routes — research snapshot\n\n**Research date:** ${checkedOn}  \n**Use:** Importable, single-table schedule observations plus a much broader route/operator coverage inventory. This is a source snapshot, not a live timetable.\n\n## Deliverables\n\n- \`georgian-intercity-routes.xlsx\` — Summary, Services, Route Coverage, Operators, Sources and Destinations sheets.\n- \`georgian-intercity-routes.json\` — same data, provenance and limitations in structured form.\n- \`georgian-intercity-services.sql\` — SQL Server CREATE TABLE and seed INSERT for a generic flat services table.\n\n## Coverage counts\n\n| Measure | Count |\n|---|---:|\n| Named transport companies/operators discovered | ${stats.transportCompaniesDiscovered} |\n| Georgian domestic/regional providers among them | ${stats.domesticOrRegionalTransportProviders} |\n| International operators listed by the Tbilisi terminal | ${stats.internationalOperatorsListed} |\n| Destination nodes listed by the regional route directory | ${stats.destinationNodesInRegionalDirectory} |\n| Destination nodes in this bundle (including airports and cross-border destinations) | ${stats.destinationNodesInBundle} |\n| Source-backed service records (timed departures and published frequency patterns) | ${stats.sourceBackedServiceRecords} |\n| Primary/institutional source records | ${stats.primaryOrInstitutionalSourceRecords} |\n| Third-party line-detail schedule records (not operator-confirmed) | ${stats.thirdPartyLineDirectoryServiceRecords} |\n| Other secondary-source schedule records | ${stats.otherSecondarySourceRecords} |\n| Service rows missing fare | ${stats.serviceRecordsMissingFare} |\n| Service rows missing arrival time | ${stats.serviceRecordsMissingArrivalTime} |\n| Service rows missing duration | ${stats.serviceRecordsMissingDuration} |\n| Service rows missing both origin and arrival stations | ${stats.serviceRecordsMissingBothStations} |\n| Service rows missing both operating days and dates | ${stats.serviceRecordsMissingDaysAndDates} |\n| Exact duplicate schedule tuples | ${stats.duplicateScheduleTupleRows} |\n| Explicit departure-time records | ${stats.explicitTimedDepartureRecords} |\n| Published frequency-pattern records, not expanded into guessed trips | ${stats.frequencyPatternRecords} |\n| Synthetic service records | ${stats.syntheticServiceRecords} |\n| Confirmed directional OD pairs (route evidence, any named operator) | ${stats.confirmedDirectionalOdPairs} |\n| Total directional OD pairs when route-directory leads are included | ${stats.totalDirectionalOdPairsIncludingRouteLeads} |\n| Regional directory line entries retained | ${stats.regionalRouteDirectoryLineRecords} |\n| Individual route-detail pages fetched successfully | ${stats.regionalRouteDirectoryDetailPagesFetched} |\n| Line pages with fare / departure section | ${stats.regionalRouteDetailPagesWithFare} / ${stats.regionalRouteDetailPagesWithDepartureSection} |\n| Unique OD pairs among those directory lines | ${stats.regionalRouteDirectoryUniqueDirectionalOdPairs} |\n\nThe route directory advertises 246 lines in its heading; its currently accessible HTML yielded ${stats.regionalRouteDirectoryLineRecords} unique line links. All linked detail pages were fetched successfully. Pages with fares and departure sections are represented as direct-time rows or frequency patterns with individual page URLs, but remain third-party, community-submitted observations rather than operator-verified active trips. Every index link also remains in Route Coverage. Directional OD counts deduplicate the same origin/destination pair while retaining separate operator rows and route variants in the workbook.\n\n## Operators found\n\nDomestic and regional names: Citybus Georgia, Metro Georgia, Omnibus Express, Georgian Bus, LTD Okriba, LTD Imertrans, LTD Sprinter, Rustavi Intertrans LLC and Tetritskaro Transport Company Ltd. (this name appears only in community route descriptions and is not independently verified). Metro also appears on the terminal list for Istanbul and Izmir. Five foreign-carrier names appear in terminal listings: Luks Karadeniz, Luks Muradoglu, Grand Tours, Green Express and Global Trans. TKT.GE/TRE.GE and Obilet are booking platforms, not carriers. ClassTour was named in a provider FAQ but no route could be tied to it. Station/dispatcher labels and ‘carrier not identified’ are not counted as companies. Easy Way's public schedule is for parcel delivery, not passenger transport.\n\n## Data rules\n\n- Every row in **Services** has a public source URL and an explicit evidence status. Operator competition is kept in separate rows.\n- Exact departure times are separate rows. Source frequency windows are kept in \`SchedulePattern\` rows; the interval is not expanded into synthetic departures.\n- Opposite directions are entered only when a source lists them. A return is not inferred from a one-way route.\n- Arrival day offset and duration minutes are calculated only when a source lists both departure and arrival times. Approximate ranges remain text.\n- Missing fare, station, duration, days or dates are null. Airport, regional and cross-border routes are flagged in route coverage.\n- “Source-backed” means the value appears in a cited public source. It does not guarantee that an old or undated listing still operates today. Historical observations are labeled.\n- No synthetic records were created.\n\n## Important limitations\n\n- The workspace contained no C# solution/project, database connection details, or existing SQL table definition. I could not write to the user's live SQL Server database or preserve an unseen schema. The SQL file is a generic single-table import candidate; map/rename it to the application's actual CRUD table after checking its schema.\n- The supplied ticket.ge URL redirects to TRE.GE. TRE provider profiles expose current destination networks, but its dynamic date search does not provide a full all-date timetable in static page text. Its route links are recorded as leads where carrier/service identity is unresolved.\n- Citybus, Metro Georgia and Georgian Bus have useful dated route listings, but not every route/date variation could be exhaustively queried. Metro and Georgian Bus examples include past dates; dates are retained per row.\n- The terminal's undated schedule snapshot lists Citybus Tbilisi–Batumi at 07:30, while the operator route page lists 08:00. The terminal row is retained with a conflict warning; use the operator page for operational confirmation.\n- Okriba's official schedule lists times under its Kutaisi branch but does not label the destination; Kutaisi Travel identifies Okriba's Kutaisi–Tbilisi service and fare but publishes a different frequency window. Both observations and the mismatch are retained. Kutaisi Travel provides other detailed regional times/fare windows but does not state an effective date. The Mestia municipal PDF is also undated. Confirm these schedules before passenger-facing publication.\n- Rustavi's 2026 municipal budget gives an aggregate five-line/130-bus-per-day figure, not time-by-time departures. No per-trip timetable was inferred.\n- Tbilisi Central lists international operators and starting prices, but trip-level current dates and schedules were not obtained. Cross-border rows are route leads, not a complete verified timetable.\n- Public dynamic booking forms did not render a reliable all-date listing in the accessible page text. No CAPTCHA, authentication or access restriction was bypassed.\n\n## Source register\n\n- [Citybus Georgia official site](https://citybus.ge/en) and its route schedule pages linked per service row.\n- [Metro Georgia Tbilisi–Batumi](https://metrogeorgia.ge/otobus-bileti/TIFLIS-BATUM), [Batumi–Tbilisi](https://metrogeorgia.ge/otobus-bileti/BATUM-TIFLIS), and [official FAQ](https://www.metrogeorgia.ge/sss).\n- [Georgian Bus timetable](https://georgianbus.com/en/page/timetable) and [routes/FAQ](https://georgianbus.com/en/frequently-asked-questions).\n- [TKT.GE Omnibus route page](https://tkt.ge/en/omnibus), [ticket.ge (redirects to TRE.GE)](https://ticket.ge/), [TRE.GE City Bus](https://tre.ge/en/citybus), [OmniBus](https://tre.ge/en/omnibus) and [Georgian Bus](https://tre.ge/en/georgianbus) provider profiles.\n- [Tbilisi Central schedule](https://busterminal.ge/en/schedule/), [destinations](https://busterminal.ge/en/destinations/) and [transport partners](https://busterminal.ge/en/partners/).\n- [Okriba official schedule](https://okriba.ge/en/branches), [Kutaisi regional transport schedule](https://kutaisi.travel/en/transport-schedule-en/) and [Kutaisi Airport public transport](https://kutaisiairport.ge/transport/).\n- [Mestia Municipality schedule PDF](https://mestia.gov.ge/sites/default/files/sakalaktashoriso_transporti.pdf) and [ProSvaneti schedule guide](https://prosvaneti.ge/en/blog/marshrutka-to-mestia).\n- [Georgia Marshrutka route directory](https://marshrutka.info/lines) and [city directory](https://marshrutka.info/cities). [Easy Way parcel delivery zones](https://www.easyway.ge/en/schedule) were checked and excluded as non-passenger service.\n- [National Parks transport information for Borjomi](https://www.nationalparks.ge/en/master/transportation/borjomi-kharagaulinp), [Rustavi Municipality](https://rustavi.gov.ge/sachiro-informacia/) and its [2026–2029 budget](https://rustavi.gov.ge/wp-content/uploads/2025/11/3-2026-%E1%83%AC%E1%83%9A%E1%83%98%E1%83%A1-%E1%83%9E%E1%83%A0%E1%83%98%E1%83%9D%E1%83%A0%E1%83%98%E1%83%A2%E1%83%94%E1%83%A2%E1%83%94%E1%83%91%E1%83%98%E1%83%A1-%E1%83%93%E1%83%9D%E1%83%99%E1%83%A3%E1%83%9B%E1%83%94%E1%83%9c%E1%83%A2%E1%83%98.pdf).\n\n## SQL Server integration\n\nThe SQL seed creates one table, \`dbo.GeorgianIntercityBusServices\`, with nullable times/fares and in-row provenance. It is compatible with a simple flat CRUD workflow, but it is **not** an insert into the absent application's table. Keep \`EvidenceStatus\` in the app if you import these rows so historical, undated, and secondary observations remain distinguishable.\n`;
await fs.writeFile(path.join(outDir,"README.md"),readme,"utf8");

// Excel workbook, using the bundled artifact runtime.
const workbook=Workbook.create();
const sheetSpecs=[
  ["Summary",[["Metric","Value","Meaning"],
    ["Research as of",checkedOn,"Dates in sources are retained; this workbook is not live."],
    ["Transport companies discovered",stats.transportCompaniesDiscovered,"9 domestic/regional names; 5 foreign terminal names. Metro also serves cross-border routes; Tetritskaro is directory-reported only."],
    ["Destinations in directory",stats.destinationNodesInRegionalDirectory,"146 city/town nodes listed; not all have verified active trips."],
    ["Destination nodes in bundle",stats.destinationNodesInBundle,"Includes airports and international destinations."],
    ["Scheduled service records",stats.scheduledServiceRecords,"Source-backed rows; exact departures plus frequency patterns."],
    ["Explicit timed departures",stats.explicitTimedDepartureRecords,"Each distinct published departure time is a separate row."],
    ["Frequency-pattern records",stats.frequencyPatternRecords,"Published windows retained as text; not expanded into trips."],
    ["Source-backed records (not synthetic)",stats.sourceBackedServiceRecords,"Includes third-party route-page observations; this does not confirm current operation."],
    ["Official/institutional source rows",stats.primaryOrInstitutionalSourceRecords,"Operator, municipal, terminal, institutional and non-guide platform evidence."],
    ["Third-party line-detail schedule rows",stats.thirdPartyLineDirectoryServiceRecords,"Community-submitted; operator and current operation not verified."],
    ["Other secondary-source schedule rows",stats.otherSecondarySourceRecords,"Secondary guide evidence; confirm with the carrier."],
    ["Synthetic records",stats.syntheticServiceRecords,"None created."],
    ["Records missing ticket price",stats.serviceRecordsMissingFare,"Source did not publish a fare."],
    ["Records missing arrival time",stats.serviceRecordsMissingArrivalTime,"Source published a departure or frequency only."],
    ["Records missing duration",stats.serviceRecordsMissingDuration,"No arrival clock or duration estimate was published."],
    ["Records missing both stations",stats.serviceRecordsMissingBothStations,"Neither origin nor destination boarding stop was published."],
    ["Records missing operating days and dates",stats.serviceRecordsMissingDaysAndDates,"Neither days nor a date range was present in the source."],
    ["Exact duplicate schedule tuples",stats.duplicateScheduleTupleRows,"No duplicate operator/OD/time/days/date/fare/station/pattern combinations found."],
    ["Confirmed directional OD pairs",stats.confirmedDirectionalOdPairs,"At least one source identifies a route in that direction."],
    ["All directional OD pairs incl. route leads",stats.totalDirectionalOdPairsIncludingRouteLeads,"Includes unverified third-party directory links."],
    ["Regional directory line links",stats.regionalRouteDirectoryLineRecords,"Page heading says 246; accessible HTML had 249 links."],
    ["Regional detail pages fetched",stats.regionalRouteDirectoryDetailPagesFetched,"Individual public line pages examined."],
    ["Regional pages with fare / departure section",`${stats.regionalRouteDetailPagesWithFare} / ${stats.regionalRouteDetailPagesWithDepartureSection}`,"Line detail page counts; these may be stale or user-submitted."],
    ["Directory unique OD pairs",stats.regionalRouteDirectoryUniqueDirectionalOdPairs,"Distinct endpoints within the directory lines."],
    ["SQL Server target", "Generic single table only","No C# project or existing SQL schema was present; live database not modified."],
  ]],
  ["Services",[Object.keys(serviceRows[0]),...serviceRows.map(s=>Object.values(s))]],
  ["Route Coverage",[Object.keys(routeRows[0]),...routeRows.map(r=>Object.values(r).map(v=>Array.isArray(v)?v.join(", "):v))]],
  ["Operators",[Object.keys(operatorRows[0]),...operatorRows.map(o=>Object.values(o))]],
  ["Sources",[["SourceId","SourceTitle","SourceUrl","SourceType","Limitations","CheckedOn"],...Object.values(sourceMap).map(s=>Object.values(s))]],
  ["Destinations",[Object.keys(destinationRows[0]),...destinationRows.map(d=>Object.values(d))]],
];
for(const [name,matrix] of sheetSpecs){
  const sheet=workbook.worksheets.add(name);
  sheet.showGridLines=false;
  const range=sheet.getRangeByIndexes(0,0,matrix.length,matrix[0].length);
  range.values=matrix;
  const header=sheet.getRangeByIndexes(0,0,1,matrix[0].length);
  header.format.fill="#17365D";
  header.format.font={bold:true,color:"#FFFFFF"};
  header.format.wrapText=true;
  header.format.rowHeight=32;
  if(matrix.length>1){
    const body=sheet.getRangeByIndexes(1,0,matrix.length-1,matrix[0].length);
    body.format.wrapText=true;
    body.format.verticalAlignment="top";
  }
  sheet.freezePanes.freezeRows(1);
  sheet.tables.add(`A1:${String.fromCharCode(64+matrix[0].length)}${matrix.length}`,true,`${name.replace(/\s/g,"")}Table`);
  sheet.getUsedRange().format.autofitColumns();
  sheet.getUsedRange().format.autofitRows();
  // Keep link/narrative fields readable without making the workbook excessively wide.
  if(name==="Services"){
    sheet.getRange("A:A").format.columnWidth=9; sheet.getRange("B:B").format.columnWidth=22; sheet.getRange("C:E").format.columnWidth=21;
    sheet.getRange("F:I").format.columnWidth=14; sheet.getRange("J:J").format.columnWidth=30; sheet.getRange("K:K").format.columnWidth=14;
    sheet.getRange("L:M").format.columnWidth=24; sheet.getRange("N:O").format.columnWidth=30; sheet.getRange("P:P").format.columnWidth=45;
    sheet.getRange("Q:Q").format.columnWidth=30; sheet.getRange("R:R").format.columnWidth=20; sheet.getRange("S:S").format.columnWidth=42;
    sheet.getRange("T:W").format.columnWidth=42; sheet.getRange("X:X").format.columnWidth=16; sheet.getRange("Y:Y").format.columnWidth=60;
  }
  if(name==="Route Coverage"){
    sheet.getRange("A:A").format.columnWidth=14; sheet.getRange("B:C").format.columnWidth=28; sheet.getRange("D:D").format.columnWidth=44;
    sheet.getRange("E:E").format.columnWidth=34; sheet.getRange("F:G").format.columnWidth=48; sheet.getRange("H:H").format.columnWidth=18;
    sheet.getRange("I:I").format.columnWidth=28; sheet.getRange("J:J").format.columnWidth=16; sheet.getRange("K:K").format.columnWidth=20;
    sheet.getRange("L:L").format.columnWidth=24; sheet.getRange("M:N").format.columnWidth=50; sheet.getRange("O:O").format.columnWidth=16; sheet.getRange("P:P").format.columnWidth=70;
  }
  if(name==="Sources"){ sheet.getRange("B:B").format.columnWidth=44; sheet.getRange("C:C").format.columnWidth=60; sheet.getRange("D:D").format.columnWidth=30; sheet.getRange("E:E").format.columnWidth=80; }
  if(name==="Operators"){ sheet.getRange("B:B").format.columnWidth=38; sheet.getRange("C:D").format.columnWidth=36; sheet.getRange("E:E").format.columnWidth=52; sheet.getRange("F:F").format.columnWidth=65; sheet.getRange("G:G").format.columnWidth=18; }
  if(name==="Destinations"){ sheet.getRange("B:C").format.columnWidth=34; sheet.getRange("D:D").format.columnWidth=48; sheet.getRange("E:E").format.columnWidth=50; }
  if(name==="Summary"){ sheet.getRange("A:A").format.columnWidth=42; sheet.getRange("B:B").format.columnWidth=20; sheet.getRange("C:C").format.columnWidth=88; }
  sheet.getUsedRange().format.autofitRows();
}
await workbook.recalculate();
const inspect=await workbook.inspect({kind:"workbook,sheet,table",maxChars:4000,tableMaxRows:3,tableMaxCols:5});
console.log(inspect.ndjson);
for(const sheetName of ["Summary","Services","Route Coverage","Operators","Sources","Destinations"]){
  const preview=await workbook.render({sheetName,autoCrop:"all",scale:sheetName==="Summary"?1:0.35,format:"png"});
  await fs.writeFile(path.join(outDir,`.preview-${sheetName.replace(/\s/g,"-")}.png`),new Uint8Array(await preview.arrayBuffer()));
}
const xlsx=await SpreadsheetFile.exportXlsx(workbook);
await xlsx.save(path.join(outDir,"georgian-intercity-routes.xlsx"));
console.log(JSON.stringify(stats));
