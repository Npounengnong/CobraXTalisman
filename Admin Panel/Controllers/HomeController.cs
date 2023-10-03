using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.DBInheritedModels;
using Entities.DBModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Helpers.ConversionHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic;
using Newtonsoft.Json;

namespace AdminPanel.Controllers
{
    public class HomeController : BaseController
    {

        private readonly IHomeServicesDAL _homeServicesDAL;
        private readonly IConstants _constants;
        private readonly ISessionManager _sessionManag;
        private readonly ICommonServicesDAL _commonServicesDAL;
        public HomeController(IHomeServicesDAL homeServicesDAL , IConstants constants, ISessionManager sessionManag, ICommonServicesDAL commonServicesDAL)
        {
            this._homeServicesDAL = homeServicesDAL;
            this._constants = constants;
            this._sessionManag = sessionManag;
            _commonServicesDAL = commonServicesDAL;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ✅ Main Model
            HomeModel model = new HomeModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.Dashboard;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardChartsData(string FromDate, string ToDate)
        {
            // ✅ Main Model
            HomeModel model = new HomeModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.Dashboard;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion



            try
            {


                #region dashboard life time statistics
                model.dashboardLifeTimeStatistics = await this._homeServicesDAL.GetDashboardLifetimeStatisticsDAL(FromDate, ToDate);
                #endregion


                #region sales per month
                model.SalesPerMonthData = await this._homeServicesDAL.GetChartSalesPerMonthDataDAL(FromDate, ToDate);
                model.SalesPerMonthData = model.SalesPerMonthData?.OrderBy(y => y.Year).ThenBy(m => m.MonthInNumber).ToList();
                #endregion

                #region order revenue
                model.OrderRevenuePerMonth = await this._homeServicesDAL.GetChartOrderRevenuePerMonthDataDAL(FromDate, ToDate);
                model.OrderRevenuePerMonth = model.OrderRevenuePerMonth?.OrderBy(y => y.Year).ThenBy(m => m.MonthInNumber).ToList();
                #endregion

                #region popular categoreis
                model.SalesPopularCategoriesData = await this._homeServicesDAL.GetSalesPopularCategoriesDataDAL(FromDate, ToDate);
                var DistinctParentCategoriesIds = model.SalesPopularCategoriesData.Select(x => x.ParentCategoryID).Distinct().ToList();

                model.distinctPopularCategoriesChartDic = new Dictionary<string, object>();
                foreach (var ParentCat in DistinctParentCategoriesIds)
                {

                    string? ParentCategoryName = model?.SalesPopularCategoriesData?.Where(p => p.ParentCategoryID == ParentCat)?.ToList()?.FirstOrDefault()?.ParentCategoryName;
                    ParentCategoryName = StringConversionHelper.TruncateAnyStringValue(ParentCategoryName, 25, true);

                    var currentPopulareCategories = model?.SalesPopularCategoriesData?.Where(p => p.ParentCategoryID == ParentCat)?.ToList();

                    Dictionary<string, object> currentCategoryDic = new Dictionary<string, object>();
                    if (currentPopulareCategories != null)
                    {
                        foreach (var crnt in currentPopulareCategories)
                        {
                            if (!currentCategoryDic.ContainsKey(crnt.CategoryName ?? "Other"))
                            {
                                currentCategoryDic.Add(crnt.CategoryName ?? "Other", crnt.TotalSale);
                            }

                        }
                    }

                    currentCategoryDic.Add("quantity", currentPopulareCategories != null ? currentPopulareCategories.Sum(s => s.TotalSale) : 0);

                    if (!currentCategoryDic.ContainsKey(ParentCategoryName ?? "Other"))
                    {
                        model?.distinctPopularCategoriesChartDic.Add(ParentCategoryName ?? "Other", currentCategoryDic);
                    }

                }
                #endregion



                model.CustomersLocationWiseData = await this._homeServicesDAL.GetChartCustomersLocationWiseDataDAL(FromDate, ToDate);

                #region popular products
                model.PopularProductsData = await this._homeServicesDAL.GetChartPopularProductsDataDAL(FromDate, ToDate);
                //--make product title/label short
                foreach (var prd in model.PopularProductsData)
                {
                    prd.ChartLabel = StringConversionHelper.TruncateAnyStringValue(prd.ChartLabel, 10, true);
                }
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

            return PartialView("~/Views/Home/PartialViews/_DashboardChart.cshtml", model);
        }

        public IActionResult Privacy()
        {
            return View();
        }


     


    }
}