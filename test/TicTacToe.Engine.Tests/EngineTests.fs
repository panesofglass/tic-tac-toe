module TicTacToe.Engine.Tests.EngineTests

open System
open Expecto
open TicTacToe.Engine
open TicTacToe.Model
open TicTacToe.Engine.Tests.TestHelpers
open System.Threading
open System.Threading.Tasks

// Helper that collects MoveResults (ignores errors)
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

                member _.OnError(_error) =
                    // Ignore errors for this collector
                    ()

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

// Helper that collects both MoveResults and Errors as separate events
let collectResultsAndErrors (game: Game) (count: int) (timeoutMs: int) =
    task {
        let results = ResizeArray<obj>()
        let tcs = TaskCompletionSource<obj[]>()
        let mutable subscription: System.IDisposable option = None

        let observer =
            { new System.IObserver<MoveResult> with
                member _.OnNext(result) =
                    results.Add(result :> obj)

                    if results.Count >= count then
                        tcs.TrySetResult(results.ToArray()) |> ignore
                        subscription |> Option.iter (fun s -> s.Dispose())

                member _.OnError(error) =
                    results.Add(error :> obj)

                    if results.Count >= count then
                        tcs.TrySetResult(results.ToArray()) |> ignore
                        subscription |> Option.iter (fun s -> s.Dispose())

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

// Helper functions for working with mixed result/error arrays
let isMoveResult (obj: obj) = obj :? MoveResult
let isException (obj: obj) = obj :? Exception

let asMoveResult (obj: obj) = obj :?> MoveResult
let asException (obj: obj) = obj :?> Exception

let getResults (objects: obj[]) =
    objects |> Array.filter isMoveResult |> Array.map asMoveResult

let getErrors (objects: obj[]) =
    objects |> Array.filter isException |> Array.map asException

// Apply moves and collect results (MoveResults only)
let applyMovesAndCollect (moves: Move list) (expectedResults: int) =
    task {
        let supervisor = createGameSupervisor ()
        let (_, game) = supervisor.CreateGame()

        // Start collecting results
        let resultsTask = collectResults game expectedResults 5000

        // Apply moves with small delays
        for move in moves do
            game.MakeMove(move)

        let! result = resultsTask
        supervisor.Dispose()
        return result
    }

// Apply moves and collect both results and errors
let applyMovesAndCollectWithErrors (moves: Move list) (expectedCount: int) =
    task {
        let supervisor = createGameSupervisor ()
        let (_, game) = supervisor.CreateGame()

        // Start collecting both results and errors
        let resultsTask = collectResultsAndErrors game expectedCount 5000

        // Apply moves with small delays
        for move in moves do
            game.MakeMove(move)

        let! result = resultsTask
        supervisor.Dispose()
        return result
    }

[<Tests>]
let gameInitializationTests =
    testList
        "Game Actor Initialization Tests"
        [ testCaseAsync "Game actor starts with X's turn"
          <| async {
              let supervisor = createGameSupervisor ()
              let (_, game) = supervisor.CreateGame()
              let! results = collectResults game 1 1000 |> Async.AwaitTask
              supervisor.Dispose()

              Expect.equal results.Length 1 "Should have initial state"
              let initialState = results.[0]
              Expect.isTrue (isXTurn initialState) "Initial game state should be X's turn"
          }

          testCaseAsync "All squares are empty in initial game state"
          <| async {
              let supervisor = createGameSupervisor ()
              let (_, game) = supervisor.CreateGame()
              let! results = collectResults game 1 1000 |> Async.AwaitTask
              supervisor.Dispose()

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
              let supervisor = createGameSupervisor ()
              let (_, game) = supervisor.CreateGame()
              let! results = collectResults game 1 1000 |> Async.AwaitTask
              supervisor.Dispose()

              let initialState = results.[0]
              let validMoves = getValidXMoves initialState

              Expect.equal validMoves.Length 9 "Should have 9 valid moves at start"

              // Check that all positions are available
              let positions = validMoves |> Array.map (fun (XPos pos) -> pos) |> Set.ofArray

              let expectedPositions =
                  [ TopLeft
                    TopCenter
                    TopRight
                    MiddleLeft
                    MiddleCenter
                    MiddleRight
                    BottomLeft
                    BottomCenter
                    BottomRight ]
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
                  [ XMove TopLeft // X
                    OMove MiddleLeft // O
                    XMove TopCenter // X
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
                  [ XMove TopLeft // X
                    OMove MiddleLeft // O
                    XMove TopCenter // X
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
                  [ XMove TopLeft // X
                    OMove TopCenter // O
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
                  [ XMove TopCenter // X
                    OMove TopLeft // O
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
                  [ XMove TopLeft // X | O | X    First row
                    OMove TopCenter // X | X | O    Second row
                    XMove MiddleLeft // O | X | O    Third row - no winning lines
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
              let supervisor = createGameSupervisor ()
              let (_, game) = supervisor.CreateGame()

              try
                  // Start collecting results BEFORE making moves (expect 4: initial + X move + Error + preserved state)
                  let resultsTask = collectResults game 4 2000

                  game.MakeMove(XMove TopLeft) // Valid
                  game.MakeMove(OMove TopLeft) // Invalid - same square

                  let! results = resultsTask |> Async.AwaitTask

                  let validResults = results |> Array.filter (fun r -> not (isError r))
                  let errorResults = results |> Array.filter isError

                  Expect.isGreaterThanOrEqual validResults.Length 2 "Should have initial + X move"
                  Expect.isGreaterThan errorResults.Length 0 "Should contain Error MoveResult for taken square"

                  let afterXMove = validResults.[1]
                  expectNoError afterXMove "X's move should succeed"

                  let error = errorResults.[0]
                  let errorMessage = match error with | Error(_, msg) -> msg | _ -> "Not an error"
                  Expect.stringContains errorMessage "Invalid move" "Should get invalid move error"

                  let gameState = getGameState afterXMove
                  expectTakenByX gameState TopLeft
              finally
                  supervisor.Dispose()
          }

          testCaseAsync "Attempting to make O move during X's turn is invalid"
          <| async {
              let supervisor = createGameSupervisor ()
              let (_, game) = supervisor.CreateGame()

              try
                  // Start collecting results BEFORE making moves (expect 3: initial + Error + preserved state)
                  let resultsTask = collectResults game 3 2000

                  game.MakeMove(OMove TopLeft) // Invalid - wrong turn

                  let! results = resultsTask |> Async.AwaitTask

                  let errorResults = results |> Array.filter isError
                  Expect.isGreaterThan errorResults.Length 0 "Should contain Error MoveResult for wrong turn"

                  let error = errorResults.[0]
                  let errorMessage = match error with | Error(_, msg) -> msg | _ -> "Not an error"
                  Expect.stringContains errorMessage "Invalid move" "Should get invalid move error"
              finally
                  supervisor.Dispose()
          }

          testCaseAsync "Attempting to make X move during O's turn is invalid"
          <| async {
              let supervisor = createGameSupervisor ()
              let (_, game) = supervisor.CreateGame()

              try
                  // Start collecting results BEFORE making moves (expect 5: initial + X move + Error + preserved state)
                  let resultsTask = collectResults game 5 2000

                  game.MakeMove(XMove TopLeft)  // Valid
                  game.MakeMove(XMove TopCenter) // Invalid - wrong turn

                  let! results = resultsTask |> Async.AwaitTask

                  let errorResults = results |> Array.filter isError
                  Expect.isGreaterThan errorResults.Length 0 "Should contain Error MoveResult for wrong turn"

                  let error = errorResults.[0]
                  let errorMessage = match error with | Error(_, msg) -> msg | _ -> "Not an error"
                  Expect.stringContains errorMessage "Invalid move" "Should get invalid move error"
              finally
                  supervisor.Dispose()
          }

          testCaseAsync "Game state is preserved after invalid move"
          <| async {
              let supervisor = createGameSupervisor ()
              let (gameId, game) = supervisor.CreateGame()

              try
                  // Start collecting results BEFORE making moves
                  let resultsTask = collectResults game 5 5000

                  // Make valid X move
                  game.MakeMove(XMove TopLeft)
                  do! Async.Sleep(50)

                  // Try invalid move (X again)
                  game.MakeMove(XMove TopCenter)
                  do! Async.Sleep(50)

                  // Make valid O move - this should work if state was preserved
                  game.MakeMove(OMove TopRight)
                  do! Async.Sleep(100) // Extra time for final move processing

                  let! results = resultsTask |> Async.AwaitTask // initial + 4 states

                  let validResults = results |> Array.filter (fun r -> not (isError r))
                  let errorResults = results |> Array.filter isError

                  Expect.isGreaterThan validResults.Length 0 "Should have valid results"
                  Expect.isGreaterThan errorResults.Length 0 "Should have error results"

                  // Final valid state should show X at TopLeft, O at TopRight
                  let finalValidState = validResults |> Array.last
                  let gameState = getGameState finalValidState
                  expectTakenByX gameState TopLeft
                  expectTakenByO gameState TopRight
                  expectEmptySquare gameState TopCenter // Invalid move should not have been applied

              finally
                  supervisor.Dispose()
          }

          testCaseAsync "Actor continues processing after error"
          <| async {
              let supervisor = createGameSupervisor ()
              let (gameId, game) = supervisor.CreateGame()

              try
                  // Start collecting results BEFORE making moves
                  let resultsTask = collectResults game 8 2000
                  
                  // Sequence: valid, invalid, valid, invalid, valid
                  let moves =
                      [ XMove TopLeft // Valid
                        XMove TopCenter // Invalid (wrong turn)
                        OMove TopCenter // Valid
                        OMove MiddleLeft // Invalid (wrong turn)
                        XMove MiddleLeft ] // Valid

                  // Apply all moves
                  for move in moves do
                      game.MakeMove(move)

                  // Time to process moves asynchronously
                  do! Async.Sleep 200

                  let! results = resultsTask |> Async.AwaitTask

                  let validResults = results |> Array.filter (fun r -> not (isError r))
                  let errorResults = results |> Array.filter isError

                  Expect.equal errorResults.Length 2 "Should have 2 error results"

                  Expect.isGreaterThanOrEqual
                      validResults.Length
                      4
                      "Should have at least 4 valid states (initial + 3 moves)"

                  Expect.isGreaterThanOrEqual
                      validResults.Length
                      4
                      "Should have at least 4 valid states (initial + 3 moves)"

                  // Verify final game state has all valid moves applied
                  let finalState = validResults |> Array.last
                  let gameState = getGameState finalState
                  expectTakenByX gameState TopLeft
                  expectTakenByO gameState TopCenter
                  expectTakenByX gameState MiddleLeft

              finally
                  supervisor.Dispose()
          } ]

[<Tests>]
let observableCompletionTests =
    testList
        "Game Observable Completion Tests"
        [ testCaseAsync "Observable completes when game ends in victory"
          <| async {
              let supervisor = createGameSupervisor ()
              let (_, game) = supervisor.CreateGame()

              try
                  let completionTcs = TaskCompletionSource<bool>()
                  let mutable subscription: System.IDisposable option = None

                  let observer =
                      { new System.IObserver<MoveResult> with
                          member _.OnNext(_) = ()

                          member _.OnError(error) =
                              completionTcs.TrySetException(error) |> ignore

                          member _.OnCompleted() =
                              completionTcs.TrySetResult(true) |> ignore }

                  subscription <- Some(game.Subscribe(observer))

                  // Play to victory
                  game.MakeMove(XMove TopLeft)
                  game.MakeMove(OMove TopCenter)
                  game.MakeMove(XMove MiddleLeft)
                  game.MakeMove(OMove TopRight)
                  game.MakeMove(XMove BottomLeft) // X wins

                  // Wait for completion with timeout
                  let! completionResult = Async.StartChild(Async.AwaitTask(completionTcs.Task), 2000) |> Async.Catch

                  match completionResult with
                  | Choice1Of2 _ -> () // Success - observable completed
                  | Choice2Of2 ex ->
                      match ex with
                      | :? System.TimeoutException ->
                          failwith
                              "Observable did not complete within timeout - this indicates the completion signal is not working"
                      | _ -> raise ex

                  subscription |> Option.iter (fun s -> s.Dispose())

              finally
                  supervisor.Dispose()
          } ]

[<Tests>]
let tests =
    testList
        "TicTacToe Engine Actor Tests"
        [ gameInitializationTests
          moveMechanicsTests
          winConditionTests
          drawConditionTests
          invalidMoveTests
          observableCompletionTests ]
