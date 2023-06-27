using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duxcycler.Source.HW;
using System.Windows;

namespace Duxcycler
{
    /// <summary>
    ///         state word of the main state machine
    /// </summary>
	public enum RES
	{
        RES_NONE            = 0,
        RES_BUSY            = 0x01,
        RES_TASK_WR_DONE    = 0x02,
        RES_TASK_ER_DONE    = 0x03,
        RES_GO_DONE         = 0x04,
        RES_STOP_DONE       = 0x05,
        RES_PCR_END         = 0x06,
        RES_RESUME          = 0x07,
	};

    public enum STATE             //	Device State.
    {                               //  Used in PCR_Task.c
        STATE_READY = 0x01,
        STATE_RUN,
        STATE_PCREND,
        STATE_STOP,
        STATE_TASK_WRITE,
        STATE_ERROR,
        STATE_PARAM_WRITE
    }

    /// <summary>
    /// state word of the  machine Operator
    /// </summary>
    public enum State_Oper
    {
        INIT = 0x00,
        COMPLETE = 0x01,
        INCOMPLETE = 0x02,
        RUN_REFRIGERATOR = 0x03
    };

    /// <summary>
    /// Send command list
    /// </summary>
    /// 
    public enum CMD
    { 
        CMD_NOP = 0,
        CMD_TASK_WRITE = 0x01,
        CMD_TASK_WRITE_END = 0x02,
        CMD_GO = 0x03,
        CMD_STOP = 0x04,
        CMD_PARAM_WRITE = 0x05,
        CMD_PARAM_WRITE_END = 0x06,
        CMD_RESUME = 0x07,
        CMD_BOOTLAODER = 0x55
    };

    public class PCR_Task
	{
		private static List<Action_PCR> listAction = new List<Action_PCR>();       // Action list
		private static PCR_Task instance = null;

        //machine state
        public STATE State;
        public RES Response;


        public int USB_Error_Cnt = 0;   // 사용 여부 확인 필요 
        public int m_ErrorFlag = 0;     // 불필요해 보임 
        public string errorMsg = "";

        public static uint DEFAULT_VID = 0x04D8;
//#if DEBUG
//        public static uint DEFAULT_PID = 0xFB76;
//#else
        public static uint DEFAULT_PID = 0xEF7F;
//#endif
        public static uint BOOTLOADER_PID = 0x003C;

        // PCR Device 통신 관련 변수 
        public Byte[] readdata = new Byte[65];      //[2010.8.30 By Soda] 64 -> 65 로 변경
        public Byte[] writedata = new Byte[65];     //[2010.8.30 By Soda] 위와 동일

        public bool IsConnected = false;            // PCR Device 연결 여부 

        public bool m_bProtocolEnd = false;
        public bool m_bCompletePCR = false;
        public bool m_bPCRrunning = false;
        public bool m_bStopProcessing = false;


        // 에러 메시지 종류
        private static int ERROR_LID_OVER = 0x01;
		private static int ERROR_CHM_OVER = 0x02;
		private static int ERROR_LID_CHM_OVER = 0x03;
		private static int ERROR_HEATSINK_OVER = 0x04;
		private static int ERROR_LID_HEATSINK_OVER = 0x05;
		private static int ERROR_CHM_HEATSINK_OVER = 0x06;
		private static int ERROR_ALL = 0x07;
		private static int ERROR_TEMP_SENSOR = 0x08;
		private static int ERROR_ALL_SYSTEM = 0x0f;

		// Rx, Tx 버퍼를 생성해준다.
		private RxAction m_RxAction = null;
        public event Action<STATE> PCR_TaskRxActionStateChangeEvent;

        public RxAction RxAction
        {
            get { return m_RxAction; }
            set
            {
                m_RxAction = value;              
            }
        }

        public TxAction m_TxAction = null;

		// PCR 관련 변수들
		public int LED_Counter = 0;
		public int List_Counter = 0;
		public int Timer_Counter = 0;
		public int m_nCur_ListNumber = 0;

