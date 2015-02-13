using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using DotNetOpenAuth.Messaging;
using Elmah;
using Newtonsoft.Json;
using SmartDb4.Attributes;
using SmartDb4.DAL;
using SmartDb4.Enums;
using SmartDb4.Extensions;
using SmartDb4.Helpers;
using SmartDb4.Models;
using WebGrease.Css.Extensions;

namespace SmartDb4.Controllers
{
    [AuthorizeEnum(Role.Referrer, Role.Admin, Role.Staff)]
    public class MembershipDataController : Controller
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        private bool _isSubmit;

        public ActionResult Index(string sortOrder, string searchString)
        {
            try
            {
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? SortConstants.NameDesc : "";
                ViewBag.DateSortParm = sortOrder == SortConstants.Date ? SortConstants.DateDesc : SortConstants.Date;

                var members = GetAllMembers();


                if (!String.IsNullOrEmpty(searchString))
                {
                    members = members.Where(s => s.MemberName.ToUpper().Contains(searchString.ToUpper()));
                }

                switch (sortOrder)
                {
                    case SortConstants.NameDesc:
                        members = members.OrderByDescending(s => s.MemberName);
                        break;
                    case SortConstants.Date:
                        members = members.OrderBy(s => s.StartDate);
                        break;
                    case SortConstants.DateDesc:
                        members = members.OrderByDescending(s => s.StartDate);
                        break;
                    default:
                        members = members.OrderBy(s => s.MemberName);
                        break;
                }

                return View(members.ToList());
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("MembershipDataController.Index() : " + ex, "Error");
                throw;
            }
        }

        public ActionResult Details(int id = 0)
        {
            try
            {
                var member = _unitOfWork.MemberRepository.GetById(id);

                if (member == null)
                {
                    return HttpNotFound();
                }

                return View(member);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("MembershipDataController.Details() : " + ex, "Error");
                throw;
            }
        }

        [AuthorizeEnum(Role.Admin, Role.Referrer)]
        public ActionResult Create()
        {
            try
            {
                var member = AddAgentDetailsToMember(new Member());

                AddViewDataSelectLists(member);

                member = AddNotesToMember(member);

                member.FormSubmitDate = DateTime.UtcNow;

                var projects = _unitOfWork.ProjectRepository.Get(includeProperties: "Classification,Supervisor",
                                                                 orderBy: x => x.OrderBy(k => k.ProjectName));

                member.ProjectMembership = new ProjectMembership { Projects = projects.ToList() };

                member.CurrentWeekAttendances = _unitOfWork.MemberAttendanceRepository.GetEmptyObjectCollection();

                UpdateMemberAttendanceRatesToDefault(member);

                //PutTypeDropDownInto(member);

                return View(member);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("MembershipDataController.Create() : " + ex, "Error");
                throw;
            }
        }

