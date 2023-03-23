//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace League.Tests.TestComponents;
public class HostingEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = Microsoft.Extensions.Hosting.Environments.Development;

    public string ApplicationName { get; set; } = null!;

    public string WebRootPath { get; set; } = null!;

    public IFileProvider WebRootFileProvider { get; set; } = null!;

    public string ContentRootPath { get; set; } = null!;

    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}
