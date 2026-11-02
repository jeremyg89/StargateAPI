using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests.Unit.Commands;

public class CreateAstronautDutyHandlerTests : UnitTestBase
{
    [Fact]
    public async Task Handle_MissingPerson_ReturnsNotFound()
    {
        var handler = new CreateAstronautDutyHandler(Context);

        var result = await handler.Handle(new CreateAstronautDuty
        {
            Name = "Jane Doe",
            Rank = "CAPT",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2020, 1, 1)
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Person not found.", result.Message);
    }

    [Fact]
    public async Task Handle_FirstDuty_CreatesDetailAndOpenDuty()
    {
        Context.People.Add(new Person { Name = "jane doe" });
        await Context.SaveChangesAsync();
        var handler = new CreateAstronautDutyHandler(Context);
        var start = new DateTime(2020, 1, 1);

        var result = await handler.Handle(new CreateAstronautDuty
        {
            Name = "Jane Doe",
            Rank = "CAPT",
            DutyTitle = "Commander",
            DutyStartDate = start
        }, CancellationToken.None);

        Assert.True(result.Success);
        var detail = await Context.AstronautDetails.SingleAsync();
        Assert.Equal("Commander", detail.CurrentDutyTitle);
        Assert.Equal(start, detail.CareerStartDate);
        Assert.Null(detail.CareerEndDate);
        Assert.Null((await Context.AstronautDuties.SingleAsync()).DutyEndDate);
    }

    [Fact]
    public async Task Handle_SecondDuty_EndsPreviousDutyTheDayBefore()
    {
        var person = new Person { Name = "jane doe" };
        Context.People.Add(person);
        await Context.SaveChangesAsync();
        Context.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = "CAPT",
            CurrentDutyTitle = "Commander",
            CareerStartDate = new DateTime(2020, 1, 1)
        });
        Context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            Rank = "CAPT",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2020, 1, 1)
        });
        await Context.SaveChangesAsync();
        var handler = new CreateAstronautDutyHandler(Context);

        await handler.Handle(new CreateAstronautDuty
        {
            Name = "jane doe",
            Rank = "CAPT",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2021, 6, 1)
        }, CancellationToken.None);

        var duties = await Context.AstronautDuties.OrderBy(d => d.DutyStartDate).ToListAsync();
        Assert.Equal(new DateTime(2021, 5, 31), duties[0].DutyEndDate);
        Assert.Null(duties[1].DutyEndDate);
    }

    [Fact]
    public async Task Handle_DuplicateDuty_ReturnsBadRequest()
    {
        var person = new Person { Name = "jane doe" };
        Context.People.Add(person);
        await Context.SaveChangesAsync();
        Context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            Rank = "CAPT",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2020, 1, 1)
        });
        await Context.SaveChangesAsync();
        var handler = new CreateAstronautDutyHandler(Context);

        var result = await handler.Handle(new CreateAstronautDuty
        {
            Name = "Jane Doe",
            Rank = "CAPT",
            DutyTitle = "Commander",
            DutyStartDate = new DateTime(2020, 1, 1, 8, 30, 0)
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Duplicate duty.", result.Message);
    }

    [Fact]
    public async Task Handle_RetiredDuty_SetsCareerEndDateToDayBefore()
    {
        Context.People.Add(new Person { Name = "jane doe" });
        await Context.SaveChangesAsync();
        var handler = new CreateAstronautDutyHandler(Context);

        await handler.Handle(new CreateAstronautDuty
        {
            Name = "jane doe",
            Rank = "CAPT",
            DutyTitle = "RETIRED",
            DutyStartDate = new DateTime(2024, 6, 1)
        }, CancellationToken.None);

        var detail = await Context.AstronautDetails.SingleAsync();
        Assert.Equal(new DateTime(2024, 5, 31), detail.CareerEndDate);
    }
}