using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ApiBackend.DTOs.TaskDtos;
using ApiBackend.DTOs.LoginDtos;


namespace ApiBackend.Tests;

public class TasksControllerTests : IntegrationTest
{
    public TasksControllerTests(TestFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMyTasks_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("api/Tasks/my-tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyTasks_ShouldReturnBadRequest_WithInvalidPaginationParameters()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        int pageValue = int.MinValue;
        int sizeValue = int.MinValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Tasks/my-tasks?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMyTasks_ShouldReturnOk_WhenAuthenticated()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        HttpResponseMessage response = await HttpClient.GetAsync("api/Tasks/my-tasks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTasks_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("api/Tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task GetTasks_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        HttpResponseMessage response = await HttpClient.GetAsync("api/Tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTasks_ShouldReturnBadRequest_WithInvalidPaginationParameters()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        int pageValue = int.MinValue;
        int sizeValue = int.MinValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Tasks?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTasks_ShouldReturnOk_WhenAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        HttpResponseMessage response = await HttpClient.GetAsync("api/Tasks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTasksById_ShouldReturnForbidden_WithInvalidTaskId()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserB();
        LoginResponseDto user = await AuthenticateUser(fieldWorkerUser);

        // Invalid since according to seeded test database taskId=1 DOES NOT belongs to this fieldWorkerUser
        int taskId = 1;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Tasks/{taskId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTasksById_ShouldReturnBadRequest_WithTaskIdOutOfBounds()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        LoginResponseDto user = await AuthenticateUser(fieldWorkerUser);

        int taskId = int.MinValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Tasks/{taskId}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTasksById_ShouldReturnNotFound_WithTaskIdInBoundsButNotExists()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        LoginResponseDto user = await AuthenticateUser(fieldWorkerUser);

        int taskId = int.MaxValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Tasks/{taskId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTasksById_ShouldReturnOk_WithValidTaskId()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        LoginResponseDto user = await AuthenticateUser(fieldWorkerUser);

        // Valid since according to seeded test database taskId=1 belongs to this fieldWorkerUser
        int taskId = 1;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Tasks/{taskId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostTask_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        var request = new CreateTaskDto();
        var dueDate = DateTime.UtcNow.AddHours(2);
        request.StoreId = 1;
        request.UserId = 1;
        request.TaskType = Entities.TaskType.PRICE_CHECK;
        request.Priority = Entities.TaskPriority.MEDIUM;
        request.DueDate = dueDate;

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostTask_ShouldReturnBadRequest_WithEmptyTaskRequest()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateTaskDto();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTask_ShouldReturnBadRequest_WithInvalidTaskRequest()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateTaskDto();
        var dueDate = DateTime.UtcNow.AddHours(2);
        request.StoreId = int.MinValue;
        request.UserId = int.MinValue;
        request.TaskType = Entities.TaskType.PRICE_CHECK;
        request.Priority = Entities.TaskPriority.MEDIUM;
        request.DueDate = dueDate;

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task PostTask_ShouldReturnCreated_WithValidTaskRequest()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateTaskDto();
        var dueDate = DateTime.UtcNow.AddHours(2);
        request.StoreId = 1;
        request.UserId = 1;
        request.TaskType = Entities.TaskType.PRICE_CHECK;
        request.Priority = Entities.TaskPriority.MEDIUM;
        request.DueDate = dueDate;

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }


    [Fact]
    public async Task PutTaskById_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var request = new CreateTaskDto();
        var dueDate = DateTime.UtcNow.AddHours(7);
        request.StoreId = 1;
        request.UserId = 1;
        request.TaskType = Entities.TaskType.PRICE_CHECK;
        request.Priority = Entities.TaskPriority.HIGH;
        request.DueDate = dueDate;

        // Valid since test database is seeded at this point
        int taskId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Tasks/{taskId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task PutTaskById_ShouldReturnBadRequest_WithTaskIdOutOfBounds()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateTaskDto();
        var dueDate = DateTime.UtcNow.AddHours(7);
        request.StoreId = 1;
        request.UserId = 1;
        request.TaskType = Entities.TaskType.PRICE_CHECK;
        request.Priority = Entities.TaskPriority.HIGH;
        request.DueDate = dueDate;

        int taskId = int.MinValue; // impossible taskId => negative value
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Tasks/{taskId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutTaskById_ShouldReturnBadRequest_WithEmptyTaskRequest()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateTaskDto();

        // Valid since test database is seeded at this point
        int taskId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Tasks/{taskId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutTaskById_ShouldReturnBadRequest_WithInvalidTaskRequest()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateTaskDto();
        var dueDate = DateTime.UtcNow.AddHours(2);
        request.StoreId = 1;
        request.UserId = int.MinValue;
        request.TaskType = Entities.TaskType.PRICE_CHECK;
        request.Priority = Entities.TaskPriority.MEDIUM;
        request.DueDate = dueDate;

        // Valid since test database is seeded at this point
        int taskId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Tasks/{taskId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutTaskById_ShouldReturnNoContent_WithValidTaskRequest()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateTaskDto();
        var dueDate = DateTime.UtcNow.AddHours(7);
        request.StoreId = 1;
        request.UserId = 1;
        request.TaskType = Entities.TaskType.PRICE_CHECK;
        request.Priority = Entities.TaskPriority.HIGH;
        request.DueDate = dueDate;

        // Valid since test database is seeded at this point
        int taskId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Tasks/{taskId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTaskById_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        // Valid since test database is seeded at this point
        int taskId = 2;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTaskById_ShouldReturnNoContent_WithValidTaskId()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        // Valid since test database is seeded at this point
        int taskId = 2;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTaskById_ShouldReturnBadRequest_WithTaskIdOutOfBounds()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        int taskId = int.MinValue;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteTaskById_ShouldReturnNotFound_WithTaskIdInBoundsButNotExists()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        int taskId = int.MaxValue;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

