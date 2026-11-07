using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly BlobStorageService _blobService;

        // Injects the registered BlobStorageService
        public UploadsController(BlobStorageService blobService)
        {
            _blobService = blobService;
        }

        
        // Uploads an event banner image to Azure Blob Storage 
        
        [HttpPost("banner")]
        public async Task<IActionResult> UploadBanner(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }

            // Validates that the file is an image
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                return BadRequest(new { message = "Only image files (.jpg, .jpeg, .png, .webp) are permitted." });
            }

            try
            {
                string blobUrl = await _blobService.UploadAsync(file, "banners");
                return Ok(new { url = blobUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to upload image to Azure Storage.", error = ex.Message });
            }
        }

        
        // Uploads a participant profile picture to Azure Blob Storage under the 'avatars' virtual folder.
       
        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                return BadRequest(new { message = "Only image files (.jpg, .jpeg, .png, .webp) are permitted." });
            }

            try
            {
                string blobUrl = await _blobService.UploadAsync(file, "avatars");
                return Ok(new { url = blobUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to upload image to Azure Storage.", error = ex.Message });
            }
        }
    }
}