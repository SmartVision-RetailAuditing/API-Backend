using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ApiBackend.DTOs.StoreDtos;


namespace ApiBackend.Tests;

public class StoresControllerTests : IntegrationTest
{
    public StoresControllerTests(TestFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetStores_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        int pageValue = 1;
        int sizeValue = 10;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Stores?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task GetStores_ShouldReturnBadRequest_WithInvalidPaginationParameters()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        int pageValue = int.MinValue;
        int sizeValue = int.MinValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Stores?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStores_ShouldReturnOk_WithValidPaginationParameters()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        int pageValue = 1;
        int sizeValue = 10;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Stores?page={pageValue}&size={sizeValue}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Fact]
    public async Task GetStoresById_ShouldReturnBadRequest_WithStoreIdOutOfBounds()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        int storeId = int.MinValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Stores/{storeId}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStoresById_ShouldReturnNotFound_WithStoreIdInBoundsButNotExists()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        int storeId = int.MaxValue;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Stores/{storeId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStoresById_ShouldReturnOk_WithValidStoreId()
    {
        var fieldWorkerUser = GetTestFieldWorkerUserA();
        await AuthenticateUser(fieldWorkerUser);

        int storeId = 1;
        HttpResponseMessage response = await HttpClient.GetAsync($"api/Stores/{storeId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostStore_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var request = new CreateStoreDto();
        request.Name = "Migros MM Kucukcekmece";
        request.ChainName = "Migros";
        request.Region = "Anadolu Yakasi";
        request.Address = "Yildiz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Stores", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostStore_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateStoreDto();
        request.Name = "Migros MM Kucukcekmece";
        request.ChainName = "Migros";
        request.Region = "Anadolu Yakasi";
        request.Address = "Yildiz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Stores", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostStore_ShouldReturnBadRequest_WithEmptyStoreRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateStoreDto();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Stores", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostStore_ShouldReturnBadRequest_WithInvalidStoreRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateStoreDto();
        request.Name = "";
        request.ChainName = "";
        request.Region = "Anadolu Yakasi";
        request.Address = "Yildiz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Stores", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task PostStore_ShouldReturnCreated_WithValidStoreRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateStoreDto();
        request.Name = "Migros MM Kucukcekmece";
        request.ChainName = "Migros";
        request.Region = "Anadolu Yakasi";
        request.Address = "Yildiz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/Stores", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }


    [Fact]
    public async Task PutStoreById_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var request = new CreateStoreDto();
        request.Name = "Migros MM Kucukcekmece";
        request.ChainName = "Migros";
        request.Region = "Avrupa Yakasi";
        request.Address = "Deniz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        // Valid since test database is seeded at this point
        int storeId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Stores/{storeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutStoreById_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        var request = new CreateStoreDto();
        request.Name = "Migros MM Kucukcekmece";
        request.ChainName = "Migros";
        request.Region = "Avrupa Yakasi";
        request.Address = "Deniz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        // Valid since test database is seeded at this point
        int storeId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Stores/{storeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task PutStoreById_ShouldReturnBadRequest_WithStoreIdOutOfBounds()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateStoreDto();
        request.Name = "Migros MM Kucukcekmece";
        request.ChainName = "Migros";
        request.Region = "Avrupa Yakasi";
        request.Address = "Deniz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        int storeId = int.MinValue; // impossible storeId => negative value
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Stores/{storeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStoreById_ShouldReturnBadRequest_WithEmptyStoreRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateStoreDto();

        // Valid since test database is seeded at this point
        int storeId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Stores/{storeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStoreById_ShouldReturnBadRequest_WithInvalidStoreRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateStoreDto();
        request.Name = "";
        request.ChainName = "";
        request.Region = "Avrupa Yakasi";
        request.Address = "Deniz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        // Valid since test database is seeded at this point
        int storeId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Stores/{storeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStoreById_ShouldReturnNoContent_WithValidStoreRequest()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        var request = new CreateStoreDto();
        request.Name = "Migros MM Kucukcekmece";
        request.ChainName = "Migros";
        request.Region = "Avrupa Yakasi";
        request.Address = "Deniz Mah, Kucukcekmece";
        request.Latitude = 12.034;
        request.Longitude = 56.078;

        // Valid since test database is seeded at this point
        int storeId = 1;
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync($"api/Stores/{storeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteStoreById_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Valid since test database is seeded at this point
        int storeId = 2;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Stores/{storeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteStoreById_ShouldReturnForbidden_WhenNotAuthorized()
    {
        var supervisorUser = GetTestSupervisorUser();
        await AuthenticateUser(supervisorUser);

        // Valid since test database is seeded at this point
        int storeId = 2;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Stores/{storeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task DeleteStoreById_ShouldReturnBadRequest_WithStoreIdOutOfBounds()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        int storeId = int.MinValue;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Stores/{storeId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteStoreById_ShouldReturnNotFound_WithStoreIdInBoundsButNotExists()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        int storeId = int.MaxValue;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Stores/{storeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteStoreById_ShouldReturnNoContent_WithValidStoreId()
    {
        var adminUser = GetTestAdminUser();
        await AuthenticateUser(adminUser);

        // Valid since test database is seeded at this point
        int storeId = 2;
        HttpResponseMessage response = await HttpClient.DeleteAsync($"api/Stores/{storeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

