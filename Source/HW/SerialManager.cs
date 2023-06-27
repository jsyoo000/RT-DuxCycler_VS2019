using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;
using Duxcycler_GLOBAL;

namespace Duxcycler
{
    public enum ARDUINO_COMMAND { 
        STEP_ACCEL = 0,                 // A : 스텝모터 가속도값 (필터 휠) (0~3000, Default : 1000)
        STEP_EMERGENCY = 1,             // E : 스텝모터 긴급정지 
        STEP_HOMING = 2,                // G : 스텝모터 홈 위치로 이동
        STEP_MOVING = 3,                // N : 스텝모터 해당 위치로 이동 (-1500~500)
        STEP_COARSE_SPEED = 4,          // H : 스텝모터 홈 이동 속도 (FAST) (0~3000, Default : 3000)
        STEP_FINE_SPEED = 5,            // S : 스텝모터 홈 이동 속도 (SLOW) (0~3000, Default : 50) 
        STEP_MAX_SPEED = 6,             // M : 스텝모터 해당 위치 이동 속도 (0~3000, Default : 1000)                                   
        LED_ONOFF = 7,                  // P : LED ON(100~150) / OFF(0) (0~255)
        TRAY_INOUT = 8,                 // X : TRAY 열림(1000) / 닫힘(2000) (1000~2000)
        HEATER_UPDOWN = 9,              // Y : 히터 상(2000)하(1000) 이동 (1000~2000) 
        GET_LED_STATE = 10,             // p : LED 상태정보 
        GET_STEP_ACCEL = 11,            // a : 스텝모터 현재 가속도값 (0~3000, Default : 1000) 
        GET_STEP_POS = 12,              // n : 스텝모터 현재 위치 (-1500~500)
        GET_STEP_COARSE_SPEED = 13,  // h : 스텝모터 홈 이동 속도 (FAST) (0~3000, Default : 3000)
        GET_STEP_FINE_SPEED = 14,  // s : 스텝모터 홈 이동 속도 (SLOW) (0~3000, Default : 50)
        GET_STEP_MAX_SPEED = 15,     // m : 스텝모터 해당 위치 이동 속도 (0~3000, Default : 1000)
        GET_HALL_SENSOR = 16            // o : 홀 센서 상태 정보 (ON:1, OFF:0)
    };
 
    public enum COMMAND_VALUE
    {
        STEP_ACCEL = 1000,                 // A : 스텝모터 가속도값 (필터 휠) (0~3000, Default : 1000)
        STEP_COARSE_SPEED = 300,           // H : 스텝모터 홈 이동 속도 (FAST) (0~3000, Default : 300)
        STEP_FINE_SPEED = 50,              // S : 스텝모터 홈 이동 속도 (SLOW) (0~3000, Default : 50) 
        STEP_MAX_SPEED = 1000,             // M : 스텝모터 해당 위치 이동 속도 (0~3000, Default : 1000)                                   
        LED_ON = 200,                      // P : LED ON(200) (0~255)
        LED_OFF = 0,                       // P : LED OFF(0) (0~255)
        TRAY_IN = 2000,                    // X : TRAY 열림(1000) / 닫힘(2000) (1000~2000)
        TRAY_OUT = 1000,                   // X : TRAY 열림(1000) / 닫힘(2000) (1000~2000)
        HEATER_UP = 2000,                  // Y : 히터 상(2000)하(1000) 이동 (1000~2000) 
        HEATER_DOWN = 1000,                // Y : 히터 상(2000)하(1000) 이동 (1000~2000) 
        //FILTER_POS_FAM = 350,              // FAM : 350
        //FILTER_POS_HEX = 700,              // HEX : 700
        //FILTER_POS_ROX = 1050,             // ROX : 1050
        //FILTER_POS_CY5 = 1400              // CY5 : 1400
        FILTER_POS_FAM = 25,               // FAM : 25
        FILTER_POS_HEX = 300,              // HEX : 300
        FILTER_POS_ROX = 600,              // ROX : 600
        FILTER_POS_CY5 = 900               // CY5 : 900
    };

    public enum CAMERA_STAND_COMMEND { STAND_UP, STAND_DOWN, STAND_STOP };                              // CAMERA STAND Commend
    public enum PANTILT_COMMEND { PAN_LEFT, PAN_RIGHT, TILT_UP, TILT_DOWN, PANTILT_RESET, NONE };       // PAN TILT Type
    public enum CAMERA_COMMEND { FOCUS_NEAR, FOCUS_FAR, FOCUS_STOP, FOCUS_SPEED10, FOCUS_SPEED15, FOCUS_SPEED20 };
    public enum HRV_COMMEND { SENSOR_PPG, SENSOR_ECG, TURN_ON, TURN_OFF, TURN_ON_DPA, TURN_OFF_DPA, START_ON, START_ON_DPA };                              // CAMERA STAND Commend
    enum serial_Packet
    {
        header		  = 0,	//header
        pulsewavehigh = 1,	//pulsewave high
        pulsewavelow  = 2,	//pulsewave low
        pulse2high    = 3,	//version info.1
        pulse2low     = 4,	//version info.2
        intervalhigh  = 5,	//interval high
        intervallow   = 6,	//interval low
        leadfault     = 7,	//read fault
        reserve       = 8	//sensor on, off
    };

