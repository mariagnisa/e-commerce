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
    public class CartController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        //get cart
        [HttpGet]
        public ActionResult ShowCart()
        {
            List<CartViewModel> Cart;
            var CartId = Request.Cookies["shoppingCart"].Value;

            if (CartId == null)
            {

            } 

            using (var connection = new SqlConnection(this.connectionString))
            {
                var query = "select * from Cart as C join Products as P on P.Id = C.ProductId where CartId = @CartId";
                var parameters = new { CartId = CartId };
                Cart = connection.Query<CartViewModel>(query, parameters).ToList();
            }

            return View(Cart);
        }

        //insert and create cart
        [HttpPost]
        public ActionResult AddItemToCart(int ProductId, int Quantity)
        {
            string CartId;

            //See if there are any cookie, if not create one
            if (Request.Cookies["shoppingCart"] == null)
            {

                CartId = Guid.NewGuid().ToString();
                HttpCookie Cookie = new HttpCookie("shoppingCart");
                Cookie.Value = CartId;
                Cookie.Expires = DateTime.Now.AddDays(30d);
                Response.Cookies.Add(Cookie);  

            } else
            {
                CartId = Request.Cookies["shoppingCart"].Value;
            }

            using (var connection = new SqlConnection(this.connectionString))
            {
                var insert = "insert into Cart (CartId, ProductId, Quantity) values (@CartId, @ProductId, @Quantity)";
                var parameters = new { CartId = CartId, ProductId = ProductId, Quantity = Quantity };
                connection.Execute(insert, parameters);
            }

            return RedirectToAction("Index", "Products");
        }
    }
}