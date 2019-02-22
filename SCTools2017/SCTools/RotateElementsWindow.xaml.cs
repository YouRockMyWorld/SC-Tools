using Autodesk.Revit.UI;
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

namespace SCTools
{
    /// <summary>
    /// RotateElementsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RotateElementsWindow : Window
    {
        public ExternalEvent ExEvent { get; set; }
        public RotateElementsEventHandler EventHandler { get; set; }
        public RotateElementsWindow()
        {
            InitializeComponent();
        }

        private void PreviewKeyDown_tb_KeyValidator(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PreviewExecute_tb_KeyValidator(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void PreviewTextInput_tb_Validator(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"^(-?\d+)(\.\d+)?$|^-?\d+\.$|^-?\d+$|^-?$");
            e.Handled = !re.IsMatch(rotate_angle.Text + e.Text);
        }

        private void Click_b_Rotate(object sender, RoutedEventArgs e)
        {
            try
            {
                if(ExEvent == null || EventHandler == null)
                {
                    TaskDialog.Show("Error", "CLICK_B_ROTATE - ExEvent or EventHandler is null");
                    return;
                }
                bool result = double.TryParse(rotate_angle.Text, out double angle);
                if (result)
                {
                    EventHandler.RotateAngle = angle;
                    ExEvent.Raise();
                }
                else
                {
                    TaskDialog.Show("Error", "请输入有效数字");
                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - CLICK_B_ROTATE_ERROR", ex.Message);
            }
        }

        private void Click_b_Cancle(object sender, RoutedEventArgs e)
        {
            try
            {
                ExEvent.Dispose();
                this.Close();
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - CLICK_B_CANCEL_ERROR", ex.Message);
            }
        }
    }
}
