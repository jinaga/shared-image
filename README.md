# Shared Image Project

This project is a .NET application that uses Azure Blob Storage for image handling. It demonstrates how to securely manage connection strings and other sensitive information using user secrets.

## Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- An Azure account with an active subscription

## Setting up Azure Storage Account and Blob Container

1. Sign in to the [Azure portal](https://portal.azure.com/).
2. Create a new storage account or use an existing one:
   - If creating a new account, click on "Create a resource" > "Storage" > "Storage account".
   - Choose your subscription, resource group, storage account name, and region.
   - Review and create the storage account.

3. Once the storage account is created, navigate to it in the Azure portal.
4. In the left menu, under "Data storage", click on "Containers".
5. Click on "+ Container" to create a new container:
   - Enter a name for your container (e.g., "sharedimage").
   - Choose the appropriate access level (e.g., "Private" for most cases).
   - Click "Create".

6. After creating the container, go back to the storage account overview.
7. In the left menu, under "Security + networking", click on "Access keys".
8. Copy the connection string for later use.

## Configuring User Secrets

1. Open a terminal in your project directory.
2. Initialize user secrets for the project:
   ```
   dotnet user-secrets init --project shared-image.csproj
   ```

3. Set the Azure Blob Storage connection string:
   ```
   dotnet user-secrets set "AzureBlobStorage:ConnectionString" "YOUR_CONNECTION_STRING" --project shared-image.csproj
   ```
   Replace `YOUR_CONNECTION_STRING` with the connection string you copied from the Azure portal.

4. Set the container name:
   ```
   dotnet user-secrets set "AzureBlobStorage:ContainerName" "YOUR_CONTAINER_NAME" --project shared-image.csproj
   ```
   Replace `YOUR_CONTAINER_NAME` with the name of the container you created (e.g., "sharedimage").

## Running the Application

1. Ensure all dependencies are installed:
   ```
   dotnet restore
   ```

2. Build the project:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   dotnet run
   ```

The application should now be running and correctly using the Azure Blob Storage configuration from your user secrets.

## Important Notes

- User secrets are for development purposes only. For production, use a more robust secret management system like Azure Key Vault.
- Never commit user secrets or connection strings to version control.
- If you need to share secrets with other developers, do so securely and not through version control.

## Troubleshooting

If you encounter any issues:
- Ensure your Azure subscription is active and the storage account is properly set up.
- Verify that the connection string and container name in your user secrets are correct.
- Check that the application can access the Azure services (no firewall or network issues).

For more information on user secrets, refer to the [official Microsoft documentation](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets).