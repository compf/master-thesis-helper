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
        public MainWindow()
        {
            LatexParser parser=new LatexParser("E:\\master-thesis-report\\masterthesis\\main.tex");
           var result= parser.Parse();
            ToString();
           var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            String text=JsonSerializer.Serialize(result,options);
            System.IO.File.WriteAllText("E:\\output.json", text);
            List<LatexBlock> items = new();
            items.Add(result);
            InitializeComponent();
            tvProject.ItemsSource = result;
        }
    }
}