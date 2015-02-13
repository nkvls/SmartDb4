using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using SmartDb4.Models;

namespace SmartDb4.DAL
{
    public class SmartDbContext : DbContext
    {
        public SmartDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<MemberToProject> MemberToProjects { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<MemberAttendance> MemberAttendances { get; set; }

        public DbSet<Nomination> Nominations { get; set; }
        public DbSet<Classification> Classifications { get; set; }
        public DbSet<FundingResponsibility> FundingResponsibilitys { get; set; }
        public DbSet<Ethnicity> Ethnicitys { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<LivingArea> LivingAreas { get; set; }
        public DbSet<MemberRole> MemberRoles { get; set; }
        public DbSet<AgeBracket> AgeBrackets { get; set; }
        public DbSet<GroupByClause> GroupByClauses { get; set; }
        public DbSet<BinaryFile> BinaryFiles { get; set; }
        public DbSet<AdminAlert> AdminAlerts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //modelBuilder.Configurations.Add(new ProjectMap());

            modelBuilder.Entity<Project>()
                .HasRequired(a => a.Supervisor)
                .WithMany()
                .HasForeignKey(u => u.SupervisorId).WillCascadeOnDelete(false);

            //configuring a primary key
            //modelBuilder.Entity<UserProfile>().HasKey(x => x.UserId);

            //Configuring a Composite Primary Key
            //modelBuilder.Entity<Department>().HasKey(t => new { t.DepartmentID, t.Name });

            //Switching off Identity for Numeric Primary Keys
            //modelBuilder.Entity<Department>().Property(t => t.DepartmentID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            //Specifying the Maximum Length on a Property
            //modelBuilder.Entity<Department>().Property(t => t.Name).HasMaxLength(50);

            //Configuring the Property to be Required
            //odelBuilder.Entity<Department>().Property(t => t.Name).IsRequired();

            //Specifying Not to Map a CLR Property to a Column in the Database
            //modelBuilder.Entity<Department>().Ignore(t => t.Budget);

            //Mapping a CLR Property to a Specific Column in the Database
            //modelBuilder.Entity<Department>().Property(t => t.Name).HasColumnName("DepartmentName");

            //Configuring the Data Type of a Database Column
            //modelBuilder.Entity<Department>().Property(p => p.Name).HasColumnType("varchar");

            //Configuring Properties on a Complex Type
            //modelBuilder.ComplexType<Details>().Property(t => t.Location).HasMaxLength(20);

            //Configuring a Property to Be Used as an Optimistic Concurrency Token
            //modelBuilder.Entity<OfficeAssignment>().Property(t => t.Timestamp).IsConcurrencyToken();

            //Specifying Not to Map a CLR Entity Type to a Table in the Database
            //modelBuilder.Ignore<OnlineCourse>();

            //Mapping an Entity Type to a Specific Table in the Database
            //modelBuilder.Entity<Department>().ToTable("t_Department");
        }

        public DbSet<Gender> Genders { get; set; }
        public DbSet<ReferralType> ReferralTypes { get; set; }
    }
}