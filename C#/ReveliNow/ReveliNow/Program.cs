using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Bind configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? "PLEASE_REPLACE_WITH_A_LONG_RANDOM_SECRET_KEY_>=_64_chars";
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "ReveliNowAPI";
var jwtAudience = jwtSection.GetValue<string>("Audience") ?? "ReveliNowClients";
var jwtExpiresMinutes = jwtSection.GetValue<int?>("ExpiresMinutes") ?? 120;

// JWT Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dev only
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Simple IDbConnection factory using Dapper
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? "Server=localhost;Database=revalinow;Trusted_Connection=True;TrustServerCertificate=True;";
    return new SqlConnection(cs);
});

// Swagger/OpenAPI configuratie
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ReveliNow API",
        Version = "v1",
        Description = "Uitgebreide OpenAPI documentatie voor de ReveliNow API.",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@revelinow.local",
            Url = new System.Uri("https://localhost/4243/swagger/index.html")
        }
    });
    c.EnableAnnotations();

    // JWT support in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Voer 'Bearer {token}' in",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new List<string>() }
    });
});

var app = builder.Build();

// Altijd Swagger aanzetten
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReveliNow API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
    c.EnableFilter();
    c.DefaultModelsExpandDepth(2);
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

//  public class Appointment
//  {
//      public string Name { get; set; }
//      public DateTime Date { get; set; }
//  
//      public Appointment(string name, DateTime date)
// {
//          Name = name;
//          Date = date;
//      }
//  }
//  
//  public class AppointmentManager
//     {
//      private List<Appointment> appointments = new List<Appointment>();
//  
//      public AppointmentManager()
//         {
                 
//      }
//
//      public List<Appointment> GetAppointments()
//         {
//             appointments = appointments.OrderBy(a => a.Date).ThenBy(a => a.Name).ToList();
//             return appointments;
//         }
//  }
//
//  class Program
//  {
//      static void Main(string[] args)
//      {
//          var manager = new AppointmentManager();
//          var appointments = manager.GetAppointments();
//          appointments.ForEach(appointment => Console.WriteLine(appointment.Name + " - " + appointment.Date));
//     }
//      
//      
// }
