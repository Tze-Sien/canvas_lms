# CanvasLMS

A learning management system built with ASP.NET Core.

## Prerequisites

Before you begin, ensure you have the following installed:

1. **.NET 7.0 SDK or later**
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

2. **Git Bash (Windows users)**
   - Download from: https://git-scm.com/download/win
   - Required for running the setup script on Windows

3. **SQL Server**
   - You can choose how to set up the SQL Server:
     - **Option 1:** Use Docker Compose (Recommended for quick setup)
     - **Option 2:** Use an existing local or remote MSSQL Server

## Database Setup Options

### Option 1: Using Docker Compose

1. Start the SQL Server container:
```bash
docker-compose up -d
```

2. Set up the database with the setup script:
```bash
./setup-database.sh localhost sa SU2orange! 
```

### Option 2: Using Local/Remote MSSQL Server

1. Ensure your MSSQL Server is running and accessible.
2. Set up the database with the setup script, replacing `<db_host>`, `<db_user>`, and `<db_password>` with your server's credentials:
```bash
./setup-database.sh <db_host> <db_user> <db_password> <db_port>
```
Example:
```bash
./setup-database.sh 192.168.1.100 sa SU2orange! 1433
```

## Starting the Application with Seed data
Note: For window user. Please run with GitBash
```
./setup-database.sh <db_host> <db_user> <db_password>
```

## Default Development Credentials

- Host: `localhost` (or your specified host)
- User: `sa`
- Password: `SU2orange!`

## Running the Application

After database setup is complete, the application will automatically start at:
http://localhost:5285

## Available Accounts

1. **Admin Account**
   - Email: admin@gmail.com
   - Password: admin

2. **Lecturer Account**
   - Email: lecturer@gmail.com
   - Password: lecturer

3. **Student Account**
   - Email: student@gmail.com
   - Password: student

## Notes for Windows Users

- Install Git Bash from https://git-scm.com/download/win
- Run all commands in Git Bash terminal
- Use `./setup-database.sh` as shown (avoid backslashes)

## Troubleshooting

1. **Script Permission Issues**
```bash
chmod +x setup-database.sh
```

2. **Database Connection Issues**
- Verify Docker is running (if using Docker Compose)
- Check SQL Server container status:
```bash
docker ps
docker logs canvas-lms-db
```

- If using a local MSSQL Server, verify network access and credentials

