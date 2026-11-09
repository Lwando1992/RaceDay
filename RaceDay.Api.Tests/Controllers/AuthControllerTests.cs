using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Tests;
using RaceDay.Contracts.DTOs;
using Xunit;

namespace RaceDay.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_CreatesUser_AndReturnsToken()
    {
        var db = TestHost.CreateDb();
        var controller = TestHost.AuthController(db);

        var result = await controller.Register(new RegisterRequest
        {
            FullName = "Jane Runner",
            Email = "jane@test.local",
            Password = "Secret#1",
            Role = "Participant"
        });

        var created = Assert.IsType<CreatedResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(created.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("jane@test.local", response.User.Email);
        Assert.Equal("Participant", response.User.Role);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var db = TestHost.CreateDb();
        var controller = TestHost.AuthController(db);

        await controller.Register(new RegisterRequest { FullName = "A", Email = "dup@test.local", Password = "x" });
        var second = await controller.Register(new RegisterRequest { FullName = "B", Email = "dup@test.local", Password = "y" });

        Assert.IsType<ConflictObjectResult>(second.Result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var db = TestHost.CreateDb();
        var controller = TestHost.AuthController(db);
        await controller.Register(new RegisterRequest { FullName = "A", Email = "login@test.local", Password = "pw123" });

        var result = await controller.Login(new LoginRequest { Email = "login@test.local", Password = "pw123" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var db = TestHost.CreateDb();
        var controller = TestHost.AuthController(db);
        await controller.Register(new RegisterRequest { FullName = "A", Email = "bad@test.local", Password = "right" });

        var result = await controller.Login(new LoginRequest { Email = "bad@test.local", Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Logout_WhenAuthenticated_ReturnsOk()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "org@test.local", "Organiser");
        db.Users.Add(organiser);
        await db.SaveChangesAsync();

        var controller = TestHost.AuthController(db);
        TestHost.SetUser(controller, organiser.Id, "Organiser");

        var result = controller.Logout();
        Assert.IsType<OkObjectResult>(result);
    }
}
