using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace SCTools
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class RotateElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApplication = commandData.Application;
                UIDocument uiDocument = uiApplication.ActiveUIDocument;
                Document document = uiDocument.Document;

                TaskDialog declaration = new TaskDialog("声明");
                declaration.MainInstruction = "使用声明：";
                declaration.MainContent = "由于能力有限，即使尽力避免，但此插件仍存在导致软件崩溃的可能性！\n使用前请对您当前的工作进行保存和备份。\n\n是否已对当前工作进行保存？";
                declaration.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                var result = declaration.Show();
                if (result == TaskDialogResult.No) return Result.Cancelled;

                RotateElementsEventHandler rotateElementsEventHandler = new RotateElementsEventHandler();
                ExternalEvent externalEvent = ExternalEvent.Create(rotateElementsEventHandler);

                RotateElementsWindow rotateElementsWindow = new RotateElementsWindow
                {
                    ExEvent = externalEvent,
                    EventHandler = rotateElementsEventHandler
                };
                rotateElementsWindow.Show();

                return Result.Succeeded;

            }
            catch (Exception ex)
            {
                message = ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace;
                return Result.Failed;
            }
        }
    }
}
