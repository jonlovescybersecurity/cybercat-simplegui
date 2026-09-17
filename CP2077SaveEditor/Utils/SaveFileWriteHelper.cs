using System;
using System.IO;
using System.Threading.Tasks;

namespace CP2077SaveEditor.Utils
{
    internal static class SaveFileWriteHelper
    {
        // The temporary file is in the destination directory so replacement stays on one volume.
        public static async Task<string> WriteAsync(string destinationPath, Stream contents)
        {
            var directory = Path.GetDirectoryName(destinationPath)
                ?? throw new ArgumentException("A destination directory is required", nameof(destinationPath));
            var temporaryPath = Path.Combine(directory, $"sav.dat.{Guid.NewGuid():N}.tmp");

            try
            {
                await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    contents.Position = 0;
                    await contents.CopyToAsync(stream);
                    stream.Flush(flushToDisk: true);
                }

                if (!File.Exists(destinationPath))
                {
                    File.Move(temporaryPath, destinationPath);
                    return null;
                }

                var backupPath = Path.Combine(directory,
                    $"sav.{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}.{Guid.NewGuid():N}.bak");
                File.Replace(temporaryPath, destinationPath, backupPath);
                return backupPath;
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
    }
}
