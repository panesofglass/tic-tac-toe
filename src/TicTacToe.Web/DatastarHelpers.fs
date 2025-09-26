module TicTacToe.Web.DatastarExtensions

open System.Text.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open StarFederation.Datastar.FSharp

type ServerSentEventGenerator with

    /// <summary>
    /// Read the client signals from the query string as a Json string and Deserialize
    /// </summary>
    /// <returns>Returns an instance of `'T`.</returns>
    member __.ReadSignalsOrFailAsync<'T>(jsonSerializerOptions: JsonSerializerOptions) : Task<'T> =
        task {
            let! signalsVopt = __.ReadSignalsAsync<'T>(jsonSerializerOptions)

            let signals =
                match signalsVopt with
                | ValueSome signals -> signals
                | ValueNone -> failwith $"Unable to deserialize {typeof<'T>} from signals"

            return signals
        }

    member __.PatchHtmlViewAsync(htmlView, ?options: PatchElementsOptions) =
        let fragment = htmlView |> Oxpecker.ViewEngine.Render.toString

        match options with
        | Some options -> __.PatchElementsAsync(fragment, options)
        | None -> __.PatchElementsAsync(fragment)

    member __.PatchSignalsAsync(signals, ?options: PatchSignalsOptions, ?jsonSerializerOptions: JsonSerializerOptions) =
        let json =
            match jsonSerializerOptions with
            | Some opts -> JsonSerializer.Serialize(signals, opts)
            | None -> JsonSerializer.Serialize(signals)

        match options with
        | Some opts -> __.PatchSignalsAsync(json, opts)
        | None -> __.PatchSignalsAsync(json)

type IServiceCollection with
    member this.AddDatastar() =
        this
            .AddHttpContextAccessor()
            .AddScoped<ServerSentEventGenerator>(fun svc ->
                let httpContextAccessor = svc.GetService<IHttpContextAccessor>()
                ServerSentEventGenerator(httpContextAccessor))
