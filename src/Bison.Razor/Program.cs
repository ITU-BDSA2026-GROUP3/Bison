using Bison.Razor.domain_model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddAuthentication()
            .AddGitHub(options =>
        {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret =
        builder.Configuration["Authentication:GitHub:ClientSecret"]!;
        });
        builder.Services.AddScoped<IObservationService, ObservationService>();
        builder.Services.AddScoped<IPostRepository, PostRepository>();
        builder.Services.AddScoped<ITaxonRepository, TaxonRepository>();
        builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<BisonContext>(options => options.UseSqlite(connectionString));

        builder.Services.AddDefaultIdentity<Author>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<BisonContext>();


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BisonContext>();

            context.Database.EnsureCreated();

            DbInitializer.Initialize(context);
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            // Configure the HTTP request pipeline.
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        //app.UseAuthentication();
        app.UseAuthorization();

        /*app.MapGet("/login", () =>
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = "/obs"
            };

            return Results.Challenge(authenticationProperties, new[] { "GitHub" });
        });

        app.MapGet("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/obs");
        }).RequireAuthorization();*/

        app.MapRazorPages();

        app.Run();
    }
}