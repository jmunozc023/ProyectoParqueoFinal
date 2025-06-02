# ProyectoParqueoFinal

ASP.NET Core project for a web application to manage parking lots at the ULACIT campus, using Entity Framework and cookies for session management.

## Description

The main goal of this project is to create an application for managing the ULACIT campus parking lot. It allows for the administration of parking spaces, users, and access management, and is designed for users, administrators, and security officers.

## Technologies Used

- **ASP.NET Core**
- **Entity Framework**
- **Azure SQL Database**
- **HTML, CSS, JavaScript**

## Intended Audience

- **Users:** Confirm personal data and the usage reports of the parking lot.
- **Administrators:** Manage users, parking spaces, and system settings.
- **Security Officers:** Validate access and manage entries/exits.

## Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/jmunozc023/ProyectoParqueoFinal.git
   ```
2. Configure your Azure SQL Database connection string in `appsettings.json`.
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Apply Entity Framework migrations:
   ```bash
   dotnet ef database update
   ```
5. Run the application locally:
   ```bash
   dotnet run
   ```
6. (Optional) Deploy to Azure following the [official Microsoft documentation](https://docs.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore).

## Usage

(Screenshots and detailed usage instructions will be added soon)

## Contributing

Currently, the main maintainer and contributor is @jmunozc023. Contributions are welcome! If you’d like to collaborate, please fork the repository and submit a pull request.

## FAQ

**What database does the project use?**  
It uses Azure SQL Database, but you can modify the connection string to use a local SQL Server if you prefer.

**Can I deploy the app to Azure?**  
Yes, the application is ready to be deployed to Azure App Service.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contact

For questions or suggestions, contact:  
- José Muñoz ([@jmunozc023](https://github.com/jmunozc023))
