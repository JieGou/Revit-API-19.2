using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SingleData;

namespace Utility
{
    public static class SolidUtil
    {
        public static double GetProximityHeight(this Element elem)
        {
            var bb = elem.get_BoundingBox(null);
            return bb.Max.Z - bb.Min.Z;
        }
        public static Solid MoveToOrigin(this Solid solid)
        {
            var bb = solid.GetBoundingBox();
            var tf = bb.Transform;
            var origin = tf.Origin;
            var translationTf = Autodesk.Revit.DB.Transform.CreateTranslation(-origin);
            return Autodesk.Revit.DB.SolidUtils.CreateTransformed(solid, translationTf);
        }
        public static Solid GetSingleSolid(this Element element)
        {
            var geoElem = element.get_Geometry(new Options());
            return geoElem.GetSingleSolid();
        }
        public static Solid GetSingleSolid(this IEnumerable<GeometryObject> geoObjs)
        {
            foreach (GeometryObject geoObj in geoObjs)
            {
                if(geoObj is GeometryInstance)
                {
                    var s = (geoObj as GeometryInstance).GetSingleSolid();
                    if (s != null) return s;
                }
                if(geoObj is Solid)
                {
                    var solid = geoObj as Solid;
                    if(solid != null && solid.Faces.Size != 0 && solid.Edges.Size != 0)
                    {
                        return solid;
                    }
                }
            }
            return null;
        }
        public static Solid GetSingleSolid(this GeometryInstance geoIns)
        {
            return GetSingleSolid(geoIns.GetInstanceGeometry());
        }
        public static Solid GetOrigin(this Element element)
        {
            if(element is FamilyInstance)
            {
                var fi = element as FamilyInstance;
                var orgGeoElem = fi.GetOriginalGeometry(new Options());
                var solid = GetSingleSolid(orgGeoElem);
                var fiTrf = fi.GetTransform();
                return Autodesk.Revit.DB.SolidUtils.CreateTransformed(solid, fiTrf);
            }
            if(element is Floor)
            {
                var floor = element as Floor;
                var bottomRf = Autodesk.Revit.DB.HostObjectUtils.GetBottomFaces(floor).First();
                var bottomFace = element.GetGeometryObjectFromReference(bottomRf) as PlanarFace;
                return Autodesk.Revit.DB.GeometryCreationUtilities.CreateExtrusionGeometry(bottomFace.GetEdgesAsCurveLoops(),
                                                                  -bottomFace.FaceNormal, floor.GetProximityHeight());
            }
            if(element is Wall)
            {
                var wall = element as Wall;
                var sideRf = Autodesk.Revit.DB.HostObjectUtils.GetSideFaces(wall,Autodesk.Revit.DB.ShellLayerType.Exterior).First();
                var sideFace = element.GetGeometryObjectFromReference(sideRf) as PlanarFace;
                return Autodesk.Revit.DB.GeometryCreationUtilities.CreateExtrusionGeometry(sideFace.GetEdgesAsCurveLoops(),
                                                                   -sideFace.FaceNormal, wall.Width);
            }
            throw new Exception();
        }
        public static Solid Scale(this Solid solid, double factor)
        {
            var centerPoint = solid.ComputeCentroid();
            var tf = Autodesk.Revit.DB.Transform.Identity.ScaleBasis(factor);
            var scaleSolid = Autodesk.Revit.DB.SolidUtils.CreateTransformed(solid, tf);
            var newCenterPoint = scaleSolid.ComputeCentroid();
            var translateVec = centerPoint - newCenterPoint;
            var translateTf = Autodesk.Revit.DB.Transform.CreateTranslation(translateVec);

            return Autodesk.Revit.DB.SolidUtils.CreateTransformed(scaleSolid, translateTf);
        }
        public static Solid Merge(this IEnumerable<Solid> solids)
        {
            var mergeSolid = solids.First();
            foreach (var solid in solids)
            {
                if (mergeSolid == solid)
                    continue;
                mergeSolid = Autodesk.Revit.DB.BooleanOperationsUtils.ExecuteBooleanOperation(mergeSolid, solid, Autodesk.Revit.DB.BooleanOperationsType.Union);
            }
            return mergeSolid;
        }
        public static Solid Difference(this Solid targetSolid, IEnumerable<Solid> otherSolid)
        {
            var mergeOtherSolid = otherSolid.Merge();
            var mergeAllSolid = BooleanOperationsUtils.ExecuteBooleanOperation(mergeOtherSolid, targetSolid, BooleanOperationsType.Union);
            return BooleanOperationsUtils.ExecuteBooleanOperation(mergeAllSolid, mergeOtherSolid, BooleanOperationsType.Difference);
        }
        public static BoundingBoxXYZ ScaleBoundingBox(BoundingBoxXYZ bb, double a)
        {
            var min = bb.Min; var max = bb.Max; var origin = (min + max) / 2;
            return new BoundingBoxXYZ
            { Min = origin + (min - origin) * a, Max = origin + (max - origin) * a };
        }
    }
}
