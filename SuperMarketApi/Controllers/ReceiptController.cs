using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class ReceiptController : Controller
    {
        private POSDbContext db;
        public IConfiguration Configuration { get; }
        public ReceiptController(POSDbContext contextOptions, IConfiguration configuration)
        {
            db = contextOptions;
            Configuration = configuration;
        }


        [HttpGet("Gettestdata")]
        public IActionResult Gettestdata(int Storeid, DateTime? fromdate, DateTime? todate)
        {
            try
            {
                SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
                sqlCon.Open();

                SqlCommand cmd = new SqlCommand("dbo.testdata", sqlCon);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add(new SqlParameter("@ordereddate", Orderedate));
                cmd.Parameters.Add(new SqlParameter("@storeId", Storeid));
                cmd.Parameters.Add(new SqlParameter("@fromDate", fromdate));
                cmd.Parameters.Add(new SqlParameter("@todate", todate));

                DataSet ds = new DataSet();
                SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
                sqlAdp.Fill(ds);
                var data = new
                {
                    receipts = ds.Tables[0],
                };
                sqlCon.Close();
                return Ok(data);
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
        // GET: ReceiptController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ReceiptController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReceiptController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReceiptController/Create
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

        // GET: ReceiptController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReceiptController/Edit/5
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

        // GET: ReceiptController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReceiptController/Delete/5
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
