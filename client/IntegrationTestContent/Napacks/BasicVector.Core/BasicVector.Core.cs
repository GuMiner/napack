using System;
using BasicVector.1;

namespace BasicVector.Core
{
    public static class BasicVectorCoreExtensions
    {
		public static BasicVector Add(this BasicVector BasicVector, BasicVector other)
		{
			return new BasicVector(BasicVector.X + other.X, BasicVector.Y + other.Y, BasicVector.Z + other.Z);
		}
		
		public static BasicVector Subtract(this BasicVector BasicVector, BasicVector other)
		{
			return new BasicVector(BasicVector.X - other.X, BasicVector.Y - other.Y, BasicVector.Z - other.Z);
		}
		
		public static double DistanceSqd(this BasicVector BasicVector, BasicVector other)
		{
			BasicVector differenceBasicVector = BasicVector.Subtract(other);
			return Math.Pow(differenceBasicVector.X, 2) + Math.Pow(differenceBasicVector.Y, 2) + Math.Pow(differenceBasicVector.Z, 2);
		}
		
		public static double Distance(this BasicVector BasicVector, BasicVector other)
		{
			return Math.Sqrt(BasicVectorCoreBasicVectorExtensions.DistanceSqd(BasicVector, other));
		}
    }
}