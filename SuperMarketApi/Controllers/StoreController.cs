using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SuperMarketApi.Models;

namespace SuperMarketApi.Controllers
{
    [Route("api/[controller]")]

    public class StoreController : Controller
    {
        private POSDbContext db;
        public IConfiguration Configuration { get; }
        public StoreController(POSDbContext contextOptions, IConfiguration configuration)
        {
            db = contextOptions;
            Configuration = configuration;
        }

        [HttpGet("GetLocation")]
        public IActionResult GetLocation(int companyId)
        {
            try
            {
                var store = db.Stores.Where(s => s.CompanyId == companyId).ToList();
                return Ok(store);
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
        [HttpPost("AddLocation")]
        public IActionResult AddLocation([FromBody] dynamic objdata)
        {
            try
            {
                dynamic jsonObj = objdata;
                Store store = jsonObj.ToObject<Store>();
                db.Stores.Add(store);
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "Location Added Successfully"
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
        [HttpGet("GetStoreById")]
        public IActionResult GetStoreById(int id)
        {
            var store = db.Stores.Find(id);
            return Ok(store);
        }
        [HttpPost("UpdateStore")]
        public IActionResult UpdateStore([FromBody] dynamic objData)
        {
            try
            {
                dynamic jsonObj = objData;
                Store store = null;
                int id = jsonObj.Id;
                store = db.Stores.Find(id);
                store.Name = jsonObj.Name;
                store.ContactNo = jsonObj.ContactNo;
                store.Email= jsonObj.Email;
                store.PostalCode= jsonObj.PostalCode;
                store.isactive = jsonObj.IsActive;
                store.OpeningTime = jsonObj.OpeningTime;
                store.ClosingTime = jsonObj.ClosingTime;
                store.Address = jsonObj.Address;
                store.City = jsonObj.City;
                store.Country = jsonObj.Country;
                store.CompanyId = jsonObj.CompanyId;
                db.Entry(store).State = EntityState.Modified;
                db.SaveChanges();

                var response = new
                {
                    status = 200,
                    msg = "store Updated Successfully"
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

        [HttpGet("Deactivate")]
        public IActionResult Deactivate(int id)
        {
            var store = db.Stores.Find(id);
            if (store.isactive == true)
            {
                store.isactive = false;
                db.SaveChanges();
            }
            else
            {
                store.isactive = true;
                db.SaveChanges();
            }

            return Ok(store);
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
