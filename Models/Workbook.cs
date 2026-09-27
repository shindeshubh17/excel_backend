namespace Backend.Models;

public class Workbook
{
    public Guid WorkbookId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class WorkbookVersion
{
    public Guid VersionId { get; set; }
    public Guid WorkbookId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePathOrStorageKey { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? Comments { get; set; }
}
