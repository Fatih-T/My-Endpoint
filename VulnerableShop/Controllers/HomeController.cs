using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VulnerableShop.Models;
using System.Diagnostics;
using System.IO;

namespace VulnerableShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

        public HomeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _env = env;
        }

        // 1. SQL Injection & XSS (Arama Modülü)
        public IActionResult Index(string query)
        {
            var products = new List<Product>();
            if (!string.IsNullOrEmpty(query))
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    // ZAFİYET: SQL Injection (Ham string birleştirme)
                    string sql = "SELECT * FROM Products WHERE ProductName LIKE '%" + query + "%'";
                    var cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductId = (int)reader["ProductId"],
                                ProductName = reader["ProductName"].ToString(),
                                Price = (decimal)reader["Price"]
                            });
                        }
                    }
                }
                // ZAFİYET: Reflected XSS (Encode edilmeden arama terimi ekrana basılacak)
                ViewBag.SearchQuery = query;
            }
            return View(products);
        }

        // 2. Path Traversal (Dosya İndirme)
        public IActionResult Download(string fileName)
        {
            // ZAFİYET: Path Traversal (Parametre sanitize edilmiyor)
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            return NotFound();
        }

        // 3. Command Injection (RCE - w3wp.exe Child Process Test)
        public IActionResult Ping(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return View();

            // ZAFİYET: Command Injection (Doğrudan shell komutuna ekleniyor)
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c ping " + ip;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            var process = Process.Start(psi);
            string result = process.StandardOutput.ReadToEnd();
            ViewBag.PingResult = result;

            return View();
        }

        // 4. Insecure File Upload
        [HttpPost]
        public async Task<IActionResult> UploadProductImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // ZAFİYET: Dosya uzantısı kontrolü yok (.aspx, .exe vb. yüklenebilir)
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                var filePath = Path.Combine(uploads, file.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                ViewBag.Message = "File uploaded to: " + filePath;
            }
            return View();
        }
    }
}
