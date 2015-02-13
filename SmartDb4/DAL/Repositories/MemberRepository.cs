using System;
using System.Collections.Generic;
using System.Data.Entity;
using SmartDb4.DAL.Interfaces;
using SmartDb4.Models;

namespace SmartDb4.DAL.Repositories
{
    public class MemberRepository : IMemberRepository, IDisposable
    {
        private readonly SmartDbContext _context;

        public MemberRepository(SmartDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Member> GetMembers()
        {
            return _context.Members.Include(m => m.Gender).Include(m => m.AgentDetails).Include(m => m.LivingArea).Include(m => m.EthnicOrigin).Include(m => m.Nomination).Include(m => m.FundingResponsibility);
        }

        public Member GetMemberById(int memberId)
        {
            return _context.Members.Find(memberId);
        }

        public void Insert(Member member)
        {
            throw new NotImplementedException();
        }

        public void Delete(int memberId)
        {
            throw new NotImplementedException();
        }

        public void Update(Member member)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}