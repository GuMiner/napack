using System;
using BasicVector_1;
using BasicVector_Core_1;

namespace PointInCylinder
{
    public class PointInCylinder
    {
        public static bool IsPointInZAlignedCylinder(Vector point, float radius, float height, Vector cylinderCenter)
        {
			double verticalDifference = Math.Abs(point.Z - cylinderCenter.Z);
            return verticalDifference < height / 2.0 && point.DistanceSqd(new BasicVector(cylinderCenter.X, cylinderCenter.Y, point.Z) < Math.Pow(radius, 2);
        }

        public static bool IsPointOutOfZAlignedCylinder(Vector point, float radius, float height, Vector cylinderCenter)
        {
            return !PointInCylinder.IsPointInCylinder(point, radius, height, cylinderCenter);
        }
    }
}