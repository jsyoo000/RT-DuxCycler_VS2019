using BitmapControl;
//using CameraControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using Duxcycler_Database;
using Duxcycler;
using System.Reflection;
using System.Drawing.Text;
using Duxcycler_IMAGE;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Duxcycler.Properties;
using CameraControlLib;
using AForge.Video.DirectShow;
using DirectShowLib;
//using Touchless.Vision.Camera;

namespace Duxcycler_GLOBAL
{
    public enum SHOWPICTUREBOX { SHOW_1X1 = 1, SHOW_1X2 = 2, SHOW_2X2 = 4, SHOW_3X3 = 9, SHOW_4X4 = 16 };
    public enum SHOWIMAGEYPTE { SHOW_EXAM, SHOW_COMPARE };
    public enum eIMAGEFORMAT { FORMAT_DICOM, FORMAT_PNG, FORMAT_JPG, FORMAT_BMP };
    public enum eGROUP_TYPE { HOLD_STAGE, PCR_STAGE };
    public enum CAMERA_PROP { FOCUS = 0, EXPOSURE = 1, BRIGHTNESS = 2, CONTRAST = 3, HUE = 4, SATURATION = 5, SHARPNESS = 6, GAMMA = 7, WHITEBALANCE = 8, BKCOMPENSATION = 9, GAIN = 10 };

    public enum PCR_STATE
    {
        M_READY = 0x01,
        M_RUN = 0x02,
        M_PCREND = 0x03,
        M_EMERGENCY_STOP = 0x04,
        M_TASK_WRITE = 0x05,
        M_TASK_READ = 0x06,
        M_ERROR = 0x07,
        M_REFRIGERATION = 0x08
    }

    // Task의 Action 구조체 
    public struct PCR_Action
    {
        public string label; //action label or GOTO
        public double temp;  //Target temperature
        public double time;  //Duration that the temperature is stable over
        public bool capture; //Capture status
    };

    //IO buffer format
    //IN buffer (PC centric)
    public enum kunPCR_Packet
    {
        IOBFI_START = 0,
        IOBFI_STATE = 1,
        IOBFI_RES = 2,
        IOBFI_ACTNO = 3,
        IOBFI_LPCNT = 4,
        IOBFI_ACTLINE = 5,  //total task line
        //IOBFI_KP		= 6,
        //IOBFI_KI		= 7,
        //IOBFI_KD		= 8,
        IOBFI_LFTTTMEH = 9,  //total left time
        IOBFI_LFTTTMEL = 10,
        IOBFI_SECLEFTH = 11, //left time per line
        IOBFI_SECLEFTL = 12,
        IOBFI_LIDTEMPH = 13,  // Lid heater temp.
        IOBFI_LIDTEMPL = 14,
        IOBFI_CHMTEMPH = 15,   // chamber temp.
        IOBFI_CHMTEMPL = 16,
        IOBFI_PWMH = 17, //pwm duration (0x00~0x3F)
        IOBFI_PWML = 18,
        IOBFI_PWMDIR = 19, //pwm direction (+,-)
        IOBFI_LABEL = 20,
        IOBFI_TEMP = 21,
        IOBFI_TIMEH = 22,
        IOBFI_TIMEL = 23,
        IOBFI_LIDTEMP = 24,
        IOBFI_REQLINE = 25,
        IOBFI_ERROR = 26,
        IOBFI_CUR_OPR = 27,
        IOBFI_SINKTMPH = 28, // Heatsink temp
        IOBFI_SINKTMPL = 29,  // Heatsink temp

        // [2010.12.6 By Soda] 1byte -> 4byte로 현재 실행중인pid 값 받는 부분 변경
        IOBFI_KP_1 = 30,
        IOBFI_KP_2 = 31,
        IOBFI_KP_3 = 32,
        IOBFI_KP_4 = 33,

        IOBFI_KI_1 = 34,
        IOBFI_KI_2 = 35,
        IOBFI_KI_3 = 36,
        IOBFI_KI_4 = 37,

        IOBFI_KD_1 = 38,
        IOBFI_KD_2 = 39,
        IOBFI_KD_3 = 40,
        IOBFI_KD_4 = 41
        //-------------------------------------------
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Kun_mdeviceList
    {
        public string DeviceName;   // Device name
        public string Manufacturer; // Manufacturer
        public string SerialNumber; // Serial number
        public uint VendorID;         // Vendor ID
        public uint ProductID;        // Product ID
        public int InputReportLen;   // Length of HID input report (bytes)
        public int OutputReportLen;  // Length of HID output report (bytes)
        public int Interface;        // Interface  
        public int Collection;       // Collection 
    }

    //#pragma pack (push, 4)
    //    typedef struct {
    //    char* DeviceName;   // Device name
    //    char* Manufacturer; // Manufacturer
    //    char* SerialNumber; // Serial number
    //    unsigned int VendorID;         // Vendor ID
    //    unsigned int ProductID;        // Product ID
    //    int InputReportLen;   // Length of HID input report (bytes)
    //    int OutputReportLen;  // Length of HID output report (bytes)
    //    int Interface;        // Interface  
    //    int Collection;       // Collection 
    //}
    //mdeviceList2;
    //#pragma pack (pop,4)

    public sealed partial class Global
    {

