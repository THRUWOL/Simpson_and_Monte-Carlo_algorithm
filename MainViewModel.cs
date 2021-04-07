using System.Collections.Generic;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;

namespace Simpson
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            Create();
        }

        public void SetPoints(List<Point> points)
        {
            PointsPlus.Clear();
            PointsMinus.Clear();
            foreach (Point p in points)  //цикл пихания точек на график
            {
                if (p.Y == 0)
                {
                    this.PointsPlus.Add(new DataPoint(p.X, p.Y));
                    this.PointsMinus.Add(new DataPoint(p.X, p.Y));
                }
                else if (p.Y > 0)
                {
                    this.PointsPlus.Add(new DataPoint(p.X, p.Y));
                }
                else
                {
                    this.PointsMinus.Add(new DataPoint(p.X, p.Y));
                }
            }
            this.MyModel.ResetAllAxes();
            this.MyModel.InvalidatePlot(true);

        }

        private void Create()
        {
            this.MyModel = new PlotModel { Title="График", PlotType = PlotType.Cartesian,};  // Юзается Декартова система координат.
            this.PointsPlus = new List<DataPoint>();
            this.PointsMinus = new List<DataPoint>();
            MyModel.Series.Add(new AreaSeries   //строим график на "положительной" части координат
            {
                Color = OxyColors.Green,
                ItemsSource = PointsPlus,
                LineStyle = LineStyle.None,
                StrokeThickness = 0.1f
            }) ;

            MyModel.Series.Add(new AreaSeries  //строим график на "отрицательной" части координат
            {
                Color = OxyColors.Blue,
                ItemsSource = PointsMinus,
                LineStyle = LineStyle.None,
                StrokeThickness = 0.1f      
            }) ;
        }

        public IList<DataPoint> PointsPlus { get; private set; }
        public IList<DataPoint> PointsMinus { get; private set; }

        public PlotModel MyModel { get; private set; }
    }
}
