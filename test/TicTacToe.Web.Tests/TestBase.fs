namespace TicTacToe.Web.Tests

open System
open System.Threading.Tasks
open NUnit.Framework
open Microsoft.Playwright

/// Base class for Playwright tests.
/// Manages browser lifecycle and provides fresh page per test.
[<AbstractClass>]
type TestBase() =
    let mutable playwright: IPlaywright = null
    let mutable browser: IBrowser = null
    let mutable context: IBrowserContext = null
    let mutable page: IPage = null

    let baseUrl =
        Environment.GetEnvironmentVariable("TEST_BASE_URL")
        |> Option.ofObj
        |> Option.filter (fun s -> not (String.IsNullOrEmpty(s)))
        |> Option.defaultValue "http://localhost:5000"

    let timeoutMs =
        Environment.GetEnvironmentVariable("TEST_TIMEOUT_MS")
        |> Option.ofObj
        |> Option.bind (fun s ->
            match Int32.TryParse(s) with
            | true, v when v > 0 -> Some v
            | _ -> None)
        |> Option.defaultValue 5000

    member _.Page = page
    member _.Context = context
    member _.BaseUrl = baseUrl
    member _.TimeoutMs = timeoutMs

    [<OneTimeSetUp>]
    member _.SetupBrowser() : Task =
        task {
            TestContext.WriteLine($"Base URL: {baseUrl}")
            TestContext.WriteLine($"Timeout: {timeoutMs}ms")

            let! pw = Playwright.CreateAsync()
            playwright <- pw

            let headed =
                Environment.GetEnvironmentVariable("HEADED")
                |> Option.ofObj
                |> Option.map (fun s -> s = "1" || s.ToLower() = "true")
                |> Option.defaultValue false

            let! b = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = not headed))
            browser <- b
        }

    [<SetUp>]
    member _.SetupPage() : Task =
        task {
            let! ctx = browser.NewContextAsync()
            context <- ctx

            let! p = ctx.NewPageAsync()
            page <- p

            try
                let options = PageGotoOptions(Timeout = Nullable(float32 timeoutMs))
                let! _ = page.GotoAsync(baseUrl, options)
                ()
            with ex ->
                let message = $"Cannot connect to {baseUrl}. Ensure the server is running: dotnet run --project src/TicTacToe.Web/"
                raise (Exception(message, ex))
        }

    [<TearDown>]
    member _.TeardownPage() : Task =
        task {
            if not (isNull page) then
                do! page.CloseAsync()
                page <- null

            if not (isNull context) then
                do! context.CloseAsync()
                context <- null
        }

    [<OneTimeTearDown>]
    member _.TeardownBrowser() =
        if not (isNull browser) then
            browser.CloseAsync() |> Async.AwaitTask |> Async.RunSynchronously
            browser <- null

        if not (isNull playwright) then
            playwright.Dispose()
            playwright <- null
