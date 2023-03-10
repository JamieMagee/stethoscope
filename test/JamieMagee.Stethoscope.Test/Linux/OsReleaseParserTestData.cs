namespace JamieMagee.Stethoscope.Test.Linux;

using JamieMagee.Stethoscope.Models;
using OperatingSystems.OsRelease;

public class OsReleaseParserTestData : TheoryData<StreamReader, OSRelease>
{
    public OsReleaseParserTestData() =>
        this.Add(
            CreateStreamReader(Path.Join("mariner", "os-release")),
            new OSRelease
            {
                PrettyName = "CBL-Mariner/Linux",
                Name = "Common Base Linux Mariner",
                Id = "mariner",
                IdLike = null,
                Version = "1.0.20210901",
                VersionId = "1.0",
                HomeUrl = "https://aka.ms/cbl-mariner",
                SupportUrl = "https://aka.ms/cbl-mariner",
                BugReportUrl = "https://aka.ms/cbl-mariner",
            });

    private static StreamReader CreateStreamReader(string path) =>
        new(Path.Join(Environment.CurrentDirectory, "Resources", "os", path));
}
