using Duxcycler_Database;
using BitmapControl;
//using CameraControl;
using CustomClassLibrary;
using Duxcycler_IMAGE;
using Duxcycler_GLOBAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomWindowForm;
using Duxcycler.Properties;
using IniParser;
using IniParser.Model;
using System.Drawing.Imaging;
//using Touchless.Vision.Camera;
using OfficeOpenXml;

using AForge.Video;
using AForge.Video.DirectShow;
using DirectShowLib;
using CameraControlLib;
//using CameraControlLib;
//using Camera = CameraControlLib.Camera;

namespace Duxcycler
{
    public partial class SetupPageA : CustomForm
    {
        // Arduino 명령어 메뉴 리스트 
        public string[] listSerialMenu = new string[17]
        {
            "STEP_ACCEL",
            "STEP_EMERGENCY",
            "STEP_HOMING",
            "STEP_MOVING",
            "STEP_COARSE_SPEED",
            "STEP_FINE_SPEED",
            "STEP_MAX_SPEED",
            "LED_ONOFF",
            "TRAY_INOUT",
            "HEATER_UPDOWN",
            "GET_LED_STATE",
            "GET_STEP_ACCEL",
            "GET_STEP_POS",
            "GET_STEP_COARSE_SPEED",
            "GET_STEP_FINE_SPEED",
            "GET_STEP_MAX_SPEED",
            "GET_HALL_SENSOR",
        };

        #region Fade in and out 관련 변수 모음
        private bool m_bIsFadeIn = true;                                            // true: Fade In, false: Fade out
        private double m_dblOpacityIncrement = .2;                                  // 나타날때 사용 변수
        private double m_dblOpacityDecrement = .1;                                  // 사라질때 사용 변수
        private const int TIMER_INTERVAL = 50;                                      // 타이머 변수 
        #endregion Fade in and out 관련 변수 모음

        // DRS Camera 처리 클래스 
        //private CameraCtl cameraClass = null;

        private string log = "";
        private bool IsCameraScan = false;                             // Camera Scan 여부 변수
        
        // 영상 이미지를 얼마나 처리하는지 표시함.
        int nFrameRate = 0;

        int xImageResultion = 320;                                      // 보여줄 이미지 x Resultion
        int yImageResultion = 240;                                      // 보여줄 이미지 y Resultion

        int xSavedImageResultion = 640;                                 // 저장할 이미지 x Resultion
        int ySavedImageResultion = 480;                                 // 저장할 이미지 y Resultion

        int displaySizeX = 640;
        int displaySizeY = 480;
        int SAVEDIMAGE_X       = 640;
        int SAVEDIMAGE_Y       = 480;
        eIMAGEFORMAT SAVEDIMAGE_TYPE = eIMAGEFORMAT.FORMAT_PNG;        // 저장할 이미지 타입 ( FORMAT_DICOM, FORMAT_PNG, FORMAT_JPG, FORMAT_BMP )
        //CAMERA_TYPE CameraType = CAMERA_TYPE.I3_USB;                   // Camera 타입
        double LevelLow        = 4900;                                  // Camera 영상에서 값을 가지고 올때 최소값( -> 0)
        double LevelHigh       = 5900;                                  // Camear 영상에서 값을 가지고 올때 최대값( -> 255)
        double LowTemperature  = 10.00;                                 // 영상의 최저 온도값( -> 0  )
        double HighTemperature = 40.00;                                 // 영상의 최고 온도값( -> 255)
        double RoomTemp_Gain = 1.0;                                     // 환경 온도 Gain( RealTemperature 카메라가 아닐경우에만 적용 ), 0.001 <= Room Temperature <= 10.0
        double RoomTemp_Offset = 0.0;                                   // 환경 온도 Offet( RealTemperature 카메라가 아닐경우에만 적용 ), -15.0 <= Room Temperature <= 15.0

        bool IsRealTemperature = false;                                 // 실제 온도 사용 여부
        bool IsDigitalOut      = false;                                 // true: 원본(오른쪽) 영상 사용, false: 가공(왼쪽)영상 사용 ( DRS 전용 )

        bool IsWL_BG           = false;                                 // Low이하, Hight이상 Palette 값을 true: Gray로 false : 검은색, 흰색으로
        bool IsAutoWindowLevel = false;                                 // Auto Window Level 변수
        bool IsWindowLevelApply = false;                                // Auto WIndow Level 적용
        int minAutoWindowLevel = 0;                                     // Auto Window Level 변수
        int maxAutoWindowLevel = 255;                                   // Auto Window Level 변수

        bool IsFrameAverage = false;                                    // 영상 Frame을 평균을 해서 처리 할 것이지 여부
        int FrameAverageCount = 3;                                      // 영상 Frame을 몇개 평균할건 건지

        bool IsRef_ImageSaved = false;                                  // Referance 영상 
        bool IsRefCalibration = false;                                  // Referance 적용 여부
        OpenCvSharp.Mat refMat = null;                                  // Referance 이미지 변수
        private List<OpenCvSharp.Mat> RefMatList = new List<OpenCvSharp.Mat>();       // Referance Image 저장용 Mat 버퍼 큐
        private List<OpenCvSharp.Mat> MatFrameList = new List<OpenCvSharp.Mat>();     // 영상 Frame 평균을 위한 Mat 버퍼 큐

        //OpenCvSharp.Mat backMat = null;                                  // Referance 이미지 변수

        // Auto Focus 처리에 사용하는 변수
        //private CAMERA_COMMEND ChangedFocusStatus      = CAMERA_COMMEND.FOCUS_FAR;
        //private CAMERA_COMMEND NowFocusStatus          = CAMERA_COMMEND.FOCUS_FAR;
        private int AutoFocusCount = 0;
        private int FCount = 0;

        bool IsAutoFocus = false;                                   // Auto Fouce 처리변수 

        double nowStdDev = 0;                                       // 지금 표준 편차
        double backStdDev = 0;                                       // 이전 표준 편차
        double maxStdDev = 0;                                       // 최대 표준 편차
        double backMaxStdDev = 0;                                       // 이전 최대 값

        // WindowLevel 초기값
        int minWindowLevel = 0;
        int maxWindowLevel = 255;

        #region 필터 사용 변수
        bool IsMedianBlur      = true;
        int MedianKsize        = 3;                               //필터의 크기(1이상의 홀수 값) (Note – 생성된 결과 필터는 ksize x ksize의 크기를 갖는다.)

        bool IsGaussianBlur    = true;
        int GaussianKSizeX     = 3;                               // 가우시안 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        int GaussianKSizeY     = 3;                               // 가우시안 커널의 Y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        double GaussianSigmaX  = 1.5;                             // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
        double GaussianSigmaY  = 1.5;                             // Y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.        

        bool IsBilateralFilter = true;
        int BilateralD         = 3;                               // 각 픽셀이웃의 직경(Diameter of each pixel neighbourhood)
        double BilateralSigmaX = 3;                               // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
        double BilateralSigmaY = 3;                               // y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.

        bool IsBlur            = true;
        int BlurKSizeX         = 3;                               // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        int BlurKSizeY         = 3;                               // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)        
        int BlurAnchorX        = -1;                              // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.
        int BlurAnchorY        = -1;                              // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.        

        bool IsBoxFilter       = true;
        int BoxKSizeX          = 3;                               // 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        int BoxKSizeY          = 3;                               // 커널의 y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)        

        bool IsSharpen         = false;                           // 선명 필터 1 사용 여부
        bool IsSharpen2        = false;                           // 선명 필터 2 사용 여부
        bool IsSharpen3        = false;                           // 선명 필터 3 사용 여부
        bool IsMorphology      = false;                           // Morphology 사용 여부
        bool IsMeansDenoising  = false;                           // MeansDenoising 사용 여부
        #endregion

        GRIDLINE_TYPE Grid_Type = GRIDLINE_TYPE.Grid_2x2;                               // Scan 창의 Grid Line Type 변수
        
        #region Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 변수 모음
        private double Zoom_Ratio = 1.0F;                               // Zoom 배율 값

        private bool IsPositioningGrid = false;                         // Positioning Grid 사용 여부( 사용하면 Zoom Image Moving 기능을 사용 못하게 한다.)
        private Point movePoint;                                        // Zoom 이미지를 이동할 때 임시변수 Point 
        private bool IsZoomMove = false;                                // 마우스 다운으로 Zoom 이미지를 이동할 수 있는지 여부 변수
        private Point zoomCenterOffset = new Point(0, 0);               // Zoom 이미지의 Center point의 Offset이다.  
        #endregion Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 변수 모음

        #region 알람 설정용 변수 모음
        OpenCvSharp.Mat tempAlarmMat = null;
        bool IsTempAlarm = false;
        double TempAlarmValue = 37.5;
        int alramTimerCount = 0;    // 5초마다 알람 이미지 저장을 위한 변수  
        int alramCheckCount = 0;    // 5초동안 알람이 울린 회수 
        #endregion 알람 설정용 변수 모음

        private Thread workerThread = null;                             // Thread 선언 
        private volatile bool isStop = false;                           // 다른 Thread에서도 접근할 수 있도록 volatile 설정을 한다.
        //OpenCvSharp.VideoCapture videoCap = null;           // CCD Capture 변수
        double Brightness = 128;
        double Contrast = 128;
        private static Mutex mut = new Mutex();             // Create a new Mutex. The creating thread does not own the mutex.

        BitmapImageCtl bitmapimageCtl = new BitmapImageCtl();           // 이미지 처리용 변수

        // palette를 보여주기위한 Bitmap Data 선언 
        Byte[,] twopaletteData = new Byte[256, 24];             // Palette Bitmap을 만들기 위한 2차원 배열
        Byte[] paletteDataV    = new Byte[256 * 24];            // 세로 Palette Bitmap 만들때 사용할 1차원 배열( 한번만 만들면 되기때문에 전역을 선언함.)
        Byte[] paletteDataH    = new Byte[256 * 24];            // 가로 Palette Bitmap 만들때 사용할 1차원 배열( 한번만 만들면 되기때문에 전역을 선언함.)

        COLOR_TYPE SCAN_Color = COLOR_TYPE.COLOR_256;

        // 이미지 저장 관련 
        public ImageInfo saveImgInfo;                  // 이미지 저장용 변수
        public bool IsRunCapture = false;              // 이미지 캡쳐  
        public bool IsSuspend = false;                 // 이미지 캡쳐시 PCR 멈춤 여부   
        public bool IsSavedImage = false;              // 한번 이미지 캡쳐할때마다 4개의 필터 이미지 저장 
        public int filterCount = 0;                    // Filter Count 
        public bool isSaving = false;
        public string excelFileName = "";              // 저장할 엑셀 파일명 

        public int delayTime = 0;                   // 타이머를 이용한 Sleep 시간 
        public int delayCount = 0;                  // 타이머를 이용한 Sleep Count  

        // PCR Device 통신 관련 변수 
        public bool bSelectEnable = false;
        public int timerCount = 0;
        public int captureCount = 0;
        public int oldActiveNo = -1;
        public int curActiveNo = -1;

        // ROI 변수 
        public ListViewItem curItem;
        public ListViewItem.ListViewSubItem curSubItem;
        public int idxSelected = 0;
        public bool cancelEdit = true;
        public int SelectedRoiIndex = 0;

        // Arduino 시리얼 통신 관련 
        public int ledState = (int)COMMAND_VALUE.LED_OFF;           // 0:OFF, 100~150:ON
        public int trayState = (int)COMMAND_VALUE.TRAY_IN;          // 2000:IN, 1000:OUT
        public int lidHeaterState = (int)COMMAND_VALUE.HEATER_DOWN; // 1000:Down, 2000:Up
        public int filterPos = 0;   // HOME:0, FAM:350, HEX:700, ROX:1050, CY5:1400

        #region Camera Property Controls
        public Bitmap _displayFrame;
        #endregion

        //ClsResize _form_resize;
        public SetupPageA()
        {
            InitializeComponent();

            //_form_resize = new ClsResize(this);
            //_form_resize._get_initial_size();

            #region Form 기본 설정
            //this.Hide();
            //this.BackgroundImage = Resources.BackImageA;                               // 윈도우 BackgroundImage 설정, 실행파일에 Resources폴더에서 읽는다.
            //this.FormBorderStyle = FormBorderStyle.None;                // 윈도우 테두리 제거
            this.AutoScaleMode = AutoScaleMode.None;
            //this.Size = new Size(1920, 1080);                           // 윈도우 크기 설정 

            // Form에 나타날때 깜박거림을 줄이기 위한 코드
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            #endregion

            textBox_listEdit.Hide();

            #region Palette용 Batmap 만들기 위해 Data 만들기
            // 생성시 Palette용 Bitmap Data를 만든다.(처음은 세로용( 24 * 256 ) )
            int lengthX = twopaletteData.GetLength(0);
            int lengthY = twopaletteData.GetLength(1);

            for (int yRes = 0; yRes < lengthY; yRes++)
                for (int xRes = 0; xRes < lengthX; xRes++)
                    twopaletteData[xRes, yRes] = Convert.ToByte(lengthX - xRes - 1);
            // 생성시 Palette용 Bitmap Data를 만든다.
            // 2차배열을 1차배열로 변경 함수(2차배열, 2차배열 시작위치, 1차배열, 1차배열 시작위치, 복사할 Byte 수)
            System.Buffer.BlockCopy(twopaletteData, 0, paletteDataV, 0, sizeof(Byte) * twopaletteData.Length);

            //세로로 만든 Palette Bitmap Data를 오른쪽을 돌려 가로로 만든다.( 256 * 24 )
            Byte[,] twopaletteDataH = BitmapImageCtl.RotateRight(twopaletteData);

            // 2차배열을 1차배열로 변경 함수(2차배열, 2차배열 시작위치, 1차배열, 1차배열 시작위치, 복사할 Byte 수)
            System.Buffer.BlockCopy(twopaletteDataH, 0, paletteDataH, 0, sizeof(Byte) * twopaletteDataH.Length);
            #endregion
        }

        #region 모니터 해상도에 맞에 버튼 등 UI 의 위치를 맞춘다. 
        // 화면 나타날때 사용 함수
        public void alignForm()
        {
            // 모니터 해상도를 얻어오고 UI의 화면 Scale 을 계산한다. 
            //Global.sMonitor = Screen.PrimaryScreen;
            //double scaleX = 0.0;
            //double scaleY = 0.0;

            //if (this.Width > Global.sMonitor.Bounds.Width || this.Height > Global.sMonitor.Bounds.Height)
            //{
            //    scaleX = (double)Global.sMonitor.Bounds.Width / this.Width;
            //    scaleY = (double)Global.sMonitor.Bounds.Height / this.Height;

            //    if (Global.rectMonitor.Width > Global.rectMonitor.Height)
            //    {
            //        Global.mScaleX = (double)Global.sMonitor.Bounds.Width / 1380.0;
            //        Global.mScaleY = (double)Global.sMonitor.Bounds.Height / 1020.0;
            //        Global.sMonitorMode = (int)eMONITORMODE.M_HOR;
            //    }
            //    else
            //    {
            //        Global.mScaleX = (double)Global.sMonitor.Bounds.Width / 1020.0;
            //        Global.mScaleY = (double)Global.sMonitor.Bounds.Height / 1380.0;
            //        Global.sMonitorMode = (int)eMONITORMODE.M_VER;
            //    }

            //    SizeF scale = new SizeF((float)scaleX, (float)scaleY);
            //    this.Scale(scale);

            //    foreach (Control control in this.Controls)
            //    {
            //        control.Font = new Font("Arial", control.Font.SizeInPoints * (float)scaleX * (float)scaleY);
            //    }
            //    foreach (Control control in doubleBufferPanel2.Controls)
            //    {
            //        control.Font = new Font("Arial", control.Font.SizeInPoints * (float)scaleX * (float)scaleY);
            //    }
            //    foreach (Control control in doubleBufferPanel10.Controls)
            //    {
            //        control.Font = new Font("Arial", control.Font.SizeInPoints * (float)scaleX * (float)scaleY);
            //    }
            //    foreach (Control control in doubleBufferPanel3.Controls)
            //    {
            //        control.Font = new Font("Arial", control.Font.SizeInPoints * (float)scaleX * (float)scaleY);
            //    }
            //    foreach (Control control in doubleBufferPanel11.Controls)
            //    {
            //        control.Font = new Font("Arial", control.Font.SizeInPoints * (float)scaleX * (float)scaleY);
            //    }
            //    foreach (Control control in doubleBufferPanel1.Controls)
            //    {
            //        control.Font = new Font("Arial", control.Font.SizeInPoints * (float)scaleX * (float)scaleY);
            //    }

            //    Debug.WriteLine("Monitor Size : {0}, {1}, {2}, {3}", Global.sMonitor.Bounds.Width, Global.sMonitor.Bounds.Height, Global.sMonitor.WorkingArea.Width, Global.sMonitor.WorkingArea.Height);
            //}
        }
        #endregion

