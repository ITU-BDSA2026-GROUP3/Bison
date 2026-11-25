namespace Bison.Core.domain_model;

public class Comment : Post
{
    public Observation Observation { get; set; } = null!;
}