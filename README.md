#  Tourism Project – Backend (ASP.NET Core Web API)

This is the backend API for the Tourism Project, built using ASP.NET Core Web API and SQL Server. It manages all backend functionality such as user accounts, tourism content, trip planning, and favorites.

---

##  Core Features

###  User Management
- Register / Login / Profile Update
## 📁 Folder Structure
- Password Recovery
- Email Verification

###  Tourism Content
- Get Tourism Types
- Get Places by Type
- Place Details with Hotels and Transportation

###  Trip Management
- Create Custom Trip
- View Ready-made Trips
- Set Duration and Budget
- Calculate Cost

###  Booking (Mock)
- Book Selected Trips
- Simulated Payment (No real transactions)

###  Favorites
- Add/Remove Favorites (Places, Hotels, Activities)
- Get All Favorites

---

##  Folder Structure
Tourism_project/
├── Controllers/ # API Controllers
├── Models/ # Entity Models
├── DTOs/ # Data Transfer Objects
├── Services/ # Business Logic
├── Migrations/ # EF Core Migrations
├── TourismDbContext.cs # DB Context
├── Program.cs # App Entry Point
└── appsettings.json # Configuration

##  Getting Started

1. **Clone the Repository**
   ```bash
   git clone https://github.com/hebagamal23/Tourism-app.git
   cd Tourism_project


