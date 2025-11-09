> **Note:** This project is a personal learning fork of the original Dotnet CRUD API created by [Christian Schou](https://github.com/Christian-Schou). All credit for the original implementation belongs to him.

<h1 align="center">
  <br>
  <a href="https://christian-schou.dk"><img src="https://github.com/Tech-With-Christian/Dotnet-Demo-CRUD-Api/blob/main/assets/img/cs-logo-polygon.png" alt="Christian Schou Logo" width="200"></a>
  <br>
  .NET CRUD API
  <br>
</h1>

<h4 align="center">This is a simple CRUD API for use in my <a href="https://blog.christian-schou.dk" target="_blank">tutorials</a>. It contains a simple implementation of a CRUD service for products, that can easily be extended.</h4>

<p align="center"><em>This repository is a fork of the original <a href="https://github.com/Tech-With-Christian/Dotnet-CRUD-Api">Dotnet CRUD API</a> by <a href="https://github.com/Christian-Schou">Christian Schou</a>.</em></p>


<p align="center">
<a href="https://christian-schou.dk"><img src="https://github.com/Tech-With-Christian/Dotnet-CRUD-Api/blob/main/assets/img/dotnet-crud-api.png" alt="Featured Image .NET CRUD API" width="90%"></a>
</p>

<p align="center">
  <a href="#key-features">Key Features</a> •
  <a href="#how-to-use">How To Use</a> •
  <a href="#download">Download</a> •
  <a href="#credits">Credits</a> •
  <a href="#license">License</a>
</p>


## Key Features

* Simple CRUD implementation
  - Super simple CRUD service implementation that can easily be extended.
* Error Handler Middleware
  - Global Error Handler Middleware taking care of exceptions in the applicaiton, providing the client with a consistent response every time. This can easily be extended with custom exceptions.
* Well documented Code (inline + wiki)
* Automatic mapping
  - Automatic Mapping of DTOs to/from Domain Models using <a href="https://automapper.org/">AutoMapper</a>.
* Database Integration using Entity Framework Core.
* MediatR (CQRS)
  - Implementation of CQRS using MediatR. All controllers will consume the mediator to handle commands and queries.
* Kafka-driven integrations
  - Order payments and external price updates are exchanged through Kafka using MassTransit.

## Price Updater Microservice

The repository now includes a companion service dedicated to managing product price versions:

- Location: `PriceUpdater/`
- Stack: ASP.NET Core Web API + EF Core (SQLite) + MassTransit (Kafka rider)
- Runs on `http://localhost:5090` by default and builds via `PriceUpdater/PriceUpdater.sln`
- Persists price versions in the shared MySQL server using the `priceupdate` database
- Contract: publishes `PriceUpdatedEvent` (`ProductId`, `Price`, `CreatedAtUtc`) on the `price-updated` Kafka topic whenever a new price version is created.

Workflow:

1. `POST /api/prices` in the PriceUpdater service stores a new immutable price record and emits a `PriceUpdatedEvent`.
2. The main CRUD API subscribes to the same Kafka topic. When it receives an event, it checks whether the price timestamp is newer than the product’s current `UpdatedAt` value.
3. If the incoming price is fresher, the product price and `UpdatedAt` timestamp are updated atomically.

### Running both services

```bash
# Terminal 1 – start Kafka (from repo root)
docker compose -f docker-compose.infrastructure.yml up -d

# Terminal 2 – run the CRUD API
dotnet run --project src/API/API.csproj

# Terminal 3 – run the Price Updater service
dotnet run --project PriceUpdater/src/PriceUpdater.API/PriceUpdater.API.csproj
```

Both services share the Kafka broker configured at `localhost:9092`. Connection strings and topic names can be customised through the respective `appsettings.json` files.

## How To Use

To clone and run this simple .NET Web API, you'll need [Git](https://git-scm.com) and [.NET SDK](https://dotnet.microsoft.com/en-us/download). From your command line:

```bash
# Clone this repository
$ git clone https://github.com/Tech-With-Christian/Dotnet-CRUD-Api.git

# Go into the repository
$ cd dotnet-crud-api

# Restore dependencies
$ dotnet restore

# Restore dependencies for the price updater
$ dotnet restore PriceUpdater/PriceUpdater.sln

# Build the price updater separately (optional)
$ dotnet build PriceUpdater/PriceUpdater.sln

# Run the app
$ dotnet watch
```

## Download

You can [download](https://github.com/Tech-With-Christian/Dotnet-CRUD-Api/archive/refs/heads/main.zip) the latest main version of this .NET CRUD API Demo for further development on Windows, macOS and Linux.

## Credits

This software uses the following open source packages:

- [AutoMapper](https://github.com/AutoMapper/AutoMapper)
- [.NET Core](https://github.com/dotnet/core)
- [MediatR](https://github.com/jbogard/MediatR)
- [Swagger](https://github.com/swagger-api)

## License

MIT License

---

> [christian-schou.dk](https://christian-schou.dk) &nbsp;&middot;&nbsp;
> GitHub [@Christian-Schou](https://github.com/Christian-Schou) &nbsp;&middot;&nbsp;
> Blog [blog.christian-schou.dk](https://blog.christian-schou.dk)

