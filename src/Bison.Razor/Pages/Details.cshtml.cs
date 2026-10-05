using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class DetailsModel : PageModel
{
    private readonly IObservationService _service;
    public List<CommentViewModel> Comments { get; set; }

    public DetailsModel(IObservationService service)
    {
        _service = service;
    }

    public async Task<ActionResult> OnGet(int id)
    {
        Comments = await _service.GetComments(id);
        return Page();
    }
}
