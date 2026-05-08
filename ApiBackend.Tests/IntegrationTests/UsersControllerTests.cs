using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ApiBackend.DTOs.UserDtos;
using ApiBackend.DTOs.LoginDtos;


namespace ApiBackend.Tests;

public class UsersControllerTests : IntegrationTest
{
    public UsersControllerTests(TestFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMyProfile_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("api/Users/profile");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_ShouldReturnOk_WhenAuthenticated()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        HttpResponseMessage response = await HttpClient.GetAsync("api/Users/profile");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyStats_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("api/Users/me/stats");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyStats_ShouldReturnOk_WhenAuthenticated()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        HttpResponseMessage response = await HttpClient.GetAsync("api/Users/me/stats");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        int pageValue = 1;
        int sizeValue = 10;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        int pageValue = 1;
        int sizeValue = 10;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnBadRequest_WithInvalidPaginationParameters()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        int pageValue = int.MinValue;
        int sizeValue = int.MinValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WithValidPaginationParameters()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        int pageValue = 1;
        int sizeValue = 10;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Valid since test database is seeded at this point
        int userId = 2;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        // Valid since test database is seeded at this point
        int userId = 2;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnBadRequest_WithStoreIdOutOfBounds()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        int userId = int.MinValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WithStoreIdInBoundsButNotExists()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        int userId = int.MaxValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnOk_WithValidStoreId()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        int userId = 2;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Users/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFieldWorkers_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("api/Users/field-workers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFieldWorkers_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        HttpResponseMessage response = await HttpClient.GetAsync("api/Users/field-workers");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetFieldWorkers_ShouldReturnOk_WhenAuthorized()
    {
        var supervisorUser = GetTestAdminUser();
        await AuthenticateUser(supervisorUser);

        HttpResponseMessage response = await HttpClient.GetAsync("api/Users/field-workers");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostUser_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var request = new CreateUserDto();
        request.FullName = "Ceren Demir";
        request.Email = "ceren@company.com";
        request.Password = "123456";
        request.Role = "SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998877";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostUser_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateUserDto();
        request.FullName = "Ceren Demir";
        request.Email = "ceren@company.com";
        request.Password = "123456";
        request.Role = "SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998877";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostUser_ShouldReturnBadRequest_WithEmptyUserRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateUserDto();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostUser_ShouldReturnBadRequest_WithInvalidUserRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateUserDto();
        request.FullName = "";
        request.Email = "";
        request.Password = "";
        request.Role = "111!~~!SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998877";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task PostUser_ShouldReturnCreated_WithValidUserRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateUserDto();
        request.FullName = "Ceren Demir";
        request.Email = "ceren@company.com";
        request.Password = "123456";
        request.Role = "SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998877";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }


    [Fact]
    public async Task PutUserById_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var request = new CreateUserDto();
        request.FullName = "Ceren Deren Demir";
        request.Email = "ceren.deren@company.com";
        request.Password = "12345678";
        request.Role = "SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998899";

        // Valid since test database is seeded at this point
        int userId = 5;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Users/{userId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutUserById_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateUserDto();
        request.FullName = "Ceren Deren Demir";
        request.Email = "ceren.deren@company.com";
        request.Password = "12345678";
        request.Role = "SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998899";

        // Valid since test database is seeded at this point
        int userId = 5;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Users/{userId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task PutUserById_ShouldReturnBadRequest_WithUserIdOutOfBounds()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateUserDto();
        request.FullName = "Ceren Deren Demir";
        request.Email = "ceren.deren@company.com";
        request.Password = "12345678";
        request.Role = "SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998899";

        int userId = int.MinValue; // impossible userId => negative value
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Users/{userId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutUserById_ShouldReturnBadRequest_WithInvalidUserRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateUserDto();
        request.FullName = "";
        request.Email = "";
        request.Password = "";
        request.Role = "!!!!@@~~1SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998899";

        // Valid since test database is seeded at this point
        int userId = 5;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Users/{userId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutUserById_ShouldReturnNoContent_WithValidUserRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateUserDto();
        request.FullName = "Ceren Deren Demir";
        request.Email = "ceren.deren@company.com";
        request.Password = "12345678";
        request.Role = "SUPERVISOR";
        request.EmployeeId = "SUP-2025-012";
        request.Phone = "+905559998899";

        // Valid since test database is seeded at this point
        int userId = 5;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Users/{userId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // [Fact]
    // public async Task PatchUserById_ShouldReturnUnauthorized_WhenNotAuthenticated()
    // {
    //     // Valid since test database is seeded at this point
    //     int userId = 6;
    //     HttpResponseMessage response = await HttpClient.PatchAsync($"api/Users/{userId}");
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    // }
    //
    // [Fact]
    // public async Task PatchUserById_ShouldReturnForbidden_WhenNotAuthorized()
    // {
    //     var supervisorUser = GetTestSupervisorUser();
    //     await AuthenticateUser(supervisorUser);
    //
    //     // Valid since test database is seeded at this point
    //     int userId = 6;
    //     HttpResponseMessage response = await HttpClient.PatchAsync($"api/Users/{userId}");
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    // }
    //
    //
    // [Fact]
    // public async Task PatchUserById_ShouldReturnBadRequest_WithUserIdOutOfBounds()
    // {
    //     var adminUser = GetTestAdminUser();
    //     await AuthenticateUser(adminUser);
    //
    //     int userId = int.MinValue;
    //     HttpResponseMessage response = await HttpClient.PatchAsync($"api/Users/{userId}");
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    // }
    //
    // [Fact]
    // public async Task PatchUserById_ShouldReturnNotFound_WithUserIdInBoundsButNotExists()
    // {
    //     var adminUser = GetTestAdminUser();
    //     await AuthenticateUser(adminUser);
    //
    //     int userId = int.MaxValue;
    //     HttpResponseMessage response = await HttpClient.PatchAsync($"api/Users/{userId}");
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    // }
    //
    // [Fact]
    // public async Task PatchUserById_ShouldReturnNoContent_WithValidUserId()
    // {
    //     var adminUser = GetTestAdminUser();
    //     await AuthenticateUser(adminUser);
    //
    //     // Valid since test database is seeded at this point
    //     int userId = 6;
    //     HttpResponseMessage response = await HttpClient.PatchAsync($"api/Users/{userId}");
    //
    //     response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    // }


    [Fact]
    public async Task ResetPassword_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var request = new AdminResetPasswordDto();
        request.NewPassword = "PASS123456";

        // Valid since test database is seeded at this point
        int userId = 5;
        HttpResponseMessage response = await HttpClient.PatchAsJsonAsync($"api/Users/{userId}/reset-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new AdminResetPasswordDto();
        request.NewPassword = "PASS123456";

        // Valid since test database is seeded at this point
        int userId = 5;
        HttpResponseMessage response = await HttpClient.PatchAsJsonAsync($"api/Users/{userId}/reset-password", request);


        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WithUserIdOutOfBounds()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new AdminResetPasswordDto();
        request.NewPassword = "PASS123456";

        int userId = int.MinValue;
        HttpResponseMessage response = await HttpClient.PatchAsJsonAsync($"api/Users/{userId}/reset-password", request);


        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnNotFound_WithUserIdInBoundsButNotExists()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new AdminResetPasswordDto();
        request.NewPassword = "PASS123456";

        int userId = int.MaxValue;
        HttpResponseMessage response = await HttpClient.PatchAsJsonAsync($"api/Users/{userId}/reset-password", request);


        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnOk_WithValidUserId()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new AdminResetPasswordDto();
        request.NewPassword = "PASS123456";

        // Valid since test database is seeded at this point
        int userId = 5;
        HttpResponseMessage response = await HttpClient.PatchAsJsonAsync($"api/Users/{userId}/reset-password", request);


        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

