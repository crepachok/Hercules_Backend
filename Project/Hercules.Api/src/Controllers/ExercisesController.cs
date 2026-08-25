using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("/exercises")]
public class ExercisesController : ControllerBase
{
    private readonly ExerciseService _service;
    public ExercisesController(ExerciseService service) => _service = service;

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAll();
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return Ok(result.Value);
    }

    [HttpGet("get-muscle-groups")]
    public async Task<IActionResult> GetAllMuscleGroups()
    {
        var result = await _service.GetAllMuscleGroups();
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return Ok(result.Value);
    }

    [HttpGet("get-filtered")]
    public async Task<IActionResult> GetFiltered([FromQuery] ExerciseSearchFilter filter)
    {
        var result = await _service.GetFiltered(filter);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return Ok(result.Value);
    }

    [HttpGet("get-by-id")]
    public async Task<IActionResult> Get([FromQuery] int exerciseId)
    {
        var result = await _service.Get(exerciseId);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return Ok(result.Value);
    }
}