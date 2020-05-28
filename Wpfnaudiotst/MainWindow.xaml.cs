using System;
using System.Collections.Generic;
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

using NAudio.Utils;
using NAudio.Wave;
using System.ComponentModel;
using System.Threading;
using System.IO.Ports;
using NAudio.CoreAudioApi;

namespace Wpfnaudiotst
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    static class count
    {
        public static int counter;
        public static int counter2;
    }
    public partial class MainWindow : Window
    {
        SerialPort port;
        public MainWindow()
        {
            count.counter = 0;
            count.counter2 = 0;

            InitializeComponent();
            //var waveIn = new WaveInEvent();
            //waveIn.DataAvailable += OnDataAvailable;
            //waveIn.StartRecording();



            var waveIn = new WasapiLoopbackCapture();
            waveIn.DataAvailable += InputBufferToFileCallback;
            waveIn.StartRecording();


            var a = waveIn.WaveFormat.BitsPerSample;
            var b = waveIn.WaveFormat.Channels;

            

            port = new SerialPort("COM3", 115200);

            //port.Open();

            

        }

        void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            
            float max = 0;
            // interpret as 16 bit audio
            for (int index = 0; index < args.BytesRecorded; index += 2)
            {
                short sample = (short)((args.Buffer[index + 1] << 8) |
                                        args.Buffer[index + 0]);
                // to floating point
                var sample32 = sample / 32768f;
                // absolute value 
                if (sample32 < 0) sample32 = -sample32;
                // is this the max value?
                if (sample32 > max) max = sample32;
            }
            count.counter = (int)(max * 100);
        }

        public void InputBufferToFileCallback(object sender, WaveInEventArgs args)
        {
            float max = 0;
            float max2 = 0;
            // interpret as 16 bit audio
            for (int index = 8; index < args.BytesRecorded; index += 8)
            {
                //float sample = (float)((args.Buffer[index + 3] << 24) |
                //                (args.Buffer[index + 2] << 16) |
                //                (args.Buffer[index + 1] << 8) |
                //                args.Buffer[index + 0]);
                //// to floating point
                //var sample32 = sample;// / 2147483648f;
                //// absolute value 
                //if (sample32 < 0) sample32 = -sample32;
                //// is this the max value?
                //if (sample32 > max) max = sample32;
                var sample = BitConverter.ToSingle(args.Buffer, index);
                if (sample < 0) sample = -sample;
                if (sample > max) max = sample;

                var sample2 = BitConverter.ToSingle(args.Buffer, index + 4);
                if (sample2 < 0) sample2 = -sample2;
                if (sample2 > max2) max2 = sample2;
            }
            count.counter = (int)(max * 100);
            count.counter2 = (int)(max2 * 100);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                (sender as BackgroundWorker).ReportProgress(0);
                Thread.Sleep(100);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
            pr_bar.Value = count.counter;
            pr_bar2.Value = count.counter2;
            //port.Write(i.ToString() + "\r\n");

        }
    }


}

