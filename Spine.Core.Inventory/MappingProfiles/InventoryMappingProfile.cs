using System;
using System.Collections.Generic;
using AutoMapper;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Core.Inventories.Commands;
using Spine.Core.Inventories.Commands.Product;
using Spine.Core.Inventories.Commands.Service;
using Spine.Core.Inventories.Queries;
using Spine.Core.Inventories.Queries.Product;
using Spine.Core.Inventories.Queries.Service;
using Spine.Data.Entities.Inventories;

namespace Spine.Core.Inventories.MappingProfiles
{
    public class InventoryMappingProfile : Profile
    {
        public InventoryMappingProfile()
        {
            CreateMap<List<GetProducts.Model>, GetProducts.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<List<GetProductCategories.Model>, GetProductCategories.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<List<GetProductAllocations.Model>, GetProductAllocations.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<List<GetInventoryLocations.Model>, GetInventoryLocations.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            
            CreateMap<List<GetServices.Model>, GetServices.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            
            CreateMap<AddProductCategory.Command, InventoryCategory>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Status.Active));

            CreateMap<AddInventoryLocation.Command, InventoryLocation>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Status.Active));

            CreateMap<UpdateInventoryLocation.Command, InventoryLocation>(MemberList.Source)
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
               .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<AddInventoryNote.Command, InventoryNote>(MemberList.Destination)
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                    .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
                    .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src => src.Id));

            CreateMap<AddProduct.Command, Inventory>(MemberList.Destination)
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
              .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
              .ForMember(dest => dest.LastRestockDate, opt => opt.MapFrom(src => src.InventoryDate))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => InventoryStatus.Active))
            .ForMember(dest => dest.InventoryType, opt => opt.MapFrom(src => InventoryType.Product));

            CreateMap<UpdateProduct.Command, Inventory>(MemberList.Source)
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
               .ForMember(dest => dest.Status, opt => opt.Ignore())
               .ForMember(dest => dest.QuantityInStock, opt => opt.Ignore())
               .ForMember(dest => dest.LastRestockDate, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitCostPrice, opt => opt.Ignore());

            CreateMap<AddBulkProduct.ProductModel, Inventory>(MemberList.Destination)
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
       .ForMember(dest => dest.SerialNo, opt => opt.MapFrom(src => src.SerialNumber))
       .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.StockKeepingUnit))
              .ForMember(dest => dest.LastRestockDate, opt => opt.MapFrom(src => src.InventoryDate))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => InventoryStatus.Active))
            .ForMember(dest => dest.InventoryType, opt => opt.MapFrom(src => InventoryType.Product))
          .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => src.Quantity))
          .ForMember(dest => dest.UnitCostPrice, opt => opt.MapFrom(src => src.CostPrice))
          .ForMember(dest => dest.UnitSalesPrice, opt => opt.MapFrom(src => src.SalesPrice));



            CreateMap<AddService.Command, Inventory>(MemberList.Source)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => 1))
            .ForMember(dest => dest.ReorderLevel, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.UnitCostPrice, opt => opt.MapFrom(src => 0.00m))
            .ForMember(dest => dest.InventoryDate, opt => opt.MapFrom(src => DateTime.Today))
            .ForMember(dest => dest.LastRestockDate, opt => opt.MapFrom(src => DateTime.Today))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => InventoryStatus.Active))
          .ForMember(dest => dest.InventoryType, opt => opt.MapFrom(src => InventoryType.Service));

            CreateMap<AddBulkServices.ServiceModel, Inventory>(MemberList.Source)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
            .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => 1))
            .ForMember(dest => dest.ReorderLevel, opt => opt.MapFrom(src => 0))
          .ForMember(dest => dest.InventoryDate, opt => opt.MapFrom(src => DateTime.Today))
          .ForMember(dest => dest.LastRestockDate, opt => opt.MapFrom(src => DateTime.Today))
          .ForMember(dest => dest.Status, opt => opt.MapFrom(src => InventoryStatus.Active))
        .ForMember(dest => dest.InventoryType, opt => opt.MapFrom(src => InventoryType.Service))
            .ForMember(dest => dest.UnitCostPrice, opt => opt.MapFrom(src => 0.00m))
         .ForMember(dest => dest.UnitSalesPrice, opt => opt.MapFrom(src => src.SalesPrice));

        }
    }
}
