using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Tests;
using RaceDay.Contracts.DTOs;
using Xunit;

namespace RaceDay.Api.Tests.Controllers;

public class EventsControllerTests
{
    [Fact]
    public async Task GetEvents_ReturnsAllEvents()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "org2@test.local", "Organiser");
        db.Users.Add(organiser);
        db.Events.Add(Factories.Event(organiser.Id, "Race A"));
        db.Events.Add(Factories.Event(organiser.Id, "Race B"));
        await db.SaveChangesAsync();

        var controller = new EventsController(db);

        var result = await controller.GetEvents();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsAssignableFrom<IEnumerable<RaceDay.Contracts.DTOs.EventDto>>(ok.Value);
        Assert.Equal(2, events.Count());
    }

    [Fact]
    public async Task Create_AsOrganiser_CreatesEvent()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "creator@test.local", "Organiser");
        db.Users.Add(organiser);
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        TestHost.SetUser(controller, organiser.Id, "Organiser");

        var result = await controller.Create(new CreateEventRequest
        {
            Name = "Marathon",
            Date = DateTime.UtcNow.AddDays(60),
            Location = "Downtown"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<RaceDay.Contracts.DTOs.EventDto>(created.Value);
        Assert.Equal("Marathon", dto.Name);
        Assert.Equal(organiser.Id, dto.OrganiserId);
    }

    [Fact]
    public async Task Update_OtherOrganisersEvent_ReturnsForbid()
    {
        var db = TestHost.CreateDb();
        var owner = Factories.User("Owner", "owner@test.local", "Organiser");
        var other = Factories.User("Other", "other@test.local", "Organiser");
        db.Users.AddRange(owner, other);
        var @event = Factories.Event(owner.Id);
        db.Events.Add(@event);
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        TestHost.SetUser(controller, other.Id, "Organiser");

        var result = await controller.Update(@event.Id, new UpdateEventRequest { Name = "Hacked", Date = DateTime.UtcNow });

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_OwnEvent_ReturnsNoContent()
    {
        var db = TestHost.CreateDb();
        var owner = Factories.User("Owner", "del@test.local", "Organiser");
        db.Users.Add(owner);
        var @event = Factories.Event(owner.Id);
        db.Events.Add(@event);
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        TestHost.SetUser(controller, owner.Id, "Organiser");

        Assert.IsType<NoContentResult>(await controller.Delete(@event.Id));
        Assert.Null(db.Events.Find(@event.Id));
    }

    [Fact]
    public async Task GetEvent_UnknownId_ReturnsNotFound()
    {
        var controller = new EventsController(TestHost.CreateDb());
        var actionResult = await controller.GetEvent(12345);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }
}
