using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class BookRepository : IBookRepository
{
    private readonly DataContext _context;


    public BookRepository(DataContext context)
    {
        _context = context;
    }


    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task LoadRelationShipsAsync(Book book)
    {
        await _context.Entry(book).Collection(p => p.Tags).LoadAsync();
        await _context.Entry(book).Collection(p => p.Highlights).LoadAsync();
        await _context.Entry(book).Collection(p => p.Bookmarks).LoadAsync();
        
        // Load the RectFs from the loaded highlights as well
        foreach (var highlight in book.Highlights)
        {
            await _context.Entry(highlight).Collection(p => p.Rects).LoadAsync();
        }
    }
    
    public IQueryable<Book> GetAllAsync(string userId, bool loadRelationships = false)
    {
        if (loadRelationships)
        {
            return _context.Books.Where(book => book.UserId == userId)
                .Include(b => b.Tags)
                .Include(b => b.Bookmarks)
                .Include(b => b.Highlights).ThenInclude(h => h.Rects);
        }
        
        return _context.Books.Where(book => book.UserId == userId);
    }

    public async Task<bool> ExistsAsync(string userId, Guid bookGuid)
    {
        return await _context.Books.AnyAsync(book => book.UserId == userId &&
                                                     book.BookId == bookGuid);
    }

    public void DeleteBook(Book book)
    {
        _context.Remove(book);
    }

    public async Task<long> GetUsedBookStorage(string userId)
    {
        var coverStorage = await _context.Books.Where(book => book.UserId == userId).SumAsync(book => book.CoverSize);
        var books = await _context.Books.Where(book => book.UserId == userId).ToListAsync();
        var bookStorage = books.Sum(book => GetBytesFromSizeString(book.DocumentSize));

        return coverStorage + (long)bookStorage;
    }

    private double GetBytesFromSizeString(string size)
    {
        size = size.Replace(" ", string.Empty);
        size = size.Replace(",", ".");
        
        int typeBegining = -1;
        for (int i = 0; i < size.Length; i++)
        {
            if (!char.IsDigit(size[i]) && size[i] != '.')
            {
                typeBegining = i;
                break;
            }
        }

        var numberString = size.Substring(0, typeBegining);
		var provider = new System.Globalization.NumberFormatInfo();
		provider.NumberDecimalSeparator = ".";
		provider.NumberGroupSeparator = ",";
		var numbers = Convert.ToDouble(numberString,provider);
		
        var type = size[typeBegining..];
        return type.ToLower() switch
        {
            "b" => numbers,
            "kb" => numbers * 1000,
            "mb" => numbers * 1000 * 1000,
            "gb" => numbers * 1000 * 1000
        };
    }
}