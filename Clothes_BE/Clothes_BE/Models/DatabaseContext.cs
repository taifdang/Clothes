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
            //producttypes=>categories
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
            //product=>product_options
            modelBuilder.Entity<Products>()
               .HasMany(product => product.product_options)
               .WithOne(patr => patr.products)
               .HasForeignKey(x => x.product_id)
               .OnDelete(DeleteBehavior.Cascade);
            //product=>product_options_images
            modelBuilder.Entity<Products>()
               .HasMany(product => product.product_option_images)
               .WithOne(patr => patr.products)
               .HasForeignKey(x => x.product_id)
               .OnDelete(DeleteBehavior.Cascade);
            //products=>product_variants
            modelBuilder.Entity<Products>()
              .HasMany(product => product.product_variants)
              .WithOne(patr => patr.products)
              .HasForeignKey(x => x.product_id)
              .OnDelete(DeleteBehavior.Cascade);
            //options=>product_options
            modelBuilder.Entity<Options>()
              .HasMany(p => p.product_options)
              .WithOne(patr => patr.options)
              .HasForeignKey(x => x.option_id)
              .OnDelete(DeleteBehavior.Cascade);
            //options=>product_options
            modelBuilder.Entity<Options>()
              .HasMany(p => p.option_values)
              .WithOne(patr => patr.options)
              .HasForeignKey(x => x.option_id)
              .OnDelete(DeleteBehavior.Cascade);
            //product_variants=>variants
            modelBuilder.Entity<ProductVariants>()
              .HasMany(p => p.variants)
              .WithOne(pv => pv.product_variants)
              .HasForeignKey(x => x.product_variant_id)
              .OnDelete(DeleteBehavior.Cascade);
            //option_values=>variants
            modelBuilder.Entity<OptionValues>()
              .HasMany(p => p.variants)
              .WithOne(pv => pv.option_values)
              .HasForeignKey(x => x.option_value_id)
              .OnDelete(DeleteBehavior.Cascade);
            //option_values=>product_option_images
            modelBuilder.Entity<OptionValues>()
              .HasMany(v => v.product_option_images)
              .WithOne(v => v.options_values)
              .HasForeignKey(s => s.option_value_id)
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