    enum serial_Packet_24
    {
        header = 0,	//header
        pulsewavehigh = 1,	    //pulsewave high
        pulsewavemiddle = 2,	//pulsewave middle
        pulsewavelow = 3,	    //pulsewave low
        intervalhigh = 4,	    //interval high
        intervallow = 5,	    //interval low
    };

    /// <summary>
    /// Class emulates long process which runs in worker thread
    /// and makes synchronous user UI operations.
    /// </summary>
    public class SerialManager
    {
        #region Members
        SerialPort serialPort = new SerialPort();

        // H(Coarse Speed), M(Max Speed), S(Fine Speed)
        public string[] listCommand = new string[17]
        {
            "A ", "E", "G", "N ", "H ", "S ", "M ", "P ", "X ", "Y ", "p ", "a ", "n ", "h ", "s ", "m ", "o "
        };

        public bool focusLimited = false;
        public bool IsFocusLimit { get { return focusLimited; } }
        public bool IsOpen { get { return this.serialPort.IsOpen; } }

        public bool FocusLimit      { set { if (!IsOpen) this.focusLimited = value; } }
        public string PortName      { set { if (!IsOpen) this.serialPort.PortName  = value; } }
        public int BaudRate         { set { if (!IsOpen) this.serialPort.BaudRate  = value; } }
        public int DataBits         { set { if (!IsOpen) this.serialPort.DataBits  = value; } }
        public Parity Parity        { set { if (!IsOpen) this.serialPort.Parity    = value; } }
        public StopBits StopBits    { set { if (!IsOpen) this.serialPort.StopBits  = value; } }
        public Handshake HandShake  { set { if (!IsOpen) this.serialPort.Handshake = value; } }

        // 카메라 온도 읽기
        // RequestEnvironmentTemp 명령 후 다음 주기에 읽는다.
        public double EnvironmentTemp { get { return envTemp; } }   
        private double envTemp = 24.5;

        // Read할때 사용하는 Buff
        public static List<byte> ReadBuff = new List<byte>();
        #endregion

        #region Functions

        /// <summary>
        /// SerialManager 생성자
        /// </summary>
        public SerialManager()
        {
            this.serialPort.PortName  = "COM3";
            this.serialPort.BaudRate  = 921600;
            this.serialPort.DataBits  = 8;
            this.serialPort.Parity    = Parity.None;
            this.serialPort.StopBits  = StopBits.One;
            this.serialPort.Handshake = Handshake.None;
        }

        /// <summary>
        /// 시리얼 포트를 Open 하는 함수 
        /// </summary>
        /// <returns></returns>
        public bool OpenPort()
        {
            try
            {
                if (!this.serialPort.IsOpen)
                {                
                    this.serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHander);

                    this.serialPort.Open();
                }

                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// 시리얼 포트를 Close 하는 함수 
        /// </summary>
        public void ClosePort()
        {
            try
            {
                if (this.serialPort.IsOpen)
                {
                    this.serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHander);

                    this.serialPort.Close();
                }
            }
            catch {            }
        }

        /// <summary>
        /// 시리얼 포트에 Data를 전송하는 함수 
        /// </summary>
        /// <param name="bySendData">전송 데이터</param>
        public void SendData(byte[] bySendData)
        {
            if (bySendData == null || bySendData.Length == 0) return;

            if (this.serialPort.IsOpen)
            {
                this.serialPort.Write(bySendData, 0, bySendData.Length);
            }
        }

        /// <summary>
        /// Data 수신시 처리 함수 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceivedHander(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort sirial = (SerialPort)sender;

            int iReadSize = serialPort.BytesToRead;
            byte[] byRead = new byte[iReadSize];
            serialPort.Read(byRead, 0, iReadSize);

            if (iReadSize <= 0)
                return;

            // 현재 틱을 가져오고
            Global.currentTick = Environment.TickCount;

            // 현재의 틱 값이 이전 틱 값 + 1000보다 클 경우
            // 즉, 마지막 갱신 시간으로부터 1초가 지난 경우
            if (Global.currentTick >= Global.previousTick + 1000)
            {
                // 이전 시간을 현재 시간으로 설정하고 
                // 계산량을 구한다.
                Global.previousTick = Global.currentTick;
                //Console.WriteLine("Calculation per second (Calc/s): {0:N0}, {1}", nFrameRate, pictureBox_Raw.ImageInfo.listShape.Count);
                Global.frameRate = 0;
            }
            Global.frameRate += (iReadSize / 8);           // Frame Rate 확인 변수

