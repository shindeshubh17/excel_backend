using Backend.Models;

namespace Backend.Repositories;

public interface IWorkbookRepository
{
    Task<Workbook> CreateWorkbookAsync(string name, string createdBy);
    Task<List<Workbook>> ListWorkbooksAsync();
    Task<Workbook?> GetWorkbookAsync(Guid workbookId);
    Task<Workbook?> UpdateWorkbookAsync(Guid workbookId, string name);

    Task<WorkbookVersion> SaveVersionAsync(
        Guid workbookId, string workbookName, string createdBy, string? comments, byte[] fileBytes);

    Task<List<WorkbookVersion>> ListVersionsAsync(Guid workbookId);
    Task<WorkbookVersion?> GetVersionAsync(Guid workbookId, Guid versionId);
    Task<byte[]> ReadVersionFileAsync(Guid workbookId, WorkbookVersion version);
}
