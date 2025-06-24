using AutoMapper;
using Bodde.EFPOC.Entities;

namespace Bodde.EFPOC.Tests
{
    internal class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<ProjectUpsertDto, Project>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ProjectProducts, opt => opt.MapFrom(src => src.ProductIds.Select(id => new ProjectProduct { ProductId = id })));
        }
    }
}
