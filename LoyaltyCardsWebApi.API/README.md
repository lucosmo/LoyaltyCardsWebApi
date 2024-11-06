# LoyaltyCardsWebApi
A WebAPI built using C# and .NET 8 to manage user data and their loyalty cards. This API provides functionality for user authentication and authorization, ensuring that only authenticated users can access their data.

## Features

- **User Authentication & Authorization**: Utilizes JWT for secure user authentication.
- **User Management**: CRUD operations for user data.
- **Loyalty Cards Management**: CRUD operations for managing user-specific loyalty cards.
- **Secure Password Handling**: Passwords are hashed using industry-standard methods.

## Technologies Used

- **C#**: Primary programming language.
- **.NET 8**: Framework used for building the API.
- **Entity Framework Core**: ORM for database operations.
- **JWT**: For secure authentication.
- **SQL Server**: Database for storing user and loyalty card data.

## Getting Started 
### Prerequisites 
- .NET 8 SDK
- SQL Server (or any compatible database)
### Installation
- **Clone the repository:**
  ```git clone https://github.com/yourusername/UserLoyaltyCardsAPI.git```
- **Set up the database:**
  Database of choice is PostgreSQL.
  Root project folder contains .env file which contains database password:
  ```POSTGRES_PASSWORD: password```
  Create .env file yourself, as this file is not published due to security reasons.
  Install Entity Framework Core:
  ```dotnet add package Microsoft.EntityFrameworkCore```
  Install Npgsql (PostgreSQL provider) for Entity Framework Core:
  ```dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL```
  Install Design package for Entity Framework Core:
  ```dotnet add package Microsoft.EntityFrameworkCore.Design```
- **JWT:**
  Install JwtBearer package:
  ```dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer```
- **Run the migrations:**
  ```dotnet ef database update```
- **Run the application:**
  ```dotnet run```
