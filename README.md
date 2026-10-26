# MÜDEK Ders Kalite Kontrol Sistemi

MÜDEK değerlendirme süreçleri için ders içeriklerini, öğrenme çıktıları ile program çıktıları arasındaki ilişkileri ve dönem içi ölçme değerlendirme verilerini bir araya getiren web uygulaması. Proje, **Ardil Süngü, Mert Can Birol ve Duygu İrem Yılmaz** tarafından üç kişilik ekip olarak geliştirilmiştir.

Öğretim üyeleri kendi derslerinin içeriklerini, sınavlarını, notlarını ve devamsızlık kayıtlarını yönetir. Yöneticiler ders ve personel tanımlarını düzenler, içerik onay ve revizyon süreçlerini yürütür ve ders içeriklerini PDF olarak dışa aktarır.

## Dallar

Uygulamanın güncel işlevleri **`develop`** dalındadır. `main` dalı ilk proje iskeletini içerir. Aşağıdaki açıklamalar `develop` sürümünü esas alır.

```powershell
git clone --branch develop https://github.com/MertCanBrl/Frontend.git
cd Frontend
```

## Özellikler

| Alan | Mevcut işlevler |
| --- | --- |
| Kimlik doğrulama | JWT ile giriş, Admin ve Instructor rolleri, Angular route guard ve HTTP interceptor |
| Yönetici paneli | Personel ve ders yönetimi, öğretim üyesi atama, program çıktıları ve genel anket soruları |
| Ders içerikleri | Ders amacı ve açıklaması, haftalık konular, öğrenme çıktıları, Bloom düzeyi ve bilgi/beceri/yetkinlik sınıflandırması |
| ÖÇ–PÇ matrisi | Öğrenme çıktıları ile program çıktılarının katkı düzeyleri üzerinden eşleştirilmesi |
| İçerik onayı | Taslak, onay bekliyor, onaylandı ve revizyon istendi durumları; içerik kilitleme ve revizyon notları |
| Ölçme değerlendirme | Sınav ve soru bazlı not girişi, dinamik değerlendirme bileşenleri, bileşen–öğrenme çıktısı ağırlıkları |
| Dönem takibi | Öğrenci listeleri, başarı istatistikleri, hafta bazlı devam ve devamsızlık durumu |
| Anket tanımları | Genel ve derse özel sorular, soruların öğrenme çıktılarıyla ağırlıklı ilişkilendirilmesi |
| Raporlama | Chart.js grafikleri; QuestPDF ile öğrenme çıktıları ve ÖÇ–PÇ matrisini içeren PDF formu |
| Arayüz | Mobil uyumlu sayfa düzeni, merkezi tasarım tokenları ve ortak buton/kart bileşenleri |
| E-posta | SMTP yapılandırıldığında yeni kullanıcı bilgilerinin e-posta ile gönderilmesi |

**ÖÇ:** Öğrenme Çıktısı. **PÇ:** Program Çıktısı. Anket modülü soru ve eşleştirme tanımlarını kapsar; öğrenci yanıtı toplama akışı içermez.

## Teknolojiler

| Katman | Teknolojiler |
| --- | --- |
| Frontend | Angular 22, TypeScript 6, Angular Signals, Reactive Forms, RxJS, Chart.js 4 |
| Backend | C#, ASP.NET Core Web API, .NET 10, Entity Framework Core 10 |
| Veritabanı | PostgreSQL 17, Npgsql, EF Core migrations |
| Kimlik ve erişim | JWT Bearer, BCrypt, rol ve ders sahipliği kontrolleri |
| PDF | QuestPDF |
| Testler | Frontend için Vitest; backend için xUnit |
| Yerel altyapı | Docker Compose ile PostgreSQL, DotNetEnv ile ortam değişkenleri |

## Proje yapısı

