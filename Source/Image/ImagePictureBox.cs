using BitmapControl;
using Duxcycler_Database;
using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Duxcycler_IMAGE
{
    // 일반 Image를 넣고 거기에 ROI 설정, 선택하는 PictureBox
    public partial class ImagePictureBox : PictureBox
    {
        private int selectedIndex = -1;                                     // 선택한 도형의 Index

        #region 클래스 외부설정 변수
        public List<RefROIShape> listShape = new List<RefROIShape>();          // ROI 도형 List
        // Image 기준 Size -> ROI 값의 기준
        public Size ImageSize { get { return this._imageSize; } set { this._imageSize = value; Invalidate(); } }
        private Size _imageSize = new Size(480, 640);

        // ROI DIff 를 보여줄지
        public bool IsShowDiff { get { return this._isshowdiff; } set { this._isshowdiff = value; } }
        private bool _isshowdiff = false;

        // PictureBox 선택 여부
        [Browsable(true)]
        public bool IsSelected { get { return this._isselected; } set { this._isselected = value; Invalidate(); } }
        private bool _isselected = false;

        // PictureBox 선택되었을 경우 Color
        [Browsable(true)]
        public Color SelectedColor { get { return this._selectedcolor; } set { this._selectedcolor = value; Invalidate(); } }
        private Color _selectedcolor = Color.Red;

        // PictureBox의 Label 문구 표시 여부
        public bool IsShowLabel { get { return this._isshowlabel; } set { this._isshowlabel = value; Invalidate(); } }
        private bool _isshowlabel = false;

        // PictureBox의 Label 문구이다. (왼쪽 상단에 표시된다.)
        public string PictureLabel { get { return this._picturelable; } set { this._picturelable = value; Invalidate(); } }
        private string _picturelable = "";

        // PictureBox의 Label 문구 Font이다.
        public Font LableFont { get { return this._labelfont; } set { this._labelfont = value; Invalidate(); } }
        private Font _labelfont = new System.Drawing.Font("Segoe UI", 8F);

        // PictureBox의 Label 배탕색이다.
        public Color LabelBackColor { get { return this._labelbackcolor; } set { this._labelbackcolor = value; Invalidate(); } }
        private Color _labelbackcolor = Color.White;

        // PictureBox의 Label Text 색이다.
        public Color LabelTextColor { get { return this._labeltextcolor; } set { this._labeltextcolor = value; Invalidate(); } }
        private Color _labeltextcolor = Color.Black;
        #endregion

        public ImagePictureBox()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.BackgroundImageLayout = ImageLayout.Center;
            this.UpdateStyles();
        }

        // ROI List에서 해당 마우스 위치에 있는 ROI 선택함수
        private bool MouseIsOverShape(Point mouse_pt, out int selected)
        {
            selected = -1;

            for (int index = 0; index < this.listShape.Count; index++)
            {
                RefROIShape rShape = this.listShape[index];

                if (rShape.GetNodeSelectable(mouse_pt) != ROISHAPEPOSTION.None)
                {
                    selected = index;                    
                }
            }

            if (selected >= 0) return true;
            return false;
        }

        // 마우스 이동 함수
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!IsShowDiff) return;

            this.MouseIsOverShape(e.Location, out int tempIndex);            // 도형 List에서 해당위치의 도형 Index를 찾는다.
            if (IsShowDiff && tempIndex >= 0)
            {
                this.Cursor = this.listShape[tempIndex].GetCursor();     // 선택된 도형의 Cursor 모양을 적용한다.                
            }
            else
            {
                this.Cursor = Cursors.Arrow;                             // 못찾으면 Cursor 모양을 Cross  적용
            }

            this.Invalidate();
        }

        // 마우스 업 함수
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            foreach (var roi in this.listShape)                         
            {
                roi.Selected = false;           // 이전 선택된 ROI를 선택 취소로 설정
                roi.IsShowDiff = false;         // Diff 안보이게
            }
            
            if (IsShowDiff && this.MouseIsOverShape(e.Location, out int sIndex))  
            {
                this.selectedIndex = sIndex;
                this.listShape[sIndex].Selected = true;                     // 선택된 ROI 선택 설정      
                this.listShape[sIndex].IsShowDiff = true;                   // Diff 보이게
            }

            this.Invalidate();
        }


        public void AddRefROI(Point LT, Point RB, int mainIndex, int subIndex, double roiDiff = 0.0, ROISHAPETYPE type = ROISHAPETYPE.Ellipse)
        {
            if (type != ROISHAPETYPE.Ellipse && type != ROISHAPETYPE.Diamond && type != ROISHAPETYPE.Rectangle) return;
            if (LT == null || RB == null) return;

            int imageWidth  = this.ImageSize.Width;
            int ImageHeight = this.ImageSize.Height;

            RefROIShape newShape = new RefROIShape(type)
            {
                ROI_MainIndex = mainIndex,                   // ROI Main Index 설정
                ROI_SubIndex  = subIndex,                    // ROI Sub Index 설정                                         
                Image_Width   = imageWidth,                  // 기준 Image Width 설정
                Image_Height  = ImageHeight,                 // 기준 Image Hedith 설정
                IsFill        = true,                        // ROI 내부를 채울지 설정
                FillColor     = Color.Red,                   // ROI 내부 색
                BorderColor   = Color.White,                   // ROI 테두리 색
                IsShowDiff    = false,                        // ROI 차이값을 보여줄지 설정
                IsShowNumber  = true,                        // ROI 번호를 보여줄지 설정
                Font          = new Font("Segoe UI", 8F),    // ROI 글자 Font 설정
                ForeColor     = Color.Red,                   // ROI 글자 색   
                FontDiff      = new Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),   // ROI Diff 글자 Font 설정
                ForeDiffColor = Color.Yellow,                 // ROI Diff 글자 색   
                ROI_Diff      = roiDiff
            };
                        
            newShape.AddPoint(0, LT); 
            newShape.AddPoint(1, RB);            

            this.listShape.Add(newShape);                  // 도형 List에 추가 
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.IsSelected) ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, this.SelectedColor, ButtonBorderStyle.Solid);

            foreach (RefROIShape cShape in this.listShape)
            {
                cShape.Draw(e.Graphics);
            }

            if (this.selectedIndex >= 0 && this.selectedIndex < this.listShape.Count) this.listShape[this.selectedIndex].Draw(e.Graphics);
        }

        public Bitmap GetBitmap(bool IsBlack = true)
        {
            Bitmap backImage = new Bitmap(this.Width, this.Height);
            using (Graphics g = Graphics.FromImage(backImage))
            {
                // 바탕색 검정으로
                if (IsBlack)    g.FillRectangle(Brushes.Black, 0, 0, this.Width, this.Height);                

                // 이미지 그리기
                if(this.BackgroundImage != null)
                    g.DrawImage(this.BackgroundImage, 0, 0, this.Width, this.Height);


                foreach (RefROIShape cShape in this.listShape)
                {
                    cShape.Draw(g);
                }

                if (this.selectedIndex >= 0 && this.selectedIndex < this.listShape.Count) this.listShape[this.selectedIndex].Draw(g);

                g.Dispose();
            }

            return backImage;
        }
    }
}
