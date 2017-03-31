using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_commerce.Models
{

    public class CheckoutViewModel
    {
        //order info
        public string OrderId { get; set; }
        public int Orderstatus { get; set; }
        public DateTime OrderDate { get; set; }
        public string CartId { get; set; }

        //order product info
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int ProductPrice { get; set; }
        public int Price { get; set; }
        
        //customer info
        public int CustomerId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }

    }
}