using System;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace courseProject
{
    public partial class Form1 : Form
    {
        private DataTable table = new DataTable()
        {
            Locale = CultureInfo.InvariantCulture
        };
        private BindingSource bindingSource = new BindingSource();
        private OleDbDataAdapter dataAdapter = new OleDbDataAdapter();
        private string connectionString;

        private DateTime now = new DateTime();
        private string formattedDateTime;

        public Form1()
        {
            InitializeComponent();
            timeChange();
            sexField.SelectedIndex = 0;
            phoneField.Text = "+79";

            connectionString = @"provider=Microsoft.Jet.OLEDB.4.0; data source = DataBase\БД.mdb";
            OleDbConnection connection = new OleDbConnection(connectionString);
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                dataGrid.DataSource = null;
            }
            else
            {
                MessageBox.Show("Не удалось подключиться к БД");
            }
            OpenTable();
        }

        private void OpenTable()
        {
            var selectCommand = "SELECT * FROM клиенты";
            dataGrid.AutoGenerateColumns = true;
            // Create a new data adapter based on the specified query.
            dataAdapter = new OleDbDataAdapter(selectCommand, connectionString);

            // Create a command builder to generate SQL update, insert, and
            // delete commands based on selectCommand.
            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapter);

            // Populate a new data table and bind it to the BindingSource.
            dataAdapter.Fill(table);
            bindingSource.DataSource = table;

            dataGrid.DataSource = bindingSource;

            filterComboBox.Items.Clear();
            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                filterComboBox.Items.Add(column.Name);
            }
        }

        private void filter(string field, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Проверяем, является ли поле 'код' числовым, чтобы применять соответствующий тип сравнения
                if (field.ToLower() == "код")
                {
                    try
                    {
                        // 'код' - это числовое поле, поэтому используем оператор '=' для точного сравнения
                        bindingSource.Filter = field + " = " + value;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Код может быть только натуральным числом!");
                        filterTextBox.Text = "";
                    }
                }
                else
                {
                    // Для других полей продолжаем использовать оператор LIKE для шаблонного поиска
                    bindingSource.Filter = field + " LIKE '%" + value + "%'";
                }
            }
            else
            {
                // Если значение пустое, очищаем фильтр
                bindingSource.Filter = "";
            }
        }

        private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            filter(filterComboBox.Text, filterTextBox.Text);
        }

        private void filterTextBox_TextChanged(object sender, EventArgs e)
        {
            filter(filterComboBox.Text, filterTextBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (isDataValid() && isDataUniq(false))
            {
                table.Rows.Add(null, nameField.Text, surnameField.Text, sexField.Text, ageField.Text, phoneField.Text, emailField.Text, formattedDateTime);
            }
        }
        private void saveButton_Click(object sender, EventArgs e)
        {
            dataAdapter.Update(table);
            table.Clear();
            dataAdapter.Fill(table);
            MessageBox.Show("Данные обновлены!");
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            table.Clear();
            dataAdapter.Fill(table);
        }

        private void changeButton_Click(object sender, EventArgs e)
        {
            if (isDataValid() && isDataUniq(true))
            {
                if (dataGrid.SelectedRows.Count > 0)
                {
                    int rowIndex = dataGrid.SelectedRows[0].Index;
                    DataRow rowToUpdate = table.Rows[rowIndex];
                    
                    rowToUpdate["Имя"] = nameField.Text;
                    rowToUpdate["Фамилия"] = surnameField.Text;
                    rowToUpdate["Пол"] = sexField.Text;
                    rowToUpdate["Возраст"] = ageField.Text;
                    rowToUpdate["Телефон"] = phoneField.Text;
                    rowToUpdate["Email"] = emailField.Text;
                }
            }
        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count > 0)
            {
                int rowIndex = dataGrid.SelectedRows[0].Index;

                idField.Text = dataGrid[0, rowIndex].Value.ToString();
                nameField.Text = dataGrid[1, rowIndex].Value.ToString();
                surnameField.Text = dataGrid[2, rowIndex].Value.ToString();
                sexField.Text = dataGrid[3, rowIndex].Value.ToString();
                if (dataGrid[4, rowIndex].Value.ToString() != "")
                    ageField.Value = Int32.Parse(dataGrid[4, rowIndex].Value.ToString());
                else
                    ageField.Value = 18;
                phoneField.Text = dataGrid[5, rowIndex].Value.ToString();
                emailField.Text = dataGrid[6, rowIndex].Value.ToString();
                dateField.Text = dataGrid[7, rowIndex].Value.ToString();
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            /*if (dataGrid.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGrid.SelectedRows)
                {
                    if(table.Rows.Count > 0)
                        table.Rows.RemoveAt(row.Index);
                }
            }*/
            dataGrid.Focus();
            SendKeys.Send("{DELETE}");
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            nameField.Text = "";
            surnameField.Text = "";
            sexField.SelectedIndex = 0;
            ageField.Text = "";
            phoneField.Text = "+79";
            emailField.Text = "";
            dateField.Text = "";
        }

        private bool isDataValid()
        {
            string patternName = @"^[а-яА-ЯёЁ\s]*$";

            // Шаблон для проверки email
            string patternEmail = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            string patternPhone = @"\+79\d{9}";
            int phoneMaxLength = 12;

            if (nameField.Text == "" || surnameField.Text == "" || ageField.Text == "" || phoneField.Text == "" || emailField.Text == "")
            {
                MessageBox.Show("Введите все данные!");
                return false;
            }

            if (!Regex.IsMatch(nameField.Text, patternName))
            {
                MessageBox.Show("Имя должно содержать русские символы!");
                return false;
            }

            if (!Regex.IsMatch(surnameField.Text, patternName))
            {
                MessageBox.Show("Фамилия должна содержать русские символы!");
                return false;
            }

            // Проверяем формат номера телефона
            if (!Regex.IsMatch(phoneField.Text, patternPhone) || phoneField.Text.Length > phoneMaxLength)
            {
                MessageBox.Show("Неверный номер телефона!\nПроверьте, что номер начинается с +79\nПример корректного номера: +79101235532");
                return false;
            }

            // Проверяем формат email
            if (!Regex.IsMatch(emailField.Text, patternEmail))
            {
                MessageBox.Show("Неверный email!\nПример корректного email: winform@gmail.com");
                return false;
            }

            // Если все проверки пройдены успешно
            return true;
        }

        private bool isDataUniq(bool forChangeButton)
        {
            DataRow selectedRow = null;
            if (forChangeButton)
            {
                int rowIndex = dataGrid.SelectedRows[0].Index;
                selectedRow = table.Rows[rowIndex];
            }

            if ((forChangeButton && selectedRow["Телефон"].ToString() != phoneField.Text) || !forChangeButton)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (row["Телефон"].ToString() == phoneField.Text)
                    {
                        MessageBox.Show("Клиент с таким номером телефона уже существует!");
                        return false;
                    }
                    if (row["Email"].ToString() == emailField.Text)
                    {
                        MessageBox.Show("Клиент с таким email уже существует!");
                        return false;
                    }
                }
            }
            return true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timeChange();
        }

        private void timeChange()
        {
            now = DateTime.Now;
            formattedDateTime = now.ToString("dd.MM.yyyy HH:mm:ss");
            currentTime.Text = formattedDateTime;
        }
    }
}
