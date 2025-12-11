using ModulerERP_MVC_.Common.Enums.Finance_Enum;

namespace ModulerERP_MVC_.Common.Extensions
{
    public class BusinessLogicException : BaseApplicationException
    {
        public BusinessLogicException(string message, string module, FinanceErrorCode financeErrorCode = FinanceErrorCode.BusinessLogicError)
            : base(message, module, financeErrorCode, StatusCodes.Status400BadRequest)
        {
        }

        public BusinessLogicException(string message, Exception innerException, string module, FinanceErrorCode financeErrorCode = FinanceErrorCode.BusinessLogicError)
            : base(message, innerException, module, financeErrorCode, StatusCodes.Status400BadRequest)
        {
        }
    }
}