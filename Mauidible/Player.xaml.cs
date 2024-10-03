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

    readonly AppDbContext dbContext;

    public Player(AppDbContext appDbContext)
    {
        InitializeComponent();
        dbContext = appDbContext;

        bookmarksPanel.IsVisible = false;

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Hide the navigation bar on mobile platforms
            mnGrid.MaximumWidthRequest = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 54;
        }
        else
        {
            // mnGrid.MaximumWidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var book = Item;

        if (book != null)
        {
            Title = book.Title;

            if (book.Chapters.Count > 0)
            {
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

    private void ListView_ItemSelected(System.Object sender, Microsoft.Maui.Controls.SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not Chapter chapter)
        {
            return;
        }

        PlayChapter(chapter);
    }

    void PlayChapter(Chapter chapter)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (MediaElement.Source != null)
            {
                MediaElement.Stop();
                MediaElement.Source = null;
            }

            MediaElement.Source = FileMediaSource.FromFile(chapter.Path);
            MediaElement.Play();
        });
    }

    private void MediaElement_MediaEnded(object sender, EventArgs e)
    {
        if (Item!.Chapters.Count == 1)
        {
            return;
        }

        if (ChaptersView?.SelectedItem is Chapter chapter)
        {
            var chapters = Item!.Chapters.OrderBy(x => x.Id).ToList();
            chapters.RemoveAll(x => x.Id <= chapter.Id);

            if (chapters.Count > 0)
            {
                MainThread.BeginInvokeOnMainThread(() => ChaptersView.SelectedItem = chapters[0]);
            }
        }
    }

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        SaveLastPosition();

        ChaptersView.SelectedItem = null;
        // Stop and cleanup MediaElement when we navigate away
        try
        {
            MediaElement.Handler?.DisconnectHandler();
        }
        catch
        {
            MediaElement.Stop();
            MediaElement.Source = null;
        }
    }

    private void AddBookmark_Clicked(object sender, EventArgs e)
    {
        var position = MediaElement.Position.TotalSeconds;

        if (position > 0 && ChaptersView.SelectedItem is Chapter chapter)
        {
            var bookmark = new Bookmark
            {
                ChapterId = chapter.Id,
                Description = chapter.Name,
                Position = position,
                Title = $"Bookmark_{TimeSpan.FromSeconds(position):hh\\:mm\\:ss}"
            };

            dbContext.Bookmarks.Add(bookmark);
            dbContext.SaveChanges();

            Bookmarks.Add(bookmark);

            MainThread.InvokeOnMainThreadAsync(() =>
            {
                BookmarksView.ItemsSource = null;
                BookmarksView.ItemsSource = Bookmarks;
                bookmarksPanel.IsVisible = true;
            });
        }
    }

    private void BookmarksView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Bookmark bookmark)
        {
            ChaptersView.SelectedItem = Item.Chapters.First(x => x.Id == bookmark.ChapterId);

            BookmarksView.SelectedItem = null;

            MainThread.BeginInvokeOnMainThread(() => MediaElement.SeekTo(TimeSpan.FromSeconds(bookmark.Position)));
        }
    }

    private void MediaSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Math.Abs(e.NewValue - MediaElement.Position.TotalSeconds) > 1) // Avoid seeking for minor changes
            {
                // Seek the media playback to the slider's new value
                MediaElement.SeekTo(TimeSpan.FromSeconds(e.NewValue));
            }
        });
    }

    private void SaveLastPosition()
    {
        if (ChaptersView.SelectedItem is Chapter chapter)
        {
            using var context = new AppDbContext();

            var book = context.AudioBooks.Find(Item!.Id);

            if (book != null)
            {
                book.LastPlayedChapterId = chapter.Id;
                book.LastPlayedPosition = MediaElement.Position.TotalSeconds;

                context.AudioBooks.Update(book);
                context.SaveChanges();
            }
        }
    }

    private void RewindButton_Clicked(object sender, EventArgs e)
    {
        var value = Math.Max(0, MediaElement.Position.TotalSeconds - 30);

        if (value > 0)
        {
            MainThread.BeginInvokeOnMainThread(() => MediaElement.SeekTo(TimeSpan.FromSeconds(value)));
        }
    }

    private void ForwardButton_Clicked(object sender, EventArgs e)
    {
        var value = Math.Min(MediaElement.Duration.TotalSeconds, MediaElement.Position.TotalSeconds + 30);

        if (value > 0)
        {
            MainThread.BeginInvokeOnMainThread(() => MediaElement.SeekTo(TimeSpan.FromSeconds(value)));
        }
    }

    private void CloseImageButton_Clicked(object sender, EventArgs e)
    {
        bookmarksPanel.IsVisible = !bookmarksPanel.IsVisible;
    }
}