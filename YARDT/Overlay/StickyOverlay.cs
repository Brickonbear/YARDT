using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Linq;
using YARDT.Properties;

namespace YARDT.Overlay
{

    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public System.Drawing.Rectangle rcNormalPosition;
    }

    class StickyOverlay : IExample
    {

       // [DllImport("user32.dll")]
       // [return: MarshalAs(UnmanagedType.Bool)]
       // static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        private readonly GraphicsWindow _window;

        private Font _font;

        private SolidBrush _FRBlue;
        private SolidBrush _IOPink;
        private SolidBrush _NXRed;
        private SolidBrush _DEYellow;
        private SolidBrush _SICyan;
        private SolidBrush _PZOrange;

        private SolidBrush _black;
        private SolidBrush _red;
        private SolidBrush _green;
        private SolidBrush _blue;

        //private Image _image;

        public StickyOverlay()
        {
            // initialize a new Graphics object
            // GraphicsWindow will do the remaining initialization

            var graphics = new Graphics
            {
                MeasureFPS = true,
                PerPrimitiveAntiAliasing = true,
                TextAntiAliasing = true,
                UseMultiThreadedFactories = false,
                VSync = true,
                WindowHandle = IntPtr.Zero
            };

            //WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
           // placement.length = Marshal.SizeOf(placement);
           // GetWindowPlacement(GetConsoleWindowHandle(), ref placement);

            // it is important to set the window to visible (and topmost) if you want to see it!
            _window = new StickyWindow(GetWindowHandle(), graphics)
            {
                IsTopmost = true,
                IsVisible = true,
                FPS = 60,
                X = 0,
                Y = 0,
                Width = 800,
                Height = 600
            };

            _window.SetupGraphics += _window_SetupGraphics;
            _window.DestroyGraphics += _window_DestroyGraphics;
            _window.DrawGraphics += _window_DrawGraphics;
        }

        ~StickyOverlay()
        {
            // you do not need to dispose the Graphics surface
            _window.Dispose();
        }

        public void Initialize()
        {
            Console.WindowWidth = 110;
            Console.WindowHeight = 35;
        }

        public void Run()
        {
            // creates the window and setups the graphics
            _window.StartThread();
        }

        private void _window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var gfx = e.Graphics;

            // creates a simple font with no additional style
            _font = gfx.CreateFont("Arial", 16);

            // colors for brushes will be automatically normalized. 0.0f - 1.0f and 0.0f - 255.0f is accepted!
            _black = gfx.CreateSolidBrush(0, 0, 0);

            _FRBlue = gfx.CreateSolidBrush(153, 218, 240);
            _IOPink = gfx.CreateSolidBrush(195, 110, 139);
            _NXRed = gfx.CreateSolidBrush(149, 54, 50);
            _DEYellow = gfx.CreateSolidBrush(218, 207, 161);
            _SICyan = gfx.CreateSolidBrush(17, 117, 79);
            _PZOrange = gfx.CreateSolidBrush(252, 154, 105);

            _red = gfx.CreateSolidBrush(Color.Red); // those are the only pre defined Colors
            _green = gfx.CreateSolidBrush(Color.Green);
            _blue = gfx.CreateSolidBrush(Color.Blue);

            //_image = gfx.CreateImage(Resources.logo); // loads the image using our image.bytes file in our resources
        }

        private void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            // you do not need to call BeginScene() or EndScene()
            var gfx = e.Graphics;

            gfx.ClearScene(); // set the background of the scene (can be transparent)

            gfx.DrawTextWithBackground(_font, _black, _DEYellow, 100, 100, "FPS: " + gfx.FPS);

            //gfx.DrawCircle(_red, 100, 100, 50, 2);
            //gfx.DashedCircle(_green, 250, 100, 50, 2);

            // Rectangle.Create takes x, y, width, height instead of left top, right, bottom
            //gfx.DrawRectangle(_blue, Rectangle.Create(350, 50, 100, 100), 2);
            //gfx.DrawRoundedRectangle(_red, RoundedRectangle.Create(500, 50, 100, 100, 6), 2);

            //gfx.DrawTriangle(_green, 650, 150, 750, 150, 700, 50, 2);

            //gfx.DrawLine(_blue, 50, 175, 750, 175, 2);
            //gfx.DashedLine(_red, 50, 200, 750, 200, 2);

            //gfx.OutlineCircle(_black, _red, 100, 275, 50, 4);
            //gfx.FillCircle(_green, 250, 275, 50);

            // parameters will always stay in this order: outline color, inner color, position, stroke
            //gfx.OutlineRectangle(_black, _blue, Rectangle.Create(350, 225, 100, 100), 4);
            //gfx.FillRoundedRectangle(_red, RoundedRectangle.Create(500, 225, 100, 100, 6));

            //gfx.FillTriangle(_green, 650, 325, 750, 325, 700, 225);

            // you could also scale the image on the fly
            //gfx.DrawImage(_image, 310, 375);
        }

        private void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            // you may want to dispose any brushes, fonts or images
        }

        private static IntPtr GetWindowHandle()
        {

            Console.WriteLine(@"Please type the process name of the window you want to attach to, e.g 'notepad.");
            Console.WriteLine("Note: If there is more than one process found, the first will be used.");

            
            var processName = Console.ReadLine();

            var process = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();

            if (process == null)
            {
                Console.WriteLine($"No process by the name of {processName} was found.");
                Console.WriteLine("Please open one or use a different name and restart the demo.");
                Console.ReadLine();
            }

            return process.MainWindowHandle;
        }
    }
}
