using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using RB4InstrumentMapper.Parsing;
using RB4InstrumentMapper.Properties;

namespace RB4InstrumentMapper
{
    public class Program
    {
        private const string WinUsbOption = "--winusb";
        private const string RevertOption = "--revert";

        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Regular app startup
            if (args.Length == 0)
            {
                App.Main();
                return 0;
            }
            else if (args.Length < 2)
            {
                Console.WriteLine("Not enough arguments!");
                return -1;
            }

            // We're swapping the driver of a USB device
            // This has to be done through a new process, since a process can't change its privelege level
            string type = args[0];
            string deviceInstance = args[1];
            int exitCode;
            switch (type)
            {
                case WinUsbOption:
                    exitCode = WinUsbBackend.SwitchDeviceToWinUSB(deviceInstance) ? 0 : -1;
                    break;
                case RevertOption:
                    exitCode = WinUsbBackend.RevertDevice(deviceInstance) ? 0 : -1;
                    break;
                default:
                    Console.WriteLine($"Invalid option '{type}'!");
                    exitCode = -1;
                    break;
            }

            Logging.CloseAll();
            return exitCode;
        }

        /// <summary>
        /// Logs unhandled exceptions to a file and prompts the user with the exception message.
        /// </summary>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            // The unhandled exception
            var unhandledException = args.ExceptionObject as Exception;

            // MessageBox message
            var message = new StringBuilder();
            message.AppendLine("An unhandled error has occured:");
            message.AppendLine();
            message.AppendLine(unhandledException.GetFirstLine());
            message.AppendLine();

            // Create log if it hasn't been created yet
            Logging.CreateMainLog();
            // Use an alternate message if log couldn't be created
            if (Logging.MainLogExists)
            {
                // Log exception
                Logging.Main_WriteLine("-------------------");
                Logging.Main_WriteLine("UNHANDLED EXCEPTION");
                Logging.Main_WriteLine("-------------------");
                Logging.Main_WriteException(unhandledException, "Unhandled exception!");

                // Complete the message buffer
                message.AppendLine("A log of the error has been created, do you want to open it?");

                // Display message
                var result = MessageBox.Show(message.ToString(), "Unhandled Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                // If user requested to, open the log
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(Logging.LogFolderPath);
                }
            }
            else
            {
                // Complete the message buffer
                message.AppendLine("An error log was unable to be created.");

                // Display message
                MessageBox.Show(message.ToString(), "Unhandled Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Close the log files
            Logging.CloseAll();
            // Save settings
            Settings.Default.Save();
        }
    }
}