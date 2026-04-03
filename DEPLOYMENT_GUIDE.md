# Deployment Guide - College Event Management Portal

## Prerequisites

- .NET 8.0 Runtime
- SQL Server 2019 or later
- IIS 10 or later (for Windows deployment)
- SSL Certificate (for HTTPS)

## Step-by-Step Deployment

### 1. Prepare the Application

#### Update Configuration
Edit `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_PRODUCTION_SERVER;Database=CollegeEventPortalDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Publish the Application
```bash
dotnet publish -c Release -o ./publish
```

### 2. Database Setup

#### Create Production Database
```sql
CREATE DATABASE CollegeEventPortalDb;
GO

USE CollegeEventPortalDb;
GO

-- Create a dedicated user
CREATE LOGIN CollegeEventUser WITH PASSWORD = 'YourStrongPassword123!';
CREATE USER CollegeEventUser FOR LOGIN CollegeEventUser;
ALTER ROLE db_owner ADD MEMBER CollegeEventUser;
GO
```

#### Run Migrations
```bash
cd publish
dotnet ef database update --connection "YOUR_CONNECTION_STRING"
```

### 3. IIS Deployment

#### Install Prerequisites
1. Install .NET 8.0 Hosting Bundle from Microsoft
2. Install URL Rewrite Module
3. Restart IIS: `iisreset`

#### Create Application Pool
```powershell
# PowerShell commands
Import-Module WebAdministration

New-WebAppPool -Name "CollegeEventPortal" -Force
Set-ItemProperty IIS:\AppPools\CollegeEventPortal -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\CollegeEventPortal -Name managedPipelineMode -Value "Integrated"
Set-ItemProperty IIS:\AppPools\CollegeEventPortal -Name startMode -Value "AlwaysRunning"
```

#### Create Website
```powershell
New-Website -Name "CollegeEventPortal" `
    -PhysicalPath "C:\inetpub\wwwroot\CollegeEventPortal" `
    -ApplicationPool "CollegeEventPortal" `
    -Port 80 `
    -HostHeader "events.college.edu"
```

#### Configure HTTPS
1. Obtain SSL certificate
2. Bind certificate to website:
```powershell
New-WebBinding -Name "CollegeEventPortal" -Protocol https -Port 443 -HostHeader "events.college.edu"
```

#### Set Permissions
```powershell
$path = "C:\inetpub\wwwroot\CollegeEventPortal"
$acl = Get-Acl $path
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS","FullControl","ContainerInherit,ObjectInherit","None","Allow")
$acl.SetAccessRule($rule)
Set-Acl $path $acl
```

### 4. Azure App Service Deployment

#### Create Resources
```bash
# Login to Azure
az login

# Create resource group
az group create --name CollegeEventPortal-RG --location eastus

# Create SQL Server
az sql server create \
    --name collegeeventdb \
    --resource-group CollegeEventPortal-RG \
    --location eastus \
    --admin-user sqladmin \
    --admin-password YourStrongPassword123!

# Create SQL Database
az sql db create \
    --resource-group CollegeEventPortal-RG \
    --server collegeeventdb \
    --name CollegeEventPortalDb \
    --service-objective S0

# Create App Service Plan
az appservice plan create \
    --name CollegeEventPortal-Plan \
    --resource-group CollegeEventPortal-RG \
    --sku B1 \
    --is-linux false

# Create Web App
az webapp create \
    --name collegeeventportal \
    --resource-group CollegeEventPortal-RG \
    --plan CollegeEventPortal-Plan \
    --runtime "DOTNET:8.0"
```

#### Configure Connection String
```bash
az webapp config connection-string set \
    --name collegeeventportal \
    --resource-group CollegeEventPortal-RG \
    --connection-string-type SQLAzure \
    --settings DefaultConnection="Server=tcp:collegeeventdb.database.windows.net,1433;Database=CollegeEventPortalDb;User ID=sqladmin;Password=YourStrongPassword123!;Encrypt=True;TrustServerCertificate=False;"
