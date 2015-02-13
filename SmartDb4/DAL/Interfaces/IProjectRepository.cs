using System;
using System.Collections.Generic;
using SmartDb4.Models;

namespace SmartDb4.DAL.Interfaces
{
    public interface IProjectRepository : IDisposable
    {
        IEnumerable<Project> GetProjects();
        Project GetProjectById(int projectId);
        void Insert(Project project);
        void Delete(int projectId);
        void Update(Project project);
        void Save();
    }
}
