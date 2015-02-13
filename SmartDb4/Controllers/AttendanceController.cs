using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using SmartDb4.Attributes;
using SmartDb4.DAL;
using SmartDb4.Enums;
using SmartDb4.Extensions;
using SmartDb4.Helpers;
using SmartDb4.Models;

namespace SmartDb4.Controllers
{
    [AuthorizeEnum(Role.Admin, Role.Staff)]
    public class AttendanceController : Controller
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        public ActionResult Index(string sortOrder, string searchString)
        {
            try
            {
                var currWeek = DateTime.Now.GetWeek();
                var isCurrWeekDataExists = false;

                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? SortConstants.NameDesc : "";
                ViewBag.YearSortParm = sortOrder == SortConstants.YearAsc ? SortConstants.Year : SortConstants.YearAsc;

                ViewBag.CurrentFilter = searchString;

                var attendance =
                    _unitOfWork.AttendanceRepository.Get(includeProperties: "Project",
                                                         orderBy: x => x.OrderBy(k => k.Year).ThenBy(k=> k.Week));

                //serching
                if (!String.IsNullOrEmpty(searchString))
                {
                    attendance = attendance.Where(s => s.Project.ProjectName.ToUpper().Contains(searchString.ToUpper()));
                }

                //sorting
                switch (sortOrder)
                {
                    case SortConstants.NameDesc:
                        attendance = attendance.OrderByDescending(s => s.Project.ProjectName);
                        break;
                    case SortConstants.YearAsc:
                        attendance = attendance.OrderBy(s => s.Year);
                        break;
                    case SortConstants.Year:
                        attendance = attendance.OrderByDescending(s => s.Year);
                        break;
                    default:
                        attendance = attendance.OrderBy(s => s.Year).ThenBy(k => k.Week); //attendance.OrderBy(s => s.Project.ProjectName);
                        break;
                }

                //check if currentweek data exists
                var attendances = attendance as IList<Attendance> ?? attendance.ToList();
                if (attendances.Any(x => x.Week == currWeek))
                {
                    isCurrWeekDataExists = true;
                }

                ViewBag.IsCurrentWeekDataExist = isCurrWeekDataExists;

                attendances.ToList().ForEach(x => x.FirstDateOfWeek = GetFirstDateOfWeek(x.Year, x.Week, CultureInfo.InvariantCulture));

                return View(attendances.ToList());
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("AttendanceController.Index() : " + ex, "Error");
                throw;
            }
        }


        //
        // GET: /Attendance/Details/5
        //public ActionResult Details(int id = 0)
        //{
        //    //Attendance attendance = db.Attendances.Find(id);
        //    //if (attendance == null)
        //    //{
        //    //    return HttpNotFound();
        //    //}
        //    //return View(attendance);
        //}

