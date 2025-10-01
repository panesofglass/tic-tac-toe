module TicTacToe.Engine.Tests.SupervisorTests

open Expecto
open TicTacToe.Engine
open TicTacToe.Model

[<Tests>]
let supervisorTests =
    testList
        "GameSupervisor Tests"
        [ testCaseAsync "Can create and track multiple concurrent games"
          <| async {
              let supervisor = createGameSupervisor ()

              try
                  // Create two games
                  let (gameId1, game1) = supervisor.CreateGame()
                  let (gameId2, game2) = supervisor.CreateGame()

                  Expect.equal (supervisor.GetActiveGameCount()) 2 "Should track 2 active games"

                  // Make sure we can retrieve both games
                  Expect.isSome (supervisor.GetGame(gameId1)) "Should find game1"
                  Expect.isSome (supervisor.GetGame(gameId2)) "Should find game2"

                  // Games should be different instances
                  Expect.notEqual game1 game2 "Games should be different instances"

              finally
                  supervisor.Dispose()
          }

          testCaseAsync "Game completes and disposes correctly"
          <| async {
              let supervisor = createGameSupervisor ()

              try
                  let (gameId, game) = supervisor.CreateGame()

                  // Complete the game
                  game.MakeMove(XMove TopLeft)
                  game.MakeMove(OMove TopCenter)
                  game.MakeMove(XMove MiddleLeft)
                  game.MakeMove(OMove TopRight)
                  game.MakeMove(XMove BottomLeft) // X wins

              finally
                  supervisor.Dispose()
          }

          testCaseAsync "Games are cleaned up when completed"
          <| async {
              let supervisor = createGameSupervisor ()

              try
                  let (gameId, game) = supervisor.CreateGame()
                  Expect.equal (supervisor.GetActiveGameCount()) 1 "Should have 1 active game"

                  // Complete the game quickly
                  game.MakeMove(XMove TopLeft)
                  game.MakeMove(OMove TopCenter)
                  game.MakeMove(XMove MiddleLeft)
                  game.MakeMove(OMove TopRight)
                  game.MakeMove(XMove BottomLeft) // X wins

                  // Allow a brief moment for async cleanup to complete
                  do! Async.Sleep(10)

                  // Game should be cleaned up
                  Expect.equal (supervisor.GetActiveGameCount()) 0 "Game should be cleaned up"
                  Expect.isNone (supervisor.GetGame(gameId)) "Game should no longer be retrievable"

              finally
                  supervisor.Dispose()
          } ]
