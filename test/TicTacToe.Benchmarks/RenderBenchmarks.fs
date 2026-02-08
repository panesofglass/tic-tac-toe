module TicTacToe.Benchmarks.RenderBenchmarks

open System.IO
open System.IO.Pipelines
open System.Text
open System.Buffers
open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open Oxpecker.ViewEngine
open TicTacToe.Model
open TicTacToe.Web.Model
open TicTacToe.Web.templates.game

[<MemoryDiagnoser>]
type RenderBenchmarks() =
    let initialState = startGame ()
    let gameId = "bench-game-1"
    let userId = "bench-user-1"
    let assignment = Some { GameId = gameId; PlayerXId = Some userId; PlayerOId = None }
    let gameCount = 6

    [<Params(10, 100, 1000, 10_000, 100_000, 1_000_000)>]
    member val ConcurrentOperations = 10 with get, set

    [<Benchmark(Baseline = true, Description = "Render.toString then write to PipeWriter")>]
    member this.GameBoardToString() =
        let tasks = Array.init this.ConcurrentOperations (fun _ ->
            task {
                let element = renderGameBoard gameId initialState userId assignment gameCount
                // Render to string (allocates 18 KB string)
                let html = Render.toString element

                // Write string to PipeWriter (simulates production Datastar.patchElements)
                let pipe = Pipe()
                use pipeStream = pipe.Writer.AsStream()
                use sw = new StreamWriter(pipeStream, Encoding.UTF8)
                do! sw.WriteAsync(html)
                do! sw.FlushAsync()
                do! pipe.Writer.CompleteAsync()

                // Read from pipe to complete the flow
                let! result = pipe.Reader.ReadAsync()
                pipe.Reader.AdvanceTo(result.Buffer.End)
                do! pipe.Reader.CompleteAsync()
            } :> Task)
        Task.WaitAll(tasks)

    [<Benchmark(Description = "Render.toTextWriterAsync (SSE realistic - PipeWriter)")>]
    member this.GameBoardToTextWriterPipe() =
        let tasks = Array.init this.ConcurrentOperations (fun _ ->
            task {
                let element = renderGameBoard gameId initialState userId assignment gameCount
                // Realistic: PipeWriter (what Kestrel/SSE uses internally)
                let pipe = Pipe()

                // Write to the pipe using StreamWriter on top of PipeWriter.AsStream()
                use pipeStream = pipe.Writer.AsStream()
                use sw = new StreamWriter(pipeStream, Encoding.UTF8, bufferSize = 1024, leaveOpen = true)
                do! Render.toTextWriterAsync sw element
                do! sw.FlushAsync()
                do! pipe.Writer.CompleteAsync()

                // Read from the pipe to complete the flow (simulates Kestrel reading)
                let! result = pipe.Reader.ReadAsync()
                pipe.Reader.AdvanceTo(result.Buffer.End)
                do! pipe.Reader.CompleteAsync()
            } :> Task)
        Task.WaitAll(tasks)

    [<Benchmark(Description = "Render.toStreamAsync (HTTP realistic - PipeWriter)")>]
    member this.GameBoardToStreamPipe() =
        let tasks = Array.init this.ConcurrentOperations (fun _ ->
            task {
                let element = renderGameBoard gameId initialState userId assignment gameCount
                // Realistic: PipeWriter (what Kestrel response does)
                let pipe = Pipe()

                // Write to the pipe
                use pipeStream = pipe.Writer.AsStream()
                do! Render.toStreamAsync pipeStream element
                do! pipe.Writer.CompleteAsync()

                // Read from the pipe to complete the flow (simulates Kestrel reading)
                let! result = pipe.Reader.ReadAsync()
                pipe.Reader.AdvanceTo(result.Buffer.End)
                do! pipe.Reader.CompleteAsync()
            } :> Task)
        Task.WaitAll(tasks)
