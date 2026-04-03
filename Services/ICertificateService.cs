using CollegeEventPortal.Models;

namespace CollegeEventPortal.Services
{
    public interface ICertificateService
    {
        Task<byte[]> GenerateCertificatePdfAsync(Certificate certificate);
        Task<string> GenerateQRCodeAsync(string data);
        Task<Certificate> CreateCertificateAsync(string userId, int eventId, int rank);
    }
}
