using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace SCTools
{
    public class CreateFormEventHandler : IExternalEventHandler
    {
        private UIDocument uiDocument;
        private Document document;

        //public List<XYZ> PointList1 { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<XYZ> PointList1 { get; set; }
        //public List<XYZ> PointList2 { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<XYZ> PointList2 { get; set; }

        public bool IsOnlyCurve { get; set; } = false;
        public bool IsSolid { get; set; } = true;


        public CreateFormEventHandler()
        {

        }
        public void Execute(UIApplication app)
        {
            try
            {
                uiDocument = app.ActiveUIDocument;
                document = uiDocument.Document;
                ReferenceArrayArray referenceArrayArray = null;
                using (Transaction ts = new Transaction(document, "生成点"))
                {
                    if (ts.Start() == TransactionStatus.Started)
                    {
                        referenceArrayArray = GetReferenceArrayArray3(document);
                    }
                    if (ts.Commit() == TransactionStatus.Committed)
                    {

                    }
                }
                if (IsOnlyCurve == false)
                {
                    using (Transaction ts = new Transaction(document, "生成体量"))
                    {
                        if (ts.Start() == TransactionStatus.Started)
                        {
                            document.FamilyCreate.NewLoftForm(IsSolid, referenceArrayArray);
                        }
                        if (ts.Commit() == TransactionStatus.Committed)
                        {

                        }
                    }
                }
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                TaskDialog.Show("Error", "只能在族编辑器中使用该功能\n请打开公制体量族，再尝试此功能");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error - EXECUTE_ERROR", ex.Message + "\n------\nTargetSite:\n" + ex.TargetSite.ToString() + "\n------\nStackTrace:\n" + ex.StackTrace + "\n------\nName:\n" + ex.ToString());
            }
        }

        //以模型线方法生成参照数组，但存在问题。很多情况无法生成体量，不确定是否因为模型线的参照平面歪歪扭扭不规整造成的？
        /*
        private ReferenceArrayArray GetReferenceArrayArray(Document document)
        {

            ReferenceArray referenceArray1 = new ReferenceArray();
            ReferenceArray referenceArray2 = new ReferenceArray();
            for (int i = 0; i < PointList1.Count; ++i)
            {
                //从点列表生成线并添加到ReferenceArray1
                if (i < PointList1.Count - 1)
                {
                    Line geomLine = Line.CreateBound(PointList1[i].Divide(304.8), PointList1[i + 1].Divide(304.8));
                    Plane geomPlane = Plane.CreateByThreePoints(new XYZ(0, 0, 0), PointList1[i].Divide(304.8), PointList1[i + 1].Divide(304.8));
                    SketchPlane sketchPlane = SketchPlane.Create(document, geomPlane);
                    ModelLine line = document.FamilyCreate.NewModelCurve(geomLine, sketchPlane) as ModelLine;
                    referenceArray1.Append(line.GeometryCurve.Reference);
                }
                else
                {
                    Line geomLine = Line.CreateBound(PointList1[i].Divide(304.8), PointList1[0].Divide(304.8));
                    Plane geomPlane = Plane.CreateByThreePoints(new XYZ(0, 0, 0), PointList1[i].Divide(304.8), PointList1[0].Divide(304.8));
                    SketchPlane sketchPlane = SketchPlane.Create(document, geomPlane);
                    ModelLine line = document.FamilyCreate.NewModelCurve(geomLine, sketchPlane) as ModelLine;
                    referenceArray1.Append(line.GeometryCurve.Reference);
                }
            }

            //从点列表生成线并添加到ReferenceArray2
            for (int i = 0; i < PointList2.Count; ++i)
            {
                if (i < PointList2.Count - 1)
                {
                    //TaskDialog.Show("te", PointList1[i].X.ToString() + "|" + PointList1[i].Y.ToString() + "|" + PointList1[i].Z.ToString());
                    //TaskDialog.Show("te", PointList1[i+1].X.ToString() + "|" + PointList1[i + 1].Y.ToString() + "|" + PointList1[i + 1].Z.ToString());
                    Line geomLine = Line.CreateBound(PointList2[i].Divide(304.8), PointList2[i + 1].Divide(304.8));
                    Plane geomPlane = Plane.CreateByThreePoints(new XYZ(0, 0, 0), PointList2[i].Divide(304.8), PointList2[i + 1].Divide(304.8));
                    SketchPlane sketchPlane = SketchPlane.Create(document, geomPlane);
                    ModelLine line = document.FamilyCreate.NewModelCurve(geomLine, sketchPlane) as ModelLine;

                    referenceArray2.Append(line.GeometryCurve.Reference);
                }
                else
                {
                    Line geomLine = Line.CreateBound(PointList2[i].Divide(304.8), PointList2[0].Divide(304.8));
                    Plane geomPlane = Plane.CreateByThreePoints(new XYZ(0, 0, 0), PointList2[i].Divide(304.8), PointList2[0].Divide(304.8));
                    SketchPlane sketchPlane = SketchPlane.Create(document, geomPlane);
                    ModelLine line = document.FamilyCreate.NewModelCurve(geomLine, sketchPlane) as ModelLine;

                    referenceArray2.Append(line.GeometryCurve.Reference);
                }
            }

            ReferenceArrayArray referenceArrayArray = new ReferenceArrayArray();
            referenceArrayArray.Append(referenceArray1);
            referenceArrayArray.Append(referenceArray2);

            return referenceArrayArray;

        }
        */

        //将所有参照点（大于两个）直接放入ReferencePointArray，再生成CurveByPoints，这样子取得的曲线不是多边形折线，而是样条曲线？
        //两个参照点生成CurveByPoints，得到的是直线
        private ReferenceArrayArray GetReferenceArrayArray2(Document document)
        {
            ReferencePointArray referencePointArray1 = new ReferencePointArray();
            ReferencePointArray referencePointArray2 = new ReferencePointArray();

            foreach (XYZ xyz in PointList1)
            {
                ReferencePoint rp = document.FamilyCreate.NewReferencePoint(xyz.Divide(304.8));
                referencePointArray1.Append(rp);
            }

            foreach (XYZ xyz in PointList2)
            {
                ReferencePoint rp = document.FamilyCreate.NewReferencePoint(xyz.Divide(304.8));
                referencePointArray2.Append(rp);
            }

            CurveByPoints curve1 = document.FamilyCreate.NewCurveByPoints(referencePointArray1);
            CurveByPoints curve2 = document.FamilyCreate.NewCurveByPoints(referencePointArray2);
            ReferenceArray referenceArray1 = new ReferenceArray();
            referenceArray1.Append(curve1.GeometryCurve.Reference);
            ReferenceArray referenceArray2 = new ReferenceArray();
            referenceArray2.Append(curve2.GeometryCurve.Reference);

            ReferenceArrayArray referenceArrayArray = new ReferenceArrayArray();
            referenceArrayArray.Append(referenceArray1);
            referenceArrayArray.Append(referenceArray2);
            return referenceArrayArray;
        }

        //循环每两个点生成一个CurveByPoints，将这些CurveByPoints放入同一个ReferenceArray，这样子是多边形折线。再将多个折线生成ReferenceArrayArray
        private ReferenceArrayArray GetReferenceArrayArray3(Document document)
        {
            ReferenceArray referenceArray1 = new ReferenceArray();
            for (int i = 0; i < PointList1.Count; ++i)
            {
                if (i < PointList1.Count - 1)
                {
                    ReferencePointArray referencePointArray = new ReferencePointArray();
                    ReferencePoint rp1 = document.FamilyCreate.NewReferencePoint(PointList1[i].Divide(304.8));
                    ReferencePoint rp2 = document.FamilyCreate.NewReferencePoint(PointList1[i + 1].Divide(304.8));
                    referencePointArray.Append(rp1);
                    referencePointArray.Append(rp2);
                    CurveByPoints curve = document.FamilyCreate.NewCurveByPoints(referencePointArray);
                    referenceArray1.Append(curve.GeometryCurve.Reference);
                }
                else
                {
                    ReferencePointArray referencePointArray = new ReferencePointArray();
                    ReferencePoint rp1 = document.FamilyCreate.NewReferencePoint(PointList1[i].Divide(304.8));
                    ReferencePoint rp2 = document.FamilyCreate.NewReferencePoint(PointList1[0].Divide(304.8));
                    referencePointArray.Append(rp1);
                    referencePointArray.Append(rp2);
                    CurveByPoints curve = document.FamilyCreate.NewCurveByPoints(referencePointArray);
                    referenceArray1.Append(curve.GeometryCurve.Reference);
                }
            }

            ReferenceArray referenceArray2 = new ReferenceArray();
            for (int i = 0; i < PointList2.Count; ++i)
            {
                if (i < PointList2.Count - 1)
                {
                    ReferencePointArray referencePointArray = new ReferencePointArray();
                    ReferencePoint rp1 = document.FamilyCreate.NewReferencePoint(PointList2[i].Divide(304.8));
                    ReferencePoint rp2 = document.FamilyCreate.NewReferencePoint(PointList2[i + 1].Divide(304.8));
                    referencePointArray.Append(rp1);
                    referencePointArray.Append(rp2);
                    CurveByPoints curve = document.FamilyCreate.NewCurveByPoints(referencePointArray);
                    referenceArray2.Append(curve.GeometryCurve.Reference);
                }
                else
                {
                    ReferencePointArray referencePointArray = new ReferencePointArray();
                    ReferencePoint rp1 = document.FamilyCreate.NewReferencePoint(PointList2[i].Divide(304.8));
                    ReferencePoint rp2 = document.FamilyCreate.NewReferencePoint(PointList2[0].Divide(304.8));
                    referencePointArray.Append(rp1);
                    referencePointArray.Append(rp2);
                    CurveByPoints curve = document.FamilyCreate.NewCurveByPoints(referencePointArray);
                    referenceArray2.Append(curve.GeometryCurve.Reference);
                }
            }

            ReferenceArrayArray referenceArrayArray = new ReferenceArrayArray();
            referenceArrayArray.Append(referenceArray1);
            referenceArrayArray.Append(referenceArray2);
            return referenceArrayArray;
        }

        public string GetName()
        {
            return "Create Form";
        }
    }
}
