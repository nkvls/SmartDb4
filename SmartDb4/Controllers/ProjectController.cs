//using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartDb4.Attributes;
using SmartDb4.DAL;
using SmartDb4.Enums;
using SmartDb4.Helpers;
using SmartDb4.Models;

namespace SmartDb4.Controllers
{
    [AuthorizeEnum(Role.Admin, Role.Staff)]
    public class ProjectController : Controller
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        //TODO : Mef IMPLEMENTATION
        //public ProjectController(IProjectRepository projectRepository)
        //{
        //    _projectRepository = projectRepository;
        //}


        //public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        public ActionResult Index(string sortOrder, string searchString)
        {
            try
            {
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? SortConstants.NameDesc : "";
                ViewBag.DateSortParm = sortOrder == SortConstants.Date ? SortConstants.DateDesc : SortConstants.Date;

                //if (searchString != null)
                //{
                //    page = 1;
                //}
                //else
                //{
                //    searchString = currentFilter;
                //}

                ViewBag.CurrentFilter = searchString;

                //var projects = _unitOfWork.ProjectRepository.Get(includeProperties: "Classification,Supervisor");
                var projects = GetAllProjects();

                if (!String.IsNullOrEmpty(searchString))
                {
                    projects = projects.Where(s => s.ProjectName.ToUpper().Contains(searchString.ToUpper()));
                }


                switch (sortOrder)
                {
                    case SortConstants.NameDesc:
                        projects = projects.OrderByDescending(s => s.ProjectName);
                        break;
                    case SortConstants.Date:
                        projects = projects.OrderBy(s => s.StartDate);
                        break;
                    case SortConstants.DateDesc:
                        projects = projects.OrderByDescending(s => s.StartDate);
                        break;
                    default:
                        projects = projects.OrderBy(s => s.ProjectName);
                        break;
                }

                //int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
                //int pageNumber = (page ?? 1);

                return View(projects.ToList());
                //return View(projects.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("ProjectController.Index() : " + ex, "Error");
                throw;
            }
        }


        public ActionResult Details(int id = 0)
        {
            try
            {
                Project project = _unitOfWork.ProjectRepository.GetById(id);
                if (project == null)
                {
                    return HttpNotFound();
                }
                //return View(project);
                return PartialView("Details", project);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("ProjectController.Details() : " + ex, "Error");
                throw;
            }
        }

        //
        // GET: /Project/Create
        public ActionResult Create()
        {
            try
            {
                ViewBag.SupervisorId = GetSupervisorSelectList(null);
                ViewBag.ClassificationId = GetClassificationSelectList(null);

                return View();
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("ProjectController.Create() : " + ex, "Error");
                throw;
            }
        }

