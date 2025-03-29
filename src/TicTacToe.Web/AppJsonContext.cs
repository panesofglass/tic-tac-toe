using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(TicTacToe.Web.Models.MoveModel))]
partial class TicTacToeJsonContext : JsonSerializerContext { }