        //#region DLL Import
        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int add(int a, int b);

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Kun_SetInstance(int instance);

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        ////public static extern int Kun_GetLibVersion(char* buf);
        //public static extern int Kun_GetLibVersion(ref string buf);       // Get DLL version string

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr Kun_GetSerialNumber();   // Get Serial Number string

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Kun_ShowVersion();                      // Show a message box containing version

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Kun_Read(Byte[] pBuf);              // Read the from the HID device.

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Kun_CloseRead();                  // Close the read pipe

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Kun_CloseWrite();                 // Close the write pipe

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Kun_Write(Byte[] pBuf);                  // Write to the HID device

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Kun_GetReportLengths(int[] input_len,   // Pointer for storing input length
        //                                                int[] output_len); // Pointer for storing output length

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Kun_SetCollection(int col);                // Specifies a collection (call prior to Open())  (0xffff = unspecified)

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Kun_GetCollection();                    // Retrieves collection setting (0xffff = unspecified)

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Kun_SetInterface(int iface);                 // Specifies an interface (call prior to Open())  (0xffff = unspecified)

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Kun_GetInterface();                     // Rerieves interface setting   (0xffff = unspecified)

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Kun_Open(uint VendorID,        // Vendor ID to search     (0xffff if unused)
        //                                    uint ProductID,    // Product ID to search    (0xffff if unused)
        //                                    string Manufacturer, // Manufacturer            (NULL if unused)
        //                                    string SerialNum,       // Serial number to search (0xffff if unused)
        //                                    string DeviceName,   // Device name to search   (NULL if unused)
        //                                    int bAsync);		   // Set TRUE for non-blocking read requests.

        //[DllImport("UsbPCRKun.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Kun_GetList(uint VendorID,      // Vendor ID to search  (0xffff if unused)
        //                                        uint ProductID,    // Product ID to search (0xffff if unused)
        //                                        string Manufacturer, // Manufacturer            (NULL if unused)
        //                                        string SerialNum,    // Serial number to search (NULL if unused)
        //                                        string DeviceName,   // Device name to search   (NULL if unused)
        //                                        Kun_mdeviceList[] pList,        // Caller's array for storing matching device(s)             
        //                                        int nMaxDevices);  // Size of the caller's array list (no.entries)
        //#endregion


        #region Camera Property Controls
        public static Stopwatch stopWatch = null;
        public static VideoCaptureDevice videoSource = null;
        public static Camera selectCam = null;
        public static IAMVideoProcAmp procAmp = null;
        public static VideoCapabilities[] videoCapabilities;
        public static VideoCapabilities[] snapshotCapabilities;

        public static List<CameraProperty> listCameraPropertys = new List<CameraProperty>();
        #endregion

        public static bool isCloseForm = false;

        // 추후 국문, 중문 개발시 적용
        public static System.Resources.ResourceManager ResManager = new System.Resources.ResourceManager("IRIS_NEW.Language.string", Assembly.GetExecutingAssembly());
        public static int SelectLanguage = 0;             // 0: 국문, 1: 영문, 2: 중문 
        public static string SelectedLanguage = "ko-KR";  // 국문(ko-KR), 1: 영문(en-US), 2: 중문(zh-CN) 

        // 설정모드
        public static bool ManagerMode = false;             // fasle: 사용자 모드, true: 관리모드
        public static bool IsTopMost   = false;            // 일반 사용모드은 경우 TopMost : true, 관리자 : false

        // 화면 표시 위치
        public static int ScreensIndex = 0;             // Screen 표시 위치

        // 버젼 설징
        public static string DuxcyclerVersion = "1.0.0";      // Duxcycler Version 표시
        public static string DuxcyclerPID = "1.0.0.0.0.1";    // Duxcycler PID 표시        

        // Properties
        public static string UserName = "BIOMEDUX";
        public static string Barcode = "";
        public static string InstrumentType = "RT-DuxCycler";
        public static string BlockType = "";
        public static int ExperimentType = 0;
        public static string Chemisty = "";
        public static int RunMode = 0;
        public static string Volume = "50.0";
        public static string Cover = "104.0";
        public static string MethodPath = "C:\\BioMedux\\Method\\";
        public static string PlatePath = "C:\\BioMedux\\Plate\\";
        public static string ResultPath = "C:\\BioMedux\\Result\\";
        public static string Comment = "";
        public static DateTime ResultDateTime = System.DateTime.Now;
        public static string MethodFile = "";
        public static string PlateFile = "";
        public static string ResultFile = "";

        // Optic 설정
        public static int accelSpeed = 1000;                                      // Step Motor 가속도 
        public static int coarseSpeed = 300;                                      // Step Motor 속도 
        public static int fineSpeed = 50;                                         // Step Motor 안정속도 
        public static int maxSpeed = 1000;                                        // Step Motor 최대속도 
        public static int ledOn = 200;                                            // LED On
        public static int ledOff = 0;                                             // LED Off
        public static int trayIn = 2000;                                          // Tray In
        public static int trayOut = 1000;                                         // Tray Out
        public static int heaterUp = 2000;                                        // LidHeader Up 
        public static int heaterDown = 1000;                                      // LidHeader Down
        public static int filterFAM_Pos = 25;                                    // FAM Filter 위치 
        public static int filterHEX_Pos = -290;                                    // HEX Filter 위치 
        public static int filterROX_Pos = -600;                                   // ROX Filter 위치 
        public static int filterCY5_Pos = -900;                                   // CY5 Filter 위치 
        public static int filterMoveDelay = 700;                                  // 필터를 움직이는데 소요되는 시간 

        // Camera 설정
        public static int SAVEDIMAGE_X = 4656;                                    // 보여줄 이미지 x Resultion ( 원본 영상이 설정된 값보다 크면 원본영상 사용 )
        public static int SAVEDIMAGE_Y = 3496;                                    // 보여줄 이미지 y Resultion ( 원본 영상이 설정된 값보다 크면 원본영상 사용 )
        public static double LowTemperature = 14.50;                              // 영상의 최저 온도값( -> 0  )
        public static double HighTemperature = 40.00;                             // 영상의 최고 온도값( -> 255)

        public static int ccdCameraNo = 0;          // CCD Camerea 번호  
        public static int ccdExposure = -2;         // CCD Exposure 값 (-13~-1)  
        public static int ExposureTime = 250;       // CCD Exposure 시간 (ExposureTime Sec)  
        public static int finalExposureTime = 250 * 3;  // CCD Exposure 시간 (ExposureTime * 3 Sec)  
        public static bool isAutoExposure = false;  // CCD AutoExposure 값  
        public static int ccdFocus = 750;           // CCD Focus 값 (1~1023) 
        public static bool isAutoFocus = false;      // CCD AutoFocus 값  
        public static int ccdWB = 4000;             // CCD Focus 값 (2800~6500) 
        public static bool isAutoWB = true;         // CCD AutoFocus 값  
        public static int ccdCompensation = 2;      // CCD Back Light Compensation 값  (0~2)
        public static int ccdGain = 0;              // CCD Gain 값 (0~6)   
        public static int ccdBrightness = 0;        // CCD Brightness 값 (-64~64) 
        public static int ccdContrast = 0;          // CCD Contrast 값 (-64~64) 
        public static int ccdSharpness = 3;         // CCD Sharpness 값 (0~6) 
        public static int ccdGamma = 72;            // CCD Gamma 값 (72~500)   

        // PCR PROTOCOL 정보 
        public static double PCR_VOLUME = 50.0;
        public static double HEATER_TEMP = 104.0;

        // 결과 저장 설정
        public static string SaveFileNameType = "ChartNO_Name_Gender_Birthday_StudyDate";               // 파일 저장 이름 형식
        public static bool IsSavePDF = true;                                                            // 파일 저장 형식  true : PDF, false : JPGE
        public static string SavePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);  // 파일 저장 위치

        // 동영상 저장 기능 사용 여부
        public static bool IsUsedSavedVideo = false;                                        // 동영상 저장 기능 사용 여부
 
        // 시리얼 설정
        public static string ArducamPort = "COM2";              // Port Name
        public static int ArducamBaudRate = 9600;               // BaudRate
        public static string OpticPort = "COM3";                // Port Name
        public static int OpticBaudRate = 9600;                 // BaudRate
        public static SerialManager ArducamSerial = null;
        public static ConcurrentQueue<Byte> byteBufQueue_revQ = new ConcurrentQueue<Byte>();       // 시리얼 바이트 버퍼 큐
        public static ConcurrentQueue<string> stringBufQueue_revQ = new ConcurrentQueue<string>();       // 시리얼 바이트 버퍼 큐

        public static int currentTick, previousTick;            // 데이터 수신 속도 
        public static int frameRate = 0;
        // Create a new Mutex. The creating thread does not own the mutex.
        public static object lockObj = new object();

        // 사용 Form 설정
        public static MainPage main = null;     // Main 화면 
        //public static ScanPage scan = null;     // SCan 화면
        
        // Database 설정
        public static string DBPath                 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DuxCycler.db");
        public static ResultManager resultManager = new ResultManager(Global.DBPath);
        public static ResultInfo selectedResult = null;                 // DB 저장 시 사용하는 사용자 정보        

        // Fileopen시 사용 변수
        public static List<ResultInfo> listResultInfos = new List<ResultInfo>();       // Fileopen시 사용할 StudyInfo list
        public static int selectedStudyIndex         = 0;                           // 선택한 Study Index
        public static int selectedImageIndex         = 0;                           // 선택한 ImageIndex

        // Show Image Type
        public static SHOWPICTUREBOX selectedShowPicturebox = SHOWPICTUREBOX.SHOW_2X2;
        public static SHOWIMAGEYPTE selectedShowImageType   = SHOWIMAGEYPTE.SHOW_EXAM;

        // 
        public static Color buttonColor = Color.Transparent;

        // Stage, Step 설정 
        public static double default_HoldStepTemp1 = 50.0;
        public static double default_HoldStepTemp2 = 95.0;
        public static double default_PcrStepTemp1 = 95.0;
        public static double default_PcrStepTemp2 = 60.0;

        public static TimeSpan default_HoldStepTime1 = new TimeSpan(0, 2, 0);  // 2분
        public static TimeSpan default_HoldStepTime2 = new TimeSpan(0, 10, 0); // 10분
        public static TimeSpan default_PcrStepTime1 = new TimeSpan(0, 0, 15);  // 15초
        public static TimeSpan default_PcrStepTime2 = new TimeSpan(0, 1, 0);   // 1분

        public static PCR_Task PCR_Manager = new PCR_Task();
 
        public static Kun_mdeviceList[] m_DeviceList = new Kun_mdeviceList[20];	//[2010.8.30 By Soda] 20개까지 usb device를 listup하기 위한 것
        public static string m_Serialnum = "";

        // Plate 설정
        public static int passiveRef = 0;           // passive Reference 0:None, 1:FAM, 2:HEX, 3:ROX, 4:CY5

        public static List<IpPlate_Target> listTargetInfos = new List<IpPlate_Target>();          // Plate Target List     
        public static List<IpPlate_Sample> listSampleInfos = new List<IpPlate_Sample>();         // Plate Target List     
        public static List<IpPlate_Sample> listBioGroupInfos = new List<IpPlate_Sample>();       // Plate Target List     

        public static Image[] Tasks = new Image[] { Resources.Task_U, Resources.Task_S, Resources.Task_N };

        // ROI 설정
        public struct IpRoiInfo
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public double Gain;
            public double Offset;
        };
        public static int RoiCount = 25;                                        // ROI 개수 
        //public static List<IpRoiInfo> listRoiInfos = new List<IpRoiInfo>();     // ROI list
        public static List<ROIShape> listRoiInfos = new List<ROIShape>();          // ROI 도형 List     

        public static int Roi_FontSize = 9;                         // ROI Font Size 
        public static int Roi_BorderWidth = 1;                      // ROI Line Thickness
        public static int Roi_Option = 2;                           // ROI Display Option 

        // Filter 적용 여부 설정
        public static bool IsMedianBlur = false;                            // MedianBlur Filter 사용 여부
        public static int MedianKsize   = 3;                               //필터의 크기(1이상의 홀수 값) (Note – 생성된 결과 필터는 ksize x ksize의 크기를 갖는다.)

        public static bool IsGaussianBlur   = false;                            // GaussianBlur Filter 사용 여부
        public static int GaussianKSizeX    = 5;                               // 가우시안 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        public static int GaussianKSizeY    = 5;                               // 가우시안 커널의 Y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        public static double GaussianSigmaX = 0;                               // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
        public static double GaussianSigmaY = 0;                               // Y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.        

        public static bool IsBilateralFilter = false;                            // Bilateral Filter 사용 여부
        public static int BilateralD         = 5;                               // 각 픽셀이웃의 직경(Diameter of each pixel neighbourhood)
        public static double BilateralSigmaX = 15;                               // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
        public static double BilateralSigmaY = 15;                               // y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.

        public static bool IsBlur     = false;                            // Blur Filter 사용 여부
        public static int BlurKSizeX  = 3;                               // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        public static int BlurKSizeY  = 3;                               // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)        
        public static int BlurAnchorX = -1;                              // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.
        public static int BlurAnchorY = -1;                              // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.        

        public static bool IsBoxFilter = false;                            // Box Filter 사용 여부
        public static int BoxKSizeX    = 3;                               // 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
        public static int BoxKSizeY    = 3;                               // 커널의 y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)                                

        public static bool IsSharpen        = false;                           // 선명 필터 1 사용 여부
        public static bool IsMorphology     = false;                           // Morphology 사용 여부
        public static bool IsMeansDenoising = true;                            // MeansDenoising 필터 사용 여부

        public static double MarginsLeft   = 10;                               // Print 페이지 왼쪽 여백
        public static double MarginsRight  = 10;                               // Print 페이지 오른쪽 여백
        public static double MarginsTop    = 10;                               // Print 페이지 윗쪽 여백
        public static double MarginsBottom = 10;                               // Print 페이지 아랫쪽 여백
        public static string Header_Left   = "Infra Red Imaging System";          // Print시 Header 왼쪽에 나오는 문구
        public static string Header_Right  = "Medicore. Co.";                     // Print시 Header 오른쪽에 나오는 문구
        public static string Footer_Left1  = "Medicore Hospital";                 // Print시 Footer 왼쪽에 나오는 문구 1
        public static string Footer_Left2  = "Tel:02-2056-2600/Fax:02-2056-2626"; // Print시 Footer 왼쪽에 나오는 문구 2

        public static bool Print_LOGO_Show              = false;                           // Print시 LOGO를 보여줄지 설정                                       
        public static bool Print_PaletteBar_Show        = true;                            // Print시 PaletteBar를 보여줄지 설정
        public static bool Print_ROI_Show               = true;                            // Print시 ROI를 보여줄지 설정
        public static bool Print_Diff_ROI_Display       = true;                            // Print시 ROI의 차이를 보여줄지 설정
        public static bool Print_ImageBackColor_Black   = true;                            // Print시 Image의 Back Color를 Black으로 설정( true : Black, false : White )
        public static bool Print_StudyAll               = false;                           // Print시 선택된 Study의 정보를 모두 인쇄할지 설정( true: 모두 인쇄 fasle: 현제 보는 정보 )

        // 그래프 변수 
        public static bool IsInterpolation = false;                    // Result Graph Interpolation
        public static double graphInterpolationScale = 25000;          // Result Graph Interpolation Scale (데이터 개수 * Scale)
        public static bool IsLowPassFilter = false;                    // Result Graph Low Pass Filter
        public static double graphYscale = 1.0;                        // Result Graph Yscale
        public static double graphThreshold = 1.5;                     // Result Graph Threshold Value
        public static int medianFilterWindow = 10;                     // Result Graph Median Filter Window Size
        public static double graphSampleRate = 1000;                   // Low Pass Filter Sampling Rate
        public static double graphCutoff = 3;                          // Low Pass Filter Cutoff Value
        public static int baselineStart = 3;                           // 베이스라인 그래프 평균 시작점
        public static int baselineEnd = 15;                            // 베이스라인 그래프 평균 종료점
        public static double baseAvgScale = 1;                         // 베이스라인 그래프 평균 개수 
        public static int thFAMColorIndex = 0;                         // FAM Threshold Color Index 
        public static int thHEXColorIndex = 1;                         // HEX Threshold Color Index 
        public static int thROXColorIndex = 2;                         // ROX Threshold Color Index 
        public static int thCY5ColorIndex = 3;                         // CY5 Threshold Color Index 

