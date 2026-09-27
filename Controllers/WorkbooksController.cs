using Backend.Models.Dtos;
using Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/workbooks")]
public class WorkbooksController : ControllerBase
{
    private readonly IWorkbookRepository _repo;

    public WorkbooksController(IWorkbookRepository repo)
    {
        _repo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkbook([FromBody] CreateWorkbookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Workbook name is required.");
        }

        var result = await _repo.CreateWorkbookAsync(request.Name, request.CreatedBy);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> ListWorkbooks()
    {
        var workbooks = await _repo.ListWorkbooksAsync();
        return Ok(workbooks);
    }

    [HttpGet("{workbookId:guid}")]
    public async Task<IActionResult> GetWorkbook(Guid workbookId)
    {
        var workbook = await _repo.GetWorkbookAsync(workbookId);
        if (workbook == null)
        {
            return NotFound($"Workbook {workbookId} not found.");
        }

        return Ok(workbook);
    }

    [HttpPut("{workbookId:guid}")]
    public async Task<IActionResult> UpdateWorkbook(Guid workbookId, [FromBody] UpdateWorkbookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Workbook name cannot be empty.");
        }

        var updated = await _repo.UpdateWorkbookAsync(workbookId, request.Name.Trim());
        if (updated == null)
        {
            return NotFound($"Workbook {workbookId} not found.");
        }

        return Ok(updated);
    }

    [HttpPost("{workbookId:guid}/versions")]
    public async Task<IActionResult> SaveVersion(Guid workbookId, [FromBody] SaveVersionRequest request)
    {
        var workbook = await _repo.GetWorkbookAsync(workbookId);
        if (workbook == null)
        {
            return NotFound($"Workbook {workbookId} not found.");
        }

        var fileBytes = Convert.FromBase64String(request.FileBase64);
        var version = await _repo.SaveVersionAsync(
            workbookId,
            request.WorkbookName,
            request.CreatedBy,
            request.Comments,
            fileBytes
        );

        var response = new SaveVersionResponse(
            version.VersionId,
            version.VersionNumber,
            version.FileName,
            version.CreatedDate
        );

        return Ok(response);
    }

    [HttpGet("{workbookId:guid}/versions")]
    public async Task<IActionResult> ListVersions(Guid workbookId)
    {
        var list = await _repo.ListVersionsAsync(workbookId);
        return Ok(list);
    }

    [HttpGet("{workbookId:guid}/versions/{versionId:guid}")]
    public async Task<IActionResult> GetVersion(Guid workbookId, Guid versionId)
    {
        var version = await _repo.GetVersionAsync(workbookId, versionId);
        if (version == null)
        {
            return NotFound($"Version {versionId} not found.");
        }

        return Ok(version);
    }

    [HttpGet("{workbookId:guid}/versions/{versionId:guid}/file")]
    public async Task<IActionResult> GetVersionFile(Guid workbookId, Guid versionId)
    {
        var version = await _repo.GetVersionAsync(workbookId, versionId);
        if (version == null)
        {
            return NotFound();
        }

        var bytes = await _repo.ReadVersionFileAsync(workbookId, version);
        var base64 = Convert.ToBase64String(bytes);

        return Ok(new VersionFileResponse(base64, version.FileName));
    }

    [HttpGet("{workbookId:guid}/versions/{versionId:guid}/download")]
    public async Task<IActionResult> DownloadVersion(Guid workbookId, Guid versionId)
    {
        var version = await _repo.GetVersionAsync(workbookId, versionId);
        if (version == null)
        {
            return NotFound();
        }

        var bytes = await _repo.ReadVersionFileAsync(workbookId, version);
        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(bytes, contentType, version.FileName);
    }
}
