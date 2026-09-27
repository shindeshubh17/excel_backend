namespace Backend.Models.Dtos;

public record CreateWorkbookRequest(string Name, string CreatedBy);

public record SaveVersionRequest(
    string WorkbookName,
    string CreatedBy,
    string? Comments,
    string FileBase64
);

public record SaveVersionResponse(
    Guid VersionId,
    int VersionNumber,
    string FileName,
    DateTime CreatedDate
);

public record VersionFileResponse(string FileBase64, string FileName);

public record UpdateWorkbookRequest(string Name);
