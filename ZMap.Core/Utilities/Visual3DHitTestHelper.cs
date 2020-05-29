using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public class Visual3DHitTestHelper
    {
        #region Fields

        #endregion

        #region Properties

        public Visual3D HitVisual3D { get; private set; }
        public GeometryModel3D HitGeometry3D { get; private set; }
        public MeshGeometry3D HitMesh3D { get; private set; }
        public Point3D HitPoint3D { get; private set; }

        #endregion

        #region Constructors

        public Visual3DHitTestHelper()
        {

        }

        #endregion

        #region Private Methods

        private void InitHitValues()
        {
            HitVisual3D = null;
            HitGeometry3D = null;
            HitMesh3D = null;
            HitPoint3D = default(Point3D);
        }

        private HitTestResultBehavior HitResultCallback(HitTestResult rawresult)
        {
            HitVisual3D = rawresult.VisualHit as Visual3D;
            if (HitVisual3D == null)
            {
                return HitTestResultBehavior.Continue;
            }

            RayHitTestResult rayResult = rawresult as RayHitTestResult;
            if (rayResult == null)
            {
                return HitTestResultBehavior.Continue;
            }

            RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;
            if (rayMeshResult == null)
            {
                return HitTestResultBehavior.Continue;
            }

            HitGeometry3D = rayMeshResult.ModelHit as GeometryModel3D;
            if (HitGeometry3D == null)
            {
                return HitTestResultBehavior.Continue;
            }

            HitMesh3D = HitGeometry3D.Geometry as MeshGeometry3D;
            if (HitMesh3D == null)
            {
                return HitTestResultBehavior.Continue;
            }

            HitPoint3D = GetHitPoint3DOnMeshGeometry(rayMeshResult, HitMesh3D);
            return HitTestResultBehavior.Stop;
        }

        private Point3D GetHitPoint3DOnMeshGeometry(RayMeshGeometry3DHitTestResult rayMeshResult, MeshGeometry3D hitmesh)
        {
            Point3D p1 = hitmesh.Positions.ElementAt(rayMeshResult.VertexIndex1);
            Point3D p2 = hitmesh.Positions.ElementAt(rayMeshResult.VertexIndex2);
            Point3D p3 = hitmesh.Positions.ElementAt(rayMeshResult.VertexIndex3);

            double weight1 = rayMeshResult.VertexWeight1;
            double weight2 = rayMeshResult.VertexWeight2;
            double weight3 = rayMeshResult.VertexWeight3;

            double x = p1.X * weight1 + p2.X * weight2 + p3.X * weight3;
            double y = p1.Y * weight1 + p2.Y * weight2 + p3.Y * weight3;
            double z = p1.Z * weight1 + p2.Z * weight2 + p3.Z * weight3;

            Point3D hitPoint3D = new Point3D(x, y, z);
            return hitPoint3D;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Public Methods

        public void HitTest(Viewport3D viewport3D, Point mouseposition)
        {
            InitHitValues();

            PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
            VisualTreeHelper.HitTest(viewport3D, null, HitResultCallback, pointparams);
        }

        public void HitTest(Viewport3D viewport3D, Point mouseposition, out Visual3D hitVisual3D, out Point3D hitPoint3D)
        {
            InitHitValues();

            PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
            VisualTreeHelper.HitTest(viewport3D, null, HitResultCallback, pointparams);

            hitVisual3D = HitVisual3D;
            hitPoint3D = HitPoint3D;
        }

        #endregion
    }
}
