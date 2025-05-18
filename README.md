# blazor-server-template

## Get Started

```sh
dotnet new install AlefCarlos.Blazor.Templates
```

## Build

To install using local directory:

```sh
dotnet new install .\
```

## Templates

###  mudblazor-server-solution

#### Using

```sh
dotnet new mudblazor-server-solution -n AppName -o ./app
```

This templates add common features to the Basic Blazor template. It ensures a production-ready application

#### features

- [x] Auth using OIDC
- [x] NotFound capability
- [x] Localization
- [x] Sync with browsertime
- [x] Global error catcher
- [x] User Session Management
- [x] Aspire Integration
    - [x] Keycloak 

#### Authentication

> This template offers a [keycloak](http://localhost:8080) resource using Aspire and a realm import as well

Start creating an user on `local` realm to [log the application in](https://localhost:7212)

#### User Session Management

##### Session Store

[Duende Blazor Server Access Token Management](https://docs.duendesoftware.com/accesstokenmanagement/blazor-server/) adds User Session management for Blazor safely.

> There is a in-memory store(ServerSideTokenStore.cs) for tokens, but you should user a persisted one.

##### Propagate token into http requests

Add the handler to the httpclient using [Http Client Factory](https://docs.duendesoftware.com/accesstokenmanagement/workers/#http-client-factory) and the token will be automaticaly injected, if the token is expired, it refreshes it befor sending the request

```csharp
builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("https://apis.company.com/catalog/");
}).AddUserAccessTokenHandler(); //<-- this one
```

##### Back-Channel Logout

This template implements RemoteSignOut following the [RFC](https://openid.net/specs/openid-connect-backchannel-1_0.html)

