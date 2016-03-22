using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Microsoft.Phone.Storage;
using System.Windows;
using System.Collections.ObjectModel;
using Windows.Phone.Storage.SharedAccess;
using Windows.Storage.Streams;

namespace RipTrail.ViewModel
{
    class FileIO
    {
       

        /// <summary>
        /// Saves a document,
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
        /// <param name="doc"></param>
        /// <returns>True for success</returns>
        public async Task<Boolean> WriteToFile(string fileName, string folder, string doc)
        {
            try
            { 
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(doc);

                // Get the local folder.
               StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

                // Create a new folder or use current one.
                var dataFolder = await local.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);

                // Create a new file.
                var file = await dataFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
               
                // Write the data.
                using (var s = await file.OpenStreamForWriteAsync())
                {
                    s.Write(fileBytes, 0, fileBytes.Length);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Delete a file from local application folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFileFromLocal(String fileName, string folder)
        {
            try
            {
                // Get the local folder.
                StorageFolder local = ApplicationData.Current.LocalFolder;

                if (local != null)
                {
                    var dataFolder = await local.GetFolderAsync(folder);

                    // Get the file.
                    var file = await dataFolder.GetFileAsync(fileName);
                    await file.DeleteAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Gets a file from a folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
        /// <returns>string</returns>
        public async Task<Stream> ReadFileFromFolder(string fileName, string folder)
        {
            try
            {
                // Get the local folder.
                StorageFolder local = ApplicationData.Current.LocalFolder;

                if (local != null)
                {
                    // Get the DataFolder folder.
                    var dataFolder = await local.GetFolderAsync(folder);

                    // Get the file.
                    return await dataFolder.OpenStreamForReadAsync(fileName);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Gets a file from a folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
        /// <returns>string</returns>
        public async Task<Stream> ReadFileFromLocalStorage(string fileName)
        {
            try
            {
                // Get the local folder.
                StorageFolder local = ApplicationData.Current.LocalFolder;

                if (local != null)
                {
                    // Get the file.
                    return await local.OpenStreamForReadAsync(fileName);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Gets all external storage files
        /// </summary>
        /// <param name="sdCard"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<ExternalStorageFile>> GetExternal_GPXFiles(string folder, string fileExtension)
        {
            ObservableCollection<ExternalStorageFile> storageFIle = new ObservableCollection<ExternalStorageFile>();

            try
            {
                ExternalStorageDevice sdCard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();
                if(sdCard != null)
                {
                    ExternalStorageFolder routesFolder = await sdCard.GetFolderAsync(folder);

                    IEnumerable<ExternalStorageFile> routeFiles = await routesFolder.GetFilesAsync();

                    foreach (ExternalStorageFile extStoreFile in routeFiles)
                    {
                        if (extStoreFile.Path.EndsWith(fileExtension))
                        {
                            storageFIle.Add(extStoreFile);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("SD card not found, Please make sure the SD card is inserted properly.");
                }
            }
            catch(FileNotFoundException)
            {              
                MessageBox.Show("The file folder " + folder + " not found.");
                return null;
            }
            return storageFIle;
        }


        /// <summary>
        /// Checks to see if a folder exists on the sd card.
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public async Task<bool> SDCard_CheckFolder(string folderName)
        {
            try
            {
                if(folderName != string.Empty)
                {
                    ExternalStorageDevice sdcard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();
                    if(sdcard != null)
                    {
                        ExternalStorageFolder folder = await sdcard.GetFolderAsync(folderName);
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch(FileNotFoundException)
            {
                return false;
            }
        }


        /// <summary>
        /// Gets a file from SD Card
        /// </summary>
        /// <param name="sdfilePath"></param>
        /// <returns>File Stream</returns>
        public async Task<Stream> SDCard_ReadFile(string sdfilePath)
        {
            try
            {
                ExternalStorageDevice sdcard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();

                if (sdcard != null)
                {
                    ExternalStorageFile file = await sdcard.GetFileAsync(sdfilePath);
                    return await file.OpenForReadAsync(); 
                }
                else
                {
                    MessageBox.Show("Please check SD Card");
                    return null;                   
                }
            }
            catch(FileNotFoundException)
            {
                MessageBox.Show("File not found.");
                return null;               
            }
        }


        /// <summary>
        /// Reads a file from local storage folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folder"></param>
        /// <returns>string</returns>
        public async Task<string> ReadFromFile(string fileName, StorageFolder folder = null)
        {
            folder = folder ?? ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(fileName);

            using (var fs = await file.OpenAsync(FileAccessMode.Read))
            {
                using (var inStream = fs.GetInputStreamAt(0))
                {
                    using (var reader = new DataReader(inStream))
                    {
                        await reader.LoadAsync((uint)fs.Size);
                        string data = reader.ReadString((uint)fs.Size);
                        reader.DetachStream();
                        return data;
                    }
                }
            }
        }


        /// <summary>
        /// Copies a file into local folder from shared storage.
        /// </summary>
        /// <returns>fileName</returns>
        public async Task<string> SharedStorageCopyFile(string fileToken)
        {
            string fileName;
            MiscFunctions misc = new MiscFunctions();
            try
            {
                fileName = SharedStorageAccessManager.GetSharedFileName(fileToken);

                await SharedStorageAccessManager.CopySharedFileAsync(Windows.Storage.ApplicationData.Current.LocalFolder, fileName, Windows.Storage.NameCollisionOption.ReplaceExisting, fileToken);
                return fileName;
            }
            catch
            {
                return null;
            }
        }

    }// End of class
}// End of namespace
