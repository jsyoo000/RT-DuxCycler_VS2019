using System;
using System.Collections.Generic;
using System.Linq;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using System.Drawing;
using Duxcycler_GLOBAL;
using Duxcycler.Properties;
using System.Drawing.Drawing2D;
using Duxcycler;

namespace Duxcycler_Database
{
    public struct IpPlate_Target
    {
        public bool check;
        public int colorIndex;
        public string name;
        public int reporter;
        public int quencher;
        public string comment;
        public int task;
        public string quantity;
    };

    public struct IpPlate_Sample
    {
        public bool check;
        public int colorIndex;
        public string name;
        public string comment;
    };

    public class Well_Info
    {
        //public int colIndex { get; set; }                 // Colum Index
        //public int rowIndex { get; set; }                 // Row Index

        public string wellTitle { get; set; }             // 선택된 Sample (셀 이름)
        public int foreColorIndex { get; set; }           // 선택된 Sample 칼라 (타원 색깔)
        public int backColorIndex { get; set; }           // 선택된 Bio Group 칼라 (배경 색깔)
        public bool isSelected { get; set; }              // 선택되었는지 여부 
        public double wellGain { get; set; }              // Well Gain 
        public double wellOffset { get; set; }            // Well Offset
        public double CtFAM { get; set; }                 // Well FAM Ct Value
        public double CtHEX { get; set; }                 // Well HEX Ct Value
        public double CtROX { get; set; }                 // Well ROX Ct Value
        public double CtCY5 { get; set; }                 // Well CY5 Ct Value

        public List<string> listTargetInfos = new List<string>();         // Selected plate target index List     
        public string SampleInfos = "";                                // Selected plate sample index List     
        public string BioGroupInfos = "";                                // Selected plate sample index List     
        public List<IpPlate_Target> listTargets = new List<IpPlate_Target>();          // Plate Target List     
    }

    public class WellManager : Well_Info
    {
        public List<Well_Info> listPlateInfos = new List<Well_Info>();          // Plate Target List     
        public int colCount { get; set; }                 // Colum Count
        public int rowCount { get; set; }                 // Row Count
        public double zoomValue { get; set; }             // Font Zoom Value

        public double ThFAM { get; set; }                 // Well FAM threshold
        public double ThHEX { get; set; }                 // Well HEX threshold
        public double ThROX { get; set; }                 // Well ROX threshold
        public double ThCY5 { get; set; }                 // Well CY5 threshold

        public int targetStartPosX = 5;                  // Target 이름 표시 X 위치 
        public int targetStartPosY = 5;                  // Target 이름 표시 Y 위치 

        // Font
        public Font Font { get { return this._font; } set { this._font = value; } }
        private Font _font = SystemFonts.DefaultFont;
        // 글자 색
        public Color FontForeColor { get { return this._fontforecolor; } set { this._fontforecolor = value; } }
        private Color _fontforecolor = Color.Black;
        // 글자 배경색
        public Color FontBackColor { get { return this._fontbackcolor; } set { this._fontbackcolor = value; } }
        private Color _fontbackcolor = Color.Transparent;

        /// <summary>
        /// Column, Row 개수만큼 Well 을 생성한다. (기본 : 5*5) 
        /// </summary>
        /// <param name="nCol">가로 개수</param>
        /// <param name="nRow">세로 개수</param>
        public WellManager(int nCol = 5, int nRow = 5)
        {
            ThFAM = 0.0;
            ThHEX = 0.0;
            ThROX = 0.0;
            ThCY5 = 0.0;

            colCount = nCol;
            rowCount = nRow;
            SetZoomScale(1.0);
            int i, j;
            for (i = 0; i < nCol; i++)
            {
                for (j = 0; j < nRow; j++)
                {
                    Well_Info wellInfo = new Well_Info();
                    wellInfo.wellTitle = "";
                    //wellInfo.colIndex = i;
                    //wellInfo.rowIndex = j;
                    wellInfo.isSelected = false;
                    listPlateInfos.Add(wellInfo);
                }
            }
        }

        /// <summary>
        /// 선택된 셀의 Gain, Offset 을 저장한다.  
        /// </summary>
        /// <param name="index">Well Index</param>
        /// <param name="gain">Gain</param>
        /// <param name="offset">Offset</param>
        public void SetOffsetGain(int index, double gain, double offset)
        {
            listPlateInfos[index].wellGain = gain;
            listPlateInfos[index].wellOffset = offset;
        }

