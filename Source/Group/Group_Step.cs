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
	public class Group_Step : System.Windows.Forms.UserControl
	{
		public const int kCollapsedHeight = 20;
		#region Members
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		//private ImageButton						m_TrashIcon;
		//public event TrashCanClickedEventHandler	TrashCanClickedEvent;

		private string							m_Caption;
		//private bool							m_bContainsTrashCan;
		//private System.Windows.Forms.GroupBox	m_GroupBox;
		private Size							m_FullSize;
        public CustomClassLibrary.ResizePanel resizePanel_Step;
        private CustomClassLibrary.CustomButton btn_AddLeft;
        private CustomClassLibrary.CustomButton btn_Remove;
        private CustomClassLibrary.CustomButton btn_AddRight;
        private bool							m_bResizingFromCollapse = false;
		#endregion Members

		public string StepName { get; set; }
		public int StepId { get; set; }
		public bool IsCapture { get; set; }
		public double StepTemp { get; set; }
		public double StepTempPreview { get; set; }
		public double StepTempNext { get; set; }
		public TimeSpan StepTime { get; set; }

		private Group_Stage group_Parent;
        private DateTimePicker dateTimePicker_StepTime;
        private NumericUpDown NumericUpDown_StepTemp;
        private CustomClassLibrary.CustomButton customButton_stepCapture;
        public bool m_bMouseEnter = false;

		public Group_Step(Group_Stage parent, double fTemp, TimeSpan captureTime, bool isCapture = false)
		{
			group_Parent = parent;
			StepTemp = fTemp;
			StepTime = captureTime;
			this.IsCapture = isCapture;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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
            this.resizePanel_Step = new CustomClassLibrary.ResizePanel();
            this.dateTimePicker_StepTime = new System.Windows.Forms.DateTimePicker();
            this.NumericUpDown_StepTemp = new System.Windows.Forms.NumericUpDown();
            this.customButton_stepCapture = new CustomClassLibrary.CustomButton();
            this.btn_Remove = new CustomClassLibrary.CustomButton();
            this.btn_AddRight = new CustomClassLibrary.CustomButton();
            this.btn_AddLeft = new CustomClassLibrary.CustomButton();
            this.resizePanel_Step.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDown_StepTemp)).BeginInit();
            this.SuspendLayout();
            // 
            // resizePanel_Step
            // 
            this.resizePanel_Step.BackColor = System.Drawing.Color.White;
            this.resizePanel_Step.Controls.Add(this.dateTimePicker_StepTime);
            this.resizePanel_Step.Controls.Add(this.NumericUpDown_StepTemp);
            this.resizePanel_Step.Controls.Add(this.customButton_stepCapture);
            this.resizePanel_Step.Controls.Add(this.btn_Remove);
            this.resizePanel_Step.Controls.Add(this.btn_AddRight);
            this.resizePanel_Step.Controls.Add(this.btn_AddLeft);
            this.resizePanel_Step.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resizePanel_Step.Location = new System.Drawing.Point(0, 0);
            this.resizePanel_Step.Name = "resizePanel_Step";
            this.resizePanel_Step.Size = new System.Drawing.Size(208, 306);
            this.resizePanel_Step.TabIndex = 2;
            this.resizePanel_Step.Paint += new System.Windows.Forms.PaintEventHandler(this.resizePanel_Step_Paint);
            this.resizePanel_Step.MouseEnter += new System.EventHandler(this.resizePanel_Step_MouseEnter);
            this.resizePanel_Step.MouseLeave += new System.EventHandler(this.resizePanel_Step_MouseLeave);
            this.resizePanel_Step.MouseMove += new System.Windows.Forms.MouseEventHandler(this.resizePanel_Step_MouseMove);
            // 
            // dateTimePicker_StepTime
            // 
            this.dateTimePicker_StepTime.CustomFormat = "HH:mm:ss";
            this.dateTimePicker_StepTime.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.dateTimePicker_StepTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_StepTime.Location = new System.Drawing.Point(69, 85);
            this.dateTimePicker_StepTime.Name = "dateTimePicker_StepTime";
            this.dateTimePicker_StepTime.ShowUpDown = true;
            this.dateTimePicker_StepTime.Size = new System.Drawing.Size(80, 23);
            this.dateTimePicker_StepTime.TabIndex = 154;
            this.dateTimePicker_StepTime.Value = new System.DateTime(2022, 12, 18, 2, 24, 0, 0);
            this.dateTimePicker_StepTime.ValueChanged += new System.EventHandler(this.dateTimePicker_StepTime_ValueChanged);
            // 
            // NumericUpDown_StepTemp
            // 
            this.NumericUpDown_StepTemp.DecimalPlaces = 1;
            this.NumericUpDown_StepTemp.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.NumericUpDown_StepTemp.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.NumericUpDown_StepTemp.Location = new System.Drawing.Point(86, 29);
            this.NumericUpDown_StepTemp.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            65536});
            this.NumericUpDown_StepTemp.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.NumericUpDown_StepTemp.Name = "NumericUpDown_StepTemp";
            this.NumericUpDown_StepTemp.Size = new System.Drawing.Size(48, 23);
            this.NumericUpDown_StepTemp.TabIndex = 153;
            this.NumericUpDown_StepTemp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NumericUpDown_StepTemp.Value = new decimal(new int[] {
            95,
            0,
            0,
            0});
            this.NumericUpDown_StepTemp.ValueChanged += new System.EventHandler(this.NumericUpDown_CycleCount_ValueChanged);
            // 
            // customButton_stepCapture
            // 
            this.customButton_stepCapture.BackColor = System.Drawing.Color.Transparent;
            this.customButton_stepCapture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.customButton_stepCapture.DisableBackgroundImage = global::Duxcycler.Properties.Resources.Capture_Disable;
            this.customButton_stepCapture.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.customButton_stepCapture.DisableImage = null;
            this.customButton_stepCapture.DownBackgroundImage = global::Duxcycler.Properties.Resources.Capture_MDown;
            this.customButton_stepCapture.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.customButton_stepCapture.DownImage = null;
            this.customButton_stepCapture.FlatAppearance.BorderSize = 0;
            this.customButton_stepCapture.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.customButton_stepCapture.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.customButton_stepCapture.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.customButton_stepCapture.ForeColor = System.Drawing.Color.White;
            this.customButton_stepCapture.HoverBackgroundImage = null;
            this.customButton_stepCapture.HoverFontColor = System.Drawing.Color.LightGray;
            this.customButton_stepCapture.HoverImage = null;
            this.customButton_stepCapture.IsSelected = false;
            this.customButton_stepCapture.Location = new System.Drawing.Point(86, 114);
            this.customButton_stepCapture.Name = "customButton_stepCapture";
            this.customButton_stepCapture.NormalBackgroundImage = global::Duxcycler.Properties.Resources.Capture_Disable;
            this.customButton_stepCapture.NormalFontColor = System.Drawing.Color.White;
            this.customButton_stepCapture.NormalImage = null;
            this.customButton_stepCapture.Size = new System.Drawing.Size(34, 34);
            this.customButton_stepCapture.TabIndex = 3;
            this.customButton_stepCapture.UseVisualStyleBackColor = false;
            this.customButton_stepCapture.Click += new System.EventHandler(this.customButton_stepCapture_Click);
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
            this.btn_Remove.Location = new System.Drawing.Point(86, 213);
            this.btn_Remove.Name = "btn_Remove";
            this.btn_Remove.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_Remove.NormalFontColor = System.Drawing.Color.White;
            this.btn_Remove.NormalImage = null;
            this.btn_Remove.Size = new System.Drawing.Size(34, 34);
            this.btn_Remove.TabIndex = 3;
            this.btn_Remove.Text = "-";
            this.btn_Remove.UseVisualStyleBackColor = false;
            this.btn_Remove.Click += new System.EventHandler(this.RemoveStep);
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
            this.btn_AddRight.Location = new System.Drawing.Point(155, 213);
            this.btn_AddRight.Name = "btn_AddRight";
            this.btn_AddRight.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_AddRight.NormalFontColor = System.Drawing.Color.White;
            this.btn_AddRight.NormalImage = null;
            this.btn_AddRight.Size = new System.Drawing.Size(34, 34);
            this.btn_AddRight.TabIndex = 3;
            this.btn_AddRight.Text = "+";
            this.btn_AddRight.UseVisualStyleBackColor = false;
            this.btn_AddRight.Click += new System.EventHandler(this.btn_AddRight_Click);
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
            this.btn_AddLeft.Location = new System.Drawing.Point(18, 213);
            this.btn_AddLeft.Name = "btn_AddLeft";
            this.btn_AddLeft.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.btn_AddLeft.NormalFontColor = System.Drawing.Color.White;
            this.btn_AddLeft.NormalImage = null;
            this.btn_AddLeft.Size = new System.Drawing.Size(34, 34);
            this.btn_AddLeft.TabIndex = 3;
            this.btn_AddLeft.Text = "+";
            this.btn_AddLeft.UseVisualStyleBackColor = false;
            this.btn_AddLeft.Click += new System.EventHandler(this.btn_AddLeft_Click);
            // 
            // Group_Step
            // 
            this.Controls.Add(this.resizePanel_Step);
            this.Name = "Group_Step";
            this.Size = new System.Drawing.Size(208, 306);
            this.Load += new System.EventHandler(this.StepBox_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.StepBox_Paint);
            this.Resize += new System.EventHandler(this.StepBox_Resize);
            this.resizePanel_Step.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NumericUpDown_StepTemp)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region Events
		private void StepBox_Load(object sender, System.EventArgs e)
		{
			SetGroupBoxCaption();

			//btn_AddLeft.Hide();
			//btn_Remove.Hide();
			//btn_AddRight.Hide();
			btn_AddLeft.BringToFront();
			btn_Remove.BringToFront();
			btn_AddRight.BringToFront();

			NumericUpDown_StepTemp.Value = new decimal(this.StepTemp);
			dateTimePicker_StepTime.Value = dateTimePicker_StepTime.Value.Date + StepTime;
			if(this.IsCapture)
				customButton_stepCapture.IsSelected = true;
			else
				customButton_stepCapture.IsSelected = false;
		}

		private void StepBox_Resize(object sender, System.EventArgs e)
		{
			int minY = 25;
			int maxY = this.Height - (70 + 30);
			int drawHeight = maxY - minY;
			double yScale = drawHeight / 100.0;
			int posX = (int)((this.Width / 2) - (NumericUpDown_StepTemp.Width / 2));
			int curTempPosY = maxY - (int)(StepTemp * yScale);
			NumericUpDown_StepTemp.Location = new Point(posX, curTempPosY - 25);
			posX = (int)((this.Width / 2) - (dateTimePicker_StepTime.Width / 2));
			dateTimePicker_StepTime.Location = new Point(posX, curTempPosY + 5);
			posX = (int)((this.Width / 2) - (customButton_stepCapture.Width / 2));
			curTempPosY = dateTimePicker_StepTime.Location.Y + dateTimePicker_StepTime.Height;
			customButton_stepCapture.Location = new Point(posX, curTempPosY);

			btn_AddLeft.Location = new Point(btn_AddLeft.Location.X, this.Height - 70);
			btn_Remove.Location = new Point(btn_Remove.Location.X, this.Height - 70);
			btn_AddRight.Location = new Point(btn_AddRight.Location.X, this.Height - 70);

			//if(m_bResizingFromCollapse != true)
			//{
			//	m_FullSize = this.Size;
			//}

			//Invalidate();
		}

		private void StepBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			//resizePanel_Step_Paint(this, e);
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

		#endregion Methods

		/// <summary>
		/// Step 영역에 마우스가 들어왔을때 발생하는 이벤트 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void resizePanel_Step_MouseEnter(object sender, EventArgs e)
        {
            m_bMouseEnter = true;
			resizePanel_Step.Invalidate();
		}

		/// <summary>
		/// Step 영역에서 마우스가 나갈때 발생하는 이벤트 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void resizePanel_Step_MouseLeave(object sender, EventArgs e)
        {
			m_bMouseEnter = false;
			resizePanel_Step.Invalidate();
		}

		/// <summary>
		/// 왼쪽의 Step Add 메뉴 실행 이벤트 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btn_AddLeft_Click(object sender, EventArgs e)
        {
			//int controlIndex = int.Parse(this.Name.Split('_')[1]);
			group_Parent.StepInsertLeft(StepId);
		}

		/// <summary>
		/// 오른쪽의 Step Add 메뉴 실행 이벤트 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btn_AddRight_Click(object sender, EventArgs e)
        {
			//int controlIndex = int.Parse(this.Name.Split('_')[1]);
			group_Parent.StepInsertRight(StepId);
		}

		/// <summary>
		/// Step Delete 메뉴 실행 이벤트 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveStep(object sender, EventArgs e)
		{
			group_Parent.StepRemove(StepId);
		}

		private void resizePanel_Step_MouseMove(object sender, MouseEventArgs e)
        {
			resizePanel_Step.Invalidate();
		}

        private void resizePanel_Step_Paint(object sender, PaintEventArgs e)
        {
			Graphics g = e.Graphics;
			//Graphics g = this.resizePanel_Step.CreateGraphics();

			int minY = 25;
			int maxY = this.Height - (70 + 30);
			int drawHeight = maxY - minY;
			List<Point> PointsInfo = new List<Point>();
			double yScale = drawHeight / 100.0;
			int previewTempPos = maxY - (int)(StepTempPreview * yScale);
			Point addPnt = new Point(0, previewTempPos);
			PointsInfo.Add(addPnt);
			int curTempPos = maxY - (int)(StepTemp * yScale);
			addPnt = new Point(this.Width / 2, curTempPos);
			PointsInfo.Add(addPnt);
			addPnt = new Point(this.Width, curTempPos);
			PointsInfo.Add(addPnt);
			addPnt = new Point(this.Width, this.Height);
			PointsInfo.Add(addPnt);
			addPnt = new Point(0, this.Height);
			PointsInfo.Add(addPnt);

			if (PointsInfo.Count > 2)
				g.FillPolygon(new SolidBrush(Color.FromArgb(213, 234, 248)), PointsInfo.ToArray());

			if (m_bMouseEnter)
			{
				Pen p = new Pen(Color.Gold, 2);

				Rectangle rec = new Rectangle(2, 2, this.Width - 4, this.Height - 4);
				g.DrawRectangle(p, rec);
			}
		}

        private void NumericUpDown_CycleCount_ValueChanged(object sender, EventArgs e)
        {
			group_Parent.group_Parent.methodEdit = true;

			StepTemp = (double)NumericUpDown_StepTemp.Value;

			int minY = 25;
			int maxY = this.Height - (70 + 30);
			int drawHeight = maxY - minY;
			double yScale = drawHeight / 100.0;
			int posX = (int)((this.Width / 2) - (NumericUpDown_StepTemp.Width / 2));
			int curTempPosY = maxY - (int)(StepTemp * yScale);
			NumericUpDown_StepTemp.Location = new Point(posX, curTempPosY - 25);
			posX = (int)((this.Width / 2) - (dateTimePicker_StepTime.Width / 2));
			dateTimePicker_StepTime.Location = new Point(posX, curTempPosY + 5);
			posX = (int)((this.Width / 2) - (customButton_stepCapture.Width / 2));
			curTempPosY = dateTimePicker_StepTime.Location.Y + dateTimePicker_StepTime.Height;
			customButton_stepCapture.Location = new Point(posX, curTempPosY);

			group_Parent.group_Parent.StageSort();
			//group_Parent.StepSort();
		}

		/// <summary>
		/// Step의 Capture 아이콘 메뉴 실행 이벤트 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void customButton_stepCapture_Click(object sender, EventArgs e)
        {
			group_Parent.group_Parent.methodEdit = true;

			if (this.IsCapture)
			{
				this.IsCapture = false;
				customButton_stepCapture.IsSelected = false;
			}
			else
			{
				this.IsCapture = true;
				customButton_stepCapture.IsSelected = true;
			}
		}

        private void dateTimePicker_StepTime_ValueChanged(object sender, EventArgs e)
        {
			group_Parent.group_Parent.methodEdit = true;

			DateTime dateTime = dateTimePicker_StepTime.Value;
			this.StepTime = dateTime.TimeOfDay;
		}

		/// <summary>
		/// Step에 저장된 설정값들을 반환한다. 
		/// </summary>
		/// <returns>Step 설정값</returns>
		public string GetSettingValues()
		{
			string stepTime = this.StepTime.ToString("hh\\:mm\\:ss");
			string saveValues = string.Format("{0:0.00}, {1}, {2}", this.StepTemp, stepTime, this.IsCapture);

			return saveValues;
		}
	}
}
