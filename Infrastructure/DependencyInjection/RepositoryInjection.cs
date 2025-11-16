using Domain.IRepository;
using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection
{
    public static class RepositoryInjection
    {
        public static IServiceCollection AddAppRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Custom Repositories
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IClinicRepository, ClinicRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<ISlotRepository, SlotRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();

            // Optional (if you still use base Generic repository somewhere)
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

            return services;
        }
    }
}
