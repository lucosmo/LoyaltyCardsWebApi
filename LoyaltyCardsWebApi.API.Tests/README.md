# LoyaltyCardsWebApi.API.Test
A test project built using C# and NUnit to validate the functionality of the LoyaltyCardsWebApi.API. This project ensures the API's methods work as expected and handles edge cases effectively.

## Features

- **Unit Testing**: Tests for individual methods and components of the API.
- **Integration Testing**: Ensures different components work seamlessly together.
- **Mocking**: Uses Moq for creating mock dependencies to isolate units under test.

## Technologies Used

- **C#**: Primary programming language.
- **NUnit**: Test framework for writing and running tests.
- **Moq**: Library for mocking dependencies.

## Getting Started
### Prerequisites
- .NET 8 SDK
- LoyaltyCardsWebApi.API project set up and running.

### Installation
- **Clone the repository:**
  ```bash git clone https://github.com/yourusername/UserLoyaltyCardsAPI.git```
- **Install required NuGet packages:**
  NUnit Test Framework:
  ```dotnet add package NUnit```
  Moq (for mocking dependencies):
  ```dotnet add package Moq```
- **Run all tests**
  ```dotnet test```