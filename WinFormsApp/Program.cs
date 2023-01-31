using System;
using System.Threading.Tasks;
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

            httpGetBitStatusService.Start(
                powerBitCallBack: OnPowerBitCallBack,
                continuousBitCallBack: OnContinuousBitCallBack);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
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
