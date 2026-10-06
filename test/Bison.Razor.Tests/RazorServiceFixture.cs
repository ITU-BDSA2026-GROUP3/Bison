using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

public class RazorServiceFixture : WebApplicationFactory<Program>
{
    public DBFacade db => Services.GetRequiredService<DBFacade>();
}