namespace CatCatGo.Domain.ValueObjects
{
    public class Result
    {
        public bool Success { get; }
        public string Message { get; }

        protected Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static Result Ok(string message = "")
        {
            return new Result(true, message);
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Ok<T>(T data, string message = "")
        {
            return new Result<T>(true, message, data);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(false, message, default);
        }

        public bool IsOk() => Success;
        public bool IsFail() => !Success;
    }

    public class Result<T> : Result
    {
        public T Data { get; }

        internal Result(bool success, string message, T data) : base(success, message)
        {
            Data = data;
        }
    }
}
