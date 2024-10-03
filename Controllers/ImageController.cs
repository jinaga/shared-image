using System.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SharedImage.Controllers;

[Route("image")]
[ApiController]
public class ImageController : ControllerBase
{
    private readonly BlobContainerClient containerClient;

    public ImageController(BlobContainerClient containerClient)
    {
        this.containerClient = containerClient;
    }

    [HttpPost(Name = "UploadMedia")]
    public async Task<IActionResult> UploadMedia(IFormFile file)
    {
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
            BlobClient blobClient = containerClient.GetBlobClient(hash);

            stream.Seek(0, SeekOrigin.Begin);  // Reset the stream for uploading
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            // Track the media type (optional: store this info in a database if needed)
            // For now, assume you're storing the media type along with the file.

            // Step 4: Return a 201 response with the URL using the computed hash
            var mediaUrl = $"http://localhost:5279/image/{hash}";
            return Created(mediaUrl, new { url = mediaUrl, hash = hash });
        }
    }

    [HttpGet("{hash}")]
    public async Task<IActionResult> GetMedia(string hash)
    {
        // Retrieve the photo from Azure Blob Storage
        BlobClient blobClient = containerClient.GetBlobClient(hash);
        if (!await blobClient.ExistsAsync())
        {
            return NotFound();
        }

        // Download the photo from Blob Storage as a stream
        var photoStream = await blobClient.OpenReadAsync();

        // Return the photo to the client via the application
        return File(photoStream, "image/png");
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
