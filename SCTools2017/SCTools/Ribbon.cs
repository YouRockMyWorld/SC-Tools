using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Media.Imaging;
using SCTools;

namespace SCTools
{
    class Ribbon : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel myPanel = application.CreateRibbonPanel("SC-Tools");
            //生成楼板按钮
            PushButtonData pushButtonData_AutoCreateFloor = new PushButtonData("RoomAutoCreateFloor", "房间生成楼板", System.Reflection.Assembly.GetExecutingAssembly().Location, "SCTools.CreateFloor");
            PushButton pushButton_AutoCreateFloor = myPanel.AddItem(pushButtonData_AutoCreateFloor) as PushButton;
            pushButton_AutoCreateFloor.LargeImage = new BitmapImage(new Uri("pack://application:,,,/SCTools;component/image/CreateFloor.png"));
            pushButton_AutoCreateFloor.ToolTip = "过滤项目中所有房间并根据房间轮廓生成楼板";

            //生成墙（面层）按钮
            PushButtonData pushButtonData_AutoCreateWall = new PushButtonData("RoomAutoCreateWall", "房间生成墙", System.Reflection.Assembly.GetExecutingAssembly().Location, "SCTools.CreateWall");
            PushButton pushButton_AutoCreateWall = myPanel.AddItem(pushButtonData_AutoCreateWall) as PushButton;
            pushButton_AutoCreateWall.LargeImage = new BitmapImage(new Uri("pack://application:,,,/SCTools;component/image/CreateWall.png"));
            pushButton_AutoCreateWall.ToolTip = "过滤项目中所有房间并根据房间轮廓生成墙（面层）";

            myPanel.AddSeparator();
            //删除房间按钮
            PushButtonData pushButtonData_AutoDeleteRoom = new PushButtonData("RoomDelete", "删除房间", System.Reflection.Assembly.GetExecutingAssembly().Location, "SCTools.DeleteAllRooms");
            PushButton pushButton_AutoDeleteRoom = myPanel.AddItem(pushButtonData_AutoDeleteRoom) as PushButton;
            pushButton_AutoDeleteRoom.LargeImage = new BitmapImage(new Uri("pack://application:,,,/SCTools;component/image/DeleteRoom.png"));
            pushButton_AutoDeleteRoom.ToolTip = "删除项目中选中的房间";

            myPanel.AddSeparator();
            //批量旋转构件
            PushButtonData pushButtonData_RotateElements = new PushButtonData("RotateElements", "批量旋转构件", System.Reflection.Assembly.GetExecutingAssembly().Location, "SCTools.RotateElements");
            PushButton pushButton_RotateElements = myPanel.AddItem(pushButtonData_RotateElements) as PushButton;
            pushButton_RotateElements.LargeImage = new BitmapImage(new Uri("pack://application:,,,/SCTools;component/image/RotateElements.png"));
            pushButton_RotateElements.ToolTip = "将选中的构件批量旋转";


            //生成体量
            PushButtonData pushButtonData_AutoCreateForm = new PushButtonData("PointAutoCreateForm", "点生成体量", System.Reflection.Assembly.GetExecutingAssembly().Location, "SCTools.CreateForm");
            PushButton pushButton_AutoCreateForm = myPanel.AddItem(pushButtonData_AutoCreateForm) as PushButton;
            pushButton_AutoCreateForm.LargeImage = new BitmapImage(new Uri("pack://application:,,,/SCTools;component/image/CreateForm.png"));
            pushButton_AutoCreateForm.ToolTip = "根据原始数据点，自动生成体量族模型";

            myPanel.AddSeparator();
            //关于
            PushButtonData pushButtonData_About = new PushButtonData("About", "  关于  ", System.Reflection.Assembly.GetExecutingAssembly().Location, "SCTools.AboutCommand");
            PushButton pushButton_About = myPanel.AddItem(pushButtonData_About) as PushButton;
            pushButton_About.LargeImage = new BitmapImage(new Uri("pack://application:,,,/SCTools;component/image/About.png"));
            pushButton_About.ToolTip = "关于";

            return Result.Succeeded;
        }
    }
}
