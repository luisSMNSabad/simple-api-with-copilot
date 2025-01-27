var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseMiddleware<UserApi.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<UserApi.Middleware.RequestResponseLoggingMiddleware>();
app.UseMiddleware<UserApi.Middleware.AuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();