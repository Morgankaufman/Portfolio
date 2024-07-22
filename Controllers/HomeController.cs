using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Diagnostics;

namespace Portfolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> SendContactForm(ContactFormModel contactForm)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", contactForm);
            }

            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);

                var from = new EmailAddress("morgankaufman13@gmail.com", "Morgan Kaufman");
                var subject = "New Contact Form Submission";
                var to = new EmailAddress("morgankaufman13@gmail.com", "Morgan Kaufman");
                var plainTextContent = $"Name: {contactForm.Name}\nEmail: {contactForm.Email}\nMessage: {contactForm.Message}";
                var htmlContent = $"<strong>Name:</strong> {contactForm.Name}<br><strong>Email:</strong> {contactForm.Email}<br><strong>Message:</strong> {contactForm.Message}";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var response = await client.SendEmailAsync(msg);
                if ((int)response.StatusCode >= 400)
                {
                    throw new Exception($"Failed to send email. Status code: {response.StatusCode}");
                }

                return RedirectToAction("Index", new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact form email.");
                ModelState.AddModelError(string.Empty, "There was an error sending your message. Please try again later.");
                return View("Index", contactForm);
            }
        }

    }
}
