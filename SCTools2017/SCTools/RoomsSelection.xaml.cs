using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Autodesk.Revit.DB.Architecture;

namespace SCTools
{
    /// <summary>
    /// RoomsSelection.xaml 的交互逻辑
    /// </summary>
    public partial class RoomsSelection : Window
    {
        private List<Element> m_rooms = new List<Element>();
        private List<MyRoom> m_myrooms = new List<MyRoom>();
        //private ObservableCollection<MyRom> m_myrooms = new ObservableCollection<MyRoom>();

        public ExternalEvent ExEvent { get; set; }
        public DeleteAllRoomsEventHandler EventHandler { get; set; }
        public RoomsSelection()
        {
            InitializeComponent();
        }

        public RoomsSelection(List<Element> rooms)
        {
            InitializeComponent();
            m_rooms = rooms;
            InitData();
        }

        private void InitData()
        {
            foreach(var room in m_rooms)
            {
                m_myrooms.Add(new MyRoom(room));
            }
            lb_RoomsList.ItemsSource = m_myrooms;
        }

        private void Click_b_SelectAll(object sender, RoutedEventArgs e)
        {
            foreach(var i in m_myrooms)
            {
                try
                {
                    i.IsChecked = true;
                }
                catch
                {
                    continue;
                }
            }
        }

        private void Click_b_CancelAll(object sender, RoutedEventArgs e)
        {
            foreach (var i in m_myrooms)
            {
                try
                {
                    i.IsChecked = false;
                }
                catch
                {
                    continue;
                }
            }
        }

        private void Click_b_ReverseAll(object sender, RoutedEventArgs e)
        {
            foreach (var i in m_myrooms)
            {
                try
                {
                    i.IsChecked = !i.IsChecked;
                }
                catch
                {
                    continue;
                }
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
                EventHandler.Rooms = (from room in m_myrooms where room.IsChecked == true select room.Element).ToList();
                if(EventHandler.Rooms.Count == 0)
                {
                    TaskDialog.Show("提示", "请至少选择一个房间");
                }
                else
                {
                    ExEvent.Raise();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - CLICK_B_APPLY_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void Click_b_Cancel(object sender, RoutedEventArgs e)
        {
            try
            {
                ExEvent.Dispose();
                this.Close();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - CLICK_B_CANCEL_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void SelectionChanged_cbb_order(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbb_order.SelectedIndex != -1)
                {
                    if (cbb_order.SelectedIndex == 0)
                    {
                        List<Element> order_list = (from element in m_rooms
                                                    orderby element.Name
                                                    select element).ToList();
                        m_myrooms = new List<MyRoom>();
                        foreach (var ele in order_list)
                        {
                            m_myrooms.Add(new MyRoom(ele));
                        }
                        lb_RoomsList.ItemsSource = m_myrooms;
                    }

                    if (cbb_order.SelectedIndex == 1)
                    {
                        List<Element> order_list = (from element in m_rooms
                                                    orderby ((Room)element).Level.Name
                                                    select element).ToList();
                        m_myrooms = new List<MyRoom>();
                        foreach (var ele in order_list)
                        {
                            m_myrooms.Add(new MyRoom(ele));
                        }
                        lb_RoomsList.ItemsSource = m_myrooms;
                    }

                    if (cbb_order.SelectedIndex == 2)
                    {
                        List<Element> order_list = (from element in m_rooms
                                                    orderby element.Id.IntegerValue
                                                    select element).ToList();
                        m_myrooms = new List<MyRoom>();
                        foreach (var ele in order_list)
                        {
                            m_myrooms.Add(new MyRoom(ele));
                        }
                        lb_RoomsList.ItemsSource = m_myrooms;
                    }
                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - SELECTIONCHANGED_CBB_ORDER_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }
    }
}
