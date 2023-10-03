using AdminPanel.Helpers;


var builder = WebApplication.CreateBuilder(args);
//--register builder.Services
ServiceExtensions.ConfigureIServiceCollection(builder);

var app = builder.Build();
ServiceExtensions.ConfigureWebApplication(app);
app.Run();









