open System
open System.IO.Compression
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Frank.Builder
open Frank.Datastar
open TicTacToe.Web
open TicTacToe.Engine
open TicTacToe.Web.Extensions

let configureServices (services: IServiceCollection) =
    services.AddRouting().AddHttpContextAccessor() |> ignore

    services
        .AddAuthorization()
        .AddAntiforgery()
        .AddAuthentication(fun options -> options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(fun options ->
            options.Cookie.Name <- "TicTacToe.User"
            options.Cookie.HttpOnly <- true
            options.Cookie.SameSite <- SameSiteMode.Strict
            options.Cookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest
            options.ExpireTimeSpan <- TimeSpan.FromDays(30.0)
            options.SlidingExpiration <- true)
    |> ignore

    services
        .AddSingleton<GameSupervisor>(fun _ -> createGameSupervisor ())
        .AddSingleton<IClaimsTransformation, GameUserClaimsTransformation>()
        .AddResponseCompression(fun opts ->
            opts.EnableForHttps <- true
            opts.MimeTypes <-
                ResponseCompressionDefaults.MimeTypes
                |> Seq.append [ "image/svg+xml"; "text/event-stream" ]
            opts.Providers.Add<BrotliCompressionProvider>()
            opts.Providers.Add<GzipCompressionProvider>())
    |> ignore

    services.Configure<BrotliCompressionProviderOptions>(fun (opts: BrotliCompressionProviderOptions) ->
        opts.Level <- CompressionLevel.Fastest)
    |> ignore

    services.Configure<GzipCompressionProviderOptions>(fun (opts: GzipCompressionProviderOptions) ->
        opts.Level <- CompressionLevel.SmallestSize)
    |> ignore

    services

// Resources
let home =
    resource "/" {
        name "Home"
        get Handlers.home
    }

let sse =
    resource "/sse" {
        name "SSE"
        datastar Handlers.sse
    }

let games =
    resource "/games" {
        name "Games"
        post Handlers.createGame
    }

let gameById =
    resource "/games/{id}" {
        name "GameById"
        get Handlers.getGame
        post Handlers.makeMove
        delete Handlers.deleteGame
    }

[<EntryPoint>]
let main args =
    webHost args {
        useDefaults

        service configureServices

        logging (fun builder ->
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning) |> ignore
            builder.AddFilter("TicTacToe.Web.Auth", LogLevel.Information) |> ignore
            builder)

        plugWhen isDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage
        plugWhenNot isDevelopment (fun app -> ExceptionHandlerExtensions.UseExceptionHandler(app, "/error", true))

        plug ResponseCompressionBuilderExtensions.UseResponseCompression
        plug StaticFileExtensions.UseStaticFiles
        plug AuthAppBuilderExtensions.UseAuthentication
        plug AuthorizationAppBuilderExtensions.UseAuthorization
        plug AntiforgeryApplicationBuilderExtensions.UseAntiforgery

        resource home
        resource sse
        resource games
        resource gameById
    }

    0
