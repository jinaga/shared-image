using Azure.Data.Tables;
using Azure.Storage.Blobs;
using SharedImage.Config;

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register BlobContainerClient
builder.Services.AddSingleton(x =>
{
    var azureBlobStorageConfig = builder.Configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageConfig>();
    if (azureBlobStorageConfig is null)
    {
        throw new InvalidOperationException("AzureBlobStorage configuration is missing.");
    }
    return new BlobContainerClient(
        azureBlobStorageConfig.ConnectionString,
        azureBlobStorageConfig.ContainerName);
});

// Register TableClient
builder.Services.AddSingleton(x =>
{
    var azureTableStorageConfig = builder.Configuration.GetSection("AzureTableStorage").Get<AzureTableStorageConfig>();
    if (azureTableStorageConfig is null)
    {
        throw new InvalidOperationException("AzureTableStorage configuration is missing.");
    }
    return new TableClient(
        azureTableStorageConfig.ConnectionString,
        azureTableStorageConfig.TableName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
