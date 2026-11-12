using System.Threading.Tasks;
using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public class ImageController : Controller
{
    private readonly IObservationService _observationService;

    public ImageController(IObservationService observationService)
    {
        _observationService = observationService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile image)
    {
        try
        {
            using var img = Image.Load(image.OpenReadStream());
            using var ms = new MemoryStream();
            if (img.Height > 720) img.Mutate(x => x.Resize(img.Width, 720));
            if (img.Width > 1280) img.Mutate(x => x.Resize(1280, img.Height));
            await img.SaveAsJpegAsync(ms);
            Byte[] bytes = ms.ToArray();
            //make a new observation with the the bytes for the field Imagedata then grab the new observation id
            int observationid = 0;// replace observationid with the actual value
            ViewBag.observationid = observationid; //this will make ViewBag.observationid null if this method is never run
            return View(image.FileName);//this might not always be a consistent way to display the file name so futher checks might be needed
        }
        catch
        {
            return BadRequest("File is not a valid image.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetImage(int id)
    {
        ObservationDto obs =  await _observationService.GetObservationFromId(id);
        if (obs?.ImgBytes == null) return NotFound();
        return File(obs.ImgBytes, "image/jpeg");
    }
}