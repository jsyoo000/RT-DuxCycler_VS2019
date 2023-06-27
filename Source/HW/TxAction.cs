using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duxcycler
{
	public class Command
	{
		public static int NOP = 0x00;
		public static int TASK_WRITE = 0x01;
		public static int TASK_END = 0x02;
		public static int GO = 0x03;
		public static int STOP = 0x04;
		public static int PARAM_WRITE = 0x05;
		public static int PARAM_END = 0x06;
		public static int RESUME = 0x07;
		public static int BOOTLOADER = 0x55;
	}

	// USB 8051 -> PC 입력버퍼
	public struct USB_inbuf
	{
		public Byte tempCoverH;
		public Byte tempCoverL;
		public Byte tempChamberH;
		public Byte tempChamberL;
		public Byte tempHeatsinkH;
		public Byte tempHeatsinkL;
	};

	// USB PC->8051 출력버퍼
	public struct USB_outbuf
	{
		public Byte PWMminusH;
		public Byte PWMminusL;
		public Byte PWMplusH;
		public Byte PWMplusL;
		public Byte HeaterOnOff;
		public Byte FanOnOff;
		public Byte Preheating;
		public Byte PCR_Start;
		public Byte direction;
		public Byte bootcode;
		public Byte LED_Y_code;
		public Byte LED_R_code;
		//PID parameter
		public Byte PIDparamStemp;
		public Byte PIDparam_P;
		public Byte PIDparam_I;
		public Byte PIDparam_D;
	};

	public class TxAction
	{
		// USB data buffer
		private USB_inbuf in_buffer;
		private USB_outbuf out_buffer;

		private byte[] Tx_Buffer;

		public static int TX_BUFSIZE = 65;

		private static int TX_HEAD = 0;
		private static int TX_CMD = 1;
		private static int TX_ACTNO = 2;
		private static int TX_TEMP = 3;
		private static int TX_TIMEH = 4;
		private static int TX_TIMEL = 5;
		private static int TX_LIDTEMP = 6;
		private static int TX_REQLINE = 7;
		private static int TX_CURRENT_ACT_NO = 8;
		private static int TX_BOOTLOADER = 10;

		public static int AF_GOTO = 250;

		/// <summary>
		/// TxAction 생성자 
		/// </summary>
		public TxAction()
		{
			Tx_Buffer = new byte[TX_BUFSIZE];
		}

		/// <summary>
		/// PCR 전송 버퍼를 초기화한다. 
		/// </summary>
		private void Tx_Clear()
		{
			//Tx_Buffer = new byte[TX_BUFSIZE];
			Array.Clear(Tx_Buffer, 0, Tx_Buffer.Length);
		}

		/// <summary>
		/// PCR에 NOP(Ox00) 명령어를 전송한다. 
		/// </summary>
		/// <returns>NOP Command</returns>
		public byte[] Tx_NOP()
		{
			Tx_Clear();
			Tx_Buffer[TX_CMD] = (byte)Command.NOP;
			return Tx_Buffer;
		}

		/// <summary>
		/// PCR Action 명령어를 전송한다. 
		/// </summary>
		/// <param name="label">레이블</param>
		/// <param name="temp">온도</param>
		/// <param name="time">시간</param>
		/// <param name="preheat">Lid Heater 온도</param>
		/// <param name="currentActNo">Action 번호</param>
		/// <returns>Action Command</returns>
		public byte[] Tx_TaskWrite(String label, String temp, String time, String preheat, int currentActNo)
		{
			Tx_Clear();
			int nlabel, ntemp, ntime, npreheat;
			npreheat = (Byte)Global.HEATER_TEMP;

			if (label == "GOTO")
			{
				nlabel = AF_GOTO;
				//npreheat = 0;
			}
			else
			{
				nlabel = Convert.ToInt32(label);
				//npreheat = Convert.ToInt32(preheat);
			}
			npreheat = Convert.ToInt32(preheat);
			ntemp = Convert.ToInt32(temp);
			ntime = Convert.ToInt32(time);
			Tx_Buffer[TX_HEAD] = 0;
			Tx_Buffer[TX_CMD] = (byte)Command.TASK_WRITE;
			Tx_Buffer[TX_ACTNO] = (byte)nlabel;
			Tx_Buffer[TX_TEMP] = (byte)ntemp;
			Tx_Buffer[TX_TIMEH] = (byte)(ntime / 256.0);
			Tx_Buffer[TX_TIMEL] = (byte)ntime;
			Tx_Buffer[TX_LIDTEMP] = (byte)npreheat;
			Tx_Buffer[TX_CURRENT_ACT_NO] = (byte)currentActNo;
			Tx_Buffer[TX_REQLINE] = (byte)currentActNo;

			return Tx_Buffer;
		}

		/// <summary>
		/// pwm 동작 정지
		/// </summary>
		/// <returns>PWM Off Command</returns>
		public byte[] Tx_PwmOff()
		{
			Tx_Clear();

			// [2010.8.30 BySoda] USBHID로 변경에 따른 버퍼사이즈 65로 변경에 따른 변경 및 read, write 함수 변경
			out_buffer.PWMminusH = 0xFF;
			out_buffer.PWMminusL = 0xFF;
			out_buffer.PWMplusH = 0xFF;
			out_buffer.PWMplusL = 0xFF;

			out_buffer.LED_Y_code = 1;

			Tx_Buffer[0] = 0; // 초기값
			Tx_Buffer[1] = out_buffer.PWMminusH;
			Tx_Buffer[2] = out_buffer.PWMminusL;
			Tx_Buffer[3] = out_buffer.PWMplusH;
			Tx_Buffer[4] = out_buffer.PWMplusL;
			Tx_Buffer[5] = out_buffer.HeaterOnOff;
			Tx_Buffer[6] = out_buffer.FanOnOff;
			Tx_Buffer[7] = out_buffer.Preheating;
			Tx_Buffer[8] = out_buffer.PCR_Start;
			Tx_Buffer[9] = out_buffer.direction;
			Tx_Buffer[10] = 0x00;   //bootcode
			Tx_Buffer[11] = out_buffer.LED_Y_code;
			Tx_Buffer[12] = out_buffer.LED_R_code;

			return Tx_Buffer;
		}

		/// <summary>
		/// 캡쳐가 끝나면 PCR을 계속하기 위해 Resume 명령어를 전송한다. 
		/// </summary>
		/// <returns>Resume Command</returns>
		public byte[] Tx_TaskResume()
		{
			Tx_Clear();
			Tx_Buffer[TX_CMD] = (byte)Command.RESUME;
			return Tx_Buffer;
		}

		/// <summary>
		/// Task 의 끝을 알리는 메시지를 전송한다. 
		/// </summary>
		/// <returns>TASK_END Command</returns>
		public byte[] Tx_TaskEnd()
		{
			Tx_Clear();
			Tx_Buffer[TX_CMD] = (byte)Command.TASK_END;
			return Tx_Buffer;
		}

		/// <summary>
		/// PCR 시작 메시지를 전송한다. 
		/// </summary>
		/// <returns>PCR Start Command</returns>
		public byte[] Tx_Go()
		{
			Tx_Clear();
			Tx_Buffer[TX_CMD] = (byte)Command.GO;
			return Tx_Buffer;
		}

		/// <summary>
		/// PCR 정지 메시지를 전송한다. 
		/// </summary>
		/// <returns>PCR Stop Command</returns>
		public byte[] Tx_Stop()
		{
			Tx_Clear();
			Tx_Buffer[TX_CMD] = (byte)Command.STOP;
			return Tx_Buffer;
		}

		/// <summary>
		/// Firmware 업데이트가 필요한 경우 BootLoader 모드로 변경하는 메시지를 전송한다. 
		/// </summary>
		/// <returns>Change BootLoader Command</returns>
		public byte[] Tx_BootLoader()
		{
			Tx_Clear();
			Tx_Buffer[TX_BOOTLOADER] = (byte)Command.BOOTLOADER;
			return Tx_Buffer;
		}
	}
}