        //
        // POST: /Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Project project)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var errors = IsDataValid(project);
                    if (errors.Length > 0)
                    {
                        ViewBag.Error = errors;
                    }
                    else
                    {
                        var userName = User.Identity.Name;
                        project.CreatedBy = userName;
                        project.CreatedOn = DateTime.UtcNow;

                        _unitOfWork.ProjectRepository.Insert(project);
                        _unitOfWork.Save();
                        //db.Projects.Add(project);
                        //db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    Utility.WriteToLog("ProjectController.Create() : " + ex, "Error");
                    throw;
                }
            }

            ViewBag.SupervisorId = GetSupervisorSelectList(project.SupervisorId);
            ViewBag.ClassificationId = GetClassificationSelectList(project.ClassificationId);

            return View(project);
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                var project = _unitOfWork.ProjectRepository.GetById(id);

                if (project == null)
                {
                    return HttpNotFound();
                }

                if (!User.IsInRole("admin") && project.Supervisor.UserName != User.Identity.Name)
                {
                    ViewBag.Error = "You do not have sufficient persmission to execute this operation. Contact system administrator for further details.";
                    var projects = GetAllProjects();
                    return View("Index", projects.ToList());
                }

                ViewBag.SupervisorId = GetSupervisorSelectList(project.SupervisorId);
                ViewBag.ClassificationId = GetClassificationSelectList(project.ClassificationId);

                return View(project);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("ProjectController.Edit() : " + ex, "Error");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var errors = IsDataValid(project);
                    if (errors.Length > 0)
                    {
                        ViewBag.Error = errors;
                    }
                    else
                    {
                        var userName = User.Identity.Name;
                        project.ModifiedBy = userName;
                        project.ModifiedOn = DateTime.UtcNow;

                        _unitOfWork.ProjectRepository.Update(project);
                        _unitOfWork.Save();
                        //db.Entry(project).State = EntityState.Modified;
                        //db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    Utility.WriteToLog("ProjectController.Edit() : " + ex, "Error");
                    throw;
                }
            }

            ViewBag.SupervisorId = GetSupervisorSelectList(project.SupervisorId);
            ViewBag.ClassificationId = GetClassificationSelectList(project.ClassificationId);

            return View(project);
        }

        //public ActionResult Delete(int id = 0)
        //{
        //    Project project = _unitOfWork.ProjectRepository.GetById(id);

        //    if (project == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(project);
        //}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                var project = _unitOfWork.ProjectRepository.GetById(id);

                //BUSINESS RULE : Project cannot be delete if members are assigned to it
                var membersAssignedToProject =
                    _unitOfWork.MemberToProjectsRepository.Get(filter: x => x.ProjectId == project.ProjectId);
                if (membersAssignedToProject.Any())
                {
                    return Json(new { Ok = false, ErrorMessage = "Cannot delete Project. The project is assigned to members" });
                }

                _unitOfWork.ProjectRepository.Delete(project);
                _unitOfWork.Save();

                return Json(new { Ok = true });
                //return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                Utility.WriteToLog("ProjectController.Delete() : " + ex, "Error");
                throw;
            }
        }

        
        public ActionResult AssignMembers(int id)
        {
            try
            {
                ViewBag.RoleId = GetRoleSelectList(null);

                var project = _unitOfWork.ProjectRepository.GetById(id);

                if (project == null)
                {
                    return HttpNotFound();
                }

                var membersToProjectColl = CreateMemberToProjectCollectionObject(project);

                return View(membersToProjectColl);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("ProjectController.AssignMembers() : " + ex, "Error");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignMembers(MembersToProjectCollection membersToProjectColl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var projectId = membersToProjectColl.AllCutdownMembers[0].ProjectId;

                    //BUSINESS RULE : Assign members cannot be greater than max no of participants assigned to the project
                    var project = _unitOfWork.ProjectRepository.GetById(projectId);
                    var maxAllowed = project.MaxNoOfParticipants;
                    var cnt = membersToProjectColl.AllCutdownMembers.Count(item => item.IsAssigned);
                    if (cnt > maxAllowed)
                    {
                        ViewBag.Error =
                            "You cannot assign members more than max allowed. The max allowed members limit is " +
                            maxAllowed;
                        var coll = CreateMemberToProjectCollectionObject(project);
                        return View("AssignMembers", coll);
                    }

                    
                        foreach (var item in membersToProjectColl.AllCutdownMembers)
                        {
                            var member = item;
                            MemberToProject memberToProject;

                            var memberToProjects = _unitOfWork.MemberToProjectsRepository.Get(filter: x => x.ProjectId == projectId && x.MemberId == member.MemberId).ToList();

                            if (memberToProjects.Count > 0)
                            {
                                memberToProject = memberToProjects[0];
                            }
                            else
                            {
                                memberToProject = GetEmptyMemberToProjectObject();
                            }

                            memberToProject.MemberToProjectId = item.MemberToProjectId;
                            memberToProject.ProjectId = item.ProjectId;
                            memberToProject.MemberId = item.MemberId;

                            memberToProject.AssignDate = item.AssignDate;
                            memberToProject.MemberRoleId = item.RoleId;
                            memberToProject.MemberRate = item.Rate;
                            memberToProject.IsAssigned = item.IsAssigned;

                            if (item.MemberAlreadyExists)
                            {
                                memberToProject.ModifiedBy = User.Identity.Name;
                                memberToProject.ModifiedOn = DateTime.UtcNow;
                            }

                            if (item.MemberAlreadyExists)
                            {
                                _unitOfWork.MemberToProjectsRepository.Update(memberToProject);
                            }
                            else
                            {
                                _unitOfWork.MemberToProjectsRepository.Insert(memberToProject);
                            }
                        }

                        _unitOfWork.Save();
                    
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    Utility.WriteToLog("ProjectController.AssignMembers() : " + ex, "Error");
                    throw;
                }
            }

            ViewBag.RoleId = GetRoleSelectList(membersToProjectColl.AllCutdownMembers[0].RoleId);

            return View(membersToProjectColl);
        }

        private MemberToProject GetEmptyMemberToProjectObject()
        {
            var memberToPro = new MemberToProject();
            memberToPro.CreatedBy = User.Identity.Name;
            memberToPro.CreatedOn = DateTime.UtcNow;

            return memberToPro;
        }

        private SelectList GetSupervisorSelectList(object selectedValue)
        {
            //return GenericSelectList.GetSelectList(SelectListEnums.User, "UserId", "UserName", selectedValue);
            var data =
                _unitOfWork.UserRepository.Get(filter: x => x.UserName.ToLower() != Constants.Superadmin,
                    orderBy: x => x.OrderBy(k => k.FirstName)).Where(x => Convert.ToInt32(x.UserTypeId) != (int)Role.Referrer);
            var dataToBind = from c in data
                             select new
                             {
                                UserId = c.UserId,
                                UserName = c.FirstName + " " + c.LastName,
                             };

            return new SelectList(dataToBind, "UserId", "UserName", selectedValue);
        }

        private SelectList GetClassificationSelectList(object selectedValue)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.Classification, "ClassificationId", "ClassificationName", selectedValue);
        }

        private SelectList GetRoleSelectList(object selectedValue)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.Roles, "RoleId", "RoleName", selectedValue);
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }

        private string IsDataValid(Project project)
        {
            var errorMessage = new StringBuilder();
            
            //end date cannot be less than start date
            if (project.EndDate.HasValue)
            {
                if (DateTime.Compare(project.EndDate.Value, project.StartDate) < 0)
                {
                    errorMessage.Append("End date cannot be less than start date");
                    errorMessage.Append(Environment.NewLine);
                }
            }

            if (project.SupervisorId <= 0)
            {
                errorMessage.Append("Please select Supervisor");
                errorMessage.Append(Environment.NewLine);
            }

            //max no of participants cannot be less than current members assgined to the project
            var memberAssigned = _unitOfWork.MemberToProjectsRepository.Get(filter: x => x.ProjectId == project.ProjectId);
            if (memberAssigned.Any())
            {
                if (memberAssigned.Count() > project.MaxNoOfParticipants)
                {
                    errorMessage.Append("This project has more assigned member than max no you are setting here. Cannot save");
                    errorMessage.Append(Environment.NewLine);
                }
            }

            return errorMessage.ToString();
        }

        private IEnumerable<Project> GetAllProjects()
        {
            var projects = _unitOfWork.ProjectRepository.Get(includeProperties: "Classification,Supervisor");

            var projectList = projects as IList<Project> ?? projects.ToList();


            //If the logged in user is admin then he has the permission to edit each user
            if (User.IsInRole("admin"))
            {
                foreach (var item in projectList)
                {
                    item.CanEdit = true;
                }
            }
            else
            {
                foreach (var item in projects)
                {
                    item.CanEdit = item.Supervisor.UserType.UserTypeId != (int)Role.Admin;
                }
            }

            return projectList;
        }

        private MembersToProjectCollection CreateMemberToProjectCollectionObject(Project project)
        {
            var membersToProjectColl = new MembersToProjectCollection();

            membersToProjectColl.Role =
                _unitOfWork.RoleRepository.Get(orderBy: x => x.OrderBy(k => k.MemberRoleName)).ToList();

            var allMembers = _unitOfWork.MemberRepository.Get(filter: f => f.IsSubmit && f.NominationId != 2, orderBy: x => x.OrderBy(k => k.MemberName)).ToList(); //2 == Old Member

            var newMemberList = allMembers.Select(member => new CutDownVersionOfMembers
            {
                MemberId = member.MemberId,
                MemberName = member.MemberName,
                AssignDate = DateTime.UtcNow,
                Rate = new decimal(0.0),
                ProjectId = project.ProjectId,
                MemberAlreadyExists = false,
                MemberToProjectId = 0
            }).ToList();

            membersToProjectColl.Project = project;
            membersToProjectColl.AllCutdownMembers = newMemberList;
            membersToProjectColl.AllMembers = allMembers;

            //var listOfMembersInAllMembersButNotInMemberToProject = new List<CutDownVersionOfMembers>();
            foreach (var item in _unitOfWork.MemberToProjectsRepository.Get())
            {
                var memberToProject = item;
                var isExists =
                    membersToProjectColl.AllCutdownMembers.Where(
                        m => m.MemberId == memberToProject.MemberId && m.ProjectId == memberToProject.ProjectId)
                        .ToList();
                if (isExists.Count > 0)
                {
                    isExists[0].AssignDate = memberToProject.AssignDate;
                    isExists[0].Rate = memberToProject.MemberRate;
                    isExists[0].RoleId = memberToProject.MemberRoleId;
                    isExists[0].MemberAlreadyExists = true;
                    isExists[0].IsAssigned = memberToProject.IsAssigned;
                    isExists[0].MemberToProjectId = memberToProject.MemberToProjectId;
                }

            }
            return membersToProjectColl;
        }
    }
}