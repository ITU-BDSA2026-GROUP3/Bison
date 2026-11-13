using Bison.Razor;
using Bison.Razor.domain_model;
using Bison.Razor.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Text.RegularExpressions;

namespace PlaywrightTests
{
    [TestFixture]
    public class EndToEndTests : PageTest
    {


        // WARNING requires that the test account is not already created
        [Test]
        public async Task Registration()
        {

            await Page.GotoAsync("http://localhost:5273/obs");


            // create a locator
            var register = Page.GetByRole(AriaRole.Link, new()
            {
                Name = "Register"
            });

            // Click the get Register link.
            await register.ClickAsync();

            // Expects the URL to contain intro.
            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/Register"));

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.Locator("css=#Input_ConfirmPassword").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Register"}).ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/RegisterConfirmation.*"));

            

            await Page.GetByRole(AriaRole.Link, new(){Name = "Click here to confirm your account"}).ClickAsync();
        }
    }
}

