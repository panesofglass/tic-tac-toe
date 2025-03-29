using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(TicTacToe.Web.Models.LoginModel))]
[JsonSerializable(typeof(TicTacToe.Web.Models.MoveModel))]
[JsonSerializable(typeof(TicTacToe.Web.Models.RegisterModel))]
partial class TicTacToeJsonContext : JsonSerializerContext { }
