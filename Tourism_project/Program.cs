using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using Tourism_project.Models;
using Tourism_project.Services;
using Tourism_project.Settings;


namespace Tourism_project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


                 #region Services
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token below."
                });
                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {

                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

                builder.Services.AddIdentity<ApplicationTourism, IdentityRole>()
                                .AddEntityFrameworkStores<ApplicationDbContext>()
                                .AddDefaultTokenProviders();
                // Authentication (JWT Bearer)
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWT:ValidIssure"],
                        ValidAudience = builder.Configuration["JWT:ValidAudance"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                    };

                    //  Œ’Ì’ —œÊœ «·√Œÿ«¡
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            var result = new
                            {
                                statusCode = 401,
                                message = "Unauthorized access. Invalid or missing token."
                            };

                            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";

                            var result = new
                            {
                                statusCode = 403,
                                message = "Forbidden. You do not have permission to access this resource."
                            };

                            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
                        }
                    };
                });


                builder.Services.AddScoped<IEmailServices, EmailServices>();


                builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSettings"));

                //  „ﬂÌ‰ «·Ã·”« 
                builder.Services.AddDistributedMemoryCache(); // «· Œ“Ì‰ ›Ì «·–«ﬂ—… · Œ“Ì‰ «·Ã·”« 
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30); //  ÕœÌœ „œ… ’·«ÕÌ… «·Ã·”…
                    options.Cookie.HttpOnly = true; // “Ì«œ… «·√„«‰ » ÕœÌœ √‰ «·ﬂÊﬂÌ“ Ì„ﬂ‰ «·Ê’Ê· ≈·ÌÂ« ›ﬁÿ „‰ «·”Ì—›—
                });


            builder.Services.AddHostedService<BookingStatusUpdateService>();

            builder.Services.AddHostedService<PaymentCleanupService>();

            builder.Services.AddHostedService<RoomAvailabilityService>();


            #endregion

            #region Configuration

            var app = builder.Build();

               // if (app.Environment.IsDevelopment())
                //{
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tourism API v1");
                        options.RoutePrefix = string.Empty;
                    });
            // }



            app.UseStaticFiles();
                app.UseSession();
                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                app.Run();


                #endregion


            }
       
    }
    
}