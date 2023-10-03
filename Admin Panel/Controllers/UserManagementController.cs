
using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.DBInheritedModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Helpers.ConversionHelpers;
using Microsoft.AspNetCore.Mvc;
using static Entities.DBInheritedModels.InheritedEntitiesLevelTwo;

namespace AdminPanel.Controllers
{
    public class UserManagementController : BaseController
    {


        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IUserManagementServicesDAL _userManagementServicesDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IFilesHelpers _filesHelpers;

        public UserManagementController(IBasicDataServicesDAL basicDataDAL, IUserManagementServicesDAL userManagementServicesDAL, IConstants constants,
            ICommonServicesDAL commonServicesDAL, ISessionManager sessionManag, IFilesHelpers filesHelpers)
        {
            this._basicDataDAL = basicDataDAL;
            this._userManagementServicesDAL = userManagementServicesDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._filesHelpers = filesHelpers;
        }



        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.AddressTypesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> AddressTypesList(AddressTypeEntity FormData)
        {
            // ✅ Main Model
            UserManagementModel model = new UserManagementModel();
         
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Address Types List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.AddressTypesList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.AddressTypeList = await _userManagementServicesDAL.GetAddressTypesListDAL(FormData);


                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.AddressTypeList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.AddressTypeList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion



                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.AddressTypeList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.AddressTypeList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/UserManagement/PartialViews/_AddressTypesList.cshtml", model);
            }

