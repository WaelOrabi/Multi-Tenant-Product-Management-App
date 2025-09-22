using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using MultiTenantProductManagementApp.Products;
using MultiTenantProductManagementApp.Stocks;

namespace MultiTenantProductManagementApp.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class MultiTenantProductManagementAppDbContext :
    AbpDbContext<MultiTenantProductManagementAppDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{

    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = default!;
    public DbSet<Stock> Stocks { get; set; } = default!;
    public DbSet<StockProduct> StockProducts { get; set; } = default!;
    public DbSet<StockProductVariant> StockProductVariants { get; set; } = default!;

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public MultiTenantProductManagementAppDbContext(DbContextOptions<MultiTenantProductManagementAppDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();


        builder.Entity<Product>(b =>
        {
            b.ToTable(MultiTenantProductManagementAppConsts.DbTablePrefix + "Products", MultiTenantProductManagementAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Category).HasMaxLength(64);
            b.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
            b.HasMany(x => x.Variants).WithOne().HasForeignKey(v => v.ProductId).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<ProductVariant>(b =>
        {
            b.ToTable(MultiTenantProductManagementAppConsts.DbTablePrefix + "ProductVariants", MultiTenantProductManagementAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Sku).HasMaxLength(64);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.Color).HasMaxLength(50);
            b.Property(x => x.Size).HasMaxLength(20);
            b.HasIndex(x => new { x.TenantId, x.ProductId, x.Sku });
        });

        builder.Entity<Stock>(b =>
        {
            b.ToTable(MultiTenantProductManagementAppConsts.DbTablePrefix + "Stocks", MultiTenantProductManagementAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
          
            b.HasIndex(x => new { x.TenantId, x.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

        });

        builder.Entity<StockProduct>(b =>
        {
            b.ToTable(MultiTenantProductManagementAppConsts.DbTablePrefix + "StockProducts", MultiTenantProductManagementAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<Stock>()
                .WithMany(s => s.Products)
                .HasForeignKey(sp => sp.StockId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne<Product>()
                .WithMany()
                .HasForeignKey(sp => sp.ProductId)
                .IsRequired();

            b.HasIndex(x => new { x.TenantId, x.StockId, x.ProductId }).IsUnique();
        });

        builder.Entity<StockProductVariant>(b =>
        {
            b.ToTable(MultiTenantProductManagementAppConsts.DbTablePrefix + "StockProductVariants", MultiTenantProductManagementAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Quantity).IsRequired();

            b.HasOne<StockProduct>()
                .WithMany(sp => sp.Variants)
                .HasForeignKey(v => v.StockProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne<ProductVariant>()
                .WithMany()
                .HasForeignKey(v => v.ProductVariantId)
                .IsRequired(false);

            b.HasIndex(x => new { x.TenantId, x.StockProductId, x.ProductVariantId }).IsUnique();
        });
    }
}
