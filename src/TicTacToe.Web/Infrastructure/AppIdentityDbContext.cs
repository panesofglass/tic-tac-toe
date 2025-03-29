using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class TicTacToeIdentityDbContext : IdentityDbContext<IdentityUser>
{
    public TicTacToeIdentityDbContext(DbContextOptions<TicTacToeIdentityDbContext> options)
        : base(options) { }
}
