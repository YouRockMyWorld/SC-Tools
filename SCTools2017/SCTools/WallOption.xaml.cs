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
    /// WallOption.xaml 的交互逻辑
    /// </summary>
    public partial class WallOption : Window
    {
        private List<Element> m_rooms = new List<Element>();
        private List<Element> m_wallType = new List<Element>();
        private List<Element> m_level = new List<Element>();
        private List<MyElement> m_toplevel = new List<MyElement>();

        public ExternalEvent ExEvent { get; set; }
        public CreateWallEventHandler EventHandler { get; set; }
        public WallOption()
        {
            InitializeComponent();
        }

        public WallOption(List<Element> rooms, List<Element> wallType, List<Element> level)
        {
            InitializeComponent();
            m_rooms = rooms;
            m_wallType = wallType;
            m_level = level;
            InitData();
        }

        private void InitData()
        {
            try
            {
                cbb_WallType.ItemsSource = m_wallType;
                cbb_BottomLevel.ItemsSource = m_level;

                m_toplevel.Add(new MyElement(null));
                foreach (Element el in m_level)
                {
                    m_toplevel.Add(new MyElement(el));
                }
                cbb_TopLevel.ItemsSource = m_toplevel;

                cbb_WallType.SelectedIndex = 0;
                cbb_TopLevel.SelectedIndex = 0;
                cbb_TopLevel.IsEnabled = false;
                //cbb_BottomLevel.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - INITDATA_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void PreviewTextInput_tb_Validator(object sender, TextCompositionEventArgs e)
        {
            try
            {
                System.Windows.Controls.TextBox tb = sender as System.Windows.Controls.TextBox;
                System.Text.RegularExpressions.Regex re = null;
                //如果是高度输入框，不能输入负数或0
                if (tb.Name == "tb_Height")
                {
                    re = new System.Text.RegularExpressions.Regex(@"^(\d+)(\.\d+)?$|^\d+\.$|^\d+$");
                }
                else
                {
                    re = new System.Text.RegularExpressions.Regex(@"^(-?\d+)(\.\d+)?$|^-?\d+\.$|^-?\d+$|^-?$");
                }
                e.Handled = !re.IsMatch(tb.Text + e.Text);
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - PREVIEWTEXTINPUT_TB_VALIDATOR_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
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
                TaskDialog.Show("Error - PREVIEWEXECUTE_TB_KEYVALIDATOR_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
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
                TaskDialog.Show("Error - PREVIEWKEYDOWN_TB_KEYVALIDATOR_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void SelectionChanged_cbb_BottomLevel(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                System.Windows.Controls.ComboBox cbb = (System.Windows.Controls.ComboBox)sender;
                if(cbb.SelectedIndex == -1)
                {
                    cbb_TopLevel.IsEnabled = false;
                }
                else
                {
                    cbb_TopLevel.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - SELECTIONCHANGED_CBB_BOTTOMLEVEL_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void SelectionChanged_cbb_TopLevel(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                System.Windows.Controls.ComboBox cbb = (System.Windows.Controls.ComboBox)sender;

                //如果是顶部约束Combobox，检查选中项是不是“未连接”
                if (cbb.Name == "cbb_TopLevel")
                {
                    if (cbb.SelectedIndex != -1)
                    {
                        if (cbb.SelectedIndex == 0)
                        {
                            tb_Height.IsEnabled = true;
                            tb_TopOffset.IsEnabled = false;
                        }
                        else
                        {
                            tb_Height.IsEnabled = false;
                            tb_TopOffset.IsEnabled = true;
                        }
                    }
                }

                //检查两个combobox所选中的标高高度是否符合底部标高小于顶部标高
                if (cbb_BottomLevel.SelectedIndex != -1 && cbb_TopLevel.SelectedIndex != -1 && cbb_TopLevel.SelectedIndex != 0)
                {
                    if (((Level)cbb_BottomLevel.SelectedItem).Elevation >= ((Level)((MyElement)cbb_TopLevel.SelectedItem).Element).Elevation)
                    {
                        TaskDialog.Show("错误", "顶部标高需大于底部标高！");
                        cbb_TopLevel.SelectedIndex = 0;
                    }
                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - SELECTIONCHANGED_CBB_TOPLEVEL_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
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
                TaskDialog.Show("Error - CLICK_B_CANCEL_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
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
                float bottomOffset = 0.0f;
                float.TryParse(tb_BottomOffset.Text, out bottomOffset);
                float height = 0.0f;
                float.TryParse(tb_Height.Text, out height);
                float topOffset = 0.0f;
                float.TryParse(tb_TopOffset.Text, out topOffset);
                EventHandler.Rooms = m_rooms;
                EventHandler.WallType = cbb_WallType.SelectedItem as Element;
                EventHandler.BottomLevel = cbb_BottomLevel.SelectedItem as Element;
                EventHandler.BottomOffset = bottomOffset;
                EventHandler.TopLevel = (cbb_TopLevel.SelectedItem as MyElement)?.Element;
                EventHandler.Height = height;
                EventHandler.TopOffset = topOffset;
                EventHandler.IsRoomBoundary = rb_IsRoomBoundary1.IsChecked.Value;
                EventHandler.IsStructural = rb_IsStructural1.IsChecked.Value;
                EventHandler.IsJoinWithWall = rb_IsJoinWithWall1.IsChecked.Value;

                ExEvent.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("CLICK_B_APPLY_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }


    }
}