            lock (Global.lockObj)
            {
                List<byte> byteList = new List<byte>(byRead);
                ReadBuff.AddRange(byteList);

                if (iReadSize <= 1)
                    return;

                BufferProcess();
            }
        }

        public void BufferProcess()
        {
            string strLog = "";
            byte[] arrayBuf = ReadBuff.ToArray();
            int i = 0;
            for (i = 0; i < arrayBuf.Length; i++)
            {
                //if (arrayBuf[i] == 0xff)
                //    break;

                strLog += string.Format("{0:X2} ", arrayBuf[i]);
                //Global.byteBufQueue_revQ.Enqueue(arrayBuf[i]);
            }

            strLog += "\n";
            Debug.Write(strLog);

            string revMsg = this.ByteToString(arrayBuf);
            Global.stringBufQueue_revQ.Enqueue(revMsg);

            ReadBuff.Clear();
        }

        /// <summary>
        /// 바이트 배열을 String으로 변환  
        /// </summary>
        /// <param name="strByte">변환할 데이터</param>
        /// <returns></returns>
        private string ByteToString(byte[] strByte) 
        {
            string str = ASCIIEncoding.ASCII.GetString(strByte); 
            //string str = Encoding.Default.GetString(strByte);
            return str; 
        }

        /// <summary>
        /// String을 바이트 배열로 변환  
        /// </summary>
        /// <param name="str">변환할 문자열</param>
        /// <returns></returns>
        private byte[] StringToByte(string str) 
        { 
            byte[] StrByte = ASCIIEncoding.ASCII.GetBytes(str);
            //byte[] StrByte = Encoding.UTF8.GetBytes(str); 
            return StrByte; 
        }

        /// <summary>
        /// Arduino Command 처리 함수 
        /// </summary>
        /// <param name="command">명령어 종류</param>
        /// <param name="value">설정 값</param>
        /// <param name="isValue">설정 값이 있는지 여부</param>
        /// <returns>로그 데이터</returns>
        public string ArduinoCommand(int command, int value, bool isValue = false)
        {
            string strLog = "";
            if (this.IsOpen)
            {
                int arduinoValue = value;
                switch (command)
                {
                    case (int)ARDUINO_COMMAND.STEP_ACCEL:
                        arduinoValue = Global.accelSpeed;
                        break;
                    case (int)ARDUINO_COMMAND.STEP_COARSE_SPEED:
                        arduinoValue = Global.coarseSpeed;
                        break;
                    case (int)ARDUINO_COMMAND.STEP_FINE_SPEED:
                        arduinoValue = Global.fineSpeed;
                        break;
                    case (int)ARDUINO_COMMAND.STEP_MAX_SPEED:
                        arduinoValue = Global.maxSpeed;
                        break;
                    case (int)ARDUINO_COMMAND.LED_ONOFF:
                        if (value == (int)COMMAND_VALUE.LED_ON)
                            arduinoValue = Global.ledOn;
                        else
                            arduinoValue = Global.ledOff;
                        break;
                    case (int)ARDUINO_COMMAND.TRAY_INOUT:
                        if (value == (int)COMMAND_VALUE.TRAY_IN)
                            arduinoValue = Global.trayIn;
                        else
                            arduinoValue = Global.trayOut;
                        break;
                    case (int)ARDUINO_COMMAND.HEATER_UPDOWN:
                        if (value == (int)COMMAND_VALUE.HEATER_UP)
                            arduinoValue = Global.heaterUp;
                        else
                            arduinoValue = Global.heaterDown;
                        break;
                    case (int)ARDUINO_COMMAND.STEP_MOVING:
                        if (value == (int)COMMAND_VALUE.FILTER_POS_HEX)
                            arduinoValue = Global.filterHEX_Pos;
                        else if (value == (int)COMMAND_VALUE.FILTER_POS_ROX)
                            arduinoValue = Global.filterROX_Pos;
                        else if (value == (int)COMMAND_VALUE.FILTER_POS_CY5)
                            arduinoValue = Global.filterCY5_Pos;
                        else
                            arduinoValue = Global.filterFAM_Pos;

                        //MessageBox.Show(arduinoValue.ToString());
                        break;
                    default:
                        arduinoValue = value;
                        break;
                }

                // Setting 값이 넘어온 경우 ... 
                if (isValue)
                    arduinoValue = value;
                string finalCommand = listCommand[command] + arduinoValue.ToString() + "\r\n";

                // Go Home 이나 Emergency 의 경우 value 값이 없음.  
                if (command == 1 || command == 2)
                    finalCommand = listCommand[command] + "\r\n";

                //MessageBox.Show(finalCommand);
                byte[] CommandByte = this.StringToByte(finalCommand);
                SendData(CommandByte);

                for (int i = 0; i < CommandByte.Length; i++)
                {
                    strLog += string.Format("{0:X2} ", CommandByte[i]);
                }
            }
            return strLog;
        }
        #endregion
    }
}
