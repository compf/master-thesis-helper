using MasterThesisHelper.model;
using MasterThesisHelper.parser;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterThesisHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LatexBlock block;
        public MainWindow()
        {
            LatexParser parser=new LatexParser(Properties.Settings.Default.ThesisPath);
               var result= parser.Parse();
            block = result;

            ToString();
           var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            String text=JsonSerializer.Serialize(result,options);

            InitializeComponent();
            tvProject.ItemsSource = result;
            tvProject.SelectedItemChanged += (sender, e) =>
            {
                var val = e.NewValue as LatexBlock;
                textEditor.Load(val.FilePath);
                textEditor.ScrollTo(val.Line,0);
            };

            
        }
        IChatModel chat = new StubChatModel();
        private void btSend_Click(object sender, RoutedEventArgs e)
        {
           string output= chat.SendAndWait(tbInstruction.Text, block.GetChecked());

            TextBox tbPrompt = new();
            tbPrompt.Text = tbInstruction.Text;
            tbPrompt.HorizontalAlignment = HorizontalAlignment.Right;
            tbPrompt.Background = Brushes.LightBlue;
            tbPrompt.IsReadOnly = true;
            stackChat.Children.Add(tbPrompt);

            TextBox tbOutput = new TextBox();
            tbOutput.MaxLines = 10;
            tbOutput.TextWrapping = TextWrapping.Wrap;
            tbOutput.IsReadOnly = true;
            tbOutput.Text = output;
            stackChat.Children.Add(tbOutput);
        }
    }
}