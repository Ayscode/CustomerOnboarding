# CustomerOnboarding

This project is a microservice API developed as part of an assesment for Wema Bank. It provides endpoints for onboarding customers, retrieving existing customers, and fetching existing banks. The API is built using ASP.NET Core, Entity Framework Core, and Microsoft SQL Server.

## Features

- **Onboard Customer**: Endpoint for onboarding a customer with phone number, email, password, state of residence, and LGA. Includes OTP verification and mapping LGAs to states.
- **Get All Customers**: Endpoint to retrieve all existing customers previously onboarded.
- **Get Existing Banks**: Endpoint to fetch the list of existing banks by consuming an external API.

## Installation

1. Clone the repository:

```bash
git clone hhttps://github.com/Ayscode/CustomerOnboarding.git
```

2. Navigate to the project directory:

```bash
cd CustomerOnboarding
```

3. Build the project:

```bash
dotnet build
```

4. Run the project:

```bash
dotnet run
```

## Usage

Once the project is running, you can access the API endpoints using tools like Postman or any web browser:

- **Onboard Customer**: `POST /api/customer/onboard`
- **Get All Customers**: `GET /api/customer`
- **Get Existing Banks**: `GET /api/bank`

Ensure to provide necessary parameters as mentioned in the [API documentation](#features).

## Configuration

- Database connection string can be configured in `appsettings.json`.
- External API base URL can be configured in `appsettings.json` if required.

## Dependencies

- ASP.NET Core
- Entity Framework Core
- Microsoft SQL Server
- Swashbuckle.AspNetCore (Swagger)

## Contributing

Contributions are welcome! Feel free to open [issues](hhttps://github.com/Ayscode/CustomerOnboarding/issues) or [pull requests](hhttps://github.com/Ayscode/CustomerOnboarding/pulls) for any improvements or features you'd like to see added.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
