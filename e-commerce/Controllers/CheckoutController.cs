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
            
            

            using (var connection = new SqlConnection(this.connectionString))
            {
                //start sql transaction 
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                transaction = connection.BeginTransaction("orderTransaction");
                // assign the command to local transaction
                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    // add customer to db 
                    command.CommandText = "insert into Customer (Firstname, Lastname, Email, Phone, Street, PostalCode, City) values (@Firstname, @Lastname, @Email, @Phone, @Street, @PostalCode, @City)";     
                    command.Parameters.AddWithValue("@Firstname", Firstname);
                    command.Parameters.AddWithValue("@Lastname", Lastname);
                    command.Parameters.AddWithValue("@Email", Email);
                    command.Parameters.AddWithValue("@Phone", Phone);
                    command.Parameters.AddWithValue("@Street", Street);
                    command.Parameters.AddWithValue("@PostalCode", PostalCode);
                    command.Parameters.AddWithValue("@City", City);
                    command.ExecuteNonQuery();
                 

                    // add order to db 
                    command.CommandText = "insert into Orders (Orderstatus, CustomerId, CartId) values (@Orderstatus, (SELECT MAX(Id) from Customer), @AddCartId)";
                    command.Parameters.AddWithValue("@Orderstatus", 1);
                    command.Parameters.AddWithValue("AddCartId", CartId);
                    command.ExecuteNonQuery();
                    
                    
                    //get products
                    command.CommandText = "select C.ProductId, C.Quantity, P.ProductPrice from Cart as C join Products as P on C.ProductId = P.Id where C.CartId = @CartId";
                    command.Parameters.AddWithValue("@CartId", CartId);
                    var products = new List<CheckoutViewModel>();
                    SqlDataReader ProductsReader = command.ExecuteReader();
                    //reader loops through all the cart items 
                    while (ProductsReader.Read())
                    {
                        var product = new CheckoutViewModel();
                        product.ProductId = Int32.Parse(ProductsReader["ProductId"].ToString());
                        product.Quantity = Int32.Parse(ProductsReader["Quantity"].ToString());
                        product.ProductPrice = Int32.Parse(ProductsReader["ProductPrice"].ToString());
                        products.Add(product);
                    }
                    //close reader
                    ProductsReader.Close();

                //add products to order item in db
                var count = 0;
                    foreach (CheckoutViewModel CartProduct in products)
                    {
                        command.CommandText = "insert into OrderProducts (OrderId, ProductId, Quantity, Price) values ((SELECT MAX(OrderId) from Orders), @ProductId" + count + ", @Quantity" + count + ", @Price" + count + ")";
                        command.Parameters.AddWithValue("@ProductId" + count + "", CartProduct.ProductId);
                        command.Parameters.AddWithValue("@Quantity" + count + "", CartProduct.Quantity);
                        command.Parameters.AddWithValue("@Price" + count + "", CartProduct.ProductPrice);
                        command.ExecuteNonQuery();
                        count++;
                    }
        
                    //delete cart from db
                    command.CommandText = "delete from Cart where CartId = @deleteCartId";
                    command.Parameters.AddWithValue("@deleteCartId", CartId);
                    command.ExecuteNonQuery();

                    //get last inserted id from orders
                    command.CommandText = "select MAX(OrderId) from Orders";
                    OrderId = Convert.ToInt32(command.ExecuteScalar());

                    // delete cart cookie
                    var CartCookie = new HttpCookie("shoppingCart");
                    CartCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(CartCookie);

                    transaction.Commit();

                } catch (Exception)
                {
                    transaction.Rollback();
                    return View("Error");
                }              
            }

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