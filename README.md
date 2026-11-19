## Configure GitHub authentication

GitHub authentication requires a client ID and client secret from the GitHub OAuth application used by the project.

Ask the repository owner or project maintainer how the development credentials should be shared. Do not send the client secret through GitHub issues, commits, pull requests, or other public channels.

Once the credentials have been provided securely, store them locally with .NET User Secrets:

```powershell
dotnet user-secrets set "Authentication:GitHub:ClientId" "<client-id>" --project src/Bison.Razor/Bison.Razor.csproj
dotnet user-secrets set "Authentication:GitHub:ClientSecret" "<client-secret>" --project src/Bison.Razor/Bison.Razor.csproj