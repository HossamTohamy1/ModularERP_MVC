using ModulerERP_MVC_.Common.ViewModel;
using ModulerERP_MVC_.Finance.Currencies.ViewModels;
using ModulerERP_MVC_.Models.Finance.DTOs;

namespace ModulerERP_MVC_.Finance.Currencies.Services
{
    public interface ICurrencyService
    {
        // للـ API - DTOs
        Task<ResponseViewModel<IEnumerable<CurrencyDto>>> GetAllCurrenciesAsync();
        Task<ResponseViewModel<CurrencyDto>> GetCurrencyByCodeAsync(string code);
        Task<ResponseViewModel<CreateCurrencyDto>> CreateCurrencyAsync(CreateCurrencyDto dto);
        Task<ResponseViewModel<UpdateCurrencyDto>> UpdateCurrencyAsync(UpdateCurrencyDto dto);
        Task<ResponseViewModel<bool>> DeleteCurrencyAsync(string code);

        // للـ Views - ViewModels
        Task<ResponseViewModel<IEnumerable<CurrencyListViewModel>>> GetAllCurrenciesForViewAsync();
    }
}