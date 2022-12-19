﻿namespace JamieMagee.Stethoscope.Test.Parsers;

using FluentAssertions;
using JamieMagee.Stethoscope.Parsers;

public class ApkDatabaseParserTest
{
    [Fact]
    public async Task Parse()
    {
        using var streamReader = new StreamReader(Path.Join(Environment.CurrentDirectory, "Resources", "Apk", "installed"));

        var result = await ApkDatabaseParser.ParseAsync(streamReader);

        result.Should().HaveCount(10);
    }
}