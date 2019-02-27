using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System.IO;
using Autodesk.Revit.DB.Structure;

namespace SCTools
{

    #region MEPTest
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class MEPTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApplication = commandData.Application;
                UIDocument uiDocument = uiApplication.ActiveUIDocument;
                Document document = uiDocument.Document;

                using (Transaction ts = new Transaction(document, "create pipe"))
                {

                    if (ts.Start() == TransactionStatus.Started)
                    {
                        FilteredElementCollector pipsystem = new FilteredElementCollector(document);
                        pipsystem.OfClass(typeof(PipingSystemType));
                        PipingSystemType pipesystemtype = pipsystem.ToList().First() as PipingSystemType;

                        FilteredElementCollector pipetype = new FilteredElementCollector(document);
                        pipetype.OfClass(typeof(PipeType));
                        PipeType ptype = pipetype.ToList().First() as PipeType;
                        Pipe tp = Pipe.Create(document, pipesystemtype.Id, ptype.Id, Level.Create(document, 5).Id, new XYZ(21048.48, 105380.9, 13430).Divide(304.8), new XYZ(21058.86, 105427.2, 14470).Divide(304.8));

                        FilteredElementCollector ductsystem = new FilteredElementCollector(document);
                        ductsystem.OfClass(typeof(MechanicalSystemType));
                        MechanicalSystemType ductsystemtype = ductsystem.ToList()[0] as MechanicalSystemType;
                        TaskDialog.Show("sss", ductsystemtype.Name + ductsystem.Count());

                        FilteredElementCollector ducttype = new FilteredElementCollector(document);
                        ducttype.OfClass(typeof(DuctType));
                        DuctType dtype = ducttype.ToList().First() as DuctType;
                        Duct d = Duct.Create(document, ductsystemtype.Id, dtype.Id, Level.Create(document, 5).Id, new XYZ(21841.02, 105322.18, 14060).Divide(304.8), new XYZ(21861.82, 105255.39, 12860).Divide(304.8));

                        FilteredElementCollector collector = new FilteredElementCollector(document);
                        collector = collector.OfClass(typeof(FamilySymbol));
                        var query = from element in collector
                                    where element.Name == "adaptive"
                                    select element; // Linq 查询
                        List<Element> famSyms = query.ToList<Element>();
                        ElementId symbolId = famSyms[0].Id;

                        FamilyInstance instance = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(document, famSyms[0] as FamilySymbol);
                        IList<ElementId> placePointIds = new List<ElementId>();
                        placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance);
                        int i = 0;
                        foreach (ElementId id in placePointIds)
                        {
                            ReferencePoint point = document.GetElement(id) as ReferencePoint;
                            if (i == 0)
                                point.Position = new XYZ(21841.02, 105322.18, 14060).Divide(304.8);
                            else
                                point.Position = new XYZ(21861.82, 105255.39, 12860).Divide(304.8);
                            ++i;
                        }

                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {

                    }
                }

