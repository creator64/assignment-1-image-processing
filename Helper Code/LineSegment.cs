using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INFOIBV.Helper_Code
{
    public class LineSegment
    {
        public (int X, int Y)  startPoint { get; private set; }
        public (int X, int Y) endPoint { get; private set; }

        public HashSet<(int X, int Y)> points { get; private set; } = new HashSet<(int X, int Y)>();

        private float theta, r;

        private int maxGap, minSegLength;

        public bool LongEnough { get { return points.Count >= minSegLength; } }
        public int Length { get { return points.Count; } }

        public LineSegment(float theta, float r, int maxGap, int minSegLength)
        {
            this.theta = theta;
            this.r = r;

            this.maxGap = maxGap;
            this.minSegLength = minSegLength;

            startPoint = (int.MaxValue, int.MaxValue);
            endPoint = (int.MinValue, int.MinValue);

        }

        private bool fallsOnSlope(int x, int y, int width, int height)
        {

            int xTransform = x - (width / 2);
            float yTransform = (float)(xTransform * Math.Cos(theta) - r) / (float)(-Math.Sin(theta));
            float yCalc = (height / 2) - yTransform;

            int roundYCalc = (int)Math.Round(yCalc);

            return roundYCalc == y;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Returns a bool that indicates whether the point could be added or not</returns>
        public bool addPoint(int x, int y, int width, int height)
        {
            if (fallsOnSlope(x, y, width, height))
            {
                if (x < startPoint.X)
                    startPoint = (x, y);
                if (x > endPoint.X)
                    endPoint = (x, y);

                
                points.Add((x, y));
                return true;
            }
            else
                return false;
        }


        private bool sameLine(LineSegment seg)
        {
            int xDist = this.startPoint.X - seg.startPoint.X;
            int yDist = this.startPoint.Y - seg.startPoint.Y;

            return (xDist <= maxGap && yDist <= maxGap);
        } 

        /// <summary>
        /// Merge one line with the current line, if the gap between them is smaller than maxGap
        /// </summary>
        /// <param name="seg"></param>
        /// <returns>A bool signifying whether the lines have been merged or not</returns>
        public bool mergeLine(LineSegment seg)
        {
            if (sameLine(seg))
            {
                if (seg.startPoint.X < startPoint.X)
                    this.startPoint = seg.startPoint;
                else if (seg.endPoint.X > endPoint.X)
                    this.endPoint = seg.endPoint;

                foreach ((int, int) point in seg.points)
                    this.points.Add(point);

                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">the width of the image</param>
        /// <param name="height">the height of the image</param>
        /// <returns></returns>
        public List<LineSegment> getSegments(int width, int height)
        {
            List<LineSegment> lineSegments = new List<LineSegment>();

            List<(int X, int Y)> pointsList = points.ToList<(int X, int Y)>();
            pointsList.Sort(((int X, int _) a, (int X, int _) b) => a.X - b.X); //sort based on X-direction

            LineSegment currentSegment = new LineSegment(theta, r, maxGap, minSegLength);

            foreach((int X, int Y) point in pointsList)
            {

                //Debug.WriteLine($"MaxGap: {maxGap}");
                //Debug.WriteLine($"X gap: {Math.Abs(currentSegment.startPoint.X - point.X)}");
                //Debug.WriteLine($"Y gap: {Math.Abs(currentSegment.endPoint.Y - point.Y)}");

                int xGap = Math.Abs(currentSegment.startPoint.X - point.X);
                int yGap = Math.Abs(currentSegment.startPoint.Y - point.Y);
                float dist = (float)Math.Sqrt(xGap * xGap + yGap * yGap);    
                if (currentSegment.Length == 0 || dist <= maxGap)
                {
                    bool result = currentSegment.addPoint(point.X, point.Y, width, height);
                    //Debug.WriteLine($"added point: {result}");  
                }
                else if(currentSegment.LongEnough) //store segment and reset currentSegment.
                {
                    lineSegments.Add(currentSegment);
                    currentSegment = new LineSegment(theta, r, maxGap, minSegLength);
                }
                else //discard current segment and reset it.
                {
                    //Debug.WriteLine($"current segment length: {currentSegment.Length}");
                    currentSegment = new LineSegment(theta, r, 50, minSegLength);
                }
            }

            if (currentSegment.LongEnough)
                lineSegments.Add(currentSegment);

            return lineSegments;
        }

        public void drawToImage(Bitmap image, Color color, int width, int height, int thickness = 1)
        {
            if(thickness < 1)
                throw new ArgumentException($"Thickness can't be lower than 1, was: {thickness}");

            int xSpan = endPoint.X - startPoint.X;
            
            if(xSpan == 0)
            {
                int ySpan = endPoint.Y - startPoint.Y;

                for(int y = 0; y <= ySpan; y++)
                {
                    int yTransform = startPoint.Y + y - (height / 2);
                    float xTransform = (float)(yTransform * Math.Sin(theta) - r) / (float)(-Math.Cos(theta));
                    float x = (width / 2) - xTransform;

                    int roundX = (int)Math.Round(x);

                    for (int i = -(thickness - 1); i <= (thickness - 1); i++)
                    {
                        for (int j = -(thickness - 1); j <= (thickness - 1); j++)
                        {
                            int newX = roundX + i;
                            int newY = startPoint.Y + y + j;
                            if (newX >= 0 && newX < image.Width && newY >= 0 && newY < image.Height)
                                image.SetPixel(newX, newY, color);
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x <= xSpan; x++)
                {
                    int xTransform = startPoint.X + x - (width / 2);
                    float yTransform = (float)(xTransform * Math.Cos(theta) - r) / (float)(-Math.Sin(theta));
                    float y = (height / 2) - yTransform;

                    int roundY = (int)Math.Round(y);

                    for (int i = -(thickness - 1); i <= (thickness - 1); i++)
                    {
                        for (int j = -(thickness - 1); j <= (thickness - 1); j++)
                        {
                            int newX = startPoint.X + x + i;
                            int newY = roundY + j;
                            if (newX >= 0 && newX < image.Width && newY >= 0 && newY < image.Height)
                                image.SetPixel(newX, newY, color);
                        }
                    }
                }
            }
        }

        public void drawPointsToImage(Bitmap image, Color color, int thickness = 1)
        {
            foreach ((int X, int Y) point in points)
            {
                for (int i = -(thickness - 1); i <= (thickness - 1); i++)
                {
                    for (int j = -(thickness - 1); j <= (thickness - 1); j++)
                    {
                        int newX = point.X + i;
                        int newY = point.Y + j;
                        if (newX >= 0 && newX < image.Width && newY >= 0 && newY < image.Height)
                            image.SetPixel(newX, newY, color);
                    }
                }
            }
        }

        public override string ToString()
        {
            string str = $"< ({startPoint.X}, {startPoint.Y}) -";

            List<(int X, int Y)> pointsList = points.ToList<(int X, int Y)>();
            pointsList.Sort(((int X, int _) a, (int X, int _) b) => a.X - b.X); //sort based on X-direction

            foreach((int X, int Y) point in pointsList)
            {
                if (point.X == startPoint.X && point.Y == startPoint.Y) continue;

                if (point.X == endPoint.X && point.Y == endPoint.Y)
                    str += $" ({point.X}, {point.Y}) >";
                else
                    str += $" ({point.X}, {point.Y}) -";
            }

            return str;
        }
    }
}