        // PCR 관련 플래그들

        public bool IsRunning
        {
            get { return m_bPCRrunning; }
        }
        public bool IsReadyToRun = true;
		public bool IsFinishPCR = false;
		public bool IsRefrigeratorEnd = false;
		public bool IsProtocolEnd = false;
		public bool IsAdmin = false;
		public bool IsGotoStart = false;
        public static string kunSerialNum = "";

        HidDeviceWrapper hidDeviceWrapper;

        /// <summary>
        /// PCR 생성자 
        /// </summary>
        public PCR_Task()
		{
			m_RxAction = new RxAction();
			m_TxAction = new TxAction();

            m_RxAction.RxActionChangeEvent += M_RxAction_RxActionChangeEvent;
            USB_Error_Cnt = 0;
        }

        /// <summary>
        /// RxAction  변경시에 발생 하는 Event Handler  각 State 변경시에 처리 
        /// </summary>
        private void M_RxAction_RxActionChangeEvent()
        {
            State = (STATE)m_RxAction.getState();
            Response = (RES)m_RxAction.getResponse();

            State_Oper curOperation = (State_Oper)m_RxAction.getCurrent_Operation();
            errorMsg = m_RxAction.ErrorMsg;

            PCR_TaskRxActionStateChangeEvent?.Invoke(State);

            switch (State)
            {  
                case STATE.STATE_RUN:
                    {


                    }
                    break;
                case STATE.STATE_READY:
                    {
                        switch (curOperation)
                        {
                            case State_Oper.COMPLETE:       //run 이 정상 종료 
                                {
                                    m_bCompletePCR = true;
                                    m_bPCRrunning = false;

                                    MessageBox.Show("PCR COMPLETE !!");
                                }
                                break;
                            case State_Oper.INCOMPLETE:     //run 중에 Incomplete 
                                {
                                    MessageBox.Show("PCR INCOMPLETE !!");
                                }
                                break;
                            case State_Oper.RUN_REFRIGERATOR:   //??  
                                {
                                    m_bProtocolEnd = true;
                                    m_bCompletePCR = true;
                                }
                                break;
                        }
                    }
                    break;
                case STATE.STATE_STOP:

                    break;
            }
        }

        /// <summary>
        /// Action 리스트를 모두 지운다.  
        /// </summary>
        public void listClear()
		{
			listAction.Clear();
		}
    
         /// <summary>
        /// Action 을 추가한다.  
        /// </summary>
        /// <param name="Label">레이블</param>
        /// <param name="Temp">온도</param>
        /// <param name="Time">시간</param>
        /// <param name="isCapture">캡쳐 여부</param>
        public void Action_Add(String Label, String Temp, String Time, bool isCapture)
		{
			Action_PCR actionPcr = new Action_PCR(Label, Temp, Time, isCapture);
			listAction.Add(actionPcr);
		}

        /// <summary>
        /// 해당 레이블의 Action 을 리턴한다. 
        /// </summary>
        /// <param name="Label">레이블</param>
        /// <returns>레이블에 해당하는 Action</returns>
        public Action_PCR GetAction(string Label)
        {
            Action_PCR actionPcr = null;
            //listAction.Add(actionPcr);

            for(int i=0; i< listAction.Count; i++)
            {
                actionPcr = listAction[i];
                if (actionPcr != null && actionPcr.getLabel() == Label)
                    break;
            }

            return actionPcr;
        }

        /// <summary>
        /// Action 개수를 가져온다.  
        /// </summary>
        /// <returns>Action 개수</returns>
        public int GetActionCount()
		{
			return listAction.Count;
		}

        /// <summary>
        /// 현재 Action 번호에 해당하는 Action을 리턴한다. 
        /// </summary>
        /// <param name="actionNo">Action 번호</param>
        /// <returns>Action</returns>
        public Action_PCR GetCurAction(int actionNo)
        {
            return listAction[actionNo];
        }

