using Microsoft.AspNetCore.Identity;

namespace Bison.Razor.domain_model;
public class Author : IdentityUser
{
    public bool IsDeleted { get; set; }

    public List<Post> Posts { get; set; } = new();
}