        // Setup Page Load 실핼함수( 초기화 )
        private void SetupPageA_Load(object sender, EventArgs e)
        {
            if (Global.isCloseForm)
            {
                this.Close();
                Global.isCloseForm = false;
                return;
            }

            //Screen[] screens;
            //screens = Screen.AllScreens;
            // scan A Form은 Screen 0에 표시
            //this.Location = new System.Drawing.Point(screens[Global.ScreensA].Bounds.Left, screens[Global.ScreensA].Bounds.Top);

            // Arduino Com Port Setting 
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            int selectIndex = -1, index = 0;
            this.comboBox_CommendSerialPort.Items.Clear();
            foreach (string strPortName in ports)
            {
                //MessageBox.Show(strPortName);
                this.comboBox_CommendSerialPort.Items.Add(strPortName);
                if (Global.ArducamPort == strPortName)
                    selectIndex = index;
                index++;                                   
            }
            if(selectIndex >=0 ) this.comboBox_CommendSerialPort.SelectedIndex = selectIndex;
            
            this.SAVEDIMAGE_X      = Global.SAVEDIMAGE_X;         // 저장할 이미지 x Resultion
            this.SAVEDIMAGE_Y      = Global.SAVEDIMAGE_Y;         // 저장할 이미지 y Resultion
            this.LowTemperature    = Global.LowTemperature;       // 영상의 최저 온도값( -> 0  )
            this.HighTemperature   = Global.HighTemperature;      // 영상의 최고 온도값( -> 255)

            this.IsMedianBlur      = Global.IsMedianBlur;
            this.MedianKsize       = Global.MedianKsize;        //필터의 크기(1이상의 홀수 값) (Note – 생성된 결과 필터는 ksize x ksize의 크기를 갖는다.)

            this.IsGaussianBlur    = Global.IsGaussianBlur;      
            this.GaussianKSizeX    = Global.GaussianKSizeX;     // 가우시안 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            this.GaussianKSizeY    = Global.GaussianKSizeY;     // 가우시안 커널의 Y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            this.GaussianSigmaX    = Global.GaussianSigmaX;     // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
            this.GaussianSigmaY    = Global.GaussianSigmaY;     // Y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.     
            
            this.IsBilateralFilter = Global.IsBilateralFilter;
            this.BilateralD        = Global.BilateralD;         // 각 픽셀이웃의 직경(Diameter of each pixel neighbourhood)
            this.BilateralSigmaX   = Global.BilateralSigmaX;    // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
            this.BilateralSigmaY   = Global.BilateralSigmaY;    // y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.

            this.IsBlur            = Global.IsBlur;
            this.BlurKSizeX        = Global.BlurKSizeX;         // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            this.BlurKSizeY        = Global.BlurKSizeY;         // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)        
            this.BlurAnchorX       = Global.BlurAnchorX;        // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.
            this.BlurAnchorY       = Global.BlurAnchorY;        // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.  
            
            this.IsBoxFilter       = Global.IsBoxFilter;
            this.BoxKSizeX         = Global.BoxKSizeX;          // 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            this.BoxKSizeY         = Global.BoxKSizeY;          // 커널의 y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.) 

            this.IsSharpen         = Global.IsSharpen;        // 선명 필터 1 사용 여부
            this.IsMorphology      = Global.IsMorphology;       // Morphology 사용 여부
            this.IsMeansDenoising  = Global.IsMeansDenoising;   // MeansDenoising 필터 사용 여부

            if (Global.ScreensIndex == 0)   this.radioButton_Set_1.Checked = true;  // A(1920 x 1080,): 1, B(1024 x 768): 2
            else                            this.radioButton_Set_2.Checked = true;  // A(1920 x 1080,): 2, B(1024 x 768): 1

            this.textBox_ImageWidth.Text  = this.SAVEDIMAGE_X.ToString();
            this.textBox_ImageHeight.Text = this.SAVEDIMAGE_Y.ToString();

            this.checkBox_BilateralFilter.Checked = this.IsBilateralFilter;
            this.textBox_BF_D.Text                = this.BilateralD.ToString();
            this.textBox_BF_SigmaX.Text           = this.BilateralSigmaX.ToString();
            this.textBox_BF_SigmaY.Text           = this.BilateralSigmaY.ToString();

            this.checkBox_Blur.Checked            = this.IsBlur;
            this.textBox_B_KSizeX.Text            = this.BlurKSizeX.ToString();
            this.textBox_B_KSizeY.Text            = this.BlurKSizeY.ToString();
            this.textBox_B_PointX.Text            = this.BlurAnchorX.ToString();
            this.textBox_B_PointY.Text            = this.BlurAnchorY.ToString();

            this.checkBox_BoxFilter.Checked       = this.IsBoxFilter;
            this.textBox_BoxF_KSizeX.Text         = this.BoxKSizeX.ToString();
            this.textBox_BoxF_KSizeY.Text         = this.BoxKSizeY.ToString();

            this.checkBox_GaussianBlur.Checked    = this.IsGaussianBlur;
            this.textBox_GBKSize_X.Text           = this.GaussianKSizeX.ToString();
            this.textBox_GBKSize_Y.Text           = this.GaussianKSizeY.ToString();
            this.textBox_GB_SigmaX.Text           = this.GaussianSigmaX.ToString();
            this.textBox_GB_SigmaY.Text           = this.GaussianSigmaY.ToString();

            this.checkBox_MedianBlur.Checked      = this.IsMedianBlur;
            this.textBox_MB_KSize.Text            = this.MedianKsize.ToString();

            this.checkBox_Sharpen.Checked       = this.IsSharpen;
            this.checkBox_Sharpen2.Checked      = this.IsSharpen2;
            this.checkBox_Sharpen3.Checked      = this.IsSharpen3;
            this.checkBox_Morphology.Checked       = this.IsMorphology;
            this.checkBox_MeansDenoising.Checked  = this.IsMeansDenoising;

            this.textBox_MB_KSize.Enabled    = !this.checkBox_MedianBlur.Checked;
            this.textBox_B_KSizeX.Enabled    = !this.checkBox_Blur.Checked;
            this.textBox_B_KSizeY.Enabled    = !this.checkBox_Blur.Checked;
            this.textBox_B_PointX.Enabled    = !this.checkBox_Blur.Checked;
            this.textBox_B_PointY.Enabled    = !this.checkBox_Blur.Checked;
            this.textBox_GBKSize_X.Enabled   = !this.checkBox_GaussianBlur.Checked;
            this.textBox_GBKSize_Y.Enabled   = !this.checkBox_GaussianBlur.Checked;
            this.textBox_GB_SigmaX.Enabled   = !this.checkBox_GaussianBlur.Checked;
            this.textBox_GB_SigmaY.Enabled   = !this.checkBox_GaussianBlur.Checked;
            this.textBox_BF_SigmaX.Enabled   = !this.checkBox_BilateralFilter.Checked;
            this.textBox_BF_SigmaY.Enabled   = !this.checkBox_BilateralFilter.Checked;
            this.textBox_BoxF_KSizeX.Enabled = !this.checkBox_BoxFilter.Checked;
            this.textBox_BoxF_KSizeY.Enabled = !this.checkBox_BoxFilter.Checked;

            #region Camera 초기화 
            InitCamera(Global.ccdCameraNo);
            #endregion

            double vidWidth = this.SAVEDIMAGE_X;
            double vidHeight = this.SAVEDIMAGE_Y;

            // pictureBox Event 처리
            pictureBox_Raw.ImageInfo = new ImageInfo
            {
                //Image_Width = this.SAVEDIMAGE_X,                             // 이미지 x Resultion 입력
                //Image_Height = this.SAVEDIMAGE_Y,                             // 이미지 y Resultion 입력
                Image_Width = (int)displaySizeX,                             // 이미지 x Resultion 입력
                Image_Height = (int)displaySizeY,                             // 이미지 y Resultion 입력
                Pixel_Format = 24,
                minWindowLevel = this.minWindowLevel,                           // min WindowLevel 입력
                maxWindowLevel = this.maxWindowLevel,                           // max WindowLevel 입력
                iIsoLow = Convert.ToInt32(this.LevelLow),                                 // 원본(오른쪽) 영상에서 값을 가지고 올때 최소값
                iIsoHigh = Convert.ToInt32(this.LevelHigh),                                // 원본(오른쪽) 영상에서 값을 가지고 올때 최대값                                                               
                selectedPaletteType = bitmapimageCtl.selectedType,                                             // 선택된 Palette Type를 저장한다. 이것은 PALETTE COLOR {0}, type 형태로 저장된다.
                ImagePalette = bitmapimageCtl.GetPalette(minWindowLevel, maxWindowLevel, this.IsWL_BG), // 적용된 Palette 입력
                IsWL_BG = this.IsWL_BG,
                //ImageBuffer = new Byte[this.SAVEDIMAGE_X * this.SAVEDIMAGE_Y * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
                //ImageBuffer = new Byte[(int)vidWidth * (int)vidHeight * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
                ImageBuffer = new Byte[(int)displaySizeX * (int)displaySizeY * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
            };

            this.saveImgInfo = new ImageInfo
            {
                Image_Width = this.SAVEDIMAGE_X,                             // 이미지 x Resultion 입력
                Image_Height = this.SAVEDIMAGE_Y,                             // 이미지 y Resultion 입력
                Pixel_Format = 24,
                minWindowLevel = this.minWindowLevel,                           // min WindowLevel 입력
                maxWindowLevel = this.maxWindowLevel,                           // max WindowLevel 입력
                iIsoLow = Convert.ToInt32(this.LevelLow),                                 // 원본(오른쪽) 영상에서 값을 가지고 올때 최소값
                iIsoHigh = Convert.ToInt32(this.LevelHigh),                                // 원본(오른쪽) 영상에서 값을 가지고 올때 최대값                                                               
                selectedPaletteType = bitmapimageCtl.selectedType,                                             // 선택된 Palette Type를 저장한다. 이것은 PALETTE COLOR {0}, type 형태로 저장된다.
                ImagePalette = bitmapimageCtl.GetPalette(minWindowLevel, maxWindowLevel, this.IsWL_BG), // 적용된 Palette 입력
                IsWL_BG = this.IsWL_BG,
                //ImageBuffer = new Byte[this.SAVEDIMAGE_X * this.SAVEDIMAGE_Y * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
                //ImageBuffer = new Byte[(int)vidWidth * (int)vidHeight * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
                ImageBuffer = new Byte[(int)SAVEDIMAGE_X * (int)SAVEDIMAGE_Y * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
            };

            //this.videoCap = new OpenCvSharp.VideoCapture(OpenCvSharp.CaptureDevice.DShow, 1);

            // Palette ComboBox 설정( eCOLOR_TYPE의 순서대로 ComboBox에 넣는다. )
            foreach (ROISHAPETYPE roitype in Enum.GetValues(typeof(ROISHAPETYPE)))
            {
                if(roitype != ROISHAPETYPE.LineAngle && roitype != ROISHAPETYPE.LineX)
                    comboBox_ROIType.Items.Add(roitype.ToString());
            }
            comboBox_ROIType.SelectedIndex = 2;

            // ROI 정보를 읽어온다. 
            int roiMainIndex = 1;
            int roiSubIndex = 1;
            foreach (ROIShape roi in Global.listRoiInfos)
            {
                this.pictureBox_Raw.ImageInfo.listShape.Add(roi.CopyTo());
                if (roiMainIndex < roi.ROI_MainIndex) roiMainIndex = roi.ROI_MainIndex;
            }
            // ROI 마지막 Main Index에서 1을 더해서 사용한다.( Sub Index는 1,2 상관없이 1부터 시작한다. )
            roiMainIndex += 1; roiSubIndex = 1;
            this.pictureBox_Raw.SetROIIndex(roiMainIndex, roiSubIndex);
            this.pictureBox_Raw.ConnectROI = null;

            ReloadROIListView();

            // Palette ComboBox 설정( eCOLOR_TYPE의 순서대로 ComboBox에 넣는다. )
            foreach (eIMAGEFORMAT imageType in Enum.GetValues(typeof(eIMAGEFORMAT)))
            {
                comboBox_ImageType.Items.Add(imageType.ToString());
            }
            comboBox_ImageType.SelectedIndex = (int)this.SAVEDIMAGE_TYPE;

            // PCR 통신 초기화 
            //Global.m_Serialnum = Global.PCR_Manager.PCR_Init();
            cmb_serialMenu.Items.AddRange(listSerialMenu);
            cmb_serialMenu.SelectedIndex = 0;

            // 기본 표시용 Timer 시작
            timer_view.Start();

            if (Global.ArducamSerial.IsOpen)
            {
                Global.ArducamSerial.ClosePort();
            }
            Button_SerialOpenClose_Click(this, null);
            // Aducam 시리얼 초기화 
            btnOpticInit_Click(this, null);

            customBtn_Scan_Click(this, null);
        }

        // Camera Setting 값들을 적용한다.  
        public void ApplyCamera()
        {
            // 노출시간을 맞춘다. 
            SetCamExposure(Global.ccdExposure);
            // 포커스를 맞춘다. 
            SetCamFocus(Global.ccdFocus);

            int index = (int)CAMERA_PROP.BRIGHTNESS;
            CameraProperty prop = Global.listCameraPropertys[index];

            EventHandler<EventArgs> Savedhandler = null;
            ManualResetEventSlim eventWaitHandle = new ManualResetEventSlim(false);
            Savedhandler = (sender, e) =>
            {
                prop.Saved -= Savedhandler;
                eventWaitHandle.Set();
            };

            if (Global.ccdBrightness >= prop.Min && Global.ccdBrightness <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = Global.ccdBrightness;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }

            index = (int)CAMERA_PROP.CONTRAST;
            prop = Global.listCameraPropertys[index];
            if (Global.ccdContrast >= prop.Min && Global.ccdContrast <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = Global.ccdContrast;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }

            index = (int)CAMERA_PROP.WHITEBALANCE;
            prop = Global.listCameraPropertys[index];
            if (Global.ccdWB >= prop.Min && Global.ccdWB <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = Global.ccdWB;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }

            index = (int)CAMERA_PROP.BKCOMPENSATION;
            prop = Global.listCameraPropertys[index];
            if (Global.ccdCompensation >= prop.Min && Global.ccdCompensation <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = Global.ccdCompensation;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }

            index = (int)CAMERA_PROP.SHARPNESS;
            prop = Global.listCameraPropertys[index];
            if (Global.ccdSharpness >= prop.Min && Global.ccdSharpness <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = Global.ccdSharpness;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }

            index = (int)CAMERA_PROP.GAIN;
            prop = Global.listCameraPropertys[index];
            if (Global.ccdGain >= prop.Min && Global.ccdGain <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = Global.ccdGain;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }

            index = (int)CAMERA_PROP.GAMMA;
            prop = Global.listCameraPropertys[index];
            if (Global.ccdGamma >= prop.Min && Global.ccdGamma <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = Global.ccdGamma;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }

        }

        // Camera 초기화 
        public void InitCamera(int devIndex)
        {
            //int devWidth = 4656;
            //int devHeigth = 3496;
            int devWidth = Global.SAVEDIMAGE_X;
            int devHeigth = Global.SAVEDIMAGE_Y;
            //int devIndex = 1;

            List<CameraDescriptor> _availableCameras = CameraDescriptor.GetAll();
            DsDevice[] cameraDevices = DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.VideoInputDevice); ;
            //CameraDescriptor preferredCamera = null;

            // Refresh the list of available cameras
            if (comboBoxCameras.Items.Count > 0)
                comboBoxCameras.Items.Clear();

            if (comboBoxCameras.Items.Count <= 0)
            {
                for (int i = 0; i < _availableCameras.Count; i++)
                {
                    if (_availableCameras[i].Name != null)
                    { 
                        //string msg = string.Format("{0}, {1}", _availableCameras[i].Name.Length, _availableCameras[i].Name);
                        //MessageBox.Show(msg);
                        comboBoxCameras.Items.Add(_availableCameras[i].Name);
                    }
                }
            }

            if (comboBoxCameras.Items.Count > 0)
            {
                if (devIndex < comboBoxCameras.Items.Count)
                    comboBoxCameras.SelectedIndex = devIndex;
                else
                    comboBoxCameras.SelectedIndex = 0;
            }

            checkBox_AutoExposure.Enabled = false;
            checkBox_AutoFocus.Enabled = false;

            cameraPropertyCombo.Items.Clear();
            if (Global.selectCam != null)
            {
                Global.listCameraPropertys.Clear();
                Global.listCameraPropertys = Global.selectCam.GetSupportedProperties();
                //foreach (var prop in selectCam.GetSupportedProperties())
                for (int i = 0; i < Global.listCameraPropertys.Count; i++)
                {
                    CameraProperty property = Global.listCameraPropertys[i];
                    cameraPropertyCombo.Items.Add(property.Id);
                }

                if (cameraPropertyCombo.Items.Count > 0)
                    cameraPropertyCombo.SelectedIndex = 0;
            }
        }

        // 카메라가 변경되는 경우 
        public void ChangeCamera(int devIndex)
        {
            //int devWidth = 4656;
            //int devHeigth = 3496;
            int devWidth = Global.SAVEDIMAGE_X;
            int devHeigth = Global.SAVEDIMAGE_Y;
            //int devIndex = 1;

            List<CameraDescriptor> _availableCameras = CameraDescriptor.GetAll();
            DsDevice[] cameraDevices = DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.VideoInputDevice); ;
            //CameraDescriptor preferredCamera = null;

            // Refresh the list of available cameras
            if (comboBoxCameras.Items.Count <= 0)
            {
                comboBoxCameras.Items.Clear();
                //comboBoxCameras.DisplayMember = "Name";
                //comboBoxCameras.DataSource = _availableCameras;
                //foreach (Camera cam in _availableCameras)
                for (int i = 0; i < _availableCameras.Count; i++)
                {
                    comboBoxCameras.Items.Add(_availableCameras[i].Name);
                }
            }

            FilterInfoCollection videoDevices;
            videoDevices = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
            //string camName = videoDevices[devIndex].MonikerString;
            //string camDescr = videoDevices[devIndex].Name;
            string camDescr = _availableCameras[devIndex].DevicePath;
            string camName = _availableCameras[devIndex].Name;

            DsDevice exactMatch = cameraDevices.FirstOrDefault(d => d.Name == camName && d.DevicePath == camDescr);
            DsDevice matchingDevice = cameraDevices.FirstOrDefault(d => d.Name == camName);
            if (matchingDevice == null)
                throw new InvalidOperationException("Could not find selected camera device");

            Global.videoSource = new VideoCaptureDevice(camDescr);
            Global.videoCapabilities = Global.videoSource.VideoCapabilities;
            Global.snapshotCapabilities = Global.videoSource.SnapshotCapabilities;

            var preferredCamera = CameraDescriptor.Find(camName, camDescr);
            Global.selectCam = preferredCamera?.Create();

            //Global.videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            //Global.videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);

            if (Global.selectCam != null)
            {
                Global.listCameraPropertys.Clear();
                Global.listCameraPropertys = Global.selectCam.GetSupportedProperties();
            }

            //Global.videoSource.ProvideSnapshots = true;
            //if (Global.videoSource.VideoCapabilities.Count() > 1)
            //    Global.videoSource.VideoResolution = Global.videoSource.VideoCapabilities[1];
            bool bFind = false;
            for (int i = 0; i < Global.videoSource.VideoCapabilities.Count(); i++)
            {
                VideoCapabilities resolution = Global.videoSource.VideoCapabilities[i];
                if (resolution.FrameSize.Width == Global.SAVEDIMAGE_X && resolution.FrameSize.Height == Global.SAVEDIMAGE_Y)
                {
                    Global.videoSource.VideoResolution = Global.videoSource.VideoCapabilities[i];
                    bFind = true;
                }
            }
            if (!bFind && Global.videoSource.VideoCapabilities.Count() > 1)
                Global.videoSource.VideoResolution = Global.videoSource.VideoCapabilities[1];

            if (comboBoxCameras.Items.Count > 0)
            {
                if (Global.ccdCameraNo < comboBoxCameras.Items.Count)
                    comboBoxCameras.SelectedIndex = devIndex;
                else
                    comboBoxCameras.SelectedIndex = 0;
            }
            //Camera c = (Camera)comboBoxCameras.SelectedItem;

            checkBox_AutoExposure.Enabled = false;
            checkBox_AutoFocus.Enabled = false;

            cameraPropertyCombo.Items.Clear();
            if (Global.selectCam != null)
            {
                Global.listCameraPropertys.Clear();
                Global.listCameraPropertys = Global.selectCam.GetSupportedProperties();
                //foreach (var prop in selectCam.GetSupportedProperties())
                for (int i = 0; i < Global.listCameraPropertys.Count; i++)
                {
                    CameraProperty property = Global.listCameraPropertys[i];
                    cameraPropertyCombo.Items.Add(property.Id);
                }

                if (cameraPropertyCombo.Items.Count > 0)
                    cameraPropertyCombo.SelectedIndex = 0;
            }
        }

