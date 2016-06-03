using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Drawing;
using Valve.VR;

using static Valve.VR.IVRCompositor;

// Copyright(c) 2016 Norbert van Adrichem
// Contact: hello@norbert.in
// Licensed under the MIT license. Please see the LICENSE file.
// Source available at https://www.github.com/Nexz/turncountervr

namespace TurnCounterVR
{
  class Program
  {
    static void Main(string[] args)
    { 
      
            // Hello fellow Vive-users!
            // Display some mumbojumbo in the console.
          Console.WriteLine("OpenVR Rotational Counter");
          Console.WriteLine("-------------------------");
          Console.WriteLine("Hacked together by Nexz :-). Crashes? rotation-vr@norbert.in");
          Console.WriteLine("Licensed under the MIT license.");
          Console.WriteLine("I'm not responsible for any damage to your computer/HMD, no");
          Console.WriteLine("guarantees, not liable for anything, use at own risk, etc etc.");
          Console.WriteLine("");
          Console.WriteLine("Overlay code based on ViveIsAwesome/OpenVROverlayTest.");
          Console.WriteLine("For proper functioning, start SteamVR first!");
            // Define our resource path
      string ResourcePath = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName + "/Resources/";

          // Init OpenVR (or well, some voodoo in the wrapper I guess)
          Console.Write("Init EVR... ");
          var error = EVRInitError.None;

      OpenVR.Init(ref error);


     Console.WriteLine("Done!");

            Console.Write("Error check... ");
            // Show error regardless. You never know.
            Console.Write(error);

            // OpenVROverlayTest Error Exception code. Speaks for itself.
            if (error != EVRInitError.None) throw new Exception();

      OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
      if (error != EVRInitError.None) throw new Exception();

      OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
      if (error != EVRInitError.None) throw new Exception();


            Console.WriteLine(" - Done!");
            Console.Write("Creating overlay... ");

            // There be lions here! Overlay time.
            var overlay = OpenVR.Overlay;

            Console.WriteLine("Done!");

            // We need to define this array because... of reasons. (Define it so we can use it later).
            Console.Write("Define pose array... ");
            var pose = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            ulong overlayHandle = 0, thumbnailHandle = 0;

            Console.WriteLine("Done!");

            // Create the clickable overlay button
            Console.Write("Create dash overlay... ");

            overlay.CreateDashboardOverlay("overlayTest", "Rotation", ref overlayHandle, ref thumbnailHandle);

            Console.WriteLine("Done!");

            // Set the backdrop :)
            Console.Write("Set from file (rotation)... ");
            overlay.SetOverlayFromFile(thumbnailHandle, $"{ResourcePath}/rotation.png");
            Console.WriteLine("Done!");
            
            // I have no idea what I'm doing (OpenVROverlayTest code), but seems pretty reasonable.
            Console.Write("Set overlay parameters... ");
            overlay.SetOverlayWidthInMeters(overlayHandle, 2.5f);
            overlay.SetOverlayInputMethod(overlayHandle, VROverlayInputMethod.Mouse);
            Console.WriteLine("Done!");

            // This was also included. Not sure if it's necessary though.
            Console.Write("Make destroyable... ");
            Console.CancelKeyPress += (s, e) => overlay.DestroyOverlay(overlayHandle);
            Console.WriteLine("Done!");

            // Well, you could loop through all tracking devices presented by the OpenVR dll
            // but I didn't implement it yet. You can check what kind of type it is using OpenVR.
            // Lets just assume it's on 0 for now.
            Console.WriteLine("Assuming HMD is on DeviceIndex 0.");

            if (OpenVR.System.IsTrackedDeviceConnected(0) == true)
            {

                Console.WriteLine("Found HMD... probably ;-).");

            } else
            {

                Console.WriteLine("HMD is not connected! Please check if SteamVR is running.");
                Thread.Sleep(5000);
                Environment.Exit(0);

            }

            // Doesnt really need to be done, but was done for testing purposes
            Console.WriteLine("Trying to fetch in 1 second...");
            Thread.Sleep(1000);

            Console.Write("Get pose from Standing (first try)... ");
           
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, pose);
          
