# .NET Siber Güvenlik Laboratuvarı - Gelişmiş EDR Analiz Rehberi

Bu proje, OWASP Top 10 zafiyetlerini ve gelişmiş Remote Code Execution (RCE) senaryolarını IIS/MSSQL ortamında test etmek için geliştirilmiştir.

## 1. Yeni Test Senaryoları

### Senaryo A: Broken Authentication & SQLi Login Bypass
- **URL:** `/Account/Login`
- **Saldırı:** Kullanıcı adı kısmına `admin' --` yazın, şifreyi boş bırakın.
- **Analiz:** SQL Injection ile kimlik doğrulamanın nasıl bypass edildiğini ve Carbon Black'in veritabanı sorgularındaki anormallikleri nasıl raporladığını izleyin.

### Senaryo B: Stored XSS (Kalıcı XSS)
- **URL:** Herhangi bir ürünün "İncele" sayfasına gidin.
- **Saldırı:** Yorum kısmına `<script>alert(document.cookie);</script>` yazın.
- **Analiz:** Bu yorum veritabanına kaydedilir ve sayfayı her ziyaret eden kullanıcıda çalışır. Uygulamanın `HttpOnly=false` ayarı sayesinde cookie'lerin nasıl çalınabileceğini test edin.

### Senaryo C: xp_cmdshell (Database RCE)
- **Saldırı:** Arama kutusuna `'; EXEC xp_cmdshell 'whoami'--` yazın.
- **Analiz:** `sqlservr.exe` altından başlatılan `cmd.exe` süreçlerini takip edin.

### Senaryo D: Web Shell & File Execution
- **Saldırı:** `uploads` modülü üzerinden bir `.aspx` web shell yükleyin ve "Web Erişimi" butonuna basın.
- **Analiz:** `w3wp.exe`'nin (IIS Worker Process) doğrudan zararlı kod çalıştırma davranışını izleyin.

## 2. Kurulum İpucu
- Projeyi IIS'e atmadan önce `dotnet publish` aldığınızdan emin olun.
- `wwwroot/uploads` dizinine **Write** yetkisi vermeyi unutmayın.
- MSSQL üzerinde `init_db.sql` scriptini çalıştırarak `xp_cmdshell`'i aktif edin.
