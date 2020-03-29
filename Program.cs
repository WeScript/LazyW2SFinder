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

        public static IntPtr procHnd = IntPtr.Zero;
        public static uint PROCESS_ALL_ACCESS = 0x1FFFFF;
        public static bool is64bit = false;
        public static bool drawyourstuff = false;
        public static bool isGameOnTop = false;
        public static Vector2 wndMargins = new Vector2(0, 0);
        public static Vector2 wndSize = new Vector2(0, 0);
        public static bool startScanningW2S = false;
        public static IntPtr gameBase = IntPtr.Zero;
        public static IntPtr gameSize = IntPtr.Zero;
        public static ulong[] returnedAddresses = new ulong[1000000];
        public static Matrix[] returnedMatrixes = new Matrix[1000000];
        public static int matrixesFnd = 0;
        public static int indexToUse = 0;
        //public static Vector3 vecToSearchFor = new Vector3(-1892.315063f, -586.0297241f, 11.85083485f); //GTA5 first mission point
        public static Vector3 vecToSearchFor = new Vector3(1.0f, 1.0f, 1.0f); //dust2 pillar close to terrorists
        public static bool pgUP = false;
        public static bool pgDN = false;
        public static ulong timertick = 0;
        public static bool drawAllAddressesAtOnce = true;
        public static bool dynamicPosition = true;


        static void Main(string[] args)
        {
            //ExitProcess(0);
            //string readText = File.ReadAllText("C:\\BMSetup.log");
            //Console.WriteLine(readText);
            Console.WriteLine(AppDomain.CurrentDomain.ToString());

            Console.WriteLine("Hello from Lazy W2S Finder :) (цлеан)!");
            //Renderer.OnRenderer += OnRenderer;
            //Memory.OnTick += OnTick;
            //Input.OnInput += OnInput;
        }


        private static void OnTick(int counter, EventArgs args)
        {
            if (procHnd == IntPtr.Zero)
            {
                var wndHnd = Memory.FindWindowName("Counter-Strike");
                if (wndHnd != IntPtr.Zero)
                {
                    var gamePID = Memory.GetPIDFromHWND(wndHnd);
                    if (gamePID > 0)
                    {
                        Renderer.ForceGameToBorderless(wndHnd);
                        procHnd = Memory.OpenProcess(PROCESS_ALL_ACCESS, gamePID);
                        if (procHnd != IntPtr.Zero)
                        {
                            is64bit = Memory.IsProcess64Bit(procHnd);
                        }
                    }
                }
            }
            else
            {
                var wndHnd = Memory.FindWindowName("Counter-Strike");
                if (wndHnd != IntPtr.Zero)
                {
                    drawyourstuff = true;
                    wndMargins = Renderer.GetWindowMargins(wndHnd);
                    wndSize = Renderer.GetWindowSize(wndHnd);
                    isGameOnTop = Renderer.IsGameOnTop(wndHnd);

                    if (startScanningW2S)
                    {
                        startScanningW2S = false;
                        Console.WriteLine("Starting to scan for possible matrix in the game!");
                        gameBase = Memory.GetModule(procHnd, "hl.exe");
                        gameSize = Memory.GetModuleSize(procHnd, "hl.exe");
                        Console.WriteLine($"Found GameBase at: {gameBase.ToString("X")} and GameSize: {gameSize.ToString("X")}");
                        matrixesFnd = Memory.FindPossibleMatrix(procHnd, gameBase, gameSize, vecToSearchFor, out returnedAddresses, (int)W2SType.TypeCustom1);
                        Console.WriteLine($"Total possible matrix found: {matrixesFnd.ToString()}");
                        for (uint i = 0; i <= matrixesFnd; i++)
                        {
                            //Console.WriteLine($"Addresses: {returnedAddresses[i].ToString("X")}");
                            returnedMatrixes[i] = Memory.ReadMatrix(procHnd, (IntPtr)returnedAddresses[i]);
                        }
                    }

                    //Memory.TickCount
                    if ((timertick + 60) < Memory.TickCount)
                    {
                        if (pgUP)
                        {
                            timertick = Memory.TickCount;
                            do
                            {
                                indexToUse++;
                                if (indexToUse > matrixesFnd)
                                {
                                    indexToUse = matrixesFnd;
                                    break;
                                }
                            }
                            while (returnedAddresses[indexToUse] == 0);
                        }
                        if (pgDN)
                        {
                            timertick = Memory.TickCount;
                            do
                            {
                                indexToUse--;
                                if (indexToUse < 0)
                                {
                                    indexToUse = 0;
                                    break;
                                }
                            }
                            while (returnedAddresses[indexToUse] == 0);
                        }
                    }


                }
                else
                {
                    Memory.CloseHandle(procHnd);
                    procHnd = IntPtr.Zero;
                    drawyourstuff = false;
                }
            }
        }

        private static void OnInput(VirtualKeyCode key, bool isPressed, EventArgs args)
        {
            if (key == VirtualKeyCode.Delete)
            {
                if (isPressed)
                {
                    matrixesFnd = 0;
                    startScanningW2S = true;
                }
            }
            if (key == VirtualKeyCode.Home)
            {
                if (isPressed)
                {
                    Console.WriteLine("Attempting to clean from invalid/static matrixes!");
                    for (uint i = 0; i <= matrixesFnd; i++)
                    {
                        if (returnedAddresses[i] != 0)
                        {
                            var tempMatrix = Memory.ReadMatrix(procHnd, (IntPtr)returnedAddresses[i]);
                            if ((tempMatrix.M11 == returnedMatrixes[i].M11) && (tempMatrix.M12 == returnedMatrixes[i].M12) && (tempMatrix.M13 == returnedMatrixes[i].M13) && (tempMatrix.M14 == returnedMatrixes[i].M14)
                                   && (tempMatrix.M21 == returnedMatrixes[i].M21) && (tempMatrix.M22 == returnedMatrixes[i].M22) && (tempMatrix.M23 == returnedMatrixes[i].M23) && (tempMatrix.M24 == returnedMatrixes[i].M24)
                                   && (tempMatrix.M31 == returnedMatrixes[i].M31) && (tempMatrix.M32 == returnedMatrixes[i].M32) && (tempMatrix.M33 == returnedMatrixes[i].M33) && (tempMatrix.M34 == returnedMatrixes[i].M34)
                                   && (tempMatrix.M41 == returnedMatrixes[i].M41) && (tempMatrix.M42 == returnedMatrixes[i].M42) && (tempMatrix.M43 == returnedMatrixes[i].M43) && (tempMatrix.M44 == returnedMatrixes[i].M44))
                            {
                                returnedAddresses[i] = 0;
                            }
                        }
                    }
                    for (uint i = 0; i <= matrixesFnd; i++)
                    {
                        if (returnedAddresses[i] != 0)
                        {
                            returnedMatrixes[i] = Memory.ReadMatrix(procHnd, (IntPtr)returnedAddresses[i]);
                        }
                    }
                    Console.WriteLine("Cleaning complete!");
                }
            }
            if (key == VirtualKeyCode.End)
            {
                if (isPressed)
                {
                    Console.WriteLine("Attempting to clean from constantly changing matrixes!");
                    for (uint i = 0; i <= matrixesFnd; i++)
                    {
                        if (returnedAddresses[i] != 0)
                        {
                            var tempMatrix = Memory.ReadMatrix(procHnd, (IntPtr)returnedAddresses[i]);
                            if ((tempMatrix.M11 != returnedMatrixes[i].M11) || (tempMatrix.M12 != returnedMatrixes[i].M12) || (tempMatrix.M13 != returnedMatrixes[i].M13) || (tempMatrix.M14 != returnedMatrixes[i].M14)
                                   && (tempMatrix.M21 != returnedMatrixes[i].M21) || (tempMatrix.M22 != returnedMatrixes[i].M22) || (tempMatrix.M23 != returnedMatrixes[i].M23) || (tempMatrix.M24 != returnedMatrixes[i].M24)
                                   && (tempMatrix.M31 != returnedMatrixes[i].M31) || (tempMatrix.M32 != returnedMatrixes[i].M32) || (tempMatrix.M33 != returnedMatrixes[i].M33) || (tempMatrix.M34 != returnedMatrixes[i].M34)
                                   && (tempMatrix.M41 != returnedMatrixes[i].M41) || (tempMatrix.M42 != returnedMatrixes[i].M42) || (tempMatrix.M43 != returnedMatrixes[i].M43) || (tempMatrix.M44 != returnedMatrixes[i].M44))
                            {
                                returnedAddresses[i] = 0;
                            }
                        }
                    }
                    for (uint i = 0; i <= matrixesFnd; i++)
                    {
                        if (returnedAddresses[i] != 0)
                        {
                            returnedMatrixes[i] = Memory.ReadMatrix(procHnd, (IntPtr)returnedAddresses[i]);
                        }
                    }
                    Console.WriteLine("Cleaning complete!");
                }
            }
            if (key == VirtualKeyCode.PageUp)
            {
                pgUP = isPressed;
            }
            if (key == VirtualKeyCode.PageDown)
            {
                pgDN = isPressed;
            }
        }



        private static void OnRenderer(int fps, EventArgs args)
        {
            if (!drawyourstuff) return;
            //if (!isGameOnTop) return;

            Renderer.DrawText($"Maximum index: {matrixesFnd.ToString()}", wndMargins.X + 100, wndMargins.Y + 90);
            Renderer.DrawText($"Current index: {indexToUse.ToString()}", wndMargins.X + 100, wndMargins.Y + 100);
            Renderer.DrawText($"Current address: {returnedAddresses[indexToUse].ToString("X")}", wndMargins.X + 100, wndMargins.Y + 110);

            Renderer.DrawText("W2SType.TypeOGL is drawn in RED", wndMargins.X + 100, wndMargins.Y + 140, Color.Red);
            Renderer.DrawText("W2SType.TypeD3D is drawn in GREEN", wndMargins.X + 100, wndMargins.Y + 150, Color.Green);
            Renderer.DrawText("W2SType.Custom1 is drawn in BLUE", wndMargins.X + 100, wndMargins.Y + 160, Color.Blue);

            //drawing box of truth
            Renderer.DrawRect(wndMargins.X + wndSize.X / 4, wndMargins.Y + 100, 50, 50, new Color(255, 255, 255, 30));


            if (dynamicPosition)
            {
                if (vecToSearchFor.X > 100.0f)
                {
                    vecToSearchFor.X = 0;
                    vecToSearchFor.Y = 0;
                    vecToSearchFor.Z = 0;
                }
                else
                {
                    vecToSearchFor.X += 0.5f;
                    vecToSearchFor.Y += 0.5f;
                    vecToSearchFor.Z += 0.5f;
                }
            }

            if (drawAllAddressesAtOnce)
            {
                int j = 0;
                for (uint i = 0; i <= matrixesFnd; i++)
                {
                    if (returnedAddresses[i] != 0)
                    {
                        Vector2 vec2D = new Vector2(0, 0);
                        var matrix = Memory.ReadMatrix(procHnd, (IntPtr)returnedAddresses[i]);
                        if (Renderer.WorldToScreen(vecToSearchFor, out vec2D, (matrix), wndMargins, wndSize, W2SType.TypeCustom1))
                        {

                            Renderer.DrawText($"{returnedAddresses[i].ToString("X")}", vec2D.X, vec2D.Y, new Color(255, 255, 255, 70), 16, TextAlignment.lefted, false);
                            if ((vec2D.X > wndMargins.X + wndSize.X / 4) && (vec2D.Y > (wndMargins.Y + 100)) && (vec2D.X < (wndMargins.X + wndSize.X / 4 + 50)) && (vec2D.Y < (wndMargins.Y + 100 + 50)))
                            {
                                //it's inside box of truth
                                j += 20;
                                Renderer.DrawText($"{returnedAddresses[i].ToString("X")}", wndMargins.X + wndSize.X / 4, wndMargins.Y + 100 + j, Color.White, 18, TextAlignment.lefted, true);
                            }
                        }
                        //if (Renderer.WorldToScreen(vecToSearchFor, out vec2D, matrix, wndMargins, wndSize, W2SType.TypeD3D))
                        //{
                        //    Renderer.DrawText($"{returnedAddresses[i].ToString("X")}", vec2D.X, vec2D.Y, Color.Green);
                        //}
                        //if (Renderer.WorldToScreen(vecToSearchFor, out vec2D, matrix, wndMargins, wndSize, W2SType.TypeCustom1))
                        //{
                        //    Renderer.DrawText($"{returnedAddresses[i].ToString("X")}", vec2D.X, vec2D.Y, Color.Blue);
                        //}
                    }
                }
            }
            else
            {
                //Vector2 vec2D = new Vector2(0, 0);
                //var matrix = Memory.ReadMatrix(procHnd, (IntPtr)/*returnedAddresses[indexToUse]*/0x04709410); //Deceit.exe+1B8EB1C
                //if (Renderer.WorldToScreen(vecToSearchFor, out vec2D, matrix, wndMargins, wndSize, W2SType.TypeOGL))
                //{
                //    Renderer.DrawFilledRect(vec2D.X - 8, vec2D.Y - 8, 16, 16, Color.Red);
                //}
                //if (Renderer.WorldToScreen(vecToSearchFor, out vec2D, matrix, wndMargins, wndSize, W2SType.TypeD3D))
                //{
                //    Renderer.DrawFilledRect(vec2D.X - 8, vec2D.Y - 8, 16, 16, Color.Green);
                //}
                //if (Renderer.WorldToScreen(vecToSearchFor, out vec2D, matrix, wndMargins, wndSize, W2SType.TypeCustom1))
                //{
                //    Renderer.DrawFilledRect(vec2D.X - 8, vec2D.Y - 8, 16, 16, Color.Blue);
                //}
            }
            Vector2 vec2D1 = new Vector2(0, 0);
            var matrix1 = Memory.ReadMatrix(procHnd, (IntPtr)/*returnedAddresses[indexToUse]*/0x1413BEB1C); //Deceit.exe+1B8EB1C
            if (Renderer.WorldToScreen(vecToSearchFor, out vec2D1, matrix1, wndMargins, wndSize, W2SType.TypeCustom1))
            {
                Renderer.DrawFilledRect(vec2D1.X - 8, vec2D1.Y - 8, 16, 16, Color.Red);
            }


        }
    }
}
