using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureStoreModels
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; } // Deberia haber una tabla especifica para precios
        public int ProductCatrgoryId { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
