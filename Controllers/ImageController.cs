using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using SharedImage.Entities;
using System.Security.Cryptography;

namespace SharedImage.Controllers;
[Route("image")]
public class ImageController : Controller
{
    private readonly BlobContainerClient _containerClient;
    private readonly TableClient _tableClient;

    public ImageController(BlobContainerClient containerClient, TableClient tableClient)
    {
        _containerClient = containerClient;
        _tableClient = tableClient;
    }

    [HttpPost()]
    public async Task<IActionResult> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Step 1: Compute the hash of the incoming media
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            var mediaBytes = stream.ToArray();
            string hash = ComputeHash(mediaBytes);

            // Step 2: Validate that the incoming media is in the format described by the Content-Type header
            if (!ValidateMediaType(file.ContentType, mediaBytes))
            {
                return BadRequest("Invalid media type or corrupted file.");
            }

            // Step 3: Store the media in Blob storage
            BlobClient blobClient = _containerClient.GetBlobClient(hash);

            if (!blobClient.Exists())
            {
                stream.Seek(0, SeekOrigin.Begin);  // Reset the stream for uploading
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                // Step 4: Store metadata in Azure Table Storage
                var mediaEntity = new MediaEntity
                {
                    PartitionKey = "media",
                    RowKey = hash,
                    ContentType = file.ContentType,
                    UploadTime = DateTimeOffset.UtcNow
                };

                await _tableClient.AddEntityAsync(mediaEntity);
            }

            // Step 5: Return a 201 response with the URL using the computed hash
            var mediaUrl = Url.Action("DownloadMedia", "Image", new { hash = hash }, Request.Scheme);
            return Created(mediaUrl, new { url = mediaUrl, hash = hash });
        }
    }

    [HttpGet("{hash}")]
    public async Task<IActionResult> DownloadMedia(string hash)
    {
        try
        {
            // Step 1: Retrieve the media metadata from Azure Table Storage
            // Try to retrieve the entity with the provided hash as the RowKey
            var mediaEntity = await _tableClient.GetEntityAsync<MediaEntity>("media", hash);

            // Step 2: Fetch the media content type from the table storage entity
            var contentType = mediaEntity.Value.ContentType;

            // Step 3: Retrieve the media from Azure Blob Storage
            BlobClient blobClient = _containerClient.GetBlobClient($"{hash}");

            // Download the media as a stream
            var mediaStream = await blobClient.OpenReadAsync();

            // Step 4: Return the file with the correct content type
            return File(mediaStream, contentType);
        }
        catch (RequestFailedException ex)
        {
            // Handle case where media does not exist or other Azure request failure
            if (ex.Status == 404)
            {
                return NotFound("Media not found.");
            }

            // Return a generic error message
            return StatusCode(500, "An error occurred while retrieving the media.");
        }
    }



    // Helper method to compute the SHA-256 hash of the media file
    private string ComputeHash(byte[] mediaBytes)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(mediaBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    // Helper method to validate the media type
    private bool ValidateMediaType(string contentType, byte[] mediaBytes)
    {
        var allowedMediaTypes = new HashSet<string>
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        // Check if the content type is supported
        if (!allowedMediaTypes.Contains(contentType))
        {
            return false;
        }

        // Additional check: Validate file signature (magic numbers) to ensure the content matches the Content-Type header
        if (!IsValidImageSignature(contentType, mediaBytes))
        {
            return false;
        }

        return true;
    }

    // Helper method to validate the file signature (magic numbers) of the image
    private bool IsValidImageSignature(string contentType, byte[] mediaBytes)
    {
        var jpgSignature = new byte[] { 0xFF, 0xD8 };
        var pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var gifSignature = new byte[] { 0x47, 0x49, 0x46 };
        var bmpSignature = new byte[] { 0x42, 0x4D };  // BMP "BM"
        var webpSignature = new byte[] { 0x52, 0x49, 0x46, 0x46 };  // "RIFF" for WEBP

        // Validate based on content type
        return contentType switch
        {
            "image/jpeg" => mediaBytes.Take(jpgSignature.Length).SequenceEqual(jpgSignature),
            "image/png" => mediaBytes.Take(pngSignature.Length).SequenceEqual(pngSignature),
            "image/gif" => mediaBytes.Take(gifSignature.Length).SequenceEqual(gifSignature),
            "image/bmp" => mediaBytes.Take(bmpSignature.Length).SequenceEqual(bmpSignature),
            "image/webp" => mediaBytes.Take(webpSignature.Length).SequenceEqual(webpSignature),
            _ => false,
        };
    }
}
