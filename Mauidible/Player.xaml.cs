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

	public Player(AppDbContext appDbContext)
	{
		InitializeComponent();
		dbContext = appDbContext;
	}

    protected override void OnNavigatedTo (NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var book = Item;

        if(book != null)
        {
            Title = book.Title;

            if(book.Chapters.Count > 0)
            {
                MediaElement.Stop();
                MediaElement.Source = FileMediaSource.FromFile(book.Chapters[0].Path);
                MediaElement.Play();
            }
        }
    }

    private void ListView_ItemSelected (System.Object sender, Microsoft.Maui.Controls.SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not Chapter item)
        {
            return;
        }

        MediaElement.Stop();
        MediaElement.Source = FileMediaSource.FromFile(item.Path);
		MediaElement.Play();
    }
}