        /// <summary>
        /// 선택된 Well의 Gain 값을 리턴한다. 
        /// </summary>
        /// <param name="index">Well Index</param>
        /// <returns>Gain 값</returns>
        public double GetGain(int index)
        {
            return listPlateInfos[index].wellGain;
        }

        /// <summary>
        /// 선택된 Well의 Offset 값을 리턴한다.
        /// </summary>
        /// <param name="index">Well Index</param>
        /// <returns>Offset 값</returns>
        public double GetOffset(int index)
        {
            return listPlateInfos[index].wellOffset;
        }

        /// <summary>
        /// Well의 선택 여부를 저장한다. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isSelect"></param>
        public void SelectWell(int index, bool isSelect)
        {
            listPlateInfos[index].isSelected = isSelect;
        }

        //  
        /// <summary>
        /// 선택된 Well의 Ct 값을 저장한다.  
        /// </summary>
        /// <param name="index">Well Index</param>
        /// <param name="dCtFAM">FAM Ct 값</param>
        /// <param name="dCtHEX">HEX Ct 값</param>
        /// <param name="dCtROX">ROX Ct 값</param>
        /// <param name="dCtCY5">CY5 Ct 값</param>
        public void SetCtValue(int index, double dCtFAM, double dCtHEX, double dCtROX, double dCtCY5)
        {
            listPlateInfos[index].CtFAM = dCtFAM;
            listPlateInfos[index].CtHEX = dCtHEX;
            listPlateInfos[index].CtROX = dCtROX;
            listPlateInfos[index].CtCY5 = dCtCY5;
        }

        /// <summary>
        /// 선택된 Well의 Ct 값을 리턴한다. 
        /// </summary>
        /// <param name="index">Well Index</param>
        /// <param name="dCtFAM">FAM Ct 값</param>
        /// <param name="dCtHEX">HEX Ct 값</param>
        /// <param name="dCtROX">ROX Ct 값</param>
        /// <param name="dCtCY5">CY5 Ct 값</param>
        public void GetCtValue(int index, ref double dCtFAM, ref double dCtHEX, ref double dCtROX, ref double dCtCY5)
        {
            dCtFAM = listPlateInfos[index].CtFAM;
            dCtHEX = listPlateInfos[index].CtHEX;
            dCtROX = listPlateInfos[index].CtROX;
            dCtCY5 = listPlateInfos[index].CtCY5;
        }

        /// <summary>
        /// 선택된 Well의 선택 상태를 리턴한다.
        /// </summary>
        /// <param name="index">Well Index</param>
        /// <returns>Well 선택 상태</returns>
        public bool IsSelectWell(int index)
        {
            return listPlateInfos[index].isSelected;
        }

        /// <summary>
        /// 전체 Well의 Zoom Scale을 변경한다.  
        /// </summary>
        /// <param name="zoomValue">Zoom Scale</param>
        public void SetZoomScale(double zoomValue)
        {
            this.zoomValue = zoomValue;

            float fontSize = (float)(4 * zoomValue);
            this.Font = new Font("Arial", fontSize);
            targetStartPosX = 15 + (int)(this.zoomValue * 8.0);
            targetStartPosY = 10 + (int)(this.zoomValue * 8.0);
        }

        /// <summary>
        /// 전체 Well 정보를 초기화한다. 
        /// </summary>
        public void ListClear()
        {
            int i, j;
            for (i = 0; i < colCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    int index = j + (i * rowCount);

                    listPlateInfos[index].listTargetInfos.Clear();
                    listPlateInfos[index].listTargets.Clear();
                    //listPlateInfos[index].listSampleInfos.Clear();
                    SampleInfos = "";
                    //listPlateInfos[index].listBioGroupInfos.Clear();
                    BioGroupInfos = "";

                    listPlateInfos[index].wellTitle = "";
                    listPlateInfos[index].foreColorIndex = -1;
                    listPlateInfos[index].backColorIndex = -1;
                }
            }

            SetZoomScale(1.0);
        }

