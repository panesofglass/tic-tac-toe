module TicTacToe.Engine.Tests.EngineTests

open Expecto
open TicTacToe.Engine
open TicTacToe.Model
open TicTacToe.Engine.Tests.TestHelpers
open System.Threading
open System.Threading.Tasks

// Simple helper that collects a specific number of results using IObservable
let collectResults (game: Game) (count: int) (timeoutMs: int) =
    task {
        let results = ResizeArray<MoveResult>()
        let tcs = TaskCompletionSource<MoveResult[]>()
        let mutable subscription: System.IDisposable option = None

        let observer =
            { new System.IObserver<MoveResult> with
                member _.OnNext(result) =
                    results.Add(result)

                    if results.Count >= count then
                        tcs.TrySetResult(results.ToArray()) |> ignore
                        subscription |> Option.iter (fun s -> s.Dispose())

                member _.OnError(error) = tcs.TrySetException(error) |> ignore

                member _.OnCompleted() =
                    tcs.TrySetResult(results.ToArray()) |> ignore }

        subscription <- Some(game.Subscribe(observer))

        // Set up timeout
        use cts = new CancellationTokenSource(timeoutMs)

        cts.Token.Register(fun () ->
            tcs.TrySetResult(results.ToArray()) |> ignore
            subscription |> Option.iter (fun s -> s.Dispose()))
        |> ignore

        return! tcs.Task
    }

// Apply moves and collect results
let applyMovesAndCollect (moves: Move list) (expectedResults: int) =
    task {
        use game = createGame ()

        // Start collecting results
        let resultsTask = collectResults game expectedResults 5000

        // Apply moves with small delays
        for move in moves do
            game.MakeMove(move)

        return! resultsTask
    }

[<Tests>]
let gameInitializationTests =
    testList
        "Game Actor Initialization Tests"
        [ testCaseAsync "Game actor starts with X's turn"
          <| async {
              let! results = collectResults (createGame()) 1 1000 |> Async.AwaitTask
              
              Expect.equal results.Length 1 "Should have initial state"
              let initialState = results.[0]
              Expect.isTrue (isXTurn initialState) "Initial game state should be X's turn"
          }

          testCaseAsync "All squares are empty in initial game state"
          <| async {
              let! results = collectResults (createGame()) 1 1000 |> Async.AwaitTask
              
              let gameState = getGameState results.[0]
              
              // Check all positions are empty
              expectEmptySquare gameState TopLeft
              expectEmptySquare gameState TopCenter
              expectEmptySquare gameState TopRight
              expectEmptySquare gameState MiddleLeft
              expectEmptySquare gameState MiddleCenter
              expectEmptySquare gameState MiddleRight
              expectEmptySquare gameState BottomLeft
              expectEmptySquare gameState BottomCenter
              expectEmptySquare gameState BottomRight
          }

          testCaseAsync "All valid moves are available at the start"
          <| async {
              let! results = collectResults (createGame()) 1 1000 |> Async.AwaitTask
              
              let initialState = results.[0]
              let validMoves = getValidXMoves initialState
              
              Expect.equal validMoves.Length 9 "Should have 9 valid moves at start"
              
              // Check that all positions are available
              let positions = validMoves |> Array.map (fun (XPos pos) -> pos) |> Set.ofArray
              
              let expectedPositions =
                  [ TopLeft; TopCenter; TopRight
                    MiddleLeft; MiddleCenter; MiddleRight
                    BottomLeft; BottomCenter; BottomRight ]
                  |> Set.ofList
              
              Expect.equal positions expectedPositions "All board positions should be valid moves"
          } ]

[<Tests>]
let moveMechanicsTests =
    testList
        "Game Actor Move Mechanics Tests"
        [ testCaseAsync "Valid moves array is updated after each move through actor"
          <| async {
              let moves = [ XMove TopLeft; OMove TopRight ]
              let! results = applyMovesAndCollect moves 3 |> Async.AwaitTask // initial + 2 moves
              
              Expect.equal results.Length 3 "Should have initial state + 2 moves"
              
              let afterXMove = results.[1]
              expectNoError afterXMove "X's move should succeed"
              
              // Check O's valid moves
              let validOMoves = getValidOMoves afterXMove
              Expect.equal validOMoves.Length 8 "Should have 8 valid moves after X plays"
              
              // Check that TopLeft is no longer a valid move
              let oPositions = validOMoves |> Array.map (fun (OPos pos) -> pos) |> Set.ofArray
              Expect.isFalse (oPositions.Contains TopLeft) "TopLeft should no longer be a valid move"
              
              let afterOMove = results.[2]
              expectNoError afterOMove "O's move should succeed"
              
              // Check X's valid moves
              let validXMoves = getValidXMoves afterOMove
              Expect.equal validXMoves.Length 7 "Should have 7 valid moves after O plays"
              
              // Check that both played positions are no longer valid moves
              let xPositions = validXMoves |> Array.map (fun (XPos pos) -> pos) |> Set.ofArray
              Expect.isFalse (xPositions.Contains TopLeft) "TopLeft should no longer be a valid move"
              Expect.isFalse (xPositions.Contains TopRight) "TopRight should no longer be a valid move"
          } ]

