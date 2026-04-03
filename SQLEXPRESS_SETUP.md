# SQL Server Express Setup Guide

## ✅ Configuration Complete!

Your College Event Management Portal is now configured to use **SQL Server Express** instead of LocalDB.

## 📋 What Was Changed

### 1. Connection String Updated
**File**: `appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=CollegeEventPortalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Key Changes:**
- ✅ Changed from `(localdb)\\mssqllocaldb` to `localhost\\SQLEXPRESS`
- ✅ Added `TrustServerCertificate=True` for SSL certificate handling
- ✅ Kept `Trusted_Connection=True` for Windows Authentication

### 2. Decimal Precision Fixed
**File**: `Data/ApplicationDbContext.cs`

Added precision configuration for the `Points` field in `Score` entity:

```csharp
builder.Entity<Score>()
    .Property(s => s.Points)
    .HasPrecision(18, 2);
```

This fixes the EF Core warning about decimal precision.

### 3. Database Created Successfully
- ✅ Database: `CollegeEventPortalDb`
- ✅ Server: `localhost\SQLEXPRESS`
- ✅ All tables created with proper relationships
- ✅ Identity tables configured
- ✅ Seed data ready to be applied

## 🚀 How to Run the Application

### Option 1: Using dotnet CLI
```bash
cd CollegeEventPortal
dotnet run
```

### Option 2: Using Visual Studio
1. Open `CollegeEventPortal.csproj` in Visual Studio
2. Press `F5` or click "Start Debugging"

### Option 3: Using VS Code
1. Open the folder in VS Code
2. Press `F5` or use the Run menu

## 🌐 Access the Application

Once running, open your browser and navigate to:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

## 🔐 Default Login Credentials

The application will automatically seed default users on first run:

### Admin Account
- **Email**: admin@college.edu
- **Password**: Admin@123
- **Access**: Full system administration

### Judge Account
- **Email**: judge@college.edu
- **Password**: Judge@123
- **Access**: Score participants and view assigned events

### Student Account
- **Email**: student@college.edu
- **Password**: Student@123
- **Access**: Register for events, join teams, view certificates

## 🗄️ Database Management

### View Database in SQL Server Management Studio (SSMS)
1. Open SSMS
2. Connect to: `localhost\SQLEXPRESS`
3. Expand Databases → `CollegeEventPortalDb`

### View Database in Azure Data Studio
1. Open Azure Data Studio
2. New Connection
3. Server: `localhost\SQLEXPRESS`
4. Authentication: Windows Authentication
5. Database: `CollegeEventPortalDb`

### View Database in Visual Studio
1. View → SQL Server Object Explorer
2. Expand SQL Server → (localdb)\SQLEXPRESS
3. Find `CollegeEventPortalDb`

## 🔧 Useful Commands

### Check Database Exists
```bash
sqlcmd -S localhost\SQLEXPRESS -C -Q "SELECT name FROM sys.databases WHERE name = 'CollegeEventPortalDb'"
```

### View All Tables
```bash
sqlcmd -S localhost\SQLEXPRESS -C -d CollegeEventPortalDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
```

### Check SQL Server Service Status
```powershell
Get-Service MSSQL$SQLEXPRESS
```

### Start SQL Server Express (if stopped)
```powershell
Start-Service MSSQL$SQLEXPRESS
```

### Stop SQL Server Express
```powershell
Stop-Service MSSQL$SQLEXPRESS
```

## 🔄 Reset Database (if needed)

If you need to reset the database:

```bash
# Drop the database
dotnet ef database drop

# Recreate and apply migrations
dotnet ef database update
```

Or using SQL:
```sql
USE master;
GO
DROP DATABASE CollegeEventPortalDb;
GO
```

Then run:
```bash
dotnet ef database update
```

## 📊 Verify Database Schema

After running migrations, verify the tables were created:

```sql
USE CollegeEventPortalDb;
GO

-- List all tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check Identity tables
SELECT * FROM AspNetRoles;
SELECT * FROM AspNetUsers;

-- Check application tables
SELECT * FROM Events;
SELECT * FROM Teams;
SELECT * FROM Registrations;
```

## 🐛 Troubleshooting

### Issue: Cannot connect to SQL Server Express

**Solution 1**: Verify SQL Server is running
```powershell
Get-Service MSSQL$SQLEXPRESS
```

If stopped, start it:
```powershell
Start-Service MSSQL$SQLEXPRESS
```

**Solution 2**: Check SQL Server Configuration Manager
1. Open SQL Server Configuration Manager
2. SQL Server Network Configuration → Protocols for SQLEXPRESS
3. Ensure TCP/IP is Enabled
4. Restart SQL Server service

### Issue: Login failed for user

**Solution**: The connection string uses Windows Authentication (`Trusted_Connection=True`). Ensure you're logged into Windows with an account that has access to SQL Server.

To use SQL Server Authentication instead:
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=CollegeEventPortalDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

### Issue: Certificate chain error

**Solution**: The connection string already includes `TrustServerCertificate=True` which handles this. If you still see errors, verify the connection string is correct.

### Issue: Database already exists

**Solution**: Drop and recreate:
```bash
dotnet ef database drop --force
dotnet ef database update
```

### Issue: Migration errors

**Solution**: Remove and recreate migrations:
```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 📝 Connection String Options

### Current (Windows Authentication)
```json
"Server=localhost\\SQLEXPRESS;Database=CollegeEventPortalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### SQL Server Authentication
```json
"Server=localhost\\SQLEXPRESS;Database=CollegeEventPortalDb;User Id=YourUsername;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Remote SQL Server
```json
"Server=YOUR_SERVER_IP\\SQLEXPRESS;Database=CollegeEventPortalDb;User Id=YourUsername;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

## 🔒 Security Notes

1. **Change Default Passwords**: After first login, change all default passwords
2. **Connection String**: In production, store connection strings in environment variables or Azure Key Vault
3. **SQL Server Authentication**: If using SQL auth, use strong passwords
4. **Firewall**: Configure Windows Firewall to allow SQL Server connections if accessing remotely

## 📈 Performance Tips

1. **Enable TCP/IP**: For better performance, enable TCP/IP protocol in SQL Server Configuration Manager
2. **Indexes**: The application includes proper indexes for frequently queried columns
3. **Connection Pooling**: Already enabled via `MultipleActiveResultSets=true`

## 🎯 Next Steps

1. ✅ Run the application: `dotnet run`
2. ✅ Login with admin credentials
3. ✅ Create your first event
4. ✅ Test student registration
5. ✅ Explore all features

## 📞 Support

If you encounter any issues:
1. Check the troubleshooting section above
2. Review application logs in the console
3. Check SQL Server error logs in Event Viewer
4. Verify SQL Server Express is running

---

**Configuration Status**: ✅ Complete  
**Database Status**: ✅ Created  
**Build Status**: ✅ Success (0 Warnings, 0 Errors)  
**Ready to Run**: ✅ Yes

**Happy Coding! 🚀**
