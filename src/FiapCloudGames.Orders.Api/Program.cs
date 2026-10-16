var builder = WebApplication.CreateBuilder(args);

builder.AddFiapServiceDefaults();
builder.Services.AddFiapApiDefaults();
builder.Services.AddFiapServiceAuthentication(builder.Configuration);
builder.Services.AddOrdersInfrastructure(builder.Configuration);
builder.Services.AddOrdersPresentation();
builder.Services.AddOrdersOpenApi();

var app = builder.Build();

app.UseFiapApiDefaults();
app.UseOrdersOpenApi();
app.UseAuthentication();
app.UseAuthorization();

app.MapFiapHealthEndpoints();
app.MapControllers();

await app.RunAsync();

public partial class Program
{
    // Fornece um construtor de instância protegido para que a classe não seja considerada "apenas estática",
    // mas ainda assim não possa ser instanciada publicamente (útil para testes de integração que expõem a classe Program).
    protected Program() { }
}
