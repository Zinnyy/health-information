using health_information.Models;
using health_information.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext<HealthDbContext>(options =>

  //  options.UseSqlServer(builder.Configuration.GetConnectionString("HealthDb")));

builder.Services.AddDbContext<HealthDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("HealthDb"), npgsqlOptions => npgsqlOptions.CommandTimeout(180))
    );
            

//options.UseNpgsql(builder.Configuration.GetConnectionString

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));
builder.Services.AddScoped(typeof(IHealthReposiotory<>), typeof(HealthRepository<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IVitalRecordService, VitalRecordService>();
builder.Services.AddScoped<IAdminService, AdminService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            builder.Configuration["UserSecret"] ?? throw new InvalidOperationException("UserSecret key is missing."))),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero // Instantly expires token when time ends
    };
});

builder.Services.AddControllers();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Replace with your React app's URL
              .AllowAnyHeader()
              .AllowAnyMethod()
               .AllowCredentials();
       
    });
});


builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the bearer scheme. Enter Bearer [space] then add your token in the text input. Example: Bearer jwttoken4839nei9",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            // ✅ Use the new Reference class instead of the old Reference property
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});
/*builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the bearer scheme. Enter Bearer [space] then add your token in the text input. Example: Bearer jwttoken4839nei9",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            // ✅ Use the new Reference class instead of the old Reference property
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger";
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactCors");
app.UseRouting();
app.UseStaticFiles();

/*app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"C:\Users\PC\source\repos\health information\wwwroot\charts"),
    RequestPath = "/charts"
});*/
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
