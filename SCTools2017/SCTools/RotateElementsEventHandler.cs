using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SCTools
{
    public class RotateElementsEventHandler : IExternalEventHandler
    {
        private UIDocument uiDocument;
        private Document document;

        private double PI = 3.1415926535897931;

        public double RotateAngle;

        public RotateElementsEventHandler()
        {

        }

        public void Execute(UIApplication app)
        {
            try
            {
                uiDocument = app.ActiveUIDocument;
                document = uiDocument.Document;

                ICollection<ElementId> elementIds = uiDocument.Selection.GetElementIds();

                if(elementIds.Count == 0)
                {
                    TaskDialog.Show("Error", "未选中任何构件，请先选择构件再执行此命令！");
                    return;
                }

                using (Transaction ts = new Transaction(document, "rotate elements"))
                {
                    if (ts.Start() == TransactionStatus.Started)
                    {
                        foreach (var elid in elementIds)
                        {
                            Element element = document.GetElement(elid);

                            Location location = element.Location;
                            LocationPoint locationPoint = location as LocationPoint;
                            if (locationPoint != null)
                            {
                                XYZ op = locationPoint.Point;
                                XYZ p = new XYZ(op.X, op.Y, op.Z + 100);
                                Line axis_line = Line.CreateBound(op, p);
                                locationPoint.Rotate(axis_line, RotateAngle / 180.0 * PI);
                            }
                        }
                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - ROTATE_ELEMENTS_EVENTHANDLER_EXECUTE_ERROR", ex.Message);
            }
        }

        public string GetName()
        {
            return "Rotate Elements";
        }
    }
}
