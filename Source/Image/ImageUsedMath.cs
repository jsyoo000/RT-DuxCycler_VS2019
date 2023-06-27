using System;

namespace Duxcycler_IMAGE
{

    /** @class: IRIS에서 사용하시는 수학 함수 모음
     *  @section: Description
     *      IRIS에서 사용하는 수학 함수 모음 클래스 이다. 
     */ 
    public static class ImageUsedMath
    {
        /** @brief: 두점 사이의 각도를 구한다.
         *  @section: Description
         *      두점 사이의 각도(return값은 표시용 Angle(0~90도)이고 realAngle은 0~360도 값이다.
         *  @param:     start       기준이 되는 점( System.Drawing.PointF 변수 )
         *  @param:     end         각도를 확인할 점( System.Drawing.PointF 변수 )
         *  @param:     realAngle   실제 각도 ( 0도~360도 ) : Output 변수
         *  @rerurn:    표시용 각도 (0도~90도)
         */
        public static float AngleBetween(System.Drawing.PointF start, System.Drawing.PointF end, out float realAngle)
        {
            float dy = end.Y - start.Y;
            float dx = end.X - start.X;
            float angle = RadianToDegree((float)Math.Atan(dy / dx) );
            realAngle = angle;

            if (dx < 0.0)
            {
                realAngle += 180.0f;
            }
            else
            {
                if (dy < 0.0) realAngle += 360.0f;
            }

            return Math.Abs(angle % 90);
        }

        /** @brief: 각도(Degree)를 라디안(Radian)으로 변한한다.
         *  @section: Description
         *      입력된 각도(Degree)를 라디안(Radian)으로 변환하는 함수로 float값을 리턴한다.
         *  @param:     degree  변환할 각도(Degree)( float변수 )
         *  @rerurn:    라디안(Radian) 값으로 float형이다.
         */
        public static float DegreeToRadian(float degree) { return (float)(degree * (Math.PI / 180.0)); }

        /** @brief: 라디안(Radian)을 각도(Degree)로 변한한다.
         *  @section: Description
         *      입력된 라디안(Radian)을 각도(Degree)로 변환하는 함수로 float값을 리턴한다.
         *  @param:     radian  변환할 라디안(Radian)( float변수 )
         *  @rerurn:    각도(Degree) 값으로 float형이다.
         */
        public static float RadianToDegree(float radian) { return (float)(radian * (180.0 / Math.PI)); }

