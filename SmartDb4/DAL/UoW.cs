using System;
using SmartDb4.DAL.Repositories;
using SmartDb4.Models;

namespace SmartDb4.DAL
{
    public class UnitOfWork : IDisposable
    {
        //TODO : Mef IMPLEMENTATION HERE
        private readonly SmartDbContext _context = new SmartDbContext();

        private GenericRepository<Project> _projectRepository;
        private UserRepository<UserProfile> _userRepository;
        private GenericRepository<Member> _memberRepository;
        private GenericRepository<Classification> _classificationRepository;
        private GenericRepository<MemberRole> _roleRepository;
        private GenericRepository<MemberToProject> _memberToProjectRepository;
        private GenericRepository<Attendance> _attendanceRepository;
        private MemberAttendanceRepository<MemberAttendance> _memberAttendanceRepository;
        private GenericRepository<Gender> _genderRepository;
        private GenericRepository<ReferralType> _referralTypeRepository;
        private GenericRepository<SexualOrientation> _sexualOrientationRepository;
        private GenericRepository<LivingArea> _livingAreaRepository;
        private GenericRepository<Ethnicity> _ethinicityRepository;
        private GenericRepository<Nomination> _nominationRepository;
        private GenericRepository<MemberRole> _memberRoleRepository;
        private GenericRepository<FundingResponsibility> _fundingResponsibilityRepository;
        private GenericRepository<UserType> _userTypeRepository;
        private GenericRepository<Notes> _notesRepository;
        private GenericRepository<AdminAlert> _adminAlertRepository;
        private GenericRepository<ReportDataModel> _reportRepository;
        private GenericRepository<AgeBracket> _ageBracketRepository;
        private GenericRepository<GroupByClause> _groupByRepository;
        private GenericRepository<BinaryFile> _binaryFileRepository;

        public GenericRepository<Project> ProjectRepository
        {
            get { return _projectRepository ?? (_projectRepository = new GenericRepository<Project>(_context)); }
        }

        public UserRepository<UserProfile> UserRepository
        {
            //get { return _userRepository ?? (_userRepository = new GenericRepository<UserProfile>(_context)); }
            get { return _userRepository ?? (_userRepository = new UserRepository<UserProfile>(_context)); }
        }

        public GenericRepository<Classification> ClassificationRepository
        {
            get { return _classificationRepository ?? (_classificationRepository = new GenericRepository<Classification>(_context)); }
        }

        public GenericRepository<MemberRole> RoleRepository
        {
            get { return _roleRepository ?? (_roleRepository = new GenericRepository<MemberRole>(_context)); }
        }

        public GenericRepository<Member> MemberRepository
        {
            get { return _memberRepository ?? (_memberRepository = new GenericRepository<Member>(_context)); }
        }

        public GenericRepository<MemberToProject> MemberToProjectsRepository
        {
            get { return _memberToProjectRepository ?? (_memberToProjectRepository = new GenericRepository<MemberToProject>(_context)); }
        }

        public GenericRepository<Attendance> AttendanceRepository
        {
            get { return _attendanceRepository ?? (_attendanceRepository = new GenericRepository<Attendance>(_context)); }
        }

        public MemberAttendanceRepository<MemberAttendance> MemberAttendanceRepository
        {
            get { return _memberAttendanceRepository ?? (_memberAttendanceRepository = new MemberAttendanceRepository<MemberAttendance>(_context)); }
        }

        public GenericRepository<Gender> GenderRepository
        {
            get { return _genderRepository ?? (_genderRepository = new GenericRepository<Gender>(_context)); }
        }

        public GenericRepository<ReferralType> ReferralTypeRepository
        {
            get { return _referralTypeRepository ?? (_referralTypeRepository = new GenericRepository<ReferralType>(_context)); }
        }

        public GenericRepository<SexualOrientation> SexualOrientationRepository
        {
            get { return _sexualOrientationRepository ?? (_sexualOrientationRepository = new GenericRepository<SexualOrientation>(_context)); }
        }
        
        public GenericRepository<LivingArea> LivingAreaRepository
        {
            get { return _livingAreaRepository ?? (_livingAreaRepository = new GenericRepository<LivingArea>(_context)); }
        }
        public GenericRepository<Ethnicity> EthinicityRepository
        {
            get { return _ethinicityRepository ?? (_ethinicityRepository = new GenericRepository<Ethnicity>(_context)); }
        }
        public GenericRepository<Nomination> NominationRepository
        {
            get { return _nominationRepository ?? (_nominationRepository = new GenericRepository<Nomination>(_context)); }
        }
        public GenericRepository<MemberRole> MemberRoleRepository
        {
            get { return _memberRoleRepository ?? (_memberRoleRepository = new GenericRepository<MemberRole>(_context)); }
        }
        public GenericRepository<FundingResponsibility> FundingResponsibilityRepository
        {
            get { return _fundingResponsibilityRepository ?? (_fundingResponsibilityRepository = new GenericRepository<FundingResponsibility>(_context)); }
        }
        public GenericRepository<UserType> UserTypeRepository
        {
            get { return _userTypeRepository ?? (_userTypeRepository = new GenericRepository<UserType>(_context)); }
        }
        public GenericRepository<Notes> NotesRepository
        {
            get { return _notesRepository ?? (_notesRepository = new GenericRepository<Notes>(_context)); }
        }
        public GenericRepository<ReportDataModel> ReportRepository
        {
            get { return _reportRepository ?? (_reportRepository = new GenericRepository<ReportDataModel>(_context)); }
        }
        public GenericRepository<AgeBracket> AgeBracketRepository
        {
            get { return _ageBracketRepository ?? (_ageBracketRepository = new GenericRepository<AgeBracket>(_context)); }
        }
        public GenericRepository<GroupByClause> GroupByRepository
        {
            get { return _groupByRepository ?? (_groupByRepository = new GenericRepository<GroupByClause>(_context)); }
        }
        public GenericRepository<BinaryFile> BinaryFileRepository
        {
            get { return _binaryFileRepository ?? (_binaryFileRepository = new GenericRepository<BinaryFile>(_context)); }
        }
        public GenericRepository<AdminAlert> AdminAlertRepository
        {
            get { return _adminAlertRepository ?? (_adminAlertRepository = new GenericRepository<AdminAlert>(_context)); }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;
        


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}