using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShashkiWPF
{

    public partial class MainWindow : Window
    {
        private bool GameStatus = false; // переменная игрового режима false - поле пустое игра не идет true - эффект обратный;
        private bool kills = false;
        private bool motion = false; // Кто ходит - true (синий) false (красный);
        private bool wasMove = false; // Был ли ход устанавливаем в true если был ход;
        private Point startPoint;
        private int? row = null;
        private int? column = null;
        private SolidColorBrush Blue = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush Red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush Brown = new SolidColorBrush(Colors.Brown);
        private SolidColorBrush Black = new SolidColorBrush(Colors.Black);
        private SolidColorBrush Aqua= new SolidColorBrush(Colors.Aqua);
        private SolidColorBrush BlueQ = new SolidColorBrush(Colors.DarkBlue); // Дамки синих
        private SolidColorBrush RedQ = new SolidColorBrush(Colors.Coral); // Дамки Красных

        public void PrintCount(int Count_of_Red, int Count_of_Blue)
        {
            countWhite.Text = "Количество: " + Count_of_Red.ToString();
            countBlack.Text = "Количество: " + Count_of_Blue.ToString();
        }
        public void PrintMotion(bool motion)
        {
            if (motion) {
                whoMove.Text = "Ход синих";
            }
            else
            {
                whoMove.Text = "Ход красных";
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            PrintCount(ListofRed().Count(), ListofBlue().Count());
        }

        public static bool IsNumberInRange(double number, double minValue, double maxValue)
        {
            return number >= minValue && number <= maxValue;
        }


        public Ellipse FindEllipseByRowAndColumn(int? row, int? column)
        {
            return Pole_Grid.Children.OfType<Ellipse>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == column);
        }
        public Rectangle FindRectangleByRowAndColumn(int? row, int? column)
        {
            return Pole_Grid.Children.OfType<Rectangle>().FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == column);
        }
        private List<Ellipse> ListofBlue()
        {
            return Pole_Grid.Children.Cast<FrameworkElement>().Where(x => x is Ellipse).Cast<Ellipse>().Where(x => x.Fill == Blue).ToList();
        }
        private List<Ellipse> ListofRed()
        {
            return Pole_Grid.Children.Cast<FrameworkElement>().Where(x => x is Ellipse).Cast<Ellipse>().Where(x => x.Fill == Red).ToList();
        }
        private List<Ellipse> ListofBlueQ()
        {
            return Pole_Grid.Children.Cast<FrameworkElement>().Where(x => x is Ellipse).Cast<Ellipse>().Where(x => x.Fill == BlueQ).ToList();
        }
        private List<Ellipse> ListofRedQ()
        {
            return Pole_Grid.Children.Cast<FrameworkElement>().Where(x => x is Ellipse).Cast<Ellipse>().Where(x => x.Fill == RedQ).ToList();
        }
        private bool IsEllipseInListRed(Ellipse ellipseToCheck)
        {
            return ListofRed().Contains(ellipseToCheck);
        }
        private bool IsEllipseInListBlue(Ellipse ellipseToCheck)
        {
            return ListofBlue().Contains(ellipseToCheck);
        }
        private bool IsEllipseInListRedQ(Ellipse ellipseToCheck)
        {
            return ListofRedQ().Contains(ellipseToCheck);
        }
        private bool IsEllipseInListBlueQ(Ellipse ellipseToCheck)
        {
            return ListofBlueQ().Contains(ellipseToCheck);
        }

        private void CanKill(int rows, int columns, Ellipse el)
        {
            Ellipse ellipse_neigbore;
            for (int i = -1; i <= 1; i = i + 2) //бежим по углам квадрата вокруг выбранной пешки
            {
                for (int j = -1; j <= 1; j = j + 2)
                {
                    if (i == 0 && j == 0) { continue; }
                    ellipse_neigbore = FindEllipseByRowAndColumn(rows + i, columns + j); //проверяем если в этих углах пешка
                    if (ellipse_neigbore != null)
                    {
                        if (ellipse_neigbore.Fill != el.Fill) // если есть смотрим на ее цвет
                        {
                            if (FindEllipseByRowAndColumn(rows + i + i, columns + j + j) == null && IsNumberInRange(rows+i+i, 0, 7) && IsNumberInRange(columns + j + j, 0, 7)) // если цвет противоположный, то смотрим есть ли
                                                                                                                                                                                // за ней место и даем возможность убить
                            {
                                kills = true;
                                MoveAbility(el);
                            }
                        }
                    }
                }
            }
        }

        private void Winner()
        {
            if (ListofRed().Count() == 0)
            {
                TheWinner.Text = "Победа синих";
                RectangleWinner.Fill = Blue;
                GameStatus = false;
                Play.Content = "ЕЩЕ РАЗ";
                Play.Background = Aqua;
            }
            if (ListofBlue().Count() == 0)
            {
                TheWinner.Text = "Победа красных";
                RectangleWinner.Fill = Red;
                GameStatus = false;
                Play.Content = "ЕЩЕ РАЗ";
                Play.Background = Aqua;
            }
        }
        private void MoveAccept(int? row, int? column, Ellipse el)
        {
            Ellipse ellipse_neigbore;
            Rectangle rectangle_neigbore;
            for (int i = -1; i <= 1; i = i + 2)
            {
                for (int j = -1; j <= 1; j = j + 2)
                {
                    if (i == 0 && j == 0) { continue; }
                    ellipse_neigbore = FindEllipseByRowAndColumn(row + i, column + j);
                    if (ellipse_neigbore != null) 
                    { 
                        if(ellipse_neigbore.Fill != el.Fill) // ищем возможность убить и подсвечаем клетку за жертвой
                        {
                            if(FindEllipseByRowAndColumn(row + i + i, column + j + j) == null && IsNumberInRange((int)row + i + i, 0, 7) && IsNumberInRange((int)column + j + j, 0, 7))
                            {
                                rectangle_neigbore = FindRectangleByRowAndColumn(row + i + i, column + j + j);
                                if(rectangle_neigbore != null) rectangle_neigbore.Fill = Brown;                                 
                            }
                        }
                    }
                    else if(!kills)
                    {
                        rectangle_neigbore = FindRectangleByRowAndColumn(row + i, column + j);
                        if (rectangle_neigbore == null) { continue; }
                        else if (IsEllipseInListRed(el) && row>row+i) 
                        {
                            rectangle_neigbore.Fill = Brown;
                        }
                        else if (IsEllipseInListBlue(el) && row<row+i)
                        {
                            rectangle_neigbore.Fill = Brown;
                        }
                        else if (IsEllipseInListBlueQ(el)||IsEllipseInListRedQ(el))
                        {
                            rectangle_neigbore.Fill = Brown;
                        }
                    }
                }
            }
        }

        private void HideAccept()
        {
            var ListRectangle = Pole_Grid.Children.Cast<FrameworkElement>()
                .Where(x => x is Rectangle)
                .Cast<Rectangle>()
                .Where(x => x.Fill == Brown)
                .ToList();
            for (int i = 0; i < ListRectangle.Count; i++)
            {
                ListRectangle[i].Fill = Black;
            }
        }

        public void Elipse_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Ellipse ellipse)
            {
                ellipse = sender as Ellipse;
                startPoint = e.GetPosition(Pole_Grid);
                column = (int?)(startPoint.X / (Pole_Grid.ActualWidth / Pole_Grid.ColumnDefinitions.Count));
                row = (int?)(startPoint.Y / (Pole_Grid.ActualHeight / Pole_Grid.RowDefinitions.Count));
                MoveAccept(row, column, ellipse);
                ellipse.CaptureMouse();
            }
        }
        private void Elipse_Drop(object sender, MouseButtonEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            Point currentPoint = e.GetPosition(Pole_Grid);
            int current_column = (int)(currentPoint.X / (Pole_Grid.ActualWidth / Pole_Grid.ColumnDefinitions.Count));
            int current_row = (int)(currentPoint.Y / (Pole_Grid.ActualHeight / Pole_Grid.RowDefinitions.Count));
            Rectangle rectangle = FindRectangleByRowAndColumn(current_row, current_column);
            if (rectangle != null)
            { 
                if (rectangle.Fill == Brown)
                {
                    Grid.SetColumn(ellipse, current_column);
                    Grid.SetRow(ellipse, current_row);
                    if (ellipse.Fill == Blue && current_row == 7) ellipse.Fill = BlueQ;
                    if (ellipse.Fill == Red && current_row == 0) ellipse.Fill = RedQ;

                    if (kills)
                    {
                        Ellipse VictimEllepse = FindEllipseByRowAndColumn((current_row + row) / 2, (current_column + column) / 2);
                        Pole_Grid.Children.Remove(VictimEllepse);
                        PrintCount(ListofRed().Count(), ListofBlue().Count());
                        column = current_column;
                        row = current_row;
                        if (motion)
                        {
                            kills = false;
                            FindandRemoveBlueEl();
                            FindandRemoveRedEl();
                            CanKill(current_row, current_column, ellipse);
                            HideAccept();
                            if (kills) return;
                        }
                        else if (!motion)
                        {
                            kills = false;
                            FindandRemoveRedEl();
                            FindandRemoveBlueEl();
                            CanKill(current_row, current_column, ellipse);
                            HideAccept();
                            if (kills) return;
                        }
                    }
                    wasMove = true;
                    kills = false;
                }
            }
            HideAccept();
            ellipse.ReleaseMouseCapture();

            if (!wasMove) return;
            motion = !motion;
            if (motion)
            {
                for(int i = 0; i<ListofBlue().Count; i++)
                {
                    var Lists = ListofBlue();
                    int rows = (int)Lists[i].GetValue(Grid.RowProperty);
                    int columns = (int)Lists[i].GetValue(Grid.ColumnProperty);
                    CanKill(rows, columns, Lists[i]);
                }
                if(!kills) FindandMoveabilityBlueEl();
                FindandRemoveRedEl();
            }
            else
            {
                for (int i = 0; i < ListofRed().Count; i++)
                {
                    var Lists = ListofRed();
                    int rows = (int)Lists[i].GetValue(Grid.RowProperty);
                    int columns = (int)Lists[i].GetValue(Grid.ColumnProperty);
                    CanKill(rows, columns, Lists[i]);
                }
                if (!kills) FindandMoveabilityRedEl();
                FindandRemoveBlueEl();
            }
            Winner();
            PrintMotion(motion);
            wasMove = !wasMove;
        }

        private void MoveAbility(Ellipse ellipse)
        {
            ellipse.MouseLeftButtonDown += Elipse_Click;
            ellipse.MouseLeftButtonUp += Elipse_Drop;
        }
        private void RemoveMoveAbility(Ellipse ellipse)
        {
            ellipse.MouseLeftButtonDown -= Elipse_Click;
            ellipse.MouseLeftButtonUp -= Elipse_Drop;
        }
        private void FillField ()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 != 0)
                    {
                        if (i < 3)
                        {
                            Ellipse ellipse = new Ellipse();
                            ellipse.Height = 50;
                            ellipse.Width = 50;
                            ellipse.Fill = Blue;
                            Grid.SetColumn(ellipse, j);
                            Grid.SetRow(ellipse, i);
                            Pole_Grid.Children.Add(ellipse);
                        }
                        else if (i > 4)
                        {
                            Ellipse ellipse = new Ellipse();
                            ellipse.Height = 50;
                            ellipse.Width = 50;
                            MoveAbility(ellipse);
                            ellipse.Fill = Red;
                            Grid.SetColumn(ellipse, j);
                            Grid.SetRow(ellipse, i);
                            Pole_Grid.Children.Add(ellipse);
                        }
                    }
                }
            }
        }

        private void FindandRemoveBlueEl() //Убрать возможность передвигать для Blue
        {
            var BlueList = ListofBlue();
            for (int i = 0; i < BlueList.Count(); i++)
            {
                RemoveMoveAbility(BlueList[i]);
            }
        }
        private void FindandRemoveRedEl() //Убрать возможность передвигать для Red
        {
            var RedList = ListofRed();
            for (int i = 0; i < RedList.Count(); i++)
            {
                RemoveMoveAbility(RedList[i]);
            }
        }
        private void FindandMoveabilityBlueEl() //Добавить возможность передвигать для Blue
        {
            var BlueList = ListofBlue();
            for (int i = 0; i < BlueList.Count(); i++)
            {
               MoveAbility(BlueList[i]);
            }
        }
        private void FindandMoveabilityRedEl() //Добавить возможность передвигать для Red
        {
            var RedList = ListofRed();
            for (int i = 0; i < RedList.Count(); i++)
            {
                MoveAbility(RedList[i]);
            }
        }
        private void Button_Click_Play(object sender, RoutedEventArgs e)
        {
            if (!GameStatus)
            {
                FillField();
                GameStatus = true;
            }
            motion = false;
            Surrender.Opacity = 1;
            PrintCount(ListofRed().Count(), ListofBlue().Count());
            PrintMotion(motion);
        }
        private void Button_Surrender(object sender, RoutedEventArgs e)
        {
            if (GameStatus) {
                var all = Pole_Grid.Children.Cast<FrameworkElement>().Where(x => x is Ellipse).Cast<Ellipse>().ToList();
                foreach (var item in all)
                {
                    Pole_Grid.Children.Remove(item);
                }
                
                PrintCount(ListofRed().Count(), ListofBlue().Count());
                if (motion) motion = true;
                GameStatus = false;
            }
        }
    }
}

