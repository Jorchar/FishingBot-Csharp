using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FishingBot_Csharp
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public Point punkt1;
        public Point punkt2;
        public Point punkt3;
        public Color bobberColor1;
        public Boolean anyMovement = false;

        Bitmap bitmap;
        Bitmap terenDoSzukania;
        Bitmap terenDoPorownania;
        Graphics g;

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(Keys));

        private KeyHandler ghk;

        public Form1()
        {
            InitializeComponent();
            ghk = new KeyHandler(Keys.Escape, this);
            ghk.Register();
            ghk = new KeyHandler(Keys.K, this);
            ghk.Register();
            ghk = new KeyHandler(Keys.J, this);
            ghk.Register();
            ghk = new KeyHandler(Keys.H, this);
            ghk.Register();
        }

        private void HandleHotkey(ref Message m)
        {
            try
            {
                this.Cursor = new Cursor(Cursor.Current.Handle);
                switch ((int)m.LParam)
                {
                    case 4718592:
                        punkt1 = new Point(Cursor.Position.X, Cursor.Position.Y);
                        label1.Text = "Top-Left: " + Cursor.Position.X.ToString() + "x" + Cursor.Position.Y.ToString();
                        break;
                    case 4849664:
                        punkt2 = new Point(Cursor.Position.X, Cursor.Position.Y);
                        label2.Text = "Bottom-Right: " + Cursor.Position.X.ToString() + "x" + Cursor.Position.Y.ToString();
                        break;
                    case 4915200:
                        punkt3 = new Point(Cursor.Position.X, Cursor.Position.Y);
                        Cursor.Position = new Point(0, 0);
                        System.Threading.Thread.Sleep(100);
                        terenDoSzukania = new Bitmap(punkt2.X - punkt1.X, punkt2.Y - punkt1.Y);
                        terenDoPorownania = new Bitmap(punkt2.X - punkt1.X, punkt2.Y - punkt1.Y);
                        g = Graphics.FromImage(terenDoSzukania);
                        g.CopyFromScreen(punkt1.X, punkt1.Y, 0, 0, terenDoSzukania.Size);
                        if ((punkt3.X > punkt1.X && punkt3.X < punkt2.X) && (punkt3.Y > punkt1.Y && punkt3.Y < punkt2.Y))
                        {
                            bobberColor1 = terenDoSzukania.GetPixel(punkt3.X - punkt1.X, punkt3.Y - punkt1.Y);
                            button2.BackColor = bobberColor1;
                        }
                        Cursor.Position = punkt3;
                        g.Dispose();
                        break;
                    case 1769472:
                        this.Close();
                        break;
                    default:
                        anyMovement = !anyMovement;
                        break;
                }
            }
            catch (System.NullReferenceException)
            {

            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
                HandleHotkey(ref m);
            base.WndProc(ref m);
        }

        Point ColorFinder()
        {
            g = Graphics.FromImage(terenDoSzukania);
            g.CopyFromScreen(punkt1.X, punkt1.Y, 0, 0, terenDoSzukania.Size);
            for(int y = 0; y < terenDoSzukania.Height; y++)
            {
                for(int x = 0; x < terenDoSzukania.Width; x++)
                {
                    if(terenDoSzukania.GetPixel(x, y).R > bobberColor1.R - 15 &&
                        terenDoSzukania.GetPixel(x, y).G > bobberColor1.G - 15 &&
                        terenDoSzukania.GetPixel(x, y).B > bobberColor1.B - 15 &&
                        terenDoSzukania.GetPixel(x, y).R < bobberColor1.R + 15 &&
                        terenDoSzukania.GetPixel(x, y).G < bobberColor1.G + 15 &&
                        terenDoSzukania.GetPixel(x, y).B < bobberColor1.B + 15)
                    {
                        return new Point(x+punkt1.X, y+punkt1.Y);
                    }
                }
            }
            return new Point(0, 0);
        }

        bool ColorFinder(Point bobberPlace)
        {
            g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(bobberPlace.X, bobberPlace.Y, 0, 0, bitmap.Size);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).R > bobberColor1.R - 15 &&
                        bitmap.GetPixel(x, y).G > bobberColor1.G - 15 &&
                        bitmap.GetPixel(x, y).B > bobberColor1.B - 15 &&
                        bitmap.GetPixel(x, y).R < bobberColor1.R + 15 &&
                        bitmap.GetPixel(x, y).G < bobberColor1.G + 15 &&
                        bitmap.GetPixel(x, y).B < bobberColor1.B + 15)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            for (int y=0; y < bmp1.Height; y++)
            {
                for(int x = 0; x < bmp1.Width; x++)
                {
                    int zmienna = bmp2.GetPixel(x, y).R;
                    if (bmp1.GetPixel(x, y).R < zmienna - 10 || bmp1.GetPixel(x, y).R > zmienna + 10)
                    {
                            return false;
                    }
                }
            }
            return true;
        }

        void AnyMovement()
        {
            while (true)
            {
                while (!anyMovement)
                {
                    System.Threading.Thread.Sleep(100);
                }
                g = Graphics.FromImage(terenDoSzukania);
                g.CopyFromScreen(punkt1.X, punkt1.Y, 0, 0, terenDoSzukania.Size);
                g.Dispose();

                while (anyMovement)
                {
                    g = Graphics.FromImage(terenDoPorownania);
                    g.CopyFromScreen(punkt1.X, punkt1.Y, 0, 0, terenDoPorownania.Size);
                    g.Dispose();
                    if (!CompareBitmaps(terenDoSzukania, terenDoPorownania))
                    {
                        System.Threading.Thread.Sleep((int)delay.Value);
                        if (klawisz.Text == "LeftClick")
                        {
                            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                            System.Threading.Thread.Sleep(10);
                            mouse_event(MOUSEEVENTF_LEFTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                            anyMovement = !anyMovement;
                        }
                        else
                        {
                            SendKeys.SendWait(klawisz.Text);
                            break;
                        }
                    }
                }
                System.Threading.Thread.Sleep((int)waitTime.Value);
            }
        }
        void FindAndDo()
        {
            while (true)
            {
                if (castOnStart.Checked)
                {
                    System.Threading.Thread.Sleep((int)waitTime.Value);
                    if (klawisz.Text == "LeftClick")
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                        System.Threading.Thread.Sleep(10);
                        mouse_event(MOUSEEVENTF_LEFTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                    }
                    else
                    {
                        SendKeys.SendWait(klawisz.Text);
                    }
                    System.Threading.Thread.Sleep((int)delay.Value);
                }
                while (true)
                {
                    punkt3 = ColorFinder();
                    g.Dispose();
                    if (punkt3.X == 0 && punkt3.Y == 0)
                    {
                        continue;
                    }
                    else
                    {
                        if (pointAndClick.Checked)
                        {
                            Cursor.Position = punkt3;
                            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)punkt3.X, (uint)punkt3.Y, 0, 0);
                            System.Threading.Thread.Sleep(10);
                            mouse_event(MOUSEEVENTF_LEFTUP, (uint)punkt3.X, (uint)punkt3.Y, 0, 0);
                        }
                        break;
                    }
                }
            }
        }

        void FindAndWait()
        {
            bitmap = new Bitmap(10, 10);
            while (true)
            {
                if (castOnStart.Checked)
                {
                    System.Threading.Thread.Sleep((int)waitTime.Value);
                    if (klawisz.Text == "LeftClick")
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                        System.Threading.Thread.Sleep(10);
                        mouse_event(MOUSEEVENTF_LEFTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                    }
                    else
                    {
                        SendKeys.SendWait(klawisz.Text);
                    }
                    System.Threading.Thread.Sleep((int)delay.Value);
                }
                punkt3 = ColorFinder();
                g.Dispose();
                if (punkt3.X == 0 && punkt3.Y == 0)
                {
                    Console.WriteLine("Color not found");
                    continue;
                }
                while (ColorFinder(punkt3))
                {
                    System.Threading.Thread.Sleep(5);
                }
                g.Dispose();
                punkt3.X -= 10;
                if (pointAndClick.Checked)
                {
                    Cursor.Position = punkt3;
                    mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)punkt3.X, (uint)punkt3.Y, 0, 0);
                    System.Threading.Thread.Sleep(10);
                    mouse_event(MOUSEEVENTF_LEFTUP, (uint)punkt3.X, (uint)punkt3.Y, 0, 0);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ghk = new KeyHandler(Keys.K, this);
            ghk.Unregiser();
            ghk = new KeyHandler(Keys.J, this);
            ghk.Unregiser();
            ghk = new KeyHandler(Keys.H, this);
            ghk.Unregiser();

            terenDoSzukania = new Bitmap(punkt2.X - punkt1.X, punkt2.Y - punkt1.Y);

            if (radioButton1.Checked)
            {
                Thread thread_1 = new Thread(new ThreadStart(FindAndDo));
                thread_1.IsBackground = true;
                thread_1.Start();
            }
            else
            {
                if (radioButton2.Checked)
                {
                    Thread thread_1 = new Thread(new ThreadStart(FindAndWait));
                    thread_1.IsBackground = true;
                    thread_1.Start();
                }
                else
                {
                    ghk = new KeyHandler((Keys)converter.ConvertFromString(textBox1.Text), this);
                    ghk.Register();
                    Thread thread_1 = new Thread(new ThreadStart(AnyMovement));
                    thread_1.IsBackground = true;
                    thread_1.Start();
                }
            }
            
            button1.Enabled = false;
            castOnStart.Checked = true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (klawisz.Focused)
            {
                klawisz.Text = "" + e.KeyCode;
            }
            if (textBox1.Focused)
            {
                textBox1.Text = "" + e.KeyCode;
            }
        }

        private void klawisz_DoubleClick(object sender, EventArgs e)
        {
            klawisz.Text = "LeftClick";
        }
    }
}
