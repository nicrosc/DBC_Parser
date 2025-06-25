# DBC Parser

A .NET 8 Blazor application for parsing and analyzing DBC (Database CAN) files used in automotive applications.

## 🚀 Features

- Upload and parse DBC files
- View DBC file details and structure
- Browse messages, signals, and attributes
- Search through DBC data
- Modern Blazor web interface

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (version 12 or higher)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

## 🛠️ Setup Instructions

### 1. Clone the Repository
```bash
git clone https://github.com/nicrosc/DBC_Parser.git
cd DBC_Parser
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Database Setup

#### Install PostgreSQL
1. Download and install PostgreSQL from [postgresql.org](https://www.postgresql.org/download/)
2. During installation, remember the password you set for the `postgres` user

#### Configure Connection String
1. Open `ZadatakJuric/ZadatakJuric/appsettings.json`
2. Update the connection string to match your PostgreSQL instance:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dbcparserdb;Username=postgres;Password=your_password_here"
  }
}
```

### 4. Build the Solution
```bash
dotnet build
```

### 5. Run the Application
```bash
cd ZadatakJuric/ZadatakJuric
dotnet run
```
