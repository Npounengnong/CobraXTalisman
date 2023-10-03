using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.DBInheritedModels;
using Entities.DBModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Helpers.ConversionHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using System.ComponentModel;

namespace AdminPanel.Controllers
{
    public class DynamicController : BaseController
    {

        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;

        public DynamicController(IBasicDataServicesDAL basicDataDAL, IConstants constants, ICommonServicesDAL commonServicesDAL, ISessionManager sessionManag)
        {
            this._basicDataDAL = basicDataDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
        }

        // ✅ Delete any record using this method Dynamcially
        [HttpPost]
        [RolesRightsAuthorizationHelper(0, 0, 0, (short)UserRightsEnum.Delete, 0, 0)]
        public async Task<IActionResult> DeleteAnyRecord(string EntityId, string primarykeyValue, string primaryKeyColumn, string tableName, int SqlDeleteType = (short)SqlDeleteTypes.PlainTableDelete)
        {


            try
            {
                bool result = await _commonServicesDAL.DeleteAnyRecordDAL(primarykeyValue, primaryKeyColumn, tableName, SqlDeleteType);
                if (result)
                {
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

        [HttpPost]
        public async Task<IActionResult> ChangeDataTableLength(int itemsPerPage)
        {


            try
            {

                HttpContext.Session.SetInt32("ITEMS_PER_PAGE", itemsPerPage);

                return Json(new { success = true, message = "Saved Successfully!" });

            }
            catch (Exception ex)
            {

                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                return Json(new { success = false, message = "An error occured on server side.", ExMsg = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLocalizationControlsJsonDataForScreen(int EntityId)
        {


            try
            {

                if (EntityId < 1)
                {
                    return Json(new { success = false, message = "Incorrect screen id!" });
                }

                string? langCode = await _sessionManag.GetLanguageCodeFromSession();
                if (String.IsNullOrWhiteSpace(langCode))
                {
                    return Json(new { success = false, message = "Incorrect language code!" });
                }

               
                ScrnsLocalizationEntity scrnsLocalization = new ScrnsLocalizationEntity()
                {
                    ScreenId = EntityId,
                    AppModuleId = (short)AppModulesEnum.AdminPanel,
                    LanguageId = CommonConversionHelper.GetLanguageIdbyLanguageCode(langCode)
                };
                var result = await _commonServicesDAL.GetScreenLocalizationJsonDataDAL(scrnsLocalization);

                if (result != null && !String.IsNullOrWhiteSpace(result.LabelsJsonData))
                {
                    return Json(new { success = true, message = "success", dataLocalization = result.LabelsJsonData });
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
