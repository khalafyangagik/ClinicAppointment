using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DbContextFolder
{
    public class ClinicDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<Reminder> Reminders => Set<Reminder>();

        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Doctor ↔ AppUser
            b.Entity<Doctor>()
                .HasOne(d => d.AppUser)
                .WithMany()
                .HasForeignKey(d => d.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient ↔ AppUser
            b.Entity<Patient>()
                .HasOne(p => p.AppUser)
                .WithMany()
                .HasForeignKey(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Clinic ↔ Doctor
            b.Entity<Doctor>()
                .HasOne(d => d.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Doctor ↔ Patient (implicit N:M)
            b.Entity<Doctor>()
     .HasMany(d => d.Patients)
     .WithMany(p => p.Doctors)
     .UsingEntity<Dictionary<string, object>>(
         "DoctorPatients",
         j => j
             .HasOne<Patient>()
             .WithMany()
             .HasForeignKey("PatientsId")
             .OnDelete(DeleteBehavior.Restrict), // ❗ change cascade → restrict
         j => j
             .HasOne<Doctor>()
             .WithMany()
             .HasForeignKey("DoctorsId")
             .OnDelete(DeleteBehavior.Cascade)   // keep cascade only on one side
     );

            // Appointment ↔ Doctor
            b.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment ↔ Patient
            b.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment ↔ Slot
            b.Entity<Appointment>()
                .HasOne(a => a.Slot)
                .WithOne()
                .HasForeignKey<Appointment>(a => a.SlotId)
                .OnDelete(DeleteBehavior.SetNull);

            // Appointment ↔ Note (1:1)
            b.Entity<Note>()
                .HasKey(n => n.AppointmentId);
            b.Entity<Note>()
                .HasOne(n => n.Appointment)
                .WithOne(a => a.Note)
                .HasForeignKey<Note>(n => n.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment ↔ Reminder (1:N)
            b.Entity<Reminder>()
                .HasOne(r => r.Appointment)
                .WithMany(a => a.Reminders)
                .HasForeignKey(r => r.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint to prevent double-booking
            b.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.StartUtc })
                .IsUnique();
        }
    }
}