        /// <summary>
        /// 현재 Action 번호를 리턴한다. (Time이 0 또는 GOTO 인 Action 제외) 
        /// </summary>
        /// <param name="readActionNo">PCR에서 수신한 Action 번호</param>
        /// <returns>Action 번호</returns>
        public int GetRealActionNo(int readActionNo)
        {
            int actNo = 0;
            int count = 0;
            for (int i = 0; i < listAction.Count; i++)
            {
                int time = Convert.ToInt32(listAction[i].getTime());
                if (time > 0)
                {
                    count++;
                    if (count >= readActionNo)
                    {
                        actNo = i;
                        break;
                    }
                }
            }
            return actNo;
        }

        /// <summary>
        /// 현재 Action Capture 완료 여부를 저장한다. 
        /// </summary>
        /// <param name="actionNo">Action 번호</param>
        /// <param name="isComplete">완료 여부</param>
        public void SetCurActionComplete(int actionNo, bool isComplete)
        {
            if (isComplete)
            { listAction[actionNo].setComplete(isComplete); }
            else
            {
                for (int i = 0; i < listAction.Count; i++)
                    listAction[actionNo].setComplete(false);
            }
        }

        /// <summary>
        /// PCR 인터페이스를 초기화한다.   
        /// </summary>
        /// <returns>PCR 시리얼 번호</returns>
        public string PCR_Init()
        {
//            hidDeviceWrapper = new HidDeviceWrapper((int)DEFAULT_VID, 61311);   //test
            hidDeviceWrapper = new HidDeviceWrapper((int)DEFAULT_VID, (int)DEFAULT_PID);

            if (hidDeviceWrapper.IsConnect)
            {
                hidDeviceWrapper.Open();
                kunSerialNum = hidDeviceWrapper.SeringNum;
                hidDeviceWrapper.DeviceDisconnectEvent += HidDeviceWrapper_DeviceDisconnectEvent;
                hidDeviceWrapper.DeviceConnectEvent += HidDeviceWrapper_DeviceConnectEvent;
                hidDeviceWrapper.DataReceived += HidDeviceWrapper_DataReceived;
            }
            return kunSerialNum;
        }

        private void HidDeviceWrapper_DataReceived(byte[] obj)
        {
            RxAction.set_Info(obj);
        }

        private void HidDeviceWrapper_DeviceDisconnectEvent(object sender, EventArgs e)
        {
            IsConnected = false;
        }

        private void HidDeviceWrapper_DeviceConnectEvent(object sender, EventArgs e)
        {
            IsConnected = true;
        }