        public static Color[] colorList = new Color[40]
        {
            Color.FromArgb( 0x00, 0x00, 0x00 ), Color.FromArgb( 0x99, 0x33, 0x00 ),
            Color.FromArgb( 0x33, 0x33, 0x00 ), Color.FromArgb( 0x00, 0x33, 0x00 ),
            Color.FromArgb( 0x00, 0x33, 0x66 ), Color.FromArgb( 0x00, 0x00, 0x80 ),
            Color.FromArgb( 0x33, 0x33, 0x99 ), Color.FromArgb( 0x33, 0x33, 0x33 ),

            Color.FromArgb( 0x80, 0x00, 0x00 ), Color.FromArgb( 0xFF, 0x66, 0x00 ),
            Color.FromArgb( 0x80, 0x80, 0x00 ), Color.FromArgb( 0x00, 0x80, 0x00 ),
            Color.FromArgb( 0x00, 0x80, 0x80 ), Color.FromArgb( 0x00, 0x00, 0xFF ),
            Color.FromArgb( 0x66, 0x66, 0x99 ), Color.FromArgb( 0x80, 0x80, 0x80 ),

            Color.FromArgb( 0xFF, 0x00, 0x00 ), Color.FromArgb( 0xFF, 0x99, 0x00 ),
            Color.FromArgb( 0x99, 0xCC, 0x00 ), Color.FromArgb( 0x33, 0x99, 0x66 ),
            Color.FromArgb( 0x33, 0xCC, 0xCC ), Color.FromArgb( 0x33, 0x66, 0xFF ),
            Color.FromArgb( 0x80, 0x00, 0x80 ), Color.FromArgb( 0x99, 0x99, 0x99 ),

            Color.FromArgb( 0xFF, 0x00, 0xFF ), Color.FromArgb( 0xFF, 0xCC, 0x00 ),
            Color.FromArgb( 0xFF, 0xFF, 0x00 ), Color.FromArgb( 0x00, 0xFF, 0x00 ),
            Color.FromArgb( 0x00, 0xFF, 0xFF ), Color.FromArgb( 0x00, 0xCC, 0xFF ),
            Color.FromArgb( 0x99, 0x33, 0x66 ), Color.FromArgb( 0xC0, 0xC0, 0xC0 ),

            Color.FromArgb( 0xFF, 0x99, 0xCC ), Color.FromArgb( 0xFF, 0xCC, 0x99 ),
            Color.FromArgb( 0xFF, 0xFF, 0x99 ), Color.FromArgb( 0xCC, 0xFF, 0xCC ),
            Color.FromArgb( 0xCC, 0xFF, 0xFF ), Color.FromArgb( 0x99, 0xCC, 0xFF ),
            Color.FromArgb( 0xCC, 0x99, 0xFF ), Color.FromArgb( 0xFF, 0xFF, 0xFF )
         };

