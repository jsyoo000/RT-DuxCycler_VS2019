using Duxcycler.Properties;
using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Duxcycler
{
    public partial class IntroPage : Form
    {
        // 이미지 선언
        Image _intro = Resources.INTRO;

        // Threading            
        private static Thread ms_oThread = null;

        // intro 화면 
        private static IntroPage intro = null;

        // Fade in and out.
        private double m_dblOpacityIncrement = .5;      // 나타날때 사용 변수
        private double m_dblOpacityDecrement = .07;     // 사라질때 사용 변수
        private const int TIMER_INTERVAL = 50;          // 타이머 변수

        public IntroPage()
        {
            InitializeComponent();

            this.BackgroundImage = _intro;                              // 윈도우 BackgroundImage 설정, 실행파일에 Resources폴더에서 읽는다.
            this.FormBorderStyle = FormBorderStyle.None;                // 윈도우 테두리 제거            

            this.AutoScaleMode = AutoScaleMode.None;          // 모니터 해상도 영향 없게 설정   

            // Form에 나타날때 깜박거림을 줄이기 위한 코드
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            // Fade 용 Timer 설정
            this.Opacity = 0.0;
            UpdateTimer.Interval = TIMER_INTERVAL;
            UpdateTimer.Start();
        }

        // 인트로 화면 실행 함수
        static public void ShowIntroScreen()
        {
            // Make sure it's only launched once.
            if (intro != null)
                return;

            // 인트로 실행 Thread 설정
            ms_oThread = new Thread(new ThreadStart(IntroPage.ShowForm))
            {
                IsBackground = true
            };
            ms_oThread.SetApartmentState(ApartmentState.STA);
            ms_oThread.Start();

            // intro 화면이 Close될 때까지 Thread 유지
            while (intro == null || intro.IsHandleCreated == false)
            {
                System.Threading.Thread.Sleep(TIMER_INTERVAL);
            }
        }

        // 인트로 화면을 생성한다.
        static private void ShowForm()
        {
            // intro 생성
            intro = new IntroPage();
                        
            intro.Show();                          // 인트로 화면 Show
            //intro.TopMost = true;                  // 화면 앞으로 설정
            Application.Run(intro);
        }

        // 인트로 화면을 닫는다.
        static public void CloseForm()
        {
            if (intro != null && intro.IsDisposed == false)
            {
                // 인트로화면을 서서이 없애기 위해 값 설정
                intro.m_dblOpacityIncrement = -intro.m_dblOpacityDecrement;
            }

            ms_oThread = null;	// we don't need these any more.
            intro = null;
        }

        // 인트로 화면이 서서히 나타나고, 사라질때 사용하는 타이머 함수
        private void UpdateTimer_Tick(object sender, System.EventArgs e)
        {
            // Calculate opacity
            if (m_dblOpacityIncrement > 0)		// 인트로 화면이 나타난다.
            {
                if (this.Opacity < 1)           // Fade in
                {
                    this.Opacity += m_dblOpacityIncrement;
                }
            }
            else                                // 인트로 화면이 사라진다.
            {
                if (this.Opacity > 0)           // Fade out
                {
                    this.Opacity += m_dblOpacityIncrement;
                }
                else                            // 인트로 화면이 다 사라지면
                {
                    UpdateTimer.Stop();         // 타이머 정지
                    this.Close();               // Intre A Form Close
                }
            }
        }

        // Form Load시 모니터 설정
        private void IntroPage_Load(object sender, EventArgs e)
        {
            // 기본 설정를 Ini 파일에서 읽어온다.
            Global.LoadSetting();

            Screen[] screens;
            screens = Screen.AllScreens;
            if (Global.ScreensIndex >= screens.Length) Global.ScreensIndex = 0;

            int centerWidth = screens[Global.ScreensIndex].Bounds.Width / 2;
            int centerHeight = screens[Global.ScreensIndex].Bounds.Height / 2;
            int posX = screens[Global.ScreensIndex].Bounds.Left + (centerWidth - (this.Width / 2));
            int posY = screens[Global.ScreensIndex].Bounds.Top + centerHeight - (this.Height / 2);
            this.Location = new System.Drawing.Point(posX, posY);
        }
    }
}
