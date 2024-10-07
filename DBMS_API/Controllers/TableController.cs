
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DBMS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        // GET: api/table
        [HttpGet]
        public IActionResult GetTables()
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return NotFound("No database loaded.");

            return Ok(DatabaseInstance.CurrentDatabase.Tables);
        }

        // GET: api/table/{tableName}
        [HttpGet("{tableName}")]
        public IActionResult GetTable(string tableName)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return NotFound("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            return Ok(table);
        }

        // POST: api/table
        [HttpPost]
        public IActionResult CreateTable([FromBody] Table newTable)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");
            if (DatabaseInstance.CurrentDatabase.Tables.Any(r => r.Name == newTable.Name))
            {
                return BadRequest("Table with this name already exists");
            }
            ValidateTable(newTable);
            DatabaseInstance.CurrentDatabase.Tables.Add(newTable);
            return Ok($"Table '{newTable.Name}' created.");
        }

        // POST: api/table/create
        [HttpPost("create")]
        public IActionResult CreateTableWithSchema([FromBody] TableCreationRequest request)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            if (DatabaseInstance.CurrentDatabase.Tables.Any(t => t.Name == request.TableName))
                return BadRequest("Table with this name already exists.");

            var schema = DatabaseInstance.CurrentDatabase.Schemas.FirstOrDefault(s => s.Name == request.SchemaName);
            if (schema == null)
                return NotFound($"Schema '{request.SchemaName}' not found.");

            var newTable = new Table(request.TableName, schema);

            ValidateTable(newTable);

            DatabaseInstance.CurrentDatabase.Tables.Add(newTable);

            return Ok($"Table '{request.TableName}' created with schema '{request.SchemaName}'.");
        }
        public class TableCreationRequest
        {
            public string TableName { get; set; }
            public string SchemaName { get; set; }
        }

        // PUT: api/table/{tableName}
        [HttpPut("{tableName}")]
        public IActionResult UpdateTable(string tableName, [FromBody] Table updatedTable)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            ValidateTable(table);

            int index = DatabaseInstance.CurrentDatabase.Tables.IndexOf(table);
            DatabaseInstance.CurrentDatabase.Tables[index] = updatedTable;

            return Ok($"Table '{tableName}' updated.");
        }

        // DELETE: api/table/{tableName}
        [HttpDelete("{tableName}")]
        public IActionResult DeleteTable(string tableName)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            DatabaseInstance.CurrentDatabase.Tables.Remove(table);
            return Ok($"Table '{tableName}' deleted.");
        }

        [HttpPost]
        [Route("api/table/ValidateTable")]
        public IActionResult ValidateTable([FromBody] Table table)
        {
            try
            {
                if (table.ValidateData())
                {
                    return Ok("Table data is valid.");
                }
                else
                {
                    return BadRequest("Table data is invalid.");
                }
            }
            catch (FormatValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (TimeValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An unexpected error occurred: {ex.Message}" });
            }
        }

        // POST: api/table/difference
        [HttpPost("difference")]
        public IActionResult GetTableDifference([FromBody] TableDifferenceRequest request)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table1 = DatabaseInstance.CurrentDatabase.GetTable(request.TableName1);
            var table2 = DatabaseInstance.CurrentDatabase.GetTable(request.TableName2);

            if (table1 == null)
                return NotFound($"Table '{request.TableName1}' not found.");
            if (table2 == null)
                return NotFound($"Table '{request.TableName2}' not found.");

            try
            {
                var differenceTable = table1.Difference(table2);
                return Ok(differenceTable);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error while computing difference: {ex.Message}");
            }
        }
        public class TableDifferenceRequest
        {
            public string TableName1 { get; set; }
            public string TableName2 { get; set; }
        }
    }
}
