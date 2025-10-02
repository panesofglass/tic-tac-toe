module TicTacToe.Web.templates.games

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Oxpecker.ViewEngine
open TicTacToe.Engine

let gamesPage (ctx: HttpContext) =
    ctx.Items["Title"] <- "All Games"
    let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
    let gameCount = supervisor.GetActiveGameCount()

    Fragment() {
        div (class' = "games-container") {
            h1 (class' = "games-title") { "All Games" }

            div (class' = "games-header") {
                div (class' = "games-count") {
                    span (class' = "count-label") { "Active Games: " }
                    span (class' = "count-value") { string gameCount }
                }

                (button (class' = "create-new-btn", type' = "button")).data ("on-click", "@post('/games')") {
                    "Create New Game"
                }
            }

            if gameCount = 0 then
                div (class' = "no-games") { p () { "No active games. Create one to get started!" } }
            else
                div (class' = "games-grid") {
                    // TODO: In the future, this will show a grid of all active games
                    // For now, just show a placeholder
                    div (class' = "game-grid-placeholder") {
                        p () { "Game grid view coming soon..." }

                        p (class' = "placeholder-note") {
                            "This will show all active games in a grid where players can join and play directly."
                        }
                    }
                }

            div (class' = "games-actions") { a (href = "/", class' = "back-link") { "← Back to Home" } }
        }
    }

let gamesStyles =
    style () {
        raw
            """
        .games-container {
            max-width: 800px;
            margin: 20px auto;
            padding: 20px;
            font-family: Arial, sans-serif;
        }

        .games-title {
            text-align: center;
            font-size: 2.5em;
            color: #333;
            margin-bottom: 30px;
        }

        .games-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 30px;
            padding: 20px;
            background-color: #f8f9fa;
            border-radius: 8px;
        }

        .games-count {
            font-size: 1.2em;
        }

        .count-label {
            color: #666;
        }

        .count-value {
            font-weight: bold;
            color: #2c3e50;
            font-size: 1.3em;
        }

        .create-new-btn {
            background-color: #27ae60;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-size: 16px;
            transition: background-color 0.2s;
        }

        .create-new-btn:hover {
            background-color: #229954;
        }

        .no-games {
            text-align: center;
            padding: 40px;
            background-color: #f5f5f5;
            border-radius: 8px;
            color: #666;
            font-size: 1.1em;
        }

        .games-grid {
            margin: 30px 0;
        }

        .game-grid-placeholder {
            text-align: center;
            padding: 60px 20px;
            background-color: #f0f8ff;
            border: 2px dashed #3498db;
            border-radius: 8px;
            color: #3498db;
        }

        .game-grid-placeholder p {
            margin: 10px 0;
            font-size: 1.1em;
        }

        .placeholder-note {
            font-size: 0.9em !important;
            font-style: italic;
            color: #7f8c8d !important;
        }

        .games-actions {
            text-align: center;
            margin-top: 30px;
        }

        .back-link {
            color: #3498db;
            text-decoration: none;
            font-size: 16px;
            padding: 10px 15px;
            border: 1px solid #3498db;
            border-radius: 4px;
            transition: all 0.2s;
        }

        .back-link:hover {
            background-color: #3498db;
            color: white;
        }
        """
    }
