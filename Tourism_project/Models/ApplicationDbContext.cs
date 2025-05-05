using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Ocsp;
using System;
using System.Data;
using System.Diagnostics;
using static Tourism_project.Models.Room;

namespace Tourism_project.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationTourism>
    {
        public ApplicationDbContext()
        {
            
        }
        
        public ApplicationDbContext(DbContextOptions options):base(options) 
        {
            
        }


        #region DbSets

        public DbSet<HotelRestriction> HotelRestrictions { get; set; }
        public DbSet<RestrictionType> RestrictionTypes { get; set; }

        // جدول المستخدمين (Identity Users)
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<ApplicationTourism> Users { get; set; }
        public DbSet<Tourism> users { get; set; }
        public DbSet<TourismType> TourismTypes { get; set; }
        public DbSet<TouristTourismType> TouristTourismTypes { get; set; }
        public DbSet<ACtivity> Activities { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Booking> bookings { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<MediaHotels> Media { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<HotelService> HotelServices { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomMedia> RoomMedias { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> paymentMethods { get; set; }


      

        public DbSet<TourismTypeLocation> TourismTypeLocations { get; set; }

        public DbSet<TemporaryOtp> TemporaryOtp { get; set; }
        public DbSet<TemporaryTourismRegistration> TemporaryTourismRegistrations { get; set; }
        public DbSet<BookingActivity> BookingActivities { get; set; }
        public DbSet<LocationActivity> LocationActivities { get; set; }

        public DbSet<AddActivityToCart> AddActivityToCarts { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ACtivity>().ToTable("Activities");
            #region AddsuperAdmin
            // إنشاء Super Admin Role
            string superAdminRoleId = Guid.NewGuid().ToString();
            string superAdminUserId = Guid.NewGuid().ToString();

            var superAdminRole = new IdentityRole
            {
                Id = superAdminRoleId,
                Name = "SuperAdmin",
                NormalizedName = "SUPERADMIN"
            };

            var superAdminUser = new ApplicationTourism
            {
                Id = superAdminUserId,
                UserName = "hebagamal",
                NormalizedUserName = "HEBAGAMAL",
                Email = "hg2194106@gmail.com",
                NormalizedEmail = "HG2194106@GMAIL.COM",
                EmailConfirmed = true,
                PhoneNumber = "123456789",
                PhoneNumberConfirmed = true
            };
            // تشفير كلمة المرور
            PasswordHasher<ApplicationTourism> passwordHasher = new PasswordHasher<ApplicationTourism>();
            superAdminUser.PasswordHash = passwordHasher.HashPassword(superAdminUser, "Admin@123");

            // ربط المستخدم بالدور
            var superAdminUserRole = new IdentityUserRole<string>
            {
                UserId = superAdminUserId,
                RoleId = superAdminRoleId
            };

            // إضافة البيانات إلى قاعدة البيانات
            modelBuilder.Entity<IdentityRole>().HasData(superAdminRole);
            modelBuilder.Entity<ApplicationTourism>().HasData(superAdminUser);
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(superAdminUserRole);
            #endregion

            #region إضافة قيد فريد على رقم الجواز PassportNumber
            // إضافة قيد فريد على رقم الجواز PassportNumber
            modelBuilder.Entity<Tourism>()
                .HasIndex(t => t.PassportNumber)
                .IsUnique();
            #endregion

            #region  // تعريف العلاقة بين Tourism و AspNetUsers

            // تعريف العلاقة بين Tourism و AspNetUsers
            modelBuilder.Entity<Tourism>()
                .HasOne(t => t.AspNetUser)
                .WithMany() // إذا لم تكن هناك قائمة من الـ Tourism في AspNetUsers
                .HasForeignKey(t => t.AspNetUserId)
                .OnDelete(DeleteBehavior.Cascade); // حذف السائح عند حذف المستخدم

            #endregion

            #region  تكوين العلاقة بين TemporaryOtp و TemporaryTourismRegistration
            // تكوين العلاقة بين TemporaryOtp و TemporaryTourismRegistration
            modelBuilder.Entity<TemporaryOtp>()
                .HasOne(otp => otp.TemporaryUser)
                .WithMany(user => user.Otps)
                .HasForeignKey(otp => otp.TemporaryUserId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region   // العلاقة many-to-many بين TourismType و Location عبر TourismTypeLocation
            // العلاقة many-to-many بين TourismType و Location عبر TourismTypeLocation
            modelBuilder.Entity<TourismTypeLocation>()
                .HasKey(ttl => new { ttl.TourismTypeId, ttl.LocationId });

            modelBuilder.Entity<TourismTypeLocation>()
                .HasOne(ttl => ttl.TourismType)
                .WithMany(tt => tt.TourismTypeLocations)
                .HasForeignKey(ttl => ttl.TourismTypeId);

            modelBuilder.Entity<TourismTypeLocation>()
                .HasOne(ttl => ttl.Location)
                .WithMany(l => l.TourismTypeLocations)
                .HasForeignKey(ttl => ttl.LocationId);
            #endregion

            #region  // العلاقة بين Hotel و Services (كثير لكثير) من خلال جدول HotelService
            // العلاقة بين Hotel و Services (كثير لكثير) من خلال جدول HotelService
            modelBuilder.Entity<HotelService>()
                .HasKey(hs => new { hs.HotelId, hs.ServiceId });
            modelBuilder.Entity<HotelService>()
                .HasOne(hs => hs.Hotel)
                .WithMany(h => h.HotelServices)
                .HasForeignKey(hs => hs.HotelId);
            modelBuilder.Entity<HotelService>()
                .HasOne(hs => hs.Service)
                .WithMany(s => s.HotelServices)
                .HasForeignKey(hs => hs.ServiceId);
            #endregion

            #region  // العلاقة بين Activity و Location (كثير لكثير) من خلال جدول LocationActivity
            // العلاقة بين Activity و Location (كثير لكثير) من خلال جدول LocationActivity
            modelBuilder.Entity<LocationActivity>()
                .HasKey(la => new { la.LocationId, la.ActivityId });
            modelBuilder.Entity<LocationActivity>()
                .HasOne(la => la.Location)
                .WithMany(l => l.locationActivities)
                .HasForeignKey(la => la.LocationId);
            modelBuilder.Entity<LocationActivity>()
                .HasOne(la => la.Activity)
                .WithMany(a => a.locationActivities)
                .HasForeignKey(la => la.ActivityId);
            #endregion

            #region // العلاقة بين Booking و Activity (كثير لكثير) من خلال جدول BookingActivity
            // العلاقة بين Booking و Activity (كثير لكثير) من خلال جدول BookingActivity
            modelBuilder.Entity<BookingActivity>()
                .HasKey(ba => new { ba.BookingId, ba.ActivityId });
            modelBuilder.Entity<BookingActivity>()
                .HasOne(ba => ba.Booking)
                .WithMany(b => b.BookingActivities)
                .HasForeignKey(ba => ba.BookingId);
            modelBuilder.Entity<BookingActivity>()
                .HasOne(ba => ba.Activity)
                .WithMany(a => a.BookingActivities)
                .HasForeignKey(ba => ba.ActivityId);
            #endregion

            #region  // العلاقة many-to-many بين Transportation و Location عبر TransportationLocation
            //// العلاقة many-to-many بين Transportation و Location عبر TransportationLocation
            //modelBuilder.Entity<TransportationLocation>()
            //    .HasKey(tl => new { tl.TransportationId, tl.LocationId });

            //modelBuilder.Entity<TransportationLocation>()
            //    .HasOne(tl => tl.Transportation)
            //    .WithMany(t => t.TransportationLocations)
            //    .HasForeignKey(tl => tl.TransportationId);

            //modelBuilder.Entity<TransportationLocation>()
            //    .HasOne(tl => tl.Location)
            //    .WithMany(l => l.TransportationLocations)
            //    .HasForeignKey(tl => tl.LocationId);
            #endregion

            #region // Location -> Hotels
            // Location -> Hotels
            modelBuilder.Entity<Hotel>()
                .HasOne(h => h.Location)
                .WithMany(l => l.Hotels)
                .HasForeignKey(h => h.LocationId);

            #endregion

            #region  // Hotel -> Rooms

            // Hotel -> Rooms
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId);

            #endregion

            #region  Hotel -> Media 
            // تحديد العلاقة بين Hotel و Media
            modelBuilder.Entity<MediaHotels>()
                .HasOne(m => m.Hotel)  // لكل وسائط (Media) فندق واحد
                .WithMany(h => h.Media)  // لكل فندق عدة وسائط
                .HasForeignKey(m => m.HotelId)  // المفتاح الأجنبي
                .OnDelete(DeleteBehavior.Cascade);  // إذا تم حذف فندق، يتم حذف الوسائط الخاصة به أيضًا

            #endregion

            #region Room-> media

            modelBuilder.Entity<RoomMedia>()
                .HasOne(rm => rm.Room)
                .WithMany(r => r.Media)
                .HasForeignKey(rm => rm.RoomId)
                .OnDelete(DeleteBehavior.Cascade); ;

            #endregion

            #region  // تحديد الدقة (precision) والمقياس (scale)
            modelBuilder.Entity<Room>()
                .Property(r => r.PricePerNight)
                .HasPrecision(18, 2);  // تحديد الدقة (precision) والمقياس (scale)

            #endregion

            #region Booking -> Room:
            modelBuilder.Entity<Booking>()
             .HasOne(b => b.Room)
             .WithMany(r => r.Bookings)
             .HasForeignKey(b => b.RoomId)
             .OnDelete(DeleteBehavior.Cascade); // يمكنك تعديل هذا السلوك حسب احتياجاتك

            #endregion

            #region Booking -> PaymentMethod:
            modelBuilder.Entity<PaymentMethod>()
    .HasKey(p => p.PaymentMethodId); // تأكد من تطابق المفتاح الأساسي مع الجدول


            // ✅ العلاقة 1 - 1 بين Booking و Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ العلاقة 1-N بين PaymentMethod و Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany(pm => pm.Payments)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict); // لا تحذف طريقة الدفع عند حذف الدفع

            // ✅ تحديد نوع العمود لـ TransactionFee
            modelBuilder.Entity<PaymentMethod>()
                .Property(p => p.TransactionFee)
                .HasColumnType("decimal(18,2)");

            #endregion

            #region  Tourist -> Booking:

            // تكوين العلاقة بين السائح والحجز
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Tourist)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TouristId);
            #endregion

            #region // تكوين العلاقة بين Tourist و TourismType باستخدام الجدول الوسيط


            // تكوين العلاقة بين Tourist و TourismType باستخدام الجدول الوسيط
            modelBuilder.Entity<TouristTourismType>()
                .HasKey(tt => new { tt.TouristId, tt.TourismTypeId });

            modelBuilder.Entity<TouristTourismType>()
                .HasOne(tt => tt.Tourist)
                .WithMany(t => t.TouristTourismTypes)
                .HasForeignKey(tt => tt.TouristId);

            modelBuilder.Entity<TouristTourismType>()
                .HasOne(tt => tt.TourismType)
                .WithMany(tt => tt.TouristTourismTypes)
                .HasForeignKey(tt => tt.TourismTypeId);
            #endregion


            modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Hotel)
            .WithMany(h => h.Favorites)
            .HasForeignKey(f => f.ItemId)
            .HasPrincipalKey(h => h.HotelId)
            .IsRequired(false);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Activity)
                .WithMany(a => a.Favorites)
                .HasForeignKey(f => f.ItemId)
                .HasPrincipalKey(a => a.ActivityId)
                .IsRequired(false);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Location)
                .WithMany(l => l.Favorites)
                .HasForeignKey(f => f.ItemId)
                .HasPrincipalKey(l => l.Id)
                .IsRequired(false);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.TourismType)
                .WithMany(t => t.Favorites)
                .HasForeignKey(f => f.ItemId)
                .HasPrincipalKey(t => t.Id)
                .IsRequired(false);


            // العلاقة many-to-many بين Hotel و RestrictionType عبر جدول وسيط HotelRestriction
            modelBuilder.Entity<HotelRestriction>()
                .HasOne(hr => hr.Hotel)
                .WithMany(h => h.Restrictions)  // هذا الجزء في فئة Hotel
                .HasForeignKey(hr => hr.HotelId);  // مفتاح الفندق الأجنبي في HotelRestriction

            modelBuilder.Entity<HotelRestriction>()
                .HasOne(hr => hr.RestrictionType)
                .WithMany(rt => rt.HotelRestrictions)  // هذا الجزء في فئة RestrictionType
                .HasForeignKey(hr => hr.RestrictionTypeId);  // مفتاح RestrictionType الأجنبي في HotelRestriction

            // تحديد المفتاح المركب للجدول الوسيط
            modelBuilder.Entity<TourismTypeLocation>()
                .HasKey(tl => new { tl.TourismTypeId, tl.LocationId });


            modelBuilder.Entity<MediaHotels>()
        .HasKey(m => m.MediaId); // تحديد MediaId كمفتاح أساسي

            modelBuilder.Entity<RoomMedia>()
       .HasKey(m => m.MediaId); // تحديد المفتاح الأساسي

            modelBuilder.Entity<Hotel>()
       .Property(h => h.HotelId)
       .HasColumnName("Id"); // هذا يربط HotelId مع العمود Id في قاعدة البيانات



            // تحديد العلاقة بين TripCart و Activity
            modelBuilder.Entity<AddActivityToCart>()
                .HasOne(tc => tc.Activity) // السلة تحتوي على نشاط واحد
                .WithMany() // النشاط يمكن أن يكون له عدة سجلات في السلة
                .HasForeignKey(tc => tc.ActivityId) // مفتاح النشاط في السلة
                .OnDelete(DeleteBehavior.Cascade); // حذف العناصر في السلة إذا تم حذف النشاط



        }

    }
}
