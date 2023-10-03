using DAL.DBContext;
using DAL.Repository.Services;
using DAL.Repository.IServices;
using PetaPoco;
using Microsoft.EntityFrameworkCore;
using Helpers.CommonHelpers;
using Helpers.AuthorizationHelpers;
using Helpers.ConversionHelpers.IConversionHelpers;
using Helpers.ConversionHelpers;
using System.Configuration;
using AdminPanel.Helpers.EmailSenderHelper;
using Helpers.AuthorizationHelpers.JwtTokenHelper;
using Helpers.CommonHelpers.ICommonHelpers;
using DocumentFormat.OpenXml.Presentation;
using Entities.DBModels;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Entities.CommonModels.AccountsModule;

namespace AdminPanel.Helpers
{
    public static class ServiceExtensions
    {

        public static string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public static void ConfigureIServiceCollection(this WebApplicationBuilder builder)
        {

            #region CORS setting for API

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.AllowAnyOrigin()
                                                          .AllowAnyHeader()
                                                          .AllowAnyMethod();

                                      //policy.WithOrigins("https://noorecommerceshop.netlify.app" , "http://noornashad-001-site3.etempurl.com")
                                      //             .AllowAnyHeader()
                                      //             .AllowAnyMethod();
                                  });
            });
            #endregion


            // Add services to the container.
            builder.Services.AddControllersWithViews();

            #region main services registeration
            //---These are the services that we will use in our project for CRUD
            builder.Services.AddSingleton<IDatabase, Database>();
            builder.Services.AddSingleton<IDataContextHelper, DataContextHelper>();
            builder.Services.AddSingleton<IDapperConnectionHelper, DapperConnectionHelper>();
            builder.Services.AddSingleton<IProductServicesDAL, ProductServicesDAL>();
            builder.Services.AddSingleton<IBasicDataServicesDAL, BasicDataServicesDAL>();
            builder.Services.AddSingleton<ICommonServicesDAL, CommonServicesDAL>();
            builder.Services.AddSingleton<IUserManagementServicesDAL, UserManagementServicesDAL>();
            builder.Services.AddSingleton<IDiscountsServicesDAL, DiscountsServicesDAL>();
            builder.Services.AddSingleton<IApiOperationServicesDAL, ApiOperationServicesDAL>();
            builder.Services.AddSingleton<IConstants, Constants>();
            builder.Services.AddSingleton<IFilesHelpers, FilesHelpers>();
            builder.Services.AddSingleton<ISessionManager, SessionManager>();
            builder.Services.AddSingleton<IConfigurationServicesDAL, ConfigurationServicesDAL>();
            builder.Services.AddSingleton<INotificationsServicesDAL, NotificationsServicesDAL>();
            builder.Services.AddSingleton<ISalesServicesDAL, SalesServicesDAL>();
            builder.Services.AddSingleton<ICalculationHelper, CalculationHelper>();
            builder.Services.AddSingleton<IHomeServicesDAL, HomeServicesDAL>();
            builder.Services.AddSingleton<IAccountsServicesDAL, AccountsServicesDAL>();
            builder.Services.AddSingleton<IOrderHelper, OrderHelper>();

            //--For session and cookies purpose
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //--Different attributes registeration
            builder.Services.AddScoped<AdminLoginAuthorization>();
            builder.Services.AddScoped<CustomerApiCallsAuthorization>();

            #endregion

            //--Configure IConfiguration Interface for static methods
            StaticMethodsDependencyInjctHelper.Initialize(builder.Configuration, null);

            #region email sender configuraiton

            var emailConfig = builder.Configuration
           .GetSection("EmailConfiguration")
           .Get<EmailConfiguration>();
            builder.Services.AddSingleton(emailConfig);
            builder.Services.AddScoped<IEmailSender, EmailSender>();

            #endregion

            #region Session and cookies settings

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);//You can set Time  in seconds, minutes 
            });

            #endregion

        }

        public static void ConfigureWebApplication(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //---For session purpose
            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();


            app.UseCors(MyAllowSpecificOrigins);


            ConfigureRoutes(app);



        }

        public static void ConfigureRoutes(this WebApplication app)
        {



            #region Other Routes

            app.MapControllerRoute(
                name: "ProductsList",
                pattern: "{langCode}/products-catalog/products-list",
                defaults: new { controller = "ProductsCatalog", action = "ProductsList" });

            app.MapControllerRoute(
                name: "UsersList",
                pattern: "{langCode}/user-management/users-list",
                defaults: new { controller = "UserManagement", action = "UsersList" });

            app.MapControllerRoute(
                name: "CountriesList",
                pattern: "{langCode}/user-management/countries-list",
                defaults: new { controller = "UserManagement", action = "CountriesList" });

            app.MapControllerRoute(
                name: "StateProvinceList",
                pattern: "{langCode}/user-management/states-list",
                defaults: new { controller = "UserManagement", action = "StateProvinceList" });

            app.MapControllerRoute(
                name: "CitiesList",
                pattern: "{langCode}/user-management/cities-list",
                defaults: new { controller = "UserManagement", action = "CitiesList" });

            app.MapControllerRoute(
                name: "AddressTypesList",
                pattern: "{langCode}/user-management/address-type-list",
                defaults: new { controller = "UserManagement", action = "AddressTypesList" });

            app.MapControllerRoute(
                name: "ColorsList",
                pattern: "{langCode}/basic-data/colors-list",
                defaults: new { controller = "BasicData", action = "ColorsList" });

            app.MapControllerRoute(
                name: "CategoriesList",
                pattern: "{langCode}/basic-data/categories-list",
                defaults: new { controller = "BasicData", action = "CategoriesList" });

            app.MapControllerRoute(
                name: "TagsList",
                pattern: "{langCode}/basic-data/tags-list",
                defaults: new { controller = "BasicData", action = "TagsList" });

            app.MapControllerRoute(
                name: "SizeList",
                pattern: "{langCode}/basic-data/size-list",
                defaults: new { controller = "BasicData", action = "SizeList" });


            app.MapControllerRoute(
                name: "ManufacturersList",
                pattern: "{langCode}/basic-data/manufacturers-list",
                defaults: new { controller = "BasicData", action = "ManufacturersList" });


            app.MapControllerRoute(
                name: "CurrenciesList",
                pattern: "{langCode}/basic-data/currencies-list",
                defaults: new { controller = "BasicData", action = "CurrenciesList" });


            app.MapControllerRoute(
                name: "AttachmentTypesList",
                pattern: "{langCode}/basic-data/attachments-types",
                defaults: new { controller = "BasicData", action = "AttachmentTypesList" });


            app.MapControllerRoute(
                name: "PaymentMethodsList",
                pattern: "{langCode}/basic-data/payment-methods",
                defaults: new { controller = "BasicData", action = "PaymentMethodsList" });

            app.MapControllerRoute(
                name: "DiscountsList",
                pattern: "{langCode}/discounts/discounts-list",
                defaults: new { controller = "Discounts", action = "DiscountsList" });

            app.MapControllerRoute(
                name: "ContactUsList",
                pattern: "{langCode}/discounts/contact-us-list",
                defaults: new { controller = "Discounts", action = "ContactUsList" });

            app.MapControllerRoute(
                name: "SubcribersList",
                pattern: "{langCode}/discounts/subscribers-list",
                defaults: new { controller = "Discounts", action = "SubcribersList" });

            app.MapControllerRoute(
                name: "SiteHomeScreenBanners",
                pattern: "{langCode}/discounts/home-page-banners",
                defaults: new { controller = "Discounts", action = "SiteHomeScreenBanners" });


            app.MapControllerRoute(
                name: "DiscountCampaigns",
                pattern: "{langCode}/discounts/discount-campaigns",
                defaults: new { controller = "Discounts", action = "DiscountCampaigns" });

            app.MapControllerRoute(
                name: "RolesRightsSetting",
                pattern: "{langCode}/configuration/roles-rights-setting",
                defaults: new { controller = "Configuration", action = "RolesRightsSetting" });

            app.MapControllerRoute(
                name: "OrdersList",
                pattern: "{langCode}/sales/orders-list",
                defaults: new { controller = "Sales", action = "OrdersList" });

            app.MapControllerRoute(
                name: "AdminPanelNotificationsList",
                pattern: "{langCode}/notifications/notifications-list",
                defaults: new { controller = "Notifications", action = "AdminPanelNotificationsList" });

            app.MapControllerRoute(
                name: "Banks",
                pattern: "{langCode}/accounts/banks",
                defaults: new { controller = "Accounts", action = "Banks" });

            app.MapControllerRoute(
                name: "UsersBankAccounts",
                pattern: "{langCode}/accounts/users-bank-accounts",
                defaults: new { controller = "Accounts", action = "UsersBankAccounts" });

            app.MapControllerRoute(
                name: "VendorPayments",
                pattern: "{langCode}/accounts/vendor-payments",
                defaults: new { controller = "Accounts", action = "VendorPayments" });

            app.MapControllerRoute(
                name: "VendorsCommissionSetup",
                pattern: "{langCode}/accounts/vendors-commission-setup",
                defaults: new { controller = "Accounts", action = "VendorsCommissionSetup" });

            app.MapControllerRoute(
                name: "SitesLogo",
                pattern: "{langCode}/configuration/sites-logo",
                defaults: new { controller = "Configuration", action = "SitesLogo" });

            app.MapControllerRoute(
                name: "ProductsReviews",
                pattern: "{langCode}/products-catalog/products-reviews",
                defaults: new { controller = "ProductsCatalog", action = "ProductsReviews" });

            app.MapControllerRoute(
                name: "ProductsBulkUpload",
                pattern: "{langCode}/products-catalog/products-bulk-upload",
                defaults: new { controller = "ProductsCatalog", action = "ProductsBulkUpload" });

            app.MapControllerRoute(
                name: "ImagesUpload",
                pattern: "{langCode}/products-catalog/images-upload",
                defaults: new { controller = "ProductsCatalog", action = "ImagesUpload" });

            app.MapControllerRoute(
                name: "CreateNewProduct",
                pattern: "{langCode}/products-catalog/create-new-product",
                defaults: new { controller = "ProductsCatalog", action = "CreateNewProduct" });

            app.MapControllerRoute(
                name: "UpdateProduct",
                pattern: "{langCode}/products-catalog/update-product/{ProductId}",
                defaults: new { controller = "ProductsCatalog", action = "UpdateProduct" });

            app.MapControllerRoute(
                name: "ProductReviewsDetail",
                pattern: "{langCode}/products-catalog/product-review-detail/{ProductId}",
                defaults: new { controller = "ProductsCatalog", action = "ProductReviewsDetail" });

            app.MapControllerRoute(
               name: "ProductVariants",
               pattern: "{langCode}/products-catalog/product-variants",
               defaults: new { controller = "ProductsCatalog", action = "ProductVariants" });

            app.MapControllerRoute(
              name: "ProductVariantDetail",
              pattern: "{langCode}/products-catalog/product-variant-detail/{ProductAttributeId}",
              defaults: new { controller = "ProductsCatalog", action = "ProductVariantDetail" });

            app.MapControllerRoute(
                name: "OrderDetail",
                pattern: "{langCode}/sales/order-detail/{OrderId}",
                defaults: new { controller = "Sales", action = "OrderDetail" });

            app.MapControllerRoute(
                name: "CreateUser",
                pattern: "{langCode}/user-management/create-new-user",
                defaults: new { controller = "UserManagement", action = "CreateUser" });

            app.MapControllerRoute(
                name: "UpdateUser",
                pattern: "{langCode}/user-management/update-user/{UserId}",
                defaults: new { controller = "UserManagement", action = "UpdateUser" });

            app.MapControllerRoute(
                name: "CreateUserBankAccount",
                pattern: "{langCode}/accounts/create-bank-account",
                defaults: new { controller = "Accounts", action = "CreateUserBankAccount" });

            app.MapControllerRoute(
                name: "UpdateUserBankAccount",
                pattern: "{langCode}/accounts/update-bank-account/{BankAccountId}",
                defaults: new { controller = "Accounts", action = "UpdateUserBankAccount" });

            app.MapControllerRoute(
                name: "VendorAccountsTransaction",
                pattern: "{langCode}/accounts/vendor-account-transactions/{VendorId}",
                defaults: new { controller = "Accounts", action = "VendorAccountsTransaction" });

            app.MapControllerRoute(
                name: "CreateNewDiscount",
                pattern: "{langCode}/discounts/create-new-discount",
                defaults: new { controller = "Discounts", action = "CreateNewDiscount" });

            app.MapControllerRoute(
                name: "UpdateDiscount",
                pattern: "{langCode}/discounts/update-discount/{DiscountId}",
                defaults: new { controller = "Discounts", action = "UpdateDiscount" });

            app.MapControllerRoute(
                name: "MenuLocalization",
                pattern: "{langCode}/configuration/menu-localization",
                defaults: new { controller = "Configuration", action = "MenuLocalization" });

            app.MapControllerRoute(
                name: "MenuLocalizationDetail",
                pattern: "{langCode}/configuration/menu-localizatino-detail/{MenuNavigationId}",
                defaults: new { controller = "Configuration", action = "MenuLocalizationDetail" });

            app.MapControllerRoute(
                name: "SitesLogo",
                pattern: "{langCode}/configuration/screens-localization",
                defaults: new { controller = "Configuration", action = "ScreensLocalization" });

            #endregion

            #region Admin Panel Web Routes
            app.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");
            #endregion

            #region API Routes
            //app.MapControllerRoute(
            //     name: "V1",
            //     pattern: "{area:exists}/api/dynamic/dataoperation/{UrlName?}",
            //     new { action = "DataOperation", controller = "ApiDynamic" }
            //   );
            #endregion


        }
    }
}
