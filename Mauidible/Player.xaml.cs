using CommunityToolkit.Maui.Views;

using Mauidible.Domain;

namespace Mauidible;

[QueryProperty("Item", "Item")]
public partial class Player : ContentPage
{
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

        if (Item?.ImageUrl != null)
        {
            var url = Item.ImageUrl!;

            coverImage.Source = url.Contains("http") ? ImageSource.FromUri(new Uri(url)) : ImageSource.FromFile(url);
        }
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
                MediaElement.Stop();
                MediaElement.Source = FileMediaSource.FromFile(book.Chapters[0].Path);
                MediaElement.Play();
                CurrentChapter = 0;
            }
        }
    }

    private void ListView_ItemSelected (System.Object sender, Microsoft.Maui.Controls.SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not Chapter item)
        {
            return;
        }

        var chapterNumber = Item.Chapters.FindIndex(0, x => x.Id == item.Id); ;

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

        var chapter = Item.Chapters[CurrentChapter];

        MainThread.BeginInvokeOnMainThread(() =>
        {
            MediaElement.Source = FileMediaSource.FromFile(chapter.Path);
            MediaElement.Stop();
            MediaElement.Play();
        });
    }

    private void ContentPage_Unloaded (object sender, EventArgs e)
    {
        MediaElement.Handler?.DisconnectHandler();
    }
}