//using Dicom;
//using Dicom.Imaging;
//using Dicom.IO.Buffer;
using Duxcycler_Database;
using Duxcycler_GLOBAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using System.Text.RegularExpressions;
using System.Linq;
using CustomClassLibrary;

namespace CoolPrintPreview
{
    // 인쇄에서 제외할 Page를 설정한다. 제외 Page를 설정하면 PrintDocument_PrintPage 해당 Page를 제외하고 나머지만 만든다.
    public delegate void SetPrintIgnorePageHandler(string PrintIgnorePages);
    /// <summary>
    /// Represents a dialog containing a <see cref="CoolPrintPreviewControl"/> control
    /// used to preview and print <see cref="PrintDocument"/> objects.
    /// </summary>
    /// <remarks>
    /// This dialog is similar to the standard <see cref="PrintPreviewDialog"/>
    /// but provides additional options such printer and page setup buttons,
    /// a better UI based on the <see cref="ToolStrip"/> control, and built-in
    /// PDF export.
    /// </remarks>
    public partial class CoolPrintPreviewDialog : Form
    {
        //--------------------------------------------------------------------
        #region ** fields

        PrintDocument _doc;

        #endregion

        //--------------------------------------------------------------------


        bool IsFileWorkerCompleted = true;      // 파일 저장이 완료되었는지
        BackgroundWorker fileWorker;            // 파일 저장용 BackgroundWorker

        #region ** ctor

        /// <summary>
        /// Initializes a new instance of a <see cref="CoolPrintPreviewDialog"/>.
        /// </summary>
        public CoolPrintPreviewDialog(bool showcheckboxprintallstudy = false) : this(null, showcheckboxprintallstudy)
        {

        }
        /// <summary>
        /// Initializes a new instance of a <see cref="CoolPrintPreviewDialog"/>.
        /// </summary>
        /// <param name="parentForm">Parent form that defines the initial size for this dialog.</param>
        public CoolPrintPreviewDialog(Control parentForm, bool showcheckboxprintallstudy = false)
        {
            InitializeComponent();

            // 파일 저장용 BackgroundWorker 설정 부분
            fileWorker = new BackgroundWorker();
            fileWorker.WorkerReportsProgress = true;
            fileWorker.WorkerSupportsCancellation = true;
            fileWorker.DoWork += new DoWorkEventHandler(FileWorker_DoWork);

            this.Opacity = 0;
            PrintIsPages.Clear();

            #region Form 기본 설정
            this.BackgroundImage = Duxcycler.Properties.Resources.BackImageB;      // 윈도우 BackgroundImage 설정, 실행파일에 Resources폴더에서 읽는다.
            this.FormBorderStyle = FormBorderStyle.None;                // 윈도우 테두리 제거
            this.Size = new Size(1024, 768);                           // 윈도우 크기 설정 

            // Form에 나타날때 깜박거림을 줄이기 위한 코드
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            Screen[] screens;
            screens = Screen.AllScreens;
            this.Location = new System.Drawing.Point(screens[Global.ScreensIndex].Bounds.Left, screens[Global.ScreensIndex].Bounds.Top);

            #endregion

            #region 이미지 버튼에 기본 이미지 설정, 배경 투명하게            
            this.imageButton_Logo_Load.NormalImage  = Duxcycler.Properties.Resources.ROISAVED_Normal;         // ROI_Saved 버튼 Normal 이미지 설정
            this.imageButton_Logo_Load.DownImage    = Duxcycler.Properties.Resources.ROISAVED_MDown;          // ROI_Saved 버튼 마우스 다운 이미지 설정
            this.imageButton_Logo_Load.DisableImage = Duxcycler.Properties.Resources.ROISAVED_Disable;        // ROI_Saved 버튼 Disable 이미지 설정
            this.imageButton_Logo_Load.BackColor    = Color.Transparent;                                        // ROI_Saved 버튼 배경 투명하게            
            #endregion

            this.textBox_Header_Left.Text = Global.Header_Left;
            this.textBox_Header_Right.Text = Global.Header_Right;
            this.textBox_Footer1.Text = Global.Footer_Left1;
            this.textBox_Footer2.Text = Global.Footer_Left2;

            if (Global.Print_LOGO_Show)
            {
                this.checkBox_LogoView.Checked = true;
                this.checkBox_LogoView.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_LogoView.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_LogoView.Checked = false;
                this.checkBox_LogoView.Font = new Font("Segoe UI", 12);
                this.checkBox_LogoView.ForeColor = Color.DarkGray;
            }

            Bitmap logoImg = null;

            String imgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("LOGO.bmp"));
            if (File.Exists(imgPath)) logoImg = new Bitmap(imgPath);

            Bitmap NewImg = new Bitmap(logoImg);
            logoImg.Dispose();
            logoImg = null;

            customPictureBox_LOGO.BackgroundImage = NewImg;

            if (Global.Print_PaletteBar_Show)
            {
                this.checkBox_PaletteBarView.Checked = true;
                this.checkBox_PaletteBarView.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_PaletteBarView.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_PaletteBarView.Checked = false;
                this.checkBox_PaletteBarView.Font = new Font("Segoe UI", 12);
                this.checkBox_PaletteBarView.ForeColor = Color.DarkGray;
            }

            if (Global.Print_ROI_Show)
            {
                this.checkBox_ROIView.Checked = true;
                this.checkBox_ROIView.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_ROIView.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_ROIView.Checked = false;
                this.checkBox_ROIView.Font = new Font("Segoe UI", 12);
                this.checkBox_ROIView.ForeColor = Color.DarkGray;
            }

            if (Global.Print_Diff_ROI_Display)
            {
                this.checkBox_DiffROIDispaly.Checked = true;
                this.checkBox_DiffROIDispaly.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_DiffROIDispaly.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_DiffROIDispaly.Checked = false;
                this.checkBox_DiffROIDispaly.Font = new Font("Segoe UI", 12);
                this.checkBox_DiffROIDispaly.ForeColor = Color.DarkGray;
            }

            // Study Info를 출력할 경우에만 보이게 처리
            this.checkBox_PrintAllStudyInfo.Visible = showcheckboxprintallstudy;

            this.checkBox_PrintPage.Checked = true;
            this.checkBox_PrintPage.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.checkBox_PrintPage.ForeColor = Color.White;

            if (Global.Print_StudyAll)
            {
                this.checkBox_PrintAllStudyInfo.Checked = true;
                this.checkBox_PrintAllStudyInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_PrintAllStudyInfo.ForeColor = Color.White;

                this.Panel_PrintRange.Visible = showcheckboxprintallstudy;
                this.checkBox_PrintPage.Visible = showcheckboxprintallstudy;
            }
            else
            {
                this.checkBox_PrintAllStudyInfo.Checked = false;
                this.checkBox_PrintAllStudyInfo.Font = new Font("Segoe UI", 12);
                this.checkBox_PrintAllStudyInfo.ForeColor = Color.DarkGray;
                this.checkBox_PrintPage.Visible = false;

                this.Panel_PrintRange.Visible = false;
                this.checkBox_PrintPage.Visible = false;
            }



