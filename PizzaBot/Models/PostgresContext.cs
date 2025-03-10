using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PizzaBot.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext() {}

    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options) {}

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderProduct> OrderProducts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductIngredient> ProductIngredients { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=900440");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_catalog", "adminpack");

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ingredients_pkey");

            entity.ToTable("ingredients");

            entity.HasIndex(e => e.Name, "ingredients_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.CourierId).HasColumnName("courier_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveryCost)
                .HasColumnType("money")
                .HasColumnName("delivery_cost");
            entity.Property(e => e.PerformedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("performed_at");
            entity.Property(e => e.ProductsCost)
                .HasColumnType("money")
                .HasColumnName("products_cost");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Обрабатывается'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Client).WithMany(p => p.OrderClients)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("orders_client_id_fkey");

            entity.HasOne(d => d.Courier).WithMany(p => p.OrderCouriers)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("orders_courier_id_fkey");
        });

        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId }).HasName("order_products_pkey");

            entity.ToTable("order_products");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Amount).HasColumnName("amount");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_products_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("order_products_product_id_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products");

            entity.HasIndex(e => e.Name, "products_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(10)
                .HasColumnName("category");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.Weight).HasColumnName("weight");
        });

        modelBuilder.Entity<ProductIngredient>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.IngredientId }).HasName("product_ingredients_pkey");

            entity.ToTable("product_ingredients");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.Amount).HasColumnName("amount");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.ProductIngredients)
                .HasForeignKey(d => d.IngredientId)
                .HasConstraintName("product_ingredients_ingredient_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductIngredients)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_ingredients_product_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
