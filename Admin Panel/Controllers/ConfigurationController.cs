using DAL.Repository.IServices;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using Entities.CommonModels;
using Entities.CommonModels.ConfigurationModule;
using Entities.DBInheritedModels;
using Entities.DBModels;
using Entities.MainModels;
using Helpers.ApiHelpers;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Helpers.ConversionHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing.Printing;

namespace AdminPanel.Controllers
{
    public class ConfigurationController : BaseController
    {

        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IConfigurationServicesDAL _configurationServicesDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IUserManagementServicesDAL _userManagementServicesDAL;
        private readonly IFilesHelpers _filesHelpers;


        public ConfigurationController(IBasicDataServicesDAL basicDataDAL, IConfigurationServicesDAL configurationServicesDAL, IConstants constants, ICommonServicesDAL commonServicesDAL,
            ISessionManager sessionManag, IUserManagementServicesDAL userManagementServicesDAL, IFilesHelpers filesHelpers)
        {
            this._basicDataDAL = basicDataDAL;
            this._configurationServicesDAL = configurationServicesDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._userManagementServicesDAL = userManagementServicesDAL;
            this._filesHelpers = filesHelpers;

        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.RolesRightsSetting, 0, 0, 0, (short)UserRightsEnum.View_All, 0)]
        public async Task<IActionResult> RolesRightsSetting(RoleRightEntity FormData)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Roles Rights";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.RolesRightsSetting;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            ViewBag.IsAjaxRequest = false;
            ViewBag.SelectedRoleId = FormData.RoleId;

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")//if request is ajax
            {
                ViewBag.IsAjaxRequest = true;
                if (FormData.RoleId < 1)
                {
                    return PartialView("~/Views/Configuration/PartialViews/_RolesRightsSetting.cshtml", model);
                }
            }


            try
            {
                #region Drop Down Area
                //--Get Rights list
                RightEntity RightsFormData = new RightEntity()
                {
                    PageNo = 1,
                    PageSize = 200
                };
                model.RightsList = await this._configurationServicesDAL.GetRightsListDAL(RightsFormData);

                //--Get Roles List
                RolesEntity RoleFormData = new RolesEntity()
                {
                    PageNo = 1,
                    PageSize = 500
                };
                model.RolesList = await this._configurationServicesDAL.GetRolesListDAL(RoleFormData);
                ViewBag.SelectedRoleName = model.RolesList?.FirstOrDefault(x => x.RoleId == FormData.RoleId)?.RoleName;


                //--Get Roles Rights List
                RoleRightEntity RoleRightFormData = new RoleRightEntity()
                {
                    PageNo = 1,
                    PageSize = 100000,
                    EntityId = FormData.EntityId,
                    RoleId = 0
                };
                model.RoleRightList = await this._configurationServicesDAL.GetRolesRightsListDAL(RoleRightFormData);

                //--Get entity list from search dropdown
                EntityEntity EntitySearchDrowdownFormData = new EntityEntity()
                {
                    PageNo = 1,
                    PageSize = 10000
                };
                model.EntityListSearchDropDown = await this._configurationServicesDAL.GetEntitiesListDAL(EntitySearchDrowdownFormData);

                #endregion


                //--Page main list
                EntityEntity EntityFormData = new EntityEntity();
                EntityFormData.PageSize = this._constants.ITEMS_PER_PAGE();
                EntityFormData.PageNo = FormData.PageNo;
                EntityFormData.EntityId = FormData.EntityId;
                model.EntityList = await _configurationServicesDAL.GetEntitiesListDAL(EntityFormData);

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.EntityList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.EntityList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
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
                ViewBag.IsAjaxRequest = true;
                return PartialView("~/Views/Configuration/PartialViews/_RolesRightsSetting.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SaveRolesRights(int RoleId, string recordValueJson)
        {

            int UserID = (await this._sessionManag.GetLoginUserIdFromSession()) ?? 0;

            string result = await this._configurationServicesDAL.SaveUpdateRoleRightsDAL(RoleId, recordValueJson, UserID);

            if (!String.IsNullOrWhiteSpace(result) && result == "Saved Successfully!")
            {
                return Json(new { success = true, message = "Saved Successfully!" });
            }
            else
            {
                return Json(new { success = false, message = result });
            }


        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.SitesLogo, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> SitesLogo(AppConfigEntity FormData)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Site Logo";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.SitesLogo;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.AppConfigKey = "AdminPanelLogo,WebsiteLogo";//-- Get only logos in this page
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.AppConfigList = await _commonServicesDAL.GetAppConfigsValuesAsyncDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.AppConfigList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.AppConfigList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.AppConfigList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.AppConfigList.Cast<dynamic?>().ToList());
                    return ExcelFileResutl;
                }

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
                return PartialView("~/Views/Configuration/PartialViews/_SitesLogo.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.SitesLogo, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateSiteLogo(AppConfigEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (FormData.AppConfigId < 1)
                {
                    return Json(new { success = false, message = "App Config Id is required!" });
                }



                if (string.IsNullOrEmpty(FormData.AppConfigKey))
                {
                    return Json(new { success = false, message = "Logo name is required!" });
                }



                #region image checking

                if (FormData.AttachmentFile != null)
                {

                    string url = await _filesHelpers.SaveFileToDirectory(FormData.AttachmentFile, null);
                    if (!String.IsNullOrWhiteSpace(url))
                    {
                        FormData.AppConfigValue = url;
                    }

                }

                #endregion

                FormData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _configurationServicesDAL.UpdateSiteLogoDAL(FormData, DataOperationType); //UpdateSiteLogoDAL
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ScreensLocalization, 0, 0, 0, (short)UserRightsEnum.View_All, 0)]
        public async Task<IActionResult> ScreensLocalization(ScrnsLocalizationEntity FormData)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Screens Localization";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ScreensLocalization;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion




