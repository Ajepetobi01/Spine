using System;
using System.Collections.Generic;
using AutoMapper;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.BillsPayments.Queries;
using Spine.Data.Entities.BillsPayments;
using Spine.Services.Interswitch;

namespace Spine.Core.BillsPayments.MappingProfiles
{
    public class BillsPaymentsMappingProfile : Profile
    {
        public BillsPaymentsMappingProfile()
        {

            CreateMap<List<GetBillPayments.Model>, GetBillPayments.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            
            CreateMap<GetBillerPaymentItems.Model, BillPayment>(MemberList.Source)
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                 .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => Constants.GetCurrentDateTime(TimeZoneInfo.Local)));
            

        }
    }
}
