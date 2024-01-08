using System.ComponentModel.DataAnnotations;

namespace Mauidible.Domain
{
    public class AudioBook
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Title { get; set; }

        public string? Author { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<Chapter> Chapters { get; set; } = [];

        public string? ImageUrl { get; set; }


        public Uri GetImageUri ()
        {
            var url = ImageUrl ?? "https://th.bing.com/th/id/OIP.kvtzPKo4EgFMoA63sF0fuQHaEc"; ;

            return new Uri(url);
        }
    }

    public class Chapter
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }

        public string? Path { get; set; }

        public Guid BookId { get; set; }

        public List<Bookmark>? Bookmarks { get; set;} = [];
    }

    public class Bookmark
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Title { get; set; }

        public string? Description { get; set; }

        public double TimeInMs { get; set; }

        public Guid ChapterId { get; set; }
    }
}