        // ini 정보를 읽어서 Global에 저장한다.
        public static void LoadSetting()
        {
            //ini the file path
            string iniPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.ini");
            if (!File.Exists(iniPath))
                Global.SavedSetting();

            //string sDirPath;
            //sDirPath = string.Format("{0}\\Ref", Application.StartupPath);
            //DirectoryInfo di = new DirectoryInfo(sDirPath);
            //if (di.Exists == false) di.Create();           // 해당 폴더가 없으면 만든다.

            FileIniDataParser parser = new FileIniDataParser();
            //try { parser.ReadFile(iniPath); } catch { Global.SavedSetting(); }        // 내부에 아무것도 없는 경우
            //Parse the ini file
            IniData parsedData = parser.ReadFile(iniPath);

            try { if (parsedData["SYSTEM"]["ScreensIndex"] != null) Global.ScreensIndex = Convert.ToInt32(parsedData["SYSTEM"]["ScreensIndex"]); } catch { }

            // 모니터 인덱스가 연결된 모니터의 겟수보다 큰지 확인
            Screen[] screens;
            screens = Screen.AllScreens;
            // 연결된 모니터 보다 크면 0으로 설정
            if (Global.ScreensIndex >= screens.Length) Global.ScreensIndex = 0;

            // 동영상 저장 기능 사용 여부
            try { if (parsedData["SYSTEM"]["IsUsedSavedVideo"] != null) Global.IsUsedSavedVideo = Convert.ToBoolean(parsedData["SYSTEM"]["IsUsedSavedVideo"]); } catch { }
            // 파일 저장 위치 
            try { if (parsedData["SYSTEM"]["SavePath"] != null) Global.SavePath = parsedData["SYSTEM"]["SavePath"]; } catch { }

            // RPROPERTIES 설정 읽어오기 
            try { if (parsedData["PROPERTIES"]["UserName"] != null) Global.UserName = parsedData["PROPERTIES"]["UserName"]; } catch { }
            try { if (parsedData["PROPERTIES"]["Barcode"] != null) Global.Barcode = parsedData["PROPERTIES"]["Barcode"]; } catch { }
            try { if (parsedData["PROPERTIES"]["InstrumentType"] != null) Global.InstrumentType = parsedData["PROPERTIES"]["InstrumentType"]; } catch { }
            try { if (parsedData["PROPERTIES"]["BlockType"] != null) Global.BlockType = parsedData["PROPERTIES"]["BlockType"]; } catch { }
            try { if (parsedData["PROPERTIES"]["ExperimentType"] != null) Global.ExperimentType = Convert.ToInt32(parsedData["PROPERTIES"]["ExperimentType"]); } catch { }
            try { if (parsedData["PROPERTIES"]["Chemisty"] != null) Global.Chemisty = parsedData["PROPERTIES"]["Chemisty"]; } catch { }
            try { if (parsedData["PROPERTIES"]["RunMode"] != null) Global.RunMode = Convert.ToInt32(parsedData["PROPERTIES"]["RunMode"]); } catch { }
            try { if (parsedData["PROPERTIES"]["Volume"] != null) Global.Volume = parsedData["PROPERTIES"]["Volume"]; } catch { }
            try { if (parsedData["PROPERTIES"]["Cover"] != null) Global.Cover = parsedData["PROPERTIES"]["Cover"]; } catch { }
            try { if (parsedData["PROPERTIES"]["MethodPath"] != null) Global.MethodPath = parsedData["PROPERTIES"]["MethodPath"]; } catch { }
            try { if (parsedData["PROPERTIES"]["PlatePath"] != null) Global.PlatePath = parsedData["PROPERTIES"]["PlatePath"]; } catch { }
            try { if (parsedData["PROPERTIES"]["ResultPath"] != null) Global.ResultPath = parsedData["PROPERTIES"]["ResultPath"]; } catch { }

            // ROI 설정 읽어오기 
            try { if (parsedData["ROI"]["Roi_FontSize"] != null)    Global.Roi_FontSize = Convert.ToInt32(parsedData["ROI"]["Roi_FontSize"]); } catch { }
            try { if (parsedData["ROI"]["Roi_BorderWidth"] != null) Global.Roi_BorderWidth = Convert.ToInt32(parsedData["ROI"]["Roi_BorderWidth"]); } catch { }
            try { if (parsedData["ROI"]["Roi_Option"] != null)      Global.Roi_Option = Convert.ToInt32(parsedData["ROI"]["Roi_Option"]); } catch { }

            try { if (parsedData["ROI"]["RoiCount"] != null)        Global.RoiCount = Convert.ToInt32(parsedData["ROI"]["RoiCount"]); } catch { }
            //listRoiInfos.Clear();
            Global.listRoiInfos.Clear();
            for (int i=0; i< Global.RoiCount; i++)
            {
                int No = i + 1;
                string title = String.Format("ROI{0}", No.ToString());
                string roiInfo = null;
                try { if (parsedData["ROI"][title] != null) roiInfo = parsedData["ROI"][title]; } catch { }

                if (roiInfo != null)
                {
                    string[] roiString = roiInfo.Split(';');
                    //IpRoiInfo stRoi;
                    //stRoi.left = Convert.ToInt32(words[0]);
                    //stRoi.top = Convert.ToInt32(words[1]);
                    //stRoi.right = Convert.ToInt32(words[2]);
                    //stRoi.bottom = Convert.ToInt32(words[3]);
                    //stRoi.Offset = Convert.ToDouble(words[4]);
                    //stRoi.Gain = Convert.ToDouble(words[5]);
                    //Global.listRoiInfos.Add(stRoi);

                    ROISHAPETYPE roiShapeType = ROISHAPETYPE.Ellipse;
                    ROIShape roi = new ROIShape(roiShapeType)
                    {
                        ChartNo = "0",   //img.ChartNo,               // 기준 Image가 포함하는 Study의 Patient ChartNo.
                        StudyID = 0,     //img.StudyID,               // 기준 Image가 포함하는 Study ID
                        ImageIndex = 0,  //img.ImageIndex             // 기준 Image Inddex
                    };

                    // 해당 ROI와 연결될 ROI가 포함된 study의 Patient Chart No를 저장한다.
                    roi.Connect_ChartNo = roiString[1];
                    // 해당 ROI와 연결될 ROI가 포함된 study index를 저장한다.
                    try { roi.Connect_StudyID = Convert.ToInt32(roiString[2]); } catch { continue; }
                    // 해당 ROI와 연결될 ROI가 포함되 Image Index를 저장한다.
                    try { roi.Connect_ImageIndex = Convert.ToInt32(roiString[3]); } catch { continue; }
                    // 해당 ROI와 연결될 ROI가 포함되 Image의 ROI ID를 저장한다.
                    try { roi.Connect_ROIID = Convert.ToInt32(roiString[4]); } catch { continue; }

                    // ROI ID를 저장한다.
                    roi.ROIID = Global.listRoiInfos.Count;        // ROI ID를 입력한다. listShape의 지금 Count가 Index가 된다.
                    try { roi.ROIID = Convert.ToInt32(roiString[5]); } catch { continue; }

                    // ROI Main Index를 저장한다.
                    try { roi.ROI_MainIndex = Convert.ToInt32(roiString[6]); } catch { continue; }
                    // ROI Sub Index를 저장한다.
                    try { roi.ROI_SubIndex = Convert.ToInt32(roiString[7]); } catch { continue; }
                    // ROI의 기준 Image의 넓이
                    try { roi.Image_Width = Convert.ToDouble(roiString[8]); } catch { continue; }
                    // ROI의 기준 Image의 높이
                    try { roi.Image_Height = Convert.ToDouble(roiString[9]); } catch { continue; }

                    // ROI내의 Min 값
                    try { roi.ROI_MinValue = Convert.ToDouble(roiString[10]); } catch { }
                    // ROI내의 Max 값
                    try { roi.ROI_MaxValue = Convert.ToDouble(roiString[11]); } catch { }
                    // ROI의 Offset 값
                    try { roi.ROI_Offset = Convert.ToDouble(roiString[12]); } catch { }
                    // ROI의 Gain 값
                    try { roi.ROI_Gain = Convert.ToDouble(roiString[13]); } catch { }
                    // ROI내의 평균값 값
                    try { roi.ROI_Average = Convert.ToDouble(roiString[14]); } catch { }
                    // ROI내의 표준편차 값
                    try { roi.ROI_Sdnn = Convert.ToDouble(roiString[15]); } catch { }
                    // ROI를 포함하는 Area 폭(Image 기준 폭이다. )
                    try { roi.ROI_Width = Convert.ToDouble(roiString[16]); } catch { }
                    // ROI를 포함하는 Area 높이(Image 기준 높이이다. )
                    try { roi.ROI_Height = Convert.ToDouble(roiString[17]); } catch { }
                    // ROI의 실제 면적(Image 기준 면적이다. )
                    try { roi.ROI_Area = Convert.ToDouble(roiString[18]); } catch { }
                    //  ROI Point 정보를 읽어온다. 점과 점 사이는 "/"로 구분한다.
                    List<PointF> readPoints = new List<PointF>();
                    try
                    {
                        string[] pointString = roiString[19].Split('/');                           // '/'구분으로 Point를 저장했다.

                        foreach (var pos in pointString)
                        {
                            string[] xyString = pos.Split(',');                                 // ','구분으로 x,y 값을 저장함
                            if (xyString.Length == 2)
                            {
                                float imageX = (float)Convert.ToDouble(xyString[0]);                  // Image Width 기준의 x 값
                                float imageY = (float)Convert.ToDouble(xyString[1]);                  // Image Height 기준의 y 값

                                readPoints.Add(new PointF(imageX, imageY));
                            }
                        }
                    }
                    catch { continue; }

                    float roiAngle = 0, roiCenterX = 0, roiCenterY = 0, roiWidth = 0, roiHeight = 0;
                    try
                    {
                        if (roiShapeType == ROISHAPETYPE.Rectangle || roiShapeType == ROISHAPETYPE.Diamond || roiShapeType == ROISHAPETYPE.Ellipse)
                        {
                            string[] rotatedAreaString = roiString[20].Split(',');
                            if (rotatedAreaString.Length == 5)
                            {
                                roiAngle = (float)Convert.ToDouble(rotatedAreaString[0]);     // this.rotatedArea.Angle
                                roiCenterX = (float)Convert.ToDouble(rotatedAreaString[1]);     // {this.rotatedArea.Center.X
                                roiCenterY = (float)Convert.ToDouble(rotatedAreaString[2]);     // {this.rotatedArea.Center.Y
                                roiWidth = (float)Convert.ToDouble(rotatedAreaString[3]);     // {this.rotatedArea.Size.Width
                                roiHeight = (float)Convert.ToDouble(rotatedAreaString[4]);     // {this.rotatedArea.Size.Height}";
                            }

                        }
                        else
                        {
                            roiAngle = (float)Convert.ToDouble(roiString[20]);
                        }
                    }
                    catch { }

                    if (roi.LoadROI(readPoints.ToArray(), roiAngle, roiCenterX, roiCenterY, roiWidth, roiHeight))
                        Global.listRoiInfos.Add(roi);
                }
            }

            // Optic 설정 읽어오기
            try { if (parsedData["OPTIC"]["accelSpeed"] != null) Global.accelSpeed = Convert.ToInt32(parsedData["OPTIC"]["accelSpeed"]); } catch { }            // Step Motor 가속도 
            try { if (parsedData["OPTIC"]["coarseSpeed"] != null) Global.coarseSpeed = Convert.ToInt32(parsedData["OPTIC"]["coarseSpeed"]); } catch { }         // Step Motor 속도 
            try { if (parsedData["OPTIC"]["fineSpeed"] != null) Global.fineSpeed = Convert.ToInt32(parsedData["OPTIC"]["fineSpeed"]); } catch { }               // Step Motor 안정속도 
            try { if (parsedData["OPTIC"]["maxSpeed"] != null) Global.maxSpeed = Convert.ToInt32(parsedData["OPTIC"]["maxSpeed"]); } catch { }                  // Step Motor 최대속도 
            try { if (parsedData["OPTIC"]["ledOn"] != null) Global.ledOn = Convert.ToInt32(parsedData["OPTIC"]["ledOn"]); } catch { }                           // LED On
            try { if (parsedData["OPTIC"]["ledOff"] != null) Global.ledOff = Convert.ToInt32(parsedData["OPTIC"]["ledOff"]); } catch { }                        // LED Off
            try { if (parsedData["OPTIC"]["trayIn"] != null) Global.trayIn = Convert.ToInt32(parsedData["OPTIC"]["trayIn"]); } catch { }                        // Tray In
            try { if (parsedData["OPTIC"]["trayOut"] != null) Global.trayOut = Convert.ToInt32(parsedData["OPTIC"]["trayOut"]); } catch { }                     // Tray Out
            try { if (parsedData["OPTIC"]["heaterUp"] != null) Global.heaterUp = Convert.ToInt32(parsedData["OPTIC"]["heaterUp"]); } catch { }                  // LidHeader Up 
            try { if (parsedData["OPTIC"]["heaterDown"] != null) Global.heaterDown = Convert.ToInt32(parsedData["OPTIC"]["heaterDown"]); } catch { }            // LidHeader Down
            try { if (parsedData["OPTIC"]["filterFAM_Pos"] != null) Global.filterFAM_Pos = Convert.ToInt32(parsedData["OPTIC"]["filterFAM_Pos"]); } catch { }   // FAM Filter 위치 
            try { if (parsedData["OPTIC"]["filterHEX_Pos"] != null) Global.filterHEX_Pos = Convert.ToInt32(parsedData["OPTIC"]["filterHEX_Pos"]); } catch { }   // HEX Filter 위치 
            try { if (parsedData["OPTIC"]["filterROX_Pos"] != null) Global.filterROX_Pos = Convert.ToInt32(parsedData["OPTIC"]["filterROX_Pos"]); } catch { }   // ROX Filter 위치 
            try { if (parsedData["OPTIC"]["filterCY5_Pos"] != null) Global.filterCY5_Pos = Convert.ToInt32(parsedData["OPTIC"]["filterCY5_Pos"]); } catch { }   // CY5 Filter 위치 
            try { if (parsedData["OPTIC"]["filterMoveDelay"] != null) Global.filterMoveDelay = Convert.ToInt32(parsedData["OPTIC"]["filterMoveDelay"]); } catch { }   // 필터를 움직이는데 소요되는 시간 

            // CAMERA 설정 읽어오기
            try { if (parsedData["CAMERA"]["CAMERA_IMAGE_X"] != null) Global.SAVEDIMAGE_X = Convert.ToInt32(parsedData["CAMERA"]["CAMERA_IMAGE_X"]); } catch { }
            try { if (parsedData["CAMERA"]["CAMERA_IMAGE_X"] != null) Global.SAVEDIMAGE_Y = Convert.ToInt32(parsedData["CAMERA"]["CAMERA_IMAGE_Y"]); } catch { }

            try { if (parsedData["CAMERA"]["LowTemperature"] != null) Global.LowTemperature = Convert.ToDouble(parsedData["CAMERA"]["LowTemperature"]); } catch { }
            try { if (parsedData["CAMERA"]["HighTemperature"] != null) Global.HighTemperature = Convert.ToDouble(parsedData["CAMERA"]["HighTemperature"]); } catch { }

            try { if (parsedData["CAMERA"]["ccdCameraNo"] != null) Global.ccdCameraNo = Convert.ToInt32(parsedData["CAMERA"]["ccdCameraNo"]); } catch { }               // CCD Camerea 번호  
            try { if (parsedData["CAMERA"]["ccdExposure"] != null) Global.ccdExposure = Convert.ToInt32(parsedData["CAMERA"]["ccdExposure"]); } catch { }               // CCD Exposure 값  
            try { if (parsedData["CAMERA"]["ExposureTime"] != null) Global.ExposureTime = Convert.ToInt32(parsedData["CAMERA"]["ExposureTime"]); } catch { }            // CCD Exposure 시간 (ExposureTime * 3)  
            try { if (parsedData["CAMERA"]["isAutoExposure"] != null) Global.isAutoExposure = Convert.ToBoolean(parsedData["CAMERA"]["isAutoExposure"]); } catch { }    // CCD Exposure 값  
            try { if (parsedData["CAMERA"]["ccdFocus"] != null) Global.ccdFocus = Convert.ToInt32(parsedData["CAMERA"]["ccdFocus"]); } catch { }               // CCD Focus 값 (1~1023) 
            try { if (parsedData["CAMERA"]["isAutoFocus"] != null) Global.isAutoFocus = Convert.ToBoolean(parsedData["CAMERA"]["isAutoFocus"]); } catch { }    // CCD AutoFocus 값  
            try { if (parsedData["CAMERA"]["ccdWB"] != null) Global.ccdWB = Convert.ToInt32(parsedData["CAMERA"]["ccdWB"]); } catch { }                  // CCD Focus 값 (2800~6500) 
            try { if (parsedData["CAMERA"]["isAutoWB"] != null) Global.isAutoWB = Convert.ToBoolean(parsedData["CAMERA"]["isAutoWB"]); } catch { }       // CCD AutoFocus 값  
            try { if (parsedData["CAMERA"]["ccdCompensation"] != null) Global.ccdCompensation = Convert.ToInt32(parsedData["CAMERA"]["ccdCompensation"]); } catch { }    // CCD Back Light Compensation 값  (0~2)
            try { if (parsedData["CAMERA"]["ccdGain"] != null) Global.ccdGain = Convert.ToInt32(parsedData["CAMERA"]["ccdGain"]); } catch { }            // CCD Gain 값 (0~6)   
            try { if (parsedData["CAMERA"]["ccdBrightness"] != null) Global.ccdBrightness = Convert.ToInt32(parsedData["CAMERA"]["ccdBrightness"]); } catch { }     // CCD Brightness 값  
            try { if (parsedData["CAMERA"]["ccdContrast"] != null) Global.ccdContrast = Convert.ToInt32(parsedData["CAMERA"]["ccdContrast"]); } catch { }           // CCD Contrast 값  
            try { if (parsedData["CAMERA"]["ccdSharpness"] != null) Global.ccdSharpness = Convert.ToInt32(parsedData["CAMERA"]["ccdSharpness"]); } catch { }        // CCD Sharpness 값  
            try { if (parsedData["CAMERA"]["ccdGamma"] != null) Global.ccdGamma = Convert.ToInt32(parsedData["CAMERA"]["ccdGamma"]); } catch { }                    // CCD Gamma 값  

            Global.finalExposureTime = Global.ExposureTime * 3;

            // Filter 설정 읽어오기
            try { if (parsedData["FILTER"]["IsMedianBlur"] != null) Global.IsMedianBlur = Convert.ToBoolean(parsedData["FILTER"]["IsMedianBlur"]); } catch { }
            try { if (parsedData["FILTER"]["MedianKsize"] != null) Global.MedianKsize = Convert.ToInt32(parsedData["FILTER"]["MedianKsize"]); } catch { }
            try { if (parsedData["FILTER"]["IsGaussianBlur"] != null) Global.IsGaussianBlur = Convert.ToBoolean(parsedData["FILTER"]["IsGaussianBlur"]); } catch { }
            try { if (parsedData["FILTER"]["GaussianKSizeX"] != null) Global.GaussianKSizeX = Convert.ToInt32(parsedData["FILTER"]["GaussianKSizeX"]); } catch { }
            try { if (parsedData["FILTER"]["GaussianKSizeY"] != null) Global.GaussianKSizeY = Convert.ToInt32(parsedData["FILTER"]["GaussianKSizeY"]); } catch { }
            try { if (parsedData["FILTER"]["GaussianSigmaX"] != null) Global.GaussianSigmaX = Convert.ToDouble(parsedData["FILTER"]["GaussianSigmaX"]); } catch { }
            try { if (parsedData["FILTER"]["GaussianSigmaY"] != null) Global.GaussianSigmaY = Convert.ToDouble(parsedData["FILTER"]["GaussianSigmaY"]); } catch { }
            try { if (parsedData["FILTER"]["IsBilateralFilter"] != null) Global.IsBilateralFilter = Convert.ToBoolean(parsedData["FILTER"]["IsBilateralFilter"]); } catch { }
            try { if (parsedData["FILTER"]["BilateralD"] != null) Global.BilateralD = Convert.ToInt32(parsedData["FILTER"]["BilateralD"]); } catch { }
            try { if (parsedData["FILTER"]["BilateralSigmaX"] != null) Global.BilateralSigmaX = Convert.ToDouble(parsedData["FILTER"]["BilateralSigmaX"]); } catch { }
            try { if (parsedData["FILTER"]["BilateralSigmaY"] != null) Global.BilateralSigmaY = Convert.ToDouble(parsedData["FILTER"]["BilateralSigmaY"]); } catch { }
            try { if (parsedData["FILTER"]["IsBlur"] != null) Global.IsBlur = Convert.ToBoolean(parsedData["FILTER"]["IsBlur"]); } catch { }
            try { if (parsedData["FILTER"]["BlurKSizeX"] != null) Global.BlurKSizeX = Convert.ToInt32(parsedData["FILTER"]["BlurKSizeX"]); } catch { }
            try { if (parsedData["FILTER"]["BlurKSizeY"] != null) Global.BlurKSizeY = Convert.ToInt32(parsedData["FILTER"]["BlurKSizeY"]); } catch { }
            try { if (parsedData["FILTER"]["BlurAnchorX"] != null) Global.BlurAnchorX = Convert.ToInt32(parsedData["FILTER"]["BlurAnchorX"]); } catch { }
            try { if (parsedData["FILTER"]["BlurAnchorY"] != null) Global.BlurAnchorY = Convert.ToInt32(parsedData["FILTER"]["BlurAnchorY"]); } catch { }
            try { if (parsedData["FILTER"]["IsBoxFilter"] != null) Global.IsBoxFilter = Convert.ToBoolean(parsedData["FILTER"]["IsBoxFilter"]); } catch { }
            try { if (parsedData["FILTER"]["BoxKSizeX"] != null) Global.BoxKSizeX = Convert.ToInt32(parsedData["FILTER"]["BoxKSizeX"]); } catch { }
            try { if (parsedData["FILTER"]["BoxKSizeY"] != null) Global.BoxKSizeY = Convert.ToInt32(parsedData["FILTER"]["BoxKSizeY"]); } catch { }
            try { if (parsedData["FILTER"]["IsSharpen"] != null) Global.IsSharpen = Convert.ToBoolean(parsedData["FILTER"]["IsSharpen"]); } catch { }
            try { if (parsedData["FILTER"]["IsMorphology"] != null) Global.IsMorphology = Convert.ToBoolean(parsedData["FILTER"]["IsMorphology"]); } catch { }
            try { if (parsedData["FILTER"]["IsMeansDenoising"] != null) Global.IsMeansDenoising = Convert.ToBoolean(parsedData["FILTER"]["IsMeansDenoising"]); } catch { }
            // Print 설정 읽어오기
            try { if (parsedData["PRINT"]["MarginsLeft"] != null) Global.MarginsLeft = Convert.ToDouble(parsedData["PRINT"]["MarginsLeft"]); } catch { }
            try { if (parsedData["PRINT"]["MarginsRight"] != null) Global.MarginsRight = Convert.ToDouble(parsedData["PRINT"]["MarginsRight"]); } catch { }
            try { if (parsedData["PRINT"]["MarginsTop"] != null) Global.MarginsTop = Convert.ToDouble(parsedData["PRINT"]["MarginsTop"]); } catch { }
            try { if (parsedData["PRINT"]["MarginsBottom"] != null) Global.MarginsBottom = Convert.ToDouble(parsedData["PRINT"]["MarginsBottom"]); } catch { }
            try { if (parsedData["PRINT"]["Header_Left"] != null) Global.Header_Left = parsedData["PRINT"]["Header_Left"]; } catch { }
            try { if (parsedData["PRINT"]["Header_Right"] != null) Global.Header_Right = parsedData["PRINT"]["Header_Right"]; } catch { }
            try { if (parsedData["PRINT"]["Footer_Left1"] != null) Global.Footer_Left1 = parsedData["PRINT"]["Footer_Left1"]; } catch { }
            try { if (parsedData["PRINT"]["Footer_Left2"] != null) Global.Footer_Left2 = parsedData["PRINT"]["Footer_Left2"]; } catch { }

            try { if (parsedData["PRINT"]["Logo_Show"]              != null) Global.Print_LOGO_Show             = Convert.ToBoolean(parsedData["PRINT"]["Logo_Show"]); }            catch { }
            try { if (parsedData["PRINT"]["PaletteBar_Show"]        != null) Global.Print_PaletteBar_Show       = Convert.ToBoolean(parsedData["PRINT"]["PaletteBar_Show"]); }      catch { }
            try { if (parsedData["PRINT"]["ROI_Show"]               != null) Global.Print_ROI_Show              = Convert.ToBoolean(parsedData["PRINT"]["ROI_Show"]); }             catch { }
            try { if (parsedData["PRINT"]["Diff_ROI_Display"]       != null) Global.Print_Diff_ROI_Display      = Convert.ToBoolean(parsedData["PRINT"]["Diff_ROI_Display"]); }     catch { }
            try { if (parsedData["PRINT"]["ImageBackColor_Black"]   != null) Global.Print_ImageBackColor_Black  = Convert.ToBoolean(parsedData["PRINT"]["ImageBackColor_Black"]); } catch { }
            try { if (parsedData["PRINT"]["Print_StudyAll"]         != null) Global.Print_StudyAll              = Convert.ToBoolean(parsedData["PRINT"]["Print_StudyAll"]); }       catch { }

            // 시리얼 설정(Camera Control)
            try { if (parsedData["SERIAL"]["ArducamPort"] != null)      Global.ArducamPort = parsedData["SERIAL"]["ArducamPort"]; } catch { }
            try { if (parsedData["SERIAL"]["ArducamBaudRate"] != null)  Global.ArducamBaudRate = Convert.ToInt32(parsedData["SERIAL"]["ArducamBaudRate"]); } catch { }
            try { if (parsedData["SERIAL"]["OpticPort"] != null)        Global.OpticPort = parsedData["SERIAL"]["OpticPort"]; } catch { }
            try { if (parsedData["SERIAL"]["OpticBaudRate"] != null)    Global.OpticBaudRate = Convert.ToInt32(parsedData["SERIAL"]["OpticBaudRate"]); } catch { }

            // PCR PROTOCOL 정보 
            try { if (parsedData["PROTOCOL"]["PCR_VOLUME"] != null) Global.PCR_VOLUME = Convert.ToDouble(parsedData["PROTOCOL"]["PCR_VOLUME"]); } catch { }
            try { if (parsedData["PROTOCOL"]["HEATER_TEMP"] != null) Global.HEATER_TEMP = Convert.ToDouble(parsedData["PROTOCOL"]["HEATER_TEMP"]); } catch { }

            // Result Graph
            try { if (parsedData["GRAPH"]["graphYscale"] != null) Global.graphYscale = Convert.ToDouble(parsedData["GRAPH"]["graphYscale"]); } catch { }
            try { if (parsedData["GRAPH"]["graphThreshold"] != null) Global.graphThreshold = Convert.ToDouble(parsedData["GRAPH"]["graphThreshold"]); } catch { }
            try { if (parsedData["GRAPH"]["medianFilterWindow"] != null) Global.medianFilterWindow = Convert.ToInt32(parsedData["GRAPH"]["medianFilterWindow"]); } catch { }
            try { if (parsedData["GRAPH"]["IsInterpolation"] != null) Global.IsInterpolation = Convert.ToBoolean(parsedData["GRAPH"]["IsInterpolation"]); } catch { }
            try { if (parsedData["GRAPH"]["graphInterpolationScale"] != null) Global.graphInterpolationScale = Convert.ToDouble(parsedData["GRAPH"]["graphInterpolationScale"]); } catch { }
            try { if (parsedData["GRAPH"]["graphSampleRate"] != null) Global.graphSampleRate = Convert.ToDouble(parsedData["GRAPH"]["graphSampleRate"]); } catch { }
            try { if (parsedData["GRAPH"]["graphCutoff"] != null) Global.graphCutoff = Convert.ToDouble(parsedData["GRAPH"]["graphCutoff"]); } catch { }
            try { if (parsedData["GRAPH"]["baselineStart"] != null) Global.baselineStart = Convert.ToInt32(parsedData["GRAPH"]["baselineStart"]); } catch { }
            try { if (parsedData["GRAPH"]["baselineEnd"] != null) Global.baselineEnd = Convert.ToInt32(parsedData["GRAPH"]["baselineEnd"]); } catch { }
            try { if (parsedData["GRAPH"]["thFAMColorIndex"] != null) Global.thFAMColorIndex = Convert.ToInt32(parsedData["GRAPH"]["thFAMColorIndex"]); } catch { }
            try { if (parsedData["GRAPH"]["thHEXColorIndex"] != null) Global.thHEXColorIndex = Convert.ToInt32(parsedData["GRAPH"]["thHEXColorIndex"]); } catch { }
            try { if (parsedData["GRAPH"]["thROXColorIndex"] != null) Global.thROXColorIndex = Convert.ToInt32(parsedData["GRAPH"]["thROXColorIndex"]); } catch { }
            try { if (parsedData["GRAPH"]["thCY5ColorIndex"] != null) Global.thCY5ColorIndex = Convert.ToInt32(parsedData["GRAPH"]["thCY5ColorIndex"]); } catch { }

            Global.SavedSetting();
        }

