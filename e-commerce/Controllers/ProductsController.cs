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
            //Get all products and display them
            List<ProductsViewModel> products;
            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    products = connection.Query<ProductsViewModel>("select * from Products").ToList();
                } catch (SqlException)
                {
                    return View("Error");
                }
            }
            return View(products);
        }

        public ActionResult Details (string id)
        {
            //Display the choosen product details
            ProductsViewModel product;
            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    var query = "select * from Products where id = @productId";
                    var parameters = new { productId = id };
                    product = connection.QuerySingleOrDefault<ProductsViewModel>(query, parameters);
                } catch (SqlException)
                {
                    return View("Error");
                }
            }

            //If the product does not exists, show 404 page
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }
    }
}