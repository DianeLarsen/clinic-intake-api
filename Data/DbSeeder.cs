using ClinicIntakeApi.Models;
using ClinicIntakeApi.Services;

namespace ClinicIntakeApi.Data;

//
// Development Seed / Backfill Data
//
// This block creates sample clinics, patients, and intake requests
// for local development.
//
// Clinics are created first because patients require a valid ClinicId.
// Patients are created next because intake requests require a valid PatientId.
// In production, seeding and backfilling would be done differently, likely as part of a data migration or separate seeding script.
//
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();

        // Get the database context.
        ClinicIntakeDbContext db =
            scope.ServiceProvider.GetRequiredService<ClinicIntakeDbContext>();

        // Create the database and tables if they do not exist.
        await db.Database.EnsureCreatedAsync();

        // Get the service used to create and update requests.
        IIntakeService intakeService = scope.ServiceProvider.GetRequiredService<IIntakeService>();

        // The seeder is trusted application-maintenance code,
        // so it may check whether any clinic has existing requests.
        bool requestsExist = db.IntakeRequests.Any();

        //
        // Ensure default clinics exist.
        //
        // Clinics are parent records.
        // A clinic can exist even if it has no patients or requests yet.
        //
        if (!db.Clinics.Any())
        {
            db.Clinics.AddRange(
                new Clinic { Name = "Monroe Clinic" },
                new Clinic { Name = "Everett Clinic" },
                new Clinic { Name = "Seattle Clinic" }
            );

            await db.SaveChangesAsync();
        }

        Clinic[] clinics = db.Clinics.ToArray();

        //
        // Sample patient identity data used for development.
        //
        var patientSeeds = new[]
        {
            new
            {
                FirstName = "Alice",
                LastName = "Johnson",
                DateOfBirth = new DateOnly(1984, 3, 12),
            },
            new
            {
                FirstName = "Bob",
                LastName = "Smith",
                DateOfBirth = new DateOnly(1979, 7, 22),
            },
            new
            {
                FirstName = "Charlie",
                LastName = "Brown",
                DateOfBirth = new DateOnly(1991, 11, 5),
            },
            new
            {
                FirstName = "Diane",
                LastName = "Larsen",
                DateOfBirth = new DateOnly(1978, 6, 14),
            },
            new
            {
                FirstName = "Emma",
                LastName = "Davis",
                DateOfBirth = new DateOnly(2002, 5, 9),
            },
            new
            {
                FirstName = "Frank",
                LastName = "Miller",
                DateOfBirth = new DateOnly(1968, 12, 1),
            },
        };

        //
        // Ensure default patients exist.
        //
        // Patients are child records of clinics.
        // Every patient must belong to a valid clinic.
        //
        if (!db.Patients.Any())
        {
            Patient[] patientsToCreate = patientSeeds
                .Select(
                    (patient, index) =>
                        new Patient
                        {
                            FirstName = patient.FirstName,
                            LastName = patient.LastName,
                            DateOfBirth = patient.DateOfBirth,

                            // Rotate patients across available clinics.
                            ClinicId = clinics[index % clinics.Length].Id,
                        }
                )
                .ToArray();

            db.Patients.AddRange(patientsToCreate);

            await db.SaveChangesAsync();
        }

        //
        // Load patients after ensuring they exist.
        //
        // These are used to create sample intake requests.
        //
        Patient[] patients = db.Patients.ToArray();

        //
        // If there are no requests at all, create sample intake requests.
        //
        // This runs only for a brand-new database.
        //
        if (!requestsExist)
        {
            for (int i = 0; i < patients.Length; i++)
            {
                Patient patient = patients[i];

                IntakeRequest? request = await intakeService.AddRequestAsync(
                    patient.Id,
                    patient.ClinicId
                );

                if (request is null)
                {
                    continue;
                }

                //
                // Give some requests different statuses
                // so filtering, sorting, and searching have useful sample data.
                //
                if (i % 3 == 0)
                {
                    await intakeService.UpdateStatusAsync(
                        request.Id,
                        RequestStatus.InReview,
                        patient.ClinicId
                    );
                }
                else if (i % 5 == 0)
                {
                    await intakeService.UpdateStatusAsync(
                        request.Id,
                        RequestStatus.Completed,
                        patient.ClinicId
                    );
                }
            }
        }
    }
}
