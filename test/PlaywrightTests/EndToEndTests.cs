using Bison.Razor;
using Bison.Razor.domain_model;
using Bison.Razor.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using Microsoft.Playwright.NUnit;
using Microsoft.Extensions.Hosting;

namespace PlaywrightTests
{
    [TestFixture]
    public class EndToEndTests : PageTest
    {
        RazorServiceFactory _factory;
        BisonContext _context;
        string uri;
        
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _factory = new RazorServiceFactory();
            uri = _factory.GetHostBaseAddress();
        }

        [SetUp]
        public async Task SetUp()
        {
            var scope = _factory.CreateScope();
            _context = scope.ServiceProvider.GetService<BisonContext>();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _factory.Dispose();
        }

        // WARNING requires that the test account is not already created
        [Test]
        public async Task RegistrationAndLoginTest()
        {
            await Page.GotoAsync(uri+"/obs");

            // Expect that there is no My Observations link
            await Expect(Page.GetByRole(AriaRole.Link, new(){Name = "My Observations"})).ToHaveCountAsync(0);

            // Click the Register link.
            await Page.GetByRole(AriaRole.Link, new(){Name = "Register"}).ClickAsync();

            // Expects the URL to be registration page.
            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/Register"));

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.Locator("css=#Input_ConfirmPassword").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Register"}).ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/RegisterConfirmation.*"));

            await Page.GetByRole(AriaRole.Link, new(){Name = "Click here to confirm your account"}).ClickAsync();


            // Login portion
            await Page.GetByRole(AriaRole.Link, new(){Name = "Login"}).ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/Login"));

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Log in"}).ClickAsync();



            // Expect being redirected to the public page and the my observations link to be there after logging in.
            await Expect(Page).ToHaveURLAsync(new Regex(".*obs"));

            await Expect(Page.GetByRole(AriaRole.Link, new(){Name = "My Observations"})).ToHaveCountAsync(1);

            // Finally log out again
            await Page.GetByRole(AriaRole.Link, new(){Name = "Logout [test@testmail.mail]"}).ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/Logout"));

            await Page.GetByRole(AriaRole.Button, new(){Name = "Click here to Logout"}).ClickAsync();
            
            // Expect that my Observations link is gone
            await Expect(Page.GetByRole(AriaRole.Link, new(){Name = "My Observations"})).ToHaveCountAsync(0);

        }
    }
}

