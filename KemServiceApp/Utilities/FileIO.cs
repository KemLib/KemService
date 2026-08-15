using System.Text;

namespace KemServiceApp.Utilities
{
    public static class FileIO
    {
        #region Properties

        #endregion

        #region Read
        public static byte[] Read(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception)
            {
                return [];
            }
        }
        public static async Task<byte[]> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await File.ReadAllBytesAsync(path, cancellationToken);
            }
            catch (Exception)
            {
                return [];
            }
        }
        public static string ReadText(string path)
        {
            try
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static async Task<string> ReadTextAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region Write
        public static void Write(string path, byte[] data)
        {
            try
            {
                File.WriteAllBytes(path, data);
            }
            catch (Exception)
            {

            }
        }
        public static async Task WriteAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            try
            {
                await File.WriteAllBytesAsync(path, data, cancellationToken);
            }
            catch (Exception)
            {

            }
        }
        public static void WriteText(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text, Encoding.UTF8);
            }
            catch (Exception)
            {

            }
        }
        public static async Task WriteTextAsync(string path, string text, CancellationToken cancellationToken = default)
        {
            try
            {
                await File.WriteAllTextAsync(path, text, Encoding.UTF8, cancellationToken);
            }
            catch (Exception)
            {

            }
        }
        #endregion
    }
}
