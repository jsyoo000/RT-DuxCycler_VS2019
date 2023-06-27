using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BitmapControl
{
    // Palette ENUM
    public enum COLOR_TYPE
    {
        COLOR_16, COLOR_32, COLOR_64, COLOR_256, COLOR_GRAY, COLOR_COMPOSITE, COLOR_SEVEN, COLOR_SIXTEEN,
        COLOR_16R, COLOR_32R, COLOR_64R, COLOR_256R, COLOR_GRAYR, COLOR_COMPOSITER, COLOR_SEVENR, COLOR_SIXTEENR,
        //{색상표
        COLOR_JET_16, COLOR_HSV_16, COLOR_HOT_16, COLOR_COOL_16, COLOR_SPRING_16, COLOR_SUMMER_16,
        COLOR_AUTUMN_16, COLOR_WINTER_16, COLOR_GRAY_16, COLOR_BONE_16, COLOR_COPPER_16, COLOR_PINK_16
    };

    public class BitmapImageCtl
    {
        // COLOR_256에서 16,32,64,259 이미지 만들기 위한 ENUM
        public enum COLOR_GAP { COLOR_256 = 1, COLOR_64 = 4, COLOR_32 = 8, COLOR_16 = 16 };

        public Color[] GrayPalette { get { return grayPalette; } set { grayPalette = value; } }
        Color[] grayPalette = new Color[256];           // Low이하, Hight이상의 색을 그래이로 하기 위한 Gray Palette 저장함.
        Color[] SelectedPalette = new Color[256];       // 선택된 Palette를 저장하기 위한 변수
        public Color minColor = Color.Black;            // 제일 낮은 Color <- SelectedPalette[0]
        public Color maxColor = Color.White;            // 제일 높은 Color <- SelectedPalette[255]

        public COLOR_TYPE selectedType = COLOR_TYPE.COLOR_256;    // palette type 선택        

        public Color[] GetPalette(int nLow = 0, int nHigh = 255, bool WL_BG = false, int nISOLow = 0, int nISOHigh = 255)
        {
            Color[] returnPalette = new Color[256];

            if (nISOLow < 0) nISOLow = 0;
            if (nISOHigh > 255) nISOHigh = 255;

            int nRange = nHigh - nLow;
            int nISORange = nISOHigh - nISOLow;
            int tmpLow = nLow;

            if (nISOLow != 0 || nISOHigh != 255)
            {
                tmpLow = nISOLow;
                nRange = Math.Abs(nISOHigh - nISOLow);
                nISORange = nRange;// nHigh - nLow;
            }

            for (int i = 0; i < 256; i++)
            {
                if (i < nISOLow) { returnPalette[i] = Color.Black; continue; }
                else if (i > nISOHigh) { returnPalette[i] = Color.White; continue; }

                if (i <= nLow)
                {
                    if (WL_BG)
                        returnPalette[i] = GrayPalette[i];
                    else if (nISOLow != 0 && i >= nISOLow)
                    {
                        float ffTemp = (float)((i - nISOLow) * 256) / (float)nISORange;
                        int nIndex = Convert.ToInt32(ffTemp);
                        if (nIndex < 0) nIndex = 0;
                        if (nIndex > 255) nIndex = 255;

                        returnPalette[i] = GrayPalette[nIndex];
                    }
                    else
                        returnPalette[i] = SelectedPalette[0];
                }
                else if (i >= nHigh)
                {
                    if (WL_BG)
                        returnPalette[i] = GrayPalette[i];
                    else if (nISOHigh != 255 && i <= nISOHigh)
                    {
                        float ffTemp = (float)((i - nISOLow) * 256) / (float)nISORange;
                        int nIndex = Convert.ToInt32(ffTemp);
                        if (nIndex < 0) nIndex = 0;
                        if (nIndex > 255) nIndex = 255;

                        returnPalette[i] = GrayPalette[nIndex];
                    }
                    else
                        returnPalette[i] = SelectedPalette[255];
                }
                else
                {
                    float ffTemp = (float)((i - tmpLow) * 256) / (float)nRange;
                    int nIndex = Convert.ToInt32(ffTemp);
                    if (nIndex < 0) nIndex = 0;
                    if (nIndex > 255) nIndex = 255;
                    returnPalette[i] = SelectedPalette[nIndex];
                }
            }

            return returnPalette;
        }

        //  GetPaletteBitmap 기준 0 ~ 255
        public Bitmap GetPaletteBitmap(byte[] data, int xResultion, int yResultion, int nIsoLow = 0, int nIsoHigh = 255)
        {
            Bitmap bmp = new Bitmap(xResultion, yResultion, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //Create a BitmapData and Lock all pixels to be written
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                                 new Rectangle(0, 0, bmp.Width, bmp.Height),
                                 System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            //Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            //Unlock the pixels
            bmp.UnlockBits(bmpData);

            // Palette 적용
            System.Drawing.Imaging.ColorPalette Level = bmp.Palette;

            if (nIsoLow < 0) nIsoLow = 0;
            if (nIsoHigh > 255) nIsoHigh = 255;

            for (int i = 0; i < 256; i++)
            {
                if ((i < nIsoLow) || (i > nIsoHigh))
                {
                    Level.Entries[i] = GrayPalette[i];
                }
                else
                {
                    Level.Entries[i] = SelectedPalette[i];
                }
            }
            bmp.Palette = Level;

            return bmp;
        }


        // 특정온도 이상 온도를 특정 색으로 표시, 기본 칼라는 Gray 이다
        public Color[] GetAlarmPalette(double dLowTemp, double dHighTemp, double dAlarmTemp)
        {
            Color[] returnPalette = new Color[256];

            // 알람 온도의 위치 변경에 따라 온도를 표시한다.
            int nAlarm = Convert.ToInt32(((dAlarmTemp - dLowTemp) * 255) / (dHighTemp - dLowTemp));

            for (int i = 0; i < 256; i++)
            {
                if (nAlarm <= i) returnPalette[i] = Color.Red;
                else returnPalette[i] = GrayPalette[i];
            }

            return returnPalette;
        }

        public BitmapImageCtl()
        {
            GrayPalette = GenerateGRAY256Palette();     // Gray 컬러 생성

            SetPalette(COLOR_TYPE.COLOR_256);         // 기본을 COLOR_GRAY한다.
        }

        // Bitmap 이미지에 palette 적용함수, 인덱스이미지만 가능
        public Bitmap SetPaletteBitmap(Bitmap bmp, int nLow, int nHigh, bool WL_BG = false, bool IsHisto = false, int nISOLow = 0, int nISOHigh = 255)
        {
            // Palette 적용
            System.Drawing.Imaging.ColorPalette Level = bmp.Palette;

            if (nISOLow < 0) nISOLow = 0;
            if (nISOHigh > 255) nISOHigh = 255;

            int nRange = nHigh - nLow;
            int nISORange = nISOHigh - nISOLow;
            int tmpLow = nLow;

            if (nISOLow != 0 || nISOHigh != 255)
            {
                tmpLow = nISOLow;
                nRange = Math.Abs(nISOHigh - nISOLow);
                nISORange = nRange;// nHigh - nLow;
            }

            for (int i = 0; i < 256; i++)
            {
                if (i < nISOLow) { Level.Entries[i] = Color.Black; continue; }
                else if (i > nISOHigh) { Level.Entries[i] = Color.White; continue; }

                if (i <= nLow)
                {
                    if (i == 0 && IsHisto)      // Histo에서만 사용함. ( 경계색)
                        Level.Entries[i] = Color.FromArgb(255, 128, 0);
                    else if (nISOLow != 0 && i >= nISOLow)
                    {
                        float ffTemp = (float)((i - nISOLow) * 256) / (float)nISORange;
                        int nIndex = Convert.ToInt32(ffTemp);
                        if (nIndex < 0) nIndex = 0;
                        if (nIndex > 255) nIndex = 255;

                        Level.Entries[i] = GrayPalette[nIndex];
                    }
                    else
                    {
                        if (WL_BG)
                            Level.Entries[i] = GrayPalette[i];
                        else
                            Level.Entries[i] = SelectedPalette[0];
                    }
                }
                else if (i >= nHigh)
                {
                    if (i == 255 && IsHisto)    // Histo에서만 사용함. ( 배경색 )
                        Level.Entries[i] = Color.FromArgb(255, 255, 255);
                    else if (nISOHigh != 255 && i <= nISOHigh)
                    {
                        float ffTemp = (float)((i - nISOLow) * 256) / (float)nISORange;
                        int nIndex = Convert.ToInt32(ffTemp);
                        if (nIndex < 0) nIndex = 0;
                        if (nIndex > 255) nIndex = 255;

                        Level.Entries[i] = GrayPalette[nIndex];
                    }
                    else
                    {
                        if (WL_BG)
                            Level.Entries[i] = GrayPalette[i];
                        else
                            Level.Entries[i] = SelectedPalette[255];
                    }
                }
                else
                {
                    float ffTemp = (float)((i - tmpLow) * 256) / (float)nRange;

                    int nIndex = Convert.ToInt32(ffTemp);
                    if (nIndex < 0) nIndex = 0;
                    if (nIndex > 255) nIndex = 255;
                    Level.Entries[i] = SelectedPalette[nIndex];
                }
            }
            bmp.Palette = Level;
            // Palette 적용
            return bmp;
        }

        // Bitmap 이미지에 특정온도 이상 온도를 특정 색으로 표시, 기본 칼라는 Gray 이다.
        public Bitmap SetPaletteBitmapAlarm(Bitmap bmp, double dLowTemp, double dHighTemp, double dAlarmTemp)
        {
            // Palette 적용
            System.Drawing.Imaging.ColorPalette Level = bmp.Palette;

            // 알람 온도의 위치 변경에 따라 온도를 표시한다.
            int nAlarm = Convert.ToInt32(((dAlarmTemp - dLowTemp) * 255) / (dHighTemp - dLowTemp));

            for (int i = 0; i < 256; i++)
            {
                if (nAlarm <= i) Level.Entries[i] = Color.Red;
                else Level.Entries[i] = GrayPalette[i];
            }
            bmp.Palette = Level;
            // Palette 적용

            return bmp;
        }

        // Byte[]의 이미지 Data를 Bitmap으로 변환 함수
        public Bitmap CopyDataToBitmap(byte[] data, int xResultion, int yResultion, int nLow, int nHigh, bool WL_BG = false, bool IsHisto = false, int nISOLow = 0, int nISOHigh = 255)
        {
            Bitmap bmp = new Bitmap(xResultion, yResultion, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //Create a BitmapData and Lock all pixels to be written
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                                 new Rectangle(0, 0, bmp.Width, bmp.Height),
                                 System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            //Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            //Unlock the pixels
            bmp.UnlockBits(bmpData);

            return SetPaletteBitmap(bmp, nLow, nHigh, WL_BG, IsHisto, nISOLow, nISOHigh);
        }

        // Byte[]의 이미지를 특정온도 이상 온도를 특정 색으로 표시, 기본 칼라는 Gray 이다.
        public Bitmap CopyDataToBitmapAlarm(byte[] data, int xResultion, int yResultion, double dLowTemp, double dHighTemp, double dAlarmTemp)
        {
            Bitmap bmp = new Bitmap(xResultion, yResultion, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //Create a BitmapData and Lock all pixels to be written
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                                 new Rectangle(0, 0, bmp.Width, bmp.Height),
                                 System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            //Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            //Unlock the pixels
            bmp.UnlockBits(bmpData);

            // Palette 적용
            System.Drawing.Imaging.ColorPalette Level = bmp.Palette;

            // 알람 온도의 위치 변경에 따라 온도를 표시한다.
            int nAlarm = Convert.ToInt32(((dAlarmTemp - dLowTemp) * 255) / (dHighTemp - dLowTemp));

            for (int i = 0; i < 256; i++)
            {
                if (nAlarm <= i) Level.Entries[i] = Color.Red;
                else Level.Entries[i] = GrayPalette[i];
            }
            bmp.Palette = Level;
            // Palette 적용

            return bmp;
        }

        #region Palette 선택하기
        // Palette 선택하기 
        public void SetPalette(COLOR_TYPE colorType)
        {
            Color[] colorTemp;
            Array.Clear(SelectedPalette, 0, SelectedPalette.Length);

            this.selectedType = colorType;

            int i;
            switch (colorType)
            {
                case COLOR_TYPE.COLOR_16:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_16, false);
                    break;
                case COLOR_TYPE.COLOR_32:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_32, false);
                    break;
                case COLOR_TYPE.COLOR_64:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_64, false);
                    break;
                case COLOR_TYPE.COLOR_256:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_256, false);
                    break;
                case COLOR_TYPE.COLOR_GRAY:
                    SelectedPalette = GenerateGRAY256Palette();
                    break;
                case COLOR_TYPE.COLOR_COMPOSITE:
                    break;
                case COLOR_TYPE.COLOR_16R:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_16, true);
                    break;
                case COLOR_TYPE.COLOR_32R:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_32, true);
                    break;
                case COLOR_TYPE.COLOR_64R:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_64, true);
                    break;
                case COLOR_TYPE.COLOR_256R:
                    SelectedPalette = GenerateColorPalette(COLOR_GAP.COLOR_256, true); // true -> Reverse
                    break;
                case COLOR_TYPE.COLOR_GRAYR:
                    SelectedPalette = GenerateGRAY256Palette(true); // true -> Reverse
                                                                    //for (i = 0; i < 256; i++)
                                                                    //{
                                                                    //    SelectedPalette[255 - i] = PaletteGrayPattern[i];
                                                                    //}
                    break;
                case COLOR_TYPE.COLOR_COMPOSITER:
                    break;
                case COLOR_TYPE.COLOR_SEVEN:
                    colorTemp = GenerateSevenColorPalette();
                    for (i = 0; i < 256; i++)
                    {
                        SelectedPalette[255 - i] = colorTemp[i];
                    }
                    break;
                case COLOR_TYPE.COLOR_SIXTEEN:
                    colorTemp = GenerateSixteenColorPalette();
                    for (i = 0; i < 256; i++)
                    {
                        SelectedPalette[255 - i] = colorTemp[i];
                    }
                    break;
                case COLOR_TYPE.COLOR_SEVENR:
                    SelectedPalette = GenerateSevenColorPalette();
                    break;
                case COLOR_TYPE.COLOR_SIXTEENR:
                    SelectedPalette = GenerateSixteenColorPalette();
                    break;
                //{색상표
                case COLOR_TYPE.COLOR_JET_16:
                    SelectedPalette = GenerateJET16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_HSV_16:
                    SelectedPalette = GenerateHSV16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_HOT_16:
                    SelectedPalette = GenerateHOT16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_COOL_16:
                    SelectedPalette = GenerateCOOL16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_SPRING_16:
                    SelectedPalette = GenerateSPRING16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_SUMMER_16:
                    SelectedPalette = GenerateSUMMER16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_AUTUMN_16:
                    SelectedPalette = GenerateAUTUMN16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_WINTER_16:
                    SelectedPalette = GenerateWINTER16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_GRAY_16:
                    SelectedPalette = GenerateGRAY16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_BONE_16:
                    SelectedPalette = GenerateBONE16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_COPPER_16:
                    SelectedPalette = GenerateCOPPER16ColorPalette();
                    break;
                case COLOR_TYPE.COLOR_PINK_16:
                    SelectedPalette = GeneratePINK16ColorPalette();
                    break;
                default:
                    break;
            }

            minColor = SelectedPalette[0];
            maxColor = SelectedPalette[255];
        }
        #endregion

        #region COLOR_256, COLOR_256R Palette 만들기
        // COLOR_256, COLOR_256R
        public Color[] GenerateColorPalette(COLOR_GAP eGap = COLOR_GAP.COLOR_256, bool isReverse = false)
        {
            Color[] ColorPattern = new Color[256];

            int nGap = (int)eGap;
            float fTemp;
            int nGapCount = 0;
            Byte red = 0, green = 0, blue = 0;

            float fTr1 = (float)160, fPhaseR1 = (float)40;
            float fTr2 = (float)300, fPhaseR2 = (float)200;
            float fTg = (float)300, fPhaseG = (float)158;
            float fTb = (float)280, fPhaseB = (float)70;

            for (int i = 0; i < 256; i++)
            {
                nGapCount++;
                if (i == 0 || nGapCount >= nGap)
                {
                    // Red 1
                    if (i < (int)fPhaseR1 * 2)
                        fTemp = (float)128 * (float)Math.Sin((float)Math.PI * (float)2 / fTr1 * ((float)i - fPhaseR1 + fTr1 / (float)4));
                    else if ((int)(fPhaseR2 - fTr2 / (float)4) < i)
                        fTemp = (float)255 * (float)Math.Sin((float)Math.PI * (float)2 / fTr2 * ((float)i - fPhaseR2 + fTr2 / (float)4));
                    else
                        fTemp = (float)0;
                    if (fTemp < (float)0)
                        fTemp = (float)0;
                    red = (Byte)fTemp;

                    // Green
                    if (((int)(fPhaseG - fTg / (float)4) < i) || (i < (int)(fPhaseG + fTg / (float)4)))
                        fTemp = (float)255 * (float)Math.Sin((float)Math.PI * (float)2 / fTg * ((float)i - fPhaseG + fTg / (float)4));
                    else
                        fTemp = (float)0;
                    if (fTemp < (float)0)
                        fTemp = (float)0;
                    green = (Byte)fTemp;

                    // Blue
                    if (i < (int)fPhaseB * 2)
                        fTemp = (float)255 * (float)Math.Sin((float)Math.PI * (float)2 / fTb * ((float)i - fPhaseB + fTb / (float)4));
                    else
                        fTemp = (float)0;
                    if (fTemp < (float)0)
                        fTemp = (float)0;
                    blue = (Byte)fTemp;
                    nGapCount = 0;
                }

                int nIndex = i;
                if (isReverse) { nIndex = 255 - i; }


                ColorPattern[nIndex] = Color.FromArgb(red, green, blue);
            }
            return ColorPattern;
        }
        #endregion

        #region COLOR_GRAY, COLOR_GRAYR Palette 만들기
        // COLOR_GRAY, COLOR_GRAYR
        public Color[] GenerateGRAY256Palette(bool isReverse = false)
        {
            Color[] ColorPattern = new Color[256];

            for (int i = 0; i < 256; i++)
            {
                int nIndex = i;
                if (isReverse) { nIndex = 255 - i; }

                ColorPattern[nIndex] = Color.FromArgb(i, i, i);
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_SEVENR Palette 만들기
        // COLOR_SEVENR, COLOR_SIXTEENR
        public Color[] GenerateSevenColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            ////////////////////////////////////////////////////////////////////
            ///			Seven Color Mapping									  // 
            int t;
            for (int k = 0; k < 8; k++)
            {
                switch (k)
                {
                    case 7: { red = 0x7c; green = 0x7c; blue = 0x7c; break; }// Black
                    case 6: { red = 0xff; green = 0x00; blue = 0xff; break; }// Violet
                    case 5: { red = 0x00; green = 0x00; blue = 0xff; break; }// Blue
                    case 4: { red = 0x00; green = 0xff; blue = 0x00; break; }// Green
                    case 3: { red = 0xff; green = 0xff; blue = 0x00; break; }// Yellow
                    case 2: { red = 0xff; green = 0x80; blue = 0x00; break; }// Orange
                    case 1: { red = 0xff; green = 0x00; blue = 0x00; break; }// Red
                    default: { red = 0xff; green = 0xff; blue = 0xff; break; }// White
                }

                for (t = 0; t < 32; t++)
                {
                    ColorPattern[t + k * 32] = Color.FromArgb(red, green, blue);

                    if (red != 0)
                        red -= 4;
                    if (green != 0)
                        green -= 4;
                    if (blue != 0)
                        blue -= 4;
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_SIXTEEN Palette 만들기
        // COLOR_SIXTEEN, COLOR_SIXTEENR
        public Color[] GenerateSixteenColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            /////////////////////////////////////////////////////////////////////
            ///		Sixteen Color
            int n;
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 15: { red = 0; green = 0; blue = 0; break; }    // 검정색
                    case 14: { red = 0x00; green = 0x00; blue = 0x80; break; }    // 진한남색
                    case 13: { red = 0x00; green = 0x00; blue = 0xc0; break; }    // 남색
                    case 12: { red = 0x00; green = 0x00; blue = 0xff; break; }    // 파랑
                    case 11: { red = 0x00; green = 0x64; blue = 0xc8; break; }    // 연파랑
                    case 10: { red = 0x0a; green = 0x80; blue = 0xc0; break; }    // 연연파랑 
                    case 9: { red = 0x32; green = 0x80; blue = 0x32; break; }    // 진한초록
                    case 8: { red = 0x00; green = 0xff; blue = 0x00; break; }    // 초록
                    case 7: { red = 0x80; green = 0xff; blue = 0x00; break; }    // 연초록
                    case 6: { red = 0xff; green = 0xff; blue = 0x00; break; }    // 노랑 
                    case 5: { red = 0xff; green = 0xdf; blue = 0x00; break; }    // 진한 노랑
                    case 4: { red = 0xff; green = 0x80; blue = 0x00; break; }    // 주황
                    case 3: { red = 0xff; green = 0x40; blue = 0x00; break; }    // 연한 빨강
                    case 2: { red = 0xff; green = 0x00; blue = 0x00; break; }    // 빨강
                    case 1: { red = 0xff; green = 0x00; blue = 0xFF; break; }    // 보라
                    default: { red = 0xff; green = 0xc0; blue = 0xff; break; }    // 연보라
                }
                for (n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                    if (red != 0)
                        red -= 2;
                    if (green != 0)
                        green -= 2;
                    if (blue != 0)
                        blue -= 2;
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_JET_16 Palette 만들기
        //COLOR_JET_16
        public Color[] GenerateJET16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //JET
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 0; green = 0; blue = 191; break; }
                    case 1: { red = 0; green = 0; blue = 255; break; }
                    case 2: { red = 0; green = 65; blue = 255; break; }
                    case 3: { red = 0; green = 127; blue = 255; break; }
                    case 4: { red = 0; green = 191; blue = 255; break; }
                    case 5: { red = 0; green = 255; blue = 255; break; }
                    case 6: { red = 63; green = 255; blue = 191; break; }
                    case 7: { red = 127; green = 255; blue = 127; break; }
                    case 8: { red = 191; green = 255; blue = 63; break; }
                    case 9: { red = 255; green = 255; blue = 0; break; }
                    case 10: { red = 255; green = 191; blue = 0; break; }
                    case 11: { red = 255; green = 127; blue = 0; break; }
                    case 12: { red = 255; green = 63; blue = 0; break; }
                    case 13: { red = 255; green = 0; blue = 0; break; }
                    case 14: { red = 191; green = 0; blue = 0; break; }
                    default: { red = 127; green = 0; blue = 0; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_HSV_16 Palette 만들기
        //COLOR_HSV_16
        public Color[] GenerateHSV16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //HSV
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 255; green = 0; blue = 0; break; }
                    case 1: { red = 255; green = 95; blue = 0; break; }
                    case 2: { red = 255; green = 191; blue = 0; break; }
                    case 3: { red = 223; green = 255; blue = 0; break; }
                    case 4: { red = 127; green = 255; blue = 0; break; }
                    case 5: { red = 31; green = 255; blue = 0; break; }
                    case 6: { red = 0; green = 255; blue = 63; break; }
                    case 7: { red = 0; green = 255; blue = 159; break; }
                    case 8: { red = 0; green = 255; blue = 255; break; }
                    case 9: { red = 0; green = 159; blue = 255; break; }
                    case 10: { red = 0; green = 63; blue = 255; break; }
                    case 11: { red = 31; green = 0; blue = 255; break; }
                    case 12: { red = 127; green = 0; blue = 255; break; }
                    case 13: { red = 223; green = 0; blue = 255; break; }
                    case 14: { red = 255; green = 0; blue = 191; break; }
                    default: { red = 255; green = 0; blue = 95; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_HOT_16 Palette 만들기
        //COLOR_HOT_16
        public Color[] GenerateHOT16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //HOT
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 42; green = 0; blue = 0; break; }
                    case 1: { red = 85; green = 0; blue = 0; break; }
                    case 2: { red = 127; green = 0; blue = 0; break; }
                    case 3: { red = 170; green = 0; blue = 0; break; }
                    case 4: { red = 213; green = 0; blue = 0; break; }
                    case 5: { red = 255; green = 0; blue = 0; break; }
                    case 6: { red = 255; green = 42; blue = 0; break; }
                    case 7: { red = 255; green = 85; blue = 0; break; }
                    case 8: { red = 255; green = 127; blue = 0; break; }
                    case 9: { red = 255; green = 170; blue = 0; break; }
                    case 10: { red = 255; green = 213; blue = 0; break; }
                    case 11: { red = 255; green = 255; blue = 0; break; }
                    case 12: { red = 255; green = 255; blue = 63; break; }
                    case 13: { red = 255; green = 255; blue = 127; break; }
                    case 14: { red = 255; green = 255; blue = 191; break; }
                    default: { red = 255; green = 255; blue = 255; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_COOL_16 Palette 만들기
        //COLOR_COOL_16
        public Color[] GenerateCOOL16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //COOL
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 0; green = 255; blue = 255; break; }
                    case 1: { red = 17; green = 238; blue = 255; break; }
                    case 2: { red = 34; green = 221; blue = 255; break; }
                    case 3: { red = 51; green = 204; blue = 255; break; }
                    case 4: { red = 68; green = 187; blue = 255; break; }
                    case 5: { red = 85; green = 170; blue = 255; break; }
                    case 6: { red = 102; green = 153; blue = 255; break; }
                    case 7: { red = 119; green = 136; blue = 255; break; }
                    case 8: { red = 136; green = 119; blue = 255; break; }
                    case 9: { red = 153; green = 102; blue = 255; break; }
                    case 10: { red = 170; green = 85; blue = 255; break; }
                    case 11: { red = 187; green = 68; blue = 255; break; }
                    case 12: { red = 204; green = 51; blue = 255; break; }
                    case 13: { red = 221; green = 34; blue = 255; break; }
                    case 14: { red = 238; green = 17; blue = 255; break; }
                    default: { red = 255; green = 0; blue = 255; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_SPRING_16 Palette 만들기
        //COLOR_SPRING_16
        public Color[] GenerateSPRING16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //SPRING
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 255; green = 0; blue = 255; break; }
                    case 1: { red = 255; green = 17; blue = 238; break; }
                    case 2: { red = 255; green = 34; blue = 221; break; }
                    case 3: { red = 255; green = 51; blue = 204; break; }
                    case 4: { red = 255; green = 68; blue = 187; break; }
                    case 5: { red = 255; green = 85; blue = 170; break; }
                    case 6: { red = 255; green = 102; blue = 153; break; }
                    case 7: { red = 255; green = 119; blue = 136; break; }
                    case 8: { red = 255; green = 136; blue = 119; break; }
                    case 9: { red = 255; green = 153; blue = 102; break; }
                    case 10: { red = 255; green = 170; blue = 85; break; }
                    case 11: { red = 255; green = 187; blue = 68; break; }
                    case 12: { red = 255; green = 204; blue = 51; break; }
                    case 13: { red = 255; green = 221; blue = 34; break; }
                    case 14: { red = 255; green = 238; blue = 17; break; }
                    default: { red = 255; green = 255; blue = 0; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_SUMMER_16 Palette 만들기
        //COLOR_SUMMER_16
        public Color[] GenerateSUMMER16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //SUMMER
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 0; green = 127; blue = 102; break; }
                    case 1: { red = 17; green = 136; blue = 102; break; }
                    case 2: { red = 34; green = 145; blue = 102; break; }
                    case 3: { red = 51; green = 153; blue = 102; break; }
                    case 4: { red = 68; green = 162; blue = 102; break; }
                    case 5: { red = 85; green = 170; blue = 102; break; }
                    case 6: { red = 102; green = 179; blue = 102; break; }
                    case 7: { red = 119; green = 187; blue = 102; break; }
                    case 8: { red = 136; green = 196; blue = 102; break; }
                    case 9: { red = 153; green = 204; blue = 102; break; }
                    case 10: { red = 170; green = 213; blue = 102; break; }
                    case 11: { red = 187; green = 221; blue = 102; break; }
                    case 12: { red = 204; green = 230; blue = 102; break; }
                    case 13: { red = 221; green = 238; blue = 102; break; }
                    case 14: { red = 238; green = 247; blue = 102; break; }
                    default: { red = 255; green = 255; blue = 102; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_AUTUMN_16 Palette 만들기
        //COLOR_AUTUMN_16
        public Color[] GenerateAUTUMN16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //AUTUMN
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 255; green = 0; blue = 0; break; }
                    case 1: { red = 255; green = 17; blue = 0; break; }
                    case 2: { red = 255; green = 34; blue = 0; break; }
                    case 3: { red = 255; green = 51; blue = 0; break; }
                    case 4: { red = 255; green = 68; blue = 0; break; }
                    case 5: { red = 255; green = 85; blue = 0; break; }
                    case 6: { red = 255; green = 102; blue = 0; break; }
                    case 7: { red = 255; green = 119; blue = 0; break; }
                    case 8: { red = 255; green = 136; blue = 0; break; }
                    case 9: { red = 255; green = 153; blue = 0; break; }
                    case 10: { red = 255; green = 170; blue = 0; break; }
                    case 11: { red = 255; green = 187; blue = 0; break; }
                    case 12: { red = 255; green = 204; blue = 0; break; }
                    case 13: { red = 255; green = 221; blue = 0; break; }
                    case 14: { red = 255; green = 238; blue = 0; break; }
                    default: { red = 255; green = 255; blue = 0; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_WINTER_16 Palette 만들기
        //COLOR_WINTER_16
        public Color[] GenerateWINTER16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //WINTER
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 0; green = 0; blue = 255; break; }
                    case 1: { red = 0; green = 17; blue = 247; break; }
                    case 2: { red = 0; green = 34; blue = 238; break; }
                    case 3: { red = 0; green = 51; blue = 230; break; }
                    case 4: { red = 0; green = 68; blue = 221; break; }
                    case 5: { red = 0; green = 85; blue = 213; break; }
                    case 6: { red = 0; green = 102; blue = 204; break; }
                    case 7: { red = 0; green = 119; blue = 196; break; }
                    case 8: { red = 0; green = 136; blue = 187; break; }
                    case 9: { red = 0; green = 153; blue = 179; break; }
                    case 10: { red = 0; green = 170; blue = 170; break; }
                    case 11: { red = 0; green = 187; blue = 162; break; }
                    case 12: { red = 0; green = 204; blue = 153; break; }
                    case 13: { red = 0; green = 221; blue = 145; break; }
                    case 14: { red = 0; green = 238; blue = 136; break; }
                    default: { red = 0; green = 255; blue = 127; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_GRAY_16 Palette 만들기
        //COLOR_GRAY_16
        public Color[] GenerateGRAY16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //GRAY
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 0; green = 0; blue = 0; break; }
                    case 1: { red = 17; green = 17; blue = 17; break; }
                    case 2: { red = 34; green = 34; blue = 34; break; }
                    case 3: { red = 51; green = 51; blue = 51; break; }
                    case 4: { red = 68; green = 68; blue = 68; break; }
                    case 5: { red = 85; green = 85; blue = 85; break; }
                    case 6: { red = 102; green = 102; blue = 102; break; }
                    case 7: { red = 119; green = 119; blue = 119; break; }
                    case 8: { red = 136; green = 136; blue = 136; break; }
                    case 9: { red = 153; green = 153; blue = 153; break; }
                    case 10: { red = 170; green = 170; blue = 170; break; }
                    case 11: { red = 187; green = 187; blue = 187; break; }
                    case 12: { red = 204; green = 204; blue = 204; break; }
                    case 13: { red = 221; green = 221; blue = 221; break; }
                    case 14: { red = 238; green = 238; blue = 238; break; }
                    default: { red = 255; green = 255; blue = 255; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }
            return ColorPattern;
        }
        #endregion

        #region COLOR_BONE_16 Palette 만들기
        //COLOR_BONE_16
        public Color[] GenerateBONE16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //BONE
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 0; green = 0; blue = 5; break; }
                    case 1: { red = 14; green = 14; blue = 25; break; }
                    case 2: { red = 29; green = 29; blue = 45; break; }
                    case 3: { red = 44; green = 44; blue = 66; break; }
                    case 4: { red = 59; green = 59; blue = 86; break; }
                    case 5: { red = 74; green = 74; blue = 106; break; }
                    case 6: { red = 89; green = 94; blue = 121; break; }
                    case 7: { red = 104; green = 115; blue = 136; break; }
                    case 8: { red = 119; green = 135; blue = 151; break; }
                    case 9: { red = 134; green = 155; blue = 166; break; }
                    case 10: { red = 149; green = 175; blue = 181; break; }
                    case 11: { red = 164; green = 196; blue = 196; break; }
                    case 12: { red = 187; green = 211; blue = 211; break; }
                    case 13: { red = 210; green = 226; blue = 226; break; }
                    case 14: { red = 233; green = 241; blue = 241; break; }
                    default: { red = 255; green = 255; blue = 255; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_COPPER_16 Palette 만들기
        //COLOR_COPPER_16
        public Color[] GenerateCOPPER16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //COPPER
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 0; green = 0; blue = 0; break; }
                    case 1: { red = 21; green = 13; blue = 8; break; }
                    case 2: { red = 42; green = 26; blue = 16; break; }
                    case 3: { red = 63; green = 39; blue = 25; break; }
                    case 4: { red = 85; green = 53; blue = 33; break; }
                    case 5: { red = 106; green = 66; blue = 42; break; }
                    case 6: { red = 127; green = 79; blue = 50; break; }
                    case 7: { red = 149; green = 93; blue = 59; break; }
                    case 8: { red = 170; green = 106; blue = 67; break; }
                    case 9: { red = 191; green = 119; blue = 76; break; }
                    case 10: { red = 213; green = 133; blue = 84; break; }
                    case 11: { red = 234; green = 146; blue = 93; break; }
                    case 12: { red = 255; green = 159; blue = 101; break; }
                    case 13: { red = 255; green = 173; blue = 110; break; }
                    case 14: { red = 255; green = 186; blue = 118; break; }
                    default: { red = 255; green = 199; blue = 127; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region COLOR_PINK_16 Palette 만들기
        //COLOR_PINK_16
        public Color[] GeneratePINK16ColorPalette()
        {
            Color[] ColorPattern = new Color[256];

            Byte red, green, blue;
            //PINK
            for (int m = 0; m < 16; m++)
            {
                switch (m)
                {
                    case 0: { red = 60; green = 0; blue = 0; break; }
                    case 1: { red = 100; green = 53; blue = 53; break; }
                    case 2: { red = 129; green = 76; blue = 76; break; }
                    case 3: { red = 152; green = 93; blue = 93; break; }
                    case 4: { red = 172; green = 107; blue = 107; break; }
                    case 5: { red = 190; green = 120; blue = 120; break; }
                    case 6: { red = 198; green = 145; blue = 132; break; }
                    case 7: { red = 205; green = 166; blue = 142; break; }
                    case 8: { red = 212; green = 184; blue = 152; break; }
                    case 9: { red = 219; green = 201; blue = 161; break; }
                    case 10: { red = 225; green = 217; blue = 170; break; }
                    case 11: { red = 232; green = 232; blue = 178; break; }
                    case 12: { red = 238; green = 238; blue = 201; break; }
                    case 13: { red = 244; green = 244; blue = 220; break; }
                    case 14: { red = 250; green = 250; blue = 239; break; }
                    default: { red = 255; green = 255; blue = 255; break; }
                }
                for (int n = 0; n < 16; n++)
                {
                    ColorPattern[n + m * 16] = Color.FromArgb(red, green, blue);
                }
            }

            return ColorPattern;
        }
        #endregion

        #region 영상을 오른쪽으로 돌리기
        // 영상을 오른쪽으로 돌리기
        public static Byte[,] RotateRight(Byte[,] matrix)
        {
            int lengthY = matrix.GetLength(0);
            int lengthX = matrix.GetLength(1);
            Byte[,] result = new Byte[lengthX, lengthY];

            for (int y = 0; y < lengthY; y++)
                for (int x = 0; x < lengthX; x++)
                    result[x, y] = matrix[lengthY - 1 - y, x];
            return result;
        }
        #endregion

        #region 영상을 왼쪽으로 돌리기
        // 영상을 왼쪽으로 돌리기
        public static Byte[,] RotateLeft(Byte[,] matrix)
        {
            int lengthY = matrix.GetLength(0);
            int lengthX = matrix.GetLength(1);
            Byte[,] result = new Byte[lengthX, lengthY];
            for (int y = 0; y < lengthY; y++)
                for (int x = 0; x < lengthX; x++)
                    result[x, y] = matrix[y, lengthX - 1 - x];
            return result;
        }
        #endregion
    }

    public static class GraphicsExtensions
    {
        // 두점 사이의 원 그리기
        public static void DrawCircle(this Graphics g, Pen pen, Point sPoint, Point ePoint)
        {
            float radiusX = (float)(ePoint.X - sPoint.X) / 2;
            float radiusY = (float)(ePoint.Y - sPoint.Y) / 2;
            float radius = radiusX;
            if (System.Math.Abs(radiusY) < System.Math.Abs(radiusX)) radius = radiusY;
            float centerX = (float)sPoint.X + radiusX;
            float centerY = (float)sPoint.Y + radiusY;

            g.DrawEllipse(pen, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        // 두점 사이의 채워진 원 그리기
        public static void FillCircle(this Graphics g, Pen pen, Point sPoint, Point ePoint)
        {
            float radiusX = (float)(ePoint.X - sPoint.X) / 2;
            float radiusY = (float)(ePoint.Y - sPoint.Y) / 2;
            float radius = radiusX;
            if (System.Math.Abs(radiusY) < System.Math.Abs(radiusX)) radius = radiusY;
            float centerX = (float)sPoint.X + radiusX;
            float centerY = (float)sPoint.Y + radiusY;

            g.DrawEllipse(pen, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        // 두점 사이의 Rectangle 그리기
        public static void DrawRectangle(this Graphics g, Pen pen, Point sPoint, Point ePoint)
        {
            int width = ePoint.X - sPoint.X;
            int height = ePoint.Y - sPoint.Y;

            Point sRectPoint = sPoint;
            if (width < 0)
            {
                width = System.Math.Abs(width);
                sRectPoint.X = ePoint.X;
            }

            if (height < 0)
            {
                height = System.Math.Abs(height);
                sRectPoint.Y = ePoint.Y;
            }

            g.DrawRectangle(Pens.Blue, new Rectangle(sRectPoint, new Size(width, height)));
        }

        // 두점 사이의 마름모 그리기
        public static void DrawRhombus(this Graphics g, Pen pen, Point sPoint, Point ePoint)
        {
            float radiusX = (float)(ePoint.X - sPoint.X) / 2;
            float radiusY = (float)(ePoint.Y - sPoint.Y) / 2;
            float centerX = (float)sPoint.X + radiusX;
            float centerY = (float)sPoint.Y + radiusY;

            Point[] polyPoints = { new Point((int)centerX, sPoint.Y), new Point(ePoint.X, (int)centerY), new Point((int)centerX, ePoint.Y), new Point(sPoint.X, (int)centerY) };
            g.DrawPolygon(Pens.Green, polyPoints);
        }

    }
}
