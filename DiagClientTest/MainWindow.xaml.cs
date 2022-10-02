using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
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
                source.Clr.All += (TraceEvent obj) => Log(obj.EventName + ":" +obj.FormattedMessage, obj);
                try {
                    source.Process();
                }
                // NOTE: This exception does not currently exist. It is something that needs to be added to TraceEvent.
                catch (Exception e) {
                    Log("Error encountered while processing events");
                    Log(e.ToString());
                }
            });

            //Task inputTask = Task.Run(() =>
            //{
            //    Console.WriteLine("Press Enter to exit");
            //    while (Console.ReadKey().Key != ConsoleKey.Enter)
            //    {
            //        Task.Delay(TimeSpan.FromMilliseconds(100));
            //    }
            //    session.Stop();
            //});

            //Task.WaitAny(streamTask/*, inputTask*/);

        }


        public void Log(string l, TraceEvent data = null)
        {

            this.Dispatcher.Invoke(() => {
                traceLog.Items.Add(new LogItem() { Name = l, Data = data });
                //traceLog.AppendText("\r" + l);
                //traceLog.ScrollToEnd();
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
            await session.StopAsync(System.Threading.CancellationToken.None);
            session?.Dispose();
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            traceLog.Items.Clear();
        }

        private void traceLog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (traceLog.SelectedItem != null && traceLog.SelectedItem is LogItem li) {
                var msg = $"{li.Data.ProviderName} " +
                    $"{Environment.NewLine} {li.Name}";

                MessageBox.Show(msg);
            }
        }

        

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new StartInfoWindow();
            wnd.Owner = this;
            if (wnd.ShowDialog() == true) {
                var si = wnd.Result;

                txtProcInfo.Text = $"{si.PInfo} - {si.Pid}";
                StartPipe(si);
            }

        }
    }
}
