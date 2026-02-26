using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ApiBackend.DTOs.LoginDtos;


namespace ApiBackend.Tests;

public class AuthControllerTests : IntegrationTest
{
    public AuthControllerTests(TestFactory factory)
        : base(factory)
    {
    }


    [Fact]
    public async Task Login_ShouldReturnBadRequest_WithEmptyLoginRequest()
    {
        var request = new LoginRequestDto();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WithInvalidLoginRequest()
    {
        var request = new LoginRequestDto();
        request.Email = "";
        request.Password = "";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WithFalseLoginCredentials()
    {
        var request = new LoginRequestDto();
        request.Email = "admin@company.com";
        request.Password = "this-is-not-the-correct-password1234";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Login_ShouldReturnOk_WithValidLoginRequest()
    {
        var request = new LoginRequestDto();
        request.Email = "admin@company.com";
        request.Password = "123456";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var request = new ChangePasswordDto();
        request.OldPassword = "123456";
        request.NewPassword = "PASS123456";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WithEmptyChangePasswordRequest()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserC();
        await AuthenticateUser(fieldWorkerUser);

        var request = new ChangePasswordDto();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WithInvalidChangePasswordRequest()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserC();
        await AuthenticateUser(fieldWorkerUser);

        var request = new ChangePasswordDto();
        request.OldPassword = "";
        request.NewPassword = "";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task ChangePassword_ShouldReturnOk_WithValidChangePasswordRequest()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserC();
        await AuthenticateUser(fieldWorkerUser);

        var request = new ChangePasswordDto();
        request.OldPassword = "123456";
        request.NewPassword = "PASS123456";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Auth/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

}

