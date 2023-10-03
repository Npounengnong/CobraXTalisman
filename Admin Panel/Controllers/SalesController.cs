using DAL.Repository.IServices;
using Entities.CommonModels;
using Entities.DBInheritedModels;
using Entities.MainModels;
using Helpers.AuthorizationHelpers;
using Helpers.CommonHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Entities.DBInheritedModels.InheritedEntitiesLevelTwo;


namespace AdminPanel.Controllers
{
    public class SalesController : BaseController
    {

        private readonly IBasicDataServicesDAL _basicDataDAL;
        private readonly IProductServicesDAL _productServicesDAL;
        private readonly IConstants _constants;
        private readonly ICommonServicesDAL _commonServicesDAL;
        private readonly ISessionManager _sessionManag;
        private readonly IUserManagementServicesDAL _userManagementServicesDAL;
        private readonly IFilesHelpers _filesHelpers;
        private readonly IDiscountsServicesDAL _discountsServicesDAL;
        private readonly ISalesServicesDAL _salesServicesDAL;

        public SalesController(IBasicDataServicesDAL basicDataDAL, IProductServicesDAL productServicesDAL, IConstants constants, ICommonServicesDAL commonServicesDAL,
            ISessionManager sessionManag, IUserManagementServicesDAL userManagementServicesDAL, IFilesHelpers filesHelpers, IDiscountsServicesDAL discountsServicesDAL, ISalesServicesDAL salesServicesDAL)
        {
            this._basicDataDAL = basicDataDAL;
            this._productServicesDAL = productServicesDAL;
            this._constants = constants;
            this._commonServicesDAL = commonServicesDAL;
            this._sessionManag = sessionManag;
            this._userManagementServicesDAL = userManagementServicesDAL;
            this._filesHelpers = filesHelpers;
            this._discountsServicesDAL = discountsServicesDAL;
            this._salesServicesDAL = salesServicesDAL;
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.OrdersList, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> OrdersList(OrderEntity FormData)
        {
            // ✅ Main Model
            SalesModel model = new SalesModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Orders List";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.OrdersList;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            try
            {

                OrderStatusesEntity categoryEntityFormData = new OrderStatusesEntity()
                {
                    PageNo = 1,
                    PageSize = 200
                };
                model.OrderStatusesList = await this._salesServicesDAL.GetOrderStatusesList(categoryEntityFormData);


                if (FormData.VendorId == null || FormData.VendorId == 0)
                {
                    #region ViewSelf Right Check
                    bool SelfRight = await _sessionManag.GetViewSelfRightForLoginUserFromSession();
                    if (SelfRight)
                    {
                        int? LoginUserId = await _sessionManag.GetLoginUserIdFromSession();
                        FormData.VendorId = LoginUserId;
                    }
                    else
                    {
                        FormData.VendorId = 0;
                    }
                    #endregion
                }




                UserEntity vendorEntityFormData = new UserEntity()
                {
                    PageNo = 1,
                    PageSize = 1000,
                    UserTypeId = (short)UserTypesEnum.Vendor,
                    UserId = Convert.ToInt32(FormData.VendorId ?? 0)
                };
                model.VendorsList = await this._userManagementServicesDAL.GetUsersListDAL(vendorEntityFormData);


                FormData.PageSize = this._constants.ITEMS_PER_PAGE();
                model.OrdersList = await _salesServicesDAL.GetOrdersListDAL(FormData);



                #region pagination data
                model.pageHelperObj = new PagerHelper();

                int TotalRecords = model?.OrdersList?.FirstOrDefault()?.TotalRecords ?? 0;
                model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.OrdersList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), FormData.PageNo);

                #endregion

                if (FormData.DataExportType != null && FormData.DataExportType == (short)DataExportTypeEnum.Excel && model?.OrdersList?.Count > 0)
                {
                    var ExcelFileResutl = await this._filesHelpers.ExportToExcel(this, model.PageBasicInfoObj.PageTitle, model.OrdersList.Cast<dynamic?>().ToList());
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
                return PartialView("~/Views/Sales/PartialViews/_OrdersList.cshtml", model);
            }

            return View(model);
        }

