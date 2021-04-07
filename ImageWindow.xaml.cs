using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Simpson
{
    /// <summary>
    /// Логика взаимодействия для ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        public ImageWindow()
        {
            InitializeComponent();
        }

        public double MonteCarlo(Bitmap bitmap, double length, int range)
        {
            Random rnd = new Random();
            Bitmap newBitmap = new Bitmap(bitmap);

            // range - число всех точек

            int hit = 0; //число попаданий
            double area = length * length; //площадь
            for (int i = 0; i < range; i++)
            {
                int x = rnd.Next(38, bitmap.Width - 8); // выбор координаты x для точки
                int y = rnd.Next(38, bitmap.Height - 35); // выбор координаты y для точки
                System.Drawing.Color col = bitmap.GetPixel(x, y); // ставим точку

                /*_______________________________________________________________________________________________________________
                |При программировании метода Монте-Карло для вычисления одномерного интеграла следует быть аккуратным в случае, |
                |когда инимальное и максимальное значения функции имеют разные знаки.                                           |
                |При случайных бросках и попаданиях под график функции следует учитывать знаки                                  |
                |(при отрицательных значениях функции нужно не суммировать число попаданий, а вычитать,                         |
                |общее число попаданий может быть отрицательным числом).                                                        |
                -----------------------------------------------------------------------------------------------------------------*/

                if (col.G >= 200 && col.B < 200)
                {
                    hit++; //суммируем число попаданий
                    newBitmap.SetPixel(x, y, System.Drawing.Color.Green);
                }
                else if(col.B >= 200 && col.G < 200)
                {
                    hit--; //вычитаем число попаданий
                    newBitmap.SetPixel(x, y, System.Drawing.Color.Blue);
                }
                else 
                {
                   newBitmap.SetPixel(x, y, System.Drawing.Color.Gray);
                }
            }

            using (MemoryStream memory = new MemoryStream())
            {
                newBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                Image.Source = bitmapimage;
            }

            return area * hit / range;


        }

    }


}
