# Trading Forge System - Backend

## Getting Started for Developers

Welcome to the backend of the Trading Forge System! This project uses a Decoupled Architecture with a C# ASP.NET Core Web API and a PostgreSQL database running in Docker
### Prerequisites
Before you start, ensure you have the following installed:
1.  [.NET 8 or 9 SDK](https://dotnet.microsoft.com/download)
2.  [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Docker Compose)
3.  [Entity Framework Core CLI tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) (Run `dotnet tool install --global dotnet-ef` if you don't have them)

### First-Time Setup (or after a fresh pull)

Whenever you pull new code from the repository, follow these steps to ensure your local environment is up-to-date:

1.  **Start the Database:**
    We use Docker to ensure everyone has the exact same database environment without needing to install PostgreSQL directly on their machine.
    ```bash
    docker compose up -d
    ```
    *(This runs the database in the background. To stop it later, run `docker compose stop`)*

2.  **Apply Database Migrations:**
    If other developers have added new tables or changed the database schema, you need to apply those changes to your local Docker database.
    ```bash
    dotnet ef database update --project TraderForge.Infrastructure --startup-project TraderForge.API
    ```

3.  **Run the API:**
    You can now run the API from your IDE (like Rider or Visual Studio), or use the CLI:
    ```bash
    dotnet run --project TraderForge.API
    ```

Your API should now be running and connected to your local Dockerized PostgreSQL instance!

### Architecture Notes

#### Real-Time Market Data
While standard CRUD operations (like authentication and user settings) use traditional REST HTTP endpoints, **market data updates (like live charting) must use WebSockets via SignalR.** 
- **Why?** Request-response polling is highly inefficient for real-time streams and will rapidly consume third-party API quotas (e.g., Binance) and server resources.
- **How?** We implement a `MarketDataHub` (SignalR). As our background polling services fetch new prices, they inject the Hub context and broadcast the data directly to connected frontend clients, eliminating the need for client-side HTTP polling.
