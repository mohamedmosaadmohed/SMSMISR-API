using System.Data.SqlClient;
using System.Data;
using SOfCO.Helpers;

namespace Task.Models
{
    public class SuccessfullyMessage
    {
            private readonly SQL _sql;

            public int Id { get; set; }
            public string MobileNumbers { get; set; }
            public string Message { get; set; }
            public string Language { get; set; }
            public decimal Cost { get; set; }
            public DateTime SentDateTime { get; set; }
        public SuccessfullyMessage(string mobileNumbers, string message, string language, decimal cost, DateTime sentDateTime)
        {
            MobileNumbers = string.Join(",", mobileNumbers);
            Message = message;
            Language = language;
            Cost = cost;
            SentDateTime = sentDateTime;
            string conection = "Data Source=MOHAMED;Initial Catalog=SMS;Integrated Security=True";
            _sql = new SQL(conection);
        }

        public async ValueTask InsertIntoDatabaseAsync()
        {
            // Call the stored procedure to insert the SMS message into the database
            var cmd = new SqlCommand("InsertSmsMessage");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MobileNumbers", MobileNumbers);
            cmd.Parameters.AddWithValue("@Message", Message);
            cmd.Parameters.AddWithValue("@Language", Language);
            cmd.Parameters.AddWithValue("@Cost", Cost);
            cmd.Parameters.AddWithValue("@SentDateTime", SentDateTime);
            await _sql.ExecuteNonQueryAsync(cmd);
        }
    }
}
