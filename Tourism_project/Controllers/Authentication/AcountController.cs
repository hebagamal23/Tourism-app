using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Tourism_project.Dtos.Authentication;
using Tourism_project.Models;
using Tourism_project.Services;
using Tourism_project.Settings;
using Microsoft.Extensions.Logging;

namespace Tourism_project.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcountController : ControllerBase
    {

        private readonly ILogger<AcountController> _logger;
        private readonly UserManager<ApplicationTourism> userManager;
        private readonly ApplicationDbContext dbContext;
        private readonly IEmailServices _emailServices;
        private readonly IConfiguration config;
        private readonly RoleManager<IdentityRole> _roleManager;


        public IPasswordHasher<ApplicationTourism> PasswordHasher { get; }

        #region Constructor
        public AcountController(ILogger<AcountController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationTourism> userManager, ApplicationDbContext dbContext, IEmailServices emailServices, IConfiguration config, IPasswordHasher<ApplicationTourism> passwordHasher)
        {
            _logger = logger;
            this.userManager = userManager;
            this.dbContext = dbContext;
            this._emailServices = emailServices;
            this.config = config;
            _roleManager = roleManager;
            PasswordHasher = passwordHasher;
        }


        #endregion

        #region Function-GenerateOtp
        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();

        }
        #endregion

        #region EndPoint_SendVerificationCode
        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode(TourismRegistration tourismRegistrationDto)
        {
            if (!new EmailAddressAttribute().IsValid(tourismRegistrationDto.Email))
            {
                return BadRequest(new { statusCode = 400, message = "The email address is not valid." });
            }

            var password = tourismRegistrationDto.PassWord;

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || password.Length > 100)
            {
                return BadRequest(new { statusCode = 400, message = "Password must be between 6 and 100 characters." });
            }

            if (!password.Any(char.IsUpper))
            {
                return BadRequest(new { statusCode = 400, message = "Password must contain at least one uppercase letter." });
            }

            if (!password.Any(char.IsDigit))
            {
                return BadRequest(new { statusCode = 400, message = "Password must contain at least one number." });
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return BadRequest(new { statusCode = 400, message = "Password must contain at least one special character." });
            }

            var existingUser = await userManager.FindByEmailAsync(tourismRegistrationDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { statusCode = 400, message = "This email is already registered." });
            }

            var existingPass = await userManager.FindByEmailAsync(tourismRegistrationDto.PassPortNumber.ToString());
            if (existingPass != null)
            {
                return BadRequest(new { statusCode = 400, message = "This Passport is already Exist." });
            }

            var otp = GenerateOtp();

            var tempUser = new TemporaryTourismRegistration
            {
                FullName = tourismRegistrationDto.FullName,
                Email = tourismRegistrationDto.Email,
                Password = tourismRegistrationDto.PassWord,
                PassportNumber = tourismRegistrationDto.PassPortNumber
            };

            dbContext.TemporaryTourismRegistrations.Add(tempUser);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Temporary user saved with ID: {tempUser.Id}");

            var tempOtp = new TemporaryOtp
            {
                OtpCode = otp,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10),
                TemporaryUserId = tempUser.Id, 
                Email = tourismRegistrationDto.Email
            };

            dbContext.TemporaryOtp.Add(tempOtp);

            await dbContext.SaveChangesAsync();

            Console.WriteLine($"OTP saved for email: {tourismRegistrationDto.Email}, OTP: {otp}");

            string textBody = $"Dear {tourismRegistrationDto.FullName},\n\nThank you for registering. Your verification code is:\n\n{otp}\n\nPlease use this code to complete your registration. The code is valid for 10 minutes.\n\nBest regards,\nTourism Team.";

            string htmlBody = $@"
                                <p>Dear {tourismRegistrationDto.FullName},</p>
                                <p>Thank you for registering. Your verification code is:</p>
                                <h2>{otp}</h2>
                                <p>Please use this code to complete your registration. The code is valid for 10 minutes.</p>
                                <p>Best regards,<br>Tourism Team.</p>  ";

            await _emailServices.SendVerificationEmail(
                tourismRegistrationDto.Email,
                "Your Verification Code",
                textBody,
                htmlBody
            );

            return Ok(new { message = "Verification code sent successfully. Please check your email." });
        }
        #endregion

        #region EndPoint_VerifyCodeAndRegister

        [HttpPost("VerifyCodeAndRegister")]
        public async Task<IActionResult> VerifyCodeAndRegister(string verificationCode)
        {
            var otpEntry = await dbContext.TemporaryOtp
                .FirstOrDefaultAsync(o => o.OtpCode == verificationCode);

            if (otpEntry == null || otpEntry.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest(new { statusCode = 400, message = "Invalid or expired verification code." });
            }

            var userEntry = await dbContext.TemporaryTourismRegistrations
                .FirstOrDefaultAsync(u => u.Id == otpEntry.TemporaryUserId);

            if (userEntry == null)
            {
                return BadRequest(new { statusCode = 400, message = "Registration data is missing." });
            }

            var applicationTourismUser = new ApplicationTourism
            {
                UserName = userEntry.FullName,
                Email = userEntry.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(applicationTourismUser, userEntry.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                }
                return BadRequest(new { statusCode = 400, message = "Failed to create user.", errors = result.Errors });
            }
            
            var insertTourism = new Tourism
            {
                PassportNumber = userEntry.PassportNumber,
                AspNetUserId = applicationTourismUser.Id,
                Poster = "posters/DefualtProfile.jpeg"
            };
           
            var roleExist = await _roleManager.RoleExistsAsync("Tourist");
            if (!roleExist)
            {

                await _roleManager.CreateAsync(new IdentityRole("Tourist"));
            }

            var roleResult = await userManager.AddToRoleAsync(applicationTourismUser, "Tourist");

            if (!roleResult.Succeeded)
            {
                return BadRequest(new { statusCode = 400, message = "Failed to assign role." });
            }


            dbContext.users.Add(insertTourism);
            await dbContext.SaveChangesAsync();

            return Ok(new { message = $"Welcome, {applicationTourismUser.UserName}! You have registered successfully." });
        }
        #endregion

        #region EndPoint_ResendVerificationCode

        [HttpPost("ResendVerificationCode")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeRequestDto requestDto)
        {
            var tempUser = await dbContext.TemporaryTourismRegistrations
                .FirstOrDefaultAsync(u => u.Email == requestDto.Email);

            if (tempUser == null)
            {
                return BadRequest(new { statusCode = 400, message = "No registration found for this email." });
            }

            var otp = GenerateOtp(); 

            var existingOtp = await dbContext.TemporaryOtp
                .FirstOrDefaultAsync(o => o.Email == requestDto.Email);

            if (existingOtp != null)
            {
                existingOtp.OtpCode = otp;
                existingOtp.ExpiryDate = DateTime.UtcNow.AddMinutes(10); 
                dbContext.TemporaryOtp.Update(existingOtp);
            }
            else
            {
                var newOtp = new TemporaryOtp
                {
                    OtpCode = otp,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(10), 
                    TemporaryUserId = tempUser.Id,
                    Email = requestDto.Email
                };
                dbContext.TemporaryOtp.Add(newOtp);
            }
            await dbContext.SaveChangesAsync();

            string textBody = $"Dear {tempUser.FullName},\n\nYour new verification code is: {otp}\n\nThe code is valid for 10 minutes.";
            string htmlBody = $@"
                                <p>Dear {tempUser.FullName},</p>
                                <p>Your new verification code is: <strong>{otp}</strong></p>
                                <p>The code is valid for 10 minutes.</p>";

            await _emailServices.SendVerificationEmail(tempUser.Email, "Your New Verification Code", textBody, htmlBody);

            return Ok(new { message = "New verification code sent successfully." });
        }
        #endregion

        #region EndPoint_Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid login request."
                });
            }

            var user = await userManager.FindByEmailAsync(userDto.UserName);
            if (user == null)
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Invalid username or password."
                });
            }

            bool isPasswordValid = await userManager.CheckPasswordAsync(user, userDto.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    statusCode = 401,
                    message = "Invalid username or password."
                });
            }

            var tourist = dbContext.users.FirstOrDefault(t => t.AspNetUserId == user.Id);
            int touristId = tourist != null ? tourist.TouristId : 0;

            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.UserName),
                                new Claim(ClaimTypes.NameIdentifier, user.Id),
                                new Claim("TouristId", touristId.ToString())
                            };

            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = config["JWT:Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "JWT Secret key is not configured."
                });
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

       
            var token = new JwtSecurityToken(
                issuer: config["JWT:ValidIssure"],
                audience: config["JWT:ValidAudance"],
                expires: DateTime.UtcNow.AddHours(3), 
                signingCredentials: signingCredentials,
                claims: claims
            );

            return Ok(new
            {
                statusCode = 200,
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        #endregion

        #region EndPoint_forgot-password 
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request." });

                // إزالة سجلات OTP القديمة
                var existingOtps = dbContext.TemporaryOtp
                    .Where(o => o.Email == model.Email);
                dbContext.TemporaryOtp.RemoveRange(existingOtps);

                // التحقق من وجود المستخدم في جدول TemporaryTourismRegistrations
                var user = await dbContext.TemporaryTourismRegistrations
                    .FirstOrDefaultAsync(t => t.Email == model.Email);
                if (user == null)
                    return BadRequest(new { statusCode = 400, message = "User not found." });

                // التحقق من وجود TemporaryUserId في جدول TemporaryTourismRegistrations
                var tourismRegistration = await dbContext.TemporaryTourismRegistrations
                    .FirstOrDefaultAsync(t => t.Id == user.Id);
                if (tourismRegistration == null)
                    return BadRequest(new { success = false, message = "No associated tourism registration found." });

                // إنشاء OTP جديد
                string otp = GenerateOtp();
                DateTime expirationTime = DateTime.UtcNow.AddMinutes(5);

                var otpVerification = new TemporaryOtp
                {
                    Email = model.Email,
                    OtpCode = otp,
                    ExpiryDate = expirationTime,
                    IsVerified = false,
                    TemporaryUserId = user.Id  // ربط الـ OTP بالـ TemporaryUserId
                };

                // إضافة OTP إلى قاعدة البيانات
                await dbContext.TemporaryOtp.AddAsync(otpVerification);
                await dbContext.SaveChangesAsync();

                // إرسال OTP عبر البريد الإلكتروني
                            var emailBody = $@"
                    <p>Hi there,</p>
                    <p>We received a request to reset your password, and we're here to help!</p>
                    <p>Your OTP (One-Time Password) to reset your password is:</p>
                    <h2 style='color: #2E86C1;'>{otp}</h2>
                    <p>This code is valid for the next <strong>10 minutes</strong>.</p>
                    <p>If you didn't request a password reset, no worries—your account is still safe. You can simply ignore this email.</p>
                    <p>Take care,</p>
                    <p><strong>The Support Team</strong></p>";

                // إرسال البريد الإلكتروني باستخدام الخدمة
                await _emailServices.SendVerificationEmail(model.Email, "Password Reset OTP", emailBody, emailBody);

                return Ok(new
                {
                   
                    message = "OTP sent to your email.",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the ForgotPassword method.");
                return StatusCode(500, new
                {
                    
                    message = "An error occurred while processing your request. Please try again later.",
                    errorDetails = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        #endregion

        #region EndPoint_validate-otp
        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp([FromBody] VerifyOtpDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid request."
                });

            var otpRecord = await dbContext.TemporaryOtp
                .FirstOrDefaultAsync(o => o.OtpCode == model.OTP);

            if (otpRecord == null)
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid OTP."
                });

            if (DateTime.UtcNow > otpRecord.ExpiryDate)
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "OTP has expired."
                } );
            otpRecord.IsVerified = true;
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                
                message = "OTP is valid. ",

            });
        }



        #endregion

        #region EndPoint_reset-password

        [HttpPost("reset-password")]
        
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request.", Errors = ModelState });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest(new { statusCode = 400, Message = "Passwords do not match." });
            }

            var otpRecord = await dbContext.TemporaryOtp
                .FirstOrDefaultAsync(o => o.IsVerified == true);

            if (otpRecord == null)
            {
                return BadRequest(new { statusCode = 400, Message = "Unauthorized or expired request." });
            }

            var user = await userManager.FindByEmailAsync(otpRecord.Email);
            if (user == null)
            {
                return BadRequest(new { statusCode = 400, Message = "User not found." });
            }

            string passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetResult = await userManager.ResetPasswordAsync(user, passwordResetToken, model.NewPassword);
            if (!resetResult.Succeeded)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    Message = "Password reset failed.",
                    Errors = resetResult.Errors.Select(e => e.Description).ToList()
                });
            }

            dbContext.TemporaryOtp.Remove(otpRecord);
            await dbContext.SaveChangesAsync();

            return Ok(new { Message = "Password reset successful." });
        }


        #endregion


    }

}

