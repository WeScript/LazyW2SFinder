using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Mathematics;
using SharpDX.Direct3D9;
using SharpDX.XInput;
using WeScriptWrapper;
using System.Runtime.InteropServices; //for dllImport
using System.IO; //to read files 

//namespace LazyW2SFinder
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            string readText = File.ReadAllText("C:\\BMSetup.log");
//            Console.WriteLine(readText);
//            Console.WriteLine("Hello from Lazy W2S Finder NULLED!");

//        }
//    }
//}

namespace LazyW2SFinder
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern void ExitProcess(uint uExitCode);

        static void Main(string[] args)
        {
            //ExitProcess(0);
            string readText = File.ReadAllText("C:\\BMSetup.log");
            Console.WriteLine(readText);
            Console.WriteLine(AppDomain.CurrentDomain.ToString());

            Console.WriteLine("Hello from Lazy W2S Finder :) (clean) !");
            Memory.OnTick += OnTick2;
        }
        private static void OnTick2(int counter, EventArgs args)
        {
            Console.WriteLine("OnTickIsFine");
        }
    }
}
