# Tic-Tac-Toe Web Application

A web-based Tic-Tac-Toe game built with F# and ASP.NET Core.

## Features

- Interactive Tic-Tac-Toe gameplay
- Server-side event streaming
- Responsive design

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- A modern web browser

### Running the Application

1. Clone the repository
2. Navigate to the project directory
3. Run the application:

```bash
dotnet run --project src/TicTacToe.Web/TicTacToe.Web.fsproj
```

4. Open your browser and navigate to `https://localhost:5001`

## User Identification

The application includes an automatic user identification system that doesn't require explicit sign-in:

### How It Works

- Users receive a persistent cookie-based identifier on their first visit
- The system uses claims-based identity through ASP.NET Core authentication
- Each user gets a unique ID and timestamps for tracking game progress
- Additional environmental information helps with device identification

### Claims Used

- `sub`: Unique user identifier (GUID)
- `created_at`: When the user was first created
- `last_visit`: When the user last visited
- `ip_address`: User's IP address (for diagnostics)
- `user_agent`: Browser information (for diagnostics)

### Security Considerations

- Cookies are configured as HttpOnly to prevent client-side script access
- SameSite policy is set to Lax to balance security with functionality
- Authentication is established before antiforgery checks
- No sensitive personal information is stored

### Accessing User Information

To access the current user's identification in a request handler:

```fsharp
let userHandler (ctx: HttpContext) =
    // Get the user ID
    let userId = ctx.User.FindClaimValue(ClaimTypes.UserId)

    // Get the created timestamp
    let created = ctx.User.FindClaimValue(ClaimTypes.Created)

    // Check if this is a returning user
    let isReturningUser = ctx.User.HasClaim(ClaimTypes.Created)

    // Use the TryGetUserId extension method
    match ctx.User.TryGetUserId() with
    | Some id -> // Use the ID
    | None -> // Handle new user case
```

## Architecture

The application uses the following technologies:

- F# as the primary language
- ASP.NET Core for the web framework
- Frank.Builder for routing and HTTP handling
- Oxpecker for view rendering
- StarFederation.Datastar for server-sent events

## Testing

Run the tests using:

```bash
dotnet test
```

## License

This project is licensed under the MIT License.
