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
                try { 
                    var query = "select * from Cart as C join Products as P on P.Id = C.ProductId where CartId = @CartId";
                    var parameters = new { CartId = CartId };
                    Cart = connection.Query<CartViewModel>(query, parameters).ToList();

                } catch (SqlException)
                {
                    return View("Error");
                }
            }

            //if a cookie exits but is empty, output message
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
            CartViewModel queryStatement;
            var jsonResponse = new UpdateCartResponseModel();

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
                try { 
                    var query = "select * from Cart where CartId = @CartId and ProductId = @ProductId";
                    var queryParameter = new { CartId = CartId, ProductId = ProductId };
                    queryStatement = connection.QuerySingleOrDefault<CartViewModel>(query, queryParameter);
                } catch (SqlException)
                {
                    return View("Error");
                }

                if (queryStatement != null)
                {
                    try { 
                        var update = "update Cart set Quantity = Quantity + @Quantity where ProductId = @ProductId";
                        var updateParameter = new { Quantity = Quantity, ProductId = ProductId };
                        connection.Execute(update, updateParameter);
                    } catch (SqlException)
                    {
                        return View("Error");
                    }
                } else
                {
                    try { 
                        var insert = "insert into Cart (CartId, ProductId, Quantity) values (@CartId, @ProductId, @Quantity)";
                        var parameters = new { CartId = CartId, ProductId = ProductId, Quantity = Quantity };
                        connection.Execute(insert, parameters);
                    } catch (SqlException)
                    {
                        return View("Error");
                    }
                }

                jsonResponse.succes = true;
                jsonResponse.message = "The product has been added to your cart.";
            }

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeQuantityInCart (int ProductId, int Quantity, string CartId)
        {
            var jsonResponse = new UpdateCartResponseModel();

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    var update = "update Cart set Quantity = @Quantity where CartId = @CartId and ProductId = @ProductId";
                    var updateParameter = new { Quantity = Quantity, CartId = CartId, ProductId = ProductId };
                    connection.Execute(update, updateParameter);
                } catch (SqlException )
                {
                    return View("Error");
                }
            }
            jsonResponse.succes = true;
            jsonResponse.message = "The quantity of the product is changed.";

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteFromCart(int ProductId, string CartId)
        {
            var jsonResponse = new UpdateCartResponseModel();

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    var delete = "delete from Cart where CartId = @CartId and ProductId = @ProductId";
                    var deleteParameter = new { CartId = CartId, ProductId = ProductId };
                    connection.Execute(delete, deleteParameter);
                } catch (SqlException)
                {
                    
                    return View("Error");
                }
            }

            jsonResponse.succes = true;
            jsonResponse.message = "The product is deleted.";

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }
    }
}