# Abysalto Stefan Parch API

## Run

```powershell
dotnet restore
dotnet run
```

Swagger UI is available at `/swagger` in the Development environment.

## Configuration

PostgreSQL is configured through:

```json
{
	"ConnectionStrings": {
		"PostgreSql": "Host=localhost;Port=5432;Database=abysalto_stefan_parch;Username=postgres;Password=postgres"
	}
}
```

Override this value with environment-specific configuration or secrets for deployed environments.
