using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
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
using static System.Net.Mime.MediaTypeNames;

namespace PasteOnlyText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _sourceString;
        private string? SourceString { 
            get => _sourceString;
            set {
                _sourceString = value;
                MainText.Text = Clipboard.GetText();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            SourceString = Clipboard.GetText();

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


        // COMMAND

        public static RoutedCommand TextModifierCommand_Uppercase = new RoutedCommand();
        public static RoutedCommand TextModifierCommand_Lowercase = new RoutedCommand();
        public static RoutedCommand TextModifierCommand_InitialUppercase = new RoutedCommand();
        public static RoutedCommand TextModifierCommand_FirstUppercase = new RoutedCommand();
        public static RoutedCommand TextModifierCommand_Reset = new RoutedCommand();

        private void TextModifier(Func<string, string> textF) {
            string full = MainText.Text;
            int position = MainText.SelectionStart;
            int length = MainText.SelectionLength;

            if (length > 0)
            {
                string txt = MainText.SelectedText;
                txt = textF(txt);
                MainText.Text = full.Remove(position, length).Insert(position, txt);
                MainText.Select(position, length);
            }
            else
            {
                MainText.Text = textF(full);
                MainText.Select(position, 0);
            }
        }
        
        private void ExecutedTextModifierCommand_Uppercase(object sender, ExecutedRoutedEventArgs e)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            TextModifier(myTI.ToUpper);
        }

        private void ExecutedTextModifierCommand_Lowercase(object sender, ExecutedRoutedEventArgs e)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            TextModifier(myTI.ToLower);
        }

        private void ExecutedTextModifierCommand_InitialUppercase(object sender, ExecutedRoutedEventArgs e)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            TextModifier(myTI.ToTitleCase);
        }

        private void ExecutedTextModifierCommand_FirstUppercase(object sender, ExecutedRoutedEventArgs e)
        {
            Func<string, string> text = txt => txt[0].ToString().ToUpper() + txt.Substring(1).ToLower();
            TextModifier(text);
        }

        private void ExecutedTextModifierCommand_Reset(object sender, ExecutedRoutedEventArgs e)
        {
            MainText.Text = SourceString;
        }

        private void CanExecuteTextModifierCommand(object sender, CanExecuteRoutedEventArgs e)
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
    }
}
