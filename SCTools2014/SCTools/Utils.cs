using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Architecture;
using System.ComponentModel;

namespace SCTools
{
    public class Utils
    {
        public static List<Element> FilterRoom(Document doc)
        {
            ElementCategoryFilter roomCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
            roomCollector.WherePasses(roomCategoryFilter);

            return roomCollector.ToElements().ToList();
        }

        public static List<Element> FilterLevel(Document doc)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            levelCollector.OfClass(typeof(Level));

            return levelCollector.ToElements().ToList();
        }

        public static List<Element> FilterFloorType(Document doc)
        {
            FilteredElementCollector floorTypeCollector = new FilteredElementCollector(doc);
            floorTypeCollector.OfCategory(BuiltInCategory.OST_Floors).OfClass(typeof(FloorType));

            return floorTypeCollector.ToElements().ToList();
        }

        public static List<Element> FilterWallType(Document doc)
        {
            FilteredElementCollector wallTyptCollector = new FilteredElementCollector(doc);
            wallTyptCollector.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(WallType));

            return wallTyptCollector.ToElements().ToList();
        }
    }

    public class MyElement
    {
        public string MyName { get; private set; }
        public Element Element { get; private set; }

        public MyElement(Element element)
        {
            if (null == element)
            {
                Element = element;
                MyName = "未连接";
            }
            else
            {
                Element = element;
                MyName = "直到标高: " + Element.Name;
            }
        }
    }

    public class MyRoom : INotifyPropertyChanged
    {
        private bool? isChecked;
        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if(isChecked != value)
                {
                    isChecked = value;
                    NotifyPropertyChanged("IsChecked");
                }
            }
        }
        public string DisplayString { get; private set; }

        public Element Element { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MyRoom(Element element)
        {
            Element = element;
            IsChecked = true;
            DisplayString = "房间名:" + ((Room)element)?.Name + " | 标高:" + ((Room)element)?.Level.Name + " | ID:" + element.Id;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
