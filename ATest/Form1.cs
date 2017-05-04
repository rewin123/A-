using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATest
{
    public partial class Form1 : Form
    {
        APath testRoom;
        int width = 100;
        int height = 100;
        int playersize = 2;
        Random r = new Random();
        public Form1()
        {
            InitializeComponent();
            testRoom = new APath(width, height, playersize);
            
            pictureBox1.Image = Draw(testRoom, new Point2D(), new Point2D(width - playersize, height - playersize), pictureBox1.Size);
            testRoom.doors.Add(new Point2D(0, 0));
            testRoom.doors.Add(new Point2D(width - playersize, height - playersize));
            testRoom.doors.Add(new Point2D(width - playersize, 1));
            testRoom.doors.Add(new Point2D(1, height - playersize));
            timer1.Start();
        }

        public Bitmap Draw(APath path, Point2D start, Point2D end, Size size)
        {
            Bitmap img = new Bitmap(size.Width, size.Height);
            List<Point2D> points = path.MakeRoute(start.x, start.y, end.x, end.y);
            Graphics gr = Graphics.FromImage(img);
            gr.Clear(Color.Wheat);
            Brush brush = Brushes.Brown;
            for(int i = 0;i < points.Count - 1;i++)
            {
                int x1 = points[i].x * size.Width / path.width;
                int x2 = points[i + 1].x * size.Width / path.width;
                int y1 = points[i].y * size.Height / path.height;
                int y2 = points[i + 1].y * size.Height / path.height;
                int rect = Math.Max(Math.Abs(x2 - x1) - 1, Math.Abs(y2 - y1) - 1);
                gr.FillRectangle(brush, x1, y1, rect, rect);
            }
            brush = Brushes.Black;
            for(int x = 0;x < path.width;x++)
            {
                for(int y = 0;y < path.height;y++)
                {
                    if(path.map[x,y])
                    {
                        int x1 = x * size.Width / path.width;
                        int x2 = (x + 1) * size.Width / path.width;
                        int y1 = y * size.Height / path.height;
                        int y2 = (y + 1) * size.Height / path.height;
                        int rect = Math.Max(Math.Abs(x2 - x1) - 1, Math.Abs(y2 - y1) - 1);
                        gr.FillRectangle(brush, x1, y1, rect, rect);
                    }
                }
            }

            return img;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int xpos = r.Next(width);
            int ypos = r.Next(height);
            int fur_width = r.Next(5);
            int fur_height = r.Next(5);
            if(testRoom.TestAdd(xpos,ypos,fur_width,fur_height))
            {
                for(int x = 0;x < fur_width;x++)
                {
                    for(int y = 0;y < fur_height;y++)
                    {
                        if(x + xpos < width && y + ypos < height)
                        {
                            testRoom.map[x + xpos, y + ypos] = true;
                        }
                    }
                }

                pictureBox1.Image = Draw(testRoom, new Point2D(), new Point2D(width - playersize, height - playersize), pictureBox1.Size);
            }
        }
    }

    public class APath
    {
        public bool[,] map;
        public int playerSize;
        public List<Point2D> doors;
        public int width;
        public int height;
        public APath(int width, int height, int playerSize)
        {
            this.width = width;
            this.height = height;
            this.playerSize = playerSize;
            map = new bool[width, height];
            doors = new List<Point2D>();
        }
        public bool TestAdd(int xpos, int ypos, int width, int height)
        {
            bool[,] walkable = MakeWalkable();
            int x1 = Math.Max(0, xpos - playerSize);
            int x2 = Math.Min(this.width - 1, xpos + width + playerSize);
            int y1 = Math.Max(0, ypos - playerSize);
            int y2 = Math.Min(this.height - 1, ypos + height + playerSize);
            for (int x = x1; x < x2; x++)
            {
                for (int y = y1; y < y2; y++)
                {
                    walkable[x, y] = false;
                }
            }
            int[,] datablock = null; //одна карта интов для того, чтобы не выделять память под каждый поиск маршрута
            for (int i = 0; i < doors.Count; i++)
            {
                for (int j = i + 1; j < doors.Count; j++)
                {
                    if (datablock == null)
                    {
                        datablock = MakeRouteMap(doors[i].x, doors[i].y, doors[j].x, doors[j].y, walkable);
                        if (datablock[doors[j].x, doors[j].y] == 0)
                            return false;
                    }
                    else
                    {
                        datablock = MakeRouteMap(doors[i].x, doors[i].y, doors[j].x, doors[j].y, walkable, datablock);
                        if (datablock[doors[j].x, doors[j].y] == 0)
                            return false;
                    }
                }
            }
            return true;
        }

        public int[,] MakeRouteMap(int x1, int y1, int x2, int y2, bool[,] walkable)
        {
            int step = 1;
            int nextstep;
            int interations = 1;
            int[,] map = new int[width, height];
            map[x1, y1] = 1;
            while (interations > 0)
            {
                interations = 0;
                nextstep = step + 1;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (map[x, y] == step)
                        {
                            if (x > 0)
                            {
                                if (map[x - 1, y] == 0 && walkable[x - 1, y])
                                {
                                    map[x - 1, y] = nextstep;
                                    interations++;
                                }
                            }
                            if (x < width)
                            {
                                if (map[x + 1, y] == 0 && walkable[x + 1, y])
                                {
                                    map[x + 1, y] = nextstep;
                                    interations++;
                                }
                            }
                            if (y > 0)
                            {
                                if (map[x, y - 1] == 0 && walkable[x, y - 1])
                                {
                                    map[x, y - 1] = nextstep;
                                    interations++;
                                }
                            }
                            if (y < height)
                            {
                                if (map[x, y + 1] == 0 && walkable[x, y + 1])
                                {
                                    map[x, y + 1] = nextstep;
                                    interations++;
                                }
                            }
                        }
                    }
                }
                step++;
                if (map[x2, y2] != 0)
                    break;
            }

            return map;
        }

        int[,] MakeRouteMap(int x1, int y1, int x2, int y2, bool[,] walkable, int[,] blockdata)
        {
            int step = 1;
            int nextstep;
            int interations = 1;

            int dwisth = width - 1;
            int dheight = height - 1;

            int[,] map = blockdata;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = 0;
                }
            }
            map[x1, y1] = 1;
            while (interations > 0)
            {
                interations = 0;
                nextstep = step + 1;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (map[x, y] == step)
                        {
                            if (x > 0)
                            {
                                if (map[x - 1, y] == 0 && walkable[x - 1, y])
                                {
                                    map[x - 1, y] = nextstep;
                                    interations++;
                                }
                            }
                            if (x < width)
                            {
                                if (map[x + 1, y] == 0 && walkable[x + 1, y])
                                {
                                    map[x + 1, y] = nextstep;
                                    interations++;
                                }
                            }
                            if (y > 0)
                            {
                                if (map[x, y - 1] == 0 && walkable[x, y - 1])
                                {
                                    map[x, y - 1] = nextstep;
                                    interations++;
                                }
                            }
                            if (y < height)
                            {
                                if (map[x, y + 1] == 0 && walkable[x, y + 1])
                                {
                                    map[x, y + 1] = nextstep;
                                    interations++;
                                }
                            }
                        }
                    }
                }
                step++;
                if (map[x2, y2] != 0)
                    break;
            }

            return map;
        }

        bool[,] MakeWalkable()
        {
            bool[,] walkable = new bool[width, height];
            bool next = true;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    next = true;
                    for (int i = 0; i < playerSize && next; i++)
                    {
                        for (int j = 0; j < playerSize && next; j++)
                        {
                            if (x + i >= width)
                            {
                                next = false;
                                break;
                            }
                            if (y + j >= height)
                            {
                                next = false;
                                break;
                            }
                            if (map[x + i, y + j])
                            {
                                next = false;
                                break;
                            }
                        }
                    }
                    walkable[x, y] = next;
                }
            }
            return walkable;
        }

        public List<Point2D> MakeRoute(int[,] map, int x2, int y2)
        {
            List<Point2D> points = new List<Point2D>();
            points.Add(new Point2D(x2, y2));
            int xnow = x2;
            int ynow = y2;
            int dwisth = width - 1;
            int dheight = height - 1;
            if (map[x2, y2] == 0)
                return points;

            while (map[xnow, ynow] != 1)
            {
                int dstep = map[xnow, ynow] - 1;
                if (xnow > 0)
                {
                    if (map[xnow - 1, ynow] == dstep)
                    {
                        xnow--;
                        points.Add(new Point2D(xnow, ynow));
                        continue;
                    }
                }
                if (xnow < dwisth)
                {
                    if (map[xnow + 1, ynow] == dstep)
                    {
                        xnow++;
                        points.Add(new Point2D(xnow, ynow));
                        continue;
                    }
                }
                if (ynow > 0)
                {
                    if (map[xnow, ynow - 1] == dstep)
                    {
                        ynow--;
                        points.Add(new Point2D(xnow, ynow));
                        continue;
                    }
                }
                if (ynow < dheight)
                {
                    if (map[xnow, ynow + 1] == dstep)
                    {
                        ynow++;
                        points.Add(new Point2D(xnow, ynow));
                        continue;
                    }
                }
            }
            return points;
        }

        public List<Point2D> MakeRoute(int x1, int y1, int x2, int y2)
        {
            return MakeRoute(MakeRouteMap(x1, y1, x2, y2, MakeWalkable()), x2, y2);
        }
    }

    public class Point2D
    {
        public int x;
        public int y;
        

        public Point2D(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }
    }
}
