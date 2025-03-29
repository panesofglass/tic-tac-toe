using System.ComponentModel.DataAnnotations;

namespace TicTacToe.Web.Models;

public record struct RegisterModel(
    [Required] [DataType(DataType.EmailAddress)] [Display(Name = "Email")] string Email,
    [Required] [DataType(DataType.Text)] [Display(Name = "Display Name")] string Name,
    [Required] [DataType(DataType.Password)] [Display(Name = "Password")] string Password
);

public record struct LoginModel(
    [Required] [DataType(DataType.EmailAddress)] [Display(Name = "Email")] string Email,
    [Required] [DataType(DataType.Password)] [Display(Name = "Password")] string Password
);
