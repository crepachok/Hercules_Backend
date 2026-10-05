using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Application;

public class TemplateServiceTests
{
    private  readonly ITestOutputHelper _output;
    private  readonly Mock<ITemplatesRepository> _tRepo;
    private  readonly Mock<IEntityRepository<ExerciseEntity>> _eRepo;
    private  readonly Mock<ICurrentUser> _user;
    private  readonly TemplateService _service;
    public TemplateServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _tRepo = new(); _eRepo = new(); _user = new();
        _service = new (_tRepo.Object, _eRepo.Object, _user.Object);
    }

    [Fact]
    public async Task Post_MissingIds_DoesntPost()
    {
        int exercisesCount = 3;
        var request = TestData.GetTemplateRequest(exercisesCount + 2);
        _eRepo.Setup(r => r.Get(request.ExerciseIds, true))
            .ReturnsAsync(TestData.GetExercises(exercisesCount).ToArray());

        var result = await _service.Post(request);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _tRepo.Verify(r => r.Post(It.IsAny<TemplateEntity>()), Times.Never);
    }

    [Fact]
    public async Task Post_ValidRequest_Posts()
    {
        var request = TestData.GetTemplateRequest();
        _eRepo.Setup(r => r.Get(request.ExerciseIds, true))
            .ReturnsAsync(TestData.GetExercises().ToArray());
        _user.Setup(u => u.UserId)
            .Returns(1);

        var result = await _service.Post(request);
        //_output.WriteLine(result.ErrorType.ToString());

        result.IsSuccess.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.None);
        _tRepo.Verify(r => r.Post(It.IsAny<TemplateEntity>()), Times.Once);
        _user.Verify(u => u.UserId, Times.Once);
    }

    [Fact]
    public async Task Delete_UnexistentTemplate_ReturnsNotFound()
    {   
        var result = await _service.Delete(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _tRepo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        _tRepo.Verify(r => r.Get(1), Times.Once);
    }

    [Fact]
    public async Task Delete_DoesntOwnTemplate_ReturnsForbidden()
    {
        _user.Setup(u => u.UserId)
            .Returns(1);
        _tRepo.Setup(r => r.Get(1, false))
            .ReturnsAsync(TestData.GetTemplate(2));

        var result = await _service.Delete(1);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        _tRepo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        _tRepo.Verify(r => r.Get(1), Times.Once);
    }

    [Fact]
    public async Task Delete_Valid_DeletesTemplate()
    {
        int templateId = 1;
        int userId = 1;
        _user.Setup(u => u.UserId)
            .Returns(userId);
        _tRepo.Setup(r => r.Get(templateId, false))
            .ReturnsAsync(TestData.GetTemplate(userId));

        var result = await _service.Delete(templateId);

        result.IsSuccess.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.None);
        _tRepo.Verify(r => r.Delete(templateId), Times.Once);
        _tRepo.Verify(r => r.Get(1), Times.Once);
    }

    [Fact]
    public async Task Update_DoesntOwnTemplate_ReturnsForbidden()
    {
        var request = TestData.GetTemplateRequest();
        _user.Setup(u => u.UserId)
            .Returns(1);
        _tRepo.Setup(r => r.Get(1, true))
            .ReturnsAsync(TestData.GetTemplate(2));

        var result = await _service.Update(1, request);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        _tRepo.Verify(r => r.Update(It.IsAny<TemplateEntity>()), Times.Never);
        _tRepo.Verify(r => r.Get(1, true), Times.Once);
    }

    [Fact]
    public async Task Update_UnexistentTemplate_ReturnsNotFound()
    {
        var request = TestData.GetTemplateRequest();

        var result = await _service.Update(1, request);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _tRepo.Verify(r => r.Update(It.IsAny<TemplateEntity>()), Times.Never);
        _tRepo.Verify(r => r.Get(1, true), Times.Once);
    }

    [Fact]
    public async Task Update_MissingIds_DoesntUpdate()
    {
        int userId = 1;
        var request = TestData.GetTemplateRequest(5);
        _tRepo.Setup(r => r.Get(1, true))
            .ReturnsAsync(TestData.GetTemplate(userId));
        _user.Setup(u => u.UserId)
            .Returns(userId);
        _eRepo.Setup(r => r.Get(request.ExerciseIds, true))
            .ReturnsAsync(TestData.GetExercises(3).ToArray());

        var result = await _service.Update(1, request);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Contain("4")
            .And.Contain("5");
        _tRepo.Verify(r => r.Update(It.IsAny<TemplateEntity>()), Times.Never);
        _tRepo.Verify(r => r.Get(1, true), Times.Once);
    }

    [Fact]
    public async Task Update_Valid_UpdatesTemplate()
    {
        var request = TestData.GetTemplateRequest();
        _tRepo.Setup(r => r.Get(1, true))
            .ReturnsAsync(TestData.GetTemplate(1));
        _user.Setup(u => u.UserId)
            .Returns(1);
        _eRepo.Setup(r => r.Get(request.ExerciseIds, true))
            .ReturnsAsync(TestData.GetExercises().ToArray());

        var result = await _service.Update(1, request);
        //_output.WriteLine(result.ErrorType.ToString());

        result.IsSuccess.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.None);
        _tRepo.Verify(r => r.Update(It.Is<TemplateEntity>(e => e.Name == request.Name)), Times.Once);
        _tRepo.Verify(r => r.Get(1, true), Times.Once);
    }
}