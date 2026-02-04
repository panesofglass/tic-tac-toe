module TicTacToe.Web.templates.home

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open TicTacToe.Web.templates.game

let homePage (ctx: HttpContext) =
    ctx.Items["Title"] <- "Tic Tac Toe"

    Fragment() {
        // Include game styles
        gameStyles

        div(class' = "game-container") {
            h1(class' = "title") { "Tic Tac Toe" }

            // Game board container for SSE updates
            // The board content will be populated via SSE on connection
            div(id = "game-board", class' = "game-board-container") {
                // Initial loading state - replaced by SSE on connect
                div(class' = "loading") { "Connecting..." }
            }

            div(class' = "game-info") {
                p() { "Play locally - X and O take turns" }
            }
        }
    }
