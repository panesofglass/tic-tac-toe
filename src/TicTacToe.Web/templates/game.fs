module TicTacToe.Web.templates.game

open Oxpecker.ViewEngine
open TicTacToe.Model

let private allPositions =
    [ TopLeft
      TopCenter
      TopRight
      MiddleLeft
      MiddleCenter
      MiddleRight
      BottomLeft
      BottomCenter
      BottomRight ]

let private getSquareValue (gameState: GameState) (position: SquarePosition) : string =
    match gameState.TryGetValue(position) with
    | true, Taken player -> player.ToString()
    | _ -> ""

let private isValidMove (validMoves: SquarePosition array) (position: SquarePosition) : bool =
    validMoves |> Array.contains position

let private renderSquare
    (gameId: string)
    (gameState: GameState)
    (validMoves: SquarePosition array)
    (currentPlayer: Player option)
    (position: SquarePosition)
    =
    let value = getSquareValue gameState position
    let positionStr = position.ToString()
    let isEmpty = System.String.IsNullOrEmpty(value)
    let canMove = isEmpty && isValidMove validMoves position

    if canMove && currentPlayer.IsSome then
        let playerStr = currentPlayer.Value.ToString()
        // Clickable button - sets signals in click handler then posts to game-specific endpoint
        button(class' = "square square-clickable", type' = "button")
            .attr("data-on:click", sprintf "$gameId = '%s'; $player = '%s'; $position = '%s'; @post('/games/%s')" gameId playerStr positionStr gameId) {
            // Show preview of move on hover
            span(class' = "preview") { playerStr }
        }
        :> HtmlElement
    else
        // Static cell - either taken or not player's turn
        div(class' = "square") {
            if not (System.String.IsNullOrEmpty(value)) then
                span(class' = "player") { value }
            else
                span(class' = "empty") { raw "·" }
        }
        :> HtmlElement

/// Render the current game state from a MoveResult with game ID for multi-game support
let renderGameBoard (gameId: string) (result: MoveResult) =
    let (gameState, currentPlayer, validMoves, status) =
        match result with
        | XTurn(state, moves) -> (state, Some X, moves |> Array.map (fun (XPos pos) -> pos), "X's turn")
        | OTurn(state, moves) -> (state, Some O, moves |> Array.map (fun (OPos pos) -> pos), "O's turn")
        | Won(state, player) -> (state, None, [||], $"{player} wins!")
        | Draw state -> (state, None, [||], "It's a draw!")
        | Error(state, msg) -> (state, None, [||], $"Error: {msg}")

    let isGameOver = currentPlayer.IsNone

    div(id = $"game-{gameId}", class' = "game-board")
        .attr("data-signals", sprintf "{gameId: '%s', player: '', position: ''}" gameId) {
        // Game status
        div(class' = "status") { h2() { status } }

        // Game board - 3x3 grid
        div(class' = "board") {
            for position in allPositions do
                renderSquare gameId gameState validMoves currentPlayer position
        }

        // Game controls - Delete button for cleanup
        div(class' = "controls") {
            button(class' = "delete-game-btn", type' = "button")
                .attr("data-on:click", sprintf "@delete('/games/%s')" gameId) {
                "Delete Game"
            }
        }
    }

/// CSS styles for the game board
let gameStyles =
    style() {
        raw
            """
        .game-container {
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            font-family: Arial, sans-serif;
        }

        .title {
            text-align: center;
            font-size: 2em;
            margin-bottom: 20px;
            color: #333;
        }

        .new-game-container {
            text-align: center;
            margin-bottom: 20px;
        }

        .games-container {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            justify-content: center;
        }

        .game-board {
            background-color: #f5f5f5;
            border-radius: 8px;
            padding: 15px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .status {
            text-align: center;
            margin-bottom: 15px;
        }

        .status h2 {
            font-size: 1.2em;
            color: #555;
            margin: 0;
        }

        .board {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            grid-gap: 4px;
            max-width: 200px;
            margin: 0 auto 15px auto;
            background-color: #333;
            padding: 4px;
        }

        .square {
            width: 60px;
            height: 60px;
            background-color: #fff;
            border: none;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5em;
            font-weight: bold;
            cursor: default;
        }

        .square-clickable {
            cursor: pointer;
            background-color: #f0f8ff;
            transition: background-color 0.2s;
        }

        .square-clickable:hover {
            background-color: #e6f3ff;
        }

        .square .player {
            color: #333;
        }

        .square .preview {
            color: #999;
        }

        .square .empty {
            color: #ccc;
            font-size: 1em;
        }

        .controls {
            text-align: center;
        }

        .new-game-btn {
            background-color: #4CAF50;
            color: white;
            padding: 12px 24px;
            font-size: 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.2s;
        }

        .new-game-btn:hover {
            background-color: #45a049;
        }

        .delete-game-btn {
            background-color: #f44336;
            color: white;
            padding: 8px 16px;
            font-size: 12px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.2s;
        }

        .delete-game-btn:hover {
            background-color: #d32f2f;
        }

        .loading {
            text-align: center;
            color: #666;
            font-style: italic;
            padding: 40px;
        }

        .game-info {
            text-align: center;
            margin-top: 20px;
            color: #666;
        }
        """
    }
