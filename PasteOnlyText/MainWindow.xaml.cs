using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

namespace PasteOnlyText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand CustomRoutedCommand = new RoutedCommand();

        private void ExecutedCustomCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Custom Command Executed");
        }

        private void CanExecuteCustomCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Control? target = e.Source as Control;

            if (target != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            MainText.Text = Clipboard.GetText();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clipboard.SetText(MainText.Text);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void MainText_TextChanged(object sender, TextChangedEventArgs e)
        {
            String TextLength = "Characters: " + MainText.Text.Length.ToString();
            //TODO: Update LineCount after layout update
            String NumberOfRows = "Rows: " + MainText.LineCount.ToString();
            string[] s = new string[] { TextLength, NumberOfRows };

            MainStatusBar_Text.Text = string.Join(" | ", s);
        }
    }
}
