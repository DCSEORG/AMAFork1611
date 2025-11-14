using Microsoft.EntityFrameworkCore;
using ExpenseManagementApp.Models;

namespace ExpenseManagementApp.Data;

public class ExpenseDbContext : DbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<ExpenseStatus> ExpenseStatus { get; set; }
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.HasIndex(e => e.RoleName).IsUnique();
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Manager)
                .WithMany(u => u.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ExpenseCategory
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.ToTable("ExpenseCategories");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.CategoryName).IsUnique();
        });

        // Configure ExpenseStatus
        modelBuilder.Entity<ExpenseStatus>(entity =>
        {
            entity.ToTable("ExpenseStatus");
            entity.HasKey(e => e.StatusId);
            entity.Property(e => e.StatusName).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.StatusName).IsUnique();
        });

        // Configure Expense
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");
            entity.HasKey(e => e.ExpenseId);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ReceiptFile).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Status)
                .WithMany(s => s.Expenses)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reviewer)
                .WithMany(u => u.ReviewedExpenses)
                .HasForeignKey(e => e.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UserId, e.StatusId });
            entity.HasIndex(e => e.SubmittedAt);
        });
    }
}
