using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography.Xml;

namespace Clothes_BE.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        //Fluent API
        public DbSet<ProductTypes> product_types { get; set; }
        public DbSet<Categories> categories { get; set; }
        public DbSet<Products> products { get; set; }       
        public DbSet<Options> options { get; set; }
        public DbSet<ProductOptions> product_options { get; set; }
        public DbSet<OptionValues> option_values { get; set; }
        public DbSet<ProductVariants> product_variants { get; set; } 
        public DbSet<ProductOptionImages> product_option_images { get; set; }
        public DbSet<Variants> variants { get; set; }
        public DbSet<Users> users { get; set; }
        public DbSet<Carts> carts { get; set; }
        public DbSet<CartItems> cart_items { get; set; }
        public DbSet<Orders> orders { get; set; }
        public DbSet<OrderDetail> order_detail { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
            //product_types=>categories
            modelBuilder.Entity<ProductTypes>()
              .HasMany(cate => cate.categories)
              .WithOne(type => type.product_types)
              .HasForeignKey(x => x.product_types_id)
              .OnDelete(DeleteBehavior.Cascade);
            //categories=>products
            modelBuilder.Entity<Categories>()
              .HasMany(product => product.products)
              .WithOne(cate => cate.categories)
              .HasForeignKey(x => x.category_id)
              .OnDelete(DeleteBehavior.Cascade);        
            //products=>product_variants
            modelBuilder.Entity<Products>()
             .HasMany(product => product.product_variants)
             .WithOne(patr => patr.products)
             .HasForeignKey(x => x.product_id)
             .OnDelete(DeleteBehavior.Cascade);
            //constraint product_options=>products=>options
            modelBuilder.Entity<ProductOptions>()
             .HasKey(p => new { p.product_id, p.option_id });
            modelBuilder.Entity<ProductOptions>()
             .HasOne(p => p.products)
             .WithMany(pv => pv.product_options)
             .HasForeignKey(x => x.product_id)
             .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductOptions>()
             .HasOne(p => p.options)
             .WithMany(pv => pv.product_options)
             .HasForeignKey(x => x.option_id)
             .OnDelete(DeleteBehavior.Cascade);                 
            //options=>option_values
            modelBuilder.Entity<Options>()
             .HasMany(p => p.option_values)
             .WithOne(patr => patr.options)
             .HasForeignKey(x => x.option_id)
             .OnDelete(DeleteBehavior.Cascade);
            //constraint varinats=>product_variants=>option_values
            modelBuilder.Entity<Variants>()
             .HasKey(p => new { p.product_variant_id, p.option_value_id });
            modelBuilder.Entity<Variants>()
             .HasOne(p => p.product_variants)
             .WithMany(pv => pv.variants)
             .HasForeignKey(x => x.product_variant_id)
             .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Variants>()
             .HasOne(p => p.option_values)
             .WithMany(pv => pv.variants)
             .HasForeignKey(x => x.option_value_id)
             .OnDelete(DeleteBehavior.Cascade);
            //options_values=>product_option_images??
            modelBuilder.Entity<ProductOptionImages>()
             .HasOne(v => v.options_values)
             .WithMany(v => v.product_option_images)
             .HasForeignKey(s => s.option_value_id)
             .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductOptionImages>()
            .HasOne(v => v.products)
            .WithMany(v => v.product_option_images)
            .HasForeignKey(s => s.product_id)
            .OnDelete(DeleteBehavior.Cascade);
            //users=>carts
            modelBuilder.Entity<Users>()
             .HasMany(p => p.carts)
             .WithOne(s => s.users)
             .HasForeignKey(x => x.user_id)
             .OnDelete(DeleteBehavior.Cascade);
            //users=>orders
            modelBuilder.Entity<Users>()
             .HasMany(p => p.orders)
             .WithOne(s => s.users)
             .HasForeignKey(x => x.user_id)
             .OnDelete(DeleteBehavior.Cascade);
            //cartitems=>cart
            modelBuilder.Entity<CartItems>()
             .HasOne(p=>p.carts)
             .WithMany(x=>x.cartItems)
             .HasForeignKey(s=>s.cart_id)
             .OnDelete(DeleteBehavior.Cascade);
            //cartitems=>cart
            modelBuilder.Entity<CartItems>()
             .HasOne(p => p.product_variants)
             .WithMany(x => x.cart_items)
             .HasForeignKey(s => s.product_variant_id)
             .OnDelete(DeleteBehavior.Cascade);
            //cartitems=>cart
            modelBuilder.Entity<OrderDetail>()
             .HasOne(p => p.orders)
             .WithMany(x => x.order_detail)
             .HasForeignKey(s => s.order_id)
             .OnDelete(DeleteBehavior.Cascade);
            //cartitems=>cart
            modelBuilder.Entity<OrderDetail>()
             .HasOne(p => p.product_variants)
             .WithMany(x => x.order_detail)
             .HasForeignKey(s => s.product_variant_id)
             .OnDelete(DeleteBehavior.Cascade);



        }

    }
}
