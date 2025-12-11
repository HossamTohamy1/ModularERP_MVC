using AutoMapper;
using ModulerERP_MVC_.Models.Finance;
using ModulerERP_MVC_.Models.Finance.DTOs;

namespace ModulerERP_MVC_.Finance.Currencies.Mapping
{
    public class CurrencyMappingProfile : Profile
    {
        public CurrencyMappingProfile()
        {
            // Currency → CurrencyDto
            CreateMap<Currency, CurrencyDto>();

            // CreateCurrencyDto → Currency
            CreateMap<CreateCurrencyDto, Currency>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore())
                .ForMember(dest => dest.Treasuries, opt => opt.Ignore())
                .ForMember(dest => dest.BankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.LedgerEntries, opt => opt.Ignore());

            // UpdateCurrencyDto → Currency
            CreateMap<UpdateCurrencyDto, Currency>()
                .ForMember(dest => dest.Code, opt => opt.Ignore()) 
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore())
                .ForMember(dest => dest.Treasuries, opt => opt.Ignore())
                .ForMember(dest => dest.BankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.LedgerEntries, opt => opt.Ignore());
        }
    }
}