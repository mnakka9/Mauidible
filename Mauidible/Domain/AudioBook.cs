using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mauidible.Domain
{
    public class AudioBook
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Title { get; set; }

        public string? Author { get; set; }

        public string? Description { get; set; }

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public List<Chapter> Chapters { get; set; } = [];

        public int? LastPlayedChapterId { get; set; }

        public double LastPlayedPosition { get; set; }

        public string? ImageUrl { get; set; }

        public Uri GetImageUri()
        {
            var url = ImageUrl ?? "https://th.bing.com/th/id/OIP.kvtzPKo4EgFMoA63sF0fuQHaEc";
            return new Uri(url);
        }

        [NotMapped]
        public double ImageMaxWidthRequest { get; set; } = 200;
        [NotMapped]
        public double ImageMaxHeightRequest { get; set; } = 280;

        [NotMapped]
        public double ImageParentMaxWidthRequest { get; set; } = 200;
        [NotMapped]
        public double ImageParentMaxHeightRequest { get; set; } = 300;

        [NotMapped]
        public double ImageParentMinHeightRequest { get; set; } = 250;
    }

    public class Chapter
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Path { get; set; }

        public Guid BookId { get; set; }

        [ForeignKey("BookId")]
        public required AudioBook Book { get; set; }

        public List<Bookmark>? Bookmarks { get; set; } = [];

        [NotMapped]
        public string? ChapterName => Name?[..20];
    }

    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public double Position { get; set; }

        public int ChapterId { get; set; }

        [ForeignKey(nameof(ChapterId))]
        public Chapter? Chapter { get; set; }
    }
}
