using System.Text.Json;
using Backend.Models;

namespace Backend.Repositories;

public class FileWorkbookRepository : IWorkbookRepository
{
    private readonly string _storagePath;

    public FileWorkbookRepository(IConfiguration config)
    {
        // default storage path inside app directory
        _storagePath = config["Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "storage");
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    private string GetWorkbookFolder(Guid id)
    {
        return Path.Combine(_storagePath, id.ToString());
    }

    private string GetWorkbookJsonPath(Guid id)
    {
        return Path.Combine(GetWorkbookFolder(id), "workbook.json");
    }

    private string GetVersionsJsonPath(Guid id)
    {
        return Path.Combine(GetWorkbookFolder(id), "versions.json");
    }

    private string GetFilesFolder(Guid id)
    {
        return Path.Combine(GetWorkbookFolder(id), "files");
    }

    public async Task<Workbook> CreateWorkbookAsync(string name, string createdBy)
    {
        var wb = new Workbook
        {
            WorkbookId = Guid.NewGuid(),
            Name = name,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow
        };

        var filesDir = GetFilesFolder(wb.WorkbookId);
        Directory.CreateDirectory(filesDir);

        var wbJson = JsonSerializer.Serialize(wb);
        await File.WriteAllTextAsync(GetWorkbookJsonPath(wb.WorkbookId), wbJson);

        var emptyVersions = JsonSerializer.Serialize(new List<WorkbookVersion>());
        await File.WriteAllTextAsync(GetVersionsJsonPath(wb.WorkbookId), emptyVersions);

        return wb;
    }

    public async Task<List<Workbook>> ListWorkbooksAsync()
    {
        var list = new List<Workbook>();
        if (!Directory.Exists(_storagePath))
        {
            return list;
        }

        var folders = Directory.GetDirectories(_storagePath);
        foreach (var folder in folders)
        {
            var jsonFile = Path.Combine(folder, "workbook.json");
            if (!File.Exists(jsonFile))
            {
                continue;
            }

            var text = await File.ReadAllTextAsync(jsonFile);
            var wb = JsonSerializer.Deserialize<Workbook>(text);
            if (wb != null)
            {
                list.Add(wb);
            }
        }

        return list.OrderByDescending(w => w.CreatedDate).ToList();
    }

    public async Task<Workbook?> GetWorkbookAsync(Guid workbookId)
    {
        var path = GetWorkbookJsonPath(workbookId);
        if (!File.Exists(path))
        {
            return null;
        }

        var text = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<Workbook>(text);
    }

    public async Task<Workbook?> UpdateWorkbookAsync(Guid workbookId, string name)
    {
        var wb = await GetWorkbookAsync(workbookId);
        if (wb == null)
        {
            return null;
        }

        wb.Name = name;
        var json = JsonSerializer.Serialize(wb);
        await File.WriteAllTextAsync(GetWorkbookJsonPath(workbookId), json);
        return wb;
    }

    public async Task<WorkbookVersion> SaveVersionAsync(
        Guid workbookId, string workbookName, string createdBy, string? comments, byte[] fileBytes)
    {
        // update workbook name if user renamed it
        var wb = await GetWorkbookAsync(workbookId);
        if (wb != null && !string.IsNullOrWhiteSpace(workbookName) && wb.Name != workbookName)
        {
            wb.Name = workbookName;
            await File.WriteAllTextAsync(GetWorkbookJsonPath(workbookId), JsonSerializer.Serialize(wb));
        }

        var versions = await ListVersionsAsync(workbookId);
        int nextVersion = versions.Count == 0 ? 1 : versions.Max(v => v.VersionNumber) + 1;

        var versionId = Guid.NewGuid();
        var fileName = $"{Sanitize(workbookName)}_V{nextVersion}.xlsx";
        var storageKey = Path.Combine("files", $"{versionId}_{fileName}");

        Directory.CreateDirectory(GetFilesFolder(workbookId));
        var fullFilePath = Path.Combine(GetWorkbookFolder(workbookId), storageKey);
        await File.WriteAllBytesAsync(fullFilePath, fileBytes);

        var newVersion = new WorkbookVersion
        {
            VersionId = versionId,
            WorkbookId = workbookId,
            VersionNumber = nextVersion,
            FileName = fileName,
            FilePathOrStorageKey = storageKey,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
            Comments = comments
        };

        versions.Add(newVersion);
        var json = JsonSerializer.Serialize(versions);
        await File.WriteAllTextAsync(GetVersionsJsonPath(workbookId), json);

        return newVersion;
    }

    public async Task<List<WorkbookVersion>> ListVersionsAsync(Guid workbookId)
    {
        var path = GetVersionsJsonPath(workbookId);
        if (!File.Exists(path))
        {
            return new List<WorkbookVersion>();
        }

        var json = await File.ReadAllTextAsync(path);
        var list = JsonSerializer.Deserialize<List<WorkbookVersion>>(json);
        if (list == null)
        {
            return new List<WorkbookVersion>();
        }

        return list.OrderByDescending(v => v.VersionNumber).ToList();
    }

    public async Task<WorkbookVersion?> GetVersionAsync(Guid workbookId, Guid versionId)
    {
        var versions = await ListVersionsAsync(workbookId);
        return versions.FirstOrDefault(v => v.VersionId == versionId);
    }

    public async Task<byte[]> ReadVersionFileAsync(Guid workbookId, WorkbookVersion version)
    {
        var filePath = Path.Combine(GetWorkbookFolder(workbookId), version.FilePathOrStorageKey);
        return await File.ReadAllBytesAsync(filePath);
    }

    private static string Sanitize(string name)
    {
        var clean = name;
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            clean = clean.Replace(invalidChar, '_');
        }
        return clean.Replace(' ', '_');
    }
}
