using System;
using System.IO;
using System.Windows.Navigation;
using Windows.Phone.Storage.SharedAccess;

namespace RipTrail.ViewModel
{
    class GPXAutoLauncher : UriMapperBase
    {
        private string tempUri;

        public override Uri MapUri (Uri uri)
        {
            tempUri = uri.ToString();
            
            if(tempUri.Contains("/FileTypeAssociation"))
            {
                // Get the file ID (after "fileToken=").
                int fileIDIndex = tempUri.IndexOf("fileToken=") + 10;
                string fileID = tempUri.Substring(fileIDIndex);

                // Get the file name.
                string incomingFileName =
                    SharedStorageAccessManager.GetSharedFileName(fileID);

                // Get the file extension.
                string incomingFileType = Path.GetExtension(incomingFileName);

                if (incomingFileType.Contains(".gpx"))
                {
                    return new Uri("/ViewRoute.xaml?fileToken=" + fileID, UriKind.Relative);
                }
                else
                {
                    return new Uri("/MainPage.xaml", UriKind.Relative);
                }

            }
            // Otherwise perform normal launch.
            return uri;

        }
    }
}
