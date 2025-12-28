# AssetTracker API

Varlık, departman ve çalışan yönetimi için örnek ASP.NET Core API projesi.

## Gereksinimler

- .NET SDK 9.x
- Docker (SQL Server için)
- Node.js (frontend için, opsiyonel)

## SQL Server (Docker)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Sena.Eser@1" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

## API Bağlantı Ayarı

`AssetTrackerPro.Api/appsettings.json` içindeki connection string’i güncelle:

```json
"ConnectionStrings": {
  "Default": "Server=localhost,1433;Database=AssetTrackerPro;User Id=sa;Password=Sena.Eser@1;TrustServerCertificate=True;Encrypt=False"
}
```

## Veritabanı Migration + Seed

Projeyi kök dizinde çalıştır:

```bash
dotnet ef database update --project AssetTrackerPro.Infrastructure --startup-project AssetTrackerPro.Api
```

Bu işlem veritabanını oluşturur ve başlangıç verilerini ekler.

## API’yi Çalıştırma

```bash
dotnet run --project AssetTrackerPro.Api/AssetTrackerPro.Api.csproj
```

Başlangıç kullanıcı bilgisi:

- `admin / admin123`

## Endpoint Örnekleri

```bash
curl -u admin:admin123 http://localhost:5000/api/assets
curl -u admin:admin123 http://localhost:5000/api/departments
```

## Frontend (Opsiyonel)

Frontend portu 5173 ve 5174 dışında ise CORS izinli originlere eklenmelidir:

```csharp
policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
      .AllowAnyHeader()
      .AllowAnyMethod();
```
# assetTracker-api
