using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TimeTracker
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    class StorageInterface
    {
        internal static String TestPlatforms()
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                return si.TestPlatforms("DOES THIS WORK?");
            }
            return "SORRY, This didn't work at all";
        }

        internal async static Task<bool> DeleteFile(string path)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                await si.DeleteFile(path);
                return true;
            }
            return false;
        }

        internal async static Task<bool> WriteBytesAsync(string path, byte[] content)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                await si.WriteBytesAsync(path, content);
                return true;
            }
            //return "SORRY, This didn't work at all";
            return false;
        }

        internal async static Task<bool> WriteStringAsync(string path, string content)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                await si.WriteStringAsync(path, content);
                return true;
            }
            //return "SORRY, This didn't work at all";
            return false;
        }

        internal async static Task<string> ReadStringAsync(string path)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                return await si.ReadStringAsync(path);
            }
            return "SORRY, This didn't work at all";
        }

        internal async static Task<byte[]> ReadBytesAsync(string path)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                return await si.ReadBytesAsync(path);
            }
            return "SORRY, This didn't work at all".ToByteArray();
        }


        internal async static Task<bool> WriteBytesPickLocationAsync(byte[] content, string name)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                await si.WriteBytesPickLocationAsync(content, name);
                return true;
            }
            return false;
        }

        internal async static Task<bool> WriteStringPickLocationAsync(string content, string name)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                await si.WriteStringPickLocationAsync(content, name);
                return true;
            }
            return false;
        }

        internal async static Task<byte[]> ReadBytesPickLocationAsync()
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                return await si.ReadBytesPickLocationAsync();
            }
            return "SORRY, This didn't work at all".ToByteArray();
        }

        internal async static Task<string> ReadStringPickLocationAsync()
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                return await si.ReadStringPickLocationAsync();
            }
            return "SORRY, This didn't work at all";
        }

        internal async static Task<bool> ShareFileBytes(byte[] content, string name)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                await si.ShareFileBytes(content, name);
                return true;
            }
            return false;
        }

        internal async static Task<bool> ShareFileString(string content, string name)
        {
            IStorageInterface si = DependencyService.Get<IStorageInterface>();
            if (si != null)
            {
                await si.ShareFileString(content, name);
                return true;
            }
            return false;
        }
    }
}