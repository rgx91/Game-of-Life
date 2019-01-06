using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Game_of_Life
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly System.Windows.Threading.DispatcherTimer DispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private GameTable GameTableLocal;
        private Generator GeneratorLocal;
        private int row = 150;
        private int column = 150;
        private int MinimumLengthOfRectangle = 4;
        private Cell[][] ArrayOfAllCell;
        private readonly Stopwatch Stopwatch = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            GameTableLocal = new GameTable(FrontCanvas);
            ArrayOfAllCell = ClearAndCreateRectangleColors();//setting up a clear table
            TextBoxHeight.Text = row.ToString();
            TextBoxWidth.Text = column.ToString();
            MinWidthHeight.Text = MinimumLengthOfRectangle.ToString();
            DispatcherTimer.Tick += DispatcherTimer_Tick;
            DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

        }
        private void CenterWindowOnScreen()
        {
            var ScreenWidth = SystemParameters.PrimaryScreenWidth;
            var ScreenHeight = SystemParameters.PrimaryScreenHeight;
            var WindowWidth = Width;
            var WindowHeight = Height;
            Left = (ScreenWidth / 2) - (WindowWidth / 2);
            Top = (ScreenHeight / 2) - (WindowHeight / 2);
        }
        private void StartGenerating(object sender, RoutedEventArgs e)
        {
            if ((string)StartButton.Content == "START")
            {
                if (GeneratorLocal == null)
                {
                    GeneratorLocal = new Generator(ArrayOfAllCell);
                }
                Stopwatch.Start();
                DispatcherTimer.Start();
                StartButton.Content = "STOP";
            }
            else
            {
                Stopwatch.Reset();
                DispatcherTimer.Stop();
                StartButton.Content = "START";
                Generator.GenNum = 0;
            }

        }
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            GeneratorLocal.CalculateNextGen();
            GameTableLocal.SourceUpdate(ArrayOfAllCell, false);
            LabelsUpdate();
        }

        private void LabelsUpdate()
        {
            if (Stopwatch.IsRunning)
            {
                GenerationsDataLabel.Content = "Generations: " + Generator.GenNum + " generations/sec= " + (Generator.GenNum / (Stopwatch.ElapsedMilliseconds / (double)1000)).ToString("00.0");

                var percentage = (Cell.FriendCount + Cell.ChangedCount) / ((double)Cell.FriendCount + Cell.ChangedCount + Cell.DeadCount);
                GameLabel.Content = "Percentage: " + percentage.ToString("p2") + " Conquered: " + (Cell.FriendCount + Cell.EnemyCount + Cell.ChangedCount);
            }


            CounterLabel.Content = "Red alive cells: " + Cell.FriendCount + ", Black alive cells: " + Cell.EnemyCount + ", Changed cells: " + Cell.ChangedCount;
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            RestartUi(true);

        }
        private void RestartUi(bool clearCellArray)
        {
            //this function reset the user interface, also calculate the best canvas size depending on the current window's size
            Stopwatch.Reset();
            DispatcherTimer.Stop();
            StartButton.Content = "START";
            GeneratorLocal = null;
            Generator.GenNum = 0;
            GenerationsDataLabel.Content = "...";
            CounterLabel.Content = "...";
            if (!clearCellArray)
            {
                TextBoxHeight.Text = ArrayOfAllCell.Length.ToString();
                TextBoxWidth.Text = ArrayOfAllCell[0].Length.ToString();
            }
            row = Convert.ToInt32(TextBoxHeight.Text);
            column = Convert.ToInt32(TextBoxWidth.Text);
            MinimumLengthOfRectangle = Convert.ToInt32(MinWidthHeight.Text);
            var RectangleWidth = (int)scrollviewer1.Width / column;//after resize of scrollviewer we need to adjust the rectangle width and height, also the canvas
            var RectangleHeight = (int)scrollviewer1.Height / row;
            if (RectangleWidth < MinimumLengthOfRectangle)
            {
                RectangleWidth = MinimumLengthOfRectangle;
            }
            if (RectangleHeight < MinimumLengthOfRectangle)
            {
                RectangleHeight = MinimumLengthOfRectangle;
            }
            FrontCanvas.Children.Clear();
            FrontCanvas.Width = Convert.ToDouble(TextBoxWidth.Text) * RectangleWidth;
            FrontCanvas.Height = Convert.ToDouble(TextBoxHeight.Text) * RectangleHeight;
            GameTableLocal = null;
            //The GameTable class has a writablebitmap variable, and this variable holds a big chunk of unmanaged memory
            GC.Collect();//We forcing the gc here twice to take care about it
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GameTableLocal = new GameTable(FrontCanvas, RectangleWidth, RectangleHeight, row, column);
            if (clearCellArray)
            {
                ArrayOfAllCell = null;
                ArrayOfAllCell = ClearAndCreateRectangleColors();

            }
            else
            {
                GameTableLocal.SourceUpdate(ArrayOfAllCell, true);
            }


        }
        private Cell[][] ClearAndCreateRectangleColors()
        {//a function for copy new values into a cell jagged array
            Cell.FriendCount = 0;
            Cell.DeadCount = 0;
            Cell.ChangedCount = 0;
            Cell.EnemyCount = 0;
            var cells = new Cell[row][];
            for (var i = 0; i < row; i++)
            {
                cells[i] = new Cell[column];
                for (var j = 0; j < column; j++)
                {
                    cells[i][j] = new Cell(Cell.DeadCellByte);
                }
            }
            return cells;
        }

        private void Random_Button_Gen(object sender, RoutedEventArgs e)
        {
            RestartUi(true);
            var r = new Random();
            for (var i = 0; i < ArrayOfAllCell.Length; i++)
            {
                for (var j = 0; j < ArrayOfAllCell[i].Length; j++)
                {
                    if (r.Next(0, 100) < 50 - RandomTrackbar.Value)//higher trackbar value causes higher chance to a live cell
                    {
                        if (r.Next(0, 100) < 50)
                        {
                            ArrayOfAllCell[i][j].SetCondition(Cell.FriendCellByte);

                        }
                        else
                        {
                            ArrayOfAllCell[i][j].SetCondition(Cell.EnemyCellByte);
                        }
                        ArrayOfAllCell[i][j].NeedColorUpdate(true);
                    }
                }
            }
            GameTableLocal.SourceUpdate(ArrayOfAllCell, false);
            LabelsUpdate();
        }

        private void Slider_Change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RandomLabel.Content = "Chance to live on random: " + (int)(50 - RandomTrackbar.Value) + "%";//smaller value=>fewer alive cell
        }
        private void UpdateSizeButtonClick(object sender, RoutedEventArgs e)
        {
            RestartUi(true);
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!IsLoaded)
            {//prevent execution on startup
                return;
            }
            scrollviewer1.Width = e.NewSize.Width - 150;//chosen values for width and height.
            scrollviewer1.Height = e.NewSize.Height - 200;
            RestartUi(false);
        }

        private void Mouse_Move(object sender, MouseEventArgs e)
        {//mouse drag
            GameTableLocal.CheckPositionInCanvas(e, ArrayOfAllCell);
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                LabelsUpdate();
            }

        }
        private void Mouse_L_down(object sender, MouseButtonEventArgs e)
        {
            GameTableLocal.CheckPositionInCanvas(e, ArrayOfAllCell);
            LabelsUpdate();
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {

            var Dialog = new Microsoft.Win32.SaveFileDialog();
            Dialog.FileName = "Document";
            Dialog.DefaultExt = ".csv";
            Dialog.Filter = "Csv documents (.csv)|*.csv";
            var result = Dialog.ShowDialog();
            if (result == true)
            {
                SaveEntireArray(Dialog.FileName);
                MessageBox.Show("Success!");

            }
        }
        private void SaveEntireArray(string filename)
        {
            var Swrt = new StreamWriter(filename);
            var FirstLine = ArrayOfAllCell.Length.ToString() + ";" + ArrayOfAllCell[0].Length;
            Swrt.WriteLine(FirstLine);
            foreach (var Cellarray in ArrayOfAllCell)
            {
                foreach (var cell in Cellarray)
                {
                    Swrt.Write(cell + ";");
                }
                Swrt.WriteLine();
            }
            Swrt.Close();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new Microsoft.Win32.OpenFileDialog();
            Dialog.DefaultExt = ".csv";
            Dialog.Filter = "Csv files (*.csv)|*.csv";
            var result = Dialog.ShowDialog();
            if (result == true)
            {
                e.Handled = true;
                ArrayOfAllCell = null;
                ArrayOfAllCell = LoadEntireArray(Dialog.FileName);
                RestartUi(false);
                LabelsUpdate();
            }

        }

        private Cell[][] LoadEntireArray(string filename)
        {
            var ItemCount = 0;
            var AllCondition = new List<byte>();
            var sr = new StreamReader(filename);
            var JaggedArrayLength = sr.ReadLine().Split(';').Select(n => Convert.ToInt32(n)).ToArray();//First line contains metadata about array length
            while (sr.Peek() != -1)
            {
                var Line = sr.ReadLine().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < Line.Length; i++)//one line contains every column in a row
                {
                    AllCondition.Add(Convert.ToByte(Line[i]));
                }

            }
            sr.Close();
            var LocalCellArray = new Cell[JaggedArrayLength[0]][];//We converting here a 1 dimensional byte array to a jagged array with using information in JaggedArrayLength and ItemCount variable as an index
            for (var i = 0; i < LocalCellArray.Length; i++)
            {
                LocalCellArray[i] = new Cell[JaggedArrayLength[1]];
                for (var j = 0; j < LocalCellArray[i].Length; j++)
                {
                    LocalCellArray[i][j] = new Cell(AllCondition[ItemCount++]);
                }
            }

            return LocalCellArray;
        }

        private void FrontCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GameTableLocal.CheckPositionInCanvas(e, ArrayOfAllCell);
            LabelsUpdate();
        }
    }
}


