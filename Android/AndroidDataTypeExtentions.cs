using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TimeTracker.Droid
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    static class AndroidDataTypeExtentions
    {
        //self explanatory...
        public static String ToUTF8String(this byte[] input)
        {
            return Encoding.UTF8.GetString(input);
        }
    }
}