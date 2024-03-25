using CommunityToolkit.Maui.Views;

using Mauidible.Domain;

using Microsoft.EntityFrameworkCore;

namespace Mauidible;

[QueryProperty("Item", "Item")]
public partial class Player : ContentPage
{
    public List<Bookmark> Bookmarks { get; set; } = [];

    public AudioBook Item
    {
        get => (AudioBook)BindingContext;
        set => BindingContext = value;
    }

    AppDbContext dbContext;

    private int CurrentChapter = 0;

    public Player (AppDbContext appDbContext)
    {
        InitializeComponent();
        dbContext = appDbContext;
    }

    protected override void OnNavigatedTo (NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var book = Item;

        if (book != null)
        {
            Title = book.Title;

            if (book.Chapters.Count > 0)
            {
                //MediaElement.Stop();
                //MediaElement.Source = FileMediaSource.FromFile(book.Chapters[0].Path);
                //MediaElement.Play();
                //CurrentChapter = 0;

                var chapterIds = book.Chapters.ConvertAll(x => x.Id);

                var bookmarks = dbContext.Bookmarks.Where(x => chapterIds.Contains(x.ChapterId)).AsNoTracking().ToList();

                if (bookmarks.Count > 0)
                {
                    Bookmarks.AddRange(bookmarks);
                    BookmarksView.ItemsSource = null;
                    BookmarksView.ItemsSource = Bookmarks;
                }

                ChaptersView.SelectedItem = book.Chapters[0];
            }
        }
    }

    private void ListView_ItemSelected (System.Object sender, Microsoft.Maui.Controls.SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not Chapter item)
        {
            return;
        }

        var chapterNumber = Item.Chapters.FindIndex(0, x => x.Id == item.Id);

        if (chapterNumber > 0)
        {
            CurrentChapter = chapterNumber;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            MediaElement.Stop();
            MediaElement.Source = FileMediaSource.FromFile(item.Path);
            MediaElement.Play();
        });
    }

    private void MediaElement_MediaEnded (object sender, EventArgs e)
    {
        CurrentChapter++;

        if (CurrentChapter >= Item.Chapters.Count)
        {
            return;
        }

        var chapter = Item.Chapters[CurrentChapter];

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ChaptersView.SelectedItem = chapter;
        });

        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        //    MediaElement.Source = FileMediaSource.FromFile(chapter.Path);
        //    MediaElement.Stop();
        //    MediaElement.Play();
        //});
    }

    private void ContentPage_Unloaded (object sender, EventArgs e)
    {
        MediaElement.Handler?.DisconnectHandler();
    }

    private void AddBookmark_Clicked (object sender, EventArgs e)
    {
        var position = MediaElement.Position.TotalMilliseconds;

        if (position > 0 && ChaptersView.SelectedItem is Chapter chapter)
        {
            var bookmark = new Bookmark
            {
                ChapterId = chapter.Id,
                Description = chapter.Name,
                TimeInMs = position,
                Title = chapter.Name
            };

            dbContext.Bookmarks.Add(bookmark);
            dbContext.SaveChanges();

            Bookmarks.Add(bookmark);

            MainThread.InvokeOnMainThreadAsync(() =>
            {
                BookmarksView.ItemsSource = null;
                BookmarksView.ItemsSource = Bookmarks;
            });
        }
    }

    private void BookmarksView_ItemSelected (object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Bookmark bookmark)
        {
            var chapter = Item.Chapters.First(x => x.Id == bookmark.ChapterId);
            ChaptersView.SelectedItem = chapter;

            MainThread.BeginInvokeOnMainThread(() => MediaElement.SeekTo(TimeSpan.FromMilliseconds(bookmark.TimeInMs)));
        }
    }
}