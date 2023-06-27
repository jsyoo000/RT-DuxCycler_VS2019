using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duxcycler
{
	public class Action_PCR
	{
		public enum DATA_TYPE { LABEL = 0, TEMP = 1, REMAININGTIME = 2, TIME = 3 };

		public static int AF_GOTO = 250;

		private String ProtocolName;
		private String Label;
		private String Temp;
		private String Time;
		private String RemainingTime;
		private bool isCapture;
		private bool isComplete;

		/// <summary>
		/// Action을 초기화한다. 
		/// </summary>
		public Action_PCR()
		{
			Label = null; Temp = null; Time = null; RemainingTime = ""; isCapture = false; isComplete = false;
		}

		/// <summary>
		/// Action을 초기화한다. 
		/// </summary>
		/// <param name="ProtocolName">프로토콜 이름</param>
		public Action_PCR(String ProtocolName)
		{
			this.ProtocolName = ProtocolName; Label = null; Temp = null; Time = null; RemainingTime = "";
		}

		/// <summary>
		/// Action을 초기화한다. 
		/// </summary>
		/// <param name="Label">레이블</param>
		/// <param name="Temp">온도</param>
		/// <param name="Time">시간</param>
		/// <param name="isCapture">캡쳐 여부</param>
		public Action_PCR(String Label, String Temp, String Time, bool isCapture)
		{
			this.Label = Label;
			this.Temp = Temp;
			this.Time = Time;
			RemainingTime = "";
			this.isCapture = isCapture;
			this.isComplete = false;
		}

		/// <summary>
		/// 현재 Action의 Label을 리턴한다. 
		/// </summary>
		/// <returns>Label</returns>
		public String getLabel()
		{
			return Label;
		}

		/// <summary>
		/// 현재 Action의 Label을 저장한다. 
		/// </summary>
		/// <param name="label">Label</param>
		public void setLabel(String label)
		{
			Label = label;
		}

		/// <summary>
		/// 현재 Action의 설정 온도를 리턴한다. 
		/// </summary>
		/// <returns>설정 온도</returns>
		public String getTemp()
        {
            return Temp;
        }

		/**
		* @details 현재 Action의 온도를 저장한다. 
		* @param[in] temp 저장할 온도 
		*/
		public void setTemp(String temp)
        {
            Temp = temp;
        }

		/// <summary>
		/// 현재 Action의 설정 시간을 리턴한다. 
		/// </summary>
		/// <returns>설정 시간</returns>
		public String getTime()
        {
            return Time;
        }

		/// <summary>
		/// 현재 Action의 시간을 저장한다. 
		/// </summary>
		/// <param name="time">Action 시간</param>
		public void setTime(String time)
        {
            Time = time;
        }

		/// <summary>
		/// Capture 여부를 리턴한다.  
		/// </summary>
		/// <returns>Capture 여부</returns>
		public bool getCapture()
		{
			return isCapture;
		}

		/// <summary>
		/// Capture 완료 여부를 리턴한다. 
		/// </summary>
		/// <returns>Capture 완료 여부</returns>
		public bool getComplete()
		{
			return isComplete;
		}

		/// <summary>
		/// Capture 완료 여부를 저장한다. 
		/// </summary>
		/// <param name="isComplete">Capture 완료 여부</param>
		public void setComplete(bool isComplete)
		{
			this.isComplete = isComplete;
		}
	}
}
