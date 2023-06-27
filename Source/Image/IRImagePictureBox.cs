using BitmapControl;
using Duxcycler_Database;
using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Duxcycler_IMAGE
{
    public enum SELECTED_TYPE
    {
        Selected,
        Leave,
        Enter
    };

    // Grid Line Enum
    public enum GRIDLINE_TYPE { Grid_None, Grid_2x2, Grid_5x5, Grid_21x21 }

    /** @class ROI 정보 넘기는 클래스 */
    public class ROIInfoEventArgs : System.EventArgs
    {
        public List<ROIShape> Rois = new List<ROIShape>();                       /** ROI 도형 정보 저장하는 List 변수 */
    }

    /** @class 자른 이미지 정보 넘기는 클래스 */
    public class CropImageInfoEventArgs : System.EventArgs
    {
        public ImageInfo CropImage { get; set; }                /** 자른 이미지 정보를 저장하는 변수 */
    }

    /** @class Bring 정보 넘기는 클래스 
     *  @section: Description
     *      Image의 그린 Line에 해당하는 온도값을 전달하는 클래스이다.
     */
    public class BringInfoEventArge : System.EventArgs
    {
        public Point nowPoint;                                  /** 마우스 위치       */
        public byte nowValue;                                   /** 마우스 위치의 값  */
        public Point startPoint;                                /** Bring 시작 위치   */
        public Point endPoint;                                  /** Bring 끝 위치     */
        public int Distance = 0;                                /** 두점 사이의 거리  */
        public byte[] bringValues;                              /** Bring Value Array */
    }

    /** @class IR Image를 처리하느 클래스로 PictureBox를 상속했다. */
    public partial class IRImagePictureBox : PictureBox
    {
    #region 클래스 내부 변수

        BitmapImageCtl bitmapimageCtl = new BitmapImageCtl();               /** Image 처리시 클래스(Palette 생성 및 적용, Bitmap 생성) */

        #region ROI 그리기, 이동, 변경시 사용 변수 모듬

        private int pointIndex = 0;                                  /** ROI를 그릴 때 사용한다. 저장할 ROI의 점 Index 이다. */
        private int selectedIndex = -1;                                 /** 선택한 ROI 도형의 Index 이다. */

        private PointF movePoint;                                           /**Image 기준의 임시 Point 변수 */
        private Point picMovePoint;                                         /** pictureBox 기준의 임시 Point 변수(Zoom, Crop, Bring) */

        private ROIShape newShape = null;                               /** 새롭게 그리고 있는 ROI 도형 변수 */

        private bool IsDrawClone = false;                              /**  Clone ROI를 그릴지 여부 */

        private ROIShape backShape = null;                               /** 이동 전의 ROI 도형 정보를 저장하는 변수 */
        private bool IsDrawBack = false;                              /** backShape를 표시할지 설정 변수 */
        private Color undoBorderColor = Color.White;                        /** backShape에 저장전에 Border Color값 저장해서 나중에 복원시 색을 되돌리기 위함. */
        private Color undoFontColor = Color.White;                        /** backShape에 저장전에 Font Color값 저장해서 나중에 복원시 색을 되돌리기 위함. */

        #endregion ROI 그리기, 이동, 변경시 사용 변수 모듬

        #region 이미지 자르기에 사용하는 변수 모음

        private Point cropPoint;                                            /** Image Crop 시작 Point      */
        private bool IsCropShape = false;                                   /** Image Crop Shape 표시 여부 */
        private bool IsCropCompleted = false;                               /** Image Crop 완료 여부       */
        private Rectangle CropRect;                                         /** Image Crop할 사각형 영역   */

        #endregion 이미지 자르기에 사용하는 변수 모음

        #region Bring에 사용하는 변수 모음

        private Point bringPoint;                                           /** Bring 시작 point            */
        private bool IsBringView = false;                                   /** Bring Line을 표시할지 여부  */
        private Point bringSP;                                              /** Bring 완료 시작 점          */
        private Point bringEP;                                              /** Bring 완료 끝 점            */
        private bool IsBringCompleted = false;                              /** Bring 완료 여부             */

        #endregion Bring에 사용하는 변수 모음

        #region Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 변수 모음

        int subMatLeft = 0;                                  /** Pan시 Left 위치 저장 변수*/
        int subMatTop = 0;                                  /** Pan시 Right 위치 저장 변수*/

        private bool IsZoomMove = false;                              /** 마우스 다운으로 Zoom 이미지를 이동할 수 있는지 여부 변수 */
        private Point zoomCenterOffset = new Point(0, 0);                    /** Zoom 이미지의 Center point의 Offset이다.  */

        #endregion Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 변수 모음

        #region 돋보기 기능 관련 변수

        double Mag_Ratio = 4.0f;                                            /** 돋보기 비율 저장 변수  */
        bool ShowMsg = false;                                           /** 돋보기 Image를 보여줄지 여부 설정 변수  */

        #endregion 돋보기 기능 관련 변수

        #region ImageROIGrid에 사용할 변수 모음

        private ImageROIGrid SelectImageGrid = null;                         /** 선택된 ImageROIGrid 저장 변수 */
        public List<ImageROIGrid> listImageGrid = new List<ImageROIGrid>();  /** ImageROIGrid를 저장할 List 변수 */

        #endregion ImageROIGrid에 사용할 변수 모음

        private MultiSelectedROIGuide multiSelectedROIGuide = null;         /** 멀티 선택용 Guide */
        private bool IsMultiSelected = false;                               /** ROI를 여러개 선택하는 모드인지 설정 변수  */
        private bool IsDrawMultiSelectedRect = false;                       /** 멀티 선택용 Rect 그리기 */
        private PointF MultiSelectStartPoint;                               /** 멀티 선택용 Rect 시작 위치 Iamge 기준으로 저장 */

    #endregion 클래스 내부 변수

    #region 클래스 외부설정 변수

        #region override 변수 모음

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The font used to display text in the control.")]
        public override Font Font { get { return base.Font; } set { base.Font = value; } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("The color used to display text in the control.")]
        public override Color ForeColor { get { return base.ForeColor; } set { base.ForeColor = value; } }

        #endregion override override

        /** PictureBox에 Grid Line Type 설정 변수 */
        [Browsable(true)]
        public GRIDLINE_TYPE GridLineType
        {
            get { return this.gridlinetype; }
            set { this.gridlinetype = value; }
        }
        private GRIDLINE_TYPE gridlinetype = GRIDLINE_TYPE.Grid_None;

        /** PictureBox에서 사용할 ImageInfo를 저장하는 변수 : ImageInfo Class내부에 listShape를 사용한다. */
        public ImageInfo ImageInfo
        {
            get { return this.imageinfo; }
            set
            {
                this.imageinfo = value;
                this.Invalidate();
            }
        }
        private ImageInfo imageinfo = null;

        /** PictureBox에 cloneShape를 설정하는 변수(선택 또는 그린 ROI를 복제하는 변수)*/
        public ROIShape CloneShape
        {
            get { return this.cloneShape; }
            set
            {
                this.cloneShape = value;

                if (this.cloneShape != null && !this.IsPan)     // cloneShape가 Null아니고 pan모드가 아닌경우
                {
                    if (this.ImageInfo != null)
                    {
                        this.cloneShape.Image_Width = this.ImageInfo.Image_Width;
                        this.cloneShape.Image_Height = this.ImageInfo.Image_Height;
                    }

                    this.IsDrawClone = true;                    // cloneShape 표시
                }
                else                                            // cloneShape가 Null이거나 pan모드인 경우                   
                    this.IsDrawClone = false;                   // cloneShape 표시 안함.
            }
        }
        private ROIShape cloneShape = null;

        /** PictureBox에 Popup 메뉴를 보일지 설정 변수  */
        public bool MenuShow
        {
            get { return this.IsMenuShow; }
            set { this.IsMenuShow = value; }
        }
        private bool IsMenuShow = true;

        /** PictureBox Focus 설정 변수*/
        public bool FocusEnalbe
        {
            get { return this.isFocus; }
            set { this.isFocus = value; }
        }
        private bool isFocus = true;

        /** PictureBox를 선택 여부 설정 변수*/
        public SELECTED_TYPE SelectedType
        {
            get { return this.selectedtype; }
            set { this.selectedtype = value; this.Invalidate(); }
        }
        private SELECTED_TYPE selectedtype = SELECTED_TYPE.Leave;

        /** PictureBox에 ROI를 보여줄지 설정 변수( 기본 보여준다. ) */
        [Browsable(true)]
        public bool IsROIView
        {
            get { return this._isroiview; }
            set
            {
                this.Cursor = Cursors.Default;
                if (this._isroiview != value)
                {
                    this._isroiview = value;

                    if (value)                                                  // true 경우에는 Zoom모드 설정, ROI 모드 취소  
                    {
                        if (!this.IsPan && !this.IsMag)                         // pan, mag 모드가 아닌경우
                        {
                            this.IsDrawBack = true;                             // backShape 표시
                            this.IsDrawClone = true;                            // cloneShape 표시
                            this.MouseDown -= this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 중복 방지을 위해 빼고 더한다.
                            this.MouseMove -= this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 중복 방지을 위해 빼고 더한다.

                            this.MouseDown += this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 적용  
                        }
                    }
                    else                                                        // false 경우에는 Zoom모드 취소, ROI 모드 설정
                    {
                        this.IsDrawBack = false;                                // backShape 표시안함.
                        this.IsDrawClone = false;                               // cloneShape 표시안함.
                        this.CloneShape = null;                                 // CloneShape 삭제
                        this.MouseDown -= this.OnMouseDown_NotDrawing;          // 평소 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_NotDrawing;          // 평소 마우스 이동 함수 취소                    
                    }
                    this.Invalidate();
                }
            }
        }
        private bool _isroiview = false;

        /** PictureBox에 Pan 기능 설정 변수*/
        [Browsable(true)]
        public bool IsPan
        {
            get { return this._ispan; }
            set
            {
                this.Cursor = Cursors.Default;
                if (this._ispan != value)
                {
                    this._ispan = value;

                    if (value)                                              // true 경우에는
                    {
                        this.IsDrawBack = false;                            // backShape 표시안함.
                        this.IsDrawClone = false;                           // cloneShape 표시안함.
                        this.IsCrop = false;                                // 자르기 모드 취소
                        this.IsMag = false;                                 // 돋보기 모드 취소
                        this.MouseDown -= this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 취소        
                        this.MouseMove -= this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 적용
                        // 중복 방지를 위해 빼고 더한다.
                        this.MouseDown -= this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 취소

                        this.MouseDown += this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 적용
                        this.MouseMove += this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 적용
                        this.MouseUp += this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 적용        
                    }
                    else                                                    // false 경우에는 
                    {
                        this.MouseDown -= this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 취소

                        if (this.IsROIView)                                 // ROIView인 경우에 
                        {
                            this.IsDrawBack = true;                         // backShape 표시함.
                            this.IsDrawClone = true;                        // cloneShape 표시함.
                            // 중복 방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_NotDrawing;  // 평소 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_NotDrawing;  // 평소 마우스 이동 함수 취소

                            this.MouseDown += this.OnMouseDown_NotDrawing;  // 평소 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_NotDrawing;  // 평소 마우스 이동 함수 적용                    
                        }
                    }
                }
            }
        }
        private bool _ispan = false;

        /** PictureBox에 Zoom 기능 설정 변수*/
        [Browsable(true)]
        public bool IsZoom
        {
            get { return this._iszoom; }
            set { this._iszoom = value; }
        }
        private bool _iszoom = false;

        /** PictureBox에 돋보기 기능 설정 변수*/
        [Browsable(true)]
        public bool IsMag
        {
            get { return this._ismag; }
            set
            {
                if (this._ismag != value)
                {
                    this._ismag = value;

                    if (value)                                              // true 경우에는
                    {
                        this.IsDrawBack = false;                            // backShape 표시안함.
                        this.IsDrawClone = false;                           // cloneShape 표시안함.
                        this.IsCrop = false;                                // 자르기 모드 취소
                        this.IsPan = false;                                 // Pan 모드 취소
                        this.MouseDown -= this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 취소        
                        this.MouseDown -= this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 취소   
                        // 중복 방지를 위해 빼고 더한다.
                        this.MouseMove -= this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 취소  

                        this.MouseMove += this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 적용
                    }
                    else                                                    // false 경우에는 
                    {
                        this.Cursor = Cursors.Default;                      // 커서 기본 모양으로

                        this.MouseMove -= this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 취소                    

                        if (this.IsROIView)                                 // ROIView인 경우에 설정
                        {
                            this.IsDrawBack = true;                         // backShape 표시함.
                            this.IsDrawClone = true;                        // cloneShape 표시함.
                            // 중복 방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_NotDrawing;  // 평소 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_NotDrawing;  // 평소 마우스 이동 함수 취소

                            this.MouseDown += this.OnMouseDown_NotDrawing;  // 평소 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_NotDrawing;  // 평소 마우스 이동 함수 적용                    
                        }
                        else if (this.IsPan)                                // Pan 모드인 경우에는 
                        {
                            this.IsDrawBack = false;                        // backShape 표시안함.
                            this.IsDrawClone = false;                       // cloneShape 표시안함.
                            // 중복방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_Zoom;        // Zoom 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_Zoom;        // Zoom 마우스 이동 함수 취소
                            this.MouseUp -= this.OnMouseUp_Zoom;            // Zoom 마우스 업 함수 취소

                            this.MouseDown += this.OnMouseDown_Zoom;        // Zoom 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_Zoom;        // Zoom 마우스 이동 함수 적용
                            this.MouseUp += this.OnMouseUp_Zoom;            // Zoom 마우스 업 함수 적용 
                        }
                    }
                }
            }
        }
        private bool _ismag = false;

        /** PictureBox에 돋보기 기능시 표시할 돋보기 이미지 변수*/
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Magnifying glass image variable")]
        public Image MagnifierImage { get; set; }

        /** PictureBox에 이미지 자르기 기능 설정 변수*/
        [Browsable(true)]
        public bool IsCrop
        {
            get { return this._iscrop; }
            set
            {
                if (this._iscrop != value)
                {
                    this._iscrop = value;

                    if (value)                                              // true 경우에는 Crop 모드 설정
                    {
                        this.IsDrawBack = false;                            // backShape 표시안함.
                        this.IsDrawClone = false;                           // cloneShape 표시안함.
                        this.IsMag = false;                                 // 돋보기 기능 취소
                        this.IsPan = false;                                 // Pan 기능 취소

                        this.MouseDown -= this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 취소        
                        this.MouseDown -= this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 취소   
                        this.MouseMove -= this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 취소
                        // 중복방지를 위해 빼고 더한다.
                        this.MouseDown -= this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Crop;                // Crop 마우스 업 함수 취소 

                        this.MouseDown += this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 적용
                        this.MouseMove += this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 적용
                        this.MouseUp += this.OnMouseUp_Crop;                // Crop 마우스 업 함수 적용 
                    }
                    else                                                    // false 경우에는 Crop 모드 취소
                    {
                        this.Cursor = Cursors.Default;

                        this.MouseDown -= this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Crop;                // Crop 마우스 업 함수 취소 

                        if (this.IsROIView)                                 // ROIView인 경우에 설정
                        {
                            this.IsDrawBack = true;                         // backShape 표시함.
                            this.IsDrawClone = true;                        // cloneShape 표시함.
                            // 중복방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_NotDrawing;  // 평소 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_NotDrawing;  // 평소 마우스 이동 함수 취소

                            this.MouseDown += this.OnMouseDown_NotDrawing;  // 평소 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_NotDrawing;  // 평소 마우스 이동 함수 적용                    
                        }
                        else if (this.IsPan)                                // Pan 기능인 경우
                        {
                            this.IsDrawBack = false;                        // backShape 표시안함.
                            this.IsDrawClone = false;                       // cloneShape 표시안함.
                            // 중복방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_Zoom;        // Zoom 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_Zoom;        // Zoom 마우스 이동 함수 취소
                            this.MouseUp -= this.OnMouseUp_Zoom;            // Zoom 마우스 업 함수 취소   

                            this.MouseDown += this.OnMouseDown_Zoom;        // Zoom 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_Zoom;        // Zoom 마우스 이동 함수 적용
                            this.MouseUp += this.OnMouseUp_Zoom;            // Zoom 마우스 업 함수 적용 
                        }
                    }
                }
            }
        }
        private bool _iscrop = false;

        /** PictureBox에 Bring 기능(두점 사이의 값을 구하기) 설정 변수*/
        [Browsable(true)]
        public bool IsBring
        {
            get { return this._isbring; }
            set
            {
                if (this._isbring != value)
                {
                    this._isbring = value;

                    if (value)                                                  // true 경우에는 Bring 모드 설정
                    {
                        this.IsDrawBack = false;                                // backShape 표시안함.
                        this.IsDrawClone = false;                               // cloneShape 표시안함.
                        this.MouseDown -= this.OnMouseDown_NotDrawing;          // 평소 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_NotDrawing;          // 평소 마우스 이동 함수 취소        
                        this.MouseDown -= this.OnMouseDown_Zoom;                // Zoom 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Zoom;                // Zoom 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Zoom;                    // Zoom 마우스 업 함수 취소   
                        this.MouseMove -= this.OnMouseMove_Msg;                 // Msg 마우스 이동 함수 취소
                        this.MouseDown -= this.OnMouseDown_Crop;                // Crop 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Crop;                // Crop 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Crop;                    // Crop 마우스 업 함수 취소 
                        // 중복방지를 위해 빼고 더한다.
                        this.MouseDown -= this.OnMouseDown_Bring;               // Bring 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Bring;               // Bring 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Bring;                   // Bring 마우스 업 함수 취소 

                        this.MouseDown += this.OnMouseDown_Bring;               // Bring 마우스 다운 함수 적용
                        this.MouseMove += this.OnMouseMove_Bring;               // Bring 마우스 이동 함수 적용
                        this.MouseUp += this.OnMouseUp_Bring;                   // Bring 마우스 업 함수 적용 
                    }
                    else                                                        // false 경우에는 Bring 모드 취소
                    {
                        this.Cursor = Cursors.Default;

                        this.MouseDown -= this.OnMouseDown_Bring;               // Bring 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Bring;               // Bring 마우스 이동 함수 취소
                        this.MouseUp -= this.OnMouseUp_Bring;                   // Bring 마우스 업 함수 취소 

                        if (this.IsROIView)                                     // ROIView인 경우에 설정
                        {
                            this.IsDrawBack = true;                             // backShape 표시함.
                            this.IsDrawClone = true;                            // cloneShape 표시함.
                            // 중복방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 취소        

                            this.MouseDown += this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 적용                    
                        }
                        else if (this.IsPan)                                    // Pan 기능시
                        {
                            this.IsDrawBack = false;                            // backShape 표시안함.
                            this.IsDrawClone = false;                           // cloneShape 표시안함.
                            // 중복방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 취소
                            this.MouseUp -= this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 취소 

                            this.MouseDown += this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 적용
                            this.MouseUp += this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 적용 
                        }
                        else if (this.IsMag)                                    // 돋보기 기능시
                        {
                            this.IsDrawBack = false;                            // backShape 표시안함.
                            this.IsDrawClone = false;                           // cloneShape 표시안함.
                            //  중복방지를 위해 빼고 더한다.
                            this.MouseMove -= this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 취소

                            this.MouseMove += this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 적용
                        }
                        else if (this.IsCrop)                                   // 자르기 기능기
                        {
                            this.IsDrawBack = false;                            // backShape 표시안함.
                            this.IsDrawClone = false;                           // cloneShape 표시안함.
                            // 중복방지를 위해 빼고 더한다.
                            this.MouseDown -= this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 취소
                            this.MouseMove -= this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 취소
                            this.MouseUp -= this.OnMouseUp_Crop;                // Crop 마우스 업 함수 취소 

                            this.MouseDown += this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 적용
                            this.MouseMove += this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 적용
                            this.MouseUp += this.OnMouseUp_Crop;                // Crop 마우스 업 함수 적용
                        }
                    }
                }
            }
        }
        private bool _isbring = false;

        /** PictureBox에 ROI를 그릴지 설정 변수*/
        [Browsable(true)]
        public bool IsROIDrawing
        {
            get { return this._isroidrawing; }
            set
            {
                if (!value) this.newShape = null;   // ROI 그리기 설정이 아닌 경우 기존에 그리던 newShape를 지운다.
                this._isroidrawing = value;
            }
        }
        private bool _isroidrawing = false;

        /** PictureBox에 이미지 Index를 표시한다. (기본값은 true ) */
        [Browsable(true)]
        public bool IsIndexShow
        {
            get { return this._isindexshow; }
            set { this._isindexshow = value; }
        }
        private bool _isindexshow = true;

        /** PictureBox에 ROI 그리는 도형 설정 변수 ( 기본값은 Ractangle이다.) */
        public ROISHAPETYPE DrawShapeType
        {
            get { return this.drawShapeType; }
            set
            {
                if (this.drawShapeType != value)        // 기존에 저장된 ROI Type과 다르면
                {
                    this.drawShapeType = value;
                    this.cloneShape = null;          // 기존 cloneShape 취소
                    this.IsDrawClone = false;         // cloneShape 그리기 취소
                    this.newShape = null;          // 그리던 newShape 취소
                    this.backShape = null;          // backShape 취소
                }
            }
        }
        private ROISHAPETYPE drawShapeType = ROISHAPETYPE.Rectangle;

        /** PictureBox에 인체 영역시 제거 ROI 도형 설정 변수 ( true : 제거 ROI 그리기 false : 일반 ROI 그리기 ) */
        [Browsable(true)]
        public bool IsRemoveArea
        {
            get { return this._isremovearea; }
            set { this._isremovearea = value; }
        }
        private bool _isremovearea = false;

        /** PictureBox에서 cloneShape 생성서 ROI의 중심을 기준으로 좌우 변환 설정 변수 ( 기본값은 flase )*/
        [Browsable(true)]
        public bool IsROIMirror
        {
            get { return this._isroimirror; }
            set { this._isroimirror = value; }
        }
        private bool _isroimirror = false;

        #region PictureBox에 온도 알람시 사용하는 변수 모음

        /** PictureBox에 온도 알람을 할지 설정 변수*/
        [Browsable(true)]
        public bool IsTempAlarm
        {
            get { return this._istempalarm; }
            set { this._istempalarm = value; }
        }
        private bool _istempalarm = false;

        /** 영상의 최저 온도값( -> 0  ), 온도 알람시 Palette에 생성에 사용한다. */
        public double LowTemperature
        {
            get { return this._lowtemperature; }
            set { this._lowtemperature = value; }
        }
        private double _lowtemperature = 14.00;

        /** 영상의 최고 온도값( -> 255), 온도 알람시 Palette에 생성에 사용한다. */
        public double HighTemperature
        {
            get { return this._hightemperature; }
            set { this._hightemperature = value; }
        }
        private double _hightemperature = 40.00;

        /** 온도 알람설정값 , 온도 알람시 Palette에 생성에 사용한다. */
        public double TempAlarmValue
        {
            get { return this._tempalarmvalue; }
            set { this._tempalarmvalue = value; }
        }
        private double _tempalarmvalue = 37.5;

        #endregion  PictureBox에 온도 알람시 사용하는 변수 모음

        /** this ROI와 연결될 ROI(*-1를 그릴경우에는 자기자신을 입력한다. *-2를 그릴경우에는 입력된 정보를 읽어서 연결될 ROI의 정보(SID, ImageIndex, ROIIndex) */
        public ROIShape ConnectROI { get { return this._conectroi; } set { this._conectroi = value; } }
        private ROIShape _conectroi = null;

        /** 앞으로 사용할 ROI Main Index를 보여준다. 설정은 못한다. */
        public int ROIMainIndex { get { return this._roimainindex; } }
        private int _roimainindex = 1;

        /** // 앞으로 사용할 ROI Sub Index를 보여준다. 설정은 못한다. */
        public int ROISubIndex { get { return this._roisubindex; } }
        private int _roisubindex = 1;

        /** Zoom 배율 값 저장 변수*/
        public double Zoom_Ratio
        {
            get { return this._zoom_ratio; }
            set
            {
                if (value < 1.0) this._zoom_ratio = 1.0;
                else if (value > 4.0) this._zoom_ratio = 4.0;
                else this._zoom_ratio = value;
            }
        }
        private double _zoom_ratio = 1.0F;
    #endregion 클래스 외부설정 변수

    #region 외부 Event 호출 함수 모음(ROI가 처음 생성되었을 때, ROI가 처음 그려지는 동안, 선택된 ROI의 모양이 변경되었을 경우, 이미지 자르기가 완료)
        // ROI가 처음 생성되었을 경우 호출.
        [Description("Fired when Added ROI.")]
        public event EventHandler AddedROI;

        // ROI가 처음 그려지는 경우 호출.
        [Description("Fired when Added ROI.")]
        public event EventHandler NewROIDrawing;

        // 선택된 ROI의 모양이 변경되는 중 호출
        [Description("Fired when Selected ROI Value is changing.")]
        public event EventHandler SelectedROIValueChanging;

        // 선택되거나 선택된 ROI의 모양이 변경이 완료될 경우 호출
        [Description("Fired when Selected ROI Value is changed.")]
        public event EventHandler SelectedROIValueChanged;

        // 이미지 자르기가 완료될 경우 완료될 경우 호출
        [Description("Fired when the crop image is completed.")]
        public event EventHandler CropImageIsCompleted;

        // 두점 사이의 값을 가지고 오기가 완료될 경우 호출
        [Description("Fired when two points are set.")]
        public event EventHandler BringTheValue;

        // Bring 모드시 움직일 때 x,y 값을 전달한다.
        public event EventHandler BringMoveInfo;
    #endregion 외부 Event 호출 함수 모음(ROI가 처음 생성되었을 때, ROI가 처음 그려지는 동안, 선택된 ROI의 모양이 변경되었을 경우, 이미지 자르기가 완료)

    #region  Picture Box 생성자, ImageROIGrid 설정 부분, SetFit, SetROIIndex
        /** @brief: Picture Box 생성자           */
        public IRImagePictureBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        #region ImageROIGrid 설정 부분이다. 이미지와 이미지를 그릴 영역을 설정한다.

        /** @brief: ImageROIGrid를 추가하는 함수이다.
        *  @section: Description
        *      Back에 그려질 Image와 크기가 주어지면 ImageROIGrid를 추가한다.
        *  @param:     img    Back에 그려질 Image변수 
        *  @param:     rect   ImageROIGrid가 그려질 크기 PictureBox기준이다.        
        */
        public void AddImageGrid(Bitmap img, Rectangle rect)
        {
            if (this.ImageInfo == null) return;

            ImageROIGrid imageROIGrid = new ImageROIGrid(img, rect)  // ImageROIGrid를 생성한다.
            {
                Image_Width = this.imageinfo.Image_Width,
                Image_Height = this.imageinfo.Image_Height
            };

            this.listImageGrid.Add(imageROIGrid);                    // ImageROIGrid를 생성한 후 listImageGrid에 추가한다.
            this.Invalidate();                                       // PictureBox를 다시 그린다.
        }

        /** @brief: ImageROIGrid를 추가하는 함수이다.
        *  @section: Description
        *      Back에 그려질 Image와 크기가 주어지면 ImageROIGrid를 추가한다.
        *  @param:     img          Back에 그려질 Image변수 
        *  @param:     center       ImageROIGrid가 그려질 중심점 PictureBox기준이다.  
        *  @param:     size         ImageROIGrid가 그려질 크기 PictureBox기준이다.  
        *  @param:     angle        ImageROIGrid가 기울어진 각도 PictureBox기준이다. 
        *  @param:     RefROIList   ImageROIGrid에 추가할 RofROIList 정보( 외부에서 만들어서 입력한다. )
        */
        public void AddImageGrid(Bitmap img, OpenCvSharp.Point2f center, OpenCvSharp.Size2f size, float angle, List<RefROIShape> RefROIList)
        {
            if (this.ImageInfo == null) return;

            ImageROIGrid imageROIGrid = new ImageROIGrid(img, center, size, angle) // ImageROIGrid를 생성한다.
            {
                Image_Width = this.imageinfo.Image_Width,
                Image_Height = this.imageinfo.Image_Height
            };

            imageROIGrid.RefROIList.AddRange(RefROIList);                           // RefROIList를 생성한 ImageROIGrid에 추가한다.
            this.listImageGrid.Add(imageROIGrid);                                   // ImageROIGrid를 listImageGrid에 추가한다.
            this.Invalidate();                                                      // PictureBox를 다시 그린다.
        }

        /** @brief: 저장된 ImageROIGrid를 모두 지운다.
         *  @section: Description
         *      저장된 ImageROIGrid를 모두 지운다.
         */
        public void RemoveAllImageGrid()
        {
            this.SelectImageGrid = null;                            // 선택된 ImageROIGrid를 지운다.
            this.listImageGrid.Clear();                             // listImageGrid를 초기화한다.
            this.Invalidate();                                      // PictureBox를 다시 그린다.
        }

        /** @brief: 저장된 ImageROIGrid의 RefROIShape 정보를 ROIShape정보를 입력한다.
         *  @section: Description
         *      저장된 ImageROIGrid의 RefROIShape 정보를 ROIShape정보를 입력한다. 현재 ROIMainIndex기준으로 저장된다.
         */
        public void ImageROIGridApply()
        {
            if (this.ImageInfo == null) return;

            // ImageROIGrid에서 ROI를 적용할 경우
            this.ConnectROI = null; // 연결할 ROI를 초기화 한다.
            this.CloneShape = null; this.IsDrawClone = false;
            this.backShape = null; this.IsDrawBack = false;

            foreach (ImageROIGrid imageGrid in this.listImageGrid)
            {
                foreach (RefROIShape refROIShape in imageGrid.GetRefROIShapes(this.ImageInfo.Image_Width, this.ImageInfo.Image_Height))
                {
                    this.imageinfo.ROIID += 1;                              // ImageInfo에 저장된 roiIndex에서 1을 증가해서 저장한다.

                    // 입력할 MainIndex => ROIMainIndex 부터 시작한다. ROI_MainIndex가 1부터 시작하여서 -1을 해야 된다.
                    int mainIndex = this.ROIMainIndex - 1 + refROIShape.ROI_MainIndex;
                    int subIndex = refROIShape.ROI_SubIndex;                    // 입력되는 서브 Index

                    ROIShape rOIShape = new ROIShape()
                    {
                        ShapeType = refROIShape.ShapeType,            // 기준 image가 포함하는 Study Patient ChartNo
                        ChartNo = this.ImageInfo.ChartNo,           // 기준 image가 포함하는 Study Patient ChartNo
                        StudyID = this.ImageInfo.StudyID,           // 기준 Image가 포함하는 Study ID
                        ImageIndex = this.ImageInfo.ImageIndex,        // 기준 Image Inddex
                        ROIID = this.imageinfo.ROIID,             // ROI Index를 입력한다. 
                        ROI_MainIndex = mainIndex,                        // ROI Main Index 설정
                        ROI_SubIndex = subIndex,                         // ROI Sub Index 설정  ( RefROIShape의 SubIndex를 추가한다.

                        Image_Width = this.ImageInfo.Image_Width,       // 기준 Image Width
                        Image_Height = this.ImageInfo.Image_Height,      // 기준 Image Height
                        NodeSelected = ROISHAPEPOSTION.None              // Node 선택을 None으로 한다.
                    };

                    // 점을 저장한다.
                    int pIndex = 0;
                    foreach (var p in refROIShape.PointInfo)
                    {
                        rOIShape.AddImagePoint(pIndex, new PointF(p.X, p.Y));
                        pIndex++;
                    }

                    // ROI의 min, max, 평균, 표준편차, 넓이를 구한다. (위치를 변경후 해야한다.)
                    rOIShape.CalROIShare(this.imageinfo.ToMat());

                    // 입력되는 Sub Index가 2인 경우, 저장된 listShape에 MainIndex가 같고 SubIndex가 1인 것과 연결한다.
                    if (subIndex == 2)
                    {
                        ROIShape connectROI = this.ImageInfo.listShape.Find(roi => (roi.ROI_MainIndex == rOIShape.ROI_MainIndex && roi.ROI_SubIndex == 1));

                        if (connectROI != null)
                        {
                            rOIShape.Connect_ChartNo = connectROI.ChartNo;       // 연결될 ROI의 Study의 Patient ChartNo
                            rOIShape.Connect_StudyID = connectROI.StudyID;       // 연결될 ROI의 Study ID
                            rOIShape.Connect_ImageIndex = connectROI.ImageIndex;    // 연결될 ROI의 Image Index
                            rOIShape.Connect_ROIID = connectROI.ROIID;         // 연결될 ROI Index
                        }
                    }

                    this.imageinfo.listShape.Add(rOIShape);                         // 도형 List에 추가                                        
                }
            }

            // Main,Sub Index 기준의 오름차순으로 정령한다.
            this.ImageInfo.listShape.Sort(delegate (ROIShape A, ROIShape B)
            {
                if ((A.ROI_MainIndex < B.ROI_MainIndex) || (A.ROI_MainIndex == B.ROI_MainIndex && A.ROI_SubIndex < B.ROI_SubIndex)) return -1;
                else return 1;
            });

            // 저장된 listShape에서 마지막 Index값을 가지고 온다.
            int lastMainIndex = -1;
            int lastSubIndex = -1;

            // 저장된 listShape에서 가장 큰 MainIndex ROI중에 SubIndex가 큰것을 구한다.
            foreach (ROIShape roi in this.ImageInfo.listShape)
            {
                if (lastMainIndex < roi.ROI_MainIndex || (lastMainIndex == roi.ROI_MainIndex && lastSubIndex < roi.ROI_SubIndex))
                {
                    lastMainIndex = roi.ROI_MainIndex;
                    lastSubIndex = roi.ROI_SubIndex;
                }
            }

            this.SetROIIndex(lastMainIndex + 1, 1);             // ROI (Main, Sub) Index를 적용한다.
        }

        #endregion Image Grid 설정 부분이다. 이미지와 이미지를 그릴 영역을 설정한다.

        /** @brief: PictureBox에 Zoom이 된 이미지를 원래이미지로 돌리는 함수
         *  @section: Description
         *      ROI를 중심을 기준으로 입력된 값 만큼 회전시킨다.
         */
        public void SetFit()
        {
            this.subMatLeft = 0;        // pan Left를 0으로
            this.subMatTop = 0;         // pan Top를 0으로
            this.Zoom_Ratio = 1.0f;     // Zoom 비율을 1.0으로
            this.Invalidate();          // PictureBox를 다시 그린다.
        }

        /** @brief: ROI Index를 설정하는 함수로 외부에서 설정할 때 사용한다.
         *  @section: Description
         *      입력된 main, sub값으로 ROI 표시 Index를 만든다. 예) main : 1, sub : 1 -> 표시 1-1 
         *  @param:     main        ROI 표시 Index 중 첫번째 Index 값 (1,2,3, .... 증가)
         *  @param:     sub         ROI 표시 Index 중 두번째 Index 값 (1 , 2)
         */
        public void SetROIIndex(int main, int sub)
        {
            if (this.imageinfo == null) return;

            // 저장된 listShape에서 마지막 Index값을 가지고 온다.
            int lastMainIndex = -1;
            int lastSubIndex = -1;

            // 저장된 listShape에서 가장 큰 MainIndex ROI중에 SubIndex가 큰것을 구한다.
            foreach (ROIShape roi in this.ImageInfo.listShape)
            {
                if (lastMainIndex < roi.ROI_MainIndex || (lastMainIndex == roi.ROI_MainIndex && lastSubIndex < roi.ROI_SubIndex))
                {
                    lastMainIndex = roi.ROI_MainIndex;
                    lastSubIndex = roi.ROI_SubIndex;
                }
            }

            if (lastMainIndex > main)                   // 마지막 MainIndex 보다 입력한 main이 작으면
            {
                main = lastMainIndex + 1;               // main은 마지막 MainIndex + 1
                sub = 1;                                // sub 는 1
            }
            else if (lastMainIndex == main)             // 마지막 MainIndex와 같으면
            {
                //if (lastSubIndex == -1 || sub == -1)    // 마지막 SubIndex -1이거나 입력된 sub가 -1이면
                //{
                //    main += 1;                          // 입력된 main에 +1
                //    sub = 1;                            // sub는 1
                //}
                //else if (lastSubIndex == sub)           // 마지막 SubIndex와 입력된 sub가 같으면
                //{
                //    sub += 1;                           // 입력된 sub에 +1
                //}
                //else if (lastSubIndex > sub)            // 마지막 SubIndex가 입력된 sub보다 크면
                //{
                //    sub = lastSubIndex + 1;             // sub는 마지막 SubIndex + 1
                //}

                //if (sub >= 3)                           // sub가 3보다 크거가 같으면
                //{
                //    sub = 1;                            // sub는 1
                //    main += 1;                          // 입력된 main에 +1
                //}

                main += 1;                          // 입력된 main에 +1
                sub = 1;                            // sub는 1
            }

            if (main < 1) { main = 1; sub = 1; }

            this._roimainindex = main;
            this._roisubindex = sub;
        }
    #endregion Picture Box 생성자, ImageROIGrid 설정 부분, SetFit, SetROIIndex

    #region ROI 겟수 가지고 오기, 모두 삭제하기, 해당 Index의 ROI 도형 선택하기, 선택된 도형 삭제, 선택 취소 함수

        /** @brief: ROI 도형 겟수 가지고 오기        */
        public int GetShapeCount()
        {
            if (this.imageinfo == null) return 0;
            return this.imageinfo.listShape.Count;
        }

        /** @brief: ROI 도형 모두 삭제 하기       */
        public void ROIRemoveAll()
        {
            if (this.imageinfo == null) return;

            if (this.IsRemoveArea) this.imageinfo.listRemoveShape.Clear();     // 인체 영역시 제거 ROI 도형 List 모두 지우기
            else this.imageinfo.listShape.Clear();

            this.selectedIndex = -1;                                            // 선택된 ROI Index 초기화
            this.Invalidate();
        }

        /** @brief: 입력된 ROI 도형 선택하기
         *  @param:     selectROI        선택할 ROI
         *  @return: true:  있다. selectedIndex 해당 ROI Index로 설정, false : 없다. selectedIndex 기존 유지
         */
        public bool SelectedROIShape(ROIShape selectROI)
        {
            if (this.IsRemoveArea) return false;                    // 인체 영역시 사용 안함.

            if (this.imageinfo == null) return false;
            if (this.imageinfo.listShape.Count == 0) return false;
            if (this.imageinfo.ChartNo != selectROI.ChartNo || this.imageinfo.StudyID != selectROI.StudyID || this.imageinfo.ImageIndex != selectROI.ImageIndex) return false;

            for (int rIndex = 0; rIndex < this.imageinfo.listShape.Count; rIndex++)
            {
                if (this.imageinfo.listShape[rIndex].ROIID == selectROI.ROIID)
                {
                    this.imageinfo.listShape[rIndex].Selected = true;
                    this.selectedIndex = rIndex;
                    return true;
                }
            }
            return false;
        }

        /** @brief: 입력된 main, sub에 해당하는 ROI 도형 선택하기
         *  @param:     main        선택할 ROI Index 중 첫번째 Index 값 (1,2,3, .... 증가)
         *  @param:     sub         선택할 ROI 표시 Index 중 두번째 Index 값 (1 , 2)
         *  @return: true:  있다. selectedIndex 해당 ROI Index로 설정, false : 없다. selectedIndex 기존 유지
         */
        public bool SelectedROIShape(int main, int sub)
        {
            if (this.IsRemoveArea) return false; // 인체 영역시 사용 안함.

            if (this.imageinfo == null) return false;
            if (this.imageinfo.listShape.Count == 0) return false;

            for (int index = 0; index < this.imageinfo.listShape.Count; index++)
            {
                ROIShape roi = this.imageinfo.listShape[index];
                if ((roi.ShapeType != ROISHAPETYPE.LineAngle && roi.ShapeType != ROISHAPETYPE.LineX) && roi.ROI_MainIndex == main && roi.ROI_SubIndex == sub)
                {
                    this.selectedIndex = index;
                    roi.Selected = true;
                    return true;
                }
            }

            return false;
        }

        /** @brief: 선택된 ROI 도형 가지고 오기         
         *  @return: selectedIndex 해당하는 ROIShape를 리턴, 없으면 null
         */
        public ROIShape GetSelectedROIShape()
        {
            if (this.IsRemoveArea) return null; // 인체 영역시 사용 안함.

            if (this.imageinfo == null) return null;
            ROIShape roi = null;

            if (this.selectedIndex >= 0 && this.selectedIndex < this.imageinfo.listShape.Count)
                roi = this.imageinfo.listShape[this.selectedIndex];

            return roi;
        }

        /** @brief: 선택된 ROI 도형을 List에서 삭제하는 함수         */
        public void SelectedRemove()
        {
            if (this.imageinfo == null || this.selectedIndex < 0) return;

            if (this.IsRemoveArea)  //인체 영역시 제거 ROI 도형
            {
                if (this.selectedIndex < this.imageinfo.listRemoveShape.Count)
                {
                    this.imageinfo.listRemoveShape.RemoveAt(this.selectedIndex);

                    this.newShape = null;
                    this.selectedIndex = -1;

                    this.Invalidate();
                }
            }
            else
            {
                if (this.selectedIndex < this.imageinfo.listShape.Count)
                {
                    this.imageinfo.listShape.RemoveAt(this.selectedIndex);

                    this.newShape = null;
                    this.selectedIndex = -1;

                    this.Invalidate();
                }
            }
        }

        /** @brief: 선택된 ROI 도형을 선택 취소한다. */
        public void ROISelecedCencel()
        {
            if (this.imageinfo == null) return;
            if (this.IsRemoveArea)   //인체 영역시 제거 ROI 도형
                foreach (var roi in this.imageinfo.listRemoveShape) { roi.Selected = false; roi.NodeSelected = ROISHAPEPOSTION.None; this.Invalidate(); }
            else
                foreach (var roi in this.imageinfo.listShape) { roi.Selected = false; roi.NodeSelected = ROISHAPEPOSTION.None; this.Invalidate(); }
        }

    #endregion ROI 겟수 가지고 오기, 모두 삭제하기, 선택된 도형 삭제, 선택 취소 함수,  ROI의 원본(Zoom, Pen) 적용 처리

    #region ROI를 list에 추가하는 함수이다.
        /** @brief: 도형 List에 추가
         *  @section: Description
         *      도형 List에 추가한다.
         *  @param:     add       추가할 ROIShare이다.
         */
        public void ListShapeAdd(ROIShape add)
        {
            if (add == null) return;
            if (this.imageinfo == null) return;
            int subIndex = -1;

            // ROI의 min, max, 평균, 표준편차, 넓이를 구한다.            
            add.CalROIShare(this.imageinfo.ToMat());

            add.NodeSelected = ROISHAPEPOSTION.None;                    // Node 선택을 None으로 한다.

            if (this.IsRemoveArea)
            {
                this.imageinfo.listRemoveShape.Add(add);            // 인체 영역시 제거 ROI 도형 List 추가
                foreach (var roi in this.imageinfo.listRemoveShape) roi.Selected = false;     // 이전 선택된 ROI를 선택 취소로 설정

                this.imageinfo.listRemoveShape[this.imageinfo.listRemoveShape.Count - 1].Selected = true;     // 선택된 ROI 선택 설정     
            }
            else
            {
                if (add.ShapeType != ROISHAPETYPE.LineAngle && add.ShapeType != ROISHAPETYPE.LineX)
                {
                    add.ROI_MainIndex = this._roimainindex;                      // ROI Main Index 설정
                    add.ROI_SubIndex = this.ROISubIndex; // ROI Sub Index 설정         
                    subIndex = add.ROI_SubIndex;
                    //if (this.ROISubIndex == 2)
                    //{
                        if (this.ConnectROI != null)
                        {
                            add.Connect_ChartNo    = this.ConnectROI.ChartNo;       // 연결될 ROI의 Study의 Patient ChartNo
                            add.Connect_StudyID    = this.ConnectROI.StudyID;       // 연결될 ROI의 Study ID
                            add.Connect_ImageIndex = this.ConnectROI.ImageIndex;    // 연결될 ROI의 Image Index
                            add.Connect_ROIID      = this.ConnectROI.ROIID;         // 연결될 ROI Index
                        }
                    //}
                    //else                                                        //  SubIndex 1인경우 오류 방지 코드 초기화 설정
                    //{
                    //    add.Connect_ChartNo    = "";       
                    //    add.Connect_StudyID    = int.MinValue;       
                    //    add.Connect_ImageIndex = int.MinValue;    
                    //    add.Connect_ROIID      = int.MinValue;
                    //    this.ConnectROI        = add;
                    //}
                }
                else            // LineAngle, LineX인 경우에 처리 부분이다.
                {
                    // ROISubIndex인 경우에는 Main Index를 1을 증가 시켜야 한다.
                    if (this.ROISubIndex == 2) this._roimainindex += 1;
                    add.ROI_MainIndex = this._roimainindex;                      // ROI Main Index 설정
                    this.ConnectROI = null;     // 연결 ROI를 null처리한다.
                }

                this.imageinfo.ROIID += 1;                          // ImageInfo에 저장된 roiIndex에서 1을 증가해서 저장한다.
                add.ChartNo = this.imageinfo.ChartNo;            // 기준 image가 포함하는 Study Patient ChartNo
                add.StudyID = this.imageinfo.StudyID;            // 기준 Image가 포함하는 Study ID
                add.ImageIndex = this.imageinfo.ImageIndex;         // 기준 Image Inddex
                add.ROIID = this.imageinfo.ROIID;              // ROI Index를 입력한다. 

                this.imageinfo.listShape.Add(add);                  // 도형 List에 추가 

                this.SetROIIndex(add.ROI_MainIndex, subIndex);
            }

            if (this.AddedROI != null)                                    // ROI가 처음 생성되었을 경우, 함수 선언이 있는 경우 생성 ROI 정보를 전달한다.
            {
                ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();
                roiInfo.Rois.Add(add);                                    // 새로 추가한 ROI 도형 정보를 저장한다.

                this.AddedROI(this, roiInfo);
            }
        }
    #endregion

    #region Key 이벤트 처리 부분

        /** @brief:  선택된 ROI도형의 중심 기준으로 회전시킨다.
         *  @section: Description
         *      선택된 ROI도형의 중심 기준으로 입력된 값 만큼 회전시킨다.
         *  @param:     selectIndex     회전시킬 ROI Index
         *  @param:     AngleInerval    회전시킬 각도(Degree)( float변수 ), 양수(>0) : 시계방향, 음수(0<): 반시계방향
         */
        public void SelectedROIRotation(int selectIndex, float AngleInerval = 1)
        {
            if (this.ImageInfo != null && selectIndex >= 0 && selectIndex < this.ImageInfo.listShape.Count)
            {
                this.cloneShape = null;                                             // clone ROI 삭제
                this.IsDrawClone = false;                                           // clone ROI 표시 안함.

                ROIShape temp = this.imageinfo.listShape[selectedIndex];

                temp.Rotation(AngleInerval);            // ROI를 AngleInerval만큼 회전

                // ROI의 min, max, 평균, 표준편차, 넓이를 구한다.;
                temp.CalROIShare(this.imageinfo.ToMat());

                this.imageinfo.listShape[selectedIndex] = temp;

                this.Invalidate();
            }
        }

        /** @brief: Key Down 이벤트 처리( ROI 세부 회전, 이동, 세부크기)
         *  @section: Description
         *      선택된 ROI도형의 중심 기준으로 입력된 값 만큼 회전시킨다.
         *  @param:     msg         처리할 창 메시지를 나타내며 참조에 의해 전달되는 System.Windows.Forms.Message입니다.(시스템 값)
         *  @param:     keyData     처리할 키를 나타내는 System.Windows.Forms.Keys 값 중 하나입니다.(시스템 값)
         *  @return                 컨트롤이 문자를 처리하면 true이고, 그렇지 않으면 false입니다.(시스템 값)
         */
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            base.ProcessCmdKey(ref msg, keyData);   // 내부 System 처리를 하기위해 
                                                                                    // 예외 처리s
            if (this.imageinfo == null || this.imageinfo.listShape.Count <= 0) return true;

            int selectIndex = this.imageinfo.listShape.FindIndex(roi => roi.Selected == true);

            if (!this.IsRemoveArea && keyData == (Keys.ControlKey | Keys.Control))  // 인체 지우기 모드가 아닌 경우에만.
            {
                this.IsMultiSelected = true;                        // Control누르고 있는 경우에 멀티 선택가능하도록
                this.Cursor = Cursors.Arrow;                        // 멀티 선택 모드인 경우 화살표 적용

                this.IsDrawBack = false;                                // backShape 표시안함.
                this.IsDrawClone = false;                               // cloneShape 표시안함.
                this.MouseDown -= this.OnMouseDown_NotDrawing;          // 평소 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_NotDrawing;          // 평소 마우스 이동 함수 취소        

                this.MouseDown -= this.OnMouseDown_Zoom;                // Zoom 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_Zoom;                // Zoom 마우스 이동 함수 취소
                this.MouseUp -= this.OnMouseUp_Zoom;                  // Zoom 마우스 업 함수 취소   

                this.MouseMove -= this.OnMouseMove_Msg;                 // Msg 마우스 이동 함수 취소

                this.MouseDown -= this.OnMouseDown_Crop;                // Crop 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_Crop;                // Crop 마우스 이동 함수 취소
                this.MouseUp -= this.OnMouseUp_Crop;                  // Crop 마우스 업 함수 취소 

                this.MouseDown -= this.OnMouseDown_Bring;               // Bring 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_Bring;               // Bring 마우스 이동 함수 취소
                this.MouseUp -= this.OnMouseUp_Bring;                 // Bring 마우스 업 함수 취소 
                // 중복 방지
                this.MouseDown -= this.OnMouseDown_MultiSelected;       // MultiSelected 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_MultiSelected;       // MultiSelected 마우스 이동 함수 취소
                this.MouseUp -= this.OnMouseUp_MultiSelected;         // MultiSelected 마우스 업 함수 취소 

                this.MouseDown += this.OnMouseDown_MultiSelected;       // MultiSelected 마우스 다운 함수 설정
                this.MouseMove += this.OnMouseMove_MultiSelected;       // MultiSelected 마우스 이동 함수 설정
                this.MouseUp += this.OnMouseUp_MultiSelected;         // MultiSelected 마우스 업 함수 설정

                this.Invalidate();
            }

            if (selectIndex >= 0)     // 선택된 ROI가 있으면
            {
                int xOffset = 0;
                int yOffset = 0;

                ROISHAPEPOSTION _node = ROISHAPEPOSTION.None;

                switch (keyData)
                {
                    // arrow keys scroll or browse, depending on ZoomMode
                    case Keys.Oemplus | Keys.Shift:
                    case Keys.Add: this.SelectedROIRotation(selectIndex, 1); return true;// ROI를 1도 만큼 회전한다.
                    case Keys.OemMinus:
                    case Keys.Subtract: this.SelectedROIRotation(selectIndex, -1); return true;// ROI를 -1도 만큼 회전한다.
                    case Keys.Left: _node = ROISHAPEPOSTION.Inside; xOffset = -1; yOffset = 0; break;      // ROI 왼쪽으로 1 pixel 이동    
                    case Keys.Up: _node = ROISHAPEPOSTION.Inside; xOffset = 0; yOffset = -1; break;      // ROI 위로 1 pixel 이동
                    case Keys.Right: _node = ROISHAPEPOSTION.Inside; xOffset = 1; yOffset = 0; break;      // ROI 오른쪽으로 1 pixel 이동
                    case Keys.Down: _node = ROISHAPEPOSTION.Inside; xOffset = 0; yOffset = 1; break;      // ROI 아래로 1 pixel 이동

                    case Keys.Control | Keys.Down: _node = ROISHAPEPOSTION.AreaBottomMiddle; xOffset = 0; yOffset = 1; break;      // ROI BottomMiddle 1 증가 
                    case Keys.Control | Keys.Up: _node = ROISHAPEPOSTION.AreaTopMiddle; xOffset = 0; yOffset = -1; break;      // ROI TopMiddle 1 증가
                    case Keys.Control | Keys.Right: _node = ROISHAPEPOSTION.AreaRightMiddle; xOffset = 1; yOffset = 0; break;      // ROI RIghtMiddle 1 증가
                    case Keys.Control | Keys.Left: _node = ROISHAPEPOSTION.AreaLeftMiddle; xOffset = -1; yOffset = 0; break;      // ROI LeftMiddle 1증가

                    case Keys.Alt | Keys.Down: _node = ROISHAPEPOSTION.AreaTopMiddle; xOffset = 0; yOffset = 1; break;      // ROI TopMiddle 1 감소
                    case Keys.Alt | Keys.Up: _node = ROISHAPEPOSTION.AreaBottomMiddle; xOffset = 0; yOffset = -1; break;      // ROI BottomMiddle 1 감소
                    case Keys.Alt | Keys.Right: _node = ROISHAPEPOSTION.AreaLeftMiddle; xOffset = 1; yOffset = 0; break;      // ROI LeftMiddle 1 감소
                    case Keys.Alt | Keys.Left: _node = ROISHAPEPOSTION.AreaRightMiddle; xOffset = -1; yOffset = 0; break;      // ROI RigheMiddle 1 감소

                    default: return true;
                }

                this.cloneShape = null;                                             // clone ROI 삭제
                this.IsDrawClone = false;                                           // clone ROI 표시 안함.

                ROIShape temp = this.imageinfo.listShape[selectIndex];
                ROISHAPEPOSTION back = temp.NodeSelected;                           // 기존의 선택 저장한다.
                temp.NodeSelected = _node;
                temp.Offset(xOffset, yOffset);                                      // 선택된 도형 Offset 만큼 이동

                // ROI의 min, max, 평균, 표준편차, 넓이를 구한다.            
                temp.CalROIShare(this.imageinfo.ToMat());

                temp.NodeSelected = back;

                // 선택된 도형이 이동 중인경우 호출
                if (this.SelectedROIValueChanging != null)
                {
                    ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();
                    roiInfo.Rois.Add(this.imageinfo.listShape[selectIndex]);                     // 선택된 ROI 도형 정보

                    this.SelectedROIValueChanging(this, roiInfo);
                }

                this.Cursor = this.imageinfo.listShape[selectIndex].GetCursor();

                this.Invalidate();
            }

            return true;
        }

        /** @brief: Key Up 이벤트 처리( ROI 세부 이동 완료 )
         *  @section: Description
         *      선택된 ROI도형의 중심 기준으로 입력된 값 만큼 회전시킨다.
         *  @param:     msg         처리할 창 메시지를 나타내며 참조에 의해 전달되는 System.Windows.Forms.Message입니다.(시스템 값)
         *  @param:     keyData     처리할 키를 나타내는 System.Windows.Forms.Keys 값 중 하나입니다.(시스템 값)
         *  @return                 컨트롤이 문자를 처리하면 true이고, 그렇지 않으면 false입니다.(시스템 값)
         */
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (this.IsMultiSelected)                                   // 멀티 선택 모드 시에 처리함.
            {
                this.IsMultiSelected = false;                     // Control를 때면 멀티선택 취소
                this.Cursor = Cursors.Cross;             // 멀티 선택 취소시 Cross로 설정

                this.MouseDown -= this.OnMouseDown_MultiSelected;       // MultiSelected 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_MultiSelected;       // MultiSelected 마우스 이동 함수 취소
                this.MouseUp -= this.OnMouseUp_MultiSelected;           // MultiSelected 마우스 업 함수 취소 

                if (this.IsROIView)                                     // ROIView인 경우에 설정
                {
                    this.IsDrawBack = true;                             // backShape 표시함.
                    this.IsDrawClone = true;                            // cloneShape 표시함.
                                                                        // 중복방지를 위해 빼고 더한다.
                    this.MouseDown -= this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 취소        

                    this.MouseDown += this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 적용                    
                }
                else if (this.IsPan)                                    // Pan 기능시
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함.
                                                                        // 중복방지를 위해 빼고 더한다.
                    this.MouseDown -= this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 취소 

                    this.MouseDown += this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 적용 
                }
                else if (this.IsMag)                                    // 돋보기 기능시
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함.
                                                                        //  중복방지를 위해 빼고 더한다.
                    this.MouseMove -= this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 취소

                    this.MouseMove += this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 적용
                }
                else if (this.IsCrop)                                   // 자르기 기능기
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함.
                                                                        // 중복방지를 위해 빼고 더한다.
                    this.MouseDown -= this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Crop;                // Crop 마우스 업 함수 취소 

                    this.MouseDown += this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Crop;                // Crop 마우스 업 함수 적용
                }
                else if (this.IsBring)
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함

                    this.MouseDown -= this.OnMouseDown_Bring;           // Bring 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_Bring;           // Bring 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Bring;                   // Bring 마우스 업 함수 취소 

                    this.MouseDown += this.OnMouseDown_Bring;           // Bring 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_Bring;           // Bring 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Bring;                   // Bring 마우스 업 함수 적용 
                }
            }
            base.OnKeyUp(e);

            if (e.Handled) return;
            // 예외 처리
            if (this.imageinfo == null || this.imageinfo.listShape.Count <= 0) return;

            int selectIndex = this.imageinfo.listShape.FindIndex(roi => roi.Selected == true);
            if (selectIndex >= 0)     // 선택된 ROI가 있으면
            {
                switch (e.KeyCode)
                {
                    // arrow keys scroll or browse, depending on ZoomMode
                    case Keys.Left: break;      // ROI 왼쪽으로 1 pixel 이동    
                    case Keys.Up: break;      // ROI 위로 1 pixel 이동
                    case Keys.Right: break;      // ROI 오른쪽으로 1 pixel 이동
                    case Keys.Down: break;      // ROI 아래로 1 pixel 이동
                    case Keys.Control | Keys.Down: break;      // ROI BottomMiddle 1 증가 
                    case Keys.Control | Keys.Up: break;      // ROI TopMiddle 1 증가
                    case Keys.Control | Keys.Right: break;      // ROI RIghtMiddle 1 증가
                    case Keys.Control | Keys.Left: break;      // ROI LeftMiddle 1증가

                    case Keys.Alt | Keys.Down: break;      // ROI TopMiddle 1 감소
                    case Keys.Alt | Keys.Up: break;      // ROI BottomMiddle 1 감소
                    case Keys.Alt | Keys.Right: break;      // ROI LeftMiddle 1 감소
                    case Keys.Alt | Keys.Left: break;      // ROI RigheMiddle 1 감소
                    default: return;
                }

                // 선택된 도형이 이동이 완료한 경우 호출
                if (this.SelectedROIValueChanged != null)
                {
                    ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();

                    if (this.imageinfo.listShape[selectIndex].ShapeType != ROISHAPETYPE.LineAngle && this.imageinfo.listShape[selectIndex].ShapeType != ROISHAPETYPE.LineX)
                    {
                        roiInfo.Rois.Add(this.imageinfo.listShape[selectIndex]);                     // 선택된 ROI 도형 정보를 저장한다.
                    }
                    this.SelectedROIValueChanged(this, roiInfo);
                }
            }
        }
    #endregion  Key 이벤트 처리 부분

    #region 마우스 이벤트 처리

        /** @brief:  ROI List에서 해당 마우스 위치에 있는 ROI 선택함수
         *  @section: Description
         *      입력된 마우스 위치(Image 기준 좌표)에 해당하는 ROI 선택하고 ROI의 Index를 알려준다.
         *  @param:     mouse_pt     마우스 위치(Image 기준 좌표)
         *  @param:     selected     ROI Index를 리턴한다.
         *  @retuen: true : 선택 false : 선택안됨.
         */
        private bool MouseIsOverShape(PointF mouse_pt, out int selected)
        {
            selected = -1;

            if (this.imageinfo == null) return false;
            for (int index = 0; index < this.imageinfo.listShape.Count; index++)
            {
                ROIShape rShape = this.imageinfo.listShape[index];

                if (rShape.GetNodeSelectable(mouse_pt) != ROISHAPEPOSTION.None)
                {
                    selected = index;
                    return true;
                }
            }

            return false;
        }

        /** @brief:  인체 영역시 제거 ROI 도형 List에서 해당 마우스 위치에 있는 ROI 선택함수
         *  @section: Description
         *      입력된 마우스 위치(Image 기준 좌표)에 해당하는 ROI 선택하고 ROI의 Index를 알려준다.
         *  @param:     mouse_pt     마우스 위치(Image 기준 좌표)
         *  @param:     selected     ROI Index를 리턴한다.
         *  @retuen: true : 선택 false : 선택안됨.
         */
        private bool MouseIsOverRemoveShape(PointF mouse_pt, out int selected)
        {
            selected = -1;

            if (this.imageinfo == null) return false;
            for (int index = 0; index < this.imageinfo.listRemoveShape.Count; index++)
            {
                ROIShape rShape = this.imageinfo.listRemoveShape[index];

                if (rShape.GetNodeSelectable(mouse_pt) != ROISHAPEPOSTION.None)
                {
                    selected = index;
                    return true;
                }
            }

            return false;
        }

        /** @brief: PictureBox 기준의 점을 Image 기준의 점으로 변경한다.
         *  @section: Description
         *      PictureBox 기준의 점을 Image 기준의 점으로 변경한다. imageinfo가 없으면 PictureBox기준점을 그냥 리턴한다. zoom, pan 적용한다.
         *  @param:     picPoint       PictureBox 기준의 점
         *  @return: Image 기준의 점
         */
        private PointF ConvertImagePoint(Point picPoint)
        {
            if (this.imageinfo == null) return new Point(picPoint.X, picPoint.Y);

            // roiAres의 값은 PictureBox의 값이다 그래서 이미지 비율에 맞추기 작업이 필요
            int picWidth = this.Width;                // 지금 PictureBox의 넓이
            int picHeight = this.Height;               // 지금 PictureBox의 높이
            int matWidth = this.imageinfo.Image_Width;       // 이미지의 넓이
            int matHeight = this.imageinfo.Image_Height;      // 이미지의 높이

            float imageX = (((float)picPoint.X * (float)matWidth / (float)picWidth) + this.subMatLeft) / (float)this.Zoom_Ratio;
            float imageY = (((float)picPoint.Y * (float)matHeight / (float)picHeight) + this.subMatTop) / (float)this.Zoom_Ratio;

            return new PointF(imageX, imageY);
        }

        /** @brief: 멀티 선택 마우스 다운 함수
         *  @section: Description
         *      멀티 선택인 경우의 마우스 다운 함수
         */
        private void OnMouseDown_MultiSelected(object sender, MouseEventArgs e)
        {
            if (this.imageinfo == null) return;

            if (this.IsRemoveArea)
            {
                this.IsMultiSelected = false;                           // Control를 때면 멀티선택 취소
                this.Cursor = Cursors.Cross;                            // 멀티 선택 취소시 Cross로 설정

                this.MouseDown -= this.OnMouseDown_MultiSelected;       // MultiSelected 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_MultiSelected;       // MultiSelected 마우스 이동 함수 취소
                this.MouseUp -= this.OnMouseUp_MultiSelected;           // MultiSelected 마우스 업 함수 취소 

                if (this.IsROIView)                                     // ROIView인 경우에 설정
                {
                    this.IsDrawBack = true;                             // backShape 표시함.
                    this.IsDrawClone = true;                            // cloneShape 표시함.
                                                                        // 중복방지를 위해 빼고 더한다.
                    this.MouseDown -= this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 취소        

                    this.MouseDown += this.OnMouseDown_NotDrawing;      // 평소 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_NotDrawing;      // 평소 마우스 이동 함수 적용                    
                }
                else if (this.IsPan)                                    // Pan 기능시
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함.
                                                                        // 중복방지를 위해 빼고 더한다.
                    this.MouseDown -= this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 취소 

                    this.MouseDown += this.OnMouseDown_Zoom;            // Zoom 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_Zoom;            // Zoom 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Zoom;                // Zoom 마우스 업 함수 적용 
                }
                else if (this.IsMag)                                    // 돋보기 기능시
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함.
                                                                        //  중복방지를 위해 빼고 더한다.
                    this.MouseMove -= this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 취소

                    this.MouseMove += this.OnMouseMove_Msg;             // Msg 마우스 이동 함수 적용
                }
                else if (this.IsCrop)                                   // 자르기 기능기
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함.
                                                                        // 중복방지를 위해 빼고 더한다.
                    this.MouseDown -= this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Crop;                // Crop 마우스 업 함수 취소 

                    this.MouseDown += this.OnMouseDown_Crop;            // Crop 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_Crop;            // Crop 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Crop;                // Crop 마우스 업 함수 적용
                }
                else if (this.IsBring)
                {
                    this.IsDrawBack = false;                            // backShape 표시안함.
                    this.IsDrawClone = false;                           // cloneShape 표시안함

                    this.MouseDown -= this.OnMouseDown_Bring;           // Bring 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_Bring;           // Bring 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Bring;                   // Bring 마우스 업 함수 취소 

                    this.MouseDown += this.OnMouseDown_Bring;           // Bring 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_Bring;           // Bring 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Bring;                   // Bring 마우스 업 함수 적용 
                }
                return;
            }

            // 이미지 기준의 점으로 변경해서 사용한다.
            PointF imagePoint = this.ConvertImagePoint(e.Location);

            if (this.MouseIsOverShape(imagePoint, out int selectIndex))                     // 기존의 ROI를 선택하면
            {
                this.cloneShape = null;                                                     // clone ROI 삭제
                this.IsDrawClone = false;                                                   // clone ROI 표시 안함.
                this.backShape = null;                                                      // Bcak ROI 삭제 
                this.IsDrawBack = false;                                                    // Back ROI 표시 안함.       

                this.imageinfo.listShape[selectIndex].NodeSelected = ROISHAPEPOSTION.None;  // 선탣 Node 초기화 해야 화면에 Guide안나옴.

                // Line Angle 이나 LineX는 처리안함.
                if (this.imageinfo.listShape[selectIndex].ShapeType == ROISHAPETYPE.LineAngle || this.imageinfo.listShape[selectIndex].ShapeType == ROISHAPETYPE.LineX) return;

                if (e.Button == MouseButtons.Right) this.imageinfo.listShape[selectIndex].Selected = false;      // 선택된 ROI 선택 취소               
                else this.imageinfo.listShape[selectIndex].Selected = true;       // 선택된 ROI 선택 설정     
            }
            else                                                                            // 선택이 아니면 선택용 Rect를 그린다.
            {
                this.IsDrawMultiSelectedRect = true;
                // 이미지 기준의 점으로 변경해서 저장
                MultiSelectStartPoint = imagePoint;
            }

        }

        /** @brief: 멀티 선택 마우스 이동 함수
         *  @section: Description
         *      멀티 선택인 경우의 마우스 이동 함수
         */
        private void OnMouseMove_MultiSelected(object sender, MouseEventArgs e)
        {
            if (this.imageinfo == null) return;

            // 기존에 선택된 모양 지우는 코드
            int selectIndex = this.imageinfo.listShape.FindIndex(roi => roi.Selected == true);
            if (selectIndex >= 0) this.imageinfo.listShape[selectIndex].NodeSelected = ROISHAPEPOSTION.None;

            // 이미지 기준의 점으로 변경해서 사용한다.
            movePoint = this.ConvertImagePoint(e.Location);

            this.Invalidate();
        }

        /** @brief: 멀티 선택 마우스 업 함수
         *  @section: Description
         *      멀티 선택인 경우의 마우스 업 함수
         */
        private void OnMouseUp_MultiSelected(object sender, MouseEventArgs e)
        {
            if (this.imageinfo == null) return;
            if (this.IsDrawMultiSelectedRect)
            {
                var minX = Math.Min(this.MultiSelectStartPoint.X, movePoint.X);
                var maxX = Math.Max(this.MultiSelectStartPoint.X, movePoint.X);
                var minY = Math.Min(this.MultiSelectStartPoint.Y, movePoint.Y);
                var maxY = Math.Max(this.MultiSelectStartPoint.Y, movePoint.Y);

                // 멀티 선택용 Rect 만들기
                RectangleF SelectedRect = new RectangleF(minX, minY, maxX - minX, maxY - minY);

                foreach (var rShape in this.imageinfo.listShape)
                {
                    if (rShape.Contains(SelectedRect))
                    {
                        rShape.Selected = true;
                    }
                }


            }
            this.IsDrawMultiSelectedRect = false;

            // 생성된 것이 없으면 다시 생성한다.
            if (multiSelectedROIGuide == null) multiSelectedROIGuide = new MultiSelectedROIGuide(this.imageinfo.Image_Width, this.imageinfo.Image_Height);
            // 기존에 저장된 ROI를 지운다.
            this.multiSelectedROIGuide.RemoveSelectedROI();
            // 다시 선택된 ROI를 입력한다.
            foreach (var rShape in this.imageinfo.listShape) if (rShape.Selected) this.multiSelectedROIGuide.AddSelectedROI(rShape);

            this.Invalidate();
        }

        /** @brief: 평소 마우스 다운 함수
         *  @section: Description
         *      아무것도 모드 아닌 경우의 마우스 다운 함수
         */
        private void OnMouseDown_NotDrawing(object sender, MouseEventArgs e)
        {
            int sIndex = -1;

            // 이미지 기준의 점으로 변경해서 사용한다.
            PointF imagePoint = this.ConvertImagePoint(e.Location);

            // 이미지 기준의 점으로 변경해서 저장
            movePoint = imagePoint;

            this.SelectImageGrid = null;
            foreach (ImageROIGrid imageGrid in this.listImageGrid)
            {
                if (imageGrid.GetNodeSelectable(imagePoint) != ROISHAPEPOSTION.None)
                {
                    this.SelectImageGrid = imageGrid;
                    break;
                }
            }

            if (this.SelectImageGrid != null)                                       // ImageROIGrid가 선택된 경우
            {
                this.cloneShape = null;                                             // clone ROI 삭제
                this.IsDrawClone = false;                                           // clone ROI 표시 안함.

                this.MouseMove -= this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 취소
                // 중복방지를 위해 빼고 더한다.
                this.MouseMove -= this.OnMouseMove_GridMoving;                      // 선택된 Grid 이동함수, 마우스 이동 함수 취소
                this.MouseUp -= this.OnMouseUp_GridMoving;                        // 선택된 Grid 이동 함수, 마우스 업 함수 취소

                this.MouseMove += this.OnMouseMove_GridMoving;                      // 선택된 Grid 이동함수, 마우스 이동 함수 적용
                this.MouseUp += this.OnMouseUp_GridMoving;                        // 선택된 Grid 이동 함수, 마우스 업 함수 적용
            }
            else if (this.multiSelectedROIGuide != null)                             // 멀티 선택용 Guide가 있는 경우 ( Grid 같이 사용)
            {
                if (e.Button == MouseButtons.Right)                                 // 멀티 선택용 취소다.
                {
                    this.multiSelectedROIGuide.NoSelectedROI();                     // 멀티 선택용 Guide에 포함된 ROI의 선택을 취소한다.
                    this.multiSelectedROIGuide = null;
                    this.Invalidate();
                    return;
                }

                if (this.multiSelectedROIGuide.GetNodeSelectable(imagePoint))
                {
                    // ROI 선택 Node를 취소
                    if (this.imageinfo != null) foreach (var roi in this.imageinfo.listShape) roi.NodeSelected = ROISHAPEPOSTION.None;

                    this.MouseMove -= this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 취소
                                                                                        // 중복방지를 위해 빼고 더한다.
                    this.MouseMove -= this.OnMouseMove_GridMoving;                      // 선택된 Grid 이동함수, 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_GridMoving;                        // 선택된 Grid 이동 함수, 마우스 업 함수 취소

                    this.MouseMove += this.OnMouseMove_GridMoving;                      // 선택된 Grid 이동함수, 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_GridMoving;                        // 선택된 Grid 이동 함수, 마우스 업 함수 적용
                }
            }
            else if (this.imageinfo != null)                                        // 아닌 경우
            {
                // 오른쪽 버튼은 모두 취소다.
                if (e.Button == MouseButtons.Right)
                {
                    // 클론이 있으면 Popup Menu를 안보이게 설정
                    if (this.cloneShape != null)
                    {
                        this.IsMenuShow = false;
                        this.cloneShape = null;
                    }
                    else if (!this.IsRemoveArea)     // 인체 영역시 제거 ROI 도형그리기가 아닌경우
                    {
                        if (this.backShape != null && this.selectedIndex >= 0 && this.selectedIndex < this.imageinfo.listShape.Count)          // backShape가 있으면 다시 돌린다.
                        {
                            this.imageinfo.listShape[this.selectedIndex] = this.backShape.CopyTo();
                            this.imageinfo.listShape[this.selectedIndex].ForeColor = this.undoFontColor;            // 이전색으로 되돌린다.
                            this.imageinfo.listShape[this.selectedIndex].BorderColor = this.undoBorderColor;          // 이전색으로 되돌린다.
                            this.imageinfo.listShape[this.selectedIndex].Selected = true;

                            // 선택된 ROI가 이전 모양으로 되돌아 갔을 경우 경우 호출
                            if (this.SelectedROIValueChanged != null)
                            {
                                ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();
                                roiInfo.Rois.Add(this.imageinfo.listShape[this.selectedIndex]);     // 선택된 ROI 정보를 저장한다.

                                this.SelectedROIValueChanged(this, roiInfo);
                            }

                            this.IsMenuShow = false;    // 되돌릴 것이 있으면 Popup Menu를 안보이게 설정
                            this.backShape = null;                                      // 다시 돌린 것이 완료되면 backShape 초기화
                        }
                    }

                    return;
                }

                this.IsDrawBack = false;                                                    // backShape를 안 그리는 것으로 설정
                this.backShape = null;                                                      // 다시 돌린 것이 완료되면 backShape 초기화

                if (this.IsRemoveArea && MouseIsOverRemoveShape(imagePoint, out sIndex))    // 인체 영역시 제거 ROI 도형를 선택하면
                {
                    foreach (var roi in this.imageinfo.listRemoveShape) roi.Selected = false;     // 이전 선택된 ROI를 선택 취소로 설정

                    this.cloneShape = null;                                                 // clone ROI 삭제
                    this.IsDrawClone = false;                                               // clone ROI 표시 안함.

                    this.selectedIndex = sIndex;                                            // 선택된 Index 저장
                    this.imageinfo.listRemoveShape[this.selectedIndex].Selected = true;     // 선택된 ROI 선택 설정              

                    this.MouseMove -= this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 취소
                    // 중복방지를 위해 빼고 더한다.
                    this.MouseMove -= this.OnMouseMove_Moving;                          // 선택된 도형 이동함수, 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Moving;                            // 선택된 도형 이동 함수, 마우스 업 함수 취소

                    this.MouseMove += this.OnMouseMove_Moving;                          // 선택된 도형 이동함수, 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Moving;                            // 선택된 도형 이동 함수, 마우스 업 함수 적용
                }
                else if (!this.IsRemoveArea && this.MouseIsOverShape(imagePoint, out sIndex))                     // 기존의 ROI를 선택하면
                {
                    foreach (var roi in this.imageinfo.listShape) roi.Selected = false;     // 이전 선택된 ROI를 선택 취소로 설정                 

                    this.cloneShape = null;                                                 // clone ROI 삭제
                    this.IsDrawClone = false;                                               // clone ROI 표시 안함.

                    this.selectedIndex = sIndex;                                            // 선택된 Index 저장
                    this.imageinfo.listShape[this.selectedIndex].Selected = true;           // 선택된 ROI 선택 설정    

                    if (this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineAngle && this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineX)
                    {
                        this.cloneShape = this.imageinfo.listShape[this.selectedIndex].Clone();                 // clone ROI 생성
                        if (this.IsROIMirror) this.cloneShape.Mirror();                                         // 좌우 대칭 실행
                        this.cloneShape.NodeSelected = ROISHAPEPOSTION.Inside;                                  // 이동으로 설정
                    }

                    this.backShape = this.imageinfo.listShape[this.selectedIndex].CopyTo();         // 이동할 ROI를 이전 위치를 저장해 놓는다.
                    this.undoBorderColor = this.imageinfo.listShape[this.selectedIndex].BorderColor;      // 나중에 undo할 때 색을 복원해야 됨.
                    this.undoFontColor = this.imageinfo.listShape[this.selectedIndex].ForeColor;        // 나중에 undo할 때 색을 복원해야 됨.
                    this.backShape.BorderColor = Color.Black;                                                   // 검정색으로 선택
                    this.backShape.ForeColor = Color.Black;                                                   // 검정색으로 선택 
                    this.IsDrawBack = true;                                                          // backShape를 그린다.

                    this.MouseMove -= this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 취소
                    // 중복방지를 위해 빼고 더한다.
                    this.MouseMove -= this.OnMouseMove_Moving;                          // 선택된 도형 이동함수, 마우스 이동 함수 취소
                    this.MouseUp -= this.OnMouseUp_Moving;                            // 선택된 도형 이동 함수, 마우스 업 함수 취소

                    this.MouseMove += this.OnMouseMove_Moving;                          // 선택된 도형 이동함수, 마우스 이동 함수 적용
                    this.MouseUp += this.OnMouseUp_Moving;                            // 선택된 도형 이동 함수, 마우스 업 함수 적용

                }
                else if (this.IsROIDrawing)                                                 // 새로 그리기 모드로 전환
                {
                    // 이전에 선택된 ROI 선택 초기화
                    foreach (var roi in this.imageinfo.listShape) roi.Selected = false;     // 이전 선택된 ROI를 선택 취소로 설정
                    this.selectedIndex = -1;

                    this.newShape = new ROIShape(this.drawShapeType)
                    {
                        ChartNo = this.imageinfo.ChartNo,                // 기준 Iamge가 포함하는 Study Patient ChartNo
                        StudyID = this.imageinfo.StudyID,                // 기준 Image가 포함하는 Study ID
                        ImageIndex = this.imageinfo.ImageIndex,             // 기준 Image Inddex
                        Image_Width = this.ImageInfo.Image_Width,            // 기준 Image Width 설정
                        Image_Height = this.ImageInfo.Image_Height,           // 기준 Image Hedith 설정
                        Font = this.Font,
                        ForeColor = this.ForeColor
                    };
                    this.pointIndex = this.newShape.AddImagePoint(0, imagePoint);       // 처음 점 추가하면 다음에 추가할 Index를 리턴해준다.(이미지 기준의 점 사용)
                    this.newShape.Selected = true;                                      // 도형을 선택한 것으로 변경
                    this.newShape.NodeSelected = ROISHAPEPOSTION.AreaRightBottom;       // 그리는 것을 오른쪽 아래로 설정  

                    this.MouseMove -= this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 취소
                    // 중복방지를 위해 빼고 더한다.
                    this.MouseMove -= this.OnMouseMove_Drawing;                         // 새로운 ROI 그리는 함수, 마우스 이동 함수 취소

                    this.MouseMove += this.OnMouseMove_Drawing;                         // 새로운 ROI 그리는 함수, 마우스 이동 함수 적용
                    if ((this.DrawShapeType != ROISHAPETYPE.Polygon && this.DrawShapeType != ROISHAPETYPE.LineX))
                    {
                        // 중복방지를 위해 빼고 더한다.
                        this.MouseUp -= this.OnMouseUp_Drawing;                         // 새로운 ROI 그리는 함수, 마우스 업 함수( Polygon, LineX는 함수 적용 안함 ) 취소
                        this.MouseUp += this.OnMouseUp_Drawing;                         // 새로운 ROI 그리는 함수, 마우스 업 함수( Polygon, LineX는 함수 적용 안함 ) 적용
                    }
                    else
                    {
                        this.MouseDown -= this.OnMouseDown_NotDrawing;
                        // 중복방지를 위해 빼고 더한다.
                        this.MouseDown -= this.OnMouseDown_PolygonDrawing;
                        this.MouseDown += this.OnMouseDown_PolygonDrawing;
                    }
                }

                // ROI가 선택되었을 경우 호출
                if (this.SelectedROIValueChanged != null && !this.IsRemoveArea)
                {
                    ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();

                    if (this.selectedIndex >= 0 && this.selectedIndex < this.imageinfo.listShape.Count)
                    {   // 선택된 ROI가 있으면서 LineAngle, LineX가 아닐경우
                        if (this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineAngle && this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineX)
                        {
                            roiInfo.Rois.Add(this.imageinfo.listShape[this.selectedIndex]);     // 선택된 ROI 정보를 저장한다.
                        }
                    }

                    this.SelectedROIValueChanged(this, roiInfo);
                }
            }

            this.Invalidate();
        }

        /** @brief: 평소 마우스 이동 함수
         *  @section: Description
         *      아무것도 모드 아닌 경우의 마우스 이동 함수
         */
        private void OnMouseMove_NotDrawing(object sender, MouseEventArgs e)
        {
            this.SelectImageGrid = null;

            // 이미지 기준의 점으로 변경해서 사용한다.
            PointF imagePoint = this.ConvertImagePoint(e.Location);

            foreach (ImageROIGrid imageGrid in this.listImageGrid)
            {
                if (imageGrid.GetNodeSelectable(imagePoint) != ROISHAPEPOSTION.None)
                {
                    this.SelectImageGrid = imageGrid;
                    break;
                }
            }

            if (this.SelectImageGrid != null)                        // ImageROIGrid가 선택된 경우
            {
                this.Cursor = this.SelectImageGrid.GetCursor();
                this.IsDrawClone = false;                            // Clone ROI를 안 그린다.
            }
            else if (this.multiSelectedROIGuide != null && this.multiSelectedROIGuide.GetNodeSelectable(imagePoint))                          // 멀티 선택용 Guide가 있는 경우
            {
                // ROI 선택 Node를 취소
                if (this.imageinfo != null) foreach (var roi in this.imageinfo.listShape) roi.NodeSelected = ROISHAPEPOSTION.None;

                if (e.Button == MouseButtons.Right)                                  // 멀티 선택용 취호다.
                {
                    this.multiSelectedROIGuide.NoSelectedROI();                     // 멀티 선택용 Guide에 포함된 ROI의 선택을 취소한다.
                    this.multiSelectedROIGuide = null;
                    this.Invalidate();
                    return;
                }

                this.Cursor = this.multiSelectedROIGuide.GetCursor();
            }
            else if (this.imageinfo != null)
            {
                int tempIndex;
                if (IsRemoveArea)
                {
                    this.MouseIsOverRemoveShape(imagePoint, out tempIndex);                         // 도형 List에서 해당위치의 도형 Index를 찾는다.
                    if (tempIndex >= 0)
                    {
                        this.Cursor = this.imageinfo.listRemoveShape[tempIndex].GetCursor();   // 선택된 도형의 Cursor 모양을 적용한다.                        
                        this.IsDrawClone = false;                                                   // Clone ROI를 안 그린다.
                    }
                }
                else
                {
                    this.MouseIsOverShape(imagePoint, out tempIndex);                               // 도형 List에서 해당위치의 도형 Index를 찾는다.
                    if (tempIndex >= 0)
                    {
                        this.Cursor = this.imageinfo.listShape[tempIndex].GetCursor();              // 선택된 도형의 Cursor 모양을 적용한다.
                        this.IsDrawClone = false;                                                   // Clone ROI를 안 그린다.
                    }
                }

                if (tempIndex < 0)
                {
                    this.Cursor = Cursors.Cross;                    // 못찾으면 Cursor 모양을 Cross  적용

                    if (this.cloneShape != null)
                    {
                        this.cloneShape.MoveTo(imagePoint);
                        this.IsDrawClone = true;                                                    // Clone ROI를 그린다.
                    }
                }
            }

            this.Invalidate();
        }

        /** @brief: Polygon, LineX 그리는 마우스 다운 함수  
         *  @section: Description
         *      Polygon, LineX 그리는 모드의 마우스 이동 함수
         */
        private void OnMouseDown_PolygonDrawing(object sender, MouseEventArgs e)
        {
            if (this.imageinfo == null) return;

            // 이미지 기준의 점으로 변경해서 사용한다.
            PointF imagePoint = this.ConvertImagePoint(e.Location);

            if (this.newShape != null)                                                              // ROI를 그리는 중( Polygon, LineX 적용 )
            {
                this.pointIndex = this.newShape.AddImagePoint(this.pointIndex, imagePoint);         // Polygon, LineX는 클릭으로 점을 추가한다.

                if (e.Button == MouseButtons.Right)                                      // 오른쪽 버튼 클릭은 그리기 종료버튼
                {
                    // Polygon, LineX 그리는 중이면 Popup Menu를 안보이게 설정
                    this.IsMenuShow = false;

                    // Polygon인 경우
                    if (this.DrawShapeType == ROISHAPETYPE.Polygon)
                    {
                        this.newShape.PolygonRemoveLastPoint();                                     // 마지막 점은 삭제한다.( Right 버튼은 취소 버튼 )

                        if (this.newShape.IsShape())
                        {
                            this.ListShapeAdd(this.newShape);
                            if (this.IsRemoveArea) this.selectedIndex = this.imageinfo.listRemoveShape.Count - 1;      // 마지막에 그린 인체 영역시 제거 ROI 도형를 선택한다.
                            else
                            {
                                this.selectedIndex = this.imageinfo.listShape.Count - 1;      // 마지막에 그린 ROI를 선택한다.
                                this.cloneShape = this.newShape.Clone();                   // clone ROI 생성
                                if (this.IsROIMirror) this.cloneShape.Mirror();                         // 좌우 대칭 실행
                                this.cloneShape.NodeSelected = ROISHAPEPOSTION.Inside;                  // 이동으로 설정
                                this.IsDrawClone = true;                                    // Clone ROI를 그린다.
                            }
                        }
                    }

                    this.newShape = null;
                    this.MouseDown -= this.OnMouseDown_PolygonDrawing;                  // Polygon, LineX 그리는 마우스 다운 함수 취소                    
                    this.MouseMove -= this.OnMouseMove_Drawing;                         // 새로운 ROI 그리는 함수, 마우스 이동 함수 취소
                    // 중복방지를 위해 빼고 더한다.
                    this.MouseDown -= this.OnMouseDown_NotDrawing;                      // 평소 마우스 다운 함수 취소
                    this.MouseMove -= this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 취소

                    this.MouseDown += this.OnMouseDown_NotDrawing;                      // 평소 마우스 다운 함수 적용
                    this.MouseMove += this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 적용
                }
                else
                {
                    // LineX인 경우는 점 4개이면 그리기 종료이다. 
                    if (!this.IsRemoveArea && this.DrawShapeType == ROISHAPETYPE.LineX && this.newShape.IsShape())
                    {
                        this.ListShapeAdd(this.newShape);
                        this.selectedIndex = this.imageinfo.listShape.Count - 1;              // 마지막에 그린 ROI를 선택한다.

                        this.newShape = null;

                        this.MouseDown -= this.OnMouseDown_PolygonDrawing;          // Polygon, LineX 그리는 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_Drawing;                 // 새로운 ROI 그리는 함수, 마우스 이동 함수 취소
                        // 중복방지를 위해 빼고 더한다.
                        this.MouseDown -= this.OnMouseDown_NotDrawing;              // 평소 마우스 다운 함수 취소
                        this.MouseMove -= this.OnMouseMove_NotDrawing;              // 평소 마우스 이동하는 함수 취소

                        this.MouseDown += this.OnMouseDown_NotDrawing;              // 평소 마우스 다운 함수 적용
                        this.MouseMove += this.OnMouseMove_NotDrawing;              // 평소 마우스 이동하는 함수 적용
                    }
                }
            }

            this.Invalidate();
        }

        /** @brief: 선택된 도형 이동함수, 마우스 이동 함수
         *  @section: Description
         *      선택된 도형 이동함수, 마우스 이동 함수
         */
        private void OnMouseMove_Moving(object sender, MouseEventArgs e)
        {
            if (this.imageinfo == null) return;

            // 이미지 기준의 점으로 변경해서 사용한다.
            PointF imagePoint = this.ConvertImagePoint(e.Location);

            float xOffset = imagePoint.X - movePoint.X;
            float yOffset = imagePoint.Y - movePoint.Y;

            if (this.IsRemoveArea)      // 인체 영역시 제거 모드
            {
                if (this.selectedIndex >= 0 && this.imageinfo.listRemoveShape[this.selectedIndex].NodeSelected != ROISHAPEPOSTION.None)
                {
                    this.imageinfo.listRemoveShape[this.selectedIndex].Offset(xOffset, yOffset);        // 선택된 도형 Offset 만큼 이동
                    ROIShape temp = this.imageinfo.listRemoveShape[this.selectedIndex];

                    // ROI의 min, max, 평균, 표준편차, 넓이를 구한다.;
                    temp.CalROIShare(this.imageinfo.ToMat());

                    this.imageinfo.listRemoveShape[this.selectedIndex] = temp;

                    this.Cursor = this.imageinfo.listRemoveShape[this.selectedIndex].GetCursor();
                }
            }
            else                        // 일반 ROI 그리기 모드
            {
                if (this.selectedIndex >= 0 && this.imageinfo.listShape[this.selectedIndex].NodeSelected != ROISHAPEPOSTION.None)
                {
                    this.imageinfo.listShape[this.selectedIndex].Offset(xOffset, yOffset);              // 선택된 도형 Offset 만큼 이동
                    ROIShape temp = this.imageinfo.listShape[this.selectedIndex];

                    // ROI의 min, max, 평균, 표준편차, 넓이를 구한다.
                    temp.CalROIShare(this.imageinfo.ToMat());

                    this.imageinfo.listShape[this.selectedIndex] = temp;

                    // 선택된 도형이 이동 중인경우 호출
                    if (this.SelectedROIValueChanging != null)
                    {
                        ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();
                        roiInfo.Rois.Add(this.imageinfo.listShape[this.selectedIndex]);     // 선택된 ROI 정보를 저장한다.

                        this.SelectedROIValueChanging(this, roiInfo);
                    }

                    this.Cursor = this.imageinfo.listShape[this.selectedIndex].GetCursor();
                }
            }
            movePoint = imagePoint;

            this.Invalidate();
        }

        /** @brief: 선택된 도형 이동 함수, 마우스 업 함수
         *  @section: Description
         *      선택된 도형 이동 함수, 마우스 업 함수
         */
        private void OnMouseUp_Moving(object sender, MouseEventArgs e)
        {
            if (!this.IsRemoveArea && this.selectedIndex >= 0 && this.imageinfo.listShape[this.selectedIndex].NodeSelected != ROISHAPEPOSTION.None)
            {
                if (this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineAngle && this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineX)
                {
                    this.cloneShape = this.imageinfo.listShape[this.selectedIndex].Clone();    // Clone ROI 생성
                    if (this.IsROIMirror) this.cloneShape.Mirror();                                         // 좌우 대칭 실행
                    this.cloneShape.NodeSelected = ROISHAPEPOSTION.Inside;                                  // 이동으로 설정
                    this.IsDrawClone = true;                                                    // Clone ROI를 그린다.

                    // 이미지 기준의 점으로 변경해서 사용한다.
                    movePoint = this.ConvertImagePoint(e.Location);
                }

                // BackShape 비교해서 위치의 변경이 없으면 backShape 취소
                if (this.backShape.IsSameLocation(this.imageinfo.listShape[this.selectedIndex]))
                {
                    this.backShape = null;
                    this.IsDrawBack = false;
                }
                //
                // 선택된 도형이 이동이 완료한 경우 호출
                if (this.SelectedROIValueChanged != null)
                {
                    ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();
                    if (this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineAngle && this.imageinfo.listShape[this.selectedIndex].ShapeType != ROISHAPETYPE.LineX)
                    {
                        roiInfo.Rois.Add(this.imageinfo.listShape[this.selectedIndex]);     // 선택된 ROI 정보를 저장한다.
                    }
                    this.SelectedROIValueChanged(this, roiInfo);
                }
            }
            this.MouseMove -= this.OnMouseMove_Moving;              // 선택된 도형 이동함수, 마우스 이동 함수 취소
            this.MouseUp -= this.OnMouseUp_Moving;                // 선택된 도형 이동 함수, 마우스 업 함수 취소
            // 중복방지를 위해 빼고 더한다.
            this.MouseMove -= this.OnMouseMove_NotDrawing;          // 평소 마우스 이동하는 함수 취소

            this.MouseMove += this.OnMouseMove_NotDrawing;          // 평소 마우스 이동하는 함수 적용
            this.IsDrawBack = false;                                // 이동이 끝나면 backShape를 그리지 않는다.
            this.Cursor = Cursors.Cross;                            // Cursor 모양을 Cross  적용
        }

        /** @brief: 새로운 ROI 그리는 함수, 마우스 이동 함수
         *  @section: Description
         *      새로운 ROI 그리는 함수, 마우스 이동 함수
         */
        private void OnMouseMove_Drawing(object sender, MouseEventArgs e)
        {
            if (this.imageinfo == null) return;

            // 이미지 기준의 점으로 변경해서 사용한다.
            PointF imagePoint = this.ConvertImagePoint(e.Location);

            if (this.newShape != null)                                      // 이동중이면 그림 이동
            {
                this.newShape.AddImagePoint(this.pointIndex, imagePoint);        // 생성중인 도형: 해당 pointIndex에 점이 없으면 추가, 있으면 교체된다.

                if (this.cloneShape != null && this.newShape.IsDraw())      // 도형 그리기 시작하면 Clone Shape 삭제
                    this.cloneShape = null;

                // ROI의 min, max, 평균, 표준편차, 넓이를 구한다.
                this.newShape.CalROIShare(this.imageinfo.ToMat());

                if (this.NewROIDrawing != null)                             // ROI가 처음 그려지는 부분으로, 함수 선언이 있는 경우 처음 그려지는 ROI 정보를 전달한다.
                {
                    ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();
                    roiInfo.Rois.Add(this.newShape);     // 선택된 ROI 정보를 저장한다.
                    this.NewROIDrawing(this, roiInfo);
                }
            }
            else
            {
                this.MouseDown -= this.OnMouseDown_PolygonDrawing;                  // Polygon, LineX 그리는 마우스 다운 함수 취소
                this.MouseMove -= this.OnMouseMove_Drawing;                         // 새로운 ROI 그리는 함수, 마우스 이동 함수 취소
                // 중복방지를 위해 빼고 더한다.
                this.MouseDown -= this.OnMouseDown_NotDrawing;                      // 평소 마우스 다운 함수 적용
                this.MouseMove -= this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 적용

                this.MouseDown += this.OnMouseDown_NotDrawing;                      // 평소 마우스 다운 함수 적용
                this.MouseMove += this.OnMouseMove_NotDrawing;                      // 평소 마우스 이동하는 함수 적용
            }

            this.Invalidate();
        }

        /** @brief: 새로운 ROI 그리는 함수, 마우스 업 함수( Rectangle, Rhombus, Ellipse, LineAngle ROI인 경우 적용 )
         *  @section: Description
         *      새로운 ROI 그리는 함수, 마우스 업 함수( Rectangle, Rhombus, Ellipse, LineAngle ROI인 경우 적용 )
         */
        private void OnMouseUp_Drawing(object sender, MouseEventArgs e)
        {
            if (this.imageinfo == null) return;

            if (this.newShape != null && this.newShape.IsShape())        // 그리는 도형이 처음만들어진 도형이고 도형이여야 저장함
            {
                this.ListShapeAdd(this.newShape);
                if (this.IsRemoveArea) this.selectedIndex = this.imageinfo.listRemoveShape.Count - 1;
                else
                {
                    this.selectedIndex = this.imageinfo.listShape.Count - 1;

                    if (this.DrawShapeType != ROISHAPETYPE.LineAngle)
                    {
                        this.cloneShape = this.newShape.Clone();                // Clone ROI 생성

                        if (this.IsROIMirror) this.cloneShape.Mirror();         // 좌우 대칭 실행

                        this.cloneShape.NodeSelected = ROISHAPEPOSTION.Inside;  // 이동으로 설정
                        this.IsDrawClone = true;                                // Clone ROI를 그린다.
                        // 이미지 기준의 점으로 변경해서 사용한다.
                        movePoint = this.ConvertImagePoint(e.Location);
                    }
                }
            }

            this.MouseMove -= this.OnMouseMove_Drawing;             // 새로운 ROI 그리는 함수, 마우스 이동 함수 취소
            this.MouseUp -= this.OnMouseUp_Drawing;               // 새로운 ROI 그리는 함수, 마우스 업 함수 취소
            // 중복방지를 위해 빼고 더한다.
            this.MouseMove -= this.OnMouseMove_NotDrawing;          // 평소 마우스 이동하는 함수 적용

            this.MouseMove += this.OnMouseMove_NotDrawing;          // 평소 마우스 이동하는 함수 적용
            this.newShape = null;

            this.Invalidate();
        }

        /** @brief: 마우스 더블클릭시 Clone Shape 저장   
         *  @section: Description
         *      마우스 더블클릭시 Clone Shape 저장   
         */
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (this.imageinfo == null) return;

            if (this.cloneShape != null && this.cloneShape.IsShape())
            {
                this.cloneShape.Selected = true;
                this.cloneShape.ChartNo = this.imageinfo.ChartNo;                // 기준 Image가 포함하는 Study Patient ChartNo
                this.cloneShape.StudyID = this.imageinfo.StudyID;                // 기준 Image가 포함하는 Study ID
                this.cloneShape.ImageIndex = this.imageinfo.ImageIndex;             // 기준 Image Inddex
                this.cloneShape.Image_Width = this.ImageInfo.Image_Width;            // 기준 Image Width 설정
                this.cloneShape.Image_Height = this.ImageInfo.Image_Height;           // 기준 Image Hedith 설정
                this.ListShapeAdd(this.cloneShape);
                if (IsRemoveArea)
                {
                    this.selectedIndex = this.imageinfo.listRemoveShape.Count - 1;                          // 마지막에 그린 ROI를 선택한다.
                }
                else
                {
                    this.selectedIndex = this.imageinfo.listShape.Count - 1;                                    // 마지막에 그린 ROI를 선택한다.
                    this.cloneShape = this.imageinfo.listShape[this.imageinfo.listShape.Count - 1].Clone();  // Clone ROI 생성

                    if (this.IsROIMirror) this.cloneShape.Mirror();                                             // 좌우 대칭 실행

                    this.cloneShape.NodeSelected = ROISHAPEPOSTION.Inside;                 // 이동으로 설정
                    this.IsDrawClone = true;                                                // Clone ROI를 그린다.
                    // 이미지 기준의 점으로 변경해서 사용한다.
                    movePoint = this.ConvertImagePoint(e.Location);
                }

                this.newShape = null;
            }
        }

        /** @brief: 마우스가 PictureBox에 들어올때 처리 함수
         *  @section: Description
         *      마우스가 PictureBox에 들어올때 처리 함수
         */
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if(FocusEnalbe)
                this.Focus();                   // Mouse가 들어오면 Focus를 설정한다.            

            if (this.imageinfo == null) return;

            if (!this.IsPan)
                this.IsDrawClone = true;                                        // 클론을 그린다.

            if (this.IsMag) Cursor.Hide();
            this.ShowMsg = true;

            this.Invalidate();
        }

        /** @brief: 마우스가 Picture Box 밖으로 나갈때 처리 함수
         *  @section: Description
         *       마우스가 Picture Box 밖으로 나갈때 처리 함수
         */
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (this.imageinfo == null) return;

            this.IsDrawClone = false;                                       // 클론을 그리지 않는다.

            if (this.IsMag) Cursor.Show();
            this.ShowMsg = false;

            this.Invalidate();
        }

        #region Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 함수 모음  ( PictureBox 기준 좌표이다.)

        /** @brief: pictureBox_Raw의 마우스 다운인경우 Zoom 이미지 이동 시작 ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *      Zoom 이미지 이동 시작한다.  Zoom이 없을때는 사용안함
         */
        private void OnMouseDown_Zoom(object sender, MouseEventArgs e)
        {
            if (this.Zoom_Ratio <= 1.0) return;     // Zoom이 없을때는 사용안함
            this.picMovePoint = e.Location;
            this.IsZoomMove = true;                 // Zoom 이미지를 이동할 수 있다.

            this.Cursor = Cursors.SizeAll;
        }

        /** @brief: pictureBox_Raw의 마우스 move 인경우 Zoom 이미지 center point의 Offset를 구한다. 마우스 모양을 손 모양으로 변경 ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *      Zoom 이미지를 이동한다.  Zoom이 없을때는 사용안함
         */
        private void OnMouseMove_Zoom(object sender, MouseEventArgs e)
        {
            if (this.Zoom_Ratio <= 1.0) return;           // Zoom이 없을때는 사용안함. PositioningGrid 사용시 사용 못하게 함.            

            if (this.IsZoomMove)
            {
                Point tmpPoint = e.Location;
                int xOffset = picMovePoint.X - tmpPoint.X;
                int yOffset = picMovePoint.Y - tmpPoint.Y;

                zoomCenterOffset.Offset(xOffset, yOffset);      // Zoom Image Center Point의 Offset을 적용한다.

                picMovePoint = tmpPoint;

                this.Invalidate();
            }
        }

        /** @brief: pictureBox_Raw의 마우스 업인경우 Zoom 이미지 이동 정지 ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *      Zoom 이미지를 이동 정지.  Zoom이 없을때는 사용안함
         */
        private void OnMouseUp_Zoom(object sender, MouseEventArgs e)
        {
            this.IsZoomMove = false;                // Zoom 이미지이동 정지

            this.Cursor = Cursors.Default;
        }

        /** @brief: pictureBox_Raw의 마우스 휠을 이용해서 Zoom 배율 설정(1.0 ~ 4.0)
         *  @section: Description
         *      pictureBox_Raw의 마우스 휠을 이용해서 Zoom 배율 설정 (1.0 ~ 4.0)
         */
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!this.IsZoom) return;

            int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

            if (lines < 0)
            {
                this.Zoom_Ratio += 0.1F;
                if (this.Zoom_Ratio > 4.0) this.Zoom_Ratio = 4.0F;
            }
            else if (lines > 0)
            {
                this.Zoom_Ratio -= 0.1F;
                if (Zoom_Ratio < 1) this.Zoom_Ratio = 1.0F;
            }

            this.Invalidate();
        }

        #endregion Zoom 기능인 경우에 Image를 이동 시키기 위해서 사용 함수 모음  ( PictureBox 기준 좌표이다.)

        #region CROP 기능인 경우 함수 모음  ( PictureBox 기준 좌표이다.)

        /** @brief: pictureBox_Raw의 마우스 다운인경우 Crop 사각형 그리기 시작  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *       pictureBox_Raw의 마우스 다운인경우 Crop 사각형 그리기 시작
         */
        private void OnMouseDown_Crop(object sender, MouseEventArgs e)
        {
            this.cropPoint = e.Location;          // Crop 시작 point 저장
            this.picMovePoint = e.Location;
            this.IsCropShape = true;                // Crop Shape를 그린다.
            this.IsCropCompleted = false;               // Crop 완료여부

            this.Cursor = Cursors.Cross;
        }

        /** @brief: pictureBox_Raw의 마우스 move 인경우 Crop 사각형 그리기  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *       pictureBox_Raw의 마우스 move 인경우 Crop 사각형 그리기
         */
        private void OnMouseMove_Crop(object sender, MouseEventArgs e)
        {
            if (this.IsCropShape)
            {
                this.picMovePoint = e.Location;

                this.Invalidate();
            }
        }

        /** @brief: pictureBox_Raw의 마우스 업인경우 Crop 사각형 그리기 정지  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *      pictureBox_Raw의 마우스 업인경우 Crop 사각형 그리기 정지
         */
        private void OnMouseUp_Crop(object sender, MouseEventArgs e)
        {
            this.IsCropShape = false;               // Crop 이미지이동 정지

            var minX = Math.Min(this.cropPoint.X, this.picMovePoint.X);
            var minY = Math.Min(this.cropPoint.Y, this.picMovePoint.Y);
            var maxX = Math.Max(this.cropPoint.X, this.picMovePoint.X);
            var maxY = Math.Max(this.cropPoint.Y, this.picMovePoint.Y);
            Rectangle cropRect = new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
            if (cropRect.Width > 0 && cropRect.Height > 0)
            {
                this.IsCropCompleted = true;
                this.CropRect = cropRect;
            }

            this.Invalidate();
            this.Cursor = Cursors.Default;
        }

        #endregion CROP 기능인 경우 함수 모음

        #region Bring 기능인 경우 함수 모음 ( PictureBox 기준 좌표이다.)

        /** @brief: pictureBox_Raw의 마우스 다운인경우 Bring Line 그리기 시작  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *       pictureBox_Raw의 마우스 다운인경우 Bring Line 그리기 시작
         */
        private void OnMouseDown_Bring(object sender, MouseEventArgs e)
        {
            this.bringPoint = e.Location;          // Bring 시작 point 저장
            this.picMovePoint = e.Location;
            this.IsBringView = true;                // Bring Line를 그린다.

            this.Cursor = Cursors.Cross;
        }

        /** @brief: pictureBox_Raw의 마우스 move 인경우 Bring Line 그리기  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *       pictureBox_Raw의 마우스 move 인경우 Bring Line 그리기
         */
        private void OnMouseMove_Bring(object sender, MouseEventArgs e)
        {
            if (this.IsBringView)
            {
                this.picMovePoint = e.Location;

                this.Invalidate();
            }

            if (this.BringMoveInfo != null && this.imageinfo != null)
            {
                float conX = (float)e.Location.X * (float)this.imageinfo.Image_Width / (float)this.Width;
                float conY = (float)e.Location.Y * (float)this.imageinfo.Image_Height / (float)this.Height;

                int nNowIndex = ((int)conX + ((int)conY * this.ImageInfo.Image_Width));

                BringInfoEventArge bringInfo = new BringInfoEventArge
                {
                    nowPoint = new Point((int)conX, (int)conY),
                    nowValue = this.ImageInfo.ImageBuffer[nNowIndex]
                };
                this.BringMoveInfo(this, bringInfo);
            }
        }

        /** @brief: pictureBox_Raw의 마우스 업인경우 Bring Line 그리기 정지  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *       pictureBox_Raw의 마우스 업인경우 Bring Line 그리기 정지 
         */
        private void OnMouseUp_Bring(object sender, MouseEventArgs e)
        {
            this.IsBringView = false;               // Crop 이미지이동 정지

            int distance = Convert.ToInt32(ImageUsedMath.DistanceToPoint(this.bringPoint, this.picMovePoint));

            if (distance > 0 && this.imageinfo != null)
            {
                this.IsBringCompleted = true;
                this.bringSP = this.bringPoint;
                this.bringEP = e.Location;

                if (this.BringTheValue != null)
                {
                    float conX = (float)e.Location.X * (float)this.imageinfo.Image_Width / (float)this.Width;
                    float conY = (float)e.Location.Y * (float)this.imageinfo.Image_Height / (float)this.Height;
                    float spX = (float)bringSP.X * (float)this.imageinfo.Image_Width / (float)this.Width;
                    float spY = (float)bringSP.Y * (float)this.imageinfo.Image_Height / (float)this.Height;
                    float epX = (float)bringEP.X * (float)this.imageinfo.Image_Width / (float)this.Width;
                    float epY = (float)bringEP.Y * (float)this.imageinfo.Image_Height / (float)this.Height;
                    int xSize = Math.Abs((int)spX - (int)epX), ySize = Math.Abs((int)spY - (int)epY);

                    int nNowIndex = ((int)conX + ((int)conY * this.ImageInfo.Image_Width));
                    BringInfoEventArge bringInfo = new BringInfoEventArge
                    {
                        nowPoint = new Point((int)conX, (int)conY),
                        nowValue = this.ImageInfo.ImageBuffer[nNowIndex],
                        startPoint = new Point((int)spX, (int)spY),
                        endPoint = new Point((int)epX, (int)epY),
                        Distance = Math.Max(xSize, ySize),
                        bringValues = new byte[Math.Max(xSize, ySize)]
                    };

                    int newX, newY, xGap, yGap;

                    for (int index = 0; index < bringInfo.Distance; index++)
                    {
                        if (xSize >= ySize) { xGap = index; yGap = (ySize * index / xSize); }
                        else { xGap = (xSize * index / ySize); yGap = index; }

                        newX = Math.Min((int)spX, (int)epX) + xGap;
                        newY = Math.Min((int)spY, (int)epY) + yGap;

                        int nRawIndex = (newX + (newY * this.ImageInfo.Image_Width));

                        bringInfo.bringValues[index] = this.ImageInfo.ImageBuffer[nRawIndex];
                    }
                    this.BringTheValue(this, bringInfo);
                }
            }

            this.Invalidate();
            this.Cursor = Cursors.Default;
        }

        #endregion Bring 기능인 경우 함수 모음

        /** @brief: pictureBox_Raw의 마우스 move 인경우 이미지 확대 기능  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *      pictureBox_Raw의 마우스 move 인경우 이미지 확대 기능
         */
        private void OnMouseMove_Msg(object sender, MouseEventArgs e)
        {
            if (this.IsMag)
            {
                picMovePoint = e.Location;

                this.Invalidate();
            }
        }

        /** @brief: pictureBox_Raw의 마우스 move 인경우 선택된 Grid 이동 함수  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *      ictureBox_Raw의 마우스 move 인경우 선택된 Grid 이동 함수
         */
        private void OnMouseMove_GridMoving(object sender, MouseEventArgs e)
        {
            PointF imagePoint = this.ConvertImagePoint(e.Location);
            float xOffset = imagePoint.X - movePoint.X;
            float yOffset = imagePoint.Y - movePoint.Y;

            if (this.SelectImageGrid != null && this.SelectImageGrid.NodeSelected != ROISHAPEPOSTION.None)      // PictureBox 기준
            {
                this.SelectImageGrid.Offset(xOffset, yOffset);        // 선택된 Grid Offset 만큼 이동
                this.Cursor = this.SelectImageGrid.GetCursor();
            }
            else if (this.multiSelectedROIGuide != null && this.multiSelectedROIGuide.NodeSelected != ROISHAPEPOSTION.None) // Image 기준
            {

                this.multiSelectedROIGuide.RotatedAreaOffset(xOffset, yOffset); // 선택된 멀티 선택용 Guide를 Offet 만큼 이동( 내부에 포함됨 ROI도 같이 이동)
                this.Cursor = this.multiSelectedROIGuide.GetCursor();
            }

            movePoint = imagePoint;

            this.Invalidate();
        }

        /** @brief: pictureBox_Raw의 마우스 업인경우 선택된 Grid 이동 정지  ( PictureBox 기준 좌표이다.)
         *  @section: Description
         *      ictureBox_Raw의 마우스 move 인경우 선택된 Grid 이동 정지
         */
        private void OnMouseUp_GridMoving(object sender, MouseEventArgs e)
        {
            if (this.multiSelectedROIGuide != null && this.multiSelectedROIGuide.NodeSelected != ROISHAPEPOSTION.None) // Image 기준
            {
                // 선택된 멀티 선택용 Guide에 포함된 ROI의 min, max, 평균, 표준편차, 넓이를 구한다.                
                this.multiSelectedROIGuide.CalSelectedROIShare(this.imageinfo.ToMat());

                // 선택된 도형이 이동 중인경우 호출
                if (this.SelectedROIValueChanged != null)
                {
                    ROIInfoEventArgs roiInfo = new ROIInfoEventArgs();

                    roiInfo.Rois.AddRange(this.multiSelectedROIGuide.SelectedROIList);

                    this.SelectedROIValueChanged(this, roiInfo);
                }
            }

            this.MouseMove -= this.OnMouseMove_GridMoving;          // 선택된 도형 Grid 함수, 마우스 이동 함수 취소
            this.MouseUp -= this.OnMouseUp_GridMoving;              // 선택된 도형 Grid 함수, 마우스 업 함수 취소
            // 중복방지를 위해 빼고 더한다.
            this.MouseMove -= this.OnMouseMove_NotDrawing;          // 평소 마우스 이동하는 함수 취소
            this.MouseMove += this.OnMouseMove_NotDrawing;          // 평소 마우스 이동하는 함수 적용
            this.Cursor = Cursors.Cross;                            // Cursor 모양을 Cross  적용
        }
    #endregion 마우스 이벤트 처리

    #region Picture Box를 그린다.

        /** @brief:  PictureBox를 그린다.
        *  @section: Description
        *      Image, ROI등, PictureBox에 그리는 부분이다.
        */
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.imageinfo == null) return;

            // 기본 이미지 그리기( ROI 포함 )
            Bitmap backImage = new Bitmap(this.Width, this.Height);
            using (Graphics g = Graphics.FromImage(backImage))
            {
                // 바탕색 검정으로
                g.FillRectangle(Brushes.Black, 0, 0, this.Width, this.Height);

                DrawIRISImage(g);       // 이미지 그리기 ( ROI 부분 같이 들어있음 )

                g.Dispose();
            }
            e.Graphics.DrawImage(backImage, 0, 0);

            // 자르기 모양 그리기
            //if (this.IsCropShape)
            //{
            //    var minX = Math.Min(this.cropPoint.X, this.picMovePoint.X);
            //    var minY = Math.Min(this.cropPoint.Y, this.picMovePoint.Y);
            //    var maxX = Math.Max(this.cropPoint.X, this.picMovePoint.X);
            //    var maxY = Math.Max(this.cropPoint.Y, this.picMovePoint.Y);
            //    e.Graphics.DrawRectangle(Pens.White, new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY)));
            //}

            // 돋보기 기능 부분
            //if (this.IsMag && this.ShowMsg)
            //{
            //    Rectangle MsgRect = new Rectangle(0, 0, this.Width / 10, this.Width / 10);
            //    MsgRect.Location = new Point(picMovePoint.X - (MsgRect.Width / 2), picMovePoint.Y - (MsgRect.Height / 2));
            //    if (this.ClientRectangle.Contains(MsgRect.Location) && (MsgRect.Right < this.Width && MsgRect.Bottom < this.Height))
            //    {
            //        // 마우스 위치에서 이미지를 캡처한다.
            //        Bitmap magImage = new Bitmap(MsgRect.Width, MsgRect.Height);
            //        magImage = backImage.Clone(MsgRect, System.Drawing.Imaging.PixelFormat.DontCare);

            //        // 캡처한 이미지를 Mag_Ratio만큼 확대한다.
            //        Rectangle ShowRect = new Rectangle(0, 0, Convert.ToInt32(MsgRect.Width * Mag_Ratio), Convert.ToInt32(MsgRect.Height * Mag_Ratio));

            //        // 원으로 그리기위한 코드이다.              
            //        Bitmap CircleImage = new Bitmap(ShowRect.Width, ShowRect.Height);
            //        using (Graphics g = Graphics.FromImage(CircleImage))
            //        {
            //            // 배경색을 설정
            //            var rect = new Rectangle(0, 0, CircleImage.Width, CircleImage.Height);
            //            using (Brush br = new SolidBrush(Color.Transparent))
            //            {
            //                g.FillRectangle(br, 0, 0, CircleImage.Width, CircleImage.Height);
            //            }

            //            // 원모양으로 Clip
            //            GraphicsPath path = new GraphicsPath();
            //            path.AddEllipse(0, 0, CircleImage.Width, CircleImage.Height);
            //            g.SetClip(path);

            //            // 소스이미지를 원모양으로 잘라 타겟이미지에 출력
            //            g.DrawImage(magImage, rect);
            //            if (MagnifierImage != null) g.DrawImage(MagnifierImage, 0, 0, CircleImage.Width, CircleImage.Height);
            //            else g.DrawEllipse(new Pen(Brushes.White, 2), rect);
            //            g.Dispose();
            //        }

            //        e.Graphics.DrawImage(CircleImage, new Point(picMovePoint.X - (ShowRect.Width / 2), picMovePoint.Y - (ShowRect.Height / 2)));
            //    }
            //}

            // Bring 그리기
            //if (this.IsBring)
            //{
            //    if (this.IsBringView)
            //    {
            //        DrawBringLine(e.Graphics, this.bringPoint, this.picMovePoint);
            //    }

            //    if (this.IsBringCompleted)
            //    {
            //        DrawBringLine(e.Graphics, this.bringSP, this.bringEP);
            //    }
            //}

            // backImage 메모리 해재
            backImage.Dispose();

            // ImageROIGrid 그리기
            //foreach (ImageROIGrid imageGrid in this.listImageGrid) imageGrid.Draw(e.Graphics, this.Width, this.Height, this.subMatLeft, this.subMatTop, this.Zoom_Ratio);//.Draw(e.Graphics);

            // 멀티 선택용 Rect 그리기
            //if (this.IsDrawMultiSelectedRect)
            //{
            //    // roiAres의 값은 PictureBox의 값이다 그래서 이미지 비율에 맞추기 작업이 필요
            //    double picWidth = this.Width;                // 지금 PictureBox의 넓이
            //    double picHeight = this.Height;               // 지금 PictureBox의 높이
            //    double imageWidth = this.imageinfo.Image_Width;       // 이미지의 넓이
            //    double imageHeight = this.imageinfo.Image_Height;      // 이미지의 높이

            //    float minX = Math.Min(this.MultiSelectStartPoint.X, movePoint.X);
            //    float maxX = Math.Max(this.MultiSelectStartPoint.X, movePoint.X);
            //    float minY = Math.Min(this.MultiSelectStartPoint.Y, movePoint.Y);
            //    float maxY = Math.Max(this.MultiSelectStartPoint.Y, movePoint.Y);

            //    float left = (float)((minX * this.Zoom_Ratio - this.subMatLeft) * picWidth / imageWidth);                      // PictureBox의 기준(zoom, pan 포함)으로 x 값을 변경한다.
            //    float top = (float)((minY * this.Zoom_Ratio - this.subMatTop) * picHeight / imageHeight);                     // PictureBox의 기준(zoom, pan 포함)으로 y 값을 변경한다.

            //    float width = (float)(((maxX - minX) * this.Zoom_Ratio) * picHeight / imageHeight);                     // PictureBox의 기준(zoom, pan 포함)으로 width 값을 변경한다.
            //    float height = (float)(((maxY - minY) * this.Zoom_Ratio) * picHeight / imageHeight);                     // PictureBox의 기준(zoom, pan 포함)으로 height 값을 변경한다

            //    Pen pen = new Pen(Brushes.LightGray, 1)
            //    {
            //        DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot
            //    };

            //    e.Graphics.DrawRectangle(pen, left, top, width, height);
            //}

            //// 멀티 선택 Guide 그리기 , Control 누른 상태에서 회전점과 가이드 점을 그리지 않는다.
            //if (this.multiSelectedROIGuide != null) this.multiSelectedROIGuide.Draw(e.Graphics, this.Width, this.Height, this.subMatLeft, this.subMatTop, this.Zoom_Ratio);
            //// Grid Line 그리기
            //if (this.GridLineType != GRIDLINE_TYPE.Grid_None)
            //{
            //    int lineCount = 2;
            //    switch (this.GridLineType)
            //    {
            //        case GRIDLINE_TYPE.Grid_2x2: lineCount = 2; break;
            //        case GRIDLINE_TYPE.Grid_5x5: lineCount = 5; break;
            //        case GRIDLINE_TYPE.Grid_21x21: lineCount = 21; break;
            //    }

            //    double wGap = (double)this.Width / (double)lineCount;
            //    double hGap = (double)this.Height / (double)lineCount;

            //    for (int index = 0; index < lineCount - 1; index++)
            //    {
            //        e.Graphics.DrawLine(Pens.DarkGray, new Point(0, (int)(hGap * (index + 1))), new Point(this.Width, (int)(hGap * (index + 1))));
            //        e.Graphics.DrawLine(Pens.DarkGray, new Point((int)(wGap * (index + 1)), 0), new Point((int)(wGap * (index + 1)), this.Height));
            //    }
            //}

            // IRImagePictureBox의 Index 값을 화면에 표시                 
            //int indexWidth = 0;
            //if (this.IsIndexShow)
            //{
            //    string strIndex = String.Format("{0}", this.ImageInfo.ImageIndex);
            //    Rectangle IndexRect = new Rectangle(0, 0, (int)((this.Font.SizeInPoints * strIndex.Length) + 1), this.Font.Height);
            //    indexWidth = IndexRect.Width;
            //    e.Graphics.FillRectangle(Brushes.White, IndexRect);

            //    e.Graphics.DrawString(strIndex, this.Font, Brushes.Black, IndexRect,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //}

            // 왼쪽의 마크 그리기
            //if (this.ImageInfo.Mark_Left != MARK_TYPE.NONE)
            //{
            //    string strMark = "L";
            //    Font markFont = new Font(this.Font.FontFamily, 22);

            //    if (this.ImageInfo.Mark_Left == MARK_TYPE.L_MARK) strMark = "L";
            //    else strMark = "R";
            //    Rectangle markTop = new Rectangle(indexWidth, 0, (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.Black, markTop,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //    markTop.Offset(-1, -1);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.White, markTop,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });

            //    Rectangle markBottom = new Rectangle(indexWidth, this.ClientRectangle.Bottom - markFont.Height,
            //                                            (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.Black, markBottom,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //    markBottom.Offset(-1, -1);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.White, markBottom,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //}

            //// 오른쪽 마크 그리기
            //if (this.ImageInfo.Mark_Right != MARK_TYPE.NONE)
            //{
            //    string strMark = "L";
            //    Font markFont = new Font(this.Font.FontFamily, 22);

            //    if (this.ImageInfo.Mark_Right == MARK_TYPE.L_MARK) strMark = "L";
            //    else strMark = "R";
            //    int rLeft = this.ClientRectangle.Right - (int)((markFont.SizeInPoints * strMark.Length) + 1);
            //    Rectangle markTop = new Rectangle(rLeft, 0,
            //                                        (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.Black, markTop,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //    markTop.Offset(-1, -1);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.White, markTop,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //    Rectangle markBottom = new Rectangle(rLeft, this.ClientRectangle.Bottom - markFont.Height,
            //                                            (int)((markFont.SizeInPoints * strMark.Length) + 1), markFont.Height);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.Black, markBottom,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //    markBottom.Offset(-1, -1);
            //    e.Graphics.DrawString(strMark, markFont, Brushes.White, markBottom,
            //                            new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            //}

            //// PictureBox 선택효과 그리기
            //switch (this.selectedtype)
            //{
            //    case SELECTED_TYPE.Selected:
            //        {
            //            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, Color.Red, ButtonBorderStyle.Solid);
            //            break;
            //        }
            //    case SELECTED_TYPE.Enter:
            //        {
            //            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, Color.White, ButtonBorderStyle.Solid);
            //            break;
            //        }
            //}
        }

        /** @brief: IR Image를 그린다.( Zoom, Pan 기능, Crop 기능, 등온선, ROI, 인체제외 영역 추가)
         *  @section: Description
         *      IR Image를 그린다.( Zoom, Pan 기능 추가), ROI, 인체제외 영역도 그린다.
         *      Crop 기능, 등온선
         */
        private void DrawIRISImage(Graphics graphics)
        {
            if (this.imageinfo != null && this.imageinfo.ImageBuffer != null)
            {
                //OpenCvSharp.Mat imageMat = new OpenCvSharp.Mat(this.imageinfo.Image_Height, this.imageinfo.Image_Width, OpenCvSharp.MatType.CV_8U, this.imageinfo.ImageBuffer);
                OpenCvSharp.Mat imageMat = null;
                if (imageinfo.Pixel_Format == 24)
                    imageMat = new OpenCvSharp.Mat(this.imageinfo.Image_Height, this.imageinfo.Image_Width, OpenCvSharp.MatType.CV_8UC3, this.imageinfo.ImageBuffer);
                //else if (imageinfo.Pixel_Format == 32)
                //    imageMat = new OpenCvSharp.Mat(this.imageinfo.Image_Height, this.imageinfo.Image_Width, OpenCvSharp.MatType.CV_8UC4, this.imageinfo.ImageBuffer);
                else
                    imageMat = new OpenCvSharp.Mat(this.imageinfo.Image_Height, this.imageinfo.Image_Width, OpenCvSharp.MatType.CV_8U, this.imageinfo.ImageBuffer);

                // 이미지 최종 저장소
                OpenCvSharp.Mat resultMat = imageMat.Clone();// new OpenCvSharp.Mat();
                #region Zoom 기능 (this.Zoom_Ratio : 1.0 ~ 4.0 사이 값, 0.1단위로 변경됨.)
                if (this.IsZoom)
                {
                    double zoomWidth = this.imageinfo.Image_Width * this.Zoom_Ratio;
                    double zoomHeight = this.imageinfo.Image_Height * this.Zoom_Ratio;

                    // 원본 Mat을 새로운 Size로 변경할 때 Zoom 기능까지 포함해서 Resize한다.
                    OpenCvSharp.Mat newMat = imageMat.Resize(new OpenCvSharp.Size(zoomWidth, zoomHeight), 0, 0, OpenCvSharp.InterpolationFlags.Cubic);
                    this.subMatLeft = (int)((zoomWidth / 2) - (this.imageinfo.Image_Width / 2)) + this.zoomCenterOffset.X;
                    this.subMatTop = (int)((zoomHeight / 2) - (this.imageinfo.Image_Height / 2)) + this.zoomCenterOffset.Y;
                    if (this.subMatLeft < 0)
                    {
                        this.subMatLeft = 0;
                        this.zoomCenterOffset.X = (int)((this.imageinfo.Image_Width / 2) - (zoomWidth / 2));
                    }
                    if (this.subMatTop < 0)
                    {
                        this.subMatTop = 0;
                        this.zoomCenterOffset.Y = (int)((this.imageinfo.Image_Height / 2) - (zoomHeight / 2));
                    }

                    if (this.subMatLeft + this.imageinfo.Image_Width > (int)zoomWidth)
                    {
                        subMatLeft = (int)zoomWidth - this.imageinfo.Image_Width;
                        this.zoomCenterOffset.X = (int)zoomWidth - this.imageinfo.Image_Width + (int)((this.imageinfo.Image_Width / 2) - (zoomWidth / 2));
                    }

                    if (this.subMatTop + this.imageinfo.Image_Height > (int)zoomHeight)
                    {
                        this.subMatTop = (int)zoomHeight - this.imageinfo.Image_Height;
                        this.zoomCenterOffset.Y = (int)zoomHeight - this.imageinfo.Image_Height + (int)((this.imageinfo.Image_Height / 2) - (zoomHeight / 2));
                    }
                    resultMat = newMat.SubMat(new OpenCvSharp.Rect(this.subMatLeft, this.subMatTop, this.imageinfo.Image_Width, this.imageinfo.Image_Height));
                }
                else
                {
                    this.subMatLeft = 0;
                    this.subMatTop = 0;
                    this.Zoom_Ratio = 1.0f;
                }
                #endregion Zoom 기능 (this.Zoom_Ratio : 1.0 ~ 4.0 사이 값, 0.1단위로 변경됨.)

                // Crop 영역이 있으면 Crop 처리한다. resultMat을 하기 때문에 Zoom 처리 후 바로해야 된다.
                //if (this.CropImageIsCompleted != null && this.IsCrop && this.IsCropCompleted && this.CropRect.Width > 0 && this.CropRect.Height > 0)
                //{
                //    this.IsCropCompleted = false;
                //    OpenCvSharp.Mat cropMat = ImageCROP(resultMat, this.CropRect);

                //    if (cropMat != null)
                //    {
                //        CropImageInfoEventArgs cropInfo = new CropImageInfoEventArgs
                //        {
                //            CropImage = new ImageInfo
                //            {
                //                Image_Width = cropMat.Width,                             // 자른 이미지 x Resultion 입력
                //                Image_Height = cropMat.Height,                            // 자른 이미지 y Resultion 입력                                                               
                //                minWindowLevel = this.imageinfo.minWindowLevel,             // min WindowLevel 입력
                //                maxWindowLevel = this.imageinfo.maxWindowLevel,             // max WindowLevel 입력
                //                iIsoLow = this.imageinfo.iIsoLow,                    // ISO Min 값 입력
                //                iIsoHigh = this.imageinfo.iIsoHigh,                   // ISO Max 값 입력
                //                selectedPaletteType = this.imageinfo.selectedPaletteType,        // 선택된 Palette Type를 저장한다. 이것은 PALETTE COLOR {0}, type 형태로 저장된다.
                //                IsWL_BG = this.imageinfo.IsWL_BG,
                //                ImagePalette = bitmapimageCtl.GetPalette(this.imageinfo.minWindowLevel, this.imageinfo.maxWindowLevel, this.imageinfo.IsWL_BG,
                //                                                                nISOLow: this.imageinfo.iIsoLow, nISOHigh: this.imageinfo.iIsoHigh), // 적용된 Palette 입력
                //                ImageBuffer = new byte[cropMat.Width * cropMat.Height]   // 자른 이미지 저장( Resize, Filter 적용 이미지 )
                //            }
                //        };

                //        cropMat.GetArray(out byte[] buff);
                //        cropInfo.CropImage.ImageBuffer = buff;  //Mat Data를 Byte[]로 변환

                //        this.CropImageIsCompleted(this, cropInfo);
                //    }
                //}

                // 등온선 설정시 등온선 Line 그리기
                //if (this.imageinfo != null && this.imageinfo.IsISOTherm)
                //{
                //    // 이미지 임시 저장소          
                //    OpenCvSharp.Mat tempMat = new OpenCvSharp.Mat();
                //    OpenCvSharp.Mat thresholdMat = new OpenCvSharp.Mat();

                //    // 이미지를 GaussianBlur 필터 적용한다.
                //    resultMat.CopyTo(tempMat);
                //    OpenCvSharp.Cv2.GaussianBlur(tempMat, resultMat, new OpenCvSharp.Size(Global.GaussianKSizeX, Global.GaussianKSizeY), Global.GaussianSigmaX, Global.GaussianSigmaY);

                //    resultMat.CopyTo(thresholdMat);

                //    for (int iT = 0; iT < 255; iT++)
                //    {
                //        // Threshold 구한다.( 0 ~ 255 )
                //        OpenCvSharp.Cv2.Threshold(thresholdMat, tempMat, iT, 255, OpenCvSharp.ThresholdTypes.Binary);
                //        // Threshold 구한 이미지를 가지고 윤각선을 구한다.
                //        OpenCvSharp.Cv2.FindContours(tempMat, out OpenCvSharp.Point[][] contoursArray, out OpenCvSharp.HierarchyIndex[] hierarchyIndexes, OpenCvSharp.RetrievalModes.List, OpenCvSharp.ContourApproximationModes.ApproxNone);
                //        // 원본이미지에 구한 윤각선을 검정색으로 그린다.
                //        OpenCvSharp.Cv2.DrawContours(resultMat, contoursArray, -1, OpenCvSharp.Scalar.Black, 1);
                //    }
                //}

                // 인체 제외 영역 그리기
                //if (this.IsRemoveArea) DrawBodyLine(resultMat);

                // OpenCv 변수를 Bitmatp을 변경
                Bitmap bmpImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(resultMat);
                // Bitmap에 Palette 적용
                //if (this.imageinfo != null && imageinfo.Pixel_Format == 8)
                //{
                //    bitmapimageCtl.SetPalette(imageinfo.selectedPaletteType);

                //    if (this.IsTempAlarm)    // 온도 Alarm용 Color 적용
                //        bmpImage = bitmapimageCtl.SetPaletteBitmapAlarm(bmpImage, this.LowTemperature, this.HighTemperature, this.TempAlarmValue);
                //    else                    // 일반 Color 적용
                //        bmpImage = bitmapimageCtl.SetPaletteBitmap(bmpImage, this.imageinfo.minWindowLevel, this.imageinfo.maxWindowLevel, this.imageinfo.IsWL_BG,
                //                                                         nISOLow: this.imageinfo.iIsoLow, nISOHigh: this.imageinfo.iIsoHigh);
                //}

                //graphics.DrawImage(bmpImage, this.ClientRectangle);
                // ROI, 인체 영역 제외 ROI 그린다.
                using (Bitmap backImage = new Bitmap(bmpImage))
                {
                    using (Graphics g = Graphics.FromImage(backImage))
                    {
                        // 인체 영역의 온도값을 저장하는 함수
                        //if (this.IsRemoveArea)
                        //{
                        //    double picWidth = this.Width;                     // PictureBox Width
                        //    double picHeight = this.Height;                    // PictureBox Height
                        //    double imageWidth = this.ImageInfo.Image_Width;     // PictureBox의 ImageInfo Width
                        //    double imageHeight = this.ImageInfo.Image_Height;    // PictureBox의 ImageInfo Height

                        //    foreach (var roi in this.imageinfo.listRemoveShape)
                        //        roi.Draw(g, backImage.Width, backImage.Height, this.subMatLeft, this.subMatTop, this.Zoom_Ratio, IsFill: true);
                        //}

                        // 저장된 도형 List를 그린다.
                        if (this.IsROIView)
                        {
                            // 이동전 Back 도형 그리기
                            if (this.IsDrawBack && this.backShape != null) this.backShape.Draw(g, backImage.Width, backImage.Height, this.subMatLeft, this.subMatTop, this.Zoom_Ratio);
                            else
                            {
                                // 클론 ROI 그리기
                                if (this.IsDrawClone && this.cloneShape != null) this.cloneShape.Draw(g, backImage.Width, backImage.Height, this.subMatLeft, this.subMatTop, this.Zoom_Ratio, IsRotatedPoint: false);
                            }

                            //  저장된 ROI 그리기
                            foreach (ROIShape cShape in imageinfo.listShape)
                            {
                                //cShape.Font = new Font(cShape.Font.FontFamily, this.imageinfo.ROIFontSize, cShape.Font.Style);                    // ImageInfo에 설정된 Font Size로 설정함
                                cShape.Font = new Font(cShape.Font.FontFamily, Global.Roi_FontSize, cShape.Font.Style);                    // ImageInfo에 설정된 Font Size로 설정함
                                cShape.Draw(g, backImage.Width, backImage.Height, this.subMatLeft, this.subMatTop, this.Zoom_Ratio);
                            }

                            // 그리고 있는 도형 그리기
                            if (this.newShape != null) this.newShape.Draw(g, backImage.Width, backImage.Height, this.subMatLeft, this.subMatTop, this.Zoom_Ratio, IsRotatedPoint: false);
                        }
                        g.Dispose();
                    }

                    graphics.DrawImage(backImage, this.ClientRectangle);
                    backImage.Dispose();
                    bmpImage.Dispose();
                    graphics.Dispose();
                }

                imageMat.Dispose();
                resultMat.Dispose();
            }
        }

        /** @brief: 입력 Mat에 인체 영역 그리기 (minWindowLevel ~ maxWindowLevel) 사이 영역 표시
         *  @section: Description
         *      입력 Mat에 인체 영역 그리기 (minWindowLevel ~ maxWindowLevel) 사이 영역 표시
         *  @param:     orgMat      인체 영역을 그릴 Mat
         */
        private void DrawBodyLine(OpenCvSharp.Mat orgMat)
        {
            if (orgMat != null && !orgMat.Empty())
            {
                //byte[] buff = new byte[orgMat.Width * orgMat.Height];
                OpenCvSharp.Mat temp = orgMat.Clone();
                temp.GetArray(out byte[] buff);

                int width = this.imageinfo.Image_Width;
                int height = this.imageinfo.Image_Height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int bIndex = (x + (y * width));
                        if (bIndex < buff.Length)
                        {
                            if (buff[bIndex] < this.imageinfo.minWindowLevel ||
                                buff[bIndex] > this.imageinfo.maxWindowLevel)
                                buff[bIndex] = 0;
                            else
                                buff[bIndex] = 255;
                        }
                    }
                }

                OpenCvSharp.Mat convertMat = new OpenCvSharp.Mat(height, width, OpenCvSharp.MatType.CV_8U, buff);
                OpenCvSharp.Cv2.FindContours(convertMat, out OpenCvSharp.Point[][] contoursArray, out OpenCvSharp.HierarchyIndex[] hierarchyIndexes, OpenCvSharp.RetrievalModes.List, OpenCvSharp.ContourApproximationModes.ApproxNone);

                // 원본이미지에 구한 윤각선을 검정색으로 그린다.
                OpenCvSharp.Cv2.DrawContours(orgMat, contoursArray, -1, OpenCvSharp.Scalar.White, 2);
            }
        }

        /** @brief: 입력 Mat에서 이미지 자르기 정해진 영역 자르기
         *  @section: Description
         *      입력 Mat에 인체 영역 그리기 (minWindowLevel ~ maxWindowLevel) 사이 영역 표시
         *  @param:     orgMat      이미지 자를 원본 Image Mat
         *  @param:     roiAres     이미지 자를 영역
         *  @return:    자른 이미지를 리턴한다. 자를 이미지가 없으면 null
         */
        public OpenCvSharp.Mat ImageCROP(OpenCvSharp.Mat orgMat, Rectangle roiAres)
        {
            if (orgMat == null) return null;
            if (roiAres.Width == 0 || roiAres.Height == 0) return null;

            int picWidth = this.Width;                // 지금 PictureBox의 넓이
            int picHeight = this.Height;               // 지금 PictureBox의 높이
            int matWidth = orgMat.Width;              // 이미지의 넓이
            int matHeight = orgMat.Height;             // 이미지의 높이

            // roiAres의 값은 PictureBox의 값이다 그래서 이미지 비율에 맞추기 작업이 필요
            float conLeft = (float)roiAres.Left * (float)matWidth / (float)picWidth;
            float conTop = (float)roiAres.Top * (float)matHeight / (float)picHeight;
            float conWidth = (float)roiAres.Width * (float)matWidth / (float)picWidth;
            float conHeight = (float)roiAres.Height * (float)matHeight / (float)picHeight;

            if (conLeft < 0) conLeft = 0; if (conTop < 0) conTop = 0; if (conWidth < 0) conWidth = 0; if (conHeight < 0) conHeight = 0;
            if (conLeft > matWidth) conLeft = matWidth; if (conTop > matHeight) conTop = matHeight;
            if (conLeft + conWidth > matWidth) conWidth = matWidth - conLeft;
            if (conTop + conHeight > matHeight) conHeight = matHeight - conTop;

            // 이미지 비율에 맞춘 ROI 영역이다.
            OpenCvSharp.Rect conRect = new OpenCvSharp.Rect((int)conLeft, (int)conTop, (int)conWidth, (int)conHeight);
            // 넓이, 높이가 0보다 작으면 처리 안한다.
            if (conRect.Width <= 0 || conRect.Height <= 0) return null;

            // 이미지에서 ROI 영역을 가지고 온다.
            OpenCvSharp.Mat mat = orgMat.SubMat(conRect).Clone();

            double viewWidth = matHeight * conRect.Width / conRect.Height;
            double viewHeight = matWidth * conRect.Height / conRect.Width;

            if (viewWidth < matWidth) viewHeight = matHeight;
            else if (viewHeight < matHeight) viewWidth = matWidth;

            // 이미지을 비율에 맞추어 키운다.
            OpenCvSharp.Mat resizeMat = mat.Resize(new OpenCvSharp.Size(viewWidth, viewHeight), 0, 0, OpenCvSharp.InterpolationFlags.Cubic);

            // left, top, right, bottom gap을 구한다.
            int GapLeft = (matWidth - (int)viewWidth) / 2;
            int GapTop = (matHeight - (int)viewHeight) / 2;
            int GapRight = matWidth - (GapLeft + (int)viewWidth);
            int GapBottom = matHeight - (GapTop + (int)viewHeight);

            // 이미지에 구한 gap만큼 테두리를 붙인다.
            OpenCvSharp.Mat resultMat = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.CopyMakeBorder(resizeMat, resultMat, GapTop, GapBottom, GapLeft, GapRight, OpenCvSharp.BorderTypes.Constant, OpenCvSharp.Scalar.Black);

            return resultMat;
        }

        /** @brief: Bring Line 그리기
         *  @section: Description
         *      Bring Line 그리기
         *  @param:     SP      Bring Line 시작 점
         *  @param:     EP      Bring Line 끝 점         
         */
        private void DrawBringLine(Graphics g, Point SP, Point EP)
        {
            Point backSP = SP, backEP = EP;
            backSP.Offset(-1, -1); backEP.Offset(-1, -1);
            int object_radius = 3;

            g.DrawLine(Pens.Black, backSP, backEP);

            g.FillEllipse(Brushes.Black, new Rectangle(backSP.X - object_radius, backSP.Y - object_radius,
                                                            2 * object_radius + 1, 2 * object_radius + 1));
            g.FillEllipse(Brushes.Black, new Rectangle(backEP.X - object_radius, backEP.Y - object_radius,
                                                            2 * object_radius + 1, 2 * object_radius + 1));

            g.DrawLine(Pens.White, SP, EP);
            g.FillEllipse(Brushes.White, new Rectangle(SP.X - object_radius, SP.Y - object_radius,
                                                            2 * object_radius + 1, 2 * object_radius + 1));
            g.FillEllipse(Brushes.White, new Rectangle(EP.X - object_radius, EP.Y - object_radius,
                                                            2 * object_radius + 1, 2 * object_radius + 1));

            DrawString(g, "SP", SP, EP);
            DrawString(g, "EP", EP, SP);

        }

        /** @brief: Bring Line에서 Text 그리기
         *  @section: Description
         *      Bring Line에서 Text 그리기
         *  @param:     text    표시할 Text
         *  @param:     tp      Bring Line에서 Text 그리기 시작 점
         *  @param:     rp      Bring Line에서 Text 그리기 끝 점         
         */
        private void DrawString(Graphics g, string text, Point tp, Point rp)
        {
            // tp와 rp이 각도 구하기
            ImageUsedMath.AngleBetween(tp, rp, out float realAngle);

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

            Rectangle AngleTextRect = new Rectangle(tp.X + 1, tp.Y + 1, (int)(this.Font.SizeInPoints * text.Length * 2), this.Font.Height);

            if (IsRectUp) AngleTextRect.Offset(0, (-1 * AngleTextRect.Height));
            if (IsRectLeft) { AngleTextRect.Offset((-1 * AngleTextRect.Width), 0); strIndexFormat.Alignment = StringAlignment.Far; }

            Rectangle AngleTextBackRect = AngleTextRect;
            AngleTextBackRect.Offset(-1, -1);
            g.DrawString(text, this.Font, Brushes.Black, AngleTextBackRect, strIndexFormat);   // Back 글자

            g.DrawString(text, this.Font, Brushes.White, AngleTextRect, strIndexFormat);          // 원래 글자 
        }
    #endregion Picture Box를 그린다.
    }
}
