using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests.Unit.Commands;

public class UpdatePersonHandlerTests : UnitTestBase
{
    [Fact]
    public async Task Handle_Rename_KeepsTheSameId()
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
        var handler = new UpdatePersonHandler(Context);

        var result = await handler.Handle(new UpdatePerson
        {
            Name = "Jane Doe",
            NewName = "Jane Doe Sr"
        }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(person.Id, result.Id);
        Assert.Equal("Person updated.", result.Message);
        Assert.Equal("jane doe sr", (await Context.People.SingleAsync()).Name);
        Assert.Equal(person.Id, (await Context.AstronautDuties.SingleAsync()).PersonId);
    }

    [Fact]
    public async Task Handle_MissingPerson_ReturnsNotFound()
    {
        var handler = new UpdatePersonHandler(Context);

        var result = await handler.Handle(new UpdatePerson
        {
            Name = "jane doe",
            NewName = "janet doe"
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Person not found.", result.Message);
    }

    [Fact]
    public async Task Handle_BlankNewName_ReturnsNameRequired()
    {
        Context.People.Add(new Person { Name = "jane doe" });
        await Context.SaveChangesAsync();
        var handler = new UpdatePersonHandler(Context);

        var result = await handler.Handle(new UpdatePerson
        {
            Name = "jane doe",
            NewName = "   "
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Name is required.", result.Message);
        Assert.Equal("jane doe", (await Context.People.SingleAsync()).Name);
    }

    [Fact]
    public async Task Handle_NameTakenBySomeoneElse_ReturnsAlreadyExists()
    {
        Context.People.Add(new Person { Name = "jane doe" });
        Context.People.Add(new Person { Name = "john doe" });
        await Context.SaveChangesAsync();
        var handler = new UpdatePersonHandler(Context);

        var result = await handler.Handle(new UpdatePerson
        {
            Name = "jane doe",
            NewName = "John Doe"
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Person already exists.", result.Message);
        var names = await Context.People.Select(p => p.Name).ToListAsync();
        Assert.Contains("jane doe", names);
        Assert.Contains("john doe", names);
    }
}