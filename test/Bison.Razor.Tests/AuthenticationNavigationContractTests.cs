namespace Bison.Razor.Tests;

public class AuthenticationNavigationContractTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void ProgramConfiguresAuthentication()
    {
        var program = ReadSourceFile("src", "Bison.Razor", "Program.cs");

        Assert.Contains("AddAuthentication", program);
        Assert.Contains("AddCookie", program);
        Assert.Contains("AddGitHub", program);
    }

    [Fact]
    public void ProgramUsesAuthenticationBeforeAuthorization()
    {
        var program = ReadSourceFile("src", "Bison.Razor", "Program.cs");

        var authenticationIndex = program.IndexOf(
            "app.UseAuthentication()",
            StringComparison.Ordinal);

        var authorizationIndex = program.IndexOf(
            "app.UseAuthorization()",
            StringComparison.Ordinal);

        Assert.True(
            authenticationIndex >= 0,
            "Program.cs must call app.UseAuthentication().");

        Assert.True(
            authorizationIndex >= 0,
            "Program.cs must call app.UseAuthorization().");

        Assert.True(
            authenticationIndex < authorizationIndex,
            "Authentication middleware must run before authorization middleware.");
    }

    [Fact]
    public void GitHubCallbackPathIsConfigured()
    {
        var program = ReadSourceFile("src", "Bison.Razor", "Program.cs");

        Assert.Contains("/signin-github", program);
    }

    [Fact]
    public void GitHubCredentialsComeFromConfiguration()
    {
        var program = ReadSourceFile("src", "Bison.Razor", "Program.cs");

        Assert.Contains("ClientId", program);
        Assert.Contains("ClientSecret", program);

        Assert.DoesNotContain("github_pat_", program);
        Assert.DoesNotContain("ghp_", program);
        Assert.DoesNotContain("gho_", program);
    }

    [Fact]
    public void LoginEndpointStartsAuthenticationChallenge()
    {
        var source = ReadApplicationSource();

        Assert.Contains("/login", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Challenge", source, StringComparison.Ordinal);
    }

    [Fact]
    public void LogoutEndpointSignsOutTheUser()
    {
        var source = ReadApplicationSource();

        Assert.Contains("/logout", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SignOut", source, StringComparison.Ordinal);
    }

    [Fact]
    public void NavigationContainsPublicObservationsLink()
    {
        var razorSource = ReadRazorSource();

        Assert.Contains("Public Observations", razorSource);
    }

    [Fact]
    public void NavigationContainsLoginLink()
    {
        var razorSource = ReadRazorSource();

        Assert.Contains("Login", razorSource);
    }

    [Fact]
    public void NavigationContainsAuthenticatedUserLinks()
    {
        var razorSource = ReadRazorSource();

        Assert.Contains("My Observations", razorSource);
        Assert.Contains("Logout", razorSource);
    }

    [Fact]
    public void NavigationChecksAuthenticationState()
    {
        var razorSource = ReadRazorSource();

        Assert.Contains("User.Identity", razorSource);
        Assert.Contains("IsAuthenticated", razorSource);
    }

    [Fact]
    public void NavigationOffersRegistrationAndLogin()
    {
        var navigation = ReadSourceFile(
            "src",
            "Bison.Razor",
            "Pages",
            "Shared",
            "_Navigation.cshtml");



        Assert.Contains("asp-page=\"/Account/Register\"", navigation,
        StringComparison.OrdinalIgnoreCase);

        Assert.Contains("asp-page=\"/Account/Login\"", navigation,
        StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NavigationUsesExpectedCssClass()
    {
        var razorSource = ReadRazorSource();

        Assert.Contains("navigation", razorSource);
    }

    private static string ReadApplicationSource()
    {
        var projectDirectory = Path.Combine(
            RepositoryRoot,
            "src",
            "Bison.Razor");

        return string.Join(
            Environment.NewLine,
            Directory
                .EnumerateFiles(
                    projectDirectory,
                    "*.*",
                    SearchOption.AllDirectories)
                .Where(path =>
                    path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText));
    }

    private static string ReadRazorSource()
    {
        var pagesDirectory = Path.Combine(
            RepositoryRoot,
            "src",
            "Bison.Razor",
            "Pages");

        return string.Join(
            Environment.NewLine,
            Directory
                .EnumerateFiles(
                    pagesDirectory,
                    "*.cshtml",
                    SearchOption.AllDirectories)
                .Select(File.ReadAllText));
    }

    private static string ReadSourceFile(params string[] pathParts)
    {
        var path = pathParts.Aggregate(
            RepositoryRoot,
            Path.Combine);

        Assert.True(
            File.Exists(path),
            $"Expected source file was not found: {path}");

        return File.ReadAllText(path);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Bison.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root containing Bison.sln.");
    }
}