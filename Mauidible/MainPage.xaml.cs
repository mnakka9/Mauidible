using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using CommunityToolkit.Maui.Storage;

using Mauidible.Domain;

using Microsoft.EntityFrameworkCore;

namespace Mauidible
{
    public partial class MainPage : ContentPage
    {
        private readonly AppDbContext _context;
        public ObservableCollection<AudioBook> Books { get; set; }
        public MainPage (AppDbContext context)
        {
            InitializeComponent();
            context ??= new AppDbContext();
            _context = context;
            Books = [];

            try
            {
                //AddBooks();

                LoadBooks();
            }
            catch (Exception ex)
            {
                var b = ex.Message;
            }

            BindingContext = this;
        }

        private async void ButtonAddNewBook_Clicked (object sender, EventArgs e)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var result = await FolderPicker.Default.PickAsync(default);
#pragma warning restore CA1416 // Validate platform compatibility
            
            List<string> audioExtensions = [".mp3", ".m4a", ".m4b"];
            List<string> imageExtensions = [".jpg", ".png"];

            if (result.IsSuccessful)
            {
                var folderFiles = Directory.GetFiles(result.Folder.Path, "*");

                var allFiles = folderFiles.Where(x => audioExtensions.Contains(Path.GetExtension(x).ToLower())).ToList();

                if (allFiles.Count < 1)
                {
                    await Toast.Make($"Folder {result.Folder.Name} doesn't have any audio files", ToastDuration.Short).Show();
                    return;
                }

                var book = new AudioBook()
                {
                    Title = result.Folder.Name,
                    CreatedDate = DateTime.Now
                };

                var imagePath = folderFiles.Where(x => imageExtensions.Contains(Path.GetExtension(x).ToLower())).FirstOrDefault() ?? "https://th.bing.com/th/id/OIP.kvtzPKo4EgFMoA63sF0fuQHaEc";

                book.ImageUrl = imagePath;
#if WINDOWS
                var chapters = allFiles.Select(x => new Chapter
                {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Path = x,
                }).ToList();

                book.Chapters = [.. chapters];
#endif
//#if ANDROID
#if ANDROID
                var tempChapters = allFiles.Select(x => new
                {
                    Path = x,
                    Name = x.Split('/').LastOrDefault() ?? string.Empty,

                }).ToList();

                try
                {
                    var chapters = tempChapters.Select(x => new Chapter
                    {
                        Name = x.Name.Split('.').FirstOrDefault(),
                        Path = x.Path,
                    }).ToList();
                    book.Chapters = [.. chapters];

                }
                catch(Exception ex)
                {
                    var x = ex.Message; 
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

        private void LoadBooks ()
        {
            var books = _context.AudioBooks.Include(x => x.Chapters).AsNoTracking().ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Books.Clear();
                books.ForEach(book =>
                {
                    book.ImageUrl ??= "https://th.bing.com/th/id/OIP.kvtzPKo4EgFMoA63sF0fuQHaEc";
                    Books.Add(book);
                });
            }); 
        }

        private async void PlayBook_Clicked (System.Object sender, System.EventArgs e)
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
