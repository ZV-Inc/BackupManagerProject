using System;
using System.IO;
using System.Windows.Forms;

namespace FileSaverInterface
{
    class DirectoryWork
    {
        /// <summary>
        /// Создаёт папку и убирает с файлов атрибуты "Только для чтения"
        /// </summary>
        public static void DirectoryCreate(string EndDir)
        {
            try
            {
                if (!Directory.Exists(EndDir))
                {
                    Directory.CreateDirectory(EndDir);

                    foreach (string fileName in Directory.GetFiles(EndDir, ".", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(fileName, FileAttributes.Normal);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"При создании папки произошла ошибка: {ex.Message}");
            }
        }

        public static void DirectoryDelete(string EndDir)
        {
            try
            {
                Directory.Delete(EndDir, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при попытке удалить папку: {ex.Message}");
            }
        }

        /// <summary>
        /// Копирует папку и файлы в ней при соответствующем условии
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                DirectoryInfo[] dirs = dir.GetDirectories(); //dirs получает инфу о всех папках внутри sourceDirName.

                Directory.CreateDirectory(destDirName);

                FileInfo[] files = dir.GetFiles(); //Получаем файлы в каталоге.

                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(tempPath, false);
                }

                foreach (string fileName in Directory.GetFiles(destDirName, ".", SearchOption.AllDirectories))
                {
                    File.SetAttributes(fileName, FileAttributes.Normal);
                }

                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string tempPath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при попытке сделать бэкап: {ex.Message}");
            }
        }
    }
}
