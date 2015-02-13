using System;
using System.Collections.Generic;
using SmartDb4.DAL.Interfaces;
using SmartDb4.Models;

namespace SmartDb4.DAL.Repositories
{
    public class MemberAttendanceRepository<T> : GenericRepository<MemberAttendance>, IMemberAttendanceRepository
    {
        public MemberAttendanceRepository(SmartDbContext context) : base(context) { }

        public IEnumerable<MemberAttendance> GetEmptyObjectCollection()
        {
            var memberAttendance = new MemberAttendance
            {
                Attendance = new Attendance(),
                CreatedOn = DateTime.UtcNow,
                Monday = false,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = false,
                Saturday = false,
                Sunday = false,
                MondayRate = (decimal)0.00,
                TuesdayRate = (decimal)0.00,
                WednesdayRate = (decimal)0.00,
                ThursdayRate = (decimal)0.00,
                FridayRate = (decimal)0.00,
                SaturdayRate = (decimal)0.00,
                SundayRate = (decimal) 0.00
            };

            return new List<MemberAttendance> {memberAttendance};
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}