        /// <summary>
        /// 해당 Well 정보를 리턴한다. 
        /// </summary>
        /// <param name="index">Well Index</param>
        /// <returns>해당 Well 정보</returns>
        public Well_Info GetWellInfo(int index)
        {
            if (index >= listPlateInfos.Count)
                return null;

            return listPlateInfos[index];
        }

        /// <summary>
        /// 해당 Well의 속성 데이터를 리턴한다. 
        /// </summary>
        /// <param name="plateType">plateType : 0(Target), 1(Sample), 2(Bio Group)</param>
        /// <param name="col">가로 위치</param>
        /// <param name="row">세로 위치</param>
        /// <returns>해당 Well의 속성 정보</returns>
        public string GetWellData(int plateType, int col, int row)
        {
            int index = col + (row * colCount);
            if (index >= listPlateInfos.Count)
                return "";

            string wellData = "";
            int dataCount = 0;
            Well_Info wellInfo = listPlateInfos[index];
            if(plateType == 0)
            {
                dataCount = wellInfo.listTargetInfos.Count;
                for (int i = 0; i < dataCount; i++)
                {
                    wellData += wellInfo.listTargetInfos[i].ToString() + "/";
                }
            }
            else if (plateType == 1)
            {
                wellData = wellInfo.SampleInfos.ToString();
            }
            else if (plateType == 2)
            {
                wellData = wellInfo.BioGroupInfos.ToString();
            }
            else if (plateType == 3)
            {
                wellData = String.Format("{0:#.00}/{1:#.00}/{2:#.00}/{3:#.00}", wellInfo.CtFAM, wellInfo.CtHEX, wellInfo.CtROX, wellInfo.CtCY5);
            }

            return wellData;
        }

