using Microsoft.AspNetCore.Identity;

namespace Bison.Razor.domain_model;
public class Author : IdentityUser
{

    public List<Post> Posts { get; set; } = new();
}