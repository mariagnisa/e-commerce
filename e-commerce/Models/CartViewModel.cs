using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using e_commerce.Models;

namespace e_commerce.Models
{
    public class CartViewModel
    {
        public string CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }    

        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int ProductPrice { get; set; }
        public string ImgPath { get; set; }
    }
}