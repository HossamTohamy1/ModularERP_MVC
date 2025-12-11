using AutoMapper;
using ModulerERP_MVC_.Common.Enums.Finance_Enum;
using ModulerERP_MVC_.Common.Extensions;
using ModulerERP_MVC_.Common.ViewModel;
using ModulerERP_MVC_.Finance.Currencies.Repositories;
using ModulerERP_MVC_.Models.Finance;
using ModulerERP_MVC_.Models.Finance.DTOs;

namespace ModulerERP_MVC_.Finance.Currencies.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(
            ICurrencyRepository repository,
            IMapper mapper,
            ILogger<CurrencyService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseViewModel<IEnumerable<CurrencyDto>>> GetAllCurrenciesAsync()
        {
            var currencies = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CurrencyDto>>(currencies);

            return ResponseViewModel<IEnumerable<CurrencyDto>>.Success(
                dtos,
                "Currencies retrieved successfully");
        }

        public async Task<ResponseViewModel<CurrencyDto>> GetCurrencyByCodeAsync(string code)
        {
            var currency = await _repository.GetCurrencyByCodeAsync(code);

            if (currency == null)
                throw new NotFoundException(
                    $"Currency '{code}' not found",
                    FinanceErrorCode.CurrencyNotFound);

            return ResponseViewModel<CurrencyDto>.Success(
                _mapper.Map<CurrencyDto>(currency),
                "Currency retrieved successfully");
        }

        public async Task<ResponseViewModel<CreateCurrencyDto>> CreateCurrencyAsync(CreateCurrencyDto dto)
        {
            // ⭐ تأكد إن الـ Code uppercase
            dto.Code = dto.Code.ToUpperInvariant();

            // ⭐ شوف لو الـ Code موجود (حتى لو ممسوح)
            var existingCurrency = await _repository.GetCurrencyByCodeIncludingDeletedAsync(dto.Code);

            if (existingCurrency != null)
            {
                if (existingCurrency.IsDeleted)
                {
                    // ⭐ لو ممسوح، ارجعه (restore)
                    existingCurrency.Name = dto.Name;
                    existingCurrency.Symbol = dto.Symbol;
                    existingCurrency.Decimals = dto.Decimals;
                    existingCurrency.IsActive = dto.IsActive;
                    existingCurrency.IsDeleted = false;
                    existingCurrency.UpdatedAt = DateTime.UtcNow;

                    await _repository.UpdateAsync(existingCurrency);

                    return ResponseViewModel<CreateCurrencyDto>.Success(
                        dto,
                        "Currency restored and updated successfully");
                }
                else
                {
                    // ⭐ لو موجود فعلاً
                    throw new BusinessLogicException(
                        $"Currency code '{dto.Code}' already exists",
                        "Finance",
                        FinanceErrorCode.DuplicateCurrencyCode);
                }
            }

            // ⭐ لو مش موجود خالص، أضفه
            var currency = new Currency
            {
                Code = dto.Code,
                Name = dto.Name,
                Symbol = dto.Symbol,
                Decimals = dto.Decimals,
                IsActive = dto.IsActive,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(currency);

            return ResponseViewModel<CreateCurrencyDto>.Success(
                dto,
                "Currency created successfully");
        }

        public async Task<ResponseViewModel<UpdateCurrencyDto>> UpdateCurrencyAsync(UpdateCurrencyDto dto)
        {
            var existing = await _repository.GetCurrencyByCodeAsync(dto.Code);

            if (existing == null)
                throw new NotFoundException(
                    $"Currency '{dto.Code}' not found",
                    FinanceErrorCode.CurrencyNotFound);

            // ⭐ حدث البيانات
            existing.Name = dto.Name;
            existing.Symbol = dto.Symbol;
            existing.Decimals = dto.Decimals;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            return ResponseViewModel<UpdateCurrencyDto>.Success(
                dto,
                "Currency updated successfully");
        }

        public async Task<ResponseViewModel<bool>> DeleteCurrencyAsync(string code)
        {
            var currency = await _repository.GetCurrencyByCodeAsync(code);

            if (currency == null)
                throw new NotFoundException(
                    $"Currency '{code}' not found",
                    FinanceErrorCode.CurrencyNotFound);

            if (await _repository.IsInUseAsync(code))
                throw new BusinessLogicException(
                    $"Cannot delete currency '{code}', it is in use",
                    "Finance",
                    FinanceErrorCode.CurrencyInUse);

            await _repository.DeleteAsync(code);

            return ResponseViewModel<bool>.Success(
                true,
                "Currency deleted successfully");
        }
    }
}