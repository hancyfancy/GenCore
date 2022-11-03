using GenCore.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCore.Data.Models
{
    public class Product
    {
        public long ProductId { get; set; }

        //max 100 characters
        public string Name { get; set; }

        //2 decimals
        public decimal Price { get; set; }

        public ProductTypeEnum Type { get; set; }

        public bool Active { get; set; }
    }
}
