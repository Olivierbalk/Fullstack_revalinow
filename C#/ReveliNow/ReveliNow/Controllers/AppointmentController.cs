using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ReveliNow.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppointmentController : ControllerBase
    {
        private static readonly List<Appointment> Appointments = new List<Appointment>
        {
            new Appointment { Id = 1, Name = "Fysioafspraak", Date = DateTime.Now.AddDays(1) },
            new Appointment { Id = 2, Name = "Doktersafspraak", Date = DateTime.Now.AddDays(2) },
            new Appointment { Id = 3, Name = "Fysioafspraak", Date = new DateTime(2023, 11, 1, 10, 0, 0) },
            new Appointment { Id = 4, Name = "Doktersafspraak", Date = new DateTime(2023, 11, 2, 14, 30, 0) },
            new Appointment { Id = 5, Name = "Revalidatieafspraak", Date = new DateTime(2023, 11, 1, 9, 0, 0) },
            new Appointment { Id = 6, Name = "Fysioafspraak", Date = new DateTime(2023, 11, 4, 11, 15, 0) },
            new Appointment { Id = 7, Name = "Doktersafspraak", Date = new DateTime(2023, 11, 5, 16, 45, 0) },
            new Appointment { Id = 8, Name = "Fysioafspraak", Date = new DateTime(2022, 11, 6, 8, 30, 0) },
            new Appointment { Id = 9, Name = "Revalidatieafspraak", Date = new DateTime(2021, 11, 7, 13, 0, 0) },
            new Appointment { Id = 10, Name = "Doktersafspraak", Date = new DateTime(2024, 11, 8, 15, 15, 0) },
            new Appointment { Id = 11, Name = "Fysioafspraak", Date = new DateTime(2025, 11, 9, 10, 45, 0) },
            new Appointment { Id = 12, Name = "Revalidatieafspraak", Date = new DateTime(2020, 11, 10, 9, 30, 0) },
            new Appointment { Id = 13, Name = "Fysioafspraak", Date = new DateTime(2026, 11, 11, 11, 0, 0) },
            new Appointment { Id = 14, Name = "Doktersafspraak", Date = new DateTime(2027, 11, 12, 14, 0, 0) },
            new Appointment { Id = 15, Name = "Revalidatieafspraak", Date = new DateTime(2028, 11, 13, 16, 30, 0) },
            new Appointment { Id = 16, Name = "Fysioafspraak", Date = new DateTime(2029, 11, 14, 8, 0, 0) },
            new Appointment { Id = 17, Name = "Doktersafspraak", Date = new DateTime(2030, 11, 15, 13, 45, 0) },
            new Appointment { Id = 18, Name = "Revalidatieafspraak", Date = new DateTime(2031, 11, 16, 10, 15, 0) },
            new Appointment { Id = 19, Name = "Fysioafspraak", Date = new DateTime(2032, 11, 17, 9, 0, 0) },
            new Appointment { Id = 20, Name = "Doktersafspraak", Date = new DateTime(2033, 11, 18, 15, 30, 0) },
            new Appointment { Id = 21, Name = "Revalidatieafspraak", Date = new DateTime(2034, 11, 19, 11, 45, 0) },
            new Appointment { Id = 22, Name = "Fysioafspraak", Date = new DateTime(2035, 11, 20, 10, 0, 0) },
            new Appointment { Id = 23, Name = "Doktersafspraak", Date = new DateTime(2036, 11, 21, 14, 30, 0) },
            new Appointment { Id = 24, Name = "Revalidatieafspraak", Date = new DateTime(2037, 11, 22, 9, 0, 0) },
            new Appointment { Id = 25, Name = "Fysioafspraak", Date = new DateTime(2038, 11, 23, 11, 15, 0) },
            new Appointment { Id = 26, Name = "Doktersafspraak", Date = new DateTime(2039, 11, 24, 16, 45, 0) },
            new Appointment { Id = 27, Name = "Revalidatieafspraak", Date = new DateTime(2040, 11, 25, 8, 30, 0) }
        };
        [HttpGet]
        [SwaggerOperation(Summary = "Haal alle afspraken op (gesorteerd op datum).")]
        public ActionResult<IEnumerable<Appointment>> GetAll() => Ok(Appointments.OrderBy(a => a.Date));

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Haal een afspraak op via ID.")]
        public ActionResult<Appointment> Get(int id)
        {
            var appt = Appointments.Find(a => a.Id == id);
            return appt == null ? NotFound() : Ok(appt);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Voeg een nieuwe afspraak toe.")]
        public ActionResult<Appointment> Post([FromBody] Appointment appointment)
        {
            appointment.Id = Appointments.Count + 1;
            Appointments.Add(appointment);
            return CreatedAtAction(nameof(Get), new { id = appointment.Id }, appointment);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Wijzig een bestaande afspraak.")]
        public IActionResult Put(int id, [FromBody] Appointment appointment)
        {
            var appt = Appointments.Find(a => a.Id == id);
            if (appt == null) return NotFound();
            appt.Name = appointment.Name;
            appt.Date = appointment.Date;
            return NoContent();
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Verwijder een afspraak.")]
        public IActionResult Delete(int id)
        {
            var appt = Appointments.Find(a => a.Id == id);
            if (appt == null) return NotFound();
            Appointments.Remove(appt);
            return NoContent();
        }
    }

    public class Appointment
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; } // Nullable voor warning
        [Required]
        public DateTime Date { get; set; }
    }
}
