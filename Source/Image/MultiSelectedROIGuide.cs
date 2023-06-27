using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Duxcycler_IMAGE
{
    /** @class: IRIS에서 ROI를 여러게 선택해서 이동, 회전 등을 하는 클래스 이다.
     *  @section: Description
     *      IRIS에서 ROI를 여러게 선택해서 이동, 회전 등을 하는 클래스 이다.
     */
    class MultiSelectedROIGuide
    {
    #region 클래스 내부 사용 변수 모음
        private const int object_radius = 3;                                                    /** 점의 크기 */
        private const int over_dist_squared = object_radius * object_radius;                    /** 거리를 측정하는 기준값 */

        private OpenCvSharp.RotatedRect rotatedRect = new OpenCvSharp.RotatedRect();             /** MultiSelectedROIGuide Image 기준으로 저장되는 도형 정보         */
        
        private bool IsSelected = false;                                                        /** MultiSelectedROIGuide의 선택 여부      */
    #endregion

    #region 클래스 외부 설정 변수 모음
        public List<ROIShape> SelectedROIList { get { return this._selectedROIList; }  }
        private List<ROIShape> _selectedROIList = new List<ROIShape>();                           /** 선택된 ROI 모음      */

        /**  MultiSelectedROIGuide의 기준 Image의 폭 */
        public double Image_Width { get { return _image_width; } set { _image_width = value; } }
        private double _image_width = -1;

        /**  MultiSelectedROIGuide의 기준 Image의 높이 */
        public double Image_Height { get { return _image_height; } set { _image_height = value; } }
        private double _image_height = -1;

        /** MultiSelectedROIGuide를 선택할 경우 변수 */
        public ROISHAPEPOSTION NodeSelected { get { return this._nodeSelected; } set { this._nodeSelected = value; } }
        private ROISHAPEPOSTION _nodeSelected = ROISHAPEPOSTION.None;

        /** MultiSelectedROIGuide의  테두리 색 */
        public Color BorderColor { get { return this._bordercolor; } set { this._bordercolor = value; } }
        Color _bordercolor = Color.LightGray;

        /** MultiSelectedROIGuide의  테두리 두께 */
        public int BorderWidth { get { return this._borderwidth; } set { this._borderwidth = value; if (this._borderwidth < 1) this._borderwidth = 1; } }
        int _borderwidth = 1;

        /**  MultiSelectedROIGuide의 테두리 배경색 */
        public Color BorderBackColor { get { return this._borderbackcolor; } set { this._borderbackcolor = value; } }
        private Color _borderbackcolor = Color.Black;

        /**  MultiSelectedROIGuide의 모서리 Point 색 */
        public Color BorderPointColor { get { return this._borderpointcolor; } set { this._borderpointcolor = value; } }
        private Color _borderpointcolor = Color.Red; 
    #endregion


        /** @brief: MultiSelectedROIGuide 도형의 생성자
         *  @param:     ImageWidth    기준이 되는 Image Width(int 형)
         *  @param:     ImageHeiht    기준이 되는 Image Height(int 형)
         */
        public MultiSelectedROIGuide(int ImageWidth, int ImageHeiht )
        {
            this.Image_Width = ImageWidth;      // 기준이 되는 Image Width
            this.Image_Height = ImageHeiht;     // 기준이 되는 Image Height
        }

        /** @brief: MultiSelectedROIGuide에 선택된 ROI를 입력한다.
         * @section: Description
         *      추가후 PictureBox 다시 그리기해야 적용된다.
         *  @param:     selectedROI     추가할 선택된 ROI         
         *  @return     추가되면 true, 안되면 false
         */
        public bool AddSelectedROI(ROIShape selectedROI)
        {
            if (selectedROI == null || !selectedROI.IsShape()) return false;    // 예외 처리
            // LineAngle, LineX는 제외다.
            if( selectedROI.ShapeType == ROISHAPETYPE.LineAngle || selectedROI.ShapeType == ROISHAPETYPE.LineX) return false;
                        
            selectedROI.Selected = true;            // 선택으로 처리
            this._selectedROIList.Add(selectedROI);  // 선택 ROI List에 추가

            List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
            foreach(ROIShape roi in this._selectedROIList)
            {
                RectangleF roiShapeArea = roi.ShapeArea();
                points.Add(new OpenCvSharp.Point(roiShapeArea.Left,     roiShapeArea.Top));
                points.Add(new OpenCvSharp.Point(roiShapeArea.Right,    roiShapeArea.Top));
                points.Add(new OpenCvSharp.Point(roiShapeArea.Right,    roiShapeArea.Bottom));
                points.Add(new OpenCvSharp.Point(roiShapeArea.Left,     roiShapeArea.Bottom));
            }

            if (points.Count > 3)
            {
                OpenCvSharp.Rect boundRect = OpenCvSharp.Cv2.BoundingRect(points);

                float centerX = (float)boundRect.Left + (float)boundRect.Width / 2f;
                float centerY = (float)boundRect.Top + (float)boundRect.Height / 2f;
                float width = (float)boundRect.Width;
                float height = (float)boundRect.Height;

                // SelecedROI 포함되는 도형을 만든다. (Image 기준)
                this.rotatedRect = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), 0);
            }

            return true;
        }

        /** @brief: MultiSelectedROIGuide에 선택된 ROI를 모두 선택 삭제한다.         
         */
        public void RemoveSelectedROI()
        {
            this._selectedROIList.Clear();
        }
                
        /** @brief: MultiSelectedROIGuide에 선택된 ROI를 모두 선택 취소한다.
         * @section: Description
         *       MultiSelectedROIGuide를 지울때 사용         
         */
        public void NoSelectedROI()
        {
            foreach (ROIShape roi in this._selectedROIList) roi.Selected = false;        
        }

        /** @brief: MultiSelectedROIGuide의 영역에 Point가 포함된 위치
         *  @section: Description
         *      MultiSelectedROIGuide의 영역에 마우스 위치가 포함된 위치 구한다.
         *  @param:     mouse_pt    원본 이미지 기준의 마우스 점( System.Drawing.PointF 변수 )
         *  @return:    포함된 위치를 리턴한다.(ROISHAPEPOSTION 형식)
         */
        public bool GetNodeSelectable(PointF mouse_pt)
        {
            this.NodeSelected = ROISHAPEPOSTION.None;
            this.IsSelected = false;

            #region 각 코너 좌표 구하기
            var ltX = rotatedRect.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = rotatedRect.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = rotatedRect.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = rotatedRect.Points()[2].Y;           // (Right, Top) Y 좌표
                             
            var lbX = rotatedRect.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = rotatedRect.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = rotatedRect.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = rotatedRect.Points()[3].Y;           // (Right, Bottom) Y 좌표

            PointF pointLT = new PointF(ltX, ltY);
            PointF pointLB = new PointF(lbX, lbY);
            PointF pointRT = new PointF(rtX, rtY);
            PointF pointRB = new PointF(rbX, rbY);           
            #endregion

            var ctX = (ltX + rtX) / 2;
            var ctY = (ltY + rtY) / 2;

            PointF pointRP = ImageUsedMath.FindDistancePoint(new System.Drawing.PointF(ctX, ctY), object_radius * 4, this.rotatedRect.Angle - 90);     // 회전점
            
            if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRP) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Rotation;               // 위쪽 회전             
            else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, new PointF(this.rotatedRect.Center.X, this.rotatedRect.Center.Y), pointRP) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Rotation;               // 위쪽 회전 
            else if (DoesRectangleContainPoint(this.rotatedRect, mouse_pt)) this.NodeSelected = ROISHAPEPOSTION.Inside;

            if (this.NodeSelected != ROISHAPEPOSTION.None) this.IsSelected = true;
            return this.IsSelected;
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

        /** @brief: 도형이 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우 Node 선택에 따른 Offset 이동( 회전과 이동만 가능) 
         *  @section: Description
         *      ROI를 이동 및 점 위치 수정할 경우에 사용한다. Image 기준으로 이동한다. 도형이 도형이 Rectangle(사각형), Diamond(다이아몬드), Ellipse(타원) 인경우
         *  @param:     xOffset       Image 기준의 x 이동 값 (float)
         *  @param:     yOffset       Image 기준의 y 이동 값 (float)
         */
        public void RotatedAreaOffset(float xOffset, float yOffset)
        {
            if (this._nodeSelected == ROISHAPEPOSTION.None) return;

            var ltX = this.rotatedRect.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = this.rotatedRect.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = this.rotatedRect.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = this.rotatedRect.Points()[2].Y;           // (Right, Top) Y 좌표

            // Node 선택에 따라 점 이동 Offset 설정
            switch (this.NodeSelected)
            {
                case ROISHAPEPOSTION.Rotation:                      // 회전 점 이동시 회전각 계산후 바로 return한다.
                    {
                        var ctX = (ltX + rtX) / 2;
                        var ctY = (ltY + rtY) / 2;

                        ImageUsedMath.AngleBetween(new PointF(this.rotatedRect.Center.X, this.rotatedRect.Center.Y), new PointF(ctX + xOffset, ctY + yOffset), out float realAngle);

                        realAngle -= 270;

                        PointF CenterP = new PointF(this.rotatedRect.Center.X, this.rotatedRect.Center.Y);  // MultiSelectedROIGuide 중심점
                        // 포함됨 ROI 회전
                        foreach (ROIShape roi in this._selectedROIList)
                        {
                            roi.Rotation(CenterP, realAngle - this.rotatedRect.Angle);                      // MultiSelectedROIGuide 중심점을 기준으로 ROI을 회전시킨다.
                        }

                        this.rotatedRect.Angle = realAngle;                                                 // MultiSelectedROIGuide를 회전시킨다.

                        break;
                    }    // 위쪽 회전                 

                case ROISHAPEPOSTION.Inside:                        // 전체 이동시 Center 점을 이동한후 return한다.
                    {
                        this.rotatedRect.Center.X += xOffset;
                        this.rotatedRect.Center.Y += yOffset;

                        foreach(ROIShape roi in this._selectedROIList)
                        {
                            roi.NodeSelected = ROISHAPEPOSTION.Inside;
                            roi.Offset(xOffset, yOffset);
                            roi.NodeSelected = ROISHAPEPOSTION.None;
                        }
                        break;
                    }
                default:
                    return;
            }
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

        /** @brief: 입력된 Image Mat에서 MultiSelectedROIGuide에 포함되어있는 ROI 도형 영역구하고, Min,Max,평균,표준편차, 넓이구해서 저장한다.
         *  @section: Description
         *      입력된 Image Mat(그래이 영상만 가능하다)에서 ROI 도형의 영역을 구하고,  
         *      this.ROI_MinValue(Min값),this.ROI_MaxValue(Max값),this.ROI_Average(평균값)this.ROI_Sdnn(표준편차값) 구해 저장한다.
         *      ROI의 Min, Max, 평균, 표준편차를 구하는 부분이므로 LinX, LineAngle은 해당되지 않는다.
         *  @param:     roiMat       구할 Image Mat(그래이 영상만 가능하다)
         */
        public void CalSelectedROIShare(OpenCvSharp.Mat imageMat)
        {
            if (imageMat == null) return;

            // 포함됨 ROI 회전
            foreach (ROIShape roi in this._selectedROIList) roi.CalROIShare(imageMat);
        }

    #region MultiSelectedROIGuide 그리기 부분
    
        /** @brief: MultiSelectedROIGuide를 그리는 함수 
         *  @section: Description
         *      ROI의 점 정보가 원본 이미지 정보기 때문에 그려질 width, height정보가 필요하다.
         *  @param:     g                   그려질 위치의 GDI+ 변수
         *  @param:     picWidth            그려질 위치의 width(int 형)
         *  @param:     picHeight           그려질 위치의 height(int 형)
         *  @param:     subMatLeft          Pan시 Left 위치
         *  @param:     subMatTop           Pan시 Right 위치
         *  @param:     zoom_Ratio          Zoom 배율 값
         */
        public void Draw(Graphics g, int picWidth, int picHeight, int subMatLeft = 0, int subMatTop = 0, double zoom_Ratio = 1.0)
        {
            #region 그리기 위해 점으로 변경한다.
            // 그리기 위해 점으로 변경한다.  
            double imageWidth = this.Image_Width;      // MultiSelectedROIGuide의 기준이 되는 Image Width
            double imageHeight = this.Image_Height;     // MultiSelectedROIGuide의 기준이 되는 Image Height

            float centerX = (float)((this.rotatedRect.Center.X * zoom_Ratio - subMatLeft) * picWidth / imageWidth);     // PictureBox의 기준(zoom, pan 포함)으로 x 값을 변경한다.
            float centerY = (float)((this.rotatedRect.Center.Y * zoom_Ratio - subMatTop) * picHeight / imageHeight);    // PictureBox의 기준(zoom, pan 포함)으로 y 값을 변경한다.
            float width = (float)((this.rotatedRect.Size.Width * zoom_Ratio) * picHeight / imageHeight);              // PictureBox의 기준(zoom, pan 포함)으로 width 값을 변경한다.
            float height = (float)((this.rotatedRect.Size.Height * zoom_Ratio) * picHeight / imageHeight);             // PictureBox의 기준(zoom, pan 포함)으로 height 값을 변경한다.

            float angle = this.rotatedRect.Angle;

            // 그리기 도형을 만든다.
            OpenCvSharp.RotatedRect rotatedDrawArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), angle);
            #endregion

            #region 각 코너 좌표 구하기
            var ltX = rotatedDrawArea.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = rotatedDrawArea.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = rotatedDrawArea.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = rotatedDrawArea.Points()[2].Y;           // (Right, Top) Y 좌표

            var lbX = rotatedDrawArea.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = rotatedDrawArea.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = rotatedDrawArea.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = rotatedDrawArea.Points()[3].Y;           // (Right, Bottom) Y 좌표

            Point pointLT = new Point(Convert.ToInt32(ltX), Convert.ToInt32(ltY));
            Point pointLB = new Point(Convert.ToInt32(lbX), Convert.ToInt32(lbY));
            Point pointRT = new Point(Convert.ToInt32(rtX), Convert.ToInt32(rtY));
            Point pointRB = new Point(Convert.ToInt32(rbX), Convert.ToInt32(rbY));

            // rotatedRect이기 때문에 polygon으로 그린다.
            Point[] polyBackPoints = { new Point(pointLT.X - 1, pointLT.Y - 1), new Point(pointRT.X - 1, pointRT.Y - 1), new Point(pointRB.X - 1, pointRB.Y - 1), new Point(pointLB.X - 1, pointLB.Y - 1) };
            Point[] polyPoints = { pointLT, pointRT, pointRB, pointLB };
            #endregion

            # region 가이드 라인 그리기
            // Back 이미지 그리기
            Pen pen = new Pen(this.BorderBackColor, this.BorderWidth) { DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot };
            g.DrawPolygon(new Pen(this.BorderBackColor, this.BorderWidth), polyBackPoints);
            // 원래 이미지 그리기
            pen.Color = this.BorderColor;
            g.DrawPolygon(pen, polyPoints);
            #endregion
                        
            #region 가이드 점 그리기
                this.DrawPoint(g, this.BorderPointColor, pointLT, object_radius);   // Left, Top Point
                this.DrawPoint(g, this.BorderPointColor, pointLB, object_radius);   // Left, Bottom Point
                this.DrawPoint(g, this.BorderPointColor, pointRT, object_radius);   // Right Top Point
                this.DrawPoint(g, this.BorderPointColor, pointRB, object_radius);   // Right Bottom Point
            #endregion

            if (this.IsSelected)
            {
                #region RotatedRect의 회전 점을 그린다.
                // rotatedDrawArea 중심점
                PointF areaCP = new PointF(rotatedDrawArea.Center.X, rotatedDrawArea.Center.Y);

                var ctX = (ltX + rtX) / 2;
                var ctY = (ltY + rtY) / 2;

                // rotatedDrawArea 중심점과 YMinPoint와의 거리에 object_radius * 4를 더한값이 회전점의 거리
                float distanceRP = ImageUsedMath.DistanceToPoint(areaCP, new System.Drawing.PointF(ctX, ctY)) + object_radius * 4;

                PointF pointRP = ImageUsedMath.FindDistancePoint(areaCP, distanceRP, rotatedDrawArea.Angle - 90);
                Point pointCT = new Point(Convert.ToInt32(rotatedDrawArea.Center.X), Convert.ToInt32(rotatedDrawArea.Center.Y));
                Point drawRP = new Point(Convert.ToInt32(pointRP.X), Convert.ToInt32(pointRP.Y));
                g.DrawLine(pen, pointCT, drawRP);

                this.DrawPoint(g, Color.Green, drawRP, object_radius + 2);
                this.DrawPoint(g, Color.LightGreen, drawRP, object_radius);

                // Polgon의 중심점 그린다.
                this.DrawPoint(g, Color.Green, pointCT, object_radius + 2);
                this.DrawPoint(g, Color.LightGreen, pointCT, object_radius);

                #endregion
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
    #endregion
    }
}
