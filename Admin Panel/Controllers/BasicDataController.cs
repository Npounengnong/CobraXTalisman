
using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.DBInheritedModels;
using Entities.DBModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AdminPanel.Controllers
{
    public class BasicDataController : BaseController
    {


        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IFilesHelpers _filesHelpers;

        public BasicDataController(IBasicDataServicesDAL basicDataDAL, IConstants constants, ICommonServicesDAL commonServicesDAL, ISessionManager sessionManag,
            IFilesHelpers filesHelpers)
        {
            this._basicDataDAL = basicDataDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._filesHelpers = filesHelpers;
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ColorsList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ColorsList(ColorEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();
            
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Color List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ColorsList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

          

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.ColorsList = await _basicDataDAL.GetColorsListDAL(FormData);

             
                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ColorsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ColorsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.ColorsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.ColorsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/BasicData/PartialViews/_ColorsList.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ColorsList, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateColor(ColorEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.ColorName))
                {
                    return Json(new { success = false, message = "Please fill the color field!" });
                }

                if (String.IsNullOrWhiteSpace(FormData.HexCode))
                {
                    return Json(new { success = false, message = "Please fill the hex code field!" });
                }

                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.ColorId < 1)
                    {
                        return Json(new { success = false, message = "ColorId is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _basicDataDAL.SaveUpdateColorDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CategoriesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CategoriesList(CategoryEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();
           
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Categories List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CategoriesList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

                #region ViewSelf Right Check
                bool SelfRight = await _sessionManag.GetViewSelfRightForLoginUserFromSession();
                if (SelfRight)
                {
                    int? LoginUserId = await _sessionManag.GetLoginUserIdFromSession();
                    FormData.CreatedBy = LoginUserId;
                }
                else
                {
                    FormData.CreatedBy = 0;
                }
                #endregion


                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.CategoryList = await _basicDataDAL.GetCategoriesListDAL(FormData);

                //--get parent categories
                model.ParentCategoryList = await _basicDataDAL.GetParentCategoriesListDAL();

              

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.CategoryList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.CategoryList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.CategoryList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.CategoryList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/BasicData/PartialViews/_CategoriesList.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CategoriesList, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateCategory(CategoryEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.Name))
                {
                    return Json(new { success = false, message = "Please fill the name field!" });
                }



                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.CategoryId < 1)
                    {
                        return Json(new { success = false, message = "Category id is null!" });
                    }

                }

                #region image checking

                if (FormData.AttachmentFile != null)
                {
                    string url = await _filesHelpers.SaveFileToDirectory(FormData.AttachmentFile, null);
                    if (!String.IsNullOrWhiteSpace(url))
                    {
                        AttachmentEntity atch = new AttachmentEntity();
                        atch.AttachmentUrl = url;
                        atch.AttachmentName = FormData.AttachmentFile.FileName;
                        atch.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                        if (FormData.AttachmentId!=null && FormData.AttachmentId>0) //--update case
                        {
                            atch.AttachmentId = Convert.ToInt32(FormData.AttachmentId);
                           

                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);
                        }
                        else
                        {
                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);
                            
                            atch.AttachmentId = AttachmentId;

                            FormData.AttachmentId= AttachmentId;
                        }

                      

                    }

                   

                }

                #endregion

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _basicDataDAL.SaveUpdateCategoryDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.SizesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> SizeList(SizeEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();
           
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Size List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.SizesList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.SizeList = await _basicDataDAL.GetSizeListDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.SizeList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.SizeList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.SizeList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.SizeList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/BasicData/PartialViews/_SizeList.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.SizesList, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateSize(SizeEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.Name))
                {
                    return Json(new { success = false, message = "Please fill the name field!" });
                }

                if (String.IsNullOrWhiteSpace(FormData.ShortName))
                {
                    return Json(new { success = false, message = "Please fill the short name field!" });
                }
                if (FormData.Inches == null || FormData.Centimeters == null)
                {
                    return Json(new { success = false, message = "Inches and centimers fields are required!" });
                }

                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.SizeId < 1)
                    {
                        return Json(new { success = false, message = "Size id is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _basicDataDAL.SaveUpdateSizeDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ManufacturersList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ManufacturersList(ManufacturerEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();
           
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Manufacturers List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ManufacturersList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.ManufacturerList = await _basicDataDAL.GetManufacturerListDAL(FormData);


             

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ManufacturerList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ManufacturerList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.ManufacturerList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.ManufacturerList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/BasicData/PartialViews/_ManufacturersList.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ManufacturersList, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateManufacturer(ManufacturerEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.Name))
                {
                    return Json(new { success = false, message = "Please fill the name field!" });
                }


                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.ManufacturerId < 1)
                    {
                        return Json(new { success = false, message = "Manufacturer id is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _basicDataDAL.SaveUpdateManufacturerDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CurrenciesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CurrenciesList(CurrencyEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();
    
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Currencies List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CurrenciesList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.CurrenciesList = await _basicDataDAL.GetCurrenciesListDAL(FormData);


            

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.CurrenciesList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.CurrenciesList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.CurrenciesList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.CurrenciesList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/BasicData/PartialViews/_CurrenciesList.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CurrenciesList, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateCurrency(CurrencyEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.CurrencyName) || String.IsNullOrWhiteSpace(FormData.CurrencyCode))
                {
                    return Json(new { success = false, message = "Please fill Currency Name and Currency Code fields!" });
                }


                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.CurrencyId < 1)
                    {
                        return Json(new { success = false, message = "Currency id is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _basicDataDAL.SaveUpdateCurrencyDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.AttachmentTypesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> AttachmentTypesList(AttachmentTypeEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Attachment Types List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.AttachmentTypesList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.AttachmentTypeList = await _basicDataDAL.GetAttachmentTypesListDAL(FormData);

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.AttachmentTypeList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.AttachmentTypeList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
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
                return PartialView("~/Views/BasicData/PartialViews/_AttachmentTypesList.cshtml", model);
            }

            return View(model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.PaymentMethodsList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> PaymentMethodsList(PaymentMethodEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Payment Methods List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.PaymentMethodsList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.PaymentMethodsList = await _basicDataDAL.GetPaymentMethodsListDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.PaymentMethodsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.PaymentMethodsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
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
                return PartialView("~/Views/BasicData/PartialViews/_PaymentMethodsList.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.PaymentMethodsList, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdatePaymentMethod(PaymentMethodEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.PaymentMethodName))
                {
                    return Json(new { success = false, message = "Please fill the payment method field!" });
                }


                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.PaymentMethodId < 1)
                    {
                        return Json(new { success = false, message = "Manufacturer id is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _basicDataDAL.SaveUpdatePaymentMethodDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.TagsList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> TagsList(TagEntity FormData)
        {
            // ✅ Main Model
            BasicDataModel model = new BasicDataModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Tags List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.TagsList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

                #region ViewSelf Right Check
                bool SelfRight = await _sessionManag.GetViewSelfRightForLoginUserFromSession();
                if (SelfRight)
                {
                    int? LoginUserId = await _sessionManag.GetLoginUserIdFromSession();
                    FormData.CreatedBy = LoginUserId;
                }
                else
                {
                    FormData.CreatedBy = 0;
                }
                #endregion

                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.TagsList = await _basicDataDAL.GetTagsListDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.TagsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.TagsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion


                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.TagsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.TagsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/BasicData/PartialViews/_TagsList.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.TagsList, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateTag(TagEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.TagName))
                {
                    return Json(new { success = false, message = "Please fill the tag name field!" });
                }


                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.TagId < 1)
                    {
                        return Json(new { success = false, message = "Tag Id is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _basicDataDAL.SaveUpdateTagDAL(FormData, DataOperationType);
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

    }

}
