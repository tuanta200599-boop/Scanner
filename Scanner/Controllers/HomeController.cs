using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Models;
using Scanner.Services;
using System.Diagnostics;

namespace Scanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Install()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> SubscribeToTopic(string token, string topics)
        {
            try
            {
                // Create the request object that the backend API expects in the body
                var requestData = new
                {
                    Token = token,
                    Topics = topics
                };

                // Proxy the request to the backend API
                // The backend API expects a SubscribeTopicRequest object in the body
                var result = await _apiService.PostAsync<object>("SubscribeToTopic", requestData);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topic: {Token}, {Topics}", token, topics);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
