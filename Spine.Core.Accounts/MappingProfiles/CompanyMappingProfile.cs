using AutoMapper;
using Spine.Common.Extensions;
using Spine.Common.Helper;
using Spine.Core.Accounts.Commands.Accounts;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.MappingProfiles
{
    public class CompanyMappingProfile : Profile
    {
        public CompanyMappingProfile()
        {
            CreateMap<Signup.Command, Company>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => SequentialGuid.Create(SequentialGuidType.SequentialAsString)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BusinessName.ToTitleCase()))
                .ForMember(dest => dest.LogoId, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()));

        }
    }
}