        /// <summary>
        /// 해당 Well의 이미지를 생성하여 리턴한다.  
        /// </summary>
        /// <param name="col">가로 위치</param>
        /// <param name="row">세로 위치</param>
        /// <param name="imgWidth">이미지 가로 크기</param>
        /// <param name="imgHeight">이미지 세로 크기</param>
        /// <returns>Well Image</returns>
        public Image GetWellImage(int col, int row, int imgWidth, int imgHeight)
        {
            int index = col + (row * colCount);
            if (index >= listPlateInfos.Count)
                return null;

            //int imgWidth = Resources.WellBkColor_Up.Width;
            //int imgHeight = Resources.WellBkColor_Up.Height;

            Bitmap wellImage = new Bitmap(imgWidth, imgHeight);

            // Bio Group 칼라를 적용한다. (배경색)
            Graphics wellGraphics = Graphics.FromImage(wellImage);
            int backIndex = listPlateInfos[index].backColorIndex;
            Brush backBrush = null;
            if (backIndex > 0)
                backBrush = new SolidBrush(Global.colorList[backIndex]);
            else
                backBrush = new SolidBrush(Color.FromArgb(235, 235, 235));

            wellGraphics.FillRectangle(backBrush, new Rectangle(0, 0, imgWidth, imgHeight));

            // Sample 칼라를 적용한다. (타원색)
            Point _Left1 = new Point(0, 0);
            Point _Left2 = new Point(imgWidth / 2, 0);
            Point _Left3 = new Point(imgWidth / 2, imgHeight);
            Point _Left4 = new Point(0, imgHeight);

            Point[] _Point = new Point[] { _Left1, _Left2, _Left3, _Left4 };
            PathGradientBrush _SetBruhs = new PathGradientBrush(_Point, WrapMode.TileFlipX);
            _SetBruhs.CenterPoint = new PointF(imgWidth / 2, imgHeight / 2);
            _SetBruhs.FocusScales = new PointF(0, 0);

            int foreIndex = listPlateInfos[index].foreColorIndex;
            if (foreIndex > 0)
                _SetBruhs.CenterColor = Global.colorList[foreIndex];
            else
                _SetBruhs.CenterColor = Color.FromArgb(226, 226, 226);

            _SetBruhs.SurroundColors = new Color[] { Color.White };
            wellGraphics.FillEllipse(_SetBruhs, new Rectangle(0, 0, imgWidth, imgHeight));

            SizeF size;
            Point pntText;
            // Sample Title 을 표시한다. 
            if (listPlateInfos[index].wellTitle.Length > 0)
            {
                size = wellGraphics.MeasureString(listPlateInfos[index].wellTitle, this.Font);
                pntText = new Point((int)(imgWidth / 2) - (int)(size.Width / 2), 5);
                // Back 글자
                wellGraphics.DrawString(listPlateInfos[index].wellTitle, this.Font, new SolidBrush(this.FontBackColor), pntText);
                // 원래 글자
                wellGraphics.DrawString(listPlateInfos[index].wellTitle, this.Font, new SolidBrush(this.FontForeColor), pntText);
            }

            // Target 리스트를 표시한다. 
            int targetCount = listPlateInfos[index].listTargetInfos.Count;
            int startX = targetStartPosX;
            int startY = targetStartPosY;
            for (int i=0; i < targetCount; i++)
            {
                int tIndex = -1;
                string tName = listPlateInfos[index].listTargetInfos[i];
                for (int j = 0; j < Global.listTargetInfos.Count; j++)
                {
                    if(tName == Global.listTargetInfos[j].name)
                    {
                        tIndex = j;
                        break;
                    }
                }

                if (tIndex < 0 || tIndex >= Global.listTargetInfos.Count)
                    continue;

                IpPlate_Target targetInfo = Global.listTargetInfos[tIndex];

                string dispString = targetInfo.name;
                size = wellGraphics.MeasureString(dispString, this.Font);
                int taskSize = (int)size.Height - 2;

                Brush textBkBrush = null;
                textBkBrush = new SolidBrush(Global.colorList[targetInfo.colorIndex]);
                wellGraphics.FillRectangle(textBkBrush, new Rectangle(startX - taskSize - 4, startY - 2, taskSize + (int)size.Width + 6, (int)size.Height + 4));

                pntText = new Point(startX, startY);
                // Back 글자
                wellGraphics.DrawString(targetInfo.name, this.Font, new SolidBrush(Color.Transparent), pntText);
                // 원래 글자
                wellGraphics.DrawString(targetInfo.name, this.Font, new SolidBrush(this.FontForeColor), pntText);

                if (targetInfo.task >= 0)
                {
                    wellGraphics.DrawImage(Global.Tasks[targetInfo.task], startX - taskSize - 2, startY, taskSize, taskSize);
                }

                string strCt = "Undetermined";
                SizeF CtSize = wellGraphics.MeasureString(strCt, this.Font);
                int CtStartX = startX + (int)CtSize.Width - taskSize - 4;
                wellGraphics.FillRectangle(textBkBrush, new Rectangle(CtStartX, startY - 2, (int)size.Width + 6, (int)size.Height + 4));

                pntText = new Point(CtStartX, startY);
                if (targetInfo.reporter == 0 && listPlateInfos[index].CtFAM > 0.0)
                    strCt = String.Format("Ct:{0:#.00}", listPlateInfos[index].CtFAM);
                else if (targetInfo.reporter == 1 && listPlateInfos[index].CtHEX > 0.0)
                    strCt = String.Format("Ct:{0:#.00}", listPlateInfos[index].CtHEX);
                else if (targetInfo.reporter == 2 && listPlateInfos[index].CtROX > 0.0)
                    strCt = String.Format("Ct:{0:#.00}", listPlateInfos[index].CtROX);
                else if (targetInfo.reporter == 3 && listPlateInfos[index].CtCY5 > 0.0)
                    strCt = String.Format("Ct:{0:#.00}", listPlateInfos[index].CtCY5);

                // Back 글자
                wellGraphics.DrawString(strCt, this.Font, new SolidBrush(Color.Transparent), pntText);
                // 원래 글자
                wellGraphics.DrawString(strCt, this.Font, new SolidBrush(this.FontForeColor), pntText);

                startY += (int)size.Height + 2;
            }

            return wellImage;
        }

