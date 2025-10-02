namespace TicTacToe.Web

open System.Text.Json.Serialization

type HomeSignal =
    { [<JsonPropertyName "delay">]
      Delay: float }

// Simple request model for move submission
type MakeMoveRequest =
    { [<JsonPropertyName "player">]
      Player: string
      [<JsonPropertyName "position">]
      Position: string }
