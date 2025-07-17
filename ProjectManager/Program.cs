using DB.Models;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services.Services;

namespace ProjectManager
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      builder.Services.AddControllers();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();

      builder.Services.AddSwaggerGen(opt =>
      {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectManager API" });

        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
          In = ParameterLocation.Header,
          Description = "Please enter token",
          Name = "Authorization",
          Type = SecuritySchemeType.Http,
          BearerFormat = "JWT",
          Scheme = "bearer"
        });

        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
            Type=ReferenceType.SecurityScheme,
            Id="Bearer"
        }
      },
      new string[]{}
    }
  });
      });

      builder.Services.AddSwaggerGen();

      builder.Services.AddAutoMapper(typeof(MapperConfig).Assembly);

      builder.Services.AddDbContext<ProjectContext>(options =>
      options.UseSqlServer("Server=(local)\\DYNAMTECH;Database=ProjectManager;Trusted_Connection=True;TrustServerCertificate=True")); //TODO: Make env var

      builder.Services.AddScoped<CognitoService>();
      builder.Services.AddScoped<ProjectsService>();
      builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      var jwtSettings = builder.Configuration.GetSection("Jwt");

      //var region = builder.Configuration["Cognito:Region"];
      //var userPoolId = builder.Configuration["Cognito:UserPoolId"];

      //options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
      };
    });

      var app = builder.Build();

      app.UseAuthentication();
      app.UseAuthorization();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();

      app.UseAuthorization();


      app.MapControllers();

      app.Run();
    }
  }
}
