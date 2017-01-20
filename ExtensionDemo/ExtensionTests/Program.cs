using System;
using BasicVector_1;
using BasicVector_Core_1;

namespace ExtensionTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicVector vector = new BasicVector(1.0, 2.0, 1.0);
            BasicVector secondaryVector = new BasicVector(2.0, 1.0, 2.0);
            Console.WriteLine("Napack Extension demo test: " + vector.Add(secondaryVector).X);
        }
    }
}