```

#### Deploy Application
```bash
# Using ZIP deployment
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip \
    --resource-group CollegeEventPortal-RG \
    --name collegeeventportal \
    --src ../app.zip
```

### 5. Post-Deployment Steps

#### Verify Database
```sql
-- Check tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Verify seed data
SELECT * FROM AspNetUsers;
SELECT * FROM AspNetRoles;
```

#### Test Application
1. Navigate to your domain
2. Test login with default credentials
3. Create a test event
4. Register a test student
5. Verify email notifications (if configured)

#### Change Default Passwords
```sql
-- This should be done through the application UI
-- Login as admin and change password immediately
```

### 6. Configure Monitoring

#### Application Insights (Azure)
```bash
az monitor app-insights component create \
    --app collegeeventportal-insights \
    --location eastus \
    --resource-group CollegeEventPortal-RG \
    --application-type web

# Get instrumentation key
az monitor app-insights component show \
    --app collegeeventportal-insights \
    --resource-group CollegeEventPortal-RG \
    --query instrumentationKey
```

Add to `appsettings.Production.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "YOUR_KEY_HERE"
  }
}
```

### 7. Backup Strategy

#### Automated SQL Backup
```sql
-- Create maintenance plan for daily backups
BACKUP DATABASE CollegeEventPortalDb
TO DISK = 'C:\Backups\CollegeEventPortalDb.bak'
WITH FORMAT, INIT, COMPRESSION;
```

#### Azure SQL Backup
```bash
# Configure long-term retention
az sql db ltr-policy set \
    --resource-group CollegeEventPortal-RG \
    --server collegeeventdb \
    --database CollegeEventPortalDb \
    --weekly-retention P4W \
    --monthly-retention P12M \
    --yearly-retention P5Y \
    --week-of-year 1
```

### 8. Performance Optimization

#### Enable Response Caching
Add to `Program.cs`:
```csharp
builder.Services.AddResponseCaching();
app.UseResponseCaching();
```

#### Enable Response Compression
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
app.UseResponseCompression();
```

#### Database Indexing
```sql
-- Add indexes for frequently queried columns
CREATE INDEX IX_Events_Status ON Events(Status);
CREATE INDEX IX_Events_StartDate ON Events(StartDate);
CREATE INDEX IX_Registrations_UserId ON Registrations(UserId);
CREATE INDEX IX_Registrations_EventId ON Registrations(EventId);
```

### 9. Security Hardening

#### Configure HTTPS Redirection
```csharp
app.UseHttpsRedirection();
app.UseHsts();
```

#### Add Security Headers
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});
```

#### Configure CORS (if needed)
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", builder =>
    {
        builder.WithOrigins("https://events.college.edu")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

### 10. Monitoring & Maintenance

#### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

#### Logging
Configure Serilog or NLog for production logging:
```json
{
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## Troubleshooting

### Common Issues

1. **500 Internal Server Error**
   - Check application logs
   - Verify connection string
   - Ensure .NET Hosting Bundle is installed

2. **Database Connection Failed**
   - Verify SQL Server is accessible
   - Check firewall rules
   - Test connection string

3. **Static Files Not Loading**
   - Verify `wwwroot` folder permissions
   - Check IIS static content feature is enabled

4. **Authentication Issues**
   - Clear browser cookies
   - Check Identity configuration
   - Verify database migrations

## Rollback Procedure

```bash
# Stop the application
iisreset /stop

# Restore previous version
xcopy /E /Y C:\Backups\PreviousVersion\* C:\inetpub\wwwroot\CollegeEventPortal\

# Restore database
sqlcmd -S SERVER -d master -Q "RESTORE DATABASE CollegeEventPortalDb FROM DISK='C:\Backups\CollegeEventPortalDb.bak' WITH REPLACE"

# Start the application
iisreset /start
```

## Support Contacts

- **Technical Support**: support@college.edu
- **Database Admin**: dba@college.edu
- **Security Team**: security@college.edu

---

**Document Version**: 1.0  
**Last Updated**: April 2026
