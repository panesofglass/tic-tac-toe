using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Engine;
using TicTacToe.Web.Models;

namespace TicTacToe.Tests.Web;

public class WebIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
}
