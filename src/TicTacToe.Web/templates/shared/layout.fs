namespace TicTacToe.Web.templates.shared

open Microsoft.AspNetCore.Http
open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria

#nowarn "3391"

module layout =
    let mainLayout (ctx: HttpContext) (content: HtmlElement) =
        section (class' = "relative") {
            div (class' = "w-full ml-xs p-4 bg-white") { content }
        }

    let html (ctx: HttpContext) (content: HtmlElement) =
        html (lang = "en") {
            head () {
                title () {
                    match ctx.Items.TryGetValue "Title" with
                    | true, title -> string title
                    | false, _ -> "F# + Datastar"
                }

                meta (charset = "utf-8")
                meta (name = "viewport", content = "width=device-width, initial-scale=1.0")
                base' (href = "/")
                link (rel = "icon", type' = "image/png", href = "/favicon.png")
                link (rel = "stylesheet", href = "/app.css")

                script (
                    type' = "module",
                    src = "https://cdn.jsdelivr.net/gh/starfederation/datastar@v1.0.0-RC.5/bundles/datastar.js",
                    crossorigin = "anonymous"
                )
            }

            body () { mainLayout ctx content }
        }
