namespace TicTacToe.Web.Models;

public class AuthModels
{
    public record RegisterModel
    {
        public required string Email { get; init; }
        public required string Name { get; init; }
        public required string Password { get; init; }

        // For model binding from form data
        public static ValueTask<RegisterModel?> BindAsync(HttpContext context)
        {
            var form = context.Request.Form;
            if (!form.TryGetValue("email", out var email) ||
                !form.TryGetValue("name", out var name) ||
                !form.TryGetValue("password", out var password))
            {
                return ValueTask.FromResult<RegisterModel?>(null);
            }

            return ValueTask.FromResult<RegisterModel?>(new RegisterModel
            {
                Email = email.ToString(),
                Name = name.ToString(),
                Password = password.ToString()
            });
        }
    }

    public record LoginModel
    {
        public required string Email { get; init; }
        public required string Password { get; init; }

        // For model binding from form data
        public static ValueTask<LoginModel?> BindAsync(HttpContext context)
        {
            var form = context.Request.Form;
            if (!form.TryGetValue("email", out var email) ||
                !form.TryGetValue("password", out var password))
            {
                return ValueTask.FromResult<LoginModel?>(null);
            }

            return ValueTask.FromResult<LoginModel?>(new LoginModel
            {
                Email = email.ToString(),
                Password = password.ToString()
            });
        }
    }
}
