using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Integration;
public class UserGuestIntegrationTests : IClassFixture<IntegrationTestsFixture>, IAsyncLifetime
{
    private IntegrationTestsFixture _fixture;
    private HttpClient _http;
    private HerculesContext _context;
    private ITestOutputHelper _output;
    public UserGuestIntegrationTests(IntegrationTestsFixture fixture, ITestOutputHelper output)
        => (_fixture, _output, _http, _context) = (fixture, output, fixture.Client, fixture.Context);

    [Fact]
    public async Task Register_ThenLogin_ReturnsToken()
    {
        //Arrange
        var cred = TestData.GetCredentials();
        var content = JsonContent.Create(cred);
        
        //Act
        var regResponce = await _http.PostAsync("/users/register", content);
        regResponce.StatusCode.Should().Be(HttpStatusCode.Created);

        var userCount = await _context.Users.CountAsync();
        userCount.Should().BeGreaterThanOrEqualTo(1);

        var loginResponce = await _http.PostAsync("/users/login", content);
        loginResponce.StatusCode.Should().Be(HttpStatusCode.OK);

        string token = await loginResponce.Content.ReadAsStringAsync();

        //Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().MatchRegex(@"^[A-Za-z0-9\-\._]+\.[A-Za-z0-9\-\._]+\.[A-Za-z0-9\-\._]+$");
        
        string[] parts = token.Split(".");
        parts.Length.Should().Be(3);
        Convert.FromBase64String(parts[0]);
    }

    [Fact]
    public async Task Register_Valid_CreatesUserInDb()
    {
        //Arrange
        var cred = TestData.GetCredentials();
        var content = JsonContent.Create(cred);

        //Act
        var regResponce = await _http.PostAsync("/users/register", content);
        regResponce.StatusCode.Should().Be(HttpStatusCode.Created);

        var userCount = await _context.Users.CountAsync();
        userCount.Should().BeGreaterThanOrEqualTo(1);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == cred.Username);

        //Assert
        user.Should().NotBeNull();
        user.Username.Should().Be(cred.Username);
    }
    public Task InitializeAsync() { return Task.CompletedTask; }
    public async Task DisposeAsync()
    {
        await _fixture.RespawnDb();
    }
}