using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface ISlotGeneratorService
    {
        Task GenerateSlotsAsync(int doctorId, DateTime startUtc, DateTime endUtc);
        Task<(bool Success, string Message, object? Data)>
           GetAvailableSlotsAsync(int doctorId, DateTime date);
    }
}
