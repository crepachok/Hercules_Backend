using Xunit.Abstractions;
using Xunit;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace Integration;
public class WorkoutIntegrationTests : IClassFixture<IntegrationTestsFixture>
{
    private IntegrationTestsFixture _fixture = null!;
    private HttpClient _http = null!;
    private HerculesContext _context = null!;
    private ITestOutputHelper _output = null!;
    public WorkoutIntegrationTests(IntegrationTestsFixture fixture, ITestOutputHelper output)
        => (_fixture, _http, _context, _output) = (fixture, fixture.Client, fixture.Context, output);

    [Fact]
    public async Task UserAddsWorkout_AddsToDatabase()
    {
        //Arrange
        var cred = TestData.GetCredentials();
        var credBody = JsonContent.Create(cred);

        var regResponce = await _http.PostAsync("/users/register", credBody);
        regResponce.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponce = await _http.PostAsync("/users/login", credBody);
        loginResponce.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await loginResponce.Content.ReadAsStringAsync();

        //Act
        var time = TestData.GetDateTimeRequest();
        var startBody = JsonContent.Create(time);

        var startRequest = new HttpRequestMessage(HttpMethod.Post, "/workouts/start");
        startRequest.Content = startBody;
        startRequest.Headers.Authorization = new ("Bearer", token);

        var startResponce = await _http.SendAsync(startRequest);

        //Assert
        startResponce.StatusCode.Should().Be(HttpStatusCode.Created);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/workouts/get-all");
        getRequest.Headers.Authorization = new ("Bearer", token);

        var getResponce = await _http.SendAsync(getRequest);
        getResponce.StatusCode.Should().Be(HttpStatusCode.OK);

        _output.WriteLine(await getResponce.Content.ReadAsStringAsync());
        WorkoutSummaryResponse[] workouts = JsonSerializer.Deserialize<WorkoutSummaryResponse[]>(
            await getResponce.Content.ReadAsStringAsync()
        )!;

        workouts.Should().NotBeNull();
        workouts.Length.Should().Be(1);
    }
}