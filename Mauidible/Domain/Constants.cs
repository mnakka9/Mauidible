namespace Mauidible.Domain;

public static class Constants
{
    public const string DatabaseFilename = "books.db";
    public static string DatabasePath (string dbName)
    {
        if(DeviceInfo.Platform == DevicePlatform.Android)
        {
            var local = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
            var sqlitePath = Path.Combine(local, dbName);
            return sqlitePath;
        }
        else if(DeviceInfo.Platform == DevicePlatform.iOS)
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var sqlitePath = Path.Combine(local, dbName);
            return sqlitePath;
        }

        return Path.Combine(FileSystem.AppDataDirectory, dbName);
    }

    public static string BooksJsonPath => Path.Combine(FileSystem.AppDataDirectory, "Book.json");
    public static string ChapterssJsonPath => Path.Combine(FileSystem.AppDataDirectory, "Chapters.json");
    public static string BookmarksJsonPath => Path.Combine(FileSystem.AppDataDirectory, "Bookmarks.json");
}