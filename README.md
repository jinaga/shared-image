# SharedImage Project

This project uses Azure Blob Storage to store images and Azure Table Storage to manage metadata. Follow these steps to set up the necessary Azure services and configure the application using User Secrets for sensitive information.

## Prerequisites

- An Azure account. If you don't have one, you can create a free account at [https://azure.com/free](https://azure.com/free).
- .NET 8.0 SDK or later installed on your development machine.

## Setting up Azure Services

1. Create an Azure Storage account:
   - Sign in to the [Azure portal](https://portal.azure.com/).
   - Click on "Create a resource" and search for "Storage account".
   - Click "Create" and fill in the required information:
     - Choose a unique name for your storage account.
     - Select the appropriate subscription, resource group, and region.
     - For performance, choose "Standard".
     - For redundancy, choose "Locally-redundant storage (LRS)" for development (you can change this later for production).
   - Click "Review + create", then "Create" to create the storage account.

2. Create a Blob Container:
   - Once the storage account is created, go to its overview page.
   - In the left menu, under "Data storage", click on "Containers".
   - Click "+ Container" at the top.
   - Give your container a name (e.g., "media-container").
   - Set the public access level to "Private (no anonymous access)".
   - Click "Create".

3. Create an Azure Table:
   - In the storage account overview, click on "Tables" in the left menu.
   - Click "+ Table" at the top.
   - Give your table a name (e.g., "MediaMetadata").
   - Click "OK" to create the table.

4. Get the Connection String:
   - In the storage account overview, click on "Access keys" in the left menu.
   - Under "key1", click "Show" next to the "Connection string".
   - Copy this connection string; you'll need it for the application configuration.

## Configuring the Application with User Secrets

We use User Secrets to store sensitive information like connection strings. This keeps the sensitive data out of source control.

1. Initialize User Secrets for the project (if not already done):
   ```
   dotnet user-secrets init
   ```

2. Add the Azure Storage connection string to User Secrets:
   ```
   dotnet user-secrets set "AzureStorage:ConnectionString" "your-connection-string-here"
   ```

3. Add the container name to User Secrets:
   ```
   dotnet user-secrets set "AzureStorage:ContainerName" "your-container-name-here"
   ```

4. Add the table name to User Secrets:
   ```
   dotnet user-secrets set "AzureStorage:TableName" "your-table-name-here"
   ```

5. Verify the secrets are set correctly:
   ```
   dotnet user-secrets list
   ```

## Running the Application

1. Restore the NuGet packages:
   ```
   dotnet restore
   ```

2. Build the application:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   dotnet run
   ```

The application should now be configured to use your Azure Blob Storage account for storing images and Azure Table Storage for storing metadata, with sensitive information stored securely in User Secrets.

## Testing

You can test the image upload functionality by sending a POST request to the `/image` endpoint with an image file. The application will store the image in the configured Azure Blob Storage container, save metadata to the Azure Table, and return a URL for accessing the image.

## Troubleshooting

- If you encounter any "Access Denied" errors, double-check that your connection string in User Secrets is correct and that the storage account, container, and table are properly set up.
- Ensure that the Azure Storage Emulator is not running if you're trying to connect to an actual Azure Storage account.
- Check that the required NuGet packages (Azure.Storage.Blobs, Azure.Data.Tables, and Microsoft.Extensions.Azure) are properly installed and restored.
- Verify that User Secrets are correctly set up and accessible in your development environment.

For any other issues, refer to the [Azure Blob Storage documentation](https://docs.microsoft.com/en-us/azure/storage/blobs/) or [Azure Table Storage documentation](https://docs.microsoft.com/en-us/azure/storage/tables/) or seek assistance from the development team.

## Security Note

Remember that User Secrets are for development purposes only. For production environments, use Azure Key Vault or other secure methods to manage sensitive configuration data.