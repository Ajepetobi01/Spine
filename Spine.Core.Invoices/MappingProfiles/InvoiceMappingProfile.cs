using System.Collections.Generic;
using AutoMapper;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Core.Invoices.Commands;
using Spine.Core.Invoices.Queries;
using Spine.Data.Entities;
using Spine.Data.Entities.Invoices;

namespace Spine.Core.Invoices.MappingProfiles
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            CreateMap<List<GetInvoices.Model>, GetInvoices.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<List<GetInvoicePayments.Model>, GetInvoicePayments.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<AddInvoice.Command, Invoice>(MemberList.Destination)
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
              .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
              .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => InvoiceStatus.Generated))
            .ForMember(dest => dest.InvoiceNoString, opt => opt.Ignore())
            .ForMember(dest => dest.BaseCurrencyId, opt => opt.Ignore());

            CreateMap<DownloadInvoicePreview.Command, Invoice>(MemberList.Destination)
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
           .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
           .ForMember(dest => dest.InvoiceStatus, opt => opt.Ignore())
         .ForMember(dest => dest.InvoiceNoString, opt => opt.Ignore())
         .ForMember(dest => dest.BaseCurrencyId, opt => opt.Ignore());

            CreateMap<AddInvoice.LineItemModel, LineItem>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
            .ForMember(dest => dest.Amount, opt => opt.Ignore())
            .ForMember(dest => dest.ItemId, opt => opt.MapFrom(src=>src.InventoryId))
            .ForMember(dest => dest.ParentItemId, opt => opt.Ignore())
            .ForMember(dest => dest.CompanyId, opt => opt.Ignore());

        }
    }
}
