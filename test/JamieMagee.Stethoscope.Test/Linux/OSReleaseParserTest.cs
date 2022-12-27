namespace JamieMagee.Stethoscope.Test.Linux;

using FluentAssertions;
using JamieMagee.Stethoscope.Models;
using OperatingSystems.OsRelease;

public class OsReleaseParserTest
{
    [Theory]
    [ClassData(typeof(OsReleaseParserTestData))]
    public async Task Should_ParseOsReleaseAsync(StreamReader reader, Release expected)
    {
        var actual = await OsReleaseParser.ParseOSReleaseAsync(reader);

        actual.Should().Be(expected);
    }
}
