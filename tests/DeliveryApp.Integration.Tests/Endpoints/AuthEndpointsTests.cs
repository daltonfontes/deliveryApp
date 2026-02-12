using System.Net;
using System.Net.Http.Json;
using DeliveryApp.Application.DTOs.Auth;
using DeliveryApp.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace DeliveryApp.Integration.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsOk_WithValidData()
    {
        var request = new RegisterRequest($"auth-test-{Guid.NewGuid():N}@test.com", "StrongPass1@", "Test User", "Customer");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrEmpty();
        body.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task Register_Returns400_WithDuplicateEmail()
    {
        var email = $"dup-{Guid.NewGuid():N}@test.com";
        var request = new RegisterRequest(email, "StrongPass1@", "Test", "Customer");

        await _client.PostAsJsonAsync("/api/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Returns400_WithWeakPassword()
    {
        var request = new RegisterRequest($"weak-{Guid.NewGuid():N}@test.com", "123", "Test", "Customer");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithValidCredentials()
    {
        var email = $"login-{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "StrongPass1@", "Test", "Admin"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "StrongPass1@"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_Returns401_WithInvalidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nonexistent@test.com", "Wrong1@"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
