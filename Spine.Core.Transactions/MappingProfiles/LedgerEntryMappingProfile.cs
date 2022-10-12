using System;
using System.Collections.Generic;
using AutoMapper;
using Spine.Core.Transactions.Queries.Reports;

namespace Spine.Core.Transactions.MappingProfiles
{
    public class LedgerEntryMappingProfile : Profile
    {
        public LedgerEntryMappingProfile()
        {
            CreateMap<List<AllLedgerEntries.Model>, AllLedgerEntries.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());
            
        }
    }
}
