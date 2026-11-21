using Bison.Razor.domain_model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bison.Razor.Pages;

[Authorize]
public class AboutMeModel: PageModel
{
    private readonly BisonContext _context;

    public Author? CurrentAuthor {get; private set;}

    public List<Observation> Observations{get; private set;} = new();

    public List<Comment> Comments {get; private set;} = new();

    public List<Proposal> Proposals {get; private set;} = new();

    public AboutMeModel(BisonContext context)    {
        _context = context;
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
        .Where(o => o.Author.Id == userId)
        .Include(o => o.Taxon)
        .OrderByDescending(o => o.TimeStamp)
        .ToListAsync();


        Comments = await _context.Comments
        .AsNoTracking()
        .Where(c => c.Author.Id == userId)
        .Include(c => c.Observation)
        .OrderByDescending(c => c.TimeStamp)
        .ToListAsync();

        Proposals = await _context.Proposals
        .AsNoTracking()
        .Where(p => p.Author.Id == userId)
        .Include(p => p.Observation)
        .Include(p => p.Taxon)
        .OrderByDescending(p => p.TimeStamp)
        .ToListAsync();

        return Page();

    }
}