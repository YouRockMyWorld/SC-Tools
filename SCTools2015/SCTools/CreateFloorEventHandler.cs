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
    public class CreateFloorEventHandler : IExternalEventHandler
    {
        //private List<Element> m_rooms;
        //private Element m_floorType;
        //private Element m_level;
        //private SpatialElementBoundaryOptions m_option;
        //private float m_offset = 0.0f;
        //private bool m_isStructural;
        private UIDocument uiDocument;
        private Document document;

        public List<Element> Rooms { get; set; }
        public Element FloorType { get; set; }
        public Element Level { get; set; }
        public SpatialElementBoundaryOptions Option { get; set; }
        public float Offset { get; set; }
        public bool IsStructural { get; set; }

        public CreateFloorEventHandler()
        {

        }
        //public CreateFloorEventHandler(List<Element> rooms, Element floorType, Element level, SpatialElementBoundaryOptions option, float offset, bool isStructural)
        //{
        //    m_rooms = rooms;
        //    m_floorType = floorType;
        //    m_level = level;
        //    m_option = option;
        //    m_offset = offset;
        //    m_isStructural = isStructural;
        //}
        public void Execute(UIApplication app)
        {
            try
            {
                uiDocument = app.ActiveUIDocument;
                document = uiDocument.Document;
                using (Transaction ts = new Transaction(document, "根据房间创建楼板"))
                {
                    ICollection<ElementId> newFloorCollection = new List<ElementId>();
                    string floorInfo = "";
                    if (ts.Start() == TransactionStatus.Started)
                    {
                        foreach (Room r in Rooms)
                        {
                            CurveArray roomBoundary = new CurveArray();
                            IList<IList<Autodesk.Revit.DB.BoundarySegment>> boundarySegments = r.GetBoundarySegments(Option);
                            if (boundarySegments != null)
                            {
                                var first = boundarySegments.FirstOrDefault();
                                if (first == null)
                                {
                                    floorInfo += "******************************\n******************************\n" + "错误 : 房间（ID " + r.Id + " ）未生成" + "\n******************************\n******************************\n";
                                    continue;
                                }
                                foreach (Autodesk.Revit.DB.BoundarySegment bs in first)
                                {
                                    Curve curve = bs.Curve;
                                    roomBoundary.Append(curve);
                                }
                            }
                            Floor newFloor = document.Create.NewFloor(roomBoundary, FloorType as FloorType, Level as Level, IsStructural);
                            newFloorCollection.Add(newFloor.Id);
                            floorInfo += "楼板类型 : " + newFloor.FloorType.Name + "\n标高 : " + document.GetElement(newFloor.LevelId).Name + "\nID : " + newFloor.Id + "\n------------------------------\n";
                        }

                        if (Level != null || Offset != 0.0f)
                        {
                            ModifyElementsOffset(newFloorCollection, Offset);
                        }
                    }
                    if(ts.Commit() == TransactionStatus.Committed)
                    {
                        if (newFloorCollection.Count > 0)
                        {
                            uiDocument.Selection.SetElementIds(newFloorCollection);
                            floorInfo = $"--- Design by Liu.SC ---\n共生成楼板{newFloorCollection.Count}个，信息如下：\n**********************************\n------------------------------\n" + floorInfo + "**********************************\n已添加进当前选择集！";
                            TaskDialog.Show("提示", floorInfo);
                        }
                        else
                        {
                            TaskDialog.Show("提示", "没有生成楼板");
                        }
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
            return "Create Floor";
        }

        private void ModifyElementsOffset(ICollection<ElementId> element, double offset)
        {
            try
            {
                foreach (ElementId el in element)
                {
                    Floor f = document.GetElement(el) as Floor;
                    if (f != null)
                    {
                        //Parameter levelPara = f.LookupParameter("自标高的高度偏移");
                        Parameter levelP = f.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                        levelP.Set(offset / 304.8);
                    }
                }
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error - MODIFYELEMENTSOFFSET_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }
    }
}
