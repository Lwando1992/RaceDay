using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Tests;
using RaceDay.Contracts.DTOs;
using Xunit;

namespace RaceDay.Api.Tests.Controllers;

public class EnrolmentsControllerTests
{
    [Fact]
    public async Task Enrol_WithValidCategory_CreatesEnrolment()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "en-org@test.local", "Organiser");
        var participant = Factories.User("Pat", "en-pat@test.local");
        db.Users.AddRange(organiser, participant);
        var @event = Factories.Event(organiser.Id);
        db.Events.Add(@event);
        db.Categories.Add(Factories.Category(@event.Id, "Senior"));
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        TestHost.SetUser(controller, participant.Id, "Participant");

        var result = await controller.Enrol(new CreateEnrolmentRequest
        {
            EventId = @event.Id,
            CategoryId = db.Categories.First().Id
        });

        var created = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<RaceDay.Contracts.DTOs.EnrolmentDto>(created.Value);
        Assert.Equal(participant.Id, dto.UserId);
        Assert.Equal("Senior", dto.CategoryName);
    }

    [Fact]
    public async Task Enrol_TwiceForSameEvent_ReturnsConflict()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "en-org2@test.local", "Organiser");
        var participant = Factories.User("Pat", "en-pat2@test.local");
        db.Users.AddRange(organiser, participant);
        var @event = Factories.Event(organiser.Id);
        db.Events.Add(@event);
        db.Categories.Add(Factories.Category(@event.Id));
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        TestHost.SetUser(controller, participant.Id, "Participant");
        var request = new CreateEnrolmentRequest { EventId = @event.Id, CategoryId = db.Categories.First().Id };

        await controller.Enrol(request);
        var second = await controller.Enrol(request);

        Assert.IsType<ConflictObjectResult>(second.Result);
    }

    [Fact]
    public async Task Enrol_CategoryFromOtherEvent_ReturnsBadRequest()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "en-org3@test.local", "Organiser");
        var participant = Factories.User("Pat", "en-pat3@test.local");
        db.Users.AddRange(organiser, participant);
        var eventA = Factories.Event(organiser.Id, "A");
        var eventB = Factories.Event(organiser.Id, "B");
        db.Events.AddRange(eventA, eventB);
        db.Categories.Add(Factories.Category(eventB.Id, "B-Category"));
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        TestHost.SetUser(controller, participant.Id, "Participant");

        var result = await controller.Enrol(new CreateEnrolmentRequest
        {
            EventId = eventA.Id,
            CategoryId = db.Categories.First().Id
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMyEnrolments_ReturnsOnlyOwnEnrolments()
    {
        var db = TestHost.CreateDb();
        var organiser = Factories.User("Org", "en-org4@test.local", "Organiser");
        var p1 = Factories.User("P1", "en-p4a@test.local");
        var p2 = Factories.User("P2", "en-p4b@test.local");
        db.Users.AddRange(organiser, p1, p2);
        var @event = Factories.Event(organiser.Id);
        db.Events.Add(@event);
        db.Categories.Add(Factories.Category(@event.Id));
        await db.SaveChangesAsync();

        var catId = db.Categories.First().Id;
        db.Enrolments.Add(new RaceDay.Api.Models.Enrolment { EventId = @event.Id, CategoryId = catId, UserId = p1.Id });
        db.Enrolments.Add(new RaceDay.Api.Models.Enrolment { EventId = @event.Id, CategoryId = catId, UserId = p2.Id });
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        TestHost.SetUser(controller, p1.Id, "Participant");

        var result = await controller.GetMyEnrolments();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var enrolments = Assert.IsAssignableFrom<IEnumerable<RaceDay.Contracts.DTOs.EnrolmentDto>>(ok.Value);
        Assert.Single(enrolments);
    }

    [Fact]
    public async Task GetEventEnrolments_AsNonOwnerOrganiser_ReturnsForbid()
    {
        var db = TestHost.CreateDb();
        var owner = Factories.User("Owner", "en-owner@test.local", "Organiser");
        var other = Factories.User("Other", "en-other@test.local", "Organiser");
        db.Users.AddRange(owner, other);
        var @event = Factories.Event(owner.Id);
        db.Events.Add(@event);
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        TestHost.SetUser(controller, other.Id, "Organiser");

        var actionResult = await controller.GetEventEnrolments(@event.Id);
        Assert.IsType<ForbidResult>(actionResult.Result);
    }
}
