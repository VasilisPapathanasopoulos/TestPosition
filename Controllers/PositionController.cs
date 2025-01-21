using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TestAPI.Models;

namespace TestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PositionController(context)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetPositions()
        {
            string query = "SELECT pos_name AS Name, pos_lat AS Lat, pos_lon AS Lon FROM positions ORDER BY pos_name";
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);
                }
            }

            return Ok(dataTable);
        }

        [HttpPost]
        public IActionResult AddPosition([FromBody] Position position)
        {
            string query = "INSERT INTO positions (pos_name, pos_lat, pos_lon) VALUES (@Name, @Lat, @Lon)";

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", position.Name);
                    cmd.Parameters.AddWithValue("@Lat", position.Lat);
                    cmd.Parameters.AddWithValue("@Lon", position.Lon);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex) when (ex.Number == 2627) // Handle duplicate key error
                    {
                        return Conflict("Position with the same name already exists.");
                    }
                }
            }

            return Ok("Position added successfully.");
        }
    }
}
