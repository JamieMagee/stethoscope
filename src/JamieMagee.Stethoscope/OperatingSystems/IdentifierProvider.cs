namespace JamieMagee.Stethoscope.OperatingSystems;

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;

public class IdentifierProvider : IIdentifierProvider
{
    private readonly IEnumerable<IIdentifier> identifiers;
    private readonly ILogger<IdentifierProvider> logger;

    public IdentifierProvider(IEnumerable<IIdentifier> identifiers, ILogger<IdentifierProvider> logger)
    {
        this.identifiers = identifiers;
        this.logger = logger;
    }

    public async Task<IRelease> IdentifyOperatingSystemAsync(string location, CancellationToken cancellationToken = default)
    {
        foreach (var identifier in this.identifiers)
        {
            var result = new Matcher()
                .AddInclude(identifier.Globs)
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(location)));

            if (result.HasMatches)
            {
                var fileStream = File.OpenRead(Path.Join(location, result.Files.First().Path));
                return await identifier.RunAsync(fileStream, cancellationToken);
            }
        }

        return null;
    }
}
