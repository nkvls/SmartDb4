using System;
using System.Collections.Generic;
using SmartDb4.Models;

namespace SmartDb4.DAL.Interfaces
{
    public interface IMemberRepository : IDisposable
    {
        IEnumerable<Member> GetMembers();
        Member GetMemberById(int memberId);
        void Insert(Member member);
        void Delete(int memberId);
        void Update(Member member);
        void Save();
    }
}
