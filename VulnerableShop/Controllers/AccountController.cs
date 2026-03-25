using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace VulnerableShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // ZAFİYET 1: SQL Injection (Login bypass için ideal: admin' --)
            // ZAFİYET 2: Broken Authentication (Zayıf şifreleme/kontrol yok)
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Users WHERE Username = '" + username + "' AND Password = '" + password + "'";
                var cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, reader["Username"].ToString()),
                            new Claim(ClaimTypes.Role, reader["Role"].ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult Profile()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");
            return View();
        }
    }
}
