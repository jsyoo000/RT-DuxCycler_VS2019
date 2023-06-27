// -------------------------------
// Color Selection Button
// Version 1.1
// -------------------------------
// Author: Thomas Ascher
// eMail: thomasascher@hotmail.com
// -------------------------------

using System ;
using System.Drawing ;
using System.ComponentModel ;
using System.Windows.Forms ;
using Duxcycler_GLOBAL;

namespace Duxcycler
{
    public class DataGridViewColorButtonCell : DataGridViewButtonCell
    {
        private System.ComponentModel.Container components = null ;

        //private Color buttonColor = Color.Transparent ;
        private string autoButton = "Automatic" ;
        private string moreButton = "More Colors..." ;
        private bool buttonPushed = false ;
        private bool panelVisible = false ;
        private int colorIndex = 0;
        private int plateType = 0;
        public MainPage pParentMain = null;

        public event EventHandler Changed ;

        public Color ColorVaule
        {
            get { return Global.colorList[colorIndex] ; }
            //set { Global.colorList[colorIndex] = value ; }
        }

        public int ColorIndex
        {
            get { return colorIndex; } set { colorIndex = value; }
        }

        // Plate Type (0:Target, 1:Sample, 2:Bio Group)
        public int PlateType
        {
            get { return plateType; }
            set { plateType = value; }
        }

        public string Automatic
        {
            get { return autoButton ; } set { autoButton = value ; }
        }

        public string MoreColors
        {
            get { return moreButton ; } set { moreButton = value ; }
        }

        protected virtual void OnChanged(object sender, EventArgs e)
        {
            //if( Changed != null )
            //    Changed(sender, e) ;

            // Bitmap ∞¥√º √ ±‚»≠
            Bitmap bitmap = new Bitmap(1000, 800, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Graphics graphics = Graphics.FromImage(bitmap);

            Rectangle cellBounds = new Rectangle();
            Rectangle ColorBoxRect = new Rectangle();
            RectangleF TextBoxRect = new RectangleF();
            GetDisplayLayout(cellBounds, ref ColorBoxRect, ref TextBoxRect);

            // Draw the cell background, if specified.
            SolidBrush cellBackground = new SolidBrush(ColorVaule);
            graphics.FillRectangle(cellBackground, ColorBoxRect);
            graphics.DrawRectangle(Pens.Black, ColorBoxRect);
            //Color lclcolor = (Color)value;
            //graphics.DrawString(lclcolor.Name.ToString(), cellStyle.Font, System.Drawing.Brushes.Black, TextBoxRect);

            cellBackground.Dispose();

            pParentMain.TargetColorChanged(PlateType, colorIndex, ColorVaule);
        }

        public DataGridViewColorButtonCell()
        {
            InitializeComponent() ;

            this.Changed += new System.EventHandler(OnChanged);
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if( components != null )
                    components.Dispose() ;
            }

            base.Dispose( disposing ) ;
        }

        #region Vom Komponenten-Designer generierter Code
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container() ;
        }
        #endregion

        protected virtual void GetDisplayLayout(Rectangle CellRect, ref Rectangle colorBoxRect, ref RectangleF textBoxRect)
        {
            const int DistanceFromEdge = 2;

            colorBoxRect.X = CellRect.X + DistanceFromEdge;
            colorBoxRect.Y = CellRect.Y + 1;
            colorBoxRect.Size = new Size((int)(1.5 * 17), CellRect.Height - (2 * DistanceFromEdge));

            // The text occupies the middle portion.
            textBoxRect = RectangleF.FromLTRB(colorBoxRect.X + colorBoxRect.Width + 5, colorBoxRect.Y + 2, CellRect.X + CellRect.Width - DistanceFromEdge, colorBoxRect.Y + colorBoxRect.Height);
        }

