using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Web.WebPages.OAuth;
using SmartDb4.Attributes;
using SmartDb4.DAL;
using SmartDb4.Enums;
using SmartDb4.Helpers;
using SmartDb4.Models;
using WebMatrix.WebData;

namespace SmartDb4.Controllers
{
    [AuthorizeEnum(Role.Admin, Role.Staff)]
    public class UserController : Controller
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        //TODO: Mef IMPLEMENTATION
        //public UserController(IUserRepository userRepository)
        //{
        //    _userRepository = userRepository;
        //}

        //
        // GET: /User/

        public ActionResult Index(string sortOrder, string searchString)
        {
            try
            {
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? SortConstants.NameDesc : "";
                ViewBag.DateSortParm = sortOrder == SortConstants.Date ? SortConstants.DateDesc : SortConstants.Date;
                ViewBag.UserTypeSortParm = sortOrder == SortConstants.UserType ? SortConstants.UserTypeDesc : SortConstants.UserType;

                var userProfile = GetAllUsers();

                if (!String.IsNullOrEmpty(searchString))
                {
                    userProfile = userProfile.Where(s => s.UserName.ToUpper().Contains(searchString.ToUpper()) || s.FirstName.ToUpper().Contains(searchString.ToUpper()));
                }

                switch (sortOrder)
                {
                    case SortConstants.NameDesc:
                        userProfile = userProfile.OrderByDescending(s => s.UserName);
                        break;
                    case SortConstants.UserTypeDesc:
                        userProfile = userProfile.OrderByDescending(s => s.UserType.UserTypeName);
                        break;
                    default:
                        userProfile = userProfile.OrderBy(s => s.FirstName);
                        break;
                }

                return View(userProfile.ToList());
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("UserController.Index() : " + ex, "Error");
                throw;
            }
        }        

        public ActionResult Details(int id = 0)
        {
            try
            {
                var userprofile = _unitOfWork.UserRepository.GetById(id);

                if (userprofile == null)
                {
                    return HttpNotFound();
                }

                return PartialView("Details", userprofile);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("UserController.Details() : " + ex, "Error");
                throw;
            }
            
        }

        public ActionResult Create()
        {
            try
            {
                ViewBag.UserTypeId = GetUserTypeSelectList();

                return View();
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("UserController.Create() : " + ex, "Error");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var roles = _unitOfWork.UserRepository.GetAllRoles();

                    //user name is same as email
                    userProfile.UserName = userProfile.Email;

                    WebSecurity.CreateUserAndAccount(
                        userProfile.UserName,
                        userProfile.Password,
                        propertyValues:
                            new
                                {
                                    Agency = userProfile.Agency,
                                    Address1 = userProfile.Address1,
                                    Address2 = userProfile.Address2,
                                    City = userProfile.City,
                                    PostCode = userProfile.PostCode,
                                    WorkTelephone = userProfile.WorkTelephone,
                                    MobileTelephone = userProfile.MobileTelephone,
                                    Email = userProfile.Email,
                                    RelationshipWithApplicant = userProfile.RelationshipWithApplicant,
                                    UserTypeId = userProfile.UserTypeId,
                                    FirstName = userProfile.FirstName,
                                    LastName = userProfile.LastName,
                                    IsHistoric = userProfile.IsHistoric,
                                    CreatedBy = User.Identity.Name,
                                    CreatedOn = DateTime.UtcNow
                                });


                    var userType = GetUserType(userProfile.UserTypeId);
                    roles.AddUsersToRoles(new[] {userProfile.UserName}, new[] {userType});

                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }

                return RedirectToAction("Index");
            }

            ViewBag.UserTypeId = GetUserTypeSelectList(userProfile.UserTypeId);
            return View(userProfile);
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                var userProfile = _unitOfWork.UserRepository.GetById(id);

                if (userProfile == null)
                {
                    return HttpNotFound();
                }

                //if the admin is being edited and the current logged in user is not admin then do not allow to edit
                if (userProfile.UserType.UserTypeId == (int)Role.Admin && !User.IsInRole("admin"))
                {
                    ViewBag.Error = "You do not have sufficient persmission to execute this operation. Contact system administrator for further details.";
                    
                    var userprofiles = GetAllUsers();

                    return View("Index", userprofiles.ToList());
                }

                userProfile.Password = "hi";
                userProfile.OldUserNameWhenChangedByAdmin = userProfile.Email;
                ViewBag.UserTypeId = GetUserTypeSelectList(userProfile.UserTypeId);

                return View(userProfile);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("UserController.Edit() : " + ex, "Error");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    userProfile.UserName = userProfile.Email;   //user name is same as email

