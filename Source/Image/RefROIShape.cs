using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Duxcycler_IMAGE
{

    // ROI 도형 클래스
    public class RefROIShape
    {
        #region 클래스 내부 변수
        private int movePoint = -1;                                         // 이동할 점
        // The "size" of an object for mouse over purposes.
        private const int object_radius = 3;

        // We're over an object if the distance squared
        // between the mouse and the object is less than this.
        private const int over_dist_squared = object_radius * object_radius;

        //ROI 도형의 Point 정보 ( PictureBox Size 기준 점 )
        public List<Point> PointInfo { get { return _pointInfo; } set { _pointInfo = value; } }
        private List<Point> _pointInfo = new List<Point>();
        #endregion 클래스 내부 변수

        #region 클래스 외부 설정 변수
        // 해당 ROI가 포함된 study index
        public string ChartNo { get { return _chartno; } set { _chartno = value; } }                 // Char NO
        private string _chartno = "";
        public int StudyID { get { return _studyid; } set { _studyid = value; } }
        private int _studyid = -1;

        // 해당 ROI가 포함되 Image Index
        public int ImageIndex { get { return _imageindex; } set { _imageindex = value; } }
        private int _imageindex = -1;

        // 해당 ROI가 포함되 Image의 ROI Index
        public int ROIID { get { return _roiid; } set { _roiid = value; } }
        private int _roiid = -1;
        // 해당 ROI와 연결될 ROI가 포함된 Chart No
        public string Connect_ChartNo { get { return _connect_chartno; } set { _connect_chartno = value; } }                 // Char NO
        private string _connect_chartno = "";
        // 해당 ROI와 연결될 ROI가 포함된 study index
        public int Connect_StudyID { get { return _connect_studyid; } set { _connect_studyid = value; } }
        private int _connect_studyid = -1;

        // 해당 ROI와 연결될 ROI가 포함되 Image Index
        public int Connect_ImageIndex { get { return _connect_imageindex; } set { _connect_imageindex = value; } }
        private int _connect_imageindex = -1;

        // 해당 ROI와 연결될 ROI가 포함되 Image의 ROI Index
        public int Connect_ROIID { get { return _connect_roiid; } set { _connect_roiid = value; } }
        private int _connect_roiid = -1;

        // ROI Index
        public int ROI_MainIndex { get { return _roi_mainindex; } set { _roi_mainindex = value; } }
        private int _roi_mainindex = -1;

        // ROI Index
        public int ROI_SubIndex { get { return _roi_subindex; } set { _roi_subindex = value; } }
        private int _roi_subindex = -1;

        // 도형의 타입
        public ROISHAPETYPE ShapeType { get { return _shapeType; } set { _shapeType = value; } }
        private ROISHAPETYPE _shapeType = ROISHAPETYPE.Rectangle;

        // ROI Node 선택변수
        public ROISHAPEPOSTION NodeSelected { get { return this._nodeSelected; } set { this._nodeSelected = value; } }
        private ROISHAPEPOSTION _nodeSelected = ROISHAPEPOSTION.None;

        // ROI Guide Point 색
        public Color GuidePointColor { get { return this._guidepointcolor; } set { this._guidepointcolor = value; } }
        private Color _guidepointcolor = Color.Red;

        // ROI Guide Line 색
        public Color GuideLineColor { get { return this._guidelinecolor; } set { this._guidelinecolor = value; } }
        private Color _guidelinecolor = Color.LightGray;

        // ROI 선택 테두리 색
        public Color SelectedBorderColor { get { return this._selectedbordercolor; } set { this._selectedbordercolor = value; } }
        private Color _selectedbordercolor = Color.Red;

        // ROI 테두리 색
        public Color BorderColor { get { return this._bordercolor; } set { this._bordercolor = value; } }
        private Color _bordercolor = Color.White;

        // ROI 테두리 배경색
        public Color BorderBackColor { get { return this._borderbackcolor; } set { this._borderbackcolor = value; } }
        private Color _borderbackcolor = Color.Black;

        // ROI 테두리 두께
        public int BorderWidth { get { return this._borderwidth; } set { this._borderwidth = value; if (this._borderwidth < 1) this._borderwidth = 1; } }
        private int _borderwidth = 1;

        // ROI 내부 색
        public Color FillColor { get { return this._fillcolor; } set { this._fillcolor = value; } }
        private Color _fillcolor = Color.Black;

        // ROI 내부에 색을 채울지 여부
        public bool IsFill { get { return this._isfill; } set { this._isfill = value; } }
        private bool _isfill = false;

        // ROI 넘버을 보여줄지
        public bool IsShowNumber { get { return this._isshownumber; } set { this._isshownumber = value; } }
        private bool _isshownumber = true;

        // ROI DIff 를 보여줄지
        public bool IsShowDiff { get { return this._isshowdiff; } set { this._isshowdiff = value; } }
        private bool _isshowdiff = false;

        // ROI Diff 표시 Font
        public Font FontDiff { get { return this._fontDiff; } set { this._fontDiff = value; } }
        private Font _fontDiff = SystemFonts.DefaultFont;

        // ROI DIff 표시 글자 색
        public Color ForeDiffColor { get { return this._foreDiffcolor; } set { this._foreDiffcolor = value; } }
        private Color _foreDiffcolor = Color.White;

        // ROI 선택 여부 
        public bool Selected { get { return this._selected; } set { this._selected = value; } }
        private bool _selected = false;

        // ROI Index 표시 Font
        public Font Font { get { return this._font; } set { this._font = value; } }
        private Font _font = SystemFonts.DefaultFont;
        // ROI Index 표시 글자 색
        public Color ForeColor { get { return this._forecolor; } set { this._forecolor = value; } }
        private Color _forecolor = Color.White;

        //  ROI Index 표시 글자 배경색
        public Color ForeBackColor { get { return this._forebackcolor; } set { this._forebackcolor = value; } }
        private Color _forebackcolor = Color.Black;
                
        // ROI의 기준 Image의 폭
        public double Image_Width { get { return _image_width; } set { _image_width = value; } }
        private double _image_width = -1;
        // ROI의 기준 Image의 높이
        public double Image_Height { get { return _image_height; } set { _image_height = value; } }
        private double _image_height = -1;

        // ROI내의 Min 값
        public double ROI_MinValue { get { return _roi_minvalue; } set { _roi_minvalue = value; } }
        private double _roi_minvalue = -1;
        // ROI내의 Max 값
        public double ROI_MaxValue { get { return _roi_maxvalue; } set { _roi_maxvalue = value; } }
        private double _roi_maxvalue = -1;
        // ROI내의 평균값 값
        public double ROI_Average { get { return _roi_average; } set { _roi_average = value; } }
        private double _roi_average = -1;
        // ROI내의 표준편차 값
        public double ROI_Sdnn { get { return _roi_sdnn; } set { _roi_sdnn = value; } }
        private double _roi_sdnn = -1;
        // 해당 ROI와 연결된 ROI의 평균값의 차이( Sub Index가 2인경우에만 값이 있다.)
        public double ROI_Diff { get { return _roi_diff; } set { _roi_diff = value; } }
        private double _roi_diff = 0;

        // ROI를 포함하는 Area 폭(Image 기준 폭이다. )
        public double ROI_Width { get { return _roi_width; } set { _roi_width = value; } }
        private double _roi_width = -1;
        // ROI를 포함하는 Area 높이(Image 기준 높이이다. )
        public double ROI_Height { get { return _roi_height; } set { _roi_height = value; } }
        private double _roi_height = -1;
        // ROI의 실제 면적(Image 기준 면적이다. )
        public double ROI_Area { get { return _roi_area; } set { _roi_area = value; } }
        private double _roi_area = -1;

        //ROI 도형의 Point 정보 ( Image Size 기준 점, OpenCV 사용함. )
        //public List<OpenCvSharp.Point2f> imagePointInfo = new List<OpenCvSharp.Point2f>(); 
        public List<PointF> imagePointInfo = new List<PointF>();
        #endregion 클래스 외부 설정 변수

        // 생성자
        public RefROIShape(ROISHAPETYPE type = ROISHAPETYPE.Rectangle)
        {
            this.PointInfo.Clear();
            this.imagePointInfo.Clear();
            this.ShapeType = type;
        }

        // ROI 도형을 복사( 정보를 그대로 복사 )
        public RefROIShape CopyTo()
        {
            //if(IsShape())   // 해당 ROI가 도형이야 한다.
            {
                RefROIShape copy = this.Clone();

                copy.ROI_MainIndex = this.ROI_MainIndex;
                copy.ROI_SubIndex = this.ROI_SubIndex;

                return copy;
            }

            //return null;
        }

        // ROI 도형의 Clone 생성( 원본의 모양과 point 정보만 입력)
        public RefROIShape Clone()
        {
            //if(IsShape())   // 해당 ROI가 도형이야 한다.
            {
                RefROIShape clone = new RefROIShape(this.ShapeType)
                {
                    // ROI Shape의 기본정보
                    ChartNo             = this.ChartNo,
                    StudyID             = this.StudyID,
                    ImageIndex          = this.ImageIndex,
                    ROIID               = this.ROIID,

                    ////////////////////////////////////////////////////////////////////세로 추가 2020-05-11
                    ROI_MainIndex       = this.ROI_MainIndex,
                    ROI_SubIndex        = this.ROI_SubIndex,
                    IsFill              = this.IsFill,
                    FillColor           = this.FillColor,                    
                    IsShowDiff          = this.IsShowDiff,              // ROI 차이값을 보여줄지 설정
                    IsShowNumber        = this.IsShowNumber,            // ROI 번호를 보여줄지 설정
                    FontDiff            = this.FontDiff,                // ROI Diff 글자 Font 설정
                    ForeDiffColor       = this.ForeDiffColor,           // ROI Diff 글자 색   
                    ////////////////////////////////////////////////////////////////////세로 추가 2020-05-11

                    Connect_ChartNo = this.Connect_ChartNo,
                    Connect_StudyID     = this.Connect_StudyID,
                    Connect_ImageIndex  = this.Connect_ImageIndex,
                    Connect_ROIID       = this.Connect_ROIID,
                    GuidePointColor     = this.GuidePointColor,
                    GuideLineColor      = this.GuideLineColor,
                    SelectedBorderColor = this.SelectedBorderColor,
                    BorderColor         = this.BorderColor,
                    //BorderWidth         = this.BorderWidth,
                    BorderWidth         = Global.Roi_BorderWidth,
                    Font                = this.Font,
                    ForeColor           = this.ForeColor
                };

                // 점 추가                      
                for (int index = 0; index < this.PointInfo.Count; index++) clone.AddPoint(index, this.PointInfo[index]);

                clone.Image_Width = this.Image_Width;               // ROI의 기준 Image의 폭
                clone.Image_Height = this.Image_Height;              // ROI의 기준 Image의 높이               
                clone.ROI_MinValue = this.ROI_MinValue;              // ROI내의 Min 값                
                clone.ROI_MaxValue = this.ROI_MaxValue;              // ROI내의 Max 값                
                clone.ROI_Average = this.ROI_Average;               // ROI내의 평균값 값                
                clone.ROI_Sdnn = this.ROI_Sdnn;                  // ROI내의 표준편차 값
                clone.ROI_Diff = this.ROI_Diff;                  // 해당 ROI와 연결된 ROI의 평균값과의 차이
                clone.ROI_Width = this.ROI_Width;                 // ROI를 포함하는 Area 폭(Image 기준 폭이다. )                
                clone.ROI_Height = this.ROI_Height;                // ROI를 포함하는 Area 높이(Image 기준 높이이다. )                
                clone.ROI_Area = this.ROI_Area;                  // ROI의 실제 면적(Image 기준 면적이다. )

                // Image 크기의 점 복사
                foreach (var p in this.imagePointInfo) clone.imagePointInfo.Add(p);

                return clone;
            }

            //return null;
        }

        // ROI가 그려지는 확인( 점이 2개 이상이면 : true )
        public bool IsDraw()
        {
            if (this.PointInfo.Count < 2)
                return false;
            else
            {
                // 두점의 거리가 over_dist_squared 보다 작은면 
                if (FindDistanceToPointSquared(this.PointInfo[0], this.PointInfo[1]) < 5) return false;

                return true;
            }
        }

        // ROI가 도형인지 확인 함수
        public bool IsShape()
        {
            Rectangle sAres = this.ShapeArea();
            if (System.Math.Abs(sAres.Width * sAres.Height) < 10) return false;  // ROI의 영역이 최소 5 이상이어야 한다.
            return true;
        }

        // 점 모두 삭제
        public void RemovePointAll()
        {
            this.PointInfo.Clear();
        }

        // 점 삭제( Polygon인 경우만)
        public void RemovePoint(int index)
        {
            if (this.ShapeType == ROISHAPETYPE.Polygon)
            {
                if (index < this.PointInfo.Count)
                {
                    this.PointInfo.RemoveAt(index);
                }
            }
        }

        // 마지막 점 기지고 오기
        public Point LastPoint()
        {
            if (this.PointInfo.Count > 0) return this.PointInfo[this.PointInfo.Count - 1];
            return new Point(0, 0);
        }

        // 점 추가 ( ROI를 새로 만들 경우에 사용한다.)
        public int AddPoint(int index, Point addPoint)
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
                    maxPoint = 2000;
                    break;
            }

            if (index < this.PointInfo.Count)
            {
                this.PointInfo[index] = addPoint;
            }
            else
            {
                if (index >= maxPoint)
                {
                    index = maxPoint - 1;
                    this.PointInfo[maxPoint - 1] = addPoint;
                }
                else
                    this.PointInfo.Add(addPoint);
            }

            if (index == 1 && this.ShapeType == ROISHAPETYPE.LineAngle)
            {
                Point centerPoint = this.PointInfo[0];    // 0번째 포이트가 중심
                Point anglePoint = addPoint;
                double realAngle = 0;
                double angle = GetAngle(centerPoint, anglePoint, ref realAngle);
                Point basePoint = new Point(anglePoint.X, centerPoint.Y);
                if (angle > 45)
                    basePoint = new Point(centerPoint.X, anglePoint.Y);

                if (this.PointInfo.Count == 3) this.PointInfo[2] = basePoint;
                else this.PointInfo.Add(basePoint);
            }

            return this.PointInfo.Count;
        }

        // ROI의 모든 점 가지고 오기
        public Point[] AllPoint() { return this.PointInfo.ToArray(); }

        // 도형을 해당 위치로 이동 ( 마지막 점을 해당 위치로 이동한다.
        public void MoveTo(Point movePoint)
        {
            if (this.PointInfo.Count < 2 || this._nodeSelected == ROISHAPEPOSTION.None) return;

            Point lastPoint = this.PointInfo[this.PointInfo.Count - 1];

            int xOffset = movePoint.X - lastPoint.X;
            int yOffset = movePoint.Y - lastPoint.Y;

            for (int Index = 0; Index < this.PointInfo.Count; Index++)
            {
                Point mPoint = this.PointInfo[Index];

                mPoint.Offset(xOffset, yOffset);
                this.PointInfo[Index] = mPoint;
            }
        }

        // Node 선택에 따른 Offset 이동 ( ROI를 이동 및 점 위치 수정할 경우에 사용한다.
        public void Offset(int xOffset, int yOffset)
        {
            if (this.PointInfo.Count < 2 || this._nodeSelected == ROISHAPEPOSTION.None) return;

            int sX = xOffset; int sY = yOffset;
            int eX = xOffset; int eY = yOffset;

            // Node 선택에 따라 점 이동 Offset 설정
            switch (this.NodeSelected)
            {
                case ROISHAPEPOSTION.AreaLeftTop: { sX = xOffset; sY = yOffset; eX = 0; eY = 0; break; }
                case ROISHAPEPOSTION.AreaLeftMiddle: { sX = xOffset; sY = 0; eX = 0; eY = 0; break; }
                case ROISHAPEPOSTION.AreaLeftBottom: { sX = xOffset; sY = 0; eX = 0; eY = yOffset; break; }
                case ROISHAPEPOSTION.AreaBottomMiddle: { sX = 0; sY = 0; eX = 0; eY = yOffset; break; }
                case ROISHAPEPOSTION.AreaRightBottom: { sX = 0; sY = 0; eX = xOffset; eY = yOffset; break; }
                case ROISHAPEPOSTION.AreaRightMiddle: { sX = 0; sY = 0; eX = xOffset; eY = 0; break; }
                case ROISHAPEPOSTION.AreaRightTop: { sX = 0; sY = yOffset; eX = xOffset; eY = 0; break; }
                case ROISHAPEPOSTION.AreaTopMiddle: { sX = 0; sY = yOffset; eX = 0; eY = 0; break; }
                case ROISHAPEPOSTION.Inside:
                    {
                        for (int Index = 0; Index < this.PointInfo.Count; Index++)
                        {
                            Point mPoint = this.PointInfo[Index];
                            mPoint.Offset(xOffset, yOffset);
                            this.PointInfo[Index] = mPoint;
                        }
                        return;
                    }
            }

            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Rectangle:
                case ROISHAPETYPE.Diamond:
                case ROISHAPETYPE.Ellipse:
                    {
                        Point sPoint = this.PointInfo[0];
                        Point ePoint = this.PointInfo[1];
                        sPoint.Offset(sX, sY);
                        ePoint.Offset(eX, eY);
                        this.PointInfo[0] = sPoint;
                        this.PointInfo[1] = ePoint;
                        break;
                    }
                case ROISHAPETYPE.LineAngle:
                    {
                        if (this.PointInfo.Count != 3) break;

                        Point centerPoint = this.PointInfo[0];    // 0번째 포이트가 중심
                        Point anglePoint = this.PointInfo[1];
                        Point basePoint = this.PointInfo[2];
                        double realAngle = 0;

                        if (this.movePoint == 1)
                        {
                            anglePoint.Offset(xOffset, yOffset);
                        }
                        else if (this.movePoint == 2)
                        {
                            if (GetAngle(centerPoint, anglePoint, ref realAngle) > 45)
                            {
                                basePoint.Offset(0, yOffset);
                                anglePoint.Offset(0, yOffset);
                            }
                            else
                            {
                                basePoint.Offset(xOffset, 0);
                                anglePoint.Offset(xOffset, 0);
                            }
                        }
                        else
                        {
                            if (GetAngle(centerPoint, anglePoint, ref realAngle) > 45)
                                centerPoint.Offset(0, yOffset);
                            else
                                centerPoint.Offset(xOffset, 0);
                        }

                        basePoint = new Point(anglePoint.X, centerPoint.Y);
                        if (GetAngle(centerPoint, anglePoint, ref realAngle) > 45)
                            basePoint = new Point(centerPoint.X, anglePoint.Y);

                        this.PointInfo[0] = centerPoint;
                        this.PointInfo[1] = anglePoint;
                        this.PointInfo[2] = basePoint;
                        break;
                    }
                case ROISHAPETYPE.LineX:
                case ROISHAPETYPE.Polygon:
                    {
                        if (this.movePoint >= 0 && this.movePoint < this.PointInfo.Count)
                        {
                            Point mPoint = this.PointInfo[this.movePoint];
                            mPoint.Offset(xOffset, yOffset);
                            this.PointInfo[this.movePoint] = mPoint;
                        }
                        break;
                    }
            }
        }

        #region 마우스 위치에 따른 Node 선택 함수, 마우스 커서 모양 리턴 함수
        // 도형이 그려진 영역에서 Point가 포함된 위치
        public ROISHAPEPOSTION GetNodeSelectable(Point mouse_pt)
        {
            this.NodeSelected = ROISHAPEPOSTION.None;

            if (!IsShape()) return ROISHAPEPOSTION.None;
            Point sP = this.PointInfo[0];
            Point eP = this.PointInfo[1];

            Point pointLT = sP;                     // 1번째 Point
            Point pointLB = new Point(sP.X, eP.Y);   // 1번째 Point X, 2번째 Point Y
            Point pointRT = new Point(eP.X, sP.Y);   // 2번째 Point X, 1번째 Point Y
            Point pointRB = eP;                    // 2번째 Point

            switch (this.ShapeType)
            {
                case ROISHAPETYPE.Rectangle:
                case ROISHAPETYPE.Diamond:
                case ROISHAPETYPE.Ellipse:
                    {
                        if (FindDistanceToPointSquared(mouse_pt, pointLT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftTop;
                        else if (FindDistanceToPointSquared(mouse_pt, pointLB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftBottom;
                        else if (FindDistanceToPointSquared(mouse_pt, pointRT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightTop;
                        else if (FindDistanceToPointSquared(mouse_pt, pointRB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightBottom;
                        else if (FindDistanceToSegmentSquared(mouse_pt, pointLT, pointLB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftMiddle;
                        else if (FindDistanceToSegmentSquared(mouse_pt, pointLB, pointRB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaBottomMiddle;
                        else if (FindDistanceToSegmentSquared(mouse_pt, pointRB, pointRT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightMiddle;
                        else if (FindDistanceToSegmentSquared(mouse_pt, pointRT, pointLT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaTopMiddle;
                        else if (this.ShapeArea().Contains(mouse_pt)) this.NodeSelected = ROISHAPEPOSTION.Inside;

                        break;
                    }
                case ROISHAPETYPE.LineAngle:
                case ROISHAPETYPE.LineX:
                case ROISHAPETYPE.Polygon:
                    {
                        this.movePoint = -1;        // 이동 Point 초기화
                        for (int Index = 0; Index < this.PointInfo.Count; Index++)
                        {
                            if (FindDistanceToPointSquared(mouse_pt, this.PointInfo[Index]) < over_dist_squared)
                            {
                                this.NodeSelected = ROISHAPEPOSTION.HitPoint;
                                this.movePoint = Index;
                                return this.NodeSelected;
                            }
                        }

                        break;
                    }
            }

            if (this.NodeSelected == ROISHAPEPOSTION.None)
            {
                if (this.ShapeType == ROISHAPETYPE.Polygon && InsidePolygon(mouse_pt))
                    this.NodeSelected = ROISHAPEPOSTION.Inside;
                else if (this.ShapeType == ROISHAPETYPE.LineAngle && this.PointInfo.Count == 3)
                {
                    Point centerPoint = this.PointInfo[0];    // 0번째 포이트가 중심
                    Point anglePoint = this.PointInfo[1];
                    Point basePoint = this.PointInfo[2];
                    if (FindDistanceToSegmentSquared(mouse_pt, anglePoint, centerPoint) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                    else if (FindDistanceToSegmentSquared(mouse_pt, basePoint, centerPoint) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                }
                else if (this.ShapeType == ROISHAPETYPE.LineX && this.PointInfo.Count == 4)
                {
                    if (FindDistanceToSegmentSquared(mouse_pt, this.PointInfo[0], this.PointInfo[1]) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                    else if (FindDistanceToSegmentSquared(mouse_pt, this.PointInfo[2], this.PointInfo[3]) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Inside;
                }
            }

            return this.NodeSelected;
        }

        // ROI의 Node 선택에 따른 커서 모양 리턴한다.
        public Cursor GetCursor()
        {
            switch (this.NodeSelected)
            {
                case ROISHAPEPOSTION.AreaLeftMiddle:
                case ROISHAPEPOSTION.AreaRightMiddle:
                    return Cursors.Hand;

                case ROISHAPEPOSTION.AreaBottomMiddle:
                case ROISHAPEPOSTION.AreaTopMiddle:
                    return Cursors.Hand;

                case ROISHAPEPOSTION.AreaLeftTop:
                case ROISHAPEPOSTION.AreaRightBottom:
                case ROISHAPEPOSTION.AreaLeftBottom:
                case ROISHAPEPOSTION.AreaRightTop:
                case ROISHAPEPOSTION.HitPoint:
                    return Cursors.Hand;

                case ROISHAPEPOSTION.Inside:
                    return Cursors.Hand;
                default:
                    return Cursors.Arrow;
            }
        }
        #endregion 마우스 위치에 따른 Node 선택 함수, 마우스 커서 모양 리턴 함수

        #region ROI 그리기 함수        
        // 도형 그리기
        // showAvg가 true이면 ROI의 평균값을 보여준다. 기본을 false이다.)
        // showDiff가 true이면 연결된 ROI와 평균값 차이를 보여준다( Subindex : 2인경우만). 기본을 false이다.)
        public void Draw(Graphics g)
        {
            if (this.PointInfo.Count < 2) return;         // 도형은 경우만 그린다.

            if (g == null) return;
            Pen p = new Pen(this.BorderColor, this.BorderWidth);

            Color FontColor = this.ForeColor;
            // ROI가 선택되었을 경우 테두리 색을 선택테두리 색으로 변경
            if (this.Selected)
            {
                p.Color = this.SelectedBorderColor;
                FontColor = Color.White;
            }
            else
            {

            }

            // ROI에 마우스 포인트가 있을 경우 GuideLine을 그린다.
            if (this.NodeSelected != ROISHAPEPOSTION.None)
            {
                p.Color = this.SelectedBorderColor;
                FontColor = Color.White;
            }
            else
            {

            }

            switch (this._shapeType)
            {
                case ROISHAPETYPE.Rectangle:
                    this.DrawRectangle(g, p, FontColor);
                    break;
                case ROISHAPETYPE.Ellipse:
                    this.DrawEllipse(g, p, FontColor);
                    break;
                case ROISHAPETYPE.Diamond:
                    this.DrawRhombus(g, p, FontColor);
                    break;
                case ROISHAPETYPE.Polygon:
                    this.DrawPolygon(g, p, FontColor);
                    break;
                case ROISHAPETYPE.LineAngle:
                    this.DrawAngle(g, p);
                    break;
                case ROISHAPETYPE.LineX:
                    this.DrawXMark(g, p);
                    break;
            }
        }

        // 점 그리기
        private void DrawPoint(Graphics g, Color c, Point corner)
        {
            Rectangle rect = new Rectangle(corner.X - object_radius, corner.Y - object_radius,
                                                            2 * object_radius + 1, 2 * object_radius + 1);
            g.FillEllipse(new SolidBrush(c), rect);
        }

        // 각도 가이드 라인 그리기
        private void DrawXMark(Graphics g, Pen pen, bool IsGuideLine = false)
        {
            if (this.PointInfo.Count < 2) return;

            // Back 이미지 그리기
            Point BackPoint0 = this.PointInfo[0]; BackPoint0.Offset(-1, -1);     // 0번째 포인트는 중심점
            Point BackPoint1 = this.PointInfo[1]; BackPoint1.Offset(-1, -1);      // 1번째 포인트는 각도점

            g.DrawLine(new Pen(this.BorderBackColor, this.BorderWidth), BackPoint0, BackPoint1);
            if (this.PointInfo.Count == 4)
            {
                Point BackPoint2 = this.PointInfo[2]; BackPoint2.Offset(-1, -1);     // 0번째 포인트는 중심점
                Point BackPoint3 = this.PointInfo[3]; BackPoint3.Offset(-1, -1);      // 1번째 포인트는 각도점
                g.DrawLine(new Pen(this.BorderBackColor, this.BorderWidth), BackPoint2, BackPoint3);
            }

            // 원래 이미지 그리기
            g.DrawLine(pen, this.PointInfo[0], this.PointInfo[1]);
            if (this.PointInfo.Count == 4)
                g.DrawLine(pen, this.PointInfo[2], this.PointInfo[3]);

            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                g.DrawLine(pen, this.PointInfo[0], this.PointInfo[1]);
                this.DrawPoint(g, this.GuidePointColor, this.PointInfo[0]);
                this.DrawPoint(g, this.GuidePointColor, this.PointInfo[1]);
                if (this.PointInfo.Count == 4)
                {
                    g.DrawLine(pen, this.PointInfo[2], this.PointInfo[3]);
                    this.DrawPoint(g, this.GuidePointColor, this.PointInfo[2]);
                    this.DrawPoint(g, this.GuidePointColor, this.PointInfo[3]);
                }
            }

            Rectangle sArea = this.ShapeArea();
            String strIndex = "";
            if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
            if (strIndex.Length > 0)
            {
                // Back 글자
                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeBackColor), new Point(sArea.Left - 1, sArea.Bottom - 1));
                // 원래 글자
                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeColor), new Point(sArea.Left, sArea.Bottom));
            }
        }

        // 각도 가이드 라인 그리기
        private void DrawAngle(Graphics g, Pen pen, bool IsGuideLine = false)
        {
            if (this.PointInfo.Count != 3) return;

            Point centerPoint = this.PointInfo[0];      // 0번째 포인트는 중심점
            Point anglePoint = this.PointInfo[1];       // 1번째 포인트는 각도점
            Point basePoint = this.PointInfo[2];        // 2번째 포인트는 배이스점

            // Back 이미지 그리기
            Point centerBackPoint = centerPoint; centerBackPoint.Offset(-1, -1);     // 0번째 포인트는 중심점
            Point angleBackPoint = anglePoint; angleBackPoint.Offset(-1, -1);      // 1번째 포인트는 각도점
            Point baseBackPoint = basePoint; baseBackPoint.Offset(-1, -1);     // 2번째 포인트는 배이스점
            g.DrawLines(new Pen(this.BorderBackColor, this.BorderWidth), new Point[] { angleBackPoint, centerBackPoint, baseBackPoint });

            // 원래 이미지 그리기
            g.DrawLines(pen, new Point[] { anglePoint, centerPoint, basePoint });

            if (IsGuideLine)    // 가이드 라인을 그릴 것인지.
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                // 가이드 라인 그린다.
                g.DrawLines(pen, new Point[] { anglePoint, centerPoint, basePoint });
                // 가이드 점 그린다.
                this.DrawPoint(g, this.GuidePointColor, anglePoint);
                this.DrawPoint(g, this.GuidePointColor, centerPoint);
                this.DrawPoint(g, this.GuidePointColor, basePoint);
            }

            String strIndex = "";
            double realAngle = 0;
            // angle은 0 ~ 90도값이다. realAngle은 0 ~ 360도 값이다.
            double angle = GetAngle(centerPoint, anglePoint, ref realAngle);
            if (this.imagePointInfo.Count == 3) angle = GetAngle(this.imagePointInfo[0], this.imagePointInfo[1], ref realAngle);
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

                Rectangle AngleTextRect = new Rectangle(centerPoint.X, centerPoint.Y + 1, (int)(this.Font.SizeInPoints * strIndex.Length), this.Font.Height);

                if (IsRectUp) AngleTextRect.Offset(0, (-1 * AngleTextRect.Height));
                if (IsRectLeft) { AngleTextRect.Offset((-1 * AngleTextRect.Width), 0); strIndexFormat.Alignment = StringAlignment.Far; }

                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자

                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
        }

        // Polygon 그리기
        private void DrawPolygon(Graphics g, Pen pen, Color FontColor, bool IsGuideLine = false)
        {
            List<Point> backPointInfo = new List<Point>();

            foreach (Point corner in this.PointInfo)
            {
                backPointInfo.Add(new Point(corner.X - 1, corner.Y - 1));
            }

            if (IsFill) g.FillPolygon(new SolidBrush(this.FillColor), this.PointInfo.ToArray());
            else g.DrawPolygon(new Pen(this.BorderBackColor, this.BorderWidth), backPointInfo.ToArray());// Back 이미지 그리기

            // 원래 이미지 그리기
            g.DrawPolygon(pen, this.PointInfo.ToArray());

            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                g.DrawPolygon(pen, this.PointInfo.ToArray());

                foreach (Point corner in this.PointInfo) this.DrawPoint(g, this.GuidePointColor, corner);
            }
            Rectangle sArea = this.ShapeArea();
            StringFormat strIndexFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

            String strIndex = "";
            int centerX = sArea.Left + sArea.Width / 2;
            int centerY = sArea.Top + sArea.Height / 2;

            if (this.IsShowNumber)
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
                if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            }
            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                Rectangle AngleTextRect = new Rectangle(centerX - (FontWidth / 2), sArea.Bottom + 1, FontWidth, this.Font.Height);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.Font, new SolidBrush(FontColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
            strIndex = "";
            if (this.IsShowDiff)
            {
                strIndex += String.Format("({0:0.00})", this.ROI_Diff);
            }

            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                int FontHeight = this.Font.Height;
                Rectangle AngleTextRect = new Rectangle(centerX - (FontWidth / 2), centerY - (FontHeight / 2), FontWidth, FontHeight);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeDiffColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
        }

        // 두점 사이의 타원 그리기 
        private void DrawEllipse(Graphics g, Pen pen, Color FontColor, bool IsGuideLine = false)
        {
            Rectangle sArea = this.ShapeArea();

            if (IsFill) g.FillEllipse(new SolidBrush(this.FillColor), sArea);
            else g.DrawEllipse(new Pen(this.BorderBackColor, this.BorderWidth), new Rectangle(sArea.Left - 1, sArea.Top - 1, sArea.Width, sArea.Height));    // Back 이미지 그리기

            // 원래 이미지 그리기
            g.DrawEllipse(pen, sArea);

            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                g.DrawRectangle(pen, sArea);

                Point pointLT = new Point(sArea.Left, sArea.Top); this.DrawPoint(g, this.GuidePointColor, pointLT);
                Point pointLB = new Point(sArea.Left, sArea.Bottom); this.DrawPoint(g, this.GuidePointColor, pointLB);
                Point pointRT = new Point(sArea.Right, sArea.Top); this.DrawPoint(g, this.GuidePointColor, pointRT);
                Point pointRB = new Point(sArea.Right, sArea.Bottom); this.DrawPoint(g, this.GuidePointColor, pointRB);
            }

            StringFormat strIndexFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
            
            String strIndex = "";
            int centerX = sArea.Left + sArea.Width / 2;
            int centerY = sArea.Top + sArea.Height / 2;

            if (this.IsShowNumber)
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
                if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            }
            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                Rectangle AngleTextRect = new Rectangle(centerX - (FontWidth/2), sArea.Bottom + 1, FontWidth, this.Font.Height);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.Font, new SolidBrush(FontColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
            strIndex = "";
            if (this.IsShowDiff)
            {
                strIndex += String.Format("({0:0.00})", this.ROI_Diff);
            }

            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                int FontHeight = this.Font.Height;
                Rectangle AngleTextRect = new Rectangle(centerX - (FontWidth / 2), centerY -(FontHeight/2), FontWidth, FontHeight);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeDiffColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
        }

        // 두점 사이의 Rectangle 그리기
        private void DrawRectangle(Graphics g, Pen pen, Color FontColor, bool IsGuideLine = false)
        {
            Rectangle sArea = this.ShapeArea();

            if (IsFill) g.FillRectangle(new SolidBrush(this.FillColor), sArea);
            else g.DrawRectangle(new Pen(this.BorderBackColor, this.BorderWidth), new Rectangle(sArea.Left - 1, sArea.Top - 1, sArea.Width, sArea.Height));     // Back 이미지 그리기

            // 원래 이미지 그리기
            g.DrawRectangle(pen, sArea);

            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                g.DrawRectangle(pen, sArea);

                Point pointLT = new Point(sArea.Left, sArea.Top); this.DrawPoint(g, this.GuidePointColor, pointLT);
                Point pointLB = new Point(sArea.Left, sArea.Bottom); this.DrawPoint(g, this.GuidePointColor, pointLB);
                Point pointRT = new Point(sArea.Right, sArea.Top); this.DrawPoint(g, this.GuidePointColor, pointRT);
                Point pointRB = new Point(sArea.Right, sArea.Bottom); this.DrawPoint(g, this.GuidePointColor, pointRB);
            }

            StringFormat strIndexFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

            String strIndex = "";
            int centerX = sArea.Left + sArea.Width / 2;
            int centerY = sArea.Top + sArea.Height / 2;

            if (this.IsShowNumber)
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
                if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            }
            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                Rectangle AngleTextRect = new Rectangle(centerX - (FontWidth / 2), sArea.Bottom + 1, FontWidth, this.Font.Height);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.Font, new SolidBrush(FontColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
            strIndex = "";
            if (this.IsShowDiff)
            {
                strIndex += String.Format("({0:0.00})", this.ROI_Diff);
            }

            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                int FontHeight = this.Font.Height;
                Rectangle AngleTextRect = new Rectangle(centerX - (FontWidth / 2), centerY - (FontHeight / 2), FontWidth, FontHeight);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeDiffColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
        }

        // 두점 사이의 마름모 그리기
        private void DrawRhombus(Graphics g, Pen pen, Color FontColor, bool IsGuideLine = false)
        {
            Rectangle sArea = this.ShapeArea();

            float radiusX = (float)(sArea.Width) / 2;
            float radiusY = (float)(sArea.Height) / 2;
            float centerX = (float)sArea.Left + radiusX;
            float centerY = (float)sArea.Top + radiusY;

            Point[] polyBackPoints = { new Point((int)centerX - 1, sArea.Top - 1), new Point(sArea.Right - 1, (int)centerY - 1), new Point((int)centerX - 1, sArea.Bottom - 1), new Point(sArea.Left - 1, (int)centerY - 1) };
            Point[] polyPoints = { new Point((int)centerX, sArea.Top), new Point(sArea.Right, (int)centerY), new Point((int)centerX, sArea.Bottom), new Point(sArea.Left, (int)centerY) };

            if (this.IsFill) g.FillPolygon(new SolidBrush(this.FillColor), polyPoints);
            else g.DrawPolygon(new Pen(this.BorderBackColor, this.BorderWidth), polyBackPoints);    // Back 이미지 그리기                            

            // 원래 이미지 그리기
            g.DrawPolygon(pen, polyPoints);

            if (IsGuideLine)
            {
                pen.Color = this.GuideLineColor;
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                g.DrawRectangle(pen, sArea);

                Point pointLT = new Point(sArea.Left, sArea.Top); this.DrawPoint(g, this.GuidePointColor, pointLT);
                Point pointLB = new Point(sArea.Left, sArea.Bottom); this.DrawPoint(g, this.GuidePointColor, pointLB);
                Point pointRT = new Point(sArea.Right, sArea.Top); this.DrawPoint(g, this.GuidePointColor, pointRT);
                Point pointRB = new Point(sArea.Right, sArea.Bottom); this.DrawPoint(g, this.GuidePointColor, pointRB);
            }

            StringFormat strIndexFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

            String strIndex = "";

            if (this.IsShowNumber)
            {
                if (this.ROI_MainIndex > 0) strIndex += this.ROI_MainIndex.ToString();
                if (this.ROI_SubIndex > 0) strIndex += "-" + this.ROI_SubIndex.ToString();
            }
            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                Rectangle AngleTextRect = new Rectangle((int)centerX - (FontWidth / 2), sArea.Bottom + 1, FontWidth, this.Font.Height);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.Font, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.Font, new SolidBrush(FontColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
            strIndex = "";
            if (this.IsShowDiff)
            {
                strIndex += String.Format("({0:0.00})", this.ROI_Diff);
            }

            if (strIndex.Length > 0)
            {
                int FontWidth = (int)(this.Font.SizeInPoints * strIndex.Length);
                int FontHeight = this.Font.Height;
                Rectangle AngleTextRect = new Rectangle((int)centerX - (FontWidth / 2), (int)centerY - (FontHeight / 2), FontWidth, FontHeight);
                Rectangle AngleTextBackRect = AngleTextRect;
                AngleTextBackRect.Offset(-1, -1);
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeBackColor), AngleTextBackRect, strIndexFormat);   // Back 글자
                g.DrawString(strIndex, this.FontDiff, new SolidBrush(this.ForeDiffColor), AngleTextRect, strIndexFormat);          // 원래 글자 
            }
        }
        #endregion 도형 그리기 함수

        #region MathFunctions(ROI가 포함된 영역, 두점의 거리, 직선과 점의 거리, Polygon 안에 점이 있는지 확인, 두점의 각도)
        // ROI가 포함된 영역
        public Rectangle ShapeArea()
        {
            // 나머지 도형은 점 2개 이상
            if (this.PointInfo.Count < 2) return new Rectangle();
            // Polygon은 최소 점이 3개 이상
            if (this.ShapeType == ROISHAPETYPE.Polygon && this.PointInfo.Count < 3) return new Rectangle();
            // LineX는 점이 4개 이여야 한다.
            if (this.ShapeType == ROISHAPETYPE.LineX && this.PointInfo.Count != 4) return new Rectangle();

            var minX = this.PointInfo.Min(p => p.X);
            var minY = this.PointInfo.Min(p => p.Y);
            var maxX = this.PointInfo.Max(p => p.X);
            var maxY = this.PointInfo.Max(p => p.Y);

            return new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
        }

        // 두점의 거리
        private int FindDistanceToPointSquared(Point pt1, Point pt2)
        {
            int dx = pt1.X - pt2.X;
            int dy = pt1.Y - pt2.Y;
            return Convert.ToInt32(Math.Sqrt(dx * dx + dy * dy));
        }

        // 직선과 점의 거리
        private double FindDistanceToSegmentSquared(PointF pt, PointF p1, PointF p2)
        {
            PointF closest;
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new PointF(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new PointF(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new PointF(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
            //return dx * dx + dy * dy;
        }

        // Polygon 안에 점이 있는지 확인
        private bool InsidePolygon(Point mouse_pt)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(this.PointInfo.ToArray());

            // See if the point is inside the GraphicsPath.
            if (path.IsVisible(mouse_pt))
            {
                return true;
            }

            return false;
        }

        // 두점 사이의 각도(return값은 표시용 Angle(0~90도)이고 realAngle은 0~360도 값이다.
        public double GetAngle(PointF start, PointF end, ref double realAngle)
        {
            double dy = end.Y - start.Y;
            double dx = end.X - start.X;
            double angle = Math.Atan(dy / dx) * (180.0 / Math.PI);
            realAngle = angle;

            if (dx < 0.0)
            {
                realAngle += 180.0;
            }
            else
            {
                if (dy < 0.0) realAngle += 360.0;
            }

            return Math.Abs(angle % 90);
        }
        #endregion MathFunctions(ROI가 포함된 영역, 두점의 거리, 직선과 점의 거리, Polygon 안에 점이 있는지 확인, 두점의 각도)
    }
}
