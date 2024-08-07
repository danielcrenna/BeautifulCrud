using System.Runtime.CompilerServices;
using BeautifulCrud;
using BeautifulCrud.AspNetCore;

[assembly: InternalsVisibleTo("BeautifulCrud.IntegrationTests")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBeautifulCrud(o =>
{
    o.Features = Features.Controllers | Features.OpenApi;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
	
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseBeautifulCrud();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program; // this is here to support in-memory API testing