        [HttpGet]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.OrderDetail, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> OrderDetail(int OrderId)
        {
            // ✅ Main Model
            SalesModel model = new SalesModel();

            #region page basic info
            model.PageBasicInfoObj = new PageBasicInfo();
            model.PageBasicInfoObj.PageTitle = "Order Detail";
            model.PageBasicInfoObj.EntityId = (int)EntitiesEnum.OrderDetail;
            model.PageBasicInfoObj.langCode = await _sessionManag.GetLanguageCodeFromSession();
            #endregion


            ViewBag.OrderId = OrderId;

            try
            {
                ShippingMethodEntity shippingMethodEntity = new ShippingMethodEntity()
                {
                    PageNo = 1,
                    PageSize = 100
                };
                model.ShippingMethodsList = await this._productServicesDAL.GetShippingMethodsListDAL(shippingMethodEntity);

                OrderStatusesEntity categoryEntityFormData = new OrderStatusesEntity()
                {
                    PageNo = 1,
                    PageSize = 200
                };
                model.OrderStatusesList = await this._salesServicesDAL.GetOrderStatusesList(categoryEntityFormData);



                UserEntity ShipperUsersEntityFormData = new UserEntity()
                {
                    PageNo = 1,
                    PageSize = 1000,
                    UserTypeId = (short)UserTypesEnum.Shipper,

                };
                model.ShippersList = await this._userManagementServicesDAL.GetUsersListDAL(ShipperUsersEntityFormData);


                OrderEntity OrderFormData = new OrderEntity()
                {

                    OrderId = OrderId,

                };



                model.OrderObj = await _salesServicesDAL.GetOrderDetailByIdDAL(OrderFormData);

              

                model.OrderShippingDetailList = JsonConvert.DeserializeObject<List<OrderShippingDetailEntity>>(model?.OrderObj?.OrderShippingDetailsDataJson ?? "[]");
                model.OrderShippingMasterData = JsonConvert.DeserializeObject<UserAddressEntity>(model?.OrderObj?.OrderShippingMasterDataJson ?? "[]");
                model.OrderItemsList = JsonConvert.DeserializeObject<List<OrderItemEntity>>(model?.OrderObj?.OrdersItemsJson ?? "[]");
                model.OrderPaymentsList = JsonConvert.DeserializeObject<List<OrdersPaymentEntity>>(model?.OrderObj?.OrderPaymentDetailsJson ?? "[]");

                //-- If the login user is vendor specially
                #region ViewSelf Right Check
                bool SelfRight = await _sessionManag.GetViewSelfRightForLoginUserFromSession();
                if (SelfRight)
                {
                    var user =  _sessionManag.GetLoginUserFromSession();
                    if (user!=null && user.UserTypeId ==(short)UserTypesEnum.Vendor )
                    {
                        model.OrderShippingDetailList = model?.OrderShippingDetailList?.Where(o => o.VendorId == user.UserId).ToList();
                    }
                }
                #endregion
               

                OrderNoteEntity OrderNoteFormData = new OrderNoteEntity()
                {
                    PageNo = 1,
                    PageSize = 1000,
                    OrderId = OrderId
                };
                model.OrderNotesList = await this._salesServicesDAL.GetOrderNotesListDAL(OrderNoteFormData);



                //#region pagination data
                //model.pageHelperObj = new PagerHelper();

                //int TotalRecords = model?.OrderShippingDetailList?.FirstOrDefault()?.TotalRecords ?? 0;
                //model.pageHelperObj = PagerHelper.Instance.MakePaginationObject(model?.OrderShippingDetailList?.Count() ?? 0, TotalRecords, _constants.ITEMS_PER_PAGE(), OrderShippingDetailFormData.PageNo);

                //#endregion

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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.OrderDetail, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateOrderShippingItemsDetail(OrderShippingDetailEntity FormData)
        {

            try
            {
                // ✅ Main Model
                SalesModel model = new SalesModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.OrderShippingDetailItemsJson) ? "Form is valid" : "Order shipping items is null!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.OrderId > 0 ? "Form is valid" : "Order id is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _salesServicesDAL.UpdateOrderShippingItemsDetailDAL(FormData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.OrderDetail, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> SaveOrderNoteReply(OrderNoteEntity FormData)
        {

            try
            {
                // ✅ Main Model
                SalesModel model = new SalesModel();
                string ValidationMsg = "Form is valid";
                List<string> validationList = new List<string>();

                #region validation area

                ValidationMsg = FormData == null ? "Form is null!" : "Form is valid";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && !String.IsNullOrWhiteSpace(FormData.Message) ? "Form is valid" : "Message is required!";
                validationList.Add(ValidationMsg);
                ValidationMsg = FormData != null && FormData.OrderId > 0 ? "Form is valid" : "Order id is required!";
                validationList.Add(ValidationMsg);


                if (validationList.Count() > 0 && validationList.Where(a => a != "Form is valid").ToList().Count > 0)
                {
                    return Json(new { success = false, message = validationList.FirstOrDefault(x => x != "Form is valid") });
                }

                #endregion

                FormData.UserId = await this._sessionManag.GetLoginUserIdFromSession();

                string result = await _salesServicesDAL.SaveOrderNoteReplyDAL(FormData);
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
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.OrderDetail, 0, 0, 0, (short)UserRightsEnum.View_All, (short)UserRightsEnum.View_Self)]
        public async Task<IActionResult> GetLatestOrderNoteMessages(OrderNoteEntity FormData)
        {
            // ✅ Main Model
            SalesModel model = new SalesModel();

            try
            {

                OrderNoteEntity OrderNoteFormData = new OrderNoteEntity()
                {
                    PageNo = 1,
                    PageSize = 1000,
                    OrderId = FormData.OrderId
                };
                model.OrderNotesList = await this._salesServicesDAL.GetOrderNotesListDAL(OrderNoteFormData);


            }
            catch (Exception ex)
            {
                await this._commonServicesDAL.LogRunTimeExceptionDAL(ex.Message, ex.StackTrace, ex.StackTrace);

                #region error model
                model.SuccessErrorMsgEntityObj = new SuccessErrorMsgEntity();
                model.SuccessErrorMsgEntityObj.ErrorMsg = "An error occured. Please try again.";
                #endregion
            }

            return PartialView("~/Views/Sales/PartialViews/_OrderNoteMessages.cshtml", model);

        }

        [HttpPost]
        [RolesRightsAuthorizationHelper((int)EntitiesEnum.OrdersList, 0, (short)UserRightsEnum.Update, 0, 0, 0)]
        public async Task<IActionResult> UpdateOrderStatus(int OrderId, int LatestStatusId)
        {

            try
            {
                // ✅ Main Model
                SalesModel model = new SalesModel();

                #region validation area

                if (OrderId < 0)
                {
                    return Json(new { success = false, message = "Order id is null!" });
                }

                if (LatestStatusId < 0)
                {
                    return Json(new { success = false, message = "Status is null!" });
                }


                #endregion

                int UserId = Convert.ToInt32(await this._sessionManag.GetLoginUserIdFromSession());

                string result = await _salesServicesDAL.UpdateOrderStatusDAL(OrderId, LatestStatusId, UserId);
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
        public async Task<IActionResult> showOrterItemVariants(int OrderId, int OrderItemId)
        {
            // ✅ Main Model
            SalesModel model = new SalesModel();

            try
            {
                model.OrderVariantsList = await this._salesServicesDAL.GetOrderVariantsDetailByIdDAL(OrderId);
                if (model.OrderVariantsList!=null)
                {
                    model.OrderVariantsList = model.OrderVariantsList.Where(x=>x.OrderItemID==OrderItemId).ToList();
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

          
            return PartialView("~/Views/Sales/PartialViews/_OrderItemVariants.cshtml", model);
        }

    }
}
