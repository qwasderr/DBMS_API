
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DBMS_API.Controllers
{
    [ApiController]
    [Route("api/table/{tableName}/[controller]")]
    public class RowController : ControllerBase
    {
        // GET: api/table/{tableName}/row
        [HttpGet]
        public IActionResult GetRows(string tableName)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            return Ok(table.Rows);
        }

        // GET: api/table/{tableName}/row/{rowId}
        [HttpGet("{rowId}")]
        public IActionResult GetRow(string tableName, int rowId)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            var row = table.Rows.FirstOrDefault(r => r.Values[0].ToInt() == rowId);
            if (row == null)
                return NotFound($"Row with ID '{rowId}' not found in table '{tableName}'.");

            return Ok(row);
        }


        [HttpPost]
        public IActionResult AddRow(string tableName, [FromBody] Row newRow)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            int newId = table._nextId;
            table._nextId++;

            Value v = new Value(newId);
            newRow.Values.Insert(0,v);

            table.Rows.Add(newRow);
            return Ok($"Row with ID '{newId}' added to table '{tableName}'.");
        }

        [HttpPut("{rowId}")]
        public IActionResult UpdateRow(string tableName, int rowId, [FromBody] Row updatedRow)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            var row = table.Rows.FirstOrDefault(r => r.Values[0].ToInt() == rowId);
            if (row == null)
                return NotFound($"Row with ID '{rowId}' not found in table '{tableName}'.");

            updatedRow.Values.Insert(0,row.Values[0]);
            int index = table.Rows.IndexOf(row);
            table.Rows[index] = updatedRow;

            return Ok($"Row '{rowId}' in table '{tableName}' updated.");
        }

        // DELETE: api/table/{tableName}/row/{rowId}
        [HttpDelete("{rowId}")]
        public IActionResult DeleteRow(string tableName, int rowId)
        {
            if (DatabaseInstance.CurrentDatabase == null)
                return BadRequest("No database loaded.");

            var table = DatabaseInstance.CurrentDatabase.GetTable(tableName);
            if (table == null)
                return NotFound($"Table '{tableName}' not found.");

            var row = table.Rows.FirstOrDefault(r => r.Values[0].ToInt() == rowId);
            if (row == null)
                return NotFound($"Row with ID '{rowId}' not found in table '{tableName}'.");

            table.Rows.Remove(row);
            return Ok($"Row '{rowId}' deleted from table '{tableName}'.");
        }
    }
}
