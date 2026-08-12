using Microsoft.EntityFrameworkCore;
using PosService.Domain.Cashiers;
using PosService.Domain.Customers;
using PosService.Domain.Registers;
using PosService.Domain.Reporting;
using PosService.Domain.Sales;
using PosService.Domain.Stores;

namespace PosService.Infrastructure.Persistence;

public class PosDbContext(DbContextOptions<PosDbContext> options) : BaseDbContext(options)
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Cashier> Cashiers => Set<Cashier>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<DailySalesReport> DailySalesReports => Set<DailySalesReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PosDbContext).Assembly);
    }
}
