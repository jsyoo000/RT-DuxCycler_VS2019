using AForge.Video;
using AForge.Video.DirectShow;
using BitmapControl;
using CameraControlLib;
using CustomClassLibrary;
using CustomWindowForm;
using DirectShowLib;
using Duxcycler.Properties;
using Duxcycler_Database;
using Duxcycler_GLOBAL;
using Duxcycler_Group;
using Duxcycler_IMAGE;
using IniParser;
using IniParser.Model;
using MathNet.Numerics;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ZedGraph;
using MathNet.Filtering;
using System.Text;

namespace Duxcycler
{
    public partial class MainPage : CustomForm
    {
        public string log = "";
        // Fade in and out.
        private bool m_bIsFadeIn = true;         // true: Fade In, false: Fade out
        private double m_dblOpacityIncrement = .2;           // 나타날때 사용 변수
        private double m_dblOpacityDecrement = .1;           // 사라질때 사용 변수
        private const int TIMER_INTERVAL = 50;           // 타이머 변수

        public int tabIndex = 0;

        // 스테이지 관련 변수 
        public List<Group_Stage> stageList = new List<Group_Stage>();
        public string methodFileName = "";
        public bool methodEdit = false;             // Method 가 수정되었는지의 여부 

        // Plate 관련 변수 
        public WellManager listWellInfo = new WellManager();
        public string plateFileName = "";

        public int plateTargetColIndex = 0;         // Target 리스트에서 선택된 Column Index
        public int plateTargetRowIndex = 0;         // Target 리스트에서 선택된 Row Index
        public int plateSampleColIndex = 0;         // Sample 리스트에서 선택된 Column Index
        public int plateSampleRowIndex = 0;         // Sample 리스트에서 선택된 Row Index
        public int plateBioGroupColIndex = 0;       // BioGroup 리스트에서 선택된 Column Index
        public int plateBioGroupRowIndex = 0;       // BioGroup 리스트에서 선택된 Row Index

        public int selectedSampleIndex = -1;        // Sample 리스트에서 선택된 Row Index
        public int selectedBioGroupIndex = -1;      // BioGroup 리스트에서 선택된 Row Index

        public string[] Target_Reporters = new string[] { "FAM", "HEX", "ROX", "CY5" };
        public string[] Target_Quenchers = new string[] { "None", "NFQ-MGB", "TAMRA" };

        public DataGridViewColorButtonCell buttonTargetCell = new DataGridViewColorButtonCell();
        public DataGridViewColorButtonCell buttonSampleCell = new DataGridViewColorButtonCell();
        public DataGridViewColorButtonCell buttonGroupCell = new DataGridViewColorButtonCell();

        public bool bReporterFind = false;

        #region Camera Property Controls
        public int delayTime = 0;                   // 타이머를 이용한 Sleep 시간 
        public int delayCount = 0;                  // 타이머를 이용한 Sleep Count  

        private Thread workerThread = null;                 // Thread 선언 
        private static Mutex mut = new Mutex();             // Create a new Mutex. The creating thread does not own the mutex.
        int nFrameRate = 0;                                 // 영상 이미지를 얼마나 처리하는지 표시함.

        public bool IsCameraScan = false;
        public bool IsCameraVisible = false;                // 카메라 초기화 성공 여부 
        #endregion

        //이미지 처리 클래스 
        BitmapImageCtl bitmapimageCtl = new BitmapImageCtl();
        double zoomValue = 1.0;

        // 이미지 저장 관련 
        ImageInfo saveImgInfo;                  // 이미지 저장용 변수
        public List<Byte[]> listImages = new List<Byte[]>();          // 저장할 이미지 리스트 List     
        bool IsRunCapture = false;              // 이미지 캡쳐  
        public bool IsSuspend = false;          // 이미지 캡쳐시 PCR 멈춤 여부   
        bool IsSavedImage = false;              // 한번 이미지 캡쳐할때마다 4개의 필터 이미지 저장 
        int filterCount = 0;                    // Filter Count 
        bool isSaving = false;
        string excelFileName = "";              // 저장할 엑셀 파일명 
        string methodResultPath = "";              // 저장할 폴더명  

        // PCR Device 통신 관련 변수 
        public bool bSelectEnable = false;
        public int timerCount = 0;
        public int captureCount = 0;
        public int oldActiveNo = -1;
        public int curActiveNo = -1;
        public int totalTime = 0;
        public int curTime = 0;
        public int maxTime = 0;
        public int saveTime = 0;        
        public bool isProgressInit = false;
        public int errorCheckTime = 0;
        public int errorCheckTimeLimit = 10 * 10;        // 10회 * 100ms(Timer 시간) * 2 = 10초
        public double errorCheckCorverTemp = 0;
        public double errorCheckInitTemp = 0;
        public bool isPcrStateNormal = false;            // PCR 이 정상적으로 시작 되었는지 여부 

        // Arduino 시리얼 통신 관련 
        public int ledState = (int)COMMAND_VALUE.LED_OFF;           // 0:OFF, 100~150:ON
        public int trayState = (int)COMMAND_VALUE.TRAY_IN;          // 2000:IN, 1000:OUT
        public int lidHeaterState = (int)COMMAND_VALUE.HEATER_DOWN; // 1000:Down, 2000:Up
        public int filterPos = 0;   // HOME:0, FAM:350, HEX:700, ROX:1050, CY5:1400

        // Save Raw Data
        private BackgroundWorker ExcelSavedworker;          // Excel Saved worker
        private string strSaveExcelFile = "";
        private bool ShowMessageBox = false;

        // 해당 그래프의 존재 유무  
        public bool[] isFAM = new bool[25];    // 해당 웰에 FAM Target 이 있는지 여부 
        public bool[] isHEX = new bool[25];    // 해당 웰에 HEX Target 이 있는지 여부 
        public bool[] isROX = new bool[25];    // 해당 웰에 ROX Target 이 있는지 여부 
        public bool[] isCY5 = new bool[25];    // 해당 웰에 CY5 Target 이 있는지 여부 

        // Raw Data 
        public List<double>[] listValuesFAM = new List<double>[25];
        public List<double>[] listValuesHEX = new List<double>[25];
        public List<double>[] listValuesROX = new List<double>[25];
        public List<double>[] listValuesCY5 = new List<double>[25];

        // Interpolation Data
        public List<double>[] listInterpolateFAM = new List<double>[25];
        public List<double>[] listInterpolateHEX = new List<double>[25];
        public List<double>[] listInterpolateROX = new List<double>[25];
        public List<double>[] listInterpolateCY5 = new List<double>[25];

        // BaseLine + Log 결과 데이터 
        public List<double>[] listResultFAM = new List<double>[25];
        public List<double>[] listResultHEX = new List<double>[25];
        public List<double>[] listResultROX = new List<double>[25];
        public List<double>[] listResultCY5 = new List<double>[25];

        // Ct 값 (전체 웰에서 최소 Ct 값 4개만 존재함.)
        public double resultThresholdFAM = 0.0;
        public double resultThresholdHEX = 0.0;
        public double resultThresholdROX = 0.0;
        public double resultThresholdCY5 = 0.0;

        // Ct 값에 해당하는 Y 결과 데이터 
        public List<double> listResultCtFAM = new List<double>();
        public List<double> listResultCtHEX = new List<double>();
        public List<double> listResultCtROX = new List<double>();
        public List<double> listResultCtCY5 = new List<double>();

        // Run Pulse Wave 
        ZedGraph.GraphPane PanePulseWave = new ZedGraph.GraphPane();
        ZedGraph.PointPairList[] listPointsFAM = new ZedGraph.PointPairList[25];
        ZedGraph.PointPairList[] listPointsHEX = new ZedGraph.PointPairList[25];
        ZedGraph.PointPairList[] listPointsROX = new ZedGraph.PointPairList[25];
        ZedGraph.PointPairList[] listPointsCY5 = new ZedGraph.PointPairList[25];
        ZedGraph.LineItem[] CurveFAM = new ZedGraph.LineItem[25];     // FAM, HEX, ROX, CY5
        ZedGraph.LineItem[] CurveHEX = new ZedGraph.LineItem[25];     // FAM, HEX, ROX, CY5
        ZedGraph.LineItem[] CurveROX = new ZedGraph.LineItem[25];     // FAM, HEX, ROX, CY5
        ZedGraph.LineItem[] CurveCY5 = new ZedGraph.LineItem[25];     // FAM, HEX, ROX, CY5

        // Result Pulse Wave 
        ZedGraph.GraphPane PaneResultWave = new ZedGraph.GraphPane();
        ZedGraph.PointPairList[] listResultPointsFAM = new ZedGraph.PointPairList[25];
        ZedGraph.PointPairList[] listResultPointsHEX = new ZedGraph.PointPairList[25];
        ZedGraph.PointPairList[] listResultPointsROX = new ZedGraph.PointPairList[25];
        ZedGraph.PointPairList[] listResultPointsCY5 = new ZedGraph.PointPairList[25];
        ZedGraph.PointPairList listThPointsFAM = new ZedGraph.PointPairList();
        ZedGraph.PointPairList listThPointsHEX = new ZedGraph.PointPairList();
        ZedGraph.PointPairList listThPointsROX = new ZedGraph.PointPairList();
        ZedGraph.PointPairList listThPointsCY5 = new ZedGraph.PointPairList();
        ZedGraph.LineItem[] CurveResultFAM = new ZedGraph.LineItem[25];     // FAM
        ZedGraph.LineItem[] CurveResultHEX = new ZedGraph.LineItem[25];     // HEX
        ZedGraph.LineItem[] CurveResultROX = new ZedGraph.LineItem[25];     // ROX
        ZedGraph.LineItem[] CurveResultCY5 = new ZedGraph.LineItem[25];     // CY5
        ZedGraph.LineItem CurveThFAM = null; // new ZedGraph.LineItem[25];     // FAM
        ZedGraph.LineItem CurveThHEX = null; // new ZedGraph.LineItem[25];     // HEX
        ZedGraph.LineItem CurveThROX = null; // new ZedGraph.LineItem[25];     // ROX
        ZedGraph.LineItem CurveThCY5 = null; // new ZedGraph.LineItem[25];     // CY5

        public int totalCycle = 0;     // 전체 캡쳐 사이클 횟수 (그래프에서는 최대 X 좌표로 활용)
        public int PulseCount = 0;
        const int CHART_VIEW_X_MAX = 5000;         ///< 500 * 10초 = 5000 , Pulse Wave를 10초보여준다.

        // DB 관련 변수 
        public bool bSearchAll = false;
        public string searchBarcode = "";                         // 검색할 Barcode
        public string searchName = "";                            // 검색할 Name
        public DateTime resultDateTime = DateTime.Now;            // 검색할 Result Date Time( 기본 Today로 설정 )
        public int selectResultIndex = -1;                        // 선택한 결과 인덱스 
        public string selectResultFile = "";                      // 선택한 결과 파일명

        /// <summary>
        /// MainPage 생성자 
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            this.ScreenIndex = Global.ScreensIndex;    // 기본 설정를 Ini 파일에서 읽는 부분 IntroPage Load함수에 있음.

            #region Form 기본 설정
            // Form에 나타날때 깜박거림을 줄이기 위한 코드                        
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.AutoScaleMode = AutoScaleMode.None;          // 모니터 해상도 영향 없게 설정   
            #endregion

            #region 사용되는 모든 Form은 이곳에서 생성한다. 생성한다.
            Global.main = this;                   // Main 화면 
                                                  //Global.scan = new ScanPage();        // SCan 화면            
            #endregion
        }