        [AuthorizeEnum(Role.Admin, Role.Referrer)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Save")]
        public ActionResult Create(Member member)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isUserInReferrerRole = User.IsInRole(Role.Referrer.ToString());

                    //Validate if Ethnicity is of AnyOther type, then check if some value in 'Other (please specify) is present'
                    //AND if not then set the error message back to the user
                    if ((member.GroupedEthnicityId == (int) EthnicityEnum.OtherWhiteBackground ||
                         member.GroupedEthnicityId == (int) EthnicityEnum.AnyOtherBlackbackground ||
                         member.GroupedEthnicityId == (int) EthnicityEnum.AnyOtherMixedMultipleEthnicBackground ||
                         member.GroupedEthnicityId == (int) EthnicityEnum.AnyotherAsianBackground ||
                         member.GroupedEthnicityId == (int) EthnicityEnum.Other) &&
                        string.IsNullOrWhiteSpace(member.OtherSpecify))
                    {
                        //RAISE ERROR AND SET  OTHER AS REQUIRED FIELD
                        ViewBag.Error = "[Monitoring Information] Select a valid value from Ethnicity or provide input in 'Other (Please specify)' input box";

                        AddViewDataSelectLists(member);

                        //PutTypeDropDownInto(member);
                        return View(member);

                    }

                    var currentDate = GetCurrentDate();
                    member.FormSubmitDate = currentDate;
                    member.ApplicationDate = member.FormSubmitDate;
                    member.CreatedBy = GetCurrentUserName();
                    member.CreatedOn = currentDate;
                    if (member.AgentId == 0)
                    {
                        member.AgentId = GetCurrentUserId();                        
                    }

                    if (_isSubmit)
                    {
                        member.IsSubmit = true;
                    }

                    using (var scope = new TransactionScope())
                    {
                        try
                        {
                            //get agent user profile
                            var agentDetails = _unitOfWork.UserRepository.GetById(member.AgentId);
                            var relationshipWithApplicant = member.AgentDetails.RelationshipWithApplicant;
                            member.AgentDetails = agentDetails;
                            member.AgentDetails.RelationshipWithApplicant = relationshipWithApplicant;
                            member.RelationshipToClient = relationshipWithApplicant;
                            member.AgentDetails.Password = "password";
                            if (member.GroupedEthnicityId != null) member.EthnicityId = member.GroupedEthnicityId.Value;

                            CreateFundingResponsibilityOnTheFly(member);
                           
                            _unitOfWork.MemberRepository.Insert(member);
                            _unitOfWork.Save();

                            InsertNotes(member);

                            UploadFiles(member);

                            if (isUserInReferrerRole)
                            {
                                InsertAdminAlert(member);
                            }

                            scope.Complete();

                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                //Utility.WriteToLog("MembershipDataController.Create() : " + ex.Message, "Error");
                                Utility.WriteToLog(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State), "Error");
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    Utility.WriteToLog(string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage), "Error");
                                }
                            }
                            throw;
                        }
                        catch (Exception ex)
                        {
                            scope.Dispose();
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                            Utility.WriteToLog("MembershipDataController.Create() : " + ex, "Error");
                            throw;
                        }
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Utility.WriteToLog("MembershipDataController.Create() : " + ex, "Error");
                    throw;
                }
            }

            AddViewDataSelectLists(member);

            //PutTypeDropDownInto(member);
            return View(member);
        }

        [HttpGet]
        public JsonResult LoadReferee(string refereeId)
        {
            int agentId;
            int.TryParse(refereeId, out agentId);
            var user = _unitOfWork.UserRepository.GetById(agentId);
            var data = new UserProfile
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Address1 = user.Address1,
                Address2 = user.Address2,
                Agency = user.Agency,
                City = user.City,
                PostCode = user.PostCode,
                WorkTelephone = user.WorkTelephone,
                MobileTelephone = user.MobileTelephone,
                RelationshipWithApplicant = user.RelationshipWithApplicant
            };

            return Json(JsonConvert.SerializeObject(data), JsonRequestBehavior.AllowGet);
           
        }

        [AuthorizeEnum(Role.Admin, Role.Referrer)]
        public ActionResult Edit(int id = 0)
        {
            try
            {
                var member = _unitOfWork.MemberRepository.GetById(id);

                if (member == null)
                {
                    return HttpNotFound();
                }

                AddViewDataSelectLists(member);

                if (!(User.IsInRole("admin") || User.IsInRole("staff")) && member.CreatedBy != User.Identity.Name)
                {
                    ViewBag.Error = "You do not have sufficient persmission to execute this operation. Contact system administrator for further details.";
                    var members = GetAllMembers();
                    return View("Index", members.ToList());
                }

                member = AddAgentDetailsToMember(member);

                member = AddNotesToMember(member);

                member = AddMemberToProjectDetails(member);

                var memberAttendanceDatas = _unitOfWork.MemberAttendanceRepository.Get(filter: x => x.MemberId == member.MemberId && member.IsSubmit, includeProperties: "Attendance").ToList();
                if (memberAttendanceDatas.Count > 0)
                {
                    member.CurrentWeekAttendances = memberAttendanceDatas;
                }
                else
                {
                    member.CurrentWeekAttendances = _unitOfWork.MemberAttendanceRepository.GetEmptyObjectCollection();
                }

                UpdateMemberAttendanceRatesToDefault(member);

                //var mondayDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                var sundayDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                member.MondayDate = sundayDate.AddDays(1).ToShortDateString();
                member.TuesdayDate = sundayDate.AddDays(2).ToShortDateString();
                member.WednesdayDate = sundayDate.AddDays(3).ToShortDateString();
                member.ThursdayDate = sundayDate.AddDays(4).ToShortDateString();
                member.FridayDate = sundayDate.AddDays(5).ToShortDateString();
                member.SaturdayDate = sundayDate.AddDays(6).ToShortDateString();
                member.SundayDate = sundayDate.AddDays(7).ToShortDateString();

                //PutTypeDropDownInto(member);


                //Since funding responsibility becomes a part of master table on each save, we need to make sure the text field is empty each time in edit mode
                member.OtherFundingResponsibility = string.Empty;

                return View(member);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("MembershipDataController.Edit() : " + ex, "Error");
                throw;
            }
        }

        [AuthorizeEnum(Role.Admin, Role.Referrer)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Edit")]
        public ActionResult Edit(Member member)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isUserInReferrerRole = User.IsInRole(Role.Referrer.ToString());
                    //member.AgentDetails = null;
                    member.ModifiedBy = GetCurrentUserName();
                    member.ModifiedOn = GetCurrentDate();
                    if (member.AgentId == 0)
                    {
                        member.AgentId = GetCurrentUserId();
                    }

                    if (_isSubmit)
                    {
                        member.IsSubmit = true;
                    }
                    using (var scope = new TransactionScope())
                    {
                        try
                        {
                            //get agent user profile
                            var agentDetails = _unitOfWork.UserRepository.GetById(member.AgentId);
                            var relationshipWithApplicant = member.AgentDetails.RelationshipWithApplicant;
                            member.RelationshipToClient = relationshipWithApplicant;
                            member.AgentDetails = agentDetails;

                            CreateFundingResponsibilityOnTheFly(member);

                            _unitOfWork.MemberRepository.Update(member);
                            _unitOfWork.Save();

                            InsertNotes(member);

                            UploadFiles(member);

                            if (isUserInReferrerRole)
                            {
                                InsertAdminAlert(member);
                            }
                            

                            scope.Complete();
                        }
                        catch (Exception ex)
                        {
                            scope.Dispose();
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");

                            //TODO: implement some logging and exception handling here
                            throw new Exception(ex.Message);
                        }
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Utility.WriteToLog("MembershipDataController.Edit() : " + ex, "Error");
                    throw;
                }
            }

            AddViewDataSelectLists(member);

            return View(member);
        }

        [AuthorizeEnum(Role.Admin, Role.Referrer)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Submit")]
        public ActionResult Submit(Member member)
        {
            if (ModelState.IsValid)
            {
                _isSubmit = true;
                member.ModifiedBy = GetCurrentUserName();
                member.ModifiedOn = GetCurrentDate();
                if (member.MemberId > 0)
                {
                    Edit(member);
                }
                else
                {
                    Create(member);
                }
            }
            return RedirectToAction("Index");
        }

        //public ActionResult Delete(int id = 0)
        //{
        //    try
        //    {
        //        var member = _unitOfWork.MemberRepository.GetById(id);

        //        if (member == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        return View(member);
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.WriteToLog("MembershipDataController.Edit() : " + ex.Message, LogLevel.Error);
        //        throw;
        //    }
        //}

        [AuthorizeEnum(Role.Admin, Role.Referrer)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var member = _unitOfWork.MemberRepository.GetById(id);

                    //BUSINESS RULE : Apply if there is any business logic you need to implement
                    //var membersAssignedToProject =
                    //_unitOfWork.MemberToProjectsRepository.Get(filter: x => x.ProjectId == project.ProjectId);
                    //if (membersAssignedToProject.Any())
                    //{
                    //    return Json(new { Ok = false, ErrorMessage = "Cannot delete Project. The project is assigned to members" });
                    //}

                    var notes = _unitOfWork.NotesRepository.Get(filter: x => x.MemberId == member.MemberId).ToList();
                    foreach (var item in notes)
                    {
                        _unitOfWork.NotesRepository.Delete(item);    
                    }

                    _unitOfWork.MemberRepository.Delete(member);

                    _unitOfWork.Save();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists see your system administrator.");
                    Utility.WriteToLog("MembershipDataController.Delete()" + ex, "Error");
                    throw;
                }
            }

            //return RedirectToAction("Index");
            return Json(new { Ok = true });
            
        }

        [HttpGet]
        public FileResult DownloadFile(int memberId, string fileName)
        {
            var files = _unitOfWork.BinaryFileRepository.Get(filter: x => x.MemberId == memberId && x.FileName == fileName);   //.Include(x => x.BinaryFilesList).ToList()[0];

            if (!files.Any())
            {
                ModelState.AddModelError("Error", "No file found with such name.");
                return null;
            }
            //var fileFromDb = data.BinaryFilesList.FirstOrDefault(x => String.Equals(x.FileName, fileName, StringComparison.CurrentCultureIgnoreCase));
            
            var fileDetailsFromDb = files.ToList()[0];
            
            var content = fileDetailsFromDb.FileContent;
            var ext = fileDetailsFromDb.FileExtension;
            var file = fileDetailsFromDb.FileName;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName,
                // always prompt the user for downloading, 
                //set to true if you want the browser to try to show the file inline
                Inline = false,
            };
            
            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(content, "application/force-download", file);
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }

        //private void PutTypeDropDownInto(Member model)
        //{
        //    model.GroupedEthnicityOptions = GetGroupedEthnicitySelectList(model.EthnicityId);
        //}

        private string GetCurrentUserName()
        {
            return User.Identity.Name;
        }

        private DateTime GetCurrentDate()
        {
            return DateTime.UtcNow;
        }

        private int GetCurrentUserId()
        {
            var currentUserName = GetCurrentUserName();
            return _unitOfWork.UserRepository.GetUserByUserName(currentUserName).UserId;
        }

        private UserProfile GetCurrentUser()
        {
             var currentUserName = GetCurrentUserName();
            var user = _unitOfWork.UserRepository.GetUserByUserName(currentUserName);
            user.Password = "password";
            user.FullName = user.FirstName + " " + user.LastName;
            return user;
        }

        private Member AddAgentDetailsToMember(Member member)
        {
            UserProfile user;
            if (member.MemberId <= 0)
            {
                user = GetCurrentUser();
            }
            else
            {
                user = _unitOfWork.UserRepository.GetById(member.AgentId);
                user.Password = "password";
                user.FullName = user.FirstName + " " + user.LastName;
            }
            user.RelationshipWithApplicant = member.RelationshipToClient;
            member.AgentDetails = user;
            
            return member;
        }

        private Member AddMemberToProjectDetails(Member member)
        {
            var memberToProjects = _unitOfWork.MemberToProjectsRepository.Get(
                filter: x => x.MemberId == member.MemberId && x.IsAssigned && member.IsSubmit, includeProperties: "Project").ToList();

            if (memberToProjects.Count > 0)
            {
                member.ProjectMembership = new ProjectMembership {Projects = new List<Project>()};
                foreach (var item in memberToProjects)
                {
                    item.Project.AssignProjectToMemberNow = true;
                    item.Project.ProjectAlreadyAssigned = true;
                    member.ProjectMembership.Projects.Add(item.Project);
                    member.MemberRoleId = item.MemberRoleId;
                }
            }

            var allprojects = _unitOfWork.ProjectRepository.Get(); 
            foreach (var item in allprojects)
            {
                //check if current project already exists in project membership
                //if yes then do not add else add with default valeus
                if (member.ProjectMembership != null)
                {
                    var existingProjects =
                        member.ProjectMembership.Projects.Where(x => x.ProjectId == item.ProjectId).ToList();
                    if (existingProjects.Count == 0)
                    {
                        item.AssignProjectToMemberNow = false;
                        item.ProjectAlreadyAssigned = false;
                        member.ProjectMembership.Projects.Add(item);
                    }
                }
                else
                {
                    member.ProjectMembership = new ProjectMembership { Projects = new List<Project>() };
                    item.AssignProjectToMemberNow = false;
                    item.ProjectAlreadyAssigned = false;
                    member.ProjectMembership.Projects.Add(item);
                }

            }

            if (member.ProjectMembership == null)
            {
                member.ProjectMembership = new ProjectMembership {Projects = new List<Project>()};
            }
            return member;
        }

        private SelectList GetGenderSelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.Gender, "GenderId", "GenderName", selectedValue);
        }

        private SelectList GetUserSelectList(object selectedValue = null)
        {
            //Get the list of the following user :
            //1. User who is logged in
            //2. All agent type users

            var currentUserName = GetCurrentUserName();
            var currentLoggedInUser = _unitOfWork.UserRepository.GetUserByUserName(currentUserName);
            var users = new List<UserProfile>();
            users.Add(currentLoggedInUser);
            //return GenericSelectList.GetSelectList(SelectListEnums.User, "UserId", "UserName", selectedValue);
            var referees = _unitOfWork.UserRepository.Get().Where(x => Convert.ToInt32(x.UserTypeId) == (int)Role.Referrer);
            referees.ForEach(x => x.FullName = x.FirstName + " " + x.LastName);
            users.AddRange(referees.ToList());

            return new SelectList(users, "UserId", "FullName", selectedValue);
        }

        private SelectList GetLivingAreaSelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.LivingArea, "LivingAreaId", "LivingAreaName", selectedValue);
        }

        private SelectList GetEthnicitySelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.Ethnicity, "EthnicityId", "EthnicityName", selectedValue);
        }

        private SelectList GetReferralTypeSelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.ReferralType, "ReferralTypeId", "ReferralTypeName", selectedValue);
        }

        private SelectList GetNominationSelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.Nomination, "NominationId", "NominationName", selectedValue);
        }

        private SelectList GetFundingResponsibilitySelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.FundingResponsibility, "FundingResponsibilityId", "FundingResponsibilityName", selectedValue);
        }

        private SelectList GetMemberRoleSelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.MemberRole, "MemberRoleId", "MemberRoleName", selectedValue);
        }

        private SelectList GetSexualOrientationSelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.SexualOrientation, "SexualOrientationId", "SexualOrientationName", selectedValue);
        }

        private SelectList GetAllRefereeSelectList(object selectedValue = null)
        {
            var referees = _unitOfWork.UserRepository.Get().Where(x => Convert.ToInt32(x.UserTypeId) == (int) Role.Referrer);
            return new SelectList(referees, "UserId", "FirstName", selectedValue);
        }

        //private IEnumerable<SelectListItem> GetGroupedEthnicitySelectList(int ethnicityId)
        //{
        //    var ethnicity =  _unitOfWork.EthinicityRepository.Get().OrderBy(t => t.GroupId);
        //    return new SelectList(ethnicity, "EthnicityId", "EthnicityName", selectedValue);
        //    //.Select(t => new GroupedSelectListItem
        //    //{
        //    //    GroupKey = t.GroupId.ToString(CultureInfo.InvariantCulture),
        //    //    GroupName = t.GroupName,
        //    //    Text = t.EthnicityName,
        //    //    Value = t.EthnicityId.ToString(CultureInfo.InvariantCulture),
        //    //    Selected = (t.EthnicityId == ethnicityId)
        //    //});
        //}

        private void UpdateMemberAttendanceRatesToDefault(Member member)
        {
            foreach (var item in member.CurrentWeekAttendances)
            {
                item.MondayRate = (decimal)1.01;
                item.TuesdayRate = (decimal)1.01;
                item.WednesdayRate = (decimal)1.01;
                item.ThursdayRate = (decimal)1.01;
                item.FridayRate = (decimal)1.01;
                item.FridayRate = (decimal)1.01;
                item.SaturdayRate = (decimal)1.01;
                item.SundayRate = (decimal)1.01;
            }
        }

        private void AddViewDataSelectLists(Member member)
        {
            ViewBag.GenderId = GetGenderSelectList(member.GenderId);
            ViewBag.AgentId = GetUserSelectList(member.AgentId);
            ViewBag.LivingAreaId = GetLivingAreaSelectList(member.LivingAreaId);
            ViewBag.GroupedEthnicityId = member.EthnicityId;
            ViewBag.NominationId = GetNominationSelectList(member.NominationId);
            ViewBag.FundingResponsibilityId = GetFundingResponsibilitySelectList(member.FundingResponsibilityId);
            ViewBag.MemberRoleId = GetMemberRoleSelectList(member.MemberRoleId);
            ViewBag.SexualOrientationId = GetSexualOrientationSelectList(member.SexualOrientationId);
            ViewBag.RefereeId = GetAllRefereeSelectList(member.AgentId);
            ViewBag.EthnicityId = GetEthnicitySelectList(member.EthnicityId);
            ViewBag.ReferralTypeId = GetReferralTypeSelectList(member.ReferralTypeId);
            if (member.ProjectMembership == null)
            {
                member.ProjectMembership = new ProjectMembership { Projects = new List<Project>() };
            }
        }

        private Member AddNotesToMember(Member member)
        {
            IEnumerable<Notes> notes;
            if (member.MemberId > 0)
            {
                notes = _unitOfWork.NotesRepository.Get(filter: x => x.MemberId == member.MemberId,
                    orderBy: x => x.OrderByDescending(y => y.NotesCreateDate));
            }
            else
            {
                notes = new List<Notes>
                {
                    new Notes
                        {
                            AlertValidDate = DateTime.Today,
                            BroadCastAsAlert = false,
                            Member = member,
                            NotesCreateDate = DateTime.UtcNow,
                            NotesDesc = string.Empty
                        }
                };
            }

            if (member.Notes == null)
            {
                member.Notes = new Collection<Notes>();
                member.Notes.AddRange(notes);
            }

            return member;
        }

        private IEnumerable<Member> GetAllMembers()
        {
            if (User.IsInRole("admin") || User.IsInRole("staff"))
            {
                return _unitOfWork.MemberRepository.Get(
                includeProperties: "Gender,AgentDetails,LivingArea,EthnicOrigin,Nomination,FundingResponsibility,Notes",
                orderBy: x => x.OrderBy(k => k.MemberName));
            }

            return _unitOfWork.MemberRepository.Get(
                includeProperties: "Gender,AgentDetails,LivingArea,EthnicOrigin,Nomination,FundingResponsibility,Notes",
                filter: x => x.CreatedBy == User.Identity.Name, orderBy: x => x.OrderBy(k => k.MemberName));
        }

        /// <summary>
        /// Adds any new uploaded file to the file collection later saved to database
        /// </summary>
        /// <param name="member"></param>
        private void UploadFiles(Member member)
        {
            if (member.Files == null || !member.Files.Any())
            {
                return;
            }

            member.BinaryFilesList = new List<BinaryFile>();

            foreach (var item in member.Files)
            {
                if(item == null)
                    continue;   //that means there is no file uploaded
                var filename = item.FileName.Substring(item.FileName.LastIndexOf("\\", StringComparison.InvariantCulture) + 1);

                var extension = filename.Substring(filename.LastIndexOf(".", System.StringComparison.Ordinal) + 1);

                var uploadedFile = new byte[item.InputStream.Length];
                item.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

                var binaryFile = new BinaryFile
                {
                    FileContent = uploadedFile,
                    FileName = filename,
                    FileExtension = extension,
                    MemberId = member.MemberId
                };

                _unitOfWork.BinaryFileRepository.Insert(binaryFile);
                
                //member.BinaryFilesList.Add(binaryFile);
            }

            _unitOfWork.Save();
        }

        private void InsertNotes(Member member)
        {
            if (member.Note != null)
            {
                var notes = new Notes
                {
                    NotesDesc = member.Note,
                    MemberId = member.MemberId,
                    BroadCastAsAlert = member.BroadCastAsAlert,
                    AlertValidDate = member.AlertValidDate,
                    NotesCreateDate = DateTime.UtcNow,
                    NotesCreatedBy = User.Identity.Name
                };

                _unitOfWork.NotesRepository.Insert(notes);
                _unitOfWork.Save();
            }
        }

        private void InsertAdminAlert(Member member)
        {
            if (_isSubmit == false) return;
            
            var currDate = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString());
            AdminAlert alert = null;

            //1. Find if alert for this member has already been generated
            var resultSet = _unitOfWork.AdminAlertRepository.Get(x => x.MemberId == member.MemberId);
            var adminAlerts = resultSet as IList<AdminAlert> ?? resultSet.ToList();
            if (adminAlerts.Any())
            {
                // If yes then update AlertCreateDate & AlertValid Date
                alert = adminAlerts.ToList()[0];

                //Before doing that check if Member Induction date is provided then set alert dates to two days back else do nothing
                //TODO : This is just a heck to the system as per requirement in doc version 4.1. Need more eleborate solution like intruducing bool var in db indicate to show alert or not or something like that
                if (member.InductionDate.HasValue && member.InductionDate.Value != null)
                {
                    alert.AlertCreatedOn = currDate.AddDays(-2);
                    alert.AlertValidDate = currDate.AddDays(-2);
                }
                else
                {
                    alert.AlertCreatedOn = currDate;
                    alert.AlertValidDate = currDate;
                }
                
                alert.AlertDesc = member.MemberName + " has been submitted " + " on " + currDate.ToShortDateString() + " for processing";
                _unitOfWork.AdminAlertRepository.Update(alert);
            }
            else
            {
                //If No, then insert alert
                alert = new AdminAlert
                {
                    AlertDesc = member.MemberName + " has been submitted " + " on " + currDate.ToShortDateString() + " for processing",
                    MemberId = member.MemberId,
                    AlertValidDate = currDate,
                    AlertCreatedOn = currDate,
                    AlertCreatedBy = User.Identity.Name,
                    //Notes: this will be an enum in future for storing other types of alerts
                    AlertType = 1
                };
                _unitOfWork.AdminAlertRepository.Insert(alert);
            }
            
            _unitOfWork.Save();
        }

        private void CreateFundingResponsibilityOnTheFly(Member member)
        {
            if (member.FundingResponsibilityId == 3 &&
                member.OtherFundingResponsibility != null && member.OtherFundingResponsibility.Trim().Length > 0)
            {
                var newBorough = new FundingResponsibility
                {
                    FundingResponsibilityName = member.OtherFundingResponsibility.Trim()
                };
                _unitOfWork.FundingResponsibilityRepository.Insert(newBorough);

                member.FundingResponsibilityId = newBorough.FundingResponsibilityId;
            }
        }
    }
}