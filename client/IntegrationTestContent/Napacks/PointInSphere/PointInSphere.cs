using System;
using BasicVector_1;
using BasicVector_Core_1;

namespace PointInSphere
{
    public class PointInSphere
    {
        public static bool IsPointInSphere(Vector point, float radius, Vector sphereCenter)
        {
            return point.DistanceSqd(sphereCenter) < Math.Pow(radius, 2);
        }

        public static bool IsPointOutOfSphere(Vector point, float radius, Vector sphereCenter)
        {
            return !PointInSphere.IsPointInSphere(point, radius, sphereCenter);
        }
    }
}