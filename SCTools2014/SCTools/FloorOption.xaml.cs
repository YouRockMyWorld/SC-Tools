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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SCTools
{
    /// <summary>
    /// FloorOption.xaml 的交互逻辑
    /// </summary>
    public partial class FloorOption : Window
    {
        private List<Element> m_rooms = new List<Element>();
        private List<Element> m_floorType = new List<Element>();
        private List<Element> m_level = new List<Element>();
        //private CreateFloorEventHandler createFloorHandler;
        //private ExternalEvent externalEvent;

        public ExternalEvent ExEvent { get; set; }
        public CreateFloorEventHandler EventHandler { get; set; }
        public FloorOption()
        {
            InitializeComponent();
        }

        public FloorOption(List<Element> rooms, List<Element> floorType, List<Element> level)
        {
            InitializeComponent();
            m_rooms = rooms;
            m_floorType = floorType;
            m_level = level;
            InitData();
        }

        private void InitData()
        {
            try
            {
                List<string> boundartType = new List<string>() { "面层", "中线", "核心层边界", "核心层中线" };
                cbb_BoundaryType.ItemsSource = boundartType;
                cbb_FloorType.ItemsSource = m_floorType;
                cbb_Level.ItemsSource = m_level;
            }
            catch(Exception ex)
            {
                TaskDialog.Show("INITDATA_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void Click_b_Apply(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ExEvent == null || EventHandler == null)
                {
                    TaskDialog.Show("Error", "CLICK_B_APPLY - ExEvent or EventHandler is null");
                    return;
                }
                SpatialElementBoundaryOptions option = new SpatialElementBoundaryOptions();
                switch (cbb_BoundaryType.SelectedIndex)
                {
                    case 0:
                        option.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;
                        break;
                    case 1:
                        option.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                        break;
                    case 2:
                        option.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary;
                        break;
                    case 3:
                        option.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreCenter;
                        break;
                }

                float offset = 0.0f;
                float.TryParse(tb_Offset.Text, out offset);

                EventHandler.Rooms = m_rooms;
                EventHandler.FloorType = cbb_FloorType.SelectedItem as Element;
                EventHandler.Level = cbb_Level.SelectedItem as Element;
                EventHandler.Option = option;
                EventHandler.Offset = offset;
                EventHandler.IsStructural = rb_IsStructural1.IsChecked.Value;

                ExEvent.Raise();
                
            }
            catch(Exception ex)
            {
                TaskDialog.Show("CLICK_B_APPLY_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void Click_b_Cancel(object sender, RoutedEventArgs e)
        {
            try
            {
                ExEvent.Dispose();
                this.Close();
            }
            catch(Exception ex)
            {
                TaskDialog.Show("CLICK_B_CANCEL_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void PreviewTextInput_tb_Validator(object sender, TextCompositionEventArgs e)
        {
            try
            {
                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"^(-?\d+)(\.\d+)?$|^-?\d+\.$|^-?\d+$|^-?$");
                e.Handled = !re.IsMatch(tb_Offset.Text + e.Text);
            }
            catch(Exception ex)
            {
                TaskDialog.Show("PREVIEWTEXTINPUT_TB_VALIDATOR_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void PreviewExecute_tb_KeyValidator(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Command == ApplicationCommands.Paste)
                {
                    e.Handled = true;
                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("PREVIEWEXECUTE_TB_KEYVALIDATOR_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void PreviewKeyDown_tb_KeyValidator(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("PREVIEWKEYDOWN_TB_KEYVALIDATOR_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        //private void Checked_rb_IsStructural1(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
