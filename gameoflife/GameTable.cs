using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Game_of_Life
{
    internal class GameTable
    {
        private readonly Canvas FrontCanvas;
        private readonly int RectangleWidth;
        private readonly int RectangleHeight;
        private readonly int Width;
        private readonly int Height;
        private int PreviousPositionX = -1;
        private int PreviousPositionY = -1;
        private Image Image;
        private WriteableBitmap WriteableBmp;
        public GameTable(Canvas canvas, int rectangleWidth = 4, int rectangleHeight = 4, int rectangleRowCount = 150, int rectangleColumnCount = 150)
        {
            FrontCanvas = canvas;
            RectangleWidth = rectangleWidth;
            RectangleHeight = rectangleHeight;
            Height = rectangleRowCount;
            Width = rectangleColumnCount;
            DrawEmptyTable();//initialize a new table with new values
            
        }

        private void DrawEmptyTable()
        {//Too high width, height or minimum size values from user causes overflow here
            //
                WriteableBmp = BitmapFactory.New(Width * RectangleWidth, Height * RectangleHeight);
            using (WriteableBmp.GetBitmapContext())
            {
                var y = 0;
                for (var i = 0; i < Height; i++)
                {
                    var x = 0 - RectangleWidth;
                    for (var j = 0; j < Width; j++)
                    {

                        x += RectangleWidth;
                        WriteableBmp.DrawRectangle(x, y, x + RectangleWidth, y + RectangleHeight, Colors.Black);
                        WriteableBmp.FillRectangle(x + 1, y + 1, x + RectangleWidth, y + RectangleHeight, Cell.DeadColor);

                    }
                    y += RectangleHeight;
                }


            }

            Image = new Image();
            Image.Source = WriteableBmp;//this image will be the children of the canvas, we just need to refresh its source, which is faster than refreshing the canvas with new images
            FrontCanvas.Children.Add(Image);


        }
        public void SourceUpdate(IEnumerable<Cell[]> JaggedArrayOfCells, bool fullupdate)//refresh the image source here
        {
            using (WriteableBmp.GetBitmapContext())
            {
                var y = 0;
                foreach (var CellArray in JaggedArrayOfCells)
                {
                    var x = 0;
                    foreach (var cell in CellArray)
                    {
                        if (fullupdate)//refresh entire game table
                        {
                            WriteableBmp.FillRectangle(x + 1, y + 1, x + RectangleWidth, y + RectangleHeight, cell.GetColor());
                        }
                        else
                        {
                            if (cell.IsColorUpdateNeeded())//refresh only the changes
                            {

                                WriteableBmp.FillRectangle(x + 1, y + 1, x + RectangleWidth, y + RectangleHeight, cell.GetColor());
                            }
                        }
                        x += RectangleWidth;
                    }
                    y += RectangleHeight;
                }
            }
        }

        private static int CompareColors(Color a, Color b)
        {//if color a matches color b this function return 100
            return 100 * (int)(
                1.0 - (Math.Abs(a.R - b.R) +
                       Math.Abs(a.G - b.G) +
                       Math.Abs(a.B - b.B)) / (256.0 * 3)
            );
        }

        public void CheckPositionInCanvas(MouseEventArgs e, Cell[][] JaggedArrayOfCells)//click event handler of canvas
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //this function called rapidly when the mouse button is down and the mouse is in motion
                //so we should prevent multiple calls in one rectangle
                //We can do this with storing the previous function call's position and compare it with new values
                var positionX = e.GetPosition(FrontCanvas).X;
                var positionY = e.GetPosition(FrontCanvas).Y;
                if (positionX % RectangleWidth == 0 || positionY % RectangleHeight == 0)//if true its a rectangle border
                {
                    return;
                }

                var ArraypositionX = (int)Math.Floor((positionX / RectangleWidth));
                var ArraypositionY = (int)Math.Floor((positionY / RectangleHeight));
                if (ArraypositionX == PreviousPositionX && ArraypositionY == PreviousPositionY)
                {
                    return;
                }
                if (CompareColors(JaggedArrayOfCells[ArraypositionY][ArraypositionX].GetColor(), Cell.AliveColor) == 100)//%
                {
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].SetCondition(Cell.DeadCellByte);
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].NeedColorUpdate(true);
                    PreviousPositionX = ArraypositionX;
                    PreviousPositionY = ArraypositionY;
                }
                else
                {
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].SetCondition(Cell.FriendCellByte);
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].NeedColorUpdate(true);
                    PreviousPositionX = ArraypositionX;
                    PreviousPositionY = ArraypositionY;
                }
                SourceUpdate(JaggedArrayOfCells, false);

            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                var positionX = e.GetPosition(FrontCanvas).X;
                var positionY = e.GetPosition(FrontCanvas).Y;
                if (positionX % RectangleWidth == 0 || positionY % RectangleHeight == 0)//if true its a rectangle border
                {
                    return;
                }

                var ArraypositionX = (int)Math.Floor((positionX / RectangleWidth));
                var ArraypositionY = (int)Math.Floor((positionY / RectangleHeight));
                if (ArraypositionX == PreviousPositionX && ArraypositionY == PreviousPositionY)
                {
                    return;
                }
                if (CompareColors(JaggedArrayOfCells[ArraypositionY][ArraypositionX].GetColor(), Cell.EnemyColor) == 100)//%
                {
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].SetCondition(Cell.DeadCellByte);
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].NeedColorUpdate(true);
                    PreviousPositionX = ArraypositionX;
                    PreviousPositionY = ArraypositionY;
                }
                else
                {
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].SetCondition(Cell.EnemyCellByte);
                    JaggedArrayOfCells[ArraypositionY][ArraypositionX].NeedColorUpdate(true);
                    PreviousPositionX = ArraypositionX;
                    PreviousPositionY = ArraypositionY;
                }
                SourceUpdate(JaggedArrayOfCells, false);
            }
            else
            { // Mouse move without mouse down  restore values to default
                PreviousPositionX = -1;
                PreviousPositionY = -1;
            }

        }

    }
}
