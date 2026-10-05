using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("/templates")]
public class TemplatesController : ControllerBase
{
    private readonly TemplateService _tService;
    public TemplatesController(TemplateService tService) => _tService = tService;

    [HttpGet("get-all/minimal")]
    public async Task<IActionResult> GetAllMinimal()
        => Ok(await _tService.GetUsersTemplates(t => t.ToMinimalResponse()));

    [HttpGet("get-all/default")]
    public async Task<IActionResult> GetAll()
        => Ok(await _tService.GetUsersTemplates(t => t.ToResponse()));

    [HttpGet("get-all/detailed")]
    public async Task<IActionResult> GetAllDetailed()
        => Ok(await _tService.GetUsersTemplates(t => t.ToDetailedResponse()));

    [HttpGet("get/minimal")]
    public async Task<IActionResult> GetMinimal([FromQuery] int templateId)
    {
        var result = await _tService.Get(templateId, t => t.ToMinimalResponse());

        return result.IsSuccess ? Ok(result.Value) : this.HandleErrorResult(result);
    }

    [HttpGet("get/default")]
    public async Task<IActionResult> Get([FromQuery] int templateId)
    {
        var result = await _tService.Get(templateId, t => t.ToResponse());

        return result.IsSuccess ? Ok(result.Value) : this.HandleErrorResult(result);
    }

    [HttpGet("get/detailed")]
    public async Task<IActionResult> GetDetailed([FromQuery] int templateId)
    {
        var result = await _tService.Get(templateId, t => t.ToDetailedResponse());

        return result.IsSuccess ? Ok(result.Value) : this.HandleErrorResult(result);
    }

    [HttpPost("post")]
    public async Task<IActionResult> Post([FromBody] TemplateRequest request)
    {
        var result = await _tService.Post(request);
        if (result.IsFailure)
            return this.HandleErrorResult(result);
        
        return Created();
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int templateId)
    {
        var result = await _tService.Delete(templateId);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }

    [HttpPatch("update")]
    public async Task<IActionResult> Update([FromQuery] int templateId, [FromBody] TemplateRequest request)
    {
        var result = await _tService.Update(templateId, request);
        if (result.IsFailure)
            return this.HandleErrorResult(result);

        return NoContent();
    }
}