        public ActionResult Create(string pid, string dt)
        {
            try
            {
                MemberAttendanceForView dataToView;

                if (!string.IsNullOrEmpty(pid))
                {
                    dataToView = LoadAttendanceData(pid, dt);
                }
                else
                {
                    ViewBag.ProjectId = GetProjectSelectList();
                    dataToView = new MemberAttendanceForView
                    {
                        Attendance = new Attendance(),
                        MemberAttendances = new List<MemberAttendance>(),
                        Projects = _unitOfWork.ProjectRepository.Get().ToList(),
                        DateForCurrentWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday)
                    };
                }

                return View(dataToView);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("AttendanceController.Create() : " + ex, "Error");
                throw;
            }
        }


        //
        // POST: /Attendance/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MemberAttendanceForView memberAttendanceData)
        {
            if (ModelState.IsValid)
            {
                if (memberAttendanceData != null)
                {
                    try
                    {
                        var status = SaveAttendanceData(memberAttendanceData);
                        if (!status)
                        {
                            return View(memberAttendanceData);
                        }

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteToLog("AttendanceController.Create() : " + ex, "Error");
                        throw;
                    }
                }
            }

            ViewBag.DisplayWeek = GetDisplayWeek(DateTime.Now.StartOfWeek(DayOfWeek.Monday));
            if (memberAttendanceData != null)
            {
                ViewBag.ProjectId = GetProjectSelectList(memberAttendanceData.ProjectId);

                return View(memberAttendanceData);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(string pid, string dt)
        {
            try
            {
                DateTime weekRequested;

                var splitString = dt.Split('-');
                if (splitString.Length < 2)
                {
                    //Utility.WriteToLog("AttendanceController.LoadAttendanceData() : Invalid dt parameter " + dt, "Error");
                    //throw new Exception("Invalid dt parameter " + dt);
                    //the data in date format
                    DateTime inputDate;
                    DateTime.TryParse(dt, out inputDate);
                    weekRequested = inputDate.FirstDateOfWeek(inputDate.Year, inputDate.GetWeek());
                }
                else
                {
                    int year;
                    int.TryParse(splitString[1], out year);
                    int week;
                    int.TryParse(splitString[0], out week);
                    weekRequested = FirstDateOfWeekISO8601(year, week);
                }
                

                var dataToView = LoadAttendanceData(pid, weekRequested.ToString());

                return View(dataToView);
            }
            catch (Exception ex)
            {
                Utility.WriteToLog("AttendanceController.Edit() : " + ex, "Error");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MemberAttendanceForView memberAttendanceData)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var status = SaveAttendanceData(memberAttendanceData);
                    if (!status)
                    {
                        return View(memberAttendanceData);
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Utility.WriteToLog("AttendanceController.Edit() : " + ex, "Error");
                    throw;
                }
            }

            ViewBag.ProjectId = GetProjectSelectList(memberAttendanceData.ProjectId);
            ViewBag.DisplayWeek = GetDisplayWeek(DateTime.Now.StartOfWeek(DayOfWeek.Monday));

            return View(memberAttendanceData);
        }


        //private Attendance GetNewAttendance(int projectId)
        //{
        //    return new Attendance
        //    {
        //        ProjectId = projectId,
        //        CreatedBy = User.Identity.Name,
        //        CreatedOn = DateTime.UtcNow,
        //        Year = DateTime.UtcNow.Year,
        //        Week = DateTime.UtcNow.GetWeek(),
        //        WeekString = GetDisplayWeek(DateTime.Now.StartOfWeek(DayOfWeek.Monday))
        //    };
        //}
        ////
        //// GET: /Attendance/Delete/5

        //public ActionResult Delete(int id = 0)
        //{
        //    //Attendance attendance = db.Attendances.Find(id);
        //    //if (attendance == null)
        //    //{
        //    //    return HttpNotFound();
        //    //}
        //    //return View(attendance);
        //}

        ////
        //// POST: /Attendance/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    //Attendance attendance = db.Attendances.Find(id);
        //    //db.Attendances.Remove(attendance);
        //    //db.SaveChanges();
        //    //return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }

        private string GetDisplayWeek(DateTime currentWeekStartdate)
        {
            var currentWeekEndDate = currentWeekStartdate.AddDays(6);

            return currentWeekStartdate.Day + " " + currentWeekStartdate.ToMonthName() + " - " + currentWeekEndDate.Day +
                   " " +
                   currentWeekEndDate.ToMonthName() + " " + currentWeekEndDate.Year;
        }

        private SelectList GetProjectSelectList(object selectValue = null)
        {
            return GenericSelectList.GetSelectList(SelectListEnums.Project, "ProjectId", "ProjectName", selectValue);
        }

        private MemberAttendanceForView GetMemberAttendanceForViewByProjectId(int projectId, DateTime weekRequested)
        {
            MemberAttendanceForView dataToView;

            var weekSought = weekRequested.GetWeek();
            var yearSought = weekRequested.Year;

            var attendances = _unitOfWork.AttendanceRepository.Get(filter: x => x.ProjectId == projectId && x.Week == weekSought && x.Year == yearSought).ToList();

            //if there is no data then pick the data from memberToProject repository for first time use
            if (!attendances.Any())
            {
                dataToView = CreateAttendanceDataForWeekAndProject(projectId, weekRequested); 
            }
            else
            {
                var attendance = attendances[0];
                //attendaceId = attendance.AttendanceId;
                dataToView = GetMemberAttendanceForViewById(attendance, weekRequested);

                //plus add any newly added member this week from memberToProject table
                var newMemberAttendanceNotYetSavedForWeekSought = CreateAttendanceDataForWeekAndProject(projectId, weekRequested);
                
                //filter down all member who have already registered for the week sought
                foreach (var item in newMemberAttendanceNotYetSavedForWeekSought.MemberAttendances)
                {
                    if (!dataToView.MemberAttendances.Exists(x => x.MemberId == item.MemberId))
                    {
                        dataToView.MemberAttendances.Add(item);  
                    }
                }
            }

            return dataToView;
        }

        private MemberAttendanceForView GetMemberAttendanceForViewById(Attendance attendance, DateTime weekRequested)
        {
            var memberAttendances = _unitOfWork.MemberAttendanceRepository.Get(filter: x => x.AttendanceId == attendance.AttendanceId).ToList();

            var dataToView = GetMemberAttendanceObject(attendance, memberAttendances, weekRequested);

            return dataToView;
        }

        private MemberAttendanceForView CreateAttendanceDataForWeekAndProject(int projectId, DateTime weekRequested)
        {
            var listOfMemberAttendance = new List<MemberAttendance>();

            foreach (var item in _unitOfWork.MemberToProjectsRepository.Get(filter: x => x.ProjectId == projectId && x.IsAssigned))
            {
                var member = _unitOfWork.MemberRepository.GetById(item.MemberId);

                //check if member start date is before the attendance week date
                //IF yes, member is included for the attendance ELSE not
                if (!member.StartDate.HasValue)
                {
                    Utility.LogWarn(
                        string.Format("CreateAttendanceDataForWeekAndProject: Start date not set for member {0} {1}",
                            member.MemberId, member.MemberName));
                }
                else
                {
                    if (DateTime.Compare(member.StartDate.Value, weekRequested) > 0 && DateTime.Compare(member.StartDate.Value, weekRequested.AddDays(7)) > 0)
                    //if(member.StartDate.Value <= weekRequested && member.StartDate.Value >=  weekRequested.AddDays(7))
                    {
                        //skip this member as member has not started for attendance in question
                        continue;
                    }
                }

                if (member.ExitDate.HasValue)
                {
                    if (DateTime.Compare(member.ExitDate.Value, weekRequested) < 0 && DateTime.Compare(member.ExitDate.Value, weekRequested.AddDays(7)) < 0)
                    {
                        //skip this member as member exit date is before th attendance week in question
                        continue;
                    }
                }

                var memberAttendances = _unitOfWork.MemberAttendanceRepository.GetEmptyObjectCollection();

                var memberAttendance = memberAttendances.FirstOrDefault();

                memberAttendance.MemberId = member.MemberId;
                memberAttendance.Member = member;

                memberAttendance.Monday = false;
                memberAttendance.Tuesday = false;
                memberAttendance.Wednesday = false;
                memberAttendance.Thursday = false;
                memberAttendance.Friday = false;
                memberAttendance.Saturday = false;
                memberAttendance.Sunday = false;

                memberAttendance.MondayRate =
                    memberAttendance.TuesdayRate =
                        memberAttendance.WednesdayRate =
                            memberAttendance.ThursdayRate =
                                memberAttendance.FridayRate =
                                    memberAttendance.SaturdayRate = memberAttendance.SundayRate = item.MemberRate;

                listOfMemberAttendance.Add(memberAttendance);
            }
            var attendance = new Attendance {AttendanceId = -1};

            return GetMemberAttendanceObject(attendance, listOfMemberAttendance, weekRequested);
        }

        private MemberAttendanceForView GetMemberAttendanceObject(Attendance attendance, List<MemberAttendance> memberAttendances, DateTime weekRequested)
        {
            var mondayDate = weekRequested.StartOfWeek(DayOfWeek.Monday); // DateTime.Now.StartOfWeek(DayOfWeek.Monday);

            return new MemberAttendanceForView
            {
                Attendance = attendance,
                AttendanceId = attendance.AttendanceId,
                MemberAttendances = memberAttendances,
                Projects = _unitOfWork.ProjectRepository.Get().ToList(),
                MondayDate = mondayDate.ToShortDateString(),
                TuesdayDate = mondayDate.AddDays(1).ToShortDateString(),
                WednesdayDate = mondayDate.AddDays(2).ToShortDateString(),
                ThursdayDate = mondayDate.AddDays(3).ToShortDateString(),
                FridayDate = mondayDate.AddDays(4).ToShortDateString(),
                SaturdayDate = mondayDate.AddDays(5).ToShortDateString(),
                SundayDate = mondayDate.AddDays(6).ToShortDateString()
            };
        }

        private void UpdateAttendance(MemberAttendanceForView memberAttendanceData, int projectId, DateTime weekRequested)
        {
            var weekSought = weekRequested.GetWeek();
            var yearSought = weekRequested.Year;

            bool isInsert = memberAttendanceData.AttendanceId == -1;

            Attendance attendance;

            if (isInsert) //that means its an INSERT
            {
                attendance = new Attendance
                    {
                        ProjectId = projectId,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.UtcNow,
                        Year = yearSought,
                        Week = weekSought,
                        WeekString = GetDisplayWeek(weekRequested.StartOfWeek(DayOfWeek.Monday))
                    };

                using (var scope = new TransactionScope())
                {
                    try
                    {
                        _unitOfWork.AttendanceRepository.Insert(attendance);
                        _unitOfWork.Save();

                        foreach (var item in memberAttendanceData.MemberAttendances)
                        {
                            var newMembAtt = GetNewMemberAttendance(attendance.AttendanceId, item);

                            _unitOfWork.MemberAttendanceRepository.Insert(newMembAtt);
                        }

                        _unitOfWork.Save();
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                        Utility.WriteToLog("AttendanceController.UpdateAttendance() : " + ex, "Error");
                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                attendance = _unitOfWork.AttendanceRepository.GetById(memberAttendanceData.AttendanceId);

                attendance.ModifiedBy = User.Identity.Name;
                attendance.ModifiedOn = DateTime.UtcNow;

                try
                {
                    _unitOfWork.AttendanceRepository.Update(attendance);
                    _unitOfWork.Save();

                    foreach (var item in memberAttendanceData.MemberAttendances)
                    {
                        var item1 = item;
                        var memberAttendance = _unitOfWork.MemberAttendanceRepository.Get(filter: x => x.MemberId == item1.Member.MemberId && x.AttendanceId == attendance.AttendanceId).ToList();
                        if (memberAttendance.Any())
                        {
                            var dataToUpdate = memberAttendance[0];
                            UpdateMemberAttendanceData(item1, dataToUpdate);
                            _unitOfWork.MemberAttendanceRepository.Update(dataToUpdate);
                        }
                    }
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    //scope.Dispose();
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    Utility.WriteToLog("AttendanceController.UpdateAttendance() : " + ex, "Error");
                    throw new Exception(ex.Message);
                }
            }
        }

        private void UpdateMemberAttendanceData(MemberAttendance source, MemberAttendance destination)
        {
            destination.ModifiedBy = User.Identity.Name;
            destination.ModifiedOn = DateTime.UtcNow;
            destination.Monday = source.Monday;
            destination.MondayRate = source.MondayRate;
            destination.Tuesday = source.Tuesday;
            destination.TuesdayRate = source.TuesdayRate;
            destination.Wednesday = source.Wednesday;
            destination.WednesdayRate = source.WednesdayRate;
            destination.Thursday = source.Thursday;
            destination.ThursdayRate = source.ThursdayRate;
            destination.Friday = source.Friday;
            destination.FridayRate = source.FridayRate;
            destination.Saturday = source.Saturday;
            destination.SaturdayRate = source.SaturdayRate;
            destination.Sunday = source.Sunday;
            destination.SundayRate = source.SundayRate;
        }

        private MemberAttendance GetNewMemberAttendance(int attendanceId, MemberAttendance item)
        {
            return new MemberAttendance
            {
                AttendanceId = attendanceId,
                MondayRate = item.MondayRate,
                TuesdayRate = item.TuesdayRate,
                WednesdayRate = item.WednesdayRate,
                ThursdayRate = item.ThursdayRate,
                FridayRate = item.FridayRate,
                SaturdayRate = item.SaturdayRate,
                SundayRate = item.SundayRate,
                Monday = item.Monday,
                Tuesday = item.Tuesday,
                Wednesday = item.Wednesday,
                Thursday = item.Thursday,
                Friday = item.Friday,
                Saturday = item.Saturday,
                Sunday = item.Sunday,
                CreatedBy = User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = User.Identity.Name,
                ModifiedOn = DateTime.UtcNow,
                MemberId = item.Member.MemberId
            };
        }

        private static DateTime GetFirstDateOfWeek(int year, int weekOfYear, CultureInfo ci)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            var firstWeekDay = jan1.AddDays(daysOffset);
            var firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
            if (firstWeek <= 1 || firstWeek > 50)
            {
                weekOfYear -= 1;
            }
            return firstWeekDay.AddDays(weekOfYear * 7);
        }

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

        private MemberAttendanceForView LoadAttendanceData(string pid, string dt)
        {
            int projectId;
            int.TryParse(pid, out projectId);
            
            DateTime weekRequested;
            DateTime.TryParse(dt, out weekRequested);

            ViewBag.ProjectId = GetProjectSelectList(pid);

            ViewBag.DisplayWeek = GetDisplayWeek(weekRequested.StartOfWeek(DayOfWeek.Monday));

            var dataToView = GetMemberAttendanceForViewByProjectId(projectId, weekRequested);

            dataToView.DateForCurrentWeek = weekRequested.StartOfWeek(DayOfWeek.Monday);

            return dataToView;
        }

        private bool SaveAttendanceData(MemberAttendanceForView memberAttendanceData)
        {
            var projectId = 0;
            if (Request.QueryString != null)
            {
                int.TryParse(Request.QueryString["pid"], out projectId);
            }
            if (projectId == 0)
            {
                ModelState.AddModelError("", "Invalid project Id. Unable to save changes. Try again, and if the problem persists see your system administrator.");
                ViewBag.ProjectId = GetProjectSelectList();
                ViewBag.DisplayWeek = GetDisplayWeek(DateTime.Now.StartOfWeek(DayOfWeek.Monday));
                //return View(memberAttendanceData);
                return false;
            }

            var weekRequested = new DateTime();
            if (Request.QueryString != null)
            {
                DateTime.TryParse(Request.QueryString["dt"], out weekRequested);
            }

            UpdateAttendance(memberAttendanceData, projectId, weekRequested);
            return true;
        }

    }
}