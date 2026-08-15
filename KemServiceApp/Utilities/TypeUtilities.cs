using System.Reflection;

namespace KemServiceApp.Utilities
{
    public static class TypeUtilities
    {
        #region Properties
        private const string ERROR_TYPE_INHERIT_CAN_NOT_NULL = "Type can not null",
            ERROR_TYPE_BASE_CAN_NOT_NULL = "TypeBase can not null",
            ERROR_CLASS_NOT_INHERIT = "Type {0} not inherit {1}",
            ERROR_LIBRARY_PART_EMPTY = "Library part can not null",
            ERROR_CLASS_NAME_EMPTY = "Class name can not null",
            ERROR_TYPE_NOT_FOUND = "Class not found",
            ERROR_FILE_NOT_FOUND = "File not found",
            ERROR_CAN_NOT_LOAD_CLASS = "Class load fail";
        #endregion

        #region Method
        public static bool TryGet_Directory(Type? type, [NotNullWhen(true)] out string? directory)
        {
            if (type == null)
            {
                directory = null;
                return false;
            }
            //
            string path = type.Assembly.Location;
            FileInfo fileInfo = new(path);
            if (fileInfo.DirectoryName == null)
            {
                directory = null;
                return false;
            }
            directory = fileInfo.DirectoryName;
            return true;
        }
        public static bool TryGet_Part(Type? type, [NotNullWhen(true)] out string? path)
        {
            if (type == null)
            {
                path = null;
                return false;
            }
            //
            path = type.Assembly.Location;
            return true;
        }
        public static bool TryGetType(string? libraryPart, string? className, [NotNullWhen(true)] out Type? type, [NotNullWhen(false)] out string? error)
        {
            if (string.IsNullOrEmpty(libraryPart))
            {
                type = null;
                error = ERROR_LIBRARY_PART_EMPTY;
                return false;
            }
            if (string.IsNullOrEmpty(className))
            {
                type = null;
                error = ERROR_CLASS_NAME_EMPTY;
                return false;
            }
            //
            try
            {
                type = Assembly.LoadFrom(libraryPart).GetType(className);
                if (type == null)
                {
                    error = ERROR_TYPE_NOT_FOUND;
                    return false;
                }
                error = null;
                return true;
            }
            catch (FileNotFoundException)
            {
                type = null;
                error = ERROR_FILE_NOT_FOUND;
                return false;
            }
            catch (Exception)
            {
                type = null;
                error = ERROR_CAN_NOT_LOAD_CLASS;
                return false;
            }
        }
        public static bool CheckInherit(Type? type, Type? typeBase, [NotNullWhen(false)] out string? error)
        {
            if (type == null)
            {
                error = ERROR_TYPE_INHERIT_CAN_NOT_NULL;
                return false;
            }
            if (typeBase == null)
            {
                error = ERROR_TYPE_BASE_CAN_NOT_NULL;
                return false;
            }
            if (!typeBase.IsAssignableFrom(type))
            {
                error = string.Format(ERROR_CLASS_NOT_INHERIT, type.Name, typeBase.Name);
                return false;
            }
            error = null;
            return true;
        }
        public static bool CheckLibraryPart(string? libraryPart, [NotNullWhen(false)] out string? error)
        {
            if (string.IsNullOrEmpty(libraryPart))
            {
                error = ERROR_LIBRARY_PART_EMPTY;
                return false;
            }
            error = null;
            return true;
        }
        public static bool CheckClassName(string? className, [NotNullWhen(false)] out string? error)
        {
            if (string.IsNullOrEmpty(className))
            {
                error = ERROR_CLASS_NAME_EMPTY;
                return false;
            }
            error = null;
            return true;
        }
        #endregion
    }
}
