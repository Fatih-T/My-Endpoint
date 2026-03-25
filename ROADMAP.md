# .NET Siber Güvenlik Laboratuvarı - Kurulum ve Test Rehberi

Bu proje, Carbon Black ve diğer EDR çözümlerinin `w3wp.exe` (IIS Worker Process) üzerindeki davranışlarını analiz etmek için tasarlanmış zafiyetli bir .NET 8.0 MVC uygulamasıdır.

## 1. Veritabanı Kurulumu (MSSQL)
1. MSSQL Server Management Studio (SSMS) açın.
2. `init_db.sql` dosyasındaki sorguları çalıştırarak `ECommerceDB` veritabanını ve tabloları oluşturun.
3. `VulnerableShop/appsettings.json` dosyasındaki `ConnectionStrings` bölümünü kendi MSSQL bilgilerinizle güncelleyin.

## 2. IIS Üzerinde Yayına Alma
1. Projeyi `dotnet publish -c Release -o ./publish` komutuyla derleyin.
2. IIS Manager'ı açın.
3. Yeni bir Web Sitesi oluşturun ve Physical Path olarak `publish` klasörünü gösterin.
4. Application Pool ayarlarında .NET CLR Version olarak "No Managed Code" seçili olduğundan emin olun (ASP.NET Core için).
5. Yazma yetkisi: `wwwroot/uploads` klasörüne IIS AppPool\<SiteAdi> kullanıcısı için yazma yetkisi verin.

## 3. Test Senaryoları & Carbon Black Analizi

### Senaryo A: Command Injection (Child Process Analizi)
- **URL:** `/Home/Ping`
- **Saldırı:** `8.8.8.8 & whoami` veya `8.8.8.8 & powershell -c "Invoke-WebRequest ..."`
- **EDR Gözlemi:** `w3wp.exe`'nin altında `cmd.exe` veya `powershell.exe` süreçlerinin oluştuğunu görmelisiniz. Carbon Black bunu "Suspicious Child Process" olarak işaretleyecektir.

### Senaryo B: SQL Injection
- **URL:** `/Home/Index?query='`
- **Saldırı:** `' UNION SELECT * FROM Users--`
- **EDR Gözlemi:** Uygulamanın veritabanı ile ham SQL üzerinden konuşması ve sonucunda sızan veriler.

### Senaryo C: Insecure File Upload
- **URL:** `/Home/UploadProductImage`
- **Saldırı:** `.aspx` veya `.exe` uzantılı bir dosya yüklemeyi deneyin.
- **EDR Gözlemi:** Dosya sisteme yazıldığında "File Write" eventlerini ve eğer çalıştırılırsa `w3wp.exe` tarafından tetiklenen yürütülebilir dosyaları takip edin.

### Senaryo D: Path Traversal
- **URL:** `/Home/Download?fileName=../../appsettings.json`
- **Gözlem:** Hassas yapılandırma dosyalarına erişim.

## 4. Carbon Black İçin İpucu
`w3wp.exe` genellikle sadece ağ trafiği oluşturması beklenen bir süreçtir. Eğer bu sürecin `cmd.exe`, `net.exe`, `whoami.exe` gibi araçları çalıştırdığını görüyorsanız, bu doğrudan bir "Post-Exploitation" belirtisidir.
