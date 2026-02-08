module TicTacToe.Benchmarks.Program

open BenchmarkDotNet.Running

[<EntryPoint>]
let main args =
    BenchmarkSwitcher
        .FromAssembly(typeof<RenderBenchmarks.RenderBenchmarks>.Assembly)
        .Run(args)
    |> ignore
    0
