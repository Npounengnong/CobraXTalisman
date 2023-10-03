using ClosedXML.Excel;
using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.CommonModels.ProductsCatalogModule;
using Entities.DBInheritedModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Helpers.ConversionHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using static Entities.DBInheritedModels.InheritedEntitiesLevelTwo;

namespace AdminPanel.Controllers
{
    public class ProductsCatalogController : BaseController
    {
        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IProductServicesDAL _productServicesDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IUserManagementServicesDAL _userManagementServicesDAL;
        private readonly IFilesHelpers _filesHelpers;
        private readonly IDiscountsServicesDAL _discountsServicesDAL;

        public ProductsCatalogController(IBasicDataServicesDAL basicDataDAL, IProductServicesDAL productServicesDAL, IConstants constants, ICommonServicesDAL commonServicesDAL,
            ISessionManager sessionManag, IUserManagementServicesDAL userManagementServicesDAL, IFilesHelpers filesHelpers, IDiscountsServicesDAL discountsServicesDAL)
        {
            this._basicDataDAL = basicDataDAL;
            this._productServicesDAL = productServicesDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._userManagementServicesDAL = userManagementServicesDAL;
            this._filesHelpers = filesHelpers;
            this._discountsServicesDAL = discountsServicesDAL;
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductsList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ProductsList(ProductEntity FormData)
        {
            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Products List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ProductsList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

                CategoryEntity categoryEntity = new CategoryEntity()
                {
                    PageNo = 1,
                    PageSize = 5000
                };
                model.CategoryList = await this._basicDataDAL.GetCategoriesListDAL(categoryEntity);


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
                model.ProductsList = await _productServicesDAL.GetProductList(FormData);



                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ProductsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ProductsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.ProductsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.ProductsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/ProductsCatalog/PartialViews/_ProductsList.cshtml", model);
            }

            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewProduct, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CreateNewProduct()
        {
            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion



            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Create Product";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CreateNewProduct;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            #region dropdown data
            ManufacturerEntity manufacturer = new ManufacturerEntity()
            {
                PageNo = 1,
                PageSize = 2000
            };
            model.ManufacturerList = await _basicDataDAL.GetManufacturerListDAL(manufacturer);

            UserEntity vendorEntity = new UserEntity()
            {
                PageNo = 1,
                PageSize = 1000,
                UserTypeId = (short)UserTypesEnum.Vendor
            };

            // -- only get login vendor list if user login is of vendor type
            var LoginUser = _sessionManag.GetLoginUserFromSession();
            if (LoginUser?.UserTypeId == (short)UserTypesEnum.Vendor)
            {
                vendorEntity.UserId = LoginUser.UserId;
            }

            model.UsersList = await this._userManagementServicesDAL.GetUsersListDAL(vendorEntity);

            CategoryEntity categoryEntity = new CategoryEntity()
            {
                PageNo = 1,
                PageSize = 5000
            };
            model.CategoryList = await this._basicDataDAL.GetCategoriesListDAL(categoryEntity);

            ShippingMethodEntity shippingMethodEntity = new ShippingMethodEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.ShippingMethodsList = await this._productServicesDAL.GetShippingMethodsListDAL(shippingMethodEntity);

            InventoryMethodEntity inventoryMethodEntity = new InventoryMethodEntity()
            {
                PageNo = 1,
                PageSize = 200
            };
            model.InventoryMethodsList = await this._productServicesDAL.GetInventoryMethodsListDAL(inventoryMethodEntity);

            WarehouseEntity warehouseEntity = new WarehouseEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.WarehousesList = await this._productServicesDAL.GetWarehousesListDAL(warehouseEntity);

            ProductAttributeEntity productAttributeEntity = new ProductAttributeEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.ProductAttributesList = await this._productServicesDAL.GetProductAttributesListDAL(productAttributeEntity);


            DiscountEntity DiscountFormData = new DiscountEntity()
            {
                PageNo = 1,
                PageSize = 10000,
                DiscountTypeId = (short)DiscountTypesEnum.AppliedOnProducts,
                IsActiveSelected = (short)IsActiveTypeEnum.Active
            };
            model.DiscountsList = await this._discountsServicesDAL.GetDiscountsListDAL(DiscountFormData);


            #endregion



            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewProduct, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> CreateNewProductPost(ProductEntity FormData)
        {

            try
            {
                // ✅ Main Model
                ProductsCatalogModel model = new ProductsCatalogModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.ProductName) ? "Form is valid" : "Product name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.FullDescription) ? "Form is valid" : "Full Description is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.Price > 0 ? "Form is valid" : "Product price is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.VendorId > 0 ? "Form is valid" : "Vendor id is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.ProductImages != null && FormData.ProductImages.Length > 0 ? "Form is valid" : "Product image is required!";
                validationList.Add(ValidationMsg);

                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion


                #region image file conversion secion
                List<ImageFileInfo> ImageFileInfosList = new List<ImageFileInfo>();
                if (FormData != null && FormData.ProductImages != null && FormData.ProductImages.Length > 0)
                {
                    foreach (IFormFile photo in FormData.ProductImages)
                    {
                        string ProductsImagesDirectory = _constants.GetAppSettingKeyValue("AppSetting", "ProductsImagesDirectory");
                        string url = await _filesHelpers.SaveFileToDirectory(photo, ProductsImagesDirectory);

                        ImageFileInfosList.Add(new ImageFileInfo { ImageName = photo.FileName, ImageGuidName = "", ImageURL = url });
                        FormData.ProductImagesJson = JsonConvert.SerializeObject(ImageFileInfosList);
                    }
                }
                #endregion



                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _productServicesDAL.CreateNewProductDAL(FormData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UpdateProduct, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> UpdateProduct(int ProductId)
        {
            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion



            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Update Product";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.UpdateProduct;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            //--Get product details by id
            model.ProductObj = await this._productServicesDAL.GetProductDetailsById(ProductId);

            #region dropdown data
            ManufacturerEntity manufacturer = new ManufacturerEntity()
            {
                PageNo = 1,
                PageSize = 2000
            };
            model.ManufacturerList = await _basicDataDAL.GetManufacturerListDAL(manufacturer);


            UserEntity vendorEntity = new UserEntity()
            {
                PageNo = 1,
                PageSize = 1000,
                UserTypeId = (short)UserTypesEnum.Vendor
            };

            // -- only get login vendor list if user login is of vendor type
            var LoginUser = _sessionManag.GetLoginUserFromSession();
            if (LoginUser?.UserTypeId == (short)UserTypesEnum.Vendor)
            {
                vendorEntity.UserId = LoginUser.UserId;
            }

            model.UsersList = await this._userManagementServicesDAL.GetUsersListDAL(vendorEntity);



            CategoryEntity categoryEntity = new CategoryEntity()
            {
                PageNo = 1,
                PageSize = 5000
            };
            model.CategoryList = await this._basicDataDAL.GetCategoriesListDAL(categoryEntity);

            ShippingMethodEntity shippingMethodEntity = new ShippingMethodEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.ShippingMethodsList = await this._productServicesDAL.GetShippingMethodsListDAL(shippingMethodEntity);

            InventoryMethodEntity inventoryMethodEntity = new InventoryMethodEntity()
            {
                PageNo = 1,
                PageSize = 200
            };
            model.InventoryMethodsList = await this._productServicesDAL.GetInventoryMethodsListDAL(inventoryMethodEntity);

            WarehouseEntity warehouseEntity = new WarehouseEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.WarehousesList = await this._productServicesDAL.GetWarehousesListDAL(warehouseEntity);

            ProductAttributeEntity productAttributeEntity = new ProductAttributeEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.ProductAttributesList = await this._productServicesDAL.GetProductAttributesListDAL(productAttributeEntity);

            DiscountEntity DiscountFormData = new DiscountEntity()
            {
                PageNo = 1,
                PageSize = 10000,
                DiscountTypeId = (short)DiscountTypesEnum.AppliedOnProducts,
                IsActiveSelected = (short)IsActiveTypeEnum.Active
            };
            model.DiscountsList = await this._discountsServicesDAL.GetDiscountsListDAL(DiscountFormData);



            #endregion


            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UpdateProduct, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateProductPost(ProductEntity FormData)
        {

            try
            {
                // ✅ Main Model
                ProductsCatalogModel model = new ProductsCatalogModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.ProductName) ? "Form is valid" : "Product name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.FullDescription) ? "Form is valid" : "Full Description is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.Price > 0 ? "Form is valid" : "Product price is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.VendorId > 0 ? "Form is valid" : "Vendor id is required!";
                validationList.Add(ValidationMsg);
                // ValidationMsg = FormData != null && FormData.ProductImages != null && FormData.ProductImages.Length > 0 ? "Form is valid" : "Product image is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.ProductId > 0 ? "Form is valid" : "Product id is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion


                #region image file conversion secion
                List<ImageFileInfo> ImageFileInfosList = new List<ImageFileInfo>();
                if (FormData != null && FormData.ProductImages != null && FormData.ProductImages.Length > 0)
                {
                    foreach (IFormFile photo in FormData.ProductImages)
                    {

                        string ProductsImagesDirectory = _constants.GetAppSettingKeyValue("AppSetting", "ProductsImagesDirectory");
                        string url = await _filesHelpers.SaveFileToDirectory(photo, ProductsImagesDirectory);


                        ImageFileInfosList.Add(new ImageFileInfo { ImageName = photo.FileName, ImageGuidName = "", ImageURL = url });
                        FormData.ProductImagesJson = JsonConvert.SerializeObject(ImageFileInfosList);
                    }
                }
                #endregion

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _productServicesDAL.UpdateProductDAL(FormData);
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
        public async Task<IActionResult> GetTagsListByKeyword(string term)
        {


            try
            {
                TagEntity tagEntity = new TagEntity()
                {
                    TagName = term,
                    PageNo = 1,
                    PageSize = 30
                };
                var TagsList = await _productServicesDAL.GetProductTagsListForCreateProductPageDAL(tagEntity);

                var results = from Tag in TagsList
                              select new
                              {
                                  id = Tag.TagId,
                                  text = Tag.TagName
                              };

                return Json(new { results = results });

            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetProductAttributeValuesByAttributeID(string ProductAttributeId)
        {

            try
            {

                var result = await _productServicesDAL.GetProductAttributeValuesByAttributeID(ProductAttributeId);
                return Json(new { success = true, message = "Saved Successfully!", result = result });

            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }


        // ✅ Delete product images
        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductsList, 0, 0, (short)UserRightsEnum.Delete, 0, 0)]
        public async Task<IActionResult> DeleteProductAttachment(string primarykeyValue, string primaryKeyColumn, string tableName, string AttachmentURL, int SqlDeleteType = (short)SqlDeleteTypes.PlainTableDelete)
        {

            try
            {

                bool result = await _commonServicesDAL.DeleteAnyRecordDAL(primarykeyValue, primaryKeyColumn, tableName, SqlDeleteType);
                if (result)
                {

                    //--Delete file from directroy
                    string outPut = await _filesHelpers.DeleteAnyFileFromDirectory(AttachmentURL);
                    if (outPut == "Deleted Successfully")
                    {
                        return Json(new { success = true, message = "Deleted Successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "File record deleted from database but the file not deleted from actual direcoty" });
                    }

                }
                else
                {
                    return Json(new { success = false, message = "An error occured on server side." });
                }
            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }


        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductsReviews, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ProductsReviews(ProductEntity FormData)
        {
            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Products Reviews";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ProductsReviews;
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
                model.ProductsList = await _productServicesDAL.GetProductsReviewsDAL(FormData);



                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ProductsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ProductsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.ProductsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.ProductsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/ProductsCatalog/PartialViews/_ProductsReviews.cshtml", model);
            }

            return View(model);
        }


        [HttpGet]
        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductReviewDetail, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ProductReviewsDetail(ProductReviewEntity FormData)
        {
            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Products Reviews Detail";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ProductReviewDetail;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

                model.ProductReviewObj = new ProductReviewEntity();
                model.ProductReviewObj.ProductId = FormData.ProductId;

                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.ProductReviewList = await _productServicesDAL.GetProductReviewsByProductIdDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ProductReviewList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ProductReviewList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
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
                return PartialView("~/Views/ProductsCatalog/PartialViews/_ProductReviewsDetail.cshtml", model);
            }

            return View(model);

        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductsBulkUpload, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ProductsBulkUpload(ColorEntity FormData)
        {
            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Products Bulk Upload";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ProductsBulkUpload;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

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


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductsBulkUpload, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> UploadProductsBulkExcelFile(IFormFile? ProductsAttachmentFile)
        {

            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();
            model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();

            //Help Link: https://www.ittutorialswithexample.com/2019/06/export-and-import-excel-closedxml-aspnet-mvc.html


            string filePath = String.Empty;
            List<string> validationList = new List<string>();

            try
            {
                if (ProductsAttachmentFile == null)
                {
                    validationList.Add("Please select products excel file!");

                    model.SuccessErrorMsgEntityObj.validationList = new List<string>();
                    model.SuccessErrorMsgEntityObj.validationList = validationList;

                    return PartialView("~/Views/ProductsCatalog/PartialViews/_BulkUploadValidation.cshtml", model);
                }

                string ext = System.IO.Path.GetExtension(ProductsAttachmentFile.FileName);
                if (String.IsNullOrEmpty(ext) || ext != ".xlsx")
                {
                    validationList.Add("Only excel file with format .xlsx is allowed!");

                    model.SuccessErrorMsgEntityObj.validationList = new List<string>();
                    model.SuccessErrorMsgEntityObj.validationList = validationList;

                    return PartialView("~/Views/ProductsCatalog/PartialViews/_BulkUploadValidation.cshtml", model);
                }


                #region image checking
                if (ProductsAttachmentFile != null)
                {
                    filePath = await _filesHelpers.SaveFileToDirectory(ProductsAttachmentFile, null);

                }
                #endregion


                #region convert excel to data table and type
                // Open the Excel file using ClosedXML.
                // Keep in mind the Excel file cannot be open when trying to read it

                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + filePath);
                DataTable? dt = new DataTable();

                //-- Convert excel first sheet to data table
                dt = await _filesHelpers.ConvertExcelFirstSheetToDataTable(fullPath, false);

                //-- Convert data table to list type
                List<ProductExcelUpload>? productList = new List<ProductExcelUpload>();
                if (dt is not null)
                {
                    productList = CommonConversionHelper.ConvertDataTableToListType<ProductExcelUpload>(dt);
                }

                //--Delete the file once all information processed successfully
                await _filesHelpers.DeleteAnyFileFromDirectory(filePath);

                #endregion

                #region main validation area

                if (productList == null || productList.Count == 0)
                {
                    validationList.Add("There is no product in the list");
                }

                if (productList?.Count > 0)
                {
                    //--Product name validation
                    if (productList.Any(i => String.IsNullOrEmpty(i.ProductName)))
                    {
                        validationList.Add("ProductName should not be null for any row");
                    }

                    //--Product short name validation
                    if (productList.Any(i => String.IsNullOrEmpty(i.ShortDescription)))
                    {
                        validationList.Add("ShortDescription should not be null for any row");
                    }

                    //--Product price
                    if (productList.Any(i => String.IsNullOrEmpty(i.Price)))
                    {
                        validationList.Add("Price should not be null for any row");
                    }

                    //-- Categories validation for all rows
                    if (productList.Any(i => String.IsNullOrEmpty(i.CategoriesIdsCommaSeperated)))
                    {
                        validationList.Add("Please select at lease one category for each row");
                    }

                    //-- Categories validation for all rows
                    if (productList.Any(i => String.IsNullOrEmpty(i.TagsIdsCommaSeperated)))
                    {
                        validationList.Add("Please select at lease one tag for each row");
                    }

                    //-- Images validation for all rows
                    if (productList.Any(i => String.IsNullOrEmpty(i.ImagesIdsCommaSeperated)))
                    {
                        validationList.Add("Select Images/Attachment ids then proceed");
                    }

                    model.SuccessErrorMsgEntityObj.validationList = new List<string>();
                    model.SuccessErrorMsgEntityObj.validationList = validationList;
                }

                if (model?.SuccessErrorMsgEntityObj?.validationList != null && model.SuccessErrorMsgEntityObj.validationList.Any())
                {

                    return PartialView("~/Views/ProductsCatalog/PartialViews/_BulkUploadValidation.cshtml", model);

                }

                #endregion



                #region save final products
                int? LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();


                if (productList?.Count > 0)
                {
                    foreach (var item in productList)
                    {
                        try
                        {
                            item.LoginUserId = LoginUserId != null ? LoginUserId.ToString() : null;

                            string rslt = await _productServicesDAL.UploadBulkProductsFromExcelDAL(item);

                            if (rslt != "Saved Successfully!")
                            {
                                validationList.Add($"Procut with name: {item.ProductName} not saved successfully!");
                            }
                        }
                        catch (Exception ex)
                        {
                            validationList.Add($"Procut with name: {item.ProductName} not saved successfully due to {ex.Message}");
                            continue;
                        }
                    }
                }


                #endregion




            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                //--Delete the file once all information processed successfully
                await _filesHelpers.DeleteAnyFileFromDirectory(filePath);

                validationList.Add($"An error occured. Error Detail: {ex.Message}");


            }

            return PartialView("~/Views/ProductsCatalog/PartialViews/_BulkUploadValidation.cshtml", model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ImagesUpload, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ImagesUpload(AttachmentEntity FormData)
        {
            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Images Upload";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ImagesUpload;
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
                model.AttachmentsList = await _productServicesDAL.GetAttachmentsListForImageUploadPageDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.AttachmentsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.AttachmentsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.AttachmentsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.AttachmentsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/ProductsCatalog/PartialViews/_ImagesUpload.cshtml", model);
            }

            return View(model);
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ImagesUpload, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> SaveAttachment(AttachmentEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.AttachmentName))
                {
                    return Json(new { success = false, message = "Please fill the image name field!" });
                }

                if (FormData.AttachmentFile == null)
                {
                    return Json(new { success = false, message = "Please fill the image file" });
                }

                string ext = System.IO.Path.GetExtension(FormData.AttachmentFile.FileName);
                List<string> imagesFormat = new List<string>
                {
                    ".jpg" ,  ".jpeg" , ".png"
                };
                if (String.IsNullOrEmpty(ext))
                {
                    return Json(new { success = false, message = "Only images are allowed for uploading!. Supported formates are .jpg, .jpeg, .png" });

                }

                if (!imagesFormat.Any(x => x == ext))
                {
                    return Json(new { success = false, message = "Only images are allowed for uploading!. Supported formates are .jpg, .jpeg, .png" });

                }


                string url = await _filesHelpers.SaveFileToDirectory(FormData.AttachmentFile, null);
                if (!String.IsNullOrWhiteSpace(url))
                {

                    FormData.AttachmentUrl = url;
                    FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();
                    FormData.IsCommonImageUpload = true;
                    int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(FormData);

                    if (AttachmentId > 0)
                    {
                        return Json(new { success = true, message = "Saved Successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "An error occured on server side." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "An error occured on server side." });
                }




            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductVariants, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ProductVariants(ProductAttributeEntity FormData)
        {
            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Product Variants";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ProductVariants;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.ProductAttributesList = await _productServicesDAL.GetProductVariantsDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ProductAttributesList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ProductAttributesList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.ProductAttributesList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.ProductAttributesList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/ProductsCatalog/PartialViews/_ProductVariants.cshtml", model);
            }

            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductVariantDetail, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> ProductVariantDetail(ProductVariantDetail FormData)
        {
            // ✅ Main Model
            ProductsCatalogModel model = new ProductsCatalogModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Product Variant Detail";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.ProductVariantDetail;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {


              



                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.ProductVariantDetailObj = new ProductVariantDetail();
                model.ProductVariantDetailObj = FormData;

                #region attribute main object
                ProductAttributeEntity productAttributeEntity = new ProductAttributeEntity()
                {
                    PageNo = 1,
                    PageSize = 2,
                    ProductAttributeId = FormData.ProductAttributeId
                };
                var ProductAttributesList = await _productServicesDAL.GetProductVariantsDAL(productAttributeEntity);
                model.ProductVariantDetailObj.AttributeDisplayName = ProductAttributesList?.FirstOrDefault()?.DisplayName;

                #endregion

                model.ProductVariantDetailList = await _productServicesDAL.GetProductVariantsDetailByIdDAL(FormData.ProductAttributeId);

                if (!String.IsNullOrEmpty(FormData.DisplayText))
                {
                    model.ProductVariantDetailList = model?.ProductVariantDetailList?.Where(x => x.DisplayText == FormData.DisplayText).ToList();
                }

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.ProductVariantDetailList?.Count() ?? 0;
                model.ProductVariantDetailList = model?.ProductVariantDetailList?.Skip((FormData.PageNo - 1) * FormData.PageSize).Take(FormData.PageSize).ToList();

                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.ProductVariantDetailList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion

            

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.ProductVariantDetailList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.ProductVariantDetailList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/ProductsCatalog/PartialViews/_ProductVariantDetail.cshtml", model);
            }

            return View(model);
        }



        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.ProductVariantDetail, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateProductVariant(ProductVariantDetail FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.DisplayText))
                {
                    return Json(new { success = false, message = "Variant value field is required!" });
                }

                if (FormData.ProductAttributeId  < 1)
                {
                    return Json(new { success = false, message = "Attribute id is required!" });
                }
              
                if (DataOperationType == 2)
                {
                    if (FormData.PrimaryKeyValue < 1)
                    {
                        return Json(new { success = false, message = "The edit row primary key is null!" });
                    }

                }

                FormData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _productServicesDAL.SaveUpdateProductVariantDAL(FormData, DataOperationType);
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
