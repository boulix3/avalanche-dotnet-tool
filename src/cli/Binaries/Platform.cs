using System.Runtime.InteropServices;

using OneOf.Types;

namespace Avalanche.Cli.Binaries;

public static class Platform
{
    public static Result<BinaryType> PlatformType { get; } = GetCurrentPlatformBinaryType();

    private static Result<BinaryType> GetCurrentPlatformBinaryType()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
            RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return BinaryType.X64OSX;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                return BinaryType.X64Linux;
            }

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                return BinaryType.Arm64Linux;
            }
        }

        string error =
            $"Unsupported platform (OS: {RuntimeInformation.RuntimeIdentifier} | ProcessArchitecture: {RuntimeInformation.ProcessArchitecture})";
        return new Error<string>(error);
    }
}