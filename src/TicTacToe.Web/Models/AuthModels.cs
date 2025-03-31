namespace TicTacToe.Web.Models;

public record struct RegisterModel(string Email, string Name, string Password)
{
    public static bool TryBind(HttpContext context, out RegisterModel model)
    {
        var form = context.Request.Form;
        if (
            !form.TryGetValue("email", out var email)
            || !form.TryGetValue("name", out var name)
            || !form.TryGetValue("password", out var password)
        )
        {
            model = default;
            return false;
        }
        model = new RegisterModel(email.ToString(), name.ToString(), password.ToString());
        return true;
    }
};

public record struct LoginModel(string Email, string Password)
{
    public static bool TryBind(HttpContext context, out LoginModel model)
    {
        var form = context.Request.Form;
        if (
            !form.TryGetValue("email", out var email)
            || !form.TryGetValue("password", out var password)
        )
        {
            model = default;
            return false;
        }
        model = new LoginModel(email.ToString(), password.ToString());
        return true;
    }
};
