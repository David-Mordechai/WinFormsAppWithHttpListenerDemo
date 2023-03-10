using System;
using System.Windows.Forms;
using WinFormsApp.Dtos;
using WinFormsApp.Services;

namespace WinFormsApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            StartHttpBitStatusService();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void StartHttpBitStatusService()
        {
            try
            {
                const int port = 8080;
                var httpBitStatusService = new HttpBitStatusService($"http://localhost:{port}");
                httpBitStatusService.Start(OnPowerBitCallBack, OnContinuousBitCallBack);
            }
            catch
            {
                // ignore
            }
        }

        private static BitStatusDto OnPowerBitCallBack(string port)
        {
            return new BitStatusDto {PowerBitStatus = "Go"};
        }

        private static BitStatusDto OnContinuousBitCallBack(string port)
        {
            return new BitStatusDto {PowerBitStatus = "NoGo"};
        }
    }
}