                    //Check for unique username/email
                    if (!String.Equals(userProfile.OldUserNameWhenChangedByAdmin, userProfile.UserName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var existingUser = _unitOfWork.UserRepository.GetUserByUserName(userProfile.UserName);
                        if (existingUser != null && existingUser.UserId > 0)
                        {
                            var message = "Unable to save changes. This user name " + userProfile.UserName + " already existed.";
                            //ModelState.AddModelError("", message);
                            ViewBag.Error = message;
                            ViewBag.UserTypeId = GetUserTypeSelectList(userProfile.UserTypeId);
                            return View(userProfile);
                        }
                    }

                    userProfile.ModifiedBy = User.Identity.Name;
                    userProfile.ModifiedOn = DateTime.UtcNow;

                    _unitOfWork.UserRepository.Update(userProfile);
                    _unitOfWork.Save();

                    var userType = GetUserType(userProfile.UserTypeId);
                    var roles = (SimpleRoleProvider)Roles.Provider;

                    //if the current value is changed from what we have in database then need to drop the existing role 
                    //and add current role as new role
                    if (!roles.IsUserInRole(userProfile.UserName, userType))
                    {
                        var rolesUserCurrentlyIn = Roles.GetRolesForUser(userProfile.UserName);
                        roles.RemoveUsersFromRoles(new[] { userProfile.UserName }, rolesUserCurrentlyIn);
                        roles.AddUsersToRoles(new[] { userProfile.UserName }, new[] { userType });

                        //IF Role is changing AND I am editing my self THEN force logout
                        if (String.Equals(userProfile.OldUserNameWhenChangedByAdmin, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            WebSecurity.Logout();
                            return RedirectToAction("Index", "SmartDbHome");
                        }
                    }

                    //IF UserName is changing AND I am editing my self THEN force logout
                    if (String.Equals(userProfile.OldUserNameWhenChangedByAdmin, User.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!String.Equals(userProfile.OldUserNameWhenChangedByAdmin, userProfile.UserName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            WebSecurity.Logout();
                            return RedirectToAction("Index", "SmartDbHome");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    Utility.WriteToLog("UserController.Edit() : " + ex, "Error");
                    throw;
                }
                

                return RedirectToAction("Index");
            }

            ViewBag.UserTypeId = GetUserTypeSelectList(userProfile.UserTypeId);
            return View(userProfile);
        }

        //public ActionResult Delete(int id = 0)
        //{
        //    UserProfile userprofile = _unitOfWork.UserRepository.GetById(id);

        //    if (userprofile == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(userprofile);
        //}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        public JsonResult DeleteConfirmed(int id)
        {
            try
            {
                UserProfile userprofile = _unitOfWork.UserRepository.GetById(id);

                //BUSINESS RULE : User can only be deleted if there is no project assigned in their name
                var projects = _unitOfWork.ProjectRepository.Get(filter: x => x.SupervisorId == userprofile.UserId);
                if (projects.Any())
                {
                    return Json(new { Ok = false, ErrorMessage = "Cannot delete user. The user is assigned to the project." });
                }

                Roles.RemoveUserFromRole(userprofile.UserName, userprofile.UserType.UserTypeName);
                Membership.DeleteUser(userprofile.UserName);

                return Json(new { Ok = true });
                //return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("UserController.Delete() : " + ex, "Error");
                throw;
            }
        }

        public ActionResult ChangePassword(string userName)
        {
            TempData["UserName"] = userName;
            return PartialView("ChangePassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePassword(LocalPasswordModel model)
        {
            try
            {
                //model.UserName = TempData["UserName"].ToString();
                bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(model.UserName));
                ViewBag.HasLocalPassword = hasLocalAccount;
                //ViewBag.ReturnUrl = Url.Action("Manage");
                if (hasLocalAccount)
                {
                    if (ModelState.IsValid)
                    {
                        // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                        bool changePasswordSucceeded;
                        try
                        {
                            //changePasswordSucceeded = WebSecurity.ChangePassword(model.UserName, model.OldPassword, model.NewPassword);
                            var token = WebSecurity.GeneratePasswordResetToken(model.UserName);
                            changePasswordSucceeded = WebSecurity.ResetPassword(token, model.NewPassword);
                        }
                        catch (Exception)
                        {
                            changePasswordSucceeded = false;
                        }

                        if (changePasswordSucceeded)
                        {
                            return Json(new { Ok = true });
                            //return RedirectToAction("Edit", new { Message = "Password changed successfully" });
                        }
                    }
                }

                ViewBag.Error = "The current password is incorrect or the new password is invalid.";
                return Json(new { Ok = false, ErrorMessage = ViewBag.Error });
                
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("UserController.ChangePassword() : " + ex, "Error");
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }

        private IEnumerable<UserProfile> GetAllUsers()
        {
            var userprofiles = _unitOfWork.UserRepository.Get(includeProperties: "UserType",
                           filter: x => x.UserName.ToLower() != Constants.Superadmin, orderBy: x => x.OrderBy(k => k.UserName));

            var userProfiles = userprofiles as IList<UserProfile> ?? userprofiles.ToList();

            //If the logged in user is admin then he has the permission to edit each user
            if (User.IsInRole("admin"))
            {
                foreach (var item in userProfiles)
                {
                    item.CanEdit = true;
                }
            }
            else
            {
                foreach (var item in userProfiles)
                {
                    item.CanEdit = item.UserType.UserTypeId != (int)Role.Admin;
                }
            }

            return userProfiles;
        }

        private SelectList GetUserTypeSelectList(object selectedValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.UserType, "UserTypeId", "UserTypeName", selectedValue);
        }


        private string GetUserType(int userTypeId)
        {
            switch (userTypeId)
            {
                case 1:
                    return Role.Admin.ToString();
                case 2:
                    return Role.Staff.ToString();
                case 3:
                    return Role.Referrer.ToString();
                default:
                    throw new Exception("InvaldArgumentException UserTypeId");
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}