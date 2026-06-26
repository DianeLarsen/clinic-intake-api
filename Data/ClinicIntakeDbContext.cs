using ClinicIntakeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicIntakeApi.Data;

public class ClinicIntakeDbContext : DbContext
{
    public ClinicIntakeDbContext(DbContextOptions<ClinicIntakeDbContext> options)
        : base(options) { }

    public DbSet<IntakeRequest> IntakeRequests => Set<IntakeRequest>();
}
