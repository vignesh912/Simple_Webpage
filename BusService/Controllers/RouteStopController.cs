using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusService.Models;
using Microsoft.AspNetCore.Http;

namespace BusService.Controllers
{
    public class RouteStopController : Controller
    {
        private readonly BusServiceContext _context;

        public RouteStopController(BusServiceContext context)
        {
            _context = context;
        }

        // GET: RouteStop
        public async Task<IActionResult> Index(string id, string name)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                if (String.IsNullOrWhiteSpace(HttpContext.Session.GetString("BusCode")))
                {
                    TempData["EmptyBusCode"] = "Bus Route code was not passed.";
                    return RedirectToAction("Index", "BusRoutes");
                }
                else
                id = HttpContext.Session.GetString("BusCode");
            }
            else
            {
                HttpContext.Session.SetString("BusCode", id);
            }

            var busServiceContext = _context.RouteStop
                    .Include(r => r.BusRouteCodeNavigation)
                        .Include(r => r.BusStopNumberNavigation)
                            .Where(x => x.BusRouteCode == id)
                                .OrderBy(s => s.OffsetMinutes);

            ViewData["BusCode"] = id;
            ViewData["RouteName"] = name;

            return View(await busServiceContext.ToListAsync());
        }

        // GET: RouteStop/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop
                .Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusStopNumberNavigation)
                .FirstOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // GET: RouteStop/Create
        public IActionResult Create( string name, string code)
        {
            ViewData["BusCode"] = code;
            ViewData["RouteName"] = name;

            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode");
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop.OrderBy(x=> x.Location), "BusStopNumber", "Location");
            return View();
        }

        // POST: RouteStop/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop, string name, string code)
        {
            if (routeStop.OffsetMinutes < 0)
                ModelState.AddModelError("OffsetMinutes", "Can't be less than 0");

            if (routeStop.OffsetMinutes == 0 &&
                _context.RouteStop.Any(x => x.OffsetMinutes == 0))
            {
                ModelState.AddModelError("OffsetMinutes", "Can't be another than 0");
            }

            if (routeStop.OffsetMinutes > 0)
            {
                if(! _context.RouteStop.Any(x => x.OffsetMinutes == 0))
                    ModelState.AddModelError("OffsetMinutes", "First add a 0 route");
            }

            if (_context.RouteStop.Any(x => x.BusRouteCode == routeStop.BusRouteCode) && _context.RouteStop.Any(x => x.BusStopNumber == routeStop.BusStopNumber))
            {
                ModelState.AddModelError("BusStopNumber", "same combination can't exist");
            }
            if (ModelState.IsValid)
            {
                _context.Add(routeStop);
                await _context.SaveChangesAsync();

                TempData["SuccessBusRoute"] = "Route Stop Created Succesfully!";

                return RedirectToAction("Index", "BusRoutes");
            }

            ViewData["BusCode"] = code;
            ViewData["RouteName"] = name;

            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop.OrderBy(x => x.Location), "BusStopNumber", "Location");
            return View(routeStop);
        }

        // GET: RouteStop/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop.FindAsync(id);
            if (routeStop == null)
            {
                return NotFound();
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop, "BusStopNumber", "BusStopNumber", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // POST: RouteStop/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop)
        {
            if (id != routeStop.RouteStopId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routeStop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteStopExists(routeStop.RouteStopId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop, "BusStopNumber", "BusStopNumber", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // GET: RouteStop/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop
                .Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusStopNumberNavigation)
                .FirstOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // POST: RouteStop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routeStop = await _context.RouteStop.FindAsync(id);
            _context.RouteStop.Remove(routeStop);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RouteStopExists(int id)
        {
            return _context.RouteStop.Any(e => e.RouteStopId == id);
        }
    }
}
