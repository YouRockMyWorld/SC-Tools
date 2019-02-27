using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;

namespace SCTools
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class CreateFloor : IExternalCommand
    {
        private ExternalEvent externalEvent;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApplication = commandData.Application;
                UIDocument uiDocument = uiApplication.ActiveUIDocument;
                Document document = uiDocument.Document;
                //过滤项目中是否存在房间，标高和楼板类型等必要信息
                List<Element> rooms = Utils.FilterRoom(document);
                if(rooms.Count == 0)
                {
                    TaskDialog.Show("Error", "没有发现房间！");
                    return Result.Failed;
                }

                List<Element> levels = Utils.FilterLevel(document);
                if(levels.Count == 0)
                {
                    TaskDialog.Show("Error", "没有发现标高！");
                    return Result.Failed;
                }

                List<Element> floorType = Utils.FilterFloorType(document);
                if(floorType.Count == 0)
                {
                    TaskDialog.Show("Error", "没有发现楼板类型!");
                    return Result.Failed;
                }

                TaskDialog declaration = new TaskDialog("声明");
                declaration.MainInstruction = "使用声明：";
                declaration.MainContent = "由于能力有限，即使尽力避免，但此插件仍存在导致软件崩溃的可能性！\n使用前请对您当前的工作进行保存和备份。\n\n是否已对当前工作进行保存？";
                declaration.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                var result = declaration.Show();
                if (result == TaskDialogResult.No) return Result.Cancelled;

                //存在则创建外部事件同时弹出交互对话框，输入相关数据信息
                CreateFloorEventHandler createFloorEventHandler = new CreateFloorEventHandler();
                externalEvent = ExternalEvent.Create(createFloorEventHandler);

                FloorOption floorOption = new FloorOption(rooms, floorType, levels);
                floorOption.ExEvent = externalEvent;
                floorOption.EventHandler = createFloorEventHandler;
                //floorOption.b_Apply.Click += Click_b_Apply;
                floorOption.Show();
 
                return Result.Succeeded;
            }

            catch (Exception e)
            {
                message = e.Message + "\n------\nTargetSite:\n" + e.TargetSite.ToString() + "\n------\nStackTrace:\n" + e.StackTrace;
                return Result.Failed;
            }
        }

        //private void Click_b_Apply(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    try
        //    {
        //    }
        //}

        
    }
}
