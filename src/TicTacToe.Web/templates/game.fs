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
        // Clickable button - sets signals in click handler then posts
        button(class' = "square square-clickable", type' = "button")
            .attr("data-on:click", sprintf "$player = '%s'; $position = '%s'; @post('/')" playerStr positionStr) {
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

/// Render the current game state from a MoveResult
let renderGameBoard (result: MoveResult) =
    let (gameState, currentPlayer, validMoves, status) =
        match result with
        | XTurn(state, moves) -> (state, Some X, moves |> Array.map (fun (XPos pos) -> pos), "X's turn")
        | OTurn(state, moves) -> (state, Some O, moves |> Array.map (fun (OPos pos) -> pos), "O's turn")
        | Won(state, player) -> (state, None, [||], $"{player} wins!")
        | Draw state -> (state, None, [||], "It's a draw!")
        | Error(state, msg) -> (state, None, [||], $"Error: {msg}")

    let isGameOver = currentPlayer.IsNone

    div(id = "game-board")
        .attr("data-signals", "{player: '', position: ''}") {
        // Game status
        div(class' = "status") { h2() { status } }

        // Game board - 3x3 grid
        div(class' = "board") {
            for position in allPositions do
                renderSquare gameState validMoves currentPlayer position
        }

        // Game controls - New Game button appears after game ends
        div(class' = "controls") {
            if isGameOver then
                button(class' = "new-game-btn", type' = "button")
                    .attr("data-on:click", "@delete('/')") {
                    "New Game"
                }
        }
    }

/// CSS styles for the game board
let gameStyles =
    style() {
        raw
            """
        .game-container {
            max-width: 400px;
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

        .game-board-container {
            margin-bottom: 20px;
        }

        .status {
            text-align: center;
            margin-bottom: 20px;
        }

        .status h2 {
            font-size: 1.5em;
            color: #555;
            margin: 0;
        }

        .board {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            grid-gap: 4px;
            max-width: 240px;
            margin: 0 auto 20px auto;
            background-color: #333;
            padding: 4px;
        }

        .square {
            width: 80px;
            height: 80px;
            background-color: #fff;
            border: none;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 2em;
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

        .loading {
            text-align: center;
            color: #666;
            font-style: italic;
        }
        """
    }
