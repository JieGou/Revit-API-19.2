﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;
using Utility;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SingleData;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;

namespace SingleData
{
    public class MEPData
    {
        private static MEPData instance;
        public static MEPData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MEPData();
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private RevitData revitData
        {
            get
            {
                return RevitData.Instance;
            }
        }

        // Sửa lại thuật toán để lấy ra các đối tượng MEPElement định nghĩa các đối tượng MEP nói chung
        private IEnumerable<Element> mepElements;
        public IEnumerable<Element> MEPElements
        {
            get
            {
                if (mepElements == null)
                {
                    mepElements = revitData.InstanceElements.Where(x => x is CableTray || x is MEPCurve ||
                        (x is FamilyInstance && x.Category != null && x.Category.IsEqual(BuiltInCategory.OST_MechanicalEquipment)));
                }
                return mepElements;
            }
            set
            {
                mepElements = value;
            }
        }

        // Nên truy xuất thêm đối tượng MEPCurve là đối tượng để truy xuất các đối tượng khác liên quan
        // MEPCurve -> Pipe, PipeInsulation - Duct, DuctInsulation
        private IEnumerable<MEPCurve> mepCurves;
        public IEnumerable<MEPCurve> MEPCurves
        {
            get
            {
                if (mepCurves == null)
                {
                    // Sử dụng OfType để lấy ra các đối tượng thuộc kiểu dữ liệu đơn giãn hơn thay vì dùng Where + Cast
                    mepCurves = MEPElements.OfType<MEPCurve>();
                }
                return mepCurves;
            }
            set
            {
                mepCurves = value;
            }
        }

        private IEnumerable<MEPCurve> mepPipes;
        public virtual IEnumerable<MEPCurve> MepPipes
        {
            get
            {
                if (mepPipes == null)
                {
                    // Kiểm tra như vậy là sai vì mepPipes thuộc kiểu IEnumerable<Autodesk.Revit.DB.MEPCurve> chứ không phải kiểu Pipe
                    //if (mepPipes is Pipe)
                    //{
                    //    mepPipes = RevitData.Instance.InstanceElements.Where(x => x.Category != null && x is Pipe).Cast<Pipe>();
                    //}
                    //if (mepPipes is PipeInsulation)
                    //{
                    //    mepPipes = RevitData.Instance.InstanceElements.Where(x => x.Category != null && x is PipeInsulation).Cast<PipeInsulation>();
                    //}

                    // Ở đây không cần dùng Cast vì đang cần truy xuất các đối tượng kiểu dữ liệu MEPCurve
                    mepPipes = MEPCurves.Where(x => x is Pipe || x is PipeInsulation);
                }
                return mepPipes;
            }
            set
            {
                mepPipes = value;
            }
        }

        private IEnumerable<MEPCurve> mepDucts;
        public virtual IEnumerable<MEPCurve> MepDucts
        {
            get
            {
                if (mepDucts == null)
                {
                    //{
                    //    if (mepDucts is Duct)
                    //    {
                    //        mepDucts = RevitData.Instance.InstanceElements.Where(x => x.Category != null && x is Duct).Cast<Duct>();
                    //    }
                    //    if (mepDucts is DuctInsulation)
                    //    {
                    //        mepDucts = RevitData.Instance.InstanceElements.Where(x => x.Category != null && x is DuctInsulation).Cast<DuctInsulation>();
                    //    }
                    //}

                    mepDucts = MEPCurves.Where(x => x is Duct || x is DuctInsulation);
                }
                return mepDucts;
            }
            set
            {
                mepDucts = value;
            }
        }

        private IEnumerable<CableTray> cableTrays;
        public virtual IEnumerable<CableTray> CableTrays
        {
            get
            {
                if (cableTrays == null)
                {
                    //cableTrays = RevitData.Instance.InstanceElements.Where(x => x.Category != null && x is CableTray).Cast<CableTray>();

                    // Truy xuất từ đối tượng MEPElement chứa tất cả các đối tượng MEP trong dự án
                    // Sử dụng OfType để lấy ra các đối tượng thuộc kiểu dữ liệu đơn giãn hơn thay vì dùng Where + Cast
                    cableTrays = MEPElements.OfType<CableTray>();
                }
                return cableTrays;
            }
            set
            {
                cableTrays = value;
            }
        }

        private IEnumerable<FamilyInstance> mechEquipments;
        public virtual IEnumerable<FamilyInstance> MechEquipments
        {
            get
            {
                if (mechEquipments == null)
                {
                    //mechEquipments = RevitData.Instance.FamilyInstances.Where
                    //                 (x => x.Category.IsEqual(BuiltInCategory.OST_MechanicalEquipment));

                    // Truy xuất từ đối tượng MEPElement chứa tất cả các đối tượng MEP trong dự án
                    mechEquipments = MEPElements.OfType<FamilyInstance>().Where(x=> x.Category != null && x.Category.IsEqual(BuiltInCategory.OST_MechanicalEquipment));
                }
                return mechEquipments;
            }
            set
            {
                mechEquipments = value;
            }
        }

        // Truy xuất như vậy là sai, các đối tượng phải thuộc kiểu Element chứ không phải MEPCurve vì CableTray hay MechEquipments không thuộc kiểu MEPCurve
        //private IEnumerable<Autodesk.Revit.DB.MEPCurve> mepElements;
        //public IEnumerable<Autodesk.Revit.DB.MEPCurve> MepElements
        //{
        //    get
        //    {
        //        if (mepElements == null)
        //        {
        //            foreach (var elem in mepElements)
        //            {
        //                var mepIns = RevitData.Instance.InstanceElements;

        //                if(elem is CableTray)
        //                    return mepIns.Where(x => x.Category != null && x is CableTray).Cast<CableTray>();
        //                if (elem is Duct)
        //                    return mepIns.Where(x => x.Category != null && x is Duct).Cast<Duct>();
        //                if (elem is DuctInsulation)
        //                    return mepIns.Where(x => x.Category != null && x is DuctInsulation).Cast<DuctInsulation>();
        //                if (elem is Pipe)
        //                    return mepIns.Where(x => x.Category != null && x is Pipe).Cast<Pipe>();
        //                if (elem is PipeInsulation)
        //                    return mepIns.Where(x => x.Category != null && x is PipeInsulation).Cast<PipeInsulation>();
        //            }
        //        }
        //        return mepElements;
        //    }     
        //}
    }
}
