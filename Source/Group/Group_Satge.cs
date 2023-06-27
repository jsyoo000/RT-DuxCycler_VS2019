using Duxcycler;
using Duxcycler_GLOBAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Duxcycler_Group
{
	/// <summary>
	/// Summary description for CollapsibleGroupBox.
	/// </summary>
	public class Group_Stage : System.Windows.Forms.UserControl
	{
		public const int kCollapsedHeight = 20;
		#region Members
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		//private ImageButton						m_TrashIcon;
		//public event CollapseBoxClickedEventHandler	CollapseBoxClickedEvent;
  //      public event Group_Stage_EnterEventHandler Group_Stage_EnterEvent;
        //public event TrashCanClickedEventHandler	TrashCanClickedEvent;

        private string							m_Caption;
		//private bool							m_bContainsTrashCan;
		//private System.Windows.Forms.GroupBox	m_GroupBox;
		private Size							m_FullSize;
        private bool							m_bResizingFromCollapse = false;

        private bool m_bMouseEnter = false;
        private int StepCount = 1;
        #endregion Members

        public MainPage group_Parent;
        public int displayHeight;

        //public eGROUP_TYPE groupType = 0;   // 0:Hold Stage, 1:PCR Stage
        private Label label_StageTitle;
        public NumericUpDown NumericUpDown_CycleCount;
        private CustomClassLibrary.CustomButton btn_Remove;
        private CustomClassLibrary.CustomButton btn_AddLeft;
        private CustomClassLibrary.CustomButton btn_AddRight;
        private CustomClassLibrary.CustomButton btn_AddRight2;
        private CustomClassLibrary.CustomButton btn_AddLeft2;
        private CustomClassLibrary.CustomButton btn_Remove2;
        private CustomClassLibrary.DoubleBufferPanel doubleBufferPanel_Backgound;
        public List<Group_Step> stepList = new List<Group_Step>();

        public string StageName { get; set; }
        public int StageId { get; set; }
        public int StageCycleCount { get; set; }
        public eGROUP_TYPE StageType { get; set; }
        public bool isNewStage { get; set; }

        public Group_Stage(MainPage parent = null, eGROUP_TYPE gType = eGROUP_TYPE.PCR_STAGE, bool isNew = true)
		{
            group_Parent = parent;
            StageType = gType;
            StageCycleCount = 40;
            isNewStage = isNew;

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            if (StageType == eGROUP_TYPE.HOLD_STAGE)
            {
                this.NumericUpDown_CycleCount.Value = 1;
                this.NumericUpDown_CycleCount.Visible = false;
            }
            else
                this.NumericUpDown_CycleCount.Value = StageCycleCount;

            displayHeight = this.group_Parent.doubleBufferPanel_Method.Height;
            this.Height = displayHeight - 30;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.label_StageTitle = new System.Windows.Forms.Label();
            this.NumericUpDown_CycleCount = new System.Windows.Forms.NumericUpDown();
            this.btn_Remove = new CustomClassLibrary.CustomButton();
            this.btn_AddLeft = new CustomClassLibrary.CustomButton();
            this.btn_AddRight = new CustomClassLibrary.CustomButton();
            this.btn_AddRight2 = new CustomClassLibrary.CustomButton();
            this.btn_AddLeft2 = new CustomClassLibrary.CustomButton();
            this.btn_Remove2 = new CustomClassLibrary.CustomButton();
            this.doubleBufferPanel_Backgound = new CustomClassLibrary.DoubleBufferPanel();
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDown_CycleCount)).BeginInit();
            this.doubleBufferPanel_Backgound.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_StageTitle
            // 
            this.label_StageTitle.BackColor = System.Drawing.Color.Transparent;
            this.label_StageTitle.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_StageTitle.ForeColor = System.Drawing.Color.White;
            this.label_StageTitle.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_StageTitle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label_StageTitle.Location = new System.Drawing.Point(36, 12);
            this.label_StageTitle.Name = "label_StageTitle";
            this.label_StageTitle.Size = new System.Drawing.Size(141, 22);
            this.label_StageTitle.TabIndex = 131;
            this.label_StageTitle.Text = "PCR Stage";
            this.label_StageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // NumericUpDown_CycleCount
            // 
            this.NumericUpDown_CycleCount.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.NumericUpDown_CycleCount.Location = new System.Drawing.Point(80, 391);
            this.NumericUpDown_CycleCount.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.NumericUpDown_CycleCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericUpDown_CycleCount.Name = "NumericUpDown_CycleCount";
            this.NumericUpDown_CycleCount.Size = new System.Drawing.Size(48, 23);
            this.NumericUpDown_CycleCount.TabIndex = 152;
            this.NumericUpDown_CycleCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NumericUpDown_CycleCount.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.NumericUpDown_CycleCount.ValueChanged += new System.EventHandler(this.NumericUpDown_CycleCount_ValueChanged);
            // 
            // btn_Remove
            // 
            this.btn_Remove.BackColor = System.Drawing.Color.Transparent;
            this.btn_Remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Remove.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.btn_Remove.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.btn_Remove.DisableImage = null;
            this.btn_Remove.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.btn_Remove.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.btn_Remove.DownImage = null;
            this.btn_Remove.FlatAppearance.BorderSize = 0;
            this.btn_Remove.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_Remove.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_Remove.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Remove.ForeColor = System.Drawing.Color.White;
            this.btn_Remove.HoverBackgroundImage = null;
            this.btn_Remove.HoverFontColor = System.Drawing.Color.LightGray;
            this.btn_Remove.HoverImage = null;
            this.btn_Remove.IsSelected = false;
            this.btn_Remove.Location = new System.Drawing.Point(87, 42);
            this.btn_Remove.Name = "btn_Remove";
            this.btn_Remove.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_Remove.NormalFontColor = System.Drawing.Color.White;
            this.btn_Remove.NormalImage = null;
            this.btn_Remove.Size = new System.Drawing.Size(34, 34);
            this.btn_Remove.TabIndex = 4;
            this.btn_Remove.Text = "-";
            this.btn_Remove.UseVisualStyleBackColor = false;
            this.btn_Remove.Click += new System.EventHandler(this.StageRemove);
            // 
            // btn_AddLeft
            // 
            this.btn_AddLeft.BackColor = System.Drawing.Color.Transparent;
            this.btn_AddLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_AddLeft.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.btn_AddLeft.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.btn_AddLeft.DisableImage = null;
            this.btn_AddLeft.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.btn_AddLeft.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.btn_AddLeft.DownImage = null;
            this.btn_AddLeft.FlatAppearance.BorderSize = 0;
            this.btn_AddLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_AddLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_AddLeft.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btn_AddLeft.ForeColor = System.Drawing.Color.White;
            this.btn_AddLeft.HoverBackgroundImage = null;
            this.btn_AddLeft.HoverFontColor = System.Drawing.Color.LightGray;
            this.btn_AddLeft.HoverImage = null;
            this.btn_AddLeft.IsSelected = false;
            this.btn_AddLeft.Location = new System.Drawing.Point(19, 42);
            this.btn_AddLeft.Name = "btn_AddLeft";
            this.btn_AddLeft.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_AddLeft.NormalFontColor = System.Drawing.Color.White;
            this.btn_AddLeft.NormalImage = null;
            this.btn_AddLeft.Size = new System.Drawing.Size(34, 34);
            this.btn_AddLeft.TabIndex = 6;
            this.btn_AddLeft.Text = "+";
            this.btn_AddLeft.UseVisualStyleBackColor = false;
            this.btn_AddLeft.Click += new System.EventHandler(this.StageInsertLeft);
            // 
            // btn_AddRight
            // 
            this.btn_AddRight.BackColor = System.Drawing.Color.Transparent;
            this.btn_AddRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_AddRight.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.btn_AddRight.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.btn_AddRight.DisableImage = null;
            this.btn_AddRight.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.btn_AddRight.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.btn_AddRight.DownImage = null;
            this.btn_AddRight.FlatAppearance.BorderSize = 0;
            this.btn_AddRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_AddRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_AddRight.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btn_AddRight.ForeColor = System.Drawing.Color.White;
            this.btn_AddRight.HoverBackgroundImage = null;
            this.btn_AddRight.HoverFontColor = System.Drawing.Color.LightGray;
            this.btn_AddRight.HoverImage = null;
            this.btn_AddRight.IsSelected = false;
            this.btn_AddRight.Location = new System.Drawing.Point(156, 42);
            this.btn_AddRight.Name = "btn_AddRight";
            this.btn_AddRight.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_AddRight.NormalFontColor = System.Drawing.Color.White;
            this.btn_AddRight.NormalImage = null;
            this.btn_AddRight.Size = new System.Drawing.Size(34, 34);
            this.btn_AddRight.TabIndex = 5;
            this.btn_AddRight.Text = "+";
            this.btn_AddRight.UseVisualStyleBackColor = false;
            this.btn_AddRight.Click += new System.EventHandler(this.StageInsertRight);
            // 
            // btn_AddRight2
            // 
            this.btn_AddRight2.BackColor = System.Drawing.Color.Transparent;
            this.btn_AddRight2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_AddRight2.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.btn_AddRight2.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.btn_AddRight2.DisableImage = null;
            this.btn_AddRight2.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.btn_AddRight2.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.btn_AddRight2.DownImage = null;
            this.btn_AddRight2.FlatAppearance.BorderSize = 0;
            this.btn_AddRight2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_AddRight2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_AddRight2.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btn_AddRight2.ForeColor = System.Drawing.Color.White;
            this.btn_AddRight2.HoverBackgroundImage = null;
            this.btn_AddRight2.HoverFontColor = System.Drawing.Color.LightGray;
            this.btn_AddRight2.HoverImage = null;
            this.btn_AddRight2.IsSelected = false;
            this.btn_AddRight2.Location = new System.Drawing.Point(156, 349);
            this.btn_AddRight2.Name = "btn_AddRight2";
            this.btn_AddRight2.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_AddRight2.NormalFontColor = System.Drawing.Color.White;
            this.btn_AddRight2.NormalImage = null;
            this.btn_AddRight2.Size = new System.Drawing.Size(34, 34);
            this.btn_AddRight2.TabIndex = 5;
            this.btn_AddRight2.Text = "+";
            this.btn_AddRight2.UseVisualStyleBackColor = false;
            this.btn_AddRight2.Click += new System.EventHandler(this.StageInsertRight);
            // 
            // btn_AddLeft2
            // 
            this.btn_AddLeft2.BackColor = System.Drawing.Color.Transparent;
            this.btn_AddLeft2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_AddLeft2.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.btn_AddLeft2.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.btn_AddLeft2.DisableImage = null;
            this.btn_AddLeft2.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.btn_AddLeft2.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.btn_AddLeft2.DownImage = null;
            this.btn_AddLeft2.FlatAppearance.BorderSize = 0;
            this.btn_AddLeft2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_AddLeft2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_AddLeft2.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btn_AddLeft2.ForeColor = System.Drawing.Color.White;
            this.btn_AddLeft2.HoverBackgroundImage = null;
            this.btn_AddLeft2.HoverFontColor = System.Drawing.Color.LightGray;
            this.btn_AddLeft2.HoverImage = null;
            this.btn_AddLeft2.IsSelected = false;
            this.btn_AddLeft2.Location = new System.Drawing.Point(19, 349);
            this.btn_AddLeft2.Name = "btn_AddLeft2";
            this.btn_AddLeft2.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_AddLeft2.NormalFontColor = System.Drawing.Color.White;
            this.btn_AddLeft2.NormalImage = null;
            this.btn_AddLeft2.Size = new System.Drawing.Size(34, 34);
            this.btn_AddLeft2.TabIndex = 6;
            this.btn_AddLeft2.Text = "+";
            this.btn_AddLeft2.UseVisualStyleBackColor = false;
            this.btn_AddLeft2.Click += new System.EventHandler(this.StageInsertLeft);
            // 
            // btn_Remove2
            // 
            this.btn_Remove2.BackColor = System.Drawing.Color.Transparent;
            this.btn_Remove2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn_Remove2.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.btn_Remove2.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.btn_Remove2.DisableImage = null;
            this.btn_Remove2.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.btn_Remove2.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.btn_Remove2.DownImage = null;
            this.btn_Remove2.FlatAppearance.BorderSize = 0;
            this.btn_Remove2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btn_Remove2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btn_Remove2.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Remove2.ForeColor = System.Drawing.Color.White;
            this.btn_Remove2.HoverBackgroundImage = null;
            this.btn_Remove2.HoverFontColor = System.Drawing.Color.LightGray;
            this.btn_Remove2.HoverImage = null;
            this.btn_Remove2.IsSelected = false;
            this.btn_Remove2.Location = new System.Drawing.Point(87, 349);
            this.btn_Remove2.Name = "btn_Remove2";
            this.btn_Remove2.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_Remove2.NormalFontColor = System.Drawing.Color.White;
            this.btn_Remove2.NormalImage = null;
            this.btn_Remove2.Size = new System.Drawing.Size(34, 34);
            this.btn_Remove2.TabIndex = 4;
            this.btn_Remove2.Text = "-";
            this.btn_Remove2.UseVisualStyleBackColor = false;
            this.btn_Remove2.Click += new System.EventHandler(this.StageRemove);
            // 
            // doubleBufferPanel_Backgound
            // 
            this.doubleBufferPanel_Backgound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(57)))), ((int)(((byte)(72)))));
            this.doubleBufferPanel_Backgound.Controls.Add(this.btn_Remove2);
            this.doubleBufferPanel_Backgound.Controls.Add(this.label_StageTitle);
            this.doubleBufferPanel_Backgound.Controls.Add(this.btn_AddLeft2);
            this.doubleBufferPanel_Backgound.Controls.Add(this.NumericUpDown_CycleCount);
            this.doubleBufferPanel_Backgound.Controls.Add(this.btn_AddRight2);
            this.doubleBufferPanel_Backgound.Controls.Add(this.btn_Remove);
            this.doubleBufferPanel_Backgound.Controls.Add(this.btn_AddRight);
            this.doubleBufferPanel_Backgound.Controls.Add(this.btn_AddLeft);
            this.doubleBufferPanel_Backgound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.doubleBufferPanel_Backgound.Location = new System.Drawing.Point(0, 0);
            this.doubleBufferPanel_Backgound.Name = "doubleBufferPanel_Backgound";
            this.doubleBufferPanel_Backgound.Size = new System.Drawing.Size(209, 423);
            this.doubleBufferPanel_Backgound.TabIndex = 3;
            this.doubleBufferPanel_Backgound.Paint += new System.Windows.Forms.PaintEventHandler(this.doubleBufferPanel_Backgound_Paint);
            this.doubleBufferPanel_Backgound.MouseEnter += new System.EventHandler(this.doubleBufferPanel_Backgound_MouseEnter);
            this.doubleBufferPanel_Backgound.MouseLeave += new System.EventHandler(this.doubleBufferPanel_Backgound_MouseLeave);
            this.doubleBufferPanel_Backgound.MouseMove += new System.Windows.Forms.MouseEventHandler(this.doubleBufferPanel_Backgound_MouseMove);
            // 
            // Group_Stage
            // 
            this.Controls.Add(this.doubleBufferPanel_Backgound);
            this.Name = "Group_Stage";
            this.Size = new System.Drawing.Size(209, 423);
            this.Load += new System.EventHandler(this.Group_Stage_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Group_Stage_Paint);
            this.Resize += new System.EventHandler(this.Group_Stage_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDown_CycleCount)).EndInit();
            this.doubleBufferPanel_Backgound.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		#region Events
		private void Group_Stage_Load(object sender, System.EventArgs e)
		{
            //btn_AddLeft.Hide();
            //btn_Remove.Hide();
            //btn_AddRight.Hide();

            // 스탭 높이를 설정한다. 
            //this.Height = displayHeight - 30;

            // 스테이지 초기 생성시 Hold Stage 와 PCR Stage를 하나씩 자동으로 생성한다. 
            if (isNewStage)
            {
                Group_Step newStep1, newStep2;
                if (StageType == eGROUP_TYPE.HOLD_STAGE)
                {
                    newStep1 = new Group_Step(this, Global.default_HoldStepTemp1, Global.default_HoldStepTime1);
                    newStep2 = new Group_Step(this, Global.default_HoldStepTemp2, Global.default_HoldStepTime2);
                }
                else
                {
                    newStep1 = new Group_Step(this, Global.default_PcrStepTemp1, Global.default_PcrStepTime1);
                    newStep2 = new Group_Step(this, Global.default_PcrStepTemp2, Global.default_PcrStepTime2);
                }

                this.stepList.Add(newStep1);
                this.stepList.Add(newStep2);

                //AddStep(newStep);
                StepSort();
            }
        }

		private void Group_Stage_Resize(object sender, System.EventArgs e)
		{
            //this.Height = displayHeight;
            //doubleBufferPanel_Backgound.Height = this.Height - 5;
            //if(m_bResizingFromCollapse != true)
            //{
            //	m_FullSize = this.Size;
            //}

            //Invalidate();
        }

		private void Group_Stage_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
            this.label_StageTitle.Text = this.StageName;
        }
        #endregion events

        #region Accessors
        [DefaultValue("")]
		public string Caption
		{
			get
			{
				return m_Caption;
			}
			set
			{
				m_Caption = value;
				SetGroupBoxCaption();
				Invalidate();
			}
		}

		[DefaultValue(true)]
		public bool ContainsTrashCan
		{
			get
			{
				//return m_TrashIcon.Visible;
				return false;
			}
			set
			{
				//m_bContainsTrashCan = value;
				//m_TrashIcon.Visible = value;
				SetGroupBoxCaption();
				Invalidate();
			}
		}

		[Browsable(false)]
		public int FullHeight
		{
			get
			{
				return m_FullSize.Height;
			}
		}

		[DefaultValue(false), Browsable(false)]
		public bool IsCollapsed
		{
			get
			{
				//#if DEBUG
				//if(m_CollapseBox.IsPlus)
				//{
				//	Debug.Assert(this.Height == kCollapsedHeight);
				//}
				//else
				//{
				//	Debug.Assert(this.Height > kCollapsedHeight);
				//}
				//#endif
				//return m_CollapseBox.IsPlus;
				return false;
			}
			set
			{
				//if(m_CollapseBox.IsPlus != value)
				//{
				//	m_CollapseBox.IsPlus = value;
				//}

				//if(m_CollapseBox.IsPlus != true)
				//{
				//	//Expand();
				//	this.Size = m_FullSize;
				//}
				//else
				//{
				//	//Collapse();
				//	m_bResizingFromCollapse = true;
				//	Size smallSize = m_FullSize;
				//	smallSize.Height = kCollapsedHeight;
				//	this.Size = smallSize;
				//	m_bResizingFromCollapse = false;
				//}

				//Invalidate();
			}
		}
		#endregion accessors

		#region Methods
		private void SetGroupBoxCaption()
		{
			//RepositionTrashCan();
		}

		//private void RepositionTrashCan()
		//{
		//	if(m_TrashIcon.Visible)
		//	{
		//		// Since the trash icon's location is a function of the caption's width,
		//		// we also need to reposition the trash icon
		//		// first, find the width of the string
		//		Graphics g = CreateGraphics();
		//		SizeF theTextSize = new SizeF();
		//		theTextSize	= g.MeasureString(m_Caption, this.Font);
		//		// Hmm... MeasureString() doesn't seem to be returning the
		//		// correct width.  Close... but not exact

		//		// 11 is the number of pixels from the beginning of the group box
		//		// to the beginning of text of the group box's caption
		//		//m_TrashIcon.Left = m_GroupBox.Location.X + 29 + (int)theTextSize.Width - 4;
		//		m_TrashIcon.Left = this.Location.X + 29 + (int)theTextSize.Width - 4;
		//		// -4 is a fudge factor.  Hey, what can I say...
		//	} 
		//}
		#endregion Methods

        /// <summary>
        /// Method에 Step을 추가한다. 
        /// </summary>
        /// <param name="theStep">추가할 Step</param>
        public void AddStep(Group_Step theStep)
        {
            this.SuspendLayout();

            int StepPosY = btn_AddLeft.Location.Y + (btn_AddLeft.Size.Height / 2);
            this.ResumeLayout(false);
        }

        /// <summary>
        /// 현재 Step 왼쪽에 새로운 Step을 추가한다. 
        /// </summary>
        /// <param name="stepID">현재 Step ID</param>
        public void StepInsertLeft(int stepID)
        {
            this.group_Parent.methodEdit = true;

            int insertID = 0;
            if (stepID == 0)
                insertID = 0;
            else
                insertID = stepID - 1;

            Group_Step newStep;
            if (StageType == eGROUP_TYPE.HOLD_STAGE)
            {
                newStep = new Group_Step(this, Global.default_HoldStepTemp1, Global.default_HoldStepTime1);
            }
            else
            {
                newStep = new Group_Step(this, Global.default_PcrStepTemp1, Global.default_PcrStepTime1);
            }
            this.stepList.Insert(insertID, newStep);

            StepSort();
        }

        /// <summary>
        /// 현재 Step 오른쪽에 새로운 Step을 추가한다.
        /// </summary>
        /// <param name="stepID">현재 Step ID</param>
        public void StepInsertRight(int stepID)
        {
            this.group_Parent.methodEdit = true;

            int insertID = 0;
            insertID = stepID + 1;

            Group_Step newStep;
            if (StageType == eGROUP_TYPE.HOLD_STAGE)
            {
                newStep = new Group_Step(this, Global.default_HoldStepTemp1, Global.default_HoldStepTime1);
            }
            else
            {
                newStep = new Group_Step(this, Global.default_PcrStepTemp1, Global.default_PcrStepTime1);
            }
            if (this.stepList.Count < insertID)
                this.stepList.Add(newStep);
            else 
                this.stepList.Insert(insertID, newStep);

            StepSort();
        }

        /// <summary>
        /// 해당 Step 을 삭제한다. 
        /// </summary>
        /// <param name="stepID">삭제할 Step ID</param>
        public void StepRemove(int stepID)
        {
            this.group_Parent.methodEdit = true;

            if (this.stepList.Count > 1)
            {
                this.stepList.RemoveAt(stepID - 1);
                StepSort();
            }
        }

        /// <summary>
        /// Step 리스트를 재정렬한다.  
        /// </summary>
        public void StepSort()
        {
            this.doubleBufferPanel_Backgound.Controls.Clear();
            doubleBufferPanel_Backgound.SuspendLayout();

            int backgroundSize = 100;
            int StepPosY = btn_AddLeft.Location.Y + (btn_AddLeft.Size.Height / 2);
            this.Width = ((this.stepList[0].Width + 5) * this.stepList.Count) + 5;
            //doubleBufferPanel_Backgound.Width = ((this.stepList[0].Width + 5) * this.stepList.Count) + 10;

            // 버튼 위치를 제조정한다. 
            int posX = (this.Width / 2) - (NumericUpDown_CycleCount.Width / 2);
            int posY = this.Height - NumericUpDown_CycleCount.Height - 10;
            NumericUpDown_CycleCount.Location = new Point(posX, posY);

            posX = (this.Width / 2) - (label_StageTitle.Width / 2);
            posY = 10;
            label_StageTitle.Location = new Point(posX, posY);

            posX = btn_AddLeft.Location.X;
            posY = btn_AddLeft.Location.Y;
            btn_AddLeft.Location = new Point(posX, posY);
            posY = NumericUpDown_CycleCount.Location.Y - btn_AddLeft2.Height - 10;
            btn_AddLeft2.Location = new Point(posX, posY);

            posX = (this.Width / 2) - (btn_Remove2.Width / 2);
            posY = btn_Remove.Location.Y;
            btn_Remove.Location = new Point(posX, posY);
            posY = NumericUpDown_CycleCount.Location.Y - btn_Remove2.Height - 10;
            btn_Remove2.Location = new Point(posX, posY);

            posX = this.Width - btn_AddRight2.Width - btn_AddLeft2.Location.X;
            posY = btn_AddRight.Location.Y;
            btn_AddRight.Location = new Point(posX, posY);
            posY = NumericUpDown_CycleCount.Location.Y - btn_AddRight2.Height - 10;
            btn_AddRight2.Location = new Point(posX, posY);

            doubleBufferPanel_Backgound.Controls.Add(btn_AddLeft);
            doubleBufferPanel_Backgound.Controls.Add(btn_AddLeft2);
            doubleBufferPanel_Backgound.Controls.Add(btn_Remove);
            doubleBufferPanel_Backgound.Controls.Add(btn_Remove2);
            doubleBufferPanel_Backgound.Controls.Add(btn_AddRight);
            doubleBufferPanel_Backgound.Controls.Add(btn_AddRight2);
            doubleBufferPanel_Backgound.Controls.Add(NumericUpDown_CycleCount);
            doubleBufferPanel_Backgound.Controls.Add(label_StageTitle);
            
            int locationX = 5;
            int locationY = StepPosY;
            for (int i = 0; i < this.stepList.Count; i++)
            {
                Group_Step newStep = this.stepList[i];
                newStep.Location = new Point(locationX, locationY);
                //this.stepList[i].Height = (int)(this.displayHeight - (StepPosY * 2));
                newStep.Height = (int)((btn_AddLeft2.Location.Y + (btn_AddLeft2.Size.Height / 2) - this.stepList[i].Location.Y));

                newStep.StepId = i + 1;
                newStep.Name = "Step_" + newStep.StepId.ToString();
                this.stepList[i] = newStep;
                doubleBufferPanel_Backgound.Controls.Add(this.stepList[i]);

                locationX += this.stepList[i].Width + 5;
                //backgroundSize += this.stepList[i].Width + 5;

                this.stepList[i].Invalidate();
            }

            doubleBufferPanel_Backgound.ResumeLayout();

            this.group_Parent.StageSort();
        }

        /// <summary>
        /// 오른쪽 Stage Insert 메뉴 실행 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StageInsertRight(object sender, EventArgs e)
        {
            this.group_Parent.StageInsertRight(this.StageId);
        }

        /// <summary>
        /// 왼쪽 Stage Insert 메뉴 실행 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StageInsertLeft(object sender, EventArgs e)
        {
            this.group_Parent.StageInsertLeft(this.StageId);
        }

        /// <summary>
        /// Stage Delete 메뉴 실행 이벤트 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StageRemove(object sender, EventArgs e)
        {
            this.group_Parent.StageRemove(this.StageId);
        }

        private void doubleBufferPanel_Backgound_Paint(object sender, PaintEventArgs e)
        {
            if (m_bMouseEnter)
            {
                Graphics g = this.doubleBufferPanel_Backgound.CreateGraphics();
                Pen p = new Pen(Color.Blue, 1);

                Rectangle rec = new Rectangle(1, 1, this.Width - 1, this.Height - 1);
                g.DrawRectangle(p, rec);
            }
        }

        private void doubleBufferPanel_Backgound_MouseEnter(object sender, EventArgs e)
        {
            m_bMouseEnter = true;
            doubleBufferPanel_Backgound.Invalidate();
            //this.group_Parent.StepsInvalidate();
        }

        private void doubleBufferPanel_Backgound_MouseLeave(object sender, EventArgs e)
        {
            m_bMouseEnter = false;
            doubleBufferPanel_Backgound.Invalidate();
        }

        private void doubleBufferPanel_Backgound_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseEnter)
            {
                Graphics g = this.doubleBufferPanel_Backgound.CreateGraphics();
                Pen p = new Pen(Color.Gold, 2);

                Rectangle rec = new Rectangle(2, 2, this.Width-4, this.Height-4);
                g.DrawRectangle(p, rec);
            }
        }

        private void NumericUpDown_CycleCount_ValueChanged(object sender, EventArgs e)
        {
            group_Parent.methodEdit = true;

            StageCycleCount = (int)NumericUpDown_CycleCount.Value;
        }
    }
}
