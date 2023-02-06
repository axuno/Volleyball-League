using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace League.Models.UploadViewModels;

public class TeamPhotoStaticFile : AbstractStaticFile
{
    public const string TeamPhotoFolder = "teamphoto";
        
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TeamPhotoStaticFile> _logger;

    /// <summary>
    /// Template for saving team photos. {0}: tenant key, {1}: team id {2}: <see cref="DateTime.Ticks"/>  {3} file extension
    /// </summary>
    private readonly string _filenameTemplate = "photo_{0}_team_{1}_t{2}.{3}";

    /// <summary>
    /// Pattern for finding team photos. {0}: tenant key, {1}: team id
    /// </summary>
    private readonly string _fileSearchPattern = "photo_{0}_team_{1}_t*.*";

    /// <summary>
    /// The folder path below <see cref="IWebHostEnvironment.WebRootPath"/> where files will be searched and stored.
    /// </summary>
    private readonly string _folder;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="webHostEnvironment"></param>
    /// <param name="tenantContext"></param>
    /// <param name="logger"></param>
    public TeamPhotoStaticFile(IWebHostEnvironment webHostEnvironment, ITenantContext tenantContext, ILogger<TeamPhotoStaticFile> logger) : base(logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _tenantContext = tenantContext;
        _logger = logger;
        _folder = TeamPhotoFolder;
    }

    public async Task<string> SaveFileAsync(IFormFile formFile, string extension, long teamId, bool withRemoveObsoleteFiles, CancellationToken cancellationToken)
    {
        var fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _folder,
            string.Format(_filenameTemplate, _tenantContext.SiteContext.FolderName, teamId,
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
                string.Format(_fileSearchPattern, _tenantContext.SiteContext.FolderName, teamId));
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
                string.Format(_fileSearchPattern, _tenantContext.SiteContext.FolderName, teamId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not delete one or more obsolete files");
        }
    }

    public (string? Filename, DateTime Date) GetFileInfo(long teamId)
    {
        return GetFileInfo(new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, _folder)),
            string.Format(_fileSearchPattern, _tenantContext.Identifier, teamId));
    }

    public (string? Uri, DateTime Date) GetUriInfo(long teamId)
    {
        var (filename, date) = GetFileInfo(teamId);
        return (filename != null ?  $"~/{_folder}/{filename}" : null, Date: date);
    }
}
