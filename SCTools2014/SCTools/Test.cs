using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace SCTools
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApplication = commandData.Application;
                UIDocument uiDocument = uiApplication.ActiveUIDocument;
                Document document = uiDocument.Document;
                //过滤项目中是否存在房间，标高和楼板类型等必要信息
                List<Element> rooms = Utils.FilterRoom(document);
                if (rooms.Count == 0)
                {
                    TaskDialog.Show("Error", "没有发现房间！");
                    return Result.Failed;
                }

                List<Element> levels = Utils.FilterLevel(document);
                if (levels.Count == 0)
                {
                    TaskDialog.Show("Error", "没有发现标高！");
                    return Result.Failed;
                }

                List<Element> wallTypes = Utils.FilterWallType(document);
                if (wallTypes.Count == 0)
                {
                    TaskDialog.Show("Error", "没有发现墙类型！");
                    return Result.Failed;
                }

                using (Transaction ts = new Transaction(document, "根据房间创建墙（面层）"))
                {
                    ICollection<ElementId> newWallCollection = new List<ElementId>();
                    string wallInfo = "";
                    if (ts.Start() == TransactionStatus.Started)
                    {
                        foreach (Room room in rooms)
                        {
                            try
                            {
                                IList<IList<Autodesk.Revit.DB.BoundarySegment>> boundarySegments = room.GetBoundarySegments(new SpatialElementBoundaryOptions() { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish });
                                if (null != boundarySegments)
                                {
                                    foreach (IList<Autodesk.Revit.DB.BoundarySegment> lb in boundarySegments)
                                    {
                                        CurveLoop curveLoop = new CurveLoop();
                                        foreach (Autodesk.Revit.DB.BoundarySegment b in lb)
                                        {
                                            Curve curve = b.Curve;
                                            curveLoop.Append(curve);
                                        }
                                        CurveLoop offsetloop = CurveLoop.CreateViaOffset(curveLoop, -((WallType)wallTypes[1]).Width / 2.0, new XYZ(0, 0, 1));

                                        for (int i = 0; i < offsetloop.Count(); ++i)
                                        {
                                            Wall wall = Wall.Create(document, offsetloop.ElementAt(i), room.Level.Id, true);
                                            newWallCollection.Add(wall.Id);
                                            Element joinedwall = lb.ElementAt(i).Element as Wall;
                                            if (null != joinedwall)
                                                JoinGeometryUtils.JoinGeometry(document, wall, joinedwall);
                                        }
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }

                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {
                        if (newWallCollection.Count > 0)
                        {
                            uiDocument.Selection.SetElementIds(newWallCollection);
                            wallInfo = $"--- Design by Liu.SC ---\n共生成楼板{newWallCollection.Count}个，信息如下：\n**********************************\n------------------------------\n" + wallInfo + "**********************************\n已添加进当前选择集！";
                            TaskDialog.Show("提示", wallInfo);
                        }
                        else
                        {
                            TaskDialog.Show("提示", "没有生成楼板");
                        }
                    }
                }

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
