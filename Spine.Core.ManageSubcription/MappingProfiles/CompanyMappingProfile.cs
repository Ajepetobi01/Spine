using AutoMapper;
using Spine.Common.Extensions;
using Spine.Common.Helper;
using Spine.Common.Models;
using Spine.Core.ManageSubcription.Filter;
using Spine.Core.ManageSubcription.ViewModel;
using Spine.Data.Entities;
using Spine.Data.Entities.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.MappingProfiles
{
    public class CompanyMappingProfile : Profile
    {
        public CompanyMappingProfile()
        {
            CreateMap<CompanyParam, Company>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BusinessName.ToTitleCase()))
                .ForMember(dest => dest.LogoId, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.OperatingSector, opt => opt.MapFrom(src => src.OperatingSector))
                .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => src.BusinessType))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()));

            

            CreateMap<CompanyParam, ApplicationUser>(MemberList.Source)
                .ForMember(x=>x.Id, d=>d.MapFrom(s=> SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(x=>x.FirstName, d=>d.MapFrom(s=>s.FirstName))
                .ForMember(x=>x.LastName, d=>d.MapFrom(s=>s.LastName))
                .ForMember(x=>x.FullName, d=>d.MapFrom(s=>$"{s.FirstName} {s.LastName}"))
                .ForMember(x=>x.UserName, d=>d.MapFrom(s=>s.Email))
                .ForMember(x=>x.NormalizedEmail, d=>d.MapFrom(s=>s.Email))
                .ForMember(x=>x.EmailConfirmed, d=>d.MapFrom(s=> false))
                .ForMember(x=>x.Gender, d=>d.MapFrom(s=>s.Gender));

            CreateMap<CompanyUploadModel, Company>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BusinessName.ToTitleCase()))
                .ForMember(dest => dest.LogoId, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()));

            CreateMap<CompanyUploadModel, ApplicationUser>(MemberList.Source)
                .ForMember(x => x.Id, d => d.MapFrom(s => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(x => x.FirstName, d => d.MapFrom(s => s.FirstName))
                .ForMember(x => x.LastName, d => d.MapFrom(s => s.LastName))
                .ForMember(x => x.FullName, d => d.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember(x => x.UserName, d => d.MapFrom(s => s.Email))
                .ForMember(x => x.NormalizedEmail, d => d.MapFrom(s => s.Email))
                .ForMember(x => x.EmailConfirmed, d => d.MapFrom(s => false))
                .ForMember(x => x.Gender, d => d.MapFrom(s => s.Gender));

            CreateMap<CompanyUploadModel, SubscriberBilling>(MemberList.Source)
               .ForMember(dest => dest.Address1, opt => opt.MapFrom(src => src.BillingAddress1))
               .ForMember(dest => dest.Address2, opt => opt.MapFrom(src => src.BillingAddress2))
               .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.BillingPostalCode))
               .ForMember(dest => dest.ID_Country, opt => opt.MapFrom(src => src.BillingID_Country))
               .ForMember(dest => dest.ID_State, opt => opt.MapFrom(src => src.BillingID_State));

            CreateMap<CompanyUploadModel, SubscriberShipping>(MemberList.Source)
               .ForMember(dest => dest.Address1, opt => opt.MapFrom(src => src.ShippingAddress1))
               .ForMember(dest => dest.Address2, opt => opt.MapFrom(src => src.ShippingAddress2))
               .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.ShippingPostalCode))
               .ForMember(dest => dest.ID_Country, opt => opt.MapFrom(src => src.ShippingID_Country))
               .ForMember(dest => dest.ID_State, opt => opt.MapFrom(src => src.ShippingID_State));

            CreateMap<UserVM, ApplicationUser>(MemberList.Source)
                .ForMember(x => x.Id, d => d.MapFrom(s => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(x => x.FirstName, d => d.MapFrom(s => s.FirstName))
                .ForMember(x => x.LastName, d => d.MapFrom(s => s.LastName))
                .ForMember(x => x.FullName, d => d.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember(x => x.UserName, d => d.MapFrom(s => s.Email))
                .ForMember(x => x.NormalizedEmail, d => d.MapFrom(s => s.Email))
                .ForMember(x => x.RoleId, d => d.MapFrom(s => s.Role))
                .ForMember(x => x.EmailConfirmed, d => d.MapFrom(s => false));


            CreateMap<RoleViewModel, ApplicationRole>(MemberList.Source)
                .ForMember(x => x.Id, d => d.MapFrom(s => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(x => x.IsSystemDefined, d => d.MapFrom(s => s.IsSubscriberRole))
                .ForMember(x => x.IsDeleted, d => d.MapFrom(s => false))
                .ForMember(x => x.Name, d => d.MapFrom(s => s.Role))
                .ForMember(x => x.Description, d => d.MapFrom(s => s.Role))
                .ForMember(x => x.NormalizedName, d => d.MapFrom(s => s.Description))
                .ForMember(x => x.IsOwnerRole, d => d.MapFrom(s => false));

            CreateMap<PaginationQuery, PaginationFilter>();
            CreateMap<GetAllPostQuery, GetAllPostFilter>();
            CreateMap<QueryAuditLog, FilterAuditLog>();
            CreateMap<RolePostQuery, RolePostFilter>();
            CreateMap<QueryAdminNotification, FilterAdminNotification>();
            CreateMap<PostUserQuery, PostUserFilter>();
            CreateMap<PostBirthDateQuery, PostBirthFilter>();
            CreateMap<PlanQuery, FilterPlan>();
            CreateMap<PromotionQuery, FilterPromotion>();
            CreateMap<PromotionalCodeQuery, FilterPromotionalCode>();
            CreateMap<PermissionModel, ModelPermissions>();
        }
    }
}
