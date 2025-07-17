
using AutoMapper;

namespace Infrastructure;

public static class Mapper
{
  static IMapper mapper;
  //static Mapper() => mapper = new AutoMapper.Mapper(MapperConfig.GetMapperConfig());
  static Mapper()
  {
    var config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<MapperConfig>(); // מוסיף את הפרופיל שכתבת
    });

    mapper = config.CreateMapper();
  }
  public static T_out Map<T_in, T_out>(T_in src, T_out dest) => mapper.Map(src, dest);

  public static T_out Map<T_in, T_out>(T_in src) => mapper.Map<T_out>(src);


  public static List<T_out> Map<T_in, T_out>(IEnumerable<T_in> src) => Map<T_in, T_out>(src.ToList());
  public static List<T_out> Map<T_in, T_out>(IQueryable<T_in> src) => Map<T_in, T_out>(src.ToList());
}