            if (Global.Print_ImageBackColor_Black)
            {
                this.radioButton_Black.Checked = true;
                this.radioButton_Black.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.radioButton_Black.ForeColor = Color.White;
                this.radioButton_White.Font = new Font("Segoe UI", 12);
                this.radioButton_White.ForeColor = Color.DarkGray;
            }
            else
            {
                this.radioButton_White.Checked = true;
                this.radioButton_Black.Font = new Font("Segoe UI", 12);
                this.radioButton_Black.ForeColor = Color.DarkGray;
                this.radioButton_White.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.radioButton_White.ForeColor = Color.White;
            }


            if (parentForm != null)
            {
                Size = parentForm.Size;
            }
        }
        #endregion

        private void CoolPrintPreviewDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fileWorker != null)      // 생성된 것이 있으면
            {
                if (fileWorker.IsBusy)   // 해당 스레드가 실행중이면
                {
                    fileWorker.CancelAsync();    // 스레드 취소
                }
            }
        }

        //--------------------------------------------------------------------
        #region ** object model

        /// <summary>
        /// Gets or sets the <see cref="PrintDocument"/> to preview.
        /// </summary>
        public PrintDocument Document
        {
            get { return _doc; }
            set
            {
                // unhook event handlers
                if (_doc != null)
                {
                    _doc.BeginPrint -= _doc_BeginPrint;
                    _doc.EndPrint -= _doc_EndPrint;
                }

                // save the value
                _doc = value;

                // hook up event handlers
                if (_doc != null)
                {
                    _doc.BeginPrint += _doc_BeginPrint;
                    _doc.EndPrint += _doc_EndPrint;
                }


                // don't assign document to preview until this form becomes visible
                if (Visible)
                {
                    _preview.Document = Document;
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------
        #region ** overloads

        /// <summary>
        /// Overridden to assign document to preview control only after the 
        /// initial activation.
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Parent의 중심에 표시
            if (this.Parent != null)
            {
                int screenLeft = this.Parent.Location.X;
                int screenTop = this.Parent.Location.Y;
                int screenWidth = this.Parent.Width;
                int screenHeight = this.Parent.Height;

                this.Location = new Point(screenLeft + (screenWidth / 2) - (this.Width / 2), screenTop + (screenHeight / 2) - (this.Height / 2));
            }

            if (_preview != null) _preview.Document = Document;

            for (int page = 0; page < _preview.PageCount; page++) this.PrintIsPages.Add(true);

            toolTip_PagesTextBox.SetToolTip(textBox_PrintRange, "페이지 번호나 페이지 범위를 쉼표(,)로 구분하여 다음과 같이 입력하십시오.\n예) 1,3,5-12");

            RadioButton_All_CheckedChanged(this.radioButton_All, null);

            this.Opacity = 1;
        }
        /// <summary>
        /// Overridden to cancel any ongoing previews when closing form.
        /// </summary>
        /// <param name="e"><see cref="FormClosingEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_preview.IsRendering && !e.Cancel)
            {
                _preview.Cancel();
            }
        }

        #endregion

        //--------------------------------------------------------------------
        public SetPrintIgnorePageHandler SetPrintIgnorePage;          // 인쇄에서 제외할 Page를 설정한다. 제외 Page를 설정하면 PrintDocument_PrintPage 해당 Page를 제외하고 나머지만 만든다.
        private List<bool> PrintIsPages = new List<bool>();     // 인쇄할 Page의 여부 설정 변수( true: 인쇄, false: 인쇄안함 )
        //--------------------------------------------------------------------
        #region ** main commands

        void _btnPrint_Click(object sender, EventArgs e)
        {
            // 저장할 파일 총 갯수 구하기
            int SendTotalCount = this.PrintIsPages.FindAll(p => p == true).Count;

            // 저장할 파일이 없으면
            if (SendTotalCount <= 0)
            {
                MessageBox.Show("No Print selected information.");
                return;
            }

            using (var dlg = new PrintDialog())
            {
                // configure dialog
                dlg.AllowSomePages = true;
                dlg.AllowSelection = true;
                dlg.UseEXDialog = true;
                dlg.Document = Document;

                // show allowed page range
                var ps = dlg.PrinterSettings;
                ps.MinimumPage = ps.FromPage = 1;
                ps.MaximumPage = ps.ToPage = _preview.PageCount;

                // 인쇄할 Page를 설정한다.
                if (this.radioButton_CurrentPage.Checked)                       // 현제 페이지만 인쇄할 경우
                {
                    ps.PrintRange = PrintRange.CurrentPage;
                }
                else if (this.radioButton_Pages.Checked)                        // 인쇄 범위 지정한 경우
                {
                    string PrintIgnorPages = "";

                    // 인쇄 제외할 Page를 설정한다.
                    for (int page = 0; page < this.PrintIsPages.Count; page++)
                    {
                        if (!this.PrintIsPages[page]) PrintIgnorPages += $"{page + 1},";
                    }
                    SetPrintIgnorePage?.Invoke(PrintIgnorPages);
                }

                // print selected page range
                _preview.Print();

                // 창을 닫는다.
                Close();
            }
        }
        void _btnPageSetup_Click(object sender, EventArgs e)
        {
            using (var dlg = new PageSetupDialog())
            {
                dlg.Document = Document;

                dlg.PageSettings.Margins.Left = Convert.ToInt32((Global.MarginsLeft) * 10);
                dlg.PageSettings.Margins.Right = Convert.ToInt32((Global.MarginsRight) * 10);
                dlg.PageSettings.Margins.Top = Convert.ToInt32((Global.MarginsTop) * 10);
                dlg.PageSettings.Margins.Bottom = Convert.ToInt32((Global.MarginsBottom) * 10);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    dlg.PageSettings.Margins = PrinterUnitConvert.Convert(dlg.PageSettings.Margins, PrinterUnit.ThousandthsOfAnInch, PrinterUnit.TenthsOfAMillimeter);
                    Global.MarginsLeft = (dlg.PageSettings.Margins.Left);
                    Global.MarginsRight = (dlg.PageSettings.Margins.Right);
                    Global.MarginsTop = (dlg.PageSettings.Margins.Top);
                    Global.MarginsBottom = (dlg.PageSettings.Margins.Bottom);

                    // 바로 적용
                    ImageButton_Apply_Click(sender, e);
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------
        #region ** zoom

        void _btnZoom_ButtonClick(object sender, EventArgs e)
        {
            _preview.ZoomMode = _preview.ZoomMode == ZoomMode.ActualSize
                ? ZoomMode.FullPage
                : ZoomMode.ActualSize;
        }
        void _btnZoom_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == _itemActualSize)
            {
                _preview.ZoomMode = ZoomMode.ActualSize;
            }
            else if (e.ClickedItem == _itemFullPage)
            {
                _preview.ZoomMode = ZoomMode.FullPage;
            }
            else if (e.ClickedItem == _itemPageWidth)
            {
                _preview.ZoomMode = ZoomMode.PageWidth;
            }
            else if (e.ClickedItem == _itemTwoPages)
            {
                _preview.ZoomMode = ZoomMode.TwoPages;
            }
            if (e.ClickedItem == _item10)
            {
                _preview.Zoom = .1;
            }
            else if (e.ClickedItem == _item100)
            {
                _preview.Zoom = 1;
            }
            else if (e.ClickedItem == _item150)
            {
                _preview.Zoom = 1.5;
            }
            else if (e.ClickedItem == _item200)
            {
                _preview.Zoom = 2;
            }
            else if (e.ClickedItem == _item25)
            {
                _preview.Zoom = .25;
            }
            else if (e.ClickedItem == _item50)
            {
                _preview.Zoom = .5;
            }
            else if (e.ClickedItem == _item500)
            {
                _preview.Zoom = 5;
            }
            else if (e.ClickedItem == _item75)
            {
                _preview.Zoom = .75;
            }
        }
        #endregion

        //--------------------------------------------------------------------
        #region ** page navigation

        // 이동시 처리함수
        void MoveProcess()
        {
            if (this.radioButton_CurrentPage.Checked)
            {
                for (int page = 0; page < this.PrintIsPages.Count; page++)
                {
                    if (page == _preview.StartPage) this.PrintIsPages[page] = true;
                    else this.PrintIsPages[page] = false;
                }
                this.checkBox_PrintPage.Enabled = false;
                this.checkBox_PrintPage.Checked = true;
            }
            else
            {
                this.checkBox_PrintPage.Enabled = true;
                if (_preview.StartPage >= 0 && _preview.StartPage < this.PrintIsPages.Count)
                {
                    this.checkBox_PrintPage.Checked = this.PrintIsPages[_preview.StartPage];
                }
            }

            SetTextBox_PrintRange();
        }
        void _btnFirst_Click(object sender, EventArgs e)
        {
            _preview.StartPage = 0;

            MoveProcess();
        }
        void _btnPrev_Click(object sender, EventArgs e)
        {
            _preview.StartPage--;

            MoveProcess();
        }
        void _btnNext_Click(object sender, EventArgs e)
        {
            _preview.StartPage++;

            MoveProcess();
        }
        void _btnLast_Click(object sender, EventArgs e)
        {
            _preview.StartPage = _preview.PageCount - 1;

            MoveProcess();
        }
        void _txtStartPage_Enter(object sender, EventArgs e)
        {
            _txtStartPage.SelectAll();
        }
        void _txtStartPage_Validating(object sender, CancelEventArgs e)
        {
            CommitPageNumber();
        }
        void _txtStartPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            if (c == (char)13)
            {
                CommitPageNumber();
                e.Handled = true;
            }
            else if (c > ' ' && !char.IsDigit(c))
            {
                e.Handled = true;
            }
        }
        void CommitPageNumber()
        {
            if (int.TryParse(_txtStartPage.Text, out int page))
            {
                _preview.StartPage = page - 1;
                MoveProcess();
            }
        }
        void _preview_StartPageChanged(object sender, EventArgs e)
        {
            var page = _preview.StartPage + 1;
            _txtStartPage.Text = page.ToString();
        }
        private void _preview_PageCountChanged(object sender, EventArgs e)
        {
            this.Update();
            Application.DoEvents();
            _lblPageCount.Text = string.Format("of {0}", _preview.PageCount);
        }

        #endregion

        //--------------------------------------------------------------------
        #region ** job control

        void _btnCancel_Click(object sender, EventArgs e)
        {
            if (_preview != null && _preview.IsRendering)
            {
                _preview.Cancel();
            }
            else
            {
                Close();
            }
        }
        void _doc_BeginPrint(object sender, PrintEventArgs e)
        {
            //_btnCancel.Text = "&Cancel";
            //_btnPrint.Enabled = _btnPageSetup.Enabled = false;


        }
        void _doc_EndPrint(object sender, PrintEventArgs e)
        {
            //_btnCancel.Text = "&Close";
            //_btnPrint.Enabled = _btnPageSetup.Enabled = true;
        }

        #endregion

        public static double InchToMillimeter(double fInch)
        {
            return fInch * 25.4;
        }

        public static double MillimeterToInch(double fMillimeter)
        {
            return fMillimeter / 25.4;
        }

        // 변경된 정보로 프린트 미리보기를 다시 적용하고, 정보 저장하기
        private void ImageButton_Apply_Click(object sender, EventArgs e)
        {
            Global.Header_Left = this.textBox_Header_Left.Text;
            Global.Header_Right = this.textBox_Header_Right.Text;
            Global.Footer_Left1 = this.textBox_Footer1.Text;
            Global.Footer_Left2 = this.textBox_Footer2.Text;

            Global.SavedSetting();


            // to show new page layout
            if (_preview != null)
            {
                _preview.StartPage = 0;
                _preview.RefreshPreview();
            }
        }

        // 현제 프린트를 파일로 저장하기
        CustomClassLibrary.AlertProgressForm alertFileProgress;

        string PrintFileName = "";
        int SendTotalCount = 0;
        int SaveCount = 0;

        private void ImageButton_Save_Click(object sender, EventArgs e)
        {
            if (Global.listResultInfos == null || Global.listResultInfos.Count == 0 || Global.selectedStudyIndex >= Global.listResultInfos.Count || Global.selectedStudyIndex < 0) return;

            // 저장할 파일 총 갯수 구하기
            this.SendTotalCount = this.PrintIsPages.FindAll(p => p == true).Count;

            // 저장할 파일이 없으면
            if (this.SendTotalCount <= 0)
            {
                MessageBox.Show("No Print selected information.");
                return;
            }

            ResultInfo study = Global.listResultInfos[Global.selectedStudyIndex]; // 선택된 Study Info 

            string[] splitString = Global.SaveFileNameType.Split('_');

            string saveFileName = "";
            bool bFirst = true;
            //foreach (string name in splitString)
            //{
            //    if (!bFirst) saveFileName += "_";
            //    if (name == "ChartNO") { saveFileName += String.Format("{0}", study.patient.ChartNo); }   // 환자 ChartNo
            //    else if (name == "Name") { saveFileName += String.Format("{0}", study.patient.Name); }   // 환자 이름
            //    else if (name == "Gender")
            //    {
            //        if (study.patient.Gender == 1) saveFileName += "M";       // 남자
            //        else saveFileName += "F";       // 여자 
            //    }
            //    else if (name == "Birthday") { saveFileName += study.patient.GetBirthday().ToString("yyyyMMdd"); }   // 환자 생년월일
            //    else if (name == "StudyDate") { saveFileName += study.StudyDateTime.ToLocalTime().ToString("yyyyMMddHHmmss"); }   // StydyDateTime

            //    bFirst = false;
            //}

            saveFileName += DateTime.Now.ToLocalTime().ToString("_yyyyMMddHHmmss"); // 생성날짜 추가 -> 누를때 마다 생성
            //if (Global.IsSavePDF)   saveFileName += ".pdf";
            //else                    saveFileName += ".jpeg";
            
            // 저장할 폴더가 없으면 생성
            DirectoryInfo di = new DirectoryInfo(Global.SavePath);
            if (di.Exists == false)                di.Create();

            this.PrintFileName = System.IO.Path.Combine(Global.SavePath, saveFileName);

            this.Cursor = Cursors.WaitCursor;
            alertFileProgress = new CustomClassLibrary.AlertProgressForm($"Print information is stored....");
            alertFileProgress.Show();
            SaveCount = 0;
            alertFileProgress.Message = string.Format("In progress, please wait...  ");
            alertFileProgress.ProgressValue = 0;

            this.IsFileWorkerCompleted = false;
            this.fileWorker.RunWorkerAsync();           // 파일 저장 BackgroundWorker 실행                    

            bool IsSavedOK = true;
            int ErrorCount = 0;
            int BackSaveCount = 0;
            while (!this.IsFileWorkerCompleted)
            {
                // 저장 진행사항 표시부분                            
                int progressValue = Convert.ToInt32((double)SaveCount / (double)SendTotalCount * 100);
                if (progressValue > 100) progressValue = 100;

                alertFileProgress.Message = string.Format("In progress, please wait... {0}/{1} ", SaveCount, SendTotalCount);
                alertFileProgress.ProgressValue = progressValue;

                if(BackSaveCount != SaveCount)
                {
                    ErrorCount    = 0;
                    BackSaveCount = SaveCount;
                }

                // SaveCount가 3초 이상 증가하지 않으면 TimeError
                if(ErrorCount > 30)
                {
                    IsSavedOK = false;
                    break;
                }

                System.Threading.Thread.Sleep(100);
            }

            alertFileProgress.ProgressValue = 100;
            alertFileProgress.Close();
            this.Cursor = Cursors.Default;


            if (IsSavedOK)
            {
                string fileNameMeg;
                if (Global.IsSavePDF) fileNameMeg = this.PrintFileName + ".pdf";
                else fileNameMeg = String.Format("{0}_*.jpeg", this.PrintFileName);

                MessageBox.Show(String.Format("The IRIS Prints information has been saved in the file({0}).", fileNameMeg));
                Close();
            }
            else
            {
                MessageBox.Show(String.Format("Failed to save. please try again."));
            }
        }

        // 파일 저장 함수
        private void FileWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.PrintFileName.Length == 0 && SendTotalCount == 0) return;

            //string strExt = Path.GetExtension(PrintFileName);
            string printImageName;
            if (Global.IsSavePDF)
            {
                printImageName = String.Format("{0}.pdf", this.PrintFileName);

                var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, (float)Global.MarginsLeft, (float)Global.MarginsRight, (float)Global.MarginsTop, (float)Global.MarginsBottom);
                using (var stream = new FileStream(printImageName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, stream);

                    document.Open();
                    System.Drawing.Image printImage = _preview.GetImage(0);    // 이미지가 A4 사이즈라서 크다.

                    for (int pIndex = 0; pIndex < _preview.PageCount; pIndex++)
                    {
                        // Print 제외는 저장하지 않는다.
                        if (!this.PrintIsPages[pIndex]) continue;

                        // 저장 진행사항 표시부분
                        SaveCount++;
                        int progressValue = Convert.ToInt32((double)SaveCount / (double)SendTotalCount * 100);
                        if (progressValue > 100) progressValue = 100;


                        printImage = _preview.GetImage(pIndex);    // 이미지가 A4 사이즈라서 크다.

                        byte[] imgByte;
                        Bitmap savedImage = ConvertTo24bpp(printImage);

                        using (MemoryStream memStream = new MemoryStream())
                        {
                            savedImage.Save(memStream, ImageFormat.Png);

                            imgByte = new byte[memStream.ToArray().Length];

                            Array.Copy(memStream.ToArray(), imgByte, imgByte.Length);
                        }

                        savedImage.Dispose();

                        var image = iTextSharp.text.Image.GetInstance(imgByte);

                        //iTextSharp.text.Image.GetInstance()
                        if (image.Height > iTextSharp.text.PageSize.A4.Height)
                        {
                            image.ScaleToFit(iTextSharp.text.PageSize.A4.Width, iTextSharp.text.PageSize.A4.Height);
                        }
                        else if (image.Width > iTextSharp.text.PageSize.A4.Width)
                        {
                            image.ScaleToFit(iTextSharp.text.PageSize.A4.Width, iTextSharp.text.PageSize.A4.Height);
                        }
                        image.Alignment = iTextSharp.text.Image.ALIGN_MIDDLE;

                        // PDF에 Print Page를 추가한다.
                        document.Add(image);

                        System.Threading.Thread.Sleep(10);
                    }

                    document.Close();
                }
            }
            else
            {
                for (int pIndex = 0; pIndex < _preview.PageCount; pIndex++)
                {
                    // Print 제외는 저장하지 않는다.
                    if (!this.PrintIsPages[pIndex]) continue;

                    // 저장 진행사항 표시부분
                    SaveCount++;
                    int progressValue = Convert.ToInt32((double)SaveCount / (double)SendTotalCount * 100);
                    if (progressValue > 100) progressValue = 100;

                    fileWorker.ReportProgress(progressValue);

                    Image printImage = _preview.GetImage(pIndex);

                    using (Bitmap savedImage = new Bitmap(printImage.Width, printImage.Height))
                    {
                        using (Graphics grp = Graphics.FromImage(savedImage))
                        {
                            grp.FillRectangle(Brushes.White, new Rectangle(0, 0, savedImage.Width, savedImage.Height));
                            grp.DrawImage(printImage, new Rectangle(0, 0, savedImage.Width, savedImage.Height));      // ImageInfo에 해당하는 Bitmap를 그린다.
                        }

                        printImageName = String.Format("{0}_{1}.jpeg", this.PrintFileName, pIndex + 1);

                        savedImage.Save(printImageName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }

            this.IsFileWorkerCompleted = true;
        }

        private void CoolPrintPreviewDialog_Load(object sender, EventArgs e)
        {
            //this.Button_Send.Enabled = Global.IsPaceSend;
        }

        // ImageBackground black/white Ckecked
        private void RadioButton_Black_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_Black.Checked)
            {
                this.radioButton_Black.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.radioButton_Black.ForeColor = Color.White;
                this.radioButton_White.Font = new Font("Segoe UI", 12);
                this.radioButton_White.ForeColor = Color.DarkGray;
            }
            else
            {
                this.radioButton_Black.Font = new Font("Segoe UI", 12);
                this.radioButton_Black.ForeColor = Color.DarkGray;
                this.radioButton_White.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.radioButton_White.ForeColor = Color.White;
            }

            this.radioButton_Black.Invalidate();
            this.radioButton_White.Invalidate();

            if (Global.Print_ImageBackColor_Black != this.radioButton_Black.Checked)
            {
                Global.Print_ImageBackColor_Black = this.radioButton_Black.Checked;

                // 바로 적용
                ImageButton_Apply_Click(sender, e);
            }
        }

        // Logo Show Checked 함수 처리
        private void CheckBox_LogoView_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_LogoView.Checked)
            {
                this.checkBox_LogoView.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_LogoView.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_LogoView.Font = new Font("Segoe UI", 12);
                this.checkBox_LogoView.ForeColor = Color.DarkGray;
            }

            this.imageButton_Logo_Load.Enabled = !this.checkBox_LogoView.Checked;

            this.checkBox_LogoView.Invalidate();

            Global.Print_LOGO_Show = this.checkBox_LogoView.Checked;

            // 바로 적용
            ImageButton_Apply_Click(sender, e);
        }

        // PaletteBar Show Checked 함수 처리
        private void CheckBox_PaletteBarView_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_PaletteBarView.Checked)
            {
                this.checkBox_PaletteBarView.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_PaletteBarView.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_PaletteBarView.Font = new Font("Segoe UI", 12);
                this.checkBox_PaletteBarView.ForeColor = Color.DarkGray;
            }

            this.checkBox_PaletteBarView.Invalidate();

            Global.Print_PaletteBar_Show = this.checkBox_PaletteBarView.Checked;

            // 바로 적용
            ImageButton_Apply_Click(sender, e);
        }

        // ROI Show Checked 함수 처리
        private void CheckBox_ROIView_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_ROIView.Checked)
            {
                this.checkBox_ROIView.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_ROIView.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_ROIView.Font = new Font("Segoe UI", 12);
                this.checkBox_ROIView.ForeColor = Color.DarkGray;
            }

            this.checkBox_ROIView.Invalidate();

            Global.Print_ROI_Show = this.checkBox_ROIView.Checked;

            // 바로 적용
            ImageButton_Apply_Click(sender, e);
        }

        // ROI의 차이를 보여줄지 처리하는 함수
        private void CheckBox_DiffROIDispaly_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_DiffROIDispaly.Checked)
            {
                this.checkBox_DiffROIDispaly.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_DiffROIDispaly.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_DiffROIDispaly.Font = new Font("Segoe UI", 12);
                this.checkBox_DiffROIDispaly.ForeColor = Color.DarkGray;
            }

            this.checkBox_DiffROIDispaly.Invalidate();

            Global.Print_Diff_ROI_Display = this.checkBox_DiffROIDispaly.Checked;

            // 바로 적용
            ImageButton_Apply_Click(sender, e);
        }

        // Print시 선택된 Study의 정보를 모두 인쇄할지 설정( true: 모두 인쇄 fasle: 현제 보는 정보 )
        private void CheckBox_PrintAllStudyInfo_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_PrintAllStudyInfo.Checked)
            {
                this.checkBox_PrintAllStudyInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_PrintAllStudyInfo.ForeColor = Color.White;
            }
            else
            {
                this.checkBox_PrintAllStudyInfo.Font = new Font("Segoe UI", 12);
                this.checkBox_PrintAllStudyInfo.ForeColor = Color.DarkGray;
            }

            Global.Print_StudyAll = this.checkBox_PrintAllStudyInfo.Checked;

            this.Panel_PrintRange.Visible = Global.Print_StudyAll;
            this.checkBox_PrintPage.Visible = Global.Print_StudyAll;
            this.checkBox_PrintPage.Invalidate();

            this.checkBox_PrintAllStudyInfo.Invalidate();

            // 바로 적용
            ImageButton_Apply_Click(sender, e);
        }

        // 해당 page를 Print할지 설정
        private void CheckBox_PrintPage_CheckedChanged(object sender, EventArgs e)
        {
            // 해당 pajge를 Print할지 설정 코드
            if (_preview.StartPage >= 0 && _preview.StartPage < this.PrintIsPages.Count)
            {
                this.PrintIsPages[_preview.StartPage] = this.checkBox_PrintPage.Checked;
            }

            if (this.checkBox_PrintPage.Checked)
            {
                this.checkBox_PrintPage.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.checkBox_PrintPage.ForeColor = Color.White;
                int pageCount = 0;
                foreach (bool print in this.PrintIsPages) if (print) pageCount++;

                if (pageCount == 1) this.radioButton_CurrentPage.Checked = true;   // 프린트 선택한 것이 한장이면 현재 페이지 인쇄설정
                else if (pageCount == this.PrintIsPages.Count) this.radioButton_All.Checked = true;   // 프린트 선택한 것이 모두이면 이면 전체 페이지 인쇄설정
                else this.radioButton_Pages.Checked = true;   // 페이지 범위 인쇄 설정
            }
            else
            {
                this.checkBox_PrintPage.Font = new Font("Segoe UI", 12);
                this.checkBox_PrintPage.ForeColor = Color.DarkGray;

                this.radioButton_Pages.Checked = true;
            }

            // 표시
            SetTextBox_PrintRange();

        }

        // 로고 이미지 변경 함수
        private void ImageButton_Logo_Load_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to change the Logo Image to use for printing?", "Change the Logo Image", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                OpenFileDialog fileOpen = new OpenFileDialog
                {
                    Title = "Image File Open",
                    Filter = "All Files (*.*) | *.*"
                };

                //파일 오픈창 로드
                DialogResult dr = fileOpen.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    string fileNmae = fileOpen.SafeFileName;
                    string fileFullname = fileOpen.FileName;

                    try
                    {
                        Bitmap logoImg = null;
                        using (var fileStream = new FileStream(fileFullname, FileMode.Open, FileAccess.Read))
                        {
                            logoImg = new Bitmap(fileStream);
                            fileStream.Close();
                        }

                        customPictureBox_LOGO.BackgroundImage = logoImg;
                        customPictureBox_LOGO.Invalidate();

                        Bitmap NewImg = new Bitmap(logoImg);
                        logoImg.Dispose();
                        logoImg = null;

                        // Logo 이미지 저장한다.
                        String imgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("LOGO.bmp"));

                        NewImg.Save(imgPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }

        // Print를 PACS에 Send한 마지막 ImageIndex 가지고 온다.
        private int LoadPacsSendPrintIndex(ResultInfo study)
        {
            int LastIndex = 1;
            //if (study != null && study.listImageInfo.Count > 0)
            //{
            //    LastIndex = study.listImageInfo.Count;

            //    string studyInfoFileName = String.Format("{0}\\Images\\{1:0000}\\{2}_StudyInfo.ini", Application.StartupPath, study.patient.PID, study.studyInstanceUID);

            //    if (File.Exists(studyInfoFileName))
            //    {

            //        FileIniDataParser parser = new FileIniDataParser();
            //        IniData studyInfoData = parser.ReadFile(studyInfoFileName);
            //        // Study에서 보낸 Print의 마지막 Index를 읽어오기 

            //        try { if (studyInfoData["STUDY_INFO"]["PRINT_PACSSEND_LASTINDEX"] != null) LastIndex = Convert.ToInt32(studyInfoData["STUDY_INFO"]["PRINT_PACSSEND_LASTINDEX"]); } catch { }
            //    }
            //}
            return LastIndex;
        }

        // Print를 PACS에 Send한 마지막 Index를 저장한다.
        private void SavePaceSendPrintIndex(ResultInfo study, int LastIndex)
        {
            //if (study != null && study.listImageInfo.Count > 0)
            //{
            //    string sDirPath;
            //    sDirPath = String.Format("{0}\\Images\\{1:0000}", Application.StartupPath, study.patient.PID);
            //    DirectoryInfo di = new DirectoryInfo(sDirPath);
            //    if (di.Exists == false)     // 환자 ID에 해당하는 폴더가 없으면.
            //    {
            //        return;
            //    }

            //    if (LastIndex < study.listImageInfo.Count) LastIndex = study.listImageInfo.Count;

            //    string studyInfoFileName = String.Format("{0}\\Images\\{1:0000}\\{2}_StudyInfo.ini", Application.StartupPath, study.patient.PID, study.studyInstanceUID);

            //    //ini the file path            
            //    FileIniDataParser parser = new FileIniDataParser();
            //    IniData studyInfoData = new IniData();

            //    studyInfoData.Sections.AddSection("STUDY_INFO");
            //    studyInfoData["STUDY_INFO"]["PRINT_PACSSEND_LASTINDEX"] = LastIndex.ToString();

            //    parser.WriteFile(studyInfoFileName, studyInfoData);
            //}
        }

        // 프린트 Send 함수
        private void ImageButton_Send_Click(object sender, EventArgs e)
        {
//            var serverIP = Global.SEND_IPAdress;               // PACS Send Server IP
//            var serverPort = Global.SEND_PortNumber;             // PACS Send Server PortNumber
//            var serverAET = Global.SEND_AE_Title;               // PACS Send Server AE Title
//            var clientAET = Global.SEND_Registered_AE_Title;    // PACS Send Server 등록된 Client AE Title

//            var IsPaceSend = Global.IsPaceSend;

//            // 저장할 파일 총 갯수 구하기
//            int SendTotalCount = this.PrintIsPages.FindAll(p => p == true).Count;

//            // 저장할 파일이 없으면
//            if (SendTotalCount <= 0)
//            {
//                MessageBox.Show("No Print selected information.");
//                return;
//            }

//            // Pacs 설정 확인
//            if (!IsPaceSend || (IPAddress.TryParse(serverIP, out _) == false || serverPort < 0 || serverAET.Length <= 0 || clientAET.Length <= 0))
//            {
//                return;
//            }

//            if (Global.listResultInfos != null && Global.listResultInfos.Count != 0 && Global.selectedStudyIndex < Global.listResultInfos.Count && Global.selectedStudyIndex >= 0)
//            {
//                StudyInfo study = Global.listResultInfos[Global.selectedStudyIndex]; // 선택된 Study Info   

//                if (study.listImageInfo.Count <= 0) return;
                
//                bool IsSendOK = true;
//                this.Cursor = Cursors.WaitCursor;

//                int SendCount = 0;
//                AlertProgressForm alertProgress = new AlertProgressForm(Title: "Print Send..");
//                alertProgress.Show();

//                // Print 마지막 Index 가지고 오기
//                int LastIndex = LoadPacsSendPrintIndex(study);

//                string strError;
//                List<int> iResList = new List<int>();
//                for (int pIndex = 0; pIndex < _preview.PageCount; pIndex++)
//                {
//                    // Print 제외는 Send하지 않는다.
//                    if (!this.PrintIsPages[pIndex]) continue;      

//                    LastIndex += pIndex + 1;
//                    DicomDataset dataset = new DicomDataset();          // Dicom Dataset를 생성한다.

//                    InfoFillDataset(dataset, study, LastIndex);  // image Index는 1부터 시작이다. 

//#region Bitmap을 만들어서 Dicom Dataset에 저장한다.    
//                    System.Drawing.Image printImage = _preview.GetImage(pIndex);    // 이미지가 A4 사이즈라서 크다.

//                    // PCAS 전송용 이미지로 만든다.
//                    double radio = 1.0 / 4.0;
//                    OpenCvSharp.Mat sendMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(ConvertTo24bpp(printImage, radio)).CvtColor(OpenCvSharp.ColorConversionCodes.BGR2RGB);

//                    // 이미지가 A4 사이즈라서 크다. 그래서 1/4로 줄려서 보낸다.
//                    //double zoomWidth = orgMat.Width / 4;
//                    //double zoomHeight = orgMat.Height / 4;
//                    //OpenCvSharp.Mat sendMat = orgMat.Resize(new OpenCvSharp.Size(zoomWidth, zoomHeight), 0, 0, OpenCvSharp.InterpolationFlags.Cubic).CvtColor(OpenCvSharp.ColorConversionCodes.BGR2RGB);

//                    int rows = sendMat.Rows;
//                    int columns = sendMat.Cols;

//                    // buffer에 pixels정보를 저장한다.
//                    byte[] pixels = new byte[columns * rows * 3];
//                    Marshal.Copy(sendMat.Data, pixels, 0, pixels.Length);

//                    sendMat.Dispose();

//                    MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);
//                    // 이미지 정보 Dicom에 저장한다.
//                    dataset.Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
//                    dataset.Add(DicomTag.Rows, (ushort)rows);
//                    dataset.Add(DicomTag.Columns, (ushort)columns);
//                    dataset.Add(DicomTag.BitsAllocated, (ushort)8);                                     // MC_ATT_BITS_ALLOCATED, 8

//                    DicomPixelData pixelData = DicomPixelData.Create(dataset, true);
//                    pixelData.BitsStored = 8;
//                    pixelData.SamplesPerPixel = 3;
//                    pixelData.HighBit = 7;
//                    pixelData.PixelRepresentation = 0;
//                    pixelData.PlanarConfiguration = 0;
//                    pixelData.AddFrame(buffer);                                                          // 저장할 이미지 버퍼 
//#endregion Bitmap을 만들어서 그림그리고 palette와 온도 값 추가해서 Dicom Dataset에 저장한다.

//                    DicomFile dicomfile = new DicomFile(dataset);

//                    SendCount++;
//                    int progressValue = Convert.ToInt32((double)SendCount / (double)SendTotalCount * 100);
//                    if (progressValue > 100) progressValue = 100;
//                    alertProgress.Message = string.Format("In progress, please wait... {0}/{1} ", SendCount, SendTotalCount);
//                    alertProgress.ProgressValue = progressValue;

//                    int iRes = Global.PACSSend(serverIP, serverPort, serverAET, clientAET, dicomfile, 20000);
//                    if (iRes != 0)
//                    {
//                        // 전송 실패시 계속 진행할 것인지를 묻는다. 
//                        strError = string.Format("Server Not Found.{0}\r\nWould you like to continue?", serverIP);
//                        if (MessageBox.Show(strError, "YesOrNo", MessageBoxButtons.YesNo) == DialogResult.No)
//                        {
//                            alertProgress.Close();
//                            return;
//                        }

//                        iResList.Add(iRes);
//                        IsSendOK = false;
//                    }
//                }

//                // Print 마지막 Index 저장하기 오기
//                SavePaceSendPrintIndex(study, LastIndex);

//                alertProgress.Close();

//                this.Cursor = Cursors.Default;

//                if (IsSendOK) MessageBox.Show("The transfer to the PACS server was successful.");
//                else
//                {
//                    String ErrCode = " ";
//                    foreach (int iRes in iResList) ErrCode += iRes.ToString() + " ";
//                    MessageBox.Show($"The transfer to the PACS server was failed. ({ErrCode})");
//                }
//            }
        }

        // Dicom에 기본 정보를 넣는다.
        //private void InfoFillDataset(DicomDataset dataset, StudyInfo study, int ImageIndex = 1)
        //{
        //    if (dataset == null || study == null || study.patient == null) return;

        //    string strSex = "M";
        //    if (study.patient.Gender == 1) strSex = "M";       // 남자
        //    else strSex = "F";       // 여자            

        //    if (study.patient.PatientComments == null) study.patient.PatientComments = "";

        //    string strImageUID = String.Format("{0}.1.{1}", study.studyInstanceUID, ImageIndex);

        //    dataset.Add(DicomTag.PatientID, Encoding.Default, String.Format("{0}", study.patient.ChartNo));     // 환자 ChartNo            
        //    dataset.Add(DicomTag.PatientName, Encoding.Default, String.Format("{0}", study.patient.Name));     // 환자 이름

        //    dataset.Add(DicomTag.PatientSex, strSex);                                   // 환자 성별            
        //    dataset.Add(DicomTag.PatientBirthDate, study.patient.GetBirthday());              // 환자 생년월일
        //    dataset.Add(DicomTag.PatientAge, string.Format("{0:000}Y", study.patient.GetAge()));   // 환자 나이

        //    dataset.Add(DicomTag.InstitutionalDepartmentName, study.patient.PatientDepartment); // // 담당과
        //    dataset.Add(DicomTag.StudyID, study.StudyID.ToString());                     // MC_ATT_STUDY_ID, (?)

        //    dataset.Add(DicomTag.StudyDate, study.StudyDateTime.ToLocalTime());          // Study 시작 날짜
        //    dataset.Add(DicomTag.StudyTime, study.StudyDateTime.ToLocalTime());          // Study 시작 시간

        //    //dataset.Add(DicomTag.ContentDate, study.StudyDateTime.ToLocalTime());        // Image 생성 날짜를 Study 시작 날짜로 입력
        //    //dataset.Add(DicomTag.ContentTime, study.StudyDateTime.ToLocalTime());        // Image 생성 시간를 Study 시작 시간로 입력

        //    dataset.Add(DicomTag.ContentDate, DateTime.Now.ToLocalTime());                  // Image 생성 날짜를 Print 보내는 날짜로 입력
        //    dataset.Add(DicomTag.ContentTime, DateTime.Now.ToLocalTime());                  // Image 생성 시간를 Print 보내는 날짜로 입력

        //    dataset.Add(DicomTag.StudyDescription, Encoding.Default, study.StudyDescription);              // Study Description
        //    dataset.Add(DicomTag.AccessionNumber, study.AccessionNumber);                // Accession Number

        //    dataset.Add(DicomTag.InstitutionName, Encoding.Default, Global.INSTITUTION_NAME);             // 병원명
        //    dataset.Add(DicomTag.AdmittingDiagnosesDescription, "");                    // MC_ATT_ADMITTING_DIAGNOSIS_DESCRIPTION, (?) strDescription, 진단 설명인데 추가 여부???
        //    dataset.Add(DicomTag.BodyPartExamined, "");                                 // MC_ATT_BODY_PART_EXAMINED, TO_PCHAR(psi->strStudyBodyPartExamined));
        //    dataset.Add(DicomTag.ReferringPhysicianName, study.patient.PatientRefDr);         // Patient 담당의사            

        //    dataset.Add(DicomTag.PatientComments, Encoding.Default, study.patient.PatientComments);      // Patient Comments

        //    dataset.Add(DicomTag.StudyInstanceUID, study.studyInstanceUID);              // StudyInstanceUID
        //    dataset.Add(DicomTag.SeriesNumber, "1");                                    // Study ID에 따른 Series 번호 ( 1로 고정한다.)

        //    dataset.Add(DicomTag.SeriesDate, study.StudyDateTime.ToLocalTime());         // MC_ATT_SERIES_DATE, Study 시작 날짜
        //    dataset.Add(DicomTag.SeriesTime, study.StudyDateTime.ToLocalTime());         // MC_ATT_SERIES_TIME, Study 시작 시간

        //    dataset.Add(DicomTag.Manufacturer, Encoding.Default, Global.MANUFACTURER);  // 제조사 명
        //    dataset.Add(DicomTag.ManufacturerModelName, Global.MANUFACTURERMODELNAME);  // 제품 명
        //    dataset.Add(DicomTag.StationName, "");                                      // MC_ATT_STATION_NAME, TO_PCHAR(psi->strStation));
        //    dataset.Add(DicomTag.Modality, Global.MODALITY);                            // MC_ATT_MODALITY, Modality
        //    dataset.Add(DicomTag.SeriesInstanceUID, study.studyInstanceUID);             // MC_ATT_SERIES_INSTANCE_UID, StudyInstanceUID

        //    dataset.Add(DicomTag.PatientOrientation, "");                               // MC_ATT_PATIENT_ORIENTATION, "");
        //    dataset.Add(DicomTag.InstanceNumber, ImageIndex);                           // Image Index
        //    dataset.Add(DicomTag.SOPClassUID, Global.DICOM_SOP_CLASS_UID_SC);               // SOPClassUID
        //    dataset.Add(DicomTag.MediaStorageSOPClassUID, Global.DICOM_SOP_CLASS_UID_SC);   // MEDIA_STORAGE_SOP_CLASS_UID, SOPClassUID));

        //    dataset.Add(DicomTag.SOPInstanceUID, strImageUID);                          // MC_ATT_SOP_INSTANCE_UID, studyInstanceUID.ImageIndex
        //    dataset.Add(DicomTag.MediaStorageSOPInstanceUID, strImageUID);              // MEDIA_STORAGE_SOP_INSTANCE_UID, TO_PCHAR(psi->strSOPInstanceUID));

        //    dataset.Add(DicomTag.ConversionType, "DV");                                 // MC_ATT_CONVERSION_TYPE, "DV"

        //    dataset.Add(DicomTag.ImageComments, Encoding.Default, "");    // MC_ATT_IMAGE_COMMENTS, TO_PCHAR(CommentBuf)); //Annotation

        //    dataset.Add(DicomTag.FileMetaInformationVersion, new Byte[2] { 0, 1 });     // MC_ATT_FILE_META_INFORMATION_VERSION, (void*)1, WriteVersionInfo);
        //    dataset.Add(DicomTag.SourceApplicationEntityTitle, "MEDICORE");             // MEDIA_SOURCE_APPLICATION_ENTITY, "MEDICORE");

        //    dataset.Add(DicomTag.ImplementationClassUID, Global.IMPLEMENTATIONCLASSUID);
        //    dataset.Add(DicomTag.ImplementationVersionName, Global.IMPLEMENTATIONVERSIONNAME);
        //}

        // Image를 24bit Bitmap으로 변경(Dicom은 24bit RGB사용함)
        private Bitmap ConvertTo24bpp(Image img, double radio = 1.0)
        {
            if (radio <= 0) radio = 1;

            int zoomWidth  = Convert.ToInt32(img.Width * radio);
            int zoomHeight = Convert.ToInt32(img.Height * radio);

            var bmp = new Bitmap(zoomWidth, zoomHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
                gr.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height));
            }

            return bmp;
        }

        private void RadioButton_All_CheckedChanged(object sender, EventArgs e)
        {
            this.checkBox_PrintPage.Enabled = true;
            if (this.radioButton_All.Checked)                    // 모두 인쇄
            {
#region 라이오버튼 모양 설정
                this.radioButton_All.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.radioButton_All.ForeColor = Color.White;
                this.radioButton_CurrentPage.Font = new Font("Segoe UI", 12);
                this.radioButton_CurrentPage.ForeColor = Color.DarkGray;
                this.radioButton_Pages.Font = new Font("Segoe UI", 12);
                this.radioButton_Pages.ForeColor = Color.DarkGray;
#endregion
                for (int page = 0; page < this.PrintIsPages.Count; page++) this.PrintIsPages[page] = true;   // 모두 인쇄로 설정
                this.checkBox_PrintPage.Checked = true;

            }
            else if (this.radioButton_CurrentPage.Checked)       // 현재 페이지 인쇄
            {
#region 라이오버튼 모양 설정
                this.radioButton_All.Font = new Font("Segoe UI", 12);
                this.radioButton_All.ForeColor = Color.DarkGray;
                this.radioButton_CurrentPage.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.radioButton_CurrentPage.ForeColor = Color.White;
                this.radioButton_Pages.Font = new Font("Segoe UI", 12);
                this.radioButton_Pages.ForeColor = Color.DarkGray;
#endregion

                this.checkBox_PrintPage.Enabled = false;

                for (int page = 0; page < this.PrintIsPages.Count; page++)
                {
                    if (page == _preview.StartPage) this.PrintIsPages[page] = true;
                    else this.PrintIsPages[page] = false;
                }

                this.checkBox_PrintPage.Checked = true;
            }
            else                                                // 인쇄범위 설정 인쇄
            {
#region 라이오버튼 모양 설정
                this.radioButton_All.Font = new Font("Segoe UI", 12);
                this.radioButton_All.ForeColor = Color.DarkGray;
                this.radioButton_CurrentPage.Font = new Font("Segoe UI", 12);
                this.radioButton_CurrentPage.ForeColor = Color.DarkGray;
                this.radioButton_Pages.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                this.radioButton_Pages.ForeColor = Color.White;
#endregion
            }

            this.radioButton_All.Invalidate();
            this.radioButton_CurrentPage.Invalidate();
            this.radioButton_Pages.Invalidate();

            this.textBox_PrintRange.Enabled = this.radioButton_Pages.Checked;
            SetTextBox_PrintRange();
        }

        // 인쇄 범위 표시하기
        private void SetTextBox_PrintRange()
        {
            if (this.radioButton_All.Checked) this.textBox_PrintRange.Text = $"1-{_preview.PageCount}";
            else
            {
                string strPrintRage = "";
                string subRanage = "";
                int count = 0;
                int start = 0, end = 0;

                for (int page = 0; page < this.PrintIsPages.Count; page++)
                {
                    if (this.PrintIsPages[page])
                    {
                        subRanage += $"{page + 1},";
                        if (count == 0) start = (page + 1);
                        end = (page + 1);
                        count++;
                    }
                    else
                    {
                        if (count > 0)
                        {
                            if (count > 2) subRanage = $"{start}-{end},";

                            strPrintRage += subRanage;
                        }

                        subRanage = "";
                        count = 0;
                        start = 0;
                        end = 0;

                    }
                }
                // 마지막 처리 안된 것 처리
                if (count > 0)
                {
                    if (count > 2) subRanage = $"{start}-{end},";

                    strPrintRage += subRanage;
                }

                // 마지막에 쉼표(,)가 들어가기 때문에 삭제한다.
                if (strPrintRage.Length > 0)
                    this.textBox_PrintRange.Text = strPrintRage.Substring(0, strPrintRage.Length - 1);
                else
                    this.textBox_PrintRange.Text = "";
            }
        }

        string SavePringRange = "";
        // Print Range TextBox에 들어올때 값을 저장한다.
        private void TextBox_PrintRange_Enter(object sender, EventArgs e)
        {
            this.SavePringRange = this.textBox_PrintRange.Text;
            this.textBox_PrintRange.SelectAll();
        }

        //Print Range TextBox 입력이 끝나고 난 뒤 처리함수
        private void TextBox_PrintRange_Validating(object sender, CancelEventArgs e)
        {
            CommitPritneRangeNumber();
        }

        // Print Range TextBox에 Key가 눌렸을 경우 처리 
        private void TextBox_PrintRange_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            if (c == (char)13)              // Enter 키 누를 경우 처리합수
            {
                CommitPritneRangeNumber();
                e.Handled = true;
            }
            else if (c != (char)8 && c != ',' && c != '-' && !char.IsDigit(c))
            {
                // 숫자, ',', '-', Backspace 키 외에는 입력이 안되게 처리
                e.Handled = true;
            }
        }

        // 인쇄범위 입력시 입력값 확인 및 처리 함수
        private void CommitPritneRangeNumber()
        {
            string PrintPageNumber = this.textBox_PrintRange.Text;

            if (this.SavePringRange != PrintPageNumber)
            {
                bool IsError = false;
                List<int> printPageNumbers = new List<int>();
                if (PrintPageNumber.Contains(",") || PrintPageNumber.Contains("-"))
                {
                    string[] commaStrings = PrintPageNumber.Split(',');
                    foreach (string commaString in commaStrings)
                    {
                        string[] dashstrings = commaString.Split('-');

                        if (dashstrings.Length == 2)
                        {
                            if (int.TryParse(dashstrings[0], out int first) && int.TryParse(dashstrings[1], out int second))
                            {
                                for (int Index = Math.Min(first, second); Index <= Math.Max(first, second); Index++)
                                {
                                    if (Index > 0 && Index < this.PrintIsPages.Count + 1)
                                        printPageNumbers.Add(Index);
                                }
                            }
                            else { IsError = true; break; }
                        }
                        else if (dashstrings.Length == 1)
                        {
                            if (int.TryParse(dashstrings[0], out int result))
                            {
                                if (result > 0 && result < this.PrintIsPages.Count + 1)
                                    printPageNumbers.Add(result);
                            }
                            else { IsError = true; break; }

                        }
                    }
                }
                else if (int.TryParse(PrintPageNumber, out int page))
                {
                    printPageNumbers.Add(page);
                }
                else { IsError = true; }

                if (IsError)
                {
                    MessageBox.Show("The printing range is wrong.");
                    this.textBox_PrintRange.Text = this.SavePringRange;
                }
                else
                {
                    //  중복 제거
                    printPageNumbers = printPageNumbers.Distinct().ToList();

                    for (int page = 0; page < this.PrintIsPages.Count; page++)
                    {
                        int IgnorePageIndex = printPageNumbers.FindIndex(i => i == (page + 1));

                        if (IgnorePageIndex >= 0) this.PrintIsPages[page] = true;
                        else this.PrintIsPages[page] = false;
                    }

                    this.checkBox_PrintPage.Checked = this.PrintIsPages[_preview.StartPage];
                    SetTextBox_PrintRange();
                }
            }

            this.SavePringRange = this.textBox_PrintRange.Text;
        }
    }
}
