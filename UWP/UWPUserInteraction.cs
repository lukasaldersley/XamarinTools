using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

[assembly: Xamarin.Forms.Dependency(typeof(TimeTracker.UWP.UWPUserInteraction))]

namespace TimeTracker.UWP
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    class UWPUserInteraction : IUserInteraction
    {
        //this was an experiment, but it didn't work. It would be to much effort to remove this from everywhere it micht possibly be called from
        public void Vibrate(double duration)
        {
            //VibrationDevice.GetDefault().Vibrate(TimeSpan.FromMilliseconds(duration));
        }

        //this creates a super basic UWP notification
        public void ShowToast(String content = "Why is there no content?", String title = "AllInOneApp")
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText(){
                                Text = title
                            },

                            new AdaptiveText(){
                                Text = content
                            }
                        }

                    }
                }
            };
            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        //this creates a popup dialog (most appropriate for critical error messages or really important instructions on how to proceed)
        public async Task<bool> ShowDialogAsync(String title = "Generic Message", String content = "Generic Content", String buttonText = "OK")
        {
            ContentDialog ErrorDialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                PrimaryButtonText = buttonText
            };
            //ErrorDialog.PrimaryButtonClick += MethodToExecOnClick;
            await ErrorDialog.ShowAsync();
            Debug.WriteLine("I'm in UWPUserInteraction.cs and am just always returning 'true'; maybe fix this someday?");
            return true;
        }
    }
}