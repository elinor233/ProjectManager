using AutoMapper;
using DB.Models.Project.Model;
using Models.Project.Models;

namespace Infrastructure;

public class MapperConfig : Profile
{
  public MapperConfig()
  {
    CreateMap<TbProject, ProjectData>()
        .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.TbTasks)); CreateMap<ProjectData, TbProject>();
    CreateMap<ProjectData, TbProject>()
    .ForMember(dest => dest.TbTasks, opt => opt.MapFrom(src => src.Tasks));

    //CreateMap<List<TbProject>, List<ProjectData>>();
    //CreateMap<List<ProjectData>, List<TbProject>>();

    CreateMap<TbTask, ProjectTask>();
    CreateMap<ProjectTask, TbTask>();

    //CreateMap<List<TbTask>, List<ProjectTask>>();
    //CreateMap<List<ProjectTask>, List<TbTask>>();
  }
}

