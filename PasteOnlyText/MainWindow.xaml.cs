using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                MainText.Text = value;
            }
        }

        private FindAndReplace FR = new FindAndReplace();

        private string? ClipboardDataString
        {
            get {
                if (Clipboard.ContainsFileDropList()) // If Clipboard has one or more files
                {
                    var files = Clipboard.GetFileDropList().Cast<string>().ToArray(); // Get all files from clipboard
                    string filepath = "";
                    foreach (var file in files)
                    {
                        filepath = filepath + file + "\n";
                    }

                    filepath = filepath.TrimEnd();

                    return filepath;

                } else {
                    return Clipboard.GetText();
                }
            }
            set
            {
                Clipboard.SetText(value);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            ButtonRegEx.DataContext = FR;
            ButtonCaseSensitive.DataContext = FR;
            ButtonUnescape.DataContext = FR;

            SourceString = ClipboardDataString;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clipboard.SetText(MainText.Text);
        }

        private async void MainText_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(500);

            MainStatusBar_Text.Text = StatusBarText();

        }

        private string StatusBarText()
        {
            string TextLength = "Characters: " + MainText.Text.Length.ToString();
            string NumberOfRows = "Rows: " + MainText.Text.Split('\n').Length;
            //string Position = "Position: " + MainText.SelectionStart;
            string FindResultCount = ResultList.Items.Count > 0 && FindGrid.Visibility != Visibility.Collapsed ? "Result: " + ResultList.Items.Count.ToString() : "";    
            string[] s = new string[] { TextLength, NumberOfRows, FindResultCount };

             return string.Join(" | ", s);
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

        private bool hideFind = false;

        public static RoutedCommand TextModifierFind = new RoutedCommand();

        private void ExecutedTextModifierFind(object sender, ExecutedRoutedEventArgs e)
        {
            if (hideFind)
            {
                FindGrid.Visibility = Visibility.Collapsed;
                FindSplitter.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(MainText, 2);
                MainText.Focus();
                hideFind = false;
            } else
            {
                FindGrid.Visibility = Visibility.Visible;
                FindSplitter.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(MainText, 1);
                FindText.Focus();
                hideFind = true;
            }
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

        public static RoutedCommand Command_Exit = new RoutedCommand();

        private void ExecutedExitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            
            FR.BaseText = MainText.Text;
            FR.Find(FindText.Text);

            ResultList.ItemsSource = FR.Result;
            MainStatusBar_Text.Text = StatusBarText();
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            MainText.Text = FR.Replace(FindText.Text, ReplaceText.Text);
            FindButton_Click(sender, e);
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultList.SelectedItem != null)
            {
                var i = ResultList.SelectedIndex;
                var item = ResultList.SelectedItem as FindAndReplace.ResultItem;
                MainText.Focus();
                MainText.Select(item!.Position, item.Text!.Length);

                ResultList.SelectedItem = ResultList.Items[i];
                ResultList.UpdateLayout();  
                var listBoxItem = (ListBoxItem) ResultList.ItemContainerGenerator.ContainerFromItem(ResultList.SelectedItem);
                ResultList.Focus();
            }
        }

        private void MainText_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }

    public class FindAndReplace
    {
        private string _baseText = "";
        public string BaseText { get { return _baseText; } set { _baseText = value; } }

        private bool _regEx;
        public bool RegEx { get { return _regEx;} set { _regEx = value; } }

        private bool _unescape = true;
        public bool Unescape { get { return _unescape; } set { _unescape = value; } }

        private bool _caseSensitive = false;
        public bool CaseSensitive { get { return _caseSensitive; } set { _caseSensitive = value; } }

        private StringComparison StringComp { get { return _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase; } }

        private int _trimLength = 10;
        public int TrimLength { get { return _trimLength; } set { _trimLength = (value < 0 ? 0 : value); } }

        private string _findText = "";
        private string FindText { get { return _unescape ? System.Text.RegularExpressions.Regex.Unescape(_findText) : _findText; } set { _findText = value; } }

        private string _replaceText = "";
        private string ReplaceText { get { return _unescape ? System.Text.RegularExpressions.Regex.Unescape(_replaceText) : _replaceText; } set { _replaceText = value; } }

        public List<ResultItem>? Result;

        public class ResultItem
        {
            public string? Text { get; set; }
            public string? TextBefore { get; set; }
            public string? TextAfter { get; set; }
            public int Position { get; set; }
        }

        public void Find(string Text)
        {
            FindText = Text;
            if (RegEx)
            {
                FindRegEx(FindText);
            } else
            {
                FindF(FindText);
            }
        }

        private void FindF(string Text)
        {
            Result = new List<ResultItem>();

            if (Text == "")
            {
                return;
            }

            int minIndex = _baseText.IndexOf(Text, StringComp);
            while (minIndex != -1)
            {

                int stringMinIndex = (minIndex - TrimLength) >= 0 ? minIndex - TrimLength : 0;
                int stringMinIndexLength = minIndex - stringMinIndex;

                int stringMaxIndex = minIndex + Text.Length;
                int stringMaxLength = (minIndex + Text.Length + TrimLength) <= _baseText.Length ? TrimLength : _baseText.Length - stringMaxIndex;
                int stringLength = (minIndex + Text.Length + TrimLength) <= _baseText.Length ? minIndex - stringMinIndex + Text.Length + TrimLength : _baseText.Length - stringMinIndex;


                ResultItem rs = new ResultItem()
                {
                    Position = minIndex,
                    Text = _baseText.Substring(minIndex, Text.Length),
                    TextBefore = _baseText.Substring(stringMinIndex, stringMinIndexLength),
                    TextAfter = _baseText.Substring(stringMaxIndex, stringMaxLength)
                };

                Result.Add(rs);
                minIndex = _baseText.IndexOf(Text, minIndex + Text.Length, StringComp);
            }
        }

        private void FindRegEx(string Text)
        {
            //TODO run this function Async
            Result = new List<ResultItem>();

            if (Text == "")
            {
                return;
            }

            try
            {

                string pattern = Text;
                Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);

                MatchCollection matches = rgx.Matches(_baseText);

                if (matches.Count > 0)
                {

                    foreach (Match match in matches)
                    {
                        int minIndex = match.Index;

                        int stringMinIndex = (minIndex - TrimLength) >= 0 ? minIndex - TrimLength : 0;
                        int stringMinIndexLength = minIndex - stringMinIndex;

                        int stringMaxIndex = minIndex + Text.Length;
                        int stringMaxLength = (minIndex + Text.Length + TrimLength) <= _baseText.Length ? TrimLength : _baseText.Length - stringMaxIndex;
                        int stringLength = (minIndex + Text.Length + TrimLength) <= _baseText.Length ? minIndex - stringMinIndex + Text.Length + TrimLength : _baseText.Length - stringMinIndex;


                        ResultItem rs = new ResultItem()
                        {
                            Position = minIndex,
                            Text = _baseText.Substring(minIndex, Text.Length),
                            TextBefore = _baseText.Substring(stringMinIndex, stringMinIndexLength),
                            TextAfter = _baseText.Substring(stringMaxIndex, stringMaxLength)
                        };

                        Result.Add(rs);
                    }
                }
            } catch
            {

            }
        }

        public string Replace(string find, string replace)
        {
            FindText = find;
            ReplaceText = replace;

            //TODO: If find is empty return Nul and manage it
            if (FindText != "")
            {
                return _baseText.Replace(FindText, ReplaceText, StringComp);
            } else
            {
                return _baseText;
            }
        }

        public void ResetResult()
        {
            Result = new List<ResultItem>();
        }
    }
}
