using Bison.Core.domain_model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bison.Razor.Pages;

[Authorize]
public class AboutMeModel: PageModel
{
    private readonly BisonContext _context;
    private readonly IObservationService _observationService;
    private readonly SignInManager<Author> _signInManager;

    public Author? CurrentAuthor {get; private set;}

    public List<Observation> Observations{get; private set;} = new();

    public List<Comment> Comments {get; private set;} = new();

    public List<Proposal> Proposals {get; private set;} = new();

    public AboutMeModel(BisonContext context, IObservationService observationService, SignInManager<Author> signInManager)    {
        _context = context;
        _observationService = observationService;
        _signInManager = signInManager;
    } 

    public async Task<IActionResult> OnGetAsync()
    {
        //Get the ID of the current user
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();

        }

        CurrentAuthor = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.Id == userId);
        
        if(CurrentAuthor == null)
        {
            return NotFound();
        }

        Observations = await _context.Observations
        .AsNoTracking()
        .Where(o => !o.IsDeleted && o.Author != null && o.Author.Id == userId)
        .Include(o => o.Taxon)
        .OrderByDescending(o => o.TimeStamp)
        .ToListAsync();


        Comments = await _context.Comments
        .AsNoTracking()
        .Where(c => !c.IsDeleted && c.Author != null && c.Author.Id == userId)
        .Include(c => c.Observation)
        .OrderByDescending(c => c.TimeStamp)
        .ToListAsync();

        Proposals = await _context.Proposals
        .AsNoTracking()
        .Where(p => !p.IsDeleted && p.Author != null && p.Author.Id == userId)
        .Include(p => p.Observation)
        .Include(p => p.Taxon)
        .OrderByDescending(p => p.TimeStamp)
        .ToListAsync();

        return Page();

    }
    public async Task<IActionResult> OnPostForgetMeAsync(
        bool keepObservations)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        await _observationService.ForgetUser(
            userId,
            keepObservations);

        await _signInManager.SignOutAsync();

        return RedirectToPage("/Public");
    }
}