                return Result.Succeeded;
            }

            catch (Exception ex)
            {
                message = ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace;
                return Result.Failed;
            }
        }

    }
    #endregion

    #region AdaptiveTest

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class AdaptiveTest : IExternalCommand
    {
        //private List<List<string>> m_data = new List<List<string>>();
        private List<ProcessedAdaptiveFamilyData> m_data = new List<ProcessedAdaptiveFamilyData>();
        private string m_file = "";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApplication = commandData.Application;
                UIDocument uiDocument = uiApplication.ActiveUIDocument;
                Document document = uiDocument.Document;

                m_file = GetFilePath();
                GetData(m_file);

                using (Transaction ts = new Transaction(document, "create pipe"))
                {

                    if (ts.Start() == TransactionStatus.Started)
                    {
                        //过滤方形管道族
                        FilteredElementCollector collector = new FilteredElementCollector(document);
                        collector = collector.OfClass(typeof(FamilySymbol));
                        var rec_query = from element in collector
                                        where element.Name == "方形管道"
                                        select element;
                        List<Element> rec_famSyms = rec_query.ToList<Element>();
                        if (rec_famSyms.Count == 0)
                        {
                            TaskDialog.Show("提示", "项目中没有发现方形管道族");
                            return Result.Cancelled;
                        }
                        FamilySymbol rec_FamilySymbol = rec_famSyms[0] as FamilySymbol;


                        //过滤圆形管道族
                        var circle_query = from element in collector
                                           where element.Name == "圆形管道"
                                           select element;
                        List<Element> circle_famSyms = circle_query.ToList<Element>();
                        if (circle_famSyms.Count == 0)
                        {
                            TaskDialog.Show("提示", "项目中没有发现圆形管道族");
                            return Result.Cancelled;
                        }
                        FamilySymbol circle_FamilySymbol = circle_famSyms[0] as FamilySymbol;


                        //过滤材质
                        FilteredElementCollector materialCollector = new FilteredElementCollector(document);
                        materialCollector.OfClass(typeof(Material));

                        string file_n = Path.GetFileNameWithoutExtension(m_file);

                        var material = (from mat in materialCollector
                                        where mat.Name == file_n
                                        select mat).ToList();

                        foreach (var data in m_data)
                        {
                            //过滤掉截面太小的数据
                            string shape = data.SectionShape;
                            if (shape.Contains("X"))
                            {
                                float widthf = 0.0f;
                                float heightf = 0.0f;
                                float.TryParse(shape.Split('X')[0], out widthf);
                                float.TryParse(shape.Split('X')[1], out heightf);
                                if (widthf < 300 && heightf < 300) continue;
                            }
                            else
                            {
                                float radius = 0.0f;
                                float.TryParse(shape, out radius);
                                if (radius < 300) continue;
                            }

                            //矩形
                            if (data.SectionShape.Contains("X"))
                            {
                                FamilyInstance instance = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(document, rec_FamilySymbol);
                                IList<ElementId> placePointIds = new List<ElementId>();
                                placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance);

                                XYZ s_p = null;
                                XYZ e_p = null;
                                double widthf = 0.0f;
                                double.TryParse(data.SectionShape.Split('X')[0], out widthf);
                                double heightf = 0.0f;
                                double.TryParse(data.SectionShape.Split('X')[1], out heightf);
                                if (data.StartPointElevationType == "内底")
                                {
                                    s_p = data.StartPoint.Add(new XYZ(0.0, 0.0, heightf / 2.0 /304.8));
                                }
                                if (data.StartPointElevationType == "外顶")
                                {
                                    s_p = data.StartPoint.Subtract(new XYZ(0.0, 0.0, heightf / 2.0 / 304.8));
                                }
                                if (data.EndPointElevationType == "内底")
                                {
                                    e_p = data.EndPoint.Add(new XYZ(0.0, 0.0, heightf / 2.0 / 304.8));
                                }
                                if (data.EndPointElevationType == "外顶")
                                {
                                    e_p = data.EndPoint.Subtract(new XYZ(0.0, 0.0, heightf / 2.0 / 304.8));
                                }

                                (document.GetElement(placePointIds[0]) as ReferencePoint).Position = s_p;
                                (document.GetElement(placePointIds[1]) as ReferencePoint).Position = e_p;

                                Parameter name = instance.LookupParameter("管道名称");
                                name.Set(data.Name);

                                Parameter materialname = instance.LookupParameter("材质");
                                materialname.Set(data.Material.Trim());

                                Parameter width = instance.LookupParameter("宽");
                                width.Set(widthf / 304.8);

                                Parameter height = instance.LookupParameter("高");
                                height.Set(heightf / 304.8);

                                //族里面颜色参数即材质
                                if (material.Count > 0)
                                {
                                    Parameter mat = instance.LookupParameter("颜色");
                                    mat.Set(material[0].Id);
                                }

                            }
                            else
                            {
                                //圆形
                                FamilyInstance instance = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(document, circle_FamilySymbol);
                                IList<ElementId> placePointIds = new List<ElementId>();
                                placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance);

                                XYZ s_p = null;
                                XYZ e_p = null;
                                double diameter = 0.0f;
                                double.TryParse(data.SectionShape, out diameter);
                                if (data.StartPointElevationType == "内底")
                                {
                                    s_p = data.StartPoint.Add(new XYZ(0.0, 0.0, diameter / 2.0 / 304.8));
                                }
                                if (data.StartPointElevationType == "外顶")
                                {
                                    s_p = data.StartPoint.Subtract(new XYZ(0.0, 0.0, diameter / 2.0 / 304.8));
                                }
                                if (data.EndPointElevationType == "内底")
                                {
                                    e_p = data.EndPoint.Add(new XYZ(0.0, 0.0, diameter / 2.0 / 304.8));
                                }
                                if (data.EndPointElevationType == "外顶")
                                {
                                    e_p = data.EndPoint.Subtract(new XYZ(0.0, 0.0, diameter / 2.0 / 304.8));
                                }

                                (document.GetElement(placePointIds[0]) as ReferencePoint).Position = s_p;
                                (document.GetElement(placePointIds[1]) as ReferencePoint).Position = e_p;

                                Parameter name = instance.LookupParameter("管道名称");
                                name.Set(data.Name);

                                Parameter materialname = instance.LookupParameter("材质");
                                materialname.Set(data.Material.Trim());

                                //float lengthf = 0.0f;
                                //float.TryParse(data.SectionShape.Split('X')[0], out lengthf);
                                //float widthf = 0.0f;
                                //float.TryParse(data.SectionShape.Split('X')[1], out widthf);


                                Parameter radius = instance.LookupParameter("半径");
                                radius.Set(diameter / 2.0 / 304.8);

                                //族里面颜色参数即材质
                                if (material.Count > 0)
                                {
                                    Parameter mat = instance.LookupParameter("颜色");
                                    mat.Set(material[0].Id);
                                }
                            }
                        }


                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {

                    }
                }

                return Result.Succeeded;
            }

            catch (Exception ex)
            {
                message = ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace;
                return Result.Failed;
            }
        }

        private string GetFilePath()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.DefaultExt = "*.txt";
            dlg.Filter = "Text document (*.txt)|*.txt";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
                //if ()
                //{

                //}
                //else
                //{
                //    //暂无更细节的验证
                //    //若有在此处做数据不正确处理
                //}
            }
            else
            {
                return "";
            }
        }

        private void GetData(string path)
        {
            try
            {
                //读取txt数据，对每一行以tab做split划分，取前三个为x1 y1 z1，后三个为x2 y2 z2
                //不做过多验证
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    string line = "";
                    m_data.Clear();
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] str_arr = line.Split('\t');
                        ProcessedAdaptiveFamilyData data = new ProcessedAdaptiveFamilyData();
                        data.Name = str_arr[0] + " - " + str_arr[1];
                        float x0 = 0.0f;
                        float y0 = 0.0f;
                        float z0 = 0.0f;
                        if (float.TryParse(str_arr[2], out x0) && float.TryParse(str_arr[3], out y0) && float.TryParse(str_arr[4], out z0))
                        {
                            data.StartPoint = new XYZ(x0, y0, z0).Divide(304.8);
                        }
                        float x1 = 0.0f;
                        float y1 = 0.0f;
                        float z1 = 0.0f;
                        if (float.TryParse(str_arr[5], out x1) && float.TryParse(str_arr[6], out y1) && float.TryParse(str_arr[7], out z1))
                        {
                            data.EndPoint = new XYZ(x1, y1, z1).Divide(304.8);
                        }
                        data.SectionShape = str_arr[8].Trim();
                        data.Material = str_arr[9].Trim();
                        data.StartPointElevationType = str_arr[10].Trim();
                        data.EndPointElevationType = str_arr[11].Trim();

                        m_data.Add(data);
                    }
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - GET_DATA", "数据格式不正确，请输入正确的数据格式\n\n" + ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

    }

    public class ProcessedAdaptiveFamilyData
    {
        public string Name { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public string SectionShape { get; set; }
        public string Material { get; set; }
        public string StartPointElevationType { get; set; }
        public string EndPointElevationType { get; set; }
    }
    #endregion


    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class PlaceFamily : IExternalCommand
    {
        private List<ProcessedJingData> m_data = new List<ProcessedJingData>();
        private string m_file = "";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApplication = commandData.Application;
                UIDocument uiDocument = uiApplication.ActiveUIDocument;
                Document document = uiDocument.Document;
                m_file = GetFilePath();
                GetData(m_file);

                FilteredElementCollector collector = new FilteredElementCollector(document);
                collector = collector.OfClass(typeof(FamilySymbol));
                var query = from element in collector
                                where element.Name == "窨井"
                                select element;
                List<Element> family = query.ToList<Element>();
                if (family.Count == 0)
                {
                    TaskDialog.Show("提示", "项目中没有发现窨井族");
                    return Result.Cancelled;
                }
                FamilySymbol jing_FamilySymbol = family[0] as FamilySymbol;
                

                using (Transaction ts = new Transaction(document, "create yinjing"))
                {
                    if (ts.Start() == TransactionStatus.Started)
                    {
                        jing_FamilySymbol.Activate();
                        foreach (var data in m_data)
                        {
                            FamilyInstance instance = document.Create.NewFamilyInstance(data.Location, jing_FamilySymbol, StructuralType.NonStructural);

                            double depthf = data.Location.Z - data.PipeElevation + 1000 / 304.8;
                            Parameter depth = instance.LookupParameter("埋深");
                            depth.Set(depthf);

                            Parameter text = instance.LookupParameter("文字说明");
                            text.Set(data.JingName);

                            if (data.SectionShape.Contains("X"))
                            {
                                double widthf = 0.0;
                                double.TryParse(data.SectionShape.Split('X')[0], out widthf);
                                double heightf = 0.0;
                                double.TryParse(data.SectionShape.Split('X')[0], out heightf);
                                if (widthf > 500 || heightf > 500)
                                {
                                    double d = widthf > heightf ? widthf : heightf;
                                    Parameter A = instance.LookupParameter("A");
                                    A.Set(d / 304.8);
                                    Parameter B = instance.LookupParameter("B");
                                    B.Set((d + 15) / 304.8);
                                }
                            }
                            else
                            {
                                double radiusf = 0.0;
                                double.TryParse(data.SectionShape, out radiusf);
                                if (radiusf > 500)
                                {
                                    Parameter A = instance.LookupParameter("A");
                                    A.Set(radiusf / 304.8);
                                    Parameter B = instance.LookupParameter("B");
                                    B.Set((radiusf + 15) / 304.8);
                                }
                            }

                        }
                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {

                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace;
                return Result.Failed;
            }
        }

        private string GetFilePath()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.DefaultExt = "*.txt";
            dlg.Filter = "Text document (*.txt)|*.txt";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            else
            {
                return "";
            }
        }

        private void GetData(string path)
        {
            try
            {
                //读取txt数据，对每一行以tab做split划分，取前三个为x1 y1 z1，后三个为x2 y2 z2
                //不做过多验证
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    string line = "";
                    m_data.Clear();
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] str_arr = line.Split('\t');
                        ProcessedJingData data = new ProcessedJingData();
                        data.Name = str_arr[0] + " - " + str_arr[1];
                        data.JingName = str_arr[2].Trim();
                        float x0 = 0.0f;
                        float y0 = 0.0f;
                        float z0 = 0.0f;
                        if (float.TryParse(str_arr[3], out x0) && float.TryParse(str_arr[4], out y0) && float.TryParse(str_arr[5], out z0))
                        {
                            data.Location = new XYZ(x0, y0, z0).Divide(304.8).Subtract(new XYZ(0.0, 0.0, 250 / 304.8));
                        }
                        float p = 0.0f;
                        if (float.TryParse(str_arr[6], out p))
                        {
                            data.PipeElevation = p / 304.8f;
                        }
                        data.SectionShape = str_arr[7].Trim();
                        data.Material = str_arr[8].Trim();

                        m_data.Add(data);
                    }
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - GET_DATA", "数据格式不正确，请输入正确的数据格式\n\n" + ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace);
            }
        }

    }

    public class ProcessedJingData
    {
        public string Name { get; set; }
        public string JingName { get; set; }
        public XYZ Location { get; set; }
        public float PipeElevation { get; set; }
        public string SectionShape { get; set; }
        public string Material { get; set; }
    }
}
