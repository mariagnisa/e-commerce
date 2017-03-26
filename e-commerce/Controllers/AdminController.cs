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
        [HttpGet]
        public ActionResult Index()
        {
            //Check session if admin is logged in or not
            if (Session["admin"] != null)
            {
                return RedirectToAction("AddProduct");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(string Username, string Password)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                //Validate username and password input against the db
                var query = "select * from Admin where Username = @username and Password = @password";
                var parameters = new { username = Username, password = Password };
                var authAdmin = connection.QuerySingleOrDefault(query, parameters);

                //If correct password and username redirect to AddProduct, otherwise throw alert box
                if (authAdmin != null)
                {
                    Session["admin"] = authAdmin;

                    return RedirectToAction("AddProduct");
                }   
            }
            System.Web.HttpContext.Current.Response.Write("<script>alert('Wrong username or password. Please try again.');</script>");
            return View();
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            //check if the admin user is logged in, otherwise redirect to login form
            if (Session["admin"] != null)
            {
                return View();              
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddProduct(ProductsViewModel model, HttpPostedFileBase file)
        {
            
            using (var connection = new SqlConnection(this.connectionString))
            {
                //verify that a file is selected
                if (file != null && file.ContentLength > 0)
                {
                    //extract the filename and adds the current timestamp
                    var fileName = Environment.TickCount + "_" + file.FileName;
                    //Save the file to given path
                    file.SaveAs(Server.MapPath("~\\Content\\img\\") + fileName);

                    var insert = "insert into Products (ProductName, ProductDescription, ProductPrice, ImgPath) values (@Name, @Description, @Price, @Path)";
                    var parameters = new { Name = model.ProductName, Description = model.ProductDescription, Price = model.ProductPrice, Path = fileName };
                    connection.Execute(insert, parameters);
                }
            }
            return RedirectToAction("AddProduct");
        }

    }
}