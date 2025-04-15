namespace TicTacToe.Web

open System
open System.Text.Json
open System.Text.Json.Serialization

type HomeSignal =
    {
        [<JsonPropertyName "delay">]
        Delay: float
    }
