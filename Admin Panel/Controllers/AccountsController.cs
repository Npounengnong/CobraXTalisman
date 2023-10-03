using DAL.Repository.IServices;
using DAL.Repository.Services;
using Entities.CommonModels;
using Entities.CommonModels.AccountsModule;
using Entities.DBInheritedModels;
using Entities.DBModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;


namespace AdminPanel.Controllers
{
    public class AccountsController : BaseController
    {

        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IFilesHelpers _filesHelpers;
        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IAccountsServicesDAL _accountsServicesDAL;
        private readonly IUserManagementServicesDAL _userManagementServicesDAL;

        public AccountsController(IBasicDataServicesDAL basicDataDAL, IProductServicesDAL productServicesDAL, IConstants constants, ICommonServicesDAL commonServicesDAL,
           ISessionManager sessionManag, IUserManagementServicesDAL userManagementServicesDAL, IFilesHelpers filesHelpers, IDiscountsServicesDAL discountsServicesDAL, IAccountsServicesDAL accountsServicesDAL,
           IUserManagementServicesDAL userManagementServicesDAL1)
        {
            this._basicDataDAL = basicDataDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._filesHelpers = filesHelpers;
            this._accountsServicesDAL = accountsServicesDAL;
            this._userManagementServicesDAL = userManagementServicesDAL;
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.Banks, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> Banks(BankMasterEntity FormData)
        {
            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Banks List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.Banks;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

                //--Get bank industry type list
                BankIndustryTypeEntity bankIndustryTypeEntity = new BankIndustryTypeEntity()
                {
                    PageNo = 1,
                    PageSize = 1000
                };
                model.BankIndustryTypeList = await this._accountsServicesDAL.GetBankIndustryTypeDAL(bankIndustryTypeEntity);
                model.BankIndustryTypeList = model?.BankIndustryTypeList?.Where(x => x.IsActive == true).ToList();

                //--Get bank statuses list
                BankStatusEntity bankStatusEntity = new BankStatusEntity()
                {
                    PageNo = 1,
                    PageSize = 1000
                };
                model.BankStatusList = await this._accountsServicesDAL.GetBankStatusesDAL(bankStatusEntity);
                model.BankStatusList = model?.BankStatusList?.Where(x => x.IsActive == true).ToList();

                //--Get countries list
                CountryEntity countryEntity = new CountryEntity()
                {
                    PageNo = 1,
                    PageSize = 300
                };
                model.CountriesList = await this._userManagementServicesDAL.GetCountriesListDAL(countryEntity);
                model.CountriesList = model?.CountriesList?.Where(x => x.IsActive == true).ToList();

                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.BankMasterList = await _accountsServicesDAL.GetBankMasterDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.BankMasterList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.BankMasterList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.BankMasterList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.BankMasterList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Accounts/PartialViews/_Banks.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.Banks, (short)UserRightsEnum.Add, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveUpdateBank(BankMasterEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.BankName))
                {
                    return Json(new { success = false, message = "Please fill the bank name field!" });
                }


                if (DataOperationType == 2)
                {
                    if (FormData.BankMasterId < 1)
                    {
                        return Json(new { success = false, message = "Bank id is null!" });
                    }

                }

                FormData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _accountsServicesDAL.SaveUpdateBankMasterDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UsersBankAccounts, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> UsersBankAccounts(BankAccountEntity FormData)
        {
            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Users Bank Accounts";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.UsersBankAccounts;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {


                FormData.PageSize = this._constants.ITEMS_PER_PAGE();

                model.BankAccountList = await _accountsServicesDAL.GetUserBankAccountsDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.BankAccountList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.BankAccountList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion


                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.BankAccountList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.BankAccountList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Accounts/PartialViews/_UsersBankAccounts.cshtml", model);
            }

            return View(model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewUserBankAccount, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CreateUserBankAccount()
        {

            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion

            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Create User Bank Account";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CreateNewUserBankAccount;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            try
            {

                //--Get bank list
                BankMasterEntity bankMasterEntity = new BankMasterEntity()
                {
                    PageNo = 1,
                    PageSize = 1000
                };
                model.BankMasterList = await _accountsServicesDAL.GetBankMasterDAL(bankMasterEntity);

                //--Get bank account types
                BankAccountType bankAccountType = new BankAccountType();
                model.BankAccountTypesList = await this._accountsServicesDAL.GetBankAccountTypesDAL(bankAccountType);


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
        public async Task<IActionResult> GetUsersListByUserName(string UserName)

        {
            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.EntityId = 0;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            UserEntity userEntityObj = new UserEntity()
            {
                UserName = UserName,
                IsActive = true,
                PageNo = 1,
                PageSize = 10
            };
            model.UsersList = await _userManagementServicesDAL.GetUsersListByUserNameDAL(userEntityObj);


            if (model.UsersList != null && model.UsersList.Count() > 0)
            {
                return Json(model.UsersList);

            }
            else
            {
                return Json(new { message = "No data found" });
            }


        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewUserBankAccount, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> CreateUserBankAccount(BankAccountEntity FormData)
        {

            try
            {
                // ✅ Main Model
                AccountsModel model = new AccountsModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.UserId > 0 ? FormData.UserId.ToString() : "") ? "Form is valid" : "User id is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.AccountTypeId > 0 ? FormData.AccountTypeId.ToString() : "") ? "Form is valid" : "Account type is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.BankBranchName) ? "Form is valid" : "Branch name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.AccountNo) ? "Form is valid" : "Account no is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion

                if (FormData.StateProvinceId == -999)
                {
                    FormData.StateProvinceId = null;
                }

                if (FormData.CityId == -999)
                {
                    FormData.CityId = null;
                }



                #region image file conversion secion
                List<ImageFileInfo> ImageFileInfosList = new List<ImageFileInfo>();
                if (FormData != null && FormData.BankAttachementFiles != null && FormData.BankAttachementFiles.Length > 0)
                {
                    foreach (IFormFile photo in FormData.BankAttachementFiles)
                    {

                        string url = await _filesHelpers.SaveFileToDirectory(photo, null);
                        if (!String.IsNullOrWhiteSpace(url))
                        {
                            AttachmentEntity atch = new AttachmentEntity();
                            atch.AttachmentUrl = url;
                            atch.AttachmentName = photo.FileName;
                            atch.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);

                            ImageFileInfosList.Add(new ImageFileInfo { AttachmentId = AttachmentId, ImageName = photo.FileName, ImageGuidName = "", ImageURL = url });

                        }

                    }

                    if (ImageFileInfosList != null && ImageFileInfosList.Count > 0)
                    {
                        FormData.BankAttachementsJson = JsonConvert.SerializeObject(ImageFileInfosList);
                    }
                    else
                    {
                        FormData.BankAttachementsJson = String.Empty;
                    }
                }
                #endregion


                var LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();
                FormData.LoginUserId = LoginUserId ?? 0;
                FormData.DataOperationType = (short)DataOperationType.Insert;

                string result = await _accountsServicesDAL.CreateUpdateUserBankAccount(FormData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UpdateUserBankAccount, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> UpdateUserBankAccount(int BankAccountId)
        {

            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion

            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Update User Bank Account";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.UpdateUserBankAccount;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            try
            {


                //--Get bank list
                BankMasterEntity bankMasterEntity = new BankMasterEntity()
                {
                    PageNo = 1,
                    PageSize = 1000
                };
                model.BankMasterList = await _accountsServicesDAL.GetBankMasterDAL(bankMasterEntity);

                //--Get bank account attachments
                BankAccountAttachmentEntity bankAccountAttachmentEntity = new BankAccountAttachmentEntity()
                {
                    BankAccountId = BankAccountId,
                    PageNo = 1,
                    PageSize = 20
                };
                model.BankAccountAttachmentEntityList = await _accountsServicesDAL.GetBankAccountAttachmentListDAL(bankAccountAttachmentEntity);

                //--Get Bank Account data by id
                model.BankAccountEntityObj = await this._accountsServicesDAL.GetBankAccountDetailByIdDAL(BankAccountId);

                //--Get bank account types
                BankAccountType bankAccountType = new BankAccountType();
                model.BankAccountTypesList = await this._accountsServicesDAL.GetBankAccountTypesDAL(bankAccountType);


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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UpdateUserBankAccount, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateUserBankAccount(BankAccountEntity FormData)
        {

            try
            {
                // ✅ Main Model
                AccountsModel model = new AccountsModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.BankAccountId > 0 ? FormData.BankAccountId.ToString() : "") ? "Form is valid" : "Bank account id is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.UserId > 0 ? FormData.UserId.ToString() : "") ? "Form is valid" : "User id is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.AccountTypeId > 0 ? FormData.AccountTypeId.ToString() : "") ? "Form is valid" : "Account type is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.BankBranchName) ? "Form is valid" : "Branch name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.AccountNo) ? "Form is valid" : "Account no is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion

                if (FormData.StateProvinceId == -999)
                {
                    FormData.StateProvinceId = null;
                }

                if (FormData.CityId == -999)
                {
                    FormData.CityId = null;
                }



                #region image file conversion secion
                List<ImageFileInfo> ImageFileInfosList = new List<ImageFileInfo>();
                if (FormData != null && FormData.BankAttachementFiles != null && FormData.BankAttachementFiles.Length > 0)
                {
                    foreach (IFormFile photo in FormData.BankAttachementFiles)
                    {

                        string url = await _filesHelpers.SaveFileToDirectory(photo, null);
                        if (!String.IsNullOrWhiteSpace(url))
                        {
                            AttachmentEntity atch = new AttachmentEntity();
                            atch.AttachmentUrl = url;
                            atch.AttachmentName = photo.FileName;
                            atch.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);

                            ImageFileInfosList.Add(new ImageFileInfo { AttachmentId = AttachmentId, ImageName = photo.FileName, ImageGuidName = "", ImageURL = url });

                        }

                    }

                    if (ImageFileInfosList != null && ImageFileInfosList.Count > 0)
                    {
                        FormData.BankAttachementsJson = JsonConvert.SerializeObject(ImageFileInfosList);
                    }
                    else
                    {
                        FormData.BankAttachementsJson = String.Empty;
                    }
                }
                #endregion


                var LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();
                FormData.LoginUserId = LoginUserId ?? 0;
                FormData.DataOperationType = (short)DataOperationType.Update;

                string result = await _accountsServicesDAL.CreateUpdateUserBankAccount(FormData);
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


        // ✅ Delete product images
        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UsersBankAccounts, 0, 0, (short)UserRightsEnum.Delete, 0, 0)]
        public async Task<IActionResult> DeleteUserBankAccountAttachment(string primarykeyValue, string primaryKeyColumn, string tableName, string AttachmentURL, int SqlDeleteType = (short)SqlDeleteTypes.PlainTableDelete)
        {

            try
            {

                bool result = await _commonServicesDAL.DeleteAnyRecordDAL(primarykeyValue, primaryKeyColumn, tableName, SqlDeleteType);
                if (result)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + AttachmentURL);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }


                    return Json(new { success = true, message = "Deleted Successfully!" });
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.VendorsCommissionSetup, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> VendorsCommissionSetup(VendorsCommissionSetupEntity FormData)
        {
            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Vendors Commission Setup";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.VendorsCommissionSetup;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {



                FormData.PageSize = this._constants.ITEMS_PER_PAGE();

                model.VendorsCommissionSetupList = await _accountsServicesDAL.GetVendorsCommissionSetupDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.VendorsCommissionSetupList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.VendorsCommissionSetupList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion


                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.VendorsCommissionSetupList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.VendorsCommissionSetupList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Accounts/PartialViews/_VendorsCommissionSetup.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.VendorsCommissionSetup, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateVendorCommission(VendorsCommissionSetupEntity FormData, int DataOperationType = (short)DataOperationType.Update)
        {

            try
            {
                if (FormData.CommissionValue < 0 || FormData.CommissionValue > 99)
                {
                    return Json(new { success = false, message = "Commission value should be b/w 0-99" });
                }

                if (FormData.UserId < 1)
                {
                    return Json(new { success = false, message = "User id is null!" });
                }




                FormData.LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _accountsServicesDAL.UpdateVendorCommissionDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.VendorPayments, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> VendorPayments(VendorsPayments FormData)
        {
            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Vendors Payments";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.VendorPayments;
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
                model.VendorsPaymentsList = await _accountsServicesDAL.GetVendorsPaymentsListDAL(FormData);

                if (model?.VendorsPaymentsList != null && model.VendorsPaymentsList.Count > 0)
                {
                    string CommaSeperatedVendorsIds = string.Join(",", model.VendorsPaymentsList.Select(x => x.VendorId).ToArray());

                    //-- get vendors Total Credit, Total Debit, Total Received, Total balance
                    model.VendorsOrdersTotalReceivedBalanceList = await _accountsServicesDAL.GetVendorsOrdersTotalReceivedBalanceDAL(CommaSeperatedVendorsIds);

                    //-- merge vendors total orders amount in the vendors payments list
                    foreach (var item in model.VendorsPaymentsList)
                    {
                        
                        item.VendorTotalCredit = Math.Round((model?.VendorsOrdersTotalReceivedBalanceList?.FirstOrDefault(x => x.VendorId == item.VendorId)?.VendorTotalCredit ?? 0) ,2 );
                        item.VendorTotalDebit = Math.Round((model?.VendorsOrdersTotalReceivedBalanceList?.FirstOrDefault(x => x.VendorId == item.VendorId)?.VendorTotalDebit ?? 0), 2);
                        item.VendorOrdersTotal = Math.Round((model?.VendorsOrdersTotalReceivedBalanceList?.FirstOrDefault(x => x.VendorId == item.VendorId)?.VendorOrdersTotal ?? 0) ,2 );
                        item.TotalReceived = Math.Round((model?.VendorsOrdersTotalReceivedBalanceList?.FirstOrDefault(x => x.VendorId == item.VendorId)?.TotalReceived ?? 0) ,2 );
                        item.TotalBalance = Math.Round((model?.VendorsOrdersTotalReceivedBalanceList?.FirstOrDefault(x => x.VendorId == item.VendorId)?.TotalBalance ?? 0) ,2 );
                    }


                }




                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.VendorsPaymentsList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.VendorsPaymentsList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.VendorsPaymentsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.VendorsPaymentsList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Accounts/PartialViews/_VendorPayments.cshtml", model);
            }

            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.VendorAccountsTransaction, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> VendorAccountsTransaction(AccountTransactionsDetail FormData)
        {

            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion


            // ✅ Main Model
            AccountsModel model = new AccountsModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Account Transactions";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.VendorAccountsTransaction;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            ViewBag.VendorId = FormData.VendorId;
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

                //--Get payment methods list
                PaymentMethodEntity paymentMethodEntity = new PaymentMethodEntity()
                {
                    PageNo = 1,
                    PageSize = 50
                };
                model.PaymentMethodsList = await this._basicDataDAL.GetPaymentMethodsListDAL(paymentMethodEntity);
                model.PaymentMethodsList = model?.PaymentMethodsList?.Where(x => x.IsActive == true).ToList();

                //--Get bank transaction events list
                BankTransEventEntity bankTransEventEntity = new BankTransEventEntity()
                {
                    PageNo = 1,
                    PageSize = 50
                };
                model.BankTransEventsList = await this._accountsServicesDAL.GetBankTransEventListDAL(bankTransEventEntity);
                model.BankTransEventsList = model?.BankTransEventsList?.Where(x => x.IsActive == true).ToList();

                //--Get this vendor bank accounts
                BankAccountEntity bankAccountEntity = new BankAccountEntity()
                {
                    UserId = FormData.VendorId,
                    PageNo = 1,
                    PageSize = 15
                };
                model.BankAccountList = await _accountsServicesDAL.GetUserBankAccountsDAL(bankAccountEntity);
                model.BankAccountList = model?.BankAccountList?.Where(x => x.IsActive == true).ToList();

                //--Get vendor basic info
                model.UserObj = new UserEntity();
                model.UserObj =  _basicDataDAL.GetUserDataByUserID(FormData.VendorId);



                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.AccountTransactionsDetailList = await _accountsServicesDAL.GetAccountTransDetailByVendorIdDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.AccountTransactionsDetailList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.AccountTransactionsDetailList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.VendorsPaymentsList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.AccountTransactionsDetailList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Accounts/PartialViews/_VendorAccountsTransaction.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.VendorAccountsTransaction, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> CreateVendorAccountTransaction(AccountTransactionsDetail FormData)
        {

            try
            {
                // ✅ Main Model
                AccountsModel model = new AccountsModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.VendorId > 0 ? FormData.VendorId.ToString() : "") ? "Form is valid" : "Vendor id is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.TransTitle) ? "Form is valid" : "Trans title is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.TransType) ? "Form is valid" : "Transaction type is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.TransAmount > 0 ? FormData.TransAmount.ToString() : "") ? "Form is valid" : "Amount is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.BankAccountId > 0 ? FormData.BankAccountId.ToString() : "") ? "Form is valid" : "Bank account is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion




                #region image file conversion secion
                List<ImageFileInfo> ImageFileInfosList = new List<ImageFileInfo>();
                if (FormData != null && FormData.AccountTransAttachementFile != null && FormData.AccountTransAttachementFile.Length > 0)
                {
                    foreach (IFormFile photo in FormData.AccountTransAttachementFile)
                    {
                        string FileDirectory = "/content/commonImages/accountsAttachments";
                        string url = await _filesHelpers.SaveFileToDirectory(photo, FileDirectory);
                        if (!String.IsNullOrWhiteSpace(url))
                        {
                            AttachmentEntity atch = new AttachmentEntity();
                            atch.AttachmentUrl = url;
                            atch.AttachmentName = photo.FileName;
                            atch.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);

                            ImageFileInfosList.Add(new ImageFileInfo { AttachmentId = AttachmentId, ImageName = photo.FileName, ImageGuidName = "", ImageURL = url });

                        }

                    }

                    if (ImageFileInfosList != null && ImageFileInfosList.Count > 0)
                    {
                        FormData.AccountTransAttachementJson = JsonConvert.SerializeObject(ImageFileInfosList);
                    }
                    else
                    {
                        FormData.AccountTransAttachementJson = String.Empty;
                    }
                }
                #endregion


                var LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();
                FormData.LoginUserId = LoginUserId ?? 0;
                FormData.DataOperationType = (short)DataOperationType.Insert;
                FormData.DefaultCurrencyCode = _constants.GetAppSettingKeyValue("AppSetting", "DefaultCurrencyCode");

                string result = await _accountsServicesDAL.CreateUpdateBankAccountTransDAL(FormData);
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
        public async Task<IActionResult> GetAccountTransEditFormDataById(AccountTransactionsDetail FormData)
        {
            // ✅ Main Model
            AccountsModel model = new AccountsModel();
            try
            {
                if (FormData == null || FormData.TransId < 1)
                {
                    return Json(null);
                }

                model.AccountTransactionsDetailObj = await _accountsServicesDAL.GetAccountTransEditFormDataByIdDAL(FormData.TransId);

            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion

            }

            return Json(model.AccountTransactionsDetailObj);
        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.VendorAccountsTransaction, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> UpdateVendorAccountTransaction(AccountTransactionsDetail FormData)
        {

            try
            {
                // ✅ Main Model
                AccountsModel model = new AccountsModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.TransId > 0 ? FormData.TransId.ToString() : "") ? "Form is valid" : "Transaction id is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.VendorId > 0 ? FormData.VendorId.ToString() : "") ? "Form is valid" : "Vendor id is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.TransTitle) ? "Form is valid" : "Trans title is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.TransType) ? "Form is valid" : "Transaction type is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.TransAmount > 0 ? FormData.TransAmount.ToString() : "") ? "Form is valid" : "Amount is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData?.BankAccountId > 0 ? FormData.BankAccountId.ToString() : "") ? "Form is valid" : "Bank account is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion




                #region image file conversion secion
                List<ImageFileInfo> ImageFileInfosList = new List<ImageFileInfo>();
                if (FormData != null && FormData.AccountTransAttachementFile != null && FormData.AccountTransAttachementFile.Length > 0)
                {
                    foreach (IFormFile photo in FormData.AccountTransAttachementFile)
                    {
                        string FileDirectory = "/content/commonImages/accountsAttachments";
                        string url = await _filesHelpers.SaveFileToDirectory(photo, FileDirectory);
                        if (!String.IsNullOrWhiteSpace(url))
                        {
                            AttachmentEntity atch = new AttachmentEntity();
                            atch.AttachmentUrl = url;
                            atch.AttachmentName = photo.FileName;
                            atch.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                            //--save attachment url in database
                            int AttachmentId = await this._commonServicesDAL.SaveUpdateAttachmentDAL(atch);

                            ImageFileInfosList.Add(new ImageFileInfo { AttachmentId = AttachmentId, ImageName = photo.FileName, ImageGuidName = "", ImageURL = url });

                        }

                    }

                    if (ImageFileInfosList != null && ImageFileInfosList.Count > 0)
                    {
                        FormData.AccountTransAttachementJson = JsonConvert.SerializeObject(ImageFileInfosList);
                    }
                    else
                    {
                        FormData.AccountTransAttachementJson = String.Empty;
                    }
                }
                #endregion


                var LoginUserId = await this._sessionManag.GetLoginUserIdFromSession();
                FormData.LoginUserId = LoginUserId ?? 0;
                FormData.DataOperationType = (short)DataOperationType.Update;
                FormData.DefaultCurrencyCode = _constants.GetAppSettingKeyValue("AppSetting", "DefaultCurrencyCode");

                string result = await _accountsServicesDAL.CreateUpdateBankAccountTransDAL(FormData);
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
        public async Task<IActionResult> GetAccountTransEditFormAttachmentsId(AccountTransactionsDetail FormData)
        {
            // ✅ Main Model
            AccountsModel model = new AccountsModel();
            try
            {
                if (FormData == null || FormData.TransId < 1)
                {
                    return PartialView("~/Views/Accounts/PartialViews/_VendorAccountsTransaction.cshtml", model);
                }

                //--Get bank account attachments
                AccountTransAttachmentEntity accountTransAttachmentEntity = new AccountTransAttachmentEntity()
                {
                    TransId = FormData.TransId,
                    PageNo = 1,
                    PageSize = 20
                };
                model.AccountTransAttachmentList = await _accountsServicesDAL.GetAccountTransEditFormAttachmentsDAL(accountTransAttachmentEntity);


            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion

            }

            return PartialView("~/Views/Accounts/PartialViews/_AccountTransEditFormAttachment.cshtml", model);
        }

        // ✅ Delete product images
        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.VendorAccountsTransaction, 0, 0, (short)UserRightsEnum.Delete, 0, 0)]
        public async Task<IActionResult> DeleteAccountTransactionAttachment(string primarykeyValue, string primaryKeyColumn, string tableName, string AttachmentURL, int SqlDeleteType = (short)SqlDeleteTypes.PlainTableDelete)
        {

            try
            {

                bool result = await _commonServicesDAL.DeleteAnyRecordDAL(primarykeyValue, primaryKeyColumn, tableName, SqlDeleteType);
                if (result)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + AttachmentURL);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }


                    return Json(new { success = true, message = "Deleted Successfully!" });
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

    }
}