        /// <summary>
        /// Method를 PCR로 전송하고 Run 한다. 
        /// </summary>
        public void PCR_Run()
        {
            int m_nNoAct = listAction.Count;
            USB_Error_Cnt = 0;
            ManualResetEvent evnetWaitHandler = new ManualResetEvent(false);
            
            //Wait State Write Done message Event Handler
            Action<STATE> WaitWriteDoneHandler = null;
            WaitWriteDoneHandler = (state) =>
            {
                if (state == STATE.STATE_TASK_WRITE && Response == RES.RES_TASK_WR_DONE)
                {
                    this.PCR_TaskRxActionStateChangeEvent -= WaitWriteDoneHandler;
                    evnetWaitHandler.Set();
                }
                else 
                {
                    writedata = m_TxAction.Tx_NOP();
                    hidDeviceWrapper.Write(writedata);
                }
            };

            //Wait Write Task End Message Event Handler
            Action<STATE> WaitWriteTaskEndHandler = null;
            WaitWriteTaskEndHandler = (state) =>
            {
                if (state == STATE.STATE_READY && Response == RES.RES_TASK_ER_DONE)
                {
                    this.PCR_TaskRxActionStateChangeEvent -= WaitWriteTaskEndHandler;
                    evnetWaitHandler.Set();
                }
                else
                {
                    writedata = m_TxAction.Tx_NOP();
                    hidDeviceWrapper.Write(writedata);
                }
            };

            //Wait Go Done message Event Handler
            Action<STATE> WaitGoDoneHandler = null;
            WaitGoDoneHandler = (state) =>
            {
                if (state == STATE.STATE_RUN && Response == RES.RES_GO_DONE)
                {
                    m_bPCRrunning = true;
                    this.PCR_TaskRxActionStateChangeEvent -= WaitGoDoneHandler;
                    evnetWaitHandler.Set();
                }
                else
                {
                    writedata = m_TxAction.Tx_NOP();
                    hidDeviceWrapper.Write(writedata);
                }
            };

            Action<STATE> WaitAnyReplyHandler = null;
            WaitAnyReplyHandler = (state) =>
            {
                this.PCR_TaskRxActionStateChangeEvent -= WaitAnyReplyHandler;
                evnetWaitHandler.Set();
            };

            //write protocol data to device	
            //parsing pcr protocol to buffer
            for (int i = 0; i < m_nNoAct; i++)
            {
                Action_PCR pcrAction = listAction[i];
                if (pcrAction == null)
                    continue;

                string TargetLidHeater = Global.HEATER_TEMP.ToString();
                string label = pcrAction.getLabel();
                string temp = pcrAction.getTemp();
                string time = pcrAction.getTime();
                writedata = m_TxAction.Tx_TaskWrite(label, temp, time, TargetLidHeater, i);
                this.PCR_TaskRxActionStateChangeEvent += WaitWriteDoneHandler;
                hidDeviceWrapper.Write(writedata);
                if(!evnetWaitHandler.WaitOne(3000))
                {
                    throw new TimeoutException("에러: Task Write Packet 의 응답이 오지 않았습니다. ");
                }
            }

            //write task 끝내기
            Array.Clear(writedata, 0, writedata.Length);
            writedata = m_TxAction.Tx_TaskEnd();
            hidDeviceWrapper.Write(writedata);
            this.PCR_TaskRxActionStateChangeEvent += WaitWriteTaskEndHandler;
            if (!evnetWaitHandler.WaitOne(1000))
            {
                throw new TimeoutException("에러: Task Write Done Packet 의 응답이 오지 않았습니다.");
            }

            //run command		
            writedata = m_TxAction.Tx_Go();
            hidDeviceWrapper.Write(writedata);
            this.PCR_TaskRxActionStateChangeEvent += WaitGoDoneHandler;
            if (!evnetWaitHandler.WaitOne(1000))
            {
                throw new TimeoutException("에러: PCR Run Packet 응답이 오지 않습니다.");
            }

            writedata = m_TxAction.Tx_PwmOff();
            hidDeviceWrapper.Write(writedata);
            this.PCR_TaskRxActionStateChangeEvent += WaitAnyReplyHandler;
            if (!evnetWaitHandler.WaitOne(1000))
            {
                throw new TimeoutException("에러: 장비로 부터 응답이 오지 않습니다.");
            }
        }

        /// <summary>
        /// Capture 시 멈추었던 PCR 을 다시 진행한다. 
        /// </summary>
        public void PcrResume()
        {
            writedata = m_TxAction.Tx_TaskResume();
            hidDeviceWrapper.Write(writedata);
            Thread.Sleep(5);
            writedata = m_TxAction.Tx_NOP();
            hidDeviceWrapper.Write(writedata);
        }

