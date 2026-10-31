using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class ObservationServiceIntegrationTests : IDisposable
{
    IObservationService service;
    
    public ObservationServiceIntegrationTests()
    {
        DbContextOptionsBuilder<BisonContext> options = new DbContextOptionsBuilder<BisonContext>();
        options.UseSqlite("../../Data/test.db");
        BisonContext bc = new  BisonContext(options.Options);

        bc.Database.EnsureCreated();

        IPostRepository _postRepository = new PostRepository(bc);
        IAuthorRepository _authorRepository = new AuthorRepository(bc);
        ITaxonRepository _taxonRepository = new TaxonRepository(bc);

        service = new ObservationService(_postRepository, _authorRepository, _taxonRepository);
    }

    public void Dispose()
    {
        
    }
}