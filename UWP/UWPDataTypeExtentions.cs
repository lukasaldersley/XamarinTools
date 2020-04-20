using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TimeTracker.UWP
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    static class UWPDataTypeExtentions
    {
        //there is no convenient way of reading bytes from a storageFile, so I created my own one.
        internal static async Task<byte[]> GetBytes(this StorageFile input)
        {
            using IRandomAccessStreamWithContentType stream = await input.OpenReadAsync();
            byte[] arr = new byte[stream.Size];
            using DataReader reader = new DataReader(stream);
            await reader.LoadAsync((uint)arr.Length);
            reader.ReadBytes(arr);
            return arr;
        }
    }
}
