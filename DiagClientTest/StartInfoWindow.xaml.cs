using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DiagClientTest
{
    /// <summary>
    /// Interaction logic for StartInfoWindow.xaml
    /// </summary>
    public partial class StartInfoWindow : Window
    {
        public StartInfoWindow()
        {
            InitializeComponent();

            rbtnLaunch.Checked += RbtnLaunch_Checked;
            rbtnLaunch.Unchecked += RbtnLaunch_Unchecked;

            List<string> pvdList = GetProviders();
            foreach (var pvd in pvdList) {
                CheckBox chk = new CheckBox();
                chk.Content = pvd;
                providers.Children.Add(chk);
            }
        }

        private List<string> GetProviders()
        {
            List<string> providers = new List<string>();
            providers.Add("Microsoft-Windows-DotNETRuntime");
            providers.Add("System.Threading.Tasks.TplEventSource");

            return providers;
        }

        public StartInfo Result { get; private set; }
        private void btnProcSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtProcName.Text))
                return;

            var processes = DiagnosticsClient.GetPublishedProcesses().Select(Process.GetProcessById).
                Where(process => process != null && process.ProcessName.Contains(txtProcName.Text, StringComparison.InvariantCultureIgnoreCase));

            procList.Items.Clear();
            foreach (var process in processes) {
                procList.Items.Add($"{process.Id}-{process.ProcessName}");
            }
            ShowOrHideProcessList();
        }

        void ShowOrHideProcessList()
        {
            if (procList.Items.Count == 0)
                procList.Visibility = Visibility.Collapsed;
            else
                procList.Visibility = Visibility.Visible;
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var pvdlst = new List<string>();
            foreach (var chk in providers.Children.OfType<CheckBox>().Where(x => x.IsChecked == true)) {
                pvdlst.Add(chk.Content.ToString());
            }

            if (pvdlst.Count == 0) {
                MessageBox.Show("no provider selected");
                return;
            }


            int pid = 0;
            string pinfo = "";
            if (procList.SelectedItem != null) {
                var procInfo = procList.SelectedItem.ToString();
                int.TryParse(procInfo.Split("-")[0], out pid);
            }


            if (pid == 0 && !string.IsNullOrEmpty(txtLaunchCmd.Text)) {
                string cmd = txtLaunchCmd.Text;
                var cmdArg = cmd.Split(" ");
                string arg = null;
                if (cmdArg.Length > 1)
                    arg = cmdArg[1];

                var p = new Process();
                p.StartInfo.FileName = cmdArg[0];
                p.StartInfo.UseShellExecute = true;
                if (arg != null)
                    p.StartInfo.Arguments = arg;
                p.Start();
                pid = p.Id;
                pinfo = p.ProcessName;
            }

            if (pid == 0) {
                MessageBox.Show("pid not fould");
                return;
            }

            this.Result = new StartInfo() { Pid = pid, PInfo = pinfo,  Providers = pvdlst };

            this.DialogResult = true;

            this.Close();   
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RbtnLaunch_Unchecked(object sender, RoutedEventArgs e)
        {
            srchPanel.Visibility = Visibility.Visible;    
            lnchPanel.Visibility = Visibility.Collapsed;    
        }

        private void RbtnLaunch_Checked(object sender, RoutedEventArgs e)
        {
            srchPanel.Visibility = Visibility.Collapsed;
            lnchPanel.Visibility = Visibility.Visible;

        }

    }
}
