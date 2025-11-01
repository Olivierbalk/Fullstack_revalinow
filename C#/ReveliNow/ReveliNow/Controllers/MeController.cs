using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;

namespace ReveliNow.Controllers
{
    [ApiController]
    [Route("me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly IDbConnection _db;
        public MeController(IDbConnection db) => _db = db;

        private int? GetPatientIdFromToken()
        {
            var claim = User.FindFirst("patient_id");
            if (claim == null) return null;
            if (int.TryParse(claim.Value, out var pid)) return pid;
            return null;
        }

        private string? GetUsername() => User.Identity?.Name;

        private void SetRlsContext()
        {
            var username = GetUsername();
            if (string.IsNullOrEmpty(username)) return;
            _db.Execute("EXEC sys.sp_set_session_context @key, @value, @read_only", new { key = "app.user", value = username, read_only = false });
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Identiteit van de ingelogde gebruiker")] 
        public IActionResult WhoAmI()
        {
            var pid = GetPatientIdFromToken();
            return Ok(new { username = GetUsername(), patientId = pid, roles = User.Claims.Where(c => c.Type.EndsWith("/role") || c.Type == "role").Select(c => c.Value).ToArray() });
        }

        // Each endpoint fetches only rows for current patient. RLS in DB also enforces this at the DB layer.
        [HttpGet("patient")]
        [SwaggerOperation(Summary = "Mijn patientprofiel")] 
        public IActionResult GetPatient()
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Patienten WHERE PatientID = @pid";
            var rows = _db.Query(sql, new { pid });
            return Ok(rows);
        }

        [HttpGet("intakes")] 
        public IActionResult GetIntakes([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Intakegesprekken WHERE PatientID = @pid" +
                      (from.HasValue ? " AND DatumIntake >= @from" : "") +
                      (to.HasValue ? " AND DatumIntake <= @to" : "") +
                      " ORDER BY DatumIntake DESC";
            var rows = _db.Query(sql, new { pid, from, to });
            return Ok(rows);
        }

        [HttpGet("intakes/{id:int}")]
        public IActionResult GetIntakeById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Intakegesprekken WHERE IntakeID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("afspraken")] 
        public IActionResult GetAfspraken([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? status = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Afspraken WHERE PatientID = @pid" +
                      (from.HasValue ? " AND DatumTijdAfspraak >= @from" : "") +
                      (to.HasValue ? " AND DatumTijdAfspraak <= @to" : "") +
                      (!string.IsNullOrWhiteSpace(status) ? " AND Status = @status" : "") +
                      " ORDER BY DatumTijdAfspraak DESC";
            var rows = _db.Query(sql, new { pid, from, to, status });
            return Ok(rows);
        }

        [HttpGet("afspraken/{id:int}")]
        public IActionResult GetAfspraakById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Afspraken WHERE AfspraakID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("medicatie")] 
        public IActionResult GetMedicatie([FromQuery] string? status = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Medicatie WHERE PatientID = @pid" +
                      (!string.IsNullOrWhiteSpace(status) ? " AND Status = @status" : "") +
                      " ORDER BY StartDatum DESC";
            var rows = _db.Query(sql, new { pid, status });
            return Ok(rows);
        }

        [HttpGet("medicatie/{id:int}")] 
        public IActionResult GetMedicatieById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Medicatie WHERE MedicatieID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("notities")] 
        public IActionResult GetNotities([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Notities WHERE PatientID = @pid" +
                      (from.HasValue ? " AND DatumTijdNotitie >= @from" : "") +
                      (to.HasValue ? " AND DatumTijdNotitie <= @to" : "") +
                      " ORDER BY DatumTijdNotitie DESC";
            var rows = _db.Query(sql, new { pid, from, to });
            return Ok(rows);
        }

        [HttpGet("notities/{id:int}")] 
        public IActionResult GetNotitieById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Notities WHERE NotitieID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("oefenplannen")] 
        public IActionResult GetOefenplannen([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Oefenplannen WHERE PatientID = @pid" +
                      (from.HasValue ? " AND StartDatum >= @from" : "") +
                      (to.HasValue ? " AND EindDatum <= @to" : "") +
                      " ORDER BY StartDatum DESC";
            var rows = _db.Query(sql, new { pid, from, to });
            return Ok(rows);
        }

        [HttpGet("oefenplannen/{id:int}")] 
        public IActionResult GetOefenplanById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Oefenplannen WHERE PatientOefenplanID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("uitgevoerde-oefeningen")] 
        public IActionResult GetUitgevoerdeOefeningen([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var rows = _db.Query(@"SELECT u.*
                                   FROM Uitgevoerde_oefeningen u
                                   JOIN Oefenplannen p ON p.PatientOefenplanID = u.PatientOefenplanID
                                   WHERE p.PatientID = @pid" +
                                   (from.HasValue ? " AND u.DatumTijdAfgevinkt >= @from" : "") +
                                   (to.HasValue ? " AND u.DatumTijdAfgevinkt <= @to" : "") +
                                   " ORDER BY u.DatumTijdAfgevinkt DESC", new { pid, from, to });
            return Ok(rows);
        }

        [HttpGet("uitgevoerde-oefeningen/{id:int}")] 
        public IActionResult GetUitgevoerdeOefeningById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault(@"SELECT TOP 1 u.*
                                                FROM Uitgevoerde_oefeningen u
                                                JOIN Oefenplannen p ON p.PatientOefenplanID = u.PatientOefenplanID
                                                WHERE u.UitgevoerdeOefeningID = @id AND p.PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("activiteiten")] 
        public IActionResult GetActiviteiten([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Activiteiten_logboek WHERE PatientID = @pid" +
                      (from.HasValue ? " AND DatumTijdActiviteit >= @from" : "") +
                      (to.HasValue ? " AND DatumTijdActiviteit <= @to" : "") +
                      " ORDER BY DatumTijdActiviteit DESC";
            var rows = _db.Query(sql, new { pid, from, to });
            return Ok(rows);
        }

        [HttpGet("activiteiten/{id:int}")] 
        public IActionResult GetActiviteitById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Activiteiten_logboek WHERE LogboekID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("accessoires")] 
        public IActionResult GetAccessoires([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? status = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Accessoires WHERE PatientID = @pid" +
                      (from.HasValue ? " AND AdviesDatum >= @from" : "") +
                      (to.HasValue ? " AND AdviesDatum <= @to" : "") +
                      (!string.IsNullOrWhiteSpace(status) ? " AND Status = @status" : "") +
                      " ORDER BY AdviesDatum DESC";
            var rows = _db.Query(sql, new { pid, from, to, status });
            return Ok(rows);
        }

        [HttpGet("accessoires/{id:int}")] 
        public IActionResult GetAccessoireById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Accessoires WHERE AccessoireID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("pijnindicaties")] 
        public IActionResult GetPijnindicaties([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Pijnindicaties WHERE PatientID = @pid" +
                      (from.HasValue ? " AND DatumTijdRegistratie >= @from" : "") +
                      (to.HasValue ? " AND DatumTijdRegistratie <= @to" : "") +
                      " ORDER BY DatumTijdRegistratie DESC";
            var rows = _db.Query(sql, new { pid, from, to });
            return Ok(rows);
        }

        [HttpGet("pijnindicaties/{id:int}")] 
        public IActionResult GetPijnindicatieById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Pijnindicaties WHERE PijnIndicatieID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("declaraties")] 
        public IActionResult GetDeclaraties([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? status = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var sql = "SELECT * FROM Declaraties WHERE PatientID = @pid" +
                      (from.HasValue ? " AND DatumHandeling >= @from" : "") +
                      (to.HasValue ? " AND DatumHandeling <= @to" : "") +
                      (!string.IsNullOrWhiteSpace(status) ? " AND Status = @status" : "") +
                      " ORDER BY DatumHandeling DESC";
            var rows = _db.Query(sql, new { pid, from, to, status });
            return Ok(rows);
        }

        [HttpGet("declaraties/{id:int}")] 
        public IActionResult GetDeclaratieById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault("SELECT TOP 1 * FROM Declaraties WHERE DeclaratieID = @id AND PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }

        [HttpGet("declaratieverwerkingen")] 
        public IActionResult GetDeclaratieverwerkingen([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? status = null)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var rows = _db.Query(@"SELECT v.*
                                   FROM Declaratieverwerkingen v
                                   JOIN Declaraties d ON d.DeclaratieID = v.DeclaratieID
                                   WHERE d.PatientID = @pid" +
                                   (from.HasValue ? " AND v.DatumVerwerking >= @from" : "") +
                                   (to.HasValue ? " AND v.DatumVerwerking <= @to" : "") +
                                   (!string.IsNullOrWhiteSpace(status) ? " AND v.StatusVerwerking = @status" : "") +
                                   " ORDER BY v.DatumVerwerking DESC", new { pid, from, to, status });
            return Ok(rows);
        }

        [HttpGet("declaratieverwerkingen/{id:int}")] 
        public IActionResult GetDeclaratieverwerkingById(int id)
        {
            var pid = GetPatientIdFromToken();
            if (pid == null) return Forbid();
            SetRlsContext();
            var row = _db.QueryFirstOrDefault(@"SELECT TOP 1 v.*
                                                FROM Declaratieverwerkingen v
                                                JOIN Declaraties d ON d.DeclaratieID = v.DeclaratieID
                                                WHERE v.VerwerkingID = @id AND d.PatientID = @pid", new { id, pid });
            return row == null ? NotFound() : Ok(row);
        }
    }
}

