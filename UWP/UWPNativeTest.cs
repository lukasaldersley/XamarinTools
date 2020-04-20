using System;
using Windows.System;

[assembly: Xamarin.Forms.Dependency(typeof(TimeTracker.UWP.UWPNativeTest))]

namespace TimeTracker.UWP
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    class UWPNativeTest : INativeTest
    {
        public async void OpenLocalFolderInWinExplorer()
        {
            await Launcher.LaunchFolderAsync(UWPStorageInterface.LOCAL_FOLDER);
        }
    }
}
