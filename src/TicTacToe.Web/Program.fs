open System
open System.IO.Compression
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Routing.Internal
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Oxpecker
open Frank.Builder
open TicTacToe.Web
open TicTacToe.Web.DatastarExtensions
open TicTacToe.Web.Extensions
open TicTacToe.Engine

let jsonOptions =
    JsonFSharpOptions
        .Default()
        .WithUnionUnwrapFieldlessTags()
        .WithSkippableOptionFields(SkippableOptionFields.Always, deserializeNullAsNone = true)
        .ToJsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

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
        .AddOxpecker()
        .AddDatastar()
        .AddSingleton<IClaimsTransformation, GameUserClaimsTransformation>()
        .AddSingleton<IJsonSerializer>(SystemTextJsonSerializer(jsonOptions))
        .AddSingleton<GameSupervisor>(fun _ -> createGameSupervisor ())
        .AddResponseCompression(fun opts ->
            opts.EnableForHttps <- true

            opts.MimeTypes <-
                ResponseCompressionDefaults.MimeTypes
                |> Seq.append (
                    seq {
                        "image/svg+xml"
                        "text/event-stream"
                    }
                )

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

let home =
    resource "/" {
        name "Home"
        get Handlers.home
    }

let graph =
    resource "graph" {
        name "Graph"
        get Handlers.graph
    }

let games =
    resource "games" {
        name "Games"
        get Handlers.games
        post (Handlers.createGame)
    }

let game =
    resource "games/{gameId}" {
        name "Game"

        get (fun (ctx: HttpContext) ->
            let gameId = ctx.GetRouteValue("gameId") |> string
            ctx |> Handlers.gamePage gameId)

        post (fun (ctx: HttpContext) ->
            let gameId = ctx.GetRouteValue("gameId") |> string
            ctx |> Handlers.makeMove gameId)
    }

let gameEvents =
    resource "games/{gameId}/events" {
        name "Game Events"

        get (fun (ctx: HttpContext) ->
            let gameId = ctx.GetRouteValue("gameId") |> string
            ctx |> Handlers.gameEvents gameId)
    }

[<EntryPoint>]
let main args =
    webHost args {
        useDefaults

        service configureServices

        logging (fun builder ->
            // Configure standard Microsoft logging
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning) |> ignore
            // Configure application authentication logging
            builder.AddFilter("TicTacToe.Web.Auth", LogLevel.Information) |> ignore

            builder.AddFilter("TicTacToe.Web.GameUserClaimsTransformation", LogLevel.Debug)
            |> ignore

            builder)

        plugWhen isDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage
        plugWhenNot isDevelopment (fun app -> ExceptionHandlerExtensions.UseExceptionHandler(app, "/error", true))

        plug ResponseCompressionBuilderExtensions.UseResponseCompression
        plug StaticFileExtensions.UseStaticFiles
        plug AuthAppBuilderExtensions.UseAuthentication
        plug AuthorizationAppBuilderExtensions.UseAuthorization
        plug AntiforgeryApplicationBuilderExtensions.UseAntiforgery

        resource home
        resource graph
        resource games
        resource game
        resource gameEvents
    }

    0
