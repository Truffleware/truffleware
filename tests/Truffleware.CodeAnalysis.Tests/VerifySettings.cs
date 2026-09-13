using System.Runtime.CompilerServices;

namespace Truffleware.CodeAnalysis.Tests;

public static class VerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        UseSourceFileRelativeDirectory("Snapshots");
        VerifySourceGenerators.Initialize();
    }
}
