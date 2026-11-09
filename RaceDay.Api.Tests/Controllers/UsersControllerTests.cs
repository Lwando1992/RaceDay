using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Tests;
using RaceDay.Contracts.DTOs;
using Xunit;

namespace RaceDay.Api.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task Profile_ReturnsCurrentUser()
    {
        var db = TestHost.CreateDb();
        var user = Factories.User("Jane Runner", "jane@test.local");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = TestHost.UsersController(db);
        TestHost.SetUser(controller, user.Id, "Participant");

        var result = await controller.Profile();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RaceDay.Contracts.DTOs.UserDto>(ok.Value);
        Assert.Equal("Jane Runner", dto.FullName);
    }

    [Fact]
    public async Task UpdateProfile_ChangesFullName()
    {
        var db = TestHost.CreateDb();
        var user = Factories.User("Old Name", "upd@test.local");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = TestHost.UsersController(db);
        TestHost.SetUser(controller, user.Id, "Participant");

        var result = await controller.UpdateProfile(new UpdateProfileRequest { FullName = "New Name", ImageUrl = "http://img/x.png" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RaceDay.Contracts.DTOs.UserDto>(ok.Value);
        Assert.Equal("New Name", dto.FullName);
        Assert.Equal("http://img/x.png", dto.ImageUrl);
    }

    [Fact]
    public async Task Profile_UnknownUser_ReturnsNotFound()
    {
        var db = TestHost.CreateDb();
        var controller = TestHost.UsersController(db);
        TestHost.SetUser(controller, 999, "Participant");

        var actionResult = await controller.Profile();
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }
}
