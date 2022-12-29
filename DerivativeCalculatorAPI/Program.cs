using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "Differentiator API",
		Description = "An ASP.NET Core Web API for differentiating"
		//TermsOfService = new Uri("https://example.com/terms"),
		//Contact = new OpenApiContact
		//{
		//	Name = "Example Contact",
		//	Url = new Uri("https://example.com/contact")
		//},
		//License = new OpenApiLicense
		//{
		//	Name = "Example License",
		//	Url = new Uri("https://example.com/license")
		//}
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger(options =>
{
	options.SerializeAsV2 = false;
});
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	options.RoutePrefix = string.Empty;
});
//}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
