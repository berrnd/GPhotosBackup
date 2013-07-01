using System;
using System.Drawing;
using System.IO;
using Google.GData.Photos;
using GPhotosBackup.Analytics;

namespace GPhotosBackup
{
    internal class Program
    {
        internal static Arguments Arguments;

        private static void Main(string[] args)
        {
            Heartbeat.Beat();

            Arguments = Arguments.Parse(args);

            DateTime start = DateTime.Now;
            try
            {
                if (string.IsNullOrWhiteSpace(Arguments.Username))
                    throw new ArgumentException("Username is empty");

                if (string.IsNullOrWhiteSpace(Arguments.Password))
                    throw new ArgumentException("Password is empty");

                //Login
                Log("Logging in...");
                PicasaService picasaService = new PicasaService("GPhotosBackup"); ;
                picasaService.setUserCredentials(Arguments.Username, Arguments.Password);

                //Query all albums
                Log("Query album list...");
                AlbumQuery albumsQuery = new AlbumQuery(PicasaQuery.CreatePicasaUri("default"));
                albumsQuery.ExtraParameters = "imgmax=d";
                PicasaFeed albumsFeed = picasaService.Query(albumsQuery);

                //Loop through all albums
                int albumCount = 0;
                int pictureCount = 0;
                long totalSize = 0;
                foreach (PicasaEntry album in albumsFeed.Entries)
                {
                    string albumTitle = album.Title.Text;
                    string albumPath = Path.Combine(Arguments.Destination, albumTitle);

                    Directory.CreateDirectory(albumPath);

                    AlbumAccessor albumAccessor = new AlbumAccessor(album);
                    uint albumPictureCount = albumAccessor.NumPhotos;

                    Log(string.Format("Found album \"{0}\" with {1} pictures", albumTitle, albumPictureCount));

                    PhotoQuery picturesQuery = new PhotoQuery(PicasaQuery.CreatePicasaUri("default", albumAccessor.Id));
                    picturesQuery.ExtraParameters = "imgmax=d";
                    PicasaFeed picturesFeed = picasaService.Query(picturesQuery);

                    foreach (PicasaEntry picture in picturesFeed.Entries)
                    {
                        string pictureTitle = picture.Title.Text;
                        string picturePath = Path.Combine(albumPath, pictureTitle + ".jpg");

                        Log(string.Format("Downloading picture {0} to {1}", pictureTitle, picturePath), true);

                        using (Stream pictureStream = picasaService.Query(new Uri(picture.Media.Content.Url)))
                        {
                            using (Bitmap pictureFile = new Bitmap(pictureStream))
                            {
                                pictureFile.Save(picturePath);
                            }
                            pictureStream.Close();
                        }

                        totalSize += new FileInfo(picturePath).Length;
                        pictureCount++;
                        if (pictureCount > 5)
                            break;
                    }
                    albumCount++;
                    if (pictureCount > 5)
                        break;
                }

                DateTime end = DateTime.Now;
                TimeSpan duration = end - start;

                Log(string.Format("Downloaded {0} albums with {1} pictures in {2}", albumCount, pictureCount, string.Format("{0:hh\\:mm\\:ss}", duration)));
                Log(string.Format("Downloaded size: {0}", FormatBytes(totalSize)));

                WaitForClose();
            }
            catch (Exception ex)
            {
                LogError(ex);
                WaitForClose();
            }
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
        }

        private static void Log(string text, bool rewriteLine)
        {
            if (rewriteLine)
                Console.Write("\r"); 
            Log(text);
        }

        private static void LogError(string text)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = currentColor;
        }

        private static void LogError(Exception exception)
        {
            LogError(exception.GetType().Name);
            LogError(exception.Message);
        }

        private static void WaitForClose()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };

            int i = 0;
            double bytesDbl = bytes;
            if (bytes > 1024)
                for (i = 0; (bytes / 1024) > 0; i++, bytes /= 1024)
                    bytesDbl = bytes / 1024.0;

            return string.Format("{0:0.##} {1}", bytesDbl, suffix[i]);
        }
    }
}
