using PCLStorage;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(TimeTracker.iOS.iOSStorageInterface))]
namespace TimeTracker.iOS
{

    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    class iOSStorageInterface :IStorageInterface
    {
        /*Probably no cross platform equivalent
        ApplicationData.Current.SharedLocalFolder;            
        ApplicationData.Current.RoamingSettings;
        ApplicationData.Current.LocalSettings;
        KnownFolders.AppCaptures;
        KnownFolders.HomeGroup;
        KnownFolders.MediaServerDevices;
        KnownFolders.Objects3D;
        KnownFolders.Playlists;
        KnownFolders.RecordedCalls;
        KnownFolders.RemovableDevices;

        SpecialFolder.Current.App;*/


        private async static Task<IFile> NavFS2(string path, CreationCollisionOption file_cco = CreationCollisionOption.OpenIfExists, CreationCollisionOption folder_cco = CreationCollisionOption.OpenIfExists)
        {
            IFolder folder;
            if (path.StartsWith("documents://"))
            {
                //folder = KnownFolders.DocumentsLibrary;
                folder = SpecialFolder.Current.Documents;
            }
            else if (path.StartsWith("local://") || path.StartsWith("special-local://"))
            {
                //folder = ApplicationData.Current.LocalFolder;
                folder = SpecialFolder.Current.Local;
            }
            else if (path.StartsWith("roaming://"))
            {
                //folder = ApplicationData.Current.RoamingFolder;
                folder = SpecialFolder.Current.Roaming;
            }
            else if (path.StartsWith("temp://"))
            {
                //folder = ApplicationData.Current.TemporaryFolder;
                //folder = SpecialFolder.Current.Temporary;
                Debug.WriteLine("Since Android can't do Temporary Files I'll just redirect this to cache");
                folder = SpecialFolder.Current.Cache;
            }
            else if (path.StartsWith("cache://"))
            {
                //folder = ApplicationData.Current.LocalCacheFolder;
                folder = SpecialFolder.Current.Cache;
            }
            else if (path.StartsWith("pictures://"))
            {
                //folder = KnownFolders.PicturesLibrary;
                //KnownFolders.CameraRoll;//sub-folder
                //KnownFolders.SavedPictures;//sub-folder
                folder = SpecialFolder.Current.Pictures;
            }
            else if (path.StartsWith("videos://"))
            {
                //folder = KnownFolders.VideosLibrary;
                folder = SpecialFolder.Current.Videos;
            }
            else if (path.StartsWith("music://"))
            {
                //folder = KnownFolders.MusicLibrary;
                folder = SpecialFolder.Current.Music;
            }
            else if (path.StartsWith("garfield://"))
            {
                //folder = await GetCustomFolder("Garfield");
                folder = SpecialFolder.Current.Pictures;
                Debug.WriteLine("I can't be bothered to let the user pick a desired folder on Android, therefore I just assign the Pictures Folder");
            }
            else if (path.StartsWith("xkcd://"))
            {
                //folder = await GetCustomFolder("XKCD");
                folder = SpecialFolder.Current.Pictures;
                Debug.WriteLine("I can't be bothered to let the user pick a desired folder on Android, therefore I just assign the Pictures Folder");
            }
            else
            {
                //folder = ApplicationData.Current.LocalFolder;
                folder = SpecialFolder.Current.Local;
                _ = UserInteraction.ShowDialogAsync("WARNUNG", "Es gab ein Problem auf den gewünschten Speicherort zuzugreifen. Der angeforderte Pfad war: \"" + path + "\". Der Pfad wurde Umgeleitet in den Lokalen Ordner.");
            }
            Debug.WriteLine("Selected appropriate folder");
            path = path.Split("://")[1];
            path = path.Replace("\\", "/");
            Debug.WriteLine(path);
            String[] dirs = path.Split("/");
            Debug.WriteLine(dirs.Length - 1);
            for (int i = 0; i < dirs.Length - 1; i++)
            {
                Debug.WriteLine("Create/open folder " + dirs[i]);
                folder = await folder.CreateFolderAsync(dirs[i], folder_cco);
                Debug.WriteLine("created/opened folder " + dirs[i]);
            }
            Debug.WriteLine("structural traverse completed");
            Debug.WriteLine("folder is null?: " + (folder == null ? "true" : "false"));
            Debug.WriteLine("folder: " + folder.ToString());
            Debug.WriteLine("dirs[dirs.length-1]: " + dirs[dirs.Length - 1]);
            Debug.WriteLine("cco: " + file_cco);
            IFile f = await folder.CreateFileAsync(dirs[dirs.Length - 1], file_cco);
            Debug.WriteLine("created/opened file");
            return f;
        }

        public async Task<String> ReadStringAsync(String path)
        {
            IFile file = await NavFS2(path);
            return await file.ReadAllTextAsync();
        }

        public async Task<byte[]> ReadBytesAsync(String path)
        {
            IFile file = await NavFS2(path);
            return await file.ReadAllBytesAsync();
        }


        public async Task WriteStringAsync(String path, String content)
        {
            IFile file = await NavFS2(path);
            await file.WriteAllTextAsync(content);
        }

        public async Task WriteBytesAsync(String path, byte[] content)
        {
            Debug.WriteLine("In Writing Method");
            IFile file = await NavFS2(path);
            Debug.WriteLine("Navigated FS");
            await file.WriteAllBytesAsync(content);
            Debug.WriteLine("wrote everything");
        }


        public async Task AppendStringAsync(String path, String content)
        {
            IFile file = await NavFS2(path);
            await file.WriteAllTextAsync(await file.ReadAllTextAsync() + content);
        }

        public bool AppendBytes(String path, byte[] content)
        {
            return false;
        }

        public async Task DeleteFile(string path)
        {
            await (await NavFS2(path)).DeleteAsync();
        }

        public async Task WriteBytesPickLocationAsync(byte[] content, string name)
        {
            Debug.WriteLine("SORRY! Android and ios don't support this, so I'll just show the share dialog");
            await ShareFileBytes(content, name);
        }

        public async Task WriteStringPickLocationAsync(string content, string name)
        {
            Debug.WriteLine("SORRY! Android and ios don't support this, so I'll just show the share dialog");
            await ShareFileString(content, name);
        }

        //took from https://devblogs.microsoft.com/xamarin/sharing-files-attachments-xamarin-essentials/
        //this is actually fine to call from PCL, but for consistency I'll call these through dependencies aswell
        public async Task ShareFileString(string content, string name)
        {
            String file = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, name);
            File.WriteAllText(file, content);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = name,//vielleicht auch etwas wie "Time Export" oder so...
                File = new ShareFile(file)
            });
        }

        //adapted from https://devblogs.microsoft.com/xamarin/sharing-files-attachments-xamarin-essentials/
        //this is actually fine to call from PCL, but for consistency I'll call these through dependencies aswell
        public async Task ShareFileBytes(byte[] content, string name)
        {
            String file = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory, name);
            File.WriteAllBytes(file, content);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = name,//vielleicht auch etwas wie "Time Export" oder so...
                File = new ShareFile(file)
            });
        }

        public async Task<byte[]> ReadBytesPickLocationAsync()
        {
            Debug.WriteLine("possibly not working -- untested");
            return (await Plugin.FilePicker.CrossFilePicker.Current.PickFile()).DataArray;
        }

        public async Task<string> ReadStringPickLocationAsync()
        {
            Debug.WriteLine("possibly not working -- untested");
            return (await Plugin.FilePicker.CrossFilePicker.Current.PickFile()).DataArray.ToUTF8String();
        }

        public string TestPlatforms(string message)
        {
            return "Hello from Android! Your Message was: " + message;
        }
    }
}