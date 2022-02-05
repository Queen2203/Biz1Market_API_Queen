using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperMarketApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMarketApi.Controllers
{
    [Route("api/[controller]")]
    public class TaxGroupController : Controller
    {
        private POSDbContext db;
        public IConfiguration Configuration { get; }
        public TaxGroupController(POSDbContext contextOptions, IConfiguration configuration)
        {
            db = contextOptions;
            Configuration = configuration;
        }
        // GET: TaxGroupController
        public ActionResult Index()
        {
            return View();
        }

        // GET: TaxGroupController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TaxGroupController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TaxGroupController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TaxGroupController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TaxGroupController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TaxGroupController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TaxGroupController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        //Master 05-01-2022
        [HttpGet("GetTaxGrp")]
        public IActionResult GetTaxGrp(int CompanyId)
        {
            var taxgroup = db.TaxGroups.Where(x => x.CompanyId == CompanyId).ToList();
            return Ok(taxgroup);
        }
        //Master 06-01-2022
       [HttpPost("AddTaxGrp")]
        public IActionResult AddTaxGrp([FromBody] dynamic taxGroupobj)
        {
            try
            {
                TaxGroup taxGroup = taxGroupobj.ToObject<TaxGroup>();
                db.TaxGroups.Add(taxGroup);
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "TaxGrp added successfully",
                    taxgroup = taxGroup
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    status = 0,
                    msg = "OOPS! Something went wrong",
                    error = new Exception(ex.Message, ex.InnerException)
                };
                return Ok(response);
            }
        }
        //Master 06-01-2022
        [HttpPost("UpdateTaxGrp")]
        public IActionResult UpdateTaxGrp([FromBody] dynamic taxGroupobj)
        {
            try
            {
                TaxGroup taxGroup = taxGroupobj.ToObject<TaxGroup>();
                db.Entry(taxGroup).State = EntityState.Modified;
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "TaxGrp updated successfully",
                    taxgroup = taxGroup
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    status = 0,
                    msg = "OOPS! Something went wrong",
                    error = new Exception(ex.Message, ex.InnerException)
                };
                return Ok(response);
            }
        }
    }
}
