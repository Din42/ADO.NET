using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Common;


namespace stock
{
    public partial class Form1 : Form
    {
       private SqlConnection sqlConnection = null;
        public Form1()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["stockDB"].ConnectionString);
            sqlConnection.Open();
            string aqlQuery = "SELECT * FROM Товары";
            
            using (SqlCommand command = new SqlCommand(aqlQuery, sqlConnection))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dt = new DataTable();
                dataAdapter.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView2.DataSource = dt;
                dataGridView3.DataSource = dt;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlCommand command = new SqlCommand(
                $"INSERT INTO [Товары] (Название, ТипТовара," +
                $" ПоставщикТовара, Количество, Себестоимость, ДатаПоставки)" +
                $" VALUES (N'{textBox1.Text}',N'{textBox2.Text}',N'{textBox3.Text}'" +
                $",N'{textBox4.Text}',N'{textBox5.Text}',N'{textBox6.Text}')",
                sqlConnection);

            MessageBox.Show(command.ExecuteNonQuery().ToString("Добавлен"));

            LoadData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand command = new SqlCommand($"UPDATE [Товары] SET ", sqlConnection);

                bool hasUpdateFields = false;

                if (!string.IsNullOrEmpty(textBox12.Text))
                {
                    command.CommandText += $"Название = N'{textBox12.Text}', ";
                    hasUpdateFields = true;
                }
                if (!string.IsNullOrEmpty(textBox11.Text))
                {
                    command.CommandText += $"ТипТовара = N'{textBox11.Text}', ";
                    hasUpdateFields = true;
                }
                if (!string.IsNullOrEmpty(textBox10.Text))
                {
                    command.CommandText += $"ПоставщикТовара = N'{textBox10.Text}', ";
                    hasUpdateFields = true;
                }
                if (!string.IsNullOrEmpty(textBox9.Text))
                {
                    command.CommandText += $"Количество = N'{textBox9.Text}', ";
                    hasUpdateFields = true;
                }
                if (!string.IsNullOrEmpty(textBox8.Text))
                {
                    command.CommandText += $"Себестоимость = N'{textBox8.Text}', ";
                    hasUpdateFields = true;
                }
                if (!string.IsNullOrEmpty(textBox7.Text))
                {
                    command.CommandText += $"ДатаПоставки = N'{textBox7.Text}', ";
                    hasUpdateFields = true;
                }

                // Удаление последней запятой и пробела, если они есть
                if (command.CommandText.EndsWith(", "))
                    command.CommandText = command.CommandText.Remove(command.CommandText.Length - 2);

                command.CommandText += $"WHERE ИдентификаторТовара = N'{textBox13.Text}'";

                if (hasUpdateFields)
                {
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Товар успешно обновлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Товар с указанным Артикулом не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Нет полей для обновления", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRecord(string itemId)
        {
            try
            {                
            // Подготавливаем SQL-запрос для удаления записи
            string query = $"DELETE FROM [Товары] WHERE ИдентификаторТовара = N'{itemId}'";
                
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    // Выполняем запрос
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись успешно удалена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData(); // Обновляем данные в DataGridView после удаления
                    }
                    else
                    {
                        MessageBox.Show("Запись с указанным идентификатором не найдена", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {            
            // Получите идентификатор записи, которую вы хотите удалить
            string itemIdToDelete = textBox14.Text;

            // Вызовите метод удаления
            DeleteRecord(itemIdToDelete); 
        }

        private void ShowSupplierWithMaxQuantity()
        {
            try
            {
                
                string query = "SELECT TOP 1 ПоставщикТовара, SUM(Количество) AS TotalQuantity " +
                               "FROM Товары " +
                               "GROUP BY ПоставщикТовара " +
                               "ORDER BY TotalQuantity DESC";

                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string supplierName = reader["ПоставщикТовара"].ToString();
                        int totalQuantity = Convert.ToInt32(reader["TotalQuantity"]);

                        MessageBox.Show($"Поставщик: {supplierName}\nОбщее количество товаров: {totalQuantity}",
                                        "Информация о поставщике с наибольшим количеством товаров",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Информация не найдена", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowSupplierWithMaxQuantity();
        }

        private void ShowSupplierWithMinQuantity()
        {
            try
            {

                string query = "SELECT TOP 1 ПоставщикТовара, SUM(Количество) AS TotalQuantity " +
                               "FROM Товары " +
                               "GROUP BY ПоставщикТовара " +
                               "ORDER BY TotalQuantity ASC";

                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string supplierName = reader["ПоставщикТовара"].ToString();
                        int totalQuantity = Convert.ToInt32(reader["TotalQuantity"]);

                        MessageBox.Show($"Поставщик: {supplierName}\nОбщее количество товаров: {totalQuantity}",
                                        "Информация о поставщике с наименьшим количеством товаров",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Информация не найдена", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            ShowSupplierWithMinQuantity();
        }

        private void ShowProductTypeWithMaxQuantity()
        {
            try
            {

                string query = "SELECT TOP 1 ТипТовара, SUM(Количество) AS TotalQuantity " +
                               "FROM Товары " +
                               "GROUP BY ТипТовара " +
                               "ORDER BY TotalQuantity DESC";

                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string supplierName = reader["ТипТовара"].ToString();
                        int totalQuantity = Convert.ToInt32(reader["TotalQuantity"]);

                        MessageBox.Show($"Тип товара: {supplierName}\nОбщее количество товаров: {totalQuantity}",
                                        "Информация о типе товара с наибольшим количеством товаров",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Информация не найдена", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ShowProductTypeWithMaxQuantity();
        }

        private void ShowProductTypeWithMinQuantity()
        {
            try
            {

                string query = "SELECT TOP 1 ТипТовара, SUM(Количество) AS TotalQuantity " +
                               "FROM Товары " +
                               "GROUP BY ТипТовара " +
                               "ORDER BY TotalQuantity ASC";

                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string supplierName = reader["ТипТовара"].ToString();
                        int totalQuantity = Convert.ToInt32(reader["TotalQuantity"]);

                        MessageBox.Show($"Тип товара: {supplierName}\nОбщее количество товаров: {totalQuantity}",
                                        "Информация о типе товара с наименьшим количеством товаров",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Информация не найдена", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ShowProductTypeWithMinQuantity();
        }

        private void ShowProductsWithDeliveryOlderThan(int days)
        {
            try
            {
                string query = $"SELECT * FROM Товары WHERE ДатаПоставки <= DATEADD(day, -{days}, GETDATE())";

                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    dataAdapter.Fill(dt);

                    if(dt.Rows.Count > 0)
                    {
                        dataGridView1.DataSource = dt;
                    }
                    else
                    {
                        MessageBox.Show("Товары с поставкой старше указанного количества дней не найдены", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int days;

            if(int.TryParse(textBox15.Text, out days))
            {
                ShowProductsWithDeliveryOlderThan(days);
            }
            else
            {
                MessageBox.Show("Введите корректное количество дней", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
