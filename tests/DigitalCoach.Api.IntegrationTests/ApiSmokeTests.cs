using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DigitalCoach.Api.Common;
using Xunit;

namespace DigitalCoach.Api.IntegrationTests;

public sealed class ApiSmokeTests(DigitalCoachApiFactory factory) : IClassFixture<DigitalCoachApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Unauthorized_Request_Returns_Standard_401()
    {
        var response = await _client.GetAsync("/api/tasks");
        var body = await ReadApiResponseAsync<object>(response);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(body.Succeeded);
        Assert.Equal("Authentication is required or the token is invalid.", body.Message);
    }

    [Fact]
    public async Task Register_Login_And_Authenticated_Endpoint_Work()
    {
        var email = $"integration.{Guid.NewGuid():N}@example.com";

        var registerResponse = await RegisterAsync(email);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerBody = await ReadApiResponseAsync<AuthTestResponse>(registerResponse);
        Assert.True(registerBody.Succeeded);
        Assert.NotNull(registerBody.Data);
        Assert.False(string.IsNullOrWhiteSpace(registerBody.Data.AccessToken));

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "StrongPass123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginBody = await ReadApiResponseAsync<AuthTestResponse>(loginResponse);
        Assert.True(loginBody.Succeeded);
        Assert.NotNull(loginBody.Data);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Data.AccessToken);
        var meResponse = await _client.GetAsync("/api/auth/me");
        var meBody = await ReadApiResponseAsync<object>(meResponse);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.True(meBody.Succeeded);
    }

    [Fact]
    public async Task Validation_Failure_Returns_Standard_ApiResponse()
    {
        var email = $"validation.{Guid.NewGuid():N}@example.com";
        var registerResponse = await RegisterAsync(email);
        var registerBody = await ReadApiResponseAsync<AuthTestResponse>(registerResponse);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerBody.Data!.AccessToken);

        var response = await _client.GetAsync("/api/tasks?page=0&pageSize=20");
        var body = await ReadApiResponseAsync<object>(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(body.Succeeded);
        Assert.Equal("Validation failed.", body.Message);
        Assert.NotNull(body.Errors);
        Assert.Contains("page", body.Errors.Keys);
    }

    [Fact]
    public async Task Health_Endpoint_Returns_200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpResponseMessage> RegisterAsync(string email)
    {
        return await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "StrongPass123!",
            birthDate = "1995-01-01",
            height = 170.5m,
            weight = 68.2m,
            gender = "female"
        });
    }

    private static async Task<ApiResponse<T>> ReadApiResponseAsync<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        var body = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(body);
        return body;
    }

    private sealed record AuthTestResponse(int UserId, string Email, string AccessToken, DateTime ExpiresAt);
}
