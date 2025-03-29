using System.ComponentModel.DataAnnotations;

namespace TicTacToe.Web.Models;

public record struct RegisterModel(
    [Required] [DataType(DataType.EmailAddress)] [Display(Name = "Email")] string Email,
    [Required] [DataType(DataType.Text)] [Display(Name = "Display Name")] string Name,
    [Required] [DataType(DataType.Password)] [Display(Name = "Password")] string Password
)
{
    // For model binding from form data
    public static ValueTask<RegisterModel?> BindAsync(HttpContext context)
    {
        var form = context.Request.Form;
        if (
            !form.TryGetValue("email", out var email)
            || !form.TryGetValue("password", out var password)
        )
        {
            return ValueTask.FromResult<RegisterModel?>(null);
        }

        return ValueTask.FromResult<RegisterModel?>(
            new RegisterModel { Email = email.ToString(), Password = password.ToString() }
        );
    }
}

public record struct LoginModel(
    [Required] [DataType(DataType.EmailAddress)] [Display(Name = "Email")] string Email,
    [Required] [DataType(DataType.Password)] [Display(Name = "Password")] string Password
)
{
    // For model binding from form data
    public static ValueTask<LoginModel?> BindAsync(HttpContext context)
    {
        var form = context.Request.Form;
        if (
            !form.TryGetValue("email", out var email)
            || !form.TryGetValue("password", out var password)
        )
        {
            return ValueTask.FromResult<LoginModel?>(null);
        }

        return ValueTask.FromResult<LoginModel?>(
            new LoginModel { Email = email.ToString(), Password = password.ToString() }
        );
    }
}
