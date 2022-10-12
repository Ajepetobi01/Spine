using System.Collections.Generic;
using AutoMapper;
using Spine.Core.Inventories.Queries.Order;

namespace Spine.Core.Inventories.MappingProfiles
{
    public class PurchaseOrderMappingProfile : Profile
    {
        public PurchaseOrderMappingProfile()
        {
            CreateMap<List<GetPurchaseOrders.Model>, GetPurchaseOrders.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<List<GetReceivedGoods.Model>, GetReceivedGoods.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            
            //CreateMap<AddInvoice.Command, Invoice>(MemberList.Destination)
            //  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
            //  .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
            //  .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => InvoiceStatus.Generated))
            //.ForMember(dest => dest.InvoiceNoString, opt => opt.Ignore());

            //CreateMap<AddInvoice.LineItemModel, LineItem>(MemberList.Destination)
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
            //.ForMember(dest => dest.Amount, opt => opt.Ignore())
            //.ForMember(dest => dest.ParentItemId, opt => opt.Ignore())
            //.ForMember(dest => dest.CompanyId, opt => opt.Ignore());


        }
    }
}