        // Global 정보를 ini에 저장한다.
        public static void SavedSetting()
        {
            //Create ini file
            FileIniDataParser parser = new FileIniDataParser();
            IniData savedData = new IniData();

            // System 
            savedData.Sections.AddSection("SYSTEM");
            savedData["SYSTEM"]["ScreensIndex"] = Global.ScreensIndex.ToString();           // Screen 표시 위치

            savedData["SYSTEM"]["IsUsedSavedVideo"] = Global.IsUsedSavedVideo.ToString();             // 동영상 저장 기능 사용 여부

            savedData["SYSTEM"]["SaveFileNameType"] = Global.SaveFileNameType;                        // 파일 저장 이름 형식
            savedData["SYSTEM"]["IsSavePDF"]        = Global.IsSavePDF.ToString();                    // 파일 저장 형식  true : PDF, false : JPGE
            savedData["SYSTEM"]["SavePath"]         = Global.SavePath;                                // 파일 저장 위치

            // RPROPERTIES 설정 읽어오기 
            savedData.Sections.AddSection("PROPERTIES");
            savedData["PROPERTIES"]["UserName"] = Global.UserName;                          
            savedData["PROPERTIES"]["Barcode"] = Global.Barcode;                            
            savedData["PROPERTIES"]["InstrumentType"] = Global.InstrumentType;              
            savedData["PROPERTIES"]["BlockType"] = Global.BlockType;                        
            savedData["PROPERTIES"]["ExperimentType"] = Global.ExperimentType.ToString();   
            savedData["PROPERTIES"]["Chemisty"] = Global.Chemisty;                          
            savedData["PROPERTIES"]["RunMode"] = Global.RunMode.ToString();                 
            savedData["PROPERTIES"]["Volume"] = Global.Volume;                              
            savedData["PROPERTIES"]["Cover"] = Global.Cover;                                
            //savedData["PROPERTIES"]["MethodPath"] = Global.MethodPath;                      
            //savedData["PROPERTIES"]["PlatePath"] = Global.PlatePath;                        
            //savedData["PROPERTIES"]["ResultPath"] = Global.ResultPath;                      

            // ROI
            savedData.Sections.AddSection("ROI");
            savedData["ROI"]["RoiCount"] = listRoiInfos.Count.ToString();                    // ROI Font Size 
            for (int roiIndex = 0; roiIndex < listRoiInfos.Count; roiIndex++)
            {
                ROIShape roi = listRoiInfos[roiIndex];
                string roiPointString = "";
                foreach (var p in roi.imagePointInfo)
                {
                    roiPointString += String.Format("{0},{1}/", p.X, p.Y);
                }

                string roiInfo = String.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21}",
                    (int)roi.ShapeType,                 // ROI Type를 저장한다.                                                    0
                    roi.Connect_ChartNo,                // 해당 ROI와 연결될 ROI가 포함된 study의 Patient Chart No를 저장한다.     1
                    roi.Connect_StudyID,                // 해당 ROI와 연결될 ROI가 포함된 study index를 저장한다.                  2  
                    roi.Connect_ImageIndex,             // 해당 ROI와 연결될 ROI가 포함되 Image Index를 저장한다.                  3
                    roi.Connect_ROIID,                  // 해당 ROI와 연결될 ROI가 포함되 Image의 ROI Index를 저장한다.            4
                    roi.ROIID,                          // ROI Index를 저장한다.                                                   5
                    roi.ROI_MainIndex,                  // ROI Main Index를 저장한다.                                              6
                    roi.ROI_SubIndex,                   // ROI Sub Index를 저장한다.                                               7
                    roi.Image_Width,                    // ROI의 기준 Image의 넓이                                                 8
                    roi.Image_Height,                   // ROI의 기준 Image의 높이                                                 9
                    roi.ROI_MinValue,                   // ROI내의 Min 값                                                          10
                    roi.ROI_MaxValue,                   // ROI내의 Max 값                                                          11  
                    roi.ROI_Offset,                     // ROI의 Offset 값                                                         12
                    roi.ROI_Gain,                       // ROI의 Gain 값                                                           13  
                    roi.ROI_Average,                    // ROI내의 평균값 값                                                       14
                    roi.ROI_Sdnn,                       // ROI내의 표준편차 값                                                     15
                    roi.ROI_Width,                      // ROI를 포함하는 Area 폭(Image 기준 폭이다. )                             16
                    roi.ROI_Height,                     // ROI를 포함하는 Area 높이(Image 기준 높이이다. )                         17    
                    roi.ROI_Area,                       // ROI의 실제 면적(Image 기준 면적이다. )                                  18
                    roiPointString,                     // ROI Point 정보를 입력한다. 점과 점 사이는 "/"로 구분한다.               19
                    roi.SaveROI(),                      // ROI의 추가정보 아래 확인                                                20
                    roi.ROI_Diff                        // ROI의 차이                                                              21
                    );

