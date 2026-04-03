using CollegeEventPortal.Data;
using CollegeEventPortal.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace CollegeEventPortal.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CertificateService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<Certificate> CreateCertificateAsync(string userId, int eventId, int rank)
        {
            var user = await _context.Users.FindAsync(userId);
            var eventData = await _context.Events.FindAsync(eventId);

            if (user == null || eventData == null)
                throw new ArgumentException("User or Event not found");

            var certificateNumber = $"CERT-{eventId}-{userId}-{DateTime.UtcNow.Ticks}";
            var qrCode = await GenerateQRCodeAsync(certificateNumber);

            var certificate = new Certificate
            {
                UserId = userId,
                EventId = eventId,
                CertificateNumber = certificateNumber,
                Rank = rank,
                QRCode = qrCode,
                IssuedAt = DateTime.UtcNow
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return certificate;
        }

        public async Task<byte[]> GenerateCertificatePdfAsync(Certificate certificate)
        {
            var user = await _context.Users.FindAsync(certificate.UserId);
            var eventData = await _context.Events.FindAsync(certificate.EventId);

            using var ms = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            // Add certificate content
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 32, new BaseColor(64, 64, 64));
            var nameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, new BaseColor(0, 0, 0));
            var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, new BaseColor(0, 0, 0));

            // Title
            var title = new Paragraph("CERTIFICATE OF ACHIEVEMENT", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 30
            };
            document.Add(title);

            // Participant name
            var name = new Paragraph(user?.FullName ?? "Participant", nameFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(name);

            // Event details
            var eventInfo = new Paragraph($"For securing Rank {certificate.Rank} in\n{eventData?.Name}", bodyFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(eventInfo);

            // Date
            var date = new Paragraph($"Date: {certificate.IssuedAt:dd MMMM yyyy}", bodyFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 30
            };
            document.Add(date);

            // Certificate number
            var certNumber = new Paragraph($"Certificate No: {certificate.CertificateNumber}", bodyFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(certNumber);

            document.Close();
            return ms.ToArray();
        }

        public async Task<string> GenerateQRCodeAsync(string data)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new QRCode(qrCodeData);
                    using var qrCodeImage = qrCode.GetGraphic(20);
                    
                    using var ms = new MemoryStream();
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    return Convert.ToBase64String(ms.ToArray());
                }
                catch
                {
                    return string.Empty;
                }
            });
        }
    }
}
