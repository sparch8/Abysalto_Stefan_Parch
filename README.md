# Cart Service API

Minimalna implementacija Web API servisa za upravljanje košaricom izrađena kao praktični dio zadatka.

## Tehnologije

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger / OpenAPI
- Serilog

## Preduvjeti

Prije pokretanja aplikacije potrebno je imati instalirano:

- .NET 8 SDK
- PostgreSQL
- Visual Studio 2022 ili Visual Studio Code

## Konfiguracija baze podataka

U datoteci `appsettings.Development.json` potrebno je konfigurirati konekcijski string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=CartDb;Username=postgres;Password=YOUR_PASSWORD"
}
```

Prije prvog pokretanja potrebno je kreirati bazu podataka izvršavanjem migracija:

```bash
dotnet ef database update
```

## Pokretanje aplikacije

Iz korijenskog direktorija projekta izvršiti:

```bash
dotnet restore
dotnet build
dotnet run
```

Nakon pokretanja Swagger dokumentacija dostupna je na:

```
https://localhost:xxxx/swagger
```

ili

```
http://localhost:xxxx/swagger
```

ovisno o konfiguraciji aplikacije.

## Implementirane funkcionalnosti

- Kreiranje košarice
- Dohvat košarice
- Dodavanje proizvoda u košaricu
- Ažuriranje količine proizvoda
- Uklanjanje proizvoda iz košarice
- Izračun ukupnog iznosa košarice

## Struktura projekta

```
Controllers/
Data/
DTOs/
Interfaces/
Middleware/
Models/
Repositories/
Services/
Program.cs
```

## Napomena

Ovaj projekt predstavlja minimalnu implementaciju jednog mikroservisa (**Cart Service**) sukladno zahtjevu zadatka. Arhitekturna dokumentacija opisuje cjelokupno rješenje koje bi u produkcijskom okruženju uključivalo dodatne servise poput Product, Order, Inventory i Payment servisa.
