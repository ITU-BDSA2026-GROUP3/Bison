using System.Threading.Tasks;
using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
[ApiController]
[Route("[controller]/[action]")]
public class ImageController : Controller
{
    private readonly IObservationService _observationService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IObservationService observationService, ILogger<ImageController> logger)
    {
        _logger = logger;
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
            await _observationService.CreateObservation(null, null, 0, bytes); //0 acts as a null assignment here because auto increment starts at 1 from sql
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