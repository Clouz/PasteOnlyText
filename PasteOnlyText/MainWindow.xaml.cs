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

namespace PasteOnlyText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

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
            MainStatusBar_Text.Text = "Characters: " + MainText.Text.Length.ToString();
        }
    }
}
