using Bison.Core.domain_model;
using Bison.Razor.Tests;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Internal;
using System.Text.RegularExpressions;
using static System.Formats.Asn1.AsnWriter;


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
            DbInitializer.Initialize(_context);
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

        [Test]
        public async Task LoginPreventionTest()
        {
            await Page.GotoAsync(uri +"/obs");

            // attempt to login without the account having been created
            await Page.GetByRole(AriaRole.Link, new(){Name = "Login"}).ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/Login"));

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");


            // Expect to still be on the login page, but with a warning
            await Page.GetByRole(AriaRole.Button, new(){Name = "Log in"}).ClickAsync();

            await Expect(Page.GetByText("Invalid login attempt.")).ToHaveCountAsync(1);




            // Click the Register link.
            await Page.GetByRole(AriaRole.Link, new(){Name = "Register as a new user"}).ClickAsync();

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.Locator("css=#Input_ConfirmPassword").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Register"}).ClickAsync();

            await Page.GetByRole(AriaRole.Link, new(){Name = "Click here to confirm your account"}).ClickAsync();


            //atempt to login to with false password

            await Page.GetByRole(AriaRole.Link, new(){Name = "Login"}).ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*Identity/Account/Login"));

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("321_DrOwSsAp");

            // Expect to still be on the login page, but with a warning
            await Page.GetByRole(AriaRole.Button, new(){Name = "Log in"}).ClickAsync();

            await Expect(Page.GetByText("Invalid login attempt.")).ToHaveCountAsync(1);
        }


        [Test]
        public async Task CreateObservationTest()
        {
            // registration portion
            await Page.GotoAsync(uri + "/obs");

            await Page.GetByRole(AriaRole.Link, new(){Name = "Register"}).ClickAsync();

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.Locator("css=#Input_ConfirmPassword").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Register"}).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new(){Name = "Click here to confirm your account"}).ClickAsync();


            // Login portion
            await Page.GetByRole(AriaRole.Link, new(){Name = "Login"}).ClickAsync();

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Log in"}).ClickAsync();

            // posting portion
            // expect to be on public page
            await Expect(Page).ToHaveURLAsync(new Regex(".*/obs"));
            await Expect(Page.GetByText("Hello what did you see, test@testmail.mail?")).ToHaveCountAsync(1);
            
            await Page.Locator("css=#NewObservationText").FillAsync("I saw mommy kissing Santa Claus");

            await Page.Locator("css=#NewObservationTaxonId").SelectOptionAsync("Mammal");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Share"}).ClickAsync();

            await Expect(Page.GetByText("I saw mommy kissing Santa Claus")).ToHaveCountAsync(1);

            await Page.Locator("css=#taxonSelect").SelectOptionAsync("Mammal");

            await Expect(Page.GetByText("I saw mommy kissing Santa Claus")).ToHaveCountAsync(1);

            await Page.Locator("css=#taxonSelect").SelectOptionAsync("Bird");

            await Expect(Page.GetByText("I saw mommy kissing Santa Claus")).ToHaveCountAsync(0);
        }


        [Test]
        public async Task CreateCommentTest()
        {
            // registration portion
            await Page.GotoAsync(uri + "/obs");

            await Page.GetByRole(AriaRole.Link, new(){Name = "Register"}).ClickAsync();

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.Locator("css=#Input_ConfirmPassword").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new()
            {
                Name = "Register"
            }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new()
            {
                Name = "Click here to confirm your account"
            }).ClickAsync();


            // Login portion
            await Page.GetByRole(AriaRole.Link, new(){Name = "Login"}).ClickAsync();

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Log in"}).ClickAsync();

            
            // posting portion
            // expect to be on public page
            await Expect(Page).ToHaveURLAsync(new Regex(".*/obs"));

            await Page.GetByRole(AriaRole.Link, new(){Name = "View details"}).First.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*/ob/1"));

            await Page.GetByRole(AriaRole.Textbox, new(){Name = "Comment"}).FillAsync("this is a comment");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Comment"}).ClickAsync();

            await Expect(Page.GetByText("this is a comment")).ToHaveCountAsync(1);
        }

        [Test]
        public async Task CreateProposalTest()
        {
            // registration portion
            await Page.GotoAsync(uri + "/obs");

            await Page.GetByRole(AriaRole.Link, new(){Name = "Register"}).ClickAsync();

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.Locator("css=#Input_ConfirmPassword").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Register"}).ClickAsync();

            await Page.GetByRole(AriaRole.Link, new(){Name = "Click here to confirm your account"}).ClickAsync();


            // Login portion
            await Page.GetByRole(AriaRole.Link, new(){Name = "Login"}).ClickAsync();

            await Page.Locator("css=#Input_Email").FillAsync("test@testmail.mail");

            await Page.Locator("css=#Input_Password").FillAsync("pAsSwOrD_123");

            await Page.GetByRole(AriaRole.Button, new(){Name = "Log in"}).ClickAsync();


            // posting portion
            // expect to be on public page
            await Expect(Page).ToHaveURLAsync(new Regex(".*/obs"));

            await Page.GetByRole(AriaRole.Link, new(){Name = "View details"}).First.ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*/ob/1"));

            await Page.GetByRole(AriaRole.Textbox, new(){Name = "Proposal"}).FillAsync("This is a bird of some kind, but not a heron");

            await Page.GetByLabel("Proposed animal:").SelectOptionAsync(new[] { "2" });

            await Page.GetByRole(AriaRole.Button, new(){Name = "Submit proposal"}).ClickAsync();

            //expect that there is a list element that somewhere has the following two pieces of text
            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new(){
                HasText = "test@testmail.mail Proposed: Bird"
            }).Filter(new()
            {
                HasText = "This is a bird of some kind, but not a heron"
            })).ToHaveCountAsync(1);
        }
    }
}




