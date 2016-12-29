using System;
using BasicVector_1;

namespace BasicVector.Core
{
    public static class BasicVectorCoreExtensions
    {
		public static BasicVector Add(this BasicVector basicVector, BasicVector other)
		{
			return new BasicVector(basicVector.X + other.X, basicVector.Y + other.Y, basicVector.Z + other.Z);
		}
		
		public static BasicVector Subtract(this BasicVector basicVector, BasicVector other)
		{
			return new BasicVector(basicVector.X - other.X, basicVector.Y - other.Y, basicVector.Z - other.Z);
		}
		
		public static double DistanceSqd(this BasicVector basicVector, BasicVector other)
		{
			BasicVector differenceBasicVector = basicVector.Subtract(other);
			return Math.Pow(differenceBasicVector.X, 2) + Math.Pow(differenceBasicVector.Y, 2) + Math.Pow(differenceBasicVector.Z, 2);
		}
		
		public static double Distance(this BasicVector basicVector, BasicVector other)
		{
			return Math.Sqrt(BasicVectorCoreBasicVectorExtensions.DistanceSqd(basicVector, other));
		}
    }
}