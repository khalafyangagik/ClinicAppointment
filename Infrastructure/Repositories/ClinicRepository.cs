using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;

namespace Infrastructure.Repositories
{
    public class ClinicRepository : GenericRepository<Clinic>, IClinicRepository
    {
        public ClinicRepository(ClinicDbContext context)
            : base(context)
        {
        }
    }
}