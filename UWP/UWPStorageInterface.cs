using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(TimeTracker.UWP.UWPStorageInterface))]

namespace TimeTracker.UWP
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools

    //lots of the stuff in here is legacy code that isn't used in here as I just 100% copied this from a private project of mine I created a long long time ago.
    //YOU REALLY SHOULD NOT TRY TO POKE AROUND IN HERE, AS IT WILL BE CONFUSING AND HORRIBLE CODE. (or as someone once put it:
    //  When I wrote this only god and I knew how this worked; now only god knows.
    class UWPStorageInterface : IStorageInterface
    {
        private static StorageFolder BaseFolder;
        public static StorageFolder LOCAL_FOLDER = ApplicationData.Current.LocalFolder;
        public static StorageFolder ROAMING_FOLDER = ApplicationData.Current.RoamingFolder;
        public const int OTHER = -1;
        public const int LOCAL = 0;
        public const int ROAMING = 1;
        public const int BASE = 2;
        public const int OPEN = 0;
        public const int REPLACE = 1;
        public static String BaseFolderFutureAccessToken { get; set; } = null;


        //Various file pickers------------------------------------------------------------------------------------------------------------------------------------------------------------
        internal async static Task<String> PickExternalStorageFile_NewFile(String suggestedName)
        {
            FileSavePicker fsPicker = new FileSavePicker()
            {
                SuggestedFileName = suggestedName,
                DefaultFileExtension = ".txt",
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            fsPicker.FileTypeChoices.Add("Textdatei", new List<string> { ".txt" });
            fsPicker.FileTypeChoices.Add("Datei", new List<string> { "." });
            StorageFile sf = await fsPicker.PickSaveFileAsync();
            String Token = null;
            if (sf != null)
            {
                Token = StorageApplicationPermissions.FutureAccessList.Add(sf);
            }
            else
            {
                return null;
            }
            return Token;
        }

        internal async static Task<String> PickExternalStorageFile_OpenFile()
        {
            FileOpenPicker foPicker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.Thumbnail
            };
            foPicker.FileTypeFilter.Add("*");
            foPicker.FileTypeFilter.Add(".txt");
            StorageFile sf = await foPicker.PickSingleFileAsync();
            String Token = null;
            if (sf != null)
            {
                Token = StorageApplicationPermissions.FutureAccessList.Add(sf);
            }
            else
            {
                return null;
            }
            return Token;
        }

        internal async static Task<String[]> PickExternalStorageFiles_OpenFiles()
        {
            FileOpenPicker foPicker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.Thumbnail
            };
            foPicker.FileTypeFilter.Add(".csv");
            foPicker.FileTypeFilter.Add(".txt");
            foPicker.FileTypeFilter.Add("*");
            IReadOnlyList<StorageFile> sf = await foPicker.PickMultipleFilesAsync();
            IEnumerator<StorageFile> sfe = sf.GetEnumerator();
            String[] Token = new String[sf.Count];
            for (int i = 0; i < sf.Count; i++)
            {
                sfe.MoveNext();
                StorageFile sfl = sfe.Current;
                if (sfl != null)
                {
                    Token[i] = StorageApplicationPermissions.FutureAccessList.Add(sfl);
                }
                else
                {
                    Token[i] = null;
                }
            }
            return Token;
        }

        internal async static Task<String> PickExternalStorageFolder()
        {
            FolderPicker fPicker = new FolderPicker()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.Thumbnail
            };
            fPicker.FileTypeFilter.Add("*");
            StorageFolder sf = await fPicker.PickSingleFolderAsync();
            String Token = null;
            if (sf != null)
            {
                Token = StorageApplicationPermissions.FutureAccessList.Add(sf);
            }
            else
            {
                return null;
            }
            return Token;
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
            return await ReadBytesFromStorageFile(await GetStorageFileFromToken(await PickExternalStorageFile_OpenFile()));
        }

        public async Task<string> ReadStringPickLocationAsync()
        {
            return await ReadFromStorageFile(await GetStorageFileFromToken(await PickExternalStorageFile_OpenFile()));
        }

        public async Task WriteBytesPickLocationAsync(byte[] content, string name)
        {
            await WriteBytesToStorageFile(await GetStorageFileFromToken(await PickExternalStorageFile_NewFile(name)), content);
        }

        public async Task WriteStringPickLocationAsync(string content, string name)
        {
            String token = await PickExternalStorageFile_NewFile(name);
            if (token == null || token.Equals(""))
            {
                await UserInteraction.ShowDialogAsync("ERROR", "Konnte den vorgang nicht abschließen");
                Debug.WriteLine("maybe add return type to this at some point? (UWPStorageInterface)");
                return;
            }
            await WriteToStorageFile(await GetStorageFileFromToken(token), content);
        }


        //load StorageFiles from internal locations----------------------------------------------------------------------------------------------------------------------
        internal static async Task<StorageFile> GetStorageFileFromToken(String Token)
        {
            if (Token == null)
            {
                return null;
            }
            return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(Token);
        }

        internal static async Task<StorageFolder> GetStorageFolderFromToken(String Token)
        {
            if (Token == null)
            {
                return null;
            }
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token);
        }

        internal async static Task<StorageFile> GetStorageFileFromLocalStorage(string fileName)
        {
            return await NavigateFileSystem(LOCAL, fileName, OPEN);
        }

        internal async static Task<StorageFile> GetStorageFileFromRoamingStorage(string fileName)
        {
            return await NavigateFileSystem(ROAMING, fileName, OPEN);
        }


        //some helper methods for other funtions in here--------------------------------------------------------------------------------------------------------------------
        private static async Task<StorageFile> NavigateFileSystem(int folderType, String targetName, int collisionOption, StorageFolder specialFolder = null)
        {
            Debug.WriteLine("STARTING");
            StorageFolder storageFolder;
            if (folderType == OTHER)
            {
                if (specialFolder == null)
                {
                    storageFolder = KnownFolders.PicturesLibrary;
                    await UserInteraction.ShowDialogAsync("WARNING", "The file could not be saved in the desired Location.\r\nIt will be saved to your PicturesLibrary");
                }
                else
                {
                    storageFolder = specialFolder;

                }
            }
            else if (folderType == BASE)
            {
                storageFolder = BaseFolder;
            }
            else if (folderType == ROAMING)
            {
                storageFolder = ApplicationData.Current.RoamingFolder;
            }
            else
            {
                storageFolder = ApplicationData.Current.LocalFolder;
            }
            targetName = targetName.Replace("\\", "/");
            String[] paths = targetName.Split('/');
            if (paths.Length > 1)
            {
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    storageFolder = await storageFolder.CreateFolderAsync(paths[i], CreationCollisionOption.OpenIfExists);
                }
            }
            if (collisionOption == REPLACE)
            {
                return await storageFolder.CreateFileAsync(paths[paths.Length - 1], CreationCollisionOption.ReplaceExisting);
            }
            else
            {
                return await storageFolder.CreateFileAsync(paths[paths.Length - 1], CreationCollisionOption.OpenIfExists);
            }
        }

        internal async static Task<ImageSource> GetImageSourceFromStorageFile(StorageFile sf)
        {
            using var randomAccessStream = await sf.OpenAsync(FileAccessMode.Read);
            var result = new BitmapImage();
            await result.SetSourceAsync(randomAccessStream);
            return result;
        }

        static async Task<StorageFolder> GetCustomFolder(String type)
        {
            String Token = await ReadFromLocalFolder("Storage." + type + ".token");
            if (Token == null || Token.Equals(""))
            {
                await UserInteraction.ShowDialogAsync("INFORMATION", "You will now be prompted to chose a Folder in which to save the Comic." +
                    "This App will create a new Folder within the Folder you selected, called \"" + type + "\", which will be used to store the images" +
                    "(in order not to confuse them with your files). The App will remember the location you have picked and will use this location until you change it in the Settings.");
                Token = await PickExternalStorageFolder();
                await WriteToLocalFolder("Storage." + type + ".token", Token);
            }
            return await GetStorageFolderFromToken(Token);
        }


        //Various File IO wrappers-----------------------------------------------------------------------------------------------------------------------------------------
        private static async Task LoadBaseFolderAsync()
        {
            String Token = await ReadFromLocalFolder("BaseFolderFutureAccessToken.token");
            if (Token == null || Token.Equals(""))
            {
                Token = await PickExternalStorageFolder();
                if (Token == null || Token.Equals(""))//Full-On FAIL => Just give up and exit, but notify the user
                {
                    await UserInteraction.ShowDialogAsync("ERROR", "Unable to load required resources.\n App will be closed.");
                    ForceTerminateApp();
                }
            }
            BaseFolderFutureAccessToken = Token;
            await WriteToLocalFolder("BaseFolderFutureAccessToken.token", BaseFolderFutureAccessToken);
        }

        internal static async Task CheckBaseFolder()
        {
            if (BaseFolder == null)
            {
                if (BaseFolderFutureAccessToken == null || BaseFolderFutureAccessToken.Equals(""))
                {
                    await LoadBaseFolderAsync();
                }
                BaseFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(BaseFolderFutureAccessToken);
            }
        }

        internal async static Task DeleteFromBaseFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, OPEN);
            await storageFile.DeleteAsync();
        }

        internal static async Task<String> ReadFromBaseFolder(String fileName)
        {
            await CheckBaseFolder();
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, OPEN);
            return await FileIO.ReadTextAsync(storageFile);
        }

        internal static async Task<IBuffer> ReadBufferFromBaseFolder(String fileName)
        {
            await CheckBaseFolder();
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, OPEN);
            return await FileIO.ReadBufferAsync(storageFile);
        }

        internal static async Task<byte[]> ReadBytesFromBaseFolder(String fileName)
        {
            await CheckBaseFolder();
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, OPEN);
            return await storageFile.GetBytes();
        }

        internal async static Task WriteToBaseFolder(String fileName, String content)
        {
            await CheckBaseFolder();
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, REPLACE);
            await FileIO.WriteTextAsync(storageFile, content);
        }

        internal async static Task WriteBufferToBaseFolder(String fileName, IBuffer content)
        {
            await CheckBaseFolder();
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, REPLACE);
            await FileIO.WriteBufferAsync(storageFile, content);
        }

        internal async static Task WriteBytesToBaseFolder(String fileName, byte[] content)
        {
            await CheckBaseFolder();
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, REPLACE);
            await FileIO.WriteBytesAsync(storageFile, content);
        }

        internal async static Task AppendToBaseFolder(String fileName, String content)
        {
            await CheckBaseFolder();
            StorageFile storageFile = await NavigateFileSystem(BASE, fileName, OPEN);
            await FileIO.AppendTextAsync(storageFile, content);
        }


        internal static async Task<String> ReadFromLocalFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, OPEN);
            return await FileIO.ReadTextAsync(storageFile);
        }

        internal static async Task<IBuffer> ReadBufferFromLocalFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, OPEN);
            return await FileIO.ReadBufferAsync(storageFile);
        }

        internal static async Task<byte[]> ReadBytesFromLocalFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, OPEN);
            return await storageFile.GetBytes();
        }

        internal static async Task WriteToLocalFolder(String fileName, String content)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, REPLACE);
            await FileIO.WriteTextAsync(storageFile, content);
        }

        internal static async Task WriteBufferToLocalFolder(String fileName, IBuffer content)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, REPLACE);
            await FileIO.WriteBufferAsync(storageFile, content);
        }

        internal static async Task WriteBytesToLocalFolder(String fileName, byte[] content)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, REPLACE);
            await FileIO.WriteBytesAsync(storageFile, content);
        }

        internal async static Task AppendToLocalFolder(String fileName, String content)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, OPEN);
            await FileIO.AppendTextAsync(storageFile, content);
        }

        internal async static Task DeleteFromLocalFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(LOCAL, fileName, OPEN);
            await storageFile.DeleteAsync();
        }


        internal static async Task<String> ReadFromSpecifiedFolder(String fileName, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            return await FileIO.ReadTextAsync(storageFile);
        }

        internal static async Task<IBuffer> ReadBufferFromSpecifiedFolder(String fileName, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            return await FileIO.ReadBufferAsync(storageFile);
        }

        internal static async Task<byte[]> ReadBytesFromSpecifiedFolder(String fileName, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            return await storageFile.GetBytes();
        }

        internal static async Task WriteToSpecifiedFolder(String fileName, String content, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, REPLACE, storageFolder);
            await FileIO.WriteTextAsync(storageFile, content);
        }

        internal static async Task WriteBufferToSpecifiedFolder(String fileName, IBuffer content, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, REPLACE, storageFolder);
            await FileIO.WriteBufferAsync(storageFile, content);
        }

        internal static async Task WriteBytesToSpecifiedFolder(String fileName, byte[] content, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, REPLACE, storageFolder);
            await FileIO.WriteBytesAsync(storageFile, content);
        }

        internal async static Task AppendToSpecifiedFolder(String fileName, String content, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            await FileIO.AppendTextAsync(storageFile, content);
        }

        internal async static Task DeleteFromSpecifiedFolder(String fileName, StorageFolder storageFolder)
        {
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            await storageFile.DeleteAsync();
        }


        internal static async Task<String> ReadFromKnownFolder(String fileName, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return null;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            return await FileIO.ReadTextAsync(storageFile);
        }

        internal static async Task<IBuffer> ReadBufferFromKnownFolder(String fileName, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return null;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            return await FileIO.ReadBufferAsync(storageFile);
        }

        internal static async Task<byte[]> ReadBytesFromKnownFolder(String fileName, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return null;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            return await storageFile.GetBytes();
        }

        internal static async Task WriteToKnownFolder(String fileName, String content, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, REPLACE, storageFolder);
            await FileIO.WriteTextAsync(storageFile, content);
        }

        internal static async Task WriteBufferToKnownFolder(String fileName, IBuffer content, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, REPLACE, storageFolder);
            await FileIO.WriteBufferAsync(storageFile, content);
        }

        internal static async Task WriteBytesToKnownFolder(String fileName, byte[] content, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, REPLACE, storageFolder);
            await FileIO.WriteBytesAsync(storageFile, content);
        }

        internal async static Task AppendToKnowndFolder(String fileName, String content, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            await FileIO.AppendTextAsync(storageFile, content);
        }

        internal async static Task DeleteFromKnownFolder(String fileName, String FolderToken)
        {
            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(FolderToken);
            if (storageFolder == null)
            {
                return;
            }
            StorageFile storageFile = await NavigateFileSystem(OTHER, fileName, OPEN, storageFolder);
            await storageFile.DeleteAsync();
        }


        internal async static Task<String> ReadFromRoamingFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, OPEN);
            return await FileIO.ReadTextAsync(storageFile);
        }

        internal async static Task<IBuffer> ReadBufferFromRoamingFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, OPEN);
            return await FileIO.ReadBufferAsync(storageFile);
        }

        internal async static Task<byte[]> ReadBytesFromRoamingFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, OPEN);
            return await storageFile.GetBytes();
        }

        internal async static Task WriteToRoamingFolder(String fileName, String content)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, REPLACE);
            await FileIO.WriteTextAsync(storageFile, content);
        }

        internal async static Task WriteBufferToRoamingFolder(String fileName, IBuffer content)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, REPLACE);
            await FileIO.WriteBufferAsync(storageFile, content);
        }

        internal async static Task WriteBytesToRoamingFolder(String fileName, byte[] content)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, REPLACE);
            await FileIO.WriteBytesAsync(storageFile, content);
        }

        internal async static Task AppendToRoamingFolder(String fileName, String content)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, OPEN);
            await FileIO.AppendTextAsync(storageFile, content);
        }

        internal async static Task DeleteFromRoamingFolder(String fileName)
        {
            StorageFile storageFile = await NavigateFileSystem(ROAMING, fileName, OPEN);
            await storageFile.DeleteAsync();
        }


        internal async static Task<String> ReadFromKnownStorageFile(String FileToken)
        {
            StorageFile sf = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
            if (sf == null)
            {
                return null;
            }
            return await ReadFromStorageFile(sf);
        }

        internal async static Task<IBuffer> ReadBufferFromKnownStorageFile(String FileToken)
        {
            StorageFile sf = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
            if (sf == null)
            {
                return null;
            }
            return await ReadBufferFromStorageFile(sf);
        }

        internal async static Task<byte[]> ReadBytesFromKnownStorageFile(String FileToken)
        {
            StorageFile sf = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
            if (sf == null)
            {
                return null;
            }
            return await ReadBytesFromStorageFile(sf);
        }

        internal static async Task WriteToKnownStorageFile(String FileToken, String content)
        {
            StorageFile sf = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
            if (sf == null)
            {
                return;
            }
            await WriteToStorageFile(sf, content);
        }

        internal static async Task WriteBufferToKnownStorageFile(String FileToken, IBuffer content)
        {
            StorageFile sf = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
            if (sf == null)
            {
                return;
            }
            await WriteBufferToStorageFile(sf, content);
        }

        internal static async Task WriteBytesToKnownStorageFile(String FileToken, byte[] content)
        {
            StorageFile sf = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
            if (sf == null)
            {
                return;
            }
            await WriteBytesToStorageFile(sf, content);
        }

        internal static async Task AppendToKnownStorageFile(String FileToken, String content)
        {
            StorageFile sf = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
            if (sf == null)
            {
                return;
            }
            await AppendToStorageFile(sf, content);
        }


        internal static async Task<String> ReadFromStorageFile(StorageFile sf)
        {
            return await FileIO.ReadTextAsync(sf);
        }

        internal static async Task<IBuffer> ReadBufferFromStorageFile(StorageFile sf)
        {
            return await FileIO.ReadBufferAsync(sf);
        }

        internal static async Task<byte[]> ReadBytesFromStorageFile(StorageFile sf)
        {
            return await sf.GetBytes();
        }

        internal async static Task WriteToStorageFile(StorageFile sf, String content)
        {
            await FileIO.WriteTextAsync(sf, content);
        }

        internal async static Task WriteBufferToStorageFile(StorageFile sf, IBuffer content)
        {
            await FileIO.WriteBufferAsync(sf, content);
        }

        internal async static Task WriteBytesToStorageFile(StorageFile sf, byte[] content)
        {
            await FileIO.WriteBytesAsync(sf, content);
        }

        internal async static Task AppendToStorageFile(StorageFile sf, String content)
        {
            await FileIO.AppendTextAsync(sf, content);
        }


        //some File IO which are SPECIFICALLY for Xamarin.Forms PCL
        private async static Task<StorageFile> NavFS2(string path, CreationCollisionOption file_cco = CreationCollisionOption.OpenIfExists, CreationCollisionOption folder_cco = CreationCollisionOption.OpenIfExists)
        {

            /*ApplicationData.Current.SharedLocalFolder;            
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

            StorageFolder folder;
            if (path.StartsWith("documents://"))
            {
                folder = KnownFolders.DocumentsLibrary;
                //SpecialFolder.Current.Documents;
            }
            else if (path.StartsWith("local://"))
            {
                folder = ApplicationData.Current.LocalFolder;
                //SpecialFolder.Current.Local;
            }
            else if (path.StartsWith("roaming://"))
            {
                folder = ApplicationData.Current.RoamingFolder;
                //SpecialFolder.Current.Roaming;
            }
            else if (path.StartsWith("temp://"))
            {
                folder = ApplicationData.Current.TemporaryFolder;
                //SpecialFolder.Current.Temporary;
            }
            else if (path.StartsWith("cache://"))
            {
                folder = ApplicationData.Current.LocalCacheFolder;
                //SpecialFolder.Current.Cache;
            }
            else if (path.StartsWith("pictures://"))
            {
                folder = KnownFolders.PicturesLibrary;
                /*KnownFolders.CameraRoll;//sub-folder
                KnownFolders.SavedPictures;//sub-folder
                SpecialFolder.Current.Pictures;*/
            }
            else if (path.StartsWith("videos://"))
            {
                folder = KnownFolders.VideosLibrary;
                //SpecialFolder.Current.Videos;
            }
            else if (path.StartsWith("music://"))
            {
                folder = KnownFolders.MusicLibrary;
                //SpecialFolder.Current.Music;
            }
            else if (path.StartsWith("garfield://"))
            {
                folder = await GetCustomFolder("Garfield");
            }
            else if (path.StartsWith("xkcd://"))
            {
                folder = await GetCustomFolder("XKCD");
            }
            else
            {
                folder = ApplicationData.Current.LocalFolder;
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
            StorageFile f = await folder.CreateFileAsync(dirs[dirs.Length - 1], file_cco);
            Debug.WriteLine("created/opened file");
            return f;
        }

        public async Task<byte[]> ReadBytesAsync(string path)
        {
            return await (await NavFS2(path)).GetBytes();
        }

        public async Task<string> ReadStringAsync(string path)
        {
            return await ReadFromStorageFile(await NavFS2(path));
        }

        public async Task WriteBytesAsync(string path, byte[] content)
        {
            await WriteBytesToStorageFile(await NavFS2(path), content);
        }

        public async Task WriteStringAsync(string path, string content)
        {
            await WriteToStorageFile(await NavFS2(path), content);
        }

        public async Task DeleteFile(string path)
        {

            await (await NavFS2(path)).DeleteAsync();
        }


        //old remains from experiments or slightly dodgy functions
        //or in case of ForceTerminateApp; this is only for use to prevent damage if I can't see any other option
        internal static void ForceTerminateApp()
        {
            Application.Current.Exit();
        }

        public string TestPlatforms(string message)
        {
            return "Hello from UWP! Your Message was: " + message;
        }
    }
}