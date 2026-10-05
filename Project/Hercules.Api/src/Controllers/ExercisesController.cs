using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("/exercises")]
public class ExercisesController : ControllerBase
{
    private readonly ExerciseService _service;
    public ExercisesController(ExerciseService service) => _service = service;

    [HttpGet("get-all/minimal")]
    public async Task<IActionResult> GetAllMinimal([FromQuery] ExerciseSearchFilter filter)
        => Ok(await _service.GetAll(e => e.ToMinimalResponse(), filter));

    [HttpGet("get-all/default")]
    public async Task<IActionResult> GetAll([FromQuery] ExerciseSearchFilter filter)
        => Ok(await _service.GetAll(e => e.ToResponse(), filter));

    [HttpGet("get-all/detailed")]
    public async Task<IActionResult> GetAllDetailed([FromQuery] ExerciseSearchFilter filter)
        => Ok(await _service.GetAll(e => e.ToDetailedResponse(), filter));

    [HttpGet("get-muscle-groups/minimal")]
    public async Task<IActionResult> GetAllMuscleGroupsMinimal()
        => Ok(await _service.GetAllMuscleGroups(m => m.ToMinimalResponse()));

    [HttpGet("get-muscle-groups/default")]
    public async Task<IActionResult> GetAllMuscleGroups()
        => Ok(await _service.GetAllMuscleGroups(m => m.ToResponse()));

    [HttpGet("get-muscle-groups/detailed")]
    public async Task<IActionResult> GetAllMuscleGroupsDetailed()
        => Ok(await _service.GetAllMuscleGroups(m => m.ToDetailedResponse()));

    [HttpGet("get/minimal")]
    public async Task<IActionResult> GetMinimal([FromQuery] int exerciseId)
    {
        var result = await _service.Get(exerciseId, e => e.ToMinimalResponse());

        return result != null ? Ok(result) : NotFound($"No exercise with id: {exerciseId}");
    }

    [HttpGet("get/default")]
    public async Task<IActionResult> Get([FromQuery] int exerciseId)
    {
        var result = await _service.Get(exerciseId, e => e.ToResponse());

        return result != null ? Ok(result) : NotFound($"No exercise with id: {exerciseId}");
    }

    [HttpGet("get/detailed")]
    public async Task<IActionResult> GetDetailed([FromQuery] int exerciseId)
    {
        var result = await _service.Get(exerciseId, e => e.ToDetailedResponse());

        return result != null ? Ok(result) : NotFound($"No exercise with id: {exerciseId}");
    }
}