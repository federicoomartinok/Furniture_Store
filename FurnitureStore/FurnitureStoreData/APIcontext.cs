using FurnitureStoreModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureStoreData
{
    public class APIcontext : DbContext
    { 
    
        //dbcontextOptions injecta dependencia de entityframework
        public APIcontext(DbContextOptions options) :base(options) { }
        public DbSet<Client> Clients { get; set; }//Esto es una representacion de la tabla cliente
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite();
        }



    }
}
