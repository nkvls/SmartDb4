using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartDb4.Attributes;
using SmartDb4.DAL;
using SmartDb4.Enums;
using SmartDb4.Helpers;
using SmartDb4.Models;

namespace SmartDb4.Controllers
{
    [AuthorizeEnum(Role.Admin, Role.Staff)]
    public class SmartDbHomeController : Controller
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public ActionResult Index()
        {
            try
            {
                var members = _unitOfWork.MemberRepository.Get().ToList();

                foreach (var member in members)
                {
                    var createdByRecord = _unitOfWork.UserRepository.GetUserByUserName(member.CreatedBy);
                    member.CreatedByName = createdByRecord.FirstName + " " + createdByRecord.LastName;

                    var member1 = member;
                   
                    var notes = _unitOfWork.NotesRepository.Get(includeProperties: "Member",
                                                               filter:
                                                                   x =>
                                                                   x.Member.MemberId == member1.MemberId &&
                                                                   x.BroadCastAsAlert &&
                                                                   (x.AlertValidDate != null &&
                                                                    DateTime.Compare(x.AlertValidDate.Value, DateTime.Now) >= 0),
                                                               orderBy: x => x.OrderByDescending(k => k.NotesCreateDate)).ToList();
                    if (notes.Count > 0)
                    {
                        member.Note = notes[0].NotesDesc;
                        //member.NotesCreatedBy = notes[0].NotesCreatedBy;
                        var notesCreatedByRecord = _unitOfWork.UserRepository.GetUserByUserName(notes[0].NotesCreatedBy);
                        member.NotesCreatedBy = notesCreatedByRecord.FirstName + " " + notesCreatedByRecord.LastName;

                        var notesCreateDate = notes[0].NotesCreateDate;
                        if (notesCreateDate != null) member.NotesCreateDate = notesCreateDate.Value;
                    }

                    var memberToProject =
                        _unitOfWork.MemberToProjectsRepository.Get(includeProperties: "Project",
                            filter: x => x.MemberId == member1.MemberId && x.IsAssigned);

                    foreach (var item in memberToProject)
                    {
                        member.Projects.Add(item.Project);
                    }
                }

                var adminAlerts =
                    _unitOfWork.AdminAlertRepository.Get(includeProperties: "Member",
                        orderBy: x => x.OrderByDescending(k => k.AlertCreatedOn)).Take(10);

                return
                    View(Tuple.Create(members.ToList(), adminAlerts.ToList()));
            
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("SmartDbHomeController.Index() : " + ex, "Error");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchMember(string searchText)
        {
            try
            {
                //find member here and if not found redirect to member page
                if (searchText.Length > 0)
                {
                    var members = from mem in _unitOfWork.MemberRepository.Get()
                                  where mem.MemberName.ToLower().Contains(searchText.ToLower())
                                  select mem;
                    var enumerable = members as Member[] ?? members.ToArray();

                    if (enumerable.Any())
                    {
                        var mem = enumerable.ToList()[0];
                        return RedirectToAction("Details", "MembershipData", new { id = mem.MemberId });
                    }
                }

                return RedirectToAction("Index", "MembershipData");
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("SmartDbHomeController.SearchMember() : " + ex, "Error");
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}