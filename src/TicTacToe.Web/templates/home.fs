module TicTacToe.Web.templates.home

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Oxpecker.ViewEngine
open TicTacToe.Engine

let homePage (ctx: HttpContext) =
    ctx.Items["Title"] <- "Tic Tac Toe - Home"
    let supervisor = ctx.RequestServices.GetRequiredService<GameSupervisor>()
    let gameCount = supervisor.GetActiveGameCount()

    Fragment() {
        div (class' = "home-container") {
            h1 (class' = "home-title") { "Tic Tac Toe" }

            div (class' = "home-content") {
                div (class' = "stats") {
                    h2 () { "Active Games" }
                    div (class' = "game-count") { string gameCount }
                }

                div (class' = "actions") {
                    (button (class' = "create-game-btn", type' = "button")).data ("on-click", "@post('/games')") {
                        "Create New Game"
                    }

                    a (href = "/games", class' = "view-games-link") { "View All Games" }
                }

                div (class' = "info") {
                    p () { "Create a new game or view existing games to play." }
                    p (class' = "tech-note") { "Powered by F#, Datastar, and Server-Sent Events" }
                }
            }
        }
    }

let homeStyles =
    style () {
        raw
            """
        .home-container {
            max-width: 600px;
            margin: 40px auto;
            padding: 20px;
            text-align: center;
            font-family: Arial, sans-serif;
        }

        .home-title {
            font-size: 3em;
            color: #333;
            margin-bottom: 30px;
        }

        .home-content {
            display: flex;
            flex-direction: column;
            gap: 30px;
        }

        .stats {
            background-color: #f5f5f5;
            padding: 20px;
            border-radius: 8px;
        }

        .stats h2 {
            margin: 0 0 10px 0;
            color: #555;
            font-size: 1.2em;
        }

        .game-count {
            font-size: 2.5em;
            font-weight: bold;
            color: #2c3e50;
        }

        .actions {
            display: flex;
            flex-direction: column;
            gap: 15px;
            align-items: center;
        }

        .create-game-btn {
            background-color: #27ae60;
            color: white;
            padding: 15px 30px;
            font-size: 18px;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: background-color 0.2s;
            font-weight: bold;
        }

        .create-game-btn:hover {
            background-color: #229954;
        }

        .view-games-link {
            color: #3498db;
            text-decoration: none;
            font-size: 16px;
            padding: 10px 20px;
            border: 2px solid #3498db;
            border-radius: 6px;
            transition: all 0.2s;
        }

        .view-games-link:hover {
            background-color: #3498db;
            color: white;
        }

        .info {
            color: #666;
            line-height: 1.6;
        }

        .info p {
            margin: 10px 0;
        }

        .tech-note {
            font-size: 0.9em;
            font-style: italic;
        }
        """
    }
