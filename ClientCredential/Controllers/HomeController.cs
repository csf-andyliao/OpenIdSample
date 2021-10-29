using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClientCredential.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClientCredential.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetWeatherForecast()
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpContent content = new StringContent("grant_type=client_credentials&client_id=client_credential&client_secret=client_credential&scope=weather_api_1", 
                                                               Encoding.UTF8, 
                                                               "application/x-www-form-urlencoded"))
                {
                    HttpResponseMessage authResponse = await client.PostAsync("https://localhost:44351/connect/token", content);
                    //return Ok(await authResponse.Content.ReadAsStringAsync());
                    Token token = JsonSerializer.Deserialize<Token>(await authResponse.Content.ReadAsStringAsync());
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token.token_type, token.access_token);
                    HttpResponseMessage responseMessage = await client.GetAsync("https://localhost:44351/WeatherForecast");
                    string weatherForecast = await responseMessage.Content.ReadAsStringAsync();
                    return Ok(weatherForecast);
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
