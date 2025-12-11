using ModulerERP_MVC_.Common.Enums.Finance_Enum;

namespace ModulerERP_MVC_.Common.Extensions
{
    public class NotFoundException : BaseApplicationException
    {
        private const string DefaultModule = "Finance";

        public NotFoundException(string message, FinanceErrorCode errorCode)
            : base(message, DefaultModule, errorCode, StatusCodes.Status404NotFound)
        {
        }

        public NotFoundException(string message, FinanceErrorCode errorCode, Exception innerException)
            : base(message, innerException, DefaultModule, errorCode, StatusCodes.Status404NotFound)
        {
        }
    }
}