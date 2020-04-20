using Windows.UI.Xaml;

namespace TimeTracker.UWP
{
    //EVERYTHING in here has been copied from a personal project of mine from a few years back.
    //copyright Lukas Aldersley (lukas@lukasaldersley.de)
    class CloseApp
    {
        //not to be used in any case other when the only way to prevent damage is through "crashing"
        public void Exit()
        {
            Application.Current.Exit();
        }
    }
}
