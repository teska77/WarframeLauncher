using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WarframeLauncher
{

    public enum WarframeStatus
    {
        PreLaunch,
        OpeningLauncher,
        LauncherOpen,
        LauncherExit,
        GameOpen,
        PasswordCopied
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WarframeStatus gameStatus = WarframeStatus.PreLaunch;
        private BackgroundWorker launcherFinderThread;
        private BackgroundWorker gameFinderThread;
        System.Diagnostics.Process launcherProcess;
        private string warframePath;
        private delegate void UpdateTextCallback(string title, string subtitle);
        private delegate void CopyPasswordCallback();

        public MainWindow()
        {
            InitializeComponent();
            CreateWorkers();
        }

        private void CreateWorkers()
        {
            launcherFinderThread = new BackgroundWorker();
            launcherFinderThread.DoWork += FinderThread_DoWork;
            launcherFinderThread.RunWorkerCompleted += LauncherFinderThread_RunWorkerCompleted;
            gameFinderThread = new BackgroundWorker();
            gameFinderThread.DoWork += FinderThread_DoWork;
            gameFinderThread.RunWorkerCompleted += GameFinderThread_RunWorkerCompleted;
        }

        private void GameFinderThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Game opened!", "Password copied to clipboard, click the logo to clear and exit"});
            this.Dispatcher.Invoke(new CopyPasswordCallback(this.CopyPassword));
        }

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            Top = 0;
            LabelTitle.Content = "Starting Warframe";
            LabelSubtitle.Content = "Opening launcher...";
            OpenLauncher();
        }

        private void ChangeState(WarframeStatus newState)
        {
            Console.WriteLine(String.Format("Changing from {0} to {1}", gameStatus.ToString(), newState.ToString()));
            gameStatus = newState;
        }

        private void OpenLauncher()
        {
            if (gameStatus == WarframeStatus.PreLaunch)
            {
                ChangeState(WarframeStatus.OpeningLauncher);
                launcherFinderThread.RunWorkerAsync("Launcher");
                System.Diagnostics.Process.Start("explorer.exe", "steam://rungameid/230410");
            } else
            {
                Console.WriteLine("Tried to open launcher when not in pre-launch state. Current State: " + gameStatus.ToString());
            }
            
        }

        private void LauncherFinderThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            launcherProcess = (System.Diagnostics.Process) e.Result;
            launcherProcess.EnableRaisingEvents = true;
            var launcherPath = launcherProcess.MainModule.FileName;
            warframePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(launcherPath, "..", ".."));
            Console.WriteLine(warframePath);
            launcherProcess.Exited += LauncherProcess_Exited;
            ChangeState(WarframeStatus.LauncherOpen);
            LabelTitle.Content = "Launcher open";
            LabelSubtitle.Content = "Launch the game to continue...";
        }

        private void UpdateText(string title, string subtitle)
        {
            LabelTitle.Content = title;
            LabelSubtitle.Content = subtitle;
        }

        private void CopyPassword()
        {
            Console.WriteLine("Password copied to clipboard!");
            Clipboard.SetText(Properties.Settings.Default.GamePassword);
        }

        private void LauncherProcess_Exited(object sender, EventArgs e)
        {
            ChangeState(WarframeStatus.LauncherExit);
            if (launcherProcess.ExitCode == 0)
            {
                if (gameStatus == WarframeStatus.LauncherExit)
                {
                    this.Dispatcher.Invoke(new UpdateTextCallback(this.UpdateText), new object[] { "Launcher closed", "Searching for game window..." });
                }
                gameFinderThread.RunWorkerAsync("Warframe.x64");
            }
        }

        private void FinderThread_DoWork(object sender, DoWorkEventArgs e)
        {
            bool foundProcess = false;
            // Wait around two seconds for the launcher/game to have time to open before spamming requests
            System.Threading.Thread.Sleep(2000);
            while (!foundProcess)
            {
                var processList = System.Diagnostics.Process.GetProcessesByName((string) e.Argument);
                foreach (var process in processList)
                {
                    Console.WriteLine(process.MainModule.FileVersionInfo.FileDescription);
                    if (process.MainModule.FileVersionInfo.FileDescription.Contains("Warframe"))
                    {
                        foundProcess = true;
                        e.Result = process;
                        break;
                    }
                }
                System.Threading.Thread.Sleep(500); // Need to rest in between otherwise may cause a Win32Exception
            }
            
        }

        private void ezgif_6_1b5366c175da_png_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                return;
            }
            if (Clipboard.ContainsText() && Clipboard.GetText() == Properties.Settings.Default.GamePassword)
            {
                Console.WriteLine("Clearing clipboard and exiting");
                Clipboard.Clear();
            }
            Application.Current.Shutdown();

        }

        private void ezgif_6_1b5366c175da_png_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var passwordForm = new PasswordBox();
            passwordForm.ShowDialog();
        }
    }
}
