using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDFTry
{
    public partial class Form1 : Form
    {
        public static int windowSizeX = 256;
        public static int windowSizeY = 256;
        public Bitmap picture;
        public List<Circle> circles = new List<Circle>();
        public int offsetByX = windowSizeX / 2;
        public int offsetByY = windowSizeY / 2;
        public double scale = 1;

        bool isMouseDown = false;
        int[] mouseCoordinates;

        Random rand = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            circles.Add(new Circle(0, 40, 20, new int[] { 255, 0, 0 }));
            circles.Add(new Circle(-30, 0, 20, new int[] { 0, 0, 255 }));
            circles.Add(new Circle(30, 0, 20, new int[] { 0, 255, 0 }));

            pictureBox1.Width = windowSizeX;
            pictureBox1.Height = windowSizeY;
            picture = new Bitmap(windowSizeX, windowSizeY);
        }

        public double SdfCircle(double x, double y, Circle circle)
        {
            double vectorX = x - circle.X;
            double vectorY = y - circle.Y;
            double length = Math.Sqrt(vectorX * vectorX + vectorY * vectorY) - circle.Radius;

            return length;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Image = picture;
            for (int y = 0; y < picture.Height; y++)
            {
                for (int x = 0; x < picture.Width; x++)
                {
                    int[] color = CalculateColor(x, y);
                    picture.SetPixel(x, y, Color.FromArgb(color[0], color[1], color[2]));
                }
            }
        }

        public int[] CalculateColor(double localX, double localY)
        {
            int[] outputColor = new int[] { 0, 0, 0 };
            double globalX = (localX - offsetByX) * scale;
            double globalY = (-localY + offsetByY) * scale;

            int directionsCount = 10;
            int stepsCount = 4;
            double angle = 2 * Math.PI / directionsCount * (rand.NextDouble() * 0.5);

            for (int i = 0; i < directionsCount; i++)
            {
                double rayX = globalX;
                double rayY = globalY;
                for (int j = 0; j < stepsCount; j++)
                {
                    double Sdf = 10000;
                    int[] color = new int[] { 0, 0, 0 };

                    foreach (Circle circle in circles)
                    {
                        double distance = SdfCircle(rayX, rayY, circle);
                        if (distance < Sdf)
                        {
                            Sdf = distance;
                            color = circle.Color;
                        }
                    }

                    if (Sdf <= 0.1)
                    {
                        outputColor[0] += color[0] / directionsCount;
                        outputColor[1] += color[1] / directionsCount;
                        outputColor[2] += color[2] / directionsCount;

                        break;
                    }
                    else if (Sdf > 500)
                    {
                        outputColor[0] = 0;
                        outputColor[1] = 0;
                        outputColor[2] = 0;
                        return outputColor;
                    }

                    rayX += Sdf * Math.Cos(angle);
                    rayY += Sdf * Math.Sin(angle);
                }
                angle += Math.PI * 2 / directionsCount * (rand.NextDouble() + 0.5);
            }

            return outputColor;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            mouseCoordinates = new int[] { e.X, e.Y };
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            mouseCoordinates = null;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                int moveByX = e.X - mouseCoordinates[0];
                int moveByY = e.Y - mouseCoordinates[1];

                offsetByX += moveByX;
                offsetByY += moveByY;

                mouseCoordinates = new int[] { e.X, e.Y };
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                scale -= 0.1;
            else
                scale += 0.1;
        }

    }

    public class Circle
    {
        public int X { get; }
        public int Y { get; }
        public double Radius { get; }
        public int[] Color { get; }

        public Circle(int x, int y, double radius, int[] color)
        {
            X = x;
            Y = y;
            Radius = radius;
            Color = color;
        }
    }
}
