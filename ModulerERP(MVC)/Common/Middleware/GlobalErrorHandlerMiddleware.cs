using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Common.Extensions;
using ModulerERP_MVC_.Common.ViewModel;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace ModulerERP_MVC_.Common.Middleware
{
    public class GlobalErrorHandlerMiddleware : IMiddleware
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly Serilog.ILogger Logger = Log.ForContext<GlobalErrorHandlerMiddleware>();

        public GlobalErrorHandlerMiddleware(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var traceId = context.TraceIdentifier;
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            // Add correlation ID to Serilog context
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].FirstOrDefault()))
            using (LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString()))
            {
                try
                {
                    Logger.Information("Request started");

                    // Add correlation ID to response headers
                    context.Response.Headers.Add("X-Correlation-Id", correlationId);

                    await next(context);

                    Logger.Information("Request completed successfully with status code {StatusCode}",
                        context.Response.StatusCode);
                }
                catch (BaseApplicationException appEx)
                {
                    await HandleApplicationExceptionAsync(context, appEx, traceId, correlationId);
                }
                catch (UnauthorizedAccessException unauthorizedEx)
                {
                    await HandleUnauthorizedExceptionAsync(context, unauthorizedEx, traceId, correlationId);
                }
                catch (Exception ex)
                {
                    await HandleUnexpectedExceptionAsync(context, ex, traceId, correlationId);
                }
            }
        }

        private async Task HandleApplicationExceptionAsync(HttpContext context, BaseApplicationException exception, string traceId, string correlationId)
        {
            using (LogContext.PushProperty("Module", exception.Module))
            using (LogContext.PushProperty("FinanceErrorCode", exception.FinanceErrorCode))
            using (LogContext.PushProperty("HttpStatusCode", exception.HttpStatusCode))
            {
                if (exception is BusinessLogicException)
                {
                    Logger.Warning(exception, "Business logic exception occurred: {Message}", exception.Message);
                }
                else if (exception is ValidationException validationEx)
                {
                    Logger.Warning(exception, "Validation exception occurred: {Message}. Validation errors: {@ValidationErrors}",
                        exception.Message, validationEx.ValidationErrors);
                }
                else
                {
                    Logger.Error(exception, "Application exception occurred: {Message}", exception.Message);
                }
            }

            object response;

            if (exception is ValidationException validationException && validationException.ValidationErrors.Any())
            {
                response = ResponseViewModel<object>.ValidationError(
                    validationException.Message,
                    validationException.ValidationErrors,
                    _environment.IsDevelopment() ? traceId : null);
            }
            else
            {
                response = ResponseViewModel<bool>.Error(
                    exception.Message,
                    exception.FinanceErrorCode,
                    _environment.IsDevelopment() ? traceId : null);
            }

            await WriteErrorResponseAsync(context, response, exception.HttpStatusCode, traceId, correlationId);
        }

        private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException exception, string traceId, string correlationId)
        {
            Logger.Warning(exception, "Unauthorized access attempt: {Message}", exception.Message);

            var response = ResponseViewModel<bool>.Error(
                "Access denied. You don't have permission to perform this action.",
                FinanceErrorCode.UnauthorizedAccess,
                _environment.IsDevelopment() ? traceId : null);

            await WriteErrorResponseAsync(context, response, StatusCodes.Status403Forbidden, traceId, correlationId);
        }

        private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception exception, string traceId, string correlationId)
        {
            Logger.Error(exception, "Unexpected error occurred");

            var response = ResponseViewModel<bool>.Error(
                _environment.IsDevelopment()
                    ? $"An unexpected error occurred. TraceId: {traceId}, CorrelationId: {correlationId}"
                    : "An unexpected error occurred. Please try again later.",
                FinanceErrorCode.InternalServerError,
                _environment.IsDevelopment() ? traceId : null);

            await WriteErrorResponseAsync(context, response, StatusCodes.Status500InternalServerError, traceId, correlationId);
        }

        private async Task WriteErrorResponseAsync(HttpContext context, object response, int statusCode, string traceId, string correlationId)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            context.Response.Headers.Add("X-Trace-Id", traceId);
            if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
            {
                context.Response.Headers.Add("X-Correlation-Id", correlationId);
            }
            else
            {
                context.Response.Headers["X-Correlation-Id"] = correlationId;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            await context.Response.WriteAsJsonAsync(response, jsonOptions);

            Logger.Information("Error response sent with status code {StatusCode}", statusCode);
        }
    }
}