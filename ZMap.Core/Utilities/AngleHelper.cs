using System;
using System.Collections.Generic;
using System.Text;

namespace ZMap.Core
{
    public static class AngleHelper
    {
        private const double RADIAN_TO_DEGREE_COEFFICIENT = 57.295779513082323;
        private const double DEGREE_TO_RADIAN_COEFFICIENT = 0.017453292519943295;

        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static double ToDegree(this int radian)
        {
            return radian * RADIAN_TO_DEGREE_COEFFICIENT;
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static double ToDegree(this float radian)
        {
            return radian * RADIAN_TO_DEGREE_COEFFICIENT;
        }

        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static double ToDegree(this double radian)
        {
            return radian * RADIAN_TO_DEGREE_COEFFICIENT;
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double ToRadian(this int degree)
        {
            return degree * DEGREE_TO_RADIAN_COEFFICIENT;
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double ToRadian(this float degree)
        {
            return degree * DEGREE_TO_RADIAN_COEFFICIENT;
        }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double ToRadian(this double degree)
        {
            return degree * DEGREE_TO_RADIAN_COEFFICIENT;
        }
    }
}
