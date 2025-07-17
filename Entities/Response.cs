using Models.Base.Models;
using Models.Project.Models;

namespace Models;

public class ProjectListResponse : ResponseBase<ResponseEnum, List<ProjectData>> { }
public class ProjectResponse : ResponseBase<ResponseEnum, ProjectData> { }
public class TaskResponse : ResponseBase<ResponseEnum, ProjectTask> { }

