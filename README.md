## .NET 8.0 API of SmartVision

```
This application runs on ASP.NET version 8.0.122 .Any package to be added must be compatible with project.When adding packages from CLI 'dotnet add <package_name> --version 8.0.0' generally works

```

### Run Options
#### Development profile (Properties/launchProfiles.json)
``` bash
 dotnet run --launch-profile Development


```


#### Any profile (Properties/launchProfiles.json)
``` bash
 dotnet run --launch-profile any

```

### Endoints (local development)
- http(s)://localhost:<PORT>/swagger => Swagger Endpoint


### Local Development Steps
1. Checkout to development branch and 'git pull'.
2. Run or create local database(ex. postgres or postgres as docker container)
3. Change 'appsettings.Development.json' file's database connection string accordingly.
3. Run with default profile 'dotnet run'

### Contributing to tests
1. Install dependencies of the project   
``` bash
dotnet restore 
```
2. Copy appsettings.Development.example.json as appsettings.Development.json and replace it's fields
- Run all Tests
``` bash
dotnet test 
```
- Run a specific class' tests 
```
dotnet test --filter "FullyQualifiedName~<NamespaceName>.<ClassName>"
```

- For example => 'ApiBackend.Tests/IntegrationTests/StoresControllerTests'
```
dotnet test --filter "FullyQualifiedName~ApiBackend.Tests.StoresControllerTests"
```
