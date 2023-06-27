using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Duxcycler_IMAGE
{
    public class ImageROIGrid
    {
        // The "size" of an object for mouse over purposes.
        private const int object_radius = 3;
        // We're over an object if the distance squared
        // between the mouse and the object is less than this.
        private const int over_dist_squared = object_radius * object_radius;


        /**  ImageROIGrid의 기준 Image의 폭 */
        public double Image_Width { get { return _image_width; } set { _image_width = value; } }
        private double _image_width = -1;

        /**  ImageROIGrid의 기준 Image의 높이 */
        public double Image_Height { get { return _image_height; } set { _image_height = value; } }
        private double _image_height = -1;


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

        //ROI RotatedRect 도형 정보        
        public OpenCvSharp.RotatedRect rotatedRect = new OpenCvSharp.RotatedRect();
        // 이미지 Rect에 사용한다.
        public Bitmap ImageSouce { get { return _imagesouce; } set { _imagesouce = value; } }
        private Bitmap _imagesouce = null;
        // ROI List
        public List<RefROIShape> RefROIList = new List<RefROIShape>();

        // Node 선택변수
        public ROISHAPEPOSTION NodeSelected { get { return this._nodeSelected; } set { this._nodeSelected = value; } }
        private ROISHAPEPOSTION _nodeSelected = ROISHAPEPOSTION.None;

        // 생성자
        public ImageROIGrid(Bitmap image, Rectangle location)
        {
            this.ImageSouce = image;
            if (this.ImageSouce != null)
                this.ImageSouce.MakeTransparent(Color.White);

            var minX = location.Left;
            var minY = location.Top;

            var width = location.Width;
            var height = location.Height;

            var centerX = minX + (width / 2);
            var centerY = minY + (height / 2);

            this.rotatedRect = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), 0);
        }

        public ImageROIGrid(Bitmap image, OpenCvSharp.Point2f center, OpenCvSharp.Size2f size, float angle)
        {
            this.ImageSouce = image;
            if (this.ImageSouce != null)
                this.ImageSouce.MakeTransparent(Color.White);

            this.rotatedRect = new OpenCvSharp.RotatedRect(center, size, angle);
        }

        /** @brief: 적용할 위치로 RefROIShape의 점을 변경한다.
         *  @section: Description
         *      적용할 위치로 RefROIShape의 점을 변경한다.
         *  @param:     refROIShape         저장된 원본 RefROIShape 값
         *  @param:     picWidth            그려질 위치의 width(int 형)
         *  @param:     picHeight           그려질 위치의 height(int 형)
         *  @param:     subMatLeft          Pan시 Left 위치
         *  @param:     subMatTop           Pan시 Right 위치
         *  @param:     zoom_Ratio          Zoom 배율 값
         */
        public RefROIShape GetRefROIShape(RefROIShape refROIShape, int picWidth, int picHeight, int subMatLeft = 0, int subMatTop = 0, double zoom_Ratio = 1.0)
        {
            RefROIShape ReturnROIShape = null;
            
            if (refROIShape != null)
            {
                #region 회전하기 전의 그리는 위치를 구해서 적용해야 됨.
                // 그리기 위해 점으로 변경한다.  
                double imageWidth = this.Image_Width;      // MultiSelectedROIGuide의 기준이 되는 Image Width
                double imageHeight = this.Image_Height;     // MultiSelectedROIGuide의 기준이 되는 Image Height

                float centerX = (float)((this.rotatedRect.Center.X * zoom_Ratio - subMatLeft) * picWidth / imageWidth);     // PictureBox의 기준(zoom, pan 포함)으로 x 값을 변경한다.
                float centerY = (float)((this.rotatedRect.Center.Y * zoom_Ratio - subMatTop) * picHeight / imageHeight);    // PictureBox의 기준(zoom, pan 포함)으로 y 값을 변경한다.
                float drawWidth = (float)((this.rotatedRect.Size.Width * zoom_Ratio) * picHeight / imageHeight);              // PictureBox의 기준(zoom, pan 포함)으로 width 값을 변경한다.
                float drawHeight = (float)((this.rotatedRect.Size.Height * zoom_Ratio) * picHeight / imageHeight);             // PictureBox의 기준(zoom, pan 포함)으로 height 값을 변경한다.

                float angle = this.rotatedRect.Angle;

                // 그리기 도형을 만든다.
                OpenCvSharp.RotatedRect rotatedDrawArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(drawWidth, drawHeight), 0);
                
                // 회전 전의 기준 위치
                var pointltX = rotatedDrawArea.Points()[1].X;
                var pointltY = rotatedDrawArea.Points()[1].Y;
                #endregion

                // 입력된 ROI의 복사본을 만든다.
                ReturnROIShape = refROIShape.Clone();
                // 복사본의 점을 지운다.
                ReturnROIShape.RemovePointAll();
                if (refROIShape.PointInfo.Count == 2)
                {
                    int minX = refROIShape.PointInfo.Min(p => p.X);
                    int minY = refROIShape.PointInfo.Min(p => p.Y);
                    int maxX = refROIShape.PointInfo.Max(p => p.X);
                    int maxY = refROIShape.PointInfo.Max(p => p.Y);

                    float pointCenterX = minX + (maxX - minX) / 2;
                    float pointCenterY = minY + (maxY - minY) / 2;
                    float conX = pointltX + pointCenterX * drawWidth / (float)refROIShape.Image_Width;                      // 그림을 그려질 기준으로 x 값을 변경한다.
                    float conY = pointltY + pointCenterY * drawHeight / (float)refROIShape.Image_Height;                    // 그림을 그려질 기준으로 y 값을 변경한다.

                    // 그려질 기준의 ROI의 중심점을 도형의 회전한 만큼 회전한 중심점을 구한다.
                    PointF newPointF = ImageUsedMath.Rotate(new PointF(conX, conY), new PointF(centerX, centerY), angle);

                    // 회전한 중심점을 (Left, Top) 점, (Right, Bottom) 점으로 입력한다.
                    ReturnROIShape.AddPoint(0, new System.Drawing.Point(Convert.ToInt32(newPointF.X - refROIShape.ROI_Width / 2), Convert.ToInt32(newPointF.Y - refROIShape.ROI_Height / 2)));
                    ReturnROIShape.AddPoint(1, new System.Drawing.Point(Convert.ToInt32(newPointF.X + refROIShape.ROI_Width / 2), Convert.ToInt32(newPointF.Y + refROIShape.ROI_Height / 2)));

                }
            }

            return ReturnROIShape;
        }

        /** @brief: RefROIShape 정보들를 읽은 함수이다.
         *  @section: Description
         *      저장된 RefROIShape 정보을 읽을 때는 사용할 Width, Height 정보를 입력해야 그것에 맞춰어 ROI 위치 정보를 변경한다.
         *  @param:     width            RefROIShape를 사용할 위치의 width(int 형)
         *  @param:     height           RefROIShape를 사용할 위치의 height(int 형)
         */
        public List<RefROIShape> GetRefROIShapes(int width, int height)
        {
            List<RefROIShape> refROIShapes = new List<RefROIShape>();

            
            foreach (RefROIShape refROIShape in this.RefROIList)
            {
                // RotatedRect크기에 해당하는 RefROI 좌표 정보를 변경한다.
                RefROIShape refROI = GetRefROIShape(refROIShape, width, height);

                refROIShapes.Add(refROI);
            }

            return refROIShapes;
        }

        /** @brief: ImageROI Grid 그리는 함수 
         *  @section: Description
         *      ImageROIGrid의 위치 정보가 원본 이미지 정보기 때문에 그려질 위치의 width, height정보가 필요하다.
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
            double imageWidth  = this.Image_Width;      // MultiSelectedROIGuide의 기준이 되는 Image Width
            double imageHeight = this.Image_Height;     // MultiSelectedROIGuide의 기준이 되는 Image Height

            float centerX = (float)((this.rotatedRect.Center.X * zoom_Ratio - subMatLeft) * picWidth / imageWidth);     // PictureBox의 기준(zoom, pan 포함)으로 x 값을 변경한다.
            float centerY = (float)((this.rotatedRect.Center.Y * zoom_Ratio - subMatTop) * picHeight / imageHeight);    // PictureBox의 기준(zoom, pan 포함)으로 y 값을 변경한다.
            float width   = (float)((this.rotatedRect.Size.Width * zoom_Ratio) * picHeight / imageHeight);              // PictureBox의 기준(zoom, pan 포함)으로 width 값을 변경한다.
            float height  = (float)((this.rotatedRect.Size.Height * zoom_Ratio) * picHeight / imageHeight);             // PictureBox의 기준(zoom, pan 포함)으로 height 값을 변경한다.

            float angle = this.rotatedRect.Angle;

            // 그리기 도형을 만든다.
            OpenCvSharp.RotatedRect rotatedDrawArea = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), angle);
            #endregion

            #region 각 코너 좌표 구하기
            int imgBoundWidth  = rotatedDrawArea.BoundingRect().Width;          // rotatedDrawArea BoundingRect 폭
            int imgBoundHeight = rotatedDrawArea.BoundingRect().Height;         // rotatedDrawArea BoundingRect 높이            
            int imgLeft        = rotatedDrawArea.BoundingRect().Left;           // rotatedDrawArea BoundingRect Left
            int imgTop         = rotatedDrawArea.BoundingRect().Top;            // rotatedDrawArea BoundingRect Top
            int imgWidth       = Convert.ToInt32(rotatedDrawArea.Size.Width);   // rotatedDrawArea 폭
            int imgHeight      = Convert.ToInt32(rotatedDrawArea.Size.Height);  // rotatedDrawArea 높이

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

            #region 이미지를 RotatedRect의 각도를 회전하여 그린다.
            if (this.ImageSouce != null && imgWidth != 0 && imgHeight != 0)
            {
                // 이미지를 RotatedRect크기로 변경
                OpenCvSharp.Mat imageMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(this.ImageSouce).Resize(new OpenCvSharp.Size(Math.Abs(imgWidth), Math.Abs(imgHeight)));

                // 이미지를 그린다. RotatedRect의 BoundingRect의 크기로 이미지를 만들고 BoundingRect의 Left, Top에 그려야 이미지가 짤리지 않느다.                                    
                g.DrawImage(RotateImage(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imageMat), imgBoundWidth, imgBoundHeight, this.rotatedRect.Angle), imgLeft, imgTop);
            }
            #endregion

            #region 점을 그린다.

            for (int index = 0; index < this.RefROIList.Count; index++)
            {
                // 저장된 점을 그리는 비율에 맞춰서 변경한다.
                RefROIShape cShape = GetRefROIShape(this.RefROIList[index], picWidth, picHeight, subMatLeft, subMatTop, zoom_Ratio);
                if (cShape != null) cShape.Draw(g);
            }

            #endregion

            #region 가이드 라인 그리기
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
                        
            #region ImageROIGrid의 회전 점을 그린다.
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

        /** @brief: 이미지 회전하는 함수
         *  @section: Description
         *      이미지 회전하는 함수 ( width와 height는 회전하는 이미지 보다 커야 이미지가 안짤린다.)
         *  @param:     b                   회전할 이미지
         *  @param:     width               회전할 이미지 넓이
         *  @param:     height              회전할 이니미 높이
         *  @param:     angle               회전각
         *  @return:    회전한 이미지
         */
        private Bitmap RotateImage(Bitmap b, int width, int height, float angle)
        {
            Bitmap returnBitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(returnBitmap);
            g.TranslateTransform((float)width / 2, (float)height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-(float)width / 2, -(float)height / 2);
            int left = (width / 2) - (b.Width / 2);
            int top = (height / 2) - (b.Height / 2);
            g.DrawImage(b, new Point(left, top));
            return returnBitmap;
        }

        /** @brief: 점그리는 함수 
         *  @section: Description
         *      점 그리는 함수
         *  @param:     g                그려질 위치의 GDI+ 변수
         *  @param:     c                점의 색
         *  @param:     corner           점의 센터 위치
         *  @param:     radius           점 크기
         */
        private void DrawPoint(Graphics g, Color c, Point corner, int radius = object_radius)
        {
            Rectangle rect = new Rectangle(corner.X - radius, corner.Y - radius,
                                                            2 * radius + 1, 2 * radius + 1);
            g.FillEllipse(new SolidBrush(c), rect);
        }

    #region 마우스 위치에 따른 Node 선택 함수, 마우스 커서 모양 리턴 함수

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
        public void Offset(float xOffset, float yOffset)
        {
            if (this._nodeSelected == ROISHAPEPOSTION.None) return;

            var ltX = this.rotatedRect.Points()[1].X;           // (Left , Top) X 좌표
            var ltY = this.rotatedRect.Points()[1].Y;           // (Left , Top) Y 좌표
            var rtX = this.rotatedRect.Points()[2].X;           // (Right, Top) X 좌표
            var rtY = this.rotatedRect.Points()[2].Y;           // (Right, Top) Y 좌표
            var lbX = this.rotatedRect.Points()[0].X;           // (Left , Bottom) X 좌표
            var lbY = this.rotatedRect.Points()[0].Y;           // (Left , Bottom) Y 좌표
            var rbX = this.rotatedRect.Points()[3].X;           // (Right, Bottom) X 좌표
            var rbY = this.rotatedRect.Points()[3].Y;           // (Right, Bottom) Y 좌표


            float width = this.rotatedRect.Size.Width;
            float height = this.rotatedRect.Size.Height;

            float centerX = this.rotatedRect.Center.X + ((float)xOffset / 2f);
            float centerY = this.rotatedRect.Center.Y + ((float)yOffset / 2f);

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

                        centerX = this.rotatedRect.Center.X + (float)(((double)xOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle)));
                        centerY = this.rotatedRect.Center.Y + (float)(((double)xOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle)));
                        break;
                    }
                case ROISHAPEPOSTION.AreaTopMiddle:                 // Top 변 이동 계산 부분
                    {
                        height -= yOffset;
                        centerX = this.rotatedRect.Center.X + (float)(((double)yOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle + 90)));
                        centerY = this.rotatedRect.Center.Y + (float)(((double)yOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle + 90)));
                        break;
                    }
                case ROISHAPEPOSTION.AreaRightMiddle:               // Right 변 이동 계산 부분
                    {
                        width += xOffset;
                        centerX = this.rotatedRect.Center.X + (float)(((double)xOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle)));
                        centerY = this.rotatedRect.Center.Y + (float)(((double)xOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle)));
                        break;
                    }
                case ROISHAPEPOSTION.AreaBottomMiddle:              // Bottom 변 이동 계산 부분
                    {
                        height += yOffset;

                        centerX = this.rotatedRect.Center.X + (float)(((double)yOffset / 2f) * Math.Cos(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle + 90)));
                        centerY = this.rotatedRect.Center.Y + (float)(((double)yOffset / 2f) * Math.Sin(ImageUsedMath.DegreeToRadian(this.rotatedRect.Angle + 90)));
                        break;
                    }

                case ROISHAPEPOSTION.Rotation:                      // 회전 점 이동시 회전각 계산후 바로 return한다.
                    {
                        var ctX = (ltX + rtX) / 2;
                        var ctY = (ltY + rtY) / 2;

                        ImageUsedMath.AngleBetween(new PointF(this.rotatedRect.Center.X, this.rotatedRect.Center.Y), new PointF(ctX + xOffset, ctY + yOffset), out float realAngle);
                        this.rotatedRect.Angle = realAngle - 270;

                        return;
                    }    // 위쪽 회전                 

                case ROISHAPEPOSTION.Inside:                        // 전체 이동시 Center 점을 이동한후 return한다.
                    {
                        this.rotatedRect.Center.X += xOffset;
                        this.rotatedRect.Center.Y += yOffset;
                        return;
                    }
                default:
                    return;
            }

            this.rotatedRect = new OpenCvSharp.RotatedRect(new OpenCvSharp.Point2f(centerX, centerY), new OpenCvSharp.Size2f(width, height), this.rotatedRect.Angle);
        }

        /** @brief: 도형이 그려진 영역에서 Point가 포함된 위치
         *  @section: Description
         *      그려진 도형에서 마우스 위치가 포함된 위치 구한다.
         *  @param:     mouse_pt    원본 이미지 기준의 마우스 점( System.Drawing.PointF 변수 )
         *  @return:    포함된 위치를 리턴한다.(ROISHAPEPOSTION 형식)
         */
        public ROISHAPEPOSTION GetNodeSelectable(PointF mouse_pt)
        {
            this.NodeSelected = ROISHAPEPOSTION.None;
                    
            var ltX = this.rotatedRect.Points()[1].X;
            var ltY = this.rotatedRect.Points()[1].Y;
            var rtX = this.rotatedRect.Points()[2].X;
            var rtY = this.rotatedRect.Points()[2].Y;

            var lbX = this.rotatedRect.Points()[0].X;
            var lbY = this.rotatedRect.Points()[0].Y;
            var rbX = this.rotatedRect.Points()[3].X;
            var rbY = this.rotatedRect.Points()[3].Y;

            PointF pointLT = new PointF(ltX, ltY);
            PointF pointLB = new PointF(lbX, lbY);
            PointF pointRT = new PointF(rtX, rtY);
            PointF pointRB = new PointF(rbX, rbY);

            var ctX = (ltX + rtX) / 2;
            var ctY = (ltY + rtY) / 2;

            PointF pointRP = ImageUsedMath.FindDistancePoint(new System.Drawing.PointF(ctX, ctY), object_radius * 4, this.rotatedRect.Angle - 90); // 회전점

            if (ImageUsedMath.DistanceToPoint(mouse_pt, pointLT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftTop;            //  Left,    Top 점 선택
            else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointLB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftBottom;         //  Left, Bottom 점 선택
            else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightTop;           // Right,    Top 점 선택
            else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightBottom;        // Right, Bottom 점 선택

            else if (ImageUsedMath.DistanceToPoint(mouse_pt, pointRP) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Rotation;               // 위쪽 회전             
            else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, new PointF(this.rotatedRect.Center.X, this.rotatedRect.Center.Y), pointRP) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.Rotation;               // 위쪽 회전 

            else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointLT, pointLB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaLeftMiddle;
            else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointLB, pointRB) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaBottomMiddle;
            else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointRB, pointRT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaRightMiddle;
            else if (ImageUsedMath.FindDistanceToSegmentSquared(mouse_pt, pointRT, pointLT) < over_dist_squared) this.NodeSelected = ROISHAPEPOSTION.AreaTopMiddle;

            else if (DoesRectangleContainPoint(this.rotatedRect, mouse_pt)) this.NodeSelected = ROISHAPEPOSTION.Inside;
                    
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

    #endregion 마우스 위치에 따른 Node 선택 함수, 마우스 커서 모양 리턴 함수

    }
}
