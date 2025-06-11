# Stonk Market Game - Architecture Plan (.NET Microservices)

## 1. Introduction

This document outlines the architectural plan for a Virtual Stock Trading Game Discord Bot, implemented using a microservice-based approach in .NET. The goal is to create a scalable, maintainable, and robust platform where Discord users can simulate stock trading with virtual currency, utilizing real market data, without any financial risk.

## 2. Architecture Overview

The bot will be composed of several independent microservices, each responsible for a specific domain. These services will communicate with each other primarily via RESTful APIs and potentially message queues for asynchronous operations.

```mermaid
graph TD
    A[Discord Client] --> B{Discord Bot Service};
    B --> C(User & Portfolio Service);
    B --> D(Market Data Service);

    C --> F[Database: UserData];
    D --> G[Database: MarketDataCache];
    D --> H[External Financial APIs];

    C --&gt; D;  // User & Portfolio needs prices from Market Data
    B --&gt; C; // Buy/Sell commands
    B --&gt; D; // Quote command

    subgraph Internal Communication
        B &lt;--&gt; C;
        B &lt;--&gt; D;
        C --&gt; D;
    end
```

## 3. Microservice Breakdown

Each microservice will be an independent .NET application (likely ASP.NET Core Web API for HTTP services) responsible for its own data and logic.

### 3.1. Discord Bot Service (Gateway Service)

* **Responsibility:**
    * Primary interface with the Discord API.
    * Listens for Discord commands (`!buy`, `!sell`, `!quote`, `!portfolio`, `!balance`, `!leaderboard`, `!register`).
    * Parses user commands and validates basic syntax.
    * Routes requests to the appropriate backend microservice.
    * Formats responses from backend services into Discord-friendly messages (e.g., Embeds).
    * Handles Discord-specific events (e.g., new member joins, server configuration).
