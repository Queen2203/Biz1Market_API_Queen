using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SuperMarketApi.Models;
/*using System.Linq;*/
namespace SuperMarketApi.Controllers
{
    [Route("api/[controller]")]
    public class VendorController : Controller
    {
        private POSDbContext db;
        public IConfiguration Configuration { get; }
        public VendorController(POSDbContext contextOptions, IConfiguration configuration)
        {
            db = contextOptions;
            Configuration = configuration;
        }
        [HttpGet("getVendorList")]
        public IActionResult getVendorList(int CompanyId)
        {
            return Json(db.Vendors.Where(x => x.CompanyId == CompanyId).ToList());
        }
        [HttpGet("getVendorListbyid")]
        public IActionResult getVendorListbyid(int vendorid)
        {
            return Json(db.Vendors.Find(vendorid));
        }
        [HttpPost("addvendors")]
        public IActionResult addvendors([FromBody] dynamic data)
        {
            try
            {
                Contact contact = new Contact();
                contact = data.ToObject<Contact>();
                contact.Zip = (string)data.PostalCode;
                db.Contacts.Add(contact);
                db.SaveChanges();
                Vendor vendor = new Vendor();
                vendor = data.ToObject<Vendor>();
                vendor.Id = contact.Id;
                db.Vendors.Add(vendor);
                db.SaveChanges();
                var response = new
                {
                    vendor = vendor,
                    status = 200,
                    msg = "Vendors added successfully"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    status = 0,
                    msg = "Something went wrong",
                    error = new Exception(ex.Message, ex.InnerException)
                };
                return Ok(response);
            }
        }
        [HttpPost("updatevendors")]
        public IActionResult updatevendors([FromBody] Vendor vendor)
        {
            try
            {
                db.Entry(vendor).State = EntityState.Modified;
                db.SaveChanges();
                var response = new
                {
                    vendor = vendor,
                    status = 200,
                    msg = "Vendor updated successfully"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    status = 0,
                    msg = "Something went wrong",
                    error = new Exception(ex.Message, ex.InnerException)
                };
                return Ok(response);
            }
        }

        // GET: VendorController
        public ActionResult Index()
        {
            return View();
        }

        // GET: VendorController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: VendorController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: VendorController/Create
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

        // GET: VendorController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: VendorController/Edit/5
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

        // GET: VendorController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: VendorController/Delete/5
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
    }
}
