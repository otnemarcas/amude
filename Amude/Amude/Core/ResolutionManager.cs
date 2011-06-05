using System;
using System.Configuration;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Amude.Core
{
    internal static class ResolutionManager
    {
        private static Point baseScreenSize = new Point(1280, 800);
        private static Point resolution = new Point();
        private static bool isFullScreen;
        private static Matrix globalTransformation;

        public static void Initialize()
        {
            if (ConfigurationManager.AppSettings["resolution"] == null ||
                !ConfigurationManager.AppSettings["resolution"].Contains('x'))
                SetPreferredResolution(baseScreenSize);

            string[] parms = ConfigurationManager.AppSettings["resolution"].Split('x');
            resolution = new Point(int.Parse(parms[0]), int.Parse(parms[1]));
            UpdateGlobalTransformationMatrix();

            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["fullscreen"]))
                SetFullScreen(true);
            isFullScreen = Boolean.Parse(ConfigurationManager.AppSettings["fullscreen"]);
        }

        private static void UpdateGlobalTransformationMatrix()
        {
            float horScaling = resolution.X / (float)baseScreenSize.X;
            float verScaling = resolution.Y / (float)baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
        }

        public static Matrix GetGlobalTransformation()
        {
            return globalTransformation;
        }

        public static int GetPreferredScreenWidth()
        {
            return resolution.X;
        }

        public static int GetPreferredScreenHeight()
        {
            return resolution.Y;
        }

        public static Point GetPreferredResolution()
        {
            return resolution;
        }

        public static void SetPreferredResolution(Point resolution)
        {
            ResolutionManager.resolution = resolution;
        }

        public static bool IsFullScreen()
        {
            return isFullScreen;
        }

        public static void SetFullScreen(bool fullScreen)
        {
            isFullScreen = fullScreen;
        }

        public static void UpdateGraphics()
        {
            UpdateGlobalTransformationMatrix();
            Controller.GetInstance().ApplyChanges();
        }

        public static void UpdateConfigFile()
        {
            IO.WriteConfig("resolution", (resolution.X + "x" + resolution.Y));
            IO.WriteConfig("fullscreen", (isFullScreen.ToString()));
        }
    }
}