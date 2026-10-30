# GitHub OAuth configuration

Bison uses GitHub OAuth to authenticate users. GitHub OAuth credentials must be supplied through secure configuration and must not be committed to the repository.

## Required configuration

The application requires the following configuration values:

- `GitHub:ClientId`
- `GitHub:ClientSecret`

The GitHub OAuth callback path is:

```text
/signin-github
```

## Create a GitHub OAuth application

Create an OAuth application through the GitHub developer settings.

For local development, configure the application with values similar to:

```text
Homepage URL:
https://localhost:<port>

Authorization callback URL:
https://localhost:<port>/signin-github
```

Replace `<port>` with the HTTPS port used by the Bison Razor application.

A separate GitHub OAuth application should be used for the deployed application because GitHub OAuth applications support a specific configured callback URL.

For the deployed application, use the public Azure App Service URL:

```text
Homepage URL:
https://<app-name>.azurewebsites.net

Authorization callback URL:
https://<app-name>.azurewebsites.net/signin-github
```

## Configure local development secrets

Initialize .NET user secrets for the Razor project if they have not already been initialized:

```powershell
dotnet user-secrets init --project src/Bison.Razor
```

Store the OAuth client ID:

```powershell
dotnet user-secrets set "GitHub:ClientId" "<client-id>" --project src/Bison.Razor
```

Store the OAuth client secret:

```powershell
dotnet user-secrets set "GitHub:ClientSecret" "<client-secret>" --project src/Bison.Razor
```

Verify that the required keys exist:

```powershell
dotnet user-secrets list --project src/Bison.Razor
```

Do not copy the resulting values into `appsettings.json`, source files, documentation, commits, screenshots, or issue descriptions.

## Configure Azure App Service

In the Azure App Service configuration, add the following application settings:

```text
GitHub__ClientId
GitHub__ClientSecret
```

ASP.NET Core converts double underscores in environment-variable names to configuration separators. Therefore, `GitHub__ClientId` supplies `GitHub:ClientId`.

Save the settings and restart the App Service after changing the configuration.

The production credentials must belong to the GitHub OAuth application configured with the deployed application's callback URL.

## Run the application locally

Start the Razor application:

```powershell
dotnet run --project src/Bison.Razor
```

Open the HTTPS address shown in the terminal and select **Login**. GitHub should request authorization and redirect back to:

```text
/signin-github
```

After successful authentication, Bison should display the authenticated GitHub username in the navigation.

## Security requirements

- Never commit a GitHub OAuth client secret.
- Never place real credentials in `appsettings.json`.
- Never place real credentials in `appsettings.Development.json`.
- Use .NET user secrets for local development.
- Use Azure App Service application settings for deployment.
- Use separate OAuth credentials for local and deployed environments.
- Revoke and replace a client secret immediately if it is exposed.