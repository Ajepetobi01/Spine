using AutoMapper;
using Spine.Common.Models;
using Spine.Services.Paystack.Misc;

namespace Spine.Core.Transactions.MappingProfiles
{
    public class BanksMappingProfile : Profile
    {
        public BanksMappingProfile()
        {
            CreateMap<ListBanks.Model, BanksModel>(MemberList.Destination)
              .ForMember(dest => dest.BankCode, opt => opt.MapFrom(src => src.Code))
              //    .ForMember(dest => dest.Currency, opt => opt.ConvertUsing(new CurrencyConverter(), src => src.Currency))
              .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Name));
        }

        //public class CurrencyConverter : IValueConverter<string, Currency?>
        //{
        //    public Currency? Convert(string source, ResolutionContext context)
        //        => ConvertPaystackCurrencyToEnum(source);

        //    private Currency ConvertPaystackCurrencyToEnum(string currency)
        //    {
        //        return currency.ToLower() switch
        //        {
        //            "ngn" => Currency.Naira, //when other bank types are supported, they should have their own cases

        //            _ => Currency.Naira,
        //        };
        //    }
        //}


        //public class CurrencyResolver : IValueResolver<ListBanks.Model, BanksModel, int>
        //{
        //    public int Resolve(ListBanks.Model source, BanksModel destination, int member, ResolutionContext context)
        //    {
        //        return source.Currency.ToLower() switch
        //        {
        //            "ngn" => (int)Currency.Naira, //when other bank types are supported, they should have their own cases

        //            _ => (int)Currency.Naira,
        //        };
        //    }
        //}

    }
}
