using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{

    public abstract class UnitTestBase : IAsyncLifetime
    {
        private SqliteConnection _connection = null!;
        protected StargateContext Context { get; private set; } = null!;
        public async Task InitializeAsync()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            await _connection.OpenAsync();
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseSqlite(_connection)
                .Options;
            Context = new StargateContext(options);
            await Context.Database.EnsureCreatedAsync();
        }
        public async Task DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
