using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Application;

public class WorkoutServiceTests
{
    private readonly Mock<IWorkoutsRepository> _wRepo;
    private readonly Mock<IEntityRepository<ExerciseEntity>> _eRepo;
    private readonly Mock<ICurrentUser> _user;
    private readonly WorkoutService _service;
    private readonly ITestOutputHelper _output;
    public WorkoutServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _wRepo = new(); _eRepo = new(); _user = new();
        _service = new (_wRepo.Object, _eRepo.Object, _user.Object);
    }

    [Fact]
    public async Task Get_UnexistentWorkout_ReturnsNotFound()
    {
        var result = await _service.Get(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _wRepo.Verify(r => r.Get(1, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Get_DoesntOwn_ReturnsForbidden()
    {
        _wRepo.Setup(r => r.Get(1, It.IsAny<bool>()))
            .ReturnsAsync(TestData.GetEmptyWorkout(1));
        _user.Setup(u => u.UserId)
            .Returns(2);
        
        var result = await _service.Get(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        _wRepo.Verify(r => r.Get(1, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Get_Valid_ReturnsWorkout()
    {
        _wRepo.Setup(r => r.Get(1, It.IsAny<bool>()))
            .ReturnsAsync(TestData.GetEmptyWorkout(1));
        _user.Setup(u => u.UserId)
            .Returns(1);
        
        var result = await _service.Get(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        _wRepo.Verify(r => r.Get(1, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Post_MissingIds_ReturnsNotFound()
    {
        //Arrange
        int[] ids = new int[] {1, 2, 3, 4};
        var request = TestData.GetWorkoutRequest(ids, true);

        _eRepo.Setup(r => r.Get(ids))
            .ReturnsAsync(TestData.GetExercises().ToArray());
        _user.Setup(u => u.UserId)
            .Returns(1);

        //Act
        var result = await _service.Post(request);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _wRepo.Verify(r => r.Post(It.IsAny<WorkoutEntity>()), Times.Never);
    }

    [Fact]
    public async Task Post_Valid_PostsWorkout()
    {
        //Arrange
        int[] ids = new int[] {1, 2, 3};
        var request = TestData.GetWorkoutRequest(ids, true);

        _eRepo.Setup(r => r.Get(ids))
            .ReturnsAsync(TestData.GetExercises().ToArray());
        _user.Setup(u => u.UserId)
            .Returns(1);

        //Act
        var result = await _service.Post(request);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.None);
        _wRepo.Verify(r => r.Post(It.IsAny<WorkoutEntity>()), Times.Once);
    }

    [Fact]
    public async Task PostCompleted_HasEmptySessionExercises_ReturnsFailure()
    {
        //Arrange
        int[] ids = new int[] {1, 2, 3};
        var request = TestData.GetWorkoutRequest(ids, hasSets: false);

        _eRepo.Setup(r => r.Get(ids))
            .ReturnsAsync(TestData.GetExercises().ToArray());
        _user.Setup(u => u.UserId)
            .Returns(1);

        //Act
        var result = await _service.Post(request);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.InvalidOperation);
        _wRepo.Verify(r => r.Post(It.IsAny<WorkoutEntity>()), Times.Never);
    }

    [Fact]
    public async Task PostCompleted_HasNoSessionExercises_ReturnsFailure()
    {
        //Arrange
        int[] ids = new int[] {1, 2, 3};
        var request = TestData.GetWorkoutRequest(ids, hasSessionExercises: false);

        _eRepo.Setup(r => r.Get(ids))
            .ReturnsAsync(TestData.GetExercises().ToArray());
        _user.Setup(u => u.UserId)
            .Returns(1);

        //Act
        var result = await _service.Post(request);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.InvalidOperation);
        _wRepo.Verify(r => r.Post(It.IsAny<WorkoutEntity>()), Times.Never);
    }

    [Fact]
    public async Task Start_Valid_PostsWorkout()
    {
        var request = TestData.GetDateTimeRequest(DateTimeOffset.Now);
        _user.Setup(u => u.UserId)
            .Returns(1);

        await _service.Start(request);

        _wRepo.Verify(r => r.Post(It.IsAny<WorkoutEntity>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DoesntOwn_ReturnsForbidden()
    {
        _user.Setup(u => u.UserId)
            .Returns(1);
        _wRepo.Setup(r => r.Get(1, false))
            .ReturnsAsync(TestData.GetEmptyWorkout(2));

        var result = await _service.Delete(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        _wRepo.Verify(r => r.Delete(1), Times.Never);
        _wRepo.Verify(r => r.Get(1, false), Times.Once);
    }

    [Fact]
    public async Task Delete_DoesntExist_ReturnsNotFound()
    {        
        var result = await _service.Delete(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _wRepo.Verify(r => r.Delete(1), Times.Never);
        _wRepo.Verify(r => r.Get(1, false), Times.Once);
    }

    [Fact]
    public async Task Delete_Valid_DeletesWorkout()
    {
        _user.Setup(u => u.UserId)
            .Returns(1);
        _wRepo.Setup(r => r.Get(1, It.IsAny<bool>()))
            .ReturnsAsync(TestData.GetEmptyWorkout(1));

        var result = await _service.Delete(1);

        result.IsSuccess.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.None);
        _wRepo.Verify(r => r.Delete(1), Times.Once);
        _wRepo.Verify(r => r.Get(1, false), Times.Once);
    }

    [Fact]
    public async Task AddSessionExercise_ExerciseDoesntExist_ReturnsNotFound()
    {
        var result = await _service.AddSessionExercise(1, 1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _wRepo.Verify(r => r.Get(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        _wRepo.Verify(r => r.Update(It.IsAny<WorkoutEntity>()), Times.Never);
        _eRepo.Verify(r => r.Get(1), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetUpdateTestMethods))]
    public async Task GenericUpdate_DoesntOwn_ReturnsForbidden(Func<WorkoutService, int, Task<Result>> action)
    {
        int workoutId = 1;
        int userId = 1;
        _user.Setup(u => u.UserId)
            .Returns(userId);
        _wRepo.Setup(r => r.Get(workoutId, true))
            .ReturnsAsync(TestData.GetEmptyWorkout(userId + 1));
        _eRepo.Setup(r => r.Get(1))
            .ReturnsAsync(TestData.GetExercises().First());

        var result = await action(_service, workoutId);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        _wRepo.Verify(r => r.Update(It.IsAny<WorkoutEntity>()), Times.Never);
        _wRepo.Verify(r => r.Get(workoutId, true), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetUpdateTestMethods))]
    public async Task GenericUpdate_Valid_UpdatesWorkout(Func<WorkoutService, int, Task<Result>> action)
    {
        int workoutId = 1;
        int userId = 1;
        var workout = TestData.GetFilledWorkout(userId);
        _user.Setup(u => u.UserId)
            .Returns(userId);
        _wRepo.Setup(r => r.Get(workoutId, true))
            .ReturnsAsync(workout);
        _eRepo.Setup(r => r.Get(1))
            .ReturnsAsync(TestData.GetExercises().First());

        var result = await action(_service, workoutId);
        //_output.WriteLine(result.ErrorType.ToString());

        result.IsSuccess.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.None);
        _wRepo.Verify(r => r.Update(It.Is<WorkoutEntity>(w => w.Id == workout.Id)), Times.Once);
        _wRepo.Verify(r => r.Get(workoutId, true), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetUpdateTestMethods))]
    public async Task GenericUpdate_DoesntExist_ReturnsNotFound(Func<WorkoutService, int, Task<Result>> action)
    {
        int workoutId = 1;
        _eRepo.Setup(r => r.Get(1))
            .ReturnsAsync(TestData.GetExercises().First());

        var result = await action(_service, workoutId);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _wRepo.Verify(r => r.Update(It.IsAny<WorkoutEntity>()), Times.Never);
        _wRepo.Verify(r => r.Get(workoutId, true), Times.Once);
    }

    public static IEnumerable<object[]> GetUpdateTestMethods()
    {
        yield return new object[] { (Func<WorkoutService, int, Task<Result>>)((s, wId) => s.AddSessionExercise(wId, 1)) };
        yield return new object[] { (Func<WorkoutService, int, Task<Result>>)((s, wId) => s.RemoveSessionExercise(wId, 1)) };
        yield return new object[] { (Func<WorkoutService, int, Task<Result>>)((s, wId) => s.AddSet(wId, 1, TestData.GetSetRequest(15, 15))) };
        yield return new object[] { (Func<WorkoutService, int, Task<Result>>)((s, wId) => s.UpdateSet(wId, 1, TestData.GetSetRequest(15, 15))) };
        yield return new object[] { (Func<WorkoutService, int, Task<Result>>)((s, wId) => s.DeleteSet(wId, 1)) };
        yield return new object[] { (Func<WorkoutService, int, Task<Result>>)((s, wId) => s.Complete(wId, TestData.GetDateTimeRequest(DateTimeOffset.UtcNow))) };
    }
}