        /** @brief: 두 점의 거리를 구한다.
        *   @section: Description
        *       입력된 두 점의 최단거리를 구하는 함수로 float값를 리턴한다.
        *       
        *   @param:   p1    첫번째 점( System.Drawing.PointF 변수 )
        *   @param:   p2    두번째 점( System.Drawing.PointF 변수 )
        *   @return:  두 점의 거리값으로 float형식이다.
        */
        public static float DistanceToPoint(System.Drawing.PointF p1, System.Drawing.PointF p2) { return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)); }

        /** @brief: 선분과 점의 거리를 구한다.
         *  @section: Description
         *      선분(p1, p2)와 점(pt)의 가장 가까운 거리를 구한다.
         *  @param:     pt  선분과 거리를 구할 점( System.Drawing.PointF 변수 )
         *  @param:     p1  선분의 첫번째 점( System.Drawing.PointF 변수 )
         *  @param:     p2  선분의 두번째 점( System.Drawing.PointF 변수 )
         *  @rerurn:    가장 가까운 거리 값으로 float형이다.
         */
        public static float FindDistanceToSegmentSquared(System.Drawing.PointF pt, System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            System.Drawing.PointF closest;
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))     // 선분이 아닌 점입니다.
            {
                // It's a point not a line segment.
                closest = p1;
                return DistanceToPoint(closest, pt);
            }

            // 거리를 최소화하는 t를 계산합니다.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);

            // 이것이 세그먼트 중 하나를 나타내는 지 확인
            // 끝점 또는 중간 점.
            if (t < 0)
            {
                closest = new System.Drawing.PointF(p1.X, p1.Y);
            }
            else if (t > 1)
            {
                closest = new System.Drawing.PointF(p2.X, p2.Y);
            }
            else
            {
                closest = new System.Drawing.PointF(p1.X + t * dx, p1.Y + t * dy);
            }

            return DistanceToPoint(closest, pt);
            //return dx * dx + dy * dy;
        }

        /** @brief: 기준 점에서 각도(degree), 거리에 해당하는 점를 구한다.
         *  @section: Description
         *      기준 점에서 주어진 각도(degree)로 주어진 거리의 점을 구한다.
         *  @param:     pt          기준 점( System.Drawing.PointF 변수 )
         *  @param:     distance    기준 점과 떨어진 거리 ( float 변수 )
         *  @param:     degree      기준 점과의 각도(degree)( float 변수 )
         *  @rerurn:    기준 점에서 각도(degree), 거리에 해당하는 점( System.Drawing.PointF 변수 )
         */
        public static System.Drawing.PointF FindDistancePoint(System.Drawing.PointF pt, float distance, float degree)
        {
            // 구할 점
            System.Drawing.PointF findPoint = new System.Drawing.PointF();

            
            findPoint.X = pt.X + (distance * (float)Math.Cos(DegreeToRadian(degree)));
            findPoint.Y = pt.Y + (distance * (float)Math.Sin(DegreeToRadian(degree)));
            
            return findPoint;
        }

        /** @brief: 두점을 지나는 직선과 점이 수직하는 교점을 구한다.
         *  @section: Description
         *      두점(p1, p2)을 지나는 직선과 점(pt)이 수직하는 교점을 구한다.
         *  @param:     pt      직선과 수직점을 구할 점( System.Drawing.PointF 변수 )
         *  @param:     p1      직선이 통과하는 첫번째 점( System.Drawing.PointF 변수 )
         *  @param:     p2      직선이 통과하는 첫번째 점( System.Drawing.PointF 변수 )
         *  @rerurn:    수직하는 교점( System.Drawing.PointF 변수 )
         */
        public static System.Drawing.PointF FindPointToSegmentSquared(System.Drawing.PointF pt, System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            // 구할 교점
            System.Drawing.PointF closest = new System.Drawing.PointF();
                        
            if(p1.X == p2.X)        // 직선이 수직인 경우
            {
                closest.X = p1.X;
                closest.Y = pt.Y;
            }
            else if(p1.Y == p2.Y)   // 직선이 수평인 경우
            {
                closest.X = pt.X;
                closest.Y = p1.Y;
            }
            else                    // 그 외의 경우
            {
                // 기울기 m1
                float m1 = (p1.Y - p2.Y) / (p1.X - p2.X);
                // 상수 k1
                float k1 = -m1 * p1.X + p1.Y;

                // 이제 선분 l 을 포함하는 직선의 방정식은 y = m1x + k1 이 구해졌습니다.
                // 남은 것은 점 p 를 지나고 위의 직선과 직교하는 직선의 방정식을 구해 봅시다.
                // 두 직선은 직교하기 때문에 m1 * m2 = -1 입니다.

                // 기울기 m2
                float m2 = -1.0f / m1;
                // p 를 지나기 때문에 yp = m2 * xp + k2 => k2 = yp - m2 * xp
                float k2 = pt.Y - m2 * pt.X;

                // 두 직선 y = m1x + k1, y = m2x + k2 의 교점을 구한다
                closest.X = (k2 - k1) / (m1 - m2);
                closest.Y = m1 * closest.X + k1;
            }

            return closest;
        }

        /** @brief: 직선과 점이 수직하는 교점을 구한다.
         *  @section: Description
         *      직선(각도, 점)과 점이 수직하는 점을 구한다.
         *  @param:     pt      직선과 수직점을 구할 점( System.Drawing.PointF 변수 )
         *  @param:     p1      직선이 통과하는 점( System.Drawing.PointF 변수 )
         *  @param:     degree  직선의 각도(degree)( float 변수 )
         *  @rerurn:    수직하는 교점( System.Drawing.PointF 변수 )
         */
        public static System.Drawing.PointF FindPointToSegmentSquared(System.Drawing.PointF pt, System.Drawing.PointF p1, float degree)
        {
            // 구할 교점
            System.Drawing.PointF closest = new System.Drawing.PointF();

            if ((degree % 180F) == 0)      // 직선이 수평인 경우
            {
                closest.X = pt.X;
                closest.Y = p1.Y;
            }   
            else if ((degree % 90F) == 0)  // 직선이 수직인 경우
            {
                closest.X = p1.X;
                closest.Y = pt.Y;
            }
            else                    // 그 외의 경우
            {
                // 기울기 m1
                float m1 = (float)Math.Tan(DegreeToRadian(degree));
                // 상수 k1
                float k1 = -m1 * p1.X + p1.Y;

                // 이제 선분 l 을 포함하는 직선의 방정식은 y = m1x + k1 이 구해졌습니다.
                // 남은 것은 점 p 를 지나고 위의 직선과 직교하는 직선의 방정식을 구해 봅시다.
                // 두 직선은 직교하기 때문에 m1 * m2 = -1 입니다.

                // 기울기 m2
                float m2 = -1.0f / m1;
                // p 를 지나기 때문에 yp = m2 * xp + k2 => k2 = yp - m2 * xp
                float k2 = pt.Y - m2 * pt.X;

                // 두 직선 y = m1x + k1, y = m2x + k2 의 교점을 구한다
                closest.X = (k2 - k1) / (m1 - m2);
                closest.Y = m1 * closest.X + k1;
            }

            return closest;
        }

        /** @brief: 기준점을 기준으로 회전한 점을 구한다.
         *  @section: Description
         *      기준점(pivot)을 기준으로 입력한 각도(radian)만큼 회전한 점을 구한다.
         *  @param:     point      회전할 점( System.Drawing.PointF 변수 )
         *  @param:     pivot      회전의 기준점( System.Drawing.PointF 변수 )
         *  @param:     degree     점의 회전 각도(degree)( float 변수 )
         */
        public static System.Drawing.PointF Rotate(System.Drawing.PointF point, System.Drawing.PointF pivot, float degree)
        {
            float x = point.X - pivot.X;
            float y = point.Y - pivot.Y;
            double a = Math.Atan(y / x);
            if (x < 0)
            {
                a += Math.PI;
            }
            float size = (float)Math.Sqrt(x * x + y * y);

            double newAngel = a + DegreeToRadian(degree);
            float newX = ((float)Math.Cos(newAngel) * size);
            float newY = ((float)Math.Sin(newAngel) * size);
            return pivot + new System.Drawing.SizeF(newX, newY);
        }

        /** @brief: Ellipe 회전 그리기
         *  @section: Description
         *      Ellipe를 입력된 각도(dergee)만큼 회전해서 그린다..
         *  @param:     g           그려질 위치의 GDI+ 변수
         *  @param:     pen         그릴 pen
         *  @param:     center      Ellipe의 중심점( System.Drawing.Point 변수 )
         *  @param:     size        Ellipe의 크기( System.Drawing.Size 변수 )
         *  @param:     dergee      회전 각도(dergee)
         *  @param:     IsFilled    Ellipe를 채울지 설정
         *  @param:     FillColor   Ellipe를 채울때 색 설정
        */
        public static void DrawEllipse(System.Drawing.Graphics graphics, System.Drawing.Pen pen, System.Drawing.Point center, System.Drawing.Size size, float dergee, bool IsFilled, System.Drawing.Color FillColor)
        {
            int h2 = size.Height / 2;
            int w2 = size.Width / 2;
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point(center.X - w2, center.Y - h2), size);

            graphics.TranslateTransform(center.X, center.Y);
            graphics.RotateTransform(dergee);
            graphics.TranslateTransform(-center.X, -center.Y);
            if (IsFilled) graphics.FillEllipse(new System.Drawing.SolidBrush(FillColor), rect);     //  Ellipe를 채울때
            graphics.DrawEllipse(pen, rect);
            graphics.ResetTransform();
        }

        /** @brief: 두 선분의 교차점 구하기
         *  @section: Description
         *      두 선분의 교차점 구하기, 두 선분이 만나지 않으면 false         
         *  @param:     AP1      A 선분 첫번째 점( System.Drawing.PointF 변수 )
         *  @param:     AP2      A 선분 두번째 점( System.Drawing.PointF 변수 )
         *  @param:     BP1      B 선분 첫번째 점( System.Drawing.PointF 변수 )
         *  @param:     BP2      B 선분 두번째 점( System.Drawing.PointF 변수 )
         *  @param:     IP       두 선분의 교차점( System.Drawing.PointF 변수 ) : output 변수, 교차안하면 new PointF()이다.
         *  @rerurn:    두 선분이 교차하면 true, 안하면 false
         */
        public static bool GetIntersectPoint(System.Drawing.PointF AP1, System.Drawing.PointF AP2, System.Drawing.PointF BP1, System.Drawing.PointF BP2, out System.Drawing.PointF IP)
        {
            float t;
            float s;
            float under = (BP2.Y - BP1.Y) * (AP2.X - AP1.X) - (BP2.X - BP1.X) * (AP2.Y - AP1.Y);
            IP = new System.Drawing.PointF();

            if (under == 0) return false;

            float _t = (BP2.X - BP1.X) * (AP1.Y - BP1.Y) - (BP2.Y - BP1.Y) * (AP1.X - BP1.X);
            float _s = (AP2.X - AP1.X) * (AP1.Y - BP1.Y) - (AP2.Y - AP1.Y) * (AP1.X - BP1.X);

            t = _t / under;
            s = _s / under;

            if (t < 0.0 || t > 1.0 || s < 0.0 || s > 1.0) return false;
            if (_t == 0 && _s == 0) return false;

            IP.X = AP1.X + t * (AP2.X - AP1.X);
            IP.Y = AP1.Y + t * (AP2.Y - AP1.Y);
            return true;
        }
    }
}
