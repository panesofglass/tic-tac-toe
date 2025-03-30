namespace TicTacToe.Web.Models;

public record struct RegisterModel(string Email, string Name, string Password);

public record struct LoginModel(string Email, string Password);
