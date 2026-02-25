using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using ApiBackend.DTOs.LoginDtos;
using ApiBackend.Services.Interfaces;
using ApiBackend.Data;

namespace ApiBackend.Tests;

public abstract class IntegrationTest : IClassFixture<TestFactory>, IAsyncLifetime
{
    protected TestFactory Factory { get; init; }
    protected HttpClient HttpClient { get; init; }
    protected IServiceProvider Services => Factory.Services;

    protected IntegrationTest(TestFactory factory)
    {
        HttpClient = factory.CreateClient();
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        await MigrateAndSeedTestDatabase();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }


    protected LoginRequestDto GetTestSupervisorUser()
    {

        var supervisorUser = new LoginRequestDto();
        supervisorUser.Email = "supervisor@company.com";
        supervisorUser.Password = "123456";
        return supervisorUser;
    }

    protected LoginRequestDto GetTestAdminUser()
    {
        var adminUser = new LoginRequestDto();
        adminUser.Email = "admin@company.com";
        adminUser.Password = "123456";

        return adminUser;
    }

    protected LoginRequestDto GetTestFieldWorkerUser()
    {
        var fieldWorkerUser = new LoginRequestDto();
        fieldWorkerUser.Email = "ahmet@company.com";
        fieldWorkerUser.Password = "123456";

        return fieldWorkerUser;
    }

    protected async Task<LoginResponseDto> AuthenticateUser(LoginRequestDto user)
    {
        if (user == null)
            throw new Exception("user parameter(LoginRequestDto) returned null!");

        using var scope = Services.CreateScope();
        if (scope == null)
            throw new Exception("Services.CreateScope() returned null!");

        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        if (authService == null)
            throw new Exception("IAuthService(context) returned null!");

        var userResponse = await authService.LoginAsync(user);
        if (userResponse == null)
            throw new Exception("LoginAsync returned null!");

        if (string.IsNullOrEmpty(userResponse.Token))
            throw new Exception("Token is null or empty!");

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userResponse.Token);

        return userResponse;
    }

    private async Task MigrateAndSeedTestDatabase()
    {
        using var scope = Services.CreateScope();
        if (scope == null)
            throw new Exception("Services.CreateScope() returned null!");

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (appDbContext == null)
            throw new Exception("appDbContext returned null!");

        await appDbContext.Database.MigrateAsync();
        await DbInitializer.SeedDevData(appDbContext);
    }

}
