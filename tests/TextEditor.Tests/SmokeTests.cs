using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TextEditor.Tests.TestHelpers;

namespace TextEditor.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void Database_CanBeCreated_AndQueried()
    {
        using var fixture = new SqliteInMemoryFixture();
        Assert.NotNull(fixture.Context.Users);
        Assert.NotNull(fixture.Context.Documents);
    }
}