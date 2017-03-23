using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using e_commerce.Models;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;

namespace e_commerce.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(ProductsViewModel model)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var insert = "insert into Products (ProductName, ProductDescription, ProductPrice, ImgPath) values (@Name, @Description, @Price, @Path)";
                var parameters = new { Name = model.ProductName, Description = model.ProductDescription, Price = model.ProductPrice, Path = model.ImgPath };
                connection.Execute(insert, parameters);
            }
                return RedirectToAction("AddProduct");
        }
    }
}