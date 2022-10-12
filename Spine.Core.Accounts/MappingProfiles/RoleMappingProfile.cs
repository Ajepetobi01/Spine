using AutoMapper;
using Spine.Common.Helper;
using Spine.Common.Models;
using Spine.Core.Accounts.Commands.Roles;
using Spine.Core.Accounts.Queries.Roles;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.MappingProfiles
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            CreateMap<AddRole.Command, ApplicationRole>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
              // .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
          .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()));

            CreateMap<PermissionModel, GetRolePermissions.Model>();
        }
    }
}
