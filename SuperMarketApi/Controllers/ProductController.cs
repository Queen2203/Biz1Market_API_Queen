using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperMarketApi.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections;

namespace SuperMarketApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private POSDbContext db;
        public IConfiguration Configuration { get; }
        public ProductController(POSDbContext contextOptions, IConfiguration configuration)
        {
            db = contextOptions;
            Configuration = configuration;
        }

        // GET: View Product_Table_Data
        [HttpGet("viewData")]
        public IActionResult Indexdata()
        {
            return Json(db.Products.ToList());
        }

        [HttpGet("adbarcodes")]
        public IActionResult adbarcodes(int productid)
        {
            Product product = db.Products.Find(productid);
            var variantgroups = db.CategoryVariantGroups.Where(x => x.CategoryId == product.CategoryId).ToList();
            Barcode barcode = new Barcode();
            barcode.ProductId = productid;
            db.Barcodes.Add(barcode);
            db.SaveChanges();
            foreach(var vg in variantgroups)
            {
                foreach(var v in db.Variants.Where(x => x.VariantGroupId == vg.Id).ToList())
                {
                    BarcodeVariant barcodeVariant = new BarcodeVariant();
                    barcodeVariant.BarcodeId = barcode.Id;
                    barcodeVariant.VariantId = v.Id;
                    db.BarcodeVariants.Add(barcodeVariant);
                    db.SaveChanges();
                }
            }
            return Ok(200);
        }


        // Add Products
        [HttpGet("addData")]
           public IActionResult addData([FromBody] Product data)
            {
                try
                {
                    db.Products.Add(data);
                    db.SaveChanges();
                    Product product = db.Products.Find(data.Id);
                    var variantgroups = db.CategoryVariantGroups.Where(x => x.CategoryId == product.CategoryId).ToList();
                    Barcode barcode = new Barcode();
                    barcode.ProductId = data.Id;
                    db.Barcodes.Add(barcode);
                    db.SaveChanges();
                    foreach (var vg in variantgroups)
                    {
                        foreach (var v in db.Variants.Where(x => x.VariantGroupId == vg.Id).ToList())
                        {
                            BarcodeVariant barcodeVariant = new BarcodeVariant();
                            barcodeVariant.BarcodeId = barcode.Id;
                            barcodeVariant.VariantId = v.Id;
                            db.BarcodeVariants.Add(barcodeVariant);
                            db.SaveChanges();
                        }
                    }
                    int compId = product.CompanyId;
                    int productId = product.Id;
                    SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
                    sqlCon.Open();
                    SqlCommand cmd = new SqlCommand("dbo.StoreProduct", sqlCon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CompanyId", compId));
                    cmd.Parameters.Add(new SqlParameter("@productId", productId));
                    int success = cmd.ExecuteNonQuery();
                    sqlCon.Close();

                var response = new
                    {
                        status = 200,
                        message = "Value Added Successfull"
                    };
                    return Ok(response);
          
                }
                catch (Exception)
                {
                    throw;
                }
           }


        // View Products
        [HttpGet("getProduct")]
        public IActionResult getProduct(int CompanyId,int StoreId)
        {
            SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
            sqlCon.Open();

            SqlCommand cmd = new SqlCommand("dbo.retriveProduct", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@CompanyId", CompanyId));
            cmd.Parameters.Add(new SqlParameter("@StoreId", StoreId));

            DataSet ds = new DataSet();
            SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
            sqlAdp.Fill(ds);

            var response = new
            {
                Product = ds.Tables[0]
            };
            return Ok(response);
        }

        [HttpPost("addProduct")]
        public IActionResult addProduct([FromBody]dynamic data,int userid, int storeid)
        {
            try
            {
                int productid;
                Product product = data.product.ToObject<Product>();
                SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
                sqlCon.Open();

                SqlCommand cmd = new SqlCommand("dbo.CreateProduct", sqlCon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@companyid", product.CompanyId));
                cmd.Parameters.Add(new SqlParameter("@name", product.Name));
                cmd.Parameters.Add(new SqlParameter("@description", product.Description));
                cmd.Parameters.Add(new SqlParameter("@unitid", product.UnitId));
                cmd.Parameters.Add(new SqlParameter("@categoryid", product.CategoryId));
                cmd.Parameters.Add(new SqlParameter("@taxgroupid", product.TaxGroupId));
                //cmd.Parameters.Add(new SqlParameter("@kotgroupid", product.KOTGroupId));
                cmd.Parameters.Add(new SqlParameter("@producttypeid", product.ProductTypeId));
                cmd.Parameters.Add(new SqlParameter("@price", product.Price));
                cmd.Parameters.Add(new SqlParameter("@imgurl", product.ImgUrl));
                cmd.Parameters.Add(new SqlParameter("@code", product.ProductCode));
                cmd.Parameters.Add(new SqlParameter("@barcode", product.BarCode));
                cmd.Parameters.Add(new SqlParameter("@brand", product.brand));
                cmd.Parameters.Add(new SqlParameter("@userid", userid));
                // cmd.Parameters.Add(new SqlParameter("@storeId", sid.StoreId));

                DataSet ds = new DataSet();
                SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
                sqlAdp.Fill(ds);
                DataRow table = ds.Tables[0].Rows[0];
                productid = (int)table["ProductId"];
                sqlCon.Close();
                if(data.variantcombinations.Count > 0)
                {
                    foreach (dynamic cmb in data.variantcombinations)
                    {
                        Barcode barcode = new Barcode();
                        barcode.ProductId = productid;
                        barcode.Updated = true;
                        barcode.BarCode = cmb.barcode;
                        db.Barcodes.Add(barcode);
                        db.SaveChanges();
                        List<Store> stores = db.Stores.Where(x => x.CompanyId == product.CompanyId).ToList();
                        foreach(Store store in stores)
                        {
                            Stock stock = new Stock();
                            stock.BarcodeId = barcode.Id;
                            stock.CompanyId = product.CompanyId;
                            stock.CreatedBy = userid;
                            stock.CreatedDate = DateTime.Now;
                            stock.ProductId = productid;
                            stock.SortOrder = -1;
                            stock.StorageStoreId = store.Id;
                            stock.StoreId = store.Id;
                            stock.StorageStoreName = store.Name;
                            db.Stocks.Add(stock);
                            db.SaveChanges();
                        }
                        foreach (int id in cmb.variantids)
                        {
                            BarcodeVariant barcodeVariant = new BarcodeVariant();
                            barcodeVariant.BarcodeId = barcode.Id;
                            barcodeVariant.Updated = true;
                            barcodeVariant.VariantId = id;
                            db.BarcodeVariants.Add(barcodeVariant);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    Barcode barcode = new Barcode();
                    barcode.ProductId = productid;
                    barcode.Updated = true;
                    barcode.BarCode = product.BarCode;
                    db.Barcodes.Add(barcode);
                    db.SaveChanges();
                    List<Store> stores = db.Stores.Where(x => x.CompanyId == product.CompanyId).ToList();
                    foreach (Store store in stores)
                    {
                        Stock stock = new Stock();
                        stock.BarcodeId = barcode.Id;
                        stock.CompanyId = product.CompanyId;
                        stock.CreatedBy = userid;
                        stock.CreatedDate = DateTime.Now;
                        stock.ProductId = productid;
                        stock.SortOrder = -1;
                        stock.StorageStoreId = store.Id;
                        stock.StoreId = store.Id;
                        stock.StorageStoreName = store.Name;
                        db.Stocks.Add(stock);
                        db.SaveChanges();
                    }
                }

                sqlCon.Open();

                SqlCommand cmd1 = new SqlCommand("dbo.BarcodeProduct", sqlCon);
                cmd1.CommandType = CommandType.StoredProcedure;

                cmd1.Parameters.Add(new SqlParameter("@CompanyId", product.CompanyId));
                cmd1.Parameters.Add(new SqlParameter("@storeid", storeid));

                DataSet ds1 = new DataSet();
                SqlDataAdapter sqlAdp1 = new SqlDataAdapter(cmd1);
                sqlAdp1.Fill(ds1);
                sqlCon.Close();
                var response = new
                {
                    msg = "Product Added Successfully",
                    product = db.Products.Find(productid),
                    stocks = ds1.Tables[0]
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

        [HttpDelete("deleteData")]
        public IActionResult deleteData(int Id)
        {
            db.Products.Remove(db.Products.Find(Id));
            db.SaveChanges();
            var responce = new
            {
                status = 500,
                message = "Value Deleted Successfull!"
            };
            return Ok(responce);
        }

        [HttpPost("updateProduct")]
        public IActionResult updateProduct([FromBody] dynamic data, int userid)
        {
            try
            {
                Product product = data.product.ToObject<Product>();
                Product oldproduct = db.Products.AsNoTracking().Where(x => x.Id == product.Id).FirstOrDefault();
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                if(oldproduct.CategoryId != product.CategoryId)
                {
                    productcategorychange(product.Id, data.variantcombinations);
                }
                else
                {
                    barcodedetailsupdate(product.Id, data.variantcombinations);
                }
                var response = new
                {
                    msg = "Product Added Successfully",
                    product = product
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
        public bool barcodedetailsupdate(int productid, dynamic variantcombinations)
        {
            List<int> bcids = new List<int>();
            foreach (dynamic cmb in variantcombinations)
            {
                if(cmb.id > 0)
                {
                    int barcodeid = (int)cmb.id;
                    Barcode barcode = db.Barcodes.Find(barcodeid);
                    barcode.BarCode = cmb.barcode;
                    db.Entry(barcode).State = EntityState.Modified;
                    db.SaveChanges();
                    bcids.Add(barcode.Id);
                }
                else if(cmb.id == 0)
                {
                    Barcode barcode = new Barcode();
                    barcode.BarCode = cmb.barcode;
                    barcode.ProductId = productid;
                    barcode.Updated = true;
                    db.Barcodes.Add(barcode);
                    db.SaveChanges();
                    bcids.Add(barcode.Id);
                    foreach (int id in cmb.variantids)
                    {
                        BarcodeVariant barcodeVariant = new BarcodeVariant();
                        barcodeVariant.BarcodeId = barcode.Id;
                        barcodeVariant.Updated = true;
                        barcodeVariant.VariantId = id;
                        db.BarcodeVariants.Add(barcodeVariant);
                        db.SaveChanges();
                    }
                }
            }
            List<Barcode> barcodes = db.Barcodes.Where(x => x.ProductId == productid).ToList();
            foreach (Barcode bc in barcodes)
            {
                if (!bcids.Contains(bc.Id))
                {
                    BarcodeVariant[] barcodeVariants = db.BarcodeVariants.Where(x => x.BarcodeId == bc.Id).ToArray();
                    db.BarcodeVariants.RemoveRange(barcodeVariants);
                    db.SaveChanges();
                }
            }
            return true;
        }
        public bool productcategorychange(int productid, dynamic variantcombinations)
        {
            List<Barcode> barcodes = db.Barcodes.Where(x => x.ProductId == productid).ToList();
            foreach(Barcode barcode in barcodes)
            {
                BarcodeVariant[] barcodeVariants = db.BarcodeVariants.Where(x => x.BarcodeId == barcode.Id).ToArray();
                db.BarcodeVariants.RemoveRange(barcodeVariants);
                db.SaveChanges();
            }
            for(int i=0; i<variantcombinations.Count; i++)
            {
                Barcode barcode = new Barcode();
                if (barcodes[i] != null)
                {
                    barcode = barcodes[i];
                    barcode.BarCode = variantcombinations[i].barcode;
                    db.Entry(barcode).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    barcode.ProductId = productid;
                    barcode.Updated = true;
                    barcode.BarCode = variantcombinations[i].barcode;
                    db.Barcodes.Add(barcode);
                    db.SaveChanges();
                }
                foreach (int id in variantcombinations[i].variantids)
                {
                    BarcodeVariant barcodeVariant = new BarcodeVariant();
                    barcodeVariant.BarcodeId = barcode.Id;
                    barcodeVariant.Updated = true;
                    barcodeVariant.VariantId = id;
                    db.BarcodeVariants.Add(barcodeVariant);
                    db.SaveChanges();
                }
            }
            foreach (dynamic cmb in variantcombinations)
            {
                Barcode barcode = new Barcode();
                barcode.ProductId = productid;
                barcode.Updated = true;
                barcode.BarCode = cmb.barcode;
                db.Barcodes.Add(barcode);
                db.SaveChanges();
                foreach (int id in cmb.variantids)
                {
                    BarcodeVariant barcodeVariant = new BarcodeVariant();
                    barcodeVariant.BarcodeId = barcode.Id;
                    barcodeVariant.Updated = true;
                    barcodeVariant.VariantId = id;
                    db.BarcodeVariants.Add(barcodeVariant);
                    db.SaveChanges();
                }
            }
            return true;
        }
        [HttpGet("getTaxgroup")]
        public IActionResult getTaxgroup(int CompanyId)
        {
            var taxgroups = db.TaxGroups.Where(x => x.CompanyId == CompanyId).ToList();

            return Ok(taxgroups);
        }

        [HttpPost("addtaxgroup")]
        public IActionResult addtaxgroup([FromBody]dynamic taxGroupobj)
        {
            try
            {
                TaxGroup taxGroup = taxGroupobj.ToObject<TaxGroup>();
                db.TaxGroups.Add(taxGroup);
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "TaxGroup added successfully",
                    taxgroup = taxGroup
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
        [HttpPost("updatetaxgroup")]
        public IActionResult updatetaxgroup([FromBody]dynamic taxGroupobj)
        {
            try
            {
                TaxGroup taxGroup = taxGroupobj.ToObject<TaxGroup>();
                db.Entry(taxGroup).State = EntityState.Modified;
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "Taxgroup updated successfully",
                    taxgroup = taxGroup
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

        [HttpGet("getUnits")]
        public IActionResult getUnits(int CompanyId)
        {
            var units = db.Units.ToList();

            return Ok(units);
        }

        [HttpGet("getorders")]
        public IActionResult getorders(int CompanyId)
        {
            var units = db.Orders.ToList();

            return Ok(units);
        }
      

        [HttpGet("getProductType")]
        public IActionResult getProductType(int CompanyId)
        {
            var producttypes = db.ProductTypes.ToList();
            return Ok(producttypes);
        }

        [HttpGet("getmasterproducts")]
        public IActionResult getmasterproducts(int CompanyId)
        {
            var products = db.Products.Where(x => x.CompanyId == CompanyId).Include(x => x.TaxGroup).Include(x => x.Category).ToList();
            return Ok(products);
        }
        [HttpGet("getproductbyid")]
        public IActionResult getproductbyid(int ProductId)
        {
            var product = db.Products.Find(ProductId);
            var barcodes = db.Barcodes.Where(x => x.ProductId == ProductId).ToList();
            var barcodevariants = db.BarcodeVariants.Where(x => barcodes.Where(y => y.Id == x.BarcodeId).Any()).ToList();
            var data = new
            {
                product = product,
                barcodes = barcodes,
                barcodevariants = barcodevariants
            };
            return Ok(data);
        }
        [HttpGet("getcategoryvariants")]
        public IActionResult getcategoryvariants(int categoryid)
        {
            try
            {
                var categoryvariantgroups = db.CategoryVariantGroups.Where(x => x.CategoryId == categoryid).ToList();
                foreach (var categoryvariantgroup in categoryvariantgroups)
                {
                    categoryvariantgroup.VariantGroupName = db.VariantGroups.Find(categoryvariantgroup.VariantGroupId).Name;
                    categoryvariantgroup.Variants = db.Variants.Where(x => x.VariantGroupId == categoryvariantgroup.VariantGroupId).ToList();
                }
                return Ok(categoryvariantgroups);
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
        [HttpGet("getmasteroption")]
        public IActionResult getmasteroption(int CompanyId)
        {
            var options = db.Variants.Where(x => x.CompanyId == CompanyId).Include(x => x.VariantGroup).ToList();

            return Ok(options);
        }
        [HttpGet("getmasteroptiongroup")]
        public IActionResult getmasteroptiongroup(int CompanyId)
        {
            var optionGroups = db.VariantGroups.Where(x => x.CompanyId == CompanyId).ToList();

            return Ok(optionGroups);
        }


        [HttpGet("getKotGroup")]
        public IActionResult getKotGroup(int CompanyId)
        {
            var kotgroups = db.KOTGroups.ToList();

            return Ok(kotgroups);
        }

        [HttpGet("getCategory")]
        public IActionResult getCategory(int CompanyId)
        {
            var categories = db.Categories.Where(x => x.CompanyId == CompanyId).ToList();

            return Ok(categories);
        }
        
        [HttpGet("getvariants")]
        public IActionResult getvariants(int CompanyId)
        {
            var variants = db.Variants.Where(x => x.CompanyId == CompanyId).Include(x => x.VariantGroup).ToList();
            return Ok(variants);
        }

        [HttpGet("getvariantgroups")]
        public IActionResult getvariantgroups(int CompanyId)
        {
            var variantgroups = db.VariantGroups.Where(x => x.CompanyId == CompanyId).ToList();
            foreach (var variantgroup in variantgroups)
            {
                variantgroup.variantcount = db.Variants.Where(x => x.VariantGroupId == variantgroup.Id).ToList().Count();
            }
            return Ok(variantgroups);
        }

        [HttpPost("addvariant")]
        public IActionResult addvariant([FromBody]Variant variant)
        {
            try
            {
                db.Variants.Add(variant);
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "Variant added successfully",
                    variant = variant
                };
                return Ok(response);
            }
            catch(Exception ex)
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

        [HttpPost("addvariantgroup")]
        public IActionResult addvariantgroup([FromBody]VariantGroup variantGroup)
        {
            try
            {
                db.VariantGroups.Add(variantGroup);
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "VariantGroup added successfully",
                    variantGroup = variantGroup
                };
                return Ok(response);
            }
            catch(Exception ex)
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
        [HttpPost("updatevariant")]
        public IActionResult updatevariant([FromBody]Variant variant)
        {
            try
            {

                db.Entry(variant).State = EntityState.Modified;
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "Variant updated successfully",
                    variant = variant
                };
                return Ok(response);
            }
            catch(Exception ex)
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

        [HttpPost("updatevariantgroup")]
        public IActionResult updatevariantgroup([FromBody]VariantGroup variantGroup)
        {
            try
            {
                db.Entry(variantGroup).State = EntityState.Modified;
                db.SaveChanges();
                var response = new
                {
                    status = 200,
                    msg = "VariantGroup updated successfully",
                    variantGroup = variantGroup
                };
                return Ok(response);
            }
            catch(Exception ex)
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

        [HttpPost("batchEntry")]
        public IActionResult batchEntry([FromBody]dynamic batcheobj,int userid)
        {
            try
            {
                List<Batch> batches = batcheobj.ToObject<List<Batch>>();
                int companyid = 0;
                SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
                JArray products = new JArray();
                foreach (Batch batch in batches)
                {
                    companyid = batch.CompanyId;
                    sqlCon.Open();

                    SqlCommand cmd = new SqlCommand("dbo.BatchEntry", sqlCon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@companyid", batch.CompanyId));
                    cmd.Parameters.Add(new SqlParameter("@batchno", batch.BatchNo));
                    cmd.Parameters.Add(new SqlParameter("@quantity", batch.Quantity));
                    cmd.Parameters.Add(new SqlParameter("@barcodeid", batch.BarcodeId));
                    cmd.Parameters.Add(new SqlParameter("@storeid", batch.StoreId));
                    cmd.Parameters.Add(new SqlParameter("@expiarydate", batch.ExpiaryDate));
                    cmd.Parameters.Add(new SqlParameter("@productid", batch.ProductId));
                    cmd.Parameters.Add(new SqlParameter("@price", batch.Price));
                    cmd.Parameters.Add(new SqlParameter("@entrydatetime", batch.EntryDateTime));
                    cmd.Parameters.Add(new SqlParameter("@userid", userid));

                    DataSet ds = new DataSet();
                    SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
                    sqlAdp.Fill(ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        JObject product = new JObject();
                        for (int j=0; j<row.ItemArray.Length; j++)
                        {
                            string column = ds.Tables[0].Columns[j].ToString();
                            column = Char.ToLowerInvariant(column[0]) + column.Substring(1);
                            var rowvalue = row.ItemArray[j];
                            product.Add(new JProperty(column, rowvalue));
                        }
                        products.Add(product);
                    }
                    var sqldata = new
                    {
                        products = ds.Tables[0]
                    };
                    sqlCon.Close();
                }
                int lastbatchno = db.Batches.Where(x => x.CompanyId == companyid).Max(x => x.BatchNo);
                var response = new
                {
                    msg = "BatchEntry Added Successfully",
                    lastbatchno = lastbatchno,
                    products = products
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
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        [HttpGet("getbarcodeproduct")]
        public IActionResult getbarcodeproduct(int CompanyId, int storeid)
        {
            SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
            sqlCon.Open();

            SqlCommand cmd = new SqlCommand("dbo.BarcodeProduct", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@CompanyId",CompanyId));
            cmd.Parameters.Add(new SqlParameter("@storeId", storeid));

            DataSet ds = new DataSet();
            SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
            sqlAdp.Fill(ds);
            int lastbatchno = 0;
            int lastorderno = 0;
            if(db.Batches.Where(x => x.CompanyId == CompanyId).Any())
            {
                lastbatchno = db.Batches.Where(x => x.CompanyId == CompanyId).Max(x => x.BatchNo);
            }
            if(db.Orders.Where(x => x.CompanyId == CompanyId).Any())
            {
                lastorderno = db.Orders.Where(x => x.CompanyId == CompanyId).Max(x => x.OrderNo);
            }
            var response = new
            {
                Products = ds.Tables[0],
                lastbatchno = lastbatchno,
                lastorderno = lastorderno
            };
            return Ok(response);
        }

        [HttpGet("getstockbarcodeproduct")]
        public IActionResult getstockbarcodeproduct(int CompanyId, int storeid)
        {
            SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
            sqlCon.Open();

            SqlCommand cmd = new SqlCommand("dbo.StockBatchProducts", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@CompanyId", CompanyId));
            cmd.Parameters.Add(new SqlParameter("@storeId", storeid));

            DataSet ds = new DataSet();
            SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
            sqlAdp.Fill(ds);
            int lastbatchno = 0;
            int lastorderno = 0;
            if (db.Batches.Where(x => x.CompanyId == CompanyId).Any())
            {
                lastbatchno = db.Batches.Where(x => x.CompanyId == CompanyId).Max(x => x.BatchNo);
            }
            if (db.Orders.Where(x => x.CompanyId == CompanyId).Any())
            {
                lastorderno = db.Orders.Where(x => x.CompanyId == CompanyId).Max(x => x.OrderNo);
            }
            var response = new
            {
                Products = ds.Tables[0],
                lastbatchno = lastbatchno,
                lastorderno = lastorderno
            };
            return Ok(response);
        }


        [HttpPost("stockEntry")]
        public IActionResult stockEntry([FromBody] List<StockBatch> stockBatches)
        {
            try
            {
                SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
                foreach (StockBatch stockbatch in stockBatches)
                {
                    sqlCon.Open();
                    SqlCommand cmd = new SqlCommand("dbo.StockBatches", sqlCon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@companyid", stockbatch.CompanyId));
                    cmd.Parameters.Add(new SqlParameter("@stockid", stockbatch.StockId));
                    cmd.Parameters.Add(new SqlParameter("@batchid", stockbatch.BatchId));
                    cmd.Parameters.Add(new SqlParameter("@quantity", stockbatch.Quantity));
                    cmd.Parameters.Add(new SqlParameter("@createddate", stockbatch.CreatedDate));
                    cmd.Parameters.Add(new SqlParameter("@createdby", stockbatch.CreatedBy));
                    cmd.Parameters.Add(new SqlParameter("@storeId", stockbatch.StockBatchId));

                    DataSet ds = new DataSet();
                    SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
                    sqlAdp.Fill(ds);
                    sqlCon.Close();
                }
                var response = new
                {
                    msg = "Stock Added Successfully",
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
        [HttpGet("getStockProduct")]
        public IActionResult getStockProduct(int CompanyId,int StoreId)
        {
            try
            {
                SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
                sqlCon.Open();

                SqlCommand cmd = new SqlCommand("dbo.StockProduct", sqlCon);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@CompanyId", CompanyId));
                cmd.Parameters.Add(new SqlParameter("@storeId", StoreId));

                DataSet ds = new DataSet();
                SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
                sqlAdp.Fill(ds);
                sqlCon.Close();

                var response = new
                {
                    Products = ds.Tables[0],
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
        [HttpPost("bulkaddproduct")]
        public IActionResult bulkaddproduct([FromBody]List<Product> products)
        {
            try
            {
                int companyid = 0;
                foreach (Product product in products)
                {
                    companyid = product.CompanyId;
                    db.Products.Add(product);
                    db.SaveChanges();
                }
                var product_list = db.Products.Where(x => x.CompanyId == companyid).ToList();
                var response = new
                {
                    status = 200,
                    product_list = product_list
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
        [HttpPost("bulkupdateproduct")]
        public IActionResult bulkupdateproduct([FromBody] List<Product> products)
        {
            try
            {
                int companyid = 0;
                foreach (Product product in products)
                {
                    companyid = product.CompanyId;
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var product_list = db.Products.Where(x => x.CompanyId == companyid).ToList();
                var response = new
                {
                    status = 200,
                    product_list = product_list
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
        [HttpPost("bulkaddoption")]
        public IActionResult bulkaddoption([FromBody] List<Variant> variants)
        {
            try
            {
                int companyid = 0;
                foreach (Variant variant in variants)
                {
                    companyid = variant.CompanyId;
                    variant.VariantGroup = null;
                    db.Variants.Add(variant);
                    db.SaveChanges();
                }
                var variant_list = db.Variants.Where(x => x.CompanyId == companyid).ToList();
                var response = new
                {
                    status = 200,
                    variant_list = variant_list
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
        [HttpPost("bulkupdateoption")]
        public IActionResult bulkupdateoption([FromBody] List<Variant> variants)
        {
            try
            {
                int companyid = 0;
                foreach (Variant variant in variants)
                {
                    companyid = variant.CompanyId;
                    variant.VariantGroup = null;
                    db.Entry(variant).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var variant_list = db.Variants.Where(x => x.CompanyId == companyid).ToList();
                var response = new
                {
                    status = 200,
                    variant_list = variant_list
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
        [HttpPost("bulkaddoptiongroup")]
        public IActionResult bulkaddoptiongroup([FromBody] List<VariantGroup> variantGroups)
        {
            try
            {
                int companyid = 0;
                foreach (VariantGroup variantGroup in variantGroups)
                {
                    companyid = variantGroup.CompanyId;
                    db.VariantGroups.Add(variantGroup);
                    db.SaveChanges();
                }
                var variantgroup_list = db.VariantGroups.Where(x => x.CompanyId == companyid).ToList();
                var response = new
                {
                    status = 200,
                    variantgroup_list = variantgroup_list
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
        [HttpPost("bulkupdateoptiongroup")]
        public IActionResult bulkupdateoptiongroup([FromBody] List<VariantGroup> variantGroups)
        {
            try
            {
                int companyid = 0;
                foreach (VariantGroup variantGroup in variantGroups)
                {
                    companyid = variantGroup.CompanyId;
                    db.Entry(variantGroup).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var variantgroup_list = db.VariantGroups.Where(x => x.CompanyId == companyid).ToList();
                var response = new
                {
                    status = 200,
                    variantgroup_list = variantgroup_list
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
        //[HttpGet("getstockbatchproducts")]
        //public IActionResult getstoreproduct(int CompanyId,int StoreId)
        //{
        //    SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("myconn"));
        //    sqlCon.Open();

        //    SqlCommand cmd = new SqlCommand("dbo.StockBatchProducts", sqlCon);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.Add(new SqlParameter("@CompanyId", CompanyId));
        //     cmd.parameters.add(new sqlparameter("@storeid",storeid));

        //    DataSet ds = new DataSet();
        //    SqlDataAdapter sqlAdp = new SqlDataAdapter(cmd);
        //    sqlAdp.Fill(ds);
        //    int lastbatchno = 0;
        //    int lastorderno = 0;
        //    if (db.Batches.Where(x => x.CompanyId == CompanyId).Any())
        //    {
        //        lastbatchno = db.Batches.Where(x => x.CompanyId == CompanyId).Max(x => x.BatchNo);
        //    }
        //    if (db.Orders.Where(x => x.CompanyId == CompanyId).Any())
        //    {
        //        lastorderno = db.Orders.Where(x => x.CompanyId == CompanyId).Max(x => x.OrderNo);
        //    }
        //    var response = new
        //    {
        //        Products = ds.Tables[0],
        //        lastbatchno = lastbatchno,
        //        lastorderno = lastorderno
        //    };
        //    return Ok(response);
        //}


        public static DateTime UnixTimeStampToDateTime(Int64 unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000);
            var istdate = TimeZoneInfo.ConvertTimeFromUtc(dtDateTime, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            return istdate;
        }
    }
}
