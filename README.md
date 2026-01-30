# Project Setup and Run Instructions

This document describes how to set up and run the project locally.

---

## Prerequisites

Before starting, make sure the following software is installed on your computer:

- Visual Studio 2022 (version 17.x or later)
- ASP.NET Core MVC and Web Development workload
- .NET SDK 8.0
- Microsoft SQL Server (LocalDB or full version)
- Git

---

## Clone the Repository

Open **Command Prompt** or **Git Bash** and navigate to the directory where you want to store the project.

Run the following command:

```bash
git clone <repository_url>
```

After the cloning process is complete, navigate to the project directory.

---

## Open the Project in Visual Studio

1. Launch Visual Studio
2. Select **Open a project or solution**
3. Open the solution file with the `.sln` extension
4. Wait until all dependencies are fully restored

---

## Configure the Database Connection

1. Open the `appsettings.json` file
2. Verify or update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DelivIQDb;Trusted_Connection=True;"
  }
}
```

3. Ensure that SQL Server is running and accessible on your local machine

---

## Apply Database Migrations

1. In Visual Studio, open  
   **Tools → NuGet Package Manager → Package Manager Console**
2. Select the project that contains the `DbContext`
3. Run the following command:

```powershell
Update-Database
```

After the command completes, the database will be created automatically.

---

## Run the Project

1. In the Visual Studio top toolbar, select **IIS Express** or **https** as the startup profile
2. Click **Run** or press **F5**
3. After startup, the web application will open automatically in your browser
