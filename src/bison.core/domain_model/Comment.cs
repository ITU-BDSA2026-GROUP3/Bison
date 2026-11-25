namespace Bison.Razor.domain_model;
public class Comment : Post
{
    public Observation Observation { get; set; } = null!;
}