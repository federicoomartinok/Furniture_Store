using FurnitureStoreModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureStoreData
{
    public class APIcontext : IdentityDbContext
    {

        //dbcontextOptions injecta dependencia de entityframework
        //Son las tablas que tiene la base de datos
        public APIcontext(DbContextOptions options) : base(options) { }
        public DbSet<Client> Clients { get; set; }//Esto es una representacion de la tabla cliente
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; } 
        public DbSet<OrderDetail> OrderDetails{ get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => new {od.OrderId,od.ProductId});
        }



    }
}
