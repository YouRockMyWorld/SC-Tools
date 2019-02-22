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
    public class CreateWallEventHandler:IExternalEventHandler
    {
        private UIDocument uiDocument;
        private Document document;

        public List<Element> Rooms { get; set; }
        public Element WallType { get; set; }
        //public Element Level { get; set; }
        public Element BottomLevel { get; set; }
        public float BottomOffset { get; set; }
        public Element TopLevel { get; set; }
        public float TopOffset { get; set; }
        public float Height { get; set; }
        //public SpatialElementBoundaryOptions Option { get; set; }
        public float Offset { get; set; }
        public bool IsRoomBoundary { get; set; }
        public bool IsStructural { get; set; }
        public bool IsJoinWithWall { get; set; }

        public CreateWallEventHandler()
        {

        }

        public void Execute(UIApplication app)
        {
            try
            {
                uiDocument = app.ActiveUIDocument;
                document = uiDocument.Document;
                using (Transaction ts = new Transaction(document, "根据房间创建墙（面层）"))
                {
                    ICollection<ElementId> newWallCollection = new List<ElementId>();
                    string wallInfo = "";
                    if(ts.Start() == TransactionStatus.Started)
                    {
                        
                        foreach (Room room in Rooms)
                        {
                            try
                            {
                                
                                IList<IList<BoundarySegment>> boundarySegments = room.GetBoundarySegments(new SpatialElementBoundaryOptions { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish });
                                
                                if (null != boundarySegments)
                                {
                                    
                                    foreach (IList<BoundarySegment> lb in boundarySegments)
                                    {
                                        
                                        CurveLoop curveLoop = new CurveLoop();
                                        foreach (BoundarySegment b in lb)
                                        {
                                            Curve curve = b.GetCurve();
                                            curveLoop.Append(curve);
                                        }
                                        CurveLoop offsetloop = CurveLoop.CreateViaOffset(curveLoop, -((WallType)WallType).Width / 2.0, new XYZ(0, 0, 1));
                                        for (int i = 0; i < offsetloop.Count(); ++i)
                                        {
                                            Wall wall = null;
                                            if (null == BottomLevel)
                                            {
                                                //TaskDialog.Show("0", BottomLevel.ToString());
                                                wall = Wall.Create(document, offsetloop.ElementAt(i), room.Level.Id, IsStructural);
                                                wallInfo += "楼板类型 : " + WallType.Name + "\n标高 : " + room.Level.Name + "\nID : " + wall.Id + "\n------------------------------\n";
                                            }
                                            else
                                            {
                                                wall = Wall.Create(document, offsetloop.ElementAt(i), BottomLevel.Id, IsStructural);
                                                wallInfo += "楼板类型 : " + WallType.Name + "\n标高 : " + BottomLevel.Name + "\nID : " + wall.Id + "\n------------------------------\n";
                                            }
                                            newWallCollection.Add(wall.Id);

                                            Element joinedwall = document.GetElement(lb.ElementAt(i).ElementId) as Wall;
                                            if (IsJoinWithWall && lb.ElementAt(i).IsValidObject && null != joinedwall)
                                                JoinGeometryUtils.JoinGeometry(document, wall, joinedwall);
                                        }
                                    }
                                }
                                SetWallParameters(newWallCollection);
                            }
                            catch (Exception ex)
                            {
                                TaskDialog.Show("Error - EXECUTE_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
                                wallInfo += "******************************\n******************************\n" + "错误 : 房间（ID " + room.Id + " ）未生成" + "\n******************************\n******************************\n";
                                continue;
                            }
                        }
                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {
                        if (newWallCollection.Count > 0)
                        {
                            uiDocument.Selection.SetElementIds(newWallCollection);
                            wallInfo = $"--- Design by Liu.SC ---\n共生成墙{newWallCollection.Count}个，信息如下：\n**********************************\n------------------------------\n" + wallInfo + "**********************************\n已添加进当前选择集！";
                            TaskDialog.Show("提示", wallInfo);
                        }
                        else
                        {
                            TaskDialog.Show("提示", "没有生成楼板");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - EXECUTE_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        private void SetWallParameters(ICollection<ElementId> elements)
        {
            try
            {
                foreach (ElementId el in elements)
                {
                    Wall wall = document.GetElement(el) as Wall;
                    if (null != wall)
                    {
                        //墙类型
                        wall.WallType = WallType as WallType;

                        //底部偏移
                        Parameter bottomOffset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                        bottomOffset.Set(BottomOffset / 304.8);
                        
                        if(null == TopLevel)
                        {
                            //无连接高度
                            Parameter height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                            height.Set(Height / 304.8);
                        }
                        else
                        {
                            //顶部约束
                            Parameter topLevel = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                            topLevel.Set(TopLevel.Id);
                            //顶部偏移
                            Parameter topOffset = wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
                            topOffset.Set(TopOffset / 304.8);
                        }

                        //房间边界
                        Parameter isRoomBoundary = wall.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING);
                        isRoomBoundary.Set(IsRoomBoundary == true ? 1 : 0);
                    }
                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - SETWALLPARAMETERS_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

        public string GetName()
        {
            return "Create Wall";
        }
    }
}