            return View(model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CountriesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CountriesList(CountryEntity FormData)
        {
            // ✅ Main Model
            UserManagementModel model = new UserManagementModel();
       
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Countries List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CountriesList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.CountriesList = await _userManagementServicesDAL.GetCountriesListDAL(FormData);

              

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.CountriesList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.CountriesList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.CountriesList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.CountriesList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/UserManagement/PartialViews/_CountriesList.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUpdateCountry(CountryEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.CountryName))
                {
                    return Json(new { success = false, message = "Please fill the country code field!" });
                }

                if (String.IsNullOrWhiteSpace(FormData.CountryCode))
                {
                    return Json(new { success = false, message = "Please fill the country code field!" });
                }

                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.CountryId < 1)
                    {
                        return Json(new { success = false, message = "CountryId is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _userManagementServicesDAL.SaveUpdateCountryDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.StatesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> StateProvinceList(StateProvinceEntity FormData)
        {
            // ✅ Main Model
            UserManagementModel model = new UserManagementModel();
      
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "State/Province List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.StatesList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            try
            {
                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.StateProvinceList = await _userManagementServicesDAL.GetStateProvinceListDAL(FormData);

                //Get countries list for drop down
                CountryEntity countryEntity = new CountryEntity();
                countryEntity.PageSize = 260; //--Total Countries in wolrd are not more than this value
                model.CountriesList = await _userManagementServicesDAL.GetCountriesListDAL(countryEntity);

               

                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.StateProvinceList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.StateProvinceList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.StateProvinceList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.StateProvinceList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/UserManagement/PartialViews/_StateProvinceList.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUpdateStateProvinceEntity(StateProvinceEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {


            try
            {
                if (String.IsNullOrWhiteSpace(FormData.StateName))
                {
                    return Json(new { success = false, message = "Please fill the StateName field!" });
                }

                if (FormData.CountryId < 1)
                {
                    return Json(new { success = false, message = "Please fill the country field!" });
                }

                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.StateProvinceId < 1)
                    {
                        return Json(new { success = false, message = "State id is null!" });
                    }

                }

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _userManagementServicesDAL.SaveUpdateStateProvinceDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CititesList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CitiesList(CityEntity FormData)
        {
            // ✅ Main Model
            UserManagementModel model = new UserManagementModel();
    
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Cities List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CititesList;
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
                model.CitiesList = await _userManagementServicesDAL.GetCitiesListDAL(FormData);

                //Get countries list for drop down
                CountryEntity countryEntity = new CountryEntity();
                countryEntity.PageSize = 300; //--Total Countries in wolrd are not more than this value
                model.CountriesList = await _userManagementServicesDAL.GetCountriesListDAL(countryEntity);

             
                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.CitiesList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.CitiesList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion


                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.CitiesList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.CitiesList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/UserManagement/PartialViews/_CitiesList.cshtml", model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUpdateCity(CityEntity FormData, int DataOperationType = (short)DataOperationType.Insert)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(FormData.CityName))
                {
                    return Json(new { success = false, message = "Please fill the city name field!" });
                }


                if (FormData.IsActive == null)
                {
                    return Json(new { success = false, message = "Please select status!" });
                }

                if (DataOperationType == 2)
                {
                    if (FormData.CityId < 1)
                    {
                        return Json(new { success = false, message = "City Id is null!" });
                    }

                }

                //-- "-999" is for others
                if (FormData.StateProvinceId!=null && FormData.StateProvinceId ==-999)
                {
                    FormData.StateProvinceId = null;
                }




                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _userManagementServicesDAL.SaveUpdateCityDAL(FormData, DataOperationType);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UsersList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> UsersList(UserEntity FormData)
        {
            // ✅ Main Model
            UserManagementModel model = new UserManagementModel();
         
            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Users List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.UsersList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            try
            {

                //--Get user Types list
                UserTypesEntity userTypesEntity = new UserTypesEntity()
                {
                    PageNo = 1,
                    PageSize = 100
                };
                model.UserTypesList = await this._userManagementServicesDAL.GetUserTypesListDAL(userTypesEntity);



                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
               
                model.UsersList = await _userManagementServicesDAL.GetUsersListDAL(FormData);

             
                #region pagination data
                model.pageHelperObj = new PagerHelper();
                int TotalRecords = model?.UsersList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.UsersList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);
                #endregion


                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.UsersList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.UsersList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/UserManagement/PartialViews/_UsersList.cshtml", model);
            }

            return View(model);
        }


        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewUser, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> CreateUser()
        {
            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion



            // ✅ Main Model
            UserManagementModel model = new UserManagementModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Create User";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.CreateNewUser;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            //--Get countries list
            CountryEntity countryEntity = new CountryEntity()
            {
                PageNo = 1,
                PageSize = 300
            };
            model.CountriesList = await this._userManagementServicesDAL.GetCountriesListDAL(countryEntity);

            //--Get user Types list
            UserTypesEntity userTypesEntity = new UserTypesEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.UserTypesList = await this._userManagementServicesDAL.GetUserTypesListDAL(userTypesEntity);


            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStateByCountryId(int CountryId)
        {

            try
            {
                StateProvinceEntity stateProvinceEntity = new StateProvinceEntity()
                {
                    PageNo = 1,
                    PageSize = 400,
                    CountryId = CountryId
                };
                var StatesList = await this._userManagementServicesDAL.GetStateProvinceListDAL(stateProvinceEntity);
                var result = (from o in StatesList
                              select new
                              {
                                  displayValue = o.StateProvinceId,
                                  displayText = o.StateName,
                              }).ToList();


                return Json(new { success = true, message = "Get Successfully!", result = result });

            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCityByStateId(int StateProvinceId)
        {

            try
            {
                CityEntity cityEntity = new CityEntity()
                {
                    PageNo = 1,
                    PageSize = 600,
                    StateProvinceId = StateProvinceId
                };
                var StatesList = await this._userManagementServicesDAL.GetCitiesListDAL(cityEntity);
                var result = (from o in StatesList
                              select new
                              {
                                  displayValue = o.CityId,
                                  displayText = o.CityName,
                              }).ToList();


                return Json(new { success = true, message = "Get Successfully!", result = result });

            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }


        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.CreateNewUser, (short)UserRightsEnum.Add, 0, 0, 0, 0)]
        public async Task<IActionResult> CreateNewUserPost(UserEntity FormData)
        {

            try
            {
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                // ✅ Main Model
                UserManagementModel model = new UserManagementModel();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.FirstName) ? "Form is valid" : "First name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.LastName) ? "Form is valid" : "Last name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.UserName) ? "Form is valid" : "User name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.EmailAddress) ? "Form is valid" : "Email is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.Password) ? "Form is valid" : "Password is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.UserTypeId > 0 ? "Form is valid" : "User type is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count()>0 && validationList.Where(a=>a!= "Form is valid").ToList().Count>0 )
                {
                    return Json(new { success = false, message = ValidationMsg });
                }


                //--check of user name or email already exists
                 ValidationMsg = await _userManagementServicesDAL.CheckIfUserAlreadyExistsDAL(FormData.UserName, FormData.EmailAddress);
                if (ValidationMsg != "User not exists")
                {
                    return Json(new { success = false, message = ValidationMsg });
                }

                #endregion


                #region image file conversion secion

                Guid ImgGuid = Guid.NewGuid();
                if (FormData != null && FormData.ProfilePictureFile != null && FormData.ProfilePictureFile.Length > 0)
                {
                    string ImageGuidName = ImgGuid.ToString().Substring(0, 3) + "_" + FormData.ProfilePictureFile.FileName;
                    string OtherImagesDirectory = _constants.GetAppSettingKeyValue("AppSetting", "OtherImagesDirectory");

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + OtherImagesDirectory, ImageGuidName);
                    var stream = new FileStream(path, FileMode.Create);
                    await FormData.ProfilePictureFile.CopyToAsync(stream);

                    FormData.ProfilePictureUrl = OtherImagesDirectory + "/" + ImageGuidName;
                }
                #endregion


                //-- "-999" is for others
                if (FormData!=null && FormData.StateProvinceId != null && FormData.StateProvinceId == -999)
                {
                    FormData.StateProvinceId = null;
                }

                //-- "-999" is for others
                if (FormData != null && FormData.CityId != null && FormData.CityId == -999)
                {
                    FormData.CityId = null;
                }

                //--Encrypt password
                FormData.Password = CommonConversionHelper.Encrypt(FormData.Password);

                //--created by user id
                FormData.CreatedBy = Convert.ToInt32(await this._sessionManag.GetLoginUserIdFromSession());

                FormData.DataOperationType = (int)DataOperationType.Insert;

                string result = await _userManagementServicesDAL.CreateUpdateUserDAL(FormData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UpdateUser, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> UpdateUser(int UserId)
        {
            #region Basic page setting area
            ViewBag.ThemeFormValidationScriptEnabled = true;
            #endregion



            // ✅ Main Model
            UserManagementModel model = new UserManagementModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Update User";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.UpdateUser;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            //--Get countries list
            CountryEntity countryEntity = new CountryEntity()
            {
                PageNo = 1,
                PageSize = 300
            };
            model.CountriesList = await this._userManagementServicesDAL.GetCountriesListDAL(countryEntity);

            //--Get user Types list
            UserTypesEntity userTypesEntity = new UserTypesEntity()
            {
                PageNo = 1,
                PageSize = 100
            };
            model.UserTypesList = await this._userManagementServicesDAL.GetUserTypesListDAL(userTypesEntity);


            //--Get user data by user id
            model.UserEntityObj = await _userManagementServicesDAL.GetUserCompleteDataByIdDAL(UserId);
            if (model.UserEntityObj==null || model.UserEntityObj.UserId<1)
            {
               
                return RedirectToAction("UsersList","UserManagement");
            }


            model.UserEntityObj.Password =!String.IsNullOrWhiteSpace(model.UserEntityObj.Password) ? (CommonConversionHelper.Decrypt(model.UserEntityObj.Password)) : ""; 

            //--Get states for user by StateProvinceId 
            if (model.UserEntityObj != null && model.UserEntityObj.StateProvinceId >0)
            {
                StateProvinceEntity stateProvinceEntity = new StateProvinceEntity()
                {
                    PageNo = 1,
                    PageSize = 100,
                    StateProvinceId = (int)model.UserEntityObj.StateProvinceId
                };
                model.StateProvinceList = await this._userManagementServicesDAL.GetStateProvinceListDAL(stateProvinceEntity);

            }

            //--Get states for user by Cityid
            if (model.UserEntityObj != null && model.UserEntityObj.CityId > 0)
            {
                CityEntity cityEntity = new CityEntity()
                {
                    PageNo = 1,
                    PageSize = 100,
                    CityId = (int)model.UserEntityObj.CityId
                };
                model.CitiesList = await this._userManagementServicesDAL.GetCitiesListDAL(cityEntity);

            }



            return View(model);
        }



        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.UpdateUser, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateUserPost(UserEntity FormData)
        {

            try
            {
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                // ✅ Main Model
                UserManagementModel model = new UserManagementModel();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.UserId>0 ? "Form is valid" : "First name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.FirstName) ? "Form is valid" : "First name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.LastName) ? "Form is valid" : "Last name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.UserName) ? "Form is valid" : "User name is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.EmailAddress) ? "Form is valid" : "Email is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.Password) ? "Form is valid" : "Password is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.UserTypeId > 0 ? "Form is valid" : "User type is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = ValidationMsg });
                }


              
                #endregion


                #region image file conversion secion

                Guid ImgGuid = Guid.NewGuid();
                if (FormData != null && FormData.ProfilePictureFile != null && FormData.ProfilePictureFile.Length > 0)
                {
                    string ImageGuidName = ImgGuid.ToString().Substring(0, 3) + "_" + FormData.ProfilePictureFile.FileName;
                    string OtherImagesDirectory = _constants.GetAppSettingKeyValue("AppSetting", "OtherImagesDirectory");

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + OtherImagesDirectory, ImageGuidName);
                    var stream = new FileStream(path, FileMode.Create);
                    await FormData.ProfilePictureFile.CopyToAsync(stream);

                    FormData.ProfilePictureUrl = OtherImagesDirectory + "/" + ImageGuidName;
                }
                #endregion

                //--Encrypt password
                FormData.Password = CommonConversionHelper.Encrypt(FormData.Password);

                //--created by user id
                FormData.CreatedBy = Convert.ToInt32(await this._sessionManag.GetLoginUserIdFromSession());

                FormData.DataOperationType = (int)DataOperationType.Update;

                string result = await _userManagementServicesDAL.CreateUpdateUserDAL(FormData);
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
