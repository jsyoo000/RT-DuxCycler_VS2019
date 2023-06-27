using BitmapControl;
using Duxcycler_GLOBAL;
using Duxcycler_IMAGE;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Duxcycler_Database
{
    public enum MARK_TYPE { L_MARK, R_MARK, NONE };

    public class ImageInfo
    {
        public string ChartNo { get; set; }                 // Char NO
        public int SID { get; set; }                        // Study ID
        public int StudyID { get; set; }                    // 환자의 따른 Study ID
        public int ImageIndex { get; set; }                 // Image Index
        public DateTime ImageDateTime { get; set; }         // Image 생성 시간
        public int Image_Width { get; set; }                // Image X Size
        public int Image_Height { get; set; }               // Image Y Size
        public int Pixel_Format { get; set; }               // Pixel Format (8bit, 24bit, 32bit)

        public int StudyNo { get; set; }                    // 이미지 번호 
        public int FilterType { get; set; }                 // Filter 종류 (0:FAM, 1:HEX, 2:ROX, CY5)

        public int ROIID = 0;                               // ROI의 ID이다. listShape의 마지막 ROI의 ID부터 증가한다.
        public List<ROIShape> listShape = new List<ROIShape>();          // ROI 도형 List     

        #region Option 인체 영역의 온도값 저장
        public List<ROIShape> listRemoveShape = new List<ROIShape>();    // 인체 영역시 제거 ROI 도형 List
        #endregion

        private Byte[] buffer = null;                       // Image Buffer
        public Byte[] ImageBuffer
        {
            get { return buffer; }
            set { if (value != null && value.Length > 0) { buffer = value.ToArray(); } }
        }
        // 이미지 마크 표시 여부
        public MARK_TYPE Mark_Left = MARK_TYPE.NONE;      // 이미지 왼쪽 마크 표시( L_MARK : 'L', R_MARK: 'R', NONE: 없음 )
        public MARK_TYPE Mark_Right = MARK_TYPE.NONE;       // 이미지 왼쪽 마크 표시( L_MARK : 'L', R_MARK: 'R', NONE: 없음 )
        // 등온선 설정
        public bool IsISOTherm { get { return this._isisotherm; } set { this._isisotherm = value; } }
        private bool _isisotherm = false;

        public int maxWindowLevel = 255;
        public int minWindowLevel = 0;
        public int iIsoLow = 0;
        public int iIsoHigh = 255;

        public bool IsWL_BG = false;

        private Color[] palette = new Color[256];
        public Color[] ImagePalette                         // Image Palete
        {
            get { return palette; }
            set { if (value != null && value.Length > 0) { palette = value.ToArray(); } }
        }
        public string ImageComment = "";                    // Image Comment

        public COLOR_TYPE selectedPaletteType = COLOR_TYPE.COLOR_256;        // 선택된 palette Type

        //public int ROIFontSize
        //{
        //    get { return this.roifontsize; }
        //    set
        //    {
        //        if (value > 20) this.roifontsize = 20;
        //        else if (value < 6) this.roifontsize = 6;
        //        else this.roifontsize = value;
        //    }
        //}
        //public int roifontsize = 9;                                       // ROI Font Size 설정

        // ROI 도형의 Clone 생성( 원본의 모양과 point 정보만 입력)
        public ImageInfo Clone()
        {
            ImageInfo clone = new ImageInfo
            {
                ChartNo = this.ChartNo,                         // Patient ChartNo
                SID = this.SID,                             // 고유 Study ID
                StudyID = this.StudyID,                         // Patient에 해당하는 Study ID
                ImageIndex = this.ImageIndex,                      // Image Index
                ImageDateTime = this.ImageDateTime,                   // Image 생성 시간                
                Image_Width = this.Image_Width,                     // 이미지 x Resultion 입력
                Image_Height = this.Image_Height,                    // 이미지 y Resultion 입력
                Pixel_Format = this.Pixel_Format,        // Image Format

                ROIID = this.ROIID,                           // ROI의 ID이다. listShape의 마지막 ROI의 ID부터 증가한다.

                IsISOTherm = this.IsISOTherm,                      // 등온선 설정

                minWindowLevel = this.minWindowLevel,                  // min WindowLevel 입력
                maxWindowLevel = this.maxWindowLevel,                  // max WindowLevel 입력
                iIsoLow = this.iIsoLow,                         // ISO Min 값 입력
                iIsoHigh = this.iIsoHigh,                        // ISO Max 값 입력

                IsWL_BG = this.IsWL_BG,

                ImageComment = this.ImageComment,                    // Image Comment
                selectedPaletteType = this.selectedPaletteType,             // 선택된 Palette Type를 저장한다. 이것은 PALETTE COLOR {0}, type 형태로 저장된다.

                //ROIFontSize = this.ROIFontSize,                     // ROI Font Size 설정

                ImagePalette = (Color[])this.ImagePalette.Clone(),   // 적용된 Palette 입력
                ImageBuffer = (byte[])this.ImageBuffer.Clone()      // 이미지 저장( Resize, Filter 적용 이미지 )
            };

            // ROI 정보 입력한다.
            clone.listShape.Clear();
            foreach (ROIShape roi in this.listShape) clone.listShape.Add(roi.CopyTo());

            #region Option 인체 영역의 온도값 저장
            // 인체 영역시 제거 ROI 도형
            clone.listRemoveShape.Clear();
            foreach (ROIShape roi in this.listRemoveShape) clone.listRemoveShape.Add(roi.CopyTo());
            #endregion 


            return clone;
        }

        public OpenCvSharp.Mat ToMat()
        {
            // 원본 이미지 Data를 Mat형식으로 변경
            //OpenCvSharp.Mat imgMat = new OpenCvSharp.Mat(new OpenCvSharp.Size(this.Image_Width, this.Image_Height), OpenCvSharp.MatType.CV_8U);
            //Marshal.Copy(this.ImageBuffer, 0, imgMat.Data, this.ImageBuffer.Length);

            //return imgMat;

            //return new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8U, this.ImageBuffer);

            if (this.Pixel_Format == 24)
                return new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8UC(3), this.ImageBuffer);
            //else if (this.Pixel_Format == 32)
            //    return new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8UC(3), this.ImageBuffer);
            else
                return new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8U, this.ImageBuffer);
        }

        public Bitmap CopyDataToBitmap(int imgWidth, int imgHeight, byte[] data)
        {
            //Marshal.Copy(data, 0, this.ImageBuffer, data.Length);

            Bitmap image = new Bitmap(imgWidth, imgHeight, PixelFormat.Format24bppRgb);
            BitmapData bmpdata = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(data, 0, bmpdata.Scan0, data.Length);
            image.UnlockBits(bmpdata);

            //Return the bitmap 
            return image;

        }

        // Bitmap으로 변환하는 함수.
        public Bitmap ToColorBitmap()
        {
            OpenCvSharp.Mat imageMat = null;
            Bitmap bmpImage = null;
            try
            {
                imageMat = new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8UC3, this.ImageBuffer);
                bmpImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imageMat, PixelFormat.Format24bppRgb);
                imageMat.Dispose();
            }
            catch (AccessViolationException e)
            {
                Console.WriteLine("File Read Error!");
            }

            return bmpImage;
        }

        public Bitmap ToBitmap(BitmapImageCtl bitmapimageCtl = null, bool BackColorBlack = true, bool IsTempAlarm = false,
            double LowTemperature = 14.00, double HighTemperature = 40.00, double TempAlarmValue = 37.5)
        {
            //OpenCvSharp.Mat imageMat = new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8U, this.ImageBuffer);
            //Bitmap bmpImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imageMat);

            OpenCvSharp.Mat imageMat = null;
            Bitmap bmpImage = null;
            if (Pixel_Format == 24)
            {
                imageMat = new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8UC3, this.ImageBuffer);
                bmpImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imageMat, PixelFormat.Format24bppRgb);
            }
            else
            {
                imageMat = new OpenCvSharp.Mat(this.Image_Height, this.Image_Width, OpenCvSharp.MatType.CV_8U, this.ImageBuffer);
                bmpImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imageMat);

                BitmapImageCtl bitCtl = new BitmapImageCtl();

                if (bitmapimageCtl != null) bitCtl.SetPalette(bitmapimageCtl.selectedType);
                else bitCtl.SetPalette(this.selectedPaletteType);

                if (IsTempAlarm)    // 온도 Alarm용 Color 적용
                    bmpImage = bitCtl.SetPaletteBitmapAlarm(bmpImage, LowTemperature, HighTemperature, TempAlarmValue);
                else                    // 일반 Color 적용
                {
                    Color[] setPalette = bitCtl.GetPalette(this.minWindowLevel, this.maxWindowLevel, this.IsWL_BG,
                                                            nISOLow: this.iIsoLow, nISOHigh: this.iIsoHigh);  // 변경된 palette를 적용한다.
                                                                                                              // Palette 적용
                    var newAliasForPalette = bmpImage.Palette; // Palette loaded from graphic device

                    for (int i = 0; i < 256; i++)
                    {
                        Color setColor = setPalette[i];
                        if (BackColorBlack == false && setColor == Color.FromArgb(0, 0, 0))
                        {
                            setColor = Color.FromArgb(255, 255, 255);
                        }
                        newAliasForPalette.Entries[i] = setColor;
                    }
                    bmpImage.Palette = newAliasForPalette; // Palette data wrote back to the graphic device
                }
            }

            return bmpImage;
        }

        // ROI와 Palette가 포함된 Bitmap 
        public Bitmap ToROIBitmap(double LevelLow = 14.00, double LevelHigh = 40.00, BitmapImageCtl bitmapimageCtl = null, bool BackColorBlack = true, bool ShowPalette = true, bool ShowROI = true, bool ShowDiff = false, bool IsTempAlarm = false, double TempAlarmValue = 37.5)
        {
            int MakeBitmapHeight = this.Image_Height;   // 만들 Bitmap 높이
            int MakeBitmapWidth = this.Image_Width;     // 만들 Bitmat 넓이
            if (ShowPalette) MakeBitmapWidth += 60;     // Palette를 추가할 경우 이미지 사이즈를 키운다.

            Bitmap imgMake = new Bitmap(MakeBitmapWidth, MakeBitmapHeight);//, PixelFormat.Format32bppRgb);            
            using (Graphics grp = Graphics.FromImage(imgMake))
            {
                grp.FillRectangle(Brushes.White, new Rectangle(0, 0, imgMake.Width, imgMake.Height));                       // 기본 이미지 색을 White로 설정

                grp.DrawImage(this.ToBitmap(bitmapimageCtl, BackColorBlack, IsTempAlarm, LevelLow, LevelHigh, TempAlarmValue), new Rectangle(0, 0, this.Image_Width, this.Image_Height));      // ImageInfo에 해당하는 Bitmap를 그린다.

                // 왼쪽의 마크 그리기
                if (this.Mark_Left != MARK_TYPE.NONE)
                {
                    string strMark = "L";
                    Font markFont = new Font("Segoe UI", 22);

                    if (this.Mark_Left == MARK_TYPE.L_MARK) strMark = "L";
                    else strMark = "R";
                    Rectangle markTop = new Rectangle(0, 0, (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
                    grp.DrawString(strMark, markFont, Brushes.Black, markTop,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                    markTop.Offset(-1, -1);
                    grp.DrawString(strMark, markFont, Brushes.White, markTop,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });

                    Rectangle markBottom = new Rectangle(0, this.Image_Height - markFont.Height,
                                                            (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
                    grp.DrawString(strMark, markFont, Brushes.Black, markBottom,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                    markBottom.Offset(-1, -1);
                    grp.DrawString(strMark, markFont, Brushes.White, markBottom,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }

                // 오른쪽 마크 그리기
                if (this.Mark_Right != MARK_TYPE.NONE)
                {
                    string strMark = "L";
                    Font markFont = new Font("Segoe UI", 22);

                    if (this.Mark_Right == MARK_TYPE.L_MARK) strMark = "L";
                    else strMark = "R";
                    int rLeft = this.Image_Width - (int)((markFont.SizeInPoints * strMark.Length) + 1);
                    Rectangle markTop = new Rectangle(rLeft, 0,
                                                        (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
                    grp.DrawString(strMark, markFont, Brushes.Black, markTop,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                    markTop.Offset(-1, -1);
                    grp.DrawString(strMark, markFont, Brushes.White, markTop,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                    Rectangle markBottom = new Rectangle(rLeft, this.Image_Height - markFont.Height,
                                                            (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
                    grp.DrawString(strMark, markFont, Brushes.Black, markBottom,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                    markBottom.Offset(-1, -1);
                    grp.DrawString(strMark, markFont, Brushes.White, markBottom,
                                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }

                // ROI를 읽어와서 image Size 기준으로 점을 그린다.
                if (ShowROI)
                {
                    foreach (ROIShape cShape in this.listShape)
                    {
                        //cShape.Font = new Font(cShape.Font.FontFamily, this.ROIFontSize, cShape.Font.Style);                    // ImageInfo에 설정된 Font Size로 설정함
                        cShape.Font = new Font(cShape.Font.FontFamily, Global.Roi_FontSize, FontStyle.Bold);                    // ImageInfo에 설정된 Font Size로 설정함
                        cShape.Draw(grp, this.Image_Width, this.Image_Height, showAvg: true, showDiff: ShowDiff, IsPrint: true);             // ROI와 평균온도를 그린다.    
                    }
                }

                // Palette 를 보여줄지 선택
                if (ShowPalette)
                {
                    Byte[,] twopaletteData = new Byte[256, 24];                                 // Palette Bitmap을 만들기 위한 2차원 배열
                    Byte[] paletteDataV = new Byte[256 * 24];                                // 세로 Palette Bitmap 만들때 사용할 1차원 배열( 한번만 만들면 되기때문에 전역을 선언함.)
                    int lengthX = twopaletteData.GetLength(0);
                    int lengthY = twopaletteData.GetLength(1);

                    for (int yRes = 0; yRes < lengthY; yRes++)
                        for (int xRes = 0; xRes < lengthX; xRes++)
                            twopaletteData[xRes, yRes] = Convert.ToByte(lengthX - xRes - 1);
                    // 생성시 Palette용 Bitmap Data를 만든다.
                    // 2차배열을 1차배열로 변경 함수(2차배열, 2차배열 시작위치, 1차배열, 1차배열 시작위치, 복사할 Byte 수)
                    System.Buffer.BlockCopy(twopaletteData, 0, paletteDataV, 0, sizeof(Byte) * twopaletteData.Length);
                    int isoLow = 0;
                    int isoHigh = 255;
                    if (this.iIsoLow != 0 && this.iIsoHigh != 255)
                    {
                        int nRange = Math.Abs(this.iIsoHigh - this.iIsoLow);
                        isoLow = Math.Abs(this.minWindowLevel - this.iIsoLow) * 255 / nRange;
                        isoHigh = 255 - (Math.Abs(this.iIsoHigh - this.maxWindowLevel) * 255 / nRange);

                        if (this.minWindowLevel < this.iIsoLow) isoLow = 0;
                        if (this.maxWindowLevel > this.iIsoHigh) isoHigh = 255;
                    }

                    Bitmap bmpPalette;

                    if (bitmapimageCtl != null)
                        bmpPalette = bitmapimageCtl.GetPaletteBitmap(paletteDataV, lengthY, lengthX, isoLow, isoHigh);
                    else
                    {
                        BitmapImageCtl bitCtl = new BitmapImageCtl();
                        bitCtl.SetPalette(this.selectedPaletteType);
                        bmpPalette = bitCtl.GetPaletteBitmap(paletteDataV, lengthY, lengthX, isoLow, isoHigh);
                    }

                    grp.DrawImage(bmpPalette, new Rectangle(this.Image_Width + 6, 0, 24, imgMake.Height));      // ImageInfo에 해당하는 Bitmap를 그린다.

                    // 온도값 표시
                    double highTemp = this.maxWindowLevel * (LevelHigh - LevelLow) / 255 + LevelLow;
                    double lowTemp = this.minWindowLevel * (LevelHigh - LevelLow) / 255 + LevelLow;

                    int tempCount = 17;
                    Font IndexFont = new System.Drawing.Font("Segoe UI", 8F);

                    double gapLocH = (float)(MakeBitmapHeight - IndexFont.Height) / (float)(tempCount - 1);
                    double gapTemp = (highTemp - lowTemp) / (float)(tempCount - 1);
                    double fTemp = highTemp;
                    double locH = 0;

                    for (int index = 0; index < tempCount; index++)
                    {
                        string strTemp = String.Format("{0:0.00}", fTemp);
                        Rectangle IndexRect = new Rectangle(this.Image_Width + 30, (int)locH, (int)(IndexFont.SizeInPoints * strTemp.Length), IndexFont.Height);

                        grp.DrawString(strTemp, IndexFont, Brushes.Black, IndexRect,
                                                new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });
                        locH += gapLocH;
                        fTemp -= gapTemp;
                    }
                }
            }

            return imgMake;
        }

        // Histo Bitmap를 만든다
        public Bitmap ToHistoBitmap(BitmapImageCtl bitmapimageCtl = null)
        {
            #region Histogram에 사용하는 변수 모음
            int minLevel = this.minWindowLevel;
            int maxLevel = this.maxWindowLevel;

            int histoMax = 200;                                                  // Histo 그래프 그리는 Bitmap 크기
            OpenCvSharp.Mat histMat = new OpenCvSharp.Mat();                                // Histogram Mat 변수                    
            const int histogramSize = 256;                                                  // Histogram Size  변수
            int[] dimensions = { histogramSize };                                    // Histogram size for each dimension
            OpenCvSharp.Rangef[] ranges = { new OpenCvSharp.Rangef(1, 254) };                   // Histogram min/max값 설정 ( Histogram 구할때 0, 255는 제외( 1 ~ 254 ) 사용 )
            Color gridColor = Color.FromArgb(255, 128, 0);                          // Histogram Bar Color         
            Color[] setPalette = this.ImagePalette;                                             // 적용할 Palette를 읽어온다.
            if (bitmapimageCtl != null)
                setPalette = bitmapimageCtl.GetPalette(this.minWindowLevel, this.maxWindowLevel, this.IsWL_BG,
                                                        nISOLow: this.iIsoLow, nISOHigh: this.iIsoHigh);  // 변경된 palette를 적용한다.
            else
            {
                BitmapImageCtl bitCtl = new BitmapImageCtl();
                bitCtl.SetPalette(this.selectedPaletteType);
                setPalette = bitCtl.GetPalette(this.minWindowLevel, this.maxWindowLevel, this.IsWL_BG,
                                                        nISOLow: this.iIsoLow, nISOHigh: this.iIsoHigh);  // 변경된 palette를 적용한다.
            }

            // Histogram을 표시할 Mat파일 선언(256 x histoMax, 바탕을 White로 구성 )
            OpenCvSharp.Mat drawHist = new OpenCvSharp.Mat(new OpenCvSharp.Size(dimensions[0], histoMax), OpenCvSharp.MatType.CV_8UC3, OpenCvSharp.Scalar.All(255));
            #endregion Histogram에 사용하는 변수 모음

            #region Histogram Data 구하는 부분.
            OpenCvSharp.Cv2.CalcHist(
                images: new[] { this.ToMat() },     // Histogram의 구할 원본 Mat 파일
                channels: new[] { 0 },              // 원본 Mat이 Gray Scale(0~255)이기 때문에 0을 입력
                mask: null,
                hist: histMat,                      // Histogram을 저장할 Mat 파일           
                dims: 1,                            // The histogram dimensionality.
                histSize: dimensions,               // Histogram size for each dimension
                ranges: ranges                      // Histogram min/max값 설정
                );
            #endregion

            #region Histogram Grid Line 그리기
            int Gap = histoMax / 5;

            OpenCvSharp.Point sPoint = new OpenCvSharp.Point(0, Gap);
            OpenCvSharp.Point ePoint = new OpenCvSharp.Point(256, Gap);
            // Draw line ...
            for (int i = 0; i < 4; i++)
            {
                // 점선으로 그린다.
                DrawDashLine(ref drawHist, sPoint, ePoint, OpenCvSharp.Scalar.LightGray);
                sPoint.Y += Gap; ePoint.Y += Gap;
            }

            #endregion Histogram Grid Line 그리기

            #region Histogram의 Min/Max 값을 구해서 drawHist 맟춘다.
            // Histogram의 Min/Max 값을 구한다.
            OpenCvSharp.Cv2.MinMaxLoc(histMat, out double minVal, out double maxVal);
            // drawHist에 그림에 맞춘다.
            if (maxVal > 10000) maxVal = 10000;
            histMat = histMat * (maxVal != 0 ? histoMax / maxVal : 0.0);
            #endregion Histogram의 Min/Max 값을 구해서 drawHist 맟춘다.

            #region Histogram를 drawHist에 그린다.
            // Auto Window Level를 위해 nMinValue/nMaxValue 값을 저장한다.
            for (int nIndex = 0; nIndex < dimensions[0]; ++nIndex)
            {
                // 적용할 Palette값을 Mat에 사용되는 color로 변환
                OpenCvSharp.Scalar color = new OpenCvSharp.Scalar(setPalette[nIndex].B, setPalette[nIndex].G, setPalette[nIndex].R);
                // histMat에서 해당 값을 읽어온다.( histMat은 float 값이므로 Int로 변경해서 사용 )
                int hValue = (int)histMat.Get<float>(nIndex);
                if (nIndex == minLevel || nIndex == maxLevel)    // Window Level위에서 Bar를 표시하기 위해 색을 변경한다.
                {
                    color = new OpenCvSharp.Scalar(gridColor.B, gridColor.G, gridColor.R);  // Window Lever Bar 색 설정
                    hValue = drawHist.Rows;                                                 // Bar를 Heigth위치까지 그리기위해 값 설정
                }
                // nIndex에 해당하는 Palette값을 적용해서 Histogram Line을 그린다.
                drawHist.Line(new OpenCvSharp.Point(nIndex, drawHist.Rows - hValue), new OpenCvSharp.Point(nIndex, drawHist.Rows), color);
            }
            #endregion Histogram를 drawHist에 그린다.

            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(drawHist);
        }

        // Histogram 함수에서 사용 (Mat 파일에 점선 그리기)
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

        #region Option ROI 내부 모든 Sell의 온도값 구하기
        // ROI 내부 모든 Sell의 온도값 구하기
        public byte[,] GetROISharePixel(ROIShape roi)
        {
            byte[,] pixcelBuf = null;

            // 나머지 도형은 점 2개 이상
            if (roi.imagePointInfo.Count < 2) return pixcelBuf;
            // Polygon은 최소 점이 3개 이상
            if (roi.ShapeType == ROISHAPETYPE.Polygon && roi.imagePointInfo.Count < 3) return pixcelBuf;
            // LineX, LineAngle를 계산안한다.
            if (roi.ShapeType == ROISHAPETYPE.LineX || roi.ShapeType == ROISHAPETYPE.LineAngle) return pixcelBuf;

            var minX = roi.imagePointInfo.Min(p => p.X);
            var minY = roi.imagePointInfo.Min(p => p.Y);
            var maxX = roi.imagePointInfo.Max(p => p.X);
            var maxY = roi.imagePointInfo.Max(p => p.Y);

            // ROI를 포함한 최소 영역 가지고 오기
            OpenCvSharp.Rect roiAres = new OpenCvSharp.Rect(new OpenCvSharp.Point(minX, minY), new OpenCvSharp.Size(maxX - minX, maxY - minY));

            // roiAres의 값은 PictureBox의 값이다 그래서 이미지 비율에 맞추기 작업이 필요
            float conLeft = roiAres.Left;
            float conTop = roiAres.Top;
            float conWidth = roiAres.Width;
            float conHeight = roiAres.Height;

            OpenCvSharp.Mat roiMat = this.ToMat();        // 이미지의 저장된 Buffer 처리를위해 Mat 파일로 변경
            // 이미지에서 ROI 영역을 가지고 온다.
            OpenCvSharp.Mat roiImage = roiMat.SubMat(roiAres).Clone();
            // 가지온 온 ROI 영역 이미지에서 실제 ROI영역을 Mask 하기위한 변수작업
            OpenCvSharp.Mat Mask = new OpenCvSharp.Mat(roiImage.Size(), roiImage.Type());
            Mask.SetTo(OpenCvSharp.Scalar.Black);

            // ROI Type에 따라 실제 ROI영역을 Mask 한다.            
            switch (roi.ShapeType)
            {
                case ROISHAPETYPE.Polygon:                                             // Polygon 모양을 White Mask한다.
                    {
                        // ROI의 점을 Mask하기위해 변경한다.(Polygon점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>();

                        foreach (var p in roi.imagePointInfo)
                        {
                            points.Add(new OpenCvSharp.Point((p.X - conLeft), (p.Y - conTop)));
                            points2f.Add(new OpenCvSharp.Point2f(p.X, p.Y));
                        }


                        ListOfListOfPoint.Add(points);

                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);         // 변경된 점으로 Mask영역을 설정한다.
                        roi.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);     // 다각형 넓이
                        //roi.ROI_Area = OpenCvSharp.Cv2.ContourArea(roi.imagePointInfo);     // 다각형 넓이
                        break;
                    }
                case ROISHAPETYPE.Diamond:                                             // 마름모 모양을 White Mask한다.
                    {
                        // ROI의 점을 Mask하기위해 변경한다(마름모 점으로 변경)
                        float radiusX = (float)(conWidth) / 2;
                        float radiusY = (float)(conHeight) / 2;
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>
                        {
                            new OpenCvSharp.Point(radiusX, 0),
                            new OpenCvSharp.Point(conWidth, radiusY),
                            new OpenCvSharp.Point(radiusX, conHeight),
                            new OpenCvSharp.Point(0, radiusY)
                        };
                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);     // 변경된 점으로 Mask영역을 설정한다.

                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>
                        {
                            new OpenCvSharp.Point(radiusX, 0),
                            new OpenCvSharp.Point(conWidth, radiusY),
                            new OpenCvSharp.Point(radiusX, conHeight),
                            new OpenCvSharp.Point(0, radiusY)
                        };

                        roi.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);           // 마름모 넓이
                        break;
                    }
                case ROISHAPETYPE.Ellipse:                                         // 타원 모양으로 Mask하는 부분
                    {
                        // ROI의 점을 Mask하기위해 변경한다.
                        OpenCvSharp.Point2f center = new OpenCvSharp.Point2f((float)conWidth / 2.0f, (float)conHeight / 2.0f);
                        OpenCvSharp.Size2f szie = new OpenCvSharp.Size2f((float)conWidth, (float)conHeight);

                        Mask.Ellipse(new OpenCvSharp.RotatedRect(center, szie, 360), OpenCvSharp.Scalar.White, -1); // 타원 모양을 White Mask한다.
                        roi.ROI_Area = (conWidth / 2.0f) * (conHeight / 2.0f) * (float)Math.PI; // 타원 넓이
                        break;
                    }
                default:                                                            // 함수 앞부분에서 LineX, LineAngle은 제외했기 때문에 이부분은 사각형 모양이다
                    {
                        Mask.SetTo(OpenCvSharp.Scalar.White);                           // ROI 전부를 White Mask한다.
                        roi.ROI_Area = conWidth * conHeight;                            // 사각형의 넓이
                        break;
                    }
            }


            OpenCvSharp.Mat dst = new OpenCvSharp.Mat();
            roiImage.CopyTo(dst, Mask);
            pixcelBuf = new byte[dst.Height, dst.Width];

            dst.GetArray(out byte[] buff);
            Buffer.BlockCopy(buff, 0, pixcelBuf, 0, sizeof(byte) * buff.Length);

            return pixcelBuf;
        }
        #endregion
    }
}
