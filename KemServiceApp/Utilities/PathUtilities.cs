namespace KemServiceApp.Utilities
{
    public static class PathUtilities
    {
        #region Properties

        #endregion

        #region Method
        public static string CombinePath(string? baseDirectory, string? path)
        {
            if (string.IsNullOrEmpty(baseDirectory))
            {
                if (string.IsNullOrEmpty(path))
                    return string.Empty;
                return path;
            }
            if (string.IsNullOrEmpty(path))
                return baseDirectory;
            //
            try
            {
                return Path.Combine(baseDirectory, path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static bool CheckPathFullyQualified(string path)
        {
            try
            {
                return Path.IsPathFullyQualified(path);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static string GetFullPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            //
            try
            {
                return Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region Directory
        public static string Directory_GetBase(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            try
            {
                DirectoryInfo directoryInfo = new(path);
                if (directoryInfo.Parent == null)
                    return string.Empty;
                return directoryInfo.Parent.FullName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static bool Directory_Exists(string? directory, out string result, bool isCreate = false)
        {
            if (string.IsNullOrEmpty(directory))
            {
                result = GetFullPath(directory);
                return false;
            }
            //
            try
            {
                DirectoryInfo directoryInfo = new(directory);
                if (directoryInfo.Exists)
                {
                    result = directoryInfo.FullName;
                    return true;
                }
            }
            catch (Exception)
            {
                result = GetFullPath(directory);
                return false;
            }
            //
            if (!isCreate)
            {
                result = GetFullPath(directory);
                return false;
            }
            //
            try
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(directory);
                result = directoryInfo.FullName;
                return true;
            }
            catch (Exception)
            {
                result = GetFullPath(directory);
                return false;
            }
        }
        public static bool Directory_Exists(string? baseDirectory, string? directory, out string result, bool isCreate = false)
        {
            string directoryCombine = CombinePath(baseDirectory, directory);
            if (Directory_Exists(directoryCombine, out result, isCreate))
                return true;
            result = GetFullPath(directoryCombine);
            return false;
        }
        #endregion

        #region File
        public static string File_GetDirectory(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            try
            {
                FileInfo fileInfo = new(path);
                if (fileInfo.DirectoryName == null)
                    return string.Empty;
                return fileInfo.DirectoryName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static bool File_Exists(string? path, out string result, bool isCreate = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                result = GetFullPath(path);
                return false;
            }
            //
            try
            {
                FileInfo fileInfo = new(path);
                if (fileInfo.Exists)
                {
                    result = fileInfo.FullName;
                    return true;
                }
            }
            catch (Exception)
            {
                result = GetFullPath(path);
                return false;
            }
            //
            if (!isCreate)
            {
                result = GetFullPath(path);
                return false;
            }
            //
            FileStream? fileStream = null;
            try
            {
                fileStream = File.Create(path);
            }
            catch (Exception)
            {
                result = GetFullPath(path);
                return false;
            }
            finally
            {
                fileStream?.Close();
            }
            //
            result = GetFullPath(path);
            return true;
        }
        public static bool File_Exists(string? baseDirectory, string? path, out string result, bool isCreate = false)
        {
            string directoryCombine = CombinePath(baseDirectory, path);
            if (File_Exists(directoryCombine, out result, isCreate))
                return true;
            result = GetFullPath(directoryCombine);
            return false;
        }
        #endregion
    }
}