        /// <summary>
        /// PCR 데이터를 읽어온다.  
        /// </summary>
        /// <returns>PCR 수신 데이터</returns>
        public string readKunPcrData()
        {
            ManualResetEvent evnetWaitHandler = new ManualResetEvent(false);

            Action<STATE> WaitAnyReplyHandler = null;
            WaitAnyReplyHandler = (state) =>
            {
                this.PCR_TaskRxActionStateChangeEvent -= WaitAnyReplyHandler;
                evnetWaitHandler.Set();
            };

            writedata = m_TxAction.Tx_NOP();
            ////send command to PCR device
            hidDeviceWrapper.Write(writedata);
            this.PCR_TaskRxActionStateChangeEvent += WaitAnyReplyHandler;
            if (!evnetWaitHandler.WaitOne(1000))
            {
                throw new TimeoutException("에러: 장비로 부터 응답이 오지 않습니다.");
            }

            string strState = ((STATE)m_RxAction.getState()).ToString();
            string strResponse = ((RES)m_RxAction.getResponse()).ToString();
            int totalAction = m_RxAction.getTotal_Action();
            int taskline = m_RxAction.getCurrent_Action();
            int taskLoop = m_RxAction.getCurrent_Loop();
            int totalTimeLeft = m_RxAction.getTotal_TimeLeft();
            double secTimeLeft = m_RxAction.getSec_TimeLeft();
            int label = m_RxAction.getLabel();
            int time = m_RxAction.getTime();
            int curTemp = m_RxAction.getTemp();
            int curOperation = m_RxAction.getCurrent_Operation();
            // Camber 온도를 가져온다. 
            double chamberTemp = m_RxAction.getChamber_Temp();
            // Cover 온도를 가져온다. 
            double coverTemp = m_RxAction.getCover_Temp();
            // Heatsink 온도를 가져온다. 
            double heatsinkTemp = m_RxAction.getHeatsink_Temp();

            string strText = "";
            strText = string.Format("state({0}),Response({1}), totalAction({2}), taskline({3}), taskLoop({4}), totalTimeLeft({5}), secTimeLeft({6}), label({7}), time({8}), curOperation({9}), chamberTemp({10}), coverTemp({11}), heatsinkTemp({12}), curTemp({13})",
                 strState, strResponse, totalAction, taskline, taskLoop, totalTimeLeft, secTimeLeft, label, time, curOperation, chamberTemp, coverTemp, heatsinkTemp, curTemp);

            return strText;
        }

        /// <summary>
        /// PCR 을 정지한다. 
        /// </summary>
        /// <returns>PCR 완료 여부</returns>
        public void PCR_Stop()
        {
            this.m_bPCRrunning = false;

            if (m_bStopProcessing == false)
            {
                m_bStopProcessing = true;

                ManualResetEvent evnetWaitHandler = new ManualResetEvent(false);

                Action<STATE> WaitStopDoneHandler = null;
                WaitStopDoneHandler = (state) =>
                {
                    switch (state)
                    {
                        case STATE.STATE_READY:
                        case STATE.STATE_STOP:
                            this.PCR_TaskRxActionStateChangeEvent -= WaitStopDoneHandler;
                            evnetWaitHandler.Set();
                            m_bStopProcessing = false;
                            break;
                        default:
                            writedata = m_TxAction.Tx_NOP();
                            hidDeviceWrapper.Write(writedata);
                            break;
                    }
                };

                Action<STATE> WaitAnyReplyHandler = null;
                WaitAnyReplyHandler = (state) =>
                {
                    this.PCR_TaskRxActionStateChangeEvent -= WaitAnyReplyHandler;
                    evnetWaitHandler.Set();
                };

                //PCR stop 명령어
                writedata = this.m_TxAction.Tx_Stop();
                hidDeviceWrapper.Write(writedata);
                this.PCR_TaskRxActionStateChangeEvent += WaitStopDoneHandler;
                if (!evnetWaitHandler.WaitOne(10000))
                {
                    throw new TimeoutException("에러: 장비로 부터 응답이 오지 않습니다.");
                }

                this.m_TxAction.Tx_PwmOff();
                this.PCR_TaskRxActionStateChangeEvent += WaitAnyReplyHandler;
                if (!evnetWaitHandler.WaitOne(1000))
                {
                    throw new TimeoutException("에러: 장비로 부터 응답이 오지 않습니다.");
                }
                
            }
        }
    }
}
