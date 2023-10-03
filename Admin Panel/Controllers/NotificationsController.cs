
using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.DBInheritedModels;
using Entities.DBModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AdminPanel.Controllers
{
    public class NotificationsController : BaseController
    {
        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly INotificationsServicesDAL _notificationsServicesDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IFilesHelpers _filesHelpers;
        private readonly IConfigurationServicesDAL _configurationServicesDAL;

        public NotificationsController(IBasicDataServicesDAL basicDataDAL, IConstants constants, ICommonServicesDAL commonServicesDAL, ISessionManager sessionManag,
            IFilesHelpers filesHelpers, INotificationsServicesDAL notificationsServicesDAL, IConfigurationServicesDAL configurationServicesDAL)
        {
            this._basicDataDAL = basicDataDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._filesHelpers = filesHelpers;
            this._notificationsServicesDAL = notificationsServicesDAL;
            this._configurationServicesDAL = configurationServicesDAL;
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.Admin_Notifications, 0, 0, 0, (short)UserRightsEnum.View_All, 0)]
        public async Task<IActionResult> AdminPanelNotificationsList(AdminPanelNotificationEntity FormData)
        {
            // ✅ Main Model
            NotificationsModel model = new NotificationsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Admin Panel Notifications";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.Admin_Notifications;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.AdminPanelNotificationsList = await _notificationsServicesDAL.GetAdminPanelNotificationsListDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();

                int TotalRecords = model?.AdminPanelNotificationsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.AdminPanelNotificationsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);

                #endregion




            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion

            }

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")//if request is ajax
            {
                return PartialView("~/Views/Notifications/PartialViews/_AdminPanelNotificationsList.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.Admin_Notifications, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> MarkAllAdminNotificationsAsRead()
        {

            try
            {
                int UserID = Convert.ToInt32(await this._sessionManag.GetLoginUserIdFromSession());

                string result = await _notificationsServicesDAL.MarkAllAdminNotificationsAsReadDAL(UserID);
                if (!String.IsNullOrWhiteSpace(result) && result == "Saved Successfully!")
                {
                    return Json(new { success = true, message = "Saved Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = result });
                }


            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }





        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.Admin_Notifications, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> MarkOnlySelectedAdminNotificationsAsRead(AdminPanelNotificationEntity FormData)
        {

            try
            {

                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.SelectedNotificationsIdsForReadJson) ? "Form is valid" : "No notification found for marking!";
                validationList.Add(ValidationMsg);

                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _notificationsServicesDAL.MarkOnlySelectedAdminNotificationsAsReadDAL(FormData);
                if (!String.IsNullOrWhiteSpace(result) && result == "Saved Successfully!")
                {
                    return Json(new { success = true, message = "Saved Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = result });
                }


            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }





        }



        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.Admin_Notifications, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> GetSiteHeaderNotifications()
        {
            // ✅ Main Model
            NotificationsModel model = new NotificationsModel();

            try
            {

                AdminPanelNotificationEntity NotificationFormData = new AdminPanelNotificationEntity()
                {
                    PageNo = 1,
                    PageSize = 20

                };
                model.AdminPanelNotificationsList = await this._notificationsServicesDAL.GetAdminPanelNotificationsListDAL(NotificationFormData);


            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion
            }

            return PartialView("~/Views/Notifications/PartialViews/_SiteHeaderNotifications.cshtml", model);

        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.Admin_Notifications, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> GetHeaderAdminUnreadNotificationsCount()
        {
            // ✅ Main Model
            NotificationsModel model = new NotificationsModel();
            int TotalUnreadNotifications = 0;
            try
            {


                TotalUnreadNotifications = await this._notificationsServicesDAL.GetHeaderAdminUnreadNotificationsCountDAL();


            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion


                return Json(new { success = false, total_unread_notifications = 0 });
            }

            return Json(new { success = true, total_unread_notifications = TotalUnreadNotifications });

        }


        [HttpGet]
        public async Task<IActionResult> GetHeaderLanguages()
        {
            // ✅ Main Model
            NotificationsModel model = new NotificationsModel();
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            try
            {

                LanguageEntity languageEntity = new LanguageEntity()
                {
                    PageNo = 1,
                    PageSize = 200
                };
                model.LanguagesList = await this._configurationServicesDAL.GetLanguagesListDAL(languageEntity);
                model.LanguagesList = model?.LanguagesList?.Where(x => x.IsActive == true).ToList();

            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion
            }

            return PartialView("~/Views/Notifications/PartialViews/_HeaderLangSection.cshtml", model);

        }

        [HttpPost]
        public async Task<IActionResult> setSelectedLanguageInSession(LanguageEntity FormData)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.LanguageCode))
                {
                    return Json(new { success = false, message = "Language code is empty!" });
                }

                _sessionManag.SetLanguageCodeInSession(FormData.LanguageCode);

                return Json(new { success = true, message = "Saved Successfully!" });
            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }

    }
}
