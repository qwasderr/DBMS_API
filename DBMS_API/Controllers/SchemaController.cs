
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DBMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchemaController : ControllerBase
    {
        // GET: api/schema
        [HttpGet]
        public IActionResult GetSchemas()
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return NotFound("No database loaded.");

            var schemas = DatabaseInstance.CurrentDatabase.Schemas.ToList();
            return Ok(schemas);
        }

        // GET: api/schema/{tableName}
        [HttpGet("{tableName}")]
        public IActionResult GetSchema(string tableName)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return NotFound("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            return Ok(table.Schema);
        }

        // POST: api/schema/add
        [HttpPost("add")]
        public IActionResult AddSchema([FromBody] Schema newSchema)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            if (DatabaseInstance.CurrentDatabase.Tables.Any(t => t.Schema.Name == newSchema.Name))
                return BadRequest($"Schema with name '{newSchema.Name}' already exists.");

            //var newTable = new Table { Schema = newSchema };
            //DatabaseInstance.CurrentDatabase.Tables.Add(newTable);
            DatabaseInstance.CurrentDatabase.Schemas.Add(newSchema);

            return Ok($"Schema '{newSchema.Name}' added.");
        }

        // DELETE: api/schema/{schemaName}
        [HttpDelete("{schemaName}")]
        public IActionResult DeleteSchema(string schemaName)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var schema = DatabaseInstance.CurrentDatabase.Schemas.FirstOrDefault(s => s.Name == schemaName);

            if (schema == null)
                return NotFound($"Schema '{schemaName}' not found.");

            DatabaseInstance.CurrentDatabase.Schemas.Remove(schema);

            return Ok($"Schema '{schemaName}' has been deleted.");
        }
    }
}
