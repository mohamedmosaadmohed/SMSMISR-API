using Google.Apis.Admin.Directory.directory_v1.Data;

namespace Task.Models
{
    public class SmsMisrProperties
    {

        public string Username { get; set; }
        public string SenderID { get; set; }
        public string Password { get; set; }

         
        public SmsMisrProperties(string username, string senderID, string password)
        {
            Username = username;
            SenderID = senderID;
            Password = password;
        }
    }
}