        // 화면 나타날때 사용 함수
        public void ShowForm()
        {
            //Button_SerialOpenClose_Click(this, null);

            fadeTimer.Stop();                       // timer 정지
            this.TopMost = false;                   // 화면 뒤에 숨김
            this.Opacity = 1;
            this.Show();

            Screen[] screens;
            screens = Screen.AllScreens;

            // scan A Form은 Screen 0에 표시
            //this.Location = new System.Drawing.Point(screens[Global.ScreensA].Bounds.Left, screens[Global.ScreensA].Bounds.Top);

            this.m_bIsFadeIn = true;
            fadeTimer.Start();
        }

        // 화면에서 사라질때 사용 함수
        public void CloseForm()
        {
            Global.isCloseForm = true;

            if (Global.PCR_Manager.IsRunning)
                customBtn_Stop_Click(this, null);

            if(this.IsCameraScan)
                customBtn_Scan_Click(this, null);

            //thrashOldCamera();
            //Global.videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            CloseCurrentVideoSource();

            this.isStop = true;             // 쓰레드 함수 종료 변수
            if (workerThread != null)
            {
                workerThread.Join();        // 쓰레드가 완전히 실행되고 쓰레드 종료                
            }

            // 기본 표시용 종료
            timer_view.Stop();

            // 열려있는 Serial port를 닫는다.
            if (Global.ArducamSerial != null) Global.ArducamSerial.ClosePort();

            // Form을 천천히 Hide하기 위한 설정
            m_bIsFadeIn = false;
            fadeTimer.Interval = TIMER_INTERVAL;
            fadeTimer.Start();
        }

        // fade Timer 함수
        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            // Calculate opacity
            if (m_bIsFadeIn)		// 화면이 나타난다.
            {
                if (this.Opacity < 1)           // Fade in
                {
                    this.Opacity += m_dblOpacityIncrement;
                }
                else
                {
                    fadeTimer.Stop();                   // 타이머 정지                    
                    this.TopMost = Global.IsTopMost;    // 화면 앞으로
                }
            }
            else                                // 화면이 사라진다.
            {
                if (this.Opacity > 0)           // Fade out
                {
                    this.Opacity -= m_dblOpacityDecrement;
                }
                else                            // 화면이 다 사라지면
                {
                    fadeTimer.Stop();           // 타이머 정지
                    Close();                // Intre A Form Close
                }
            }
        }

        // 설정값 Global에 적용한다.
        public void Saved()
        {                                    
            try { this.SAVEDIMAGE_X = Convert.ToInt32(this.textBox_ImageWidth.Text); } catch { }
            try { this.SAVEDIMAGE_Y = Convert.ToInt32(this.textBox_ImageHeight.Text); } catch { }

            Global.SAVEDIMAGE_X      = this.SAVEDIMAGE_X;       // 저장할 이미지 x Resultion
            Global.SAVEDIMAGE_Y      = this.SAVEDIMAGE_Y;       // 저장할 이미지 y Resultion

            Global.ccdCameraNo = comboBoxCameras.SelectedIndex;

            Global.IsMedianBlur      = this.IsMedianBlur;
            Global.MedianKsize       = this.MedianKsize;        //필터의 크기(1이상의 홀수 값) (Note – 생성된 결과 필터는 ksize x ksize의 크기를 갖는다.)
            Global.IsGaussianBlur    = this.IsGaussianBlur;
            Global.GaussianKSizeX    = this.GaussianKSizeX;     // 가우시안 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            Global.GaussianKSizeY    = this.GaussianKSizeY;     // 가우시안 커널의 Y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            Global.GaussianSigmaX    = this.GaussianSigmaX;     // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
            Global.GaussianSigmaY    = this.GaussianSigmaY;     // Y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.   
            Global.IsBilateralFilter = this.IsBilateralFilter;
            Global.BilateralD        = this.BilateralD;         // 각 픽셀이웃의 직경(Diameter of each pixel neighbourhood)
            Global.BilateralSigmaX   = this.BilateralSigmaX;    // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
            Global.BilateralSigmaY   = this.BilateralSigmaY;    // y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.
            Global.IsBlur            = this.IsBlur;
            Global.BlurKSizeX        = this.BlurKSizeX;         // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            Global.BlurKSizeY        = this.BlurKSizeY;         // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)        
            Global.BlurAnchorX       = this.BlurAnchorX;        // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.
            Global.BlurAnchorY       = this.BlurAnchorY;        // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다. 
            Global.IsBoxFilter       = this.IsBoxFilter;
            Global.BoxKSizeX         = this.BoxKSizeX;          // 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            Global.BoxKSizeY         = this.BoxKSizeY;          // 커널의 y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.) 

            Global.IsSharpen         = this.IsSharpen;        // 선명 필터 1 사용 여부
            Global.IsMeansDenoising  = this.IsMorphology;       // Morphology 필터 사용 여부
            Global.IsMeansDenoising  = this.IsMeansDenoising;   // MeansDenoising 필터 사용 여부

            if (this.radioButton_Set_1.Checked) { Global.ScreensIndex = 0; }  // A(1920 x 1080,): 1, B(1024 x 768): 2
            else                                { Global.ScreensIndex = 1; }  // A(1920 x 1080,): 2, B(1024 x 768): 1

            Screen[] screens;
            screens = Screen.AllScreens;
            if (screens.Length == 1)
            {
                Global.ScreensIndex = 0;
            }

            // ROI 정보 입력한다.
            Global.listRoiInfos.Clear();
            // listview_ROIInfo에 정보 추가한다.         
            //foreach (ROIShape roi in this.pictureBox_Raw.ImageInfo.listShape)
            for (int rIndex = 0; rIndex < this.pictureBox_Raw.ImageInfo.listShape.Count; rIndex++)
            {
                ROIShape roi = this.pictureBox_Raw.ImageInfo.listShape[rIndex];
                roi.ROI_Gain = Convert.ToDouble(listView_ROIInfo.Items[rIndex].SubItems[3].Text);
                roi.ROI_Offset = Convert.ToDouble(listView_ROIInfo.Items[rIndex].SubItems[4].Text);
                Global.listRoiInfos.Add(roi.CopyTo());
            }
            Global.SavedSetting();
        }

        // 작업 쓰레드 
        public void DoWork()
        {
            // 신호가 없다면 계속 반복하고, 신호가 설정되면 중지
            while (!this.isStop)
            {
                if (_displayFrame != null)
                {
                    // 32비트 이미지를 24비트 이미지로 변환한다. 
                    Bitmap bmp24 = (Bitmap)_displayFrame.Clone();

                    xSavedImageResultion = Global.SAVEDIMAGE_X;
                    ySavedImageResultion = Global.SAVEDIMAGE_Y;

                    pictureBox_Raw.ImageInfo.minWindowLevel = this.minWindowLevel;                                                         // min WindowLevel 입력
                    pictureBox_Raw.ImageInfo.maxWindowLevel = this.maxWindowLevel;                                                         // max WindowLevel 입력
                    pictureBox_Raw.ImageInfo.iIsoLow = 0;                                                                           // ISO Min 값 입력
                    pictureBox_Raw.ImageInfo.iIsoHigh = 255;                                                                         // ISO Max 값 입력

                    #region 이미지 화면에 표시 부분
                    if (pictureBox_Raw.InvokeRequired)      // 실시간으로 온도를 구하려면 ImageInfo에 값을 넣어야 한다.
                    {
                        //Size resize = new Size(displaySizeX, displaySizeY);
                        //Bitmap resizeImage = new Bitmap(bmp24, resize);
                        //Bitmap b2 = ConvertTo24bpp(resizeImage);
                        byte[] imageData = ImageToByteArray(bmp24);
                        pictureBox_Raw.BeginInvoke(new Action(() => Array.Copy(imageData, pictureBox_Raw.ImageInfo.ImageBuffer, pictureBox_Raw.ImageInfo.ImageBuffer.Length)));
                        //resizeImage.Dispose();
                        //b2.Dispose();

                        pictureBox_Raw.BeginInvoke(new Action(() => pictureBox_Raw.Invalidate()));
                    }
                    #endregion 이미지 화면에 표시 부분
                    bmp24.Dispose();

                    _displayFrame.Dispose();
                    _displayFrame = null;

                    nFrameRate++;           // Frame Rate 확인 변수
                }

                Thread.Sleep(100);
            }
            
        }

        // 이미지 저장
        private void SaveCaptureImage(Bitmap saveImage)
        {
            isSaving = true;

            // 파일을 저장한다. 
            string imgPath = AppDomain.CurrentDomain.BaseDirectory + "Images\\";
            DateTime dt = DateTime.Now;
            string fileName = string.Format("{1:yyyy}{1:MM}{2:dd}{3:HH}{4:mm}{5:ss}_{6}.png", dt, dt, dt, dt, dt, dt, filterCount);
            string strPath = imgPath + "\\" + fileName;

            //Bitmap saveImage = this.saveImgInfo.ToColorBitmap();
            saveImage.Save(strPath, System.Drawing.Imaging.ImageFormat.Png);

            //excelFileName = string.Format("{1:yyyy}{1:MM}{2:dd}{3:HH}{4:mm}{5:ss}.xlsx", dt, dt, dt, dt, dt, dt, filterCount);
            strPath = imgPath + "\\" + excelFileName;
            this.saveImgInfo.FilterType = filterCount;
            this.saveImgInfo.StudyNo = captureCount;
            SaveExcel(strPath, this.saveImgInfo);

            // Filter를 HOME 으로 이동한다. 
            //filterPos = 0;
            //log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_HOMING, filterPos);
            //this.Logger(log);
            //Thread.Sleep(300);

            // ROI 값을 추출한다. 
            //GetRoiValues(this.filterCount, this.saveImgInfo);
            this.filterCount++;
            //IsSavedImage = false;

            // 4개의 이미지를 모두 저장하면 캡쳐를 중지한다. 
            if (filterCount >= 4)
            {
                this.IsSavedImage = false;
                this.EndCapture();
            }
            else
            {
                if (filterCount == 1)
                    filterPos = (int)COMMAND_VALUE.FILTER_POS_HEX;
                else if (filterCount == 2)
                    filterPos = (int)COMMAND_VALUE.FILTER_POS_ROX;
                else if (filterCount == 3)
                    filterPos = (int)COMMAND_VALUE.FILTER_POS_CY5;

                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MOVING, filterPos);
                this.Logger(log);
                // 필터를 움직이는데 소요되는 시간 
                //Thread.Sleep(Global.filterMoveDelay);
                this.delayCount = 0;
                this.delayTime = Global.filterMoveDelay;

                //this.IsSavedImage = true;
            }

