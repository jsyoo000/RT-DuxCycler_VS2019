using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Duxcycler
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new IntroPage());

            //중복 실행 방지 코드
            Mutex mutex = new Mutex(true, "Duxcycler", out bool bnew);
            if (bnew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(true);

                // 인트로 화면 시작
                IntroPage.ShowIntroScreen();
                Application.Run(new MainPage());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("The Duxcycler program is running.");
                Application.Exit();
            }
        }
    }
}
