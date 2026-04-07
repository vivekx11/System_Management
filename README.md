# College Event Management Portal

A production-ready ASP.NET Core MVC web application for managing college events with role-based authentication, team management, scoring system, and certificate generation.

## Features

### Authentication & Authorization
- ASP.NET Identity with role-based access control
- Roles: Admin, Student, Judge, Team Leader
- Email verification and password reset functionality
- Secure login/logout with remember me option

### Event Management (3 Types)
1. **Debugging Competition**
2. **Code in the Dark**
3. **UI/UX Design Challenge**

Each event supports:
- Solo and team registration
- Custom team size limits
- Registration deadlines
- Multiple rounds
- Real-time status tracking

### Student Features
- Browse and register for events
- Create/join teams with invite codes
- Upload submission files
- View leaderboards
- Download certificates
- Receive notifications
- Track participation history

### Admin Features
- Create, edit, and delete events
- Approve/reject registrations
- Manage users and teams
- Assign judges to events
- Publish results and rounds
- Export reports
- Analytics dashboard with charts
- Send announcements
- Activity logs

### Judge Features
- View assigned events
- Score participants/teams
- Add remarks and feedback
- Round-based evaluation
- Shortlist teams
- Declare winners

### Team Management
- Create teams with unique invite codes
- Join teams using invite codes
- Team leader controls
- Member management
- Team-based scoring

### Certificate System
- Auto-generate PDF certificates
- Dynamic participant names and ranks
- QR code verification
- Certificate download
- Verification portal

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Identity
- **UI**: Bootstrap 5, Font Awesome
- **PDF Generation**: iTextSharp
- **QR Codes**: QRCoder

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full version)
- Visual Studio 2022 or VS Code

## Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd CollegeEventPortal
```

### 2. Update Connection String
Edit `appsettings.json` and update the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CollegeEventPortalDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

For SQL Server:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=CollegeEventPortalDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Create Database Migration
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Run the Application
```bash
dotnet run
```

The application will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

## Default Credentials

After running the application, the following default accounts are created:

### Admin Account
- Email: admin@college.edu
- Password: Admin@123

### Judge Account
- Email: judge@college.edu
- Password: Judge@123

### Student Account
- Email: student@college.edu
- Password: Student@123

**Important**: Change these passwords in production!

## Database Schema

### Main Tables
- **AspNetUsers** - User accounts with Identity
- **Events** - Event information
- **Teams** - Team details with invite codes
- **TeamMembers** - Team membership
- **Registrations** - Event registrations
- **Scores** - Participant/team scores
- **Submissions** - File submissions
- **Certificates** - Generated certificates
- **Notifications** - User notifications
- **ActivityLogs** - System activity tracking
- **EventJudges** - Judge assignments

## Project Structure

```
CollegeEventPortal/
├── Areas/
│   ├── Admin/
│   │   ├── Controllers/
│   │   └── Views/
│   └── Judge/
│       ├── Controllers/
│       └── Views/
├── Controllers/
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── StudentController.cs
│   ├── TeamController.cs
│   └── CertificateController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── Event.cs
│   ├── Team.cs
│   ├── Registration.cs
│   ├── Score.cs
│   ├── Certificate.cs
│   └── ...
├── Services/
│   ├── CertificateService.cs
│   ├── NotificationService.cs
│   └── Interfaces/
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── ...
├── Views/
│   ├── Account/
│   ├── Home/
│   ├── Student/
│   ├── Team/
│   └── Shared/
└── wwwroot/
    ├── css/
    ├── js/
    └── lib/
```

## Deployment

### IIS Deployment

1. Publish the application:
```bash
dotnet publish -c Release -o ./publish
```

2. Create an IIS Application Pool:
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated

3. Create IIS Website:
   - Point to the publish folder
   - Assign the application pool
   - Configure bindings

4. Update `appsettings.json` with production connection string

### Azure Deployment

1. Create Azure SQL Database
2. Create Azure App Service (Windows, .NET 8)
3. Configure connection string in App Service Configuration
4. Deploy using:
   - Visual Studio Publish
   - Azure DevOps
   - GitHub Actions

```bash
# Azure CLI deployment
az webapp up --name your-app-name --resource-group your-rg --runtime "DOTNETCORE:8.0"
```

## Configuration

### Email Settings (for production)
Add to `appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "noreply@college.edu",
  "SenderName": "College Events",
  "Username": "your-email",
  "Password": "your-password"
}
```

### File Upload Settings
```json
"AppSettings": {
  "MaxFileUploadSize": 10485760,
  "AllowedFileExtensions": ".pdf,.doc,.docx,.zip,.rar,.jpg,.png",
  "UploadPath": "uploads"
}
```

## Security Best Practices

1. **Change default passwords** immediately
2. **Enable HTTPS** in production
3. **Use strong connection strings** with encrypted passwords
4. **Enable CORS** only for trusted domains
5. **Implement rate limiting** for API endpoints
6. **Regular security updates** for NuGet packages
7. **Enable logging** and monitoring
8. **Backup database** regularly

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string format
- Ensure database exists
- Check firewall settings

### Migration Errors
```bash
# Reset migrations
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Package Restore Issues
```bash
dotnet clean
dotnet restore --force
dotnet build
```

## License

This project is licensed under the MIT License.

## Support

For issues and questions:
- Create an issue in the repository
- Contact: support@college.edu

## Contributors

- Development Team
- College IT Department

---

**Version**: 1.0.0  
**Last Updated**: 7 Of April 2026
