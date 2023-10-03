using AdminPanel.Helpers.EmailSenderHelper;
using DAL.Repository.IServices;
using Entities.DBInheritedModels;
using Entities.DBModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Helpers.ConversionHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdminPanel.Controllers
{
    public class AuthenticationController : Controller
    {

        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IEmailSender _emailSender;
        private readonly IUserManagementServicesDAL _userManagementServicesDAL;
        private readonly IConfiguration _configuration;

        public AuthenticationController(ICommonServicesDAL commonServicesDAL, ISessionManager sessionManag, IEmailSender emailSender,
            IUserManagementServicesDAL userManagementServicesDAL, IConfiguration configuration)
        {
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._emailSender = emailSender;
            this._userManagementServicesDAL = userManagementServicesDAL;
            this._configuration = configuration;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginJsonUser(string EmailAddress, string Password)
        {
            if (String.IsNullOrEmpty(EmailAddress) || String.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Please fill both email & password!" });
            }

            try
            {
                UserEntity? usr = new UserEntity();
                Password = CommonConversionHelper.Encrypt(Password);
                usr = await this._commonServicesDAL.GetUserLogin(EmailAddress, Password);

                if (usr != null)
                {
                    if (usr.UserId > 0)
                    {
                        #region set User Data
                        this._sessionManag.SetUserDataInSession(usr);
                        #endregion


                        #region set Menus
                        this._sessionManag.SetMenusInSession();
                        #endregion

                        #region set user roles rights
                        this._sessionManag.SetUserRightsInSession(usr.UserId);
                        #endregion

                        #region set user roles rights
                        this._sessionManag.SetUserRightsInSession(usr.UserId);
                        #endregion

                        #region set app configs values
                        AppConfigEntity appConfigEntity = new AppConfigEntity()
                        {
                            PageNo = 1,
                            PageSize = 200,
                            AppConfigKey = "AdminPanelLogo"  //-- You can pass here comma seperated values with out space

                        };
                        this._sessionManag.SetAdminPanelBasicAppConfigsInSession(appConfigEntity);
                        #endregion

                        


                        return Json(new { success = true, use_type_id = usr.UserTypeId, message = "Login Successfully!" });
                    }
                    else
                    {

                        return Json(new { success = false, message = "Incorrect email or password!" });
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Incorrect email or password!" });
                }
            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured. Please try again", ExMsg = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult LogoutUser()
        {

            string msg = RemoveSessionAndCookies();
            if (msg != "Removed Successfully!")
            {
                return RedirectToAction("Login", "Authentication");
            }

            return RedirectToAction("Login", "Authentication");
        }


        public string RemoveSessionAndCookies()
        {
            string result = "Removed Successfully!";
            HttpContext.Session.Clear();
            if (Request.Cookies != null)
            {
                try
                {
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message;
                    result = "An error occured.";
                    return result;
                }

            }

            return result;

        }

        [HttpGet]
        public IActionResult PasswordRecovery()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ValidateEmailAndSendOTP(string recover_email)
        {
            if (String.IsNullOrEmpty(recover_email))
            {
                return Json(new { success = false, message = "Please fill email field!" });
            }

            try
            {
                //-- 1. Valiedate email from data base if exists
                var user = await _userManagementServicesDAL.GetUserByEmailAddressDAL(recover_email);
                if (user == null || user.UserId < 1)
                {
                    return Json(new { success = false, message = "Incorrect email. Please enter your correct email address!" });
                }

                //-- 2. Generate OTP and save in database
                int OTP = CommonConversionHelper.GenerateRandomNumber();
                string OTPResponseFromDB = await this._userManagementServicesDAL.SaveOTPLogInformationDAL((short)ApiStatusCodes.OK, "Peding", "OTP Generated", OTP, null, recover_email, null, null, true, null);

                if (String.IsNullOrEmpty(OTPResponseFromDB) || OTPResponseFromDB != "Saved Successfully!")
                {
                    return Json(new { success = false, message = "An error occured in saving OTP. Please try again!" });
                }

                //-- 3. Send OTP in email to user
                try
                {
                    List<EmailAddressEntity> emailAddresses = new List<EmailAddressEntity>();
                    emailAddresses.Add(new EmailAddressEntity { DisplayName = "User", Address = recover_email });
                    string SiteTitle = _configuration.GetSection("AppSetting").GetSection("WebsiteTitle").Value;
                    var message = new EmailMessage(emailAddresses, "Recover Password", String.Format("Your OTP for password recovery is: {0}", OTP), String.Format("{0} , Recover Password", SiteTitle));
                    _emailSender.SendEmail(message);
                }
                catch (Exception ex)
                {
                    await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                    return Json(new { success = false, message = "An error occured in sending email. Please try again!" });
                }


                //-- 4. return user success and lets user enter otp that he recieve in email and password and confirm password
                return Json(new { success = true, message = "An OTP has been sent to your email. Please confirm OTP & enter new password!" });

            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured. Please try again", ExMsg = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTPAndChangePassword(string? recover_email, int? Otp, string? Password, string? ConfirmPassword)
        {

            #region validation area

            if (String.IsNullOrEmpty(recover_email))
            {

                return Json(new { success = false, message = "Please fill email field!" });
            }

            if (Otp == null)
            {

                return Json(new { success = false, message = "Please fill OTP field." });
            }

            if (String.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Please enter password." });
            }

            if (String.IsNullOrEmpty(ConfirmPassword))
            {
                return Json(new { success = false, message = "Please enter confirm password." });
            }

            if (Password.Length < 6 || ConfirmPassword.Length < 6)
            {

                return Json(new { success = false, message = "Password & Confirm Password fields lenght should not be less than 6 characters." });
            }

            if (Password != ConfirmPassword)
            {
                return Json(new { success = false, message = "Password does not match!" });
            }

            #endregion


            try
            {

                //-- 1. Valiedate email from data base if exists
                var user = await _userManagementServicesDAL.GetUserByEmailAddressDAL(recover_email);
                if (user == null || user.UserId < 1)
                {
                    return Json(new { success = false, message = "Incorrect email. Please try again!" });
                }

                //-- 2. Validate the OTP from data base
                var IsValidOTP = await this._userManagementServicesDAL.ValidateOTPByEmailDAL(recover_email, Convert.ToInt32(Otp));

                //--Update the OTP Count by Email
                string UpdateOTPResponse = await this._userManagementServicesDAL.UpdateOTPAttemptsByEmailDAL(recover_email);


                if (IsValidOTP != null && !String.IsNullOrWhiteSpace(IsValidOTP.EmailAddress))
                {


                    string PasswordResetResponse = "";

                    //-- 3. Reset user password
                    Password = CommonConversionHelper.Encrypt(Password);
                    PasswordResetResponse = await this._userManagementServicesDAL.ResetUserPasswordDAL(recover_email, Password);


                    //--De activate otps by email address
                    string DeActivateResponse = await this._userManagementServicesDAL.DeActivateOTPsByEmail(recover_email);




                    if (PasswordResetResponse == "Saved Successfully!")
                    {

                        return Json(new { success = true, message = "Password reset successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "An error occured. Please try again" });
                    }


                }
                else
                {
                    return Json(new { success = false, message = "Invalid OTP that you enter!" });
                }



            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured. Please try again", ExMsg = ex.Message });
            }

        }

    }
}
