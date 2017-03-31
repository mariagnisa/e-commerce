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

        //get cart
        [HttpGet]
        public ActionResult Index()
        {
            List<CartViewModel> Cart;
            var CartCookie = Request.Cookies["shoppingCart"];
            
            //If cart is empty output message
            if (CartCookie == null)
            {
                ViewBag.Message = "Your cart is currently empty.";
                return View();
            }

            var CartId = CartCookie.Value;

            using (var connection = new SqlConnection(this.connectionString))
            {
                var query = "select * from Cart as C join Products as P on P.Id = C.ProductId where CartId = @CartId";
                var parameters = new { CartId = CartId };
                Cart = connection.Query<CartViewModel>(query, parameters).ToList();
                ViewBag.TotalSum = Cart.Sum(p => p.ProductPrice * p.Quantity);
            }

            if (!Cart.Any())
            {
                ViewBag.Message = "Your cart is currently empty.";
                return View();
            }

            return View(Cart);
        }

        //add product to cart
        [HttpPost]
        public ActionResult AddItemToCart(int ProductId, int Quantity)
        {
            string CartId;

            //check if there is any cart, if not create one
            if (Request.Cookies["shoppingCart"] == null)
            {
                //Give the cookie an unique id
                CartId = Guid.NewGuid().ToString();
                //save cart id in cookie
                HttpCookie Cookie = new HttpCookie("shoppingCart");
                Cookie.Value = CartId;
                Cookie.Expires = DateTime.Now.AddDays(30d);
                Response.Cookies.Add(Cookie);  

            } else
            {
                CartId = Request.Cookies["shoppingCart"].Value;
            }

            //If product already exists update the quantity otherwise add product to cart
            using (var connection = new SqlConnection(this.connectionString))
            {
                var query = "select * from Cart where CartId = @CartId and ProductId = @ProductId";
                var queryParameter = new { CartId = CartId, ProductId = ProductId };
                var queryStatement = connection.QuerySingleOrDefault<CartViewModel>(query, queryParameter);

                if (queryStatement != null)
                {
                    var update = "update Cart set Quantity = Quantity + @Quantity where ProductId = @ProductId";
                    var updateParameter = new { Quantity = Quantity, ProductId = ProductId };
                    connection.Execute(update, updateParameter);
                } else
                {
                    var insert = "insert into Cart (CartId, ProductId, Quantity) values (@CartId, @ProductId, @Quantity)";
                    var parameters = new { CartId = CartId, ProductId = ProductId, Quantity = Quantity };
                    connection.Execute(insert, parameters);
                }

            }

            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public ActionResult ChangeQuantityInCart (int ProductId, int Quantity, string CartId)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var update = "update Cart set Quantity = @Quantity where CartId = @CartId and ProductId = @ProductId";
                var updateParameter = new { Quantity = Quantity, CartId = CartId, ProductId = ProductId };
                connection.Execute(update, updateParameter);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteFromCart(int ProductId, string CartId)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var delete = "delete from Cart where CartId = @CartId and ProductId = @ProductId";
                var deleteParameter = new { CartId = CartId, ProductId = ProductId };
                connection.Execute(delete, deleteParameter);
            }
            return RedirectToAction("Index");
        }
    }
}