﻿using Microsoft.CodeAnalysis;
using OrleanSpaces.Analyzers.Tests.Fixtures;

namespace OrleanSpaces.Analyzers.Tests;

public class DefaultableTests : CodeFixFixture
{
    public DefaultableTests()
        : base(new DefaultableAnalyzer(), new DefaultableCodeFixProvider(), DefaultableAnalyzer.Diagnostic.Id)
    {

    }

    [Fact]
    public void Should_Create_Diagnostic()
    {
        Assert.Equal("OSA001", DefaultableAnalyzer.Diagnostic.Id);
        Assert.Equal(DiagnosticCategory.Performance, DefaultableAnalyzer.Diagnostic.Category);
        Assert.Equal(DiagnosticSeverity.Info, DefaultableAnalyzer.Diagnostic.DefaultSeverity);
        Assert.Equal("Avoid instantiation by default constructor or expression.", DefaultableAnalyzer.Diagnostic.Title);
        Assert.Equal("Avoid instantiation of '{0}' by default constructor or expression.", DefaultableAnalyzer.Diagnostic.MessageFormat);
        Assert.True(DefaultableAnalyzer.Diagnostic.IsEnabledByDefault);
    }

    private static string ComposeSource(string code) => @$"
using OrleanSpaces.Tuples;

class T 
{{
    void M() 
    {{
        {code}
    }} 
}}";

    #region SpaceUnit

    [Fact]
    public void SpaceUnit_DefaultConstructor() =>
        TestCodeFix(
            source: ComposeSource("SpaceUnit unit = [|new SpaceUnit()|];"),
            fixedSource: ComposeSource("SpaceUnit unit = SpaceUnit.Null;"));

    [Fact]
    public void SpaceUnit_DefaultImplicitConstructor() =>
        TestCodeFix(
            source: ComposeSource("SpaceUnit unit = [|new()|];"),
            fixedSource: ComposeSource("SpaceUnit unit = SpaceUnit.Null;"));

    [Fact]
    public void SpaceUnit_DefaultExpression() =>
        TestCodeFix(
            source: ComposeSource("SpaceUnit unit = [|default(SpaceUnit)|];"),
            fixedSource: ComposeSource("SpaceUnit unit = SpaceUnit.Null;"));

    [Fact]
    public void SpaceUnit_DefaultImplicitExpression() =>
        TestCodeFix(
            source: ComposeSource("SpaceUnit unit = [|default|];"),
            fixedSource: ComposeSource("SpaceUnit unit = SpaceUnit.Null;"));

    #endregion

    #region SpaceTuple

    [Fact]
    public void SpaceTuple_DefaultConstructor() =>
        TestCodeFix(
            source: ComposeSource("SpaceTuple tuple = [|new SpaceTuple()|];"),
            fixedSource: ComposeSource("SpaceTuple tuple = SpaceTuple.Null;"));

    [Fact]
    public void SpaceTuple_DefaultImplicitConstructor() =>
        TestCodeFix(
            source: ComposeSource("SpaceTuple tuple = [|new()|];"),
            fixedSource: ComposeSource("SpaceTuple tuple = SpaceTuple.Null;"));

    [Fact]
    public void SpaceTuple_DefaultExpression() =>
        TestCodeFix(
            source: ComposeSource("SpaceTuple tuple = [|default(SpaceTuple)|];"),
            fixedSource: ComposeSource("SpaceTuple tuple = SpaceTuple.Null;"));

    [Fact]
    public void SpaceTuple_DefaultImplicitExpression() =>
        TestCodeFix(
            source: ComposeSource("SpaceTuple tuple = [|default|];"),
            fixedSource: ComposeSource("SpaceTuple tuple = SpaceTuple.Null;"));

    #endregion
}