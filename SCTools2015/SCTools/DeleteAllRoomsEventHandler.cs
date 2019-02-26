using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace SCTools
{
    public class DeleteAllRoomsEventHandler : IExternalEventHandler
    {
        private UIDocument uiDocument;
        private Document document;

        public List<Element> Rooms { get; set; }

        public DeleteAllRoomsEventHandler()
        {

        }
        public void Execute(UIApplication app)
        {
            try
            {
                uiDocument = app.ActiveUIDocument;
                document = uiDocument.Document;

                using (Transaction ts = new Transaction(document, "删除房间"))
                {
                    string deleteInfo = "";
                    if (ts.Start() == TransactionStatus.Started)
                    {
                        for (int i = 0; i < Rooms.Count; ++i)
                        {
                            try
                            {
                                string s = "房间名:" + Rooms[i].Name + " | 标高:" + ((Room)Rooms[i]).Level.Name + " | ID:" + Rooms[i].Id + "  已删除\n";
                                document.Delete(Rooms[i].Id);
                                deleteInfo += s;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {
                        deleteInfo = "--- Design by Liu.SC ---\n" + deleteInfo;
                        TaskDialog.Show("提示", deleteInfo);
                    }

                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - EXECUTE_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        public string GetName()
        {
            return "Delete All Rooms";
        }
    }
}