```text
Frontend/
├── backend/
│   ├── Controllers/       # Kimlik, yönetici, öğretim üyesi, ÖÇ ve PÇ API'leri
│   ├── Data/              # DbContext ve tasarım zamanı bağlantısı
│   ├── Migrations/        # Şema ve başlangıç verileri
│   ├── Models/            # Varlıklar ve DTO'lar
│   ├── Services/          # JWT, e-posta ve PDF servisleri
│   └── Utils/             # Devamsızlık hesapları ve doğal sıralama
├── backend.Tests/         # Devamsızlık hesaplama birim testleri
├── frontend/src/
│   ├── app/core/          # Servisler, modeller, guard ve interceptor
│   ├── app/features/      # Yönetici ve öğretim üyesi ekranları
│   ├── app/shared/ui/     # Ortak arayüz bileşenleri
│   └── environments/      # Geliştirme ve üretim API adresleri
├── .env.example
└── docker-compose.yml
```

## Yerel kurulum

Komutlar PowerShell içindir. Backend ve frontend ayrı terminallerde çalıştırılır.

### Gereksinimler

- .NET 10 SDK.
- Node.js 24.15 veya üzeri bir 24.x sürümü ve npm 11. Depodaki Angular paketleri Node.js `^22.22.3 || ^24.15.0 || >=26.0.0` aralığını ister.
- Docker ve Docker Compose.
- EF Core CLI 10.0.9. Kurulu değilse: `dotnet tool install --global dotnet-ef --version 10.0.9`.

### 1. Ortam değişkenleri

Depo kökünde:

```powershell
Copy-Item .env.example .env
```

`.env` içindeki PostgreSQL ve bağlantı dizesi değerlerini birbiriyle eşleştirin:

```dotenv
POSTGRES_USER=appuser
POSTGRES_PASSWORD=CHANGE_ME_LOCAL_DB_PASSWORD
POSTGRES_DB=appdb
POSTGRES_PORT=5433

ConnectionStrings__DefaultConnection=Host=localhost;Port=5433;Database=appdb;Username=appuser;Password=CHANGE_ME_LOCAL_DB_PASSWORD

Jwt__Key=CHANGE_ME_TO_A_RANDOM_KEY_OF_AT_LEAST_32_BYTES
Jwt__Issuer=MudekApp
Jwt__Audience=MudekAppUsers
```

Yer tutucu parola ve JWT anahtarını değiştirin. `POSTGRES_PASSWORD` ile bağlantı dizesindeki `Password` aynı olmalıdır; `.env.example` dosyasındaki örnek değerler doğrudan eşleşmez. JWT imzalama anahtarı en az 32 bayt olmalıdır. `.env` Git tarafından izlenmez.

### 2. PostgreSQL

Depo kökünde:

```powershell
docker compose up -d postgres
docker compose ps
```

Compose yalnız PostgreSQL servisini başlatır. Veriler `postgres-data` adlı Docker volume'unda tutulur. Veritabanı bağlantı kabul etmeye hazır olduğunda migration adımına geçin.

### 3. Backend ve veritabanı şeması

```powershell
cd backend
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

API: `http://localhost:5042`. Geliştirme ortamındaki OpenAPI JSON: `http://localhost:5042/openapi/v1.json`.

Backend ve EF Core tasarım zamanı fabrikası `.env` dosyasını `../.env` yolundan okur; komutları `backend` klasöründe çalıştırın. Depoda Swagger UI kurulumu bulunmaz.

### 4. Frontend

İkinci terminalde, depo kökünden:

```powershell
cd frontend
npm.cmd ci
npm.cmd start
```

Arayüz: `http://localhost:4200`. Geliştirme API adresi `frontend/src/environments/environment.ts` içindeki `http://localhost:5042/api` değeridir.

### 5. SMTP

Yeni kullanıcı bilgilerinin e-posta ile iletilmesi için `.env` dosyasına ekleyin:

```dotenv
Email__SmtpHost=smtp.example.com
Email__SmtpPort=587
Email__Username=YOUR_SMTP_USER
Email__Password=YOUR_SMTP_PASSWORD
Email__From=noreply@example.com
```

