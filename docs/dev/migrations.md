# Database Migrations Guide

## Overview

This project uses Entity Framework Core for data persistence, with separate DbContext classes for:
- Identity data (ASP.NET Core Identity)
- Game-related data (player statistics, game history)

## Initial Setup

1. Identity Database Setup
   ```shell
   dotnet ef migrations add InitialIdentitySchema --context ApplicationDbContext
   dotnet ef database update --context ApplicationDbContext
   ```

2. Game Database Setup
   ```shell
   dotnet ef migrations add InitialGameSchema --context GameDbContext
   dotnet ef database update --context GameDbContext
   ```

## Database Contexts

### ApplicationDbContext

Handles ASP.NET Core Identity tables:
- Users
- Roles
- UserClaims
- UserTokens
- UserLogins
- RoleClaims

### GameDbContext

Manages game-related data:
- Players (linked to Identity users)
- Games
- Moves
- PlayerStatistics

## Adding New Migrations

1. Create a new migration:
   ```shell
   dotnet ef migrations add <MigrationName> --context <ContextName>
   ```

2. Apply pending migrations:
   ```shell
   dotnet ef database update --context <ContextName>
   ```

3. Remove last migration (if not applied):
   ```shell
   dotnet ef migrations remove --context <ContextName>
   ```

## Testing Setup

1. In-Memory Database
   ```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
       options.UseInMemoryDatabase("TestDb"));
   ```

2. SQLite for Integration Tests
   ```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlite("DataSource=:memory:"));
   ```

## Player-Identity Mapping

The `IGamePlayerRepository` implements the mapping between Identity users and game players:

```csharp
public interface IGamePlayerRepository
{
    Task<Player> GetOrCreatePlayerAsync(string userId);
    Task<Player?> GetPlayerByIdAsync(string playerId);
    Task UpdatePlayerStatisticsAsync(string playerId, GameResult result);
}
```

## Data Seeding

1. Development Environment:
   ```csharp
   protected override void OnModelCreating(ModelBuilder builder)
   {
       base.OnModelCreating(builder);
       
       if (Environment.IsDevelopment())
       {
           SeedDevelopmentData(builder);
       }
   }
   ```

2. Test Environment:
   ```csharp
   public static class TestDataSeeder
   {
       public static async Task SeedTestDataAsync(ApplicationDbContext context)
       {
           // Seed test users and related data
       }
   }
   ```

## Future Enhancements

1. Player Statistics
   - Win/loss tracking
   - Achievement system
   - Leaderboards
   - Game history

2. Performance Optimizations
   - Indexes for common queries
   - Caching strategies
   - Query optimization

## Best Practices

1. Always backup the database before applying migrations in production
2. Test migrations in development environment first
3. Use transactions for related data operations
4. Keep migration files under source control
5. Document breaking changes in migrations
6. Use appropriate isolation levels for concurrent operations

