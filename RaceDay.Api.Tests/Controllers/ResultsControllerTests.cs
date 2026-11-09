using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Controllers;
using RaceDay.Api.Tests;
using RaceDay.Contracts.DTOs;
using Xunit;

namespace RaceDay.Api.Tests.Controllers;

public class ResultsControllerTests
{
    private static async Task<(RaceDay.Api.Models.Enrolment enrolment, int ownerId)> SeedAsync(RaceDay.Api.Data.ApplicationDbContext db)
    {
        var owner = Factories.User("Owner", $"res-owner{Guid.NewGuid():N}@test.local", "Organiser");
        var participant = Factories.User("Pat", $"res-pat{Guid.NewGuid():N}@test.local");
        db.Users.AddRange(owner, participant);
        var @event = Factories.Event(owner.Id);
        db.Events.Add(@event);
        db.Categories.Add(Factories.Category(@event.Id));
        await db.SaveChangesAsync();

        var enrolment = new RaceDay.Api.Models.Enrolment
        {
            EventId = @event.Id,
            CategoryId = db.Categories.First().Id,
            UserId = participant.Id
        };
        db.Enrolments.Add(enrolment);
        await db.SaveChangesAsync();
        return (enrolment, owner.Id);
    }

    [Fact]
    public async Task Create_AsOwnerOrganiser_CapturesResult()
    {
        var db = TestHost.CreateDb();
        var (enrolment, ownerId) = await SeedAsync(db);

        var controller = new ResultsController(db);
        TestHost.SetUser(controller, ownerId, "Organiser");

        var result = await controller.Create(new CreateResultRequest
        {
            EnrolmentId = enrolment.Id,
            FinishTimeSeconds = 2730.5,
            Position = 3
        });

        var created = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<RaceDay.Contracts.DTOs.ResultDto>(created.Value);
        Assert.Equal(2730.5, dto.FinishTimeSeconds);
        Assert.Equal(3, dto.Position);
        Assert.Equal(2730.5, db.Results.First().FinishTimeSeconds);
    }

    [Fact]
    public async Task Create_ForOtherOrganisersEvent_ReturnsForbid()
    {
        var db = TestHost.CreateDb();
        var (enrolment, _) = await SeedAsync(db);
        var other = Factories.User("Other", $"res-other{Guid.NewGuid():N}@test.local", "Organiser");
        db.Users.Add(other);
        await db.SaveChangesAsync();

        var controller = new ResultsController(db);
        TestHost.SetUser(controller, other.Id, "Organiser");

        var actionResult = await controller.Create(
            new CreateResultRequest { EnrolmentId = enrolment.Id, FinishTimeSeconds = 100, Position = 1 });
        Assert.IsType<ForbidResult>(actionResult.Result);
    }

    [Fact]
    public async Task Create_NonPositiveFinishTime_ReturnsBadRequest()
    {
        var db = TestHost.CreateDb();
        var (enrolment, ownerId) = await SeedAsync(db);

        var controller = new ResultsController(db);
        TestHost.SetUser(controller, ownerId, "Organiser");

        var actionResult = await controller.Create(
            new CreateResultRequest { EnrolmentId = enrolment.Id, FinishTimeSeconds = 0, Position = 1 });
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetMyResults_ReturnsOnlyOwnResults()
    {
        var db = TestHost.CreateDb();
        var (enrolment, ownerId) = await SeedAsync(db);
        db.Results.Add(new RaceDay.Api.Models.Result
        {
            EnrolmentId = enrolment.Id,
            FinishTimeSeconds = 1800,
            Position = 2
        });
        await db.SaveChangesAsync();

        var controller = new ResultsController(db);
        TestHost.SetUser(controller, enrolment.UserId, "Participant");

        var result = await controller.GetMyResults();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var results = Assert.IsAssignableFrom<IEnumerable<RaceDay.Contracts.DTOs.ResultDto>>(ok.Value);
        var dto = Assert.Single(results);
        Assert.Equal(1800, dto.FinishTimeSeconds);
        Assert.Equal(2, dto.Position);
    }
}
