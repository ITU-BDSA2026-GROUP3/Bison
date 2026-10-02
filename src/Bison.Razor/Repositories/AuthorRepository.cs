




namespace Bison.Razor.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly BisonContext _context;

        public AuthorRepository(BisonContext context)
        {
            _context = context;
        }
        public async Task<Author?> ReadById(int id)
        {
            return await _context.Authors.FindAsync(id);
        }
        public async Task<Author?> ReadByEmail(string email)
        {
            return await _context.Authors.FirstOrDefaultAsync(a => a.Email == email);
        }
        public async Task<List<Author>> ReadAll()
        {
            return await _context.Authors.ToListAsync();
        }
        public async Task<List<Author>> ReadWithPosts(int authorId)
        {
            return await _context.Authors.Where(a => a.Id == authorId).Include(a => a.Posts).ToListAsync();
        }
        public async Task<int> Create(Author author)
        {
            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();
            return author.Id;
        }

        public async Task Update(Author author)
        {
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author is not null)
            {
                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
            }
        }

    }
}