using StereoKit;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

class HandsFree
{
    static void Main(string[] args)
    {
        SK.Initialize(new SKSettings { appName = "HandsFree" });

        SK.Run(() =>
        {
            // Import libraries for getting/setting window/cursor stuff
            [DllImport("user32.dll")]
            static extern bool SetProcessDPIAware();
            [DllImport("user32.dll")]
            static extern IntPtr GetForegroundWindow();
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

            // Get head position quaternion from OpenXR via StereoKit
            float q0 = Input.Head.orientation.w;
            float q1 = Input.Head.orientation.x;
            float q2 = Input.Head.orientation.y;
            float q3 = Input.Head.orientation.z;

            // Do math to get head angles
            double pitchDeg = Math.Atan2(q2 * q3 + q0 * q1, 0.5 - (Math.Pow(q1, 2) + Math.Pow(q2, 2))) * 180 / Math.PI;
            double yawDeg = Math.Asin(-2 * (q1 * q3 - q0 * q2)) * 180 / Math.PI;
            //double rollDeg = Math.Atan2(q1 * q2 + q0 * q3, 0.5 - (Math.Pow(q2, 2) + Math.Pow(q3, 2))) * 180 / Math.PI;

            // Headset angles that correspond to min/max cursor deflection within viewport
            double yawMin = -90;
            double yawMax = 90;
            double pitchMin = -60;
            double pitchMax = 45;

            // Decompose angles into commanded normalized positions
            double yawCommand = 1 - (yawDeg - yawMin) / (yawMax - yawMin);
            if (yawCommand < 0) yawCommand = 0;
            if (yawCommand > 1) yawCommand = 1;

            double pitchCommand = (pitchDeg - pitchMin) / (pitchMax - pitchMin);
            if (pitchCommand < 0) pitchCommand = 0;
            if (pitchCommand > 1) pitchCommand = 1;

            // Get foreground window size/position and assign simplified variables
            SetProcessDPIAware();
            Rectangle viewport;
            GetWindowRect(GetForegroundWindow(), out viewport);

            float vpXMin = viewport.Left;
            float vpXMax = viewport.Right;
            float vpYMin = viewport.Bottom;
            float vpYMax = viewport.Top;

            // Re-compose cursor command position using normalized commands and window dimensions
            int cursorXCommand = (int)(vpXMin + yawCommand * (vpXMax-vpXMin));
            int cursorYCommand = (int)(vpYMin + pitchCommand * (vpYMax - vpYMin));
            System.Drawing.Point cursorCommand = new System.Drawing.Point(cursorXCommand, cursorYCommand);

            // Draw cursor at commanded position
            System.Windows.Forms.Cursor.Position = cursorCommand;

            // Write debug stuff
            System.Console.Clear();
            System.Console.WriteLine("Pitch: " + pitchDeg);
            System.Console.WriteLine("Yaw:   " + yawDeg);
            //System.Console.WriteLine("Roll:  " + rollDeg);
            System.Console.WriteLine("Pitch Command Norm: " + pitchCommand);
            System.Console.WriteLine("Yaw Command Norm:   " + yawCommand);
            System.Console.WriteLine("Window XMin: " + vpXMin);
            System.Console.WriteLine("Window XMax: " + vpXMax);
            System.Console.WriteLine("Window YMin: " + vpYMin);
            System.Console.WriteLine("Window YMax: " + vpYMax);
            System.Console.WriteLine("Cursor Command X: " + cursorXCommand);
            System.Console.WriteLine("Cursor Command Y: " + cursorYCommand);

        });
    }
}