        /// <summary>
        /// MainPage 초기화 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_Load(object sender, EventArgs e)
        {
            this.Hide();
            this.Panel_Show.Initial_Size();
            this.ResizePanel_Menu.Initial_Size();
            this.ResizePanel_SubMemu1.Initial_Size();
            this.ResizePanel_SubMemu2.Initial_Size();
            this.ResizePanel_SubMemu3.Initial_Size();
            this.resizePanel_Run.Initial_Size();

            // 현재 Export 탭은 구현되지 않아서 임시로 삭제함. 
            TabControl_Selected.TabPages.Remove(tabPage1);
            TabControl_Selected.TabPages.Remove(tabPage4);
            TabControl_Selected.TabPages.Remove(tabPage6);

            // PCR 통신 초기화 
            Global.m_Serialnum = Global.PCR_Manager.PCR_Init();

            // 시리얼 통신 초기화 부분
            if (Global.ArducamSerial == null) Global.ArducamSerial = new SerialManager();
            if (!Global.ArducamSerial.IsOpen)
            {
                Global.ArducamSerial.PortName = Global.ArducamPort;
                Global.ArducamSerial.BaudRate = Global.ArducamBaudRate;

                // 시리얼 통신을 Open 한다.
                Global.ArducamSerial.OpenPort();
            }

            // Camera 초기화 
            IsCameraVisible = InitCamera(Global.ccdCameraNo);

            // COM Port 가 비정상이거나 카메라가 비정상이면 Run 버튼을 비활성화 한다. 
            if (!Global.ArducamSerial.IsOpen || !IsCameraVisible)
            {
                pictureBox_comGreen.Visible = false;
                pictureBox_comDisable.Visible = true;

                btnStartRun.Enabled = false;
            }
            else
            {
                pictureBox_comGreen.Visible = true;
                pictureBox_comDisable.Visible = false;

                btnStartRun.Enabled = true;
            }

            LoadProperties();            // Property 를 초기화한다. 
            ReloadLayout();              // 화면 Layout를 구성한다.
            ReloadWellTable();           // Well Table 을 재설정한다. 
            UpdateWellTable("", "", "");

            // 최대,최소,간격을 임의로 조정
            progressBar_Wait.Style = ProgressBarStyle.Blocks;
            progressBar_Wait.Minimum = 0;
            progressBar_Wait.Maximum = 100;
            progressBar_Wait.Step = 2;
            progressBar_Wait.Value = 30;

            this.Show();
            this.Activate();
            IntroPage.CloseForm();                              // 인트로 화면을 닫는다.

            // Method 탭를 초기화한다. 
            flatComboBox_StageType.SelectedIndex = (int)eGROUP_TYPE.PCR_STAGE;
            MethodInit();
            // Plate 탭을 초기화한다. 
            PlateInit();
            // Well Table 에 이미지를 채운다.             
            UpdateWellTable("", "", "");

            // Run Graph 초기화 
            CreateGraph(this.ZedGraph_Pulse);
            this.ZedGraph_Pulse.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(MyContextMemuBuilder);
            InitRunGraph(0, 100, 0, 260);

            // Result Graph 초기화 
            CreateResultGraph(this.zedGraph_Result);
            this.zedGraph_Result.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(MyContextMemuBuilder);
            InitResultGraph(0, 100, 0, 260);

            if (Global.IsInterpolation)
                Global.baseAvgScale = Global.graphInterpolationScale;

            // Threshold Color 초기화 
            colorBtn_thFAM.Color = Global.colorList[Global.thFAMColorIndex];
            colorBtn_thHEX.Color = Global.colorList[Global.thHEXColorIndex];
            colorBtn_thROX.Color = Global.colorList[Global.thROXColorIndex];
            colorBtn_thCY5.Color = Global.colorList[Global.thCY5ColorIndex];
            colorBtn_thFAM.ColorIndex = Global.thFAMColorIndex;
            colorBtn_thHEX.ColorIndex = Global.thHEXColorIndex;
            colorBtn_thROX.ColorIndex = Global.thROXColorIndex;
            colorBtn_thCY5.ColorIndex = Global.thCY5ColorIndex;

            btnThApply.Enabled = false;
            btnThAuto.Enabled = false;

            flatComboBox_GraphType.Items.Add("Raw Data Plot");
            flatComboBox_GraphType.Items.Add("Base Line Plot");
            flatComboBox_GraphType.Items.Add("Threshold Plot");
            flatComboBox_GraphType.Items.Add("Sigmoidal Curve Plot");
            flatComboBox_GraphType.SelectedIndex = 0;

            dateTime_SearchStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);       // 검색할 Study Date Low( 기본 Today로 설정 )
            dateTime_SearchEnd.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);       // 검색할 Study Date High( 기본 Today로 설정 )
            //SearchResult();

            this.saveImgInfo = new ImageInfo
            {
                Image_Width = Global.SAVEDIMAGE_X,                             // 이미지 x Resultion 입력
                Image_Height = Global.SAVEDIMAGE_Y,                             // 이미지 y Resultion 입력
                Pixel_Format = 24,
                minWindowLevel = Convert.ToInt32(Global.LowTemperature),                           // min WindowLevel 입력
                maxWindowLevel = Convert.ToInt32(Global.HighTemperature),                           // max WindowLevel 입력
                iIsoLow = Convert.ToInt32(Global.LowTemperature),                                 // 원본(오른쪽) 영상에서 값을 가지고 올때 최소값
                iIsoHigh = Convert.ToInt32(Global.HighTemperature),                                // 원본(오른쪽) 영상에서 값을 가지고 올때 최대값                                                               
                selectedPaletteType = bitmapimageCtl.selectedType,                                             // 선택된 Palette Type를 저장한다. 이것은 PALETTE COLOR {0}, type 형태로 저장된다.
                ImagePalette = bitmapimageCtl.GetPalette(Convert.ToInt32(Global.HighTemperature), Convert.ToInt32(Global.LowTemperature), false), // 적용된 Palette 입력
                IsWL_BG = false,
                //ImageBuffer = new Byte[this.SAVEDIMAGE_X * this.SAVEDIMAGE_Y * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
                //ImageBuffer = new Byte[(int)vidWidth * (int)vidHeight * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
                ImageBuffer = new Byte[(int)Global.SAVEDIMAGE_X * (int)Global.SAVEDIMAGE_Y * 3]// 결과 이미지 저장( Resize, Filter 적용 이미지 )
            };

            Global.MethodPath = AppDomain.CurrentDomain.BaseDirectory + "Method\\";
            Global.PlatePath = AppDomain.CurrentDomain.BaseDirectory + "Plate\\";
            Global.ResultPath = AppDomain.CurrentDomain.BaseDirectory + "Result\\";

            // 파일 저장용 BackgroundWorker 설정 부분
            ExcelSavedworker = new BackgroundWorker();
            ExcelSavedworker.WorkerReportsProgress = false;
            ExcelSavedworker.WorkerSupportsCancellation = false;
            ExcelSavedworker.DoWork += new DoWorkEventHandler(ExcelSavedWorker_DoWork);

            ExcelSavedworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExcelSavedWorker_RunWorkerCompleted);

            SaveProperties();

            this.WindowState = FormWindowState.Maximized;       // Form 최대 Size로   
        }


        private void ExcelSavedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;

            if (this.ShowMessageBox) MessageBox.Show($"'{this.strSaveExcelFile}' saved is completed!!");
        }

        private void ExcelSavedWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SavedRawData(this.strSaveExcelFile);
        }

        #region Camera 설정 관련 함수  
        /// <summary>
        /// 웹캠 초기화 함수 
        /// </summary>
        /// <returns></returns>
        public bool CamStart()
        {
            // open it
            OpenVideoSource(Global.videoSource);

            return true;
        }

        /// <summary>
        /// Close video source if it is running 
        /// </summary>
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();

                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                videoSourcePlayer.Stop();
                videoSourcePlayer.VideoSource = null;
            }
        }

        /// <summary>
        /// Open video source 
        /// </summary>
        /// <param name="source">선택 카메라</param>
        private void OpenVideoSource(IVideoSource source)
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // stop current video source
            CloseCurrentVideoSource();

            Global.videoSource.NewFrame += new NewFrameEventHandler(video_MainFrame);

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();

            // reset stop watch
            Global.stopWatch = null;

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// eventhandler if new frame is ready 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void video_MainFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //mut.WaitOne();
            if (this.IsCameraScan)
            {
                //mut.WaitOne();

                Bitmap img = (Bitmap)eventArgs.Frame.Clone();
                // Rotate the image by 180 degrees
                img.RotateFlip(RotateFlipType.Rotate180FlipNone);

                int xResultion = Global.SAVEDIMAGE_X;
                int yResultion = Global.SAVEDIMAGE_Y;

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

                //mut.ReleaseMutex();
            }
            //mut.ReleaseMutex();

            Thread.Sleep(100);
        }

        /// <summary>
        /// 이미지 저장 함수  
        /// </summary>
        /// <param name="saveImage">저장할 이미지</param>
        private void SaveCaptureImage(Bitmap saveImage)
        {
            isSaving = true;

            // 파일을 저장한다. 
            string imgPath = this.methodResultPath;
            DateTime dt = DateTime.Now;
            string fileName = string.Format("{0:yyyy}{1:MM}{2:dd}{3:HH}{4:mm}{5:ss}_{6}.png", dt, dt, dt, dt, dt, dt, filterCount);
            string strPath = imgPath + "\\" + fileName;

            //Bitmap saveImage = this.saveImgInfo.ToColorBitmap();
            saveImage.Save(strPath, System.Drawing.Imaging.ImageFormat.Png);

            strPath = imgPath + "\\" + excelFileName;
            this.saveImgInfo.FilterType = filterCount;
            this.saveImgInfo.StudyNo = captureCount;
            SaveExcel(strPath, this.saveImgInfo);

            // ROI 값을 추출한다. 
            this.filterCount++;

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
            }

            isSaving = false;
        }

        /// <summary>
        /// Camera Setting 값들을 적용한다.   
        /// </summary>
        public void ApplyCamera()
        {
            EventHandler<EventArgs> Savedhandler = null;
            ManualResetEventSlim eventWaitHandle = new ManualResetEventSlim(false);

            // 노출시간을 맞춘다. 
            SetCamExposure(Global.ccdExposure);

            // 포커스를 맞춘다. 
            SetCamFocus(Global.ccdFocus);

            int index = (int)CAMERA_PROP.BRIGHTNESS;
            CameraProperty prop = Global.listCameraPropertys[index];

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

        /// <summary>
        /// Camera 초기화  
        /// </summary>
        /// <param name="devIndex">선택 카메라 인덱스</param>
        /// <returns>성공 여부</returns>
        public bool InitCamera(int devIndex)
        {
            bool isCamVisible = false;
            int devWidth = Global.SAVEDIMAGE_X;
            int devHeigth = Global.SAVEDIMAGE_Y;

            List<CameraDescriptor> _availableCameras = CameraDescriptor.GetAll();
            DsDevice[] cameraDevices = DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.VideoInputDevice);

            string camDescr = string.Empty;
            string camName = string.Empty;
            
            if (devIndex >= _availableCameras.Count)
                return false;

            camDescr = _availableCameras[devIndex].DevicePath;
            camName = _availableCameras[devIndex].Name;

            FilterInfoCollection videoDevices;
            videoDevices = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);

            DsDevice exactMatch = cameraDevices.FirstOrDefault(d => d.Name == camName && d.DevicePath == camDescr);
            DsDevice matchingDevice = cameraDevices.FirstOrDefault(d => d.Name == camName);
            if (matchingDevice == null)
                throw new InvalidOperationException("Could not find selected camera device");

            Global.videoSource = new VideoCaptureDevice(camDescr);
            Global.videoCapabilities = Global.videoSource.VideoCapabilities;
            Global.snapshotCapabilities = Global.videoSource.SnapshotCapabilities;

            var preferredCamera = CameraDescriptor.Find(camName, camDescr);
            Global.selectCam = preferredCamera?.Create();

            Global.videoSource.ProvideSnapshots = true;
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

            if (Global.selectCam != null)
            {
                Global.listCameraPropertys.Clear();
                Global.listCameraPropertys = Global.selectCam.GetSupportedProperties();

                isCamVisible = true;
            }
            else
                isCamVisible = false;

            return isCamVisible;
        }

        /// <summary>
        /// Focus 를 특정값으로 맞춘다. 
        /// </summary>
        /// <param name="nValue"></param>
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

        /// <summary>
        /// 노출시간을 특정값으로 맞춘다. 
        /// </summary>
        /// <param name="nValue"></param>
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
#endregion

        /// <summary>
        /// 결과파일에 Propertie 정보를 저장한다. 
        /// </summary>
        /// <param name="ExcelFileName">결과파일 이름</param>
        private void SaveExcelProperties(string ExcelFileName)
        {
            //create a fileinfo object of an excel file on the disk
            FileInfo file = new FileInfo(ExcelFileName);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // Properties 탭이 있으면 지우고 새로 생성하여 저장한다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Properties")
                    {
                        resultIndex = i;
                        break;
                    }
                }
                if (resultIndex >= 0)
                    package.Workbook.Worksheets.Delete(worksheet);

                worksheet = package.Workbook.Worksheets.Add("Properties");

                worksheet.Cells[1, 1].Value = Global.selectedResult.UserName;
                worksheet.Cells[2, 1].Value = Global.selectedResult.Barcode;
                worksheet.Cells[3, 1].Value = Global.selectedResult.InstrumentType;
                worksheet.Cells[4, 1].Value = Global.selectedResult.BlockType;
                worksheet.Cells[5, 1].Value = Global.selectedResult.ExperimentType.ToString();
                worksheet.Cells[6, 1].Value = Global.selectedResult.Chemisty;
                worksheet.Cells[7, 1].Value = Global.selectedResult.RunMode.ToString();
                worksheet.Cells[8, 1].Value = Global.selectedResult.Volume;
                worksheet.Cells[9, 1].Value = Global.selectedResult.Cover;
                worksheet.Cells[10, 1].Value = Global.selectedResult.Comment;

                package.Save();
            }
        }

        /// <summary>
        /// 결과파일과 Method 파일에 Method 정보를 저장한다.  
        /// </summary>
        /// <param name="ExcelFileName">결과파일 이름</param>
        /// <param name="MethodFileName">Method 파일 이름</param>
        private void SaveExcelMethod(string ExcelFileName, string MethodFileName)
        {
            //create a fileinfo object of an excel file on the disk
            FileInfo file = new FileInfo(ExcelFileName);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // Result 탭이 있으면 지우고 새로 생성하여 저장한다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Method")
                    {
                        resultIndex = i;
                        break;
                    }
                }
                if (resultIndex >= 0)
                    package.Workbook.Worksheets.Delete(worksheet);

                worksheet = package.Workbook.Worksheets.Add("Method");

                var lines = File.ReadAllLines(MethodFileName);
                var rowCounter = 1;
                foreach (var line in lines)
                {
                    worksheet.Cells[rowCounter, 1].Value = line;

                    rowCounter++;
                }

                package.Save();
            }
        }

        /// <summary>
        /// 결과파일과 Plate 파일에 Plate 정보를 저장한다.  
        /// </summary>
        /// <param name="ExcelFileName">결과파일 이름</param>
        /// <param name="PlateFileName">Plate 파일 이름</param>
        private void SaveExcelPlate(string ExcelFileName, string PlateFileName)
        {
            //create a fileinfo object of an excel file on the disk
            FileInfo file = new FileInfo(ExcelFileName);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // Plate 탭이 있으면 지우고 새로 생성하여 저장한다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Plate")
                    {
                        resultIndex = i;
                        break;
                    }
                }
                if (resultIndex >= 0)
                    package.Workbook.Worksheets.Delete(worksheet);

                worksheet = package.Workbook.Worksheets.Add("Plate");

                var lines = File.ReadAllLines(PlateFileName);
                var rowCounter = 1;
                foreach (var line in lines)
                {
                    worksheet.Cells[rowCounter, 1].Value = line;

                    rowCounter++;
                }

                package.Save();
            }
        }

        /// <summary>
        /// 영상에서 결과값을 계산하고 결과파일에 결과값을 저장한다.  
        /// </summary>
        /// <param name="ExcelFileName">결과파일 이름</param>
        /// <param name="imgInfo">영상정보</param>
        private void SaveExcel(string ExcelFileName, ImageInfo imgInfo)
        {
            string roiValues = imgInfo.FilterType.ToString() + " : ";

            //create a fileinfo object of an excel file on the disk
            FileInfo file = new FileInfo(ExcelFileName);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // Result 탭이 있으면 Result 탭에 저장하고 없으면 새로 생성하여 저장한다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Result")
                    {
                        resultIndex = i;
                        break;
                    }
                }
                if (resultIndex == -1)
                    worksheet = package.Workbook.Worksheets.Add("Result");

                //if (package.Workbook.Worksheets.Count > 0)
                //    worksheet = package.Workbook.Worksheets.First();
                //else
                //    worksheet = package.Workbook.Worksheets.Add(String.Format("{0}_ROI", imgInfo.ImageDateTime.ToString("yyyyMMdd_HHmm")));

                int lastRow = 1;
                int lastColumn = 1;
                int curRow = lastRow + 1;
                if (worksheet.Dimension != null)
                {
                    lastRow = worksheet.Dimension.End.Row;
                    lastColumn = worksheet.Dimension.End.Column;
                    curRow = lastRow + 1;
                }

                //Add the headers
                if (lastRow == 1)
                {    
                    worksheet.Cells[1, 1].Value = "No.";
                    worksheet.Cells[1, 2].Value = "Filter";
                    worksheet.Cells[1, 3].Value = "Time";
                    worksheet.Cells[2, 2].Value = "Gain";
                    worksheet.Cells[3, 2].Value = "Offset";
                    for (int i = 0; i < Global.listRoiInfos.Count; i++)
                    {
                        ROIShape roi = Global.listRoiInfos[i];

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
                int saveTime = maxTime - totalTime;
                worksheet.Cells[curRow, 3].Value = saveTime.ToString();

                // split image into 3 single-channel matrices
                //OpenCvSharp.Cv2.Split(this.pictureBox_Raw.ImageInfo.ToMat(), out OpenCvSharp.Mat[] bgr);
                OpenCvSharp.Cv2.Split(this.saveImgInfo.ToMat(), out OpenCvSharp.Mat[] bgr);

                // listview_ROIInfo에 정보 추가한다.         
                for (int rIndex = 0; rIndex < Global.listRoiInfos.Count; rIndex++)
                {
                    ROIShape roi = Global.listRoiInfos[rIndex];

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

                    int xPos = imgInfo.StudyNo;
                    // 그래프 포인트를 저장한다. 
                    if (imgInfo.FilterType == 1)
                        listPointsHEX[rIndex].Add(new ZedGraph.PointPair(xPos, roiAverage));
                    else if (imgInfo.FilterType == 2)
                        listPointsROX[rIndex].Add(new ZedGraph.PointPair(xPos, roiAverage));
                    else if (imgInfo.FilterType == 3)
                        listPointsCY5[rIndex].Add(new ZedGraph.PointPair(xPos, roiAverage));
                    else
                        listPointsFAM[rIndex].Add(new ZedGraph.PointPair(xPos, roiAverage));

                    //PanePulseWave.XAxis.Scale.Min = 0;
                    //PanePulseWave.XAxis.Scale.Max = maxTime + 60;
                    this.ZedGraph_Pulse.AxisChange();
                    //this.ZedGraph_Pulse.Refresh();
                    this.ZedGraph_Pulse.Invalidate();

                    string strRoi = String.Format("{0:#.0}", roiAverage);
                    //worksheet.Cells[curRow, rIndex + 4].Value = xPos.ToString();
                    //worksheet.Cells[curRow + 1, rIndex + 4].Value = strRoi;
                    worksheet.Cells[curRow, rIndex + 4].Value = strRoi;

                    roiValues += String.Format("{0:#.0}, ", roiAverage);
                }
                bgr[0].Dispose();
                bgr[1].Dispose();
                bgr[2].Dispose();
                Logger($"{roiValues}");

                //save the changes
                package.Save();
            }
        }

        /// <summary>
        /// Result File Load 버튼 클릭 이벤트 함수 (현재 사용 안함.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtnResultLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Result\\";
            //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
            DirectoryInfo di = new DirectoryInfo(openFileDlg.InitialDirectory);
            if (di.Exists == false)
                di.Create();
            openFileDlg.Filter = "Excel File(*.xlsx)|*.xlsx";
            openFileDlg.Title = "Load an Result File";
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string fullPath = Path.GetFullPath(openFileDlg.FileName);

                this.Cursor = Cursors.WaitCursor;

                // 결과 파일에서 Properties 탭 데이터를 읽어온다. 
                LoadExcelProperties(fullPath);
                // 결과 파일에서 Method 탭 데이터를 읽어온다. 
                LoadExcelMethod(fullPath);
                // 결과 파일에서 Plate 탭 데이터를 읽어온다. 
                LoadExcelPlate(fullPath);
                // 결과 파일에서 CtValue 탭 데이터를 읽어온다. 
                LoadExcelCtValue(fullPath);
                // 결과 파일에서 Result 탭 데이터를 읽어온다. 
                LoadResult(fullPath);
                flatComboBox_GraphType.SelectedIndex = 0;

                SaveProperties();

                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Result File Open 버튼 클릭 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResultOpen_Click(object sender, EventArgs e)
        {
            methodEdit = false;

            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Result\\";
            //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
            DirectoryInfo di = new DirectoryInfo(openFileDlg.InitialDirectory);
            if (di.Exists == false)
                di.Create();
            openFileDlg.Filter = "Excel File(*.xlsx)|*.xlsx";
            openFileDlg.Title = "Load an Result File";
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string fullPath = Path.GetFullPath(openFileDlg.FileName);

                this.Cursor = Cursors.WaitCursor;

                // 결과 파일에서 Properties 탭 데이터를 읽어온다. 
                LoadExcelProperties(fullPath);
                // 결과 파일에서 Method 탭 데이터를 읽어온다. 
                LoadExcelMethod(fullPath);
                // 결과 파일에서 Plate 탭 데이터를 읽어온다. 
                LoadExcelPlate(fullPath);
                // 결과 파일에서 CtValue 탭 데이터를 읽어온다. 
                LoadExcelCtValue(fullPath);
                // 결과 파일에서 Result 탭 데이터를 읽어온다. 
                LoadResult(fullPath);
                flatComboBox_GraphType.SelectedIndex = 0;

                SaveProperties();

                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// 결과 파일에서 Properties 탭 데이터를 읽어온다.  
        /// </summary>
        /// <param name="resultPath">결과파일 이름</param>
        public void LoadExcelProperties(string resultPath)
        {
            if (Global.selectedResult == null)
                return;

            FileInfo fileInfo = new FileInfo(resultPath);
            using (var package = new ExcelPackage(fileInfo))
            {
                if (package.Workbook.Worksheets.Count <= 0)
                    return;

                // Result 탭이 있는지 찾고 없으면 더이상 진행하지 않는다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Properties")
                    {
                        resultIndex = i;
                        break;
                    }
                }

                if (resultIndex == -1)
                {
                    //MessageBox.Show("Not found Result !");
                    return;
                }

                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int rowCount = worksheet.Dimension.End.Row;     //get row count
                if (rowCount > 9)
                    rowCount = 9;

                string strRow = "";
                Global.selectedResult.UserName = strRow;
                Global.selectedResult.Barcode = strRow;
                Global.selectedResult.InstrumentType = strRow;
                Global.selectedResult.BlockType = strRow;
                Global.selectedResult.ExperimentType = 0;
                Global.selectedResult.Chemisty = strRow;
                Global.selectedResult.RunMode = 0;
                Global.selectedResult.Volume = strRow;
                Global.selectedResult.Cover = strRow;
                Global.selectedResult.Comment = strRow;

                //We will start from row 2 since row 1 is a header row.
                for (int row = 1; row <= rowCount; row++)
                {
                    strRow = "";
                    if (worksheet.Cells[row, 1].Value != null)
                    {
                        strRow = worksheet.Cells[row, 1].Value.ToString();
                        if (row == 1) Global.selectedResult.UserName = strRow;
                        else if (row == 2) Global.selectedResult.Barcode = strRow;
                        else if (row == 3) Global.selectedResult.InstrumentType = strRow;
                        else if (row == 4) Global.selectedResult.BlockType = strRow;
                        else if (row == 5) Global.selectedResult.ExperimentType = Convert.ToInt32(strRow);
                        else if (row == 6) Global.selectedResult.Chemisty = strRow;
                        else if (row == 7) Global.selectedResult.RunMode = Convert.ToInt32(strRow);
                        else if (row == 8) Global.selectedResult.Volume = strRow;
                        else if (row == 9) Global.selectedResult.Cover = strRow;
                        else if (row == 10) Global.selectedResult.Comment = strRow;
                    }
                }

                // Properties 탭을 셋팅한다. 
                Global.UserName = Global.selectedResult.UserName;
                Global.InstrumentType = Global.selectedResult.InstrumentType;
                Global.ExperimentType = Global.selectedResult.ExperimentType;
                Global.Chemisty = Global.selectedResult.Chemisty;
                Global.Volume = Global.selectedResult.Volume;
                Global.Cover = Global.selectedResult.Cover;
                Global.Comment = Global.selectedResult.Comment;

                LoadProperties();
            }
        }

        /// <summary>
        /// 결과파일에서 Method 탭 데이터를 읽어온다.  
        /// </summary>
        /// <param name="resultPath">결과파일 이름</param>
        public void LoadExcelMethod(string resultPath)
        {
            if (Global.selectedResult == null)
                return;

            FileInfo fileInfo = new FileInfo(resultPath);
            using (var package = new ExcelPackage(fileInfo))
            {
                if (package.Workbook.Worksheets.Count <= 0)
                    return;

                // Result 탭이 있는지 찾고 없으면 더이상 진행하지 않는다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Method")
                    {
                        resultIndex = i;
                        break;
                    }
                }

                if (resultIndex == -1)
                {
                    //MessageBox.Show("Not found Result !");
                    return;
                }

                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int rowCount = worksheet.Dimension.End.Row;     //get row count
                string strMethod = "";
                //We will start from row 2 since row 1 is a header row.
                for (int row = 1; row <= rowCount; row++)
                {
                    string strRow = "";
                    if (worksheet.Cells[row, 1].Value != null)
                    {
                        strRow = worksheet.Cells[row, 1].Value.ToString();
                        strMethod += strRow + "\r\n";
                    }
                    else
                        strMethod += "\r\n";
                }

                // Method 파일로 저장하고 다시 읽어온다. 
                //Global.MethodPath = Global.ResultPath.Replace("\\Result\\", "\\Method\\");
                //Global.MethodPath = AppDomain.CurrentDomain.BaseDirectory + "Method\\";
                Global.MethodFile = Path.GetFileNameWithoutExtension(resultPath) + ".method";
                this.methodFileName = Global.MethodPath + Global.MethodFile;

                //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
                DirectoryInfo di = new DirectoryInfo(Global.MethodPath);
                if (di.Exists == false)
                    di.Create();

                string saveFileName = Global.MethodPath + Global.MethodFile;
                File.WriteAllText(saveFileName, strMethod, Encoding.Default);

                Thread.Sleep(500);

                LoadMethod(saveFileName);
            }
        }

        /// <summary>
        /// 결과파일에서 Plate 탭 데이터를 읽어온다.  
        /// </summary>
        /// <param name="resultPath">결과파일 이름</param>
        public void LoadExcelPlate(string resultPath)
        {
            if (Global.selectedResult == null)
                return;

            FileInfo fileInfo = new FileInfo(resultPath);
            using (var package = new ExcelPackage(fileInfo))
            {
                if (package.Workbook.Worksheets.Count <= 0)
                    return;

                // Result 탭이 있는지 찾고 없으면 더이상 진행하지 않는다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Plate")
                    {
                        resultIndex = i;
                        break;
                    }
                }

                if (resultIndex == -1)
                {
                    //MessageBox.Show("Not found Result !");
                    return;
                }

                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int rowCount = worksheet.Dimension.End.Row;     //get row count
                string strMethod = "";
                //We will start from row 2 since row 1 is a header row.
                for (int row = 1; row <= rowCount; row++)
                {
                    string strRow = "";
                    if (worksheet.Cells[row, 1].Value != null)
                    {
                        strRow = worksheet.Cells[row, 1].Value.ToString();
                        strMethod += strRow + "\r\n";
                    }
                    else
                        strMethod += "\r\n";
                }

                // Method 파일로 저장하고 다시 읽어온다. 
                //Global.PlatePath = Global.ResultPath.Replace("\\Result\\", "\\Plate\\");
                //Global.MethodPath = AppDomain.CurrentDomain.BaseDirectory + "Method\\";
                Global.PlateFile = Path.GetFileNameWithoutExtension(resultPath) + ".plate";

                //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
                DirectoryInfo di = new DirectoryInfo(Global.PlatePath);
                if (di.Exists == false)
                    di.Create();

                string saveFileName = Global.PlatePath + Global.PlateFile;
                File.WriteAllText(saveFileName, strMethod, Encoding.Default);

                Thread.Sleep(500);

                LoadPlate(saveFileName);
            }
        }

        /// <summary>
        /// 결과파일에서 CtValue 탭 데이터를 읽어온다.  
        /// </summary>
        /// <param name="resultPath">결과파일 이름</param>
        public void LoadExcelCtValue(string resultPath)
        {
            if (Global.selectedResult == null)
                return;

            FileInfo fileInfo = new FileInfo(resultPath);
            using (var package = new ExcelPackage(fileInfo))
            {
                if (package.Workbook.Worksheets.Count <= 0)
                    return;

                // Result 탭이 있는지 찾고 없으면 더이상 진행하지 않는다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "CtValue")
                    {
                        resultIndex = i;
                        break;
                    }
                }

                if (resultIndex == -1)
                {
                    //MessageBox.Show("Not found Result !");
                    return;
                }

                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int rowCount = worksheet.Dimension.End.Row;     //get row count
                string strValue = "";
                // Threshold 값을 읽어온다. 
                if (worksheet.Cells[1, 1].Value != null)    // FAM Threshold
                {
                    strValue = worksheet.Cells[1, 1].Value.ToString();
                    listWellInfo.ThFAM = Convert.ToDouble(strValue);
                }
                if (worksheet.Cells[1, 2].Value != null)    // HEX Threshold
                {
                    strValue = worksheet.Cells[1, 2].Value.ToString();
                    listWellInfo.ThHEX = Convert.ToDouble(strValue);
                }
                if (worksheet.Cells[1, 3].Value != null)    // ROX Threshold
                {
                    strValue = worksheet.Cells[1, 3].Value.ToString();
                    listWellInfo.ThROX = Convert.ToDouble(strValue);
                }
                if (worksheet.Cells[1, 4].Value != null)    // CY5 Threshold
                {
                    strValue = worksheet.Cells[1, 4].Value.ToString();
                    listWellInfo.ThCY5 = Convert.ToDouble(strValue);
                }

                numericUpDown_ThFAM.Value = new decimal(listWellInfo.ThFAM);
                numericUpDown_ThHEX.Value = new decimal(listWellInfo.ThHEX);
                numericUpDown_ThROX.Value = new decimal(listWellInfo.ThROX);
                numericUpDown_ThCY5.Value = new decimal(listWellInfo.ThCY5);

                // 각 웰의 Ct 값을 읽어온다. 
                for (int welIndex = 1; welIndex < 26; welIndex++)
                {
                    Well_Info wellInfo = listWellInfo.GetWellInfo(welIndex);
                    if (worksheet.Cells[2, welIndex].Value != null)    // FAM Ct 값
                    {
                        strValue = worksheet.Cells[2, welIndex].Value.ToString();
                        wellInfo.CtFAM = Convert.ToDouble(strValue);
                    }
                    if (worksheet.Cells[3, welIndex].Value != null)    // HEX Ct 값
                    {
                        strValue = worksheet.Cells[3, welIndex].Value.ToString();
                        wellInfo.CtHEX = Convert.ToDouble(strValue);
                    }
                    if (worksheet.Cells[4, welIndex].Value != null)    // ROX Ct 값
                    {
                        strValue = worksheet.Cells[4, welIndex].Value.ToString();
                        wellInfo.CtROX = Convert.ToDouble(strValue);
                    }
                    if (worksheet.Cells[5, welIndex].Value != null)    // CY5 Ct 값
                    {
                        strValue = worksheet.Cells[5, welIndex].Value.ToString();
                        wellInfo.CtCY5 = Convert.ToDouble(strValue);
                    }
                    listWellInfo.listPlateInfos[welIndex - 1] = wellInfo;
                }
            }
        }

        /// <summary>
        /// 결과파일의 CtValue 탭에 데이터를 저장한다.  
        /// </summary>
        /// <param name="resultPath">결과파일 이름</param>
        public void SaveExcelCtValue(string resultPath)
        {
            if (Global.selectedResult == null)
                return;

            FileInfo fileInfo = new FileInfo(resultPath);
            using (var package = new ExcelPackage(fileInfo))
            {
                if (package.Workbook.Worksheets.Count <= 0)
                    return;

                // Result 탭이 있는지 찾고 없으면 더이상 진행하지 않는다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "CtValue")
                    {
                        resultIndex = i;
                        break;
                    }
                }

                if (resultIndex >= 0)
                    package.Workbook.Worksheets.Delete(worksheet);

                worksheet = package.Workbook.Worksheets.Add("CtValue");

                // Threshold 값을 저장한다. 
                worksheet.Cells[1, 1].Value = listWellInfo.ThFAM.ToString();
                worksheet.Cells[1, 2].Value = listWellInfo.ThHEX.ToString();
                worksheet.Cells[1, 3].Value = listWellInfo.ThROX.ToString();
                worksheet.Cells[1, 4].Value = listWellInfo.ThCY5.ToString();

                // 각 웰의 Ct 값을 저장한다. 
                for (int welIndex = 0; welIndex < 25; welIndex++)
                {
                    Well_Info wellInfo = listWellInfo.GetWellInfo(welIndex);
                    worksheet.Cells[2, welIndex + 1].Value = wellInfo.CtFAM.ToString();
                    worksheet.Cells[3, welIndex + 1].Value = wellInfo.CtHEX.ToString();
                    worksheet.Cells[4, welIndex + 1].Value = wellInfo.CtROX.ToString();
                    worksheet.Cells[5, welIndex + 1].Value = wellInfo.CtCY5.ToString();
                }
            }
        }

        /// <summary>
        /// 사용자 정의 Threshold 적용 함수 
        /// Apply 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnThApply_Click(object sender, EventArgs e)
        {
            listWellInfo.ThFAM = (double)numericUpDown_ThFAM.Value;
            listWellInfo.ThHEX = (double)numericUpDown_ThHEX.Value;
            listWellInfo.ThROX = (double)numericUpDown_ThROX.Value;
            listWellInfo.ThCY5 = (double)numericUpDown_ThCY5.Value;

            // Ct 값을 계산하고 그래프에 추가한다. 
            curveCalibration();
            flatComboBox_GraphType.SelectedIndex = 2;
            flatComboBox_GraphType_SelectedIndexChanged(this, null);

            Global.SavedSetting();
        }

        /// <summary>
        /// 자동으로 Threshold를 계산하는 함수 
        /// Auto Threshold 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnThAuto_Click(object sender, EventArgs e)
        {
            listWellInfo.ThFAM = 0.0;
            listWellInfo.ThHEX = 0.0;
            listWellInfo.ThROX = 0.0;
            listWellInfo.ThCY5 = 0.0;

            // Ct 값을 계산하고 그래프에 추가한다. 
            curveCalibration();

            flatComboBox_GraphType.SelectedIndex = 2;
            flatComboBox_GraphType_SelectedIndexChanged(this, null);

            Global.SavedSetting();
        }

        /// <summary>
        /// 결과파일에서 결과 데이터를 읽고 그래프를 표시한다.   
        /// </summary>
        /// <param name="resultPath">결과파일 이름</param>
        public void LoadResult(string resultPath)
        {
            FileInfo fileInfo = new FileInfo(resultPath);
            using (var package = new ExcelPackage(fileInfo))
            {
                if (package.Workbook.Worksheets.Count <= 0)
                    return;

                //get the first worksheet in the workbook
                //ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                // Result 탭이 있는지 찾고 없으면 더이상 진행하지 않는다. 
                ExcelWorksheet worksheet = null;
                int resultIndex = -1;
                for (int i = 1; i <= package.Workbook.Worksheets.Count; i++)
                {
                    worksheet = package.Workbook.Worksheets[i];
                    if (worksheet.Name == "Result")
                    {
                        resultIndex = i;
                        break;
                    }
                }

                if (resultIndex == -1)
                {
                    MessageBox.Show(new Form { TopMost = true }, "Not found Result !");
                    return;
                }

                Global.ResultFile = Path.GetFileNameWithoutExtension(resultPath) + ".xlsx";
                Global.ResultPath = resultPath.Replace(Global.ResultFile, "");

                textBox_resultFilePath.Text = Global.ResultFile;

                int colMax = 25 + 3;
                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int rowCount = worksheet.Dimension.End.Row;     //get row count
                if (colCount > colMax)
                    colCount = colMax;

                int totalCycle = Convert.ToInt32(worksheet.Cells[rowCount, 1].Value.ToString());
                InitRunGraph(0, totalCycle + 1, 0, 260);
                InitResultGraph(0, totalCycle + 1, 0, 260);

                for (int i = 0; i < 25; i++)
                {
                    if (listValuesFAM[i] == null)
                        listValuesFAM[i] = new List<double>();
                    listValuesFAM[i].Clear();

                    if (listValuesHEX[i] == null)
                        listValuesHEX[i] = new List<double>();
                    listValuesHEX[i].Clear();

                    if (listValuesROX[i] == null)
                        listValuesROX[i] = new List<double>();
                    listValuesROX[i].Clear();

                    if (listValuesCY5[i] == null)
                        listValuesCY5[i] = new List<double>();
                    listValuesCY5[i].Clear();
                }

                //We will start from row 2 since row 1 is a header row.
                for (int row = 4; row <= rowCount; row += 4)
                {
                    if (row + 4 > (rowCount + 1))
                        break;

                    // X좌표 : Cycle  
                    int xPos = Convert.ToInt32(worksheet.Cells[row, 1].Value.ToString());
                    int rIndex = 0;
                    for (int col = 4; col <= colCount; col++)
                    {
                        if (worksheet.Cells[row, col].Value == null)
                            continue;

                        double yPosFAM = Convert.ToDouble(worksheet.Cells[row, col].Value.ToString());
                        double yPosHEX = Convert.ToDouble(worksheet.Cells[row + 1, col].Value.ToString());
                        double yPosROX = Convert.ToDouble(worksheet.Cells[row + 2, col].Value.ToString());
                        double yPosCY5 = Convert.ToDouble(worksheet.Cells[row + 3, col].Value.ToString());

                        if (yPosROX > 0)
                            Debug.WriteLine("index({0}), FAM({1}), HEX({2}), ROX({3}), CY5({4})", rIndex, yPosFAM, yPosHEX, yPosROX, yPosCY5);

                        double gain = 1.0;
                        double offset = 0.0;
                        if (rIndex < Global.listRoiInfos.Count)
                        {
                            ROIShape roiInfo = Global.listRoiInfos[rIndex];
                            gain = roiInfo.ROI_Gain;
                            offset = roiInfo.ROI_Offset;

                            yPosFAM = (yPosFAM * gain) + offset;
                            yPosHEX = (yPosHEX * gain) + offset;
                            yPosROX = (yPosROX * gain) + offset;
                            yPosCY5 = (yPosCY5 * gain) + offset;
                        }

                        // 그래프 포인트를 저장한다. 
                        listValuesFAM[rIndex].Add(yPosFAM);
                        listValuesHEX[rIndex].Add(yPosHEX);
                        listValuesROX[rIndex].Add(yPosROX);
                        listValuesCY5[rIndex].Add(yPosCY5);

                        rIndex++;
                    }
                }

                //if (Global.IsInterpolation)
                InterpolationGraph((int)Global.baseAvgScale, Global.graphYscale);
                //else
                //    InterpolationGraph(1, Global.graphYscale);

                // Ct 값을 계산하고 그래프에 추가한다. 
                curveCalibration();

                //curveFittingRawData();
                flatComboBox_GraphType.SelectedIndex = 0;

                PanePulseWave.YAxis.Scale.MaxAuto = true;
                PanePulseWave.YAxis.Scale.MinAuto = true;
                PanePulseWave.YAxis.Scale.MinorStepAuto = true;
                PanePulseWave.YAxis.Scale.MajorStepAuto = true;

                this.ZedGraph_Pulse.AxisChange();
                //this.ZedGraph_Pulse.Invalidate();
                this.ZedGraph_Pulse.Refresh();

                PaneResultWave.YAxis.Scale.MaxAuto = true;
                PaneResultWave.YAxis.Scale.MinAuto = true;
                PaneResultWave.YAxis.Scale.MinorStepAuto = true;
                PaneResultWave.YAxis.Scale.MajorStepAuto = true;

                //PanePulseWave.XAxis.Scale.Min = 0;
                //PanePulseWave.XAxis.Scale.Max = maxTime + 60;
                this.zedGraph_Result.AxisChange();
                this.zedGraph_Result.Refresh();
                //this.zedGraph_Result.Invalidate();
            }
        }

        /// <summary>
        /// 이미지를 바이트배열 변환하는 함수 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray(Bitmap image) 
        {
            BitmapData bmpdata = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            int numbytes = bmpdata.Stride * bmpdata.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(ptr, bytedata, 0, numbytes);
            image.UnlockBits(bmpdata);

            return bytedata;
        }

        /// <summary>
        /// 32비트 이미지를 24비트 이미지로 변환하는 함수 
        /// </summary>
        /// <param name="img">32비트 이미지</param>
        /// <returns></returns>
        public static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        /// <summary>
        /// Camera를 Scan/Stop 한다.
        /// </summary>
        private void Running_StartStop()
        {
            this.btnStartRun.Enabled = false;   // 중복 조작 방지
            if (this.IsCameraScan)                    // Camera Scan 중이면 종료한다.
            {
                CloseCurrentVideoSource();

                this.IsCameraScan = false;
                this.btnStartRun.Text = "START RUN";

                Global.PCR_Manager.PCR_Stop();
            }
            else                                    // Camera Stop 중이면 Scan한다.
            {
                // Camera Control 클래스 생성
                if (CamStart())        // Camera 영상캡처 시작
                {
                    this.IsCameraScan = true;
                    this.btnStartRun.Text = "STOP RUN";
                }
            }

            this.btnStartRun.Enabled = true;   // 중복 조작 방지
        }

        /// <summary>
        /// Run 그래프를 생성 하는 함수
        /// </summary>
        /// <param name="zgc"></param>
        private void CreateGraph(ZedGraph.ZedGraphControl zgc)
        {
            PanePulseWave = zgc.GraphPane;

            // Fill the background of the chart rect and pane
            PanePulseWave.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);
            PanePulseWave.Fill = new Fill(Color.White, Color.SlateGray, 45.0f);

            PanePulseWave.Title.Text = "Result";
            PanePulseWave.XAxis.Title.Text = "";
            PanePulseWave.XAxis.Scale.Min = 0;
            PanePulseWave.XAxis.Scale.Max = 1000;

            PanePulseWave.YAxis.Title.Text = "";
            PanePulseWave.YAxis.Scale.Min = 0;
            PanePulseWave.YAxis.Scale.Max = 260;

            PanePulseWave.XAxis.Scale.MinorStep = 1;        //x축 작은칸 설정
            PanePulseWave.XAxis.Scale.MajorStep = 5;       //x축 큰칸 설정
            PanePulseWave.YAxis.Scale.MinorStep = 5;        //y축 작은칸 설정
            PanePulseWave.YAxis.Scale.MajorStep = 30;       //y축 큰칸 설정

            // 큰 칸
            PanePulseWave.XAxis.MajorGrid.DashOff = 1; // 선없는 부분 DashOn과 비율적으로 나타납니다. 
            PanePulseWave.XAxis.MajorGrid.DashOn = 3; // 선있는 부분 DashOff와 비율적으로 나타납니다.
            // 작은 칸
            PanePulseWave.XAxis.MinorGrid.DashOff = 1;
            PanePulseWave.XAxis.MinorGrid.DashOn = 1;

            PanePulseWave.XAxis.MajorGrid.IsVisible = true;
            PanePulseWave.YAxis.MajorGrid.IsVisible = true;
            PanePulseWave.XAxis.MinorGrid.IsVisible = true;
            PanePulseWave.YAxis.MinorGrid.IsVisible = true;

            int i = 0;
            Color lineColor;
            for (i = 0; i < 25; i++)
            {
                if (i < 5) lineColor = Global.colorList[16];
                else if (i < 10) lineColor = Global.colorList[17];
                else if (i < 15) lineColor = Global.colorList[18];
                else if (i < 20) lineColor = Global.colorList[19];
                else lineColor = Global.colorList[20];

                string label = null;
                SymbolType sType = SymbolType.None;            

                listPointsFAM[i] = new ZedGraph.PointPairList();
                listPointsHEX[i] = new ZedGraph.PointPairList();
                listPointsROX[i] = new ZedGraph.PointPairList();
                listPointsCY5[i] = new ZedGraph.PointPairList();

                // Run Graph
                CurveFAM[i] = PanePulseWave.AddCurve(label, listPointsFAM[i], lineColor, sType);
                CurveFAM[i].Line.Width = 3;
                CurveFAM[i].Line.IsAntiAlias = true;
                CurveFAM[i].Line.IsSmooth = true;
                CurveFAM[i].Line.SmoothTension = 0.7F;

                CurveHEX[i] = PanePulseWave.AddCurve(label, listPointsHEX[i], lineColor, sType);
                CurveHEX[i].Line.Width = 3;
                CurveHEX[i].Line.IsAntiAlias = true;
                CurveHEX[i].Line.IsSmooth = true;
                CurveHEX[i].Line.SmoothTension = 0.7F;

                CurveROX[i] = PanePulseWave.AddCurve(label, listPointsROX[i], lineColor, sType);
                CurveROX[i].Line.Width = 3;
                CurveROX[i].Line.IsAntiAlias = true;
                CurveROX[i].Line.IsSmooth = true;
                CurveROX[i].Line.SmoothTension = 0.7F;

                CurveCY5[i] = PanePulseWave.AddCurve(label, listPointsCY5[i], lineColor, sType);
                CurveCY5[i].Line.Width = 3;
                CurveCY5[i].Line.IsAntiAlias = true;
                CurveCY5[i].Line.IsSmooth = true;
                CurveCY5[i].Line.SmoothTension = 0.7F;
            }

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }

        /// <summary>
        /// Run 그래프 초기화 함수 
        /// </summary>
        /// <param name="scaleMinX">그래프 X 최소값</param>
        /// <param name="scaleMaxX">그래프 X 최대값</param>
        /// <param name="scaleMinY">그래프 Y 최소값</param>
        /// <param name="scaleMaxY">그래프 Y 최대값</param>
        public void InitRunGraph(int scaleMinX, int scaleMaxX, int scaleMinY, int scaleMaxY)
        {
            // Preview, Start 시 차트 초기화 한다.
            for (int i = 0; i < 25; i++)
            {
                if (listPointsFAM[i] != null) listPointsFAM[i].Clear();
                if (listPointsHEX[i] != null) listPointsHEX[i].Clear();
                if (listPointsROX[i] != null) listPointsROX[i].Clear();
                if (listPointsCY5[i] != null) listPointsCY5[i].Clear();
            }
            PanePulseWave.XAxis.Scale.Min = scaleMinX;
            PanePulseWave.XAxis.Scale.Max = scaleMaxX;
            PanePulseWave.YAxis.Scale.Min = scaleMinY;
            PanePulseWave.YAxis.Scale.Max = scaleMaxY;

            PanePulseWave.XAxis.Scale.MinorStep = 1;        //x축 작은칸 설정
            PanePulseWave.XAxis.Scale.MajorStep = 5;       //x축 큰칸 설정
            //PanePulseWave.YAxis.Scale.MajorStep = 30;       //y축 큰칸 설정
            //PanePulseWave.YAxis.Scale.MinorStep = 5;        //y축 작은칸 설정

            this.ZedGraph_Pulse.AxisChange();
            //this.ZedGraph_Pulse.Invalidate();
            this.zedGraph_Result.Refresh();

            PulseCount = 0;
        }

        /// <summary>
        /// Result 그래프를 생성 하는 함수
        /// </summary>
        /// <param name="zgc">ZedGraph Control</param>
        private void CreateResultGraph(ZedGraph.ZedGraphControl zgc)
        {
            PaneResultWave = zgc.GraphPane;

            // Fill the background of the chart rect and pane
            PaneResultWave.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);
            PaneResultWave.Fill = new Fill(Color.White, Color.SlateGray, 45.0f);

            PaneResultWave.Title.Text = "Result";
            PaneResultWave.XAxis.Title.Text = "";
            PaneResultWave.XAxis.Scale.Min = 0;
            PaneResultWave.XAxis.Scale.Max = 1000;

            PaneResultWave.YAxis.Title.Text = "";
            PaneResultWave.YAxis.Scale.Min = 0;
            PaneResultWave.YAxis.Scale.Max = 260;

            PaneResultWave.XAxis.Scale.MinorStep = 15;        //x축 작은칸 설정
            PaneResultWave.XAxis.Scale.MajorStep = 60;       //x축 큰칸 설정
            PaneResultWave.YAxis.Scale.MinorStep = 5;        //y축 작은칸 설정
            PaneResultWave.YAxis.Scale.MajorStep = 30;       //y축 큰칸 설정

            // 큰 칸
            PaneResultWave.XAxis.MajorGrid.DashOff = 1; // 선없는 부분 DashOn과 비율적으로 나타납니다. 
            PaneResultWave.XAxis.MajorGrid.DashOn = 3; // 선있는 부분 DashOff와 비율적으로 나타납니다.
            // 작은 칸
            PaneResultWave.XAxis.MinorGrid.DashOff = 1;
            PaneResultWave.XAxis.MinorGrid.DashOn = 1;

            PaneResultWave.XAxis.MajorGrid.IsVisible = true;
            PaneResultWave.YAxis.MajorGrid.IsVisible = true;
            PaneResultWave.XAxis.MinorGrid.IsVisible = true;
            PaneResultWave.YAxis.MinorGrid.IsVisible = true;

            int i = 0;
            Color lineColor;
            string label = null;
            SymbolType sType = SymbolType.None;
            for (i = 0; i < 25; i++)
            {
                if (i < 5) lineColor = Global.colorList[16];
                else if (i < 10) lineColor = Global.colorList[17];
                else if (i < 15) lineColor = Global.colorList[18];
                else if (i < 20) lineColor = Global.colorList[19];
                else lineColor = Global.colorList[20];

                listResultPointsFAM[i] = new ZedGraph.PointPairList();
                listResultPointsHEX[i] = new ZedGraph.PointPairList();
                listResultPointsROX[i] = new ZedGraph.PointPairList();
                listResultPointsCY5[i] = new ZedGraph.PointPairList();

                // Run Graph
                CurveResultFAM[i] = PaneResultWave.AddCurve(label, listResultPointsFAM[i], lineColor, sType);
                CurveResultFAM[i].Line.Width = 3;
                CurveResultFAM[i].Line.IsAntiAlias = true;

                CurveResultHEX[i] = PaneResultWave.AddCurve(label, listResultPointsHEX[i], lineColor, sType);
                CurveResultHEX[i].Line.Width = 3;
                CurveResultHEX[i].Line.IsAntiAlias = true;

                CurveResultROX[i] = PaneResultWave.AddCurve(label, listResultPointsROX[i], lineColor, sType);
                CurveResultROX[i].Line.Width = 3;
                CurveResultROX[i].Line.IsAntiAlias = true;

                CurveResultCY5[i] = PaneResultWave.AddCurve(label, listResultPointsCY5[i], lineColor, sType);
                CurveResultCY5[i].Line.Width = 3;
                CurveResultCY5[i].Line.IsAntiAlias = true;
            }

            // Ct Value Graph
            CurveThFAM = PaneResultWave.AddCurve(label, listThPointsFAM, colorBtn_thFAM.Color, sType);
            CurveThFAM.Line.Width = 3;
            CurveThFAM.Line.IsAntiAlias = true;

            CurveThHEX = PaneResultWave.AddCurve(label, listThPointsHEX, colorBtn_thHEX.Color, sType);
            CurveThHEX.Line.Width = 3;
            CurveThHEX.Line.IsAntiAlias = true;

            CurveThROX = PaneResultWave.AddCurve(label, listThPointsROX, colorBtn_thROX.Color, sType);
            CurveThROX.Line.Width = 3;
            CurveThROX.Line.IsAntiAlias = true;

            CurveThCY5 = PaneResultWave.AddCurve(label, listThPointsCY5, colorBtn_thCY5.Color, sType);
            CurveThCY5.Line.Width = 3;
            CurveThCY5.Line.IsAntiAlias = true;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }

        /// <summary>
        /// Result 그래프 초기화 함수 
        /// </summary>
        /// <param name="scaleMinX">그래프 X 최소값</param>
        /// <param name="scaleMaxX">그래프 X 최대값</param>
        /// <param name="scaleMinY">그래프 Y 최소값</param>
        /// <param name="scaleMaxY">그래프 Y 최대값</param>
        public void InitResultGraph(int scaleMinX, int scaleMaxX, int scaleMinY, int scaleMaxY)
        {
            // Preview, Start 시 차트 초기화 한다.
            for (int i = 0; i < 25; i++)
            {
                if (listResultPointsFAM[i] != null) listResultPointsFAM[i].Clear();
                if (listResultPointsHEX[i] != null) listResultPointsHEX[i].Clear();
                if (listResultPointsROX[i] != null) listResultPointsROX[i].Clear();
                if (listResultPointsCY5[i] != null) listResultPointsCY5[i].Clear();
            }
            if (listThPointsFAM != null) listThPointsFAM.Clear();
            if (listThPointsHEX != null) listThPointsHEX.Clear();
            if (listThPointsROX != null) listThPointsROX.Clear();
            if (listThPointsCY5 != null) listThPointsCY5.Clear();

            PaneResultWave.XAxis.Scale.Min = scaleMinX;
            PaneResultWave.XAxis.Scale.Max = scaleMaxX;
            PaneResultWave.YAxis.Scale.Min = scaleMinY;
            PaneResultWave.YAxis.Scale.Max = scaleMaxY;

            PaneResultWave.XAxis.Scale.MajorStep = 5;          //x축 큰칸 설정
            PaneResultWave.XAxis.Scale.MinorStep = 1;    //x축 작은칸 설정
            //PaneResultWave.YAxis.Scale.MinorStep = 5;        //y축 작은칸 설정
            //PaneResultWave.YAxis.Scale.MajorStep = 30;       //y축 큰칸 설정

            this.zedGraph_Result.AxisChange();
            //this.zedGraph_Result.Invalidate();
            this.zedGraph_Result.Refresh();

            PulseCount = 0;
        }

        /// <summary>
        /// Well 테이블의 정보를 읽어 그래프의 칼라와 표시 여부를 결정하는 함수 
        /// </summary>
        private void ShowResultGraph()
        {
            int curveCount = PaneResultWave.CurveList.Count;
            if (curveCount <= 0)
                return;

            //FAM, HEX, ROX, CY5 Color 
            Color[] wellColor = new Color[4];
            bool[] isColor = new bool[4];
            for (int welIndex=0; welIndex < 25; welIndex++)
            {
                for (int i = 0; i < 4; i++)
                    isColor[i] = false;

                //bool isSelect = listWellInfo.IsSelectWell(i);
                Well_Info wellInfo = listWellInfo.GetWellInfo(welIndex);
                // 웰에 저장된 칼라 정보를 가져온다. 
                int targetCount = wellInfo.listTargetInfos.Count;
                for (int tIndex = 0; tIndex < targetCount; tIndex++)
                {
                    int targetIndex = -1;
                    for (int i = 0; i < Global.listTargetInfos.Count; i++)
                    {
                        if (wellInfo.listTargetInfos[tIndex] == Global.listTargetInfos[i].name)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                    //int targetIndex = wellInfo.listTargetInfos[tIndex];

                    if (targetIndex >= 0)
                    {
                        IpPlate_Target targetInfo = Global.listTargetInfos[targetIndex];
                        wellColor[targetInfo.reporter] = Global.colorList[targetInfo.colorIndex];
                        isColor[targetInfo.reporter] = true;
                    }
                }

                // 웰이 선택되어 있고 타겟 칼라 정보가 있으면 라인 그래프를 그린다. 
                bool isSelect = wellInfo.isSelected;
                for (int fIndex = 0; fIndex < 4; fIndex++)
                {
                    int index = fIndex + (welIndex * 4);
                    if (isSelect && isColor[fIndex])
                    {
                        PaneResultWave.CurveList[index].IsVisible = true;
                        PaneResultWave.CurveList[index].Color = wellColor[fIndex];

                        PanePulseWave.CurveList[index].IsVisible = true;
                        PanePulseWave.CurveList[index].Color = wellColor[fIndex];
                    }
                    else
                    {
                        PaneResultWave.CurveList[index].IsVisible = false;

                        PanePulseWave.CurveList[index].IsVisible = false;
                    }
                }
            }

            // Baseline 과 Threshold 선택시에만 Threshold값 그래프를 표시한다. 
            Color[] thColor = new Color[4];
            thColor[0] = Global.colorList[Global.thFAMColorIndex];
            thColor[1] = Global.colorList[Global.thHEXColorIndex];
            thColor[2] = Global.colorList[Global.thROXColorIndex];
            thColor[3] = Global.colorList[Global.thCY5ColorIndex];
            for (int i = 100; i < 104; i++)
            {
                //if (flatComboBox_GraphType.SelectedIndex == 1 || flatComboBox_GraphType.SelectedIndex == 2)
                if (flatComboBox_GraphType.SelectedIndex == 2)
                {
                    PaneResultWave.CurveList[i].IsVisible = true;
                    PaneResultWave.CurveList[i].Color = thColor[i - 100];
                }
                else
                    PaneResultWave.CurveList[i].IsVisible = false;
            }

            this.zedGraph_Result.AxisChange();
            //this.zedGraph_Result.Invalidate();
            this.zedGraph_Result.Refresh();

            this.ZedGraph_Pulse.AxisChange();
            this.ZedGraph_Pulse.Refresh();
        }

        /// <summary>
        /// 그래프에서 오른쪽 마우스를 눌렀을때 메뉴를 표시하는 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="menuStrip"></param>
        /// <param name="mousePt"></param>
        /// <param name="objState"></param>
        private void MyContextMemuBuilder(ZedGraph.ZedGraphControl sender, ContextMenuStrip menuStrip, System.Drawing.Point mousePt, ZedGraph.ZedGraphControl.ContextMenuObjectState objState)
        {
            // 안쓰는 메뉴 지우기
            menuStrip.Items.RemoveByKey("print");
            menuStrip.Items.RemoveByKey("page_setup");

            // Raw 값 저장 Memu 생성
            if (listResultPointsFAM[0].Count > 0)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Name = "saved_rawdata";
                item.Tag = "saved_rawdata";

                item.Text = "Save RawData As..";
                item.Click += new System.EventHandler(SavedRawDataMenu);

                menuStrip.Items.Add(item);
            }
        }

        /// <summary>
        /// 그래프에서 Save Raw Data 메뉴 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SavedRawDataMenu(object sender, EventArgs e)
        {
            if (listResultPointsFAM[0].Count <= 0) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Enter a file name to save.",
                OverwritePrompt = true,
                Filter = "Excel 파일 (.xlsx, .xls) | *.xlsx; *.xls"
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            this.strSaveExcelFile = saveFileDialog.FileName;
            this.ShowMessageBox = true;

            // 비동기(Async)로 실행 
            this.ExcelSavedworker.RunWorkerAsync();
            this.Cursor = Cursors.WaitCursor;
        }

        /// <summary>
        /// 그래프 데이터를 엑셀로 저장하는 함수 
        /// </summary>
        /// <param name="savedFileName">엑셀파일 이름</param>
        private void SavedRawData(string savedFileName)
        {
            if (savedFileName.Length <= 0 || listResultPointsFAM[0].Count <= 0) return;

            //if (this.RadioButton_PTG.Checked) strSenser = "PTG";
            using (ExcelPackage package = new ExcelPackage())
            {
                //ExcelWorksheet worksheet = package.Workbook.Worksheets.Add($"{strSenser}_PulseWave_{PulseCount / 500}s");
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add($"Result");

                //Add the headers
                worksheet.Cells[1, 1].Value = "No.";
                worksheet.Cells[1, 2].Value = "Filter";
                worksheet.Cells[1, 3].Value = "Time";
                worksheet.Cells[2, 2].Value = "Gain";
                worksheet.Cells[3, 2].Value = "Offset";
                int i = 0;
                for (i = 0; i < Global.listRoiInfos.Count; i++)
                {
                    ROIShape roi = Global.listRoiInfos[i];

                    int wellNo = i + 1;
                    worksheet.Cells[1, i + 4].Value = wellNo.ToString();
                    double roiGain = roi.ROI_Gain;
                    double roiOffset = roi.ROI_Offset;
                    worksheet.Cells[2, i + 4].Value = roiGain.ToString();
                    worksheet.Cells[3, i + 4].Value = roiOffset.ToString();
                }

                int startRow = 4;
                int row = 0;
                int col = 0;

                int listCount = listResultPointsFAM[col].Count;
                for (row = 0; row < listCount; row++)
                {
                    for (int filterNo = 0; filterNo < 4; filterNo++)
                    {
                        int rowIndex = startRow + filterNo + (row * 4);
                        worksheet.Cells[rowIndex, 1].Value = row;       // Cycle No.
                        worksheet.Cells[rowIndex, 2].Value = filterNo;  // Filter No
                        worksheet.Cells[rowIndex, 3].Value = row;       // Time 은 Cycle 번호를 저장한다. 
                    }
                }

                for (col = 0; col < 25; col++)
                {
                    listCount = listResultPointsFAM[col].Count;
                    //foreach (ZedGraph.PointPair pp in listPointsFAM[i])
                    for (row = 0; row < listCount; row++)
                    {
                        ZedGraph.PointPair listFAM = listResultPointsFAM[col][row];

                        // x(time), y(value) 2칸씩 이동하며 저장한다. 
                        int rowIndex = startRow + (row * 4);
                        //worksheet.Cells[rowIndex, 1].Value = row;       // Cycle No.
                        //worksheet.Cells[rowIndex, 2].Value = 0;         // Filter No
                        //worksheet.Cells[rowIndex + 1, 2].Value = 0;     // Filter No
                        //worksheet.Cells[rowIndex, 3].Value = row;       // Time 은 Cycle 번호를 저장한다. 

                        worksheet.Cells[rowIndex, col + 4].Value = listFAM.Y;   // Y 좌표 
                        
                        ZedGraph.PointPair listHEX = listResultPointsHEX[col][row];
                        // y(value) 1칸씩 이동하며 저장한다. 
                        rowIndex = startRow + 1 + (row * 4);
                        //worksheet.Cells[rowIndex, 1].Value = row;
                        //worksheet.Cells[rowIndex, 2].Value = 1;
                        //worksheet.Cells[rowIndex, 3].Value = row;       // Time 은 Cycle 번호를 저장한다. 

                        //worksheet.Cells[rowIndex, col + 3].Value = listHEX.X;
                        worksheet.Cells[rowIndex, col + 4].Value = listHEX.Y;

                        ZedGraph.PointPair listROX = listResultPointsROX[col][row];
                        // y(value) 1칸씩 이동하며 저장한다. 
                        rowIndex = startRow + 2 + (row * 4);
                        //worksheet.Cells[rowIndex, 1].Value = row;
                        //worksheet.Cells[rowIndex, 2].Value = 2;
                        //worksheet.Cells[rowIndex, 3].Value = row;       // Time 은 Cycle 번호를 저장한다. 

                        //worksheet.Cells[rowIndex, col + 3].Value = listROX.X;
                        worksheet.Cells[rowIndex, col + 4].Value = listROX.Y;

                        ZedGraph.PointPair listCY5 = listResultPointsCY5[col][row];
                        // y(value) 1칸씩 이동하며 저장한다. 
                        rowIndex = startRow + 3 + (row * 4);
                        //worksheet.Cells[rowIndex, 1].Value = row;
                        //worksheet.Cells[rowIndex, 2].Value = 3;
                        //worksheet.Cells[rowIndex, 3].Value = row;       // Time 은 Cycle 번호를 저장한다. 

                        //worksheet.Cells[rowIndex, col + 3].Value = listCY5.X;
                        worksheet.Cells[rowIndex, col + 4].Value = listCY5.Y;
                    }

                    //startRow += 4;
                }

                var xlFile = new FileInfo(savedFileName);
                if (xlFile.Exists) xlFile.Delete();  // ensures we create a new workbook
                                                     // save our new workbook in the output directory and we are done!
                package.SaveAs(xlFile);
            }
        }

        /// <summary>
        /// 비트맵 구하기
        /// </summary>
        /// <param name="image">이미지</param>
        /// <param name="translucency">반투명도</param>
        /// <returns>비트맵</returns>
        private Bitmap GetTransBitmap(Image image, float translucency)
        {
            ColorMatrix colorMatrix = new ColorMatrix
            (
                new float[][]
                {
                    new float[] {1, 0, 0, 0           , 0},
                    new float[] {0, 1, 0, 0           , 0},
                    new float[] {0, 0, 1, 0           , 0},
                    new float[] {0, 0, 0, translucency, 0},
                    new float[] {0, 0, 0, 0           , 1},
                }
            );

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);

            System.Drawing.Point[] pointArray =
            {
                new System.Drawing.Point(0, 0),
                new System.Drawing.Point(image.Width, 0),
                new System.Drawing.Point(0, image.Height),
            };

            Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);
            Bitmap bitmap = new Bitmap(image.Width, image.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(image, pointArray, rectangle, GraphicsUnit.Pixel, imageAttributes);
            }

            return bitmap;
        }

        #region Target 리스트 처리 
        /// <summary>
        /// Target 추가 메뉴 실행 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomBtn_AddTarget_Click(object sender, EventArgs e)
        {
            dataGridView_Targets.Rows.Add();
            int rowCount = dataGridView_Targets.Rows.Count;
            dataGridView_Targets.Rows[rowCount - 1].Cells[0].Value = false;
            DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_Targets.Rows[rowCount - 1].Cells[1];
            int colorIndex = rowCount % 40;
            colorBtn.ColorIndex = colorIndex;
            //string name = "Target" + rowCount.ToString();
            // 동일한 이름이 있는지 확인하고 없으면 추가되는 이름으로 사용한다. 
            int i, j;
            string name = "";
            for (i = 0; i < 25; i++)
            {
                name = "Target" + (i + 1).ToString();
                bool isSameName = false;
                for (j = 0; j < rowCount; j++)
                {
                    if (name == (string)dataGridView_Targets.Rows[j].Cells[2].Value)
                    {
                        isSameName = true;
                        break;
                    }
                }

                if (!isSameName)
                    break;
            }

            dataGridView_Targets.Rows[rowCount - 1].Cells[2].Value = name;
            dataGridView_Targets.Rows[rowCount - 1].Cells[3].Value = "FAM";
            //DataGridViewComboBoxCell comboCell = (DataGridViewComboBoxCell)dataGridView_Targets.Rows[rowCount - 1].Cells[3];
            //comboCell.RowIndex = 0;
            dataGridView_Targets.Rows[rowCount - 1].Cells[4].Value = "NFQ-MGB";

            // Plate 리스트에 디폴트값을 추가한다. 
            IpPlate_Target plateInfo = new IpPlate_Target();
            plateInfo.check = false;
            plateInfo.colorIndex = colorIndex;
            plateInfo.name = name;
            plateInfo.reporter = 3;
            plateInfo.quencher = 1;

            Global.listTargetInfos.Add(plateInfo);
        }

        /// <summary>
        /// Target 초기화 함수 
        /// </summary>
        public void PlateInit_Targets()
        {
            // 전체적으로 폰트 적용하기
            //this.dataGridView_Targets.Font = new Font("Tahoma", 10, FontStyle.Regular);

            // Colum 의 해더부분을 지정하기
            this.dataGridView_Targets.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold);

            // Row 해더부분을 지정하기
            this.dataGridView_Targets.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 14, FontStyle.Regular);
            this.dataGridView_Targets.RowTemplate.Height = 30;

            // 체크박스 추가
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.Name = "chk_Select";
            dataGridView_Targets.Columns.Add(checkBoxColumn);

            // 버튼 추가
            DataGridViewButtonColumn btnColorColumn = new DataGridViewButtonColumn();
            btnColorColumn.CellTemplate = buttonTargetCell;
            // Color 선택 
            btnColorColumn.HeaderText = "";
            btnColorColumn.Name = "btn_Color";
            dataGridView_Targets.Columns.Add(btnColorColumn);

            // 텍스트박스 추가
            DataGridViewTextBoxColumn textNameColumn = new DataGridViewTextBoxColumn();
            // 이름 입력 
            textNameColumn.HeaderText = "Name";
            textNameColumn.Name = "text_Name";
            dataGridView_Targets.Columns.Add(textNameColumn);

            // 콤보박스 추가
            DataGridViewComboBoxColumn cmbReportColumn = new DataGridViewComboBoxColumn();
            // Reporter 선택 
            cmbReportColumn.HeaderText = "Reporter";
            cmbReportColumn.Name = "combo_Reporter";
            cmbReportColumn.Items.AddRange(Target_Reporters);
            dataGridView_Targets.Columns.Add(cmbReportColumn);

            // Quencher 선택 
            DataGridViewComboBoxColumn cmbQuencherColumn = new DataGridViewComboBoxColumn();
            cmbQuencherColumn.HeaderText = "Quencher";
            cmbQuencherColumn.Name = "combo_Quencher";
            cmbQuencherColumn.Items.AddRange(Target_Quenchers);
            dataGridView_Targets.Columns.Add(cmbQuencherColumn);

            // Comment 입력
            DataGridViewTextBoxColumn textCommmentColumn = new DataGridViewTextBoxColumn();
            textCommmentColumn.HeaderText = "Commment";
            textCommmentColumn.Name = "text_Comment";
            dataGridView_Targets.Columns.Add(textCommmentColumn);

            // Task 선택 
            DataGridViewComboBoxColumn cmbTaskColumn = new DataGridViewComboBoxColumn();
            cmbTaskColumn.HeaderText = "Task";
            cmbTaskColumn.Name = "combo_Task";
            cmbTaskColumn.Items.Add("U");
            cmbTaskColumn.Items.Add("S");
            cmbTaskColumn.Items.Add("N");

            //cmbTaskColumn.Items.AddRange(Tasks);
            dataGridView_Targets.Columns.Add(cmbTaskColumn);
            dataGridView_Targets.EditingControlShowing +=
                new DataGridViewEditingControlShowingEventHandler(
                dataGridView_Targets_EditingControlShowing);

            // Quantity 입력 : Task가 S 일 경우에만 활성화 됨. 
            DataGridViewTextBoxColumn textQuantityColumn = new DataGridViewTextBoxColumn();
            textQuantityColumn.HeaderText = "Quantity";
            textQuantityColumn.Name = "text_Quantity";
            dataGridView_Targets.Columns.Add(textQuantityColumn);

            // 아이템 삭제 
            DataGridViewImageColumn imageRemoveColumn = new DataGridViewImageColumn();
            imageRemoveColumn.HeaderText = "";
            imageRemoveColumn.Name = "image_Remove";
            imageRemoveColumn.Image = Resources.List_Remove;
            dataGridView_Targets.Columns.Add(imageRemoveColumn);

            dataGridView_Targets.Columns[0].Width = 50;
            dataGridView_Targets.Columns[1].Width = 50;
            dataGridView_Targets.Columns[2].Width = 120;
            dataGridView_Targets.Columns[3].Width = 120;
            dataGridView_Targets.Columns[4].Width = 120;
            dataGridView_Targets.Columns[5].Width = 120;
            dataGridView_Targets.Columns[6].Width = 50;
            dataGridView_Targets.Columns[7].Width = 120;
            dataGridView_Targets.Columns[8].Width = 50;

            Global.listTargetInfos.Clear();

            // 초기화할 때 타겟 하나를 추가한다. 
            CustomBtn_AddTarget_Click(this, null);
        }

        /// <summary>
        /// Target 체크박스 업데이트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Targets_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridView_Targets.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>
        /// Target의 Combo Box와 Text Box가 변경되었을때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Targets_EditingControlShowing(object sender,
            DataGridViewEditingControlShowingEventArgs e)
        {
            // ComboBox 가 눌렸을때 이벤트 생성 
            ComboBox combo = e.Control as ComboBox;
            if (combo != null)
            {
                ComboBox cb = (ComboBox)e.Control;
                cb.DrawMode = DrawMode.OwnerDrawFixed;
                cb.SelectedIndex = 0;
                cb.DrawItem -= combobox_DrawItem;
                cb.DrawItem += combobox_DrawItem;

                // Remove an existing event-handler, if present, to avoid 
                // adding multiple handlers when the editing control is reused.
                combo.SelectedIndexChanged -=
                    new EventHandler(ComboBox_SelectedIndexChanged);

                // Add the event handler. 
                combo.SelectedIndexChanged +=
                    new EventHandler(ComboBox_SelectedIndexChanged);
            }

            // TextBox 가 눌렸을때 이벤트 생성 
            TextBox textBox = e.Control as TextBox;
            if (textBox != null)
            {
                textBox.TextChanged -=
                    new EventHandler(TextBox_SelectedIndexChanged);

                textBox.TextChanged +=
                    new EventHandler(TextBox_SelectedIndexChanged);
            }
        }

        /// <summary>
        /// TextBox가 변경되었을때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = ((TextBox)sender).Text;
            // 이름일 경우 리스트에 같은 이름이 있는지 이름을 변경하지 않는다.  
            if (plateTargetColIndex == 2)
            {
                bool bFind = false;
                IpPlate_Target plateInfo;
                for (int i = 0; i < Global.listTargetInfos.Count; i++)
                {
                    if (i == plateTargetRowIndex)
                        continue;

                    plateInfo = Global.listTargetInfos[i];
                    if (text == plateInfo.name)
                    {
                        bFind = true;
                        break;
                    }
                }

                plateInfo = Global.listTargetInfos[plateTargetRowIndex];
                if (bFind)
                {
                    string msg = String.Format("Target name is not unique : {0}", text);
                    MessageBox.Show(new Form { TopMost = true }, msg);
                    ((TextBox)sender).Text = plateInfo.name;
                }
                else
                {
                    plateInfo.name = text;
                    Global.listTargetInfos[plateTargetRowIndex] = plateInfo;
                    //this.UpdateWellTable_Target(plateInfo, plateTargetRowIndex);
                    this.UpdateWellTable(plateInfo.name, "", "");
                }
            }
        }

        /// <summary>
        /// ComboBox가 변경되었을때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int comboIndex = ((ComboBox)sender).SelectedIndex;
            IpPlate_Target plateInfo = Global.listTargetInfos[plateTargetRowIndex];
            if (plateTargetColIndex == 3)           // Reporter 선택이 변경되었을때.. 
            {
                Rectangle rDraw;
                this.Cursor = Cursors.WaitCursor;

                // 현재 Target이 선택되어 있고 같은 이름의 Reporter가 선택되어 있으면 Reporter를 변경하지 않는다. 
                if (plateInfo.check)
                {
                    bool bFind = false;
                    int findedIndex = 0;
                    IpPlate_Target pInfo;
                    for (int i = 0; i < Global.listTargetInfos.Count; i++)
                    {
                        if (i == plateTargetRowIndex)
                            continue;

                        pInfo = Global.listTargetInfos[i];
                        if (pInfo.check && plateInfo.reporter == pInfo.reporter)
                        {
                            bFind = true;
                            findedIndex = i;
                            break;
                        }
                    }

                    if (bFind)
                    {
                        string msg = String.Format("Dye '{0}' already used in the well.", this.Target_Reporters[plateInfo.reporter]);
                        MessageBox.Show(new Form { TopMost = true }, msg);

                        bReporterFind = true;

                        dataGridView_Targets.Rows[plateTargetRowIndex].Cells[0].Value = false;
                        plateInfo.check = false;
                        Global.listTargetInfos[plateTargetRowIndex] = plateInfo;
                        this.UpdateWellTable(plateInfo.name, "", "");
                        rDraw = dataGridView_Targets.GetCellDisplayRectangle(3, plateTargetRowIndex, true);
                        dataGridView_Targets.Invalidate(rDraw);

                        this.Cursor = Cursors.Default;
                        return;
                    }
                }

                plateInfo.reporter = comboIndex;
                if (comboIndex == 0) plateInfo.colorIndex = 13;     // FAM
                else if (comboIndex == 1) plateInfo.colorIndex = 26;     // HEX
                else if (comboIndex == 2) plateInfo.colorIndex = 27;     // ROX
                else if (comboIndex == 3) plateInfo.colorIndex = 16;     // CY5
                Global.listTargetInfos[plateTargetRowIndex] = plateInfo;

                TargetColorChanged(0, plateInfo.colorIndex, Global.colorList[plateInfo.colorIndex]);

                rDraw = dataGridView_Targets.GetCellDisplayRectangle(1, plateTargetRowIndex, true);
                dataGridView_Targets.Invalidate(rDraw);
                Thread.Sleep(500);
                this.Cursor = Cursors.Default;
            }
            else if (plateTargetColIndex == 4)      // quencher 선택이 변경되었을때.. 
            {
                plateInfo.quencher = comboIndex;
                Global.listTargetInfos[plateTargetRowIndex] = plateInfo;
            }
            else if (plateTargetColIndex == 6)      // task 선택이 변경되었을때.. 
            {
                plateInfo.task = comboIndex;
                Global.listTargetInfos[plateTargetRowIndex] = plateInfo;
            }
        }

        /// <summary>
        /// Target 리스트 클릭 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Targets_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            plateTargetColIndex = e.ColumnIndex;
            plateTargetRowIndex = e.RowIndex;

            this.Cursor = Cursors.WaitCursor;
            dataGridView_Targets.BeginEdit(true);

            IpPlate_Target plateInfo = Global.listTargetInfos[plateTargetRowIndex];

            // Check Box 상태정보 저장 
            if (e.ColumnIndex == 0)
            {
                if (Convert.ToBoolean(dataGridView_Targets.Rows[plateTargetRowIndex].Cells[0].EditedFormattedValue) == true)
                {
                    // 같은 이름의 Reporter가 선택되어 있으면 체크가 안되도록 한다. 
                    bool bFind = false;
                    int findedIndex = 0;
                    IpPlate_Target pInfo;
                    for (int i = 0; i < Global.listTargetInfos.Count; i++)
                    {
                        if (i == plateTargetRowIndex)
                            continue;

                        pInfo = Global.listTargetInfos[i];
                        if (pInfo.check && plateInfo.reporter == pInfo.reporter)
                        {
                            bFind = true;
                            findedIndex = i;
                            break;
                        }
                    }

                    if (bFind)
                    {
                        string msg = String.Format("Dye '{0}' already used in the well.", this.Target_Reporters[plateInfo.reporter]);
                        MessageBox.Show(new Form { TopMost = true }, msg);

                        bReporterFind = true;

                        dataGridView_Targets.Rows[plateTargetRowIndex].Cells[0].Value = false;
                        plateInfo.check = false;
                    }
                    else
                    {
                        plateInfo.check = true;
                    }
                }
                else
                {
                    plateInfo.check = false;
                }
                Global.listTargetInfos[plateTargetRowIndex] = plateInfo;
                this.UpdateWellTable(plateInfo.name, "", "");
                //this.UpdateWellTable_Target(plateInfo, plateTargetRowIndex);
            }
            // Color 선택 정보 저장 
            else if (e.ColumnIndex == 1)
            {
                System.Drawing.Point pt = dataGridView_Targets.PointToScreen(
                        dataGridView_Targets.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Location);
                buttonTargetCell.PlateType = 0;
                buttonTargetCell.ClickHandler(pt, this);
            }
            // 아이템 삭제  
            else if (e.ColumnIndex == 8)
            {
                //string deleteName = (string)dataGridView_Targets.Rows[plateTargetRowIndex].Cells[1].Value;
                // 삭제하기 전에 해당 아이템을 사용하는 셀이 있는지 확인하고 있으면 삭제한다. 
                if (this.listWellInfo.DeleteCheckTarget(plateInfo.name) == true)
                {
                    Global.listTargetInfos.RemoveAt(plateTargetRowIndex);
                    this.listWellInfo.DeleteAllRowIndex(0, plateInfo.name);
                    dataGridView_Targets.Rows.Remove(dataGridView_Targets.Rows[plateTargetRowIndex]);
                    plateTargetRowIndex = -1;

                    UpdateWellTable("", "", "");
                }
            }

            Thread.Sleep(500);
            dataGridView_Targets.EndEdit();
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Target, Sample, Bio Group Color 가 변경되었을때 불리는 이벤트 함수 
        /// </summary>
        /// <param name="plateType">plateType : 0(Target), 1(Sample), 2(Bio Group)</param>
        /// <param name="colorIndex"></param>
        /// <param name="colorValue"></param>
        public void TargetColorChanged(int plateType, int colorIndex, Color colorValue)
        {
            if (plateType == 0)              // Plate Target 
            {
                IpPlate_Target plateInfo = Global.listTargetInfos[plateTargetRowIndex];
                plateInfo.colorIndex = colorIndex;
                Global.listTargetInfos[plateTargetRowIndex] = plateInfo;
                DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_Targets.Rows[plateTargetRowIndex].Cells[1];
                colorBtn.ColorIndex = colorIndex;
            }
            else if (plateType == 1)      // Plate Sample 
            {
                IpPlate_Sample sampleInfo = Global.listSampleInfos[plateSampleRowIndex];
                sampleInfo.colorIndex = colorIndex;
                Global.listSampleInfos[plateSampleRowIndex] = sampleInfo;
                DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_Samples.Rows[plateSampleRowIndex].Cells[1];
                colorBtn.ColorIndex = colorIndex;
            }
            else if (plateType == 2)      // Plate Biological Group 
            {
                IpPlate_Sample BioGroupInfo = Global.listBioGroupInfos[plateBioGroupRowIndex];
                BioGroupInfo.colorIndex = colorIndex;
                Global.listBioGroupInfos[plateBioGroupRowIndex] = BioGroupInfo;
                DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_BioGroup.Rows[plateBioGroupRowIndex].Cells[1];
                colorBtn.ColorIndex = colorIndex;
            }
        }

        /// <summary>
        /// Target 리스트 Painting 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Targets_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            plateTargetColIndex = e.ColumnIndex;
            plateTargetRowIndex = e.RowIndex;
            if (dataGridView_Targets.Rows.Count <= 0)
                return;
            if (!(e.ColumnIndex == 3 || e.ColumnIndex == 4 || e.ColumnIndex == 6 || e.ColumnIndex == 8))
                return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            if (plateTargetRowIndex >= Global.listTargetInfos.Count)
                return;

            //dataGridView_Targets.BeginEdit(true);

            Graphics g = e.Graphics;
            Rectangle rDraw = dataGridView_Targets.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ContentForeground);

            IpPlate_Target plateInfo = Global.listTargetInfos[plateTargetRowIndex];

            // 콤보박스에 이미지 리스트를 표시한다. 
            if (e.ColumnIndex == 3 || e.ColumnIndex == 4)
            {
                string n = "";
                if (e.ColumnIndex == 3)
                    n = Target_Reporters[plateInfo.reporter];
                else
                    n = Target_Quenchers[plateInfo.quencher];
                Font f = new Font("Segoe UI Semibold", 12, FontStyle.Regular);
                //Brush b = new SolidBrush(Color.Yellow);

                //if (textBox1.Text.IndexOf(n) >= 0)
                //    g.FillRectangle(b, rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);//노란색네모를 그릴때 1씩간격을둠.

                g.DrawString(n, f, Brushes.Black, rDraw.X, rDraw.Top); //원래항목을 DrawString으로 그림.
            }
            else if (e.ColumnIndex == 6)
            {
                int y = rDraw.Y + 1;
                int midX = rDraw.X + 5;

                //int rowIndex = dataGridView_Targets.Columns[6].CellTemplate.RowIndex;
                int rowIndex = plateInfo.task;
                if (rowIndex >= 0)
                {
                    g.DrawImage(Global.Tasks[rowIndex], new Rectangle(midX, y + 2, 18, 18));
                }
            }
            else if (e.ColumnIndex == 8)
            {
                int y = rDraw.Y + 1;
                int midX = (int)(rDraw.Width / 2) + rDraw.X;

                g.DrawImage(Resources.List_Remove, new Rectangle(midX - 9, y + 2, 18, 18));
            }
            e.Handled = true;

            //dataGridView_Targets.EndEdit();
        }

        /// <summary>
        /// Target 콤보박스 리스트에 아이템을 표시한다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void combobox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush br = SystemBrushes.WindowText;
            Rectangle rDraw;
            bool bSelected = e.State == DrawItemState.Selected;
            bool bValue = e.State == DrawItemState.ComboBoxEdit;

            //if ((e.Index < 0) || (columnIndex != 1))
            if (e.Index < 0 && !bSelected)
                return;

            Rectangle rect = e.Bounds;//콤보박스 DropDown부분의 사각형크기

            // 콤보박스에 이미지 리스트를 표시한다. 
            if (plateTargetColIndex == 3 || plateTargetColIndex == 4)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Segoe UI Semibold", 12, FontStyle.Regular);
                //Brush b = new SolidBrush(Color.Yellow);

                //if (textBox1.Text.IndexOf(n) >= 0)
                //    g.FillRectangle(b, rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);//노란색네모를 그릴때 1씩간격을둠.

                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top); //원래항목을 DrawString으로 그림.
            }
            // 콤보박스에 이미지 리스트를 표시한다. 
            else if (plateTargetColIndex == 6 && e.Index < 3)
            {
                rDraw = e.Bounds;
                rDraw.Inflate(-1, -1);

                int x, y;

                x = e.Bounds.Left + 25;
                y = e.Bounds.Top + 1;
                int midX = (int)(e.Bounds.Width / 2) + e.Bounds.Left;

                // Show image and ignore text.
                //Image[] Tasks = new Image[] { Resources.Task_U, Resources.Task_S, Resources.Task_N };
                g.DrawImage(Global.Tasks[e.Index], new Rectangle(midX - 6, y + 2, 18, 18));
            }
        }
        #endregion

        #region Plate Sample 리스트 관련 함수 
        /// <summary>
        /// Sample 리스트 초기화 함수 
        /// </summary>
        public void PlateInit_Samples()
        {
            // 전체적으로 폰트 적용하기
            //this.dataGridView_Samples.Font = new Font("Tahoma", 10, FontStyle.Regular);

            // Colum 의 해더부분을 지정하기
            this.dataGridView_Samples.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold);

            // Row 해더부분을 지정하기
            this.dataGridView_Samples.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 14, FontStyle.Regular);
            this.dataGridView_Samples.RowTemplate.Height = 30;

            // 체크박스 추가
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.Name = "chk_Select";
            dataGridView_Samples.Columns.Add(checkBoxColumn);

            // 버튼 추가
            DataGridViewButtonColumn btnColorColumn = new DataGridViewButtonColumn();
            btnColorColumn.CellTemplate = buttonSampleCell;
            // Color 선택 
            btnColorColumn.HeaderText = "";
            btnColorColumn.Name = "btn_Color";
            dataGridView_Samples.Columns.Add(btnColorColumn);

            // 텍스트박스 추가
            DataGridViewTextBoxColumn textNameColumn = new DataGridViewTextBoxColumn();
            // 이름 입력 
            textNameColumn.HeaderText = "Sample Name";
            textNameColumn.Name = "text_Name";
            dataGridView_Samples.Columns.Add(textNameColumn);

            // Comment 입력
            DataGridViewTextBoxColumn textCommmentColumn = new DataGridViewTextBoxColumn();
            textCommmentColumn.HeaderText = "Commment";
            textCommmentColumn.Name = "text_Comment";
            dataGridView_Samples.Columns.Add(textCommmentColumn);

            // 아이템 삭제 
            DataGridViewImageColumn imageRemoveColumn = new DataGridViewImageColumn();
            imageRemoveColumn.HeaderText = "";
            imageRemoveColumn.Name = "image_Remove";
            imageRemoveColumn.Image = Resources.List_Remove;
            dataGridView_Samples.Columns.Add(imageRemoveColumn);

            dataGridView_Samples.EditingControlShowing +=
                new DataGridViewEditingControlShowingEventHandler(
                dataGridView_Samples_EditingControlShowing);

            dataGridView_Samples.Columns[0].Width = 50;
            dataGridView_Samples.Columns[1].Width = 50;
            dataGridView_Samples.Columns[2].Width = 220;
            dataGridView_Samples.Columns[3].Width = 420;
            dataGridView_Samples.Columns[4].Width = 50;

            Global.listSampleInfos.Clear();

            // 초기화할 때 샘플 하나를 추가한다. 
            CustomBtn_AddSample_Click(this, null);
        }

        /// <summary>
        /// Sample TextBox 가 변경되었을때의 이벤트 함수  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Samples_EditingControlShowing(object sender,
            DataGridViewEditingControlShowingEventArgs e)
        {
            // TextBox 가 눌렸을때 이벤트 생성 
            TextBox textBox = e.Control as TextBox;
            if (textBox != null)
            {
                textBox.TextChanged -=
                    new EventHandler(Samples_TextBox_SelectedIndexChanged);

                textBox.TextChanged +=
                    new EventHandler(Samples_TextBox_SelectedIndexChanged);
            }
        }

        /// <summary>
        /// Sample TextBox 가 변경되었을때의 이벤트 함수  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Samples_TextBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (plateSampleRowIndex < 0)
                return;

            string text = ((TextBox)sender).Text;

            //// 이름일 경우 리스트에 같은 이름이 있는지 이름을 변경하지 않는다.  
            //if (plateSampleColIndex == 2)
            //{
            //    bool bFind = false;
            //    IpPlate_Sample plateInfo;
            //    for (int i = 0; i < Global.listSampleInfos.Count; i++)
            //    {
            //        if (i == plateSampleRowIndex)
            //            continue;

            //        plateInfo = Global.listSampleInfos[i];
            //        if (text == plateInfo.name)
            //        {
            //            bFind = true;
            //            break;
            //        }
            //    }

            //    plateInfo = Global.listSampleInfos[plateSampleRowIndex];
            //    if (bFind)
            //    {
            //        string msg = String.Format("Target name is not unique : {0}", text);
            //        MessageBox.Show(msg);
            //        ((TextBox)sender).Text = plateInfo.name;
            //    }
            //    else
            //    {
            //        plateInfo.name = text;
            //        Global.listSampleInfos[plateSampleRowIndex] = plateInfo;
            //        //this.UpdateWellTable_Sample(plateInfo, plateSampleRowIndex);
            //        this.UpdateWellTable("", plateInfo.name, "");
            //    }
            //}

            IpPlate_Sample plateInfo = Global.listSampleInfos[plateSampleRowIndex];
            plateInfo.name = text;
            Global.listSampleInfos[plateSampleRowIndex] = plateInfo;
            if (plateInfo.check)
                UpdateWellTable("", plateInfo.name, "");
        }

        /// <summary>
        /// Sample 추가 메뉴 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomBtn_AddSample_Click(object sender, EventArgs e)
        {
            dataGridView_Samples.Rows.Add();
            int rowCount = dataGridView_Samples.Rows.Count;
            dataGridView_Samples.Rows[rowCount - 1].Cells[0].Value = false;
            DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_Samples.Rows[rowCount - 1].Cells[1];
            int colorIndex = rowCount % 40;
            colorBtn.ColorIndex = colorIndex;
            // 동일한 이름이 있는지 확인하고 없으면 추가되는 이름으로 사용한다. 
            string name = "";
            for (int i = 0; i < 25; i++)
            {
                name = "Sample" + (i + 1).ToString();
                bool isSameName = false;
                for (int j = 0; j < rowCount; j++)
                {
                    if(name == (string)dataGridView_Samples.Rows[j].Cells[2].Value)
                    {
                        isSameName = true;
                        break;
                    }
                }

                if (!isSameName)
                    break;
            }
            dataGridView_Samples.Rows[rowCount - 1].Cells[2].Value = name;

            // sample 리스트에 디폴트값을 추가한다. 
            IpPlate_Sample sampleInfo = new IpPlate_Sample();
            sampleInfo.check = false;
            sampleInfo.colorIndex = colorIndex;
            sampleInfo.name = name;
            Global.listSampleInfos.Add(sampleInfo);
        }

        /// <summary>
        /// Sample TextBox 가 변경되었을때의 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Samples_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridView_Samples.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>
        /// Sample 리스트 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Samples_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            plateSampleColIndex = e.ColumnIndex;
            plateSampleRowIndex = e.RowIndex;

            this.Cursor = Cursors.WaitCursor;
            dataGridView_Samples.BeginEdit(true);

            IpPlate_Sample plateInfo = Global.listSampleInfos[plateSampleRowIndex];

            // Check Box 상태정보 저장 
            if (e.ColumnIndex == 0)
            {
                //dataGridView_Targets.CommitEdit(DataGridViewDataErrorContexts.Commit);
                if (Convert.ToBoolean(dataGridView_Samples.Rows[plateSampleRowIndex].Cells[0].EditedFormattedValue) == true)
                {
                    // 기존에 선택된 샘플이 있으면 해제한다. 
                    for (int i = 0; i < Global.listSampleInfos.Count; i++)
                    {
                        if (plateSampleRowIndex != i)
                        {
                            IpPlate_Sample selectInfo = Global.listSampleInfos[i];
                            selectInfo.check = false;
                            Global.listSampleInfos[i] = selectInfo;
                            dataGridView_Samples.Rows[i].Cells[0].Value = false;

                            // 같은 샘플 이름이 있는 경우 
                            if (Global.listSampleInfos[plateSampleRowIndex].name == selectInfo.name)
                            {
                                string msg = String.Format("Target name is not unique : {0}", selectInfo.name);
                                MessageBox.Show(new Form { TopMost = true }, msg);
                            }
                        }
                    }

                    plateInfo.check = true;
                    selectedSampleIndex = plateSampleRowIndex;
                }
                else
                {
                    if (selectedSampleIndex == plateSampleRowIndex)
                        selectedSampleIndex = -1;

                    plateInfo.check = false;
                }
                Global.listSampleInfos[plateSampleRowIndex] = plateInfo;
                //this.UpdateWellTable_Sample(plateInfo, plateSampleRowIndex);
                this.UpdateWellTable("", plateInfo.name, "");
            }
            // Color 선택 정보 저장 
            else if (e.ColumnIndex == 1)
            {
                System.Drawing.Point pt = dataGridView_Samples.PointToScreen(
                        dataGridView_Samples.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Location);
                buttonSampleCell.PlateType = 1;
                buttonSampleCell.ClickHandler(pt, this);
            }
            // 아이템 삭제  
            else if (e.ColumnIndex == 4)
            {
                //string deleteName = (string)dataGridView_Samples.Rows[selectedSampleIndex].Cells[0].Value;

                // 삭제하기 전에 해당 아이템을 사용하는 셀이 있는지 확인하고 있으면 삭제한다. 
                if (this.listWellInfo.DeleteCheckSample(plateInfo.name) == true)
                {
                    //IpPlate_Target targetInfo = Global.listSampleInfos[plateSampleRowIndex];
                    //listWellInfo.UpdateTargetIndex(colIndex, rowIndex, targetName, targetInfo.check, targetInfo);

                    Global.listSampleInfos.RemoveAt(plateSampleRowIndex);
                    this.listWellInfo.DeleteAllRowIndex(1, plateInfo.name);
                    dataGridView_Samples.Rows.Remove(dataGridView_Samples.Rows[plateSampleRowIndex]);
                    plateSampleRowIndex = -1;

                    UpdateWellTable("", "", "");
                }
            }

            Thread.Sleep(500);
            dataGridView_Samples.EndEdit();
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Sample 리스트 Painting 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Samples_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            plateSampleColIndex = e.ColumnIndex;
            plateSampleRowIndex = e.RowIndex;
            if (dataGridView_Samples.Rows.Count <= 0)
                return;
            if (!(e.ColumnIndex == 4))
                return;
            if (e.RowIndex < 0)
                return;

            Graphics g = e.Graphics;
            Rectangle rDraw = dataGridView_Samples.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ContentForeground);

            if (e.ColumnIndex == 4)
            {
                int y = rDraw.Y + 1;
                int midX = (int)(rDraw.Width / 2) + rDraw.X;

                g.DrawImage(Resources.List_Remove, new Rectangle(midX - 9, y + 2, 18, 18));
            }
            e.Handled = true;
        }

        /// <summary>
        /// Sample 리스트의 Color 가 변경되었을때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_SampleColor_CheckedChanged(object sender, EventArgs e)
        {
            UpdateWellTable("", "", "");
        }
        #endregion

        #region Plate Biological Replicate Group 리스트 관련 함수 
        /// <summary>
        /// Bio Group 초기화 함수 
        /// </summary>
        public void PlateInit_Groups()
        {
            // 전체적으로 폰트 적용하기
            //this.dataGridView_Samples.Font = new Font("Tahoma", 10, FontStyle.Regular);

            // Colum 의 해더부분을 지정하기
            this.dataGridView_BioGroup.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold);

            // Row 해더부분을 지정하기
            this.dataGridView_BioGroup.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 14, FontStyle.Regular);
            this.dataGridView_BioGroup.RowTemplate.Height = 30;

            // 체크박스 추가
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.Name = "chk_Select";
            dataGridView_BioGroup.Columns.Add(checkBoxColumn);

            // 버튼 추가
            DataGridViewButtonColumn btnColorColumn = new DataGridViewButtonColumn();
            btnColorColumn.CellTemplate = buttonSampleCell;
            // Color 선택 
            btnColorColumn.HeaderText = "";
            btnColorColumn.Name = "btn_Color";
            dataGridView_BioGroup.Columns.Add(btnColorColumn);

            // 텍스트박스 추가
            DataGridViewTextBoxColumn textNameColumn = new DataGridViewTextBoxColumn();
            // 이름 입력 
            textNameColumn.HeaderText = "Sample Name";
            textNameColumn.Name = "text_Name";
            dataGridView_BioGroup.Columns.Add(textNameColumn);

            // Comment 입력
            DataGridViewTextBoxColumn textCommmentColumn = new DataGridViewTextBoxColumn();
            textCommmentColumn.HeaderText = "Commment";
            textCommmentColumn.Name = "text_Comment";
            dataGridView_BioGroup.Columns.Add(textCommmentColumn);

            // 아이템 삭제 
            DataGridViewImageColumn imageRemoveColumn = new DataGridViewImageColumn();
            imageRemoveColumn.HeaderText = "";
            imageRemoveColumn.Name = "image_Remove";
            imageRemoveColumn.Image = Resources.List_Remove;
            dataGridView_BioGroup.Columns.Add(imageRemoveColumn);

            dataGridView_BioGroup.Columns[0].Width = 50;
            dataGridView_BioGroup.Columns[1].Width = 50;
            dataGridView_BioGroup.Columns[2].Width = 220;
            dataGridView_BioGroup.Columns[3].Width = 420;
            dataGridView_BioGroup.Columns[4].Width = 50;

            Global.listBioGroupInfos.Clear();
        }

        /// <summary>
        /// Bio Group 추가 메뉴 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomBtn_AddBioGroup_Click(object sender, EventArgs e)
        {
            dataGridView_BioGroup.Rows.Add();
            int rowCount = dataGridView_BioGroup.Rows.Count;
            dataGridView_BioGroup.Rows[rowCount - 1].Cells[0].Value = false;
            DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_BioGroup.Rows[rowCount - 1].Cells[1];
            int colorIndex = rowCount % 40;
            colorBtn.ColorIndex = colorIndex;
            //string name = "Biological Group " + rowCount.ToString();
            // 동일한 이름이 있는지 확인하고 없으면 추가되는 이름으로 사용한다. 
            string name = "";
            for (int i = 0; i < 25; i++)
            {
                name = "Biological Group" + (i + 1).ToString();
                bool isSameName = false;
                for (int j = 0; j < rowCount; j++)
                {
                    if (name == (string)dataGridView_BioGroup.Rows[j].Cells[2].Value)
                    {
                        isSameName = true;
                        break;
                    }
                }

                if (!isSameName)
                    break;
            }
            dataGridView_BioGroup.Rows[rowCount - 1].Cells[2].Value = name;

            // sample 리스트에 디폴트값을 추가한다. 
            IpPlate_Sample sampleInfo = new IpPlate_Sample();
            sampleInfo.check = false;
            sampleInfo.colorIndex = colorIndex;
            sampleInfo.name = name;
            Global.listBioGroupInfos.Add(sampleInfo);
        }

        /// <summary>
        /// Bio Group Text Box 가 변경되었을때 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_BioGroup_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridView_BioGroup.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>
        /// Bio Group 리스트 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_BioGroup_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            plateBioGroupColIndex = e.ColumnIndex;
            plateBioGroupRowIndex = e.RowIndex;

            this.Cursor = Cursors.WaitCursor;
            dataGridView_BioGroup.BeginEdit(true);

            IpPlate_Sample plateInfo = Global.listBioGroupInfos[plateBioGroupRowIndex];

            // Check Box 상태정보 저장 
            if (e.ColumnIndex == 0)
            {
                if (Convert.ToBoolean(dataGridView_BioGroup.Rows[plateBioGroupRowIndex].Cells[0].EditedFormattedValue) == true)
                {
                    // 기존에 선택된 Bio Group 이 있으면 해제한다. 
                    for (int i = 0; i < Global.listSampleInfos.Count; i++)
                    {
                        if (plateSampleRowIndex != i)
                        {
                            IpPlate_Sample selectInfo = Global.listBioGroupInfos[i];
                            selectInfo.check = false;
                            Global.listBioGroupInfos[i] = selectInfo;
                            dataGridView_BioGroup.Rows[i].Cells[0].Value = false;
                        }
                    }
                    plateInfo.check = true;
                    selectedBioGroupIndex = plateBioGroupRowIndex;
                }
                else
                {
                    if (selectedBioGroupIndex == plateBioGroupRowIndex)
                        selectedBioGroupIndex = -1;

                    plateInfo.check = false;
                }

                Global.listBioGroupInfos[plateBioGroupRowIndex] = plateInfo;
                this.UpdateWellTable("", "", plateInfo.name);
            }
            // Color 선택 정보 저장 
            else if (e.ColumnIndex == 1)
            {
                System.Drawing.Point pt = dataGridView_BioGroup.PointToScreen(
                        dataGridView_BioGroup.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Location);
                buttonGroupCell.PlateType = 2;
                buttonGroupCell.ClickHandler(pt, this);
            }
            // 아이템 삭제  
            else if (e.ColumnIndex == 4)
            {
                // 삭제하기 전에 해당 아이템을 사용하는 셀이 있는지 확인하고 있으면 삭제한다. 
                //if (this.listWellInfo.DeleteCheckBioGroup(plateBioGroupRowIndex, plateInfo.name) == true)
                if (this.listWellInfo.DeleteCheckBioGroup(plateInfo.name) == true)
                {
                    Global.listBioGroupInfos.RemoveAt(plateBioGroupRowIndex);
                    this.listWellInfo.DeleteAllRowIndex(2, plateInfo.name);
                    dataGridView_BioGroup.Rows.Remove(dataGridView_BioGroup.Rows[plateBioGroupRowIndex]);
                    plateBioGroupRowIndex = -1;

                    UpdateWellTable("", "", "");
                }
            }

            Thread.Sleep(500);
            dataGridView_BioGroup.EndEdit();
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Bio Group 리스트 Painting 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_BioGroup_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            plateBioGroupColIndex = e.ColumnIndex;
            plateBioGroupRowIndex = e.RowIndex;
            if (dataGridView_BioGroup.Rows.Count <= 0)
                return;
            if (!(e.ColumnIndex == 4))
                return;
            if (e.RowIndex < 0)
                return;

            Graphics g = e.Graphics;
            Rectangle rDraw = dataGridView_BioGroup.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ContentForeground);

            if (e.ColumnIndex == 4)
            {
                int y = rDraw.Y + 1;
                int midX = (int)(rDraw.Width / 2) + rDraw.X;

                g.DrawImage(Resources.List_Remove, new Rectangle(midX - 9, y + 2, 18, 18));
            }
            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// Plate 탭 초기화 함수 
        /// </summary>
        public void PlateInit()
        {
            this.listWellInfo.ListClear();
            ComboBox_PassiveRef.SelectedIndex = 0;

            PlateInit_Targets();
            PlateInit_Samples();
            PlateInit_Groups();
        }

        /// <summary>
        /// 화면 나타날때 사용 함수 
        /// </summary>
        public void ShowForm()
        {
            fadeTimer.Stop();                                               // timer 정지
            this.TopMost = false;
            this.Opacity = 1;
            this.Show();

            Screen[] screens;
            screens = Screen.AllScreens;
            // scan A Form은 Screen 0에 표시
            this.Location = new System.Drawing.Point(screens[Global.ScreensIndex].Bounds.Left, screens[Global.ScreensIndex].Bounds.Top);

            this.m_bIsFadeIn = true;
            fadeTimer.Start();
        }

        /// <summary>
        /// 화면에서 사라질때 사용 함수 
        /// </summary>
        public void HideForm()
        {
            // Form을 천천히 Hide하기 위한 설정
            m_bIsFadeIn = false;
            fadeTimer.Interval = TIMER_INTERVAL;
            fadeTimer.Start();
        }

        /// <summary>
        /// fade Timer 함수(천천히 Show, Hide 하는 Timer 함수)  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    fadeTimer.Stop();           // 타이머 정지

                    this.TopMost = Global.IsTopMost;        // 화면을 앞으로
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
                    this.Hide();                // Intre A Form Close
                }
            }
        }

        /// <summary>
        /// RPROPERTIES 설정 초기화 함수  
        /// </summary>
        public void LoadProperties()
        {
            textBox_Name.Text = Global.UserName;
            textBox_InstrumentType.Text = Global.InstrumentType;
            comboBox_ExperimentType.SelectedIndex = Global.ExperimentType;
            textBox_Chemistry.Text = Global.Chemisty;
            TextBox_Volume.Text = Global.Volume;
            textBox_Cover.Text = Global.Cover;
            TextBox_Comments.Text = Global.Comment;            
        }

        /// <summary>
        /// RPROPERTIES 설정 저장 함수  
        /// </summary>
        /// <param name="isReset">초기화 여부</param>
        public void SaveProperties(bool isReset = false)
        {
            Global.UserName = textBox_Name.Text;
            Global.InstrumentType = textBox_InstrumentType.Text;
            Global.ExperimentType = comboBox_ExperimentType.SelectedIndex;
            Global.Chemisty = textBox_Chemistry.Text;
            Global.Volume = TextBox_Volume.Text;
            Global.Cover = textBox_Cover.Text;
            Global.Comment = TextBox_Comments.Text;
            //Global.MethodFile = textBox_filePath.Text;
            //if(this.methodFileName.Length > 0)
            //    Global.MethodPath = this.methodFileName.Replace(Global.MethodFile, "");

            if (Global.selectedResult == null)
            {
                Result result = new Result
                {
                    UserName = Global.UserName,
                    Barcode = Global.Barcode,
                    InstrumentType = Global.InstrumentType,
                    BlockType = Global.BlockType,
                    ExperimentType = Global.ExperimentType,
                    Chemisty = Global.Chemisty,
                    RunMode = Global.RunMode,
                    Volume = Global.Volume,
                    Cover = Global.Cover,
                    Comment = Global.Comment,
                    ResultDateTime = Global.ResultDateTime,
                    MethodPath = Global.MethodPath,
                    PlatePath = Global.PlatePath,
                    ResultPath = Global.ResultPath,
                    MethodFile = Global.MethodFile,
                    PlateFile = Global.PlateFile,
                    ResultFile = Global.ResultFile
                };

                Global.selectedResult = new ResultInfo(result);
            }
            else
            {
                Global.selectedResult.UserName = Global.UserName;
                Global.selectedResult.Barcode = Global.Barcode;
                Global.selectedResult.InstrumentType = Global.InstrumentType;
                Global.selectedResult.BlockType = Global.BlockType;
                Global.selectedResult.ExperimentType = Global.ExperimentType;
                Global.selectedResult.Chemisty = Global.Chemisty;
                Global.selectedResult.RunMode = Global.RunMode;
                Global.selectedResult.Volume = Global.Volume;
                Global.selectedResult.Cover = Global.Cover;
                Global.selectedResult.Comment = Global.Comment;
                if (isReset)
                {
                    Global.selectedResult.MethodPath = "";
                    Global.selectedResult.PlatePath = "";
                    Global.selectedResult.ResultPath = "";
                    Global.selectedResult.MethodFile = "";
                    Global.selectedResult.PlateFile = "";
                    Global.selectedResult.ResultFile = "";
                }
                else
                {
                    Global.selectedResult.MethodPath = Global.MethodPath;
                    Global.selectedResult.PlatePath = Global.PlatePath;
                    Global.selectedResult.ResultPath = Global.ResultPath;
                    Global.selectedResult.MethodFile = Global.MethodFile;
                    Global.selectedResult.PlateFile = Global.PlateFile;
                    Global.selectedResult.ResultFile = Global.ResultFile;
                }
            }

            Global.SavedSetting();
        }

        /// <summary>
        /// 화면 Layout 구성 함수 
        /// </summary>
        public void ReloadLayout()
        {
            #region Main Page 위치설정
            this.Panel_Show.Controls.Clear();
            this.Panel_Show.Controls.Add(TabControl_Selected);
            //this.TabControl_Selected.Height = (int)((double)(this.Panel_Show.Height - this.Panel_Show.Margin.Size.Height) * (0.85));
            this.TabControl_Selected.Height = (int)((double)(this.Panel_Show.Height - this.Panel_Show.Margin.Size.Height) * (0.95));
            this.TabControl_Selected.Width = (int)((double)(this.Panel_Show.Width - this.Panel_Show.Margin.Size.Width) * (1.0));

            this.customBtn_Previous.Left = this.TabControl_Selected.Location.X;
            this.customBtn_Previous.Top = this.TabControl_Selected.Location.Y + this.TabControl_Selected.Height + 10;

            this.customBtn_Next.Left = this.TabControl_Selected.Location.X + this.TabControl_Selected.Width - this.customBtn_Next.Width;
            this.customBtn_Next.Top = this.TabControl_Selected.Location.Y + this.TabControl_Selected.Height + 10;

            this.Panel_Show.Controls.Add(this.customBtn_Previous);                  // Previous Button control에 추가
            this.Panel_Show.Controls.Add(this.customBtn_Next);                      // Next Button control에 추가
            #endregion Study Tab 위치설정

            #region Method Tab 위치설정
            doubleBufferPanel_Method.Height = (int)(this.TabControl_Selected.Height * 0.77);
            customBtn_scrollLeft.Height = (int)(this.TabControl_Selected.Height * 0.77);
            customBtn_scrollRight.Height = (int)(this.TabControl_Selected.Height * 0.77);
            #endregion Study Tab 위치설정

            #region Method Tab 위치설정
            //chart_Run.Height = (int)(this.TabControl_Selected.Height * 0.55);
            //int posY = chart_Run.Location.Y + chart_Run.Height + 10;
            //progressBar_Wait.Location = new Point(chart_Run.Location.X, posY);
            ZedGraph_Pulse.Height = (int)(this.TabControl_Selected.Height * 0.55);
            int posY = ZedGraph_Pulse.Location.Y + ZedGraph_Pulse.Height + 10;
            progressBar_Wait.Location = new System.Drawing.Point(ZedGraph_Pulse.Location.X, posY);
            //int posX = progressBar_Wait.Location.X + progressBar_Wait.Width + 10;
            label_RemainingTime.Location = new System.Drawing.Point(label_RemainingTime.Location.X, posY);
            posY = progressBar_Wait.Location.Y + progressBar_Wait.Height + 10;
            materialListView_Action.Location = new System.Drawing.Point(ZedGraph_Pulse.Location.X, posY); ;
            //posX = materialListView_Action.Location.X + materialListView_Action.Width + 10;
            //int sizeY = materialListView_Action.Height - customBtn_LogClear.Height - 10;
            //ListBox_Msg.Height += 13;
            ListBox_Msg.Location = new System.Drawing.Point(ListBox_Msg.Location.X, posY);
            int posX = (label_RemainingTime.Location.X + label_RemainingTime.Width) - btnEmergency.Width;
            btnEmergency.Location = new System.Drawing.Point(posX, posY);
            posY = materialListView_Action.Location.Y + materialListView_Action.Height - customBtn_LogClear.Height;
            customBtn_LogClear.Location = new System.Drawing.Point(customBtn_LogClear.Location.X, posY);
            btnTrayOut.Location = new System.Drawing.Point(posX, posY);
            posY = btnTrayOut.Location.Y - btnTrayIn.Height - 5;
            btnTrayIn.Location = new System.Drawing.Point(btnEmergency.Location.X, posY);
            posY = btnTrayIn.Location.Y - btnHeaterDown.Height - 5;
            btnHeaterDown.Location = new System.Drawing.Point(posX, posY);
            posY = btnHeaterDown.Location.Y - btnHeaterUp.Height - 5;
            btnHeaterUp.Location = new System.Drawing.Point(posX, posY);
            #endregion Study Tab 위치설정
        }

        /// <summary>
        /// MainPage Resize 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_Resize(object sender, EventArgs e)
        {
            ReloadLayout();
            ReloadWellTable();
            UpdateWellTable("", "", "");
        }

        /// <summary>
        /// 키보드 처리 함수  
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Shift | Keys.Back:
                    // Run 상태이면 Stop 한다.
                    if (this.IsCameraScan)
                    {
                        Running_StartStop();
                    }
                    var dlg = new SetupPageA();
                    this.Cursor = Cursors.Default;
                    dlg.ShowDialog(this);
                    // ROI를 포함한 저장된 셋팅값을 다시 읽는다. 
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        Global.LoadSetting();

                    dlg.Dispose();
                    Global.isCloseForm = false;

                    if (Global.ArducamSerial.IsOpen)
                        Global.ArducamSerial.ClosePort();

                    // 시리얼 통신 초기화 부분
                    if (Global.ArducamSerial == null) Global.ArducamSerial = new SerialManager();
                    if (!Global.ArducamSerial.IsOpen)
                    {
                        Global.ArducamSerial.PortName = Global.ArducamPort;
                        Global.ArducamSerial.BaudRate = Global.ArducamBaudRate;

                        // 시리얼 통신을 Open 한다.
                        Global.ArducamSerial.OpenPort();
                    }

                    // Accel, Speed, Filter 위치 등을 초기화한다. 
                    InitArducam();

                    // Camera 초기화 
                    IsCameraVisible = InitCamera(Global.ccdCameraNo);

                    // COM Port 가 비정상이거나 카메라가 비정상이면 Run 버튼을 비활성화 한다. 
                    if (!Global.ArducamSerial.IsOpen || !IsCameraVisible)
                    {
                        btnStartRun.Enabled = false;
                    }
                    else
                    {
                        btnStartRun.Enabled = true;
                    }

                    return true;
            }
            return false;
        }

        /// <summary>
        /// 종료시 메시지 박스로 확인하여 처리 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult.Yes != MessageBox.Show("Are you sure you want to exit?", this.Text, MessageBoxButtons.YesNo))
            {
                e.Cancel = true;
            }
            else
            {
                // Run 상태이면 Stop 한다.
                if (this.IsCameraScan)
                    Running_StartStop();

                //his.isStop = true;             // 쓰레드 함수 종료 변수
                if (workerThread != null)
                {
                    workerThread.Join();        // 쓰레드가 완전히 실행되고 쓰레드 종료                
                }

                // 트레이를 꺼내기 전에 Lid Heater 의 상태를 원위치로 이동한다. (고장 발생 방지)
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_HOMING, 0);
                this.Logger(log);
                Thread.Sleep(300);
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_UP);
                this.Logger(log);
                Thread.Sleep(300);
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_IN);
                this.Logger(log);
                Thread.Sleep(300);

                // 열려있는 Serial port를 닫는다.
                if (Global.ArducamSerial != null) Global.ArducamSerial.ClosePort();
            }
            Global.ScreensIndex = this.ScreenIndex;
            Global.SavedSetting();
        }

        /// <summary>
        /// MainPage 타이머 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Main_Tick(object sender, EventArgs e)
        {
            // COM Port 가 비정상이거나 카메라가 비정상이면 Run 버튼을 비활성화 한다. 
            if (!Global.ArducamSerial.IsOpen || !IsCameraVisible)
            {
                pictureBox_comGreen.Visible = false;
                pictureBox_comDisable.Visible = true;
            }
            else
            {
                pictureBox_comGreen.Visible = true;
                pictureBox_comDisable.Visible = false;

                //btnStartRun.Enabled = false;
            }

            // 타이머를 이용한 delay (100 ms)
            if (this.delayTime > 0)
            {
                int resultTime = delayCount * 100;
                if (resultTime >= this.delayTime)
                    this.delayTime = 0;

                delayCount++;
            }

            // Arduino 시리얼 수신 데이터 분석 및 출력 
            revDataAnlysis();

            int curState = Global.PCR_Manager.RxAction.getState();
            if (Global.PCR_Manager.IsRunning)
            {
                string readData = Global.PCR_Manager.readKunPcrData();
                Logger($"{readData}");

                if (timerCount % 5 == 0)
                {
                    //// 시작을 눌렀는데 상태 정보가 ready 이면 Run 이 될때까지 Run을 실행한다. 
                    //if (curState != (int)PCR_STATE.M_RUN && timerCount % 20 == 0)
                    //{
                    //    //Global.PCR_Manager.PCR_Stop();
                    //    //Thread.Sleep(100);
                    //    Global.PCR_Manager.PCR_Run();
                    //}
                    
                    int actionNo = Global.PCR_Manager.RxAction.getCurrent_Action();
                    int curLoop = Global.PCR_Manager.RxAction.getCurrent_Loop();
                    double actionTime = Global.PCR_Manager.RxAction.getSec_TimeLeft();
                    totalTime = Global.PCR_Manager.RxAction.getTotal_TimeLeft();
                    //int actionNo = Global.PCR_Manager.m_RxAction.getLabel();
                    if (actionNo > 0 && totalTime > 0 && actionNo < Global.PCR_Manager.GetActionCount() && actionNo < materialListView_Action.Items.Count)
                    {
                        curActiveNo = actionNo - 1;

                        // 현재 Action이 끝나기 10초전에 캡쳐한다. 
                        // filterCount 가 0 이 아니면 캡쳐가 진행중임.  
                        int realActionNo = Global.PCR_Manager.GetRealActionNo(actionNo);
                        Action_PCR CurAction = Global.PCR_Manager.GetCurAction(realActionNo);
                        int curTime = Convert.ToInt32(CurAction.getTime());
                        //if (IsSuspend)
                        //{
                        //    if (curTime == 0)
                        //        customBtn_Resume_Click(this, null);
                        //    else
                        //        realActionNo += 1;
                        //}

                        //if(!CurAction.getComplete() && CurAction.getCapture() && actionTime > 0 && actionTime<=10)
                        if (curTime > 0 && !IsRunCapture && !IsSuspend && CurAction.getCapture() && actionTime > 0 && actionTime <= 10)
                        {
                            IsSuspend = true;
                            // 캡쳐가 여러번 되는 것을 방지 
                            //oldActiveNo = curActiveNo;
                            //Global.PCR_Manager.SetCurActionComplete(curActiveNo, true);
                            StartCapture();
                        }

                        // 캡쳐가 끝나면 다시 PCR을 Resume 한다. 
                        if (!IsRunCapture && IsSuspend) // && CurAction.getCapture())
                        {
                            if (actionTime == 0)
                                Global.PCR_Manager.PcrResume();
                            else
                                IsSuspend = false;
                        }

                        if (realActionNo != oldActiveNo)
                        {
                            // 캡쳐가 여러번 되는 것을 방지 
                            oldActiveNo = realActionNo;
                        }

                        // 현재 진행중인 PCR 위치를 표시한다. 
                        for (int i = 0; i < materialListView_Action.Items.Count; i++)
                        {
                            string subItem = materialListView_Action.Items[i].SubItems[0].Text;
                            if (subItem == "GOTO")
                                continue;

                            int curNo = Convert.ToInt32(materialListView_Action.Items[i].SubItems[0].Text);
                            if (curNo == actionNo)
                            {
                                curActiveNo = i;
                                break;
                            }
                        }

                        bSelectEnable = !bSelectEnable;
                        materialListView_Action.Items[curActiveNo].Selected = bSelectEnable;
                        materialListView_Action.EnsureVisible(curActiveNo);
                        //materialListView_Action.TopItem = materialListView_Action.Items[actionNo];
                        materialListView_Action.Invalidate();
                    }

                    if(!isProgressInit && totalTime > 0)
                    {
                        // 최대,최소,간격을 임의로 조정
                        progressBar_Wait.Style = ProgressBarStyle.Continuous;
                        progressBar_Wait.Minimum = 0;
                        progressBar_Wait.Maximum = totalTime;
                        //progressBar_Wait.Maximum = this.totalCycle + 1;
                        progressBar_Wait.Step = 2;
                        progressBar_Wait.Value = 0;

                        maxTime = totalTime;
                        //InitRunGraph(0, totalTime + 60, 0, 260);
                        //InitRunGraph(0, curLoop + 5, 0, 260);

                        isProgressInit = true;
                    }
                    else
                    {
                        if (totalTime < progressBar_Wait.Maximum)
                            progressBar_Wait.Value = progressBar_Wait.Maximum - totalTime;
                        else
                            progressBar_Wait.Value = progressBar_Wait.Maximum;

                        curTime = progressBar_Wait.Value;
                        int hour = totalTime / (60 * 60);
                        int time = totalTime - (hour * 60 * 60);
                        int min = time / 60;
                        int sec = time - (min * 60);
                        label_RemainingTime.Text = string.Format("{0}:{1}:{2}", hour.ToString("D2"), min.ToString("D2"), sec.ToString("D2"));
                    }

                    // 10회 * 500ms(Timer 시간) * 2 = 10초
                    // 10초 동안 Cover 온도가 5도 이상 차이나지 않으면 정지하고 에러 표시한다. 
                    double coverTemp = Global.PCR_Manager.RxAction.getCover_Temp();
                    if (!isPcrStateNormal)
                    {
                        double diffTemp = coverTemp - errorCheckCorverTemp;
                        if (errorCheckTime < errorCheckTimeLimit)
                        {
                            // 초기온도값이 95도를 넘으면 정상으로 간주하고 더이상 체크하지 않느다.
                            // 아닌 경우 초기값을 저장한다. 
                            if (errorCheckInitTemp == 0.0 && coverTemp > 0.0)
                            {
                                errorCheckInitTemp = coverTemp;
                                if (coverTemp > 95.0)
                                    isPcrStateNormal = true;
                            }
                            else
                            {
                                diffTemp = coverTemp - errorCheckInitTemp;
                                if (diffTemp >= 5.0)
                                    isPcrStateNormal = true;
                            }
                        }
                        else
                        {
                            diffTemp = coverTemp - errorCheckInitTemp;
                            if (!isPcrStateNormal && diffTemp < 5.0)
                            {
                                this.Logger("PCR stalled by Lid-heating failure. Please restart the PCR.");
                                StopRun();
                                MessageBox.Show(new Form { TopMost = true }, "PCR stalled by Lid-heating failure. Please restart the PCR.");
                            }
                            else
                            {
                                isPcrStateNormal = true;
                            }
                        }
                    }
                    errorCheckTime++;

                    //일단 삭제 후 sync 오류 해결 필요 
                    //// 완료 되었는데도 Run 상태이면 Stop 명령을 실행한다. 
                    //if (captureCount > 0 && curLoop == 0 && actionTime == 0 && totalTime == 0)
                    //{
                    //    StopRun();
                    //}
                }

                timerCount++;
            }
        }

        /// <summary>
        /// 4장의 이미지를 캡쳐하기 전 실행한다.  
        /// </summary>
        private void StartCapture()
        {
            IsRunCapture = true;

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

        /// <summary>
        /// 4장의 이미지를 캡쳐한 후 실행한다.  
        /// </summary>
        private void EndCapture()
        {
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

        /// <summary>
        /// 시리얼 수신 데이터를 분석하여 처리한다. 
        /// </summary>
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
        }

        /// <summary>
        /// Well 테이블 선택이 변경되었을때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataViewImages_SelectionChanged(object sender, EventArgs e)
        {
            UpdateWellTable("", "", "");
        }

        /// <summary>
        /// Well 테이블 Zoom In 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_ZoomIn_Click(object sender, EventArgs e)
        {
            zoomValue += 0.5;
            if (zoomValue > 5.0)
                zoomValue = 5.0;

            // 선택된 웰을 저장한다. 
            int row, col;
            bool[] wellSelected = new bool[25];
            for (col = 0; col < 5; col++)
            {
                for (row = 0; row < 5; row++)
                {
                    int index = col + (row * 5);
                    if (dataViewImages.Rows[row].Cells[col].Selected)
                        wellSelected[index] = true;
                    else
                        wellSelected[index] = false;
                }
            }

            this.listWellInfo.SetZoomScale(zoomValue);
            ReloadWellTable();
            UpdateWellTable("", "", "");

            // 저장된 선택 웰을 업데이트한다. 
            for (col = 0; col < 5; col++)
            {
                for (row = 0; row < 5; row++)
                {
                    int index = col + (row * 5);
                    dataViewImages.Rows[row].Cells[col].Selected = wellSelected[index];
                }
            }
        }

        /// <summary>
        /// Well 테이블 Zoom Out 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_ZoomOut_Click(object sender, EventArgs e)
        {
            zoomValue -= 0.5;
            if (zoomValue < 1.0)
                zoomValue = 1.0;

            // 선택된 웰을 저장한다. 
            int row, col;
            bool[] wellSelected = new bool[25];
            for (col = 0; col < 5; col++)
            {
                for (row = 0; row < 5; row++)
                {
                    int index = col + (row * 5);
                    if (dataViewImages.Rows[row].Cells[col].Selected)
                        wellSelected[index] = true;
                    else
                        wellSelected[index] = false;
                }
            }

            this.listWellInfo.SetZoomScale(zoomValue);
            ReloadWellTable();
            UpdateWellTable("", "", "");

            // 저장된 선택 웰을 업데이트한다. 
            for (col = 0; col < 5; col++)
            {
                for (row = 0; row < 5; row++)
                {
                    int index = col + (row * 5);
                    dataViewImages.Rows[row].Cells[col].Selected = wellSelected[index];
                }
            }
        }

        /// <summary>
        /// Well 테이블 Zoom Fit 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_ZoomFit_Click(object sender, EventArgs e)
        {
            zoomValue = 1.0;

            // 선택된 웰을 저장한다. 
            int row, col;
            bool[] wellSelected = new bool[25];
            for (col = 0; col < 5; col++)
            {
                for (row = 0; row < 5; row++)
                {
                    int index = col + (row * 5);
                    if (dataViewImages.Rows[row].Cells[col].Selected)
                        wellSelected[index] = true;
                    else
                        wellSelected[index] = false;
                }
            }

            this.listWellInfo.SetZoomScale(zoomValue);
            ReloadWellTable();
            UpdateWellTable("", "", "");

            // 저장된 선택 웰을 업데이트한다. 
            for (col = 0; col < 5; col++)
            {
                for (row = 0; row < 5; row++)
                {
                    int index = col + (row * 5);
                    dataViewImages.Rows[row].Cells[col].Selected = wellSelected[index];
                }
            }
        }

        /// <summary>
        /// MainPage 탭 컨트롤이 변경되었을때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_Selected_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabIndex = TabControl_Selected.SelectedIndex;
            //if (tabIndex != 0)
            //{
            //    SaveProperties();
            //    Global.SavedSetting();
            //}

            if (this.IsCameraScan && tabIndex == 3)
            {
                TabControl_Selected.SelectedIndex = 2;
                return;
            }

            //string tabItem = ((sender as TabControl).SelectedItem as TabItem).Header as string;
            switch (TabControl_Selected.SelectedIndex)
            {
                //case 0:       // Properties
                //    break;
                case 0:         // Method                    
                    break;
                case 1:         // Plate                    
                    break;
                case 2:         // Run
                    // Method가 저장되지 않았거나 수정된 경우 
                    if (textBox_filePath.Text.Length <= 0 || methodEdit)
                    {
                        btnStartRun.Enabled = false;
                        //btnStopRun.Enabled = false;

                        //if (DialogResult.Yes == MessageBox.Show("Do you want to save the current method file ?", this.Text, MessageBoxButtons.YesNo))
                        {
                            bool bCreate = false;
                            if (textBox_filePath.Text.Length <= 0)
                            {
                                string mathodPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Method");
                                Global.MethodFile = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss") + ".method";
                                this.methodFileName = mathodPath + "\\" + Global.MethodFile;
                                bCreate = true;
                            }
                            SaveMethod(this.methodFileName, bCreate);    // Save

                            bCreate = false;
                            if (textBox_plateFilePath.Text.Length <= 0)
                            {
                                string platePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plate");
                                Global.PlateFile = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss") + ".plate";
                                this.plateFileName = platePath + "\\" + Global.PlateFile;
                                bCreate = true;
                            }
                            SavePlate(this.plateFileName, bCreate);    // Save

                            if (this.methodFileName.Length > 0)
                            {
                                btnStartRun.Enabled = true;
                                //btnStopRun.Enabled = true;
                                PCRLoad(this.methodFileName);

                                methodEdit = false;
                            }
                        }
                    }
                    else
                    {
                        btnStartRun.Enabled = true;
                        //btnStopRun.Enabled = true;
                        PCRLoad(this.methodFileName);
                    }

                    // COM Port 가 비정상이거나 카메라가 비정상이면 Run 버튼을 비활성화 한다. 
                    if (!Global.ArducamSerial.IsOpen || !IsCameraVisible)
                    {
                        btnStartRun.Enabled = false;
                    }
                    else
                    {
                        btnStartRun.Enabled = true;
                    }

                    break;
                case 3:         // Result                    
                    if (Global.selectedResult.ResultFile != null && Global.selectedResult.ResultFile.Length > 0)
                    {
                        string resultPath = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
                        LoadResult(resultPath);
                    }

                    break;
                case 4:         // Export                    
                    break;
                default:
                    break;
            }

            if (TabControl_Selected.SelectedIndex >= (TabControl_Selected.TabCount - 1))
                customBtn_Next.Visible = false;
            else
                customBtn_Next.Visible = true;

            if (TabControl_Selected.SelectedIndex <= 0)
                customBtn_Previous.Visible = false;
            else
                customBtn_Previous.Visible = true;
        }

        /// <summary>
        /// Method를 초기화한다.  
        /// </summary>
        public void MethodInit()
        {
            this.methodFileName = "";
            Group_Stage groupHoldStage = new Group_Stage(this, eGROUP_TYPE.HOLD_STAGE);
            this.stageList.Add(groupHoldStage);
            Group_Stage groupPcrStage = new Group_Stage(this, eGROUP_TYPE.PCR_STAGE);
            this.stageList.Add(groupPcrStage);
            //AddStage(groupInit);
            StageSort();
        }

        /// <summary>
        /// Stage를 추가한다. 
        /// </summary>
        /// <param name="theGroupBox">추가할 Stage</param>
        public void AddStage(Group_Stage theGroupBox)
        {
            this.SuspendLayout();
            this.doubleBufferPanel_Method.Controls.Add(theGroupBox);

            theGroupBox.Location = new System.Drawing.Point(5, 5);
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Step 리스트를 재정렬한다.  
        /// </summary>
        public void StageSort()
        {
            // 스크롤이 초기화 되는 것을 방지한다. 
            System.Drawing.Point ptPnlBayView = doubleBufferPanel_Method.AutoScrollPosition;
            this.doubleBufferPanel_Method.Controls.Clear();
            doubleBufferPanel_Method.SuspendLayout();

            int locationX = 5;
            int locationY = 5;
            int maxHscrollValue = this.doubleBufferPanel_Method.Width;
            int i = 0;
            int j = 0;
            for (i = 0; i < this.stageList.Count; i++)
            {
                #region Step 들간 전후 온도를 설정한다. 
                for (j = 0; j < this.stageList[i].stepList.Count; j++)
                {
                    // 첫번째 스테이지의 경우 첫번째 스탭의 경우 초기값으로 설정하고 아닌 경우는 이전 스탭의 온도를 설정한다. 
                    // 이전 스테이지가 존재하는 경우 첫번째 스탭의 이전 온도는 이전 스테이지의 마지막 온도로 설정한다. 
                    if (i == 0)
                    {
                        if (j == 0)
                        {
                            if (this.stageList[i].StageType == eGROUP_TYPE.HOLD_STAGE)
                                this.stageList[i].stepList[j].StepTempPreview = 30.0;
                            else
                                this.stageList[i].stepList[j].StepTempPreview = Global.default_PcrStepTemp1;
                        }
                        else
                        {
                            this.stageList[i].stepList[j].StepTempPreview = this.stageList[i].stepList[j - 1].StepTemp;
                        }
                    }
                    else
                    {
                        if (j == 0)
                            this.stageList[i].stepList[j].StepTempPreview = this.stageList[i - 1].stepList[this.stageList[i - 1].stepList.Count - 1].StepTemp;
                        else
                            this.stageList[i].stepList[j].StepTempPreview = this.stageList[i].stepList[j - 1].StepTemp;
                    }

                    // 스테이지의 마지막 스탭의 경우 다음 스테이지의 첫번째 스탭온도를 다음 온도로 설정한다. 
                    if (j == this.stageList[i].stepList.Count - 1)
                    {
                        // 다음 스테이지가 있는 경우 
                        if (i < this.stageList.Count - 1 && this.stageList[i + 1].stepList.Count > 0)
                            this.stageList[i].stepList[j].StepTempNext = this.stageList[i + 1].stepList[0].StepTemp;
                        else
                        {
                            if (this.stageList[i].StageType == eGROUP_TYPE.HOLD_STAGE)
                                this.stageList[i].stepList[j].StepTempNext = Global.default_HoldStepTemp1;
                            else
                                this.stageList[i].stepList[j].StepTempNext = Global.default_PcrStepTemp1;
                        }
                    }
                    else
                    {
                        this.stageList[i].stepList[j].StepTempNext = this.stageList[i].stepList[j + 1].StepTemp;
                    }
                }
                #endregion

                this.stageList[i].StageId = i + 1;
                if (this.stageList[i].StageType == eGROUP_TYPE.HOLD_STAGE)
                    this.stageList[i].StageName = "HoldStage_" + this.stageList[i].StageId.ToString();
                else
                    this.stageList[i].StageName = "PCRStage_" + this.stageList[i].StageId.ToString();
                this.stageList[i].Name = this.stageList[i].StageName;
                if (this.stageList[i].StageType == eGROUP_TYPE.HOLD_STAGE)
                    this.stageList[i].StageName = "Hold Stage";
                else
                    this.stageList[i].StageName = "PCR Stage";

                this.doubleBufferPanel_Method.Controls.Add(this.stageList[i]);

                this.stageList[i].Location = new System.Drawing.Point(locationX, locationY);
                //this.stageList[i].Height = this.doubleBufferPanel_Method.Height - 30;
                locationX += this.stageList[i].Width + 5;
                this.stageList[i].Invalidate();
            }

            doubleBufferPanel_Method.ResumeLayout();
            doubleBufferPanel_Method.AutoScrollPosition = new System.Drawing.Point(Math.Abs(ptPnlBayView.X), Math.Abs(doubleBufferPanel_Method.AutoScrollPosition.Y));
        }

        /// <summary>
        /// Stage를 활성 또는 비활성 상태로 만든다. 
        /// </summary>
        /// <param name="isEnable">활성 여부</param>
        public void StageEnable(bool isEnable)
        {
            for (int i = 0; i < this.stageList.Count; i++)
            {
                this.stageList[i].Enabled = isEnable;
            }
        }

        /// <summary>
        /// 오른쪽에 Stage를 추가한다. 
        /// </summary>
        /// <param name="stageID">Stage ID</param>
        public void StageInsertRight(int stageID)
        {
            methodEdit = true;

            int insertID = 0;
            insertID = stageID;

            Group_Stage newStage = new Group_Stage(this, (eGROUP_TYPE)flatComboBox_StageType.SelectedIndex);
            if (this.stageList.Count < insertID)
                this.stageList.Add(newStage);
            else
                this.stageList.Insert(insertID, newStage);

            StageSort();
        }

        /// <summary>
        /// 왼쪽에 Stage를 추가한다. 
        /// </summary>
        /// <param name="stageID"></param>
        public void StageInsertLeft(int stageID)
        {
            methodEdit = true;

            int insertID = 0;
            if (stageID == 0)
                insertID = 0;
            else
                insertID = stageID - 1;

            Group_Stage newStage = new Group_Stage(this, (eGROUP_TYPE)flatComboBox_StageType.SelectedIndex);
            this.stageList.Insert(insertID, newStage);

            AddStage(newStage);
            StageSort();
        }

        /// <summary>
        /// Stage를 삭제한다. 
        /// </summary>
        /// <param name="stageID">삭제할 Stage ID</param>
        public void StageRemove(int stageID)
        {
            methodEdit = true;

            if (this.stageList.Count > 1)
            {
                this.stageList.RemoveAt(stageID - 1);
                StageSort();
            }
        }

        /// <summary>
        /// Method 파일을 읽어 리스트에 추가한다.  
        /// </summary>
        /// <param name="filePath">Method 파일</param>
        public void PCRLoad(string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            FileIniDataParser parser = new FileIniDataParser();
            IniData parsedData = parser.ReadFile(fullPath);

            int stageCount = 0;
            try { if (parsedData["HEADER"]["STAGE_COUNT"] != null) stageCount = Convert.ToInt32(parsedData["HEADER"]["STAGE_COUNT"]); } catch { }
            // 스테이지 존재하지 않으면 더이상 진행하지 않는다. 
            if (stageCount <= 0)
            {
                string errorMsg = string.Format("Stage Count Error = {0}", stageCount);
                MessageBox.Show(new Form { TopMost = true }, errorMsg);
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
            totalCycle = 0;     // 전체 캡쳐 사이클 횟수 (그래프에서는 최대 X 좌표로 활용)
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
                    if (isCapture)
                    {
                        totalCycle++;

                        secTime = 0;
                        isCapture = false;
#if DEBUG
#else
                        Global.PCR_Manager.Action_Add(actionCount.ToString(), stepTemp.ToString(), secTime.ToString(), isCapture);
#endif
                    }

                    if (stageType == eGROUP_TYPE.PCR_STAGE && j == stepCount - 1)
                    {
                        materialListView_Action.Items.Add(new ListViewItem(new String[] {
                                String.Format("{0}", "GOTO"),                               // 번호
                                String.Format("{0}", startStep),                            // 회귀 번호 
                                String.Format("{0}", stageCycle)                            // 반복 회수 
                            }));
                        Global.PCR_Manager.Action_Add("GOTO", startStep.ToString(), stageCycle.ToString(), false);

                        // 사이클 사이에 캡쳐가 몇개 들어있는지 체크하고 개수*사이클 만큼 총 사이클에 더한다. 
                        int captureCnt = 0;
                        for (int step = startStep; step < actionCount + 1; step++)
                        {
                            Action_PCR action = Global.PCR_Manager.GetAction(step.ToString());
                            if (action != null && action.getCapture())
                                captureCnt++;
                        }
                        totalCycle += (captureCnt * stageCycle);
                    }

                    actionCount++;
                }
            }
            materialListView_Action.EndUpdate();

            // 그래프를 초기화한다. 
            if (totalCycle > 0 && Global.ResultFile.Length <= 0)
            {
                InitRunGraph(0, totalCycle + 1, 0, 260);
                InitResultGraph(0, totalCycle + 1, 0, 260);
            }
        }

        /// <summary>
        /// Method 파일을 저장한다.  
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveMethod(string filePath = "", bool isCreate = false)
        {
            //string saveFileName = filePath;
            if (filePath.Length <= 0)
            {
                SaveFileDialog saveFileDlg = new SaveFileDialog();
                saveFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Method\\";
                //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
                DirectoryInfo di = new DirectoryInfo(saveFileDlg.InitialDirectory);
                if (di.Exists == false)
                    di.Create();
                saveFileDlg.Filter = "Method File(*.method)|*.method";
                saveFileDlg.Title = "Save an Method File";
                if (saveFileDlg.ShowDialog() == DialogResult.OK)
                {
                    //saveFileName = Path.GetFileNameWithoutExtension(saveFileDlg.FileName) + ".method";
                    filePath = Path.GetFullPath(saveFileDlg.FileName);
                }
                else
                    return;
            }
            else
            {
                // 자동 생성이 아닌 경우 파일 존재 유무를 확인한다. 
                if (!isCreate)
                {
                    string errorMsg = string.Format("File nout found!\r\n{0}", filePath);
                    if (!File.Exists(filePath))
                        MessageBox.Show(new Form { TopMost = true }, errorMsg);
                }
            }

            Global.MethodFile = Path.GetFileNameWithoutExtension(filePath) + ".method";
            //Global.MethodPath = filePath.Replace(Global.MethodFile, "");
            Global.MethodPath = filePath.Substring(0, filePath.Length - Global.MethodFile.Length);
            SaveProperties();

            //Create ini file
            FileIniDataParser parser = new FileIniDataParser();
            IniData savedData = new IniData();

            savedData.Sections.AddSection("HEADER");
            savedData["HEADER"]["STAGE_COUNT"] = this.stageList.Count.ToString();           // Stage Count 

            // 스테이지 리스트를 저장한다. 
            for (int i = 0; i < this.stageList.Count; i++)
            {
                string sectionName = string.Format("STAGE{0}", i + 1);     // Section Name
                savedData.Sections.AddSection(sectionName);
                int nType = (int)this.stageList[i].StageType;
                savedData[sectionName]["STAGE_TYPE"] = nType.ToString();           // Step Count 
                savedData[sectionName]["STAGE_CYCLE"] = this.stageList[i].StageCycleCount.ToString();           // Step Count 
                savedData[sectionName]["STEP_COUNT"] = this.stageList[i].stepList.Count.ToString();           // Step Count 

                // 스텝 리스트를 저장한다. 
                for (int j = 0; j < this.stageList[i].stepList.Count; j++)
                {
                    string subTitle = string.Format("STEP{0}", j + 1);     // Step Name
                                                                           // 저장된 스탭값을 얻어온다. 
                    savedData[sectionName][subTitle] = this.stageList[i].stepList[j].GetSettingValues();
                }
            }

            //string fullPath = Path.GetFullPath(saveFileDlg.FileName);

            //Save the file
            string iniPath = filePath;
            parser.WriteFile(iniPath, savedData);

            this.methodFileName = filePath;
            string saveFileName = Path.GetFileNameWithoutExtension(filePath) + ".method";
            textBox_filePath.Text = saveFileName;
        }

        /// <summary>
        /// Method Save 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SaveMethod_Click(object sender, EventArgs e)
        {
            methodEdit = false;

            SaveMethod(this.methodFileName);
            // 결과가 있으면 결과 엑셀 파일에 Method 탭을 만들고 내용을 저장한다. 
            if(Global.selectedResult != null && Global.selectedResult.ResultFile.Length > 0)
            {
                string resultExcel = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
                string methodExcel = Global.selectedResult.MethodPath + Global.selectedResult.MethodFile;
                SaveExcelMethod(resultExcel, methodExcel);
            }
        }

        /// <summary>
        /// Method Load 버튼 클릭 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_LoadMethod_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Method\\";
            //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
            DirectoryInfo di = new DirectoryInfo(openFileDlg.InitialDirectory);
            if (di.Exists == false)
                di.Create();
            openFileDlg.Filter = "Method File(*.method)|*.method";
            openFileDlg.Title = "Load an Method File";
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string fullPath = Path.GetFullPath(openFileDlg.FileName);
                LoadMethod(fullPath);

                totalCycle = 40;
                InitRunGraph(0, totalCycle + 1, 0, 260);
                InitResultGraph(0, totalCycle + 1, 0, 260);

                methodEdit = false;
            }
        }

        /// <summary>
        /// Method 파일을 읽어온다.  
        /// </summary>
        /// <param name="fullPath"></param>
        public void LoadMethod(string fullPath)
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData parsedData = parser.ReadFile(fullPath);

            int stageCount = 0;
            try { if (parsedData["HEADER"]["STAGE_COUNT"] != null) stageCount = Convert.ToInt32(parsedData["HEADER"]["STAGE_COUNT"]); } catch { }
            // 스테이지 존재하지 않으면 더이상 진행하지 않는다. 
            if (stageCount <= 0)
            {
                string errorMsg = string.Format("Stage Count Error = {0}", stageCount);
                MessageBox.Show(new Form { TopMost = true }, errorMsg);
                return;
            }

            this.stageList.Clear();
            // 스테이지 리스트를 읽어온다. 
            for (int i = 0; i < stageCount; i++)
            {
                string sectionName = string.Format("STAGE{0}", i + 1);     // Section Name
                eGROUP_TYPE stageType = eGROUP_TYPE.PCR_STAGE;
                int stageCycle = 0;
                try { if (parsedData[sectionName]["STAGE_TYPE"] != null) stageType = (eGROUP_TYPE)Convert.ToInt32(parsedData[sectionName]["STAGE_TYPE"]); } catch { }
                try { if (parsedData[sectionName]["STAGE_CYCLE"] != null) stageCycle = Convert.ToInt32(parsedData[sectionName]["STAGE_CYCLE"]); } catch { }
                int stepCount = 0;
                try { if (parsedData[sectionName]["STEP_COUNT"] != null) stepCount = Convert.ToInt32(parsedData[sectionName]["STEP_COUNT"]); } catch { }

                // 스테이지 생성시 스탭이 존재하지 않으면 기본스탭을 생성하고
                // 존재하면 설정된 개수만큼 스탭을 생성한다. 
                Group_Stage groupStage = null;
                if (stepCount <= 0)
                    groupStage = new Group_Stage(this, (eGROUP_TYPE)stageType, true);
                else
                    groupStage = new Group_Stage(this, (eGROUP_TYPE)stageType, false);
                groupStage.StageCycleCount = stageCycle;
                groupStage.NumericUpDown_CycleCount.Value = stageCycle;
                this.stageList.Add(groupStage);

                // 스텝 리스트를 읽어온다. 
                string strValues = "";
                for (int j = 0; j < stepCount; j++)
                {
                    string subTitle = string.Format("STEP{0}", j + 1);     // Step Name
                    try { if (parsedData[sectionName][subTitle] != null) strValues = parsedData[sectionName][subTitle]; } catch { }
                    string[] words = strValues.Split(',');
                    if (words.Length != 3)
                        continue;

                    double stepTemp = Convert.ToDouble(words[0]);
                    TimeSpan stepTime = TimeSpan.Parse(words[1]);
                    bool isCapture = Convert.ToBoolean(words[2]);
                    Group_Step newStep = new Group_Step(groupStage, stepTemp, stepTime, isCapture);
                    this.stageList[this.stageList.Count - 1].stepList.Add(newStep);
                }
                this.stageList[this.stageList.Count - 1].StepSort();
                //this.stageList.Add(groupStage);
                //this.StageSort();
            }
            StageSort();

            this.methodFileName = fullPath;
            textBox_filePath.Text = Path.GetFileNameWithoutExtension(fullPath) + ".method";
            //textBox_filePath.Text = openFileDlg.SafeFileName;

            Global.MethodFile = textBox_filePath.Text;
            Global.MethodPath = this.methodFileName.Replace(Global.MethodFile, "");

            methodEdit = false;
        }

        /// <summary>
        /// Method 탭을 초기화한다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_NewMethod_Click(object sender, EventArgs e)
        {
            methodEdit = false;

            textBox_filePath.Text = "";
            this.stageList.Clear();
            MethodInit();

            totalCycle = 40;
            InitRunGraph(0, totalCycle + 1, 0, 260);
            InitResultGraph(0, totalCycle + 1, 0, 260);
        }

        /// <summary>
        /// Method SaveAs 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_SaveAsMethod_Click(object sender, EventArgs e)
        {
            SaveMethod();
        }

        /// <summary>
        /// Plate Load 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_PlateLoad_Click(object sender, EventArgs e)
        {
            selectedSampleIndex = -1;
            selectedBioGroupIndex = -1;

            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Plate\\";
            //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
            DirectoryInfo di = new DirectoryInfo(openFileDlg.InitialDirectory);
            if (di.Exists == false)
                di.Create();
            openFileDlg.Filter = "Plate File(*.plate)|*.plate";
            openFileDlg.Title = "Load an Plate File";
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                string fullPath = Path.GetFullPath(openFileDlg.FileName);

                dataViewImages.ColumnHeadersVisible = false;
                dataViewImages.SelectAll();
                dataViewImages.ColumnHeadersVisible = true;

                LoadPlate(fullPath);

                SaveProperties(true);
            }
        }

        /// <summary>
        /// Plate 파일을 읽어온다. 
        /// </summary>
        /// <param name="fullPath">Plate 파일 이름</param>
        public void LoadPlate(string fullPath)
        {
            this.listWellInfo.ListClear();

            dataGridView_Targets.Rows.Clear();
            Global.listTargetInfos.Clear();
            dataGridView_Samples.Rows.Clear();
            Global.listSampleInfos.Clear();
            dataGridView_BioGroup.Rows.Clear();
            Global.listBioGroupInfos.Clear();

            FileIniDataParser parser = new FileIniDataParser();
            IniData parsedData = parser.ReadFile(fullPath);

            int targetCount = 0;
            int sampleCount = 0;
            int bioGroupCount = 0;

            try { if (parsedData["GENERAL"]["passiveRef"] != null) Global.passiveRef = Convert.ToInt32(parsedData["TARGET"]["passiveRef"]); } catch { }
            ComboBox_PassiveRef.SelectedIndex = Global.passiveRef;

            try { if (parsedData["TARGET"]["TARGET_COUNT"] != null) targetCount = Convert.ToInt32(parsedData["TARGET"]["TARGET_COUNT"]); } catch { }
            try { if (parsedData["SAMPLE"]["SAMPLE_COUNT"] != null) sampleCount = Convert.ToInt32(parsedData["SAMPLE"]["SAMPLE_COUNT"]); } catch { }
            try { if (parsedData["BIOGROUP"]["BIOGROUP_COUNT"] != null) bioGroupCount = Convert.ToInt32(parsedData["BIOGROUP"]["BIOGROUP_COUNT"]); } catch { }

            // Plate 정보가 존재하지 않으면 더이상 진행하지 않는다. 
            if (targetCount <= 0 && sampleCount <= 0 && bioGroupCount <= 0)
            {
                string errorMsg = string.Format("Plate infomation Error!");
                MessageBox.Show(new Form { TopMost = true }, errorMsg);
                return;
            }

            int i, j;
            int colCount = 0;
            int rowCount = 0;

            // TARGET 리스트를 읽어온다. 
            dataGridView_Targets.Rows.Clear();
            Global.listTargetInfos.Clear();
            for (i = 0; i < targetCount; i++)
            {
                string subTitle = string.Format("TARGET_ROW{0}", i + 1);    // Target sub title
                string subData = "";
                try { if (parsedData["TARGET"][subTitle] != null) subData = parsedData["TARGET"][subTitle]; } catch { }
                string[] words = subData.Split('/');
                if (words.Length < 8)
                    continue;

                IpPlate_Target targetInfo = new IpPlate_Target();
                targetInfo.check = Convert.ToBoolean(words[0]);
                targetInfo.colorIndex = Convert.ToInt32(words[1]);
                targetInfo.name = words[2];
                targetInfo.reporter = Convert.ToInt32(words[3]);
                targetInfo.quencher = Convert.ToInt32(words[4]);
                targetInfo.comment = words[5];
                targetInfo.task = Convert.ToInt32(words[6]);
                targetInfo.quantity = words[7];

                Global.listTargetInfos.Add(targetInfo);

                dataGridView_Targets.Rows.Add();
                rowCount = dataGridView_Targets.Rows.Count;
                dataGridView_Targets.Rows[rowCount - 1].Cells[0].Value = targetInfo.check;
                DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_Targets.Rows[rowCount - 1].Cells[1];
                colorBtn.ColorIndex = targetInfo.colorIndex;
                dataGridView_Targets.Rows[rowCount - 1].Cells[2].Value = targetInfo.name;
                //DataGridViewComboBoxCell cmbReporter = (DataGridViewComboBoxCell)dataGridView_Targets.Rows[rowCount - 1].Cells[3];
                //((ComboBox)cmbReporter).SelectedIndex = targetInfo.reporter;
                dataGridView_Targets.Rows[rowCount - 1].Cells[3].Value = Target_Reporters[targetInfo.reporter];
                dataGridView_Targets.UpdateCellValue(3, rowCount - 1);
                dataGridView_Targets.Rows[rowCount - 1].Cells[4].Value = Target_Quenchers[targetInfo.quencher];
                dataGridView_Targets.UpdateCellValue(4, rowCount - 1);
                dataGridView_Targets.Rows[rowCount - 1].Cells[5].Value = targetInfo.comment;
                //dataGridView_Targets.Rows[rowCount - 1].Cells[6].Value = this.Tasks[targetInfo.task];
                //dataGridView_Targets.UpdateCellValue(6, rowCount - 1);
                dataGridView_Targets.Rows[rowCount - 1].Cells[7].Value = targetInfo.quantity;
            }

            // Sample 리스트를 읽어온다. 
            dataGridView_Samples.Rows.Clear();
            Global.listSampleInfos.Clear();
            for (i = 0; i < sampleCount; i++)
            {
                string subTitle = string.Format("SAMPLE_ROW{0}", i + 1);    // Target sub title
                string subData = "";
                try { if (parsedData["SAMPLE"][subTitle] != null) subData = parsedData["SAMPLE"][subTitle]; } catch { }
                string[] words = subData.Split('/');
                if (words.Length < 4)
                    continue;

                IpPlate_Sample smapleInfo = new IpPlate_Sample();
                smapleInfo.check = Convert.ToBoolean(words[0]);
                smapleInfo.colorIndex = Convert.ToInt32(words[1]);
                smapleInfo.name = words[2];
                smapleInfo.comment = words[3];

                Global.listSampleInfos.Add(smapleInfo);

                dataGridView_Samples.Rows.Add();
                rowCount = dataGridView_Samples.Rows.Count;
                dataGridView_Samples.Rows[rowCount - 1].Cells[0].Value = smapleInfo.check;
                DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_Samples.Rows[rowCount - 1].Cells[1];
                colorBtn.ColorIndex = smapleInfo.colorIndex;
                dataGridView_Samples.Rows[rowCount - 1].Cells[2].Value = smapleInfo.name;
                dataGridView_Samples.Rows[rowCount - 1].Cells[3].Value = smapleInfo.comment;
            }

            // Bio Group 리스트를 읽어온다. 
            dataGridView_BioGroup.Rows.Clear();
            Global.listBioGroupInfos.Clear();
            for (i = 0; i < bioGroupCount; i++)
            {
                string subTitle = string.Format("BIOGROUP_ROW{0}", i + 1);    // Target sub title
                string subData = "";
                try { if (parsedData["BIOGROUP"][subTitle] != null) subData = parsedData["BIOGROUP"][subTitle]; } catch { }
                string[] words = subData.Split('/');
                if (words.Length < 4)
                    continue;

                IpPlate_Sample smapleInfo = new IpPlate_Sample();
                smapleInfo.check = Convert.ToBoolean(words[0]);
                smapleInfo.colorIndex = Convert.ToInt32(words[1]);
                smapleInfo.name = words[2];
                smapleInfo.comment = words[3];

                Global.listBioGroupInfos.Add(smapleInfo);

                dataGridView_BioGroup.Rows.Add();
                rowCount = dataGridView_BioGroup.Rows.Count;
                dataGridView_BioGroup.Rows[rowCount - 1].Cells[0].Value = smapleInfo.check;
                DataGridViewColorButtonCell colorBtn = (DataGridViewColorButtonCell)dataGridView_BioGroup.Rows[rowCount - 1].Cells[1];
                colorBtn.ColorIndex = smapleInfo.colorIndex;
                dataGridView_BioGroup.Rows[rowCount - 1].Cells[2].Value = smapleInfo.name;
                dataGridView_BioGroup.Rows[rowCount - 1].Cells[3].Value = smapleInfo.comment;
            }

            // Well 정보를 읽어온다.
            try { if (parsedData["WELLS"]["COL_COUNT"] != null) colCount = Convert.ToInt32(parsedData["WELLS"]["COL_COUNT"]); } catch { }
            try { if (parsedData["WELLS"]["ROW_COUNT"] != null) rowCount = Convert.ToInt32(parsedData["WELLS"]["ROW_COUNT"]); } catch { }

            List<int> listTargetIdx = new List<int>();
            int subIndex = 0;
            string plateName = "";

            for (i = 0; i < colCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    int index = j + (i * rowCount);

                    if (index < Global.listRoiInfos.Count)
                    {
                        ROIShape roiInfo = Global.listRoiInfos[index];
                        this.listWellInfo.SetOffsetGain(index, roiInfo.ROI_Gain, roiInfo.ROI_Offset);
                    }

                    // Target Data
                    string subTitle = string.Format("WELL{0}_TARGET", index + 1);
                    string subData = "";
                    try { if (parsedData["WELLS"][subTitle] != null) subData = parsedData["WELLS"][subTitle]; } catch { }
                    string[] words = subData.Split('/');
                    if (words.Length <= 0)
                        continue;

                    for (subIndex = 0; subIndex < words.Length; subIndex++)
                    {
                        if (words[subIndex].Length <= 0)
                            break;

                        plateName = words[subIndex];

                        // 인덱스 찾기 
                        int plateIndex = -1;
                        for (int tIndex = 0; tIndex < Global.listTargetInfos.Count; tIndex++)
                        {
                            if(Global.listTargetInfos[tIndex].name == plateName)
                            {
                                plateIndex = tIndex;
                                break;
                            }
                        }
                        if (plateIndex >= 0)
                            this.listWellInfo.UpdateTargetIndex(j, i, plateName, true, Global.listTargetInfos[plateIndex]);

                        // 첫번째 셀의 타겟 리스트를 저장한다. 
                        //if (i == 0 && j == 0)
                        //    listTargetIdx.Add(plateIndex);
                    }

                    // Sample Data
                    subTitle = string.Format("WELL{0}_SAMPLE", index + 1);
                    subData = "";
                    try { if (parsedData["WELLS"][subTitle] != null) subData = parsedData["WELLS"][subTitle]; } catch { }
                    words = subData.Split('/');
                    if (words.Length <= 0)
                        continue;

                    if (words[0].Length > 0)
                    {
                        plateName = words[0];
                        // 인덱스 찾기 
                        int plateIndex = -1;
                        for (int tIndex = 0; tIndex < Global.listSampleInfos.Count; tIndex++)
                        {
                            if (Global.listSampleInfos[tIndex].name == plateName)
                            {
                                plateIndex = tIndex;
                                break;
                            }
                        }
                        if (plateIndex >= 0)
                            this.listWellInfo.UpdateSampleIndex(j, i, plateName, true, Global.listSampleInfos[plateIndex]);
                    }

                    // Bio Group Data
                    subTitle = string.Format("WELL{0}_BIOGROUP", index + 1);
                    subData = "";
                    try { if (parsedData["WELLS"][subTitle] != null) subData = parsedData["WELLS"][subTitle]; } catch { }
                    words = subData.Split('/');
                    if (words.Length <= 0)
                        continue;

                    for (subIndex = 0; subIndex < words.Length; subIndex++)
                    {
                        if (words[subIndex].Length <= 0)
                            break;

                        //plateIndex = Convert.ToInt32(words[subIndex]);
                        plateName = words[subIndex];
                        // 인덱스 찾기 
                        int plateIndex = -1;
                        for (int tIndex = 0; tIndex < Global.listBioGroupInfos.Count; tIndex++)
                        {
                            if (Global.listBioGroupInfos[i].name == plateName)
                            {
                                plateIndex = i;
                                break;
                            }
                        }
                        if (plateIndex >= 0)
                            this.listWellInfo.UpdateBioGroupIndex(j, i, plateName, true, Global.listBioGroupInfos[plateIndex]);

                        // 첫번째 셀의 bio Group 인덱스를 저장한다. 
                        //if (i == 0 && j == 0)
                        //   bioGroupIdx = plateIndex;
                    }
                }
            }

            plateName = Path.GetFileNameWithoutExtension(fullPath) + ".plate";
            //Global.PlatePath = fullPath.Replace(plateName, "");
            Global.PlatePath = fullPath.Substring(0, fullPath.Length - plateName.Length);
            Global.PlateFile = plateName;

            this.plateFileName = fullPath;
            textBox_plateFilePath.Text = plateName;

            this.UpdateWellTable("", "", "");
        }

        /// <summary>
        /// Plate 파일을 저장한다.  
        /// </summary>
        /// <param name="filePath">Plate 파일 이름</param>
        public void SavePlate(string filePath = "", bool isCreate = false)
        {
            if (filePath.Length <= 0)
            {
                SaveFileDialog saveFileDlg = new SaveFileDialog();
                saveFileDlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Plate\\";
                //폴더 존재유무 확인하고 없으면 폴더를 생성한다. 
                DirectoryInfo di = new DirectoryInfo(saveFileDlg.InitialDirectory);
                if (di.Exists == false)
                    di.Create();
                saveFileDlg.Filter = "Plate File(*.plate)|*.plate";
                saveFileDlg.Title = "Save an Plate File";
                if (saveFileDlg.ShowDialog() == DialogResult.OK)
                {
                    filePath = Path.GetFullPath(saveFileDlg.FileName);
                }
                else
                    return;
            }
            else
            {
                if (!isCreate)
                {
                    string errorMsg = string.Format("File nout found!\r\n{0}", filePath);
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show(new Form { TopMost = true }, errorMsg);
                        return;
                    }
                }
            }

            this.Cursor = Cursors.WaitCursor;

            //Create ini file
            FileIniDataParser parser = new FileIniDataParser();
            IniData savedData = new IniData();

            savedData.Sections.AddSection("GENERAL");
            savedData["GENERAL"]["passiveRef"] = Global.passiveRef.ToString();           // passive Reference 0:None, 1:FAM, 2:HEX, 3:ROX, 4:CY5

            savedData.Sections.AddSection("TARGET");
            savedData["TARGET"]["TARGET_COUNT"] = Global.listTargetInfos.Count.ToString();           // Stage Count 

            int i, j;
            // Target 리스트를 저장한다. 
            for (i = 0; i < Global.listTargetInfos.Count; i++)
            {
                IpPlate_Target targetInfo = Global.listTargetInfos[i];
                string subTitle = string.Format("TARGET_ROW{0}", i + 1);    // Target sub title
                string subData = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/",
                    targetInfo.check.ToString(), targetInfo.colorIndex, targetInfo.name, targetInfo.reporter,
                    targetInfo.quencher, targetInfo.comment, targetInfo.task, targetInfo.quantity);
                savedData["TARGET"][subTitle] = subData;                    // Target Row Data
            }

            savedData.Sections.AddSection("SAMPLE");
            savedData["SAMPLE"]["SAMPLE_COUNT"] = Global.listSampleInfos.Count.ToString();           // Stage Count 

            // Sample 리스트를 저장한다. 
            for (i = 0; i < Global.listSampleInfos.Count; i++)
            {
                IpPlate_Sample sampleInfo = Global.listSampleInfos[i];
                string subTitle = string.Format("SAMPLE_ROW{0}", i + 1);    // Step Name
                string subData = string.Format("{0}/{1}/{2}/{3}/",
                    sampleInfo.check.ToString(), sampleInfo.colorIndex, sampleInfo.name, sampleInfo.comment);
                savedData["SAMPLE"][subTitle] = subData;                    // Target Row Data
            }

            savedData.Sections.AddSection("BIOGROUP");
            savedData["BIOGROUP"]["BIOGROUP_COUNT"] = Global.listBioGroupInfos.Count.ToString();           // Stage Count 

            // Sample 리스트를 저장한다. 
            for (i = 0; i < Global.listBioGroupInfos.Count; i++)
            {
                IpPlate_Sample sampleInfo = Global.listBioGroupInfos[i];
                string subTitle = string.Format("BIOGROUP_ROW{0}", i + 1);    // Step Name
                string subData = string.Format("{0}/{1}/{2}/{3}/",
                    sampleInfo.check.ToString(), sampleInfo.colorIndex, sampleInfo.name, sampleInfo.comment);
                savedData["BIOGROUP"][subTitle] = subData;                    // Target Row Data
            }

            // Well 정보를 저장한다.
            savedData.Sections.AddSection("WELLS");
            int colCount = this.listWellInfo.colCount;
            int rowCount = this.listWellInfo.rowCount;
            savedData["WELLS"]["COL_COUNT"] = colCount.ToString();           // Stage Count 
            savedData["WELLS"]["ROW_COUNT"] = rowCount.ToString();           // Stage Count 

            for (i = 0; i < colCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    int index = j + (i * rowCount);

                    string subTitle = string.Format("WELL{0}_TARGET", index + 1);
                    string subData = this.listWellInfo.GetWellData(0, j, i);
                    savedData["WELLS"][subTitle] = subData;                    // Target Row Data
                    subTitle = string.Format("WELL{0}_SAMPLE", index + 1);
                    subData = this.listWellInfo.GetWellData(1, j, i);
                    savedData["WELLS"][subTitle] = subData;                    // Sample Row Data
                    subTitle = string.Format("WELL{0}_GOUP", index + 1);
                    subData = this.listWellInfo.GetWellData(2, j, i);
                    savedData["WELLS"][subTitle] = subData;                    // Bio Group Row Data
                    //subTitle = string.Format("WELL{0}_CtValue", index + 1);
                    //subData = this.listWellInfo.GetWellData(3, j, i);
                    //savedData["WELLS"][subTitle] = subData;                    // Ct Value
                }
            }

            //Save the file
            string iniPath = filePath;
            parser.WriteFile(iniPath, savedData);

            string plateFileName = Path.GetFileNameWithoutExtension(filePath) + ".plate";
            //Global.PlatePath = filePath.Replace(plateFileName, "");
            Global.PlatePath = filePath.Substring(0, filePath.Length - plateFileName.Length);
            Global.PlateFile = plateFileName;
            SaveProperties();

            this.plateFileName = filePath;
            textBox_plateFilePath.Text = plateFileName;

            if (selectResultFile.Length > 0)
            {
                Result rInfo = Global.resultManager.SearchResultFile(selectResultFile);
                if (rInfo != null)
                {
                    Global.selectedResult.PlatePath = Global.PlatePath;
                    Global.selectedResult.PlateFile = Global.PlateFile;
                    //Global.selectedResult.SetResult(rInfo);
                    Global.resultManager.UpdateResult(Global.selectedResult.GetResult());
                }
            }

            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Plate Save 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_PlateSave_Click(object sender, EventArgs e)
        {
            SavePlate(this.plateFileName);
            // 결과가 있으면 결과 엑셀 파일에 Plate 탭을 만들고 내용을 저장한다. 
            if (Global.selectedResult != null && Global.selectedResult.ResultFile.Length > 0)
            {
                string resultExcel = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
                string plateExcel = Global.selectedResult.PlatePath + Global.selectedResult.PlateFile;
                SaveExcelPlate(resultExcel, plateExcel);
            }
        }

        /// <summary>
        /// Plate SaveAs 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_PlateSaveAs_Click(object sender, EventArgs e)
        {
            SavePlate();
        }

        /// <summary>
        /// Plate New 버튼 클릭 이벤트 함수. Plate 탭을 초기화한다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_PlateNew_Click(object sender, EventArgs e)
        {
            textBox_plateFilePath.Text = "";
            selectedSampleIndex = -1;
            selectedBioGroupIndex = -1;

            this.listWellInfo.ListClear();

            dataGridView_Targets.Rows.Clear();
            Global.listTargetInfos.Clear();
            dataGridView_Samples.Rows.Clear();
            Global.listSampleInfos.Clear();
            dataGridView_BioGroup.Rows.Clear();
            Global.listBioGroupInfos.Clear();

            // 초기화할 때 Target 하나를 추가한다. 
            CustomBtn_AddTarget_Click(this, null);
            // 초기화할 때 Sample 하나를 추가한다. 
            CustomBtn_AddSample_Click(this, null);

            this.UpdateWellTable("", "", "");

            SaveProperties(true);
        }

        /// <summary>
        /// Well 테이블 마우스 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataViewImages_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int colIndex = e.ColumnIndex;
            int rowIndex = e.RowIndex;

            if (colIndex < 0 || rowIndex < 0)
                return;

            // 선택된 셀의 Target 정보를 업데이트한다. 
            int i = 0, j = 0;
            bool bFind = false;
            string subData = this.listWellInfo.GetWellData(0, colIndex, rowIndex);
            string[] words = subData.Split('/');
            int rowCount = Global.listTargetInfos.Count;
            if (rowCount > 0)
                dataGridView_Samples.BeginEdit(true);

            // Target 리스트 초기화 
            for (i = 0; i < rowCount; i++)
            {
                dataGridView_Targets.Rows[i].Cells[0].Value = false;
                //listIndex.Add(Convert.ToInt32(words[j]));

                IpPlate_Target plateInfo = Global.listTargetInfos[i];
                plateInfo.check = (bool)dataGridView_Targets.Rows[i].Cells[0].Value;
                Global.listTargetInfos[i] = plateInfo;
            }

            // 선택된 셀의 타겟 인덱스 리스트를 업데이트한다. 
            int rIndex = 0;
            int plateIndex = -1;
            for (j = 0; j < words.Length; j++)
            {
                if (words[j].Length > 0)
                {
                    //rIndex = Convert.ToInt32(words[j]);
                    string tName = words[j];
                    // 인덱스 찾기 
                    plateIndex = -1;
                    for (int tIndex = 0; tIndex < Global.listTargetInfos.Count; tIndex++)
                    {
                        if (Global.listTargetInfos[tIndex].name == tName)
                        {
                            plateIndex = tIndex;
                            break;
                        }
                    }
                    if (plateIndex >= 0)
                    {
                        rIndex = plateIndex;
                        if (rIndex < Global.listTargetInfos.Count)
                        {
                            dataGridView_Targets.Rows[rIndex].Cells[0].Value = true;
                            //listIndex.Add(Convert.ToInt32(words[j]));

                            IpPlate_Target plateInfo = Global.listTargetInfos[rIndex];
                            plateInfo.check = (bool)dataGridView_Targets.Rows[rIndex].Cells[0].Value;
                            Global.listTargetInfos[rIndex] = plateInfo;
                        }
                    }
                }
            }
            dataGridView_Targets.EndEdit();

            // 선택된 셀의 Sample 정보를 업데이트한다. 
            bFind = false;
            subData = this.listWellInfo.GetWellData(1, colIndex, rowIndex);
            //words = subData.Split('/');
            //rIndex = Convert.ToInt32(subData);
            // 인덱스 찾기 
            plateIndex = -1;
            for (int tIndex = 0; tIndex < Global.listSampleInfos.Count; tIndex++)
            {
                if (Global.listSampleInfos[tIndex].name == subData)
                {
                    plateIndex = tIndex;
                    break;
                }
            }

            rowCount = Global.listSampleInfos.Count;
            if (rowCount > 0)
                dataGridView_Samples.BeginEdit(true);

            // Sample 리스트 초기화 
            for (i = 0; i < rowCount; i++)
            {
                dataGridView_Samples.Rows[i].Cells[0].Value = false;

                IpPlate_Sample plateInfo = Global.listSampleInfos[i];
                plateInfo.check = (bool)dataGridView_Samples.Rows[i].Cells[0].Value;
                Global.listSampleInfos[i] = plateInfo;
            }

            if (plateIndex >= 0)
            {
                rIndex = plateIndex;

                // 선택된 셀의 샘플 인덱스를 업데이트한다. 
                if (rIndex >= 0 && rIndex < Global.listSampleInfos.Count)
                {
                    dataGridView_Samples.Rows[rIndex].Cells[0].Value = true;

                    IpPlate_Sample plateInfo = Global.listSampleInfos[rIndex];
                    plateInfo.check = (bool)dataGridView_Samples.Rows[rIndex].Cells[0].Value;
                    Global.listSampleInfos[rIndex] = plateInfo;
                }
            }

            dataGridView_Samples.EndEdit();

            // 선택된 셀의 Bio Group 정보를 업데이트한다. 
            bFind = false;
            subData = this.listWellInfo.GetWellData(2, colIndex, rowIndex);
            words = subData.Split('/');
            rowCount = Global.listBioGroupInfos.Count;
            if(rowCount > 0)
                dataGridView_BioGroup.BeginEdit(true);
            for (i = 0; i < rowCount; i++)
            {
                IpPlate_Sample plateInfo = Global.listBioGroupInfos[i];
                if (subData.Length > 0 && words.Length > 0)
                {
                    for (j = 0; j < words.Length; j++)
                    {
                        //if (words[j].Length > 0 && i == Convert.ToInt32(words[j]))
                        if (words[j].Length > 0 && plateInfo.name == words[j])
                        {
                            dataGridView_BioGroup.Rows[i].Cells[0].Value = true;
                            bFind = true;
                            break;
                        }
                    }
                    if (!bFind)
                        dataGridView_BioGroup.Rows[i].Cells[0].Value = false;
                }
                else
                    dataGridView_BioGroup.Rows[i].Cells[0].Value = false;

                plateInfo.check = (bool)dataGridView_BioGroup.Rows[i].Cells[0].Value;
                Global.listBioGroupInfos[i] = plateInfo;
            }
            dataGridView_BioGroup.EndEdit();
        }

        /// <summary>
        /// Well Table 화면구성 함수 
        /// </summary>
        public void ReloadWellTable()
        {
            if (dataViewImages == null)
                return;

            dataViewImages.Rows.Clear();
            dataViewImages.Columns.Clear();
            dataViewImages.RowHeadersWidth = 80; // Math.Abs(dataViewImages.Width - dataViewImages.Height);
            dataViewImages.ColumnHeadersHeight = 60;

            int _imageSize = (int)(((this.dataViewImages.Width - dataViewImages.RowHeadersWidth - 60) / 5) * zoomValue);
            int numColumnsForWidth = _imageSize; // (dataViewImages.Width - 10) / (_imageSize + 20);

            // Dynamically create the columns
            for (int index = 0; index < 5; index++)
            {
                DataGridViewImageColumn dataGridViewColumn = new DataGridViewImageColumn();

                dataViewImages.Columns.Add(dataGridViewColumn);
                dataViewImages.Columns[index].Width = _imageSize + 5;
                int headerText = index + 1;
                dataViewImages.Columns[index].HeaderText = headerText.ToString();

                headerText += 64;
                char decoded = Convert.ToChar(headerText);
                dataViewImages.Rows.Add();
                dataViewImages.Rows[index].Height = _imageSize + 5;
                dataViewImages.Rows[index].HeaderCell.Value = decoded.ToString();
            }

            this.ResizePanel_SubMemu2.Controls.Add(dataViewImages);
        }

        /// <summary>
        /// Well Table 에 이미지를 채운다. 
        /// </summary>
        public void DataFillWellTable()
        {
            if (dataViewImages.Rows.Count != 5)
                return;

            for (int colIndex = 0; colIndex < 5; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < 5; rowIndex++)
                {
                    // Load the image from the file and add to the DataGridView
                    Image imageUp = Resources.WellBk_Up;
                    Image imageDown = Resources.WellBk_Down;
                    if (checkBox_SampleColor.CheckState == CheckState.Checked)
                    {
                        imageUp = Resources.WellBkColor_Up;
                        imageDown = Resources.WellBkColor_Down;
                    }
                    //Size imgSize = new Size(dataViewImages.Rows[rowIndex].Cells[colIndex].Size)
                    Image resizeImage = null;
                    if (dataViewImages.Rows[rowIndex].Cells[colIndex].Selected)
                        resizeImage = (Image)(new Bitmap(imageDown, dataViewImages.Rows[rowIndex].Cells[colIndex].Size));
                    else
                        resizeImage = (Image)(new Bitmap(imageUp, dataViewImages.Rows[rowIndex].Cells[colIndex].Size));
                    dataViewImages.Rows[rowIndex].Cells[colIndex].Value = resizeImage;
                    dataViewImages.Rows[rowIndex].Cells[colIndex].ToolTipText = colIndex.ToString() + "," + rowIndex.ToString();
                }
            }
        }

        /// <summary>
        /// Well Table 에 정보를 업데이트한다. 
        /// </summary>
        /// <param name="targetName">Target 이름</param>
        /// <param name="sampleName">Sample 이름</param>
        /// <param name="bioGroupName">Bio Group 이름</param>
        public void UpdateWellTable(string targetName, string sampleName, string bioGroupName)
        {
            if (dataViewImages.Rows.Count != 5)
                return;

            Image imageUp = Resources.WellBk_Up;
            Image imageDown = Resources.WellBk_Down;
            if (checkBox_SampleColor.CheckState == CheckState.Checked)
            {
                imageUp = Resources.WellBkColor_Up;
                imageDown = Resources.WellBkColor_Down;
            }

            dataViewImages.BeginEdit(true);
            for (int colIndex = 0; colIndex < 5; colIndex++)
            {
                for (int rowIndex = 0; rowIndex < 5; rowIndex++)
                {
                    Image resizeImage = null;
                    // 해당 셀의 정보에 맞는 이미지를 얻어온다. 
                    Image wellImage = null;
                    if (dataViewImages.Rows[rowIndex].Cells[colIndex].Selected)
                    {
                        if (targetName.Length >= 0)
                        {
                            int targetIndex = -1;
                            for (int i = 0; i < Global.listTargetInfos.Count; i++)
                            {
                                if (Global.listTargetInfos[i].name == targetName)
                                {
                                    targetIndex = i;
                                    break;
                                }
                            }

                            if (targetIndex >= 0)
                            {
                                IpPlate_Target targetInfo = Global.listTargetInfos[targetIndex];
                                listWellInfo.UpdateTargetIndex(colIndex, rowIndex, targetName, targetInfo.check, targetInfo);
                            }
                        }
                        if (sampleName.Length >= 0)
                        {
                            int sampleIndex = -1;
                            for (int i = 0; i < Global.listSampleInfos.Count; i++)
                            {
                                if (Global.listSampleInfos[i].name == sampleName)
                                {
                                    sampleIndex = i;
                                    break;
                                }
                            }

                            if (sampleIndex >= 0)
                            {
                                IpPlate_Sample sampleInfo = Global.listSampleInfos[sampleIndex];
                                listWellInfo.UpdateSampleIndex(colIndex, rowIndex, sampleName, sampleInfo.check, sampleInfo);
                            }
                        }
                        if (bioGroupName.Length >= 0)
                        {
                            int bioIndex = -1;
                            for (int i = 0; i < Global.listBioGroupInfos.Count; i++)
                            {
                                if (Global.listBioGroupInfos[i].name == bioGroupName)
                                {
                                    bioIndex = i;
                                    break;
                                }
                            }

                            if (bioIndex >= 0)
                            {
                                IpPlate_Sample bioGroupInfo = Global.listBioGroupInfos[bioIndex];
                                listWellInfo.UpdateBioGroupIndex(colIndex, rowIndex, bioGroupName, bioGroupInfo.check, bioGroupInfo);
                            }
                        }

                        wellImage = listWellInfo.GetWellImage(colIndex, rowIndex, dataViewImages.Rows[rowIndex].Cells[colIndex].Size.Width, dataViewImages.Rows[rowIndex].Cells[colIndex].Size.Height);
                        if (wellImage != null)
                            resizeImage = GetTransBitmap(wellImage, 0.5f);
                        else
                            resizeImage = (Image)(new Bitmap(imageDown, dataViewImages.Rows[rowIndex].Cells[colIndex].Size));
                    }
                    else
                    {
                        wellImage = listWellInfo.GetWellImage(colIndex, rowIndex, dataViewImages.Rows[rowIndex].Cells[colIndex].Size.Width, dataViewImages.Rows[rowIndex].Cells[colIndex].Size.Height);
                        if (wellImage != null)
                            resizeImage = wellImage;
                        else
                            resizeImage = (Image)(new Bitmap(imageUp, dataViewImages.Rows[rowIndex].Cells[colIndex].Size));
                    }
                    dataViewImages.Rows[rowIndex].Cells[colIndex].Value = resizeImage;
                    //dataViewImages.Rows[2].Cells[2].Value = resizeImage;

                    int index = colIndex + (rowIndex * 5);
                    bool isSelect = dataViewImages.Rows[rowIndex].Cells[colIndex].Selected;
                    listWellInfo.SelectWell(index, isSelect);
                }
            }

            //dataViewImages.Update();
            //dataViewImages.Refresh();
            //dataViewImages.Invalidate();
            dataViewImages.EndEdit();

            ShowResultGraph();
        }

        /// <summary>
        /// Well 테이블의 정보가 변경될때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataViewImages_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataViewImages.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>
        /// Well 테이블 Painting 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataViewImages_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            int col = e.ColumnIndex;
            int row = e.RowIndex;
            int index = row + (col * listWellInfo.rowCount);
            if (index < 0 || index >= 25)
                return;
        }

        /// <summary>
        /// Run 탭의 Start 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartRun_Click(object sender, EventArgs e)
        {
            if (textBox_filePath.Text.Length <= 0)
                return;

            // plate 탭에서 샘플 및 target이 1개도 되지 않고 run을 시도하면 물어보고 진행 
            bool selectPlate = false;
            for (int i = 0; i < listWellInfo.listPlateInfos.Count; i++)
            {
                if (listWellInfo.listPlateInfos[i].listTargetInfos.Count > 0 && listWellInfo.listPlateInfos[i].SampleInfos.Length > 0)
                {
                    selectPlate = true;
                    break;
                }
            }
            if (!selectPlate)
            {
                if (DialogResult.No == MessageBox.Show("Plate setting is empty. Are you sure to start?\r\n(Recovery function available after experiment)", this.Text, MessageBoxButtons.YesNo))
                    return;
            }

            Logger("System checking... Please wait 10 seconds.");

            // Start 가 실행되면 다른 버튼들은 비활성화한다. 
            btnResultOpen.Enabled = false;

            //TabControl_Selected.Enabled = false;
            this.btnStartRun.Enabled = false;
            customBtnToday.Enabled = false;
            customBtnSearch.Enabled = false;
            customBtnAll.Enabled = false;
            customBtnLoad.Enabled = false;
            customBtnDelete.Enabled = false;
            customBtn_Previous.Enabled = false;
            customBtn_Next.Enabled = false;
            materialListView_Result.Enabled = false;

            dataGridView_Targets.Enabled = false;
            dataGridView_Samples.Enabled = false;
            dataGridView_BioGroup.Enabled = false;
            CustomBtn_AddTarget.Enabled = false;
            CustomBtn_AddSample.Enabled = false;
            CustomBtn_AddBioGroup.Enabled = false;
            customBtn_PlateNew.Enabled = false;
            customBtn_PlateLoad.Enabled = false;
            customBtn_PlateSave.Enabled = false;
            customBtn_PlateSaveAs.Enabled = false;

            btn_NewMethod.Enabled = false;
            btn_LoadMethod.Enabled = false;
            btn_SaveMethod.Enabled = false;
            customBtn_SaveAsMethod.Enabled = false;
            //doubleBufferPanel_Method.Enabled = false;
            StageEnable(false);
            customBtn_scrollLeft.Enabled = false;
            customBtn_scrollRight.Enabled = false;

            // Accel, Speed, Filter 위치 등을 초기화한다. 
            InitArducam();
            InitRunGraph(0, totalCycle + 1, 0, 260);
            InitResultGraph(0, totalCycle + 1, 0, 260);

            // Camera Control 클래스 생성
            if (CamStart())        // Camera 영상캡처 시작
            {
                this.IsCameraScan = true;
            }

            // Run 전에 아래의 순서로 실행한다. (고장 발생 방지)
            // Heater Up - Tray In - Filter Home - Filter Down 
            //btnHeaterUp_Click(this, null);
            //Thread.Sleep(300);
            //btnTrayIn_Click(this, null);
            //Thread.Sleep(300);
            MoveFilter(0);
            Thread.Sleep(300);
            btnHeaterDown_Click(this, null);
            Thread.Sleep(300);

            // 카메라 설정값들을 적용한다. 
            ApplyCamera();

            //if (!this.IsCameraScan)
            {
                string resultPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Result");

                // 저장할 폴더가 없으면 생성
                DirectoryInfo di = new DirectoryInfo(resultPath);
                if (di.Exists == false) di.Create();

                if (textBox_filePath.Text.Length > 30)
                {
                    MessageBox.Show(new Form { TopMost = true }, "Method or Result file name is too long !");
                    btnStopRun_Click(this, null);
                    return;
                }

                // Method 이름 + 날짜 폴더 생성 
                string fileName = textBox_Name.Text + "_" + textBox_filePath.Text.Substring(0, textBox_filePath.Text.Length - 7);
                methodResultPath = resultPath + "\\" + fileName + DateTime.Now.ToLocalTime().ToString("_yyyyMMddHHmmss");
                di = new DirectoryInfo(methodResultPath);
                di.Create();

                //DateTime dt = DateTime.Now;
                //excelFileName = string.Format("{1}_{2:yyyy}{3:MM}{4:dd}{5:HH}{6:mm}{7:ss}.xlsx", textBox_filePath.Text, dt, dt, dt, dt, dt, dt);
                excelFileName = fileName + DateTime.Now.ToLocalTime().ToString("_yyyyMMddHHmmss") + ".xlsx"; // 생성날짜 추가 -> 누를때 마다 생성

                //Global.ResultPath = di.FullName + "\\";
                Global.ResultPath = methodResultPath + "\\";
                Global.ResultFile = excelFileName;
                Global.ResultDateTime = DateTime.Now;
                textBox_resultFilePath.Text = Global.ResultFile;

                SaveProperties();

                if (Global.selectedResult != null)
                {
                    string saveFile = "";
                    string resultFile = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
                    // 엑셀 파일에 Properties 탭을 생성하고 내용을 저장한다. 
                    SaveExcelProperties(resultFile);

                    // 엑셀 파일에 Method 탭을 생성하고 내용을 저장한다. 
                    if (Global.selectedResult.MethodFile.Length > 0)
                    {
                        saveFile = Global.selectedResult.MethodPath + Global.selectedResult.MethodFile;
                        SaveExcelMethod(resultFile, saveFile);
                    }
                    // 엑셀 파일에 Method 탭을 생성하고 내용을 저장한다. 
                    if (Global.selectedResult.PlateFile.Length > 0)
                    {
                        saveFile = Global.selectedResult.PlatePath + Global.selectedResult.PlateFile;
                        SaveExcelPlate(resultFile, saveFile);
                    }

                    // DB 저장 
                    // 결과를 입력한다. 성공하면 Result ID를 반환한다.
                    //Global.selectedResult.RID = Global.resultManager.InsertResult(Global.selectedResult.GetResult());
                }

                errorCheckTime = 0;     // 10초 동안 Cover 온도가 올라가지 않으면 Stop 명령을 실행하고 에러 메시지를 표시한다. 
                errorCheckCorverTemp = 0.0;
                errorCheckInitTemp = 0.0;
                isPcrStateNormal = false;

                totalTime = 0;
                isProgressInit = false;
                oldActiveNo = -1;
                bSelectEnable = false;
                timerCount = 0;
                captureCount = 0;
                Global.PCR_Manager.PCR_Run();
            }
        }

        /// <summary>
        /// Run 탭의 Stop 버튼 클릭 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopRun_Click(object sender, EventArgs e)
        {
            // 강제 정지하면 결과를 저장하지 않는다. 
            textBox_resultFilePath.Text = "";
            Global.ResultPath = "";
            Global.ResultFile = "";

            StopRun();
        }

        /// <summary>
        /// PCR Run 상태를 정지한다. 
        /// </summary>
        private void StopRun()
        {
            CloseCurrentVideoSource();

            this.IsCameraScan = false;

            Global.PCR_Manager.PCR_Stop();

            if (textBox_filePath.Text.Length > 0)
            {
                // COM Port 가 비정상이거나 카메라가 비정상이면 Run 버튼을 비활성화 한다. 
                if (!Global.ArducamSerial.IsOpen || !IsCameraVisible)
                {
                    btnStartRun.Enabled = false;
                }
                else
                {
                    btnStartRun.Enabled = true;
                }
            }

            // 비활성화되었던 메뉴들을 활성화한다.
            btnResultOpen.Enabled = true;

            //TabControl_Selected.Enabled = true;
            customBtnToday.Enabled = true;
            customBtnSearch.Enabled = true;
            customBtnAll.Enabled = true;
            customBtnLoad.Enabled = true;
            customBtnDelete.Enabled = true;
            customBtn_Previous.Enabled = true;
            customBtn_Next.Enabled = true;
            materialListView_Result.Enabled = true;

            dataGridView_Targets.Enabled = true;
            dataGridView_Samples.Enabled = true;
            dataGridView_BioGroup.Enabled = true;
            CustomBtn_AddTarget.Enabled = true;
            CustomBtn_AddSample.Enabled = true;
            CustomBtn_AddBioGroup.Enabled = true;
            customBtn_PlateNew.Enabled = true;
            customBtn_PlateLoad.Enabled = true;
            customBtn_PlateSave.Enabled = true;
            customBtn_PlateSaveAs.Enabled = true;

            btn_NewMethod.Enabled = true;
            btn_LoadMethod.Enabled = true;
            btn_SaveMethod.Enabled = true;
            customBtn_SaveAsMethod.Enabled = true;
            //doubleBufferPanel_Method.Enabled = true;
            StageEnable(true);
            customBtn_scrollLeft.Enabled = true;
            customBtn_scrollRight.Enabled = true;

            // 정상 종료되면 Ct값 업데이트 및 결과를 로드한다. 
            if (Global.selectedResult.ResultFile != null && Global.selectedResult.ResultFile.Length > 0)
            {
                string resultPath = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
                LoadResult(resultPath);
            }
        }

        /// <summary>
        /// Next 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_Next_Click(object sender, EventArgs e)
        {
            if (TabControl_Selected.SelectedIndex >= (TabControl_Selected.TabCount - 1))
            {
                return;
            }

            TabControl_Selected.SelectedIndex++;
            customBtn_Previous.Visible = true;
        }

        /// <summary>
        /// Previous 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_Previous_Click(object sender, EventArgs e)
        {
            if (TabControl_Selected.SelectedIndex <= 0)
                return;

            TabControl_Selected.SelectedIndex--;
            customBtn_Next.Visible = true;
        }

        /// <summary>
        /// Method 탭의 Scroll Right 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_scrollRight_Click(object sender, EventArgs e)
        {
            int curPos = doubleBufferPanel_Method.HorizontalScroll.Value;
            if (curPos < doubleBufferPanel_Method.HorizontalScroll.Maximum)
                doubleBufferPanel_Method.HorizontalScroll.Value += 50;
            else
                doubleBufferPanel_Method.HorizontalScroll.Value = doubleBufferPanel_Method.HorizontalScroll.Maximum;
        }

        /// <summary>
        /// Method 탭의 Scroll Left 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_scrollLeft_Click(object sender, EventArgs e)
        {
            int curPos = doubleBufferPanel_Method.HorizontalScroll.Value;
            if (curPos > 50)
                doubleBufferPanel_Method.HorizontalScroll.Value -= 50;
            else
                doubleBufferPanel_Method.HorizontalScroll.Value = 0;
        }

        /// <summary>
        /// 탭 컨트롤 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_Selected_Click(object sender, EventArgs e)
        {
            int tabIndex = TabControl_Selected.SelectedIndex;
        }

        /// <summary>
        /// Emergency 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEmergency_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_EMERGENCY, 0);
            this.Logger(log);
        }

        /// <summary>
        /// Tray In 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrayIn_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            // 트레이를 꺼내기 전에 Lid Heater 의 상태를 원위치로 이동한다. (고장 발생 방지)
            btnHeaterUp_Click(this, null);
            Thread.Sleep(300);
            MoveFilter(0);
            Thread.Sleep(300);

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_IN);
            this.Logger(log);
            trayState = (int)COMMAND_VALUE.TRAY_IN;
        }

        /// <summary>
        /// Tray Out 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrayOut_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            // 트레이를 꺼내기 전에 Lid Heater 의 상태를 원위치로 이동한다. (고장 발생 방지)
            btnHeaterUp_Click(this, null);
            Thread.Sleep(300);
            MoveFilter(0);
            Thread.Sleep(300);

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.TRAY_INOUT, (int)COMMAND_VALUE.TRAY_OUT);
            this.Logger(log);
            trayState = (int)COMMAND_VALUE.TRAY_OUT;
        }

        /// <summary>
        /// Heater Up 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHeaterUp_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_UP);
            this.Logger(log);
            lidHeaterState = (int)COMMAND_VALUE.HEATER_UP;
        }

        /// <summary>
        /// Heater Down 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHeaterDown_Click(object sender, EventArgs e)
        {
            if (Global.ArducamSerial == null) return;

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.HEATER_UPDOWN, (int)COMMAND_VALUE.HEATER_DOWN);
            this.Logger(log);
            lidHeaterState = (int)COMMAND_VALUE.HEATER_DOWN;
        }

        /// <summary>
        /// Optic 제어 초기화 함수 
        /// </summary>
        private void InitArducam()
        {
            if (Global.ArducamSerial == null) return;

            Cursor.Current = Cursors.WaitCursor;         // 똥글뱅이 돌아가는 동그라미 커서

            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_COARSE_SPEED, (int)COMMAND_VALUE.STEP_COARSE_SPEED);
            this.Logger(log);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_FINE_SPEED, (int)COMMAND_VALUE.STEP_FINE_SPEED);
            this.Logger(log);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MAX_SPEED, (int)COMMAND_VALUE.STEP_MAX_SPEED);
            this.Logger(log);
            log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_ACCEL, (int)COMMAND_VALUE.STEP_ACCEL);
            this.Logger(log);      

            Cursor.Current = Cursors.Default;         // 똥글뱅이 돌아가는 동그라미 커서
        }

        /// <summary>
        /// 필터의 위치를 이동한다. 
        /// </summary>
        /// <param name="nPos">FAM : 25, HEX : 300, ROX : 600, CY5 : 900</param>
        private void MoveFilter(int nPos)
        {
            if (Global.ArducamSerial == null) return;

            if (nPos == 0)
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_HOMING, nPos);
            else
                log = Global.ArducamSerial.ArduinoCommand((int)ARDUINO_COMMAND.STEP_MOVING, nPos);
            this.Logger(log);
            filterPos = nPos;
        }

        /// <summary>
        /// 로그창의 내용을 모두 지운다. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void customBtn_LogClear_Click(object sender, EventArgs e)
        {
            ListBox_Msg.Items.Clear();
        }

        /// <summary>
        /// Log 처리 함수 
        /// </summary>
        /// <param name="msg">Log Message</param>
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

        /// <summary>
        /// LED On 상태 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// FAM Check Box 상태 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkFAM_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFAM.Checked)
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

        /// <summary>
        /// HEX Check Box 상태 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// ROX Check Box 상태 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// CY5 Check Box 상태 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Data Interpolation 함수 
        /// </summary>
        /// <param name="input">Input Data</param>
        /// <param name="factor">Interpolation factor</param>
        /// <returns>Interpolation 결과</returns>
        public static double[] Interpolate(double[] input, int factor)
        {
            double[] inputData = new double[input.Length];

            //Median filter 
            MedianFilter filter = new MedianFilter();
            int window3 = Global.medianFilterWindow; // * (int)Global.baseAvgScale;
            inputData = filter.GetMedianFiltration(input, window3);

            int inputLength = input.Length;
            int outputLength = inputLength * factor;

            double[] output = new double[outputLength];

            for (int i = 0; i < inputLength; i++)
            {
                output[i * factor] = inputData[i];

                // Perform linear interpolation between the input samples
                for (int j = 1; j < factor; j++)
                {
                    float t = (float)j / factor;
                    Vector3 v0 = new Vector3((float)inputData[i]);
                    Vector3 v1 = new Vector3((float)inputData[Math.Min(i + 1, inputLength - 1)]);
                    Vector3 v = Vector3.Lerp(v0, v1, t);
                    //v.CopyTo((float)output, i * factor + j);
                    output[i * factor + j] = v.Y;
                }
            }

            return output;
        }

        /// <summary>
        /// Interpolation 그래프를 생성하고 디스플레이한다. 
        /// </summary>
        /// <param name="scaleX">Scale X</param>
        /// <param name="scaleY">Scale Y</param>
        public void InterpolationGraph(int scaleX, double scaleY)
        {
            int wellCount = listValuesFAM.Count();
            for (int i = 0; i < wellCount; i++)
            {
                if (listInterpolateFAM[i] == null)
                    listInterpolateFAM[i] = new List<double>();
                listInterpolateFAM[i].Clear();

                if (listInterpolateHEX[i] == null)
                    listInterpolateHEX[i] = new List<double>();
                listInterpolateHEX[i].Clear();

                if (listInterpolateROX[i] == null)
                    listInterpolateROX[i] = new List<double>();
                listInterpolateROX[i].Clear();

                if (listInterpolateCY5[i] == null)
                    listInterpolateCY5[i] = new List<double>();
                listInterpolateCY5[i].Clear();

                listResultPointsFAM[i].Clear();
                listResultPointsHEX[i].Clear();
                listResultPointsROX[i].Clear();
                listResultPointsCY5[i].Clear();

                if (scaleX == 1.0)
                {
                    listInterpolateFAM[i].AddRange(listValuesFAM[i].ToArray());
                    listInterpolateHEX[i].AddRange(listValuesHEX[i].ToArray());
                    listInterpolateROX[i].AddRange(listValuesROX[i].ToArray());
                    listInterpolateCY5[i].AddRange(listValuesCY5[i].ToArray());

                    for (int j = 0; j < listValuesFAM[i].Count(); j++)
                    {
                        listResultPointsFAM[i].Add(new ZedGraph.PointPair(j, listValuesFAM[i][j]));
                        listResultPointsHEX[i].Add(new ZedGraph.PointPair(j, listValuesHEX[i][j]));
                        listResultPointsROX[i].Add(new ZedGraph.PointPair(j, listValuesROX[i][j]));
                        listResultPointsCY5[i].Add(new ZedGraph.PointPair(j, listValuesCY5[i][j]));
                    }
                }
                else
                {
                    int distance = (int)Global.graphInterpolationScale;
                    double[] resultFAM = Interpolate(listValuesFAM[i].ToArray(), distance);
                    double[] resultHEX = Interpolate(listValuesHEX[i].ToArray(), distance);
                    double[] resultROX = Interpolate(listValuesROX[i].ToArray(), distance);
                    double[] resultCY5 = Interpolate(listValuesCY5[i].ToArray(), distance);

                    listInterpolateFAM[i].AddRange(resultFAM);
                    listInterpolateHEX[i].AddRange(resultHEX);
                    listInterpolateROX[i].AddRange(resultROX);
                    listInterpolateCY5[i].AddRange(resultCY5);

                    for (int j = 0; j < resultFAM.Count(); j++)
                    {
                        listResultPointsFAM[i].Add(new ZedGraph.PointPair(j, resultFAM[j]));
                        listResultPointsHEX[i].Add(new ZedGraph.PointPair(j, resultHEX[j]));
                        listResultPointsROX[i].Add(new ZedGraph.PointPair(j, resultROX[j]));
                        listResultPointsCY5[i].Add(new ZedGraph.PointPair(j, resultCY5[j]));

                        listPointsFAM[i].Add(new ZedGraph.PointPair(j, resultFAM[j]));
                        listPointsHEX[i].Add(new ZedGraph.PointPair(j, resultHEX[j]));
                        listPointsROX[i].Add(new ZedGraph.PointPair(j, resultROX[j]));
                        listPointsCY5[i].Add(new ZedGraph.PointPair(j, resultCY5[j]));
                    }
                }
            }

            PanePulseWave.XAxis.Scale.MaxAuto = true;
            PanePulseWave.XAxis.Scale.MinAuto = true;
            PanePulseWave.XAxis.Scale.MinorStepAuto = true;
            PanePulseWave.XAxis.Scale.MajorStepAuto = true;

            this.ZedGraph_Pulse.AxisChange();
            this.ZedGraph_Pulse.Refresh();

            PaneResultWave.XAxis.Scale.MaxAuto = true;
            PaneResultWave.XAxis.Scale.MinAuto = true;
            PaneResultWave.XAxis.Scale.MinorStepAuto = true;
            PaneResultWave.XAxis.Scale.MajorStepAuto = true;

            this.zedGraph_Result.AxisChange();
            this.zedGraph_Result.Refresh();
        }

        /// <summary>
        /// 원본 데이터 그래프를 생성하고 표시한다. 
        /// </summary>
        private void curveFittingRawData()
        {
            int wellCount = listInterpolateFAM.Count();
            for (int i = 0; i < wellCount; i++)
            {
                double fs = Global.graphSampleRate; //sampling rate 
                double[] dataFAM = listInterpolateFAM[i].ToArray();
                double[] dataHEX = listInterpolateHEX[i].ToArray();
                double[] dataROX = listInterpolateROX[i].ToArray();
                double[] dataCY5 = listInterpolateCY5[i].ToArray();

                double[] resultFAM = null;
                double[] resultHEX = null;
                double[] resultROX = null;
                double[] resultCY5 = null;

                if (Global.IsLowPassFilter)
                {
                    MedianFilter filter = new MedianFilter();
                    int window3 = Global.medianFilterWindow; // * (int)Global.baseAvgScale;
                    resultFAM = filter.GetMedianFiltration(dataFAM, window3);
                    resultHEX = filter.GetMedianFiltration(dataHEX, window3);
                    resultROX = filter.GetMedianFiltration(dataROX, window3);
                    resultCY5 = filter.GetMedianFiltration(dataCY5, window3);
                }
                else
                {
                    resultFAM = dataFAM;
                    resultHEX = dataHEX;
                    resultROX = dataROX;
                    resultCY5 = dataCY5;
                }

                int rowCount = listInterpolateFAM[i].Count;
                for (int j = 0; j < rowCount; j++)
                {
                    double passiveRefValue = 1.0;
                    if (Global.passiveRef == 1)
                        passiveRefValue = resultFAM[j];
                    else if (Global.passiveRef == 2)
                        passiveRefValue = resultHEX[j];
                    else if (Global.passiveRef == 3)
                        passiveRefValue = resultROX[j];
                    else if (Global.passiveRef == 4)
                        passiveRefValue = resultCY5[j];

                    passiveRefValue += 1.0;
   
                    listResultPointsFAM[i][j].Y = resultFAM[j] / passiveRefValue; // * Global.graphYscale;
                    listResultPointsHEX[i][j].Y = resultHEX[j] / passiveRefValue; // * Global.graphYscale;
                    listResultPointsROX[i][j].Y = resultROX[j] / passiveRefValue; // * Global.graphYscale;
                    listResultPointsCY5[i][j].Y = resultCY5[j] / passiveRefValue; // * Global.graphYscale;
                }
            }

            PaneResultWave.YAxis.Scale.MaxAuto = true;
            PaneResultWave.YAxis.Scale.MinAuto = true;
            PaneResultWave.YAxis.Scale.MinorStepAuto = true;
            PaneResultWave.YAxis.Scale.MajorStepAuto = true;

            // Calculate the Axis Scale Ranges
            this.zedGraph_Result.AxisChange();
            this.zedGraph_Result.Refresh();
        }

        /// <summary>
        /// Base Line 그래프를 생성하고 표시한다. 
        /// </summary>
        private void curveFittingBaseLine()
        {
            List<double>[] listFAM = new List<double>[25];
            List<double>[] listHEX = new List<double>[25];
            List<double>[] listROX = new List<double>[25];
            List<double>[] listCY5 = new List<double>[25];

            int i, j;
            int rowCount = 0;
            double valueFAM, valueROX, valueHEX, valueCY5;
            int wellCount = listInterpolateFAM.Count();
            for (i = 0; i < wellCount; i++)
            {
                listFAM[i] = new List<double>();
                listHEX[i] = new List<double>();
                listROX[i] = new List<double>();
                listCY5[i] = new List<double>();

                double[] tempFAM = listInterpolateFAM[i].ToArray();
                double[] tempHEX = listInterpolateHEX[i].ToArray();
                double[] tempROX = listInterpolateROX[i].ToArray();
                double[] tempCY5 = listInterpolateCY5[i].ToArray();

                // Passive Reference 가 선택된 경우 
                if (Global.passiveRef > 0)
                {
                    rowCount = tempFAM.Count();
                    double Threshold = Global.graphThreshold;
                    for (j = 0; j < rowCount; j++)
                    {
                        double passiveRefValue = 1.0;
                        if (Global.passiveRef == 1)
                            passiveRefValue = tempFAM[j];
                        else if (Global.passiveRef == 2)
                            passiveRefValue = tempHEX[j];
                        else if (Global.passiveRef == 3)
                            passiveRefValue = tempROX[j];
                        else if (Global.passiveRef == 4)
                            passiveRefValue = tempCY5[j];

                        passiveRefValue += 1.0;

                        valueFAM = tempFAM[j] / passiveRefValue;
                        valueHEX = tempHEX[j] / passiveRefValue;
                        valueROX = tempROX[j] / passiveRefValue;
                        valueCY5 = tempCY5[j] / passiveRefValue;

                        listFAM[i].Add(valueFAM);
                        listHEX[i].Add(valueHEX);
                        listROX[i].Add(valueROX);
                        listCY5[i].Add(valueCY5);
                    }
                }
                else
                {
                    listFAM[i].AddRange(tempFAM);
                    listHEX[i].AddRange(tempHEX);
                    listROX[i].AddRange(tempROX);
                    listCY5[i].AddRange(tempCY5);
                }
            }

            // 노이즈 구간의 평균값을 구한다. 
            List<double> listAvgFAM = new List<double>();
            List<double> listAvgHEX = new List<double>();
            List<double> listAvgROX = new List<double>();
            List<double> listAvgCY5 = new List<double>();
            for (i = 0; i < wellCount; i++)
            {
                int startX = (int)(Global.baselineStart * Global.graphInterpolationScale);
                int endX = (int)(Global.baselineEnd * Global.graphInterpolationScale);
                // 그래프 개수가 베이스라인 평균 개수보다 작으면 그래프 개수의 전체 길이의 7%~40%로 평균한다. 
                if (listFAM[0].Count < endX)
                {
                    startX = (int)(listFAM[0].Count * 0.07);
                    endX = (int)(listFAM[0].Count * 0.4);
                }

                List<double> partFAM = listFAM[i].GetRange(startX, endX);
                double averageFAM = partFAM.Average();
                List<double> partHEX = listHEX[i].GetRange(startX, endX);
                double averageHEX = partHEX.Average();
                List<double> partROX = listROX[i].GetRange(startX, endX);
                double averageROX = partROX.Average();
                List<double> partCY5 = listCY5[i].GetRange(startX, endX);
                double averageCY5 = partCY5.Average();

                listAvgFAM.Add(averageFAM);
                listAvgHEX.Add(averageHEX);
                listAvgROX.Add(averageROX);
                listAvgCY5.Add(averageCY5);
            }

            // Raw 데이터에서 평균값을 뺀다. 
            List<double>[] resultFAM = new List<double>[25];
            List<double>[] resultHEX = new List<double>[25];
            List<double>[] resultROX = new List<double>[25];
            List<double>[] resultCY5 = new List<double>[25];
            for (i = 0; i < wellCount; i++)
            {
                resultFAM[i] = new List<double>();
                resultHEX[i] = new List<double>();
                resultROX[i] = new List<double>();
                resultCY5[i] = new List<double>();

                double averageFAM = listAvgFAM[i];
                double averageHEX = listAvgHEX[i];
                double averageROX = listAvgROX[i];
                double averageCY5 = listAvgCY5[i];

                rowCount = listFAM[i].Count;
                for (j = 0; j < rowCount; j++)
                {
                    valueFAM = (listFAM[i][j]) - averageFAM;
                    if (valueFAM < 0) valueFAM = 0;
                    resultFAM[i].Add(valueFAM);

                    valueHEX = (listHEX[i][j]) - averageHEX;
                    if (valueHEX < 0) valueHEX = 0;
                    resultHEX[i].Add(valueHEX);

                    valueROX = (listROX[i][j]) - averageROX;
                    if (valueROX < 0) valueROX = 0;
                    resultROX[i].Add(valueROX);

                    valueCY5 = (listCY5[i][j]) - averageCY5;
                    if (valueCY5 < 0) valueCY5 = 0;
                    resultCY5[i].Add(valueCY5);
                }
            }

            // 결과 데이터를 그래프에 추가한다.  
            for (i = 0; i < wellCount; i++)
            {
                rowCount = resultFAM[i].Count;
                for (j = 0; j < rowCount; j++)
                {
                    listResultPointsFAM[i][j].Y = resultFAM[i][j];
                    listResultPointsHEX[i][j].Y = resultHEX[i][j];
                    listResultPointsROX[i][j].Y = resultROX[i][j];
                    listResultPointsCY5[i][j].Y = resultCY5[i][j];
                }
            }

            // Threshold 라인을 그린다. 
            for (j = 0; j < rowCount; j++)
            {
                if (listThPointsFAM.Count > 0) listThPointsFAM[j].Y = Math.Pow(10, resultThresholdFAM);
                if (listThPointsHEX.Count > 0) listThPointsHEX[j].Y = Math.Pow(10, resultThresholdHEX);
                if (listThPointsROX.Count > 0) listThPointsROX[j].Y = Math.Pow(10, resultThresholdROX);
                if (listThPointsCY5.Count > 0) listThPointsCY5[j].Y = Math.Pow(10, resultThresholdCY5);
            }

            PaneResultWave.YAxis.Scale.MaxAuto = true;
            PaneResultWave.YAxis.Scale.MinAuto = true;
            PaneResultWave.YAxis.Scale.MinorStepAuto = true;
            PaneResultWave.YAxis.Scale.MajorStepAuto = true;

            // Calculate the Axis Scale Ranges
            this.zedGraph_Result.AxisChange();
            this.zedGraph_Result.Refresh();
            //this.zedGraph_Result.Invalidate();
        }

        /// <summary>
        /// 모든 웰의 Ct 값을 생성한다.  
        /// </summary>
        private void curveCalibration()
        {
            if (listInterpolateFAM[0] == null)
                return;

            List<double>[] listFAM = new List<double>[25];
            List<double>[] listHEX = new List<double>[25];
            List<double>[] listROX = new List<double>[25];
            List<double>[] listCY5 = new List<double>[25];

            int i, j;
            int rowCount = listInterpolateFAM[0].Count;
            double valueFAM, valueROX, valueHEX, valueCY5;
            Array.Clear(isFAM, 0, isFAM.Length);
            Array.Clear(isHEX, 0, isHEX.Length);
            Array.Clear(isROX, 0, isROX.Length);
            Array.Clear(isCY5, 0, isCY5.Length);

            int wellCount = listInterpolateFAM.Count();
            for (i = 0; i < wellCount; i++)
            {
                //Well_Info wellInfo = listWellInfo.GetWellInfo(i);
                int indexFAM = listWellInfo.FindTargetIndex(i, 0);
                int indexHEX = listWellInfo.FindTargetIndex(i, 1);
                int indexROX = listWellInfo.FindTargetIndex(i, 2);
                int indexCY5 = listWellInfo.FindTargetIndex(i, 3);
                if (indexFAM >= 0) isFAM[i] = true;
                else isFAM[i] = false;
                if (indexHEX >= 0) isHEX[i] = true;
                else isHEX[i] = false;
                if (indexROX >= 0) isROX[i] = true;
                else isROX[i] = false;
                if (indexCY5 >= 0) isCY5[i] = true;
                else isCY5[i] = false;

                //if (!isFAM[i] && !isHEX[i] && !isROX[i] && !isCY5[i])
                //    continue;

                double[] resultFAM = null;
                double[] resultHEX = null;
                double[] resultROX = null;
                double[] resultCY5 = null;
                // 모든 값의 로그를 구한다. 
                //if (isFAM[i])
                {
                    listFAM[i] = new List<double>();
                    resultFAM = listInterpolateFAM[i].Select(value => Math.Log10(value)).ToArray();
                }
                //if (isHEX[i])
                {
                    listHEX[i] = new List<double>();
                    resultHEX = listInterpolateHEX[i].Select(value => Math.Log10(value)).ToArray();
                }
                //if (isROX[i])
                {
                    listROX[i] = new List<double>();
                    resultROX = listInterpolateROX[i].Select(value => Math.Log10(value)).ToArray();
                }
                //if (isCY5[i])
                {
                    listCY5[i] = new List<double>();
                    resultCY5 = listInterpolateCY5[i].Select(value => Math.Log10(value)).ToArray();
                }

                // Passive Reference 가 선택된 경우 
                if (Global.passiveRef > 0)
                {   
                    double Threshold = Global.graphThreshold;
                    for (j = 0; j < rowCount; j++)
                    {
                        double passiveRefValue = 1.0;
                        if (resultFAM != null && Global.passiveRef == 1)
                            passiveRefValue = resultFAM[j];
                        else if (resultHEX != null && Global.passiveRef == 2)
                            passiveRefValue = resultHEX[j];
                        else if (resultROX != null && Global.passiveRef == 3)
                            passiveRefValue = resultROX[j];
                        else if (resultCY5 != null && Global.passiveRef == 4)
                            passiveRefValue = resultCY5[j];

                        passiveRefValue += 1.0;
                        Math.Log10(passiveRefValue);

                        if (resultFAM != null)
                        {
                            valueFAM = resultFAM[j] / passiveRefValue;
                            listFAM[i].Add(valueFAM);
                        }
                        if (resultHEX != null)
                        {
                            valueHEX = resultHEX[j] / passiveRefValue;
                            listHEX[i].Add(valueHEX);
                        }
                        if (resultROX != null)
                        {
                            valueROX = resultROX[j] / passiveRefValue;
                            listROX[i].Add(valueROX);
                        }
                        if (resultCY5 != null)
                        {
                            valueCY5 = resultCY5[j] / passiveRefValue;
                            listCY5[i].Add(valueCY5);
                        }
                    }
                }
                else
                {
                    if (resultFAM != null) listFAM[i].AddRange(resultFAM);
                    if (resultHEX != null) listHEX[i].AddRange(resultHEX);
                    if (resultROX != null) listROX[i].AddRange(resultROX);
                    if (resultCY5 != null) listCY5[i].AddRange(resultCY5);
                }
            }

            // 노이즈 구간의 평균값을 구한다. 
            List<double> listAvgFAM = new List<double>();
            List<double> listAvgHEX = new List<double>();
            List<double> listAvgROX = new List<double>();
            List<double> listAvgCY5 = new List<double>();
            for (i = 0; i < wellCount; i++)
            {
                int startX = (int)(Global.baselineStart * Global.graphInterpolationScale);
                int endX = (int)(Global.baselineEnd * Global.graphInterpolationScale);
                // 그래프 개수가 베이스라인 평균 개수보다 작으면 그래프 개수의 전체 길이의 7%~40%로 평균한다. 
                if (listFAM[0].Count < endX)
                {
                    startX = (int)(listFAM[0].Count * 0.07);
                    endX = (int)(listFAM[0].Count * 0.4);
                }

                double averageFAM = 0.0;
                double averageHEX = 0.0;
                double averageROX = 0.0;
                double averageCY5 = 0.0;

                //if (isFAM[i])
                {
                    List<double> partFAM = listFAM[i].GetRange(startX, endX);
                    averageFAM = partFAM.Average();
                    listAvgFAM.Add(averageFAM);
                }
                //if (isHEX[i])
                {
                    List<double> partHEX = listHEX[i].GetRange(startX, endX);
                    averageHEX = partHEX.Average();
                    listAvgHEX.Add(averageHEX);
                }
                //if (isROX[i])
                {
                    List<double> partROX = listROX[i].GetRange(startX, endX);
                    averageROX = partROX.Average();
                    listAvgROX.Add(averageROX);
                }
                //if (isCY5[i])
                {
                    List<double> partCY5 = listCY5[i].GetRange(startX, endX);
                    averageCY5 = partCY5.Average();
                    listAvgCY5.Add(averageCY5);
                }
            }

            // Raw 데이터에서 평균값을 뺀다. 
            for (i = 0; i < wellCount; i++)
            {
                if (!isFAM[i] && !isHEX[i] && !isROX[i] && !isCY5[i])
                    continue;

                double averageFAM = 0.0;
                double averageHEX = 0.0;
                double averageROX = 0.0;
                double averageCY5 = 0.0;

                if (isFAM[i])
                {
                    listResultFAM[i] = new List<double>();
                    averageFAM = listAvgFAM[i];
                }
                if (isHEX[i])
                {
                    listResultHEX[i] = new List<double>();
                    averageHEX = listAvgHEX[i];
                }
                if (isROX[i])
                {
                    listResultROX[i] = new List<double>();
                    averageROX = listAvgROX[i];
                }
                if (isCY5[i])
                {
                    listResultCY5[i] = new List<double>();
                    averageCY5 = listAvgCY5[i];
                }

                //rowCount = listFAM[i].Count;
                for (j = 0; j < rowCount; j++)
                {
                    if (isFAM[i])
                    {
                        valueFAM = (listFAM[i][j]) - averageFAM;
                        if (valueFAM < 0) valueFAM = 0;
                        listResultFAM[i].Add(valueFAM);
                    }
                    if (isHEX[i])
                    {
                        valueHEX = (listHEX[i][j]) - averageHEX;
                        if (valueHEX < 0) valueHEX = 0;
                        listResultHEX[i].Add(valueHEX);
                    }
                    if (isROX[i])
                    {
                        valueROX = (listROX[i][j]) - averageROX;
                        if (valueROX < 0) valueROX = 0;
                        listResultROX[i].Add(valueROX);
                    }
                    if (isCY5[i])
                    {
                        valueCY5 = (listCY5[i][j]) - averageCY5;
                        if (valueCY5 < 0) valueCY5 = 0;
                        listResultCY5[i].Add(valueCY5);
                    }
                }
            }

            // 히스토그램에서 가장 넓은 구간을 찾는다. 
            double[] thresholdFAM = new double[wellCount];
            double[] thresholdHEX = new double[wellCount];
            double[] thresholdROX = new double[wellCount];
            double[] thresholdCY5 = new double[wellCount];
            int[] ctValueFAM = new int[wellCount];
            int[] ctValueHEX = new int[wellCount];
            int[] ctValueROX = new int[wellCount];
            int[] ctValueCY5 = new int[wellCount];
            int[] findFAMSx = new int[wellCount];
            int[] findHEXSx = new int[wellCount];
            int[] findROXSx = new int[wellCount];
            int[] findCY5Sx = new int[wellCount];
            int[] findFAMEx = new int[wellCount];
            int[] findHEXEx = new int[wellCount];
            int[] findROXEx = new int[wellCount];
            int[] findCY5Ex = new int[wellCount];
            int FAMCnt = 0, HEXCnt = 0, ROXCnt = 0, CY5Cnt = 0;
            double FAMSum = 0.0, HEXSum = 0.0, ROXSum = 0.0, CY5Sum = 0.0;
            for (i = 0; i < wellCount; i++)
            {
                if (!isFAM[i] && !isHEX[i] && !isROX[i] && !isCY5[i])
                    continue;

                if (isFAM[i])
                {
                    double[] tempFAM = listResultFAM[i].ToArray();
                    // Threshold 값을 찾는다. 
                    double logFAM = FindThreshold(tempFAM, ref ctValueFAM[i], ref findFAMSx[i], ref findFAMEx[i]);
                    thresholdFAM[i] = listResultFAM[i][ctValueFAM[i]];
                    if (thresholdFAM[i] > 0.0)
                        FAMSum += thresholdFAM[i];
                    FAMCnt++;
                }
                if (isHEX[i])
                {
                    double[] tempHEX = listResultHEX[i].ToArray();
                    double logHEX = FindThreshold(tempHEX, ref ctValueHEX[i], ref findHEXSx[i], ref findHEXEx[i]);
                    thresholdHEX[i] = listResultHEX[i][ctValueHEX[i]];
                    if (thresholdHEX[i] > 0.0)
                        HEXSum += thresholdHEX[i];
                    HEXCnt++;
                }
                if (isROX[i])
                {
                    double[] tempROX = listResultROX[i].ToArray();
                    double logROX = FindThreshold(tempROX, ref ctValueROX[i], ref findROXSx[i], ref findROXEx[i]);
                    thresholdROX[i] = listResultROX[i][ctValueROX[i]];
                    if (thresholdROX[i] > 0.0)
                        ROXSum += thresholdROX[i];
                    ROXCnt++;
                }
                if (isCY5[i])
                {
                    double[] tempCY5 = listResultCY5[i].ToArray();
                    double logCY5 = FindThreshold(tempCY5, ref ctValueCY5[i], ref findCY5Sx[i], ref findCY5Ex[i]);
                    thresholdCY5[i] = listResultCY5[i][ctValueCY5[i]];
                    if (thresholdCY5[i] > 0.0)
                        CY5Sum += thresholdCY5[i];
                    CY5Cnt++;
                }
            }

            // 배열에서 0 이 아닌 최소값(Y 값)을 찾는다. 
            resultThresholdFAM = 0.0;
            resultThresholdHEX = 0.0;
            resultThresholdROX = 0.0;
            resultThresholdCY5 = 0.0;

            // 저장된 Threshold 가 있으면 저장된 값으로 이후를 계산한다. 
            if (listWellInfo.ThFAM > 0.0) resultThresholdFAM = listWellInfo.ThFAM;
            else
            {
                if (FAMCnt > 0)
                {
                    // 배열값이 모두 0이면 에러 발생 
                    if (FAMSum > 0.0)
                        resultThresholdFAM = thresholdFAM.Where(num => num != 0).Max();
                    numericUpDown_ThFAM.Value = new decimal(resultThresholdFAM);
                }
            }
            if (listWellInfo.ThHEX > 0.0) resultThresholdHEX = listWellInfo.ThHEX;
            else
            {
                if (HEXCnt > 0)
                {
                    // 배열값이 모두 0이면 에러 발생 
                    if (HEXSum > 0.0)
                        resultThresholdHEX = thresholdHEX.Where(num => num != 0).Max();
                    numericUpDown_ThHEX.Value = new decimal(resultThresholdHEX);
                }
            }
            if (listWellInfo.ThROX > 0.0) resultThresholdROX = listWellInfo.ThROX;
            else
            {
                if (ROXCnt > 0)
                {
                    // 배열값이 모두 0이면 에러 발생 
                    if (ROXSum > 0.0)
                        resultThresholdROX = thresholdROX.Where(num => num != 0).Max();
                    numericUpDown_ThROX.Value = new decimal(resultThresholdROX);
                }
            }
            if (listWellInfo.ThCY5 > 0.0) resultThresholdCY5 = listWellInfo.ThCY5;
            else
            {
                if (CY5Cnt > 0)
                {
                    // 배열값이 모두 0이면 에러 발생 
                    if (CY5Sum > 0.0)
                        resultThresholdCY5 = thresholdCY5.Where(num => num != 0).Max();
                    numericUpDown_ThCY5.Value = new decimal(resultThresholdCY5);
                }
            }

            if (listThPointsFAM.Count > 0) listThPointsFAM.Clear();
            if (listThPointsHEX.Count > 0) listThPointsHEX.Clear();
            if (listThPointsROX.Count > 0) listThPointsROX.Clear();
            if (listThPointsCY5.Count > 0) listThPointsCY5.Clear();

            for (j = 0; j < rowCount; j++)
            {
                if (FAMCnt > 0) listThPointsFAM.Add(new ZedGraph.PointPair(j, resultThresholdFAM));
                if (HEXCnt > 0) listThPointsHEX.Add(new ZedGraph.PointPair(j, resultThresholdHEX));
                if (ROXCnt > 0) listThPointsROX.Add(new ZedGraph.PointPair(j, resultThresholdROX));
                if (CY5Cnt > 0) listThPointsCY5.Add(new ZedGraph.PointPair(j, resultThresholdCY5));
            }

            listResultCtFAM.Clear();
            listResultCtHEX.Clear();
            listResultCtROX.Clear();
            listResultCtCY5.Clear();
            // Ct 값에 해당하는 Y 값을 그래프에 추가한다. 
            for (i = 0; i < wellCount; i++)
            {
                if (!isFAM[i] && !isHEX[i] && !isROX[i] && !isCY5[i])
                    continue;

                double ctFAM = 0.0;
                double ctHEX = 0.0;
                double ctROX = 0.0;
                double ctCY5 = 0.0;

                // 배열에서 threshold 에 가장 가까운 값과 X 인덱스와 값을 찾는다. 
                if (isFAM[i])
                {
                    if (findFAMSx[i] < 0) findFAMSx[i] = 0;
                    if (findFAMSx[i] < findFAMEx[i])
                    {
                        double closestFAM = FindClosestValue(listResultFAM[i].ToArray(), findFAMSx[i], findFAMEx[i], resultThresholdFAM, out int closestFAMIndex);
                        ctFAM = closestFAMIndex / Global.graphInterpolationScale;
                    }
                    else ctFAM = 0.0;
                    listResultCtFAM.Add(ctFAM);
                }
                if (isHEX[i])
                {
                    if (findHEXSx[i] < 0) findHEXSx[i] = 0;
                    if (findHEXSx[i] < findHEXEx[i])
                    {
                        double closestHEX = FindClosestValue(listResultHEX[i].ToArray(), findHEXSx[i], findHEXEx[i], resultThresholdHEX, out int closestHEXIndex);
                        ctHEX = closestHEXIndex / Global.graphInterpolationScale;
                    }
                    else ctHEX = 0.0;
                    listResultCtHEX.Add(ctHEX);
                }
                if (isROX[i])
                {
                    if (findROXSx[i] < 0) findROXSx[i] = 0;
                    if (findROXSx[i] < findROXEx[i])
                    {
                        double closestROX = FindClosestValue(listResultROX[i].ToArray(), findROXSx[i], findROXEx[i], resultThresholdROX, out int closestROXIndex);
                        ctROX = closestROXIndex / Global.graphInterpolationScale;
                    }
                    else ctROX = 0.0;
                    listResultCtROX.Add(ctROX);
                }
                if (isCY5[i])
                {
                    if (findCY5Sx[i] < 0) findCY5Sx[i] = 0;
                    if (findCY5Sx[i] < findCY5Ex[i])
                    {
                        double closestCY5 = FindClosestValue(listResultCY5[i].ToArray(), findCY5Sx[i], findCY5Ex[i], resultThresholdCY5, out int closestCY5Index);
                        ctCY5 = closestCY5Index / Global.graphInterpolationScale;
                    }
                    else ctCY5 = 0.0;
                    listResultCtCY5.Add(ctCY5);
                }

                listWellInfo.SetCtValue(i, ctFAM, ctHEX, ctROX, ctCY5);
            }
            UpdateWellTable("", "", "");

            if (Global.selectedResult != null)
            {
                string resultExcel = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
                SaveExcelCtValue(resultExcel);
            }
        }

        /// <summary>
        /// 배열에서 threshold 에 가장 가까운 값과 인덱스를 계산한다. 
        /// </summary>
        /// <param name="array">Input Data</param>
        /// <param name="startIndex">검색 시작 인덱스</param>
        /// <param name="endIndex">검색 끝 인덱스</param>
        /// <param name="threshold">Threshold</param>
        /// <param name="closestIndex">가장 가까운 인덱스</param>
        /// <returns>찾은 인덱스의 Y 값</returns>
        static double FindClosestValue(double[] array, int startIndex, int endIndex, double threshold, out int closestIndex)
        {
            double minDifference = double.MaxValue;
            double closestValue = 0;
            closestIndex = -1;

            for (int i = startIndex; i < endIndex; i++)
            {
                double difference = Math.Abs(array[i] - threshold);

                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestValue = array[i];
                    closestIndex = i;
                }
            }

            return closestValue;
        }

        /// <summary>
        /// 해당 그래프에서 Threshold 를 찾는다. 
        /// 그래프에서 가장 넓은 영역에서 노이즈보다 큰 첫번째 값이 Threshold 이다. 
        /// </summary>
        /// <param name="data">Input Data</param>
        /// <param name="thIndex">Threshold Index</param>
        /// <param name="startIndex">가장 넓은 영역의 시작 인덱스(X)</param>
        /// <param name="endIndex">가장 넓은 영역의 끝 인덱스(X)</param>
        /// <returns>Threshold 값(Y)</returns>
        public double FindThreshold(double[] data, ref int thIndex, ref int startIndex, ref int endIndex)
        {
            // 영역이 가장 큰 시작 좌표와 끝 좌표를 찾는다. 
            int sX = 0, eX = 0;
            double maxArea = FindLargestArea(data, ref sX, ref eX);
            startIndex = sX;
            endIndex = eX;
            // 찾은 좌표의 처음이 평균을 구한 구간의 끝보다 작으면 0 을 리턴한다. 
            int endX = (int)(Global.baselineEnd * Global.graphInterpolationScale);
            if (data.Length < endX)
                endX = data.Length / 2;
            if(sX <= endX)
                return 0;

            double dataMax = data.Max();
            double noiseMax = 0.0;
            double dataMin = dataMax;
            int ctX = 0;
            if (sX > 0 && eX > 0)
            {
                double[] noiseArray = new double[sX];
                // 노이즈 영역만큼 배열을 복사하고 가장 큰값을 찾는다. 
                Array.ConstrainedCopy(data, 0, noiseArray, 0, sX);
                noiseMax = noiseArray.Max();
                // 노이즈 영역의 최대값보다는 크고 실제 데이터 영영에서 가장 작은값을 찾는다. 
                dataMin = data.Max();
                for (int x = sX; x < eX; x++)
                {
                    if (data[x] > noiseMax && data[x] < dataMin)
                    {
                        dataMin = data[x];
                        ctX = x;
                    }
                }
            }
            thIndex = ctX; // / Global.graphInterpolationScale;
            return dataMin;
        }

        /// <summary>
        /// 그래프에서 가장 넓은 영역을 찾는다. 
        /// </summary>
        /// <param name="histogram">Input Data</param>
        /// <param name="startIndex">가장 넓은 영역의 시작 인덱스(X)</param>
        /// <param name="endIndex">가장 넓은 영역의 끝 인덱스(X)</param>
        /// <returns></returns>
        public double FindLargestArea(double[] histogram, ref int startIndex, ref int endIndex)
        {
            int start = -1; // 구간의 시작 인덱스
            int end = -1; // 구간의 끝 인덱스
            int maxLength = 0; // 가장 긴 구간의 길이

            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] > 0)
                {
                    if (start == -1)
                    {
                        // 새로운 구간 시작
                        start = i;
                    }
                }
                else
                {
                    if (start != -1)
                    {
                        // 구간 끝
                        int length = i - start;
                        if (length > maxLength)
                        {
                            // 더 긴 구간 발견
                            maxLength = length;
                            end = i - 1;
                        }
                        start = -1;
                    }
                }
            }

            if (start != -1)
            {
                // 마지막 구간이 입력 배열의 끝에 있는 경우
                int length = histogram.Length - start;
                if (length > maxLength)
                {
                    maxLength = length;
                    end = histogram.Length - 1;
                }
            }

            Debug.WriteLine("가장 넓은 적분 구간: " + start + "부터 " + end);

            startIndex = start;
            endIndex = end;

            return maxLength;
        }

        /// <summary>
        /// Threshold 그래프를 생성하고 표시한다. 
        /// </summary>
        private void curveFittingThreshold()
        {
            int i, j;
            int wellCount = listInterpolateFAM.Count();
            int rowCount = listResultFAM[0].Count();
            for (i = 0; i < wellCount; i++)
            {
                if (!isFAM[i] && !isHEX[i] && !isROX[i] && !isCY5[i])
                    continue;

                for (j = 0; j < rowCount; j++)
                {
                    if (isFAM[i]) listResultPointsFAM[i][j].Y = listResultFAM[i][j];
                    if (isHEX[i]) listResultPointsHEX[i][j].Y = listResultHEX[i][j];
                    if (isROX[i]) listResultPointsROX[i][j].Y = listResultROX[i][j];
                    if (isCY5[i]) listResultPointsCY5[i][j].Y = listResultCY5[i][j]; 
                }
            }

            // Threshold 라인을 그린다. 
            for (j = 0; j < rowCount; j++)
            {
                if (listThPointsFAM.Count > 0) listThPointsFAM[j].Y = resultThresholdFAM;
                if (listThPointsHEX.Count > 0) listThPointsHEX[j].Y = resultThresholdHEX;
                if (listThPointsROX.Count > 0) listThPointsROX[j].Y = resultThresholdROX;
                if (listThPointsCY5.Count > 0) listThPointsCY5[j].Y = resultThresholdCY5;
            }

            PaneResultWave.YAxis.Scale.MaxAuto = true;
            PaneResultWave.YAxis.Scale.MinAuto = true;
            PaneResultWave.YAxis.Scale.MinorStepAuto = true;
            PaneResultWave.YAxis.Scale.MajorStepAuto = true;

            // Calculate the Axis Scale Ranges
            this.zedGraph_Result.AxisChange();
            this.zedGraph_Result.Refresh();
        }

        /// <summary>
        /// 기울기 구하기
        /// </summary>
        /// <returns>점(X1, Y1)과 점(X2, Y2)를 직선으로 연결했을 때의 1차 방정식 기울기를 구한다.</returns>
        public double GetGradien(double x1, double y1, double x2, double y2)
        {
            if (y1 == y2)
                return 0.0;

            return (y2 - y1) / (x2 - x1);
        }

        /// <summary>
        /// Sigmoidal 그래프를 생성하고 표시한다. 
        /// </summary>
        private void curveFittingSigmoidal()
        {
            double graphMin = 10000000;
            double graphMax = -10000000;

            // 그래프 개수가 베이스라인 평균 개수보다 작으면 그래프 개수의 절반으로 평균한다. 
            int startX = (int)(Global.baselineStart * Global.baseAvgScale);
            int endX = (int)(Global.baselineEnd * Global.baseAvgScale);
            if (listInterpolateFAM[0].Count < endX)
                endX = listInterpolateFAM[0].Count / 2;

            List<double>[] listFAM = new List<double>[25];
            List<double>[] listHEX = new List<double>[25];
            List<double>[] listROX = new List<double>[25];
            List<double>[] listCY5 = new List<double>[25];

            int wellCount = listInterpolateFAM.Count();
            for (int i = 0; i < wellCount; i++)
            {
                listFAM[i] = new List<double>();
                listHEX[i] = new List<double>();
                listROX[i] = new List<double>();
                listCY5[i] = new List<double>();

                double[] dataFAM = listInterpolateFAM[i].ToArray();
                double[] dataHEX = listInterpolateHEX[i].ToArray();
                double[] dataROX = listInterpolateROX[i].ToArray();
                double[] dataCY5 = listInterpolateCY5[i].ToArray();

                double[] resultFAM = null;
                double[] resultHEX = null;
                double[] resultROX = null;
                double[] resultCY5 = null;
 
                if (Global.IsLowPassFilter)
                {
                    MedianFilter filter = new MedianFilter();
                    int window3 = Global.medianFilterWindow; // * (int)Global.baseAvgScale;
                    resultFAM = filter.GetMedianFiltration(dataFAM, window3);
                    resultHEX = filter.GetMedianFiltration(dataHEX, window3);
                    resultROX = filter.GetMedianFiltration(dataROX, window3);
                    resultCY5 = filter.GetMedianFiltration(dataCY5, window3);        
                }
                else
                {
                    resultFAM = dataFAM;
                    resultHEX = dataHEX;
                    resultROX = dataROX;
                    resultCY5 = dataCY5;
                }

                // Passive Reference 가 선택된 경우 
                if (Global.passiveRef > 0)
                {
                    int rowCount = resultFAM.Count();
                    double Threshold = Global.graphThreshold;
                    for (int j = 0; j < rowCount; j++)
                    {
                        double passiveRefValue = 1.0;
                        if (Global.passiveRef == 1)
                            passiveRefValue = resultFAM[j];
                        else if (Global.passiveRef == 2)
                            passiveRefValue = resultHEX[j];
                        else if (Global.passiveRef == 3)
                            passiveRefValue = resultROX[j];
                        else if (Global.passiveRef == 4)
                            passiveRefValue = resultCY5[j];

                        double valueFAM = resultFAM[j] / passiveRefValue;
                        double valueHEX = resultHEX[j] / passiveRefValue;
                        double valueROX = resultROX[j] / passiveRefValue;
                        double valueCY5 = resultCY5[j] / passiveRefValue;

                        listFAM[i].Add(valueFAM);
                        listHEX[i].Add(valueHEX);
                        listROX[i].Add(valueROX);
                        listCY5[i].Add(valueCY5);
                    }
                }
                else
                {
                    listFAM[i].AddRange(resultFAM);
                    listHEX[i].AddRange(resultHEX);
                    listROX[i].AddRange(resultROX);
                    listCY5[i].AddRange(resultCY5);
                }
            }

            List<double> listAvgFAM = new List<double>();
            List<double> listAvgHEX = new List<double>();
            List<double> listAvgROX = new List<double>();
            List<double> listAvgCY5 = new List<double>();
            for (int i = 0; i < wellCount; i++)
            {
                double sumFAM = 0.0;
                double sumHEX = 0.0;
                double sumROX = 0.0;
                double sumCY5 = 0.0;
                for (int j = startX; j < endX; j++)
                {
                    sumFAM += listFAM[i][j];
                    sumHEX += listHEX[i][j];
                    sumROX += listROX[i][j];
                    sumCY5 += listCY5[i][j];
                }
                double averageFAM = sumFAM / (endX - startX);
                double averageHEX = sumHEX / (endX - startX);
                double averageROX = sumROX / (endX - startX);
                double averageCY5 = sumCY5 / (endX - startX);

                listAvgFAM.Add(averageFAM);
                listAvgHEX.Add(averageHEX);
                listAvgROX.Add(averageROX);
                listAvgCY5.Add(averageCY5);
            }

            // Baseline 값을 계산한다. 
            List<double>[] listBaseFAM = new List<double>[25];
            List<double>[] listBaseHEX = new List<double>[25];
            List<double>[] listBaseROX = new List<double>[25];
            List<double>[] listBaseCY5 = new List<double>[25];
            for (int i = 0; i < wellCount; i++)
            {
                listBaseFAM[i] = new List<double>();
                listBaseHEX[i] = new List<double>();
                listBaseROX[i] = new List<double>();
                listBaseCY5[i] = new List<double>();

                int rowCount = listFAM[i].Count;
                for (int j = 0; j < rowCount; j++)
                {
                    double baseValue = (listFAM[i][j] - listAvgFAM[i]);
                    if (baseValue < 0.0)
                        baseValue = 0.0;
                    listBaseFAM[i].Add(baseValue);
                    baseValue = (listHEX[i][j] - listAvgHEX[i]);
                    if (baseValue < 0.0)
                        baseValue = 0.0;
                    listBaseHEX[i].Add(baseValue);
                    baseValue = (listROX[i][j] - listAvgROX[i]);
                    if (baseValue < 0.0)
                        baseValue = 0.0;
                    listBaseROX[i].Add(baseValue);
                    baseValue = (listCY5[i][j] - listAvgCY5[i]);
                    if (baseValue < 0.0)
                        baseValue = 0.0;
                    listBaseCY5[i].Add(baseValue);
                }
            }

            // Sigmoid 값을 계산한다. 
            for (int i = 0; i < wellCount; i++)
            {
                int rowCount = listInterpolateFAM[i].Count;
                for (int j = 0; j < rowCount; j++)
                {
                    double averageFAM = listBaseFAM[i].Average();
                    double averageHEX = listBaseHEX[i].Average();
                    double averageROX = listBaseROX[i].Average();
                    double averageCY5 = listBaseCY5[i].Average();

                    listResultPointsFAM[i][j].Y = Sigmoid(listBaseFAM[i][j] - averageFAM);
                    listResultPointsHEX[i][j].Y = Sigmoid(listBaseHEX[i][j] - averageHEX);
                    listResultPointsROX[i][j].Y = Sigmoid(listBaseROX[i][j] - averageROX);
                    listResultPointsCY5[i][j].Y = Sigmoid(listBaseCY5[i][j] - averageCY5);

                    if (graphMin > listResultPointsFAM[i][j].Y) graphMin = listResultPointsFAM[i][j].Y;
                    if (graphMin > listResultPointsHEX[i][j].Y) graphMin = listResultPointsHEX[i][j].Y;
                    if (graphMin > listResultPointsROX[i][j].Y) graphMin = listResultPointsROX[i][j].Y;
                    if (graphMin > listResultPointsCY5[i][j].Y) graphMin = listResultPointsCY5[i][j].Y;

                    if (graphMax < listResultPointsFAM[i][j].Y) graphMax = listResultPointsFAM[i][j].Y;
                    if (graphMax < listResultPointsHEX[i][j].Y) graphMax = listResultPointsHEX[i][j].Y;
                    if (graphMax < listResultPointsROX[i][j].Y) graphMax = listResultPointsROX[i][j].Y;
                    if (graphMax < listResultPointsCY5[i][j].Y) graphMax = listResultPointsCY5[i][j].Y;
                }
            }

            PaneResultWave.YAxis.Scale.MaxAuto = true;
            PaneResultWave.YAxis.Scale.MinAuto = true;
            PaneResultWave.YAxis.Scale.MinorStepAuto = true;
            PaneResultWave.YAxis.Scale.MajorStepAuto = true;

            // Calculate the Axis Scale Ranges
            this.zedGraph_Result.AxisChange();
            this.zedGraph_Result.Refresh();
        }

        /// <summary>
        /// 그래프 타입이 변경될때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void flatComboBox_GraphType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = flatComboBox_GraphType.SelectedIndex;
            if (listInterpolateFAM[0] == null)
                return;

            int listPoints = listInterpolateFAM[0].Count;
            if (listPoints <= 0)
                return;

            // Threshold 그래프에서만 Apply, Auto Threshold 버튼을 활성화한다. 
            if(index == 2)
            {
                btnThApply.Enabled = true;
                btnThAuto.Enabled = true;
            }
            else
            {
                btnThApply.Enabled = false;
                btnThAuto.Enabled = false;
            }

            this.Cursor = Cursors.WaitCursor;

            if (index == 0)  // 원본 데이터를 그래프로 출력한다. 
            {
                curveFittingRawData();
            }
            else if (index == 1)  // curve Fitting BaseLine
            {
                curveFittingBaseLine();
            }
            else if (index == 2)  // curve Fitting Threshold
            {
                curveFittingThreshold();
            }
            else if (index == 3)  // curve Fitting Sigmoidal
            {
                curveFittingSigmoidal();
            }

            ShowResultGraph();
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Sigmoid 계산 함수 
        /// </summary>
        /// <param name="x">원본 데이터</param>
        /// <returns></returns>
        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        #region 데이터베이스 관련 함수 (현재 사용 안함.)
        // 조건에 맞는 결과 찾기
        private void SearchResult()
        {
            Global.listResultInfos.Clear();               // 모든환자 지우기

            DateTime studyDateLow = dateTime_SearchStart.Value;
            DateTime studyDateHigh = dateTime_SearchEnd.Value;
            searchName = textBox_UserName.Text;

            // 검색된 환자를 patients에 저장함.
            foreach (var p in Global.resultManager.SearchResults(this.bSearchAll, searchBarcode, searchName, studyDateLow, studyDateHigh))
            {
                ResultInfo result = new ResultInfo(p);
                Global.listResultInfos.Add(result);
            }

            ListViewReload();                   // 환자 정보 ListView에 표시            
        }

        // 결과정보를 listview에 나타내기
        private void ListViewReload()
        {
            // listview 지우기
            materialListView_Result.Items.Clear();

            if (Global.listResultInfos.Count == 0) return;

            AlertProgressForm alertProgress = new AlertProgressForm(Title: "Loading Patient...");
            alertProgress.Show();

            //리스트뷰가 업데이트가 끝날 때까지 UI 갱신 중지
            materialListView_Result.BeginUpdate();
            string strName = "";
            string strResultDate = "";
            string strMethodeFile = "";
            string strPlateFile = "";
            string strResultFile = "";
            int loadCount = 0;

            foreach (ResultInfo result in Global.listResultInfos)
            {
                loadCount++;
                int progressValue = Convert.ToInt32(((double)loadCount / (double)Global.listResultInfos.Count) * 100);
                alertProgress.Message = string.Format("Loading Patient({0}_{1}) Data ..", result.UserName, result.ResultDateTime.ToString("MM/dd/yyyy"));
                alertProgress.ProgressValue = progressValue;

                Debug.WriteLine(" {0} : {1}, {2}", result.RID, result.Barcode, result.UserName);
                strName = result.UserName.Replace("^", "");      // 이름입력
                strResultDate = result.ResultDateTime.ToString("MM/dd/yyyy");
                strMethodeFile = result.MethodFile;
                strPlateFile = result.PlateFile;
                strResultFile = result.ResultFile;

                materialListView_Result.Items.Add(new ListViewItem(new String[] {
                    loadCount.ToString(),                       // No.
                    strName,                                    // Name
                    strResultDate,                              // Result Date
                    strMethodeFile,                             // Methode File
                    strPlateFile,                               // Plate File
                    strResultFile                               // Result File
                }));
            }

            // 리스트뷰를 새로고침하여 보여줌
            materialListView_Result.EndUpdate();

            alertProgress.Close();
        }


        private void customBtnLoad_Click(object sender, EventArgs e)
        {
            if (selectResultFile.Length <= 0)
                return;

            this.Cursor = Cursors.WaitCursor;

            Result rInfo = Global.resultManager.SearchResultFile(selectResultFile);
            if (rInfo != null)
            {
                if (Global.selectedResult == null)
                    Global.selectedResult = new ResultInfo(rInfo);
                else 
                    Global.selectedResult.SetResult(rInfo);

                string filePath = rInfo.MethodPath + rInfo.MethodFile;
                if (File.Exists(filePath))
                    LoadMethod(filePath);
                else
                    MessageBox.Show(new Form { TopMost = true }, "Not found Method File !");
                filePath = rInfo.PlatePath + rInfo.PlateFile;
                if (File.Exists(filePath))
                {
                    dataViewImages.ColumnHeadersVisible = false;
                    dataViewImages.SelectAll();
                    dataViewImages.ColumnHeadersVisible = true;

                    LoadPlate(filePath);
                }
                else
                    MessageBox.Show(new Form { TopMost = true }, "Not found Plate File !");
                filePath = rInfo.ResultPath + rInfo.ResultFile;
                if (File.Exists(filePath))
                { LoadResult(filePath); flatComboBox_GraphType.SelectedIndex = 0; }
                else
                    MessageBox.Show(new Form { TopMost = true }, "Not found Result File !");
            }

            this.Cursor = Cursors.Default;
        }

        private void customBtnDelete_Click(object sender, EventArgs e)
        {
            Result rInfo = Global.resultManager.SearchResultFile(selectResultFile);
            if (rInfo != null)
            {
                Global.resultManager.DeleteResult(rInfo);
                materialListView_Result.Items[selectResultIndex].Remove();
            }
        }

        private void materialListView_Result_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            selectResultIndex = e.ItemIndex;
            if (selectResultIndex >= 0)
                selectResultFile = materialListView_Result.Items[selectResultIndex].SubItems[5].Text;
            else
                selectResultFile = "";
        }
        #endregion

        /// <summary>
        /// 그래프 Smoothing 필터 함수 (현재 사용 안함.) 
        /// </summary>
        private void curveSmoothing()
        {
            double[] data = Generate.Sinusoidal(128, 44100, 350, 20);
            OnlineFilter bandPass = OnlineFilter.CreateBandpass(ImpulseResponse.Finite, 44100, 340, 360);
            bandPass.ProcessSamples(data); //data is replaced with result
        }

        /// <summary>
        /// 그래프 Butterworth 필터 함수 (현재 사용 안함.)
        /// </summary>
        /// <param name="indata"></param>
        /// <param name="deltaTimeinsec"></param>
        /// <param name="CutOff"></param>
        /// <returns></returns>
        private double[] Butterworth(double[] indata, double deltaTimeinsec, double CutOff)
        {
            if (indata == null) return null;
            if (CutOff == 0) return indata;
            double Samplingrate = 1 / deltaTimeinsec;
            long dF2 = indata.Length - 1; // The data range is set with dF2
            double[] Dat2 = new double[dF2 + 4]; // Array with 4 extra points front and back
            double[] data = indata; // Ptr., changes passed data
            for (long r = 0; r < dF2; r++)
            {
                Dat2[2 + r] = indata[r];
            }
            Dat2[1] = Dat2[0] = indata[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = indata[dF2];
            const double pi = 3.14159265358979;
            double wc = Math.Tan(CutOff * pi / Samplingrate);
            double k1 = 1.414213562 * wc; // Sqrt(2) * wc
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;
            // RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
            double[] DatYt = new double[dF2 + 4];
            DatYt[1] = DatYt[0] = indata[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2]
                + d * DatYt[s - 1] + e * DatYt[s - 2];
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];
            // FORWARD filter
            double[] DatZt = new double[dF2 + 2];

            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4]
                + d * DatZt[-t + 1] + e * DatZt[-t + 2];
            }
            // Calculated points are written
            for (long p = 0; p < dF2; p++)
            {
                data[p] = DatZt[p];
            }
            return data;
        }

        /// <summary>
        /// 그래프 Interpolation 메뉴 실행 이벤트 함수 (현재 사용 안함.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_Interpolation_CheckedChanged(object sender, EventArgs e)
        {
            Global.IsInterpolation = checkBox_Interpolation.Checked;

            if (Global.IsInterpolation)
                Global.baseAvgScale = Global.graphInterpolationScale;
            else
                Global.baseAvgScale = 1;

            string fullPath = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
            LoadResult(fullPath);
        }

        /// <summary>
        /// 그래프 Low Pass 필터 메뉴 실행 이벤트 함수 (현재 사용 안함.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_LowPass_CheckedChanged(object sender, EventArgs e)
        {
            Global.IsLowPassFilter = checkBox_LowPass.Checked;
        }

        /// <summary>
        /// Passive Reference 가 변경될때의 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_PassiveRef_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.passiveRef = ComboBox_PassiveRef.SelectedIndex;
            curveCalibration();
        }

        /// <summary>
        /// Resum 버튼 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResume_Click(object sender, EventArgs e)
        {
            Global.PCR_Manager.PcrResume();
        }

        /// <summary>
        /// FAM threshold 칼라 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorBtn_thFAM_Changed(object sender, EventArgs e)
        {
            Global.thFAMColorIndex = colorBtn_thFAM.ColorIndex;
        }

        /// <summary>
        /// HEX threshold 칼라 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorBtn_thHEX_Changed(object sender, EventArgs e)
        {
            Global.thHEXColorIndex = colorBtn_thHEX.ColorIndex;
        }

        /// <summary>
        /// ROX threshold 칼라 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorBtn_thROX_Changed(object sender, EventArgs e)
        {
            Global.thROXColorIndex = colorBtn_thROX.ColorIndex;
        }

        /// <summary>
        /// CY5 threshold 칼라 변경 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorBtn_thCY5_Changed(object sender, EventArgs e)
        {
            Global.thCY5ColorIndex = colorBtn_thCY5.ColorIndex;
        }

        /// <summary>
        /// Propertie Save 메뉴 클릭 이벤트 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveProperties_Click(object sender, EventArgs e)
        {
            this.SaveProperties();
            if (Global.selectedResult != null && Global.selectedResult.ResultFile.Length > 0)
            {
                string resultFile = Global.selectedResult.ResultPath + Global.selectedResult.ResultFile;
                // 엑셀 파일에 Properties 탭을 생성하고 내용을 저장한다. 
                SaveExcelProperties(resultFile);
            }
        }
    }
}
