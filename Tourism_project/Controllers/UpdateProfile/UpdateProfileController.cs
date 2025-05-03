using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Tourism_project.Dtos.UpdateProfile;
using Tourism_project.Models;
using Tourism_project.Services;

namespace Tourism_project.Controllers.UpdateProfile
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationTourism> userManager;

        private readonly IEmailServices _emailServices;
        private readonly ApplicationDbContext _context;

        public UpdateProfileController(UserManager<ApplicationTourism> userManager, IEmailServices emailServices, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this._emailServices = emailServices;
            this._context = context;
        }


        #region EndPoint_UpdateUserName

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdateUserName")]
        public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameDto model)
        {
            // التحقق من إدخال اسم المستخدم الجديد
            if (string.IsNullOrWhiteSpace(model.NewUserName))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "New username cannot be empty." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "User not found." });
            }

            user.UserName = model.NewUserName;
            await userManager.UpdateAsync(user);

            return Ok(new { message = "Username updated successfully." });
        }


        #endregion

        #region EndPoint_UpdatePassword

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest( new
                {
                    StatusCode = 400,
                    Message = "User not found." });
            }

            // تحقق من كلمة المرور القديمة
            var checkPassword = await userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!checkPassword)
            {
                return BadRequest( new
                {
                    StatusCode = 400,
                    Message = "Old password is incorrect."});
            }
            // تحقق من أن NewPassword و ConfirmPassword متطابقان
            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest(new { statusCode = 400, Message = "Passwords do not match." });
            }

            // تحديث كلمة المرور
            var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Password updated successfully." });
        }
        #endregion

        #region EndPoint_SendOtpForEmailUpdate

        //[HttpPost("SendOtpForEmailUpdate")]
        //public async Task<IActionResult> SendOtpForEmailUpdate([FromBody] UpdateEmailDto model)
        //{
        //    // التحقق من صحة البريد الإلكتروني الجديد
        //    if (!new EmailAddressAttribute().IsValid(model.NewEmail))
        //    {
        //        return BadRequest("Invalid email format.");
        //    }

        //    // تحقق إذا كان البريد الإلكتروني الجديد مستخدمًا مسبقًا
        //    var existingUser = await userManager.FindByEmailAsync(model.NewEmail);
        //    if (existingUser != null)
        //    {
        //        return BadRequest("This email is already taken.");
        //    }

        //    // إنشاء كود OTP
        //    var otp = new Random().Next(100000, 999999).ToString();

        //    // إرسال الكود عبر خدمة البريد الإلكتروني
        //    string subject = "Email Verification Code";
        //    string message = $"Your verification code is: {otp}. It is valid for 10 minutes.";
        //    await _emailServices.SendVerificationEmail(model.NewEmail, subject, message, message);

        //    // حفظ الكود في الجلسة
        //    HttpContext.Session.SetString("OtpForEmailUpdate", otp);
        //    HttpContext.Session.SetString("NewEmailForUpdate", model.NewEmail);

        //    return Ok(new { message = "OTP sent successfully. Please check your email." });
        //}

        #endregion

        #region EndPoint_UpdateEmailWithOtp

        //[HttpPost("UpdateEmailWithOtp")]
        //public async Task<IActionResult> UpdateEmailWithOtp([FromBody] ConfirmOtpDto model)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var user = await userManager.FindByIdAsync(userId);

        //    if (user == null)
        //    {
        //        return BadRequest("User not found.");
        //    }

        //    // التحقق من كود OTP من الجلسة
        //    var savedOtp = HttpContext.Session.GetString("OtpForEmailUpdate");
        //    var newEmail = HttpContext.Session.GetString("NewEmailForUpdate");

        //    if (string.IsNullOrEmpty(savedOtp) || savedOtp != model.Otp)
        //    {
        //        return BadRequest("Invalid or expired OTP.");
        //    }

        //    // تحديث البريد الإلكتروني
        //    user.Email = newEmail;
        //    user.UserName = newEmail;  // تحديث اسم المستخدم ليطابق البريد الإلكتروني
        //    var result = await userManager.UpdateAsync(user);

        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(result.Errors);
        //    }

        //    // إزالة OTP من الجلسة بعد نجاح العملية
        //    HttpContext.Session.Remove("OtpForEmailUpdate");
        //    HttpContext.Session.Remove("NewEmailForUpdate");

        //    return Ok(new { message = "Email updated successfully." });
        //}
        #endregion

       

        #region EndPoint_UpdateProfilePicture


            [HttpPost("update-profile-picture")]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
            return BadRequest(new
            {
                StatusCode = 400,
                Message = "User not found." });

        var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
        var maxAllowedSize = 2 * 1024 * 1024;

        var fileExtension = Path.GetExtension(dto.Poster.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(new
            {
                StatusCode = 400,
                Messager = "Only .jpg ,.jpeg and .png images are allowed!" });

        if (dto.Poster.Length > maxAllowedSize)
            return BadRequest(new
            {
                StatusCode = 400,
                Message = "Max allowed size for poster is 2MB." });

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string uniqueFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
        string filePath = Path.Combine(directoryPath, uniqueFileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Poster.CopyToAsync(stream);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while saving the file.", Details = ex.Message });
        }

        var tourism = await _context.users.FirstOrDefaultAsync(t => t.AspNetUserId == userId);
        if (tourism == null)
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Tourist not found."
                    });

                tourism.Poster = $"Images/{uniqueFileName}";
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Failed to save poster link to database.", Details = ex.Message });
        }

        return Ok(new
        {
            Message = "Profile picture updated successfully.",
            ImageUrl = tourism.Poster
        });
    }


        #endregion





        #region EndPoint_GetUserProfile


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetUserProfile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                // استخراج الـ UserId من التوكين
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    // التوكن غير صالح أو لا يحتوي على UserId
                    return Unauthorized(new
                    {
                        statusCode = 401,
                        message = "Invalid token or user not authorized."
                    });
                }

                Console.WriteLine($"Extracted UserId: {userId}");

                // ابحث عن المستخدم
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "User not found."
                    });
                }

                // ابحث عن السياحة
                var tourism = await _context.users.FirstOrDefaultAsync(t => t.AspNetUserId == userId);
                if (tourism == null)
                {
                    Console.WriteLine($"Tourist profile not found for UserId: {userId}");
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "Tourist profile not found."
                    });
                }

                // تحويل الصورة إلى Base64 إذا كانت موجودة
                var profilePicture = tourism.Poster;

                // إرجاع البيانات
                return Ok(new
                {
                    
                        UserName = user.UserName,
                        Email = user.Email,
                        ProfilePicture = profilePicture
                    
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while processing the request.",
                    details = ex.Message
                });
            }
        }



        #endregion

    }




}

