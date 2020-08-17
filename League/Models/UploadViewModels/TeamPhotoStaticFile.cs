using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using League.DI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace League.Models.UploadViewModels
{
    public class TeamPhotoStaticFile : AbstractStaticFile
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly SiteContext _siteContext;
        private readonly ILogger<TeamPhotoStaticFile> _logger;

        /// <summary>
        /// Template for saving team photos. {0}: organization key, {1}: team id {2}: <see cref="DateTime.Ticks"/>  {3} file extension
        /// </summary>
        private string _filenameTemplate = "photo_{0}_team_{1}_t{2}.{3}";

        /// <summary>
        /// Pattern for finding team photos. {0}: organization key, {1}: team id
        /// </summary>
        private string _fileSearchPattern = "photo_{0}_team_{1}_t*.*";

        /// <summary>
        /// The folder path below <see cref="IWebHostEnvironment.WebRootPath"/> where files will be searched and stored.
        /// </summary>
        private readonly string _folder;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="webHostEnvironment"></param>
        /// <param name="siteContext"></param>
        /// <param name="logger"></param>
        public TeamPhotoStaticFile(IWebHostEnvironment webHostEnvironment, SiteContext siteContext, ILogger<TeamPhotoStaticFile> logger) : base(logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _siteContext = siteContext;
            _logger = logger;
            _folder = _siteContext.Photos.TeamPhotoFolder;
        }

        public async Task<string> SaveFileAsync(IFormFile formFile, string extension, long teamId, bool withRemoveObsoleteFiles, CancellationToken cancellationToken)
        {
            var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _folder,
                string.Format(_filenameTemplate, _siteContext.FolderName, teamId,
                    DateTime.UtcNow.Ticks, extension.TrimStart('.')));

            var filename = await SaveFileAsync(formFile, fullFilePath, cancellationToken);

            if (withRemoveObsoleteFiles) DeleteObsoleteFiles(teamId);

            return filename;
        }

        public void DeleteMostRecentFile(long teamId)
        {
            try
            {
                DeleteMostRecentFile(new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, _folder)),
                    string.Format(_fileSearchPattern, _siteContext.FolderName, teamId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not delete most recent file");
            }
        }

        private void DeleteObsoleteFiles(long teamId)
        {
            try
            {
               DeleteObsoleteFiles(new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, _folder)),
                   string.Format(_fileSearchPattern, _siteContext.FolderName, teamId));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not delete one or more obsolete files");
            }
        }

        public (string Filename, DateTime Date) GetFileInfo(long teamId)
        {
            return GetFileInfo(new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, _folder)),
                string.Format(_fileSearchPattern, _siteContext.OrganizationKey, teamId));
        }

        public (string Uri, DateTime Date) GetUriInfo(long teamId)
        {
            var fi = GetFileInfo(teamId);
            return (fi.Filename != null ?  $"~/{_folder}/{fi.Filename}" : null, fi.Date);
        }
    }
}
