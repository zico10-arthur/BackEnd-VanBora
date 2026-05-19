namespace VanBora.Domain.Common;

public readonly struct Error
{
    public static readonly Error None = default;

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public IReadOnlyList<Error>? Errors { get; }

    public Error(string code, string message, ErrorType type = ErrorType.Validation, IReadOnlyList<Error>? errors = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Errors = errors;
    }

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error Validation(IReadOnlyList<Error> errors) =>
        new("VALIDATION_FAILED", "Um ou mais campos estão inválidos.", ErrorType.Validation, errors);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);
}
