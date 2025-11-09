namespace Domain.IServices
{
    public interface ISlotGeneratorService
    {
        Task<(bool Success, string Message)> GenerateSlotsForCurrentDoctorAsync(int appUserId,DateTime startUtc,DateTime endUtc);
        Task<(bool Success, string Message, object? Data)>
           GetAvailableSlotsAsync(int doctorId, DateTime date);
    }
}
