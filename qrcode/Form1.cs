using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using com.google.zxing.qrcode;
using com.google.zxing;
using com.google.zxing.common;
using System.Collections;
using System.Diagnostics;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private const int WIDTH = 600;
        private const int HEIGHT = 600;
        private Square[,] squares = null;
        private WhereIAm whereIAm = null;

        public Form1()
        {
            InitializeComponent();
        }
        
        private ByteMatrix QRCodeGen(string text, int width, int height)
        {
            try
            {
                QRCodeWriter writer = new QRCodeWriter(0);
                Hashtable hints = new Hashtable();

                hints.Add(EncodeHintType.ERROR_CORRECTION, com.google.zxing.qrcode.decoder.ErrorCorrectionLevel.M);
                hints.Add("Version", "7");

                ByteMatrix _matrix = writer.encode(text, BarcodeFormat.QR_CODE, width, height, hints);

                #region Supress border white line
                int row = -1;
                int col = -1;
                int endRow = 0;
                int endCol = 0;
                bool isWhiteLine = true;

                while (isWhiteLine)
                {
                    row++;
                    for (int i = 0; i < _matrix.Width; i++)
                    {
                        if (_matrix.Array[row][i] == 0)
                        {
                            isWhiteLine = false;
                            break;
                        }
                    }
                }

                isWhiteLine = true;
                endRow = _matrix.Height;
                while (isWhiteLine)
                {
                    endRow--;
                    for (int i = 0; i < _matrix.Width; i++)
                    {
                        if (_matrix.Array[endRow][i] == 0)
                        {
                            isWhiteLine = false;
                            break;
                        }
                    }
                }

                isWhiteLine = true;
                while (isWhiteLine)
                {
                    col++;
                    for (int i = 0; i < _matrix.Height; i++)
                    {
                        if (_matrix.Array[i][col] == 0)
                        {
                            isWhiteLine = false;
                            break;
                        }
                    }
                }

                isWhiteLine = true;
                endCol = _matrix.Width;
                while (isWhiteLine)
                {
                    endCol--;
                    for (int i = 0; i < _matrix.Height; i++)
                    {
                        if (_matrix.Array[i][endCol] == 0)
                        {
                            isWhiteLine = false;
                            break;
                        }
                    }
                }

                #endregion
                
                ByteMatrix final = new ByteMatrix(endCol - col, endRow - row);
                for (int y = row; y < endRow; y++)
                {
                    for (int x = col; x < endCol; x++)
                    {
                        final.Array[x - col][y - row] = _matrix.Array[x][y];
                    }
                }


                return final;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void CreateSquares(ByteMatrix byteMatrix, int squareSize)
        {
            squares = new Square[(byteMatrix.Width / squareSize) + 1, (byteMatrix.Height / squareSize) + 1];

            for (int y = 0, cntY = 0; y < byteMatrix.Height; y+=squareSize, cntY++)
            {
                for (int x = 0, cntX = 0; x < byteMatrix.Width; x+=squareSize, cntX++)
                {
                    Color clr;
                    if (byteMatrix.Array[x][y] == 0)
                        clr = Color.Black;
                    else
                        clr = Color.White;

                    squares[cntX, cntY] = new Square(
                        new Size(squareSize, squareSize),
                        new System.Drawing.Point(cntX * squareSize, cntY * squareSize)
                        );
                    squares[cntX, cntY].Color = clr;
                }
            }
        }

        private int GetSizeOf1Square(ByteMatrix byteMatrix)
        {
            // The first line is compose initaly by seven black square
            int size = 0;
            // Try if it's a border (sound's like all pixel are white [-1])
            int row = 0;
            bool found = false;
            while (!found)
            {
                for (int x = 0; x < byteMatrix.Array[row].Length; x++)
                {
                    if (byteMatrix.Array[row][x] == 0)
                    {
                        found = true;
                        break;
                    }
                }
                row++;
            }
            //while (byteMatrix.Array[row][size++] == -1) ;
            while (byteMatrix.Array[row][size++] == 0) ;
            return ((size - 1) / 7);
        }

        private Button FindButton(string xy)
        {
            foreach (Button btn in this.Controls.OfType<Button>())
            {
                if (btn.Name == xy)
                    return btn;
            }
            return null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ClientSize = new Size(WIDTH, HEIGHT);

            

            
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: break;
                case Keys.Down:
                    Point loc = whereIAm.Location;
                    break;
                case Keys.Left: break;
                case Keys.Right: break;
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Get the QRCode matrix
            ByteMatrix matrix = QRCodeGen("C# fournit également l'instruction foreach", WIDTH, HEIGHT);

            int squareSize = GetSizeOf1Square(matrix);

            this.ClientSize = new Size(matrix.Width, matrix.Height);

            CreateSquares(matrix, squareSize);

            whereIAm = new WhereIAm(squares[0, 0].Size, squares[squares.GetLength(1) - 2, 1].Location);
            whereIAm.x = squares.GetLength(1) - 2;
            whereIAm.y = 1;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (squares != null)
            {
                try
                {
                    // Show the QRCode
                    foreach (Square square in squares)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(square.Color), new Rectangle(square.Location, square.Size));
                    }
                    // Draw under the cell the current location
                    e.Graphics.FillRectangle(new SolidBrush(Color.Red), new Rectangle(whereIAm.Location, whereIAm.Size));
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    public class WhereIAm : Square
    {
        public int x;
        public int y;

        public WhereIAm(Size size, Point location)
        {
            base.Location =  location;
            base.Size = size;
        }
    }

    public class Square
    {
        private Size size;
        private Point location;
        private Color color;

        public Size Size
        {
            get { return size; }
            set { size = value; }
        }

        public Point Location
        {
            get { return location; }
            set { location = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public Square()
        {
            this.size = new Size(1, 1);
            this.location = new Point(1, 1);
        }

        public Square(Size size, Point location)
        {
            this.size = size;
            this.location = location;
        }
    }
}
