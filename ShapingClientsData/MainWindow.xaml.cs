using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Data.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace ShapingClientsData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataTable clientsTable = new DataTable();
        DataTable birthdayTable = new DataTable();
        DataTable expiredTable = new DataTable();
        public delegate void MethodDelegate(object row);
        DataRow selectedRow;
        string connectionString;
        int selectedId;
        bool saved;
        bool lastCheckSaved;
        List<object> LastSavedData = new List<object>();
        List<object> InputFields;
        bool addingClients;
        int nomberOfClients = 0;
        DatabaseHandler databaseHandler;
        enum Mounths
        {
            января,
            февраля,
            марта,
            апреля,
            мая,
            июня,
            июля,
            августа,
            сентября,
            октября,
            ноября,
            декабря
        }
        public MainWindow()
        {
            InitializeComponent();
            clientDataGrid.ItemsSource = null;
            clientDataGrid.ItemsSource = clientsTable.DefaultView;
            lastCheckSaved = true;
            saved = true;           
            connectionString = ConfigurationManager.ConnectionStrings["ShapingClientsData.Properties.Settings.ClientsConnectionString"].ConnectionString;
            databaseHandler = new DatabaseHandler(connectionString);
            databaseHandler.SetConnection();
            UpdateTab();
            ShowClients();
            comboBox1.SelectedIndex = 0;
            labelСhanges.Visibility = Visibility.Hidden;
            ShowClientData();
            Cashdata();
            ShowOncomingDates("Birthday", listBox, listBoxDate, birthdayTable); //"ExpiredDate"
            ShowOncomingDates("ExpiredDate", listBoxExpired, listBox2, expiredTable);
            Subscribe();


        }

        private void Subscribe()
        {
            comboBox1.SelectionChanged += ComboBox_SelectionChanged;
            comboBox1.PreviewMouseUp += ComboBox1_MouseEnter;
            listBox.SelectionChanged += UncomingBirthdaySelected;
            listBoxExpired.SelectionChanged += ListBox1_SelectionChanged;
            addClient.TextChanged += AddClient_TextChanged;
            addingClients = false;
            textBox.PreviewMouseDown += Any_PreviewMouseDown;
            lastVisitDate.PreviewMouseDown += Any_PreviewMouseDown;
            expiredDate.PreviewMouseDown += Any_PreviewMouseDown;
            phoneNomber.PreviewMouseDown += Any_PreviewMouseDown;
            email.PreviewMouseDown += Any_PreviewMouseDown;
            birthday.PreviewMouseDown += Any_PreviewMouseDown;
            comment.PreviewMouseDown += Any_PreviewMouseDown;
            textBox.TextChanged += TextBox_TextChanged;
            lastVisitDate.SelectedDateChanged += LastVisitDate_SelectedDateChanged;
            expiredDate.SelectedDateChanged += Any_SelectedDateChanged;
            phoneNomber.TextChanged += Any_TextChanged;
            email.TextChanged += Any_TextChanged;
            birthday.SelectedDateChanged += Any_SelectedDateChanged;
            comment.TextChanged += Any_TextChanged;
            listBox.PreviewMouseUp += ListBox_PreviewMouseUp;
            listBoxExpired.PreviewMouseUp += ListBoxExpired_PreviewMouseUp;
            this.Closing += MainWindow_Closing1;
            clientDataGrid.LoadingRow += ClientDataGrid_LoadingRow;
        }

        #region EventHandlers

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxSelected(listBoxExpired);
        }

        private void ComboBox1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CheckForChanges();
        }

        private void UncomingBirthdaySelected(object sender, SelectionChangedEventArgs e)
        {
            ListBoxSelected(listBox);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ShowClientData();
            Cashdata();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Удалить этого клиента из базы данных?",
                "Удаление клиента", MessageBoxButtons.YesNo);
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                databaseHandler.DeleteLineFromDB("Client", selectedId);
                UpdateTab();
                ShowClients();
                ShowClientData();
                ShowOncomingDates("Birthday", listBox, listBoxDate, birthdayTable);
                ShowOncomingDates("ExpiredDate", listBoxExpired, listBox2, expiredTable);
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {

            if (CheckForDubble(addClient.Text))
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Клиент " + addClient.Text + " уже добавлен в базу. добавить еще раз?",
            "Добавление клиента", MessageBoxButtons.YesNo);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    AddClient();
                }
                else if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    comboBox1.SelectedItem = addClient.Text;
                }
            }
            else
            {
                AddClient();
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            ShapingClientsData.ClientsDataSet clientsDataSet = ((ShapingClientsData.ClientsDataSet)(this.FindResource("clientsDataSet")));
            // Load data into the table Client. You can modify this code as needed.
            ShapingClientsData.ClientsDataSetTableAdapters.ClientTableAdapter clientsDataSetClientTableAdapter = new ShapingClientsData.ClientsDataSetTableAdapters.ClientTableAdapter();
            clientsDataSetClientTableAdapter.Fill(clientsDataSet.Client);
            System.Windows.Data.CollectionViewSource clientViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("clientViewSource")));
            clientViewSource.View.MoveCurrentToFirst();
        }

        private void updateButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            RegisterOneVisit();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            PhoneSearch(textBoxPhoneSearch.Text);
        }

        #endregion

        private void RegisterOneVisit()
        {
            
            int remained = Convert.ToInt32(textBox.Text);
            if (remained > 0)
            {
                remained--;
                try
                {
                    if (lastVisitDate.SelectedDate == DateTime.Today)
                    {
                        DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Сегодня уже есть отмечанное посещение. Все равно отметить?",
                        "Повторное", MessageBoxButtons.YesNo);
                        if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            databaseHandler.UpdateItemInDB("Client", "VisitsLeft", selectedId, remained);
                        }
                    }
                    else
                    {
                        DatabaseHandler databaseHandler = new DatabaseHandler(connectionString);
                        databaseHandler.SetConnection();
                        databaseHandler.UpdateItemInDB("Client", "VisitsLeft", selectedId, remained);
                        databaseHandler.UpdateItemInDB("Client", "LastCheckDate", selectedId, DateTime.Today);
                    }

                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
                finally
                {
                    UpdateTab();
                    ShowClients();
                    ShowClientData();
                    lastCheckSaved = true;
                }
            }
        }

        private void CheckForChanges()
        {
            if (!(saved && lastCheckSaved))
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Сохранить изменения клиента " + comboBox1.SelectedItem + "?",
                      "Сохранение", MessageBoxButtons.YesNo);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    Update();
                }
                else if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    lastCheckSaved = true;
                    saved = true;
                }
            }
        }

        private void ListBoxSelected(System.Windows.Controls.ListBox listBox)
        {
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                try
                {
                    if (listBox.SelectedItem.ToString() == comboBox1.Items[i].ToString())
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }
                catch (Exception ex) { break; }

            }
        }

        private void Cashdata()
        {
            InputFields = new List<object>()
            {
                textBox.Text,
                lastVisitDate.SelectedDate,
                expiredDate.SelectedDate,
                phoneNomber.Text,
                email.Text,
                birthday.SelectedDate,              
                comment.Text
            };
            LastSavedData.Clear();
            for (int i = 0; i < InputFields.Count; i++)
            {
                LastSavedData.Add(InputFields[i]);
            }
            
        }

        private void ShowClientData()
        {

            lastCheckSaved = true;
            saved = true;

            foreach (DataRow row in clientsTable.Rows)
            {
                if (row["Surnames"] == comboBox1.SelectedItem)
                {
                    selectedRow = row;
                    selectedId = (int)row["Id"];
                    
                    FillInputTextElement(textBox, row["VisitsLeft"]);
                    FillInputDateElement(lastVisitDate, row["LastCheckDate"]);
                    FillInputDateElement(expiredDate, row["ExpiredDate"]);
                    FillInputTextElement(phoneNomber, row["Phone"]);
                    FillInputTextElement(email, row["Email"]);
                    FillInputDateElement(birthday, row["Birthday"]);
                    FillInputTextElement(comment, row["Comment"]);
                }                
            }
        }

        private void UpdateTab()
        {           
            databaseHandler.FillTabFromDB(clientsTable, "Client");
        }

        private void ShowClients()
        {
            nomberOfClients = 0;
            int selectedIndex = comboBox1.SelectedIndex;
            List<object> surnames = new List<object>();
            comboBox1.Items.Clear();
            foreach (DataRow row in clientsTable.Rows)
            {
                surnames.Add(row["Surnames"]);
                nomberOfClients++;
            }

            surnames.Sort();
            foreach (Object name in surnames)
            {
                comboBox1.Items.Add(name);
            }
            comboBox1.SelectedIndex = selectedIndex;
            labelNomberOfClients.Content = nomberOfClients;
        }

        #region ShowMethods

        private void FillInputTextElement(System.Windows.Controls.TextBox inputElement, object inputData)
        {
            try
            {
                inputElement.Text = inputData.ToString();

            }
            catch (Exception ex) { inputElement.Text = ""; }                    
        }

        private void FillInputDateElement(DatePicker datePicker, object inputData)
        {
            try
            {
                datePicker.SelectedDate = (System.DateTime)inputData;

            }
            catch (Exception ex) { datePicker.SelectedDate = null; }
        }

        private void undoButton__Click(object sender, RoutedEventArgs e)
        {
            ShowClientData();
        }



        private void ClientDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void MainWindow_Closing1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!saved)
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Данные клиента " + comboBox1.SelectedItem + " изменены, но не сохранены. Все равно закрыть?",
            "Сохранение", MessageBoxButtons.YesNo);
                if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
            else
            {
                var p = new Process();
                p.StartInfo.FileName = "ConsoleApp1.exe";  // just for example, you can use yours.
                p.Start();
            }
        }

        private void ListBoxExpired_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxSelected(listBoxExpired);
        }

        private void ListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxSelected(listBox);
        }


        private void Any_TextChanged(object sender, TextChangedEventArgs e)
        {
            AnyChange();
        }


        private void Any_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            AnyChange();
        }

        private void LastVisitDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            lastCheckSaved = false;
            AnyChange();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            lastCheckSaved = false;
            AnyChange();
        }

        private void Any_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckIfAdd();
        }

        private void AddClient_TextChanged(object sender, TextChangedEventArgs e)
        {
            addingClients = true;
        }

        #endregion

        private bool CheckForDubble(string clientName)
        {
            foreach (string item in comboBox1.Items)
            {
                if (item == clientName)
                {
                    return true;
                }
            }
            return false;
        }

        private void ShowOncomingDates(string dateColumn, System.Windows.Controls.ListBox surnames, System.Windows.Controls.ListBox dates, DataTable dataTable)
        {
            if (dataTable.Columns.Count == 0)
            {
                dataTable.Columns.Add("DayOfYear");
                dataTable.Columns.Add(dateColumn);
                dataTable.Columns.Add("Surname");
            }

            dataTable.Clear();

            foreach (DataRow row in clientsTable.Rows)
            {
                if (row[dateColumn] != null)
                {
                    FillBirthdayTab(row[dateColumn], row["Surnames"], dataTable);
                }
            }
            DataView view = new DataView(dataTable);
            view.Sort = "DayOfYear ASC";
            surnames.Items.Clear();
            dates.Items.Clear();
            foreach (DataRowView row in view)
            {
                surnames.Items.Add(row["Surname"]);
                dates.Items.Add(row[dateColumn]);
            }
            
        }

        private void FillBirthdayTab(object date, object surname, DataTable dataTable)
        {
            try
            {
                System.DateTime targetDate = Convert.ToDateTime(date);
                if ((targetDate.DayOfYear == DateTime.Today.DayOfYear)|| ((targetDate.Month == DateTime.Today.Month)&&(targetDate.Day == DateTime.Today.Day))||
                    ((targetDate.DayOfYear > DateTime.Today.DayOfYear)&&((targetDate.DayOfYear - DateTime.Today.DayOfYear) < 4)))
                {
                    var mounth = (Mounths)targetDate.Month - 1;
                    string dayAndMounth = targetDate.Day.ToString() +" " + mounth.ToString();
                    dataTable.Rows.Add(targetDate.DayOfYear, dayAndMounth, surname);
                }    
            }
            catch(Exception ex) { }
        }

        private void CheckIfAdd()
        {
            if (addingClients)
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Добавить клиента " + addClient.Text + "?",
                  "Добавление клиента", MessageBoxButtons.YesNo);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    AddClient();
                }
                else if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    addingClients = false;
                }
            }
            
        }
        private void AddClient()
        {
            
            addingClients = false;
            databaseHandler.InsertToDB("Client", "Surnames", addClient.Text);
            UpdateTab();
            ShowClients();
            foreach (string item in comboBox1.Items)
            {
                if (addClient.Text == item)
                {
                    comboBox1.SelectedItem = item;
                    break;
                }
            }
            ShowClientData();
            clientDataGrid.Items.Refresh();

        }

        private void AnyChange()
        {
            saved = false;
            labelСhanges.Visibility = Visibility.Hidden;
        }
        private void Update()
        {            
            Dictionary<string, object> setOfParam = new Dictionary<string, object>()
            {
                ["VisitsLeft"] = textBox.Text,
                ["LastCheckDate"] = lastVisitDate.SelectedDate,
                ["ExpiredDate"] = expiredDate.SelectedDate,
                ["Phone"] = phoneNomber.Text,
                ["Email"] = email.Text,
                ["Birthday"] = birthday.SelectedDate,
                ["Comment"] = comment.Text
            };
            foreach (var item in setOfParam)
            {
                if (item.Value != null)
                {
                    databaseHandler.UpdateItemInDB("Client", item.Key, selectedId, item.Value);
                }
            }
            UpdateTab();
            ShowClients();
            ShowClientData();
            ShowOncomingDates("Birthday", listBox, listBoxDate, birthdayTable);
            ShowOncomingDates("ExpiredDate", listBoxExpired, listBox2, expiredTable);
            labelСhanges.Visibility = Visibility.Visible;
            saved = true;
            lastCheckSaved = true;
        }

        private void PhoneSearch(string phoneNomber)
        {           
            foreach (DataRow row in clientsTable.Rows)
            {
                if (row["Phone"] != null)
                {
                    string phone = row["Phone"].ToString();
                    string clean = phone.Replace(@"+", string.Empty)
                                        .Replace(@"-", string.Empty)
                                        .Replace(@"(", string.Empty)
                                        .Replace(@")", string.Empty)
                                        .Replace(" ", string.Empty);

                    if (clean == phoneNomber)
                    {
                        //System.Windows.Forms.MessageBox.Show("found");
                        comboBox1.SelectedItem = row["Surnames"];
                        break;
                    }
                }                
            }
        }

        
    }
}
