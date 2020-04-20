using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace TimeTracker.iOS
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    static class iOSDataTypeExtentions
    {
        //self explanatory...
        public static String ToUTF8String(this byte[] input)
        {
            return Encoding.UTF8.GetString(input);
        }
    }
}