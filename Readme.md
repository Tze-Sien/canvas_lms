# CanvasLMS Development Setup Guide

This document explains how to set up the development environment for CanvasLMS. It covers:

- **VS Code Setup:** Opening and working on the project in Visual Studio Code.
- **Database Setup (Docker Compose):** Creating and managing the development database.
- **EF Core Migrations:** Adding, updating, and rolling back migrations.
- **Starting Your Project:** Running the project with live reloading and troubleshooting restart issues.

---

## Prerequisites

Make sure you have the following installed:

- [Visual Studio Code](https://code.visualstudio.com/)
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Docker Compose enabled)
- (Optional) [Docker Extension for VS Code](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker)
- (Optional) [Git](https://git-scm.com/)

---

## 1. VS Code Setup

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/Tze-Sien/canvas_lms.git
   cd CanvasLMS
   ```

2. **Open the Project:**

   - Launch VS Code.
   - Open the folder via **File > Open Folder…**.

3. **Install Recommended Extensions:**

   - C# Dev Kit
   - Docker


---

## 2. Database Setup with Docker Compose

The project includes a `docker-compose.yml` file at the root to manage the database container.

### Build and Start Containers:

```bash
docker-compose up -d
```

This command builds (if needed) and starts the defined services.

### Verify Containers:

```bash
docker-compose ps
```

### Stop Containers (if needed):

```bash
docker-compose down
```

---

## 3. Managing EF Core Migrations

Keep your database schema in sync with your application model by using EF Core migrations.

### Add a Migration:

```bash
dotnet ef migrations add <MigrationName>
```

Replace `<MigrationName>` with a descriptive name (e.g., `InitialCreate`).

### Apply the Migration:

```bash
dotnet ef database update
```

This command applies all pending migrations to update the database schema.

### Reverting a Migration:

To roll back to a specific migration:

```bash
dotnet ef database update <TargetMigration>
```

The `<TargetMigration>` is the name of the migration to which you wish to revert.

> **Reference:** For further details on EF Core migrations, refer to [Microsoft’s EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/).

---

## 4. Running Your Project

To run CanvasLMS and enable live reloading during development:

### Start the Application with Watch Mode:

```bash
dotnet watch run
```

This command automatically restarts your application when code changes are detected.

### Manual Restart:

If live reloading fails, press `Ctrl+C` to stop the process and re-run the command above.

### Access the Application:

Open your browser and navigate to `http://localhost:5000` (or the port specified by your project).

---

## Troubleshooting

### Docker Issues:

- Use `docker-compose logs` to inspect container logs.
- Ensure Docker Desktop is running properly.

### EF Core Migrations:

- Verify the connection string in your `DbContext` configuration.
- Ensure the `Microsoft.EntityFrameworkCore.Tools` package is installed.

### VS Code Issues:

- Reload VS Code or reopen the folder if extensions behave unexpectedly.
- Check the integrated terminal for error messages.

---

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [VS Code Dev Containers Guide](https://code.visualstudio.com/docs/devcontainers/create-dev-container)