            Console.WriteLine("Done!");

            // Well, I was using this for another way of calculating the turns, but it didn't work.
            Console.WriteLine("Setting starting position at " + pose[0].mDeviceToAbsoluteTracking.m2);
            float newCheckM2;
            float newCheckM8;
            float newCheckM10;

            int numSpin = 0;

            string drawString = "";

            // Working with quadrants to determine spin:

            /*  ________________________
             * |           |           |
             * |    3      |    0      |
             * |___________|___________|
             * |           |           |
             * |    2      |    1      |
             * |___________|___________|
             */

            int prevQuarter = 0;
            int curQuarter = 0; // 0 = Right Top, 1 = Right Bottom, 2 = Left Bottom, 3 = Left Top
            Console.WriteLine("Starting main loop!");

      while (true)
      {
           
            // Get the pose! For real now.
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, pose);
                
                // In testing I found that I could use these values to find out what direction we're facing
                // M8 is essentially M2 but negative, but when building it I thought it would be good to
                // use it for some reason... Not sure why anymore. But it works!
                newCheckM2 = pose[0].mDeviceToAbsoluteTracking.m2;
                newCheckM8 = pose[0].mDeviceToAbsoluteTracking.m8;
                newCheckM10 = pose[0].mDeviceToAbsoluteTracking.m10;

                // Determine the quadrant we're in. You could even use V(a2*b2=c2) to calculate the actual degrees
                if (newCheckM2 > 0 && newCheckM8 < 0 && newCheckM10 < 0) { curQuarter = 0; }
                if (newCheckM2 > 0 && newCheckM8 < 0 && newCheckM10 > 0) { curQuarter = 1; }
                if (newCheckM2 < 0 && newCheckM8 > 0 && newCheckM10 > 0) { curQuarter = 2; }
                if (newCheckM2 < 0 && newCheckM8 > 0 && newCheckM10 < 0) { curQuarter = 3; }

                // If we've changed quadrant, check which way we're spinning.
                if (curQuarter != prevQuarter)
                {
                 
                    // Yay, we know stuff! Lets add it to the number of rotations (or subtract).   
                    if (curQuarter == 1 && prevQuarter == 2)
                    {
                        numSpin++;
                    }

                    if (curQuarter == 2 && prevQuarter == 1)
                    {
                        numSpin--;
                    }

                    // Determine the string to be shown
                    
                    if (numSpin < 0) { drawString = "Spin left " + Math.Abs(numSpin).ToString() + " times!"; }
                    if (numSpin > 0) { drawString = "Spin right " + Math.Abs(numSpin).ToString() + " times!"; }
                    if (numSpin == 0) { drawString = "You're untangled!"; }

                    // Okay, so I have no clue how OpenGL works (for now) apart from the fact that I need to
                    // generate a texture I don't know how. Fonts are also a challenge, from what I've read.

                    // SO! Simply hackity hack, generate a PNG that we can use with the SetOverlayFromFile funciton
                    // provided by OpenVR.

                    // I sorta stole this from StackExchange, but adapted it for my needs. Sigh, I feel dirty.
                    Bitmap bmp = new Bitmap(260, 60);
                    Graphics g = Graphics.FromImage(bmp);
                    g.FillRectangle(Brushes.Black, 0, 0, 260, 60);
                    RectangleF rectf = new RectangleF(20, 10, 240, 50);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.DrawString(drawString, new Font("Tahoma", 20), Brushes.White, rectf);
                    g.Flush();

                    // Save it, as I'm not sure if I can pass along the Bitmap as itself.
                    bmp.Save($"{ResourcePath}/spin.png", System.Drawing.Imaging.ImageFormat.Png);

                    // Update the 'last' quadrant.
                    prevQuarter = curQuarter;

                    // Set the overlay.
                    if (overlay.IsOverlayVisible(overlayHandle))
                        overlay.SetOverlayFromFile(overlayHandle, $"{ResourcePath}/spin.png");
                    // HOORAY!

                }
                                
      }
    }
  }
}