using System;
 
namespace HelloWorld
{
    class Hello {         
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string text;
            Console.Write("Enter string:");
            text = Console.ReadLine();
            Console.WriteLine("You entered : {text}");
        }
    }
}
