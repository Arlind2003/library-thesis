using System.Text.Json;
using Caktoje.Constants.Enums;
using Caktoje.Data;
using Caktoje.Data.Bindings;
using Caktoje.Data.Resources;
using Caktoje.Data.Resources.Common;
using Caktoje.Data.Resources.OpenLibrary;
using Caktoje.Models;
using Microsoft.EntityFrameworkCore;

namespace Caktoje.Services.Admin;

public class OpenLibraryService
{
    private readonly HttpClient _httpClient;
    private readonly CaktojeDbContext _context;
    private readonly StorageService _storageService;

    public OpenLibraryService(HttpClient httpClient, CaktojeDbContext context, StorageService storageService)
    {
        _httpClient = httpClient;
        _context = context;
        _storageService = storageService;
    }

    public async Task<OpenLibraryBook?> GetBookByIsbnAsync(string isbn)
    {
        var formattedIsbn = $"ISBN:{isbn}";
        var url = $"https://openlibrary.org/api/books?bibkeys={formattedIsbn}&format=json&jscmd=details";

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var response = await _httpClient.GetFromJsonAsync<Dictionary<string, OpenLibraryBook>>(url, options);

        if (response != null && response.TryGetValue(formattedIsbn, out var book))
        {
            return book;
        }

        return null;
    }

    public async Task<BookResource?> GetBookAutocompleteByIsbnAsync(string isbn)
    {
        var existing = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        if(existing != null)
        {
            return null;
        }

        var olBook = await GetBookByIsbnAsync(isbn);
        await Task.Delay(1000);
        if (olBook == null || olBook.Details == null) return null;

        List<AuthorResource> authors = [];
        List<CategoryResource> categories = [];

            foreach (var author in olBook.Details.Authors)
            {
                var olAuthor = await GetAndSaveAuthorByKeyAsync(author.Key);
                if (olAuthor != null)
                {
                    authors.Add(new AuthorResource
                    {
                        Id = olAuthor.Id,
                        Name = olAuthor.FullName,
                        Biography = olAuthor.Biography,
                        Image = new FileResource
                        {
                            FileName = olAuthor.Searchable?.File?.FileName ?? "",
                        }
                    });
                }
                await Task.Delay(1000);
            }
            foreach (var genre in olBook.Details.Genres)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == genre);
                if (category == null)
                {
                    category = new Category
                    {
                        Name = genre,
                        Searchable = new Searchable
                        {
                            Name = genre,
                            Type = SearchableType.Category,
                            FileId = null,
                        }
                    };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
                categories.Add(new CategoryResource
                {
                    Id = category.Id,
                    Name = category.Name,
                    Image = new FileResource
                    {
                        FileName = category.Searchable?.File?.FileName ?? "",
                    }
                });
        }
        
        var image = await _storageService.SaveFileByUrl(olBook.ThumbnailUrl);


        return new BookResource
        {
            Id = 0,
            ISBN = isbn,
            BookStocks = [],
            Categories = categories,
            Name = olBook.Details.Title,
            Authors = authors,
            Description = olBook.Details.Description,
            Image = new FileResource
            {
                FileName = image.FileName,
            }
        };
    }

    public async Task<Author?> GetAndSaveAuthorByKeyAsync(string authorKey)
    {
        var existingAuthor = await _context.Authors.FirstOrDefaultAsync(a => a.ExternalId == authorKey);
        if (existingAuthor != null) return existingAuthor;

        var url = $"https://openlibrary.org{authorKey}.json";
        
        var olAuthor = await _httpClient.GetFromJsonAsync<OpenLibraryAuthor>(url);
        
        if (olAuthor == null) return null;

        var authorImageFile = olAuthor.Photos.Where(p=>p!=-1).Any() ?  await GetAuthorImageAsync(olAuthor.Photos.First(p => p != -1)) : null;

        var newAuthor = new Author
        {
            FullName = olAuthor.PersonalName,
            ExternalId = authorKey,
            Biography = olAuthor.Bio?.Value,
            Searchable = new Searchable
            {
                Name = olAuthor.PersonalName,
                FileId = authorImageFile?.Id,
                Type = SearchableType.Author
            }
        };

        _context.Authors.Add(newAuthor);
        await _context.SaveChangesAsync();

        return newAuthor;
    }

    private async Task<Models.File?> GetAuthorImageAsync(int id)
    {
        var url = $"https://covers.openlibrary.org/p/id/{id}-M.jpg";

        var image = await _storageService.SaveFileByUrl(url);
        if(image == null) return null;

        Models.File newFile = new ()
        {
            FileName = image.FileName,
            RelativeDirectory = image.Directory
        };
        await _context.Files.AddAsync(newFile);
        await _context.SaveChangesAsync();

        return newFile;
    }

}