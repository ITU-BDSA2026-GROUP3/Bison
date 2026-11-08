using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

public class ImageController : Controller
{
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile image)
    {
        try
        {
            using var img = Image.Load(image.OpenReadStream());
        }
        catch
        {
            return BadRequest("File is not a valid image.");
        }
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);
        //add code to add it to server
        return View();
    }
}