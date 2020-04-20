using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TimeTracker
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    public class UserInteraction
    {
        /*
         * possibly use this instead:
         * https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/local-notifications
         */

        public static bool Vibrate(double duration)
        {
            IUserInteraction ui = DependencyService.Get<IUserInteraction>();
            if (ui != null)
            {
                ui.Vibrate(duration);
                return true;
            }
            return false;
        }

        public static bool ShowToast(String content = "Why is there no content?", String title = "AllInOneApp")
        {
            IUserInteraction ui = DependencyService.Get<IUserInteraction>();
            if (ui != null)
            {
                ui.ShowToast(content, title);
                return true;
            }
            return false;
        }

        public async static Task<bool> ShowDialogAsync(String title = "Generic Message", String content = "Generic Content", String buttonText = "OK")
        {
            IUserInteraction ui = DependencyService.Get<IUserInteraction>();
            if (ui != null)
            {
                return await ui.ShowDialogAsync(title, content);
            }
            return false;
        }
    }
}