SMTP ayarı yoksa uygulama kullanıcı kaydını tamamlar, ancak e-posta göndermez ve bu durumu loglar. Oluşturulan giriş bilgilerinin kullanıcıya ulaşması için SMTP'yi yapılandırın.

## Kullanım akışı

1. Yönetici kullanıcıları ve dersleri tanımlar; dersleri öğretim üyelerine atar.
2. Öğretim üyesi dersinin genel bilgilerini, konularını, öğrenme çıktılarını, ÖÇ–PÇ eşleştirmesini ve anket sorularını düzenler.
3. Ders içeriği incelemeye gönderilir. Yönetici onay verir veya gerekçesiyle revizyon ister; onaylanan içerik kilitlenir.
4. Öğretim üyesi dönem derslerinde öğrencileri, sınavları, değerlendirme bileşenlerini, notları ve haftalık devamsızlıkları takip eder.
5. Yönetici ders içeriğini ve ÖÇ–PÇ eşleştirmesini PDF olarak indirir.

Başlangıç verileri yönetici ve öğretim üyesi demo hesapları içerir. Bu hesaplar yerel geliştirme içindir; gerçek ortamda parolaları değiştirilmelidir.

## API özeti

| Yol | Amaç |
| --- | --- |
| `POST /api/auth/login` | JWT ile giriş |
| `/api/admin/users` | Personel yönetimi |
| `/api/admin/courses` | Ders yönetimi ve atamalar |
| `/api/admin/approvals` | İçerik onay listesi |
| `/api/admin/courses/{courseId}/pdf` | Ders içeriği PDF çıktısı |
| `/api/instructor/course-contents` | Öğretim üyesinin ders içerikleri |
| `/api/instructor/term-courses` | Dönem dersleri ve not/devamsızlık alt kaynakları |
| `/api/courses/{courseId}/learning-outcomes` | Ders öğrenme çıktıları |
| `/api/courses/{courseId}/mapping` | ÖÇ–PÇ matrisi |
| `/api/program-outcomes` | Program çıktıları ve grupları |

Kimlik doğrulamalı isteklerde `Authorization: Bearer <token>` başlığı kullanılır. Desteklenen HTTP metotları ve istek şemaları için geliştirme OpenAPI belgesini inceleyin.

## Build ve test komutları

Backend için depo kökünde:

```powershell
dotnet build backend/Backend.csproj
dotnet test backend.Tests/Backend.Tests.csproj
```

Frontend için `frontend` klasöründe:

```powershell
npm.cmd run build
npm.cmd test -- --watch=false
```

Üretim build'i `frontend/src/environments/environment.production.ts` dosyasını kullanır. Dosyadaki API yer tutucusunu gerçek HTTPS API adresiyle değiştirin.

## Yapılandırma notları

- Frontend origin listesi `Cors:AllowedOrigins` üzerinden okunur; ortam değişkeni örneği `Cors__AllowedOrigins__0=https://frontend.example.com`.
- Gerçek veritabanı, JWT ve SMTP sırlarını `.env` veya dağıtım ortamında sağlayın.
- Önceki README'de belirtilen geçmiş veritabanı parolası ifşası nedeniyle gerçek ortamda kullanılan ilgili kimlik bilgileri yenilenmelidir.
- PDF servisinde Erciyes Üniversitesi ve Mühendislik Fakültesi bilgileri sabittir; logo ve QR alanları yer tutucudur.
- Compose dosyası yerel PostgreSQL içindir; backend ve frontend servisleri bu dosyada yer almaz.

## Geliştirenler

- **Ardil Süngü** — [ardilsungu](https://github.com/ardilsungu)
- **Mert Can Birol** — [MertCanBrl](https://github.com/MertCanBrl)
- **Duygu İrem Yılmaz**

Ekip üyeleri backend, frontend ve iş akışlarının geliştirilmesine birlikte katkıda bulunmuştur. Bireysel değişiklikler commit ve pull request geçmişinden izlenebilir.
