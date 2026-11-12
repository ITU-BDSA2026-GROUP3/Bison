using Bison.Razor.domain_model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class RazorEndToEndTests : IDisposable, IClassFixture<RazorServiceFactory>
{
    HttpClient Client;
    RazorServiceFactory _factory;
    BisonContext _context;

    public RazorEndToEndTests(RazorServiceFactory factory)
    {
        _factory = factory;
        var scope = _factory.CreateScope();
        _context = scope.ServiceProvider.GetService<BisonContext>();
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect = false});
        Client.DefaultRequestHeaders.Add("Test-Authentication", "true");
    }

    [Fact]
    public async Task ObservationsEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil"};
        Author author2 = new Author{UserName = "Emil Smil", Email = "smil@com.emil"};
        Taxon taxon1 = new Taxon{Name = "Sej gut", Description = "Lololo"};
        Taxon taxon2 = new Taxon{Name = "Sej guttere", Description = "Lololoslos"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "I saw a Sej gut", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author2, Taxon = taxon2, Text = "I saw a Sejere guttere", TimeStamp = DateTime.Now};

        _context.AddRange(author1, author2, taxon1, taxon2, obs1, obs2);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync("/obs");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);

        //Assert
        Assert.Contains("Emil", content); // Observation 1 author
        Assert.Contains("I saw a Sej gut", content); // Observation 1 text
        Assert.Contains("Emil Smil", content); // Observation 2 author
        Assert.Contains("I saw a Sejere guttere", content); // Observation 2 text
    }

    [Fact]
    public async Task NoObservationsEndToEnd()
    {
        //Arrange

        //Act
        var response = await Client.GetAsync("/obs");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("<em>There are no observations so far.</em>", content);
    }

    [Fact]
    public async Task ObservationDetailsEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil"};
        Author author2 = new Author{UserName = "Emil Smil", Email = "smil@com.emil"};
        Author author3 = new Author{UserName = "Admin", Email = "admin@com.emil"};
        Taxon taxon1 = new Taxon{Name = "Sej gut", Description = "Lololo"};
        Taxon taxon2 = new Taxon{Name = "Sej guttere", Description = "Lololoslos"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "I saw a Sej gut", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author2, Taxon = taxon2, Text = "I saw a Sejere guttere", TimeStamp = DateTime.Now};
        Comment com1 = new Comment{Author = author2, Observation = obs1, Text = "min gut var sejere HAHAHAHA", TimeStamp = DateTime.Now};
        Proposal prop1 = new Proposal{Author = author3, Taxon = taxon2, Observation = obs1, Text = "Det er faktisk en sej guttere", TimeStamp = DateTime.Now};
        
        _context.AddRange(author1, author2, taxon1, taxon2, obs1, obs2, com1, prop1);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync("/ob/1");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Observation 1", content); // Observation Id

        Assert.Contains("Emil", content); //Original author
        Assert.Contains("I saw a Sej gut", content); // Original text

        Assert.Contains("Emil Smil", content); // Comment author
        Assert.Contains("min gut var sejere HAHAHAHA", content); // Comment text

        Assert.Contains("Proposed:", content); // Proposal 
        Assert.Contains("Admin", content); // Proposal author
        Assert.Contains("Sej guttere", content); // Proposal taxon
        Assert.Contains("Det er faktisk en sej guttere", content); // Proposal text
    }
    
    [Fact]
    public async Task NoObservationDetialsEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil"};
        Taxon taxon1 = new Taxon{Name = "Sej gut", Description = "Lololo"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "I saw a Sej gut", TimeStamp = DateTime.Now};

        _context.AddRange(author1, taxon1, obs1);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync("/ob/1");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Observation 1", content); // Observation Id

        Assert.Contains("Emil", content); //Original author
        Assert.Contains("I saw a Sej gut", content); // Original text

        Assert.Contains("<em>No comments for this observation </em>", content); // No comments text
        Assert.Contains("<em>No proposals for this observation</em>", content); // No proposals text
    }

    [Fact]
    public async Task ObservationsFromAuthorEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil",Id = "1"};
        Author author2 = new Author{UserName = "Jonas", Email = "jonais@com.jonas", Id = "2" };
        Taxon taxon1 = new Taxon{Name = "Villads", Description = "Pepsi manden"};
        Taxon taxon2 = new Taxon{Name = "Rune", Description = "warhammer 41000 skaber"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "I saw a Villads", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author2, Taxon = taxon2, Text = "I saw a Rune, that was cool", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Author = author1, Taxon = taxon2, Text = "I saw a Rune also", TimeStamp = DateTime.Now};

        _context.AddRange(author1, author2, taxon1, taxon2, obs1, obs2, obs3);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync("/obs/1");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Emil's Observations", content);

        //First observation
        Assert.Contains("Emil", content);
        Assert.Contains("I saw a Villads", content);

        //Second observation
        Assert.DoesNotContain("Jonas", content);
        Assert.DoesNotContain("I saw a Rune, that was cool", content);

        //Third observation
        Assert.Contains("Emil", content);
        Assert.Contains("I saw a Rune also", content);
    }

    [Fact]
    public async Task ZeroObservationsFromAuthorEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil", Id = "1"};

        _context.AddRange(author1);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync("/obs/1");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Emil's Observations", content);
        Assert.Contains("<em>There are no Observations so far.</em>", content);
    }

    [Fact]
    public async Task ObservationsFromNonExistingAuthorEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil"};
        Taxon taxon1 = new Taxon{Name = "Sej gut", Description = "Lololo"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "I saw a Sej gut", TimeStamp = DateTime.Now};

        _context.AddRange(author1, taxon1, obs1);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync("/obs/-1");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("<em>The given Author doesn't exist.</em>", content);

        //Emils observation
        Assert.DoesNotContain("Emil", content);
        Assert.DoesNotContain("I saw a Sej gut", content);
    } 

    [Fact]
    public async Task ObservationsFromTaxonEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil"};
        Author author2 = new Author{UserName = "Stabil"};
        Taxon taxon1 = new Taxon{Name = "top dawg"};
        Taxon taxon2 = new Taxon{Name = "middle dog", ancestor = taxon1};
        Taxon taxon3 = new Taxon{Name = "lonely dog"};
        Taxon taxon4 = new Taxon{Name = "small dog", ancestor = taxon2};
        Taxon taxon5 = new Taxon{Name = "smallest dog the size of an atom", ancestor = taxon4};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author2, Taxon = taxon2, Text = "coolmand", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Author = author2, Taxon = taxon3, Text = "ild og is", TimeStamp = DateTime.Now};
        Observation obs4 = new Observation{Author = author1, Taxon = taxon4, Text = "smack", TimeStamp = DateTime.Now};
        Observation obs5 = new Observation{Author = author1, Taxon = taxon5, Text = "smoll", TimeStamp = DateTime.Now};

        _context.AddRange(author1, author2, taxon1, taxon2, taxon3, taxon4, taxon5, obs1, obs2, obs3, obs4, obs5);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync($"/obs?taxonId={taxon2.Id}");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Filter by Taxon:", content); //Filter text
        Assert.Contains("middle dog", content); //Dropdown taxon text

        //Observation 1 (does not contain as it is parent and not subtaxon)
        Assert.DoesNotContain("Wow", content);

        //Observation 2 (contains as it is the taxon filtered by)
        Assert.Contains("coolmand", content);

        //Observation 3 (does not contain as it isn't the same species)
        Assert.DoesNotContain("ild og is", content);

        //Observation 4 (does contain as it is the child of the filtered taxon)
        Assert.Contains("smack", content);

        //Observation 5 (does contain as it is the grandchild of the filtered taxon)
        Assert.Contains("smoll", content);
    }

    [Fact]
    public async Task ZeroObservationsFromTaxonEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil"};
        Taxon taxon1 = new Taxon{Name = "Sej gut", Description = "Lololo"};
        Taxon taxon2 = new Taxon{Name = "et dyr der ikke kan beskrive med danske ord (den er fra England)", Description = "Los lossos"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "I saw a Sej gut", TimeStamp = DateTime.Now};

        _context.AddRange(author1, taxon1, taxon2, obs1);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync($"/obs?taxonId={taxon2.Id}");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Filter by Taxon:", content); //Filter text
        Assert.Contains("et dyr der ikke kan beskrive med danske ord (den er fra England)", content); //Dropdown taxon text

        Assert.Contains("<em>There are no observations so far.</em>", content);
    }

    [Fact]
    public async Task ObservationsFromNonExistingTaxonEndToEnd()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil", Email = "email@com.emil"};
        Taxon taxon1 = new Taxon{Name = "Sej gut", Description = "Lololo"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "I saw a Sej gut", TimeStamp = DateTime.Now};

        _context.AddRange(author1, taxon1, obs1);
        _context.SaveChanges();

        //Act
        var response = await Client.GetAsync("/obs?taxonId=2");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Filter by Taxon:", content); //Filter text
        Assert.Contains("Show All", content); //Dropdown taxon text

        Assert.Contains("Emil", content); //Defaults to show all when an invalid taxon is selected, therefore all observations show
        Assert.Contains("I saw a Sej gut", content);
    }

    public void Dispose()
    {
        
    }
}