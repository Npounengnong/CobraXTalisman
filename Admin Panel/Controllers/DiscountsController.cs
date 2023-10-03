using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.DBInheritedModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Microsoft.AspNetCore.Mvc;
using static Entities.DBInheritedModels.InheritedEntitiesLevelTwo;

namespace AdminPanel.Controllers
{
    public class DiscountsController : BaseController
    {


        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IUserManagementServicesDAL _userManagementServicesDAL;
        private readonly IDiscountsServicesDAL _discountsServicesDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IFilesHelpers _filesHelpers;

        public DiscountsController(IBasicDataServicesDAL basicDataDAL, IUserManagementServicesDAL userManagementServicesDAL, IConstants constants,
            ICommonServicesDAL commonServicesDAL, ISessionManager sessionManag, IDiscountsServicesDAL discountsServicesDAL, IFilesHelpers filesHelpers)
        {
            this._basicDataDAL = basicDataDAL;
            this._userManagementServicesDAL = userManagementServicesDAL;
            this._discountsServicesDAL = discountsServicesDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._filesHelpers = filesHelpers;
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.DiscountsList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> DiscountsList(DiscountEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Discount List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.DiscountsList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

                //--Get user discount type list
                DiscountTypeEntity discountTypeEntity = new DiscountTypeEntity()
                {
                    PageNo = 1,
                    PageSize = 100
                };
                model.DiscountTypeList = await this._discountsServicesDAL.GetDiscountTypesListDAL(discountTypeEntity);
            


                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.DiscountsList = await _discountsServicesDAL.GetDiscountsListDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.DiscountsList?.FirstOrDefault()?.TotalRecords ?? 0;

                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.DiscountsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);

                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.DiscountsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.DiscountsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Discounts/PartialViews/_DiscountsList.cshtml", model);
            }

