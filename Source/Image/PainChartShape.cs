using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duxcycler_IMAGE
{
    // Pain Chart Image Type
    public enum PAINCHART_IMAGE
    {
        TYPE_ANTERIOR,  // ANTERIOR Type
        TYPE_RTLAT,     // RT LAT Type
        TYPE_FOOT,      // FOOT Type
        TYPE_POSTERIOR, // POSTERIOR Type
        TYPE_LTLAT      // LT LAT Type
    };
        
    public enum DRAWING_TYPE
    {
        TYPE_Ache = 0,    // Ache Type
        TYPE_Burning = 1,    // Burning Type
        TYPE_Stabbing = 2,    // Numbing Type
        TYPE_Numbing = 3,    // PinNeedle Type
        TYPE_PinNeedle = 4,    // Oter Type
        TYPE_Other = 5,    // Other Type
        TYPE_Eraser = 6     // Eraser Type
    };

    public enum DRAWING_PAN_TYPE { PAN_1 = 1, PAN_2 = 2, PAN_4 = 4 }

    public class PainChartShape
    {
        public static Color[] Drawing_Color_Type = new Color[] {
                                                Color.FromArgb(170, 81, 43),    // 0 Ache Type
                                                Color.FromArgb(255, 0, 0),      // 1 Burning Type
                                                Color.FromArgb(255, 163, 0),    // 2 Numbing Type
                                                Color.FromArgb(0, 0, 255),      // 3 PinNeedle Type
                                                Color.FromArgb(0, 255, 0),      // 4 Oter Type
                                                Color.FromArgb(255, 100, 200),  // 5 Other Type
                                                Color.FromArgb(255, 255, 255)   // 6 Other Type
                                            };

        public Point Location;
        public DRAWING_TYPE DrawingType = DRAWING_TYPE.TYPE_Ache;
        public DRAWING_PAN_TYPE DrawingPan = DRAWING_PAN_TYPE.PAN_2;

        public PainChartShape(Point point, DRAWING_TYPE dType = DRAWING_TYPE.TYPE_Ache, DRAWING_PAN_TYPE pan = DRAWING_PAN_TYPE.PAN_1)
        {
            this.Location = point; this.DrawingType = dType; this.DrawingPan = pan;
        }
                
        // 그리는 함수
        public void Draw(Graphics g, Rectangle picRect, Rectangle drawRect, int subMatLeft = 0, int subMatTop = 0, float Zoom_Ratio = 1.0f)
        {
            switch (this.DrawingType)
            {
                case DRAWING_TYPE.TYPE_Ache:        DrawAche(g, picRect, drawRect, subMatLeft, subMatTop, Zoom_Ratio);        break;
                case DRAWING_TYPE.TYPE_Burning:     DrawBurning(g, picRect, drawRect, subMatLeft, subMatTop, Zoom_Ratio);     break;
                case DRAWING_TYPE.TYPE_Stabbing:    DrawStabbing(g, picRect, drawRect, subMatLeft, subMatTop, Zoom_Ratio);    break;
                case DRAWING_TYPE.TYPE_Numbing:     DrawNumbing(g, picRect, drawRect, subMatLeft, subMatTop, Zoom_Ratio);     break;
                case DRAWING_TYPE.TYPE_PinNeedle:   DrawPinNeedle(g, picRect, drawRect, subMatLeft, subMatTop, Zoom_Ratio);   break;
                case DRAWING_TYPE.TYPE_Other:       DrawOther(g, picRect, drawRect, subMatLeft, subMatTop, Zoom_Ratio);       break;
            }
        }
        // TYPE_Ache
        public void DrawAche(Graphics g, Rectangle picRect, Rectangle drawRect, int subMatLeft = 0, int subMatTop = 0, float Zoom_Ratio = 1.0f)
        {
            int picWidth  = picRect.Width;                // 지금 PictureBox의 넓이
            int picHeight = picRect.Height;               // 지금 PictureBox의 높이
            int drawWidth  = drawRect.Width;              // 이미지의 넓이
            int drawHeight = drawRect.Height;             // 이미지의 높이
            
            float conX = (((float)this.Location.X * Zoom_Ratio - (float)subMatLeft) * (float)picWidth / (float)drawWidth);                      // PictureBox의 기준으로 x 값을 변경한다.
            float conY = (((float)this.Location.Y * Zoom_Ratio - (float)subMatTop) * (float)picHeight / (float)drawHeight);                     // PictureBox의 기준으로 y 값을 변경한다.
            
            Point DrawPoint = new Point((int)conX, (int)conY);
            
            Point start = DrawPoint;    Point end   = DrawPoint;
            Pen pen = new Pen(Drawing_Color_Type[(int)DRAWING_TYPE.TYPE_Ache], (int)(1*Zoom_Ratio));
            switch (this.DrawingPan)
            {
                case DRAWING_PAN_TYPE.PAN_1:
                    start.Offset((int)(-1 * Zoom_Ratio), (int)(-1 * Zoom_Ratio)); end.Offset((int)(1 * Zoom_Ratio), (int)(1 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
                case DRAWING_PAN_TYPE.PAN_2:
                    start.Offset((int)(-1 * Zoom_Ratio), (int)(-2 * Zoom_Ratio)); end.Offset((int)(3 * Zoom_Ratio), (int)(2 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(-3 * Zoom_Ratio), (int)(-2 * Zoom_Ratio)); end.Offset((int)(1 * Zoom_Ratio), (int)(2 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
                case DRAWING_PAN_TYPE.PAN_4:
                    start.Offset((int)(-1 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(7 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(-3 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(5 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(-5 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(3 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(-7 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(1 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
            }
        }
        // TYPE_Burning
        public void DrawBurning(Graphics g, Rectangle picRect, Rectangle drawRect, int subMatLeft = 0, int subMatTop = 0, float Zoom_Ratio = 1.0f)
        {
            int picWidth = picRect.Width;                // 지금 PictureBox의 넓이
            int picHeight = picRect.Height;               // 지금 PictureBox의 높이
            int drawWidth = drawRect.Width;              // 이미지의 넓이
            int drawHeight = drawRect.Height;             // 이미지의 높이
            
            float conX = (((float)this.Location.X * Zoom_Ratio - (float)subMatLeft) * (float)picWidth / (float)drawWidth);                      // PictureBox의 기준으로 x 값을 변경한다.
            float conY = (((float)this.Location.Y * Zoom_Ratio - (float)subMatTop) * (float)picHeight / (float)drawHeight);                     // PictureBox의 기준으로 y 값을 변경한다.

            Point DrawPoint = new Point((int)conX, (int)conY);

            Pen pen = new Pen(Drawing_Color_Type[(int)DRAWING_TYPE.TYPE_Burning], (int)(1 * Zoom_Ratio));
            int width = (int)((3 * (int)this.DrawingPan) * Zoom_Ratio);

            Rectangle rectPoint = new Rectangle(DrawPoint.X - (width/2), DrawPoint.Y - (width / 2), width, width);
            this.DrawStar(g, pen, 8, 2, rectPoint);
        }
        // TYPE_Stabbing
        public void DrawStabbing(Graphics g, Rectangle picRect, Rectangle drawRect, int subMatLeft = 0, int subMatTop = 0, float Zoom_Ratio = 1.0f)
        {
            int picWidth = picRect.Width;                // 지금 PictureBox의 넓이
            int picHeight = picRect.Height;               // 지금 PictureBox의 높이
            int drawWidth = drawRect.Width;              // 이미지의 넓이
            int drawHeight = drawRect.Height;             // 이미지의 높이

            float conX = (((float)this.Location.X * Zoom_Ratio - (float)subMatLeft) * (float)picWidth / (float)drawWidth);                      // PictureBox의 기준으로 x 값을 변경한다.
            float conY = (((float)this.Location.Y * Zoom_Ratio - (float)subMatTop) * (float)picHeight / (float)drawHeight);                     // PictureBox의 기준으로 y 값을 변경한다.

            Point DrawPoint = new Point((int)conX, (int)conY);

            Point start = DrawPoint; Point end = DrawPoint;
            Pen pen = new Pen(Drawing_Color_Type[(int)DRAWING_TYPE.TYPE_Stabbing], (int)(1 * Zoom_Ratio));
            switch (this.DrawingPan)
            {
                case DRAWING_PAN_TYPE.PAN_1:
                    start.Offset(0, (int)(-1 * Zoom_Ratio)); end.Offset(0, (int)(1 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
                case DRAWING_PAN_TYPE.PAN_2:
                    start.Offset((int)(1 * Zoom_Ratio),(int)(-2 * Zoom_Ratio)); end.Offset((int)(1 * Zoom_Ratio), (int)(2 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(-1 * Zoom_Ratio), (int)(-2 * Zoom_Ratio)); end.Offset((int)(-1 * Zoom_Ratio), (int)(2 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
                case DRAWING_PAN_TYPE.PAN_4:
                    start.Offset((int)(3 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(3 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(1 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(1 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(-1 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(-1 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(-3 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(-3 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
            }
        }
        // TYPE_Numbing
        public void DrawNumbing(Graphics g, Rectangle picRect, Rectangle drawRect, int subMatLeft = 0, int subMatTop = 0, float Zoom_Ratio = 1.0f)
        {
            int picWidth = picRect.Width;                // 지금 PictureBox의 넓이
            int picHeight = picRect.Height;               // 지금 PictureBox의 높이
            int drawWidth = drawRect.Width;              // 이미지의 넓이
            int drawHeight = drawRect.Height;             // 이미지의 높이

            float conX = (((float)this.Location.X * Zoom_Ratio - (float)subMatLeft) * (float)picWidth / (float)drawWidth);                      // PictureBox의 기준으로 x 값을 변경한다.
            float conY = (((float)this.Location.Y * Zoom_Ratio - (float)subMatTop) * (float)picHeight / (float)drawHeight);                     // PictureBox의 기준으로 y 값을 변경한다.

            Point DrawPoint = new Point((int)conX, (int)conY);

            Brush brush = new SolidBrush(Drawing_Color_Type[(int)DRAWING_TYPE.TYPE_Numbing]);
            Pen pen = new Pen(Drawing_Color_Type[(int)DRAWING_TYPE.TYPE_Numbing], (int)(1 * Zoom_Ratio));
            Point point = DrawPoint;

            int count = 2 +  (int)this.DrawingPan;

            point.Offset((int)(-2 * count / 2 * Zoom_Ratio),(int)(-3 * count / 2 * Zoom_Ratio));

            for(int y = 0; y < count; y++)
            {
                for (int x = 0; x< count; x++)
                {
                    Point drawPoint = point;
                    drawPoint.Offset((int)(x * 2 * Zoom_Ratio), (int)(y * 3 * Zoom_Ratio));
                    Rectangle rectPoint = new Rectangle(drawPoint.X, drawPoint.Y, (int)(1 * Zoom_Ratio), (int)(1 * Zoom_Ratio));
                    g.DrawEllipse(pen, rectPoint);
                }
            }
        }
        // TYPE_PinNeedle
        public void DrawPinNeedle(Graphics g, Rectangle picRect, Rectangle drawRect, int subMatLeft = 0, int subMatTop = 0, float Zoom_Ratio = 1.0f)
        {
            int picWidth = picRect.Width;                // 지금 PictureBox의 넓이
            int picHeight = picRect.Height;               // 지금 PictureBox의 높이
            int drawWidth = drawRect.Width;              // 이미지의 넓이
            int drawHeight = drawRect.Height;             // 이미지의 높이

            float conX = (((float)this.Location.X * Zoom_Ratio - (float)subMatLeft) * (float)picWidth / (float)drawWidth);                      // PictureBox의 기준으로 x 값을 변경한다.
            float conY = (((float)this.Location.Y * Zoom_Ratio - (float)subMatTop) * (float)picHeight / (float)drawHeight);                     // PictureBox의 기준으로 y 값을 변경한다.

            Point DrawPoint = new Point((int)conX, (int)conY);
                        
            Point start = DrawPoint; Point end = DrawPoint;
            Pen pen = new Pen(Drawing_Color_Type[(int)DRAWING_TYPE.TYPE_PinNeedle], (int)(1 * Zoom_Ratio));
            switch (this.DrawingPan)
            {
                case DRAWING_PAN_TYPE.PAN_1:
                    start.Offset((int)(1 * Zoom_Ratio), (int)(-1 * Zoom_Ratio)); end.Offset((int)(-1 * Zoom_Ratio), (int)(1 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
                case DRAWING_PAN_TYPE.PAN_2:
                    start.Offset((int)(3 * Zoom_Ratio), (int)(-2 * Zoom_Ratio)); end.Offset((int)(-1 * Zoom_Ratio), (int)(2 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(1 * Zoom_Ratio), (int)(-2 * Zoom_Ratio)); end.Offset((int)(-3 * Zoom_Ratio), (int)(2 *Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
                case DRAWING_PAN_TYPE.PAN_4:
                    start.Offset((int)(7 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(-1 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(5 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(-3 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(3 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(-5 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);

                    start = DrawPoint; end = DrawPoint;
                    start.Offset((int)(1 * Zoom_Ratio), (int)(-4 * Zoom_Ratio)); end.Offset((int)(-7 * Zoom_Ratio), (int)(4 * Zoom_Ratio));
                    g.DrawLine(pen, start, end);
                    break;
            }
        }
        // TYPE_Other
        public void DrawOther(Graphics g, Rectangle picRect, Rectangle drawRect, int subMatLeft = 0, int subMatTop = 0, float Zoom_Ratio = 1.0f)
        {
            int picWidth = picRect.Width;                // 지금 PictureBox의 넓이
            int picHeight = picRect.Height;               // 지금 PictureBox의 높이
            int drawWidth = drawRect.Width;              // 이미지의 넓이
            int drawHeight = drawRect.Height;             // 이미지의 높이

            float conX = (((float)this.Location.X * Zoom_Ratio - (float)subMatLeft) * (float)picWidth / (float)drawWidth);                      // PictureBox의 기준으로 x 값을 변경한다.
            float conY = (((float)this.Location.Y * Zoom_Ratio - (float)subMatTop) * (float)picHeight / (float)drawHeight);                     // PictureBox의 기준으로 y 값을 변경한다.

            Point DrawPoint = new Point((int)conX, (int)conY);
            
            Point BottomLeft  = DrawPoint; 
            Point BottomRight = DrawPoint;
            Point Top         = DrawPoint;
            Pen pen = new Pen(Drawing_Color_Type[(int)DRAWING_TYPE.TYPE_Other], (int)(1 * Zoom_Ratio));
            int lineWidth = (int)((int)this.DrawingPan * Zoom_Ratio);

            BottomLeft.Offset(lineWidth, lineWidth); BottomRight.Offset(-1 * lineWidth, lineWidth); Top.Offset(0, -1 * lineWidth);
                        
            g.DrawPolygon(pen, new Point[] { BottomLeft, BottomRight, Top });
        }

        // Draw the star.
        private void DrawStar(Graphics gr, Pen the_pen, int num_points, int skip, Rectangle rect)
        {
            // Get the star's points.
            PointF[] star_points =
                MakeStarPoints(-Math.PI / 2, num_points, skip, rect);

            // Draw the star.
            //gr.FillPolygon(the_brush, star_points);
            gr.DrawPolygon(the_pen, star_points);
        }

        // Generate the points for a star.
        private PointF[] MakeStarPoints(double start_theta, int num_points, int skip, Rectangle rect)
        {
            double theta, dtheta;
            PointF[] result;
            float rx = rect.Width / 2f;
            float ry = rect.Height / 2f;
            float cx = rect.X + rx;
            float cy = rect.Y + ry;

            // If this is a polygon, don't bother with concave points.
            if (skip == 1)
            {
                result = new PointF[num_points];
                theta = start_theta;
                dtheta = 2 * Math.PI / num_points;
                for (int i = 0; i < num_points; i++)
                {
                    result[i] = new PointF(
                        (float)(cx + rx * Math.Cos(theta)),
                        (float)(cy + ry * Math.Sin(theta)));
                    theta += dtheta;
                }
                return result;
            }

            // Find the radius for the concave vertices.
            double concave_radius =
                CalculateConcaveRadius(num_points, skip);

            // Make the points.
            result = new PointF[2 * num_points];
            theta = start_theta;
            dtheta = Math.PI / num_points;
            for (int i = 0; i < num_points; i++)
            {
                result[2 * i] = new PointF(
                    (float)(cx + rx * Math.Cos(theta)),
                    (float)(cy + ry * Math.Sin(theta)));
                theta += dtheta;
                result[2 * i + 1] = new PointF(
                    (float)(cx + rx * Math.Cos(theta) * concave_radius),
                    (float)(cy + ry * Math.Sin(theta) * concave_radius));
                theta += dtheta;
            }
            return result;
        }

        // Calculate the inner star radius.
        private double CalculateConcaveRadius(int num_points, int skip)
        {
            // For really small numbers of points.
            if (num_points < 5) return 0.33f;

            // Calculate angles to key points.
            double dtheta = 2 * Math.PI / num_points;
            double theta00 = -Math.PI / 2;
            double theta01 = theta00 + dtheta * skip;
            double theta10 = theta00 + dtheta;
            double theta11 = theta10 - dtheta * skip;

            // Find the key points.
            PointF pt00 = new PointF(
                (float)Math.Cos(theta00),
                (float)Math.Sin(theta00));
            PointF pt01 = new PointF(
                (float)Math.Cos(theta01),
                (float)Math.Sin(theta01));
            PointF pt10 = new PointF(
                (float)Math.Cos(theta10),
                (float)Math.Sin(theta10));
            PointF pt11 = new PointF(
                (float)Math.Cos(theta11),
                (float)Math.Sin(theta11));

            // See where the segments connecting the points intersect.
            FindIntersection(pt00, pt01, pt10, pt11,
                out bool lines_intersect, out bool segments_intersect,
                out PointF intersection, out PointF close_p1, out PointF close_p2);

            // Calculate the distance between the
            // point of intersection and the center.
            return Math.Sqrt(
                intersection.X * intersection.X +
                intersection.Y * intersection.Y);
        }

        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        private void FindIntersection( PointF p1, PointF p2, PointF p3, PointF p4, out bool lines_intersect, out bool segments_intersect,
            out PointF intersection,
            out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }
    }
}
