namespace JamieMagee.Stethoscope.Test.Parsers;

using Catalogers.Apk;
using FluentAssertions;

public class ApkDatabaseParserTest
{
    [Fact]
    public async Task Should_ParseAsync_Installed()
    {
        using var streamReader = new StreamReader(Path.Join(Environment.CurrentDirectory, "Resources", "Apk", "installed"));

        var result = await ApkDatabaseParser.ParseAsync(streamReader);

        result.Should().HaveCount(14);
    }
}
