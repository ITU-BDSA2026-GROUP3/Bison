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

    public ActionResult OnGet(long id)
    {
        Comments = _service.GetComments(id);
        return Page();
    }
}