[<Tests>]
let winConditionTests =
    testList
        "Game Actor Win Condition Tests"
        [ testCaseAsync "X wins with top row through actor"
          <| async {
              let moves =
                  [ XMove TopLeft    // X
                    OMove MiddleLeft // O  
                    XMove TopCenter  // X
                    OMove MiddleCenter // O
                    XMove TopRight ] // X wins
              
              // Expect: initial + 5 moves = 6 results
              let! results = applyMovesAndCollect moves 6 |> Async.AwaitTask
              
              Expect.isGreaterThanOrEqual results.Length 6 "Should have all move results"
              let finalResult = results.[results.Length - 1]
              
              Expect.isTrue (isWon finalResult X) "X should win with top row"
              
              let gameState = getGameState finalResult
              expectTakenByX gameState TopLeft
              expectTakenByX gameState TopCenter  
              expectTakenByX gameState TopRight
          }

          testCaseAsync "O wins with middle row through actor"
          <| async {
              let moves =
                  [ XMove TopLeft    // X
                    OMove MiddleLeft // O
                    XMove TopCenter  // X
                    OMove MiddleCenter // O
                    XMove BottomRight // X
                    OMove MiddleRight ] // O wins
              
              let! results = applyMovesAndCollect moves 7 |> Async.AwaitTask
              let finalResult = results.[results.Length - 1]
              
              expectNoError finalResult "Applying moves should succeed"
              Expect.isTrue (isWon finalResult O) "O should win with middle row"
              
              let gameState = getGameState finalResult
              expectTakenByO gameState MiddleLeft
              expectTakenByO gameState MiddleCenter
              expectTakenByO gameState MiddleRight
          }

          testCaseAsync "X wins with left column through actor"
          <| async {
              let moves =
                  [ XMove TopLeft    // X
                    OMove TopCenter  // O
                    XMove MiddleLeft // X
                    OMove MiddleCenter // O
                    XMove BottomLeft ] // X wins
              
              let! results = applyMovesAndCollect moves 6 |> Async.AwaitTask
              let finalResult = results.[results.Length - 1]
              
              expectNoError finalResult "Applying moves should succeed"
              Expect.isTrue (isWon finalResult X) "X should win with left column"
              
              let gameState = getGameState finalResult
              expectTakenByX gameState TopLeft
              expectTakenByX gameState MiddleLeft
              expectTakenByX gameState BottomLeft
          }

          testCaseAsync "O wins with diagonal through actor"
          <| async {
              let moves =
                  [ XMove TopCenter  // X
                    OMove TopLeft    // O
                    XMove MiddleLeft // X
                    OMove MiddleCenter // O
                    XMove BottomLeft // X
                    OMove BottomRight ] // O wins
              
              let! results = applyMovesAndCollect moves 7 |> Async.AwaitTask
              let finalResult = results.[results.Length - 1]
              
              expectNoError finalResult "Applying moves should succeed"
              Expect.isTrue (isWon finalResult O) "O should win with diagonal"
              
              let gameState = getGameState finalResult
              expectTakenByO gameState TopLeft
              expectTakenByO gameState MiddleCenter
              expectTakenByO gameState BottomRight
          } ]

[<Tests>]
let drawConditionTests =
    testList
        "Game Actor Draw Condition Tests"
        [ testCaseAsync "Game ends in a draw when board is full with no winner"
          <| async {
              // This sequence creates a full board with no winner
              let moves =
                  [ XMove TopLeft     // X | O | X    First row
                    OMove TopCenter   // X | X | O    Second row
                    XMove MiddleLeft  // O | X | O    Third row - no winning lines
                    OMove MiddleRight
                    XMove TopRight
                    OMove BottomLeft
                    XMove BottomCenter
                    OMove BottomRight
                    XMove MiddleCenter ] // Final move, fills board with no winner
              
              let! results = applyMovesAndCollect moves 10 |> Async.AwaitTask // initial + 9 moves
              let finalResult = results.[results.Length - 1]
              
              expectNoError finalResult "Applying moves should succeed"
              Expect.isTrue (isDraw finalResult) "Game should end in a draw"
              
              // Verify board is full
              let gameState = getGameState finalResult
              expectTakenByX gameState TopLeft
              expectTakenByO gameState TopCenter
              expectTakenByX gameState TopRight
              expectTakenByX gameState MiddleLeft
              expectTakenByX gameState MiddleCenter
              expectTakenByO gameState MiddleRight
              expectTakenByO gameState BottomLeft
              expectTakenByX gameState BottomCenter
              expectTakenByO gameState BottomRight
          } ]

[<Tests>]
let invalidMoveTests =
    testList
        "Game Actor Invalid Move Tests"
        [ testCaseAsync "Attempting to move in an already taken square is invalid"
          <| async {
              let moves = [ XMove TopLeft; OMove TopLeft ] // O tries to move in same square
              let! results = applyMovesAndCollect moves 3 |> Async.AwaitTask
              
              Expect.isGreaterThanOrEqual results.Length 2 "Should have initial + X move + error"
              
              let afterXMove = results.[1]
              expectNoError afterXMove "X's move should succeed"
              
              // Look for error result
              let errorResult = results |> Array.tryFind isError
              Expect.isSome errorResult "Should contain error result"
              
              match errorResult with
              | Some err -> expectError err "Invalid move" "Should get invalid move error"
              | None -> failwith "Expected error result"
              
              let gameState = getGameState afterXMove
              expectTakenByX gameState TopLeft
          }

          testCaseAsync "Attempting to make O move during X's turn is invalid"
          <| async {
              let moves = [ OMove TopLeft ] // O tries to move first (should be X)
              let! results = applyMovesAndCollect moves 2 |> Async.AwaitTask
              
              let errorResult = results |> Array.tryFind isError
              Expect.isSome errorResult "Should contain error result for wrong turn"
              
              match errorResult with
              | Some err -> expectError err "Invalid move" "Should get invalid move error"
              | None -> failwith "Expected error result"
          } ]

[<Tests>]
let tests =
    testList
        "TicTacToe Engine Actor Tests"
        [ gameInitializationTests
          moveMechanicsTests
          winConditionTests
          drawConditionTests
          invalidMoveTests ]

