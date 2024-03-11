using Microsoft.EntityFrameworkCore;

namespace CustomerOnboarding.Entities
{
    public class ApplicationDbContext : AuditableDbContext
    {
       
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<ApplicationUser>().HasIndex(b => b.Id);
            base.OnModelCreating(builder);

        }

        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<MockOTP> MockOTP { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Lga> Lgas { get; set; }
        public DbSet<ApiLogs> ApiLogs { get; set; }

    }
}