        /// <summary>
        /// Target의 선택 여부에 따라 해당 셀에 선택된 Target 인덱스를 추가 또는 삭제한다.  
        /// </summary>
        /// <param name="col">가로 위치</param>
        /// <param name="row">세로 위치</param>
        /// <param name="targetName">Target 이름</param>
        /// <param name="bCheck">선택 여부</param>
        /// <param name="targetInfo">Target 정보</param>
        public void UpdateTargetIndex(int col, int row, string targetName, bool bCheck, IpPlate_Target targetInfo)
        {
            int index = col + (row * colCount);
            if (index >= listPlateInfos.Count)
                return;

            Well_Info wellInfo = listPlateInfos[index];
            // 같은 인덱스가 있는지 확인한다. 
            int targetCount = wellInfo.listTargetInfos.Count;
            int removeIndex = -1;
            for (int i=0; i< targetCount; i++)
            {
                if (wellInfo.listTargetInfos[i] == targetName)
                {
                    removeIndex = i;
                    break;
                }
            }

            // 타겟이 선택되고 중복되는 인덱스가 없는 경우 추가한다. 
            //if (bCheck && removeIndex == -1)
            if (bCheck)
            {
                if (removeIndex == -1)
                {
                    wellInfo.listTargetInfos.Add(targetName);
                    wellInfo.listTargets.Add(targetInfo);
                }
            }
            // 현재 인덱스가 존재하면 삭제한다. 
            else if (removeIndex != -1)
            {
                wellInfo.listTargetInfos.RemoveAt(removeIndex);
                //데이터를 포함한 구조체의 List 내의 인덱스를 구하고 삭제한다.
                int nIndex = wellInfo.listTargets.FindIndex(x => x.name == targetInfo.name);
                if (nIndex >= 0)
                    wellInfo.listTargets.RemoveAt(nIndex);
            }
        }

        /// <summary>
        /// 현재 Well에 해당 Target이 있는지 확인한다.  
        /// </summary>
        /// <param name="wellIndex">Well Index</param>
        /// <param name="reporterIndex">Target Index</param>
        /// <returns>Target Index : 없으면 -1이 리턴된다.</returns>
        public int FindTargetIndex(int wellIndex, int reporterIndex)
        {
            //Well_Info wellInfo = listPlateInfos[wellIndex];
            //int nIndex = wellInfo.listTargets.FindIndex(x => x.reporter == reporterIndex);
            int nIndex = listPlateInfos[wellIndex].listTargets.FindIndex(x => x.reporter == reporterIndex);
            return nIndex;
        }

        /// <summary>
        /// Sample의 선택 여부에 따라 해당 Well에 선택된 Sample 인덱스를 추가 또는 삭제한다.
        /// </summary>
        /// <param name="col">가로 위치</param>
        /// <param name="row">세로 위치</param>
        /// <param name="sampleName">Sample 이름</param>
        /// <param name="bCheck">선택 여부</param>
        /// <param name="sampleInfo">Sample 정보</param>
        public void UpdateSampleIndex(int col, int row, string sampleName, bool bCheck, IpPlate_Sample sampleInfo)
        {
            int index = col + (row * colCount);
            if (index >= listPlateInfos.Count)
                return;

            Well_Info wellInfo = listPlateInfos[index];
            if (bCheck)
            {
                wellInfo.SampleInfos = sampleName;
                wellInfo.wellTitle = sampleInfo.name;
                wellInfo.foreColorIndex = sampleInfo.colorIndex;
                listPlateInfos[index] = wellInfo;
            }
            else 
            {
                wellInfo.SampleInfos = "";
                wellInfo.wellTitle = "";
                wellInfo.foreColorIndex = -1;
            }
            listPlateInfos[index] = wellInfo;
        }

        /// <summary>
        /// Bio Group의 선택 여부에 따라 해당 Well에 선택된 Bio Group 인덱스를 추가 또는 삭제한다.
        /// </summary>
        /// <param name="col">가로 위치</param>
        /// <param name="row">세로 위치</param>
        /// <param name="sampleName">Bio Group 이름</param>
        /// <param name="bCheck">선택 여부</param>
        /// <param name="sampleInfo">Bio Group 정보</param>
        public void UpdateBioGroupIndex(int col, int row, string bioGroupName, bool bCheck, IpPlate_Sample bioGroupInfo)
        {
            int index = col + (row * colCount);
            if (index >= listPlateInfos.Count)
                return;

            Well_Info wellInfo = listPlateInfos[index];

            // Bio Group이 선택되고 중복되는 인덱스가 없는 경우 추가한다. 
            if (bCheck) // && removeIndex == -1)
            {
                //wellInfo.listBioGroupInfos.Add(bioGroupName);
                wellInfo.BioGroupInfos = bioGroupName;
                wellInfo.backColorIndex = bioGroupInfo.colorIndex;
            }
            // 현재 인덱스가 존재하면 삭제한다. 
            else  //if (removeIndex != -1)
            {
                //wellInfo.listBioGroupInfos.RemoveAt(removeIndex);
                wellInfo.BioGroupInfos = "";
                wellInfo.backColorIndex = -1;
            }
            listPlateInfos[index] = wellInfo;
        }

