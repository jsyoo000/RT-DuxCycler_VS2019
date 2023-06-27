using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duxcycler
{
	public class RxAction
	{
		private int State;
        private int Response;
        private int Cover_TempH;
		private int Cover_TempL;
		private int Chamber_TempH;
		private int Chamber_TempL;
		private int Heatsink_TempH;
		private int Heatsink_TempL;
		private int Current_Operation;
		private int Current_Action;
		private int Current_Loop;
		private int Total_Action;
		private int Error;
		private int Serial_H;
		private int Serial_L;
		private int Total_TimeLeft;
		private int Sec_TimeLeft;
		private int Firmware_Version;

		// adding for task write
		private int Label;
		private int Temp;
		private int Time_H;
		private int Time_L;
		private int ReqLine;

		public static int RX_BUFSIZE = 64;

		public static int RX_STATE = 0;
		public static int RX_RES = 1;
		public static int RX_CURRENTACTNO = 2;
		public static int RX_CURRENTLOOP = 3;
		public static int RX_TOTALACTNO = 4;
		public static int RX_KP = 5;
		public static int RX_KI = 6;
		public static int RX_KD = 7;
		public static int RX_LEFTTIMEH = 8;
		public static int RX_LEFTTIMEL = 9;
		public static int RX_LEFTSECTIMEH = 10;
		public static int RX_LEFTSECTIMEL = 11;
		public static int RX_LIDTEMPH = 12;
		public static int RX_LIDTEMPL = 13;
		public static int RX_CHMTEMPH = 14;
		public static int RX_CHMTEMPL = 15;
		public static int RX_PWMH = 16;
		public static int RX_PWML = 17;
		public static int RX_PWMDIR = 18;
		public static int RX_LABEL = 19;
		public static int RX_TEMP = 20;
		public static int RX_TIMEH = 21;
		public static int RX_TIMEL = 22;
		public static int RX_LIDTEMP = 23;
		public static int RX_REQLINE = 24;
		public static int RX_ERROR = 25;
		public static int RX_CUR_OPR = 26;
		public static int RX_SINKTEMPH = 27;
		public static int RX_SINKTEMPL = 28;
		public static int RX_KP_1 = 29;
		public static int RX_KI_1 = 33;
		public static int RX_KD_1 = 37;
		public static int RX_SERIALH = 41;    // not using this version.
		public static int RX_SERIALL = 42;    // only bluetooth version
		public static int RX_SERIALRESERV = 43;
		public static int RX_VERSION = 44;

		public static int AF_GOTO = 250;

        public event Action RxActionChangeEvent;
        /// <summary>
        /// RxAction 생성자 
        /// </summary>
        public RxAction()
		{
			State = 0;
			Response = 0;
			Cover_TempH = 0; Cover_TempL = 0;
			Chamber_TempH = 0; Chamber_TempL = 0;
			Heatsink_TempH = 0; Heatsink_TempL = 0;
			Current_Operation = 0; Current_Action = 0; Current_Loop = -1;
			Total_Action = 0; Error = 0; Total_TimeLeft = 0;
			Sec_TimeLeft = 0; Serial_H = 0; Serial_L = 0;
			Label = 0; Temp = 0; Time_H = 0; Time_L = 0;
			ReqLine = 0;
		}

		/// <summary>
		/// PCR 수신 데이터를 저장한다. 
		/// </summary>
		/// <param name="buffer">PCR 수신 데이터</param>
		public void set_Info(byte[] buffer)
		{
			State = (int)(buffer[RX_STATE] & 0xff);
            Response = (int)(buffer[RX_RES] & 0xff);
            Current_Action = (int)(buffer[RX_CURRENTACTNO] & 0xff);
			Current_Loop = (int)(buffer[RX_CURRENTLOOP] & 0xff);
			Total_Action = (int)(buffer[RX_TOTALACTNO] & 0xff);
			Total_TimeLeft = (int)((buffer[RX_LEFTTIMEH] & 0xff) * 256 + (buffer[RX_LEFTTIMEL] & 0xff));
			Sec_TimeLeft = (int)(buffer[RX_LEFTSECTIMEH] & 0xff) * 256 + (int)(buffer[RX_LEFTSECTIMEL] & 0xff);
			Cover_TempH = (int)(buffer[RX_LIDTEMPH] & 0xff);
			Cover_TempL = (int)(buffer[RX_LIDTEMPL] & 0xff);
			Chamber_TempH = (int)(buffer[RX_CHMTEMPH] & 0xff);
			Chamber_TempL = (int)(buffer[RX_CHMTEMPL] & 0xff);
			Heatsink_TempH = (int)(buffer[RX_SINKTEMPH] & 0xff);
			Heatsink_TempL = (int)(buffer[RX_SINKTEMPL] & 0xff);
			Current_Operation = (int)(buffer[RX_CUR_OPR] & 0xff);
			Error = (int)(buffer[RX_ERROR] & 0xff);
			Serial_H = (int)(buffer[RX_SERIALH] & 0xff);
			Serial_L = (int)(buffer[RX_SERIALL] & 0xff);
			Firmware_Version = (int)(buffer[RX_VERSION] & 0xff);
			Label = (int)(buffer[RX_LABEL] & 0xff);
			Temp = (int)(buffer[RX_TEMP] & 0xff);
			Time_H = (int)(buffer[RX_TIMEH] & 0xff);
			Time_L = (int)(buffer[RX_TIMEL] & 0xff);
			ReqLine = (int)(buffer[RX_REQLINE] & 0xff);

            RxActionChangeEvent?.Invoke(); // 이벤트 발생
        }

		/// <summary>
		/// 현재 PCR 상태를 리턴한다. 
		/// </summary>
		/// <returns>PCR 상태</returns>
		public int getState()
		{
			return State;
		}

        /// <summary>
        /// 현재 Response
        /// </summary>
        /// <returns>응답 상태</returns>
        public int getResponse()
        {
            return Response;
        }

        /// <summary>
        /// 현재 Cover 온도를 리턴한다. 
        /// </summary>
        /// <returns>Cover 온도</returns>
        public double getCover_Temp()
		{
			double Temp = (double)(Cover_TempH) + (double)(Cover_TempL) * 0.1;

			return Temp;
		}

		/// <summary>
		/// Chamber 온도를 리턴한다.  
		/// </summary>
		/// <returns>Chamber 온도</returns>
		public double getChamber_Temp()
		{
			double Temp = (double)(Chamber_TempH) + (double)(Chamber_TempL) * 0.1;

			return Temp;
		}

		/// <summary>
		/// Heatsink 온도를 리턴한다.  
		/// </summary>
		/// <returns></returns>
		public double getHeatsink_Temp()
		{
			double Temp = (double)(Heatsink_TempH) + (double)(Heatsink_TempL) * 0.1;

			return Temp;
		}

		/// <summary>
		/// Operation 값을 리턴한다. 
		/// </summary>
		/// <returns>Operation 값</returns>
		public int getCurrent_Operation()
		{
			return Current_Operation;
		}

		// 현재 Action 번호를 리턴한다. 
		public int getCurrent_Action()
		{
			return Current_Action;
		}

		// 현재 Action Stage 루프의 번호를 리턴한다. 
		public int getCurrent_Loop()
		{
			return Current_Loop;
		}

		// 전체 Action 개수를 리턴한다. 
		public int getTotal_Action()
		{
			return Total_Action;
		}

		// 에러 여부를 리턴한다. 
		public int getError()
		{
			return Error;
		}

		// 에러일 경우 에러 메시지를 리턴한다. 
		public string ErrorMsg
		{
			get
			{ 
				string Errorstr = "";
				switch (getError())
				{
					case 0:
                        Errorstr = " ";
                        break;
                    case 1:
						Errorstr = "LID overheating error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					case 2:
						// [2010.12.2 By Soda] 교수님께서 일단 챔버 에러는 실행하지 말라고 하심..
						Errorstr = "Chamber overheating error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					case 3:
						Errorstr = "LID Heater and Chamber overheating error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					case 4:
						Errorstr = "Heat Sink overheating error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					case 5:
						Errorstr = "LID Heater and Heat Sink overheating error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					case 6:
						Errorstr = "Chamber and Heat Sink overheating error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					case 7:
						Errorstr = "LID Heater and Chamber, Heat Sink overheating error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					case 255:               // 온도센서 에러시 작동
						Errorstr = "Temperature Sensor error! Please power-off and check MyPCR machine! Program will be shutdown!";
						break;
					default:
                        Errorstr = "Unknown Error";
                        break;
                }
                return Errorstr;
            }	
		}

		public int getTotal_TimeLeft()
		{
			return Total_TimeLeft;
		}

		public int getSec_TimeLeft()
		{
			return Sec_TimeLeft;
		}

		public int getSerial_H()
		{
			return Serial_H;
		}

		public int getSerial_L()
		{
			return Serial_L;
		}

		public int getFirmware_Version()
		{
			return Firmware_Version;
		}

		public int getLabel()
		{
			return Label;
		}

		public int getTemp()
		{
			return Temp;
		}

		public int getTime_H()
		{
			return Time_H;
		}

		public int getTime_L()
		{
			return Time_L;
		}

		public int getTime()
		{
			int time = ((int)(Time_H * 256.0) + (int)(Time_L));
			return time;
		}

		public int getReqLine()
		{
			return ReqLine;
		}
	}
}
