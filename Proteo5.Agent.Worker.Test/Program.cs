using System;

namespace Proteo5.Agent.Worker.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{DateTime.Now:u} - Worker Test Start");
            Console.WriteLine($"{DateTime.Now:u} - Worker Step 1. Wait 2 seconds.");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine($"{DateTime.Now:u} - Worker Step 2. Wait 5 seconds.");
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine($"{DateTime.Now:u} - Worker Step 3. Wait 3 seconds.");
            System.Threading.Thread.Sleep(3000);
            Console.WriteLine($"{DateTime.Now:u} - Worker Test Finish");
        }
    }
}
