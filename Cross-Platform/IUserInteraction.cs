using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    public interface IUserInteraction
    {
        void Vibrate(double duration);
        void ShowToast(String content = "Why is there no content?", String title = "TimeTracker");
        Task<bool> ShowDialogAsync(String title = "Generic Message", String content = "Generic Content", String buttonText = "OK");
    }
}
