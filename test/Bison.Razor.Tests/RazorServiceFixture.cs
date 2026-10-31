using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

public class RazorServiceFixture : WebApplicationFactory<Program>
{
    //Get the services used by the program
    public IObservationService _observationService => Services.GetRequiredService<IObservationService>();
    public IPostRepository _postRepository => Services.GetRequiredService<IPostRepository>();
    public IAuthorRepository _authorRepository => Services.GetRequiredService<IAuthorRepository>();
    public ITaxonRepository _taxonRepository => Services.GetRequiredService<ITaxonRepository>();
}