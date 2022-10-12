using System;
using System.Collections.Generic;
using AutoMapper;
using Spine.Common.Helper;
using Spine.Core.Accounts.Commands.Accounts;
using Spine.Core.Accounts.Commands.Users;
using Spine.Core.Accounts.Queries.Users;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<List<GetUsers.Model>, GetUsers.Response>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
                .ForMember(dest => dest.PageCount, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())
                .ForMember(dest => dest.PageLength, opt => opt.Ignore());
            
            CreateMap<Signup.Command, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
            .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email.ToLower()));


            CreateMap<InviteUser.Command, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
            //   .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            //   .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email.ToLower()));
        }
    }
}
