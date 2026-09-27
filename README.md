# Backend Web API (.NET 10)

This is the backend REST API service for the spreadsheet application, built with **ASP.NET Core (.NET 10)**.

It provides endpoints to manage workbooks, save immutable version snapshots with comments, and stream/download Excel files.

---

## Overview & Design

1. **Routing:** Uses ASP.NET Core controller attribute routing (`[Route("api/[controller]")]`) with standard HTTP verbs (`GET`, `POST`, `PUT`).
2. **Persistence:** Uses a local file-based repository (`FileWorkbookRepository`) in the `storage/` directory. Each workbook gets its own folder containing `versions.json` (metadata index) and the actual `.xlsx` files (`1.xlsx`, `2.xlsx`, etc.). This avoids external database dependencies so anyone can clone and run it directly.
3. **CORS:** Configured in `Program.cs` to allow requests from the Angular development server (`http://localhost:4200`).
4. **Versioning:** Every save increments the version number (`V1`, `V2`, etc.) and stores an immutable snapshot. Older versions can be reloaded at any time without overwriting the previous history.

---

## Project Structure

```text
excel_backend/
├── Controllers/
│   └── WorkbooksController.cs        # Endpoints for workbooks and versions
├── Models/
│   ├── Workbook.cs                   # Workbook and Version domain entities
│   └── Dtos.cs                       # Request and response DTOs
├── Repositories/
│   ├── IWorkbookRepository.cs        # Repository interface
│   └── FileWorkbookRepository.cs     # File-system storage implementation
├── storage/                          # Folder where workbooks are saved
│   └── .gitkeep
├── Properties/
│   └── launchSettings.json           # Local dev port configurations (5000/5001)
├── appsettings.json                  # Application settings
├── excel_backend.csproj              # .NET 10 project file
└── Program.cs                        # App entry point, DI, and CORS setup
```

---

## Storage Layout

Workbooks and their versions are saved on disk as follows:

```text
storage/
└── {workbookId}/
    ├── versions.json      # JSON log containing version list, timestamps, comments
    ├── 1.xlsx             # Version 1 binary file
    ├── 2.xlsx             # Version 2 binary file
    └── ...
```

---

## API Endpoints

### 1. `GET /api/workbooks`
Returns the list of all workbooks.

**Response:**
```json
[
  {
    "workbookId": "d3b07384-d113-4940-b6f7-111111111111",
    "name": "Student Grades",
    "createdBy": "demo-user",
    "createdDate": "2026-09-27T10:00:00Z"
  }
]
```

### 2. `POST /api/workbooks`
Creates a new workbook entry.

**Request:**
```json
{
  "name": "Sales Report",
  "createdBy": "demo-user"
}
```

**Response (201 Created):**
```json
{
  "workbookId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "Sales Report",
  "createdBy": "demo-user",
  "createdDate": "2026-09-27T10:00:00Z"
}
```

### 3. `PUT /api/workbooks/{id}`
Renames an existing workbook.

**Request:**
```json
{
  "name": "Sales Report Q3"
}
```

### 4. `GET /api/workbooks/{workbookId}/versions`
Returns the version history of a workbook, ordered by newest first.

**Response:**
```json
[
  {
    "versionId": "e2f3a4b5-c6d7-8901-2345-6789abcdef01",
    "workbookId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "versionNumber": 2,
    "savedDate": "2026-09-27T11:00:00Z",
    "savedBy": "demo-user",
    "fileName": "Sales Report Q3_v2.xlsx",
    "fileSize": 18450,
    "comments": "Added August data"
  },
  {
    "versionId": "c1d2e3f4-a5b6-7890-1234-56789abcdef0",
    "workbookId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "versionNumber": 1,
    "savedDate": "2026-09-27T10:00:00Z",
    "savedBy": "demo-user",
    "fileName": "Sales Report Q3_v1.xlsx",
    "fileSize": 15200,
    "comments": "Initial draft"
  }
]
```

### 5. `POST /api/workbooks/{workbookId}/versions`
Saves a new version with a Base64-encoded `.xlsx` file and optional comments.

**Request:**
```json
{
  "workbookId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "workbookName": "Sales Report Q3",
  "createdBy": "demo-user",
  "comments": "Added August data",
  "fileBase64": "UEsDBBQAAAAIA..."
}
```

**Response (200 OK):**
```json
{
  "versionId": "e2f3a4b5-c6d7-8901-2345-6789abcdef01",
  "versionNumber": 2,
  "fileName": "Sales Report Q3_v2.xlsx",
  "savedDate": "2026-09-27T11:00:00Z"
}
```

### 6. `GET /api/workbooks/{workbookId}/versions/{versionId}/file`
Fetches the Base64 file string to load the sheet into the browser.

### 7. `GET /api/workbooks/{workbookId}/versions/{versionId}/download`
Directly downloads the `.xlsx` binary file with attachment header.

---

## How to Run

1. Make sure .NET 10 SDK is installed:
   ```bash
   dotnet --version
   ```

2. Build:
   ```bash
   dotnet build
   ```

3. Run:
   ```bash
   dotnet run
   ```
   The API will listen at `http://localhost:5000`.
