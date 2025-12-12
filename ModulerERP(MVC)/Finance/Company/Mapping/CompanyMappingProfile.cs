using AutoMapper;
using ModulerERP_MVC_.Finance.Company.ViewModels;

namespace ModulerERP_MVC_.Finance.Company.Mapping
{
    public class CompanyMappingProfile : Profile
    {
        public CompanyMappingProfile()
        {
            // Company -> CompanyListViewModel
            CreateMap<Models.Finance.Company, CompanyListViewModel>()
                .ForMember(dest => dest.CurrencyName,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Name : string.Empty))
                .ForMember(dest => dest.TreasuriesCount,
                    opt => opt.MapFrom(src => src.Treasuries.Count))
                .ForMember(dest => dest.BankAccountsCount,
                    opt => opt.MapFrom(src => src.BankAccounts.Count))
                .ForMember(dest => dest.VouchersCount,
                    opt => opt.MapFrom(src => src.Vouchers.Count));

            // Company -> CompanyDetailsViewModel
            CreateMap<Models.Finance.Company, CompanyDetailsViewModel>()
                .ForMember(dest => dest.CurrencyName,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Name : string.Empty))
                .ForMember(dest => dest.CurrencySymbol,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty))
                .ForMember(dest => dest.TreasuriesCount,
                    opt => opt.MapFrom(src => src.Treasuries.Count))
                .ForMember(dest => dest.BankAccountsCount,
                    opt => opt.MapFrom(src => src.BankAccounts.Count))
                .ForMember(dest => dest.GlAccountsCount,
                    opt => opt.MapFrom(src => src.GlAccounts.Count))
                .ForMember(dest => dest.VouchersCount,
                    opt => opt.MapFrom(src => src.Vouchers.Count));

            // CreateCompanyViewModel -> Company
            CreateMap<CreateCompanyViewModel, Models.Finance.Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Treasuries, opt => opt.Ignore())
                .ForMember(dest => dest.BankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.GlAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // UpdateCompanyViewModel -> Company
            CreateMap<UpdateCompanyViewModel, Models.Finance.Company>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Treasuries, opt => opt.Ignore())
                .ForMember(dest => dest.BankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.GlAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.Vouchers, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Company -> UpdateCompanyViewModel
            CreateMap<Models.Finance.Company, UpdateCompanyViewModel>();
        }
    }
}