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
            using var ms = new MemoryStream();
            img.SaveAsPng(ms);
            Byte[] bytes = ms.ToArray();
            //make a new observation with the the bytes for the field Imagedata then grab the new observation id
            return View(image.FileName);//this might not always be a consistent way to display the file name so futher checks might be needed
        }
        catch
        {
            return BadRequest("File is not a valid image.");
        }
    }
}