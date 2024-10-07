
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DBMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Databases");

        public DatabaseController()
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        // GET: api/database
        [HttpGet]
        public IActionResult GetDatabase()
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return NotFound("No database loaded.");

            return Ok(DatabaseInstance.CurrentDatabase);
        }
        [HttpPost("Create")]
        public IActionResult CreateDatabase([FromBody] DatabaseCreationRequest request)
        {
            if (string.IsNullOrEmpty(request.DatabaseName))
            {
                return BadRequest("Database name cannot be null or empty.");
            }

            if (DatabaseInstance.CurrentDatabase != null && DatabaseInstance.CurrentDatabase.Name == request.DatabaseName)
            {
                    return Conflict($"A database with the name '{request.DatabaseName}' already exists.");   
            }
            if (System.IO.File.Exists(Path.Combine(_storagePath, $"{request.DatabaseName}.json")))
            {
                return Conflict($"A database with the name '{request.DatabaseName}' already exists.");
            }

            Database newDatabase = new Database
            {
                Name = request.DatabaseName,
                Schemas = new List<Schema>()
            };
            //_databases.Add(newDatabase);
            DatabaseInstance.CurrentDatabase = newDatabase;

            try
            {
                string filePath = Path.Combine(_storagePath, $"{request.DatabaseName}.json");
                string jsonContent = JsonSerializer.Serialize(newDatabase, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, jsonContent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving the database to disk: {ex.Message}");
            }

            return Ok($"Database '{newDatabase.Name}' created and saved successfully.");
        }

        // POST: api/database/load
        [HttpPost("load")]
        public IActionResult LoadDatabase([FromBody] string filePath)
        {
            DatabaseInstance.CurrentDatabase = new Database();
            DatabaseInstance.CurrentDatabase.LoadFromDisk(filePath);
            return Ok("Database loaded.");
        }

        // POST: api/database/save
        [HttpPost("save")]
        public IActionResult SaveDatabase([FromBody] string filePath)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database to save.");

            DatabaseInstance.CurrentDatabase.SaveToDisk(filePath);
            return Ok("Database saved.");
        }
    }
    public class DatabaseCreationRequest
    {
        public string DatabaseName { get; set; }
    }
}
