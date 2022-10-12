using System.Collections.Generic;
using AutoMapper;
using Spine.Common.Helper;
using Spine.Core.Customers.Commands;
using Spine.Core.Customers.Queries;
using Spine.Data.Entities;

namespace Spine.Core.Customers.MappingProfiles
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            CreateMap<List<GetCustomers.Model>, GetCustomers.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());

            CreateMap<AddCustomer.Command, Customer>(MemberList.Destination)
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
              .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
              .ForMember(dest => dest.BusinessType, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()));
            
            
            CreateMap<UpdateCustomer.Command, Customer>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                 .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()));

            CreateMap<AddBulkCustomer.CustomerModel, Customer>(MemberList.Destination)
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CustomerName))
           .ForMember(dest => dest.TIN, opt => opt.MapFrom(src => src.TaxIdentificationNumber))
              .ForMember(dest => dest.BusinessType, opt => opt.Ignore())
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress.Trim().ToLower()));

            //CreateMap<AddBulkCustomer.CustomerModel, AddBulkCustomer.AddressModel>()
            //    .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(model => model))
            //    .ForMember(dest => dest.BillingAddress, opt => opt.MapFrom(model => model));


            CreateMap<AddBulkCustomer.CustomerModel, AddBulkCustomer.AddressModel>(MemberList.Destination)
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


            CreateMap<AddCustomerNote.Command, CustomerNote>(MemberList.Destination)
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
           .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
           .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id));

            CreateMap<AddCustomerReminder.Command, CustomerReminder>(MemberList.Destination)
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
        .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
        .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Id));
            
            CreateMap<AddCustomerAddress.Command, CustomerAddress>(MemberList.Destination)
         .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
         .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
         .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId));
            
            CreateMap<UpdateCustomerAddress.Command, CustomerAddress>()
          .ForMember(dest => dest.Id, opt => opt.Ignore())
          .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UserId));
        }
    }
}
