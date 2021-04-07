using org.mariuszgromada.math.mxparser;
using OxyPlot;  // для графиков (сторонняя либа)
using OxyPlot.Wpf; // для графиков (сторонняя либа)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using Expression = org.mariuszgromada.math.mxparser.Expression; //парсер, ГЫ (сторонняя либа)

namespace Simpson
{
    public partial class MainWindow : Window
    {
        List<Point> res = new List<Point>();

        public MainWindow()
        {
            InitializeComponent();
        }
        
        
        Argument x = new Argument("x");

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // достаём значения из текст-боксов
            double a = Convert.ToDouble(textBoxA.Text);
            double b = Convert.ToDouble(textBoxB.Text);
            int n = Convert.ToInt32(textBoxN.Text);

            x.setArgumentValue(1);
            Expression expression = new Expression(textBox1.Text, x); //место для формулы


            if (expression != null)
            {
                if ((bool)ControlAccuracy.IsChecked == true)
                {
                    double accuracy = Convert.ToDouble(textBoxAccuracy.Text);
                    n = 2;
                    while (true)
                    {
                        double sim1 = SimpsonMethod(expression, b, a, n * 2); //алгоритм 2S
                        double sim2 = SimpsonMethod(expression, b, a, n * 4); //алгоритм 4S
                        if (Runge(sim1, sim2) <= accuracy || n > 16384)  // ограничено, чтоб не работало вечность
                        {
                            labelSimpson.Content = $"Симпсон({n}): " + sim1;
                            break;
                        }

                        n *= 2;
                    }
                }
                else
                {
                    double simpson = SimpsonMethod(expression, b, a, n);
                    labelSimpson.Content = "Симпсон: " + simpson;

                    double runge = Runge(SimpsonMethod(expression, b, a, 2 * n), simpson);
                    labelRunge.Content = "Рунге: " + runge;
                }

                
                CreateMonteCarlo();
            }
        }

        // Метод Симпсона (Просто вычисляем значения для интеграла. Сначала для первой пары (S2), потом для второй (S4))
        public double SimpsonMethod(Expression f, double b, double a, int n)
        {
            res.Clear(); // чистим поле, перед посевом
            double sum = 0;
            double h = (b - a) / n; // вычисляем шаг
            string stringFunction = f.getExpressionString();
            

            for (int i = 0; i < n; i++)
            {
                
                x.setArgumentValue(a + h * i);
                double exp1 = new Expression(stringFunction, x).calculate();

                x.setArgumentValue(a + h * (i + 0.5f));
                double exp2 = new Expression(stringFunction, x).calculate();

                x.setArgumentValue(a + h * (i + 1));
                double exp3 = new Expression(stringFunction, x).calculate();

                sum += (exp1 + 4 * exp2 + exp3);

                res.Add(new Point(a + h * i, exp1));
            }

            return sum * h / 6;
        }

        // Монте-Карло (тут картинка графика экспортируется, сама работа метода представлена в ImageWindow)
        public void CreateMonteCarlo()
        {
            
            int range = Convert.ToInt32(textBoxR.Text);
            int trials = Convert.ToInt32(textBoxI.Text);
            double Monte_result = 0; // итоговый результат
            double Monte_acc = 0; // стандартный разброс (погрешность)
            double[] Monte_arr = new double[trials]; //Массив с результатами вычислений на каждом испытании
            ImageWindow window = new ImageWindow();
            
            (this.DataContext as MainViewModel).SetPoints(res);
            var stream = new MemoryStream();
            var pngExporter = new PngExporter { Width = 512, Height = 512, Background = OxyColors.White }; //Определяем размер нашей картинки для работы метода
            pngExporter.Export((this.DataContext as MainViewModel).MyModel, stream);

            double axisLength = (this.DataContext as MainViewModel).MyModel.DefaultXAxis.ActualMaximum - (this.DataContext as MainViewModel).MyModel.DefaultXAxis.ActualMinimum;
            System.Drawing.Bitmap myBitmap = new System.Drawing.Bitmap(stream);

            for (int i = 0; i < trials; i++)
            {
                Monte_arr[i] = window.MonteCarlo(myBitmap, axisLength, range);

                Monte_result += Monte_arr[i];

                if (i == trials - 1)
                {
                    /*__________________________________________________________________________________________________________________________
                    |На этом этапе перед выходом из цикла находится среднее значение Монте-Карло и расчитывается среднеквадратичная погрешность. |
                    |Погрешность считается следующим образом:                                                                                    |
                    |    Из результата каждой итерации i (все эти результаты хранятся в массиве Monte_arr) вычитается среднее значение.          |
                    |    Затем данное число возводится в квадрат                                                                                 |
                    |    Все значения суммируются, делятся на количество итераций trials                                                         |
                    |    Из получившегося результата извлекают квадратный корень.                                                                |
                    -----------------------------------------------------------------------------------------------------------------------------*/

                    Monte_result /= trials; 
                    for (int j = 0; j < trials; j++) {
                        Monte_acc += Math.Pow((Monte_arr[j] - Monte_result), 2);
                    }

                    Monte_acc = Math.Sqrt(Monte_acc / trials); 
                }
            }

            labelMonteAccuracy.Content = "Стандартный разброс: ±" + Monte_acc;
            labelMonte.Content = "Монте-Карло: " + Monte_result;

            window.Show(); 
        }


        
        // Рунге для контроля погрешности Симпсона
        public double Runge(double fun1, double fun2)
        {
            return 1 / 15f * (Math.Abs(fun1 - fun2));
        }

         /*__________________________________________________________________  
         |Ниже находятся флаги, для контроля погрешности.                   |
         |Если чекбокс активирован (Checked), то работаем с погрешностью    |
         |Если чекбокс не активирован (Uncheked), то работаем с разбиениями |
         -------------------------------------------------------------------*/
        private void ControlAccuracy_Checked(object sender, RoutedEventArgs e)
        {
            textBoxAccuracy.IsEnabled = true;
            textBoxN.IsEnabled = false;
        }

        private void ControlAccuracy_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxAccuracy.IsEnabled = false;
            textBoxN.IsEnabled = true;
        }

    }
}
