using System;
using System.Data.SqlClient;

namespace Petrol
{
    internal class DataBaseHelper
    {
        private readonly string connectionString = "Data Source=DESKTOP-N5LCA8G;Initial Catalog=petrol;Integrated Security=True;Encrypt=False;";

        public SqlConnection GetConnection()
        {
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }

                throw new Exception("Bağlantı sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public void CloseConnection(SqlConnection connection)
        {
            try
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Bağlantı kapatılırken bir hata oluştu: " + ex.Message);
            }
        }
    }
}
