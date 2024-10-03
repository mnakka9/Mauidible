using ATL;
using Mauidible.Domain;

namespace Mauidible.Services;

public interface IMetadataService
{
    Task<AudioBook> FillMetadataAndGetBook(FileInfo fileInfo, string? imagePath);
}

public class MetadataService : IMetadataService
{
    public async Task<AudioBook> FillMetadataAndGetBook(FileInfo fileInfo, string? imagePath)
    {
        Track track = new(fileInfo.FullName);

        string bookName = (track.Album, track.Title) switch
        {
            (var album, _) when !string.IsNullOrEmpty(album) => album,
            (_, var title) when !string.IsNullOrEmpty(title) => title,
            _ => fileInfo.Name
        };

        var book = new AudioBook()
        {
            Title = bookName,
            Author = track.Artist,
            Description = track.Comment,
            LastUpdatedDate = DateTime.Now,
            ImageUrl = imagePath,
        };

        if (book.ImageUrl is null or { Length: 0 })
        {
            var pictures = track.EmbeddedPictures;

            if (pictures.Count > 0)
            {
                string? imageExtension = pictures[0].MimeType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    _ => null // Default case if MIME type doesn't match known types
                };

                if (!string.IsNullOrEmpty(imageExtension))
                {
                    string imageFileName = $"{bookName}{imageExtension}";
                    string imageFilePath = Path.Combine(fileInfo.Directory!.FullName, imageFileName);

                    await File.WriteAllBytesAsync(imageFilePath, pictures[0].PictureData);

                    book.ImageUrl = imageFilePath;
                }
            }
        }

        return book;
    }
}
