using BookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

[ApiController]
[Route("files")]

public sealed class FilesController : ControllerBase
{
    
private readonly BlobStorageService _blobStorageService;


    public FilesController(BlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    [HttpPost("upload")]
    [DisableRequestSizeLimit]
    [IgnoreAntiforgeryToken]

    public async Task<IActionResult> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        
        var blobName = await _blobStorageService.UploadAsync(file, cancellationToken); 

        return Ok(new
        {
            Uploaded = true,
            BlobName = blobName
        });
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var files = await _blobStorageService.ListAsync(cancellationToken);
        return Ok(files);
    }

    [HttpGet("{blobName}/download")]
    public async Task<IActionResult> DownloadAsync(string blobName ,CancellationToken cancellationToken)
    {
        try
        {
            var downloadResult = await _blobStorageService.DownloadAsync(blobName, cancellationToken);
            return File(downloadResult.Content.ToStream(), downloadResult.Details.ContentType ?? "application/octet-stream", blobName);
        }

        catch(FileNotFoundException)
        {
            return NotFound(new
            {
                Message = $"File '{blobName}' was not found."
            });
        }
    }

     [HttpDelete("{blobName}")]
    public async Task<IActionResult> DeleteAsync(
        string blobName,
        CancellationToken cancellationToken)
    {
        var deleted = await _blobStorageService.DeleteAsync(
            blobName,
            cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                Message = $"File '{blobName}' was not found."
            });
        }

        return NoContent();
    }

}