                int No = roiIndex + 1;
                string title = String.Format("ROI{0}", No.ToString());
                savedData["ROI"][title] = roiInfo;                    // ROI 정보 
            }

            savedData["ROI"]["Roi_FontSize"] =      Global.Roi_FontSize.ToString();                    // ROI Font Size 
            savedData["ROI"]["Roi_BorderWidth"] =   Global.Roi_BorderWidth.ToString();                    // ROI Line Thickness
            savedData["ROI"]["Roi_Option"] =        Global.Roi_Option.ToString();                    // ROI Display Option 

            // Optic 설정 저장
            savedData.Sections.AddSection("OPTIC");
            savedData["OPTIC"]["coarseSpeed"] = Global.coarseSpeed.ToString();     // Step Motor 속도 
            savedData["OPTIC"]["fineSpeed"] = Global.fineSpeed.ToString();         // Step Motor 안정속도 
            savedData["OPTIC"]["maxSpeed"] = Global.maxSpeed.ToString();           // Step Motor 최대속도 
            savedData["OPTIC"]["accelSpeed"] = Global.accelSpeed.ToString();       // Step Motor 가속도 
            savedData["OPTIC"]["ledOn"] = Global.ledOn.ToString();                 // LED On
            savedData["OPTIC"]["ledOff"] = Global.ledOff.ToString();               // LED Off
            savedData["OPTIC"]["trayIn"] = Global.trayIn.ToString();               // Tray In
            savedData["OPTIC"]["trayOut"] = Global.trayOut.ToString();             // Tray Out
            savedData["OPTIC"]["heaterUp"] = Global.heaterUp.ToString();           // LidHeader Up 
            savedData["OPTIC"]["heaterDown"] = Global.heaterDown.ToString();       // LidHeader Down
            savedData["OPTIC"]["filterFAM_Pos"] = Global.filterFAM_Pos.ToString(); // FAM Filter 위치 
            savedData["OPTIC"]["filterHEX_Pos"] = Global.filterHEX_Pos.ToString(); // HEX Filter 위치 
            savedData["OPTIC"]["filterROX_Pos"] = Global.filterROX_Pos.ToString(); // ROX Filter 위치 
            savedData["OPTIC"]["filterCY5_Pos"] = Global.filterCY5_Pos.ToString(); // CY5 Filter 위치 
            savedData["OPTIC"]["filterMoveDelay"] = Global.filterMoveDelay.ToString(); // 필터를 움직이는데 소요되는 시간

            savedData.Sections.AddSection("CAMERA");
            savedData["CAMERA"]["CAMERA_IMAGE_X"] = Global.SAVEDIMAGE_X.ToString();           // 보여줄 이미지 x Resultion ( 원본 영상이 설정된 값보다 크면 원본영상 사용 )
            savedData["CAMERA"]["CAMERA_IMAGE_Y"] = Global.SAVEDIMAGE_Y.ToString();           // 보여줄 이미지 y Resultion ( 원본 영상이 설정된 값보다 크면 원본영상 사용 )
            savedData["CAMERA"]["LowTemperature"] = Global.LowTemperature.ToString();         // 영상의 최저 온도값( -> 0  ) ( 추후에 Setting으로 처리해야함.)
            savedData["CAMERA"]["HighTemperature"] = Global.HighTemperature.ToString();        // 영상의 최고 온도값( -> 255) ( 추후에 Setting으로 처리해야함.)

            savedData["CAMERA"]["ccdCameraNo"] = Global.ccdCameraNo.ToString();        // CCD Camerea 번호  
            savedData["CAMERA"]["ccdExposure"] = Global.ccdExposure.ToString();        // CCD Exposure 값  
            savedData["CAMERA"]["ExposureTime"] = Global.ExposureTime.ToString();      // CCD Exposure 시간 (ExposureTime * 3) 
            savedData["CAMERA"]["isAutoExposure"] = Global.isAutoExposure.ToString();  // CCD Auto Exposure 값  
            savedData["CAMERA"]["ccdFocus"] = Global.ccdFocus.ToString();                       // CCD Focus 값 (1~1023) 
            savedData["CAMERA"]["isAutoFocus"] = Global.isAutoFocus.ToString();                 // CCD AutoFocus 값  
            savedData["CAMERA"]["ccdWB"] = Global.ccdWB.ToString();                             // CCD Focus 값 (2800~6500) 
            savedData["CAMERA"]["isAutoWB"] = Global.isAutoWB.ToString();                       // CCD AutoFocus 값  
            savedData["CAMERA"]["ccdCompensation"] = Global.ccdCompensation.ToString();         // CCD Back Light Compensation 값  (0~2)
            savedData["CAMERA"]["ccdGain"] = Global.ccdGain.ToString();                         // CCD Gain 값 (0~6)   
            savedData["CAMERA"]["ccdBrightness"] = Global.ccdBrightness.ToString();    // CCD Brightness 값  
            savedData["CAMERA"]["ccdContrast"] = Global.ccdContrast.ToString();        // CCD Contrast 값  
            savedData["CAMERA"]["ccdSharpness"] = Global.ccdSharpness.ToString();      // CCD Sharpness 값  
            savedData["CAMERA"]["ccdGamma"] = Global.ccdGamma.ToString();              // CCD Gamma 값  