            return View(model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewDiscount, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CreateNewDiscount()
        {
            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion



            // ✅ Main Model
            DiscountModel model = new DiscountModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Create Discount";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CreateNewDiscount;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            //--Get  discount type list
            DiscountTypeEntity discountTypeEntity = new DiscountTypeEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.DiscountTypeList = await this._discountsServicesDAL.GetDiscountTypesListDAL(discountTypeEntity);



            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDiscountProductsPartialPage(DiscountProductsMappingEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();



            try
            {

            
                FormData.PageSize = 2000;
                model.DiscountProductsMappingList = await _discountsServicesDAL.GetDiscountProductsMappingListDAL(FormData);

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.DiscountProductsMappingList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.DiscountProductsMappingList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
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

            return PartialView("~/Views/Discounts/PartialViews/DiscountProducts/_DiscountProductsPartialPage.cshtml", model);
        }


        [HttpGet]
        public async Task<IActionResult> ShowAllProductsForDiscount(ProductEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();



            try
            {

                CategoryEntity categoryEntity = new CategoryEntity()
                {
                    PageNo = 1,
                    PageSize = 5000
                };
                model.CategoryList = await this._basicDataDAL.GetCategoriesListDAL(categoryEntity);


                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.ProductsList = await _discountsServicesDAL.GetProductsListForDiscountDAL(FormData);

              
                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ProductsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ProductsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
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

            if (FormData.IsDiscountCreatePageSearchEnabled)
            {
                return PartialView("~/Views/Discounts/PartialViews/DiscountProducts/_DiscountProductsList.cshtml", model);
            }

            return PartialView("~/Views/Discounts/PartialViews/DiscountProducts/_DiscountProductsModal.cshtml", model);
        }


        [HttpGet]
        public async Task<IActionResult> GetDiscountCategoriesPartialPage(DiscountCategoriesMappingEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();



            try
            {


                FormData.PageSize = 2000;
                model.DiscountCategoriesMappingList = await _discountsServicesDAL.GetDiscountCategoriesMappingListDAL(FormData);


                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model != null && model.DiscountCategoriesMappingList != null && model.DiscountCategoriesMappingList.Count() > 0
                                && model.DiscountCategoriesMappingList.FirstOrDefault() != null ? model.DiscountCategoriesMappingList.FirstOrDefault().TotalRecords : 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model.DiscountCategoriesMappingList != null && model.DiscountCategoriesMappingList != null ? model.DiscountCategoriesMappingList.Count() : 0, TotalRecords, FormData.PageSize, FormData.PageNo);


            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion

            }

            return PartialView("~/Views/Discounts/PartialViews/DiscountCategories/_DiscountCategoriesPartialPage.cshtml", model);
        }


        [HttpGet]
        public async Task<IActionResult> ShowAllCategoriesForDiscount(CategoryEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();



            try
            {

                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.CategoryList = await _discountsServicesDAL.GetCategoriesListForDiscountDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.CategoryList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.CategoryList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
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

            if (FormData.IsDiscountCreatePageSearchEnabled)
            {
                return PartialView("~/Views/Discounts/PartialViews/DiscountCategories/_DiscountCategoriesList.cshtml", model);
            }

            return PartialView("~/Views/Discounts/PartialViews/DiscountCategories/_DiscountCategoriesModal.cshtml", model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewDiscount, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> InsertUpdateDiscount(DiscountEntity FormData)
        {

            try
            {
                // ✅ Main Model
                DiscountModel model = new DiscountModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.Title) ? "Form is valid" : "Title is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.DiscountTypeId > 0 ? "Form is valid" : "Discount type is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.DiscountValueType > 0 ? "Form is valid" : "Discount value type is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.DiscountValue > 0 ? "Form is valid" : "Discount value is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData == null ? "Select end date!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData?.EndDate < DateTime.Now ? "End should be greater than today date!" : "Form is valid";
                validationList.Add(ValidationMsg);

                if (FormData != null && FormData.IsCouponCodeRequired != null && FormData.IsCouponCodeRequired == true)
                {
                    if (String.IsNullOrWhiteSpace(FormData.CouponCode))
                    {
                        ValidationMsg = "Coupon code required!";
                        validationList.Add(ValidationMsg);
                    }
                }

                if (FormData != null && FormData.IsBoundToMaxQuantity)
                {
                    if (FormData.MaxQuantity==null || FormData.MaxQuantity<1)
                    {
                        ValidationMsg = "Max quantity required!";
                        validationList.Add(ValidationMsg);
                    }
                }

                


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = ValidationMsg });
                }

                #endregion



                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _discountsServicesDAL.InsertUpdateDiscountDAL(FormData);
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


        //--Update product
        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UpdateDiscount, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> UpdateDiscount(int DiscountId)
        {
            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion



            // ✅ Main Model
            DiscountModel model = new DiscountModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Update Discount";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.UpdateDiscount;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            //--Get  discount type list
            DiscountTypeEntity discountTypeEntity = new DiscountTypeEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.DiscountTypeList = await this._discountsServicesDAL.GetDiscountTypesListDAL(discountTypeEntity);



            //--Get discount details by id
            model.DiscountObj = await this._discountsServicesDAL.GetDiscountDetailsById(DiscountId);

       

            return View(model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ContactUsList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ContactUsList(ContactUsEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Contact Us List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ContactUsList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.ContactUsList = await _discountsServicesDAL.GetContactUsListDAL(FormData);

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model != null && model.ContactUsList != null && model.ContactUsList.Count() > 0
                                && model.ContactUsList.FirstOrDefault() != null ? model.ContactUsList.FirstOrDefault().TotalRecords : 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model.ContactUsList != null && model.ContactUsList != null ? model.ContactUsList.Count() : 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);

                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.ContactUsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.ContactUsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Discounts/PartialViews/_ContactUsList.cshtml", model);
            }

            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.SubscribersList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> SubcribersList(SubscriberEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Subcribers List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.SubscribersList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.SubscribersList = await _discountsServicesDAL.GetSubscribersListDAL(FormData);

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model != null && model.SubscribersList != null && model.SubscribersList.Count() > 0
                                && model.SubscribersList.FirstOrDefault() != null ? model.SubscribersList.FirstOrDefault().TotalRecords : 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model.SubscribersList != null && model.SubscribersList != null ? model.SubscribersList.Count() : 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);

                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.SubscribersList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.SubscribersList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Discounts/PartialViews/_SubcribersList.cshtml", model);
            }

            return View(model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.SiteHomeBannersList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> SiteHomeScreenBanners(HomeScreenBannerEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();
       
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Home Screen Banners";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.SiteHomeBannersList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.HomeScreenBannersList = await _discountsServicesDAL.GetHomeScreenBannersListDAL(FormData);

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model != null && model.HomeScreenBannersList != null && model.HomeScreenBannersList.Count() > 0
                                && model.HomeScreenBannersList.FirstOrDefault() != null ? model.HomeScreenBannersList.FirstOrDefault().TotalRecords : 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model.HomeScreenBannersList != null && model.HomeScreenBannersList != null ? model.HomeScreenBannersList.Count() : 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);

                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.HomeScreenBannersList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.HomeScreenBannersList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Discounts/PartialViews/_SiteHomeScreenBanners.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SaveUpdateHomeScreenBanner(HomeScreenBannerEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.MainTitle))
                {
                    return Json(new { success = false, message = "Please fill the main title field!" });
                }

                if (String.IsNullOrWhiteSpace(FormData.TopTitle))
                {
                    return Json(new { success = false, message = "Please fill the top title field" });
                }



                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

              

                if (DataOperationType== 1 && FormData.BannerImgUrlFile == null)//--Insert case
                {
                    return Json(new { success = false, message = "Please select banner image!" });
                }


                #region image checking


                if (FormData.BannerImgUrlFile != null)
                {
                    string url = await _filesHelpers.SaveFileToDirectory(FormData.BannerImgUrlFile, null);
                    if (!String.IsNullOrWhiteSpace(url))
                    {
                        AttachmentEntity atch = new AttachmentEntity();
                        atch.AttachmentUrl = url;
                        atch.AttachmentName = FormData.BannerImgUrlFile.FileName;
                        atch.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                        if (FormData.AttachmentId != null && FormData.AttachmentId > 0) //--update case
                        {
                            atch.AttachmentId = Convert.ToInt32(FormData.AttachmentId);


                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);
                            FormData.AttachmentId = AttachmentId;
                        }
                        else
                        {
                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);

                            atch.AttachmentId = AttachmentId;

                            FormData.AttachmentId = AttachmentId;
                        }

                    }

                }

                #endregion

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _discountsServicesDAL.SaveUpdateHomeScreenBannerDAL(FormData, DataOperationType);
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
        public async Task<IActionResult> GetHomeScreenBannerDetailById(int BannerId)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();

           

            try
            {

                if (BannerId < 1)
                {
                    return Json(model.HomeScreenBannersList?.FirstOrDefault());
                }

                HomeScreenBannerEntity FormData = new HomeScreenBannerEntity();
                FormData.BannerId = BannerId;
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.HomeScreenBannersList = await _discountsServicesDAL.GetHomeScreenBannersListDAL(FormData);

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model != null && model.HomeScreenBannersList != null && model.HomeScreenBannersList.Count() > 0
                                && model.HomeScreenBannersList.FirstOrDefault() != null ? model.HomeScreenBannersList.FirstOrDefault().TotalRecords : 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model.HomeScreenBannersList != null && model.HomeScreenBannersList != null ? model.HomeScreenBannersList.Count() : 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);

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


            return Json(model.HomeScreenBannersList?.FirstOrDefault());
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.DiscountCampaigns, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> DiscountCampaigns(DiscountsCampaignEntity FormData)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();
    
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Campaigns";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.DiscountCampaigns;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.DiscountsCampaignList = await _discountsServicesDAL.GetDiscountsCampaignDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.DiscountsCampaignList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.DiscountsCampaignList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                //if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.SizeList?.Count > 0)
                //{
                //    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.SizeList.Cast<dynamic?>().ToList());
                //    return ExcelFileResutl;
                //}

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
                return PartialView("~/Views/Discounts/PartialViews/_DiscountCampaigns.cshtml", model);
            }

            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.DiscountCampaigns, 0, 0, 0, (short)UserRightsEnum.View_All, 0)]
        public async Task<IActionResult> GetCampaignDetailById(int CampaignId)
        {
            // ✅ Main Model
            DiscountModel model = new DiscountModel();
           

            try
            {
               
                model.DiscountsCampaignObj = await _discountsServicesDAL.GetCampaignDetailByIdDAL(CampaignId);

            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion

            }

            return PartialView("~/Views/Discounts/PartialViews/_CampaignDetail.cshtml", model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.DiscountCampaigns, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateDiscountCampaigns(DiscountsCampaignEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.MainTitle))
                {
                    return Json(new { success = false, message = "Main title is required!" });
                }
                if (String.IsNullOrWhiteSpace(FormData.DiscountTitle))
                {
                    return Json(new { success = false, message = "Discount title is required!" });
                }
                if (String.IsNullOrWhiteSpace(FormData.Body))
                {
                    return Json(new { success = false, message = "Body is required!" });
                }

              

                if (DataOperationType == 2)
                {
                    if (FormData.CampaignId < 1)
                    {
                        return Json(new { success = false, message = "Campaign Id is null!" });
                    }

                }


                if (DataOperationType == 1)
                {
                    if (FormData.CoverPictureFile == null) //-- During update time, check if profile photo is null
                    {
                        return Json(new { success = false, message = "Cover image is required!" });
                    }

                }


                #region image checking

                if (FormData.CoverPictureFile != null)
                {
                    string url = await _filesHelpers.SaveFileToDirectory(FormData.CoverPictureFile, null);
                    if (!String.IsNullOrWhiteSpace(url))
                    {
                        AttachmentEntity atch = new AttachmentEntity();
                        atch.AttachmentUrl = url;
                        atch.AttachmentName = FormData.CoverPictureFile.FileName;
                        atch.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                        if (FormData.CoverPictureId != null && FormData.CoverPictureId > 0) //--update case
                        {
                            atch.AttachmentId = Convert.ToInt32(FormData.CoverPictureId);


                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);
                        }
                        else
                        {
                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);

                            atch.AttachmentId = AttachmentId;

                            FormData.CoverPictureId = AttachmentId;
                        }

                    }

                }

                #endregion

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _discountsServicesDAL.SaveUpdateDiscountsCampaignDAL(FormData, DataOperationType);
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
