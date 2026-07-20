using ClinicIntakeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicIntakeApi.Data;

public class ClinicIntakeDbContext : DbContext
{
    public ClinicIntakeDbContext(DbContextOptions<ClinicIntakeDbContext> options)
        : base(options) { }

    public DbSet<IntakeRequest> IntakeRequests => Set<IntakeRequest>();

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Do not allow a clinic to be deleted while it still has patients.
    modelBuilder.Entity<Patient>()
        .HasOne(patient => patient.Clinic)
        .WithMany(clinic => clinic.Patients)
        .HasForeignKey(patient => patient.ClinicId)
        .OnDelete(DeleteBehavior.Restrict);

    // Do not allow a clinic to be deleted while it still has intake requests.
    modelBuilder.Entity<IntakeRequest>()
        .HasOne(request => request.Clinic)
        .WithMany(clinic => clinic.Requests)
        .HasForeignKey(request => request.ClinicId)
        .OnDelete(DeleteBehavior.Restrict);
}
}
