using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;

namespace StargateAPI.Tests.Unit.Queries;

public class GetPersonByNameHandlerTests : UnitTestBase
{
    [Fact]
    public async Task Handle_MissingPerson_ReturnsNull()
    {
        var handler = new GetPersonByNameHandler(Context);

        var result = await handler.Handle(
            new GetPersonByName { Name = "jane doe" },
            CancellationToken.None);

        Assert.Null(result.Person);
    }

    [Fact]
    public async Task Handle_DifferentCasing_ReturnsThePerson()
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
        await Context.SaveChangesAsync();
        var handler = new GetPersonByNameHandler(Context);

        var result = await handler.Handle(
            new GetPersonByName { Name = "Jane Doe" },
            CancellationToken.None);

        Assert.NotNull(result.Person);
        Assert.Equal(person.Id, result.Person.PersonId);
        Assert.Equal("jane doe", result.Person.Name);
        Assert.Equal("CAPT", result.Person.CurrentRank);
    }
}