using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ParkitectAssetEditor.Compression
{
    static class ArchiveHelper
    {
        /// <summary>
        /// Creates a zip.
        /// </summary>
        /// <param name="outPathname">The out pathname.</param>
        /// <param name="folderName">Name of the folder.</param>
        public static void CreateZip(string outPathname, string folderName)
        {
            var zipStream = new ZipOutputStream(File.Create(outPathname));

            zipStream.SetLevel(9); // highest level
            
            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            var folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

            CompressFolder(folderName, zipStream, folderOffset);

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        /// <summary>
        /// Compresses a folder recursively.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="zipStream">The zip stream.</param>
        /// <param name="folderOffset">The folder offset.</param>
        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                var entryName = file.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                
                zipStream.PutNextEntry(new ZipEntry(entryName)
                {
                    DateTime = fileInfo.LastWriteTime,
                    Size = fileInfo.Length
                });

                // Zip the file in buffered chunks
                var buffer = new byte[4096];
                using (var streamReader = File.OpenRead(file))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            var folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
    }
}
