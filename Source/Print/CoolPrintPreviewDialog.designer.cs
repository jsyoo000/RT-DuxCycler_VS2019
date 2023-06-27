namespace CoolPrintPreview
{
    partial class CoolPrintPreviewDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CoolPrintPreviewDialog));
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this._btnZoom = new System.Windows.Forms.ToolStripSplitButton();
            this._itemActualSize = new System.Windows.Forms.ToolStripMenuItem();
            this._itemFullPage = new System.Windows.Forms.ToolStripMenuItem();
            this._itemPageWidth = new System.Windows.Forms.ToolStripMenuItem();
            this._itemTwoPages = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this._item500 = new System.Windows.Forms.ToolStripMenuItem();
            this._item200 = new System.Windows.Forms.ToolStripMenuItem();
            this._item150 = new System.Windows.Forms.ToolStripMenuItem();
            this._item100 = new System.Windows.Forms.ToolStripMenuItem();
            this._item75 = new System.Windows.Forms.ToolStripMenuItem();
            this._item50 = new System.Windows.Forms.ToolStripMenuItem();
            this._item25 = new System.Windows.Forms.ToolStripMenuItem();
            this._item10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._btnFirst = new System.Windows.Forms.ToolStripButton();
            this._btnPrev = new System.Windows.Forms.ToolStripButton();
            this._txtStartPage = new System.Windows.Forms.ToolStripTextBox();
            this._lblPageCount = new System.Windows.Forms.ToolStripLabel();
            this._btnNext = new System.Windows.Forms.ToolStripButton();
            this._btnLast = new System.Windows.Forms.ToolStripButton();
            this._separator = new System.Windows.Forms.ToolStripSeparator();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_Footer1 = new System.Windows.Forms.TextBox();
            this.textBox_Header_Left = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.radioButton_Black = new System.Windows.Forms.RadioButton();
            this.radioButton_White = new System.Windows.Forms.RadioButton();
            this.doubleBufferPanel1 = new CustomClassLibrary.DoubleBufferPanel();
            this.Button_Apply = new CustomClassLibrary.CustomButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBox_Footer2 = new System.Windows.Forms.TextBox();
            this.textBox_Header_Right = new System.Windows.Forms.TextBox();
            this.doubleBufferPanel2 = new CustomClassLibrary.DoubleBufferPanel();
            this.imageButton_Logo_Load = new CustomClassLibrary.ImageButton();
            this.checkBox_LogoView = new System.Windows.Forms.CheckBox();
            this.customPictureBox_LOGO = new CustomClassLibrary.CustomPictureBox();
            this.doubleBufferPanel3 = new CustomClassLibrary.DoubleBufferPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.doubleBufferPanel4 = new CustomClassLibrary.DoubleBufferPanel();
            this.checkBox_PrintAllStudyInfo = new System.Windows.Forms.CheckBox();
            this.checkBox_DiffROIDispaly = new System.Windows.Forms.CheckBox();
            this.checkBox_ROIView = new System.Windows.Forms.CheckBox();
            this.checkBox_PaletteBarView = new System.Windows.Forms.CheckBox();
            this.doubleBufferPanel5 = new CustomClassLibrary.DoubleBufferPanel();
            this.checkBox_PrintPage = new System.Windows.Forms.CheckBox();
            this._preview = new CoolPrintPreview.CoolPrintPreviewControl();
            this.Panel_PrintRange = new CustomClassLibrary.DoubleBufferPanel();
            this.textBox_PrintRange = new System.Windows.Forms.TextBox();
            this.radioButton_CurrentPage = new System.Windows.Forms.RadioButton();
            this.radioButton_Pages = new System.Windows.Forms.RadioButton();
            this.radioButton_All = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip_PagesTextBox = new System.Windows.Forms.ToolTip(this.components);
            this.Button_PageSetup = new CustomClassLibrary.CustomButton();
            this.Button_Save = new CustomClassLibrary.CustomButton();
            this.Button_Send = new CustomClassLibrary.CustomButton();
            this.Button_Print = new CustomClassLibrary.CustomButton();
            this.Button_Cancel = new CustomClassLibrary.CustomButton();
            this._toolStrip.SuspendLayout();
            this.doubleBufferPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.doubleBufferPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageButton_Logo_Load)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.customPictureBox_LOGO)).BeginInit();
            this.doubleBufferPanel3.SuspendLayout();
            this.doubleBufferPanel4.SuspendLayout();
            this.doubleBufferPanel5.SuspendLayout();
            this.Panel_PrintRange.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolStrip
            // 
            this._toolStrip.BackColor = System.Drawing.Color.Transparent;
            this._toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnZoom,
            this.toolStripSeparator2,
            this._btnFirst,
            this._btnPrev,
            this._txtStartPage,
            this._lblPageCount,
            this._btnNext,
            this._btnLast,
            this._separator});
            this._toolStrip.Location = new System.Drawing.Point(153, 2);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this._toolStrip.Size = new System.Drawing.Size(217, 25);
            this._toolStrip.TabIndex = 0;
            this._toolStrip.Text = "toolStrip1";
            // 
            // _btnZoom
            // 
            this._btnZoom.AutoToolTip = false;
            this._btnZoom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._itemActualSize,
            this._itemFullPage,
            this._itemPageWidth,
            this._itemTwoPages,
            this.toolStripMenuItem1,
            this._item500,
            this._item200,
            this._item150,
            this._item100,
            this._item75,
            this._item50,
            this._item25,
            this._item10});
            this._btnZoom.ForeColor = System.Drawing.Color.White;
            this._btnZoom.Image = ((System.Drawing.Image)(resources.GetObject("_btnZoom.Image")));
            this._btnZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._btnZoom.Name = "_btnZoom";
            this._btnZoom.Size = new System.Drawing.Size(71, 22);
            this._btnZoom.Text = "&Zoom";
            this._btnZoom.ButtonClick += new System.EventHandler(this._btnZoom_ButtonClick);
            this._btnZoom.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this._btnZoom_DropDownItemClicked);
            // 
            // _itemActualSize
            // 
            this._itemActualSize.Image = ((System.Drawing.Image)(resources.GetObject("_itemActualSize.Image")));
            this._itemActualSize.Name = "_itemActualSize";
            this._itemActualSize.Size = new System.Drawing.Size(136, 22);
            this._itemActualSize.Text = "Actual Size";
            // 
            // _itemFullPage
            // 
            this._itemFullPage.Image = ((System.Drawing.Image)(resources.GetObject("_itemFullPage.Image")));
            this._itemFullPage.Name = "_itemFullPage";
            this._itemFullPage.Size = new System.Drawing.Size(136, 22);
            this._itemFullPage.Text = "Full Page";
            // 
            // _itemPageWidth
            // 
            this._itemPageWidth.Image = ((System.Drawing.Image)(resources.GetObject("_itemPageWidth.Image")));
            this._itemPageWidth.Name = "_itemPageWidth";
            this._itemPageWidth.Size = new System.Drawing.Size(136, 22);
            this._itemPageWidth.Text = "Page Width";
            // 
            // _itemTwoPages
            // 
            this._itemTwoPages.Image = ((System.Drawing.Image)(resources.GetObject("_itemTwoPages.Image")));
            this._itemTwoPages.Name = "_itemTwoPages";
            this._itemTwoPages.Size = new System.Drawing.Size(136, 22);
            this._itemTwoPages.Text = "Two Pages";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(133, 6);
            // 
            // _item500
            // 
            this._item500.Name = "_item500";
            this._item500.Size = new System.Drawing.Size(136, 22);
            this._item500.Text = "500%";
            // 
            // _item200
            // 
            this._item200.Name = "_item200";
            this._item200.Size = new System.Drawing.Size(136, 22);
            this._item200.Text = "200%";
            // 
            // _item150
            // 
            this._item150.Name = "_item150";
            this._item150.Size = new System.Drawing.Size(136, 22);
            this._item150.Text = "150%";
            // 
            // _item100
            // 
            this._item100.Name = "_item100";
            this._item100.Size = new System.Drawing.Size(136, 22);
            this._item100.Text = "100%";
            // 
            // _item75
            // 
            this._item75.Name = "_item75";
            this._item75.Size = new System.Drawing.Size(136, 22);
            this._item75.Text = "75%";
            // 
            // _item50
            // 
            this._item50.Name = "_item50";
            this._item50.Size = new System.Drawing.Size(136, 22);
            this._item50.Text = "50%";
            // 
            // _item25
            // 
            this._item25.Name = "_item25";
            this._item25.Size = new System.Drawing.Size(136, 22);
            this._item25.Text = "25%";
            // 
            // _item10
            // 
            this._item10.Name = "_item10";
            this._item10.Size = new System.Drawing.Size(136, 22);
            this._item10.Text = "10%";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // _btnFirst
            // 
            this._btnFirst.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._btnFirst.Image = ((System.Drawing.Image)(resources.GetObject("_btnFirst.Image")));
            this._btnFirst.ImageTransparentColor = System.Drawing.Color.Red;
            this._btnFirst.Name = "_btnFirst";
            this._btnFirst.Size = new System.Drawing.Size(23, 22);
            this._btnFirst.Text = "First Page";
            this._btnFirst.Click += new System.EventHandler(this._btnFirst_Click);
            // 
            // _btnPrev
            // 
            this._btnPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._btnPrev.Image = ((System.Drawing.Image)(resources.GetObject("_btnPrev.Image")));
            this._btnPrev.ImageTransparentColor = System.Drawing.Color.Red;
            this._btnPrev.Name = "_btnPrev";
            this._btnPrev.Size = new System.Drawing.Size(23, 22);
            this._btnPrev.Text = "Previous Page";
            this._btnPrev.Click += new System.EventHandler(this._btnPrev_Click);
            // 
            // _txtStartPage
            // 
            this._txtStartPage.AutoSize = false;
            this._txtStartPage.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this._txtStartPage.Name = "_txtStartPage";
            this._txtStartPage.Size = new System.Drawing.Size(32, 23);
            this._txtStartPage.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this._txtStartPage.Enter += new System.EventHandler(this._txtStartPage_Enter);
            this._txtStartPage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._txtStartPage_KeyPress);
            this._txtStartPage.Validating += new System.ComponentModel.CancelEventHandler(this._txtStartPage_Validating);
            // 
            // _lblPageCount
            // 
            this._lblPageCount.ForeColor = System.Drawing.Color.White;
            this._lblPageCount.Name = "_lblPageCount";
            this._lblPageCount.Size = new System.Drawing.Size(11, 22);
            this._lblPageCount.Text = " ";
            // 
            // _btnNext
            // 
            this._btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._btnNext.Image = ((System.Drawing.Image)(resources.GetObject("_btnNext.Image")));
            this._btnNext.ImageTransparentColor = System.Drawing.Color.Red;
            this._btnNext.Name = "_btnNext";
            this._btnNext.Size = new System.Drawing.Size(23, 22);
            this._btnNext.Text = "Next Page";
            this._btnNext.Click += new System.EventHandler(this._btnNext_Click);
            // 
            // _btnLast
            // 
            this._btnLast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._btnLast.Image = ((System.Drawing.Image)(resources.GetObject("_btnLast.Image")));
            this._btnLast.ImageTransparentColor = System.Drawing.Color.Red;
            this._btnLast.Name = "_btnLast";
            this._btnLast.Size = new System.Drawing.Size(23, 22);
            this._btnLast.Text = "Last Page";
            this._btnLast.Click += new System.EventHandler(this._btnLast_Click);
            // 
            // _separator
            // 
            this._separator.Name = "_separator";
            this._separator.Size = new System.Drawing.Size(6, 25);
            this._separator.Visible = false;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(10, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(468, 65);
            this.label5.TabIndex = 101;
            this.label5.Text = "IRIS Print Preview";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_Footer1
            // 
            this.textBox_Footer1.BackColor = System.Drawing.Color.Black;
            this.textBox_Footer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_Footer1.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.textBox_Footer1.ForeColor = System.Drawing.Color.White;
            this.textBox_Footer1.Location = new System.Drawing.Point(92, 84);
            this.textBox_Footer1.Name = "textBox_Footer1";
            this.textBox_Footer1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_Footer1.Size = new System.Drawing.Size(353, 32);
            this.textBox_Footer1.TabIndex = 131;
            this.textBox_Footer1.TabStop = false;
            // 
            // textBox_Header_Left
            // 
            this.textBox_Header_Left.BackColor = System.Drawing.Color.Black;
            this.textBox_Header_Left.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_Header_Left.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.textBox_Header_Left.ForeColor = System.Drawing.Color.White;
            this.textBox_Header_Left.Location = new System.Drawing.Point(92, 7);
            this.textBox_Header_Left.Name = "textBox_Header_Left";
            this.textBox_Header_Left.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_Header_Left.Size = new System.Drawing.Size(353, 32);
            this.textBox_Header_Left.TabIndex = 132;
            this.textBox_Header_Left.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(7, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 28);
            this.label1.TabIndex = 129;
            this.label1.Text = "Header";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(7, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 28);
            this.label4.TabIndex = 130;
            this.label4.Text = "Footer";
            // 
            // radioButton_Black
            // 
            this.radioButton_Black.AutoSize = true;
            this.radioButton_Black.BackColor = System.Drawing.Color.Transparent;
            this.radioButton_Black.Checked = true;
            this.radioButton_Black.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButton_Black.ForeColor = System.Drawing.Color.White;
            this.radioButton_Black.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioButton_Black.Location = new System.Drawing.Point(236, 16);
            this.radioButton_Black.Name = "radioButton_Black";
            this.radioButton_Black.Size = new System.Drawing.Size(68, 25);
            this.radioButton_Black.TabIndex = 133;
            this.radioButton_Black.TabStop = true;
            this.radioButton_Black.Tag = "2";
            this.radioButton_Black.Text = "Black";
            this.radioButton_Black.UseVisualStyleBackColor = false;
            this.radioButton_Black.CheckedChanged += new System.EventHandler(this.RadioButton_Black_CheckedChanged);
            // 
            // radioButton_White
            // 
            this.radioButton_White.AutoSize = true;
            this.radioButton_White.BackColor = System.Drawing.Color.Transparent;
            this.radioButton_White.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButton_White.ForeColor = System.Drawing.Color.White;
            this.radioButton_White.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioButton_White.Location = new System.Drawing.Point(359, 16);
            this.radioButton_White.Name = "radioButton_White";
            this.radioButton_White.Size = new System.Drawing.Size(73, 25);
            this.radioButton_White.TabIndex = 134;
            this.radioButton_White.Tag = "1";
            this.radioButton_White.Text = "White";
            this.radioButton_White.UseVisualStyleBackColor = false;
            this.radioButton_White.CheckedChanged += new System.EventHandler(this.RadioButton_Black_CheckedChanged);
            // 
            // doubleBufferPanel1
            // 
            this.doubleBufferPanel1.BackColor = System.Drawing.Color.Transparent;
            this.doubleBufferPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.doubleBufferPanel1.Controls.Add(this.Button_Apply);
            this.doubleBufferPanel1.Controls.Add(this.pictureBox1);
            this.doubleBufferPanel1.Controls.Add(this.textBox_Footer2);
            this.doubleBufferPanel1.Controls.Add(this.textBox_Footer1);
            this.doubleBufferPanel1.Controls.Add(this.textBox_Header_Right);
            this.doubleBufferPanel1.Controls.Add(this.textBox_Header_Left);
            this.doubleBufferPanel1.Controls.Add(this.label4);
            this.doubleBufferPanel1.Controls.Add(this.label1);
            this.doubleBufferPanel1.Location = new System.Drawing.Point(21, 80);
            this.doubleBufferPanel1.Name = "doubleBufferPanel1";
            this.doubleBufferPanel1.Size = new System.Drawing.Size(457, 219);
            this.doubleBufferPanel1.TabIndex = 136;
            // 
            // Button_Apply
            // 
            this.Button_Apply.BackColor = System.Drawing.Color.Transparent;
            this.Button_Apply.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Button_Apply.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.Button_Apply.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.Button_Apply.DisableImage = null;
            this.Button_Apply.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.Button_Apply.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.Button_Apply.DownImage = null;
            this.Button_Apply.FlatAppearance.BorderSize = 0;
            this.Button_Apply.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Button_Apply.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Button_Apply.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Button_Apply.ForeColor = System.Drawing.Color.White;
            this.Button_Apply.HoverBackgroundImage = null;
            this.Button_Apply.HoverFontColor = System.Drawing.Color.LightGray;
            this.Button_Apply.HoverImage = null;
            this.Button_Apply.IsSelected = false;
            this.Button_Apply.Location = new System.Drawing.Point(314, 157);
            this.Button_Apply.Name = "Button_Apply";
            this.Button_Apply.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.Button_Apply.NormalFontColor = System.Drawing.Color.White;
            this.Button_Apply.NormalImage = null;
            this.Button_Apply.Size = new System.Drawing.Size(131, 57);
            this.Button_Apply.TabIndex = 203;
            this.Button_Apply.Text = "Apply";
            this.Button_Apply.UseVisualStyleBackColor = false;
            this.Button_Apply.Click += new System.EventHandler(this.ImageButton_Apply_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(7, 79);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(440, 1);
            this.pictureBox1.TabIndex = 140;
            this.pictureBox1.TabStop = false;
            // 
            // textBox_Footer2
            // 
            this.textBox_Footer2.BackColor = System.Drawing.Color.Black;
            this.textBox_Footer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_Footer2.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.textBox_Footer2.ForeColor = System.Drawing.Color.White;
            this.textBox_Footer2.Location = new System.Drawing.Point(92, 119);
            this.textBox_Footer2.Name = "textBox_Footer2";
            this.textBox_Footer2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_Footer2.Size = new System.Drawing.Size(353, 32);
            this.textBox_Footer2.TabIndex = 133;
            this.textBox_Footer2.TabStop = false;
            // 
            // textBox_Header_Right
            // 
            this.textBox_Header_Right.BackColor = System.Drawing.Color.Black;
            this.textBox_Header_Right.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_Header_Right.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.textBox_Header_Right.ForeColor = System.Drawing.Color.White;
            this.textBox_Header_Right.Location = new System.Drawing.Point(92, 43);
            this.textBox_Header_Right.Name = "textBox_Header_Right";
            this.textBox_Header_Right.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_Header_Right.Size = new System.Drawing.Size(353, 32);
            this.textBox_Header_Right.TabIndex = 132;
            this.textBox_Header_Right.TabStop = false;
            // 
            // doubleBufferPanel2
            // 
            this.doubleBufferPanel2.BackColor = System.Drawing.Color.Transparent;
            this.doubleBufferPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.doubleBufferPanel2.Controls.Add(this.imageButton_Logo_Load);
            this.doubleBufferPanel2.Controls.Add(this.checkBox_LogoView);
            this.doubleBufferPanel2.Controls.Add(this.customPictureBox_LOGO);
            this.doubleBufferPanel2.Location = new System.Drawing.Point(21, 304);
            this.doubleBufferPanel2.Name = "doubleBufferPanel2";
            this.doubleBufferPanel2.Size = new System.Drawing.Size(457, 66);
            this.doubleBufferPanel2.TabIndex = 137;
            // 
            // imageButton_Logo_Load
            // 
            this.imageButton_Logo_Load.DialogResult = System.Windows.Forms.DialogResult.None;
            this.imageButton_Logo_Load.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.imageButton_Logo_Load.DisableImage = null;
            this.imageButton_Logo_Load.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.imageButton_Logo_Load.DownImage = null;
            this.imageButton_Logo_Load.ForeColor = System.Drawing.Color.White;
            this.imageButton_Logo_Load.HoverFontColor = System.Drawing.Color.LightGray;
            this.imageButton_Logo_Load.HoverImage = null;
            this.imageButton_Logo_Load.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.imageButton_Logo_Load.IsSelected = false;
            this.imageButton_Logo_Load.Location = new System.Drawing.Point(403, 14);
            this.imageButton_Logo_Load.Name = "imageButton_Logo_Load";
            this.imageButton_Logo_Load.NormalFontColor = System.Drawing.Color.White;
            this.imageButton_Logo_Load.NormalImage = null;
            this.imageButton_Logo_Load.Size = new System.Drawing.Size(45, 45);
            this.imageButton_Logo_Load.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageButton_Logo_Load.TabIndex = 138;
            this.imageButton_Logo_Load.TabStop = false;
            this.imageButton_Logo_Load.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.imageButton_Logo_Load.Click += new System.EventHandler(this.ImageButton_Logo_Load_Click);
            // 
            // checkBox_LogoView
            // 
            this.checkBox_LogoView.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_LogoView.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBox_LogoView.ForeColor = System.Drawing.Color.White;
            this.checkBox_LogoView.Location = new System.Drawing.Point(12, 15);
            this.checkBox_LogoView.Name = "checkBox_LogoView";
            this.checkBox_LogoView.Size = new System.Drawing.Size(139, 32);
            this.checkBox_LogoView.TabIndex = 137;
            this.checkBox_LogoView.Text = "LOGO View";
            this.checkBox_LogoView.UseVisualStyleBackColor = false;
            this.checkBox_LogoView.CheckedChanged += new System.EventHandler(this.CheckBox_LogoView_CheckedChanged);
            // 
            // customPictureBox_LOGO
            // 
            this.customPictureBox_LOGO.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.customPictureBox_LOGO.IsSelected = false;
            this.customPictureBox_LOGO.IsShowLabel = false;
            this.customPictureBox_LOGO.LabelBackColor = System.Drawing.Color.White;
            this.customPictureBox_LOGO.LabelTextColor = System.Drawing.Color.Black;
            this.customPictureBox_LOGO.LableFont = new System.Drawing.Font("Segoe UI", 8F);
            this.customPictureBox_LOGO.Location = new System.Drawing.Point(155, 14);
            this.customPictureBox_LOGO.Name = "customPictureBox_LOGO";
            this.customPictureBox_LOGO.PictureLabel = "";
            this.customPictureBox_LOGO.SelectedColor = System.Drawing.Color.Red;
            this.customPictureBox_LOGO.Size = new System.Drawing.Size(242, 35);
            this.customPictureBox_LOGO.TabIndex = 136;
            this.customPictureBox_LOGO.TabStop = false;
            // 
            // doubleBufferPanel3
            // 
            this.doubleBufferPanel3.BackColor = System.Drawing.Color.Transparent;
            this.doubleBufferPanel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.doubleBufferPanel3.Controls.Add(this.radioButton_White);
            this.doubleBufferPanel3.Controls.Add(this.radioButton_Black);
            this.doubleBufferPanel3.Controls.Add(this.label3);
            this.doubleBufferPanel3.Location = new System.Drawing.Point(21, 375);
            this.doubleBufferPanel3.Name = "doubleBufferPanel3";
            this.doubleBufferPanel3.Size = new System.Drawing.Size(457, 55);
            this.doubleBufferPanel3.TabIndex = 138;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(7, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(193, 28);
            this.label3.TabIndex = 129;
            this.label3.Text = "Image Background";
            // 
            // doubleBufferPanel4
            // 
            this.doubleBufferPanel4.BackColor = System.Drawing.Color.Transparent;
            this.doubleBufferPanel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.doubleBufferPanel4.Controls.Add(this.checkBox_PrintAllStudyInfo);
            this.doubleBufferPanel4.Controls.Add(this.checkBox_DiffROIDispaly);
            this.doubleBufferPanel4.Controls.Add(this.checkBox_ROIView);
            this.doubleBufferPanel4.Controls.Add(this.checkBox_PaletteBarView);
            this.doubleBufferPanel4.Location = new System.Drawing.Point(21, 435);
            this.doubleBufferPanel4.Name = "doubleBufferPanel4";
            this.doubleBufferPanel4.Size = new System.Drawing.Size(457, 91);
            this.doubleBufferPanel4.TabIndex = 138;
            // 
            // checkBox_PrintAllStudyInfo
            // 
            this.checkBox_PrintAllStudyInfo.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_PrintAllStudyInfo.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBox_PrintAllStudyInfo.ForeColor = System.Drawing.Color.White;
            this.checkBox_PrintAllStudyInfo.Location = new System.Drawing.Point(260, 10);
            this.checkBox_PrintAllStudyInfo.Name = "checkBox_PrintAllStudyInfo";
            this.checkBox_PrintAllStudyInfo.Size = new System.Drawing.Size(192, 32);
            this.checkBox_PrintAllStudyInfo.TabIndex = 142;
            this.checkBox_PrintAllStudyInfo.Text = "Print All Study Info.";
            this.checkBox_PrintAllStudyInfo.UseVisualStyleBackColor = false;
            this.checkBox_PrintAllStudyInfo.CheckedChanged += new System.EventHandler(this.CheckBox_PrintAllStudyInfo_CheckedChanged);
            // 
            // checkBox_DiffROIDispaly
            // 
            this.checkBox_DiffROIDispaly.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_DiffROIDispaly.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBox_DiffROIDispaly.ForeColor = System.Drawing.Color.White;
            this.checkBox_DiffROIDispaly.Location = new System.Drawing.Point(260, 48);
            this.checkBox_DiffROIDispaly.Name = "checkBox_DiffROIDispaly";
            this.checkBox_DiffROIDispaly.Size = new System.Drawing.Size(192, 32);
            this.checkBox_DiffROIDispaly.TabIndex = 137;
            this.checkBox_DiffROIDispaly.Text = "Diff. ROI Display";
            this.checkBox_DiffROIDispaly.UseVisualStyleBackColor = false;
            this.checkBox_DiffROIDispaly.CheckedChanged += new System.EventHandler(this.CheckBox_DiffROIDispaly_CheckedChanged);
            // 
            // checkBox_ROIView
            // 
            this.checkBox_ROIView.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_ROIView.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBox_ROIView.ForeColor = System.Drawing.Color.White;
            this.checkBox_ROIView.Location = new System.Drawing.Point(12, 48);
            this.checkBox_ROIView.Name = "checkBox_ROIView";
            this.checkBox_ROIView.Size = new System.Drawing.Size(124, 32);
            this.checkBox_ROIView.TabIndex = 137;
            this.checkBox_ROIView.Text = "Show ROI";
            this.checkBox_ROIView.UseVisualStyleBackColor = false;
            this.checkBox_ROIView.CheckedChanged += new System.EventHandler(this.CheckBox_ROIView_CheckedChanged);
            // 
            // checkBox_PaletteBarView
            // 
            this.checkBox_PaletteBarView.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_PaletteBarView.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBox_PaletteBarView.ForeColor = System.Drawing.Color.White;
            this.checkBox_PaletteBarView.Location = new System.Drawing.Point(12, 10);
            this.checkBox_PaletteBarView.Name = "checkBox_PaletteBarView";
            this.checkBox_PaletteBarView.Size = new System.Drawing.Size(241, 32);
            this.checkBox_PaletteBarView.TabIndex = 137;
            this.checkBox_PaletteBarView.Text = "Save Image with Scalebar";
            this.checkBox_PaletteBarView.UseVisualStyleBackColor = false;
            this.checkBox_PaletteBarView.CheckedChanged += new System.EventHandler(this.CheckBox_PaletteBarView_CheckedChanged);
            // 
            // doubleBufferPanel5
            // 
            this.doubleBufferPanel5.BackColor = System.Drawing.Color.Transparent;
            this.doubleBufferPanel5.Controls.Add(this.checkBox_PrintPage);
            this.doubleBufferPanel5.Controls.Add(this._toolStrip);
            this.doubleBufferPanel5.Controls.Add(this._preview);
            this.doubleBufferPanel5.Location = new System.Drawing.Point(494, 12);
            this.doubleBufferPanel5.Name = "doubleBufferPanel5";
            this.doubleBufferPanel5.Size = new System.Drawing.Size(508, 659);
            this.doubleBufferPanel5.TabIndex = 140;
            // 
            // checkBox_PrintPage
            // 
            this.checkBox_PrintPage.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_PrintPage.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.checkBox_PrintPage.ForeColor = System.Drawing.Color.White;
            this.checkBox_PrintPage.Location = new System.Drawing.Point(3, 3);
            this.checkBox_PrintPage.Name = "checkBox_PrintPage";
            this.checkBox_PrintPage.Size = new System.Drawing.Size(121, 24);
            this.checkBox_PrintPage.TabIndex = 143;
            this.checkBox_PrintPage.Text = "Print Page";
            this.checkBox_PrintPage.UseVisualStyleBackColor = false;
            this.checkBox_PrintPage.CheckedChanged += new System.EventHandler(this.CheckBox_PrintPage_CheckedChanged);
            // 
            // _preview
            // 
            this._preview.AutoScroll = true;
            this._preview.BackColor = System.Drawing.Color.Black;
            this._preview.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._preview.Document = null;
            this._preview.Location = new System.Drawing.Point(0, 28);
            this._preview.Name = "_preview";
            this._preview.Size = new System.Drawing.Size(508, 631);
            this._preview.TabIndex = 1;
            this._preview.StartPageChanged += new System.EventHandler(this._preview_StartPageChanged);
            this._preview.PageCountChanged += new System.EventHandler(this._preview_PageCountChanged);
            // 
            // Panel_PrintRange
            // 
            this.Panel_PrintRange.BackColor = System.Drawing.Color.Transparent;
            this.Panel_PrintRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Panel_PrintRange.Controls.Add(this.textBox_PrintRange);
            this.Panel_PrintRange.Controls.Add(this.radioButton_CurrentPage);
            this.Panel_PrintRange.Controls.Add(this.radioButton_Pages);
            this.Panel_PrintRange.Controls.Add(this.radioButton_All);
            this.Panel_PrintRange.Controls.Add(this.label6);
            this.Panel_PrintRange.Controls.Add(this.label2);
            this.Panel_PrintRange.Location = new System.Drawing.Point(21, 531);
            this.Panel_PrintRange.Name = "Panel_PrintRange";
            this.Panel_PrintRange.Size = new System.Drawing.Size(320, 125);
            this.Panel_PrintRange.TabIndex = 141;
            // 
            // textBox_PrintRange
            // 
            this.textBox_PrintRange.BackColor = System.Drawing.Color.Black;
            this.textBox_PrintRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_PrintRange.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.textBox_PrintRange.ForeColor = System.Drawing.Color.White;
            this.textBox_PrintRange.Location = new System.Drawing.Point(96, 76);
            this.textBox_PrintRange.Name = "textBox_PrintRange";
            this.textBox_PrintRange.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_PrintRange.Size = new System.Drawing.Size(208, 32);
            this.textBox_PrintRange.TabIndex = 135;
            this.textBox_PrintRange.TabStop = false;
            this.textBox_PrintRange.Enter += new System.EventHandler(this.TextBox_PrintRange_Enter);
            this.textBox_PrintRange.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_PrintRange_KeyPress);
            this.textBox_PrintRange.Validating += new System.ComponentModel.CancelEventHandler(this.TextBox_PrintRange_Validating);
            // 
            // radioButton_CurrentPage
            // 
            this.radioButton_CurrentPage.AutoSize = true;
            this.radioButton_CurrentPage.BackColor = System.Drawing.Color.Transparent;
            this.radioButton_CurrentPage.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButton_CurrentPage.ForeColor = System.Drawing.Color.White;
            this.radioButton_CurrentPage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioButton_CurrentPage.Location = new System.Drawing.Point(96, 38);
            this.radioButton_CurrentPage.Name = "radioButton_CurrentPage";
            this.radioButton_CurrentPage.Size = new System.Drawing.Size(129, 25);
            this.radioButton_CurrentPage.TabIndex = 134;
            this.radioButton_CurrentPage.Tag = "2";
            this.radioButton_CurrentPage.Text = "Current Page";
            this.radioButton_CurrentPage.UseVisualStyleBackColor = false;
            this.radioButton_CurrentPage.CheckedChanged += new System.EventHandler(this.RadioButton_All_CheckedChanged);
            // 
            // radioButton_Pages
            // 
            this.radioButton_Pages.AutoSize = true;
            this.radioButton_Pages.BackColor = System.Drawing.Color.Transparent;
            this.radioButton_Pages.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButton_Pages.ForeColor = System.Drawing.Color.White;
            this.radioButton_Pages.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioButton_Pages.Location = new System.Drawing.Point(9, 80);
            this.radioButton_Pages.Name = "radioButton_Pages";
            this.radioButton_Pages.Size = new System.Drawing.Size(83, 25);
            this.radioButton_Pages.TabIndex = 134;
            this.radioButton_Pages.Tag = "2";
            this.radioButton_Pages.Text = "Pages :";
            this.radioButton_Pages.UseVisualStyleBackColor = false;
            this.radioButton_Pages.CheckedChanged += new System.EventHandler(this.RadioButton_All_CheckedChanged);
            // 
            // radioButton_All
            // 
            this.radioButton_All.AutoSize = true;
            this.radioButton_All.BackColor = System.Drawing.Color.Transparent;
            this.radioButton_All.Checked = true;
            this.radioButton_All.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.radioButton_All.ForeColor = System.Drawing.Color.White;
            this.radioButton_All.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioButton_All.Location = new System.Drawing.Point(9, 38);
            this.radioButton_All.Name = "radioButton_All";
            this.radioButton_All.Size = new System.Drawing.Size(47, 25);
            this.radioButton_All.TabIndex = 134;
            this.radioButton_All.TabStop = true;
            this.radioButton_All.Tag = "2";
            this.radioButton_All.Text = "All";
            this.radioButton_All.UseVisualStyleBackColor = false;
            this.radioButton_All.CheckedChanged += new System.EventHandler(this.RadioButton_All_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(12, 92);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 13);
            this.label6.TabIndex = 131;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 28);
            this.label2.TabIndex = 131;
            this.label2.Text = "Print Range";
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.BackColor = System.Drawing.Color.Transparent;
            this.Button_PageSetup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Button_PageSetup.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.Button_PageSetup.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.Button_PageSetup.DisableImage = null;
            this.Button_PageSetup.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.Button_PageSetup.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.Button_PageSetup.DownImage = null;
            this.Button_PageSetup.FlatAppearance.BorderSize = 0;
            this.Button_PageSetup.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Button_PageSetup.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Button_PageSetup.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Button_PageSetup.ForeColor = System.Drawing.Color.White;
            this.Button_PageSetup.HoverBackgroundImage = null;
            this.Button_PageSetup.HoverFontColor = System.Drawing.Color.LightGray;
            this.Button_PageSetup.HoverImage = null;
            this.Button_PageSetup.IsSelected = false;
            this.Button_PageSetup.Location = new System.Drawing.Point(347, 531);
            this.Button_PageSetup.Name = "Button_PageSetup";
            this.Button_PageSetup.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.Button_PageSetup.NormalFontColor = System.Drawing.Color.White;
            this.Button_PageSetup.NormalImage = null;
            this.Button_PageSetup.Size = new System.Drawing.Size(131, 57);
            this.Button_PageSetup.TabIndex = 204;
            this.Button_PageSetup.Text = "PageSetup";
            this.Button_PageSetup.UseVisualStyleBackColor = false;
            this.Button_PageSetup.Click += new System.EventHandler(this._btnPageSetup_Click);
            // 
            // Button_Save
            // 
            this.Button_Save.BackColor = System.Drawing.Color.Transparent;
            this.Button_Save.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Button_Save.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.Button_Save.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.Button_Save.DisableImage = null;
            this.Button_Save.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.Button_Save.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.Button_Save.DownImage = null;
            this.Button_Save.FlatAppearance.BorderSize = 0;
            this.Button_Save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Button_Save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Button_Save.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Button_Save.ForeColor = System.Drawing.Color.White;
            this.Button_Save.HoverBackgroundImage = null;
            this.Button_Save.HoverFontColor = System.Drawing.Color.LightGray;
            this.Button_Save.HoverImage = null;
            this.Button_Save.IsSelected = false;
            this.Button_Save.Location = new System.Drawing.Point(21, 678);
            this.Button_Save.Name = "Button_Save";
            this.Button_Save.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.Button_Save.NormalFontColor = System.Drawing.Color.White;
            this.Button_Save.NormalImage = null;
            this.Button_Save.Size = new System.Drawing.Size(230, 78);
            this.Button_Save.TabIndex = 205;
            this.Button_Save.Text = "Save";
            this.Button_Save.UseVisualStyleBackColor = false;
            this.Button_Save.Click += new System.EventHandler(this.ImageButton_Save_Click);
            // 
            // Button_Send
            // 
            this.Button_Send.BackColor = System.Drawing.Color.Transparent;
            this.Button_Send.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Button_Send.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.Button_Send.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.Button_Send.DisableImage = null;
            this.Button_Send.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.Button_Send.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.Button_Send.DownImage = null;
            this.Button_Send.FlatAppearance.BorderSize = 0;
            this.Button_Send.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Button_Send.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Button_Send.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Button_Send.ForeColor = System.Drawing.Color.White;
            this.Button_Send.HoverBackgroundImage = null;
            this.Button_Send.HoverFontColor = System.Drawing.Color.LightGray;
            this.Button_Send.HoverImage = null;
            this.Button_Send.IsSelected = false;
            this.Button_Send.Location = new System.Drawing.Point(271, 678);
            this.Button_Send.Name = "Button_Send";
            this.Button_Send.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.Button_Send.NormalFontColor = System.Drawing.Color.White;
            this.Button_Send.NormalImage = null;
            this.Button_Send.Size = new System.Drawing.Size(230, 78);
            this.Button_Send.TabIndex = 206;
            this.Button_Send.Text = "Send";
            this.Button_Send.UseVisualStyleBackColor = false;
            this.Button_Send.Click += new System.EventHandler(this.ImageButton_Send_Click);
            // 
            // Button_Print
            // 
            this.Button_Print.BackColor = System.Drawing.Color.Transparent;
            this.Button_Print.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Button_Print.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.Button_Print.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.Button_Print.DisableImage = null;
            this.Button_Print.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.Button_Print.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.Button_Print.DownImage = null;
            this.Button_Print.FlatAppearance.BorderSize = 0;
            this.Button_Print.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Button_Print.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Button_Print.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Button_Print.ForeColor = System.Drawing.Color.White;
            this.Button_Print.HoverBackgroundImage = null;
            this.Button_Print.HoverFontColor = System.Drawing.Color.LightGray;
            this.Button_Print.HoverImage = null;
            this.Button_Print.IsSelected = false;
            this.Button_Print.Location = new System.Drawing.Point(521, 678);
            this.Button_Print.Name = "Button_Print";
            this.Button_Print.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.Button_Print.NormalFontColor = System.Drawing.Color.White;
            this.Button_Print.NormalImage = null;
            this.Button_Print.Size = new System.Drawing.Size(230, 78);
            this.Button_Print.TabIndex = 207;
            this.Button_Print.Text = "Print";
            this.Button_Print.UseVisualStyleBackColor = false;
            this.Button_Print.Click += new System.EventHandler(this._btnPrint_Click);
            // 
            // Button_Cancel
            // 
            this.Button_Cancel.BackColor = System.Drawing.Color.Transparent;
            this.Button_Cancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Button_Cancel.DisableBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Disable;
            this.Button_Cancel.DisableFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.Button_Cancel.DisableImage = null;
            this.Button_Cancel.DownBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_MDown;
            this.Button_Cancel.DownFontColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(248)))), ((int)(((byte)(203)))));
            this.Button_Cancel.DownImage = null;
            this.Button_Cancel.FlatAppearance.BorderSize = 0;
            this.Button_Cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Button_Cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Button_Cancel.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Button_Cancel.ForeColor = System.Drawing.Color.White;
            this.Button_Cancel.HoverBackgroundImage = null;
            this.Button_Cancel.HoverFontColor = System.Drawing.Color.LightGray;
            this.Button_Cancel.HoverImage = null;
            this.Button_Cancel.IsSelected = false;
            this.Button_Cancel.Location = new System.Drawing.Point(771, 678);
            this.Button_Cancel.Name = "Button_Cancel";
            this.Button_Cancel.NormalBackgroundImage = global::Duxcycler.Properties.Resources.SUBMEMU1_BTN_Normal;
            this.Button_Cancel.NormalFontColor = System.Drawing.Color.White;
            this.Button_Cancel.NormalImage = null;
            this.Button_Cancel.Size = new System.Drawing.Size(230, 78);
            this.Button_Cancel.TabIndex = 208;
            this.Button_Cancel.Text = "Cancel";
            this.Button_Cancel.UseVisualStyleBackColor = false;
            this.Button_Cancel.Click += new System.EventHandler(this._btnCancel_Click);
            // 
            // CoolPrintPreviewDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.Button_Cancel);
            this.Controls.Add(this.Button_Print);
            this.Controls.Add(this.Button_Send);
            this.Controls.Add(this.Button_Save);
            this.Controls.Add(this.Button_PageSetup);
            this.Controls.Add(this.Panel_PrintRange);
            this.Controls.Add(this.doubleBufferPanel5);
            this.Controls.Add(this.doubleBufferPanel4);
            this.Controls.Add(this.doubleBufferPanel3);
            this.Controls.Add(this.doubleBufferPanel2);
            this.Controls.Add(this.doubleBufferPanel1);
            this.Controls.Add(this.label5);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CoolPrintPreviewDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IRIS Print Preview";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CoolPrintPreviewDialog_FormClosing);
            this.Load += new System.EventHandler(this.CoolPrintPreviewDialog_Load);
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this.doubleBufferPanel1.ResumeLayout(false);
            this.doubleBufferPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.doubleBufferPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imageButton_Logo_Load)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.customPictureBox_LOGO)).EndInit();
            this.doubleBufferPanel3.ResumeLayout(false);
            this.doubleBufferPanel3.PerformLayout();
            this.doubleBufferPanel4.ResumeLayout(false);
            this.doubleBufferPanel5.ResumeLayout(false);
            this.doubleBufferPanel5.PerformLayout();
            this.Panel_PrintRange.ResumeLayout(false);
            this.Panel_PrintRange.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip _toolStrip;
        private CoolPrintPreviewControl _preview;
        private System.Windows.Forms.ToolStripSplitButton _btnZoom;
        private System.Windows.Forms.ToolStripMenuItem _itemActualSize;
        private System.Windows.Forms.ToolStripMenuItem _itemFullPage;
        private System.Windows.Forms.ToolStripMenuItem _itemPageWidth;
        private System.Windows.Forms.ToolStripMenuItem _itemTwoPages;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem _item500;
        private System.Windows.Forms.ToolStripMenuItem _item200;
        private System.Windows.Forms.ToolStripMenuItem _item150;
        private System.Windows.Forms.ToolStripMenuItem _item100;
        private System.Windows.Forms.ToolStripMenuItem _item75;
        private System.Windows.Forms.ToolStripMenuItem _item50;
        private System.Windows.Forms.ToolStripMenuItem _item25;
        private System.Windows.Forms.ToolStripMenuItem _item10;
        private System.Windows.Forms.ToolStripButton _btnFirst;
        private System.Windows.Forms.ToolStripButton _btnPrev;
        private System.Windows.Forms.ToolStripTextBox _txtStartPage;
        private System.Windows.Forms.ToolStripLabel _lblPageCount;
        private System.Windows.Forms.ToolStripButton _btnNext;
        private System.Windows.Forms.ToolStripButton _btnLast;
        private System.Windows.Forms.ToolStripSeparator _separator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_Footer1;
        private System.Windows.Forms.TextBox textBox_Header_Left;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioButton_Black;
        private System.Windows.Forms.RadioButton radioButton_White;
        private CustomClassLibrary.DoubleBufferPanel doubleBufferPanel1;
        private CustomClassLibrary.DoubleBufferPanel doubleBufferPanel2;
        private CustomClassLibrary.DoubleBufferPanel doubleBufferPanel3;
        private System.Windows.Forms.Label label3;
        private CustomClassLibrary.DoubleBufferPanel doubleBufferPanel4;
        private CustomClassLibrary.DoubleBufferPanel doubleBufferPanel5;
        private System.Windows.Forms.TextBox textBox_Header_Right;
        private System.Windows.Forms.TextBox textBox_Footer2;
        private CustomClassLibrary.CustomPictureBox customPictureBox_LOGO;
        private System.Windows.Forms.CheckBox checkBox_LogoView;
        private System.Windows.Forms.CheckBox checkBox_ROIView;
        private System.Windows.Forms.CheckBox checkBox_PaletteBarView;
        private System.Windows.Forms.CheckBox checkBox_DiffROIDispaly;
        private CustomClassLibrary.ImageButton imageButton_Logo_Load;
        private System.Windows.Forms.CheckBox checkBox_PrintAllStudyInfo;
        private System.Windows.Forms.CheckBox checkBox_PrintPage;
        private CustomClassLibrary.DoubleBufferPanel Panel_PrintRange;
        private System.Windows.Forms.TextBox textBox_PrintRange;
        private System.Windows.Forms.RadioButton radioButton_Pages;
        private System.Windows.Forms.RadioButton radioButton_All;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RadioButton radioButton_CurrentPage;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolTip toolTip_PagesTextBox;
        private CustomClassLibrary.CustomButton Button_Apply;
        private CustomClassLibrary.CustomButton Button_PageSetup;
        private CustomClassLibrary.CustomButton Button_Save;
        private CustomClassLibrary.CustomButton Button_Send;
        private CustomClassLibrary.CustomButton Button_Print;
        private CustomClassLibrary.CustomButton Button_Cancel;
    }
}