        //const float phi = 1.618f;
        protected override void Paint(Graphics graphics,
                                        Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
                                        DataGridViewElementStates elementState, object value,
                                        object formattedValue, string errorText,
                                        DataGridViewCellStyle cellStyle,
                                        DataGridViewAdvancedBorderStyle advancedBorderStyle,
                                        DataGridViewPaintParts paintParts)
        {
            //base.OnPaint( e ) ;
            //formattedValue = null;

            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue,
                        errorText, cellStyle, advancedBorderStyle, paintParts);

            Rectangle ColorBoxRect = new Rectangle();
            RectangleF TextBoxRect = new RectangleF();
            GetDisplayLayout(cellBounds, ref ColorBoxRect, ref TextBoxRect);

            // Draw the cell background, if specified.
            if ((paintParts & DataGridViewPaintParts.Background) ==
                DataGridViewPaintParts.Background)
            {
                SolidBrush cellBackground = new SolidBrush(ColorVaule);
                graphics.FillRectangle(cellBackground, ColorBoxRect);
                graphics.DrawRectangle(Pens.Black, ColorBoxRect);

                cellBackground.Dispose();
            }
        }

        public void ClickHandler(Point pt, MainPage pParent)
        {
            pParentMain = pParent;
            //Point pt = PointToScreen(new Point(Left, Bottom));

            //Point pt = new Point(0, 0);
            ColorPanel panel = new ColorPanel(pt, this);
            panel.Show();
            //ColorVaule = panel.colorButton.ColorVaule;
        }

        //protected override void OnMouseDown( MouseEventArgs e )
        //{
        //    buttonPushed = true ;
        //    base.OnMouseDown( e ) ;
        //}

        //protected override void OnMouseUp( MouseEventArgs e )
        //{
        //    buttonPushed = false ;
        //    base.OnMouseUp( e ) ;
        //}

        //protected override void OnClick( EventArgs e )
        //{
        //    panelVisible = true ;
        //    Refresh() ;

        //    Point pt = Parent.PointToScreen( new Point( Left, Bottom ) ) ;

        //    ColorPanel panel = new ColorPanel( pt, this ) ;
        //    panel.Show() ;
        //}

        public class ColorPanel : System.Windows.Forms.Form
        {
            public DataGridViewColorButtonCell colorButton;
            private int colorIndex = -1 ;
            // ADDED: Ignazio Di Napoli - neclepsio@hotmail.com
            private int keyboardIndex = -50 ;
            // END ADDED

            public ColorPanel( Point pt, DataGridViewColorButtonCell button )
            {
                colorButton = button ;

                FormBorderStyle = FormBorderStyle.FixedDialog ;
                MinimizeBox = false ;
                MaximizeBox = false ;
                ControlBox = false ;
                ShowInTaskbar = false ;
                TopMost = true ;

                SetStyle( ControlStyles.DoubleBuffer, true ) ;
                SetStyle( ControlStyles.UserPaint, true ) ;
                SetStyle( ControlStyles.AllPaintingInWmPaint, true ) ;

                Width = 156 ;
                Height = 100 ;

                if( colorButton.autoButton != "" )
                    Height += 23 ;
                if( colorButton.moreButton != "" )
                    Height += 23 ;

                CenterToScreen() ;
                Location = pt ;

                Capture = true ;
            }

            protected override void OnClosed( EventArgs e )
            {
                base.OnClosed( e ) ;

                colorButton.panelVisible = false ;
                //colorButton.Refresh() ;
            }

            protected override void OnPaint( PaintEventArgs e )
            {
                base.OnPaint( e ) ;

                Pen darkPen = new Pen( SystemColors.ControlDark ) ;
                Pen lightPen = new Pen( SystemColors.ControlLightLight ) ;
                SolidBrush lightBrush = new SolidBrush( SystemColors.ControlLightLight ) ;
                bool selected = false ;
                //int x = 6, y = 5 ;
                int x = 6, y = 25;

                //if( colorButton.autoButton != "" )
                //{
                //    selected = colorButton.ColorVaule == Color.Transparent ;
                //    DrawButton( e, x, y, colorButton.autoButton, 100, selected ) ;
                //    y += 23 ;
                //}

                for ( int i = 0 ; i < 40 ; i++ )
                {
                    if( colorButton.ColorVaule.ToArgb() == Global.colorList[i].ToArgb() )
                        selected = true ;

                    if( colorIndex == i )
                    {
                        e.Graphics.DrawRectangle( lightPen, x - 3, y - 3, 17, 17 ) ;
                        e.Graphics.DrawLine( darkPen, x - 2, y + 14, x + 14, y + 14 ) ;
                        e.Graphics.DrawLine( darkPen, x + 14, y - 2, x + 14, y + 14 ) ;
                    }
                    else if( colorButton.ColorVaule.ToArgb() == Global.colorList[i].ToArgb() )
                    {
                        // ADDED: Ignazio Di Napoli - neclepsio@hotmail.com
                        if( keyboardIndex == -50 )
                            keyboardIndex = i ;
                        // END ADDED

                        e.Graphics.FillRectangle( lightBrush, x - 3, y - 3, 18, 18 ) ;
                        e.Graphics.DrawLine( darkPen, x - 3, y - 3, x + 13, y - 3 ) ;
                        e.Graphics.DrawLine( darkPen, x - 3, y - 3, x - 3, y + 13 ) ;
                    }

                    e.Graphics.FillRectangle( new SolidBrush(Global.colorList[i] ), x, y, 11, 11 ) ;
                    e.Graphics.DrawRectangle( darkPen, x, y, 11, 11 ) ;

                    if( ( i + 1 ) % 8 == 0 )
                    {
                        x = 6 ;
                        y += 18 ;
                    }
                    else
                        x += 18 ;
                }

                if( colorButton.moreButton != "" )
                    DrawButton( e, x, y, colorButton.moreButton, 101, ! selected ) ;
            }

            // ADDED: Ignazio Di Napoli - neclepsio@hotmail.com
            protected override void OnKeyDown( KeyEventArgs e )
            {
                if( e.KeyCode == Keys.Escape )
                    Close() ;
                else if( e.KeyCode == Keys.Left )
                    MoveIndex( -1 ) ;
                else if( e.KeyCode == Keys.Up )
                    MoveIndex( -8 ) ;
                else if( e.KeyCode == Keys.Down )
                    MoveIndex( +8 ) ;
                else if( e.KeyCode == Keys.Right )
                    MoveIndex( +1 ) ;
                else if( e.KeyCode == Keys.Enter ||
                         e.KeyCode == Keys.Space )
                    OnClick( EventArgs.Empty ) ;
                else
                    base.OnKeyDown( e ) ;
            }

            private void MoveIndex( int delta )
            {
                int lbound = ( colorButton.autoButton != "" ? -8 : 0 ) ;
                int ubound = 39 + ( colorButton.moreButton != "" ? 8 : 0 ) ;
                int d = ubound - lbound + 1 ;

                if( delta == -1 && keyboardIndex < 0 )
                    keyboardIndex = ubound ;
                else if( delta == 1 && keyboardIndex > 39 )
                    keyboardIndex = lbound ;
                else if( delta == 1 && keyboardIndex < 0 )
                    keyboardIndex = 0 ;
                else if( delta == -1 && keyboardIndex > 39 )
                    keyboardIndex = 39 ;
                else
                    keyboardIndex += delta ;

                if( keyboardIndex < lbound )
                    keyboardIndex += d ;
                if( keyboardIndex > ubound )
                    keyboardIndex -= d ;

                if( keyboardIndex < 0 )
                    colorIndex = 100 ;
                else if( keyboardIndex > 39 )
                    colorIndex = 101 ;
                else
                    colorIndex = keyboardIndex ;

                Refresh() ;
            }
            // END ADDED

            protected override void OnMouseDown( MouseEventArgs e )
            {
                if( RectangleToScreen( ClientRectangle ).Contains( Cursor.Position ) )
                    base.OnMouseDown( e ) ;
                else
                    Close() ;
            }

            protected override void OnMouseMove( MouseEventArgs e )
            {
                base.OnMouseMove( e ) ;

                if( RectangleToScreen( ClientRectangle ).Contains( Cursor.Position ) )
                {
                    Point pt = PointToClient( Cursor.Position ) ;
                    int x = 6, y = 5 ;

                    if( colorButton.autoButton != "" )
                    {
                        if( SetColorIndex( new Rectangle( x - 3, y - 3, 143, 22 ), pt, 100 ) )
                            return ;

                        y += 23 ;
                    }

                    for( int i = 0 ; i < 40 ; i++ )
                    {
                        if( SetColorIndex( new Rectangle( x - 3, y - 3, 17, 17 ), pt, i ) )
                            return ;

                        if( ( i + 1 ) % 8 == 0 )
                        {
                            x = 6 ;
                            y += 18 ;
                        }
                        else
                            x += 18 ;
                    }

                    if( colorButton.moreButton != "" )
                    {
                        if( SetColorIndex( new Rectangle( x - 3, y - 3, 143, 22 ), pt, 101 ) )
                            return ;
                    }
                }

                if( colorIndex != -1 )
                {
                    colorIndex = -1 ;
                    Invalidate() ;
                }
            }

            protected override void OnClick( EventArgs e )
            {
                if( colorIndex < 0 || colorIndex > 40)
                    return ;

                //if (colorIndex < 40)
                    //colorButton.ColorVaule = Global.colorList[colorIndex] ;
                //    colorButton.ColorIndex = colorIndex;
                //else if (colorIndex == 100)
                //    colorButton.ColorVaule = Color.Transparent;
                //else
                //{
                //    ColorDialog dlg = new ColorDialog();
                //    dlg.Color = colorButton.ColorVaule;
                //    dlg.FullOpen = true;

                //    if (dlg.ShowDialog(this) != DialogResult.OK)
                //    {
                //        Close();
                //        return;
                //    }

                //    colorButton.ColorVaule = dlg.Color;
                //}

                Close() ;
                //colorButton.OnChanged( EventArgs.Empty ) ;
                colorButton.ColorIndex = colorIndex;
                colorButton.OnChanged(this, null);
            }

            protected void DrawButton( PaintEventArgs e, int x, int y, string text,
                                       int index, bool selected )
            {
                Pen darkPen = new Pen( SystemColors.ControlDark ) ;
                Pen lightPen = new Pen( SystemColors.ControlLightLight ) ;
                SolidBrush lightBrush = new SolidBrush( SystemColors.ControlLightLight ) ;

                if( colorIndex == index )
                {
                    e.Graphics.DrawRectangle( lightPen, x - 3, y - 3, 143, 22 ) ;
                    e.Graphics.DrawLine( darkPen, x - 2, y + 19, x + 140, y + 19 ) ;
                    e.Graphics.DrawLine( darkPen, x + 140, y - 2, x + 140, y + 19 ) ;
                }
                else if( selected )
                {
                    e.Graphics.FillRectangle( lightBrush, x - 3, y - 3, 144, 23 ) ;
                    e.Graphics.DrawLine( darkPen, x - 3, y - 3, x + 139, y - 3 ) ;
                    e.Graphics.DrawLine( darkPen, x - 3, y - 3, x - 3, y + 18 ) ;
                }

                Rectangle rc = new Rectangle( x, y, 137, 16 ) ;
                SolidBrush textBrush = new SolidBrush( SystemColors.ControlText ) ;

                StringFormat textFormat = new StringFormat() ;
                textFormat.Alignment = StringAlignment.Center ;
                textFormat.LineAlignment = StringAlignment.Center ;

                e.Graphics.DrawRectangle( darkPen, rc ) ;
                //e.Graphics.DrawString( text, colorButton.Font, textBrush, rc, textFormat ) ;
            }

            protected bool SetColorIndex( Rectangle rc, Point pt, int index )
            {
                if( rc.Contains( pt ) )
                {
                    if( colorIndex != index )
                    {
                        colorIndex = index ;
                        Invalidate() ;
                    }

                    return true ;
                }

                return false ;
            }
        }
    }
}
