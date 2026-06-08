# Nexora

Nexora is a marketplace web API built with **ASP.NET Core (.NET 8)** following **Clean Architecture** principles. It provides core marketplace functionality such as user authentication, product listings, categories, carts, orders, reviews, and favorites.

## Tech Stack

- **.NET 8** / ASP.NET Core (Minimal APIs)
- **PostgreSQL** with **Entity Framework Core**
- **ASP.NET Core Identity** for authentication & authorization (incl. Google OAuth)
- **JWT** for token-based authentication
- **Azure Blob Storage** for file/image uploads (avatars, product images)
- **Clean Architecture** (Domain / Application / Infrastructure / API layers)

## Project Structure

```
Server/
├── Nexora.Api/              # Presentation layer — entry point, endpoints, configuration
├── Nexora.Application/      # Application layer — services, commands, DTOs, interfaces
│   ├── Orders/
│   ├── Product/
│   ├── Users/
│   ├── Reviews/
│   ├── Favorites/
│   ├── Carts/
│   ├── Categories/
│   ├── Addresses/
│   └── Common/
├── Nexora.Domain/           # Domain layer — entities, enums, exceptions, mappers, migrations
├── Nexora.Infrastructure/   # Infrastructure layer — EF Core, repositories, external services
├── Nexora.Tests/            # NUnit test suite (unit tests for application services)
└── Server.sln               # Solution file
```

### Architectural layers

- **Domain** — Core business entities (`ApplicationUser`, `Listing`, `Order`, `Review`, `FavoriteList`, etc.), enums, custom exceptions, and EF Core migrations. Has no dependencies on other layers.
- **Application** — Business logic and use cases, organized by feature (Orders, Products, Users, Reviews, Favorites, Carts, Categories, Addresses). Defines repository interfaces and contains commands/DTOs/mappers.
- **Infrastructure** — Implements the interfaces defined in Application (EF Core `DbContext`, repositories, blob storage, email/avatar services, Google authentication, etc.).
- **Api** — The composition root: wires up dependency injection, configures middleware, and exposes minimal API endpoints.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (see `Nexora.Api/global.json` — pinned to `8.0.24`)
- PostgreSQL database
- Azure Storage account (for blob/image uploads)

### Configuration

Configure the following sections in `Nexora.Api/appsettings.json` (or via user secrets / environment variables — **do not commit real credentials**):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "AzureStorage": {
    "ConnectionString": "...",
    "ContainerName": "..."
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "https://localhost:7065"
  }
}
```

### Run the API

```bash
cd Nexora.Api
dotnet restore
dotnet ef database update      # apply EF Core migrations
dotnet run
```

The API will be available at the URL configured in `launchSettings.json` / `Jwt:Issuer` (e.g. `https://localhost:7065`).

## Running Tests

The `Nexora.Tests` project contains an NUnit test suite covering the application services (Listings, Orders, Favorites, Reviews, Auth, Users) using **Moq**, **FluentAssertions**, and **MockQueryable.Moq**.

```bash
dotnet test
```

## License

This project currently has no license specified.
