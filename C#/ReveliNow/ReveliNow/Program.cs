using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
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
    c.EnableAnnotations(); // Nu werkt dit, want de package is geïnstalleerd
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
