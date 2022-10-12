using System.Collections.Generic;
using AutoMapper;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Core.Inventories.Commands.Vendor;
using Spine.Core.Inventories.Queries.Vendor;
using Spine.Data.Entities.Vendor;

namespace Spine.Core.Inventories.MappingProfiles
{
    public class VendorMappingProfile : Profile
    {
        public VendorMappingProfile()
        {
            CreateMap<List<GetVendors.Model>, GetVendors.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<List<GetVendorPayments.Model>, GetVendorPayments.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<AddVendor.Command, Vendor>(MemberList.Destination)
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()));
            
            CreateMap<UpdateVendor.Command, Vendor>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()));

            
              CreateMap<AddBulkVendor.VendorModel, Vendor>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
           .ForMember(dest => dest.TIN, opt => opt.MapFrom(src => src.TaxIdentificationNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress.Trim().ToLower()));
              
              
            CreateMap<AddBulkVendor.VendorModel, AddBulkVendor.AddressModel>(MemberList.Destination)
           .ForPath(dest => dest.BillingAddress.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
           .ForPath(dest => dest.BillingAddress.AddressLine1, opt => opt.MapFrom(src => src.BillingAddressLine1))
           .ForPath(dest => dest.BillingAddress.AddressLine2, opt => opt.MapFrom(src => src.BillingAddressLine2))
           .ForPath(dest => dest.BillingAddress.State, opt => opt.MapFrom(src => src.BillingState))
           .ForPath(dest => dest.BillingAddress.Country, opt => opt.MapFrom(src => src.BillingCountry))
           .ForPath(dest => dest.BillingAddress.PostalCode, opt => opt.MapFrom(src => src.BillingPostalCode))

           .ForPath(dest => dest.ShippingAddress.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
              .ForPath(dest => dest.ShippingAddress.AddressLine1, opt => opt.MapFrom(src => src.ShippingAddressLine1))
           .ForPath(dest => dest.ShippingAddress.AddressLine2, opt => opt.MapFrom(src => src.ShippingAddressLine2))
           .ForPath(dest => dest.ShippingAddress.State, opt => opt.MapFrom(src => src.ShippingState))
           .ForPath(dest => dest.ShippingAddress.Country, opt => opt.MapFrom(src => src.ShippingCountry))
           .ForPath(dest => dest.ShippingAddress.PostalCode, opt => opt.MapFrom(src => src.ShippingPostalCode));

            
            CreateMap<AddVendorAddress.Command, VendorAddress>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => src.VendorId));
            
            CreateMap<UpdateVendorAddress.Command, VendorAddress>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VendorId, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UserId));

        }
    }
}