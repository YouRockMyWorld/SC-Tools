using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace SCTools
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class DeleteAllRooms : IExternalCommand
    {
        private ExternalEvent externalEvent;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApplication = commandData.Application;
                UIDocument uiDocument = uiApplication.ActiveUIDocument;
                Document document = uiDocument.Document;
                //过滤项目中是否存在房间
                List<Element> rooms = Utils.FilterRoom(document);
                if (rooms.Count == 0)
                {
                    TaskDialog.Show("Error", "没有发现房间！");
                    return Result.Failed;
                }

                TaskDialog declaration = new TaskDialog("声明");
                declaration.MainInstruction = "使用声明：";
                declaration.MainContent = "由于能力有限，即使尽力避免，但此插件仍存在导致软件崩溃的可能性！\n使用前请对您当前的工作进行保存和备份。\n\n是否已对当前工作进行保存？";
                declaration.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                var result = declaration.Show();
                if (result == TaskDialogResult.No) return Result.Cancelled;

                DeleteAllRoomsEventHandler deleteAllRoomsEventHandler = new DeleteAllRoomsEventHandler();
                externalEvent = ExternalEvent.Create(deleteAllRoomsEventHandler);

                RoomsSelection roomsSelection = new RoomsSelection(rooms)
                {
                    ExEvent = externalEvent,
                    EventHandler = deleteAllRoomsEventHandler
                };
                roomsSelection.Show();
                return Result.Succeeded;
            }

            catch (Exception e)
            {
                message = e.Message + "\n------\nTargetSite:\n" + e.TargetSite.ToString() + "\n------\nStackTrace:\n" + e.StackTrace;
                return Result.Failed;
            }
        }
    }
}
