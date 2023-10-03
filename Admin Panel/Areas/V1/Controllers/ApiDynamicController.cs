using DAL.Repository.IServices;
using Entities.DBInheritedModels;
using Helpers.ApiHelpers;
using Helpers.AuthorizationHelpers;
using Helpers.AuthorizationHelpers.JwtTokenHelper;
using Helpers.CommonHelpers;
using Helpers.ConversionHelpers;
using Helpers.ConversionHelpers.IConversionHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;

namespace AdminPanel.Areas.V1.Controllers
{
    [Route("api/v1/dynamic")] //-- "dynamic" is controller name with out api keyword"
    [ApiController]
    [Area("V1")]
    public class ApiDynamicController : ApiBaseController
    {

        private readonly IApiOperationServicesDAL _apiOperationServicesDAL;
        private readonly ICalculationHelper _calculationHelper;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IConstants _constants;

        public ApiDynamicController(IApiOperationServicesDAL apiOperationServices, ICommonServicesDAL commonServicesDAL, ISessionManager sessionManag, 
            IConstants constants , ICalculationHelper calculationHelper)
        {
            this._apiOperationServicesDAL = apiOperationServices;
            _commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._constants = constants;
            this._calculationHelper = calculationHelper;
        }


        [Route("dataoperation/{UrlName?}")]
        [ServiceFilter(typeof(CustomerApiCallsAuthorization))]
        public async Task<APIActionResult> DataOperation(string UrlName, [FromBody] Dictionary<string, object> param)
        {
            #region Basic declaration
            //--Api result type declared in resultType variable
            string resultType = "json";

            NoorAppAPIResult result = new NoorAppAPIResult();
            APIActionResult apiActionResult;

            result.ActionType = (ActionTypeEnum)Enum.Parse(typeof(ActionTypeEnum), resultType, true);

            //--This data variable will be used for storing data
            string? data = string.Empty;
            #endregion

            try
            {

                //--Get Api Configuration
                var ApiConfiguration = await this._apiOperationServicesDAL.GetAPIConfiguration(UrlName);


                if (param != null && param.Count != 0)
                {
                    Dictionary<string, object>? requestParameters = new Dictionary<string, object>();
                    if (param.ContainsKey("requestParameters"))
                    {
                        string? ParamKeyValue = param["requestParameters"].ToString();
                        if (!String.IsNullOrWhiteSpace(ParamKeyValue))
                        {
                            requestParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(ParamKeyValue);

                            //check of requestParameters contains any password key  
                            if (UrlName == "get-user-login" || UrlName == "signup-user")
                            {
                                if (requestParameters != null && requestParameters.ContainsKey("Password"))
                                {
                                    requestParameters["Password"] = requestParameters["Password"] != null
                                                                    && !String.IsNullOrWhiteSpace(requestParameters["Password"].ToString())
                                                                    ?
                                                                    (CommonConversionHelper.Encrypt(requestParameters["Password"].ToString()))
                                                                    :
                                                                    requestParameters["Password"];
                                }
                            }


                        }

                    }


                 

                    data = await this._apiOperationServicesDAL.GetApiData(requestParameters, ApiConfiguration);

                    #region set JWT Token
                    if (UrlName.Contains("get-user-login"))
                    {
                        result.Token =JwtManager.GetJwtToken( data ?? "{}");
                    }

                    #endregion


                    #region if required further calculation like Discount region
                    if (ApiConfiguration?.IsFurtherCalculationRequired == true)
                    {
                        try
                        {
                            data = await _calculationHelper.CalculateDiscountsForProducts( (data ?? "[]"));
                        }
                        catch (Exception ex) //--in case of exception, return normal json
                        {
                            #region log error
                            await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);
                            #endregion
                        }

                    }
                    #endregion

                  

                }

                #region result
                result.Data = data;
                result.StatusCode = 200;
                result.StatusMessage = "Ok";
                result.ErrorMessage = String.Empty;
                apiActionResult = new APIActionResult(result);
                #endregion

            }
            catch (Exception ex)
            {

                #region log error
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);
                #endregion

                result.StatusCode = 501;
                result.StatusMessage = "Error";
                result.ErrorMessage = "An error is occured while processing your request.";
                apiActionResult = new APIActionResult(result);
            }


            return apiActionResult;
        }


       
    }
}
