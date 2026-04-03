using CollegeEventPortal.Data;
using CollegeEventPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeEventPortal.Controllers
{
    [Authorize]
    public class CertificateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICertificateService _certificateService;

        public CertificateController(ApplicationDbContext context, ICertificateService certificateService)
        {
            _context = context;
            _certificateService = certificateService;
        }

        public async Task<IActionResult> Download(int id)
        {
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (certificate == null)
                return NotFound();

            var pdfBytes = await _certificateService.GenerateCertificatePdfAsync(certificate);

            return File(pdfBytes, "application/pdf", $"Certificate_{certificate.CertificateNumber}.pdf");
        }

        public async Task<IActionResult> Verify(string certificateNumber)
        {
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);

            if (certificate == null)
            {
                ViewBag.IsValid = false;
                return View();
            }

            ViewBag.IsValid = true;
            return View(certificate);
        }
    }
}
