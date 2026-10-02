
namespace Bison.Razor.Repositories
{
    public class TestDTO // for testing!
    {
        public TestDTO(string text, DateTime t)
        {
        }
    }

    public interface IAuthorRepository
    {
        Task<Author?> ReadById(int id);
        Task<Author?> ReadByEmail(string email);
        Task<List<Author>> ReadAll();
        Task<List<Author>> ReadWithPosts(int authorId);

        Task<int> Create(Author author);
        Task Update(Author author);
        Task Delete(int id);
    }
}
