namespace SmartDb4.DAL.Repositories
{
    //public class ProjectRepository : GenericRepository<Project>, IProjectRepository, IDisposable
    //public class ProjectRepository : IProjectRepository, IDisposable
    //{
    //    private readonly SmartDbContext _context;

    //    public ProjectRepository(SmartDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public void Dispose()
    //    {
    //        //Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }

    //    public IEnumerable<Project> GetProjects()
    //    {
    //        return _context.Projects.Include(x => x.Classification).Include(x => x.Supervisor);
    //    }

    //    public Project GetProjectById(int projectId)
    //    {
    //        return _context.Projects.Find(projectId);
    //    }

    //    public void Insert(Project project)
    //    {
    //        _context.Projects.Add(project);
    //    }

    //    public void Delete(int projectId)
    //    {
    //        var project = _context.Projects.Find(projectId);
    //        _context.Projects.Remove(project);
    //    }

    //    public void Update(Project project)
    //    {
    //        _context.Entry(project).State = EntityState.Modified;
    //    }

    //    public void Save()
    //    {
    //        _context.SaveChanges();
    //    }
    //}
}