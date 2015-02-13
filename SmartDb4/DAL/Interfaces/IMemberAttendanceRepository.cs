using System;
using System.Collections;
using System.Collections.Generic;
using SmartDb4.Models;

namespace SmartDb4.DAL.Interfaces
{
    public interface IMemberAttendanceRepository : IDisposable
    {
        IEnumerable<MemberAttendance> GetEmptyObjectCollection();
    }
}
