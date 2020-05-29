using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public static class CameraTransformHelper
    {
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="factor"></param>
        /// <param name="minFieldOfView"></param>
        /// <param name="maxFieldOfView"></param>
        public static void ZoomIn(this PerspectiveCamera camera, double factor, double minFieldOfView, double maxFieldOfView)
        {
            if (camera.FieldOfView + factor < minFieldOfView)
            {
                camera.FieldOfView = minFieldOfView;
            }
            else if (camera.FieldOfView + factor > maxFieldOfView)
            {
                camera.FieldOfView = maxFieldOfView;
            }

            camera.FieldOfView += factor;
        }

        /// <summary>
        /// 摄像机绕指定轴旋转
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="rotateAxis"></param>
        /// <param name="rotateAngle"></param>
        public static void RotateAroundAxis(this PerspectiveCamera camera, Vector3D rotateAxis, double rotateAngle)
        {
            Vector3D oldLookDirection = camera.LookDirection;

            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            rotateTransform3D.Rotation = new AxisAngleRotation3D(rotateAxis, rotateAngle);
            Matrix3D matrix = rotateTransform3D.Value;

            //更新摄像机拍摄方向
            Point3D newCameraPosition = matrix.Transform(new Point3D(oldLookDirection.X, oldLookDirection.Y, oldLookDirection.Z));
            camera.LookDirection = new Vector3D(newCameraPosition.X, newCameraPosition.Y, newCameraPosition.Z);

            //更新摄像机向上向量
            Vector3D horizontalVector3D = Vector3D.CrossProduct(new Vector3D(0, -1, 0), camera.LookDirection);
            Vector3D newUpDirection = Vector3D.CrossProduct(horizontalVector3D, camera.LookDirection);
            camera.UpDirection = new Vector3D(newUpDirection.X, newUpDirection.Y, newUpDirection.Z);
        }

        /// <summary>
        /// 原地垂直旋转
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="rotateAngle"></param>
        public static void VerticalRotateInSitu(this PerspectiveCamera camera, double rotateAngle)
        {
            //旋转轴
            Vector3D rotateAxis = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);
            RotateAroundAxis(camera, rotateAxis, rotateAngle);
        }

        /// <summary>
        /// 原地水平旋转
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="rotateAngle"></param>
        public static void HorizontalRotateInSitu(this PerspectiveCamera camera, double rotateAngle)
        {
            //旋转轴
            Vector3D rotateAxis = new Vector3D(0, 1, 0);
            RotateAroundAxis(camera, rotateAxis, rotateAngle);
        }
    }
}
