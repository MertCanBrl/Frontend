# MÜDEK Ders Kalite Kontrol Sistemi

## Kurulum

### Ön Gereksinimler
- .NET 10 SDK
- Node.js 20+
- Docker & Docker Compose

### 1. Ortam değişkenleri
Repo kökünde `.env` dosyası oluşturun (`.env.example`'a bakın):

```bash
cp .env.example .env
# .env içindeki değerleri doldurun
```

`.env` dosyası git tarafından **takip edilmez**.

### 2. Veritabanı (PostgreSQL)
```bash
docker compose up -d
```

### 3. Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet ef database update   # migration'ları uygula
dotnet run
```

Backend varsayılan olarak `http://localhost:5042` adresinde çalışır.

### 4. Frontend

```bash
cd frontend
npm install
npm start           # geliştirme: http://localhost:4200
npm run build       # production build
```

### 5. Migration oluşturma (şema değişikliğinde)

```bash
cd backend
dotnet ef migrations add <MigrationAdı>
dotnet ef database update
```

---

## Önemli Güvenlik Notları

### DB Parolası (KRİTİK)
`appsettings.json` içindeki `ConnectionStrings:DefaultConnection` yalnızca örnek placeholder içerir.
Gerçek parola **asla** `appsettings.json`'a yazılmamalı; `.env` dosyası veya environment variable ile sağlanmalıdır.

**Git geçmişinde `eski DB parolası` parolası sızmış durumdadır.**
Production deploy'dan önce DB parolasını döndürün (rotate edin):
```sql
ALTER USER appuser WITH PASSWORD 'yeni-guclu-parola';
```
ve `.env` dosyasını güncelleyin.

### JWT Anahtarı
`Jwt__Key` environment variable olarak verilmelidir. Varsayılan değer production'da kullanılmamalıdır.

### Production API URL
`frontend/src/environments/environment.ts` içindeki `apiUrl` değerini production domain'iniz ile değiştirin:
```typescript
apiUrl: 'https://api.mudek.yourdomain.com/api'
```

### Demo Hesaplar
`admin@mudek.edu.tr` ve `ali.vural@mudek.edu.tr` demo hesapları seed migration ile oluşturulur.
Production'da ilk deploy sonrası bu hesapların şifrelerini değiştirin.

### SMTP
EmailService `Email:SmtpHost` konfigürasyonu gerektirir. Yapılandırılmazsa kullanıcı şifreleri e-posta ile iletilmez.
`.env`'e SMTP değerlerini ekleyin:
```
Email__SmtpHost=smtp.yourprovider.com
Email__SmtpPort=587
Email__Username=...
Email__Password=...
Email__From=noreply@yourdomain.com
```

### CORS
`appsettings.json` içindeki `Cors:AllowedOrigins` dizisini production frontend URL'inize göre güncelleyin veya `Cors__AllowedOrigins__0` environment variable kullanın.

---

## Kullanıcı Rolleri

| Rol | Erişim |
|-----|--------|
| Admin | Kullanıcı yönetimi, ders yönetimi, onay akışı |
| Instructor | Kendi derslerinin içerik girişi, öğrenme çıktıları, ÖÇ-PÇ eşleştirme |
