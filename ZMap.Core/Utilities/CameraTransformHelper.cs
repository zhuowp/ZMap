using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public static class CameraTransformHelper
    {
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

        public static void VerticalRotateInSitu(this PerspectiveCamera camera, double rotateAngle)
        {
            //旋转轴
            Vector3D rotateAxis = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);

            //角度旋转
            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            rotateTransform3D.Rotation = new AxisAngleRotation3D(rotateAxis, rotateAngle);
            Matrix3D matrix = rotateTransform3D.Value;

            //更新摄像机拍摄方向
            Point3D newCameraPosition = matrix.Transform(new Point3D(camera.LookDirection.X, camera.LookDirection.Y, camera.LookDirection.Z));
            camera.LookDirection = new Vector3D(newCameraPosition.X, newCameraPosition.Y, newCameraPosition.Z);

            //更新摄像机向上向量
            Vector3D newUpDirection = Vector3D.CrossProduct(rotateAxis, camera.LookDirection);
            newUpDirection.Normalize();
            camera.UpDirection = newUpDirection;
        }

        public static void HorizontalRotateInSitu(this PerspectiveCamera camera, double rotateAngle)
        {
            Vector3D oldLookDirection = camera.LookDirection;
            Vector3D oldUpDirection = camera.UpDirection;

            //角度旋转
            Vector3D rotateAxis = new Vector3D(0, 1, 0);
            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            rotateTransform3D.Rotation = new AxisAngleRotation3D(rotateAxis, rotateAngle);
            Matrix3D matrix = rotateTransform3D.Value;

            //更新摄像机拍摄方向
            Point3D newCameraPosition = matrix.Transform(new Point3D(oldLookDirection.X, oldLookDirection.Y, oldLookDirection.Z));
            camera.LookDirection = new Vector3D(newCameraPosition.X, newCameraPosition.Y, newCameraPosition.Z);

            //更新摄像机向上向量
            Point3D newUpDirection1 = matrix.Transform(new Point3D(oldUpDirection.X, oldUpDirection.Y, oldUpDirection.Z));
            camera.UpDirection = new Vector3D(newUpDirection1.X, newUpDirection1.Y, newUpDirection1.Z);
        }

    }
}
