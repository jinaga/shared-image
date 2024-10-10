using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Azure Blob Storage and Table Storage
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage:ConnectionString"]);
    clientBuilder.AddTableServiceClient(builder.Configuration["AzureStorage:ConnectionString"]);
});

builder.Services.AddSingleton(sp =>
{
    var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
    var containerName = builder.Configuration["AzureStorage:ContainerName"];
    return blobServiceClient.GetBlobContainerClient(containerName);
});

builder.Services.AddSingleton(sp =>
{
    var tableServiceClient = sp.GetRequiredService<TableServiceClient>();
    var tableName = builder.Configuration["AzureStorage:TableName"];
    return tableServiceClient.GetTableClient(tableName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