            // Filter 적용 여부 설정
            savedData.Sections.AddSection("FILTER");
            savedData["FILTER"]["IsMedianBlur"] = Global.IsMedianBlur.ToString();               // MedianBlur Filter 사용 여부
            savedData["FILTER"]["MedianKsize"]  = Global.MedianKsize.ToString();                //필터의 크기(1이상의 홀수 값) (Note – 생성된 결과 필터는 ksize x ksize의 크기를 갖는다.)

            savedData["FILTER"]["IsGaussianBlur"] = Global.IsGaussianBlur.ToString();             // GaussianBlur Filter 사용 여부
            savedData["FILTER"]["GaussianKSizeX"] = Global.GaussianKSizeX.ToString();             // 가우시안 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            savedData["FILTER"]["GaussianKSizeY"] = Global.GaussianKSizeY.ToString();            // 가우시안 커널의 Y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            savedData["FILTER"]["GaussianSigmaX"] = Global.GaussianSigmaX.ToString();             // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
            savedData["FILTER"]["GaussianSigmaY"] = Global.GaussianSigmaY.ToString();             // Y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.        

            savedData["FILTER"]["IsBilateralFilter"] = Global.IsBilateralFilter.ToString();          // Bilateral Filter 사용 여부
            savedData["FILTER"]["BilateralD"]        = Global.BilateralD.ToString();                // 각 픽셀이웃의 직경(Diameter of each pixel neighbourhood)
            savedData["FILTER"]["BilateralSigmaX"]   = Global.BilateralSigmaX.ToString();            // x 방향의 표준편차. 0이 사용되는 경우, 자동으로 커널크기로부터 계산된다.
            savedData["FILTER"]["BilateralSigmaY"]   = Global.BilateralSigmaY.ToString();            // y 방향의 표준편차. 0이 사용되는 경우, sigmaX와 같은 값을 갖는다.

            savedData["FILTER"]["IsBlur"]      = Global.IsBlur.ToString();                     // Blur Filter 사용 여부
            savedData["FILTER"]["BlurKSizeX"]  = Global.BlurKSizeX.ToString();                 // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            savedData["FILTER"]["BlurKSizeY"]  = Global.BlurKSizeY.ToString();                 // 커널의 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)        
            savedData["FILTER"]["BlurAnchorX"] = Global.BlurAnchorX.ToString();                // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.
            savedData["FILTER"]["BlurAnchorY"] = Global.BlurAnchorY.ToString();                // Point(-1,-1) 값은 앵커(anchor)가 커널의 중앙에 위치 한다는 것을 의미한다. 사용자가 원하면 위치를 지정 할 수 있다.        

            savedData["FILTER"]["IsBoxFilter"] = Global.IsBoxFilter.ToString();                // Box Filter 사용 여부
            savedData["FILTER"]["BoxKSizeX"]   = Global.BoxKSizeX.ToString();                  // 커널의 X 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)
            savedData["FILTER"]["BoxKSizeY"]   = Global.BoxKSizeY.ToString();                  // 커널의 y 사이즈 (두 커널의 차원은 양수이고 홀수 이어야 한다.)                

            savedData["FILTER"]["IsSharpen"]        = Global.IsSharpen.ToString();                  // 선명 필터 1 사용 여부
            savedData["FILTER"]["IsMorphology"]     = Global.IsMorphology.ToString();               // Morphology 사용 여부
            savedData["FILTER"]["IsMeansDenoising"] = Global.IsMeansDenoising.ToString();           // MeansDenoising 사용 여부

            savedData["PRINT"]["MarginsLeft"]   = Global.MarginsLeft.ToString();                // Print 페이지 왼쪽 여백( 밀리미터 단위이다. * 10해서 넣는다.)
            savedData["PRINT"]["MarginsRight"]  = Global.MarginsRight.ToString();               // Print 페이지 오른쪽 여백( 밀리미터 단위이다. * 10해서 넣는다.)
            savedData["PRINT"]["MarginsTop"]    = Global.MarginsTop.ToString();                 // Print 페이지 윗쪽 여백( 밀리미터 단위이다. * 10해서 넣는다.)
            savedData["PRINT"]["MarginsBottom"] = Global.MarginsBottom.ToString();              // Print 페이지 아랫쪽 여백( 밀리미터 단위이다. * 10해서 넣는다.)

            savedData["PRINT"]["Header_Left"]           = Global.Header_Left;                           // Print시 Header 왼쪽에 나오는 문구
            savedData["PRINT"]["Header_Right"]          = Global.Header_Right;                          // Print시 Header 오른쪽에 나오는 문구
            savedData["PRINT"]["Footer_Left1"]          = Global.Footer_Left1;                          // Print시 Footer 왼쪽에 나오는 문구
            savedData["PRINT"]["Footer_Left2"]          = Global.Footer_Left2;                          // Print시 Footer 왼쪽에 나오는 문구
            savedData["PRINT"]["Logo_Show"]             = Global.Print_LOGO_Show.ToString();            // Print시 LOGO를 보여줄지 설정                                       
            savedData["PRINT"]["PaletteBar_Show"]       = Global.Print_PaletteBar_Show.ToString();      // Print시 PaletteBar를 보여줄지 설정
            savedData["PRINT"]["ROI_Show"]              = Global.Print_ROI_Show.ToString();             // Print시 ROI를 보여줄지 설정
            savedData["PRINT"]["Diff_ROI_Display"]      = Global.Print_Diff_ROI_Display.ToString();     // Print시 ROI의 차이를 보여줄지 설정
            savedData["PRINT"]["ImageBackColor_Black"]  = Global.Print_ImageBackColor_Black.ToString(); // Print시 Image의 Back Color를 Black으로 설정( true : Black, false : White )
            savedData["PRINT"]["Print_StudyAll"]        = Global.Print_StudyAll.ToString();             // Print시 선택된 Study의 정보를 모두 인쇄할지 설정( true: 모두 인쇄 fasle: 현제 보는 자료 )

            // 시리얼 설정(Camera Control)
            savedData["SERIAL"]["ArducamPort"]      = Global.ArducamPort;                       // Arducam Serial Port Name
            savedData["SERIAL"]["ArducamBaudRate"]  = Global.ArducamBaudRate.ToString();        // Arducam Serial BaudRate
            savedData["SERIAL"]["OpticPort"]        = Global.OpticPort;                         // Optic Serial Port Name
            savedData["SERIAL"]["OpticBaudRate"]    = Global.OpticBaudRate.ToString();          // Optic Serial BaudRate

            // PCR PROTOCOL 정보 
            savedData["PROTOCOL"]["PCR_VOLUME"] = Global.PCR_VOLUME.ToString();          // PCR_VOLUME
            savedData["PROTOCOL"]["HEATER_TEMP"] = Global.HEATER_TEMP.ToString();          // HEATER_TEMP

            // Result Graph
            savedData["GRAPH"]["graphYscale"] = Global.graphYscale.ToString();          // Result Graph YScale
            savedData["GRAPH"]["graphThreshold"] = Global.graphThreshold.ToString();    // Result Graph Threshold
            savedData["GRAPH"]["medianFilterWindow"] = Global.medianFilterWindow.ToString();    // Result Graph Median Filter Window Size
            savedData["GRAPH"]["IsInterpolation"] = Global.IsInterpolation.ToString();  // Result Graph Interpolation 여부 
            savedData["GRAPH"]["graphInterpolationScale"] = Global.graphInterpolationScale.ToString();    // Result Graph Interpolation Scale
            savedData["GRAPH"]["graphSampleRate"] = Global.graphSampleRate.ToString();  // Low Pass Filter Sampling Rate
            savedData["GRAPH"]["graphCutoff"] = Global.graphCutoff.ToString();          // Low Pass Filter Cutoff Value
            savedData["GRAPH"]["baselineStart"] = Global.baselineStart.ToString();      // 베이스라인 그래프 평균 시작점
            savedData["GRAPH"]["baselineEnd"] = Global.baselineEnd.ToString();          // 베이스라인 그래프 평균 종료점
            savedData["GRAPH"]["thFAMColorIndex"] = Global.thFAMColorIndex.ToString();  // FAM Threshold Color Index
            savedData["GRAPH"]["thHEXColorIndex"] = Global.thHEXColorIndex.ToString();  // HEX Threshold Color Index
            savedData["GRAPH"]["thROXColorIndex"] = Global.thROXColorIndex.ToString();  // ROX Threshold Color Index
            savedData["GRAPH"]["thCY5ColorIndex"] = Global.thCY5ColorIndex.ToString();  // CY5 Threshold Color Index

            //Save the file
            string iniPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.ini");
            parser.WriteFile(iniPath, savedData);
        }

        // InputBox 함수
        public static DialogResult InputBox(string title, string promptText, ref string value, bool IsPassword = false)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            if (IsPassword) textBox.PasswordChar = '*';

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 40, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;

            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;

            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOk.TextAlign = ContentAlignment.BottomCenter;

            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.TextAlign = ContentAlignment.BottomCenter;

            form.TopMost = true;
            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        // 이미지 선명하게 하는 필터함수 1
        public static void Sharpen_1(OpenCvSharp.Mat image, out OpenCvSharp.Mat result)
        {
            result = new OpenCvSharp.Mat();

            OpenCvSharp.Cv2.GaussianBlur(image, result, new OpenCvSharp.Size(0, 0), 3);
            OpenCvSharp.Cv2.AddWeighted(image, 1.5, result, -0.5, 0, result);
        }

