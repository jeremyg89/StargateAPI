using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;

namespace StargateAPI.Tests.Unit.Queries;

public class GetPeopleHandlerTests : UnitTestBase
{
    [Fact]
    public async Task Handle_NoPeople_ReturnsEmptyList()
    {
        var handler = new GetPeopleHandler(Context);

        var result = await handler.Handle(new GetPeople(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.People);
    }

    [Fact]
    public async Task Handle_People_IncludesRankAndTitleWhenPresent()
    {
        var withDetail = new Person { Name = "jane doe" };
        Context.People.Add(withDetail);
        Context.People.Add(new Person { Name = "john doe" });
        await Context.SaveChangesAsync();
        Context.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = withDetail.Id,
            CurrentRank = "CAPT",
            CurrentDutyTitle = "Commander",
            CareerStartDate = new DateTime(2020, 1, 1),
            CareerEndDate = new DateTime(2024, 5, 31)
        });
        await Context.SaveChangesAsync();
        var handler = new GetPeopleHandler(Context);

        var result = await handler.Handle(new GetPeople(), CancellationToken.None);

        var jane = Assert.Single(result.People, p => p.Name == "jane doe");
        Assert.Equal("CAPT", jane.CurrentRank);
        Assert.Equal("Commander", jane.CurrentDutyTitle);
        Assert.Equal(new DateTime(2020, 1, 1), jane.CareerStartDate);
        var john = Assert.Single(result.People, p => p.Name == "john doe");
        Assert.Equal(string.Empty, john.CurrentRank);
        Assert.Equal(string.Empty, john.CurrentDutyTitle);
        Assert.Null(john.CareerStartDate);
    }
}