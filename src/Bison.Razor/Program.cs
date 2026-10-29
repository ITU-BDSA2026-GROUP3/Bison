using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // this line is optional

app.MapRazorPages();

app.Run();
