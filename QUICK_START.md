# 🚀 Quick Start Guide

## Run the Application (3 Steps)

### 1. Navigate to Project
```bash
cd CollegeEventPortal
```

### 2. Run Application
```bash
dotnet run
```

### 3. Open Browser
```
http://localhost:5228
```
(Check console for actual port)

---

## 🔐 Login Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | admin@college.edu | Admin@123 |
| **Judge** | judge@college.edu | Judge@123 |
| **Student** | student@college.edu | Student@123 |

---

## ✅ Configuration Status

- ✅ SQL Server Express: **Connected**
- ✅ Database: **Created** (CollegeEventPortalDb)
- ✅ Migrations: **Applied**
- ✅ Build: **Success** (0 Warnings, 0 Errors)
- ✅ Ready to Run: **YES**

---

## 🔧 Useful Commands

### Run Application
```bash
dotnet run
```

### Build Application
```bash
dotnet build
```

### Reset Database
```bash
dotnet ef database drop --force
dotnet ef database update
```

### Check SQL Server
```powershell
Get-Service MSSQL$SQLEXPRESS
```

### View Database
```bash
sqlcmd -S localhost\SQLEXPRESS -C -Q "SELECT name FROM sys.databases WHERE name = 'CollegeEventPortalDb'"
```

---

## 📚 Documentation

- `README.md` - Complete guide
- `SQLEXPRESS_SETUP.md` - SQL Server setup
- `CONFIGURATION_COMPLETE.md` - What was done
- `DEPLOYMENT_GUIDE.md` - Production deployment

---

## 🎯 First Steps After Login

### As Admin:
1. Create your first event
2. Set event details and rules
3. Publish the event

### As Student:
1. Browse available events
2. Register for an event
3. Create or join a team

### As Judge:
1. View assigned events
2. Score participants
3. View leaderboards

---

**Need Help?** Check `SQLEXPRESS_SETUP.md` for troubleshooting!
