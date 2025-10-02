module TicTacToe.Web.templates.game

open Microsoft.AspNetCore.Http
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
        // Clickable button for valid moves
        (((button (class' = "square square-clickable", type' = "button"))
            .data ("on-click", $"@post('/games/{gameId}/moves')"))
            .data ("player", playerStr))
            .data ("position", positionStr) {
            // Show preview of move on hover
            span (class' = "preview") { playerStr }
        }
        :> HtmlElement
    else
        // Static cell - either taken or not player's turn
        div (class' = "square") {
            if not (System.String.IsNullOrEmpty(value)) then
                span (class' = "player") { value }
            else
                span (class' = "empty") { "·" }
        }
        :> HtmlElement

// Render the current game state from a MoveResult
let renderGameBoard (gameId: string) (result: MoveResult) =
    let (gameState, currentPlayer, validMoves, status) =
        match result with
        | XTurn(state, moves) -> (state, Some X, moves |> Array.map (fun (XPos pos) -> pos), "X's turn")
        | OTurn(state, moves) -> (state, Some O, moves |> Array.map (fun (OPos pos) -> pos), "O's turn")
        | Won(state, player) -> (state, None, [||], $"{player} wins!")
        | Draw state -> (state, None, [||], "It's a draw!")
        | Error(state, msg) -> (state, None, [||], $"Error: {msg}")

    Fragment() {
        // Game status
        div (class' = "status") { h2 () { status } }

        // Game board - 3x3 grid
        div (class' = "board") {
            for position in allPositions do
                renderSquare gameId gameState validMoves currentPlayer position
        }

        // Game controls
        div (class' = "controls") {
            if currentPlayer.IsNone then
                (button (class' = "new-game-btn", type' = "button")).data ("on-click", "@post('/games')") { "New Game" }
        }
    }

// Main game page that connects to SSE
let gamePage (gameId: string) (ctx: HttpContext) =
    ctx.Items["Title"] <- "Tic Tac Toe"

    Fragment() {
        div (class' = "game-container") {
            h1 (class' = "title") { "Tic Tac Toe" }

            // Game board container that will be updated via SSE
            ((div (id = "game-board", class' = "game-board-container")).data ("sse-connect", $"/games/{gameId}/events"))
                .data ("sse-on-patch", "@patch") {
                // Initial loading state
                div (class' = "loading") { "Loading game..." }
            }

            div (class' = "game-info") { p () { $"Game ID: {gameId}" } }
        }
    }

// CSS styles
let gameStyles =
    style () {
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

        .game-info {
            text-align: center;
            margin-top: 20px;
            font-size: 0.9em;
            color: #666;
        }

        .loading {
            text-align: center;
            color: #666;
            font-style: italic;
        }
        """
    }
