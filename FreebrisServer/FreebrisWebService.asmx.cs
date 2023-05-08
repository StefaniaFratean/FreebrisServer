using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services;

namespace FreebrisServer
{
    /// <summary>
    /// Summary description for FreebrisWebService
    /// </summary>
    /// 
    [WebService(Description ="Serviciu Web pentru proiectul semestrial Freebris, materie II", Name = "FreebrisWebService", Namespace = "FreebrisServer")]
    //[WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class FreebrisWebService : System.Web.Services.WebService
    {

        string result;
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string GetAllUsersNames()
        {
            try
            {
                SqlConnection connection = new SqlConnection("ConnectionService");
                SqlCommand cmd = new SqlCommand("SELECT name FROM Users", connection);
                connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    result = result + dr[0].ToString();
                }

            }
            catch (Exception ex)
            {
                return "" + ex;
            }
            return result;
        }

        [WebMethod]
        public bool CheckPassword(string username, string password)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT password FROM Users WHERE username = \'" + username + "\'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            byte[] hashedPassword = {};

            string pass = "";
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    pass = dr.GetString(0);
                    pass = pass.Trim();
                    var sha = SHA256.Create();
                    var asByteArray = Encoding.Default.GetBytes(password);
                    hashedPassword = sha.ComputeHash(asByteArray);
                }
            }

            if (pass.Equals(Convert.ToBase64String(hashedPassword)))
            {
                return true;
            }
            return false;
        }

        [WebMethod]
        public bool ChangePassword(string username, string password)
        {
            SqlConnection connection = new SqlConnection();

            //SqlCommand cmd = new SqlCommand("update password FROM Users WHERE username = \'" + username + "\'", connection);
            SqlCommand cmd = new SqlCommand("update Users set password=@password WHERE username = \'" + username + "\'", connection);

            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();

            var sha = SHA256.Create();
            var asByteArray = Encoding.Default.GetBytes(password);
            var hashedPassword = sha.ComputeHash(asByteArray);
            cmd.Parameters.AddWithValue("@password", Convert.ToBase64String(hashedPassword));

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                connection.Close();
                return false;
            }
            connection.Close();
            return true;
        }

        [WebMethod]
        public void SendEmail(string email, string subject, string text)
        {
            string fromMail = "ahs.sarah.2002@gmail.com";
            string fromPassword = "hjbxeikvbuxbdfpd";

           MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = subject;
            message.To.Add(new MailAddress(email));
            message.Body = text;
            message.IsBodyHtml = false;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            smtpClient.Send(message);
        }

        private int GetId(string tableName)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM " + tableName, connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            int idNr = 0;
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    idNr = dr.GetInt32(0);
                    //pass = pass.Trim();
                }
            }
            return idNr + 1;
        }

        public void AddUserToDB(int id, string username, string password, string email)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("INSERT INTO Users VALUES ('" + id + "', '" + username + "', '" + password + "', '" + 0  + "', '" + "admin', '" + 1 + "', '" + email + "')", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            cmd.ExecuteReader();
        }

        [WebMethod]
        public bool CreateUser(string username, string password, string email)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT username FROM Users WHERE username = \'" + username + "\'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            byte[] hashedPassword;
            if (dr.HasRows)
            {
                //while (dr.Read())
                //{
                //    int test = dr.GetInt32(0);
                //}
                return false;
            }
            else
            {
                int id = GetId("Users");
                //string email = username + "@gmail.com";
                var sha = SHA256.Create();
                var asByteArray = Encoding.Default.GetBytes(password);
                hashedPassword = sha.ComputeHash(asByteArray);
                AddUserToDB(id, username, Convert.ToBase64String(hashedPassword), email);
            }
            return true;
        }

        [WebMethod]
        public DataTable GetAllBooks()
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Books", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            DataTable dt = new DataTable();
            dt.Load(dr);
            dt.TableName = "Games";
            return dt;
        }


        [WebMethod]
        public void CreateBook(string name, int dimension, string description, string username)
        {
            int id = GetId("Book");
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("INSERT INTO Books VALUES ('" + id + "', '" + name + "', '" + 0 + "', '" + username + "', '" + dimension + "', '" + description + "')", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            cmd.ExecuteReader();

        }

        [WebMethod]
        public void ChangeEmail(string username, string newEmail)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("UPDATE Users SET email = '" + newEmail + "' WHERE name = '" + username + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            cmd.ExecuteReader();
        }

        [WebMethod]
        public string GetEmail(string username)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT email FROM Users WHERE name = '" + username + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                result = result + dr[0].ToString();
            }
            return result;
        }
    }
}
