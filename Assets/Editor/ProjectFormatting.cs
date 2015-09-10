using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;

public class ProjectFormatting : AssetPostprocessor
{
    // Undocumented, called when CS project changes
    static public void OnGeneratedCSProjectFiles()
    {
        foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln"))
        {
            var content = File.ReadAllText(file);
            File.WriteAllText(file, Regex.Replace(content, Pattern, Replacement.Replace("$", "$$")));
        }
    }

    static readonly string Pattern =
        @"(?s)GlobalSection\(MonoDevelopProperties\) = preSolution.*EndGlobalSection";

    // Matches Tools/MonoDevelop.mdpolicy
    static readonly string Replacement = string.Join("\r\n", new[]
        {
            "GlobalSection(MonoDevelopProperties) = preSolution",
            "   StartupItem = Assembly-CSharp.csproj",
            "   Policies = $0",
            "   $0.TextStylePolicy = $1",
            "   $1.inheritsSet = null",
            "   $1.scope = text/x-csharp",
            "   $0.CSharpFormattingPolicy = $2",
            "   $2.IndentSwitchBody = True",
            "   $2.IndentBlocksInsideExpressions = True",
            "   $2.AnonymousMethodBraceStyle = NextLine",
            "   $2.PropertyBraceStyle = NextLine",
            "   $2.PropertyGetBraceStyle = NextLine",
            "   $2.PropertySetBraceStyle = NextLine",
            "   $2.EventBraceStyle = NextLine",
            "   $2.EventAddBraceStyle = NextLine",
            "   $2.EventRemoveBraceStyle = NextLine",
            "   $2.StatementBraceStyle = NextLine",
            "   $2.ArrayInitializerBraceStyle = NextLine",
            "   $2.ElseNewLinePlacement = NewLine",
            "   $2.CatchNewLinePlacement = NewLine",
            "   $2.FinallyNewLinePlacement = NewLine",
            "   $2.BeforeMethodDeclarationParentheses = False",
            "   $2.BeforeMethodCallParentheses = False",
            "   $2.BeforeConstructorDeclarationParentheses = False",
            "   $2.BeforeDelegateDeclarationParentheses = False",
            "   $2.NewParentheses = False",
            "   $2.SpacesBeforeBrackets = False",
            "   $2.inheritsSet = Mono",
            "   $2.inheritsScope = text/x-csharp",
            "   $2.scope = text/x-csharp",
            "   $0.TextStylePolicy = $3",
            "   $3.FileWidth = 120",
            "   $3.EolMarker = Windows",
            "   $3.inheritsSet = VisualStudio",
            "   $3.inheritsScope = text/plain",
            "   $3.scope = text/plain",
            "   $0.DotNetNamingPolicy = $4",
            "   $4.DirectoryNamespaceAssociation = None",
            "   $4.ResourceNamePolicy = FileFormatDefault",
            "EndGlobalSection",
        });
}