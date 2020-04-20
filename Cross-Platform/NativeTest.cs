using Xamarin.Forms;

namespace TimeTracker
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    class NativeTest
    {
        public static bool OpenLocalFolderInWinExplorer()
        {
            INativeTest nt = DependencyService.Get<INativeTest>();
            if (nt != null)
            {
                nt.OpenLocalFolderInWinExplorer();
                return true;
            }
            return false;
        }
    }
}
