using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("/workouts")]
public class WorkoutsController : ControllerBase
{
    private readonly WorkoutService _wService;
    public WorkoutsController(WorkoutService wService) => _wService = wService;

    [HttpGet("get-all/detailed")]
    public async Task<IActionResult> GetAllDetailed([FromQuery] int amount = 100, [FromQuery] int page = 0)
        => Ok(await _wService.GetAll(amount, page, (w) => w.ToDetailedDTO()));

    [HttpGet("get-all/summary")]
    public async Task<IActionResult> GetAllSummary([FromQuery] int amount = 100, [FromQuery] int page = 0)
        => Ok(await _wService.GetAll(amount, page, (w) => w.ToSummaryDTO()));        

    [HttpGet("get-all-filtered/detailed")]
    public async Task<IActionResult> GetAllFilteredDetailed([FromQuery] WorkoutSearchFilter filter)
        => Ok(await _wService.GetAllFiltered(filter, (w) => w.ToDetailedDTO()));

    [HttpGet("get-all-filtered/summary")]
    public async Task<IActionResult> GetAllFilteredSummary([FromQuery] WorkoutSearchFilter filter)
        => Ok(await _wService.GetAllFiltered(filter, (w) => w.ToSummaryDTO()));

    [HttpGet("get/detailed")]
    public async Task<IActionResult> GetDetailed([FromQuery] int workoutId)
    {
        var result = await _wService.Get(workoutId, (w) => w.ToDetailedDTO());
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return Ok(result.Value);
    }

    [HttpGet("get/summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int workoutId)
    {
        var result = await _wService.Get(workoutId, (w) => w.ToSummaryDTO());
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return Ok(result.Value);
    }

    [HttpPost("post")]
    public async Task<IActionResult> Post([FromBody] WorkoutRequest request)
    {
        var result = await _wService.Post(request);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return Created();
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] DateTimeRequest time)
    {
        await _wService.Start(time);
        return Created();
    }

    [HttpPatch("complete")]
    public async Task<IActionResult> Complete([FromQuery] int workoutId, [FromBody] DateTimeRequest time)
    {
        var result = await _wService.Complete(workoutId, time);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int workoutId)
    {
        var result = await _wService.Delete(workoutId);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }

    [HttpPatch("session-exercises/add")]
    public async Task<IActionResult> AddSessionExercise([FromQuery] int workoutId, [FromQuery] int exerciseId)
    {
        var result = await _wService.AddSessionExercise(workoutId, exerciseId);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }

    [HttpPatch("session-exercises/delete")]
    public async Task<IActionResult> RemoveSessionExercise([FromQuery] int workoutId, [FromQuery] int sessionId)
    {
        var result = await _wService.RemoveSessionExercise(workoutId, sessionId);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }

    [HttpPatch("sets/add")]
    public async Task<IActionResult> AddSet([FromQuery] int workoutId, [FromQuery] int sessionId, [FromBody] SetRequest request)
    {
        var result = await _wService.AddSet(workoutId, sessionId, request);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }

    [HttpPatch("sets/update")]
    public async Task<IActionResult> UpdateSet([FromQuery] int workoutId, [FromQuery] int setId, [FromBody] SetRequest request)
    {
        var result = await _wService.UpdateSet(workoutId, setId, request);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }
    [HttpPatch("sets/delete")]
    public async Task<IActionResult> DeleteSet([FromQuery] int workoutId, [FromQuery] int setId)
    {
        var result = await _wService.DeleteSet(workoutId, setId);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }
}