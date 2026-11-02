using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using System.Net;

namespace StargateAPI.Tests.Unit.Commands;

public class CreatePersonHandlerTests : UnitTestBase
{
    [Fact]
    public async Task Handle_NewPerson_StoresLowercaseName()
    {
        var handler = new CreatePersonHandler(Context);

        var result = await handler.Handle(
            new CreatePerson { Name = "Jane Doe" },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Person created.", result.Message);
        Assert.Equal("jane doe", (await Context.People.SingleAsync()).Name);
    }

    [Fact]
    public async Task Handle_ExistingPerson_ReturnsAlreadyExists()
    {
        Context.People.Add(new Person { Name = "jane doe" });
        await Context.SaveChangesAsync();
        var handler = new CreatePersonHandler(Context);

        var result = await handler.Handle(
            new CreatePerson { Name = "Jane Doe" },
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Person already exists.", result.Message);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.ResponseCode);
        Assert.Equal(1, await Context.People.CountAsync());
    }
}