            try
            {
                #region Drop Down Area
                //--Get languages list
                LanguageEntity languageEntity = new LanguageEntity()
                {
                    PageNo = 1,
                    PageSize = 200
                };
                model.LanguagesList = await this._configurationServicesDAL.GetLanguagesListDAL(languageEntity);
                model.LanguagesList = model?.LanguagesList?.Where(x=>x.IsActive==true).ToList();

                //--Get entity list from search dropdown
                EntityEntity EntitySearchDrowdownFormData = new EntityEntity()
                {
                    PageNo = 1,
                    PageSize = 10000,
                    EntityTypeId = (short)EntityTypesEnum.Screen
                };
                model.EntityListSearchDropDown = await this._configurationServicesDAL.GetEntitiesListDAL(EntitySearchDrowdownFormData);

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


            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ScreensLocalization, 0, 0, 0, (short)UserRightsEnum.View_All, 0)]
        public async Task<IActionResult> ScreensLocalizationSearch(ScrnsLocalizationEntity FormData)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ScreensLocalization;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            try
            {

                ScrnsLocalizationEntity scrnsLocalization = new ScrnsLocalizationEntity()
                {
                    ScreenId = FormData.EntityId ?? 999999999,
                    AppModuleId = (short)AppModulesEnum.AdminPanel,
                    LanguageId = FormData.LanguageId
                };
                var screenLocalizationData = await _commonServicesDAL.GetScreenLocalizationJsonDataDAL(scrnsLocalization);
                if (screenLocalizationData != null && !String.IsNullOrWhiteSpace(screenLocalizationData.LabelsJsonData))
                {
                    model.LocalizationList = new List<LocalizationLabelsInfoEntity>();
                    Dictionary<string, object>? LabelsJsonDataDictionary = new Dictionary<string, object>();

                    LabelsJsonDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>?>(screenLocalizationData.LabelsJsonData);

                    if (LabelsJsonDataDictionary != null && LabelsJsonDataDictionary.ContainsKey("labelsJsonData")  )
                    {
                        model.LocalizationList = JsonConvert.DeserializeObject<List<LocalizationLabelsInfoEntity>?>(LabelsJsonDataDictionary["labelsJsonData"].ToString());

                        if (model?.LocalizationList!=null && model.LocalizationList.Count > 0)
                        {
                            #region pagination data
                            model.pageHelperObj = new PagerHelper();
                            int TotalRecords = model?.LocalizationList?.Count() ?? 0;
                            model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.LocalizationList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                            #endregion

                            #region get required data according to page size
                            FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                            model.LocalizationList = model?.LocalizationList?.Skip((FormData.PageNo - 1) * FormData.PageSize).Take(FormData.PageSize).ToList();
                            #endregion

                            #region assign localization id to each row

                            foreach (var item in model?.LocalizationList)
                            {
                                item.ScrnLocalizationId = screenLocalizationData.ScrnLocalizationId;
                                item.ScreenName = screenLocalizationData.ScreenName;
                                item.LanguageName = screenLocalizationData.LanguageName;
                            }
                            #endregion
                        }

                    }
                }





            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion

            }


            return PartialView("~/Views/Configuration/PartialViews/_ScreensLocalizationSearch.cshtml", model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ScreensLocalization, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> SaveScreenLocalizationLabel(LocalizationLabelsInfoEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {

                #region validation region
                if (String.IsNullOrWhiteSpace(FormData.labelHtmlId))
                {
                    return Json(new { success = false, message = "Html id field is required!" });
                }
                if (String.IsNullOrWhiteSpace(FormData.text))
                {
                    return Json(new { success = false, message = "Text field is required!" });
                }

                if (String.IsNullOrWhiteSpace(FormData.description))
                {
                    return Json(new { success = false, message = "description field is required!" });
                }

                if (FormData.LanguageId==null || FormData.LanguageId < 1)
                {
                    return Json(new { success = false, message = "Please first select language in the search field!" });
                }

                if (FormData.LanguageId == null || FormData.EntityId < 1)
                {
                    return Json(new { success = false, message = "Please first select screen in the search field!" });
                }
                #endregion


                ScrnsLocalizationEntity scrnsLocalization = new ScrnsLocalizationEntity()
                {
                    ScreenId = FormData.EntityId ?? 0,
                    AppModuleId = (short)AppModulesEnum.AdminPanel,
                    LanguageId = FormData.LanguageId ?? 0
                };


                var LocalizationList = new List<LocalizationLabelsInfo>();
                Dictionary<string, object>? LabelsJsonDataDictionary = new Dictionary<string, object>();
                var screenLocalizationData = await _commonServicesDAL.GetScreenLocalizationJsonDataDAL(scrnsLocalization);

                if (screenLocalizationData != null && !String.IsNullOrWhiteSpace(screenLocalizationData.LabelsJsonData))
                {
                    LabelsJsonDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>?>(screenLocalizationData.LabelsJsonData);

                    if (LabelsJsonDataDictionary != null && LabelsJsonDataDictionary.ContainsKey("labelsJsonData") && LabelsJsonDataDictionary["labelsJsonData"] != null)
                    {
                        LocalizationList = JsonConvert.DeserializeObject<List<LocalizationLabelsInfo>?>(LabelsJsonDataDictionary["labelsJsonData"].ToString());

                        if (LocalizationList!=null && LocalizationList.Any(x=>x.labelHtmlId == FormData.labelHtmlId))
                        {
                            foreach (var item in LocalizationList.Where(x => x.labelHtmlId == FormData.labelHtmlId))
                            {
                                item.text = FormData.text;
                                item.description = FormData.description;
                                item.toolTip =String.IsNullOrWhiteSpace(FormData.toolTip) ? "" : FormData.toolTip;
                            }
                        }
                        else
                        {
                            var LocalizationLabelsInfoObj = new LocalizationLabelsInfo();
                            LocalizationLabelsInfoObj.labelHtmlId = FormData.labelHtmlId;
                            LocalizationLabelsInfoObj.description = FormData.description;
                            LocalizationLabelsInfoObj.text = FormData.text;

                            LocalizationList.Add(LocalizationLabelsInfoObj);
                        }

                        LabelsJsonDataDictionary["labelsJsonData"] = LocalizationList;

                    }

                    screenLocalizationData.LabelsJsonData = JsonConvert.SerializeObject(LabelsJsonDataDictionary);
                }
                else
                {
                    screenLocalizationData = new ScrnsLocalizationEntity();
                    screenLocalizationData.ScreenId = Convert.ToInt32(FormData.EntityId);
                    screenLocalizationData.LanguageId = Convert.ToInt32(FormData.LanguageId);
                    screenLocalizationData.AppModuleId =(short)AppModulesEnum.AdminPanel;

                    var LocalizationLabelsInfoObj = new LocalizationLabelsInfo();
                    LocalizationLabelsInfoObj.labelHtmlId = FormData.labelHtmlId;
                    LocalizationLabelsInfoObj.description = FormData.description;
                    LocalizationLabelsInfoObj.text = FormData.text;
                    LocalizationLabelsInfoObj.toolTip = String.IsNullOrWhiteSpace(FormData.toolTip) ? "" : FormData.toolTip;

                    LocalizationList.Add(LocalizationLabelsInfoObj);

                    LabelsJsonDataDictionary.Add("screenId", Convert.ToInt32(FormData.EntityId));
                    LabelsJsonDataDictionary.Add("languageId", Convert.ToInt32(FormData.LanguageId));
                    LabelsJsonDataDictionary["labelsJsonData"] = LocalizationList;

                    screenLocalizationData.LabelsJsonData = JsonConvert.SerializeObject(LabelsJsonDataDictionary);
                }






                screenLocalizationData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _configurationServicesDAL.SaveScreenLocalizationLabelDAL(screenLocalizationData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ScreensLocalization, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateScreenLocalizationLabel(LocalizationLabelsInfoEntity FormData, int DataOperationType = (short)DataOperationType.Update)
        {

            try
            {

                #region validation region
                if (String.IsNullOrWhiteSpace(FormData.labelHtmlId))
                {
                    return Json(new { success = false, message = "Html id field is required!" });
                }
                if (String.IsNullOrWhiteSpace(FormData.text))
                {
                    return Json(new { success = false, message = "Text field is required!" });
                }

                if (String.IsNullOrWhiteSpace(FormData.description))
                {
                    return Json(new { success = false, message = "description field is required!" });
                }

                if ( FormData.ScrnLocalizationId < 1)
                {
                    return Json(new { success = false, message = "Screen localization id is empty for this row!" });
                }

              
                #endregion


                ScrnsLocalizationEntity scrnsLocalization = new ScrnsLocalizationEntity()
                {
                    ScrnLocalizationId = FormData.ScrnLocalizationId ,
                   
                };


                var LocalizationList = new List<LocalizationLabelsInfo>();
                Dictionary<string, object>? LabelsJsonDataDictionary = new Dictionary<string, object>();
                var screenLocalizationData = await _commonServicesDAL.GetScreenLocalizationJsonDataDAL(scrnsLocalization);

                if (screenLocalizationData != null && !String.IsNullOrWhiteSpace(screenLocalizationData.LabelsJsonData))
                {
                    LabelsJsonDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>?>(screenLocalizationData.LabelsJsonData);

                    if (LabelsJsonDataDictionary != null && LabelsJsonDataDictionary.ContainsKey("labelsJsonData") && LabelsJsonDataDictionary["labelsJsonData"] != null)
                    {
                        LocalizationList = JsonConvert.DeserializeObject<List<LocalizationLabelsInfo>?>(LabelsJsonDataDictionary["labelsJsonData"].ToString());

                        if (LocalizationList != null && LocalizationList.Any(x => x.labelHtmlId == FormData.labelHtmlId))
                        {
                            foreach (var item in LocalizationList.Where(x => x.labelHtmlId == FormData.labelHtmlId))
                            {
                                item.text = FormData.text;
                                item.description = FormData.description;
                                item.toolTip = String.IsNullOrWhiteSpace(FormData.toolTip) ? "" : FormData.toolTip;
                            }
                        }
                        else
                        {
                            var LocalizationLabelsInfoObj = new LocalizationLabelsInfo();
                            LocalizationLabelsInfoObj.labelHtmlId = FormData.labelHtmlId;
                            LocalizationLabelsInfoObj.description = FormData.description;
                            LocalizationLabelsInfoObj.text = FormData.text;
                            LocalizationLabelsInfoObj.toolTip = String.IsNullOrWhiteSpace(FormData.toolTip) ? "" : FormData.toolTip;

                            LocalizationList.Add(LocalizationLabelsInfoObj);
                        }

                        LabelsJsonDataDictionary["labelsJsonData"] = LocalizationList;

                    }

                    screenLocalizationData.LabelsJsonData = JsonConvert.SerializeObject(LabelsJsonDataDictionary);
                }
                else
                {
                    return Json(new { success = false, message = "No row found for this data!" });
                }




                screenLocalizationData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _configurationServicesDAL.SaveScreenLocalizationLabelDAL(screenLocalizationData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ScreensLocalization, 0, 0, (short)UserRightsEnum.Delete, 0, 0)]
        public async Task<IActionResult> DeleteScreenLocalizationLabel(LocalizationLabelsInfoEntity FormData, int DataOperationType = (short)DataOperationType.Delete)
        {

            try
            {

                #region validation region
                if (String.IsNullOrWhiteSpace(FormData.labelHtmlId))
                {
                    return Json(new { success = false, message = "Html id field is required!" });
                }
               

                if (FormData.ScrnLocalizationId < 1)
                {
                    return Json(new { success = false, message = "Screen localization id is empty for this row!" });
                }


                #endregion


                ScrnsLocalizationEntity scrnsLocalization = new ScrnsLocalizationEntity()
                {
                    ScrnLocalizationId = FormData.ScrnLocalizationId,

                };


                var LocalizationList = new List<LocalizationLabelsInfo>();
                Dictionary<string, object>? LabelsJsonDataDictionary = new Dictionary<string, object>();
                var screenLocalizationData = await _commonServicesDAL.GetScreenLocalizationJsonDataDAL(scrnsLocalization);

                if (screenLocalizationData != null && !String.IsNullOrWhiteSpace(screenLocalizationData.LabelsJsonData))
                {
                    LabelsJsonDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>?>(screenLocalizationData.LabelsJsonData);

                    if (LabelsJsonDataDictionary != null && LabelsJsonDataDictionary.ContainsKey("labelsJsonData") && LabelsJsonDataDictionary["labelsJsonData"] != null)
                    {
                        LocalizationList = JsonConvert.DeserializeObject<List<LocalizationLabelsInfo>?>(LabelsJsonDataDictionary["labelsJsonData"].ToString());

                        if (LocalizationList != null && LocalizationList.Any(x => x.labelHtmlId == FormData.labelHtmlId))
                        {

                            var deletedRow = LocalizationList.FirstOrDefault(x => x.labelHtmlId == FormData.labelHtmlId);
                            if (deletedRow != null)
                            {
                                LocalizationList.Remove(deletedRow);

                            }

                        }
                       

                        LabelsJsonDataDictionary["labelsJsonData"] = LocalizationList ?? new List<LocalizationLabelsInfo>();

                    }

                    screenLocalizationData.LabelsJsonData = JsonConvert.SerializeObject(LabelsJsonDataDictionary);
                }
                else
                {
                    return Json(new { success = false, message = "No row found for this data!" });
                }




                screenLocalizationData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _configurationServicesDAL.SaveScreenLocalizationLabelDAL(screenLocalizationData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.MenuLocalization, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> MenuLocalization(MenuNavigationEntity FormData)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Menu Localization";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.MenuLocalization;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.menuNavigationList = await _configurationServicesDAL.GetNavMenusListForLocalizationDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.menuNavigationList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.menuNavigationList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.menuNavigationList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.menuNavigationList.Cast<dynamic?>().ToList());
                    return ExcelFileResutl;
                }

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
                return PartialView("~/Views/Configuration/PartialViews/_MenuLocalization.cshtml", model);
            }

            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.MenuLocalizationDetail, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> MenuLocalizationDetail(int MenuNavigationId)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Menu Localization Detail";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.MenuLocalizationDetail;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
               

                LanguageEntity languageEntity = new LanguageEntity()
                {
                    PageNo = 1,
                    PageSize = 200
                };
                model.LanguagesList = await this._configurationServicesDAL.GetLanguagesListDAL(languageEntity);
                model.LanguagesList = model?.LanguagesList?.Where(x => x.IsActive == true).ToList();

                //--get complete list
                MenuNavigationEntity FormData = new MenuNavigationEntity()
                {
                    PageSize = this._constants.ITEMS_PER_PAGE(),
                    MenuNavigationId = MenuNavigationId,
                    PageNo = 1

                };

               
                model.menuNavigationList = await _configurationServicesDAL.GetNavMenusListForLocalizationDAL(FormData);
                model.MenuNavigationObj = model?.menuNavigationList?.FirstOrDefault(x=>x.MenuNavigationId==MenuNavigationId);



                if (model?.MenuNavigationObj != null && !String.IsNullOrEmpty(model.MenuNavigationObj.LocalizationJsonData))
                {
                    model.LocalizationMenuLabelsChildList = JsonConvert.DeserializeObject<List<LocalizationMenuLabelInfoChild>?>(model.MenuNavigationObj.LocalizationJsonData);


                    #region pagination data
                    model.pageHelperObj = new PagerHelper();
                    int TotalRecords = model?.LocalizationMenuLabelsChildList?.Count() ?? 0;
                    model.LocalizationMenuLabelsChildList = model?.LocalizationMenuLabelsChildList?.Skip((FormData.PageNo - 1) * FormData.PageSize).Take(FormData.PageSize).ToList();
                    model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.LocalizationMenuLabelsChildList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                    #endregion


                    foreach (var item in model.LocalizationMenuLabelsChildList)
                    {
                        item.MenuNavigationName = model?.MenuNavigationObj?.MenuNavigationName;
                        item.LanguageName = model?.LanguagesList?.Where(x => x.LanguageId == item.langId).FirstOrDefault()?.LanguageName;
                    }

                }


                 
                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.menuNavigationList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.menuNavigationList.Cast<dynamic?>().ToList());
                    return ExcelFileResutl;
                }

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
                return PartialView("~/Views/Configuration/PartialViews/_MenuLocalizationDetail.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.MenuLocalizationDetail, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> SaveMenuLocalizationLabelText(LocalizationMenuLabelInfoChild FormData, int DataOperationType = (short)DataOperationType.Insert)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            try
            {

                #region validation region
                if (FormData.MenuNavigationId < 1)
                {
                    return Json(new { success = false, message = "Menu navigation id is null!" });
                }
                if (String.IsNullOrWhiteSpace(FormData.text))
                {
                    return Json(new { success = false, message = "Text field is required!" });
                }

                if (FormData.langId == null || FormData.langId < 1)
                {
                    return Json(new { success = false, message = "Language is null" });
                }

                #endregion


                //--get complete list
                MenuNavigationEntity menuEntityForm = new MenuNavigationEntity()
                {
                    PageSize = this._constants.ITEMS_PER_PAGE(),
                    MenuNavigationId = FormData.MenuNavigationId,
                    PageNo = 1

                };
                model.menuNavigationList = await _configurationServicesDAL.GetNavMenusListForLocalizationDAL(menuEntityForm);
                model.MenuNavigationObj = model?.menuNavigationList?.FirstOrDefault(x=>x.MenuNavigationId ==FormData.MenuNavigationId);
                if (model?.MenuNavigationObj != null && !String.IsNullOrEmpty(model.MenuNavigationObj.LocalizationJsonData))
                {
                    model.LocalizationMenuLabelsBaseList = JsonConvert.DeserializeObject<List<LocalizationMenuLabelInfoBase>?>(model.MenuNavigationObj.LocalizationJsonData);

                    LocalizationMenuLabelInfoBase? menuIfo = new LocalizationMenuLabelInfoBase();
                    if (model.LocalizationMenuLabelsBaseList != null && model.LocalizationMenuLabelsBaseList.Count() > 0 &&
                        model.LocalizationMenuLabelsBaseList.Where(x => x.langId == FormData.langId)?.ToList().Count() > 0)
                    {
                        menuIfo = model.LocalizationMenuLabelsBaseList.Where(x => x.langId == FormData.langId).FirstOrDefault();
                        menuIfo.text = FormData.text;
                        if (model == null || model.LocalizationMenuLabelsBaseList == null)
                        {
                            model.LocalizationMenuLabelsBaseList = new List<LocalizationMenuLabelInfoBase>();
                        }
                       
                      
                    }
                    else
                    {
                        menuIfo.langId = FormData.langId;
                        menuIfo.text = FormData.text;
                        if (model == null || model.LocalizationMenuLabelsBaseList == null)
                        {
                            model.LocalizationMenuLabelsBaseList = new List<LocalizationMenuLabelInfoBase>();
                        }
                        model.LocalizationMenuLabelsBaseList.Add(menuIfo);
                    }

                    menuEntityForm.LocalizationJsonData = JsonConvert.SerializeObject(model.LocalizationMenuLabelsBaseList);

                }
                else
                {
                    return Json(new { success = false, message = "No menu information exists" });
                }

                menuEntityForm.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();
               


                string result = await _configurationServicesDAL.SaveMenuLocalizationLabelDAL(menuEntityForm);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.MenuLocalizationDetail, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> UpdateMenuLocalizationLabelText(LocalizationMenuLabelInfoChild FormData, int DataOperationType = (short)DataOperationType.Insert)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            try
            {

                #region validation region
                if (FormData.MenuNavigationId < 1)
                {
                    return Json(new { success = false, message = "Menu navigation id is null!" });
                }
                if (String.IsNullOrWhiteSpace(FormData.text))
                {
                    return Json(new { success = false, message = "Text field is required!" });
                }

                if (FormData.langId == null || FormData.langId < 1)
                {
                    return Json(new { success = false, message = "Language is null" });
                }

                #endregion


                //--get complete list
                MenuNavigationEntity menuEntityForm = new MenuNavigationEntity()
                {
                    PageSize = this._constants.ITEMS_PER_PAGE(),
                    MenuNavigationId = FormData.MenuNavigationId,
                    PageNo = 1

                };
                model.menuNavigationList = await _configurationServicesDAL.GetNavMenusListForLocalizationDAL(menuEntityForm);
                model.MenuNavigationObj = model?.menuNavigationList?.FirstOrDefault(x=>x.MenuNavigationId ==FormData.MenuNavigationId);
                if (model?.MenuNavigationObj != null && !String.IsNullOrEmpty(model.MenuNavigationObj.LocalizationJsonData))
                {
                    model.LocalizationMenuLabelsBaseList = JsonConvert.DeserializeObject<List<LocalizationMenuLabelInfoBase>?>(model.MenuNavigationObj.LocalizationJsonData);

                    LocalizationMenuLabelInfoBase? menuIfo = new LocalizationMenuLabelInfoBase();
                    if (model.LocalizationMenuLabelsBaseList != null && model.LocalizationMenuLabelsBaseList.Count() > 0 &&
                        model.LocalizationMenuLabelsBaseList.Where(x => x.langId == FormData.langId)?.ToList().Count() > 0)
                    {
                        menuIfo = model.LocalizationMenuLabelsBaseList.Where(x => x.langId == FormData.langId).FirstOrDefault();
                        menuIfo.langId = FormData.langId;
                        menuIfo.text = FormData.text;
                        if (model == null || model.LocalizationMenuLabelsBaseList == null)
                        {
                            model.LocalizationMenuLabelsBaseList = new List<LocalizationMenuLabelInfoBase>();
                        }
                       

                    }
                    else
                    {
                        return Json(new { success = false, message = "Text already exit for this menu for the mentioned language!" });

                    }

                    menuEntityForm.LocalizationJsonData = JsonConvert.SerializeObject(model.LocalizationMenuLabelsBaseList);

                }
                else
                {
                    return Json(new { success = false, message = "No menu information exists" });
                }

                menuEntityForm.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();



                string result = await _configurationServicesDAL.SaveMenuLocalizationLabelDAL(menuEntityForm);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.MenuLocalizationDetail, 0, 0, (short)UserRightsEnum.Delete, 0, 0)]
        public async Task<IActionResult> DeleteMenuLocalizationText(LocalizationMenuLabelInfoChild FormData, int DataOperationType = (short)DataOperationType.Delete)
        {
            // ✅ Main Model
            ConfigurationModel model = new ConfigurationModel();

            try
            {

                #region validation region
                if (FormData.MenuNavigationId < 1)
                {
                    return Json(new { success = false, message = "Menu navigation id is null!" });
                }
            
                if (FormData.langId == null || FormData.langId < 1)
                {
                    return Json(new { success = false, message = "Language is null" });
                }

                #endregion

                MenuNavigationEntity menuEntityForm = new MenuNavigationEntity()
                {
                    PageSize = this._constants.ITEMS_PER_PAGE(),
                    MenuNavigationId = FormData.MenuNavigationId,
                    PageNo = 1

                };
                model.menuNavigationList = await _configurationServicesDAL.GetNavMenusListForLocalizationDAL(menuEntityForm);
                model.MenuNavigationObj = model?.menuNavigationList?.FirstOrDefault(x => x.MenuNavigationId == FormData.MenuNavigationId);
                if (model?.MenuNavigationObj != null && !String.IsNullOrEmpty(model.MenuNavigationObj.LocalizationJsonData))
                {
                    model.LocalizationMenuLabelsBaseList = JsonConvert.DeserializeObject<List<LocalizationMenuLabelInfoBase>?>(model.MenuNavigationObj.LocalizationJsonData);
                    var deleteMenuRow = model?.LocalizationMenuLabelsBaseList?.Where(x => x.langId == FormData.langId)?.FirstOrDefault();

                    if (deleteMenuRow != null && model!=null && model.LocalizationMenuLabelsBaseList!=null)
                    {
                        model.LocalizationMenuLabelsBaseList.Remove(deleteMenuRow);

                        menuEntityForm.LocalizationJsonData = JsonConvert.SerializeObject(model.LocalizationMenuLabelsBaseList);
                    }
                    else
                    {
                        return Json(new { success = false, message = "No menu information exists" });
                    }

                   

                }
                else
                {
                    return Json(new { success = false, message = "No menu information exists" });
                }

                menuEntityForm.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();



                string result = await _configurationServicesDAL.SaveMenuLocalizationLabelDAL(menuEntityForm);
                if (!String.IsNullOrWhiteSpace(result) && result == "Saved Successfully!")
                {
                    return Json(new { success = true, message = "Deleted Successfully!" });
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
        public async Task<IActionResult> TranslateAllScreensTestTest()
        {

            try
            {

                //--get complete list
                ScrnsLocalizationEntity scrnListObj = new ScrnsLocalizationEntity()
                {
                    LanguageId = (short)LanguagesEnum.Arabic,

                };
                var allScreenLocalization = await _commonServicesDAL.TestTestDAL(scrnListObj);


                //---Dummy starts here
                List<testLocalization> fffff = new List<testLocalization>();
                foreach (var item in allScreenLocalization)
                {
                    testLocalization kkk = new testLocalization();
                    kkk.ScrnLocalizationId = item.ScrnLocalizationId;
                    kkk.JsonData = JsonConvert.DeserializeObject<dynamic>(item.LabelsJsonData);

                    fffff.Add(kkk);
                }
                var FinalJsonForTranslate = JsonConvert.SerializeObject(fffff);


                //---Dummy ends here




                foreach (var item in allScreenLocalization)
                {
                    ScrnsLocalizationEntity scrnsLocalization = new ScrnsLocalizationEntity()
                    {
                        ScrnLocalizationId = item.ScrnLocalizationId,

                    };
                    var LocalizationList = new List<LocalizationLabelsInfo>();
                    Dictionary<string, object>? LabelsJsonDataDictionary = new Dictionary<string, object>();
                    var screenLocalizationData = await _commonServicesDAL.GetScreenLocalizationJsonDataDAL(scrnsLocalization);

                    if (screenLocalizationData != null && !String.IsNullOrWhiteSpace(screenLocalizationData.LabelsJsonData))
                    {
                        LabelsJsonDataDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>?>(screenLocalizationData.LabelsJsonData);

                        if (LabelsJsonDataDictionary != null && LabelsJsonDataDictionary.ContainsKey("labelsJsonData") && LabelsJsonDataDictionary["labelsJsonData"] != null)
                        {
                            LocalizationList = JsonConvert.DeserializeObject<List<LocalizationLabelsInfo>?>(LabelsJsonDataDictionary["labelsJsonData"].ToString());

                            if (LocalizationList != null)
                            {
                                foreach (var lcl in LocalizationList)
                                {
                                    string newText = string.Empty;
                                    string newDescription = string.Empty;
                                    string newTooltip = string.Empty;
                                    //--call translate api
                                    #region translate api

                                    #endregion

                                    lcl.text = newText;
                                    lcl.description = newDescription;
                                    lcl.toolTip = newTooltip;
                                }
                            }

                            LabelsJsonDataDictionary["labelsJsonData"] = LocalizationList;

                        }

                        screenLocalizationData.LabelsJsonData = JsonConvert.SerializeObject(LabelsJsonDataDictionary);
                    }


                    screenLocalizationData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                    string result = string.Empty;
                    // result = await _configurationServicesDAL.SaveScreenLocalizationLabelDAL(screenLocalizationData);
                    if (!String.IsNullOrWhiteSpace(result) && result == "Saved Successfully!")
                    {
                        return Json(new { success = true, message = "Saved Successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = result });
                    }
                }

                return Json(new { success = false, message = "" });

            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }


    }

    public class testLocalization
    {
        public int ScrnLocalizationId { get; set; }
        public object? JsonData { get; set; }

    }
}