        // 이미지 선명하게 하는 필터함수 2
        public static void Sharpen_2(OpenCvSharp.Mat image, out OpenCvSharp.Mat result)
        {
            OpenCvSharp.Mat blurred = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.GaussianBlur(image, blurred, new OpenCvSharp.Size(), 3);

            float alpha = 1.0f;

            result = (1 + alpha) * image - alpha * blurred;


            //result = new OpenCvSharp.Mat();

            //float[] data =  {  0.1f,0.4f,0.1f,
            //                   0.4f,  3f,0.4f,
            //                   0.1f,0.4f,0.1f};

            //OpenCvSharp.Mat kernal = new OpenCvSharp.Mat(3, 3, OpenCvSharp.MatType.CV_32F, data);
            //kernal /= 5.0;      // 정규화를 위해 3로 나눔.
            //OpenCvSharp.Cv2.Filter2D(image, result, image.Depth(), kernal);
        }

        // 이미지 선명하게 하는 필터함수 3
        public static void Sharpen_3(OpenCvSharp.Mat image, out OpenCvSharp.Mat result)
        {
            result = new OpenCvSharp.Mat();

            float[] data =  {  -1,-1,-1,-1,-1,
                               -1, 2, 2, 2,-1,
                               -1, 2, 8, 2,-1,
                               -1, 2, 2, 2,-1,
                               -1,-1,-1,-1,-1};

            OpenCvSharp.Mat kernal = new OpenCvSharp.Mat(5, 5, OpenCvSharp.MatType.CV_32F, data);
            kernal /= 8.0;      // 정규화를 위해 8로 나눔.
            OpenCvSharp.Cv2.Filter2D(image, result, image.Depth(), kernal);
        }

        // PACS Send 함수( Fo-Dicom 4.0.1 버젼 사용 )
        //-2: Server Connect Error, -1: sendDcm Null, 0: 성공, Cancel = 1, Pending = 2, Warning = 3, Failure = 4
        //public static int PACSSend(string sIP, int sPort, string sAET, string cAET, DicomFile sendDcm, int Timeout = 10000)
        //{
        //    int iRes = -1;
        //    if (sendDcm == null) return iRes;

        //    DicomCStoreRequest cstore = new DicomCStoreRequest(sendDcm)
        //    {
        //        OnResponseReceived = (DicomCStoreRequest request, DicomCStoreResponse response) =>
        //        {
        //            if (response.Status.State == DicomState.Pending)
        //            {
        //                Debug.WriteLine("Sending is in progress. please wait: ");
        //            }
        //            else if (response.Status.State == DicomState.Success)
        //            {
        //                Debug.WriteLine("Sending successfully finished");
        //            }
        //            else if (response.Status.State == DicomState.Failure)
        //            {
        //                Debug.WriteLine("Error sending datasets: " + response.Status.Description);
        //            }
        //            Debug.WriteLine(response.Status);

        //            iRes = Convert.ToInt32(response.Status.State);
        //        }
        //    };

        //    var client = new Dicom.Network.Client.DicomClient(sIP, sPort, false, cAET, sAET);

        //    try
        //    {
        //        client.AddRequestAsync(cstore).Wait(500);

        //        if (!client.SendAsync().Wait(Timeout))
        //        {
        //            Debug.WriteLine("서버 연결 Timeout");
        //            iRes = -3;
        //        }
        //    }
        //    catch
        //    {
        //        Debug.WriteLine("서버 연결 실패");
        //        iRes = -2;
        //    }

        //    return iRes;
        //}

        //// Viewer IP List를 읽어온다.
        //public static Dictionary<string, string> LoadViewerInfo()
        ////public static List<string> LoadViewerIPList()
        //{
        //    string FullViewerFileName;
        //    FullViewerFileName = String.Format("{0}\\ViewerList.ini", Application.StartupPath);

        //    Dictionary<string, string> ViewerInfo = new Dictionary<string, string>();
        //    if (!File.Exists(FullViewerFileName)) return ViewerInfo;       //파일이 없으면 진행하지 않는다.
        //    //List<string> IPList = new List<string>();
        //    //if (!File.Exists(FullViewerFileName)) return IPList;       //파일이 없으면 진행하지 않는다.

        //    FileIniDataParser parser = new FileIniDataParser();
        //    IniData viewerData = parser.ReadFile(FullViewerFileName);
        //    // Viewer 겟수 읽어오기 
        //    int viewerCount = 0;
        //    try { if (viewerData["VIEWER_INFO"]["VIEWER_COUNT"] != null) viewerCount = Convert.ToInt32(viewerData["VIEWER_INFO"]["VIEWER_COUNT"]); } catch { }
        //    //if (viewerCount <= 0) return IPList;                   // 겟수가 0이면 진행하지 않는다.
        //    if (!File.Exists(FullViewerFileName)) return ViewerInfo;        // 겟수가 0이면 진행하지 않는다

        //    for (int vIndex = 0; vIndex < viewerCount; vIndex++)
        //    {
        //        string viewerSection = String.Format("VIEWER_{0}", vIndex + 1);

        //        // Viewer Title 읽어온다.
        //        string Viewer_Send = "";                               // Viewer Send
        //        try { if (viewerData[viewerSection]["VIEWER_SEND"] != null) Viewer_Send = viewerData[viewerSection]["VIEWER_SEND"]; } catch { continue; }

        //        string Viewer_Title = "";                               // Viewer Title
        //        try { if (viewerData[viewerSection]["VIEWER_TITLE"] != null) Viewer_Title = viewerData[viewerSection]["VIEWER_TITLE"]; } catch { continue; }

        //        string Viewer_IPAdress = "";                // Viewer IP
        //        try { if (viewerData[viewerSection]["VIEWER_IPADDRESS"] != null) Viewer_IPAdress = viewerData[viewerSection]["VIEWER_IPADDRESS"]; } catch { continue; }

        //        if (Viewer_Title.Length > 0 && Viewer_IPAdress.Length > 0 && Viewer_Send == "Y")
        //        {
        //            ViewerInfo.Add(Viewer_Title, Viewer_IPAdress);
        //            //IPList.Add(Viewer_IPAdress);
        //        }
        //    }

        //    return ViewerInfo;
        //    //return IPList;
        //}


        // Label 해당하는 글씨크기를 가지고 온다.
        public static Font AutoFontSize(Font font, int width, int height, String text)
        {
            Font ft;

            Single Faktor, FaktorX, FaktorY;

            SizeF sz;
            using (var image = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(image))
                {
                    sz = g.MeasureString(text, font);
                }
            }

            FaktorX = (width) / sz.Width;
            FaktorY = (height) / sz.Height;

            if (FaktorX > FaktorY)
                Faktor = FaktorY;
            else
                Faktor = FaktorX;
            ft = font;

            if (Faktor > 0)
                ft = new Font(font.Name, font.SizeInPoints * (Faktor) - 1, System.Drawing.FontStyle.Bold);

            return ft;
        }

        // 현재 운영체제의 환경을 얻어온다. 
        public static int GetOSConfig()
        {
            int nProcess = 64;
            if (Environment.Is64BitOperatingSystem == true)
            {
                nProcess = 64; //“64 Bit”;
            }
            else
            {
                nProcess = 32; //“32 Bit”;
            }

            return nProcess;
        }

        // USB Driver Restart (반드시 관리자 권한 실행이 필요함.)
        public static void Restart_USB(string hwID)
        {
            try
            {
                //string textKey = "install";
                //Global.executeCMD(textKey);
                //return;

                string processName = "";
                int nOsBit = GetOSConfig();
                if (nOsBit == 32)
                    processName = "devcon_x32.exe";
                else
                    processName = "devcon_x64.exe";

                string camInitSwPath = Application.StartupPath + "\\" + processName;
                if (!File.Exists(camInitSwPath))       // Camera를 초기화하는 파일이 없으면 
                {
                    MessageBox.Show("Not found Camera initialize file ! (Devcon)");
                    return;
                }
                if (hwID.Length <= 0)
                {
                    MessageBox.Show("Not found Camera Vender ID !");
                    return;
                }

                //hwID = @"*VID_0C45*";
                string strCmd = "restart " + hwID;
                //MessageBox.Show(strCmd);

                ProcessStartInfo procInfo = new ProcessStartInfo();
                //procInfo.CreateNoWindow = false;
                procInfo.CreateNoWindow = true;
                procInfo.UseShellExecute = false;
                //procInfo.UseShellExecute = true;
                procInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //procInfo.FileName = "devcon_x64 restart *VEN_0303*";
                procInfo.FileName = processName;
                //procInfo.FileName = "hw_reset.bat";
                //procInfo.Arguments = "restart @*VID_0547*";
                procInfo.Arguments = strCmd;
                procInfo.WorkingDirectory = Application.StartupPath;
                procInfo.Verb = "runas";
                Process.Start(procInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }

    public static class Extensions
    {
        // bytes[]로 저장된 것을 double[]로 변경하는 함수
        public static double[] ToDoubleArray(this byte[] bytes)
        {
            List<double> listDouble = new List<double>();

            for (int i = 0; i < bytes.Length; i += sizeof(double))
                listDouble.Add(BitConverter.ToDouble(bytes, i));

            return listDouble.ToArray();
        }
    }


    public class UserFont
    {
        static PrivateFontCollection privateFonts = new PrivateFontCollection();

        public static Font NanumGothicFont(float size = 9f, FontStyle style = FontStyle.Regular)
        {
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Font");
            try
            {
                privateFonts.AddFontFile(Path.Combine(FilePath, "NanumGothic.ttf"));
                return new Font(privateFonts.Families[0], size, style);
            }
            catch
            {
                return new Font(SystemFonts.DefaultFont.FontFamily.Name, size, style);
            }


        }

        public static Font NanumGothicBoldFont(float size = 9f, FontStyle style = FontStyle.Regular)
        {
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "Font");
            try
            {
                privateFonts.AddFontFile(Path.Combine(FilePath, "NanumGothicBold.ttf"));
                return new Font(privateFonts.Families[0], size, style);
            }
            catch
            {
                return new Font(SystemFonts.DefaultFont.FontFamily.Name, size, style);
            }
        }

        public static Font NanumGothicExtraBoldFont(float size = 9f, FontStyle style = FontStyle.Regular)
        {
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "Font");
            try
            {
                privateFonts.AddFontFile(Path.Combine(FilePath, "NanumGothicExtraBold.ttf"));
                return new Font(privateFonts.Families[0], size, style);
            }
            catch
            {
                return new Font(SystemFonts.DefaultFont.FontFamily.Name, size, style);
            }
        }

        public static Font NanumGothicLightFont(float size = 9f, FontStyle style = FontStyle.Regular)
        {
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "Font");
            try
            {
                privateFonts.AddFontFile(Path.Combine(FilePath, "NanumGothicLight.ttf"));
                return new Font(privateFonts.Families[0], size, style);
            }
            catch
            {
                return new Font(SystemFonts.DefaultFont.FontFamily.Name, size, style);
            }
        }

    }
}
