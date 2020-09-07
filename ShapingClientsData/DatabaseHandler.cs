using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace ShapingClientsData
{
    public class DatabaseHandler
    {
        SqlConnection sqlConnection;
        string connectionString;
        public DatabaseHandler(string connectionStr)
        {
            connectionString = connectionStr;
        }

        public void SetConnection()
        {
            sqlConnection = new SqlConnection(connectionString);
        }

        public bool FillTabFromDB(DataTable dataTable, string dataBaseTabName)
        {
            dataTable.Clear();
            try
            {
                string query = "select * from " + dataBaseTabName;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);
                using (sqlDataAdapter)
                {
                    sqlDataAdapter.Fill(dataTable);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        public void DeleteLineFromDB(string dataBaseTabName, int id)
        {          
            string query = "DELETE FROM " + dataBaseTabName + " WHERE Id = @ClientId";
            HandleQuery(query, "@ClientId", id);
        }

        public void InsertToDB(string dataBaseTabName, string columnName, object data)
        {
            string quety = "INSERT " + dataBaseTabName + " (" + columnName + ") values (@Surname)";
            HandleQuery(quety, "@Surname", data);
        }

        public void UpdateItemInDB(string dataBaseTabName, string columnName, int id, object data)
        {
            string query = "UPDATE " + dataBaseTabName + " SET " + columnName + " = @Parameter where Id = @CurrentId";
            HandleQuery(query, "@Parameter", data, "@CurrentId", id);
        }

        private void HandleQuery(string query, string parametr1Name, object parametr1, string parametr2Name = null, object parametr2 = null)
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue(parametr1Name, parametr1);
                if (parametr2 != null)
                {
                    sqlCommand.Parameters.AddWithValue(parametr2Name, parametr2);
                }
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }
        }

    }
}
