using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace BackupManagerLib
{
    public class BackupType
    {
        private DirectoryExtensions _directoryExtensions = new DirectoryExtensions();
        private string _dateTime;

        public async void ZipArviceBackupType(string sourceDirectory, string destinationDirectory)
        {
            await Task.Run(() =>
            {
                string EndZip;
                _dateTime = DateTime.Now.ToString().Split(' ')[0];
                int ZipVersion = 1;

                if (!Directory.Exists(destinationDirectory))
                {
                    _directoryExtensions.DirectoryCreate(destinationDirectory);
                }

            m1: EndZip = $"{destinationDirectory}\\Backup-{_dateTime} [{ZipVersion}]";

                if (File.Exists($"{EndZip}.zip"))
                {
                    ZipVersion++;
                    goto m1;
                }
                else
                {
                    EndZip = $"{destinationDirectory}\\Backup-{_dateTime} [{ZipVersion}]";

                    ZipFile.CreateFromDirectory(sourceDirectory, $"{EndZip}.zip");

                    ZipVersion = 1;
                }
            });
        }

        public async void OverwriteFolderBackupType(string sourceDirectory, string destinationDirectory)
        {
            await Task.Run(() =>
            {
                string EndOverwriteFolder;
                _dateTime = DateTime.Now.ToString().Split(' ')[0];

                int OverwriteFolderVersion = 1;

                if (!Directory.Exists(destinationDirectory))
                {
                    _directoryExtensions.DirectoryCreate(destinationDirectory);
                }

            m1: EndOverwriteFolder = $"{destinationDirectory}\\Backup-{_dateTime} [{OverwriteFolderVersion}]";

                if (Directory.Exists(EndOverwriteFolder))
                {
                    OverwriteFolderVersion++;
                    goto m1;
                }
                else
                {
                    EndOverwriteFolder = $"{destinationDirectory}\\Backup-{_dateTime} [{OverwriteFolderVersion}]";

                    _directoryExtensions.DirectoryCreate(EndOverwriteFolder);
                    _directoryExtensions.DirectoryCopy(sourceDirectory, EndOverwriteFolder, true);

                    OverwriteFolderVersion = 1;
                }
            });
        }

        public async void SingeFolderBackupType(string sourceDirectory, string destinationDirectory)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(destinationDirectory))
                {
                    Directory.Delete(destinationDirectory, true);
                    _directoryExtensions.DirectoryCreate(destinationDirectory);
                    _directoryExtensions.DirectoryCopy(sourceDirectory, destinationDirectory, true);
                }
                else
                {
                    _directoryExtensions.DirectoryCreate(destinationDirectory);
                    _directoryExtensions.DirectoryCopy(sourceDirectory, destinationDirectory, true);
                }
            });
        }
    }
}
