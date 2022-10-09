using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiagClientTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            //traceLog.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

        }


        EventPipeSession session;
        public void PrintEventsLive(int processId, List<EventPipeProvider> pvdList)
        {

            var client = new DiagnosticsClient(processId);
            session = client.StartEventPipeSession(pvdList, false);


            Task streamTask = Task.Run(() => {
                var source = new EventPipeEventSource(session.EventStream);


                source.Clr.All += (TraceEvent obj) => Log(obj.ID.ToString(), obj.EventName + ":" +obj.FormattedMessage, obj);
                try {
                    //TODO: ?
                    source.Process();
                }
                catch (Exception e) {
                    Log(null, "Error encountered while processing events");
                    Log(null, e.ToString());
                }
            });
           

        }

        string[] evtIds;
        public void Log(string eid, string l, TraceEvent data = null)
        {

            this.Dispatcher.Invoke(() => {

                
                if (eid != null && evtIds?.Contains(eid) == true) {
                    traceLog2.Items.Add(new LogItem() { Name = l, Data = data });
                    if (traceLog2.Items.Count > 500) {
                        traceLog2.Items.RemoveAt(0);
                    }
                }
                else {
                    traceLog.Items.Add(new LogItem() { Name = l, Data = data });

                    if (traceLog.Items.Count > 500) {
                        traceLog.Items.RemoveAt(0);
                    }
                }
            });
        }


        void StartPipe(StartInfo si)
        {
            var pvdlst = new List<EventPipeProvider>();
            foreach (var pwd in si.Providers) {
                pvdlst.Add(new EventPipeProvider(pwd, EventLevel.Informational, (long)ClrTraceEventParser.Keywords.Default));
            }

            if (pvdlst.Count == 0) {
                MessageBox.Show("no provider selected");
                return;
            }


            PrintEventsLive(si.Pid, pvdlst);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            session?.Stop();
            session?.Dispose();
        }

        private async void btnStopPipe_Click(object sender, RoutedEventArgs e)
        {
            if (session == null)
                return;


            await session.StopAsync(System.Threading.CancellationToken.None);
            session?.Dispose();
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            traceLog.Items.Clear();
        }

        private void traceLog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tl = sender as ListBox;
            if (tl.SelectedItem != null && tl.SelectedItem is LogItem li) {
                var msg = $"{li.Data.ProviderName} " +
                    $"{Environment.NewLine} {li.Name}";

                string dataStr = null;


                foreach (var pi in li.Data.GetType().GetProperties()) {

                    try {
                        var v = pi.GetValue(li.Data);
                        if (v != null) {
                            if (!string.IsNullOrEmpty(dataStr))
                                dataStr += Environment.NewLine;
                            dataStr += pi.Name + ":" + v.ToString();
                        }
                    }
                    catch (Exception) {
                        //prop threw ex
                    }
                }

                MessageBox.Show(msg + " " + dataStr);
            }
        }



        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new StartInfoWindow();
            wnd.Owner = this;
            if (wnd.ShowDialog() == true) {
                var si = wnd.Result;

                txtProcInfo.Text = $"{si.PInfo} - {si.Pid}";

                evtIds = si.Events2Focus;
                if(evtIds?.Length > 0)
                    txtEventsToFocus.Text = String.Join(",", evtIds); 

                StartPipe(si);


                //processi izleyecek bişeyler yapalım..
                //await WatchProc(si.Pid);
                //txtProcInfo.Foreground = Brushes.Red;

            }

        }

        private Task WatchProc(int pid)
        {
            return Task.Run(() => {
                while (true) {

                    var p = Process.GetProcessById(pid);

                    if (p == null)
                        break;

                    Thread.Sleep(1000);
                }
            });
        }


    }
}
