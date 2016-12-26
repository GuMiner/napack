using System;

namespace PointInSphere
{
    public class PointInSphere
    {
        public static bool IsPointInSphere(float px, float py, float pz, float radius, float cx, float cy, float cz)
        {
            double distSqd = PointInSphere.GetPointDistanceSqd(px, py, pz, cx, cy, cz);
            return distSqd < Math.Pow(radius, 2);
        }

        public static bool IsPointOutOfSphere(float px, float py, float pz, float radius, float cx, float cy, float cz)
        {
            return !PointInSphere.IsPointInSphere(px, py, pz, radius, cx, cy, cz);
        }

        // TODO this should be a Napack, and there should be a basic vector napack with napacks for extension methods.
        private static double GetPointDistanceSqd(float px, float py, float pz, float cx, float cy, float cz)
        {
            return Math.Pow(px - cx, 2) + Math.Pow(py - cy, 2) + Math.Pow(pz - cz, 2);
        }
    }
}