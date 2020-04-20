using PCLStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    static class DataTypeExtentions
    {
        public static int ToIntCeil(this Double inpt)
        {
            int x = (int)(inpt);
            if (inpt - (double)x == 0.0)
            {
                return x;
            }
            else
            {
                return x + 1;
            }
        }

        public static String ToAsciiString(this byte[] input, bool respectAsciiControlChars = false)
        {

            String result = "";
            if (respectAsciiControlChars)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] <= 0x7f)
                    {
                        result += (char)(input[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] >= 0x20 && input[i] <= 0x7e)
                    {
                        result += (char)(input[i]);
                    }
                }
            }
            return result;
        }

        public static String GetHexString(this String input)
        {
            return "\\x" + BitConverter.ToString(input.ToByteArray()).Replace("-", "\\x");
        }

        public static String[] DivideToLength(this String X, int length)
        {
            int t = (int)Math.Ceiling((double)X.Length / length);
            Debug.WriteLine(X.Length);
            Debug.WriteLine(length);
            Debug.WriteLine(t);
            String[] O = new String[t];
            for (int i = 0; i < t; i++)
            {
                /*int e = ((i + 1) * length) - 1;
                if (e >= X.Length)
                {
                    e = X.Length - 1;
                }*/
                if (i == t - 1)
                {
                    O[i] = X.Substring(i * length);
                }
                else
                {
                    O[i] = X.Substring(i * length, length);
                }
            }
            return O;
        }

        public static byte[] GetByteQuartetFromInt(this int length)
        {
            byte[] result = { 0, 0, 0, 0 };
            int temporaryCopyOfLength = length;
            temporaryCopyOfLength >>= 24;
            result[0] = (byte)temporaryCopyOfLength;
            length -= (temporaryCopyOfLength << 24);

            temporaryCopyOfLength = length;
            temporaryCopyOfLength >>= 16;
            result[1] = (byte)temporaryCopyOfLength;
            length -= (temporaryCopyOfLength << 16);

            temporaryCopyOfLength = length;
            temporaryCopyOfLength >>= 8;
            result[2] = (byte)temporaryCopyOfLength;
            length -= (temporaryCopyOfLength << 8);

            temporaryCopyOfLength = length;
            result[3] = (byte)temporaryCopyOfLength;
            return result;
        }

        /// <summary>
        /// this method takes an array of three bytes and converts them into an integer.
        /// I need to do this because I can only Send/Receive byte arrays but want to transmit the length of my message beforehand to prevent buffer overruns
        /// </summary>
        /// <param name="quartet"></param>
        /// <returns></returns>
        public static int GetIntFromByteQuartet(this byte[] quartet)
        {
            int len = quartet[0];
            len <<= 8;
            len += quartet[1];
            len <<= 8;
            len += quartet[2];
            len <<= 8;
            len += quartet[3];
            return len;
        }

        public static void PrintStackTrace(this Exception e)
        {
            Debug.WriteLine("An Exception occurred");
            Debug.WriteLine(e.Data);
            Debug.WriteLine(e.HelpLink);
            Debug.WriteLine(e.HResult);
            Debug.WriteLine(e.InnerException);
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.Source);
            Debug.WriteLine(e.StackTrace);
            Debug.WriteLine(e.ToString());
        }

        public static byte[] ToUTF8ByteArray(this String input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static String ToUTF8String(this byte[] input)
        {
            return Encoding.UTF8.GetString(input);
        }

        public static String[] Split(this String toSplit, String splitWith)
        {
            return toSplit.Split(new string[] { splitWith }, StringSplitOptions.RemoveEmptyEntries);
        }

        /*public async static Task<StorageFile[]> GetStorageFileArray(this StorageFolder inputFolder)
        {
            IReadOnlyList<StorageFile> fileList = await inputFolder.GetFilesAsync();
            IEnumerator<StorageFile> fileEnumerator = fileList.GetEnumerator();
            StorageFile[] outputArray = new StorageFile[fileList.Count];
            for (int i = 0; i < outputArray.Length; i++)
            {
                fileEnumerator.MoveNext();
                outputArray[i] = fileEnumerator.Current;
            }
            return outputArray;
        }

        public async static Task<StorageFolder[]> GetStorageFolderArray(this StorageFolder inputFolder)
        {
            IReadOnlyList<StorageFolder> folderList = await inputFolder.GetFoldersAsync();
            IEnumerator<StorageFolder> folderEnumerator = folderList.GetEnumerator();
            StorageFolder[] outputArray = new StorageFolder[folderList.Count];
            for (int i = 0; i < outputArray.Length; i++)
            {
                folderEnumerator.MoveNext();
                outputArray[i] = folderEnumerator.Current;
            }
            return outputArray;
        }

        public async static Task CopyFolderContentsTo(this StorageFolder source, StorageFolder target)
        {
            StorageFolder[] folders = await GetStorageFolderArray(source);
            for (int i = 0; i < folders.Length; i++)
            {
                await folders[i].CopyFolderContentsTo(await target.CreateFolderAsync(folders[i].Name, CreationCollisionOption.OpenIfExists));
            }
            StorageFile[] files = await GetStorageFileArray(source);
            for (int i = 0; i < files.Length; i++)
            {
                await files[i].CopyAsync(target, files[i].Name, NameCollisionOption.ReplaceExisting);
            }
        }*/

        public static String ToString(this byte[] input)
        {
            String result = "";
            for (int i = 0; i < input.Length; i++)
            {
                result += (char)(input[i]);
            }
            return result;
        }

        public static char[] ToCharArray(this byte[] input)
        {
            char[] output = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (char)(input[i]);
            }
            return output;
        }

        public static String ToString(this char[] input)
        {
            String result = "";
            for (int i = 0; i < input.Length; i++)
            {
                result += input[i];
            }
            return result;
        }

        public static byte[] ToByteArray(this char[] input)
        {
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(input[i]);
            }
            return output;
        }

        public static char[] ToCharArray(this String input)
        {
            return input.ToArray();
        }

        /// <summary>
        /// This method takes a String and returns the bytes the String is made of as an array.
        /// This is probably horribly inefficient, but I don't really care for now.
        /// </summary>
        /// <param name="input">String to be converted to a byte array</param>
        /// <returns> the byte array representing the input String</returns>
        public static byte[] ToByteArray(this String input)
        {
            byte[] output = new byte[input.Length];
            char[] inpt = input.ToCharArray();
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)(inpt[i]);
            }
            return output;
        }

        public static String ToFixedLength(this String input, int desiredLength)
        {
            char[] cArr = input.ToCharArray();
            if (input.Length < desiredLength)
            {
                int counter = 0;
                for (int i = input.Length; i < desiredLength; i++)
                {
                    if (counter == cArr.Length)
                    {
                        counter = 0;
                    }
                    input += cArr[counter];
                    counter++;
                }
            }
            else if (input.Length > desiredLength)
            {
                input = "";
                for (int i = 0; i < desiredLength; i++)
                {
                    input += cArr[i];
                }
            }
            return input;
        }

        public static String RandomAlphanumerical(this String s, int length)
        {
            Random random = new Random();
            String randomString = "";
            char[] availableChars = {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
            };
            for (int i = 0; i < length; i++)
            {
                randomString += availableChars[((int)(random.NextDouble() * 61))];
            }
            return randomString;
        }

        /*
         * Entfernt alle zeichen die nicht in den zahlen in den tabellen auftauchen dürfen
         * */
        public static String ToCorrectInputFormat(this String input)
        {
            List<byte> allowedBytes = new List<byte> { ((byte)('0')), ((byte)('1')), ((byte)('2')), ((byte)('3')), ((byte)('4')), ((byte)('5')), ((byte)('6')), ((byte)('7')), ((byte)('8')), ((byte)('9')), ((byte)('|')), ((byte)(',')) };
            for (byte i = byte.MinValue; i < byte.MaxValue; i++)
            {
                if (!allowedBytes.Contains(i))
                {
                    input = input.Replace(((char)((i))).ToString(), "");
                }
            }
            return input;
        }

        /*
         * Das gleiche wie oben, nur mit leicht anderem zeichensatz
         * */
        public static String ToInteger(this String input)
        {
            List<byte> allowedBytes = new List<byte> { ((byte)('0')), ((byte)('1')), ((byte)('2')), ((byte)('3')), ((byte)('4')), ((byte)('5')), ((byte)('6')), ((byte)('7')), ((byte)('8')), ((byte)('9')) };
            for (byte i = byte.MinValue; i < byte.MaxValue; i++)
            {
                if (!allowedBytes.Contains(i))
                {
                    input = input.Replace(((char)((i))).ToString(), "");
                }
            }
            return input;
        }

        /*
         * erzeigt einen String mit festgelegter länge (entweder kürzen oder verlängern)
         * Diese Methode ist für den umgang mit den Tabellen, also zahlen in Strings gedacht
         * */
        public static String ToLength(this String input, int length)
        {
            if (input.Length > length)
            {
                input = input.Substring(0, length);
            }
            for (int x = input.Length; x < length; x++)
            {
                input += "0";//da es um Zahlen geht "0" und nicht " "
            }
            return input;
        }

        /*
         * Gibt ein feld von allen Dateien im eingabeordner zurück
         * */
        public async static Task<IFile[]> GetStorageFileArray(this IFolder inputFolder)
        {
            IList<IFile> fileList = await inputFolder.GetFilesAsync();
            IEnumerator<IFile> fileEnumerator = fileList.GetEnumerator();
            IFile[] outputArray = new IFile[fileList.Count];
            for (int i = 0; i < outputArray.Length; i++)
            {
                fileEnumerator.MoveNext();
                outputArray[i] = fileEnumerator.Current;
            }
            return outputArray;
        }

        /*
         * Gibt ein feld von allen ordnern im eingabeordner zurück
         * */
        public async static Task<IFolder[]> GetStorageFolderArray(this IFolder inputFolder)
        {
            IList<IFolder> folderList = await inputFolder.GetFoldersAsync();
            IEnumerator<IFolder> folderEnumerator = folderList.GetEnumerator();
            IFolder[] outputArray = new IFolder[folderList.Count];
            for (int i = 0; i < outputArray.Length; i++)
            {
                folderEnumerator.MoveNext();
                outputArray[i] = folderEnumerator.Current;
            }
            return outputArray;
        }

        /*
         * kopiert den gesamten ordnerinhalt in einen anderen ordner (z.B. für Backups des Installationsordners)
         * *//*
        public async static Task CopyFolderContentsTo(this IFolder source, IFolder target)
        {
            IFolder[] folders = await GetStorageFolderArray(source);
            for (int i = 0; i < folders.Length; i++)
            {
                await folders[i].CopyFolderContentsTo(await target.CreateFolderAsync(folders[i].Name, CreationCollisionOption.OpenIfExists));
            }
            IFile[] files = await GetStorageFileArray(source);
            files[0].
            for (int i = 0; i < files.Length; i++)
            {
                await (await target.CreateFileAsync(files[i].Name, CreationCollisionOption.ReplaceExisting)).WriteAllBytesAsync(await files[i].ReadAllBytesAsync());
            }
        }*/
    }
}