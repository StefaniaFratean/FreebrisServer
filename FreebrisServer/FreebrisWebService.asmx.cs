using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services;
using System.Xml.Linq;

namespace FreebrisServer
{
    /// <summary>
    /// Summary description for FreebrisWebService
    /// </summary>
    /// 
    [WebService(Description = "Serviciu Web pentru proiectul semestrial Freebris, materie II", Name = "FreebrisWebService", Namespace = "FreebrisServer")]
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
            byte[] hashedPassword = { };

            string pass = "";
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    pass = dr.GetString(0);
                    pass = pass.Trim();
                    var sha = SHA256.Create();
                    var asByteArray = Encoding.Default.GetBytes(password);
                    //var asByteArray = Encoding.Default.GetBytes(password);
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
        public bool IsAdmin(string username)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT typeOfAccount FROM Users WHERE username = \'" + username + "\'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            string typeOfAccount = "";
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    typeOfAccount = dr.GetString(0);
                }
            }

            if (typeOfAccount == "admin")
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
        public int AddPoints(string id, int points)
        {
            //still in lucru
            SqlConnection connection = new SqlConnection();
            SqlCommand OldPoints = new SqlCommand("SELECT points From Users WHERE id = '" + id + "'", connection);
            SqlCommand cmd = new SqlCommand("UPDATE Users SET points = '" + points + "' WHERE id = '" + id + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = OldPoints.ExecuteReader();


            int pt = 0;
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    pt = dr.GetInt32(0) + points;
                    return dr.GetInt32(dr.GetOrdinal(points.ToString()));
                }
            }

            return pt;
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
            message.Attachments.Add(new Attachment(text));
            message.IsBodyHtml = false;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            smtpClient.Send(message);
        }

        private int GenerateId(string tableName)
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
            SqlCommand cmd = new SqlCommand("INSERT INTO Users VALUES ('" + id + "', '" + username + "', '" + password + "', '" + 0 + "', '" + "admin', '" + 1 + "', '" + email + "')", connection);
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
                int id = GenerateId("Users");
                //string email = username + "@gmail.com";
                var sha = SHA256.Create();
                var asByteArray = Encoding.Default.GetBytes(password);
                hashedPassword = sha.ComputeHash(asByteArray);
                AddUserToDB(id, username, Convert.ToBase64String(hashedPassword), email);
            }
            return true;
        }

        [WebMethod]
        public int GetId(string username, string table)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT id FROM "+ table +" WHERE username = \'" + username + "\'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            int idNr = 0;
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    idNr = dr.GetInt32(0);
                    //pass = pass.Trim();
                }
            }
            return idNr;
        }
        [WebMethod]
        public int GetPoints(string username)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT points FROM Users WHERE username = \'" + username + "\'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            int idNr = 0;
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    idNr = dr.GetInt32(0);
                    //pass = pass.Trim();
                }
            }
            return idNr;
        }

        [WebMethod]
        public bool DeleteUser(int id)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE id = \'" + id + "\'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            int cd = cmd.ExecuteNonQuery();
            if (cd == 0)
            {
                return false;
            }
            return true;
        }

        [WebMethod]
        public bool DeleteBook(int id)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("DELETE FROM Books WHERE id = \'" + id + "\'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            int cd = cmd.ExecuteNonQuery();
            if (cd == 0)
            {
                return false;
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
            dt.TableName = "Books";
            return dt;
        }


        [WebMethod]
        public void CreateBook(string name, int size, int idAuthor, int idIconBook)
        {
            int id = GenerateId("Books");
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("INSERT INTO Books VALUES ('" + id + "', '" + name + "', '" + size + "', '" + "1" + "', '" + idAuthor + "', '" + idIconBook + "')", connection);
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
            SqlCommand cmd = new SqlCommand("SELECT email FROM Users WHERE username = '" + username + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                result = result + dr[0].ToString();
            }
            return result;
        }

        [WebMethod]
        public DataTable GetBooksByTitle(string bookName)
        {
            DataTable books = GetAllBooks();
            var filteredRows = books.AsEnumerable()
                .Where(row => row.Field<string>("name").IndexOf(bookName, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            DataTable dt = books.Clone();
            dt.Rows.Clear();
            foreach(DataRow d in filteredRows)
            {
                dt.Rows.Add(d.ItemArray);
            }
            dt.TableName = "Books";
            return dt;
            //var booksFiltered = filteredRows.Select(row => new Books
            //{
            //    id = row.Field<int>("id"),
            //    name = row.Field<string>("name"),
            //    pdfFile = row.Field<string>("pdfFile"),
            //    idAuthor = row.Field<int>("id"),
            //    idIconBook = row.Field<int>("idIconBook")
            //}).ToArray();

            //return booksFiltered;
        }

        [WebMethod]
        public DataTable GetBooksByAuthor(string authorName)
        {
            List<Books> books = new List<Books>();

            var authorId = GetUserId(authorName);
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Books WHERE idAuthor = '" + authorId + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dt.TableName = "Books";
            return dt;

            //using (SqlDataReader reader = cmd.ExecuteReader())
            //{
            //    while (reader.Read())
            //    {
            //        int id = Convert.ToInt32(reader["id"]);
            //        string name = reader["name"].ToString();
            //        var book = new Books
            //        {
            //            id = id,
            //            name = name,
            //        };
            //        books.Add(book);
            //    }

            //    return books.ToArray();
            //}
        }
        public int GetUserId(string authorName)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT id FROM Users WHERE username = '" + authorName + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            object result = cmd.ExecuteScalar();

            if (result != null)
            {
                return Convert.ToInt32(result);
            }
            else
            {
                return -1;
            }
        }
        [WebMethod]
        public string GetIcon(int id)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT imgpath FROM Icons WHERE id = '" + id + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            object result = cmd.ExecuteScalar();

            return Convert.ToString(result);
        }
        [WebMethod]
        public string GetIconBook(int id)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT imgpath FROM IconBooks WHERE id = '" + id + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            object result = cmd.ExecuteScalar();

            return Convert.ToString(result);
        }
        [WebMethod]
        public string GetUsername(int id)
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand("SELECT username FROM Users WHERE id = '" + id + "'", connection);
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionWebService"].ToString();
            connection.Open();
            object result = cmd.ExecuteScalar();

            return Convert.ToString(result);
        }
    }
}

public class Books
{
    public int id { get; set; }
    public string name { get; set; }

    public float size {  get; set; }    

    public string pdfFile { get; set; }

    public int idAuthor { get; set; }

    public int idIconBook { get; set; }
}
