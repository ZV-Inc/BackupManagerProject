using System;
using System.Diagnostics;
using System.IO;

namespace FileSaver
{
    class DirectoryWork
    {
        private static EventLog ServiceLogger = new EventLog();

        /// <summary>
        /// Создаёт папку и убирает с файлов атрибуты "Только для чтения"
        /// </summary>
        public static void DirectoryCreate(string EndDir)
        {
            try
            {
                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                if (!Directory.Exists(EndDir))
                {
                    Directory.CreateDirectory(EndDir);

                    foreach (string fileName in Directory.GetFiles(EndDir, ".", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(fileName, FileAttributes.Normal);
                    }
                }

                ServiceLogger.WriteEntry($"Создание папки {EndDir} успешно.", EventLogEntryType.SuccessAudit, 5);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"При создании папки произошла ошибка: {ex.Message}");
            }
        }

        public static void DirectoryDelete(string EndDir)
        {
            try
            {
                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

                ServiceLogger.WriteEntry("Удаление конечной папки...", EventLogEntryType.Warning, 4);

                try
                {
                    Directory.Delete(EndDir, true);
                }
                catch (DirectoryNotFoundException)
                {
                    ServiceLogger.WriteEntry("Не удалось найти " + EndDir + ", удаление невозможно", EventLogEntryType.Error, 3);
                }
                catch (IOException)
                {
                    ServiceLogger.WriteEntry("Файл с тем же именем и расположении, заданном path, уже существует.\n" +
                        "- или - Каталог является текущим рабочим каталогом приложения.\n" +
                        "- или - Каталог, заданный параметром path, не пустой.\n" +
                        "- или - Каталог доступен только для чтения или содержит файл, доступный только для чтения.\n" +
                        "- или - Каталог используется другим процессом.", EventLogEntryType.Error, 3);
                }
                catch
                {
                    ServiceLogger.WriteEntry("Other exception, folder: " + EndDir, EventLogEntryType.Error, 3);
                }

                ServiceLogger.WriteEntry("Процедура удаления окончена.", EventLogEntryType.Warning, 4);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"Возникла ошибка при попытке удалить папку: {ex.Message}");
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
                ServiceLogger.Source = "FileSaverServiceSource";
                ServiceLogger.Log = "FileSaverServiceLog";

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

                ServiceLogger.WriteEntry("Копирование файлов успешно.", EventLogEntryType.SuccessAudit, 6);
            }
            catch (Exception ex)
            {
                ServiceLogger.WriteEntry($"Возникла ошибка при попытке сделать бэкап: {ex.Message}");
            }
        }
    }
}
