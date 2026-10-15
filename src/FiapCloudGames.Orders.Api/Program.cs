var builder = WebApplication.CreateBuilder(args);

builder.AddFiapServiceDefaults();
builder.Services.AddFiapApiDefaults();
builder.Services.AddOrdersnfrastructure(builder.Configuration);
builder.Services.AddOrdersPresentation();
builder.Services.AddOrdersOpenApi();

//TODO: verificar o building blocks.
//builder.Services.AddFiapServiceAuthentication(builder.Configuration);

var app = builder.Build();

app.UseFiapApiDefaults();
app.UseOrdersOpenApi();
app.UseAuthentication();
app.UseAuthorization();

app.MapFiapHealthEndpoints();
app.MapControllers();

await app.RunAsync();

public partial class Program;
