using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Duxcycler_IMAGE
{
    /** @brief ROI 도형 ENUM */
    public enum ROISHAPETYPE
    {
        Rectangle,          // 사각형     ( 두점으로 구성 )
        Diamond,            // 다이아몬드 ( 두점으로 구성 )
        Ellipse,            // 타원       ( 두점으로 구성 )
        Polygon,            // 다각형     ( n개의 점으로 구성( 3개이상) )
        LineAngle,          // 각도 Line  (3 점으로 구성)
        LineX               // x자 Line   ( 4 점으로 구성
    };

    /** ROI 선택 위치 ENUM */
    public enum ROISHAPEPOSTION
    {
        // Rectangle, Rhombus, Ellipse 인경우에 해당함. 
        AreaLeftMiddle,         // Left Line 선택
        AreaTopMiddle,          // Top Line 선택        
        AreaRightMiddle,        // Right Line 선택
        AreaBottomMiddle,       // Bottom Line 선택

        AreaLeftTop,            // (Left, Top) 점 선택
        AreaRightTop,           // (Right, Top) 점 선택
        AreaLeftBottom,         // (Left, Bottom) 점 선택
        AreaRightBottom,        // (Right, Bottom) 점 선택

        // Polygon, LineAngle, LineX 인 경우에 해당함.
        HitPoint,               // 해당 점이동

        // 모두 적용
        Inside,                 // 도형 이동 선택
        Rotation,               // 회전 선택

        None                    // 선택 없음
    };

    /** @class ROI 도형 클래스 */
    public class ROIShape
    {
    #region 클래스 내부 변수

        /** ROI 이동할 점의 Index */
        private int movePointIndex = -1;

        /** 점의 크기 */
        private const int object_radius = 3;

        /** 거리를 측정하는 기준값 */
        private const int over_dist_squared = object_radius * object_radius;

        /** 입력 시 Y축에 가장 작은 값을 구하기 변수( Polygon RotatedPoint를 구하기 위해 ) */
        private PointF yMinPoint = new PointF();

        /** 입력 시 Y축이 가장 작은 값을 저장한다. ( Polygon RotatedPoint를 구하기 위해 ) */
        private int yMinPointIndex = 0;

        /** Polygon의 회전 각을 저장한다.*/
        private float polygonAngle = 0;

        /** Polygon의 중심점*/
        private PointF polygonCenter = new PointF();

        /** PictureBox or 파일에 표시하기 위한 Point List ( Polygon, LineAngle, LineX ) */
        private List<Point> PointsInfo = new List<Point>();

        /** PictureBox or 파일에 표시하기 위한 rotatedDrawArea ( Rectangle, Diamond, Ellipse) */
        private OpenCvSharp.RotatedRect rotatedDrawArea = new OpenCvSharp.RotatedRect();

        /** PictureBox or 파일에 표시하기 위한 Font Size 변경을 위해*/
        private Font DrawFont = SystemFonts.DefaultFont;
    #endregion 클래스 내부 변수

    #region 클래스 외부 설정 변수
        /** 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우에 사용, 회전을 적용하기 위한 변수 */
        public OpenCvSharp.RotatedRect rotatedArea = new OpenCvSharp.RotatedRect();

        /** ROI 도형의 Point 정보 ( Image Size 기준 점 )     */
        public List<PointF> imagePointInfo = new List<PointF>();

        /**  해당 ROI가 포함된 ChartNo */
        public string ChartNo { get { return _chartno; } set { _chartno = value; } }                 // Char NO
        private string _chartno = "";

        /**  해당 ROI가 포함된 Study ID */
        public int StudyID { get { return _studyid; } set { _studyid = value; } }
        private int _studyid = -1;

        /**  해당 ROI가 포함되 Image Index */
        public int ImageIndex { get { return _imageindex; } set { _imageindex = value; } }
        private int _imageindex = -1;

        /**  해당 ROI가 포함되 Image의 ROI Index */
        public int ROIID { get { return _roiid; } set { _roiid = value; } }
        private int _roiid = -1;

        /**  해당 ROI와 연결될 ROI가 포함된 Chart No */
        public string Connect_ChartNo { get { return _connect_chartno; } set { _connect_chartno = value; } }                 // Char NO
        private string _connect_chartno = "";

        /**  해당 ROI와 연결될 ROI가 포함된 study index */
        public int Connect_StudyID { get { return _connect_studyid; } set { _connect_studyid = value; } }
        private int _connect_studyid = -1;

        /**  해당 ROI와 연결될 ROI가 포함되 Image Index */
        public int Connect_ImageIndex { get { return _connect_imageindex; } set { _connect_imageindex = value; } }
        private int _connect_imageindex = -1;

        /**  해당 ROI와 연결될 ROI가 포함되 Image의 ROI Index */
        public int Connect_ROIID { get { return _connect_roiid; } set { _connect_roiid = value; } }
        private int _connect_roiid = -1;

        /**  ROI Index */
        public int ROI_MainIndex { get { return _roi_mainindex; } set { _roi_mainindex = value; } }
        private int _roi_mainindex = -1;

        /**  ROI Index */
        public int ROI_SubIndex { get { return _roi_subindex; } set { _roi_subindex = value; } }
        private int _roi_subindex = -1;

        /** 도형의 타입 */
        public ROISHAPETYPE ShapeType { get { return _shapeType; } set { _shapeType = value; } }
        private ROISHAPETYPE _shapeType = ROISHAPETYPE.Rectangle;

        /**  ROI Node 선택변수 */
        public ROISHAPEPOSTION NodeSelected { get { return this._nodeSelected; } set { this._nodeSelected = value; } }
        private ROISHAPEPOSTION _nodeSelected = ROISHAPEPOSTION.None;

        /**  ROI Guide Point 색 */
        public Color GuidePointColor { get { return this._guidepointcolor; } set { this._guidepointcolor = value; } }
        private Color _guidepointcolor = Color.Red;

        /**  ROI Guide Line 색 */
        public Color GuideLineColor { get { return this._guidelinecolor; } set { this._guidelinecolor = value; } }
        private Color _guidelinecolor = Color.LightGray;

        /**  ROI 선택 테두리 색 */
        public Color SelectedBorderColor { get { return this._selectedbordercolor; } set { this._selectedbordercolor = value; } }
        private Color _selectedbordercolor = Color.Red;

        /**  ROI의 내부 색    */
        public Color FilledColor { get { return this._filledcolor; } set { this._filledcolor = value; } }
        private Color _filledcolor = Color.Black;

        /**  ROI 테두리 색 */
        public Color BorderColor { get { return this._bordercolor; } set { this._bordercolor = value; } }
        private Color _bordercolor = Color.White;

        /**  ROI 테두리 배경색 */
        public Color BorderBackColor { get { return this._borderbackcolor; } set { this._borderbackcolor = value; } }
        private Color _borderbackcolor = Color.Black;

        /**  ROI 테두리 두께 */
        public int BorderWidth { get { return this._borderwidth; } set { this._borderwidth = value; if (this._borderwidth < 1) this._borderwidth = 1; } }
        private int _borderwidth = 1;

        /**  ROI 선택 여부 */
        public bool Selected { get { return this._selected; } set { this._selected = value; } }
        private bool _selected = false;

        /**  ROI Index 표시 Font */
        public Font Font { get { return this._font; } set { this._font = value; } }
        private Font _font = SystemFonts.DefaultFont;

        /** ROI Index 표시 글자 색 */
        public Color ForeColor { get { return this._forecolor; } set { this._forecolor = value; } }
        private Color _forecolor = Color.White;

        /**   ROI Index 표시 글자 배경색 */
        public Color ForeBackColor { get { return this._forebackcolor; } set { this._forebackcolor = value; } }
        private Color _forebackcolor = Color.Black;

    #endregion 클래스 외부 설정 변수

    #region IRIS_BT_9000에서 사용하는 내용 : ImageInfo Class에서 필요한 내용
        /**  ROI의 기준 Image의 폭 */
        public double Image_Width { get { return _image_width; } set { _image_width = value; } }
        private double _image_width = -1;

        /**  ROI의 기준 Image의 높이 */
        public double Image_Height { get { return _image_height; } set { _image_height = value; } }
        private double _image_height = -1;

        /**  ROI내의 Min 값 */
        public double ROI_MinValue { get { return _roi_minvalue; } set { _roi_minvalue = value; } }
        private double _roi_minvalue = -1;

        /**  ROI내의 Max 값 */
        public double ROI_MaxValue { get { return _roi_maxvalue; } set { _roi_maxvalue = value; } }
        private double _roi_maxvalue = -1;

        /**  ROI내의 평균값 값 */
        public double ROI_Average { get { return _roi_average; } set { _roi_average = value; } }
        private double _roi_average = -1;

        /**  ROI내의 표준편차 값 */
        public double ROI_Sdnn { get { return _roi_sdnn; } set { _roi_sdnn = value; } }
        private double _roi_sdnn = -1;

        /**  해당 ROI와 연결된 ROI의 평균값의 차이( Sub Index가 2인경우에만 값이 있다.) */
        public double ROI_Diff { get { return _roi_diff; } set { _roi_diff = value; } }
        private double _roi_diff = 0;

        /**  ROI를 포함하는 Area 폭(Image 기준 폭이다. ) */
        public double ROI_Width { get { return _roi_width; } set { _roi_width = value; } }
        private double _roi_width = -1;

        /**  ROI를 포함하는 Area 높이(Image 기준 높이이다. ) */
        public double ROI_Height { get { return _roi_height; } set { _roi_height = value; } }
        private double _roi_height = -1;

        /**  ROI의 실제 면적(Image 기준 면적이다. ) */
        public double ROI_Area { get { return _roi_area; } set { _roi_area = value; } }
        private double _roi_area = -1;

        /**  ROI 내의 RGB 값을 계산할때 사용하는 Offset 값 */
        public double ROI_Offset { get { return _roi_offset; } set { _roi_offset = value; } }
        private double _roi_offset = 0.0;

        /**  ROI 내의 RGB 값을 계산할때 사용하는 Gain 값 */
        public double ROI_Gain { get { return _roi_gain; } set { _roi_gain = value; } }
        private double _roi_gain = 1.0;

        #endregion IRIS_BT_9000에서 사용하는 내용 : ImageInfo Class에서 필요한 내용

        #region 생성자, CopyTo, Clone, AddImagePoint, Save, Load  

        /** @brief: ROI 도형의 생성자
         *  @section: Description
         *      ROI 도형 Clase를 생성한다. 기본값은 Rectangle이다.
         *  @param:     type       ROI 도형의 모양(기본값은 Rectangle이다.)
         */
        public ROIShape(ROISHAPETYPE type = ROISHAPETYPE.Rectangle)
        {
            this.imagePointInfo.Clear();
            this.ShapeType = type;
        }

        /** @brief: ROI 도형을 복사( 정보를 그대로 복사 )
         *  @section: Description
         *      ROI 도형을 그대로 복사한다.         
         */
        public ROIShape CopyTo()
        {

            ROIShape copy = this.Clone();
            copy.ROI_MainIndex = this.ROI_MainIndex;
            copy.ROI_SubIndex = this.ROI_SubIndex;

            return copy;
        }

        /** @brief: ROI 도형의 Clone 생성( 원본의 모양과 point 정보만 입력)
         *  @section: Description
         *      ROI 도형의 원본의 모양과 point 정보만 입력
         */
        public ROIShape Clone()
        {
            ROIShape clone = new ROIShape(this.ShapeType)
            {
                // ROI Shape의 기본정보
                ChartNo = this.ChartNo,
                StudyID = this.StudyID,
                ImageIndex = this.ImageIndex,
                ROIID = this.ROIID,
                Connect_ChartNo = this.Connect_ChartNo,
                Connect_StudyID = this.Connect_StudyID,
                Connect_ImageIndex = this.Connect_ImageIndex,
                Connect_ROIID = this.Connect_ROIID,
                GuidePointColor = this.GuidePointColor,
                GuideLineColor = this.GuideLineColor,
                SelectedBorderColor = this.SelectedBorderColor,
                BorderColor = this.BorderColor,
                //BorderWidth = Global.Roi_BorderWidth,
                BorderWidth = Global.Roi_BorderWidth,
                Font = this.Font,
                ForeColor = this.ForeColor
            };

            clone.Image_Width = this.Image_Width;               // ROI의 기준 Image의 폭
            clone.Image_Height = this.Image_Height;              // ROI의 기준 Image의 높이               
            clone.ROI_MinValue = this.ROI_MinValue;              // ROI내의 Min 값                
            clone.ROI_MaxValue = this.ROI_MaxValue;              // ROI내의 Max 값                
            clone.ROI_Average = this.ROI_Average;               // ROI내의 평균값                 
            clone.ROI_Offset = this.ROI_Offset;                 // ROI의 Offset 값                
            clone.ROI_Gain = this.ROI_Gain;                     // ROI의 Gain 값                
            clone.ROI_Sdnn = this.ROI_Sdnn;                  // ROI내의 표준편차 값
            clone.ROI_Diff = this.ROI_Diff;                  // 해당 ROI와 연결된 ROI의 평균값과의 차이
            clone.ROI_Width = this.ROI_Width;                 // ROI를 포함하는 Area 폭(Image 기준 폭이다. )                
            clone.ROI_Height = this.ROI_Height;                // ROI를 포함하는 Area 높이(Image 기준 높이이다. )                
            clone.ROI_Area = this.ROI_Area;                  // ROI의 실제 면적(Image 기준 면적이다. )
            clone.rotatedArea = this.rotatedArea;               // 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우에 사용, 회전을 적용하기 위한 변수 
            clone.polygonAngle = this.polygonAngle;              // Polygon의 회전 각을 저장한다.
            clone.polygonCenter = this.polygonCenter;             // Polygon의 중심점을 저장한다.
            clone.yMinPointIndex = this.yMinPointIndex;            // 입력 시 Y축이 가장 작은 값을 저장한다. ( Polygon RotatedPoint를 구하기 위해 ) 

            // Image 크기의 점 복사
            foreach (var p in this.imagePointInfo) clone.imagePointInfo.Add(p);

            return clone;
        }

        /** @brief: ROI 도형과 같은 위치에 있는지 확인하는 함수
         *  @section: Description
         *      ROI 도형과 비교할 도형과 같은 위치에 있으면 true, 아니면 false
         *  @param:     roi       비교할 ROI 변수
         *  @return     ROI 도형과 비교할 도형과 같은 위치에 있으면 true, 아니면 false
         */
        public bool IsSameLocation(ROIShape roi)
        {
            if (this.ShapeType != roi.ShapeType) return false;

            // 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우에 rotatedArea를 생성
            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                if (this.rotatedArea.Center != roi.rotatedArea.Center || this.rotatedArea.Size != roi.rotatedArea.Size || this.rotatedArea.Angle != roi.rotatedArea.Angle) return false;
            }
            else
            {
                // Image 크기의 점 복사
                foreach (var p in this.imagePointInfo)
                {
                    int findex = roi.imagePointInfo.FindIndex(fp => (fp.X == p.X && fp.Y == p.Y));
                    if (findex < 0) return false;
                }

            }
            return true;
        }

        /** @brief: 점 추가 ( ROI를 새로 만들 경우에 사용한다. Image 기준으로 입력한다.)
         *  @section: Description
         *      원본 이미지 기준의 Point값을 저장한다.
         *  @param:     index       추가할 index, 도형의 맞춰서 점을 입력한다.
         *  @param:     addPoint    추가할 점( System.Drawing.PointF 변수 )
         *  @return:    추가된 총 Image Point 겟수를 리턴한다.
         */
        public int AddImagePoint(int index, PointF addPoint)
        {
            int maxPoint = 2;
            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Rectangle:
                case ROISHAPETYPE.Diamond:
                case ROISHAPETYPE.Ellipse:
                case ROISHAPETYPE.LineAngle:
                    maxPoint = 2;
                    break;
                case ROISHAPETYPE.LineX:
                    maxPoint = 4;
                    break;
                case ROISHAPETYPE.Polygon:
                    maxPoint = int.MaxValue;
                    break;
            }

            #region 조건에 맞춰서 점 추가한다.
            if (index < this.imagePointInfo.Count)
            {
                this.imagePointInfo[index] = addPoint;
            }
            else
            {
                if (index >= maxPoint)
                {
                    index = maxPoint - 1;
                    this.imagePointInfo[maxPoint - 1] = addPoint;
                }
                else
                {
                    this.imagePointInfo.Add(addPoint);
                    yMinPoint = addPoint; yMinPointIndex = 0;
                }
            }
            #endregion

            // y축 값이 가장 작은 점을 저장한다.
            int findIndex = this.imagePointInfo.FindIndex(p => p.Y < yMinPoint.Y);
            if (findIndex >= 0 && findIndex < this.imagePointInfo.Count)
            {
                yMinPointIndex = findIndex;
                yMinPoint = this.imagePointInfo[yMinPointIndex];
            }

            #region 점이 들어오면 도형의 따라 설정하는 부분
            if (index == 1)
            {
                if (this.ShapeType == ROISHAPETYPE.LineAngle)           // LineAngle일 경우에 두번째 점이 들어오면 각도를 표시하기위해 각도를 구한다.
                {
                    PointF centerPoint = this.imagePointInfo[0];    // 0번째 포이트가 중심
                    PointF anglePoint = addPoint;
                    float angle = ImageUsedMath.AngleBetween(centerPoint, anglePoint, out _);

                    PointF basePoint = new PointF(anglePoint.X, centerPoint.Y);
                    if (angle > 45)
                        basePoint = new PointF(centerPoint.X, anglePoint.Y);

                    if (this.imagePointInfo.Count == 3) this.imagePointInfo[2] = basePoint;
                    else this.imagePointInfo.Add(basePoint);
                }
                // 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우에 rotatedArea를 생성
                else if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
                {
                    // ROI가 포함된 영역을 구한다.
                    var minX = this.imagePointInfo.Min(p => p.X);
                    var minY = this.imagePointInfo.Min(p => p.Y);
                    var maxX = this.imagePointInfo.Max(p => p.X);
                    var maxY = this.imagePointInfo.Max(p => p.Y);

                    float width = maxX - minX;
                    float height = maxY - minY;

                    var centerX = minX + (width / 2);
                    var centerY = minY + (height / 2);

                    this.rotatedArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), 0);
                }
            }

            //            if(this.ShapeType == ROISHAPETYPE.Polygon)  // 도형의 Polygon인 경우 MinAreaRect를 구한다.
            {
                List<OpenCvSharp.Point> polygon = new List<OpenCvSharp.Point>();
                foreach (var roiPoint in this.imagePointInfo) polygon.Add(new OpenCvSharp.Point(roiPoint.X, roiPoint.Y));
                var moments = OpenCvSharp.Cv2.Moments(polygon, false);
                this.polygonCenter = new PointF((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
            }
            #endregion

            return this.imagePointInfo.Count;       // 추가된 총 Image Point 겟수를 리턴한다.
        }

        /** @brief: ROI 각도 정보 읽는다.
         *  @section: Description
         *       ROI의 각도 정보를 읽는다.
         *  @rerurn: ROI의 각도 정보(float 변수)     
         */
        public string SaveROI()
        {
            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                return $"{this.rotatedArea.Angle},{this.rotatedArea.Center.X},{this.rotatedArea.Center.Y},{this.rotatedArea.Size.Width},{this.rotatedArea.Size.Height}";
            }
            else
            {
                return $"{this.polygonAngle}";
            }
        }

        /** @brief: ROI의 점 정보와 각도 정보로 ROI를 다시 만든다.
         *  @section: Description
         *       ROI의 점 정보와 각도 정보로 ROI를 다시 만든다.
         *  @param:     inputPoint    입력할 ROI 도형의 Point 정보 ( Image Size 기준 점 )
         *  @param:     inputAngle    입력할 각도(Degree)( float변수 )
         *  @rerurn:    true: 입력 성공, flase : 입력 실패
         */
        public bool LoadROI(PointF[] inputPoint, float inputAngle, float roiCenterX, float roiCenterY, float roiWidth, float roiHeight)
        {
            // 나머지 도형은 점 2개 이상
            if (inputPoint.Length < 2) return false;
            // Polygon은 최소 점이 3개 이상
            if (this.ShapeType == ROISHAPETYPE.Polygon && inputPoint.Length < 3) return false;
            // LineAngle는 점의 3개 이다.
            if (this.ShapeType == ROISHAPETYPE.LineAngle && inputPoint.Length != 3) return false;
            // LineX는 점이 4개 이다
            if (this.ShapeType == ROISHAPETYPE.LineX && inputPoint.Length != 4) return false;

            // 점을 입력 한다.
            this.imagePointInfo.Clear();
            this.imagePointInfo.AddRange(inputPoint);

            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                if ((roiWidth * roiHeight) <= 0)
                {
                    // ROI가 포함된 영역을 구한다.
                    var minX = this.imagePointInfo.Min(p => p.X);
                    var minY = this.imagePointInfo.Min(p => p.Y);
                    var maxX = this.imagePointInfo.Max(p => p.X);
                    var maxY = this.imagePointInfo.Max(p => p.Y);

                    roiWidth = maxX - minX;
                    roiHeight = maxY - minY;

                    roiCenterX = minX + (roiWidth / 2);
                    roiCenterY = minY + (roiWidth / 2);

                    inputAngle = 0;
                }

                this.rotatedArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(roiCenterX, roiCenterY), new OpenCvSharp.Size2f(roiWidth, roiHeight), inputAngle);
            }
            else
            {
                // 도형의 중심점을 구한다.
                List<OpenCvSharp.Point> polygon = new List<OpenCvSharp.Point>();
                foreach (var roiPoint in this.imagePointInfo) polygon.Add(new OpenCvSharp.Point(roiPoint.X, roiPoint.Y));
                var moments = OpenCvSharp.Cv2.Moments(polygon, false);
                this.polygonCenter = new PointF((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));

                // 회전각을 저장한다.
                this.polygonAngle = inputAngle;
            }

            #region 각 코너 좌표 구하기
            var ltX = this.rotatedArea.Points()[1].X;                 // (Left , Top) X 좌표
            var ltY = this.rotatedArea.Points()[1].Y;                 // (Left , Top) Y 좌표
            var rtX = this.rotatedArea.Points()[2].X;                 // (Right, Top) X 좌표
            var rtY = this.rotatedArea.Points()[2].Y;                 // (Right, Top) Y 좌표

            var lbX = this.rotatedArea.Points()[0].X;                 // (Left , Bottom) X 좌표
            var lbY = this.rotatedArea.Points()[0].Y;                 // (Left , Bottom) Y 좌표
            var rbX = this.rotatedArea.Points()[3].X;                 // (Right, Bottom) X 좌표
            var rbY = this.rotatedArea.Points()[3].Y;                 // (Right, Bottom) Y 좌표                        
            #endregion

            RectangleF roiAres = this.ShapeArea();                   // ROI를 포함한 최소 영역 가지고 오기

            this.ROI_Width = roiAres.Width;        // ROI가 포함하는 Area 폭
            this.ROI_Height = roiAres.Height;       // ROI가 포함하는 Area 높이

            // ROI Type에 따라 실제 ROI영역을 구한다.
            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Polygon:                                                  // Polygon 모양을 White Mask한다.
                    {
                        // ROI의 점을 Mask하기위해 변경한다.(Polygon점으로 변경)                        
                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>();
                        foreach (var p in this.imagePointInfo)
                            points2f.Add(new OpenCvSharp.Point2f(p.X, p.Y));
                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);              // 다각형 넓이                        
                        break;
                    }
                case ROISHAPETYPE.Diamond:                                                  // 마름모 모양을 White Mask한다.
                    {
                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>
                        {
                            new OpenCvSharp.Point((ltX + rtX) / 2, (ltY + rtY) / 2),    // TC
                            new OpenCvSharp.Point((rtX + rbX) / 2, (rtY + rbY) / 2),    // RC
                            new OpenCvSharp.Point((rbX + lbX) / 2, (rbY + lbY) / 2),    // BC
                            new OpenCvSharp.Point((lbX + ltX) / 2, (lbY + ltY) / 2)     // LC
                        };

                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);           // 마름모 넓이
                        break;
                    }
                case ROISHAPETYPE.Rectangle:                                        // Rectangle 모양을 White Mask한다.
                    {
                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(this.rotatedArea.Points()); // Rectangle 넓이
                        break;
                    }
                case ROISHAPETYPE.Ellipse:                                         // 타원 모양으로 Mask하는 부분
                    {
                        this.ROI_Area = (this.rotatedArea.Size.Width / 2.0f) * (this.rotatedArea.Size.Height / 2.0f) * (float)Math.PI; // 타원 넓이
                        break;
                    }
            }

            return true;
        }


    #endregion 생성자, CopyTo, Clone, AddImagePoint, Save, Load  

    #region IsDraw, IsShape, ShapeArea, RemovePointAll, PolygonRemoveLastPoint

        /** @brief: ROI가 그려지는 확인( 점이 2개 이상이면 : true )
         *  @section: Description
         *      ROI가 그려지는 조건은 점이 2개 이상이어야 한다.
         *  @return : 점이 2개 이상 -> true, 아니면 -> false
         */
        public bool IsDraw()
        {
            if (this.imagePointInfo.Count < 2)
                return false;
            else
            {
                // 두점의 거리가 over_dist_squared 보다 작은면 
                if (ImageUsedMath.DistanceToPoint(this.imagePointInfo[0], this.imagePointInfo[1]) < 5f) return false;

                return true;
            }
        }

        /** @brief: ROI가 도형인지 확인 함수
         *  @section: Description
         *      ROI의 영역이 최소 10 이상이어야 한다.
         *  @return :  ROI의 영역이 최소 10 이상 -> true, 아니면 -> false
         */
        public bool IsShape()
        {
            // 나머지 도형은 점 2개 이상
            if (this.imagePointInfo.Count < 2) return false;
            // Polygon은 최소 점이 3개 이상
            if (this.ShapeType == ROISHAPETYPE.Polygon && this.imagePointInfo.Count < 3) return false;
            // LineX는 점이 4개 이여야 한다.
            if (this.ShapeType == ROISHAPETYPE.LineX && this.imagePointInfo.Count != 4) return false;


            // ROI가 포함된 영역을 구한다.
            var minX = this.imagePointInfo.Min(p => p.X);
            var minY = this.imagePointInfo.Min(p => p.Y);
            var maxX = this.imagePointInfo.Max(p => p.X);
            var maxY = this.imagePointInfo.Max(p => p.Y);
            float width = maxX - minX;
            float height = maxY - minY;

            if (System.Math.Abs(width * height) < 10) return false;  // ROI의 영역이 최소 10 이상이어야 한다.
            return true;
        }

        /** @brief: ROI를 포함한 최소 영역 구하는 함수
         *  @section: Description
         *      ROI의 점들을 포함한 최소 영역을 구한다.
         *  @return :  ROI의 점들이 포함된 최소 영역(System.Drawing.RectangleF 변수)
         */
        public RectangleF ShapeArea()
        {
            // 나머지 도형은 점 2개 이상
            if (this.imagePointInfo.Count < 2) return new RectangleF();
            // Polygon은 최소 점이 3개 이상
            if (this.ShapeType == ROISHAPETYPE.Polygon && this.imagePointInfo.Count < 3) return new RectangleF();
            // LineX는 점이 4개 이여야 한다.
            if (this.ShapeType == ROISHAPETYPE.LineX && this.imagePointInfo.Count != 4) return new RectangleF();



            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                OpenCvSharp.Rect boundingRect = this.rotatedArea.BoundingRect();

                return new RectangleF(boundingRect.Left, boundingRect.Top, boundingRect.Width, boundingRect.Height);
            }
            else
            {
                // ROI가 포함된 영역을 구한다.
                var minX = this.imagePointInfo.Min(p => p.X);
                var minY = this.imagePointInfo.Min(p => p.Y);
                var maxX = this.imagePointInfo.Max(p => p.X);
                var maxY = this.imagePointInfo.Max(p => p.Y);
                float width = maxX - minX;
                float height = maxY - minY;

                return new RectangleF(minX, minY, width, height);
            }
        }

        public RectangleF ShapeAreaResize()
        {
            // 나머지 도형은 점 2개 이상
            if (this.PointsInfo.Count < 2) return new RectangleF();
            // Polygon은 최소 점이 3개 이상
            if (this.ShapeType == ROISHAPETYPE.Polygon && this.PointsInfo.Count < 3) return new RectangleF();
            // LineX는 점이 4개 이여야 한다.
            if (this.ShapeType == ROISHAPETYPE.LineX && this.PointsInfo.Count != 4) return new RectangleF();



            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                OpenCvSharp.Rect boundingRect = this.rotatedDrawArea.BoundingRect();

                return new RectangleF(boundingRect.Left, boundingRect.Top, boundingRect.Width, boundingRect.Height);
            }
            else
            {
                // ROI가 포함된 영역을 구한다.
                var minX = this.PointsInfo.Min(p => p.X);
                var minY = this.PointsInfo.Min(p => p.Y);
                var maxX = this.PointsInfo.Max(p => p.X);
                var maxY = this.PointsInfo.Max(p => p.Y);
                float width = maxX - minX;
                float height = maxY - minY;

                return new RectangleF(minX, minY, width, height);
            }
        }

        /** @brief: ROI의 점 모두 삭제
         *  @section: Description
         *      ROI의 점 모두 삭제된다. (주의해서 사용)
         */
        public void RemovePointAll() { this.imagePointInfo.Clear(); }

        /** @brief: Polygon의 마지막 점 삭제( 그리는 도중에 사용한다. )
         *  @section: Description
         *      Polygon을 그릴 경우에 그리기 종료시 마지막 점을 삭제한다.
         */
        public void PolygonRemoveLastPoint()
        {
            // Polygon이고 점의 4개 이상인 경우에만 적용
            if (this.ShapeType == ROISHAPETYPE.Polygon && this.imagePointInfo.Count > 3)
                this.imagePointInfo.RemoveAt(this.imagePointInfo.Count - 1);
        }
    #endregion

    #region Rotation, Mirror, Flip, AllImagePoint, MoveTo, InsidePolygon, DoesRectangleContainPoint, RotatedAreaOffset, Offset, CalROIShare

        /** @brief: ROI를 중심을 기준으로 회전시킨다. (LineAngle, LineX는 적용안됨.)
         *  @section: Description
         *      ROI를 중심을 기준으로 입력된 값 만큼 회전시킨다.
         *  @param:     AngleInerval    회전시킬 각도 량(Degree)( float변수 ), 양수 : 시계방향, 음수: 반시계방향
         *  @rerurn:    리턴값 없음.
         */
        public void Rotation(float AngleInerval = 1)
        {
            // Rectangle, Diamond, Ellipse 인경우는 rotatedArea의 각도를 변경한다.
            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                this.rotatedArea.Angle += AngleInerval;

                this.imagePointInfo.Clear();
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
            }
            else if (this.ShapeType == ROISHAPETYPE.Polygon)                 // Polygon의 중심점을 기준으로 AngleInerval만큼 이동한다.
            {
                if (!float.IsNaN(polygonCenter.X) && !float.IsNaN(polygonCenter.Y))       // Polygen의 중심점을 구한경우
                {
                    for (int index = 0; index < this.imagePointInfo.Count; index++)
                    {
                        PointF p = this.imagePointInfo[index];
                        this.imagePointInfo[index] = ImageUsedMath.Rotate(p, polygonCenter, AngleInerval);
                    }
                    this.polygonAngle += AngleInerval;
                }
            }
        }

        /** @brief: 주어진 점를 중심을 기준으로 회전시킨다. (LineAngle, LineX는 적용안됨.)
         *  @section: Description
         *      주이진 점를 중심을 기준으로 입력된 값 만큼 회전시킨다.
         *  @param:     pt              회전에 중심이 될 점( System.Drawing.PointF 변수 )
         *  @param:     AngleInerval    회전시킬 각도 량(Degree)( float변수 ), 양수 : 시계방향, 음수: 반시계방향
         *  @rerurn:    리턴값 없음.
         */
        public void Rotation(PointF pt, float AngleInerval = 1)
        {
            // Rectangle, Diamond, Ellipse 인경우는 rotatedArea의 각도를 변경한다.
            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                float ltX = this.rotatedArea.Points()[1].X;         // (Left , Top) X 좌표
                float ltY = this.rotatedArea.Points()[1].Y;         // (Left , Top) Y 좌표
                float rtX = this.rotatedArea.Points()[2].X;         // (Right, Top) X 좌표
                float rtY = this.rotatedArea.Points()[2].Y;         // (Right, Top) Y 좌표

                float ctX = (ltX + rtX) / 2;                        // Top Line 중심점 X
                float ctY = (ltY + rtY) / 2;                        // Top Line 중심점 Y                

                float centerX = this.rotatedArea.Center.X;          // 도형의 중심점 X
                float centerY = this.rotatedArea.Center.Y;          // 도형의 중심점 Y


                PointF rCTP = ImageUsedMath.Rotate(new PointF(ctX, ctY), pt, AngleInerval);           // 회전한 Top Line 중심점
                PointF rCP = ImageUsedMath.Rotate(new PointF(centerX, centerY), pt, AngleInerval);    // 회전한 도형의 중심점

                ImageUsedMath.AngleBetween(rCP, rCTP, out float realAngle);                     // 회전한 Top Line 중심점과 도형의 중심점의 각도를 구한다.
                realAngle -= 270;       // 도형의 구한 각도의 270을 빼야 된다.

                // 회전한 도형의 중심점과, 구한 회전한 Top Line 중심점과 도형의 중심점의 각도를 가지고 도형을 다시 만든다.
                this.rotatedArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(rCP.X, rCP.Y), this.rotatedArea.Size, realAngle);

                this.imagePointInfo.Clear();
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
            }
            else if (this.ShapeType == ROISHAPETYPE.Polygon)                 // Polygon의 중심점을 기준으로 AngleInerval만큼 이동한다.
            {
                if (!float.IsNaN(polygonCenter.X) && !float.IsNaN(polygonCenter.Y))       // Polygen의 중심점을 구한경우
                {
                    for (int index = 0; index < this.imagePointInfo.Count; index++)
                    {
                        PointF p = this.imagePointInfo[index];
                        this.imagePointInfo[index] = ImageUsedMath.Rotate(p, pt, AngleInerval);
                    }
                    this.polygonAngle += AngleInerval;
                }
            }
        }

        /** @brief: ROI를 중심을 기준으로 좌추대칭변환한다. (LineAngle, LineX는 적용안됨.)
         *  @section: Description
         *      ROI를 중심을 기준으로 좌우로 대칭 변환한다. (LineAngle, LineX는 적용안됨.)         
         *  @rerurn:    리턴값 없음.
         */
        public void Mirror()
        {
            // ROI Type이 LineAngle, LineX는 적용안됨.
            if (this.ShapeType == ROISHAPETYPE.LineAngle || this.ShapeType == ROISHAPETYPE.LineX) return;

            List<PointF> imageTempPointInfo = new List<PointF>();
            if (this.ShapeType == ROISHAPETYPE.Polygon) // ROI Type이 Polygon인 경우
            {
                float centerX = this.polygonCenter.X;
                float centerY = this.polygonCenter.Y;
                float pX, pY, gepX, gepY;
                foreach (var pf in this.imagePointInfo)
                {
                    gepX = centerX - pf.X;      // centerX와 pf.x의 차
                    gepY = centerY - pf.Y;      // centerY와 pf.Y의 차

                    // 좌우 대칭 처리                    
                    pX = pf.X + (gepX * 2); // 차의 두배만큼 더한다. 
                    pY = pf.Y;

                    imageTempPointInfo.Add(new PointF(pX, pY));
                }

                this.imagePointInfo.Clear();
                this.imagePointInfo.AddRange(imageTempPointInfo);
            }
            else                                            // ROI Type이 Rectangle, Diamond, Ellipse 인 경우
            {
                // rotatedArea의 중심점을 구한다.
                float centerX = this.rotatedArea.Center.X;
                float centerY = this.rotatedArea.Center.Y;

                float pX, pY, gepX, gepY;
                // rotatedArea의 모든점을 좌우 or 상하 대칭이동한다.
                foreach (var pf in this.rotatedArea.Points())
                {
                    gepX = centerX - pf.X;      // centerX와 pf.x의 차
                    gepY = centerY - pf.Y;      // centerY와 pf.Y의 차

                    // 좌우 대칭 처리
                    pX = pf.X + (gepX * 2); // 차의 두배만큼 더한다. 
                    pY = pf.Y;

                    imageTempPointInfo.Add(new PointF(pX, pY));
                }

                var ltX = imageTempPointInfo[1].X;           // (Left , Top) X 좌표
                var ltY = imageTempPointInfo[1].Y;           // (Left , Top) Y 좌표                
                var rtX = imageTempPointInfo[2].X;           // (Right, Top) X 좌표
                var rtY = imageTempPointInfo[2].Y;           // (Right, Top) Y 좌표                

                // 대칭이동한 (Left , Top)점, (Right, Top)점의 중심점을 구한다.
                var ctX = (ltX + rtX) / 2;
                var ctY = (ltY + rtY) / 2;

                // 대칭이동한 Top Line의 중심점과 rotatedArea의 중심점의 각도를 구한다.
                ImageUsedMath.AngleBetween(new PointF(centerX, centerY), new PointF(ctX, ctY), out float realAngle);

                // 각도만큼 회전한다.
                this.rotatedArea.Angle = realAngle - 270;

                // 회전후 (Left, Top)점, (Right, Bottom)점 저장한다.
                this.imagePointInfo.Clear();
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
            }
        }

        /** @brief: ROI를 중심을 기준으로 상하대칭변환한다. (LineAngle, LineX는 적용안됨.)
         *  @section: Description
         *      ROI를 중심을 기준으로 상하로 대칭 변환한다. (LineAngle, LineX는 적용안됨.)         
         *  @rerurn:    리턴값 없음.
         */
        public void Flip()
        {
            // ROI Type이 LineAngle, LineX는 적용안됨.
            if (this.ShapeType == ROISHAPETYPE.LineAngle || this.ShapeType == ROISHAPETYPE.LineX) return;

            List<PointF> imageTempPointInfo = new List<PointF>();
            if (this.ShapeType == ROISHAPETYPE.Polygon) // ROI Type이 Polygon인 경우
            {
                float centerX = this.polygonCenter.X;
                float centerY = this.polygonCenter.Y;
                float pX, pY, gepX, gepY;
                foreach (var pf in this.imagePointInfo)
                {
                    gepX = centerX - pf.X;      // centerX와 pf.x의 차
                    gepY = centerY - pf.Y;      // centerY와 pf.Y의 차

                    // 상하 대칭 처리
                    pX = pf.X;
                    pY = pf.Y + (gepY * 2); // 차의 두배만큼 더한다.                     

                    imageTempPointInfo.Add(new PointF(pX, pY));
                }

                this.imagePointInfo.Clear();
                this.imagePointInfo.AddRange(imageTempPointInfo);
            }
            else                                            // ROI Type이 Rectangle, Diamond, Ellipse 인 경우
            {
                // rotatedArea의 중심점을 구한다.
                float centerX = this.rotatedArea.Center.X;
                float centerY = this.rotatedArea.Center.Y;

                float pX, pY, gepX, gepY;
                // rotatedArea의 모든점을 좌우 or 상하 대칭이동한다.
                foreach (var pf in this.rotatedArea.Points())
                {
                    gepX = centerX - pf.X;      // centerX와 pf.x의 차
                    gepY = centerY - pf.Y;      // centerY와 pf.Y의 차
                    // 상하 대칭 처리
                    pX = pf.X;
                    pY = pf.Y + (gepY * 2); // 차의 두배만큼 더한다.                     

                    imageTempPointInfo.Add(new PointF(pX, pY));
                }

                var ltX = imageTempPointInfo[1].X;           // (Left , Top) X 좌표
                var ltY = imageTempPointInfo[1].Y;           // (Left , Top) Y 좌표                
                var rtX = imageTempPointInfo[2].X;           // (Right, Top) X 좌표
                var rtY = imageTempPointInfo[2].Y;           // (Right, Top) Y 좌표                

                // 대칭이동한 (Left , Top)점, (Right, Top)점의 중심점을 구한다.
                var ctX = (ltX + rtX) / 2;
                var ctY = (ltY + rtY) / 2;

                // 대칭이동한 Top Line의 중심점과 rotatedArea의 중심점의 각도를 구한다.
                ImageUsedMath.AngleBetween(new PointF(centerX, centerY), new PointF(ctX, ctY), out float realAngle);

                // 각도만큼 회전한다.
                this.rotatedArea.Angle = realAngle - 270;

                // 회전후 (Left, Top)점, (Right, Bottom)점 저장한다.
                this.imagePointInfo.Clear();
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
            }
        }

        /** @brief: ROI의 모든 점 가지고 오기
         *  @section: Description
         *      ROI의 모든 점을 가지고 온다. ( Array형태 )
         *  @rerurn:    추가된 총 Image Point 겟수를 리턴한다.
         */
        public PointF[] AllImagePoint() { return this.imagePointInfo.ToArray(); }

        /** @brief: 도형을 해당 위치로 이동 ( Center 점을 해당 위치로 이동한다. )
         *  @section: Description
         *      도형의 Center 점을 해당 위치로 이동한다.
         *  @param:     movePoint       이동할 점
         */
        public void MoveTo(PointF movePoint)
        {
            if (this.imagePointInfo.Count < 2 || this._nodeSelected == ROISHAPEPOSTION.None) return;

            // 도형이 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우에 rotatedArea 다시 만들지 or center를 이동할 지 확인
            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                this.rotatedArea.Center.X = movePoint.X;
                this.rotatedArea.Center.Y = movePoint.Y;

                // 이동후 (Left, Top)점, (Right, Bottom)점 저장한다.
                this.imagePointInfo.Clear();
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
                this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
            }
            else
            {
                PointF lastPoint = this.imagePointInfo[0];
                foreach (var pf in this.imagePointInfo) if (pf.Y > lastPoint.Y) lastPoint = pf;

                float xOffset = movePoint.X - lastPoint.X;
                float yOffset = movePoint.Y - lastPoint.Y;

                List<OpenCvSharp.Point> polygon = new List<OpenCvSharp.Point>();
                for (int Index = 0; Index < this.imagePointInfo.Count; Index++)
                {
                    PointF mPoint = this.imagePointInfo[Index];

                    mPoint.X += xOffset;
                    mPoint.Y += yOffset;

                    this.imagePointInfo[Index] = mPoint;

                    polygon.Add(new OpenCvSharp.Point(mPoint.X, mPoint.Y));
                }
                // 도형의 중심점을 구한다.            
                var moments = OpenCvSharp.Cv2.Moments(polygon, false);
                this.polygonCenter = new PointF((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
            }
        }

        /** @brief: Polygon 안에 점이 있는지 확인
         *  @section: Description
         *      원본 이미지 기준의 Point값이 Polygon안에 있는지 확인한다.
         *  @param:     mouse_pt    원본 이미지 기준의 마우스 점( System.Drawing.PointF 변수 )
         *  @rerurn:    Polygon 안에 점이 있으면 -> true, 없으면 -> false
         */
        private bool InsidePolygon(PointF mouse_pt)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(this.imagePointInfo.ToArray());

            // See if the point is inside the GraphicsPath.
            if (path.IsVisible(mouse_pt))
            {
                return true;
            }

            return false;
        }

        /** @brief: RotatedRect 내부 점 Check
         *  @section: Description
         *      RotatedRect 내부 점인지 검사 함수
         *  @param:     rectangle   RotatedRect 변수(OpenCvSharp.RotatedRect 변수)
         *  @param:     point       검사할 점( System.Drawing.PointF 변수 )
         *  @rerurn:    RotatedRect 안에 점이 있으면 -> true, 없으면 -> false
         */
        bool DoesRectangleContainPoint(OpenCvSharp.RotatedRect rectangle, PointF point)
        {
            //Get the corner points.
            OpenCvSharp.Point2f[] corners = rectangle.Points();

            //Check if the point is within the rectangle.
            double indicator = OpenCvSharp.Cv2.PointPolygonTest(corners, new OpenCvSharp.Point2f(point.X, point.Y), false);
            bool rectangleContainsPoint = (indicator >= 0);
            return rectangleContainsPoint;
        }

        /** @brief: 도형이 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우 Node 선택에 따른 Offset 이동 
         *  @section: Description
         *      ROI를 이동 및 점 위치 수정할 경우에 사용한다. Image 기준으로 이동한다. 도형이 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우
         *  @param:     xOffset       Image 기준의 x 이동 값 (float)
         *  @param:     yOffset       Image 기준의 y 이동 값 (float)
         */
        private void RotatedAreaOffset(float xOffset, float yOffset)
        {
            if (this._nodeSelected == ROISHAPEPOSTION.None) return;

            var ltX = this.rotatedArea.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = this.rotatedArea.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = this.rotatedArea.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = this.rotatedArea.Points()[2].Y;           // (Right, Top) Y 좌표
            var lbX = this.rotatedArea.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = this.rotatedArea.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = this.rotatedArea.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = this.rotatedArea.Points()[3].Y;           // (Right, Bottom) Y 좌표


            float width = this.rotatedArea.Size.Width;
            float height = this.rotatedArea.Size.Height;

            float centerX = this.rotatedArea.Center.X + ((float)xOffset / 2f);
            float centerY = this.rotatedArea.Center.Y + ((float)yOffset / 2f);

            // Node 선택에 따라 점 이동 Offset 설정
            switch (this.NodeSelected)
            {
                case ROISHAPEPOSTION.AreaLeftTop:                   // (Left , Top) 점 이동 계산 부분
                    {
                        ltX += xOffset; ltY += yOffset;

                        width = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(ltX, ltY), new PointF(rtX, rtY), new PointF(rbX, rbY));
                        height = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(ltX, ltY), new PointF(lbX, lbY), new PointF(rbX, rbY));
                        break;
                    }
                case ROISHAPEPOSTION.AreaRightTop:                  // (Right, Top) 점 이동 계산 부분
                    {
                        rtX += xOffset; rtY += yOffset;

                        width = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(rtX, rtY), new PointF(ltX, ltY), new PointF(lbX, lbY));
                        height = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(rtX, rtY), new PointF(rbX, rbY), new PointF(lbX, lbY));
                        break;
                    }
                case ROISHAPEPOSTION.AreaLeftBottom:                // (Left , Bottom) 점 이동 계산 부분
                    {
                        lbX += xOffset; lbY += yOffset;

                        width = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(lbX, lbY), new PointF(rbX, rbY), new PointF(rtX, rtY));
                        height = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(lbX, lbY), new PointF(ltX, ltY), new PointF(rtX, rtY));
                        break;
                    }
                case ROISHAPEPOSTION.AreaRightBottom:               // (Right, Bottom) 점 이동 계산 부분
                    {
                        rbX += xOffset; rbY += yOffset;

                        width = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(rbX, rbY), new PointF(lbX, lbY), new PointF(ltX, ltY));
                        height = ImageUsedMath.FindDistanceToSegmentSquared(new PointF(rbX, rbY), new PointF(rtX, rtY), new PointF(ltX, ltY));
                        break;
                    }

                case ROISHAPEPOSTION.AreaLeftMiddle:                // Left 변 이동 계산 부분
                    {
                        width -= xOffset;

                        centerX = this.rotatedArea.Center.X + (float)(((double)xOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle)));
                        centerY = this.rotatedArea.Center.Y + (float)(((double)xOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle)));
                        break;
                    }
                case ROISHAPEPOSTION.AreaTopMiddle:                 // Top 변 이동 계산 부분
                    {
                        height -= yOffset;
                        centerX = this.rotatedArea.Center.X + (float)(((double)yOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle + 90)));
                        centerY = this.rotatedArea.Center.Y + (float)(((double)yOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle + 90)));
                        break;
                    }
                case ROISHAPEPOSTION.AreaRightMiddle:               // Right 변 이동 계산 부분
                    {
                        width += xOffset;
                        centerX = this.rotatedArea.Center.X + (float)(((double)xOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle)));
                        centerY = this.rotatedArea.Center.Y + (float)(((double)xOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle)));
                        break;
                    }
                case ROISHAPEPOSTION.AreaBottomMiddle:              // Bottom 변 이동 계산 부분
                    {
                        height += yOffset;

                        centerX = this.rotatedArea.Center.X + (float)(((double)yOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle + 90)));
                        centerY = this.rotatedArea.Center.Y + (float)(((double)yOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedArea.Angle + 90)));
                        break;
                    }

                case ROISHAPEPOSTION.Rotation:                      // 회전 점 이동시 회전각 계산후 바로 return한다.
                    {
                        var ctX = (ltX + rtX) / 2;
                        var ctY = (ltY + rtY) / 2;

                        ImageUsedMath.AngleBetween(new PointF(this.rotatedArea.Center.X, this.rotatedArea.Center.Y), new PointF(ctX + xOffset, ctY + yOffset), out float realAngle);
                        this.rotatedArea.Angle = realAngle - 270;

                        // 회전후 (Left, Top)점, (Right, Bottom)점 저장한다.
                        this.imagePointInfo.Clear();
                        this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
                        this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
                        return;
                    }    // 위쪽 회전                 

                case ROISHAPEPOSTION.Inside:                        // 전체 이동시 Center 점을 이동한후 return한다.
                    {
                        this.rotatedArea.Center.X += xOffset;
                        this.rotatedArea.Center.Y += yOffset;

                        // 이동후 (Left, Top)점, (Right, Bottom)점 저장한다.
                        this.imagePointInfo.Clear();
                        this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
                        this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
                        return;
                    }
                default:
                    return;
            }

            this.rotatedArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), this.rotatedArea.Angle);

            // 이동후 (Left, Top)점, (Right, Bottom)점 저장한다.
            this.imagePointInfo.Clear();
            this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[1].X, this.rotatedArea.Points()[1].Y));
            this.imagePointInfo.Add(new PointF(this.rotatedArea.Points()[3].X, this.rotatedArea.Points()[3].Y));
        }

        /** @brief: Node 선택에 따른 Offset 이동 
         *  @section: Description
         *      ROI를 이동 및 점 위치 수정할 경우에 사용한다. Image 기준으로 이동한다.
         *  @param:     xOffset       Image 기준의 x 이동 값 (float)
         *  @param:     yOffset       Image 기준의 y 이동 값 (float)
         */
        public void Offset(float xOffset, float yOffset)
        {
            if (this.imagePointInfo.Count < 2 || this._nodeSelected == ROISHAPEPOSTION.None) return;

            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                // 도형이 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우
                RotatedAreaOffset(xOffset, yOffset);
                return;
            }

            // Node 선택에 따라 점 이동 Offset 설정
            switch (this.NodeSelected)
            {
                case ROISHAPEPOSTION.Inside:
                    {
                        List<OpenCvSharp.Point> polygonInside = new List<OpenCvSharp.Point>();

                        for (int Index = 0; Index < this.imagePointInfo.Count; Index++)
                        {
                            PointF mPoint = this.imagePointInfo[Index];

                            mPoint.X += xOffset;
                            mPoint.Y += yOffset;

                            this.imagePointInfo[Index] = mPoint;
                            polygonInside.Add(new OpenCvSharp.Point(mPoint.X, mPoint.Y));
                        }

                        var momentsinside = OpenCvSharp.Cv2.Moments(polygonInside, false);

                        // 도형의 중심점을 구한다.
                        this.polygonCenter = new PointF((float)(momentsinside.M10 / momentsinside.M00), (float)(momentsinside.M01 / momentsinside.M00));
                        return;
                    }
            }

            switch (this.ShapeType)
            {
                case ROISHAPETYPE.LineAngle:
                    {
                        if (this.imagePointInfo.Count != 3) break;

                        PointF centerPoint = this.imagePointInfo[0];    // 0번째 포이트가 중심
                        PointF anglePoint = this.imagePointInfo[1];

                        if (this.movePointIndex == 1)
                        {
                            anglePoint.X += xOffset;
                            anglePoint.Y += yOffset;
                        }
                        else if (this.movePointIndex == 2)
                        {
                            if (ImageUsedMath.AngleBetween(centerPoint, anglePoint, out _) > 45) anglePoint.Y += yOffset;
                            else anglePoint.X += xOffset;
                        }
                        else
                        {
                            if (ImageUsedMath.AngleBetween(centerPoint, anglePoint, out _) > 45) centerPoint.Y += yOffset;
                            else centerPoint.X += xOffset;
                        }

                        PointF basePoint = new PointF(anglePoint.X, centerPoint.Y);
                        if (ImageUsedMath.AngleBetween(centerPoint, anglePoint, out _) > 45)
                            basePoint = new PointF(centerPoint.X, anglePoint.Y);

                        this.imagePointInfo[0] = centerPoint;
                        this.imagePointInfo[1] = anglePoint;
                        this.imagePointInfo[2] = basePoint;
                        break;
                    }
                case ROISHAPETYPE.LineX:
                case ROISHAPETYPE.Polygon:
                    {
                        if (this.NodeSelected == ROISHAPEPOSTION.Rotation && this.yMinPointIndex >= 0 && this.yMinPointIndex < this.imagePointInfo.Count)
                        {
                            if (!float.IsNaN(this.polygonCenter.X) && !float.IsNaN(this.polygonCenter.Y))       // Polygen의 중심점을 구한경우
                            {
                                // Polygen의 중심점과 YMinPoint와의 거리에 object_radius * 4를 더한값이 회전점의 거리
                                float distanceRP = ImageUsedMath.DistanceToPoint(this.polygonCenter, new PointF(this.imagePointInfo[this.yMinPointIndex].X, this.imagePointInfo[this.yMinPointIndex].Y)) + object_radius * 4;
                                // 회전점을 구한다.
                                PointF pointRP = ImageUsedMath.FindDistancePoint(this.polygonCenter, distanceRP, this.polygonAngle - 90);

                                // Polygen의 중심점과 회전점이 이동한 각도구하기
                                ImageUsedMath.AngleBetween(this.polygonCenter, new PointF(pointRP.X + xOffset, pointRP.Y + yOffset), out float realAngle);

                                // Polygen의 점들을 (realAngle - 270) - this.polygonAngle만큼 회전한다.      
                                float rotationAngle = (realAngle - 270) - this.polygonAngle;
                                this.Rotation(rotationAngle);
                            }
                        }
                        else
                        {
                            if (this.movePointIndex >= 0 && this.movePointIndex < this.imagePointInfo.Count)
                            {
                                PointF mPoint = this.imagePointInfo[this.movePointIndex];

                                mPoint.X += xOffset;
                                mPoint.Y += yOffset;

                                this.imagePointInfo[this.movePointIndex] = mPoint;
                            }
                        }
                        break;
                    }
            }

            // 도형의 중심점을 구한다.
            List<OpenCvSharp.Point> polygon = new List<OpenCvSharp.Point>();
            foreach (var roiPoint in this.imagePointInfo) polygon.Add(new OpenCvSharp.Point(roiPoint.X, roiPoint.Y));
            var moments = OpenCvSharp.Cv2.Moments(polygon, false);
            this.polygonCenter = new PointF((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));
        }

        /** @brief: 입력된 Image Mat에서 ROI 도형 영역구하고, Min,Max,평균,표준편차, 넓이구해서 저장한다.
         *  @section: Description
         *      입력된 Image Mat(그래이 영상만 가능하다)에서 ROI 도형의 영역을 구하고,  
         *      this.ROI_MinValue(Min값),this.ROI_MaxValue(Max값),this.ROI_Average(평균값)this.ROI_Sdnn(표준편차값) 구해 저장한다.
         *      ROI의 Min, Max, 평균, 표준편차를 구하는 부분이므로 LinX, LineAngle은 해당되지 않는다.
         *  @param:     roiMat       구할 Image Mat(그래이 영상만 가능하다)
         */
        public void CalROIShare(OpenCvSharp.Mat imageMat)
        {
            // ROI의 Min, Max, 평균, 표준편차를 구하는 부분이므로 LinX, LineAngle은 해당되지 않는다.
            if (this.ShapeType == ROISHAPETYPE.LineX || this.ShapeType == ROISHAPETYPE.LineAngle) return;

            // 그래이 영상만 가능하다.
            if (imageMat == null || imageMat.Type() != OpenCvSharp.MatType.CV_8UC1) return;

            // 도형이 아니면
            if (this.IsShape() == false) return;

            int matWidth = imageMat.Width;              // 이미지의 넓이
            int matHeight = imageMat.Height;             // 이미지의 높이

            RectangleF roiAres = this.ShapeArea();                   // ROI를 포함한 최소 영역 가지고 오기

            // roiAres의 값의 예외처리
            float conLeft = roiAres.Left;
            float conTop = roiAres.Top;
            float conWidth = roiAres.Width;
            float conHeight = roiAres.Height;

            if (conLeft < 0) conLeft = 0; if (conTop < 0) conTop = 0; if (conWidth < 0) conWidth = 0; if (conHeight < 0) conHeight = 0;

            if (conLeft > matWidth) conLeft = matWidth; if (conTop > matHeight) conTop = matHeight;
            if (conLeft + conWidth > matWidth) conWidth = matWidth - conLeft;
            if (conTop + conHeight > matHeight) conHeight = matHeight - conTop;

            this.ROI_Width = conWidth;        // ROI가 포함하는 Area 폭
            this.ROI_Height = conHeight;       // ROI가 포함하는 Area 높이

            // 이미지 ROI 영역이다.
            OpenCvSharp.Rect conRect = new OpenCvSharp.Rect((int)conLeft, (int)conTop, (int)conWidth, (int)conHeight);

            // 넓이, 높이가 0보다 작으면 처리 안한다.
            if (conRect.Width <= 0 || conRect.Height <= 0) return;

            // 이미지에서 ROI 영역을 가지고 온다.
            OpenCvSharp.Mat roiImage = imageMat.SubMat(conRect).Clone();

            // 가지온 온 ROI 영역 이미지에서 실제 ROI영역을 Mask 하기위한 변수작업
            OpenCvSharp.Mat Mask = new OpenCvSharp.Mat(new OpenCvSharp.Size(conWidth, conHeight), roiImage.Type());
            Mask.SetTo(OpenCvSharp.Scalar.Black);

            // ROI Type에 따라 실제 ROI영역을 Mask 한다.            
            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Polygon:                                                  // Polygon 모양을 White Mask한다.
                    {
                        // ROI의 점을 Mask하기위해 변경한다.(Polygon점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>();

                        foreach (var p in this.imagePointInfo)
                        {
                            points.Add(new OpenCvSharp.Point((p.X - conLeft), (p.Y - conTop)));
                            points2f.Add(new OpenCvSharp.Point2f(p.X, p.Y));
                        }

                        ListOfListOfPoint.Add(points);

                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);         // 변경된 점으로 Mask영역을 설정한다.
                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);              // 다각형 넓이                        
                        break;
                    }
                case ROISHAPETYPE.Diamond:                                                  // 마름모 모양을 White Mask한다.
                    {
                        #region 각 코너 좌표 구하기
                        var ltX = this.rotatedArea.Points()[1].X - conLeft;                 // (Left , Top) X 좌표
                        var ltY = this.rotatedArea.Points()[1].Y - conTop;                  // (Left , Top) Y 좌표
                        var rtX = this.rotatedArea.Points()[2].X - conLeft;                 // (Right, Top) X 좌표
                        var rtY = this.rotatedArea.Points()[2].Y - conTop;                  // (Right, Top) Y 좌표

                        var lbX = this.rotatedArea.Points()[0].X - conLeft;                 // (Left , Bottom) X 좌표
                        var lbY = this.rotatedArea.Points()[0].Y - conTop;                  // (Left , Bottom) Y 좌표
                        var rbX = this.rotatedArea.Points()[3].X - conLeft;                 // (Right, Bottom) X 좌표
                        var rbY = this.rotatedArea.Points()[3].Y - conTop;                  // (Right, Bottom) Y 좌표                        
                        #endregion

                        // ROI의 점을 Mask하기위해 변경한다(마름모 점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>
                        {
                            new OpenCvSharp.Point((ltX + rtX) / 2, (ltY + rtY) / 2),   // TC
                            new OpenCvSharp.Point((rtX + rbX) / 2, (rtY + rbY) / 2),   // RC
                            new OpenCvSharp.Point((rbX + lbX) / 2, (rbY + lbY) / 2),   // BC
                            new OpenCvSharp.Point((lbX + ltX) / 2, (lbY + ltY) / 2)    // LC
                        };
                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);     // 변경된 점으로 Mask영역을 설정한다.

                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>
                        {
                            new OpenCvSharp.Point((ltX + rtX) / 2, (ltY + rtY) / 2),    // TC
                            new OpenCvSharp.Point((rtX + rbX) / 2, (rtY + rbY) / 2),    // RC
                            new OpenCvSharp.Point((rbX + lbX) / 2, (rbY + lbY) / 2),    // BC
                            new OpenCvSharp.Point((lbX + ltX) / 2, (lbY + ltY) / 2)     // LC
                        };

                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);           // 마름모 넓이
                        break;
                    }
                case ROISHAPETYPE.Rectangle:                                        // Rectangle 모양을 White Mask한다.
                    {
                        #region 각 코너 좌표 구하기
                        var ltX = this.rotatedArea.Points()[1].X - conLeft;                 // (Left , Top) X 좌표
                        var ltY = this.rotatedArea.Points()[1].Y - conTop;                  // (Left , Top) Y 좌표
                        var rtX = this.rotatedArea.Points()[2].X - conLeft;                 // (Right, Top) X 좌표
                        var rtY = this.rotatedArea.Points()[2].Y - conTop;                  // (Right, Top) Y 좌표

                        var lbX = this.rotatedArea.Points()[0].X - conLeft;                 // (Left , Bottom) X 좌표
                        var lbY = this.rotatedArea.Points()[0].Y - conTop;                  // (Left , Bottom) Y 좌표
                        var rbX = this.rotatedArea.Points()[3].X - conLeft;                 // (Right, Bottom) X 좌표
                        var rbY = this.rotatedArea.Points()[3].Y - conTop;                  // (Right, Bottom) Y 좌표                        
                        #endregion

                        // ROI의 점을 Mask하기위해 변경한다(Rectangle 점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>
                        {
                            new OpenCvSharp.Point(ltX, ltY),                // (Left , Top)
                            new OpenCvSharp.Point(rtX, rtY),                // (Right, Top)
                            new OpenCvSharp.Point(rbX, rbY),                // (Right, Bottom)
                            new OpenCvSharp.Point(lbX, lbY)                 // (Left , Bottom)
                        };
                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);     // 변경된 점으로 Mask영역을 설정한다.

                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(this.rotatedArea.Points()); // Rectangle 넓이
                        break;
                    }
                case ROISHAPETYPE.Ellipse:                                         // 타원 모양으로 Mask하는 부분
                    {
                        OpenCvSharp.RotatedRect rotatedRect = this.rotatedArea;
                        rotatedRect.Center.X -= conLeft; rotatedRect.Center.Y -= conTop;
                        Mask.Ellipse(rotatedRect, OpenCvSharp.Scalar.White, -1); // 타원 모양을 White Mask한다.
                        this.ROI_Area = (rotatedRect.Size.Width / 2.0f) * (rotatedRect.Size.Height / 2.0f) * (float)Math.PI; // 타원 넓이
                        break;
                    }
                default: return;
            }

            // roiImage에서 Mask영역에 해당하는 Min, Max 값을 구한다.
            OpenCvSharp.Cv2.MinMaxLoc(roiImage, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc, Mask);
            // roiImage에서 Mask영역에 해당하는 평균, 표준편차 값을 구한다.
            OpenCvSharp.Cv2.MeanStdDev(roiImage, out OpenCvSharp.Scalar mean, out OpenCvSharp.Scalar stdDev, Mask);

            // 구한 ROI 값을 저장한다.
            this.ROI_MinValue = minVal;
            this.ROI_MaxValue = maxVal;
            this.ROI_Average = mean.Val0;
            //this.ROI_Average = (mean.Val0 * this.ROI_Gain) + this.ROI_Offset;
            this.ROI_Sdnn = stdDev.Val0;
        }

        /** @brief: 입력된 Image Mat에 맞게 ROI 도형 영역구하고, Min,Max,평균,표준편차, 넓이구해서 저장한다.
         *  @section: Description
         *      입력된 Image Mat(그래이 영상만 가능하다)에서 ROI 도형의 영역을 구하고,  
         *      this.ROI_MinValue(Min값),this.ROI_MaxValue(Max값),this.ROI_Average(평균값)this.ROI_Sdnn(표준편차값) 구해 저장한다.
         *      ROI의 Min, Max, 평균, 표준편차를 구하는 부분이므로 LinX, LineAngle은 해당되지 않는다.
         *  @param:     roiMat       구할 Image Mat(그래이 영상만 가능하다)
         */
        public void CalROIShareResize(OpenCvSharp.Mat imageMat)
        {
            // ROI의 Min, Max, 평균, 표준편차를 구하는 부분이므로 LinX, LineAngle은 해당되지 않는다.
            if (this.ShapeType == ROISHAPETYPE.LineX || this.ShapeType == ROISHAPETYPE.LineAngle) return;

            // 그래이 영상만 가능하다.
            if (imageMat == null || imageMat.Type() != OpenCvSharp.MatType.CV_8UC1) return;

            // 도형이 아니면
            if (this.IsShape() == false) return;

            int matWidth = imageMat.Width;              // 이미지의 넓이
            int matHeight = imageMat.Height;             // 이미지의 높이

            this.Convert_ROI(matWidth, matHeight, 0, 0, 1.0);

            RectangleF roiAres = this.ShapeAreaResize();                   // ROI를 포함한 최소 영역 가지고 오기

            // roiAres의 값의 예외처리
            float conLeft = roiAres.Left;
            float conTop = roiAres.Top;
            float conWidth = roiAres.Width;
            float conHeight = roiAres.Height;

            if (conLeft < 0) conLeft = 0; if (conTop < 0) conTop = 0; if (conWidth < 0) conWidth = 0; if (conHeight < 0) conHeight = 0;

            if (conLeft > matWidth) conLeft = matWidth; if (conTop > matHeight) conTop = matHeight;
            if (conLeft + conWidth > matWidth) conWidth = matWidth - conLeft;
            if (conTop + conHeight > matHeight) conHeight = matHeight - conTop;

            this.ROI_Width = conWidth;        // ROI가 포함하는 Area 폭
            this.ROI_Height = conHeight;       // ROI가 포함하는 Area 높이

            // 이미지 ROI 영역이다.
            OpenCvSharp.Rect conRect = new OpenCvSharp.Rect((int)conLeft, (int)conTop, (int)conWidth, (int)conHeight);

            // 넓이, 높이가 0보다 작으면 처리 안한다.
            if (conRect.Width <= 0 || conRect.Height <= 0) return;

            // 이미지에서 ROI 영역을 가지고 온다.
            OpenCvSharp.Mat roiImage = imageMat.SubMat(conRect).Clone();

            // 가지온 온 ROI 영역 이미지에서 실제 ROI영역을 Mask 하기위한 변수작업
            OpenCvSharp.Mat Mask = new OpenCvSharp.Mat(new OpenCvSharp.Size(conWidth, conHeight), roiImage.Type());
            Mask.SetTo(OpenCvSharp.Scalar.Black);

            // ROI Type에 따라 실제 ROI영역을 Mask 한다.            
            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Polygon:                                                  // Polygon 모양을 White Mask한다.
                    {
                        // ROI의 점을 Mask하기위해 변경한다.(Polygon점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>();

                        foreach (var p in this.imagePointInfo)
                        {
                            points.Add(new OpenCvSharp.Point((p.X - conLeft), (p.Y - conTop)));
                            points2f.Add(new OpenCvSharp.Point2f(p.X, p.Y));
                        }

                        ListOfListOfPoint.Add(points);

                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);         // 변경된 점으로 Mask영역을 설정한다.
                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);              // 다각형 넓이                        
                        break;
                    }
                case ROISHAPETYPE.Diamond:                                                  // 마름모 모양을 White Mask한다.
                    {
                        #region 각 코너 좌표 구하기
                        var ltX = this.rotatedArea.Points()[1].X - conLeft;                 // (Left , Top) X 좌표
                        var ltY = this.rotatedArea.Points()[1].Y - conTop;                  // (Left , Top) Y 좌표
                        var rtX = this.rotatedArea.Points()[2].X - conLeft;                 // (Right, Top) X 좌표
                        var rtY = this.rotatedArea.Points()[2].Y - conTop;                  // (Right, Top) Y 좌표

                        var lbX = this.rotatedArea.Points()[0].X - conLeft;                 // (Left , Bottom) X 좌표
                        var lbY = this.rotatedArea.Points()[0].Y - conTop;                  // (Left , Bottom) Y 좌표
                        var rbX = this.rotatedArea.Points()[3].X - conLeft;                 // (Right, Bottom) X 좌표
                        var rbY = this.rotatedArea.Points()[3].Y - conTop;                  // (Right, Bottom) Y 좌표                        
                        #endregion

                        // ROI의 점을 Mask하기위해 변경한다(마름모 점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>
                        {
                            new OpenCvSharp.Point((ltX + rtX) / 2, (ltY + rtY) / 2),   // TC
                            new OpenCvSharp.Point((rtX + rbX) / 2, (rtY + rbY) / 2),   // RC
                            new OpenCvSharp.Point((rbX + lbX) / 2, (rbY + lbY) / 2),   // BC
                            new OpenCvSharp.Point((lbX + ltX) / 2, (lbY + ltY) / 2)    // LC
                        };
                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);     // 변경된 점으로 Mask영역을 설정한다.

                        List<OpenCvSharp.Point2f> points2f = new List<OpenCvSharp.Point2f>
                        {
                            new OpenCvSharp.Point((ltX + rtX) / 2, (ltY + rtY) / 2),    // TC
                            new OpenCvSharp.Point((rtX + rbX) / 2, (rtY + rbY) / 2),    // RC
                            new OpenCvSharp.Point((rbX + lbX) / 2, (rbY + lbY) / 2),    // BC
                            new OpenCvSharp.Point((lbX + ltX) / 2, (lbY + ltY) / 2)     // LC
                        };

                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(points2f);           // 마름모 넓이
                        break;
                    }
                case ROISHAPETYPE.Rectangle:                                        // Rectangle 모양을 White Mask한다.
                    {
                        #region 각 코너 좌표 구하기
                        var ltX = this.rotatedArea.Points()[1].X - conLeft;                 // (Left , Top) X 좌표
                        var ltY = this.rotatedArea.Points()[1].Y - conTop;                  // (Left , Top) Y 좌표
                        var rtX = this.rotatedArea.Points()[2].X - conLeft;                 // (Right, Top) X 좌표
                        var rtY = this.rotatedArea.Points()[2].Y - conTop;                  // (Right, Top) Y 좌표

                        var lbX = this.rotatedArea.Points()[0].X - conLeft;                 // (Left , Bottom) X 좌표
                        var lbY = this.rotatedArea.Points()[0].Y - conTop;                  // (Left , Bottom) Y 좌표
                        var rbX = this.rotatedArea.Points()[3].X - conLeft;                 // (Right, Bottom) X 좌표
                        var rbY = this.rotatedArea.Points()[3].Y - conTop;                  // (Right, Bottom) Y 좌표                        
                        #endregion

                        // ROI의 점을 Mask하기위해 변경한다(Rectangle 점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>
                        {
                            new OpenCvSharp.Point(ltX, ltY),                // (Left , Top)
                            new OpenCvSharp.Point(rtX, rtY),                // (Right, Top)
                            new OpenCvSharp.Point(rbX, rbY),                // (Right, Bottom)
                            new OpenCvSharp.Point(lbX, lbY)                 // (Left , Bottom)
                        };
                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);     // 변경된 점으로 Mask영역을 설정한다.

                        this.ROI_Area = OpenCvSharp.Cv2.ContourArea(this.rotatedArea.Points()); // Rectangle 넓이
                        break;
                    }
                case ROISHAPETYPE.Ellipse:                                         // 타원 모양으로 Mask하는 부분
                    {
                        OpenCvSharp.RotatedRect rotatedRect = this.rotatedDrawArea;
                        rotatedRect.Center.X -= conLeft; rotatedRect.Center.Y -= conTop;
                        Mask.Ellipse(rotatedRect, OpenCvSharp.Scalar.White, -1); // 타원 모양을 White Mask한다.
                        this.ROI_Area = (rotatedRect.Size.Width / 2.0f) * (rotatedRect.Size.Height / 2.0f) * (float)Math.PI; // 타원 넓이
                        break;
                    }
                default: return;
            }

            // roiImage에서 Mask영역에 해당하는 Min, Max 값을 구한다.
            OpenCvSharp.Cv2.MinMaxLoc(roiImage, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc, Mask);
            // roiImage에서 Mask영역에 해당하는 평균, 표준편차 값을 구한다.
            OpenCvSharp.Cv2.MeanStdDev(roiImage, out OpenCvSharp.Scalar mean, out OpenCvSharp.Scalar stdDev, Mask);

            // 구한 ROI 값을 저장한다.
            this.ROI_MinValue = minVal;
            this.ROI_MaxValue = maxVal;
            //this.ROI_Average = mean.Val0;
            this.ROI_Average = (mean.Val0 * this.ROI_Gain) + this.ROI_Offset;
            this.ROI_Sdnn = stdDev.Val0;
        }

        /** @brief: 입력된 Image Mat에서 ROI 도형 영역을 마킹하고, 도형의 영역 Mat을 리턴한다.
         *  @section: Description
         *      입력된 Image Mat(그래이 영상만 가능하다)에 ROI 도형의 영역을 검정으로 마킹하고, 도형의 영역에 해당하는 Mat를 리턴한다.         
         *  @param:     imageMat       ROI를 마킹할 Image Mat(그래이 영상만 가능하다)
         *  @param:     IsROIMark      true : 입력된 Image Mat에 ROI 마킹, false: ROI 마킹안함 (검정색)
         *  @return: 도형에 해당하는 영역을 ImageMat에서 추출해서 리턴한다.
         */
        public OpenCvSharp.Mat ROIMark(ref OpenCvSharp.Mat imageMat, bool IsROIMark = false)
        {
            // ROI의 Min, Max, 평균, 표준편차를 구하는 부분이므로 LinX, LineAngle은 해당되지 않는다.
            if (this.ShapeType == ROISHAPETYPE.LineX || this.ShapeType == ROISHAPETYPE.LineAngle) return null;

            // 그래이 영상만 가능하다.
            if (imageMat == null || imageMat.Type() != OpenCvSharp.MatType.CV_8UC1) return null;

            // 도형이 아니면
            if (this.IsShape() == false) return null;

            int matWidth = imageMat.Width;              // 이미지의 넓이
            int matHeight = imageMat.Height;             // 이미지의 높이

            RectangleF roiAres = this.ShapeArea();                   // ROI를 포함한 최소 영역 가지고 오기

            // roiAres의 값은 예외처리
            float conLeft = roiAres.Left;
            float conTop = roiAres.Top;
            float conWidth = roiAres.Width;
            float conHeight = roiAres.Height;

            if (conLeft < 0) conLeft = 0; if (conTop < 0) conTop = 0; if (conWidth < 0) conWidth = 0; if (conHeight < 0) conHeight = 0;

            if (conLeft > matWidth) conLeft = matWidth; if (conTop > matHeight) conTop = matHeight;
            if (conLeft + conWidth > matWidth) conWidth = matWidth - conLeft;
            if (conTop + conHeight > matHeight) conHeight = matHeight - conTop;

            this.ROI_Width = conWidth;        // ROI가 포함하는 Area 폭
            this.ROI_Height = conHeight;       // ROI가 포함하는 Area 높이

            // 이미지 ROI 영역이다.
            OpenCvSharp.Rect conRect = new OpenCvSharp.Rect((int)conLeft, (int)conTop, (int)conWidth, (int)conHeight);

            // 넓이, 높이가 0보다 작으면 처리 안한다.
            if (conRect.Width <= 0 || conRect.Height <= 0) return null;

            // 이미지에서 ROI 영역을 가지고 온다.
            OpenCvSharp.Mat roiImage = imageMat.SubMat(conRect).Clone();

            // 가지온 온 ROI 영역 이미지에서 실제 ROI영역을 Mask 하기위한 변수작업
            OpenCvSharp.Mat Mask = new OpenCvSharp.Mat(new OpenCvSharp.Size(conWidth, conHeight), roiImage.Type());
            Mask.SetTo(OpenCvSharp.Scalar.Black);

            // ROI Type에 따라 실제 ROI영역을 Mask 한다.            
            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Polygon:                                                  // Polygon 모양을 White Mask한다.
                    {
                        // ROI의 점을 Mask하기위해 변경한다.(Polygon점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();

                        List<List<OpenCvSharp.Point>> ListOfListOfPointMark = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> pointsMark = new List<OpenCvSharp.Point>();

                        foreach (var p in this.imagePointInfo)
                        {
                            points.Add(new OpenCvSharp.Point(p.X - conLeft, p.Y - conTop));     // ROI 추출영역 기준
                            if (IsROIMark) pointsMark.Add(new OpenCvSharp.Point(p.X, p.Y));      // 전체 이미지 지준
                        }

                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);         // 변경된 점으로 Mask영역을 설정한다.

                        if (IsROIMark)          // 전체 이미지에 검정으로 Mark한다.
                        {
                            ListOfListOfPointMark.Add(pointsMark);
                            imageMat.FillPoly(ListOfListOfPointMark, OpenCvSharp.Scalar.Black);
                        }

                        break;
                    }
                case ROISHAPETYPE.Diamond:                                                  // 마름모 모양을 White Mask한다.
                    {
                        #region 각 코너 좌표 구하기
                        var ltX = this.rotatedArea.Points()[1].X;                  // (Left , Top) X 좌표
                        var ltY = this.rotatedArea.Points()[1].Y;                  // (Left , Top) Y 좌표
                        var rtX = this.rotatedArea.Points()[2].X;                  // (Right, Top) X 좌표
                        var rtY = this.rotatedArea.Points()[2].Y;                  // (Right, Top) Y 좌표

                        var lbX = this.rotatedArea.Points()[0].X;                  // (Left , Bottom) X 좌표
                        var lbY = this.rotatedArea.Points()[0].Y;                  // (Left , Bottom) Y 좌표
                        var rbX = this.rotatedArea.Points()[3].X;                  // (Right, Bottom) X 좌표
                        var rbY = this.rotatedArea.Points()[3].Y;                  // (Right, Bottom) Y 좌표     

                        var tcX = (ltX + rtX) / 2;                                 // Top Line Center X 좌표
                        var tcY = (ltY + rtY) / 2;                                 // Top Line Center Y 좌표
                        var rcX = (rtX + rbX) / 2;                                 // Right Line Center X 좌표
                        var rcY = (rtY + rbY) / 2;                                 // Right Line Center Y 좌표
                        var bcX = (rbX + lbX) / 2;                                 // Bottom Line Center X 좌표
                        var bcY = (rbY + lbY) / 2;                                 // Bottom  Line Center Y 좌표
                        var lcX = (lbX + ltX) / 2;                                 // Left Line Center X 좌표
                        var lcY = (lbY + ltY) / 2;                                 // Left Line Center Y 좌표
                        #endregion

                        // ROI의 점을 Mask하기위해 변경한다(마름모 점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>
                        {
                            new OpenCvSharp.Point(tcX - conLeft, tcY - conTop),   // TC
                            new OpenCvSharp.Point(rcX - conLeft, rcY - conTop),   // RC
                            new OpenCvSharp.Point(bcX - conLeft, bcY - conTop),   // BC
                            new OpenCvSharp.Point(lcX - conLeft, lcY - conTop)    // LC
                        };
                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);     // 변경된 점으로 Mask영역을 설정한다.

                        if (IsROIMark)          // 전체 이미지에 검정으로 Mark한다.
                        {
                            List<List<OpenCvSharp.Point>> ListOfListOfPointMark = new List<List<OpenCvSharp.Point>>();
                            List<OpenCvSharp.Point> pointsMark = new List<OpenCvSharp.Point>
                            {
                                new OpenCvSharp.Point(tcX, tcY),   // TC
                                new OpenCvSharp.Point(rcX, rcY),   // RC
                                new OpenCvSharp.Point(bcX, bcY),   // BC
                                new OpenCvSharp.Point(lcX, lcY)    // LC
                            };

                            ListOfListOfPointMark.Add(pointsMark);
                            imageMat.FillPoly(ListOfListOfPointMark, OpenCvSharp.Scalar.Black);
                        }
                        break;
                    }
                case ROISHAPETYPE.Rectangle:                                        // Rectangle 모양을 White Mask한다.
                    {
                        #region 각 코너 좌표 구하기
                        var ltX = this.rotatedArea.Points()[1].X;           // (Left , Top) X 좌표
                        var ltY = this.rotatedArea.Points()[1].Y;           // (Left , Top) Y 좌표
                        var rtX = this.rotatedArea.Points()[2].X;           // (Right, Top) X 좌표
                        var rtY = this.rotatedArea.Points()[2].Y;           // (Right, Top) Y 좌표

                        var lbX = this.rotatedArea.Points()[0].X;           // (Left , Bottom) X 좌표
                        var lbY = this.rotatedArea.Points()[0].Y;           // (Left , Bottom) Y 좌표
                        var rbX = this.rotatedArea.Points()[3].X;           // (Right, Bottom) X 좌표
                        var rbY = this.rotatedArea.Points()[3].Y;           // (Right, Bottom) Y 좌표                        
                        #endregion

                        // ROI의 점을 Mask하기위해 변경한다(Rectangle 점으로 변경)
                        List<List<OpenCvSharp.Point>> ListOfListOfPoint = new List<List<OpenCvSharp.Point>>();
                        List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>
                        {
                            new OpenCvSharp.Point(ltX - conLeft, ltY - conTop),                // (Left , Top)
                            new OpenCvSharp.Point(rtX - conLeft, rtY - conTop),                // (Right, Top)
                            new OpenCvSharp.Point(rbX - conLeft, rbY - conTop),                // (Right, Bottom)
                            new OpenCvSharp.Point(lbX - conLeft, lbY - conTop)                 // (Left , Bottom)
                        };
                        ListOfListOfPoint.Add(points);
                        Mask.FillPoly(ListOfListOfPoint, OpenCvSharp.Scalar.White);     // 변경된 점으로 Mask영역을 설정한다.

                        if (IsROIMark)          // 전체 이미지에 검정으로 Mark한다.
                        {
                            List<List<OpenCvSharp.Point>> ListOfListOfPointMark = new List<List<OpenCvSharp.Point>>();
                            List<OpenCvSharp.Point> pointsMark = new List<OpenCvSharp.Point>
                            {
                                new OpenCvSharp.Point(ltX, ltY),                                // (Left , Top)
                                new OpenCvSharp.Point(rtX, rtY),                                // (Right, Top)
                                new OpenCvSharp.Point(rbX, rbY),                                // (Right, Bottom)
                                new OpenCvSharp.Point(lbX, lbY)                                 // (Left , Bottom)
                            };

                            ListOfListOfPointMark.Add(pointsMark);
                            imageMat.FillPoly(ListOfListOfPointMark, OpenCvSharp.Scalar.Black);
                        }

                        break;
                    }
                case ROISHAPETYPE.Ellipse:                                         // 타원 모양으로 Mask하는 부분
                    {
                        OpenCvSharp.RotatedRect rotatedRect = this.rotatedArea;
                        rotatedRect.Center.X -= conLeft; rotatedRect.Center.Y -= conTop;
                        Mask.Ellipse(rotatedRect, OpenCvSharp.Scalar.White, -1); // 타원 모양을 White Mask한다. 

                        if (IsROIMark)          // 전체 이미지에 검정으로 Mark한다.
                        {
                            imageMat.Ellipse(this.rotatedArea, OpenCvSharp.Scalar.Black, -1); // 타원 모양을 White Mask한다. 
                        }

                        break;
                    }
                default: return null;
            }

            OpenCvSharp.Mat dst = new OpenCvSharp.Mat();
            roiImage.CopyTo(dst, Mask);

            return dst;
        }

    #endregion CalROIShare, Rotation, Mirror, Flip, AllImagePoint, MoveTo, InsidePolygon, DoesRectangleContainPoint, RotatedAreaOffset, Offset 

    #region 마우스 위치에 따른 Node 선택 함수, 마우스 커서 모양 리턴 함수

        /** @brief: 도형이 그려진 영역에서 Point가 포함된 위치
         *  @section: Description
         *      그려진 도형에서 마우스 위치가 포함된 위치 구한다.
         *  @param:     mouse_pt    원본 이미지 기준의 마우스 점( System.Drawing.PointF 변수 )
         *  @return:    포함된 위치를 리턴한다.(ROISHAPEPOSTION 형식)
         */
        public ROISHAPEPOSTION GetNodeSelectable(PointF mouse_pt)
        {
            this.NodeSelected = ROISHAPEPOSTION.None;

            if (!IsShape()) return ROISHAPEPOSTION.None;

            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Rectangle:
                case ROISHAPETYPE.Diamond:
                case ROISHAPETYPE.Ellipse:
                    {
                        var ltX = this.rotatedArea.Points()[1].X;
                        var ltY = this.rotatedArea.Points()[1].Y;
                        var rtX = this.rotatedArea.Points()[2].X;
                        var rtY = this.rotatedArea.Points()[2].Y;

                        var lbX = this.rotatedArea.Points()[0].X;
                        var lbY = this.rotatedArea.Points()[0].Y;
                        var rbX = this.rotatedArea.Points()[3].X;
                        var rbY = this.rotatedArea.Points()[3].Y;


                        PointF pointLT = new PointF(ltX, ltY);
                        PointF pointLB = new PointF(lbX, lbY);
                        PointF pointRT = new PointF(rtX, rtY);
                        PointF pointRB = new PointF(rbX, rbY);

                        var ctX = (ltX + rtX) / 2;
                        var ctY = (ltY + rtY) / 2;

                        PointF pointRP = ImageUsedMath.FindDistancePoint(new System.Drawing.PointF(ctX, ctY), object_radius * 4, this.rotatedDrawArea.Angle - 90); // 회전점

                        if (ImageUsedMath.DistanceToPoint(mouse_pt, pointLT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftTop;            //  Left,    Top 점 선택
                        else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointLB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftBottom;         //  Left, Bottom 점 선택
                        else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightTop;           // Right,    Top 점 선택
                        else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightBottom;        // Right, Bottom 점 선택

                        //else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRP) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Rotation;               // 위쪽 회전             
                        //else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, new PointF(this.rotatedArea.Center.X, this.rotatedArea.Center.Y), pointRP) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Rotation;               // 위쪽 회전 

                        else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointLT, pointLB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftMiddle;
                        else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointLB, pointRB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaBottomMiddle;
                        else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointRB, pointRT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightMiddle;
                        else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointRT, pointLT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaTopMiddle;

                        else if (DoesRectangleContainPoint(this.rotatedArea, mouse_pt)) this.NodeSelected = ROISHAPEPOSTION.Inside;

                        break;
                    }
                case ROISHAPETYPE.LineAngle:
                case ROISHAPETYPE.LineX:
                case ROISHAPETYPE.Polygon:
                    {
                        this.movePointIndex = -1;        // 이동 Point 초기화
                        for (int Index = 0; Index < this.imagePointInfo.Count; Index++)
                        {
                            if (ImageUsedMath.DistanceToPoint(mouse_pt, this.imagePointInfo[Index]) < over_dist_squared)
                            {
                                this.NodeSelected = ROISHAPEPOSTION.HitPoint;
                                this.movePointIndex = Index;
                                return this.NodeSelected;
                            }
                        }

                        if (this.ShapeType == ROISHAPETYPE.Polygon && this.yMinPointIndex >= 0 && this.yMinPointIndex < this.imagePointInfo.Count)
                        {
                            if (!float.IsNaN(this.polygonCenter.X) && !float.IsNaN(this.polygonCenter.Y))       //// Polygen의 중심점을 구한경우
                            {
                                // Polygen의 중심점과 YMinPoint와의 거리에 object_radius * 4를 더한값이 회전점의 거리
                                float distanceRP = ImageUsedMath.DistanceToPoint(this.polygonCenter, new PointF(this.imagePointInfo[this.yMinPointIndex].X, this.imagePointInfo[this.yMinPointIndex].Y)) + object_radius * 4;
                                // 회전점을 구한다.
                                PointF pointRP = ImageUsedMath.FindDistancePoint(this.polygonCenter, distanceRP, this.polygonAngle - 90);

                                if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRP) < over_dist_squared || ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, this.polygonCenter, pointRP) < over_dist_squared)
                                    this.NodeSelected = ROISHAPEPOSTION.Rotation;               // 위쪽 회전  
                            }
                        }
                        break;
                    }
            }

            #region Polygon, LineX, LineAngle 인 경우 Inside 처리부분
            if (this.NodeSelected == ROISHAPEPOSTION.None)
            {
                if (this.ShapeType == ROISHAPETYPE.Polygon && this.InsidePolygon(mouse_pt))
                    this.NodeSelected = ROISHAPEPOSTION.Inside;
                else if (this.ShapeType == ROISHAPETYPE.LineAngle && this.imagePointInfo.Count == 3)
                {
                    PointF centerPoint = this.imagePointInfo[0];    // 0번째 포이트가 중심
                    PointF anglePoint = this.imagePointInfo[1];
                    PointF basePoint = this.imagePointInfo[2];
                    if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, anglePoint, centerPoint) < (float)over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                    else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, basePoint, centerPoint) < (float)over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                }
                else if (this.ShapeType == ROISHAPETYPE.LineX && this.imagePointInfo.Count == 4)
                {
                    if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, this.imagePointInfo[0], this.imagePointInfo[1]) < (float)over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                    else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, this.imagePointInfo[2], this.imagePointInfo[3]) < (float)over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                }
            }
            #endregion

            return this.NodeSelected;
        }

        /** @brief: ROI의 Node 선택에 따른 커서 모양 리턴한다.
         *  @section: Description
         *      GetNodeSelectable 함수를 실행해야 된다. ROI의 Node 선택에 따른 커서 모양 리턴한다.
         *  @return:    커서 모양 리턴한다.
         */
        public Cursor GetCursor()
        {
            switch (this.NodeSelected)
            {
                case ROISHAPEPOSTION.AreaLeftMiddle:
                case ROISHAPEPOSTION.AreaRightMiddle:
                    return Cursors.SizeWE;

                case ROISHAPEPOSTION.AreaBottomMiddle:
                case ROISHAPEPOSTION.AreaTopMiddle:
                    return Cursors.SizeNS;

                case ROISHAPEPOSTION.Rotation:           // 위쪽 회전  
                    return Cursors.Hand;

                case ROISHAPEPOSTION.AreaLeftTop:
                case ROISHAPEPOSTION.AreaRightBottom:
                case ROISHAPEPOSTION.AreaLeftBottom:
                case ROISHAPEPOSTION.AreaRightTop:
                case ROISHAPEPOSTION.HitPoint:
                    return Cursors.Arrow;

                case ROISHAPEPOSTION.Inside:
                    return Cursors.SizeAll;
                default:
                    return Cursors.Cross;
            }
        }

        /** @brief: 주어진 Rect안에 ROI의 점이 포함되었는지 확인
         *  @section: Description
         *      주어진 Rect안에 ROI의 점이 하나라도 포함되거나, ROI와 Rect가 겹치는지 확인한다.
         *  @param:     rect   ROI의 점이 포함되는지 확인하는 Rect(RectangleF 변수)
         *  @return:    주어진 Rect안에 ROI의 점이 하나라도 포함되거나, ROI와 Rect가 겹치면 true, 없으면 false
         */
        public bool Contains(RectangleF rect)
        {
            // LineAngle, LineX는 포함안함.
            if (this.ShapeType == ROISHAPETYPE.LineAngle || this.ShapeType == ROISHAPETYPE.LineX) return false;

            // 선택 영역에 ROI의 점이 하나라도 포함되었는지 확인
            foreach (var point in this.imagePointInfo) if (rect.Contains(point)) return true;

            // 선택영역에 ROI의 점이 포함되지 않았으면 선택영역의 꼭지점이 ROI내부에 있는지 확인한다.

            var ltX = this.rotatedArea.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = this.rotatedArea.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = this.rotatedArea.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = this.rotatedArea.Points()[2].Y;           // (Right, Top) Y 좌표

            var lbX = this.rotatedArea.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = this.rotatedArea.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = this.rotatedArea.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = this.rotatedArea.Points()[3].Y;           // (Right, Bottom) Y 좌표

            PointF pointLT = new PointF(ltX, ltY);
            PointF pointLB = new PointF(lbX, lbY);
            PointF pointRT = new PointF(rtX, rtY);
            PointF pointRB = new PointF(rbX, rbY);

            PointF[] polyPoints = { pointLT, pointRT, pointRB, pointLB };

            // 다각형인 경우
            if (this.ShapeType == ROISHAPETYPE.Polygon) polyPoints = this.imagePointInfo.ToArray();

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(polyPoints);

            PointF rectLT = new PointF(rect.Left, rect.Top);
            PointF rectLB = new PointF(rect.Left, rect.Bottom);
            PointF rectRT = new PointF(rect.Right, rect.Top);
            PointF rectRB = new PointF(rect.Right, rect.Bottom);

            // ROI에 선택 영역이 들어간 경우 Check
            if (path.IsVisible(rectLT)) return true;
            if (path.IsVisible(rectLB)) return true;
            if (path.IsVisible(rectRT)) return true;
            if (path.IsVisible(rectRB)) return true;

            // ROI와 선택 영영이 크로스 되는 경우 Check
            for (int index = 0; index < polyPoints.Length; index++)
            {
                PointF startP = polyPoints[index];
                PointF endP = polyPoints[(index + 1) % polyPoints.Length];

                if (ImageUsedMath.GetIntersectPoint(startP, endP, rectLT, rectRT, out _)) return true;
                if (ImageUsedMath.GetIntersectPoint(startP, endP, rectRT, rectRB, out _)) return true;
                if (ImageUsedMath.GetIntersectPoint(startP, endP, rectRB, rectLB, out _)) return true;
                if (ImageUsedMath.GetIntersectPoint(startP, endP, rectLB, rectLT, out _)) return true;
            }


            return false;
        }

    #endregion 마우스 위치에 따른 Node 선택 함수, 마우스 커서 모양 리턴 함수

    #region ROI 그리기 함수     

        /** @brief: ROI의 Image Point 정보를 size(picWidth, picHeight)로 변경한다.
         *  @section: Description
         *      ROI의 점 정보가 원본 이미지 정보기준의 point 정보를 그리기 위한 Point 정보로 변경한다. zoom과 pan도 적용한다.
         *  @param:     picWidth    그려질 위치의 width(int 형)
         *  @param:     picHeight   그려질 위치의 height(int 형)
         *  @param:     subMatLeft  Pan시 Left 위치
         *  @param:     subMatTop   Pan시 Right 위치
         *  @param:     zoom_Ratio  Zoom 배율 값
         */
        public void Convert_ROI(int picWidth, int picHeight, int subMatLeft = 0, int subMatTop = 0, double zoom_Ratio = 1.0)
        {
            double imageWidth = this.Image_Width;      // ROI의 기준 Image Width
            double imageHeight = this.Image_Height;     // ROI의 기준 Image Height

            // 도형이 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우에 rotatedArea 다시 만들지 or center를 이동할 지 확인
            if (this.ShapeType == ROISHAPETYPE.Rectangle || this.ShapeType == ROISHAPETYPE.Diamond || this.ShapeType == ROISHAPETYPE.Ellipse)
            {
                float centerX = (float)((this.rotatedArea.Center.X * zoom_Ratio - subMatLeft) * picWidth / imageWidth);                      // PictureBox의 기준(zoom, pan 포함)으로 x 값을 변경한다.
                float centerY = (float)((this.rotatedArea.Center.Y * zoom_Ratio - subMatTop) * picHeight / imageHeight);                     // PictureBox의 기준(zoom, pan 포함)으로 y 값을 변경한다.
                float width = (float)((this.rotatedArea.Size.Width * zoom_Ratio) * picHeight / imageHeight);                     // PictureBox의 기준(zoom, pan 포함)으로 width 값을 변경한다.
                float height = (float)((this.rotatedArea.Size.Height * zoom_Ratio) * picHeight / imageHeight);                     // PictureBox의 기준(zoom, pan 포함)으로 height 값을 변경한다.

                float angle = this.rotatedArea.Angle;

                // 그리기 도형을 만든다.
                this.rotatedDrawArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), angle);

                var ltX = this.rotatedDrawArea.Points()[1].X;
                var ltY = this.rotatedDrawArea.Points()[1].Y;

                var rbX = this.rotatedDrawArea.Points()[3].X;
                var rbY = this.rotatedDrawArea.Points()[3].Y;

                this.PointsInfo.Clear();
                this.PointsInfo.Add(new Point(Convert.ToInt32(ltX), Convert.ToInt32(ltY)));
                this.PointsInfo.Add(new Point(Convert.ToInt32(rbX), Convert.ToInt32(rbY)));
            }
            else
            {
                this.PointsInfo.Clear();

                foreach (var roiPoint in this.imagePointInfo)
                {
                    int conX = Convert.ToInt32((roiPoint.X * zoom_Ratio - subMatLeft) * picWidth / imageWidth);                      // PictureBox의 기준(zoom, pan 포함)으로 x 값을 변경한다.
                    int conY = Convert.ToInt32((roiPoint.Y * zoom_Ratio - subMatTop) * picHeight / imageHeight);                     // PictureBox의 기준(zoom, pan 포함)으로 y 값을 변경한다.

                    this.PointsInfo.Add(new Point(conX, conY));
                }
            }

            float fontsize = Math.Min((float)(this.Font.Size * zoom_Ratio * picWidth / imageWidth), (float)(this.Font.Size * zoom_Ratio * picHeight / picHeight));

            //this.DrawFont = new Font(this.Font.FontFamily, fontsize, this.Font.Style);
            this.DrawFont = new Font(this.Font.FontFamily, fontsize, FontStyle.Bold);
        }

        /** @brief: ROI를 그리는 함수 
         *  @section: Description
         *      ROI의 점 정보가 원본 이미지 정보기 때문에 그려질 width, height정보가 필요하다.
         *  @param:     g                   그려질 위치의 GDI+ 변수
         *  @param:     picWidth            그려질 위치의 width(int 형)
         *  @param:     picHeight           그려질 위치의 height(int 형)
         *  @param:     subMatLeft          Pan시 Left 위치
         *  @param:     subMatTop           Pan시 Right 위치
         *  @param:     zoom_Ratio          Zoom 배율 값
         *  @param:     showAvg             true이면 ROI의 평균값을 보여준다. 기본을 false이다.
         *  @param:     showDiff            true이면 연결된 ROI와 평균값 차이를 보여준다( Subindex : 2인경우만). 기본을 false이다.
         *  @param:     IsFill              ROI 내부를 색을 채울지 설정
         *  @param:     IsRotatedPoint      회전점을 그릴지 선택
         */
        public void Draw(Graphics g, int picWidth, int picHeight, int subMatLeft = 0, int subMatTop = 0, double zoom_Ratio = 1.0, bool showAvg = false, bool showDiff = false, bool IsFill = false, bool IsRotatedPoint = false, bool IsPrint = false)
        {
            // ROI를 그리는 점으로 변경한다.  
            this.Convert_ROI(picWidth, picHeight, subMatLeft, subMatTop, zoom_Ratio);

            if (this.PointsInfo.Count < 2) return;         // 도형은 경우만 그린다.

            if (g == null) return;
            bool IsGuideLine = false;
            Pen p = new Pen(this.BorderColor, Global.Roi_BorderWidth);

            // ROI가 선택되었을 경우 테두리 색을 선택테두리 색으로 변경 단, 이미지 출력 및 Dicom 전송인 경우는 색 변경 안함.
            if (!IsPrint && this.Selected) p.Color = this.SelectedBorderColor;

            // ROI에 마우스 포인트가 있을 경우 GuideLine을 그린다.
            if (this.NodeSelected != ROISHAPEPOSTION.None) IsGuideLine = true;

            // 이미지 출력인 경우 GuideLine 안그린다.
            if (IsPrint) IsGuideLine = false;

            switch (this._shapeType)
            {
                case ROISHAPETYPE.Rectangle:
                    this.DrawRectangle(g, picWidth, picHeight, p, showAvg, showDiff, IsGuideLine, IsFill, IsRotatedPoint);
                    break;
                case ROISHAPETYPE.Ellipse:
                    this.DrawEllipse(g, picWidth, picHeight, p, showAvg, showDiff, IsGuideLine, IsFill, IsRotatedPoint);
                    break;
                case ROISHAPETYPE.Diamond:
                    this.DrawDiamond(g, picWidth, picHeight, p, showAvg, showDiff, IsGuideLine, IsFill, IsRotatedPoint);
                    break;
                case ROISHAPETYPE.Polygon:
                    this.DrawPolygon(g, picWidth, picHeight, p, showAvg, showDiff, IsGuideLine, IsFill, IsRotatedPoint);
                    break;
                case ROISHAPETYPE.LineAngle:
                    this.DrawAngle(g, p, IsGuideLine);
                    break;
                case ROISHAPETYPE.LineX:
                    this.DrawXMark(g, p, IsGuideLine);
                    break;
            }
        }

        /** @brief: 점을 그리는 함수
         *  @section: Description
         *      그려질 위치 기준점이다. radius의 크기의 원을 그린다.
         *  @param:     g           그려질 위치의 GDI+ 변수
         *  @param:     c           점의 색
         *  @param:     corner      그려질 위치 점
         *  @param:     radius      점의 크기         
         */
        // 점 그리기
        private void DrawPoint(Graphics g, Color c, Point corner, int radius)
        {
            Rectangle rect = new Rectangle(corner.X - radius, corner.Y - radius, 2 * radius, 2 * radius);
            g.FillEllipse(new SolidBrush(c), rect);
        }

        /** @brief: X 가이드 라인 그리기
         *  @section: Description
         *      그려질 위치 기준점이다. X 가이드 라인 그리기
         *  @param:     g           그려질 위치의 GDI+ 변수
         *  @param:     pen         그릴 pen
         *  @param:     IsGuideLine GuideLien을 그릴지 여부
         */
        private void DrawXMark(Graphics g, Pen pen, bool IsGuideLine = false)
        {
            if (this.PointsInfo.Count < 2) return;

            // Back 이미지 그리기
            Point BackPoint0 = this.PointsInfo[0]; BackPoint0.Offset(-1, -1);   // 0번째 포인트는 중심점
            Point BackPoint1 = this.PointsInfo[1]; BackPoint1.Offset(-1, -1);   // 1번째 포인트는 각도점

            g.DrawLine(new Pen(this.BorderBackColor, Global.Roi_BorderWidth), BackPoint0, BackPoint1);
            if (this.PointsInfo.Count == 4)
            {
                Point BackPoint2 = this.PointsInfo[2]; BackPoint2.Offset(-1, -1);   // 0번째 포인트는 중심점
                Point BackPoint3 = this.PointsInfo[3]; BackPoint3.Offset(-1, -1);   // 1번째 포인트는 각도점
                g.DrawLine(new Pen(this.BorderBackColor, Global.Roi_BorderWidth), BackPoint2, BackPoint3);
            }

            // 원래 이미지 그리기
            g.DrawLine(pen, this.PointsInfo[0], this.PointsInfo[1]);
            if (this.PointsInfo.Count == 4)
                g.DrawLine(pen, this.PointsInfo[2], this.PointsInfo[3]);

            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                g.DrawLine(pen, this.PointsInfo[0], this.PointsInfo[1]);
                this.DrawPoint(g, this.GuidePointColor, this.PointsInfo[0], object_radius);
                this.DrawPoint(g, this.GuidePointColor, this.PointsInfo[1], object_radius);
                if (this.PointsInfo.Count == 4)
                {
                    g.DrawLine(pen, this.PointsInfo[2], this.PointsInfo[3]);
                    this.DrawPoint(g, this.GuidePointColor, this.PointsInfo[2], object_radius);
                    this.DrawPoint(g, this.GuidePointColor, this.PointsInfo[3], object_radius);
                }
            }




            String strIndex = "";
            if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
            if (strIndex.Length > 0)
            {
                var minX = this.PointsInfo.Min(p => p.X);
                var minY = this.PointsInfo.Min(p => p.Y);
                var maxX = this.PointsInfo.Max(p => p.X);
                var maxY = this.PointsInfo.Max(p => p.Y);

                var centerX = (maxX + minX) / 2;
                var centerY = (maxY + minY) / 2;
                int index = this.PointsInfo.FindIndex(p => (p.X > centerX && p.Y > centerY));

                Point textPoint = new Point(maxX, maxY);
                if (index >= 0 && index < this.PointsInfo.Count) textPoint = this.PointsInfo[index];

                // Back 글자
                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeBackColor), new Point(textPoint.X - 1, textPoint.Y - 1));
                // 원래 글자
                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeColor), new Point(textPoint.X, textPoint.Y));
            }
        }

        /** @brief: 각도 가이드 라인 그리기
         *  @section: Description
         *      그려질 위치 기준점이다. 각도 가이드 라인 그리기
         *  @param:     g           그려질 위치의 GDI+ 변수
         *  @param:     pen         그릴 pen
         *  @param:     IsGuideLine GuideLien을 그릴지 여부
         */
        private void DrawAngle(Graphics g, Pen pen, bool IsGuideLine = false)
        {
            if (this.PointsInfo.Count != 3) return;

            Point centerPoint = this.PointsInfo[0];         // 0번째 포인트는 중심점
            Point anglePoint = this.PointsInfo[1];         // 1번째 포인트는 각도점
            Point basePoint = this.PointsInfo[2];         // 2번째 포인트는 배이스점

            // Back 이미지 그리기
            Point centerBackPoint = centerPoint; centerBackPoint.Offset(-1, -1);     // 0번째 포인트는 중심점
            Point angleBackPoint = anglePoint; angleBackPoint.Offset(-1, -1);      // 1번째 포인트는 각도점
            Point baseBackPoint = basePoint; baseBackPoint.Offset(-1, -1);       // 2번째 포인트는 배이스점
            g.DrawLines(new Pen(this.BorderBackColor, Global.Roi_BorderWidth), new Point[] { angleBackPoint, centerBackPoint, baseBackPoint });

            // 원래 이미지 그리기
            g.DrawLines(pen, new Point[] { anglePoint, centerPoint, basePoint });

            if (IsGuideLine)    // 가이드 라인을 그릴 것인지.
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                // 가이드 라인 그린다.
                g.DrawLines(pen, new Point[] { anglePoint, centerPoint, basePoint });
                // 가이드 점 그린다.
                this.DrawPoint(g, this.GuidePointColor, anglePoint, object_radius);
                this.DrawPoint(g, this.GuidePointColor, centerPoint, object_radius);
                this.DrawPoint(g, this.GuidePointColor, basePoint, object_radius);
            }

            String strIndex = "";
            // angle은 0 ~ 90도값이다. realAngle은 0 ~ 360도 값이다.            
            float angle = ImageUsedMath.AngleBetween(centerPoint, anglePoint, out float realAngle);
            if (this.imagePointInfo.Count == 3) angle = ImageUsedMath.AngleBetween(this.imagePointInfo[0], this.imagePointInfo[1], out realAngle);

            // angle은 0 ~ 45도로 표시한다
            if (angle > 45) angle = 90 - angle;
            // Main Index가 설정되면 각도를 표시한다.
            if (this.ROI_MainIndex > 0) strIndex += String.Format("{0} ({1:#.0}°)", this.ROI_MainIndex, angle);
            // 처음 그리는 동안은 각도를 표시하지 않는다.
            if (strIndex.Length > 0)
            {
                // 각도표시 위치 설정 부분( center Point 기준으로 위치 결정한다.)
                bool IsRectUp = false; bool IsRectLeft = false;
                StringFormat strIndexFormat = new StringFormat() { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Near };
                if (realAngle > 0 && realAngle <= 45) { IsRectUp = true; IsRectLeft = false; }
                else if (realAngle > 45 && realAngle <= 90) { IsRectUp = false; IsRectLeft = true; }
                else if (realAngle > 90 && realAngle <= 135) { IsRectUp = false; IsRectLeft = false; }
                else if (realAngle > 135 && realAngle <= 180) { IsRectUp = true; IsRectLeft = true; }
                else if (realAngle > 180 && realAngle <= 225) { IsRectUp = false; IsRectLeft = true; }
                else if (realAngle > 225 && realAngle <= 270) { IsRectUp = true; IsRectLeft = false; }
                else if (realAngle > 270 && realAngle <= 315) { IsRectUp = true; IsRectLeft = true; }
                else if (realAngle > 315 && realAngle <= 360) { IsRectUp = false; IsRectLeft = false; }

                Rectangle AngleTextRect = new Rectangle(centerPoint.X, centerPoint.Y + 1, (int)(this.DrawFont.SizeInPoints * strIndex.Length), this.DrawFont.Height);

                if (IsRectUp) AngleTextRect.Offset(0, (-1 * AngleTextRect.Height));
                if (IsRectLeft) { AngleTextRect.Offset((-1 * AngleTextRect.Width), 0); strIndexFormat.Alignment = StringAlignment.Far; }

                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자

                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
        }

        /** @brief: Polygon 그리기
         *  @section: Description
         *      그려질 위치 기준점이다. Polygon 그리기
         *  @param:     picWidth            그려질 이미지의 넓이
         *  @param:     picHeight           그려질 이미지의 높이
         *  @param:     g           그려질 위치의 GDI+ 변수
         *  @param:     pen         그릴 pen
         *  @param:     showAvg     true이면 ROI의 평균값을 보여준다. 기본을 false이다.
         *  @param:     showDiff    true이면 연결된 ROI와 평균값 차이를 보여준다( Subindex : 2인경우만). 기본을 false이다.
         *  @param:     IsGuideLine GuideLien을 그릴지 여부
         *  @param:     IsFill      ROI 내부를 색을 채울지 설정
        */
        private void DrawPolygon(Graphics g, int picWidth, int picHeight, Pen pen, bool showAvg, bool showDiff = false, bool IsGuideLine = false, bool IsFill = false, bool IsRotatedPoint = false)
        {
            List<Point> backPointInfo = new List<Point>();

            //  가장  Y축이 큰 점에 쓴다.
            Point textPoint = this.PointsInfo[0];

            foreach (Point corner in this.PointsInfo)
            {
                backPointInfo.Add(new Point(corner.X - 1, corner.Y - 1));
                if (textPoint.Y < corner.Y) textPoint = corner;
            }

            if (IsFill) g.FillPolygon(new SolidBrush(this.FilledColor), this.PointsInfo.ToArray());
            else g.DrawPolygon(new Pen(this.BorderBackColor, Global.Roi_BorderWidth), backPointInfo.ToArray());// Back 이미지 그리기

            // 원래 이미지 그리기
            g.DrawPolygon(pen, this.PointsInfo.ToArray());

            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                g.DrawPolygon(pen, this.PointsInfo.ToArray());

                foreach (Point corner in this.PointsInfo) this.DrawPoint(g, this.GuidePointColor, corner, object_radius);

                #region RotatedRect의 회전 점을 그린다.
                if (IsRotatedPoint && (this.yMinPointIndex >= 0 && this.yMinPointIndex < this.PointsInfo.Count))
                {
                    // Polygon 중심점 구하기
                    List<OpenCvSharp.Point> polygon = new List<OpenCvSharp.Point>();
                    foreach (var roiPoint in this.PointsInfo) polygon.Add(new OpenCvSharp.Point(roiPoint.X, roiPoint.Y));
                    var moments = OpenCvSharp.Cv2.Moments(polygon, false);
                    PointF AreaCenter = new PointF((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));

                    if (!float.IsNaN(polygonCenter.X) && !float.IsNaN(polygonCenter.Y))   // Polygen의 중심점을 구했으면
                    {
                        // Polygen의 중심점과 YMinPoint와의 거리에 object_radius * 4를 더한값이 회전점의 거리
                        float distanceRP = ImageUsedMath.DistanceToPoint(AreaCenter, new PointF(this.PointsInfo[this.yMinPointIndex].X, this.PointsInfo[this.yMinPointIndex].Y)) + object_radius * 4;
                        // 회전점을 구한다.
                        PointF pointRP = ImageUsedMath.FindDistancePoint(AreaCenter, distanceRP, this.polygonAngle - 90);

                        Point pointCT = new Point(Convert.ToInt32(AreaCenter.X), Convert.ToInt32(AreaCenter.Y));
                        Point drawRP = new Point(Convert.ToInt32(pointRP.X), Convert.ToInt32(pointRP.Y));
                        // Polgon의 중심점과 회전점을 GuigeLine으로 연결
                        g.DrawLine(pen, pointCT, drawRP);

                        // 회전점 그린다.
                        this.DrawPoint(g, Color.Green, drawRP, object_radius + 2);
                        this.DrawPoint(g, Color.LightGreen, drawRP, object_radius);

                        // Polgon의 중심점 그린다.
                        this.DrawPoint(g, Color.Green, pointCT, object_radius + 2);
                        this.DrawPoint(g, Color.LightGreen, pointCT, object_radius);
                    }
                }
                #endregion
            }

            // ROI가 포함된 영역을 구한다.            
            String strIndex = "";
            if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
            if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            if (showAvg)
            {
                double roiAverage = Math.Truncate((this.ROI_Average * (Global.HighTemperature - Global.LowTemperature) / 255 + Global.LowTemperature) * 10) / 10;

                strIndex += String.Format("({0:#.0})", roiAverage);
                if (showDiff && this.ROI_SubIndex == 2)// && this.ROI_Diff != 0)    // ROI_Diff 0도 표시
                {
                    strIndex += String.Format(", {0:0.0}", this.ROI_Diff);
                }
            }
            if (strIndex.Length > 0)
            {
                // 그려질 글씨 길이 구하기
                SizeF sz = g.MeasureString(strIndex, this.DrawFont);

                // 이미지 밖으로 ROI 글자가 나가는 것 방지 코드
                int drawX = textPoint.X;
                int drawY = textPoint.Y + 5;
                if ((int)(drawX + sz.Width) > picWidth) drawX = (int)(picWidth - sz.Width);
                if ((int)(drawY + sz.Height) > picHeight) drawY = (int)(picHeight - sz.Height);

                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeBackColor), new Point(drawX - 1, drawY - 1)); // Back 글자
                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeColor), new Point(drawX, drawY));         // 원래 글자
            }
        }

        /** @brief: Ellipse 그리기
         *  @section: Description
         *      Ellipse 그리기 위해 rotatedArea -> rotatedDrawArea로 변경해서 사용한다.
         *  @param:     picWidth            그려질 이미지의 넓이
         *  @param:     picHeight           그려질 이미지의 높이
         *  @param:     g                   그려질 위치의 GDI+ 변수
         *  @param:     pen                 그릴 pen
         *  @param:     showAvg             true이면 ROI의 평균값을 보여준다. 기본을 false이다.
         *  @param:     showDiff            true이면 연결된 ROI와 평균값 차이를 보여준다( Subindex : 2인경우만). 기본을 false이다.
         *  @param:     IsGuideLine         GuideLien을 그릴지 여부
         *  @param:     IsFill              ROI 내부를 색을 채울지 설정
         *  @param:     IsRotatedPoint      회전점을 그릴지 선택
        */
        private void DrawEllipse(Graphics g, int picWidth, int picHeight, Pen pen, bool showAvg, bool showDiff = false, bool IsGuideLine = false, bool IsFill = false, bool IsRotatedPoint = false)
        {
            if ((this.rotatedDrawArea.Size.Width * this.rotatedDrawArea.Size.Height) < 10) return;

            #region 각 좌표 구하기
            var ltX = this.rotatedDrawArea.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = this.rotatedDrawArea.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = this.rotatedDrawArea.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = this.rotatedDrawArea.Points()[2].Y;           // (Right, Top) Y 좌표

            var lbX = this.rotatedDrawArea.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = this.rotatedDrawArea.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = this.rotatedDrawArea.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = this.rotatedDrawArea.Points()[3].Y;           // (Right, Bottom) Y 좌표

            Point pointLT = new Point(Convert.ToInt32(ltX), Convert.ToInt32(ltY));
            Point pointLB = new Point(Convert.ToInt32(lbX), Convert.ToInt32(lbY));
            Point pointRT = new Point(Convert.ToInt32(rtX), Convert.ToInt32(rtY));
            Point pointRB = new Point(Convert.ToInt32(rbX), Convert.ToInt32(rbY));

            Point pointTC = new Point(Convert.ToInt32((ltX + rtX) / 2), Convert.ToInt32((ltY + rtY) / 2));
            Point pointRC = new Point(Convert.ToInt32((rtX + rbX) / 2), Convert.ToInt32((rtY + rbY) / 2));
            Point pointBC = new Point(Convert.ToInt32((rbX + lbX) / 2), Convert.ToInt32((rbY + lbY) / 2));
            Point pointLC = new Point(Convert.ToInt32((lbX + ltX) / 2), Convert.ToInt32((lbY + ltY) / 2));

            int ellipseCenterX = Convert.ToInt32(this.rotatedDrawArea.Center.X);
            int ellipseCenterY = Convert.ToInt32(this.rotatedDrawArea.Center.Y);
            int ellipseWidth = Convert.ToInt32(this.rotatedDrawArea.Size.Width);
            int ellipseHeight = Convert.ToInt32(this.rotatedDrawArea.Size.Height);
            #endregion

            // Back 이미지 그리기
            ImageUsedMath.DrawEllipse(g, new Pen(this.BorderBackColor, Global.Roi_BorderWidth), new Point(ellipseCenterX - 1, ellipseCenterY - 1), new Size(ellipseWidth, ellipseHeight), this.rotatedDrawArea.Angle, false, this.FilledColor);
            // 원래 이미지 그리기
            ImageUsedMath.DrawEllipse(g, pen, new Point(ellipseCenterX, ellipseCenterY), new Size(ellipseWidth, ellipseHeight), this.rotatedDrawArea.Angle, IsFill, this.FilledColor);

            #region 가이드 라인 및 점 그리기
            if (IsGuideLine)
            {
                Point[] polyPoints = { pointLT, pointRT, pointRB, pointLB };

                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                // 가이드 Line 그리기
                g.DrawPolygon(pen, polyPoints);

                // 가이드 점 그리기
                this.DrawPoint(g, this.GuidePointColor, pointLT, object_radius);   // Left, Top Point
                this.DrawPoint(g, this.GuidePointColor, pointLB, object_radius);   // Left, Bottom Point
                this.DrawPoint(g, this.GuidePointColor, pointRT, object_radius);   // Right Top Point
                this.DrawPoint(g, this.GuidePointColor, pointRB, object_radius);   // Right Bottom Point

                #region RotatedRect의 회전 점을 그린다.
                if (IsRotatedPoint)
                {
                    // rotatedDrawArea 중심점
                    PointF areaCP = new PointF(this.rotatedDrawArea.Center.X, this.rotatedDrawArea.Center.Y);

                    var ctX = (ltX + rtX) / 2;
                    var ctY = (ltY + rtY) / 2;

                    // rotatedDrawArea 중심점과 YMinPoint와의 거리에 object_radius * 4를 더한값이 회전점의 거리
                    float distanceRP = ImageUsedMath.DistanceToPoint(areaCP, new System.Drawing.PointF(ctX, ctY)) + object_radius * 4;

                    PointF pointRP = ImageUsedMath.FindDistancePoint(areaCP, distanceRP, this.rotatedDrawArea.Angle - 90);
                    Point pointCT = new Point(Convert.ToInt32(this.rotatedDrawArea.Center.X), Convert.ToInt32(this.rotatedDrawArea.Center.Y));
                    Point drawRP = new Point(Convert.ToInt32(pointRP.X), Convert.ToInt32(pointRP.Y));
                    g.DrawLine(pen, pointCT, drawRP);

                    this.DrawPoint(g, Color.Green, drawRP, object_radius + 2);
                    this.DrawPoint(g, Color.LightGreen, drawRP, object_radius);

                    // Polgon의 중심점 그린다.
                    this.DrawPoint(g, Color.Green, pointCT, object_radius + 2);
                    this.DrawPoint(g, Color.LightGreen, pointCT, object_radius);
                }
                #endregion
            }
            #endregion

            #region ROI Index 및 온도값/온도차 표시
            String strIndex = "";
            if (Global.Roi_Option == 1)
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
            }
            else
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
                if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            }
            if (showAvg)
            {
                double roiAverage = Math.Truncate((this.ROI_Average * (Global.HighTemperature - Global.LowTemperature) / 255 + Global.LowTemperature) * 10) / 10;
                if (Global.Roi_Option == 3)
                    strIndex += String.Format("\r\n({0:#.0})", roiAverage);
                else
                    strIndex += String.Format("({0:#.0})", roiAverage);

                if (showDiff && this.ROI_SubIndex == 2)// && this.ROI_Diff != 0)    // ROI_Diff 0도 표시
                {
                    strIndex += String.Format(", {0:0.0}", this.ROI_Diff);
                }
            }

            if (strIndex.Length > 0)
            {
                //  가장  Y축이 큰 점에 쓴다.
                Point textPoint = pointBC;
                if (textPoint.Y < pointTC.Y) textPoint = pointTC;
                if (textPoint.Y < pointRC.Y) textPoint = pointRC;
                if (textPoint.Y < pointLC.Y) textPoint = pointLC;

                // 그려질 글씨 길이 구하기
                SizeF sz = g.MeasureString(strIndex, this.DrawFont);

                // 이미지 밖으로 ROI 글자가 나가는 것 방지 코드
                int drawX = textPoint.X;
                int drawY = textPoint.Y + 5;
                if ((int)(drawX + sz.Width) > picWidth) drawX = (int)(picWidth - sz.Width);
                if ((int)(drawY + sz.Height) > picHeight) drawY = (int)(picHeight - sz.Height);

                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeBackColor), new Point(drawX - 1, drawY - 1)); // Back 글자
                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeColor), new Point(drawX, drawY));         // 원래 글자
            }
            #endregion
        }

        /** @brief: Rectangle 그리기
         *  @section: Description
         *      Rectangle 그리기 위해 rotatedArea -> rotatedDrawArea로 변경해서 사용한다.
         *  @param:     picWidth            그려질 이미지의 넓이
         *  @param:     picHeight           그려질 이미지의 높이
         *  @param:     g                   그려질 위치의 GDI+ 변수
         *  @param:     pen                 그릴 pen
         *  @param:     showAvg             true이면 ROI의 평균값을 보여준다. 기본을 false이다.
         *  @param:     showDiff            true이면 연결된 ROI와 평균값 차이를 보여준다( Subindex : 2인경우만). 기본을 false이다.
         *  @param:     IsGuideLine         GuideLien을 그릴지 여부
         *  @param:     IsFill              ROI 내부를 색을 채울지 설정
         *  @param:     IsRotatedPoint      회전점을 그릴지 선택
        */
        private void DrawRectangle(Graphics g, int picWidth, int picHeight, Pen pen, bool showAvg, bool showDiff = false, bool IsGuideLine = false, bool IsFill = false, bool IsRotatedPoint = false)
        {
            if ((this.rotatedDrawArea.Size.Width * this.rotatedDrawArea.Size.Height) < 10) return;

            // rotatedRect의 포함한 최소 Rect
            //OpenCvSharp.Rect boundRect = this.rotatedDrawArea.BoundingRect();

            #region 각 코너 좌표 구하기
            var ltX = this.rotatedDrawArea.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = this.rotatedDrawArea.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = this.rotatedDrawArea.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = this.rotatedDrawArea.Points()[2].Y;           // (Right, Top) Y 좌표

            var lbX = this.rotatedDrawArea.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = this.rotatedDrawArea.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = this.rotatedDrawArea.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = this.rotatedDrawArea.Points()[3].Y;           // (Right, Bottom) Y 좌표

            Point pointLT = new Point(Convert.ToInt32(ltX), Convert.ToInt32(ltY));
            Point pointLB = new Point(Convert.ToInt32(lbX), Convert.ToInt32(lbY));
            Point pointRT = new Point(Convert.ToInt32(rtX), Convert.ToInt32(rtY));
            Point pointRB = new Point(Convert.ToInt32(rbX), Convert.ToInt32(rbY));
            #endregion

            // rotatedRect이기 때문에 polygon으로 그린다.
            Point[] polyBackPoints = { new Point(pointLT.X - 1, pointLT.Y - 1), new Point(pointRT.X - 1, pointRT.Y - 1), new Point(pointRB.X - 1, pointRB.Y - 1), new Point(pointLB.X - 1, pointLB.Y - 1) };
            Point[] polyPoints = { pointLT, pointRT, pointRB, pointLB };

            // Back 이미지 그리기
            g.DrawPolygon(new Pen(this.BorderBackColor, Global.Roi_BorderWidth), polyBackPoints);
            // 도형을 채운다.
            if (IsFill) g.FillPolygon(new SolidBrush(this.FilledColor), polyPoints);
            // 원래 이미지 그리기
            g.DrawPolygon(pen, polyPoints);

            # region 가이드 라인 및 점 그리기
            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                // 가이드 Line 그리기
                g.DrawPolygon(pen, polyPoints);

                // 가이드 점 그리기
                this.DrawPoint(g, this.GuidePointColor, pointLT, object_radius);   // Left, Top Point
                this.DrawPoint(g, this.GuidePointColor, pointLB, object_radius);   // Left, Bottom Point
                this.DrawPoint(g, this.GuidePointColor, pointRT, object_radius);   // Right Top Point
                this.DrawPoint(g, this.GuidePointColor, pointRB, object_radius);   // Right Bottom Point

                #region RotatedRect의 회전 점을 그린다.
                if (IsRotatedPoint)
                {
                    // rotatedDrawArea 중심점
                    PointF areaCP = new PointF(this.rotatedDrawArea.Center.X, this.rotatedDrawArea.Center.Y);

                    var ctX = (ltX + rtX) / 2;
                    var ctY = (ltY + rtY) / 2;

                    // rotatedDrawArea 중심점과 YMinPoint와의 거리에 object_radius * 4를 더한값이 회전점의 거리
                    float distanceRP = ImageUsedMath.DistanceToPoint(areaCP, new System.Drawing.PointF(ctX, ctY)) + object_radius * 4;

                    PointF pointRP = ImageUsedMath.FindDistancePoint(areaCP, distanceRP, this.rotatedDrawArea.Angle - 90);
                    Point pointCT = new Point(Convert.ToInt32(this.rotatedDrawArea.Center.X), Convert.ToInt32(this.rotatedDrawArea.Center.Y));
                    Point drawRP = new Point(Convert.ToInt32(pointRP.X), Convert.ToInt32(pointRP.Y));
                    g.DrawLine(pen, pointCT, drawRP);

                    this.DrawPoint(g, Color.Green, drawRP, object_radius + 2);
                    this.DrawPoint(g, Color.LightGreen, drawRP, object_radius);

                    // Polgon의 중심점 그린다.
                    this.DrawPoint(g, Color.Green, pointCT, object_radius + 2);
                    this.DrawPoint(g, Color.LightGreen, pointCT, object_radius);
                }
                #endregion
            }
            #endregion

            # region ROI Index 및 온도값/온도차 표시
            String strIndex = "";
            if (Global.Roi_Option == 1)
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
            }
            else
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
                if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            }
            if (showAvg)
            {
                double roiAverage = Math.Truncate((this.ROI_Average * (Global.HighTemperature - Global.LowTemperature) / 255 + Global.LowTemperature) * 10) / 10;
                if (Global.Roi_Option == 3)
                    strIndex += String.Format("\r\n({0:#.0})", roiAverage);
                else
                    strIndex += String.Format("({0:#.0})", roiAverage);

                if (showDiff && this.ROI_SubIndex == 2)// && this.ROI_Diff != 0)    // ROI_Diff 0도 표시
                {
                    strIndex += String.Format(", {0:0.0}", this.ROI_Diff);
                }
            }

            if (strIndex.Length > 0)
            {
                //  가장  Y축이 큰 점에 쓴다.
                Point textPoint = pointRB;
                if (textPoint.Y < pointLT.Y) textPoint = pointLT;
                if (textPoint.Y < pointRT.Y) textPoint = pointRT;
                if (textPoint.Y < pointLB.Y) textPoint = pointLB;

                // 그려질 글씨 길이 구하기
                SizeF sz = g.MeasureString(strIndex, this.DrawFont);

                // 이미지 밖으로 ROI 글자가 나가는 것 방지 코드
                int drawX = textPoint.X;
                int drawY = textPoint.Y + 5;
                if ((int)(drawX + sz.Width) > picWidth) drawX = (int)(picWidth - sz.Width);
                if ((int)(drawY + sz.Height) > picHeight) drawY = (int)(picHeight - sz.Height);


                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeBackColor), new Point(drawX - 1, drawY - 1)); // Back 글자
                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeColor), new Point(drawX, drawY));         // 원래 글자
            }
            #endregion
        }

        /** @brief: Diamond 그리기
         *  @section: Description
         *      Rectangle 그리기 위해 rotatedArea -> rotatedDrawArea로 변경해서 사용한다.
         *  @param:     picWidth            그려질 이미지의 넓이
         *  @param:     picHeight           그려질 이미지의 높이
         *  @param:     g                   그려질 위치의 GDI+ 변수
         *  @param:     pen                 그릴 pen
         *  @param:     showAvg             true이면 ROI의 평균값을 보여준다. 기본을 false이다.
         *  @param:     showDiff            true이면 연결된 ROI와 평균값 차이를 보여준다( Subindex : 2인경우만). 기본을 false이다.
         *  @param:     IsGuideLine            GuideLien을 그릴지 여부
         *  @param:     IsFill              ROI 내부를 색을 채울지 설정
         *  @param:     IsRotatedPoint      회전점을 그릴지 선택
        */
        private void DrawDiamond(Graphics g, int picWidth, int picHeight, Pen pen, bool showAvg, bool showDiff = false, bool IsGuideLine = false, bool IsFill = false, bool IsRotatedPoint = false)
        {
            if ((this.rotatedDrawArea.Size.Width * this.rotatedDrawArea.Size.Height) < 10) return;

            #region 각 코너 좌표 구하기
            var ltX = this.rotatedDrawArea.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = this.rotatedDrawArea.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = this.rotatedDrawArea.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = this.rotatedDrawArea.Points()[2].Y;           // (Right, Top) Y 좌표

            var lbX = this.rotatedDrawArea.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = this.rotatedDrawArea.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = this.rotatedDrawArea.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = this.rotatedDrawArea.Points()[3].Y;           // (Right, Bottom) Y 좌표

            Point pointLT = new Point(Convert.ToInt32(ltX), Convert.ToInt32(ltY));
            Point pointLB = new Point(Convert.ToInt32(lbX), Convert.ToInt32(lbY));
            Point pointRT = new Point(Convert.ToInt32(rtX), Convert.ToInt32(rtY));
            Point pointRB = new Point(Convert.ToInt32(rbX), Convert.ToInt32(rbY));

            Point pointTC = new Point(Convert.ToInt32((ltX + rtX) / 2), Convert.ToInt32((ltY + rtY) / 2));
            Point pointRC = new Point(Convert.ToInt32((rtX + rbX) / 2), Convert.ToInt32((rtY + rbY) / 2));
            Point pointBC = new Point(Convert.ToInt32((rbX + lbX) / 2), Convert.ToInt32((rbY + lbY) / 2));
            Point pointLC = new Point(Convert.ToInt32((lbX + ltX) / 2), Convert.ToInt32((lbY + ltY) / 2));
            #endregion

            // rotatedRect이기 때문에 polygon으로 그린다.
            Point[] polyBackPoints = { new Point(pointTC.X - 1, pointTC.Y - 1), new Point(pointRC.X - 1, pointRC.Y - 1), new Point(pointBC.X - 1, pointBC.Y - 1), new Point(pointLC.X - 1, pointLC.Y - 1) };
            Point[] polyPoints = { pointTC, pointRC, pointBC, pointLC };

            // Back 이미지 그리기
            g.DrawPolygon(new Pen(this.BorderBackColor, Global.Roi_BorderWidth), polyBackPoints);
            // 도형을 채운다.
            if (IsFill) g.FillPolygon(new SolidBrush(this.FilledColor), polyPoints);
            // 원래 이미지 그리기
            g.DrawPolygon(pen, polyPoints);

            # region 가이드 라인 및 점 그리기
            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                // 가이드 Line 그리기
                g.DrawPolygon(pen, new Point[] { pointLT, pointRT, pointRB, pointLB });

                // 가이드 점 그리기
                this.DrawPoint(g, this.GuidePointColor, pointLT, object_radius);   // Left, Top Point
                this.DrawPoint(g, this.GuidePointColor, pointLB, object_radius);   // Left, Bottom Point
                this.DrawPoint(g, this.GuidePointColor, pointRT, object_radius);   // Right Top Point
                this.DrawPoint(g, this.GuidePointColor, pointRB, object_radius);   // Right Bottom Point

                #region RotatedRect의 회전 점을 그린다.
                if (IsRotatedPoint)
                {
                    // rotatedDrawArea 중심점
                    PointF areaCP = new PointF(this.rotatedDrawArea.Center.X, this.rotatedDrawArea.Center.Y);

                    var ctX = (ltX + rtX) / 2;
                    var ctY = (ltY + rtY) / 2;

                    // rotatedDrawArea 중심점과 YMinPoint와의 거리에 object_radius * 4를 더한값이 회전점의 거리
                    float distanceRP = ImageUsedMath.DistanceToPoint(areaCP, new System.Drawing.PointF(ctX, ctY)) + object_radius * 4;

                    PointF pointRP = ImageUsedMath.FindDistancePoint(areaCP, distanceRP, this.rotatedDrawArea.Angle - 90);
                    Point pointCT = new Point(Convert.ToInt32(this.rotatedDrawArea.Center.X), Convert.ToInt32(this.rotatedDrawArea.Center.Y));
                    Point drawRP = new Point(Convert.ToInt32(pointRP.X), Convert.ToInt32(pointRP.Y));
                    g.DrawLine(pen, pointCT, drawRP);

                    this.DrawPoint(g, Color.Green, drawRP, object_radius + 2);
                    this.DrawPoint(g, Color.LightGreen, drawRP, object_radius);

                    // Polgon의 중심점 그린다.
                    this.DrawPoint(g, Color.Green, pointCT, object_radius + 2);
                    this.DrawPoint(g, Color.LightGreen, pointCT, object_radius);
                }
                #endregion
            }
            #endregion

            # region ROI Index 및 온도값/온도차 표시
            String strIndex = "";
            if (Global.Roi_Option == 1)
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
            }
            else
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
                if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            }
            if (showAvg)
            {
                double roiAverage = Math.Truncate((this.ROI_Average * (Global.HighTemperature - Global.LowTemperature) / 255 + Global.LowTemperature) * 10) / 10;
                if (Global.Roi_Option == 3)
                    strIndex += String.Format("\r\n({0:#.0})", roiAverage);
                else
                    strIndex += String.Format("({0:#.0})", roiAverage);

                if (showDiff && this.ROI_SubIndex == 2)// && this.ROI_Diff != 0)    // ROI_Diff 0도 표시
                {
                    strIndex += String.Format(", {0:0.0}", this.ROI_Diff);
                }
            }

            if (strIndex.Length > 0)
            {
                //  가장  Y축이 큰 점에 쓴다.
                Point textPoint = pointBC;
                if (textPoint.Y < pointTC.Y) textPoint = pointTC;
                if (textPoint.Y < pointRC.Y) textPoint = pointRC;
                if (textPoint.Y < pointLC.Y) textPoint = pointLC;

                // 그려질 글씨 길이 구하기
                SizeF sz = g.MeasureString(strIndex, this.DrawFont);

                // 이미지 밖으로 ROI 글자가 나가는 것 방지 코드
                int drawX = textPoint.X;
                int drawY = textPoint.Y + 5;
                if ((int)(drawX + sz.Width) > picWidth) drawX = (int)(picWidth - sz.Width);
                if ((int)(drawY + sz.Height) > picHeight) drawY = (int)(picHeight - sz.Height);

                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeBackColor), new Point(drawX - 1, drawY - 1)); // Back 글자
                g.DrawString(strIndex, this.DrawFont, new SolidBrush(this.ForeColor), new Point(drawX, drawY));         // 원래 글자
            }
            #endregion
        }

        #endregion 도형 그리기 함수
    }
}
