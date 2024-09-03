using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ManutencaoFrotaWeb.Models;

namespace ManutencaoFrotaWeb.Controllers
{
    public class AutenticacaoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AutenticacaoController> _logger;

        public AutenticacaoController(IHttpClientFactory httpClientFactory, ILogger<AutenticacaoController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var loginData = new
                {
                    usuario = loginViewModel.Usuario,
                    senha = loginViewModel.Senha
                };
                var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7289/api/Autenticacao/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(jsonResponse);

                    // Armazena o token JWT no localStorage ou em um cookie seguro
                    HttpContext.Session.SetString("JwtToken", loginResponse.Token);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogError($"Falha no login. Status Code: {response.StatusCode}. Body: {await response.Content.ReadAsStringAsync()}");
                    ModelState.AddModelError(string.Empty, "Erro ao realizar login. Por favor, tente novamente.");
                    return View(loginViewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro durante a tentativa de login.");
                ModelState.AddModelError(string.Empty, "Erro ao realizar login. Por favor, tente novamente.");
                return View(loginViewModel);
            }
        }

        public IActionResult Login()
        {
            // Verifica se o usuário já está autenticado
            if (HttpContext.Session.GetString("JwtToken") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Login", "Autenticacao");
        }
    }
}
