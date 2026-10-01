using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bison.Razor.Tests;

public class RazorServiceFixture : IAsyncLifetime
{
    public WebApplication app;
    public HttpClient Client;
    public string BaseURL { get; } = "http://localhost:5000";
    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddSingleton<IObservationService, ObservationService>();


        app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapRazorPages();

        await app.StartAsync();

        var serverURLs = app.Urls;

        var baseURL = serverURLs.First();

        Client = new HttpClient {BaseAddress = new Uri(baseURL)};
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        await app.StopAsync();
    }
}