namespace CatCatGo.Network
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public int StatusCode { get; }
        public string ErrorMessage { get; }
        public bool IsOffline { get; }

        private ApiResponse(bool isSuccess, T data, int statusCode, string errorMessage, bool isOffline)
        {
            IsSuccess = isSuccess;
            Data = data;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            IsOffline = isOffline;
        }

        public static ApiResponse<T> Success(T data, int statusCode = 200)
        {
            return new ApiResponse<T>(true, data, statusCode, null, false);
        }

        public static ApiResponse<T> Fail(int statusCode, string errorMessage)
        {
            return new ApiResponse<T>(false, default, statusCode, errorMessage, false);
        }

        public static ApiResponse<T> FailWithData(T data, int statusCode, string errorMessage)
        {
            return new ApiResponse<T>(false, data, statusCode, errorMessage, false);
        }

        public static ApiResponse<T> Offline(string reason)
        {
            return new ApiResponse<T>(false, default, 0, reason, true);
        }
    }
}
