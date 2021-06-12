using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace bmi_calculator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The Current user
        private readonly Person _person = new Person();
        // List for saving the last users
        private readonly List<Person> _savedPersons = new List<Person>();
        // Regex used vor validating user input
        private readonly Regex _validateInputRex = new Regex("^\\d{1,4}[.,]\\d{0,2}$|^\\d{1,4}$");

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _person;
            DataObject.AddPastingHandler(TxtHeight, PasteHandler);
            DataObject.AddPastingHandler(TxtWeight, PasteHandler);
        }

        private void CalculateBmiButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayBmiInfo.Foreground = new SolidColorBrush(Colors.Green);
            double bmi = _person.Bmi;
            if (bmi.Equals(0))
            {
                DisplayBmiInfo.Foreground = new SolidColorBrush(Colors.Red);
                DisplayBmiInfo.Content = "Überprüfe deine Eingaben";
                return;
            }

            DisplayBmi.Content = bmi.ToString(CultureInfo.CurrentCulture);
            DisplayBmiInfoText(bmi);

            if (_savedPersons.Count == 10)
                _savedPersons.RemoveAt(0);

            _savedPersons.Add(new Person { Height = _person.Height, Weight = _person.Weight });
        }

        // For saving the last 10 user to a txt file
        private void SaveBmiButton_Click(object sender, RoutedEventArgs e)
        {
            if (_savedPersons.Count == 0)
            {
                MessageBox.Show("Es sind noch keine User im verlauf");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file|*.txt|CSV file|*.csv",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() != true) return;

            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (var file = File.CreateText(saveFileDialog.FileName))
            {
                foreach (Person person in _savedPersons)
                {
                    file.WriteLine(person.GetFormattedString());
                }
            }
        }

        // For displaying the BMI to the screen
        private void DisplayBmiInfoText(double bmi)
        {
            DisplayBmiInfo.Content = "";
            if (bmi < 20)
                MessageBox.Show("Sie haben Untergewicht");

            else if (bmi <= 25)
                DisplayBmiInfo.Content = "Sie haben Optmalgewicht";

            else MessageBox.Show("Sie haben Übergewicht");
        }

        // Validate user input
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_validateInputRex.IsMatch(((TextBox)sender).Text + e.Text);
        }

        private void PasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string))) return;

            string pasteText = e.DataObject.GetData(typeof(string)) as string;
            if (pasteText != null && !_validateInputRex.IsMatch(pasteText))
            {
                e.CancelCommand();
            }

        }
    }

    // Person Object if you later want to do something with it.
    public class Person
    {
        public double Weight { get; set; }
        public double Height { get; set; }
        public double Bmi
        {
            get
            {
                if (Weight <= 0 || Height <= 0) return 0;
                double bmi = Weight / Math.Pow(Height / 100, 2);
                return Math.Round(bmi, 2);
            }
        }
        public string GetFormattedString()
        {
            return Weight + "-" + Height + "-" + Bmi;
        }
    }
}
