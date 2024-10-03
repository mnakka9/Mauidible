using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using CommunityToolkit.Maui.Storage;

using Mauidible.Domain;
using Mauidible.Services;
using Microsoft.EntityFrameworkCore;

namespace Mauidible
{
    public partial class MainPage : ContentPage
    {
        private readonly AppDbContext _context;
        private readonly IMetadataService metadataService;
        public ObservableCollection<AudioBook> Books { get; set; }
        public MainPage(AppDbContext context, IMetadataService metadata)
        {
            InitializeComponent();
            context ??= new AppDbContext();
            _context = context;
            metadataService = metadata;
            Books = [];

            try
            {
                LoadBooks();
            }
            catch (Exception ex)
            {
                _ = Shell.Current.DisplayAlert("Error", ex.Message, "Close").ConfigureAwait(false);
            }

            BindingContext = this;
        }

        private async void ButtonAddNewBook_Clicked(object sender, EventArgs e)
        {
            var result = await FolderPicker.Default.PickAsync(default);

            List<string> audioExtensions = [.. ExtensionsList.AudioExtensions];
            List<string> imageExtensions = [.. ExtensionsList.ImageExtensions];

            if (result.IsSuccessful)
            {
                var folderFiles = Directory.GetFiles(result.Folder.Path, "*");

                var allFiles = folderFiles.Where(x => audioExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();

                if (allFiles.Count < 1)
                {
                    await Toast.Make($"Folder {result.Folder.Name} doesn't have any audio files", ToastDuration.Short).Show();
                    return;
                }

                var firstFile = allFiles[0];
                var imagePath = folderFiles.FirstOrDefault(x => imageExtensions.Contains(Path.GetExtension(x).ToLower()));

                var book = await metadataService.FillMetadataAndGetBook(new FileInfo(firstFile), imagePath);

                book.ImageUrl = imagePath;
#if WINDOWS
                var chapters = allFiles.ConvertAll(x => new Chapter
                {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Path = x,
                    Book = book
                });

                chapters = [.. chapters.OrderBy(x => x.Name)];

                book.Chapters = [.. chapters];
#endif
                //#if ANDROID
#if ANDROID
                var tempChapters = allFiles.ConvertAll(x => new
                {
                    Path = x,
                    Name = x.Split('/').LastOrDefault() ?? string.Empty,

                });

                try
                {
                    var chapters = tempChapters.ConvertAll(x => new Chapter
                    {
                        Name = x.Name.Split('.').FirstOrDefault(),
                        Path = x.Path,
                        Book = book
                    });

                    chapters = [..chapters.OrderBy(x => x.Name)];

                    book.Chapters = [.. chapters];
                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                    await Toast.Make("Error in adding book!", ToastDuration.Short).Show();
                    return;
                }
#endif

                _context.AudioBooks.Add(book);
                _context.SaveChanges();

                await Toast.Make("Book added!", ToastDuration.Short).Show();

                LoadBooks();
            }
            else
            {
                await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(default);
            }
        }

        private void LoadBooks()
        {
            var books = _context.AudioBooks.Include(x => x.Chapters).AsNoTracking().ToList();

            books = [.. books.OrderByDescending(x => x.LastUpdatedDate)];

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Books.Clear();
                books.ForEach(book =>
                {
                    book.ImageUrl ??= "https://th.bing.com/th/id/OIP.kvtzPKo4EgFMoA63sF0fuQHaEc";
                    Books.Add(book);
                });

                if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    // Hide the navigation bar on mobile platforms
                    mainLayout.MaximumWidthRequest = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 54;

                    foreach (var book in books)
                    {
                        book.ImageParentMaxHeightRequest = 250;
                        book.ImageParentMaxWidthRequest = 100;
                        book.ImageParentMinHeightRequest = 180;

                        book.ImageMaxWidthRequest = 100;
                        book.ImageMaxHeightRequest = 180;
                    }
                }
                else
                {
                    // mnGrid.MaximumWidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
                }
            });
        }

        private async void PlayBook_Clicked(System.Object sender, System.EventArgs e)
        {
            var button = sender as ImageButton;

            if (button!.BindingContext is not AudioBook item)
                return;

            await Shell.Current.GoToAsync(nameof(Player), true, new Dictionary<string, object>
            {
                ["Item"] = item
            });
        }
    }
}
