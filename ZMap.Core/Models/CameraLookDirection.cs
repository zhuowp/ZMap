using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public struct CameraLookDirection
    {
        #region Properties

        public Vector3D LookDirection { get; private set; }
        public Vector3D UpDirection { get; private set; }

        #endregion

        #region Constructors

        public CameraLookDirection(Vector3D lookDirection, Vector3D upDirection)
        {
            LookDirection = lookDirection;
            UpDirection = upDirection;
        }

        #endregion

        #region Public Methods

        public static bool operator ==(CameraLookDirection left, CameraLookDirection right)
        {
            return left.LookDirection == right.LookDirection
                && left.UpDirection == right.UpDirection;
        }

        public static bool operator !=(CameraLookDirection left, CameraLookDirection right)
        {
            return left.LookDirection != right.LookDirection
                || left.UpDirection != right.UpDirection;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CameraLookDirection))
            {
                return false;
            }

            CameraLookDirection lookDirection = (CameraLookDirection)obj;
            return this == lookDirection;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("LookDirection:{0},{1},{2}  UpDirection:{3},{4},{5}",
                LookDirection.X, LookDirection.Y, LookDirection.Z, UpDirection.X, UpDirection.Y, UpDirection.Z);
        }

        #endregion
    }
}
