using System.Net;
using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class AuthenticationTests : IDisposable, IClassFixture<RazorServiceFactory>
{
    RazorServiceFactory _factory;
    BisonContext _context;

    public AuthenticationTests(RazorServiceFactory factory)
    {
        _factory = factory;
        var scope = _factory.CreateScope();
        _context = scope.ServiceProvider.GetService<BisonContext>();
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Theory]
    [InlineData("/obs")] //Public Timeline
    [InlineData("/ob/1")] //Details

    public async Task EndpointsReturnSuccessForUserNotLoggedIn(string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        Author author = new Author{Name = "Emil", Email = "email"};
        Taxon taxon = new Taxon{Name = "Cool animal", Description = "Epic animal"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "sejt dyr jeg så", TimeStamp = DateTime.Now};
        
        _context.AddRange(author, taxon, obs);
        _context.SaveChanges();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    [Theory]
    [InlineData("/obs/1")] //User Timeline

    public async Task EndpointsReturnDeniedForUserNotLoggedIn(string url)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect = false});
        Author author = new Author{Name = "Emil", Email = "email"};
        Taxon taxon = new Taxon{Name = "Cool animal", Description = "Epic animal"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "sejt dyr jeg så", TimeStamp = DateTime.Now};
        
        _context.AddRange(author, taxon, obs);
        _context.SaveChanges();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/obs")] //Public Timeline
    [InlineData("/ob/1")] //Details
    [InlineData("/obs/1")] //User Timeline

    public async Task EndpointsReturnSuccessForUserLoggedIn(string url)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect = false});
        client = AuthorizeClient(client);
        Author author = new Author{Name = "Emil", Email = "email"};
        Taxon taxon = new Taxon{Name = "Cool animal", Description = "Epic animal"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "sejt dyr jeg så", TimeStamp = DateTime.Now};
        
        _context.AddRange(author, taxon, obs);
        _context.SaveChanges();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task LoginRedirectsUser()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect = false});

        // Act
        var response = await client.GetAsync("/login");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLoginButtonWhenNotLoggedIn()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect = false});

        // Act
        var response = await client.GetAsync("/obs");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Register", content);
        Assert.Contains("Login", content);
    }

    [Fact]
    public async Task LogoutButtonWhenLoggedIn()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect = false});
        client = AuthorizeClient(client);

        // Act
        var response = await client.GetAsync("/obs");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Logout", content);
    }

    private HttpClient AuthorizeClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Test-Authentication", "true");

        return client;
    }

    public void Dispose()
    {
        
    }
}