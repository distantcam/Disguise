using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace Config
{
    public partial class App : Application
    {
        public static XDocument FodyWeaveFile;

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                if (Debugger.IsAttached)
                {
                    args = new string[1];
                    args[0] = "TestFodyWeaver.xml";
                    if (!File.Exists(args[0]))
                        using (var testFile = File.CreateText(args[0]))
                        {
                            testFile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                            testFile.WriteLine("<Weavers />");
                        }
                }
                else
                {
                    MessageBox.Show("Must provide a FodyWeaver.xml file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var file = new FileInfo(args[0]);
            if (!file.Exists)
            {
                MessageBox.Show("Must provide a FodyWeaver.xml file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var fileStream = file.OpenRead())
                {
                    FodyWeaveFile = XDocument.Load(fileStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unable to open '{0}'. {1}", file, ex), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            App app = new App();
            app.InitializeComponent();
            app.Run();

            using (var fileStream = file.Create())
            {
                FodyWeaveFile.Save(fileStream);
            }
        }
    }
}