using System;
using System.Collections.Generic;
using System.IO;
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
    /// CreateForm.xaml 的交互逻辑
    /// </summary>
    public partial class FormOption : Window
    {
        private string m_filePath = "";
        //private List<XYZ> m_pList1 = new List<XYZ>();
        private System.Collections.ObjectModel.ObservableCollection<XYZ> m_pList1 = new System.Collections.ObjectModel.ObservableCollection<XYZ>();
        //private List<XYZ> m_pList2 = new List<XYZ>();
        private System.Collections.ObjectModel.ObservableCollection<XYZ> m_pList2 = new System.Collections.ObjectModel.ObservableCollection<XYZ>();


        public ExternalEvent ExEvent { get; set; }
        public CreateFormEventHandler EventHandler { get; set; }
        public FormOption()
        {
            InitializeComponent();
        }

        private void Click_b_Open(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                dlg.DefaultExt = "*.txt";
                dlg.Filter = "Text document (*.txt)|*.txt";

                bool? result = dlg.ShowDialog();
                if(result == true)
                {
                    m_filePath = dlg.FileName;
                    GetData(m_filePath);
                    if (DataValidation())
                    {
                        b_Create.IsEnabled = true;
                        tb_DataFilePath.Text = m_filePath;
                    }
                    else
                    {
                        //暂无更细节的验证
                        //若有在此处做数据不正确处理
                    }
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - CLICK OPEN", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void GetData(string path)
        {
            try
            {
                //读取txt数据，对每一行以tab做split划分，取前三个为x1 y1 z1，后三个为x2 y2 z2
                //不做过多验证
                using (StreamReader sr = new StreamReader(path))
                {
                    string line = "";
                    m_pList1.Clear();
                    m_pList2.Clear();
                    while((line = sr.ReadLine()) != null)
                    {
                        string[] str_arr = line.Split('\t');
                        float x0 = default(float);
                        float y0 = 0.0f;
                        float z0 = 0.0f;
                        if(float.TryParse(str_arr[0], out x0) && float.TryParse(str_arr[1], out y0) && float.TryParse(str_arr[2], out z0))
                        {
                            m_pList1.Add(new XYZ(x0, y0, z0));
                        }

                        float x1 = 0.0f;
                        float y1 = 0.0f;
                        float z1 = 0.0f;
                        if (float.TryParse(str_arr[3], out x1) && float.TryParse(str_arr[4], out y1) && float.TryParse(str_arr[5], out z1))
                        {
                            m_pList2.Add(new XYZ(x1, y1, z1));
                        }
                    }
                }

                dg_PointList1.ItemsSource = m_pList1;
                dg_PointList2.ItemsSource = m_pList2;
                
            }
            catch (Exception ex)
            {
                tb_DataFilePath.Text = "";
                b_Create.IsEnabled = false;
                TaskDialog.Show("Error - GET_DATA", "数据格式不正确，请输入正确的数据格式\n\n" + ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void Click_b_CreateForm(object sender, RoutedEventArgs e)
        {
            try
            {

                if (ExEvent == null || EventHandler == null)
                {
                    TaskDialog.Show("Error", "CLICK_B_CREATEFORM - ExEvent or EventHandler is null");
                    return;
                }

                EventHandler.PointList1 = m_pList1;
                EventHandler.PointList2 = m_pList2;
                EventHandler.IsSolid = cb_IsSolid.IsChecked == true ? true : false;
                EventHandler.IsOnlyCurve = cb_IsOnlyCurve.IsChecked == true ? true : false;

                ExEvent.Raise();

                //b_Create.IsEnabled = false;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - CREATE FORM", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        //当前只做了点列表个数验证，没有做更细节的验证
        private bool DataValidation()
        {
            if (m_pList1.Count == 0 || m_pList2.Count == 0)
            {
                return false;
            }

            return true;
        }
    }
}
