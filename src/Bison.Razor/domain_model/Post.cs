namespace Bison.Razor.domain_model;
public abstract class Post
{
    public int Id { get; set; }

    public string Text { get; set; } = "";
    public DateTime TimeStamp { get; set; }

    public Author Author { get; set; } = null!;
}
