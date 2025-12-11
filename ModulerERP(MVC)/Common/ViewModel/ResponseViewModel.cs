using ModulerERP_MVC_.Common.Enums.Finance_Enum;

namespace ModulerERP_MVC_.Common.ViewModel
{
    public class ResponseViewModel<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public FinanceErrorCode? FinanceErrorCode { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, string[]> ValidationErrors { get; set; }
        public DateTime Timestamp { get; set; }

        public ResponseViewModel()
        {
            Timestamp = DateTime.UtcNow;
        }

        public static ResponseViewModel<T> Success(T data, string message = "Operation completed successfully")
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }
        public static ResponseViewModel<T> Fail(string message, FinanceErrorCode? errorCode = null)
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = false,
                Message = message,
                FinanceErrorCode = errorCode,
                Data = default
            };
        }
        public static ResponseViewModel<T> Error(string message, Enums.Finance_Enum.FinanceErrorCode errorCode, string traceId = null)
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = false,
                Message = message,
                FinanceErrorCode = errorCode,
                TraceId = traceId
            };
        }

        public static ResponseViewModel<T> ValidationError(string message, Dictionary<string, string[]> validationErrors, string traceId = null)
        {
            return new ResponseViewModel<T>
            {
                IsSuccess = false,
                Message = message,
                FinanceErrorCode = Enums.Finance_Enum.FinanceErrorCode.ValidationError,
                ValidationErrors = validationErrors,
                TraceId = traceId
            };
        }
    }
}