            isSaving = false;
        }

        public static byte[] ImageToByteArray(Bitmap image) //이미지를 바이트배열 변환
        {
            //using (var ms = new MemoryStream())
            //{
            //    image.Save(ms, ImageFormat.Bmp);
            //    return ms.ToArray();
            //}

            BitmapData bmpdata = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            int numbytes = bmpdata.Stride * bmpdata.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(ptr, bytedata, 0, numbytes);  
            image.UnlockBits(bmpdata);

            return bytedata;
        }

        public static Bitmap ConvertTo24bpp(Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
                gr.Dispose();
            }
            return bmp;
        }

        // Camera 시작
        public bool CamStart()
        {
            // open it
            OpenVideoSource(Global.videoSource);

            this.isStop = false;                    // 쓰레드 함수 종료 변수(true: 쓰레스 종료, false: 쓰레스 계속)
            workerThread = new Thread(this.DoWork); // 쓰레드 생성
            workerThread.Start();                   // 쓰레스 시작

            return true;
        }

        // Camera를 Scan/Stop 한다.
        private void Button_StartStop_Click(object sender, EventArgs e)
        {
            this.customBtn_Scan.Enabled = false;   // 중복 조작 방지
            if(this.IsCameraScan)                    // Camera Scan 중이면 종료한다.
            {
                this.IsCameraScan = false;
                this.customBtn_Scan.Text = "SCAN";

                this.isStop = true;             // 쓰레드 함수 종료 변수
                if (workerThread != null)
                {
                    workerThread.Join();        // 쓰레드가 완전히 실행되고 쓰레드 종료                
                }
            }
            else                                    // Camera Stop 중이면 Scan한다.
            {
                // Camera Control 클래스 생성
 
                if (CamStart())        // Camera 영상캡처 시작
                {
                    this.IsCameraScan = true;
                    this.customBtn_Scan.Text = "STOP";

                    // Scan중이면 Camera Type 변경이 불가능하게 설정한다.
                    //this.radioButton_DRSPCI.Enabled = false;
                    //this.radioButton_DRSUSB.Enabled = false;
                }                    
            }

            this.customBtn_Scan.Enabled = true;   // 중복 조작 방지
        }
        
        // Camera Type 변경 함수
        private void RadioButton_CameraType_Changed(object sender, EventArgs e)
        {
            //if (radioButton_DRSPCI.Checked)         this.CameraType = CAMERA_TYPE.DRS_PCI;
            //else if (radioButton_DRSUSB.Checked)    this.CameraType = CAMERA_TYPE.DRS_USB;
            //else                                    this.CameraType = CAMERA_TYPE.I3_USB;
        }
        
        // Mat 파일에 점선 그리기
        public void DrawDashLine(ref OpenCvSharp.Mat img, OpenCvSharp.Point sPoint, OpenCvSharp.Point ePoint, OpenCvSharp.Scalar color, int thickness = 1, int gap = 4)
        {
            float dx = sPoint.X - ePoint.X;
            float dy = sPoint.Y - ePoint.Y;

            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            List<OpenCvSharp.Point> pts = new List<OpenCvSharp.Point>();

            for (int iP = 0; iP < dist; iP += gap)
            {
                float r = (float)iP / dist;
                int x = Convert.ToInt32((sPoint.X * (1.0 - r) + ePoint.X * r) + .5);
                int y = Convert.ToInt32((sPoint.Y * (1.0 - r) + ePoint.Y * r) + .5);
                OpenCvSharp.Point p = new OpenCvSharp.Point(x, y);
                pts.Add(p);
            }

            foreach (var p in pts)
            {
                img.Line(new OpenCvSharp.Point(p.X - thickness, p.Y),
                                    new OpenCvSharp.Point(p.X + thickness, p.Y),
                                    color, thickness, OpenCvSharp.LineTypes.AntiAlias);
            }
        }

        // 시리얼 수신 데이터를 분석하여 처리한다. 
        private void revDataAnlysis()
        {
            int i = 0;
            // 시리얼 수신 데이터 파싱 
            int bufSize = Global.stringBufQueue_revQ.Count;
            //if (bufSize < 4)
            if (bufSize <= 0)
                return;

            string[] arrayQ = Global.stringBufQueue_revQ.ToArray();
            for (i = 0; i < bufSize; i++)
            {
                Debug.WriteLine(arrayQ[i]);
                Logger($"{arrayQ[i]}");

                Global.stringBufQueue_revQ.TryDequeue(out string stringResult);
            }
            
            //Debug.WriteLine("Max : {0}, Pos : {1}", Global.Focus_Max_Pos, Global.Focus_Cur_Pos);
        }

        // 화면 표시용 Timer 함수
        private void Timer_view_Tick(object sender, EventArgs e)
        {
            // 타이머를 이용한 delay (100 ms)
            if(this.delayTime > 0)
            {
                int resultTime = delayCount * 100;
                if (resultTime >= this.delayTime)
                    this.delayTime = 0;

                delayCount++;
            }

            // 저장된 ROI의 온도 구하는 함수
            UpdateROIListView();

            // Arduino 시리얼 수신 데이터 분석 및 출력 
            revDataAnlysis();

            int curState = Global.PCR_Manager.RxAction.getState();
            if (Global.PCR_Manager.IsRunning)
            {
                string readData = Global.PCR_Manager.readKunPcrData();
                Logger($"{readData}");

                // 시작을 눌렀는데 상태 정보가 ready 이면 Run 이 될때까지 Run을 실행한다. 
                if (curState != (int)PCR_STATE.M_RUN)
                    Global.PCR_Manager.PCR_Run();

                if (timerCount % 5 == 0)
                {
                    int actionNo = Global.PCR_Manager.RxAction.getCurrent_Action();
                    int curLoop = Global.PCR_Manager.RxAction.getCurrent_Loop();
                    double actionTime = Global.PCR_Manager.RxAction.getSec_TimeLeft();
                    double totalTime = Global.PCR_Manager.RxAction.getTotal_TimeLeft();
                    //int actionNo = Global.PCR_Manager.m_RxAction.getLabel();
                    if (actionNo > 0 && totalTime > 0 && actionNo < Global.PCR_Manager.GetActionCount() && actionNo < materialListView_Action.Items.Count)
                    {
                        curActiveNo = actionNo - 1;

                        // 현재 Action이 끝나기 10초전에 캡쳐한다. 
                        // filterCount 가 0 이 아니면 캡쳐가 진행중임.  
                        int realActionNo = Global.PCR_Manager.GetRealActionNo(actionNo);
                        Action_PCR CurAction = Global.PCR_Manager.GetCurAction(realActionNo);
                        int curTime = Convert.ToInt32(CurAction.getTime());

                        if (curTime > 0 && !IsRunCapture && !IsSuspend && CurAction.getCapture() && actionTime > 0 && actionTime <= 10)
                        {
                            IsSuspend = true;
                            StartCapture();
                        }

                        // 캡쳐가 끝나면 다시 PCR을 Resume 한다. 
                        if (!IsRunCapture && IsSuspend && CurAction.getCapture())
                        {
                            if (actionTime == 0)
                                customBtn_Resume_Click(this, null);
                            else
                                IsSuspend = false;
                        }

                        if (realActionNo != oldActiveNo)
                        {
                            // 캡쳐가 여러번 되는 것을 방지 
                            oldActiveNo = realActionNo;
                        }

                        bSelectEnable = !bSelectEnable;
                        materialListView_Action.Items[curActiveNo].Selected = bSelectEnable;
                        materialListView_Action.EnsureVisible(curActiveNo);
                        //materialListView_Action.TopItem = materialListView_Action.Items[actionNo];
                        materialListView_Action.Invalidate();
                    }

                    // 완료 되었는데도 Run 상태이면 Stop 명령을 실행한다. 
                    if (captureCount > 0 && curLoop == 0 && actionTime == 0 && totalTime == 0)
                    {
                        customBtn_Stop_Click(this, null);
                    }
                }

                timerCount++;
            }
            else
            {
                // 완료 되었는데도 Run 상태이면 Stop 명령을 실행한다. 
                if (this.IsCameraScan && captureCount > 0)
                    customBtn_Stop_Click(this, null);
            }
        }

        private static AForge.Math.Geometry.GrahamConvexHull GetHullFinder()
        {
            return new AForge.Math.Geometry.GrahamConvexHull();
        }

        // ROIInfo의 내용으로 listView_ROIInfo를 다시 표시한다.
        private void ReloadROIListView()
        {
            if (this.pictureBox_Raw.ImageInfo == null)
                return;

            // listview_ROIInfo 지우기
            listView_ROIInfo.Items.Clear();

            if (this.pictureBox_Raw.ImageInfo == null || this.pictureBox_Raw.ImageInfo.listShape == null) return;
            //리스트뷰가 업데이트가 끝날 때까지 UI 갱신 중지
            listView_ROIInfo.BeginUpdate();

            // split image into 3 single-channel matrices
            OpenCvSharp.Cv2.Split(this.pictureBox_Raw.ImageInfo.ToMat(), out OpenCvSharp.Mat[] bgr);

            // listview_ROIInfo에 정보 추가한다.         
            for (int rIndex = 0; rIndex < this.pictureBox_Raw.ImageInfo.listShape.Count; rIndex++)
            {
                ROIShape roi = this.pictureBox_Raw.ImageInfo.listShape[rIndex];
               
                // Green 영상으로 데이터를 추출한다. 
                roi.CalROIShare(bgr[1]);
                double roiAverageG = roi.ROI_Average;
                // Red 영상으로 데이터를 추출한다. 
                roi.CalROIShare(bgr[2]);
                double roiAverageR = roi.ROI_Average;
                double roiGain = roi.ROI_Gain;
                double roiOffset = roi.ROI_Offset;

                listView_ROIInfo.Items.Add(new ListViewItem(new String[] {
                            //String.Format("{0}-{1}", roi.ROI_MainIndex, roi.ROI_SubIndex),  // 번호
                            String.Format("{0}", roi.ROI_MainIndex),                        // 번호
                            String.Format("{0:#.0}", roiAverageG),                          // ROI내부의 평균 온도
                            String.Format("{0:0.0}", roiAverageR),                          // ROI내부의 표준편차
                            String.Format("{0:#.0}", roiGain),                              // ROI내부의 최소 온도
                            String.Format("{0:#.0}", roiOffset)                             // ROI내부의 최고 온도                            
                        }));
            }
            bgr[0].Dispose();
            bgr[1].Dispose();
            bgr[2].Dispose();

            listView_ROIInfo.EndUpdate();
        }

        // ROIInfo의 내용으로 listView_ROIInfo를 다시 표시한다.
        private void UpdateROIListView()
        {
            if (!this.IsCameraScan)
                return;
            if (this.pictureBox_Raw.ImageInfo == null || this.pictureBox_Raw.ImageInfo.listShape == null) 
                return;

            //리스트뷰가 업데이트가 끝날 때까지 UI 갱신 중지
            listView_ROIInfo.BeginUpdate();

            //mut.WaitOne();

            // ROI 개수가 같지 않으면 리스트를 다시 구성한다. 
            if (listView_ROIInfo.Items.Count != this.pictureBox_Raw.ImageInfo.listShape.Count)
                ReloadROIListView();

            // split image into 3 single-channel matrices
            OpenCvSharp.Cv2.Split(pictureBox_Raw.ImageInfo.ToMat(), out OpenCvSharp.Mat[] bgr);

            // listview_ROIInfo에 정보 추가한다.         
            for (int rIndex = 0; rIndex < this.pictureBox_Raw.ImageInfo.listShape.Count; rIndex++)
            {
                ROIShape roi = this.pictureBox_Raw.ImageInfo.listShape[rIndex];

                // Green 영상으로 데이터를 추출한다. 
                roi.CalROIShare(bgr[1]);
                string roiAverageG = String.Format("{0:#.0}", roi.ROI_Average);
                // Red 영상으로 데이터를 추출한다. 
                roi.CalROIShare(bgr[2]);
                string roiAverageR = String.Format("{0:#.0}", roi.ROI_Average);
                double roiGain = roi.ROI_Gain;
                double roiOffset = roi.ROI_Offset;

                if (rIndex >= listView_ROIInfo.Items.Count)
                    break;

                listView_ROIInfo.Items[rIndex].SubItems[1].Text = roiAverageG;
                listView_ROIInfo.Items[rIndex].SubItems[2].Text = roiAverageR;
            }

            bgr[0].Dispose();
            bgr[1].Dispose();
            bgr[2].Dispose();

            //mut.ReleaseMutex();

            listView_ROIInfo.EndUpdate();
        }

        #region CheckBox로 Filter 사용 여부 설정
        // MedianBlur Filter 사용여부 CheckBox 함수
        private void CheckBox_MedianBlur_CheckedChanged(object sender, EventArgs e)
        {
            if(this.checkBox_MedianBlur.Checked)
            {
                try {
                    int mbksize = Convert.ToInt32(textBox_MB_KSize.Text);
                    if ((mbksize % 2) != 0) MedianKsize = mbksize;
                    else textBox_MB_KSize.Text = MedianKsize.ToString();
                } catch { }
            }

            this.IsMedianBlur = this.checkBox_MedianBlur.Checked;

            this.textBox_MB_KSize.Enabled = !this.IsMedianBlur;
        }
        // Blur Filter 사용여부 CheckBox 함수
        private void CheckBox_Blur_CheckedChanged(object sender, EventArgs e)
        {
            if(this.checkBox_Blur.Checked)
            {
                try {
                    int blurksizex = Convert.ToInt32(textBox_B_KSizeX.Text);
                    if ((blurksizex % 2) != 0) BlurKSizeX = blurksizex;
                    else                       textBox_B_KSizeX.Text = BlurKSizeX.ToString();
                
                    int blurksizey = Convert.ToInt32(textBox_B_KSizeY.Text);
                    if ((blurksizey % 2) != 0)  BlurKSizeY = blurksizey;
                    else                        textBox_B_KSizeY.Text = BlurKSizeY.ToString();
                
                    int bluranchorx = Convert.ToInt32(textBox_B_PointX.Text);
                    if ((bluranchorx % 2) != 0) BlurAnchorX = bluranchorx;
                    else                        textBox_B_PointX.Text = BlurAnchorX.ToString();
                
                    int bluranchory = Convert.ToInt32(textBox_B_PointY.Text);
                    if ((bluranchory % 2) != 0) BlurAnchorY = bluranchory;
                    else                        textBox_B_PointY.Text = BlurAnchorY.ToString();
                } catch { }
            }

            this.IsBlur = this.checkBox_Blur.Checked;

            this.textBox_B_KSizeX.Enabled = !this.checkBox_Blur.Checked;
            this.textBox_B_KSizeY.Enabled = !this.checkBox_Blur.Checked;
            this.textBox_B_PointX.Enabled = !this.checkBox_Blur.Checked;
            this.textBox_B_PointY.Enabled = !this.checkBox_Blur.Checked;



        }
        // GaussianBlur Filter 사용여부 CheckBox 함수
        private void CheckBox_GaussianBlur_CheckedChanged(object sender, EventArgs e)
        {
            if(this.checkBox_GaussianBlur.Checked)
            {
                try {
                    int gbksizeX = Convert.ToInt32(textBox_GBKSize_X.Text);
                    if (gbksizeX == 0 ||(gbksizeX % 2) != 0)    GaussianKSizeX = gbksizeX;
                    else                                        textBox_GBKSize_X.Text = GaussianKSizeX.ToString();
               
                    int gbksizeY = Convert.ToInt32(textBox_GBKSize_Y.Text);
                    if (gbksizeY == 0 || (gbksizeY % 2) != 0)   GaussianKSizeY = gbksizeY;
                    else                                        textBox_GBKSize_Y.Text = GaussianKSizeY.ToString();
               
                    double gaussiansigmax = Convert.ToDouble(textBox_GB_SigmaX.Text);
                    if (gaussiansigmax == 0 || (gaussiansigmax % 2) != 0)  GaussianSigmaX = gaussiansigmax;
                    else                            textBox_GB_SigmaX.Text = GaussianSigmaX.ToString();
                
                    double gaussiansigmay = Convert.ToDouble(textBox_GB_SigmaY.Text);
                    if (gaussiansigmay == 0 || (gaussiansigmay % 2) != 0) GaussianSigmaY = gaussiansigmay;
                    else textBox_GB_SigmaY.Text = GaussianSigmaY.ToString();
                } catch { }
            }

            this.IsGaussianBlur = this.checkBox_GaussianBlur.Checked;

            this.textBox_GBKSize_X.Enabled = !this.checkBox_GaussianBlur.Checked;
            this.textBox_GBKSize_Y.Enabled = !this.checkBox_GaussianBlur.Checked;
            this.textBox_GB_SigmaX.Enabled = !this.checkBox_GaussianBlur.Checked;
            this.textBox_GB_SigmaY.Enabled = !this.checkBox_GaussianBlur.Checked;

        }
        // Bilateral Filter 사용여부 CheckBox 함수
        private void CheckBox_BilateralFilter_CheckedChanged(object sender, EventArgs e)
        {
            if(this.checkBox_BilateralFilter.Checked)
            {
                try {
                    int bilaterald = Convert.ToInt32(textBox_BF_D.Text);
                    if ((bilaterald % 2) != 0)  BilateralD = bilaterald;
                    else                        textBox_BF_D.Text = BilateralD.ToString();
                
                    double bilateralsigmax = Convert.ToDouble(textBox_BF_SigmaX.Text);
                    if ((bilateralsigmax % 2) != 0) BilateralSigmaX = bilateralsigmax;
                    else                            textBox_BF_SigmaX.Text = BilateralSigmaX.ToString();
                
                    double bilateralsigmay = Convert.ToDouble(textBox_BF_SigmaY.Text);
                    if ((bilateralsigmay % 2) != 0) BilateralSigmaY = bilateralsigmay;
                    else                            textBox_BF_SigmaY.Text = BilateralSigmaY.ToString();
                } catch { }
            }
            this.IsBilateralFilter         = this.checkBox_BilateralFilter.Checked;

            this.textBox_BF_D.Enabled      = !this.checkBox_BilateralFilter.Checked;
            this.textBox_BF_SigmaX.Enabled = !this.checkBox_BilateralFilter.Checked;
            this.textBox_BF_SigmaY.Enabled = !this.checkBox_BilateralFilter.Checked;
        }
        // Box Filter 사용여부 CheckBox 함수
        private void CheckBox_BoxFilter_CheckedChanged(object sender, EventArgs e)
        {
            if(this.checkBox_BoxFilter.Checked)
            {
                try {
                    int boxksizex = Convert.ToInt32(textBox_BoxF_KSizeX.Text);
                    if ((boxksizex % 2) != 0)   BoxKSizeX = boxksizex;
                    else                        textBox_BoxF_KSizeX.Text = BoxKSizeX.ToString();
                
                    int boxksizey = Convert.ToInt32(textBox_BoxF_KSizeY.Text);
                    if ((boxksizey % 2) != 0)   BoxKSizeY = boxksizey;
                    else                        textBox_BoxF_KSizeY.Text = BoxKSizeY.ToString();
                } catch { }
            }

            this.IsBoxFilter                 = this.checkBox_BoxFilter.Checked;

            this.textBox_BoxF_KSizeX.Enabled = !this.checkBox_BoxFilter.Checked;
            this.textBox_BoxF_KSizeY.Enabled = !this.checkBox_BoxFilter.Checked;
        }
        // Sharpen 1 Filter 사용여부 CheckBox 함수
        private void CheckBox_Sharpen_CheckedChanged(object sender, EventArgs e)
        {
            this.IsSharpen = checkBox_Sharpen.Checked;
        }
        // Sharpen 2 Filter 사용여부 CheckBox 함수
        private void checkBox_Sharpen2_CheckedChanged(object sender, EventArgs e)
        {
            this.IsSharpen2 = checkBox_Sharpen2.Checked;
        }
        // Sharpen 3 Filter 사용여부 CheckBox 함수
        private void checkBox_Sharpen3_CheckedChanged(object sender, EventArgs e)
        {
            this.IsSharpen3 = checkBox_Sharpen3.Checked;
        }
        // Sharpen 2 Filter 사용여부 CheckBox 함수
        private void CheckBox_Morphology_CheckedChanged(object sender, EventArgs e)
        {
            this.IsMorphology = checkBox_Morphology.Checked;
        }
        // MeansDenoising Filter 사용여부 CheckBox 함수
        private void CheckBox_MeansDenoising_CheckedChanged(object sender, EventArgs e)
        {
            this.IsMeansDenoising = checkBox_MeansDenoising.Checked;
        }
        #endregion
        
        // ROI 모양 설정 ComboBox 함수
        private void ComboBox_ROIType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 선택한 Index를 ROISHAPETYPE으로 변경( Form Load시 인텍슬 맞춰 넣었다)
            ROISHAPETYPE selectedROIType = (ROISHAPETYPE)comboBox_ROIType.SelectedIndex;

            this.pictureBox_Raw.DrawShapeType = selectedROIType;
        }

        // 모든 ROI 지우기
        private void Button_ClearAll_Click(object sender, EventArgs e)
        {
            if (this.pictureBox_Raw.ImageInfo == null || this.pictureBox_Raw.ImageInfo.listShape == null) return;

            if (MessageBox.Show("Are you sure you want to remove all ROIs?", "Remove All ROIs", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.pictureBox_Raw.ImageInfo.listShape.Clear();
                this.pictureBox_Raw.SetROIIndex(1, 1);
            }
            ReloadROIListView();
        }

        // 선택한 ROI 지우기
        private void Button_Clear_Click(object sender, EventArgs e)
        {
            if (this.pictureBox_Raw.ImageInfo == null || this.pictureBox_Raw.ImageInfo.listShape == null) return;

            this.pictureBox_Raw.SelectedRemove();
            ReloadROIListView();
        }

        // 시리얼 포트를 열고 닫는 함수( Camera Commend/온도 일기 기능 Serial)
        private void Button_SerialOpenClose_Click(object sender, EventArgs e)
        {
            // 시리얼 통신 초기화 부분
            if (Global.ArducamSerial == null) Global.ArducamSerial = new SerialManager();
            if (Global.ArducamSerial.IsOpen)
            {
                Global.ArducamSerial.ClosePort();
            }
            else
            {
                Global.ArducamSerial.PortName = Global.ArducamPort;
                Global.ArducamSerial.BaudRate = Global.ArducamBaudRate;
                if (!Global.ArducamSerial.OpenPort())
                {
                }
            }

            if (Global.ArducamSerial.IsOpen)
            {
                this.btnSerialOpen.Text = "Close";
                this.comboBox_CommendSerialPort.Enabled = false;
                this.btnTrayInOut.Enabled = true;
                this.checkBox_LEDOn.Enabled = true;
                this.btnLidHeaterUpDn.Enabled = true;
                this.cmb_serialMenu.Enabled = true;
                this.serialValue.Enabled = true;
                this.btnSerialSend.Enabled = true;
            }
            else
            {
                this.btnSerialOpen.Text = "Open";
                this.comboBox_CommendSerialPort.Enabled = true;
                this.btnTrayInOut.Enabled = false;
                this.checkBox_LEDOn.Enabled = false;
                this.btnLidHeaterUpDn.Enabled = false;
                this.cmb_serialMenu.Enabled = false;
                this.serialValue.Enabled = false;
                this.btnSerialSend.Enabled = false;
            }

            this.label_EnvTemp.Text = "";
        }

        #region Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 함수 모음
        // pictureBox_Raw의 마우스 다운인경우 Zoom 이미지 이동 시작
        private void PictureBox_Raw_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.Zoom_Ratio <= 1.0 || this.IsPositioningGrid || checkBox_ROIDraw.Checked) return;  // Zoom이 없을때는 사용안함. PositioningGrid 사용시 사용 못하게 함.
            this.movePoint = e.Location;
            this.IsZoomMove = true;                 // Zoom 이미지를 이동할 수 있다.

            this.Cursor = Cursors.SizeAll;
        }

        // pictureBox_Raw의 마우스 move 인경우 Zoom 이미지 center point의 Offset를 구한다. 마우스 모양을 손 모양으로 변경
        private void PictureBox_Raw_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.Zoom_Ratio <= 1.0 || this.IsPositioningGrid)    // Zoom이 없을때는 사용안함. PositioningGrid 사용시 사용 못하게 함.
            {
                if (!this.IsPositioningGrid) this.Cursor = Cursors.Default;          // PositioningGrid 사용시에는 Cursor를 설정하지 않는다.
                return;
            }

            if (this.IsZoomMove)
            {
                Point tmpPoint = e.Location;
                int xOffset = movePoint.X - tmpPoint.X;
                int yOffset = movePoint.Y - tmpPoint.Y;

                zoomCenterOffset.Offset(xOffset, yOffset);      // Zoom Image Center Point의 Offset을 적용한다.

                movePoint = tmpPoint;
            }
        }

        // pictureBox_Raw의 마우스 업인경우 Zoom 이미지 이동 정지
        private void PictureBox_Raw_MouseUp(object sender, MouseEventArgs e)
        {
            this.IsZoomMove = false;                // Zoom 이미지이동 정지

            this.Cursor = Cursors.Default;
        }

        // pictureBox_Raw의 마우스 휠인경우 Zoom 배율 조정
        private void PictureBox_Raw_MouseWheel(object sender, MouseEventArgs e)
        {
            int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            PictureBox pb = (PictureBox)sender;

            if (lines < 0)
            {
                this.Zoom_Ratio += 0.1F;
                if (this.Zoom_Ratio > 4.0) this.Zoom_Ratio = 4.0F;
            }
            else if (lines > 0)
            {
                this.Zoom_Ratio -= 0.1F;
                if (Zoom_Ratio < 1) this.Zoom_Ratio = 1.0F;
            }
            
            //this.numericUpDown_ZoomValue.Value = new decimal(this.Zoom_Ratio);  // Zoom 배율 조정 (1.0 ~ 4.0, 0.1단위로 이동 )
            //Debug.WriteLine("Zoom : {0}", Zoom_Ratio);
        }

        // NumericUpDown 함수 Zoom 배율 조정 (1.0 ~ 4.0, 0.1단위로 이동 )
        private void NumericUpDown_ZoomValue_ValueChanged(object sender, EventArgs e)
        {
            //this.Zoom_Ratio = (double)this.numericUpDown_ZoomValue.Value;
            
            //trackBar_ZoomTrackBar.Value = (int)(this.Zoom_Ratio * 10);
        }

        // TrackBar 함수 Zoom 배율 조정 (1.0 ~ 4.0, 0.1단위로 이동 )
        private void TrackBar_ZoomTrackBar_Scroll(object sender, EventArgs e)
        {
            //this.Zoom_Ratio = (double)trackBar_ZoomTrackBar.Value / 10;
            //this.numericUpDown_ZoomValue.Value = new decimal(this.Zoom_Ratio);  // Zoom 배율 조정 (1.0 ~ 4.0, 0.1단위로 이동 )
        }       
        #endregion Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 함수 모음

        // ROI을 그릴지 말지 선택
        private void CheckBox_ROIDraw_CheckedChanged(object sender, EventArgs e)
        {
            this.pictureBox_Raw.IsROIDrawing = checkBox_ROIDraw.Checked;
            this.pictureBox_Raw.CloneShape = null;
        }

        private void imageButton_Ok_Click(object sender, EventArgs e)
        {
            Saved();
            this.DialogResult = DialogResult.OK;
            CloseForm();
        }

        private void imageButton_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            CloseForm();
        }

        private void comboBox_ImageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 선택한 Index를 ROISHAPETYPE으로 변경( Form Load시 인텍슬 맞춰 넣었다)
            this.SAVEDIMAGE_TYPE = (eIMAGEFORMAT)comboBox_ImageType.SelectedIndex;
        }

        private void customBtn_Scan_Click(object sender, EventArgs e)
        {
            this.customBtn_Scan.Enabled = false;   // 중복 조작 방지
            if (this.IsCameraScan)                    // Camera Scan 중이면 종료한다.
            {
                this.IsCameraScan = false;
                this.customBtn_Scan.Text = "SCAN";
                customBtn_Save.Enabled = false;

                this.isStop = true;             // 쓰레드 함수 종료 변수
                //if (workerThread != null)
                //{
                //    workerThread.Join();        // 쓰레드가 완전히 실행되고 쓰레드 종료                
                //}

                //thrashOldCamera();
                CloseCurrentVideoSource();

                //BitmapData bmpdata = Resources.end_image.LockBits(new Rectangle(0, 0, Resources.end_image.Width, Resources.end_image.Height), ImageLockMode.ReadOnly, Resources.end_image.PixelFormat);
                //int numbytes = bmpdata.Stride * bmpdata.Height;
                //byte[] bytedata = new byte[numbytes];
                //IntPtr ptr = bmpdata.Scan0;

                //Marshal.Copy(ptr, pictureBox_Raw.ImageInfo.ImageBuffer, 0, numbytes);                //Marshal.Copy(result, 0, pictureBox_Raw.ImageInfo.ImageBuffer, pictureBox_Raw.ImageInfo.ImageBuffer.Length); //Mat Data를 Byte[]로 변환
                //pictureBox_Raw.Invalidate();
            }
            else                                    // Camera Stop 중이면 Scan한다.
            {
                if (CamStart())        // Camera 영상캡처 시작
                {
                    this.IsCameraScan = true;
                    this.customBtn_Scan.Text = "STOP";
                    customBtn_Save.Enabled = true;
                }
            }

            this.customBtn_Scan.Enabled = true;   // 중복 조작 방지
        }

        private void customBtn_CCDSetup_Click(object sender, EventArgs e)
        {
            // snap camera
            if (Global.videoSource != null)
            {
                //this.pictureBox_Raw.FocusEnalbe = false;
                //_frameSource.Camera.ShowPropertiesDialog();
                //this.pictureBox_Raw.FocusEnalbe = true;

                Global.videoSource.DisplayPropertyPage(IntPtr.Zero); //This will display a form with camera controls
            }

            //this.videoCap.Set(OpenCvSharp.VideoCaptureProperties.Settings, 0);        // 0 ~ 100      (100)

            //this.videoCap.Set(OpenCvSharp.VideoCaptureProperties.AutoExposure, 0);  // 0 : 설정, 1 : 해제 (0)
            //this.videoCap.Set(OpenCvSharp.VideoCaptureProperties.Exposure, 3);    // (-6)

            //this.videoCap.Set(OpenCvSharp.CaptureProperty.Brightness, Global.ccdBrightness);    // (0)
            //this.videoCap.Set(OpenCvSharp.CaptureProperty.Contrast, Global.ccdContrast);        // (32)
            //this.videoCap.Set(OpenCvSharp.CaptureProperty.Gain, Global.ccdGain);         // 72 ~ 500     (0)
            //this.videoCap.Set(OpenCvSharp.CaptureProperty.Gamma, Global.ccdGamma);        // 0 ~ 100      (100)
        }

        private void customBtn_Load_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Method\\";
            openFileDlg.Filter = "Method File(*.method)|*.method";
            openFileDlg.Title = "Load an Method File";
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string fullPath = Path.GetFullPath(openFileDlg.FileName);
                FileIniDataParser parser = new FileIniDataParser();
                IniData parsedData = parser.ReadFile(fullPath);

                int stageCount = 0;
                try { if (parsedData["HEADER"]["STAGE_COUNT"] != null) stageCount = Convert.ToInt32(parsedData["HEADER"]["STAGE_COUNT"]); } catch { }
                // 스테이지 존재하지 않으면 더이상 진행하지 않는다. 
                if (stageCount <= 0)
                {
                    string errorMsg = string.Format("Stage Count Error = {0}", stageCount);
                    MessageBox.Show(errorMsg);
                    return;
                }

                //this.stageList.Clear();
                Global.PCR_Manager.listClear();
                materialListView_Action.Items.Clear();
                //PCR_Action actionList = new PCR_Action();

                //리스트뷰가 업데이트가 끝날 때까지 UI 갱신 중지
                materialListView_Action.BeginUpdate();

                // 스테이지 리스트를 읽어온다. 
                int actionCount = 1;
                for (int i = 0; i < stageCount; i++)
                {
                    string sectionName = string.Format("STAGE{0}", i + 1);     // Section Name
                    eGROUP_TYPE stageType = eGROUP_TYPE.PCR_STAGE;
                    int stageCycle = 0;
                    try { if (parsedData[sectionName]["STAGE_TYPE"] != null) stageType = (eGROUP_TYPE)Convert.ToInt32(parsedData[sectionName]["STAGE_TYPE"]); } catch { }
                    try { if (parsedData[sectionName]["STAGE_CYCLE"] != null) stageCycle = Convert.ToInt32(parsedData[sectionName]["STAGE_CYCLE"]); } catch { }
                    int stepCount = 0;
                    try { if (parsedData[sectionName]["STEP_COUNT"] != null) stepCount = Convert.ToInt32(parsedData[sectionName]["STEP_COUNT"]); } catch { }

                    // 스텝 리스트를 읽어온다. 
                    string strValues = "";
                    int startStep = 0;
                    for (int j = 0; j < stepCount; j++)
                    {
                        string subTitle = string.Format("STEP{0}", j + 1);     // Step Name
                        try { if (parsedData[sectionName][subTitle] != null) strValues = parsedData[sectionName][subTitle]; } catch { }
                        string[] words = strValues.Split(',');
                        if (words.Length != 3)
                            continue;

                        // 반복되는 시작 시점을 저장한다. 
                        if (j == 0)
                            startStep = actionCount;

                        double stepTemp = Convert.ToDouble(words[0]);
                        TimeSpan stepTime = TimeSpan.Parse(words[1]);
                        bool isCapture = Convert.ToBoolean(words[2]);                     
                        // 초로 변환  
                        int secTime = (stepTime.Hours * 60 * 60) + (stepTime.Minutes * 60) + stepTime.Seconds;        
                        Global.PCR_Manager.Action_Add(actionCount.ToString(), stepTemp.ToString(), secTime.ToString(), isCapture);

                        materialListView_Action.Items.Add(new ListViewItem(new String[] {
                                String.Format("{0}", actionCount),     // 번호
                                String.Format("{0:#.0}", stepTemp),                          // 온도
                                String.Format("{0}", secTime),                               // 시간(초)
                                String.Format("{0}", Convert.ToInt32(isCapture))                              // Capture 여부 
                            }));

                        // Capture 를 위해 PCR을 멈춤. 
                        if(isCapture)
                        {
                            secTime = 0;
                            isCapture = false;
                            Global.PCR_Manager.Action_Add(actionCount.ToString(), stepTemp.ToString(), secTime.ToString(), isCapture);                        
                        }

                        if (stageType == eGROUP_TYPE.PCR_STAGE && j == stepCount - 1)
                        {
                            materialListView_Action.Items.Add(new ListViewItem(new String[] {
                                String.Format("{0}", "GOTO"),                               // 번호
                                String.Format("{0}", startStep),                            // 회귀 번호 
                                String.Format("{0}", stageCycle)                            // 반복 회수 
                            }));
                            Global.PCR_Manager.Action_Add("GOTO", startStep.ToString(), stageCycle.ToString(), false);
                        }

                        actionCount++;
                    }
                }
                materialListView_Action.EndUpdate();
            }
        }

        private void customBtn_ReadData_Click(object sender, EventArgs e)
        {
            string readData = Global.PCR_Manager.readKunPcrData();
            Logger($"{readData}");
        }

        private void customBtn_Clear_Click(object sender, EventArgs e)
        {
            ListBox_Msg.Items.Clear();
        }

        /** @brief: Log 처리 함수 
         * @param   msg     Log Message
         */
        private void Logger(string msg)
        {
            // Log 발생 시간을 적어서 표시한다.
            string showMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss : ") + msg;

            // ( 내부에서 Theard가 UI 사용시 돌고 있어서 아래와 같이 구현해야됨. )
            if (this.ListBox_Msg.InvokeRequired)
            {
                this.ListBox_Msg.BeginInvoke(new Action(() => this.ListBox_Msg.Items.Insert(0, showMessage)));
            }
            else this.ListBox_Msg.Items.Insert(0, showMessage);
        }

        private void customBtn_Run_Click(object sender, EventArgs e)
        {
            // Run 전에 아래의 순서로 실행한다. (고장 발생 방지)
            // Heater Up - Tray In - Filter Home - Filter Down 
            btnLidHeaterUp_Click(this, null);
            Thread.Sleep(300);
            btnTrayIn_Click(this, null);
            Thread.Sleep(300);
            this.btnGoHome_Click(this, null);
            Thread.Sleep(300);
            btnLidHeaterUpDn_Click(this, null);
            Thread.Sleep(300);

            // 카메라 설정값들을 적용한다. 
            ApplyCamera();

            DateTime dt = DateTime.Now;
            excelFileName = string.Format("{1:yyyy}{1:MM}{2:dd}{3:HH}{4:mm}{5:ss}.xlsx", dt, dt, dt, dt, dt, dt);

            oldActiveNo = -1;
            bSelectEnable = false;
            timerCount = 0;
            captureCount = 0;
            Global.PCR_Manager.PCR_Run();
        }

        private void customBtn_Stop_Click(object sender, EventArgs e)
        {
            if (IsRunCapture)
                EndCapture();

            Global.PCR_Manager.PCR_Stop();

            //bool bComplete = Global.PCR_Manager.PCR_Stop();
            //if (bComplete)
            //    Logger("PCR ended !!");
            //else
            //    Logger("PCR incomplete!!");
        }

        private void listView_ROIInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            curItem = listView_ROIInfo.GetItemAt(e.X, e.Y);
            if (curItem == null)
                return;

            curSubItem = curItem.GetSubItemAt(e.X, e.Y);
            idxSelected = curItem.SubItems.IndexOf(curSubItem);
            // Offset, Gain 아이템만 수정 가능하도록 .. 
            if (idxSelected < 3)
                return;

            int subLeft = curSubItem.Bounds.Left + 2;
            int subWidth = curSubItem.Bounds.Width;
            textBox_listEdit.SetBounds(subLeft + listView_ROIInfo.Left, curSubItem.Bounds.Top + listView_ROIInfo.Top, 
                subWidth, curSubItem.Bounds.Height);

            textBox_listEdit.Text = curSubItem.Text;
            textBox_listEdit.Show();
            textBox_listEdit.Focus();
        }

        private void textBox_listEdit_Leave(object sender, EventArgs e)
        {
            textBox_listEdit.Hide();
            if (cancelEdit == false)
            {
                if (idxSelected == 3)        // Gain
                {
                    double Gain = Convert.ToDouble(textBox_listEdit.Text);
                    if (Gain > 0.0)
                    {
                        curSubItem.Text = textBox_listEdit.Text;
                        this.pictureBox_Raw.ImageInfo.listShape[SelectedRoiIndex].ROI_Gain = Convert.ToDouble(curSubItem.Text);
                    }
                }
                else if (idxSelected == 4)  // Offset
                {
                    double Offset = Convert.ToDouble(textBox_listEdit.Text);
                    curSubItem.Text = textBox_listEdit.Text;
                    this.pictureBox_Raw.ImageInfo.listShape[SelectedRoiIndex].ROI_Offset = Convert.ToDouble(curSubItem.Text);
                }
            }
            //else
            //    cancelEdit = false;

            listView_ROIInfo.Focus();
        }

        private void textBox_listEdit_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Enter:
                    cancelEdit = false;
                    e.Handled = true;
                    textBox_listEdit.Hide();
                    break;
                case Keys.Escape:
                    cancelEdit = true;
                    e.Handled = true;
                    textBox_listEdit.Hide();
                    break;
            }
        }

        private void listView_ROIInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView_ROIInfo.SelectedItems.Count <= 0)
                return;

            //ListView lv = sender as ListView;
            //lv.FullRowSelect = true;
            SelectedRoiIndex = listView_ROIInfo.SelectedItems[0].Index;

            if (SelectedRoiIndex >= 0 && SelectedRoiIndex < this.pictureBox_Raw.ImageInfo.listShape.Count)
            {
                ROIShape roi = this.pictureBox_Raw.ImageInfo.listShape[SelectedRoiIndex];
                pictureBox_Raw.SelectedROIShape(roi);
            }
        }

        private void checkBox_AutoFocus_CheckedChanged(object sender, EventArgs e)
        {
            //if (Global.CameraPropertyControlInitializationComplete)
            //{
            //    CameraPropertyValue value = new CameraPropertyValue(false, CameraPropertyValue, checkBox_AutoFocus.Checked);
            //    CurrentCamera.SetCameraProperty(SelectedCameraProperty, value);
            //}
        }

        private void checkBox_AutoExposure_CheckedChanged(object sender, EventArgs e)
        {
            //if (Global.CameraPropertyControlInitializationComplete)
            //{
            //    CameraPropertyValue value = new CameraPropertyValue(false, CameraPropertyValue, checkBox_AutoExposure.Checked);
            //    CurrentCamera.SetCameraProperty(SelectedCameraProperty, value);
            //}
        }

        //private void setFrameSource(CameraFrameSource cameraFrameSource)
        //{
        //    if (_frameSource == cameraFrameSource)
        //        return;

        //    _frameSource = cameraFrameSource;
        //}

        //private void InitializeCameraPropertyControls()
        //{
        //    Global.CameraPropertyControlInitializationComplete = false;

        //    Global.CurrentCameraPropertyCapabilities = CurrentCamera.CameraPropertyCapabilities;
        //    Global.CurrentCameraPropertyRanges = new Dictionary<CameraProperty, CameraPropertyRange>();

        //    //cameraPropertyValueTypeSelection.SelectedIndex = 0;

        //    cameraPropertyValue.Items.Clear();
        //    cameraPropertyValue.Items.AddRange(Global.DisplayPropertyValues.Keys.ToArray());

        //    Global.CameraPropertyControlInitializationComplete = true;

        //    cameraPropertyValue.SelectedIndex = 0;
        //}

        //private void cameraPropertyValueValue_EnabledChanged(object sender, EventArgs e)
        //{
        //    if (Global.CameraPropertyControlInitializationComplete && !Global.SuppressCameraPropertyValueValueChangedEvent && cameraPropertyValueValue.Enabled)
        //    {
        //        CameraPropertyValue value = CurrentCamera.GetCameraProperty(SelectedCameraProperty, true);
        //        cameraPropertyValueValue.Value = value.Value;
        //        //if (SelectedCameraProperty == CameraProperty.Exposure_lgSec)
        //        //    checkBox_AutoExposure.Checked = value.IsAuto;
        //        //if (SelectedCameraProperty == CameraProperty.FocalLength_mm)
        //        //    checkBox_AutoFocus.Checked = value.IsAuto;
        //    }
        //}

        //private void cameraPropertyValueValue_ValueChanged(object sender, EventArgs e)
        //{
        //    if (Global.CameraPropertyControlInitializationComplete && !Global.SuppressCameraPropertyValueValueChangedEvent)
        //    {
        //        CameraPropertyValue value = new CameraPropertyValue(false, CameraPropertyValue, Global.IsCameraPropertyAuto);
        //        CurrentCamera.SetCameraProperty(SelectedCameraProperty, value);
        //    }
        //}

        //private void UpdateCameraPropertyRange(CameraPropertyCapabilities propertyCapabilities)
        //{
        //    String text;
        //    if (Global.IsSelectedCameraPropertySupported && propertyCapabilities.IsGetRangeSupported && propertyCapabilities.IsGetSupported)
        //    {
        //        CameraPropertyRange range = CurrentCamera.GetCameraPropertyRange(SelectedCameraProperty);
        //        text = String.Format("[ {0}, {1} ], step: {2}", range.Minimum, range.Maximum, range.Step);

        //        Int32 decimalPlaces;
        //        Decimal minimum, maximum, increment;
        //        //if (IsCameraPropertyValueTypeValue)
        //        //{
        //            minimum = range.Minimum;
        //            maximum = range.Maximum;
        //            increment = range.Step;
        //            decimalPlaces = 0;
        //        //}
        //        //else if (IsCameraPropertyValueTypePercentage)
        //        //{
        //        //    minimum = 0;
        //        //    maximum = 100;
        //        //    increment = 0.01M;
        //        //    decimalPlaces = 2;
        //        //}
        //        //else
        //        //    throw new NotSupportedException(String.Format("Camera property value type '{0}' is not supported.", (String)cameraPropertyValueTypeSelection.SelectedItem));

        //        cameraPropertyValueValue.Minimum = minimum;
        //        cameraPropertyValueValue.Maximum = maximum;
        //        cameraPropertyValueValue.Increment = increment;
        //        cameraPropertyValueValue.DecimalPlaces = decimalPlaces;

        //        if (Global.CurrentCameraPropertyRanges.ContainsKey(SelectedCameraProperty))
        //            Global.CurrentCameraPropertyRanges[SelectedCameraProperty] = range;
        //        else
        //            Global.CurrentCameraPropertyRanges.Add(SelectedCameraProperty, range);

        //        CameraPropertyValue value = CurrentCamera.GetCameraProperty(SelectedCameraProperty, true);

        //        Global.SuppressCameraPropertyValueValueChangedEvent = true;
        //        cameraPropertyValueValue.Value = value.Value;
        //        //if (SelectedCameraProperty == CameraProperty.Exposure_lgSec)
        //        //    checkBox_AutoExposure.Checked = value.IsAuto;
        //        //if (SelectedCameraProperty == CameraProperty.FocalLength_mm)
        //        //    checkBox_AutoFocus.Checked = value.IsAuto;
        //        Global.SuppressCameraPropertyValueValueChangedEvent = false;
        //    }
        //    else
        //        text = "N/A";

        //    cameraPropertyRangeValue.Text = text;
        //}

        //private void cameraPropertyValue_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (Global.CameraPropertyControlInitializationComplete)
        //    {
        //        Global.IsSelectedCameraPropertySupported = CurrentCamera.IsCameraPropertySupported(SelectedCameraProperty);
        //        CameraPropertyCapabilities propertyCapabilities = Global.CurrentCameraPropertyCapabilities[SelectedCameraProperty];

        //        UpdateCameraPropertyRange(propertyCapabilities);

        //        //checkBox_AutoExposure.Enabled = cameraPropertyValueValue.Enabled = cameraPropertyValueTypeSelection.Enabled = IsSelectedCameraPropertySupported && propertyCapabilities.IsFullySupported;
        //        if (SelectedCameraProperty == CameraProperty.Exposure_lgSec)
        //        {
        //            checkBox_AutoExposure.Enabled = cameraPropertyValueValue.Enabled = Global.IsSelectedCameraPropertySupported && propertyCapabilities.IsFullySupported;
        //            checkBox_AutoFocus.Enabled = false;
        //        }
        //        if (SelectedCameraProperty == CameraProperty.FocalLength_mm)
        //        {
        //            checkBox_AutoFocus.Enabled = cameraPropertyValueValue.Enabled = Global.IsSelectedCameraPropertySupported && propertyCapabilities.IsFullySupported;
        //            checkBox_AutoExposure.Enabled = false;
        //        }
        //    }
        //}

        //public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps)
        //{
        //    if (frame == null)
        //        return;

        //    //mut.WaitOne();
        //    if (this.IsCameraScan && frame.Image != null)
        //    {
        //        if (this.delayTime == 0 && _latestFrame == null && _displayFrame == null)
        //        {
        //            mut.WaitOne();

        //            _latestFrame = (Bitmap)frame.Image.Clone();

        //            //Bitmap bmp24 = (Bitmap)_latestFrame.Clone();
        //            if (_displayFrame == null)
        //            {
        //                Size resize = new Size(displaySizeX, displaySizeY);
        //                Bitmap resizeImage = new Bitmap(_latestFrame, resize);
        //                Bitmap b2 = ConvertTo24bpp(resizeImage);
        //                _displayFrame = (Bitmap)b2.Clone();
        //                b2.Dispose();
        //                resizeImage.Dispose();
        //            }

        //            int xResultion = Global.SAVEDIMAGE_X;
        //            int yResultion = Global.SAVEDIMAGE_Y;

        //            #region 이미지 화면에 표시 부분
        //            //if (pictureBox_Raw.InvokeRequired)      // 실시간으로 온도를 구하려면 ImageInfo에 값을 넣어야 한다.
        //            //{
        //            //    Size resize = new Size(displaySizeX, displaySizeY);
        //            //    Bitmap resizeImage = new Bitmap(bmp24, resize);
        //            //    Bitmap b2 = ConvertTo24bpp(resizeImage);
        //            //    byte[] imageData = ImageToByteArray(b2);
        //            //    pictureBox_Raw.BeginInvoke(new Action(() => Array.Copy(imageData, pictureBox_Raw.ImageInfo.ImageBuffer, pictureBox_Raw.ImageInfo.ImageBuffer.Length)));
        //            //    resizeImage.Dispose();
        //            //    b2.Dispose();

        //            //    pictureBox_Raw.BeginInvoke(new Action(() => pictureBox_Raw.Invalidate()));
        //            //}
        //            #endregion 이미지 화면에 표시 부분

        //            #region 이미지 저장 부분
        //            if (IsSavedImage && !isSaving) // && this.saveWorker.IsBusy != true)                 // 스레드 중복 실행 방지
        //            {
        //                Bitmap b1 = ConvertTo24bpp(_latestFrame);
        //                byte[] imageData = ImageToByteArray(b1);
        //                b1.Dispose();
        //                saveImgInfo.ImageBuffer = imageData;
        //                this.saveImgInfo.ImageDateTime = DateTime.Now;               // 저장할 때 시간을 입력한다.
        //                                                                             //this.saveWorker.RunWorkerAsync();
        //                SaveCaptureImage();
        //            }
        //            #endregion 이미지 저장 부분
        //            //bmp24.Dispose();
        //            _latestFrame.Dispose();
        //            _latestFrame = null;
        //            //frame.Image.Dispose();
        //            //frame.Image = null;

        //            nFrameRate++;           // Frame Rate 확인 변수

        //            mut.ReleaseMutex();
        //        }
        //    }
        //    //mut.ReleaseMutex();

        //    Thread.Sleep(100);
        //}

        //private void thrashOldCamera()
        //{
        //    // Trash the old camera
        //    if (_frameSource != null)
        //    {
        //        _frameSource.NewFrame -= OnImageCaptured;
        //        _frameSource.Camera.Dispose();
        //        setFrameSource(null);
        //        //pictureBoxDisplay.Paint -= new PaintEventHandler(drawLatestImage);
        //    }
        //}

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();
                //videoSourcePlayer.WaitForStop();
                //videoSource.Stop();
                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                //if (videoSourcePlayer.IsRunning) 
                    videoSourcePlayer.Stop();
                videoSourcePlayer.VideoSource = null;
            }

            //if (videoSourcePlayer.VideoSource != null)
            //{
            //    videoSourcePlayer.SignalToStop();

            //    // wait ~ 3 seconds
            //    for (int i = 0; i < 30; i++)
            //    {
            //        if (!videoSourcePlayer.IsRunning)
            //            break;
            //        System.Threading.Thread.Sleep(100);
            //    }

            //    if (videoSourcePlayer.IsRunning)
            //    {
            //        videoSourcePlayer.Stop();
            //    }

            //    videoSourcePlayer.VideoSource = null;
            //}
        }

        // Open video source
        private void OpenVideoSource(IVideoSource source)
        {
            if (source == null)
                return;

            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            //FilterInfoCollection videoDevices;
            //videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[1].MonikerString);
            //videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);

            // stop current video source
            CloseCurrentVideoSource();

            //videoSource.DesiredFrameSize = new Size(160, 120);
            //source = videoSource;

            source.NewFrame += new NewFrameEventHandler(video_NewFrame);

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();
            //source.Start();

            // reset stop watch
            Global.stopWatch = null;

            // start timer
            //timer.Start();

            this.Cursor = Cursors.Default;
        }

        //eventhandler if new frame is ready
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //Bitmap img = (Bitmap)eventArgs.Frame.Clone();
            //do processing here
            //pictureBox1.Image = img;

            //mut.WaitOne();
            if (this.IsCameraScan)
            {
                if (_displayFrame == null) // && img == null)
                {
                    Bitmap img = (Bitmap)eventArgs.Frame.Clone();
                    // Rotate the image by 180 degrees
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);

                    //Bitmap bmp24 = (Bitmap)_latestFrame.Clone();
                    if (_displayFrame == null)
                    {
                        Size resize = new Size(displaySizeX, displaySizeY);
                        Bitmap resizeImage = new Bitmap(img, resize);
                        Bitmap b2 = ConvertTo24bpp(resizeImage);
                        _displayFrame = (Bitmap)b2.Clone();
                        b2.Dispose();
                        resizeImage.Dispose();
                    }

                    int xResultion = Global.SAVEDIMAGE_X;
                    int yResultion = Global.SAVEDIMAGE_Y;

                    #region 이미지 화면에 표시 부분
                    //if (pictureBox_Raw.InvokeRequired)      // 실시간으로 온도를 구하려면 ImageInfo에 값을 넣어야 한다.
                    //{
                    //    Size resize = new Size(displaySizeX, displaySizeY);
                    //    Bitmap resizeImage = new Bitmap(bmp24, resize);
                    //    Bitmap b2 = ConvertTo24bpp(resizeImage);
                    //    byte[] imageData = ImageToByteArray(b2);
                    //    pictureBox_Raw.BeginInvoke(new Action(() => Array.Copy(imageData, pictureBox_Raw.ImageInfo.ImageBuffer, pictureBox_Raw.ImageInfo.ImageBuffer.Length)));
                    //    resizeImage.Dispose();
                    //    b2.Dispose();

                    //    pictureBox_Raw.BeginInvoke(new Action(() => pictureBox_Raw.Invalidate()));
                    //}
                    #endregion 이미지 화면에 표시 부분

                    #region 이미지 저장 부분
                    if (this.delayTime == 0 && IsSavedImage && !isSaving) // && this.saveWorker.IsBusy != true)                 // 스레드 중복 실행 방지
                    {
                        mut.WaitOne();

                        Bitmap b1 = ConvertTo24bpp(img);
                        byte[] imageData = ImageToByteArray(b1);
                        b1.Dispose();
                        //saveImgInfo.ImageBuffer = imageData;
                        Array.Copy(imageData, saveImgInfo.ImageBuffer, imageData.Length);
                        this.saveImgInfo.ImageDateTime = DateTime.Now;               // 저장할 때 시간을 입력한다.
                                                                                     //this.saveWorker.RunWorkerAsync();
                        Bitmap saveImage = this.saveImgInfo.CopyDataToBitmap(xResultion, yResultion, imageData);
                        SaveCaptureImage(saveImage);

                        mut.ReleaseMutex();
                    }
                    #endregion 이미지 저장 부분
                    //bmp24.Dispose();
                    img.Dispose();
                    img = null;
                    //frame.Image.Dispose();
                    //frame.Image = null;

                    nFrameRate++;           // Frame Rate 확인 변수
                }
            }
            //mut.ReleaseMutex();

            Thread.Sleep(100);
        }

        private void cameraPropertyValue_EnabledChanged(object sender, EventArgs e)
        {
            //if (cameraPropertyValue.Enabled)
            //    InitializeCameraPropertyControls();
        }

        private void checkBox_LEDOn_CheckedChanged(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            if (checkBox_LEDOn.Checked)
            {
                ledState = (int)COMMAND_VALUE.LED_ON;
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.LED_ONOFF, ledState);
            }
            else
            {
                ledState = (int)COMMAND_VALUE.LED_OFF;
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.LED_ONOFF, ledState);
            }
            this.Logger(log);
        }

        private void btnLidHeaterUpDn_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_DOWN);
            this.Logger(log);
            lidHeaterState = (int)COMMAND_VALUE.HEATER_DOWN;

            //if (lidHeaterState == (int)COMMAND_VALUE.HEATER_DOWN)
            //{
            //    Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_UP);
            //    btnLidHeaterUpDn.Text = "Heater Down";
            //    lidHeaterState = (int)COMMAND_VALUE.HEATER_UP;
            //}
            //else
            //{
            //    Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_DOWN);
            //    btnLidHeaterUpDn.Text = "Heater Up";
            //    lidHeaterState = (int)COMMAND_VALUE.HEATER_DOWN;
            //}
        }
        private void btnLidHeaterUp_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_UP);
            this.Logger(log);
            lidHeaterState = (int)COMMAND_VALUE.HEATER_UP;
        }

        private void btnTrayIn_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            // 트레이를 꺼내기 전에 Lid Heater 의 상태를 원위치로 이동한다. (고장 발생 방지)
            btnLidHeaterUp_Click(this, null);
            Thread.Sleep(300);
            btnGoHome_Click(this, null);
            Thread.Sleep(300);

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_IN);
            this.Logger(log);
            trayState = (int)COMMAND_VALUE.TRAY_IN;
        }
        private void btnTrayInOut_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            // 트레이를 꺼내기 전에 Lid Heater 의 상태를 원위치로 이동한다. (고장 발생 방지)
            btnLidHeaterUp_Click(this, null);
            Thread.Sleep(300);
            btnGoHome_Click(this, null);
            Thread.Sleep(300);

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_OUT);
            this.Logger(log);
            trayState = (int)COMMAND_VALUE.TRAY_OUT;

            //if (trayState == (int)COMMAND_VALUE.TRAY_IN)
            //{
            //    Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_OUT);
            //    btnTrayInOut.Text = "Tray Out";
            //    trayState = (int)COMMAND_VALUE.TRAY_OUT;
            //}
            //else
            //{
            //    // 트레이를 꺼내기 전에 Lid Heater 의 상태를 원위치로 이동한다. (고장 발생 방지)
            //    btnGoHome_Click(this, null);
            //    Thread.Sleep(300);
            //    Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_UP);
            //    Thread.Sleep(300);
            //    if (lidHeaterState == (int)COMMAND_VALUE.HEATER_DOWN)
            //    {
            //        btnLidHeaterUpDn.Text = "Heater Down";
            //        lidHeaterState = (int)COMMAND_VALUE.HEATER_UP;
            //    }

            //    Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_IN);
            //    btnTrayInOut.Text = "Tray In";
            //    trayState = (int)COMMAND_VALUE.TRAY_IN;
            //}
        }

        private void cmb_serialMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmb_serialMenu.SelectedIndex == 0)           // A : 스텝모터 가속도값 (필터 휠) (0~3000, Default : 1000)
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 0;
            }
            else if (cmb_serialMenu.SelectedIndex == 1)     // E : 스텝모터 긴급정지 
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 1;
                serialValue.Value = 1;
            }
            else if (cmb_serialMenu.SelectedIndex == 2)     // G : 스텝모터 홈 위치로 이동
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 1;
                serialValue.Value = 1;
            }
            else if (cmb_serialMenu.SelectedIndex == 3)     // N : 스텝모터 해당 위치로 이동 (-1500~500)
            {
                serialValue.Minimum = -1500;
                serialValue.Maximum = 500;
                serialValue.Value = 0;
            }
            else if (cmb_serialMenu.SelectedIndex == 4)     // H : 스텝모터 홈 이동 속도 (FAST) (0~3000, Default : 3000)
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 3000;
            }
            else if (cmb_serialMenu.SelectedIndex == 5)     // S : 스텝모터 홈 이동 속도 (SLOW) (0~3000, Default : 50) 
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 50;
            }
            else if (cmb_serialMenu.SelectedIndex == 6)     // M : 스텝모터 해당 위치 이동 속도 (0~3000, Default : 1000)  
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 1000;
            }
            else if (cmb_serialMenu.SelectedIndex == 7)     // P : LED ON(100~150) / OFF(0) (0~255)
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 255;
                serialValue.Value = 0;
            }
            else if (cmb_serialMenu.SelectedIndex == 8)     // X : TRAY 열림(1000) / 닫힘(2000) (1000~2000)
            {
                serialValue.Minimum = 1000;
                serialValue.Maximum = 2000;
                serialValue.Value = 2000;
            }
            else if (cmb_serialMenu.SelectedIndex == 9)     // Y : 히터 상(2000)하(1000) 이동 (1000~2000) 
            {
                serialValue.Minimum = 1000;
                serialValue.Maximum = 2000;
                serialValue.Value = 2000;
            }
            else if (cmb_serialMenu.SelectedIndex == 10)     // p : LED 상태정보 얻어오기 
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 1;
                serialValue.Value = 1;
            }
            else if (cmb_serialMenu.SelectedIndex == 11)     // a : 스텝모터 현재 가속도값 얻어오기 
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 0;
            }
            else if (cmb_serialMenu.SelectedIndex == 12)     // n : 스텝모터 현재 위치 (-1500~500)
            {
                serialValue.Minimum = -1500;
                serialValue.Maximum = 500;
                serialValue.Value = 0;
            }
            else if (cmb_serialMenu.SelectedIndex == 13)     // h : 스텝모터 홈 이동 속도 (FAST) (0~3000, Default : 3000)
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 3000;
            }
            else if (cmb_serialMenu.SelectedIndex == 14)     // s : 스텝모터 홈 이동 속도 (SLOW) (0~3000, Default : 50)
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 50;
            }
            else if (cmb_serialMenu.SelectedIndex == 15)     // m : 스텝모터 해당 위치 이동 속도 (0~3000, Default : 1000)
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 3000;
                serialValue.Value = 1000;
            }
            else if (cmb_serialMenu.SelectedIndex == 15)     // o : 홀 센서 상태 정보 (ON:1, OFF:0)
            {
                serialValue.Minimum = 0;
                serialValue.Maximum = 1;
                serialValue.Value = 0;
            }

            label_dataRange.Text = String.Format("{0} ~ {1}", serialValue.Minimum, serialValue.Maximum);
        }

        private void btnSerialSend_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            log = Global.ArducamSerial.ArduinoCommand(cmb_serialMenu.SelectedIndex, (int)serialValue.Value, true);
            this.Logger(log);
        }

        private void btnOpticInit_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            Cursor.Current = Cursors.WaitCursor;         // 똥글뱅이 돌아가는 동그라미 커서

            // Lid Heater 가 Down(Forward) 상태에서 Tray(Chamber)를 동작하지 않도록 주의한다. 
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_UP);
            this.Logger(log);
            Thread.Sleep(100);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_COARSE_SPEED, (int)COMMAND_VALUE.STEP_COARSE_SPEED);
            this.Logger(log);
            Thread.Sleep(100);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_FINE_SPEED, (int)COMMAND_VALUE.STEP_FINE_SPEED);
            this.Logger(log);
            Thread.Sleep(100);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MAX_SPEED, (int)COMMAND_VALUE.STEP_MAX_SPEED);
            this.Logger(log);
            Thread.Sleep(100);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_ACCEL, (int)COMMAND_VALUE.STEP_ACCEL);
            this.Logger(log);
            Thread.Sleep(100);
            //log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_HOMING, 0);
            //this.Logger(log);
            //Thread.Sleep(100);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_IN);
            this.Logger(log);
            Thread.Sleep(100);

            Cursor.Current = Cursors.Default;         // 똥글뱅이 돌아가는 동그라미 커서
        }

        // 4장의 이미지를 캡쳐하기 전 실행한다. 
        private void StartCapture()
        {
            IsRunCapture = true;

            // Filter를 FAM 으로 이동한다. 
            //filterPos = 0;
            //log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_HOMING, 0);
            //this.Logger(log);
            //Thread.Sleep(300);

            // LED 를 ON 한다. 
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.LED_ONOFF, (int)COMMAND_VALUE.LED_ON);
            this.Logger(log);
            Thread.Sleep(300);

            // 노출시간에 따라 대기 시간을 달리한다. 
            // -1(1.25초), -2(0.75초), -3(1초), -4(0.75초)
            SetCamExposure(Global.ccdExposure);

            // Focus를 맞춘다. 
            SetCamFocus(Global.ccdFocus);

            this.IsSavedImage = false;
            filterCount = 0;

            // FAM Filter로 이동 
            filterPos = (int)COMMAND_VALUE.FILTER_POS_FAM;
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MOVING, filterPos);
            this.Logger(log);
            // 필터를 움직이는데 소요되는 시간 
            //Thread.Sleep(Global.filterMoveDelay);
            this.delayCount = 0;
            this.delayTime = Global.filterMoveDelay;

            this.IsSavedImage = true;
        }

        // 4장의 이미지를 캡쳐한 후 실행한다. 
        private void EndCapture()
        {
            // PCR Capture 완료 상태를 초기화한다. 
            Global.PCR_Manager.SetCurActionComplete(oldActiveNo, false);

            // Filter를 HOME 으로 이동한다. 
            filterPos = 0;
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_HOMING, filterPos);
            this.Logger(log);
            Thread.Sleep(300);

            // LED 를 OFF 한다. 
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.LED_ONOFF, (int)COMMAND_VALUE.LED_OFF);
            this.Logger(log);

            this.IsSavedImage = false;
            filterCount = 0;
            IsRunCapture = false;
            captureCount++;
            //oldActiveNo = -1;
        }

        private void customBtn_Save_Click(object sender, EventArgs e)
        {
            string imgPath = AppDomain.CurrentDomain.BaseDirectory + "Images\\";
            //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
            DirectoryInfo di = new DirectoryInfo(imgPath);
            if (di.Exists == false)
                di.Create();

            DateTime dt = DateTime.Now;
            excelFileName = string.Format("{1:yyyy}{1:MM}{2:dd}{3:HH}{4:mm}{5:ss}.xlsx", dt, dt, dt, dt, dt, dt);

            StartCapture();
        }

        // Focus 를 특정값으로 맞춘다.
        private string GetRoiValues(int filterType, ImageInfo shotImage)
        {
            string roiValues = filterType.ToString() + " : ";
            for (int rIndex = 0; rIndex < this.pictureBox_Raw.ImageInfo.listShape.Count; rIndex++)
            {
                ROIShape roi = this.pictureBox_Raw.ImageInfo.listShape[rIndex];

                // create a matrix for split
                //OpenCvSharp.Mat[] bgr = new OpenCvSharp.Mat[3];
                // split image into 3 single-channel matrices
                OpenCvSharp.Cv2.Split(shotImage.ToMat(), out OpenCvSharp.Mat[] bgr);

                // Red 영상으로 데이터를 추출한다. 
                if (filterType == 0 || filterType == 1) // FAM, HEX : Green 영상으로 데이터를 추출한다. 
                    roi.CalROIShare(bgr[1]);
                else                                    // ROX, CY5 : Red 영상으로 데이터를 추출한다. 
                    roi.CalROIShare(bgr[2]);

                roiValues += String.Format("{0:#.0}, ", roi.ROI_Average);
            }
            Logger($"{roiValues}");

            return roiValues;
        }

        // 결과값을 엑셀에 저장한다. 
        private void SaveExcel(string ExcelFileName, ImageInfo imgInfo)
        {
            if (imgInfo == null)
                return;

            string roiValues = imgInfo.FilterType.ToString() + " : ";

            //create a fileinfo object of an excel file on the disk
            FileInfo file = new FileInfo(ExcelFileName);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                bool IsSaved = false;

                ExcelWorksheet worksheet = null;
                if (package.Workbook.Worksheets.Count > 0)
                    worksheet = package.Workbook.Worksheets.First();
                else
                    worksheet = package.Workbook.Worksheets.Add(String.Format("{0}_ROI", imgInfo.ImageDateTime.ToString("yyyyMMdd_HHmm")));

                int lastRow = 1;
                int lastColumn = 1;
                int curRow = lastRow + 1;
                if (worksheet.Dimension != null)
                {
                    lastRow = worksheet.Dimension.End.Row;
                    lastColumn = worksheet.Dimension.End.Column;
                    curRow = lastRow + 1;
                }

                //ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(String.Format("{0}_{1}_ROI", study.patient.Name.Replace("^", ""), study.StudyDateTime.ToLocalTime().ToString("yyyyMMdd_HHmm")));

                //Add the headers
                if (lastRow == 1)
                {
                    bool IsTitle = false;
                    worksheet.Cells[1, 1].Value = "No.";
                    worksheet.Cells[1, 2].Value = "Filter";
                    worksheet.Cells[1, 3].Value = "Time";
                    worksheet.Cells[2, 2].Value = "Gain";
                    worksheet.Cells[3, 2].Value = "Offset";
                    for (int i = 0; i < this.pictureBox_Raw.ImageInfo.listShape.Count; i++)
                    {
                        ROIShape roi = this.pictureBox_Raw.ImageInfo.listShape[i];

                        int col = i + 3;
                        int wellNo = i + 1;
                        worksheet.Cells[1, i + 4].Value = wellNo.ToString();

                        double roiGain = roi.ROI_Gain;
                        double roiOffset = roi.ROI_Offset;
                        worksheet.Cells[2, i + 4].Value = roiGain.ToString();
                        worksheet.Cells[3, i + 4].Value = roiOffset.ToString();
                    }

                    curRow = 4;
                }

                worksheet.Cells[curRow, 1].Value = imgInfo.StudyNo.ToString();
                worksheet.Cells[curRow, 2].Value = imgInfo.FilterType.ToString();
                worksheet.Cells[curRow, 3].Value = imgInfo.StudyNo.ToString();

                // split image into 3 single-channel matrices
                //OpenCvSharp.Cv2.Split(this.pictureBox_Raw.ImageInfo.ToMat(), out OpenCvSharp.Mat[] bgr);
                OpenCvSharp.Cv2.Split(this.saveImgInfo.ToMat(), out OpenCvSharp.Mat[] bgr);

                int cellIndex = 2;
                // listview_ROIInfo에 정보 추가한다.         
                for (int rIndex = 0; rIndex < this.pictureBox_Raw.ImageInfo.listShape.Count; rIndex++)
                {
                    ROIShape roi = this.pictureBox_Raw.ImageInfo.listShape[rIndex];

                    // Green 영상으로 데이터를 추출한다. 
                    double roiAverage = 0.0;
                    if (imgInfo.FilterType == 0 || imgInfo.FilterType == 1)
                    {
                        roi.CalROIShareResize(bgr[1]);
                        roiAverage = roi.ROI_Average;
                    }
                    // Red 영상으로 데이터를 추출한다. 
                    else
                    {
                        roi.CalROIShareResize(bgr[2]);
                        roiAverage = roi.ROI_Average;
                    }

                    string strRoi = String.Format("{0:#.0}", roiAverage);
                    worksheet.Cells[curRow, rIndex + 4].Value = strRoi;

                    roiValues += String.Format("{0:#.0}, ", roiAverage);
                }
                bgr[0].Dispose();
                bgr[1].Dispose();
                bgr[2].Dispose();
                Logger($"{roiValues}");

                //save the changes
                package.Save();

                //if (cellIndex == 2) package.Workbook.Worksheets.Delete(worksheet);           // Worksheet에 저장된 ROI가 없으면 지운다.
                //else IsSaved = true;                                          // Worksheet에 저장된 ROI가 있으면 파일로 저장한다.

                //// 저장할 정보가 있으면 파일로 저장한다.
                //if (IsSaved)
                //{
                //    var xlFile = new FileInfo(ExcelFileName);
                //    if (xlFile.Exists) xlFile.Delete();  // ensures we create a new workbook
                //    // save our new workbook in the output directory and we are done!
                //    package.SaveAs(xlFile);

                //    MessageBox.Show(String.Format("The ROI information has been saved in the Excel file({0}).", ExcelFileName));
                //}
            }
        }

        // Focus 를 특정값으로 맞춘다.
        private void SetCamFocus(int nValue)
        {
            int index = (int)CAMERA_PROP.FOCUS;
            CameraProperty prop = Global.listCameraPropertys[index];

            EventHandler<EventArgs> Savedhandler = null;
            ManualResetEventSlim eventWaitHandle = new ManualResetEventSlim(false);
            Savedhandler = (sender, e) =>
            {
                prop.Saved -= Savedhandler;
                eventWaitHandle.Set();
            };

            if (nValue >= prop.Min && nValue <= prop.Max)
            {             
                prop.Flags = CameraPropertyFlags.Manual;
                prop.Value = nValue - 1;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();

                prop.Flags = CameraPropertyFlags.Manual;
                prop.Value = nValue;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();
            }
        }

        // 노출시간을 특정값으로 맞춘다.
        private void SetCamExposure(int nValue)
        {
            int index = (int)CAMERA_PROP.EXPOSURE;
            CameraProperty prop = Global.listCameraPropertys[index];

            EventHandler<EventArgs> Savedhandler = null;
            ManualResetEventSlim eventWaitHandle = new ManualResetEventSlim(false);
            Savedhandler = (sender, e) =>
            {
                prop.Saved -= Savedhandler;
                eventWaitHandle.Set();
            };

            if (nValue >= prop.Min && nValue <= prop.Max)
            {
                prop.Flags = CameraPropertyFlags.Automatic;
                prop.Value = nValue;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();

                prop.Flags = CameraPropertyFlags.Manual;
                prop.Value = nValue;
                prop.Saved += Savedhandler;
                prop.Save();
                eventWaitHandle.Wait();

            }
        }

        private void btnGoHome_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            chkFAM.Checked = false;
            chkHEX.Checked = false;
            chkROX.Checked = false;
            chkCY5.Checked = false;
            filterPos = 0;
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_HOMING, 0);
            this.Logger(log);
        }

        private void chkFAM_CheckedChanged(object sender, EventArgs e)
        {
            if(chkFAM.Checked)
            {
                chkHEX.Checked = false;
                chkROX.Checked = false;
                chkCY5.Checked = false;

                if (Global.ArducamSerial == null) return;
                filterPos = (int)COMMAND_VALUE.FILTER_POS_FAM;
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MOVING, (int)COMMAND_VALUE.FILTER_POS_FAM);
                this.Logger(log);
            }
        }

        private void chkHEX_CheckedChanged(object sender, EventArgs e)
        {
            if (chkHEX.Checked)
            {
                chkFAM.Checked = false;
                chkROX.Checked = false;
                chkCY5.Checked = false;

                if (Global.ArducamSerial == null) return;
                filterPos = (int)COMMAND_VALUE.FILTER_POS_HEX;
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MOVING, (int)COMMAND_VALUE.FILTER_POS_HEX);
                this.Logger(log);
            }
        }

        private void chkROX_CheckedChanged(object sender, EventArgs e)
        {
            if (chkROX.Checked)
            {
                chkFAM.Checked = false;
                chkHEX.Checked = false;
                chkCY5.Checked = false;

                if (Global.ArducamSerial == null) return;
                filterPos = (int)COMMAND_VALUE.FILTER_POS_ROX;
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MOVING, (int)COMMAND_VALUE.FILTER_POS_ROX);
                this.Logger(log);
            }
        }

        private void chkCY5_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCY5.Checked)
            {
                chkFAM.Checked = false;
                chkROX.Checked = false;
                chkHEX.Checked = false;

                if (Global.ArducamSerial == null) return;
                filterPos = (int)COMMAND_VALUE.FILTER_POS_CY5;
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MOVING, (int)COMMAND_VALUE.FILTER_POS_CY5);
                this.Logger(log);
             }
        }

        private void customBtn_Focus_Click(object sender, EventArgs e)
        {
            this.SetCamFocus(Global.ccdFocus);
        }

        private void customBtn_Exposure_Click(object sender, EventArgs e)
        {
            this.SetCamExposure(Global.ccdExposure);
        }

        private void btnEmergency_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;
            
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_EMERGENCY, 0);
            this.Logger(log);
        }

        private void comboBoxCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.IsCameraScan)
            {
                customBtn_Scan_Click(this, null);
                ChangeCamera(comboBoxCameras.SelectedIndex);
            }
        }

        // 시리얼 포트 Name을 선택한다. Close시에만 선택 가능
        private void comboBox_CommendSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_CommendSerialPort.Text.Length > 0)
                Global.ArducamPort = comboBox_CommendSerialPort.Text;
        }

        private void cameraPropertyCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cameraPropertyCombo.SelectedIndex;
            CameraProperty prop = Global.listCameraPropertys[index];

            if (index == (int)CAMERA_PROP.FOCUS)
                checkBox_AutoFocus.Enabled = true;
            else
                checkBox_AutoFocus.Enabled = false;

            if (index == (int)CAMERA_PROP.EXPOSURE)
                checkBox_AutoExposure.Enabled = true;
            else
                checkBox_AutoExposure.Enabled = false;

            string text = String.Format("[ {0}, {1} ], step: {2}", prop.Min, prop.Max, prop.MinimumStepSize);

            Int32 decimalPlaces;
            Decimal minimum, maximum, increment;
            minimum = prop.Min;
            maximum = prop.Max;
            increment = prop.MinimumStepSize;
            decimalPlaces = 0;

            cameraPropertyValue.Minimum = minimum;
            cameraPropertyValue.Maximum = maximum;
            cameraPropertyValue.Increment = increment;
            cameraPropertyValue.DecimalPlaces = decimalPlaces;

            cameraPropertyRangeValue.Text = text;
        }

        private void cameraPropertyValue_ValueChanged(object sender, EventArgs e)
        {
            int index = cameraPropertyCombo.SelectedIndex;
            CameraProperty prop = Global.listCameraPropertys[index];

            EventHandler<EventArgs> Savedhandler = null;
            ManualResetEventSlim eventWaitHandle = new ManualResetEventSlim(false);
            Savedhandler = (sender2, e2) =>
            {
                prop.Saved -= Savedhandler;
                eventWaitHandle.Set();
            };

            prop.Value = (int)cameraPropertyValue.Value;
            prop.Flags = CameraPropertyFlags.Manual;
            if (index == (int)CAMERA_PROP.FOCUS)
            {
                if (checkBox_AutoFocus.Checked)
                    prop.Flags = CameraPropertyFlags.Automatic; 
            }

            if (index == (int)CAMERA_PROP.EXPOSURE)
            {
                if (checkBox_AutoExposure.Checked)
                    prop.Flags = CameraPropertyFlags.Automatic;
            }
            prop.Saved += Savedhandler;
            prop.Save();
            eventWaitHandle.Wait();
        }

        private void customBtn_Resume_Click(object sender, EventArgs e)
        {
            Global.PCR_Manager.PcrResume();
        }

        private void customBtnExtract_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory + "Result\\";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;         // 똥글뱅이 돌아가는 동그라미 커서

                string saveFileName = Path.GetFileNameWithoutExtension(folderBrowserDialog.SelectedPath);
                DateTime dt = DateTime.Now;
                string excelFileName = string.Format("{0}.xlsx", saveFileName);

                //MessageBox.Show(folderBrowserDialog.SelectedPath);
                // 루트 디렉터리와 모든 하위 디렉터리에 있는 파일 목록을 가져옵니다.
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.png", SearchOption.AllDirectories);
                string strPath = folderBrowserDialog.SelectedPath + "\\" + excelFileName;
                captureCount = 0;
                for (int i = 0; i < files.Count(); i++)
                {
                    string filterNo = "";
                    int index = files[i].IndexOf(".png");
                    if (index > 0)
                        filterNo = files[i].Substring(index - 1, 1);
                    else
                        continue;

                    string bmpPath = files[i];
                    Image b1 = Bitmap.FromFile(bmpPath);
                    byte[] imageData = ImageToByteArray((Bitmap)b1);
                    b1.Dispose();
                    Array.Copy(imageData, saveImgInfo.ImageBuffer, imageData.Length);
                    //this.saveImgInfo.ImageDateTime = DateTime.Now;               // 저장할 때 시간을 입력한다.
                    //                                                            //this.saveWorker.RunWorkerAsync();
                    //Bitmap saveImage = this.saveImgInfo.CopyDataToBitmap(b1.Width, b1.Height, imageData);
                    //SaveCaptureImage(saveImage);

                    this.saveImgInfo.FilterType = Convert.ToInt32(filterNo);
                    this.saveImgInfo.StudyNo = captureCount;
                    SaveExcel(strPath, this.saveImgInfo);

                    if (this.saveImgInfo.FilterType == 3)
                        captureCount++;
                }

                captureCount = 0;
                Cursor.Current = Cursors.Default;              // 일반 화살표
            }
        }
    }
}
