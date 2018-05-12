using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Exam_Prep
{
    class Program
    {
        static void Main(string[] args)
        {
            string regex = @"^\($"; 
            if(Regex.IsMatch("(", regex))
            {
                Console.WriteLine("sadsa");
            }


            double x = 15e3;
            Console.WriteLine(x);
        }
    }
}
