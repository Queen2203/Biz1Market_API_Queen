using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperMarketApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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


        [HttpGet("GetIndex")]
        public IActionResult GetIndex(int CompanyId)
        {
            try
            {
                SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
                sqlCon.Open();
                SqlCommand cmd = new SqlCommand("dbo.Customer", sqlCon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@CompId", CompanyId));
                DataSet ds = new DataSet();
                SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
                sqlAdp.Fill(ds);
                DataTable table = ds.Tables[0];

                var data = new
                {
                    Order = ds.Tables[0]
                };
                sqlCon.Close();
                return Ok(table);
            }
            catch (Exception e)
            {
                var error = new
                {
                    error = new Exception(e.Message, e.InnerException),
                    status = 0,
                    msg = "Something went wrong  Contact our service provider"
                };
                return Json(error);
            }
        }

        [HttpPost("Update")]
        public IActionResult Update([FromBody] JObject data)
        {
            try
            {
                dynamic cust = data;
                Customer customer = cust.ToObject<Customer>();
                customer.CreatedDate = db.Customers.Where(x => x.Id == customer.Id).AsNoTracking().FirstOrDefault().CreatedDate;
                customer.ModifiedDate = DateTime.Now;
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                var error = new
                {
                    status = 200,
                    msg = "The Customer updated successfully"
                };

                return Json(error);
            }
            catch (Exception e)
            {
                var st = e.ToString();
                var error = new
                {
                    error = new Exception(e.Message, e.InnerException),
                    status = 0,
                    msg = "Something went wrong  Contact our service provider"
                };
                return Json(error);
            }
        }




        [HttpGet("Delete")]
        [EnableCors("AllowOrigin")]
        public IActionResult Delete(int Id)
        {
            var orders = db.Orders.Where(x => x.CustomerId == Id).ToList();
            if (orders.Count == 0)
            {
                var cust = db.Customers.Find(Id);
                db.Customers.Remove(cust);
                db.SaveChanges();
                var error = new
                {
                    status = 200,
                    msg = "The Customer deleted successfully"
                };
                return Json(error);
            }
            else
            {
                var error = new
                {
                    status = 0,
                    msg = "Something went wrong  Contact our service provider"
                };
                return Json(error);
            }
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
