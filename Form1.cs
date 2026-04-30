using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace SimplePaint
{
    public partial class Form1 : Form
    {
        enum ToolType { Line, Rectangle, Circle }  // 사용할 도형 타입
        private Bitmap canvasBitmap;          // 실제 그림이 저장되는 비트맵
        private Graphics canvasGraphics;      // 비트맵위에 그리기 위한 객체
        private bool isDrawing= false;       // 현재 드래그중인지 여부
        private Point startPoint;             // 드래그 시작점
        private Point endPoint;               // 드래그 끝점
        private ToolType currentTool= ToolType.Line;  // 현재 선택된 도형
        private Color currentColor= Color.Black;      // 현재 색상
        private int currentLineWidth= 2;              // 현재 선 두께

        public Form1()
        {
            InitializeComponent();

            canvasBitmap = new Bitmap(picCanvas.Width, picCanvas.Height); 
            canvasGraphics = Graphics.FromImage(canvasBitmap); 
            canvasGraphics.Clear(Color.White);   // 캔버스를 흰색으로 초기화

            picCanvas.Image = canvasBitmap;   // 그린 그림을 화면(PictureBox)에표시

            // 마우스이벤트연결
            picCanvas.MouseDown+= PicCanvas_MouseDown;
            picCanvas.MouseMove+= PicCanvas_MouseMove;
            picCanvas.MouseUp+= PicCanvas_MouseUp;

            // picCanvas가 다시 그려질때 PicCanvas_Paint함수를 실행하도록 연결
            picCanvas.Paint+= PicCanvas_Paint;
            
            // 도형 선택 버튼 이벤트연결
            btnLine.Click+= btnLine_Click;
            btnRectangle.Click+= btnRectangle_Click;
            btnCircle.Click+= btnCircle_Click;

            // 색상 콤보 박스 이벤트 연결
            cmbColor.SelectedIndexChanged += cmbColor_SelectedIndexChanged;
            cmbColor.SelectedIndex= 0;  // 기본값: Black

            // 선 두께 트랙바 이벤트 연결
            trbLineWidth.Minimum= 1;    // 최소값
            trbLineWidth.Maximum= 10;   // 최대값
            trbLineWidth.Value= 2;
            trbLineWidth.ValueChanged+= trbLineWidth_ValueChanged;

            // 저장 버튼 이벤트 연결
            btnSaveFile.Click += btnSaveFile_Click;
        }

        private void PicCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;             // 드래그시작
            startPoint= e.Location;      // 시작점저장
         }
        private void PicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;       // 그림그리기와상관없는마우스움직임은무시
            endPoint= e.Location;        // 현재위치갱신
            
            // picCanvas를다시그려라(Paint 이벤트를발생시킨다)
            picCanvas.Invalidate();       // 화면다시그리기(미리보기)
              }

        private void PicCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;     // 그림그리기와상관없는마우스움직임은무시
            isDrawing= false;          // 드래그종료
            endPoint= e.Location;
            // 실제비트맵에도형그리기(확정)

            using (Pen pen= new Pen(currentColor, currentLineWidth))
            {
                DrawShape(canvasGraphics, pen, startPoint, endPoint);
            }
            picCanvas.Invalidate();     // 다시그려서결과반영, Paint 이벤트발생
        }

        private void PicCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (!isDrawing) return;
            
            // 점선펜(미리보기용)
            using (Pen previewPen= new Pen(currentColor, currentLineWidth))
            {
                previewPen.DashStyle= DashStyle.Dash;
                DrawShape(e.Graphics, previewPen, startPoint, endPoint);
            }
        }

        private void DrawShape(Graphics g, Pen pen, Point p1, Point p2) 
        { 
            Rectangle rect = GetRectangle(p1, p2);
            switch (currentTool)
            { 
                case ToolType.Line:
                    g.DrawLine(pen, p1, p2); 
                    break;
                case ToolType.Rectangle:
                    g.DrawRectangle(pen, rect); 
                    break;
                case ToolType.Circle:
                    g.DrawEllipse(pen, rect); 
                    break; 
            } 
        }

        private Rectangle GetRectangle(Point p1, Point p2) 
        {
            return new Rectangle
                (Math.Min(p1.X, p2.X), 
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y)
                ); 
        }

        private void btnLine_Click(object sender, EventArgs e) 
        { 
            currentTool = ToolType.Line; 
        }
        private void btnRectangle_Click(object sender, EventArgs e) 
        {
            currentTool = ToolType.Rectangle; 
        }
        private void btnCircle_Click(object sender, EventArgs e) 
        {
            currentTool = ToolType.Circle;
        }

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbColor.SelectedIndex)
            {
                case 0: // Black 검정
                currentColor= Color.Black;
                    break;
                case 1: // Red 빨강
                currentColor= Color.Red;
                    break;
                case 2: // Blue 파랑
                currentColor= Color.Blue;
                    break;
                case 3: // Green 녹색
                currentColor= Color.Green;
                    break;
                default:
                    currentColor= Color.Black;
                    break;
            }
        }

        private void trbLineWidth_ValueChanged(object sender, EventArgs e) 
        {
            currentLineWidth = trbLineWidth.Value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG 이미지(*.png)|*.png|JPG 이미지(*.jpg)|*.jpg|BMP 이미지(*.bmp)|*.bmp";
                saveDialog.Title = "그림 저장";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveDialog.FileName;
                    try
                    {
                        canvasBitmap.Save(fileName);
                        MessageBox.Show("파일이 저장되었습니다.", "저장 완료");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"저장 중 오류가 발생했습니다: {ex.Message}", "오류");
                    }
                }
            }
        }
    }
}
