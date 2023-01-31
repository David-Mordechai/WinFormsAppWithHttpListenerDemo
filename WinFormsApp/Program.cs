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
            var httpGetBitStatusService =
                new HttpGetBitStatusService<BitStatusDto>(port: 8080);
            httpGetBitStatusService.Start(callBack: OnGetBitRequest);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static BitStatusDto OnGetBitRequest(string port)
        {
            return new BitStatusDto {PowerBitStatus = "Go"};
        }
    }
}
