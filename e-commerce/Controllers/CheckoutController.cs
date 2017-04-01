using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using e_commerce.Models;
using Dapper;
using System.Configuration;
using System.Data.SqlClient;

namespace e_commerce.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        
        // GET: Checkout
        [HttpGet]
        public ActionResult Index()
        {
            List<CheckoutViewModel> CartProducts;
            string CartId = Request.Cookies["shoppingCart"].Value;

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    var query = "select * from Cart as C join Products as P on P.Id = C.ProductId where CartId = @CartId";
                    var parameters = new { CartId = CartId };
                    CartProducts = connection.Query<CheckoutViewModel>(query, parameters).ToList();
                    
                } catch (SqlException)
                {
                    return View("Error");
                }
            }
            return View(CartProducts);
        }

        [HttpPost]
        public ActionResult AddOrder(string Firstname, string Lastname, string Email, int Phone, string Street, int PostalCode, string City)
        {
            string CartId = Request.Cookies["shoppingCart"].Value;
            int OrderId;
            int CustomerId = 0;
            List<CheckoutViewModel> Products;

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    // add customer to db and get last inserted customer id
                    var insert = "insert into Customer (Firstname, Lastname, Email, Phone, Street, PostalCode, City) values (@Firstname, @Lastname, @Email, @Phone, @Street, @PostalCode, @City)";
                    var parameters = new { Firstname = Firstname, Lastname = Lastname, Email = Email, Phone = Phone, Street = Street, PostalCode = PostalCode, City = City };
                    connection.Execute(insert, parameters);
                    CustomerId = connection.Query<int>("SELECT MAX(Id) from Customer").First();
                } catch (SqlException)
                {
                    return View("Error");
                }

                try
                {
                    // add order to db and get last inserted order id
                    var insertOrder = "insert into Orders (Orderstatus, CustomerId, CartId) values (@Orderstatus, @CustomerId, @CartId)";
                    var OrderParameters = new { Orderstatus = 1, CustomerId = CustomerId, CartId = CartId };
                    connection.Execute(insertOrder, OrderParameters);
                    OrderId = connection.Query<int>("SELECT MAX(OrderId) from Orders").First();
                } catch (SqlException)
                {
                    return View("Error");
                }

                try
                {
                    //get products
                    var selectProducts = "select C.ProductId, C.Quantity, P.ProductPrice from Cart as C join Products as P on C.ProductId = P.Id where C.CartId = @CartId";
                    var productsParameters = new { CartId = CartId };
                    Products = connection.Query<CheckoutViewModel>(selectProducts, productsParameters).ToList();
                } catch (SqlException)
                {
                    return View("Error");
                }

                try
                {
                    //add products to order item in db
                    foreach (CheckoutViewModel CartProduct in Products)
                    {
                        var OrderProducts = "insert into OrderProducts (OrderId, ProductId, Quantity, Price) values (@OrderId, @ProductId, @Quantity, @Price)";
                        var OrderParam = new { OrderId = OrderId, ProductId = CartProduct.ProductId, Quantity = CartProduct.Quantity, Price = CartProduct.ProductPrice };
                        connection.Execute(OrderProducts, OrderParam);
                    }
                } catch (SqlException)
                {
                    return View("Error");
                }

                try { 
                    //delete cart from db
                    var deleteCart = "delete from Cart where CartId = @CartId";
                    var DeleteParam = new { CartId = CartId };
                    connection.Execute(deleteCart, DeleteParam);
                } catch (SqlException)
                {
                    return View("Error");
                }
                
            }


            // delete cart cookie
            var CartCookie = new HttpCookie("shoppingCart");
            CartCookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(CartCookie);

            return RedirectToAction("ConfirmOrder", new { OrderId = OrderId });
        }

        [HttpGet]
        public ActionResult ConfirmOrder(int OrderId)
        {
            List<CheckoutViewModel> Order;

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    var query = "select * from Orders as O join Customer as Cu on Cu.Id = O.CustomerId join OrderProducts as Op on O.OrderId = Op.OrderId join Products as P on Op.ProductId = P.Id where O.OrderId = @OrderId";
                    var parameters = new { OrderId = OrderId };
                    Order = connection.Query<CheckoutViewModel>(query, parameters).ToList();
                    ViewBag.TotalSum = Order.Sum(p => p.ProductPrice * p.Quantity);
                } catch (SqlException)
                {
                    return View("Error");
                }
            }
    
            return View(Order);

        }

    }
}