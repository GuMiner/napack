using System;

namespace BasicVector
{
    public class BasicVector
    {
		public BasicVector()
		{
			this.X = 0;
			this.Y = 0;
			this.Z = 0;
		}
		
		public BasicVector(double x, double y, double z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}
		
        public double X { get; set; }
		
		public double Y { get; set; }
		
		public double Z { get; set; }
    }
}