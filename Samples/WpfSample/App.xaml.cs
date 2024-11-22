using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string InitialStartDirectory { get; set; }

        public App()
        {
            InitialStartDirectory = Environment.CurrentDirectory;
        }
    }


    public class ThemeOverride
    {

        public static void SetThemeWindowOverride(MetroWindow window, string forceTheme = null)
        {
            if (string.IsNullOrEmpty(forceTheme))
                forceTheme = "Dark";

            if (forceTheme == "Dark")
            {
                window.WindowTitleBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#333");
                window.NonActiveWindowTitleBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#222");
                window.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#494949");
            }
            else
            {
                // Light theme
                window.WindowTitleBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#CBD4EF");
                window.NonActiveWindowTitleBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#CBD4EF"));
            }
        }
    }
}
