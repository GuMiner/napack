using System;   
using BasicVector_1;
using BasicVector_Core_1;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Demo -- A simple demonstration application illustrating Napack usage.");

            BasicVector vector = new BasicVector(1.0, 2.0, 3.0);
            BasicVector secondaryVector = new BasicVector(2.0, 3.0, 4.0);
            BasicVector additionResult = vector.Add(secondaryVector);
            Console.WriteLine($"Test Vector: ({vector.X}, {vector.Y}, {vector.Z})");
            Console.WriteLine("Done.");
        }
    }
}
