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
    public class ProductsController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        // GET: Products
        public ActionResult Index()
        {
            List<ProductsViewModel> products;
            using (var connection = new SqlConnection(this.connectionString))
            {
                products = connection.Query<ProductsViewModel>("select * from Products").ToList();
            }
            return View(products);
        }

        public ActionResult Details (string id)
        {
            ProductsViewModel product;
            using (var connection = new SqlConnection(this.connectionString))
            {
                var query = "select * from Products where id = @productId";
                var parameters = new { productId = id };
                product = connection.QuerySingleOrDefault<ProductsViewModel>(query, parameters);
            }

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }
    }
}