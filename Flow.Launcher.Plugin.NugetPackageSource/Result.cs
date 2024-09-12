namespace Flow.Launcher.Plugin.NugetPackageSource
{
    public class Result
    {
        public bool Success { get; set; }
        public List<Error> Errors { get; set; }

        public static Result Ok()
            => new() { Success = true };

        public static Result Failure()
            => new() { Success = false };

        public static Result Failure(Error error)
            => new() { Success = false, Errors = new List<Error> { error } };

        public static Result Failure(IEnumerable<Error> errors)
            => new() { Success = false, Errors = errors.ToList() };

        public static implicit operator Result(Error error)
            => new() { Success = false, Errors = new List<Error> { error } };

        public static implicit operator Result(List<Error> errors)
            => new() { Success = false, Errors = errors };

        public Result AddError(Error error)
        {
            Errors ??= new List<Error>();
            Errors.Add(error);
            Success = false;
            return this;
        }
    }

    public class Result<TData> : Result
    {
        public TData Data { get; set; }

        public static Result<TData> Ok(TData data)
            => new() { Success = true, Data = data };

        public new static Result<TData> Failure()
            => new() { Success = false };

        public new static Result<TData> Failure(Error error)
            => new() { Success = false, Errors = new List<Error> { error } };

        public new static Result<TData> Failure(IEnumerable<Error> errors)
            => new() { Success = false, Errors = errors.ToList() };

        public static implicit operator Result<TData>(TData data)
            => new() { Success = true, Data = data };

        public static implicit operator Result<TData>(Error error)
            => new() { Success = false, Errors = new List<Error> { error } };

        public static implicit operator Result<TData>(List<Error> errors)
            => new() { Success = false, Errors = errors };
    }

    public class Error
    {
        public Error(ErrorCode errorCode, string description, string fieldName)
        {
            ErrorCode = errorCode;
            Description = description;
            FieldName = fieldName;
        }

        public ErrorCode ErrorCode { get; set; }
        public string FieldName { get; set; }
        public string Description { get; set; }
    }

    public enum ErrorCode : short
    {
        ModelStateNotValid = 0,
        FieldDataInvalid = 1,
        NotFound = 2,
        AccessDenied = 3,
        ErrorInIdentity = 4,
        Exception = 5,
    }
}