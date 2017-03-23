using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_commerce.Models
{
    public class CartViewModel
    {
        public string CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }    
    }
}