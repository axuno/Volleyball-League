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
    public abstract class AbstractStaticFile
    {
        private readonly ILogger<AbstractStaticFile> _logger;

        protected string DateTimeTicksMarker = "_t";

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="logger"></param>
        protected AbstractStaticFile(ILogger<AbstractStaticFile> logger)
        {
            _logger = logger;
        }

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
