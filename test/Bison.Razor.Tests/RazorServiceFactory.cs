using System.Data.Common;
using Bison.Razor.domain_model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bison.Razor.Tests;

public class RazorServiceFactory : WebApplicationFactory<Program>
{
    public IHost? _host;
    private string baseAddress = "http://127.0.0.1:0";
    private bool useFakeAuthentication = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(IDbContextOptionsConfiguration<BisonContext>));

            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            if (dbConnectionDescriptor != null)
                services.Remove(dbConnectionDescriptor);

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<BisonContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });

        if (useFakeAuthentication)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "IntegrationTest";
                    options.DefaultChallengeScheme = "IntegrationTest";
                    options.DefaultScheme = "IntegrationTest";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "IntegrationTest",
                    options => { });
            });
        }

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Github:ClientId"] = "test-client-id",
                ["Authentication:Github:ClientSecret"] = "test-client-secret"
            });
        });

        builder.UseEnvironment("Development");
        builder.UseSetting("urls", baseAddress);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();

        // Configure and start a real Kestrel host on a dynamic free port
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());
        _host = builder.Build();
        _host.Start();

        // Get the actual localhost URL/port Kestrel bound to
        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();

        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _host?.Dispose();
        base.Dispose(disposing);
    }

    public string GetHostBaseAddress()
    {
        if (_host is null)
        {
            using var _ = CreateDefaultClient();
        }
        return ClientOptions.BaseAddress.ToString().TrimEnd('/');
    }

    public IServiceScope CreateScope()
    {
        if (_host is null)
        {
            using var _ = CreateDefaultClient();
        }
        return _host!.Services.CreateScope();
    }

    public void UseFakeAuthentication()
    {
        useFakeAuthentication = true;
    }
}