using System.Threading.Tasks;
using Bison.Core.domain_model;
using Bison.infrastructure.DTOs;
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
    
    [HttpGet]
    public async Task<IActionResult> GetImage(int id)
    {
        ObservationDto obs =  await _observationService.GetObservationFromId(id);
        if (obs?.ImgBytes == null) return NotFound();
        return File(obs.ImgBytes, "image/jpeg");
    }
}