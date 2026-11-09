using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Tests;
using RaceDay.Contracts.DTOs;
using Xunit;

namespace RaceDay.Api.Tests.Controllers;

public class CategoriesControllerTests
{
    [Fact]
    public async Task Create_ForOwnEvent_CreatesCategory()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "cat-org@test.local", "Organiser");
        db.Users.Add(organiser);
        var @event = Factories.Event(organiser.Id);
        db.Events.Add(@event);
        await db.SaveChangesAsync();

        var controller = new CategoriesController(db);
        TestHost.SetUser(controller, organiser.Id, "Organiser");

        var result = await controller.Create(@event.Id, new CreateCategoryRequest { Name = "Veterans" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<RaceDay.Contracts.DTOs.CategoryDto>(created.Value);
        Assert.Equal("Veterans", dto.Name);
        Assert.Equal(@event.Id, dto.EventId);
    }

    [Fact]
    public async Task Create_ForUnknownEvent_ReturnsNotFound()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "cat-org2@test.local", "Organiser");
        db.Users.Add(organiser);
        await db.SaveChangesAsync();

        var controller = new CategoriesController(db);
        TestHost.SetUser(controller, organiser.Id, "Organiser");

        var actionResult = await controller.Create(999, new CreateCategoryRequest { Name = "X" });
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetCategories_ReturnsAll()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "cat-org3@test.local", "Organiser");
        db.Users.Add(organiser);
        var @event = Factories.Event(organiser.Id);
        db.Events.Add(@event);
        db.Categories.Add(Factories.Category(@event.Id, "Senior"));
        db.Categories.Add(Factories.Category(@event.Id, "Junior"));
        await db.SaveChangesAsync();

        var controller = new CategoriesController(db);
        var result = await controller.GetCategories();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<RaceDay.Contracts.DTOs.CategoryDto>>(ok.Value);
        Assert.Equal(2, categories.Count());
    }

    [Fact]
    public async Task Delete_OtherOrganisersCategory_ReturnsForbid()
    {
        var db = TestHost.CreateDb();
        var owner = Factories.User("Owner", "cat-owner@test.local", "Organiser");
        var other = Factories.User("Other", "cat-other@test.local", "Organiser");
        db.Users.AddRange(owner, other);
        var @event = Factories.Event(owner.Id);
        db.Events.Add(@event);
        var category = Factories.Category(@event.Id);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var controller = new CategoriesController(db);
        TestHost.SetUser(controller, other.Id, "Organiser");

        Assert.IsType<ForbidResult>(await controller.Delete(category.Id));
    }
}
