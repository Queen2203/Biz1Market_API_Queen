using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SuperMarketApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMarketApi.Controllers
{
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private POSDbContext db;
        public IConfiguration Configuration { get; }
        public CustomerController(POSDbContext contextOptions, IConfiguration configuration)
        {
            db = contextOptions;
            Configuration = configuration;
        }

        // View Customer Detail
        [HttpGet("GetCustomerList")]
        public IActionResult GetCustomerList(int CompanyId)
        {
            return Json(db.Customers.Where(x => x.CompanyId == CompanyId).ToList());          
        }


        // Add Customer Details
        [HttpPost("addData")]
        public IActionResult IndexAdd([FromBody] Customer data)
        {
            var check = db.Customers.Where(x => x.PhoneNo.Equals(data.PhoneNo)).FirstOrDefault();
            if (check == null)
            {
                db.Customers.Add(data);
                db.SaveChanges();
                var responce = new
                {
                    status = 200,
                    message = "Customer Data Added!"
                };
                return Ok(responce);
            }
            else
            {
                var responce = new
                {
                    status = 500,
                    message = "Customer Ph. No. already existed!"
                };
                return Ok(responce);

            }
        }

        // put: Update Customer Details
        [HttpPut("updateData")]
        public IActionResult IndexUpdate([FromBody] Customer data)
        {
            db.Entry(data).State = EntityState.Modified;
            db.SaveChanges();
            var responce = new
            {
                status = 500,
                message = "Value Updated Successfull!"
            };
            return Ok(responce);
        }

        // Delete: Customer Details
        [HttpDelete("deleteData")]
        public IActionResult IndexDelete(int Id)
        {
            db.Customers.Remove(db.Customers.Find(Id));
            db.SaveChanges();
            var responce = new
            {
                status = 500,
                message = "Value Deleted Successfull!"
            };
            return Ok(responce);
        }

    }
}