        /// <summary>
        /// 해당 Well의 Target 정보에서 해당 Target을 삭제한다.  
        /// </summary>
        /// <param name="deleteName">Target 이름</param>
        /// <returns>성공 여부</returns>
        public bool DeleteCheckTarget(string deleteName)
        {
            // 현재 아이템이 셀에 적용되어 있으면 지울건지 메세지 박스를 띄운다. 
            int i, j;
            for (i = 0; i < colCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    int index = j + (i * rowCount);

                    int listCount = listPlateInfos[index].listTargetInfos.Count;
                    for (int row = 0; row < listCount; row++)
                    {
                        //if (listPlateInfos[index].listTargetInfos[row].check && listPlateInfos[index].listTargetInfos[row].name == targetInfo.name)
                        if(listPlateInfos[index].listTargetInfos[row] == deleteName)
                        {
                            if (MessageBox.Show("The Target is assigned to wells in the plate. Are you sure you want to remove this target from all wells in the plate?",
                                "Warnning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 모든 Well에서 해당 인덱스를 삭제한다.  
        /// </summary>
        /// <param name="plateType">plateType : 0(Target), 1(Sample), 2(Bio Group)</param>
        /// <param name="deleteName"></param>
        public void DeleteAllRowIndex(int plateType, string deleteName)
        {
            int i, j;
            for (i = 0; i < colCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    int index = j + (i * rowCount);

                    if (plateType == 0)
                    {
                        int listCount = listPlateInfos[index].listTargetInfos.Count;
                        for (int row = 0; row < listCount; row++)
                        {
                            if (listPlateInfos[index].listTargetInfos[row] == deleteName)
                            {
                                listPlateInfos[index].listTargetInfos.RemoveAt(row);
                                break;
                            }
                        }
                    }
                    else if (plateType == 1)
                    {
                        if (listPlateInfos[index].SampleInfos == deleteName)
                        {
                            listPlateInfos[index].SampleInfos = "";
                            listPlateInfos[index].wellTitle = "";
                            listPlateInfos[index].foreColorIndex = -1;
                        }
                    }
                    else if (plateType == 2)
                    {
                        if (listPlateInfos[index].BioGroupInfos == deleteName)
                        {
                            listPlateInfos[index].BioGroupInfos = "";
                            listPlateInfos[index].backColorIndex = -1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 해당 Well의 Sample 정보에서 해당 Sample을 삭제한다.  
        /// </summary>
        /// <param name="deleteName">Sample 이름</param>
        /// <returns>성공 여부</returns>
        public bool DeleteCheckSample(string deleteName)
        {
            // 현재 아이템이 셀에 적용되어 있으면 지울건지 메세지 박스를 띄운다. 
            int i, j;
            for (i = 0; i < colCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    int index = j + (i * rowCount);

                    if (listPlateInfos[index].SampleInfos == deleteName)
                    {
                        if (MessageBox.Show("The sample is assigned to wells in the plate. Are you sure you want to remove this sample from all wells in the plate?",
                            "Warnning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                            return true;
                        else
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 해당 Well의 Bio Group 정보에서 해당 Bio Group을 삭제한다.  
        /// </summary>
        /// <param name="bioName">Bio Group 이름</param>
        /// <returns>성공 여부</returns>
        public bool DeleteCheckBioGroup(string bioName)
        {
            // 현재 아이템이 셀에 적용되어 있으면 지울건지 메세지 박스를 띄운다. 
            int i, j;
            for (i = 0; i < colCount; i++)
            {
                for (j = 0; j < rowCount; j++)
                {
                    int index = j + (i * rowCount);

                    if (listPlateInfos[index].BioGroupInfos == bioName)
                    {
                        if (MessageBox.Show("The biological replicate group is assigned to wells in the plate. Are you sure you want to remove this biological replicate group from all wells in the plate?",
                            "Warnning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                            return true;
                        else
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
