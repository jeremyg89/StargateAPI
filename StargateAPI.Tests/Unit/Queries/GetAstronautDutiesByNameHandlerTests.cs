using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;

namespace StargateAPI.Tests.Unit.Queries;

public class GetAstronautDutiesByNameHandlerTests : UnitTestBase
{
    [Fact]
    public async Task Handle_MissingPerson_ReturnsNoDuties()
    {
        var handler = new GetAstronautDutiesByNameHandler(Context);

        var result = await handler.Handle(
            new GetAstronautDutiesByName { Name = "jane doe" },
            CancellationToken.None);

        Assert.Null(result.Person);
        Assert.Empty(result.AstronautDuties);
    }

    [Fact]
    public async Task Handle_PersonWithDuties_ReturnsNewestFirst()
    {
        var person = new Person { Name = "jane doe" };
        Context.People.Add(person);
        await Context.SaveChangesAsync();
        Context.AstronautDuties.AddRange(
            new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "CAPT",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2020, 1, 1)
            },
            new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "CAPT",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2024, 6, 1)
            });
        await Context.SaveChangesAsync();
        var handler = new GetAstronautDutiesByNameHandler(Context);

        var result = await handler.Handle(
            new GetAstronautDutiesByName { Name = "Jane Doe" },
            CancellationToken.None);

        Assert.NotNull(result.Person);
        Assert.Equal("RETIRED", result.AstronautDuties[0].DutyTitle);
        Assert.Equal("Commander", result.AstronautDuties[1].DutyTitle);
    }
}