* **Technologies:**
    * .NET (C#)
    * `Discord.Net` library for Discord API interaction.
    * `HttpClient` for calling other internal microservices.
* **APIs Consumed:** User & Portfolio Service API, Market Data Service API.
* **Database:** None (stateless, acts as a router/transformer).

### 3.2. User & Portfolio Service

* **Responsibility:**
    * Manages all user-related data: Discord User ID, virtual cash balance.
    * Manages user stock holdings: Ticker, quantity, average buy price.
    * Handles the core trading logic: processing buy/sell orders, updating balances and holdings.
    * Calculates individual portfolio values.
    * Manages user registration.
* **APIs Exposed (RESTful):**
    * `POST /users/register`: Registers a new user with initial virtual capital.
    * `GET /users/{userId}/balance`: Retrieves user's virtual cash balance.
    * `GET /users/{userId}/portfolio`: Retrieves user's stock holdings and their current calculated value.
    * `POST /trades/buy`: Processes a buy order. Requires current stock price from Market Data Service.
    * `POST /trades/sell`: Processes a sell order. Requires current stock price from Market Data Service.
    * `GET /leaderboard`: Returns a list of users ranked by total portfolio value.
    * `POST /admin/reset-game`: Resets all user data (admin-only).
* **APIs Consumed:** Market Data Service API (for current stock prices during trades and portfolio valuation).
* **Database:** Dedicated database (e.g., PostgreSQL, SQL Server) for `Users`, `Portfolios`, and `Transactions` tables.

### 3.3. Market Data Service

* **Responsibility:**
    * Connects to external financial data providers (e.g., Alpha Vantage, Finnhub, Twelve Data).
    * Fetches real-time stock quotes.
    * Fetches historical stock data for charting.
    * Implements caching mechanisms to adhere to external API rate limits and improve performance.
    * Handles parsing and normalizing data from external APIs.
* **APIs Exposed (RESTful):**
    * `GET /quotes/{ticker}`: Returns the current price and basic quote information for a given ticker.
    * `GET /history/{ticker}?timeframe={timeframe}`: Returns historical price data for charting.
* **Technologies:**
    * .NET (C#)
    * `HttpClient` for external API calls.
    * `Microsoft.Extensions.Caching.Memory` for in-memory caching, or connect to a distributed cache (e.g., Redis) for more advanced caching.
* **APIs Consumed:** External Financial Data APIs.
* **Database:** Dedicated database (e.g., Redis for cache, or a relational DB for persistent cache if needed) to store cached market data and manage API call rates.

### 3.4. (Optional) Leaderboard & Analytics Service

* **Responsibility:**
    * Periodically (or event-driven) aggregates portfolio values from the User & Portfolio Service.
    * Maintains and calculates the server-wide leaderboard.
    * Could potentially track other game statistics (e.g., most traded stock, total volume).
* **APIs Exposed (RESTful):**
    * `GET /leaderboard`: Returns the current top players.
* **APIs Consumed:** User & Portfolio Service API (to get individual portfolio values).
* **Database:** Dedicated database for cached leaderboard data and analytics.

### 3.5. (Optional) Transaction History Service

* **Responsibility:**
    * Receives transaction events (buy/sell) from the User & Portfolio Service.
    * Stores detailed transaction logs.
    * Provides an API for users to view their past trades.
* **APIs Exposed (RESTful):**
    * `GET /transactions/{userId}`: Retrieves a user's transaction history.
* **APIs Consumed:** User & Portfolio Service (could publish events to a message queue, which this service consumes).
* **Database:** Dedicated database for historical transaction data.

## 4. Inter-Service Communication Strategy

* **Synchronous:**
    * **HTTP/REST:** Primary communication method for direct requests (e.g., Discord Bot Service calling User & Portfolio Service for a trade).
    * **Technologies:** `HttpClient` in .NET.
* **Asynchronous (for advanced decoupling/scalability):**
    * **Message Queues:** Could be used for events (e.g., "TradeExecuted" event published by User & Portfolio Service, consumed by Transaction History Service). Also for long-running or background tasks.
    * **Technologies:** RabbitMQ, Kafka, Azure Service Bus, AWS SQS/SNS. Use corresponding .NET client libraries (e.g., `MassTransit`, `NServiceBus`).

## 5. Database Strategy

* **Database Per Service:** Each microservice will own its dedicated database instance. This reduces coupling between services, allows independent scaling, and enables different database technologies if needed (e.g., SQL for User & Portfolio, Redis for Market Data cache).
* **ORM:** Entity Framework Core will be used for Object-Relational Mapping to interact with SQL databases.

## 6. Technology Stack (.NET Specific)

* **Framework:** .NET 8 (or latest LTS version)
* **Programming Language:** C#
* **Web Framework:** ASP.NET Core (for RESTful APIs)
* **Discord Library:** `Discord.Net`
* **ORM:** `Microsoft.EntityFrameworkCore`
* **Database Providers:** `Npgsql.EntityFrameworkCore.PostgreSQL` (for PostgreSQL), `Microsoft.EntityFrameworkCore.SqlServer` (for SQL Server), `Microsoft.EntityFrameworkCore.Sqlite` (for SQLite in dev/simple cases).
* **HTTP Client:** `System.Net.Http.HttpClient`
* **Configuration:** `Microsoft.Extensions.Configuration` for managing settings (API keys, connection strings) securely.
* **Dependency Injection:** Built-in .NET DI.
* **Logging:** `Microsoft.Extensions.Logging` (e.g., Serilog).

## 7. Deployment & Hosting Considerations

* **Containerization:** Docker will be used to containerize each microservice. This ensures consistent environments across development, testing, and production.
* **Orchestration:** Kubernetes (K8s) or Docker Compose (for simpler deployments/dev) can be used to manage and deploy the multiple containers.
* **Cloud Providers:** Azure, AWS, GCP, or similar cloud platforms are ideal for hosting microservices, offering managed database services, container orchestration, and networking.
* **Scaling:** Each service can be scaled independently based on its workload (e.g., Market Data Service might need more resources than the Leaderboard Service).
* **Networking:** Proper network configuration (e.g., internal DNS, load balancing) is crucial for inter-service communication.

## 8. Development Phases / Roadmap

1.  **Phase 1: Core Service Development (MVP)**
    * **Discord Bot Service:** Basic command handling, routing.
    * **Market Data Service:** Integration with one financial API, `!quote` functionality, basic caching.
    * **User & Portfolio Service:** `!register`, `!balance`, `!buy` (without sell), basic portfolio tracking.
    * **Database Setup:** Initial schema for users and portfolios.
    * Local Docker Compose setup for development.
2.  **Phase 2: Full Trading & Portfolio Management**
    * Complete `!buy` and `!sell` logic in User & Portfolio Service.
    * Implement `!portfolio` command (requiring Market Data Service for current prices).
    * Refine error handling and user feedback messages.
3.  **Phase 3: Competition & Enhancements**
    * **Leaderboard:** Implement `!leaderboard` in User & Portfolio Service (or a new dedicated service if complexity warrants).
    * **Charting:** Implement `!chart` in Market Data Service (requires charting library like `ScottPlot`).
    * Add Discord Embeds for richer UI.
    * Implement robust API rate limit handling and retry mechanisms.
4.  **Phase 4: Advanced Features & Refinements**
    * (Optional) Implement Transaction History Service.
    * (Optional) Implement Admin commands (e.g., `!reset_game`).
    * Consider message queues for asynchronous operations.
    * Monitoring and logging setup.

## 9. Key Considerations & Disclaimers

* **API Rate Limits:** External financial APIs have strict rate limits. The Market Data Service's caching strategy is paramount to avoid hitting these limits and incurring costs.
* **Latency:** Ensure communication between services is efficient to minimize response times for Discord commands.
* **Security:** API keys and sensitive configuration must be managed securely (e.g., environment variables, Azure Key Vault, AWS Secrets Manager).
* **Data Consistency:** While microservices promote independence, ensuring data consistency (e.g., between user balance and trades) is critical and requires careful design.
* **Disclaimer:** **Crucially, the bot must clearly state that it is for *virtual trading only* and provides *no real financial advice or real money trading opportunities*.** This must be prominent in the bot's status and command responses.
* **Learning Curve:** Microservices introduce complexity. Be prepared for increased setup, deployment, and debugging overhead compared to a monolithic application.

---