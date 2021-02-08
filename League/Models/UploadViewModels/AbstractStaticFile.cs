using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace League.Models.UploadViewModels
{
    /// <summary>
    /// Abstract class for managing static files.
    /// </summary>
    public abstract class AbstractStaticFile
    {
        private readonly ILogger<AbstractStaticFile> _logger;

        /// <summary>
        /// The token that is used to append the <see cref="DateTime.Ticks"/> to file name.
        /// </summary>
        protected string DateTimeTicksMarker = "_t";

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="logger"></param>
        protected AbstractStaticFile(ILogger<AbstractStaticFile> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Saves an <see cref="IFormFile"/> the the <see ref="fullFilePath"/>.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="fullFilePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<string> SaveFileAsync(IFormFile formFile, string fullFilePath, CancellationToken cancellationToken)
        {
            try
            {
                await using var outStream = new FileStream(fullFilePath, FileMode.Create);
                await formFile.CopyToAsync(outStream, cancellationToken);
                return Path.GetFileName(fullFilePath);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "File '{0}' could not be saved", fullFilePath);
                throw;
            }
        }

        /// <summary>
        /// Deletes all but the latest 2 files from the specified <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="searchPattern"></param>
        protected void DeleteObsoleteFiles(DirectoryInfo dirInfo, string searchPattern)
        {
            var fileInfos = dirInfo.GetFiles(searchPattern).OrderByDescending(fi => fi.LastWriteTimeUtc);

            // Remove all files except for the 2 most recent
            foreach (var fileInfo in fileInfos.Skip(2))
            {
                try
                {
                    File.Delete(fileInfo.FullName);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "File '{0}' could not be deleted", fileInfo.FullName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the most recent file from the specified <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="searchPattern"></param>
        protected void DeleteMostRecentFile(DirectoryInfo dirInfo, string searchPattern)
        {
            var fileInfo = dirInfo.GetFiles(searchPattern).OrderByDescending(fi => fi.LastWriteTimeUtc).FirstOrDefault();
            if (fileInfo != null)
            {
                try
                {
                    File.Delete(fileInfo.FullName);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "File '{0}' could not be deleted", fileInfo.FullName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="ValueTuple"/> with the file name and the date extracted from the <see cref="DateTime.Ticks"/> part of the file name.
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        protected (string Filename, DateTime Date) GetFileInfo(DirectoryInfo dirInfo, string searchPattern)
        {
            var fileInfo = dirInfo.GetFiles(searchPattern).OrderByDescending(fi => fi.LastWriteTimeUtc)
                .FirstOrDefault();

            if (fileInfo == null) return (null, DateTime.UtcNow);

            var tickIndex = fileInfo.Name.LastIndexOf(DateTimeTicksMarker, StringComparison.InvariantCulture);
            if (tickIndex + DateTimeTicksMarker.Length >= fileInfo.Name.Length)
                return (fileInfo.Name, fileInfo.LastWriteTimeUtc);

            return long.TryParse(fileInfo.Name.Substring(tickIndex + DateTimeTicksMarker.Length), NumberStyles.Any,
                null, out var ticks)
                ? (fileInfo.Name, new DateTime(ticks))
                : (fileInfo.Name, fileInfo.LastWriteTimeUtc);
        }
    }
}
