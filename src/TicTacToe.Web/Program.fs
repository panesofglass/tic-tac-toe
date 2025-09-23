open System
open System.IO.Compression
open System.Text.Json.Serialization
open System.Threading.Tasks
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
open StarFederation.Datastar
open StarFederation.Datastar.FSharp
open TicTacToe.Web
open TicTacToe.Web.DatastarExtensions
open TicTacToe.Web.Extensions
open TicTacToe.Web.templates
open TicTacToe.Web.templates.shared

let jsonOptions =
    JsonFSharpOptions
        .Default()
        .WithUnionUnwrapFieldlessTags()
        .WithSkippableOptionFields(SkippableOptionFields.Always, deserializeNullAsNone = true)
        .ToJsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

let configureServices (services: IServiceCollection) =
    services
        .AddRouting()
        .AddHttpContextAccessor()
    |> ignore

    services
        .AddAuthorization()
        .AddAntiforgery()
        .AddAuthentication(fun options ->
            options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(fun options ->
            options.Cookie.Name <- "TicTacToe.User"
            options.Cookie.HttpOnly <- true
            options.Cookie.SameSite <- SameSiteMode.Strict
            options.Cookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest
            options.ExpireTimeSpan <- TimeSpan.FromDays(30.0)
            options.SlidingExpiration <- true
        )
    |> ignore

    services
        .AddOxpecker()
        .AddDatastar()
        .AddSingleton<IClaimsTransformation, GameUserClaimsTransformation>()
        .AddSingleton<IJsonSerializer>(SystemTextJsonSerializer(jsonOptions))
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
            opts.Providers.Add<GzipCompressionProvider>()
        )
    |> ignore

    services.Configure<BrotliCompressionProviderOptions>(fun (opts: BrotliCompressionProviderOptions) ->
        opts.Level <- CompressionLevel.Fastest
    )
    |> ignore

    services.Configure<GzipCompressionProviderOptions>(fun (opts: GzipCompressionProviderOptions) ->
        opts.Level <- CompressionLevel.SmallestSize
    )
    |> ignore

    services

let htmlView' f (ctx: HttpContext) = f ctx |> layout.html ctx |> ctx.WriteHtmlView

[<Literal>]
let Message = "Hello world"

let messageView' (ctx: HttpContext) =
    let datastar = ctx.RequestServices.GetRequiredService<ServerSentEventGenerator>()
    do datastar.StartServerEventStreamAsync() |> ignore

    let htmlopts = { PatchElementsOptions.Defaults with PatchMode = Append; Selector = ValueSome "#remote-text" }

    task {
        let! signals = datastar.ReadSignalsOrFailAsync<HomeSignal>(jsonOptions)

        for i = 0 to Message.Length do
            let html = Message.Substring(0, Message.Length - i) |> home.msgFragment
            do! datastar.PatchHtmlViewAsync(html, htmlopts)
            do! Task.Delay(TimeSpan.FromMilliseconds(signals.Delay))

        return! datastar.PatchHtmlViewAsync(home.msgFragment "Done", htmlopts)
    }
    :> Task

let graph =
    resource "graph" {
        name "Graph"

        get (fun (ctx: HttpContext) ->
            let graphWriter = ctx.RequestServices.GetRequiredService<DfaGraphWriter>()

            let endpointDataSource = ctx.RequestServices.GetRequiredService<EndpointDataSource>()

            use sw = new IO.StringWriter()
            graphWriter.Write(endpointDataSource, sw)
            ctx.Response.WriteAsync(sw.ToString())
        )
    }

let home =
    resource "/" {
        name "Home"
        get (htmlView' home.html)
    }

let messages =
    resource "messages" {
        name "Messages"
        get messageView'
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
            builder.AddFilter("TicTacToe.Web.GameUserClaimsTransformation", LogLevel.Debug) |> ignore
            builder
        )

        plugWhen isDevelopment DeveloperExceptionPageExtensions.UseDeveloperExceptionPage
        plugWhenNot isDevelopment (fun app -> ExceptionHandlerExtensions.UseExceptionHandler(app, "/error", true))

        plug ResponseCompressionBuilderExtensions.UseResponseCompression
        plug StaticFileExtensions.UseStaticFiles
        plug AuthAppBuilderExtensions.UseAuthentication
        plug AuthorizationAppBuilderExtensions.UseAuthorization
        plug AntiforgeryApplicationBuilderExtensions.UseAntiforgery

        resource home
        resource messages
        resource graph
    }

    0
