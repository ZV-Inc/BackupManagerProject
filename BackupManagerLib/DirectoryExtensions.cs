using System.IO;

namespace BackupManagerLib
{
    public class DirectoryExtensions
    {
        /// <summary>
        /// Создаёт каталог по указанному пути, если он ещё не существует.
        /// </summary>
        public void DirectoryCreate(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void DirectoryDelete(string directory)
        {
            Directory.Delete(directory, true);
        }

        /// <summary>
        /// Копирует один каталог в другой, при наличии соответствующей инструкции, все файлы и подпапки в нём.
        /// </summary>
        /// <param name="sourceDirectoryName">Исходный каталог.</param>
        /// <param name="destinationDirectoryName">Конечный каталог в который скопируется исходный.</param>
        /// <param name="copySubDirectories">Если true - скопируются все файлы и подпапки в исходном каталоге.</param>
        public void DirectoryCopy(string sourceDirectoryName, string destinationDirectoryName, bool copySubDirectories)
        {
            DirectoryInfo directory = new DirectoryInfo(sourceDirectoryName);

            DirectoryInfo[] directories = directory.GetDirectories();

            Directory.CreateDirectory(destinationDirectoryName);

            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destinationDirectoryName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (string fileName in Directory.GetFiles(destinationDirectoryName, ".", SearchOption.AllDirectories))
            {
                File.SetAttributes(fileName, FileAttributes.Normal);
            }

            if (copySubDirectories)
            {
                foreach (DirectoryInfo subDirectories in directories)
                {
                    string tempPath = Path.Combine(destinationDirectoryName, subDirectories.Name);
                    DirectoryCopy(subDirectories.FullName, tempPath, copySubDirectories);
                }
            }
        }
    }
}