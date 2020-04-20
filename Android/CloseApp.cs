using Android.App;
using Xamarin.Forms;

namespace TimeTracker.Droid
{
    class CloseApp
    {
        //this code is taken from https://github.com/lukasaldersley/XamarinTools
        //this is only to be used if I can't really see any other option to prevent damage
        public void Exit()
        {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            var activity = (Activity)Forms.Context;
#pragma warning restore CS0618 // Typ oder Element ist veraltet
            activity.FinishAffinity();
        }
    }
}