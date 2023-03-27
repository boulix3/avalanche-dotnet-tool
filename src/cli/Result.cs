using OneOf;
using OneOf.Types;

namespace Avalanche.Cli;

[GenerateOneOf]
public partial class Result<T> : OneOfBase<T, Error<string>>, IResult
{
    public bool Success => IsT0;
    public string ErrorMsg => IsT1 ? AsT1.Value : string.Empty;
}

public interface IResult
{
    bool Success { get; }
    string ErrorMsg { get; }
}