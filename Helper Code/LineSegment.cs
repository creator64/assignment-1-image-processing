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

        public float theta { get; private set; }
        public float r { get; private set; }

        private int maxGap, minSegLength;

        public bool LongEnough { get { return points.Count >= minSegLength; } }
        public int Length { get { return points.Count; } }

        private List<(int X, int Y)> orderedPointsList;

        private int dX;
        private int dY; 

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
        /// Initialises the start and end points of the Line segment and creates a points list accordingly.
        /// Done only when all points have been added.
        /// </summary>
        public void close()
        {
            if (points.Count == 0 ) return;

            List<(int X, int Y)> pointsList = points.ToList();
            int minX = pointsList.Min(((int X, int Y) a) => a.X);
            int minY = pointsList.Min(((int X, int Y) a) => a.Y);
            int maxX = pointsList.Max(((int X, int Y) a) => a.X);
            int maxY = pointsList.Max(((int X, int Y) a) => a.Y);

            this.dX = maxX - minX;
            this.dY = maxY - minY;


            if(dX >= dY)
            {
                pointsList.Sort(((int X, int Y) a, (int X, int Y) b) => a.X - b.X); //sort based on X-direction
                startPoint = pointsList.First();
                endPoint = pointsList.Last();
            }
            else
            {
                pointsList.Sort(((int X, int Y) a, (int X, int Y) b) => a.Y - b.Y); //sort based on Y-direction
                startPoint = pointsList.First();
                endPoint = pointsList.Last();

            }
            this.orderedPointsList = pointsList;

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

            LineSegment currentSegment = new LineSegment(theta, r, maxGap, minSegLength);
            (int X, int Y) prevPoint = (0, 0); //filler, gets changed later

            foreach((int X, int Y) point in orderedPointsList)
            {

                //Debug.WriteLine($"MaxGap: {maxGap}");
                //Debug.WriteLine($"X gap: {Math.Abs(currentSegment.startPoint.X - point.X)}");
                //Debug.WriteLine($"Y gap: {Math.Abs(currentSegment.endPoint.Y - point.Y)}");

                int xGap = Math.Abs(currentSegment.startPoint.X - point.X);
                int yGap = Math.Abs(currentSegment.startPoint.Y - point.Y);
                float dist = distancePoints(prevPoint, point);
                //Debug.WriteLine($"Gap: {dist}");

                if (currentSegment.Length == 0 || dist <= maxGap)
                {
                    currentSegment.addPoint(point.X, point.Y, width, height);
                    //Debug.WriteLine($"added point: {result}");  
                }
                else if(currentSegment.LongEnough) //store segment and reset currentSegment.
                {
                    currentSegment.close();
                    lineSegments.Add(currentSegment);
                    currentSegment = new LineSegment(theta, r, maxGap, minSegLength);
                }
                else //discard current segment and reset it.
                {
                    //Debug.WriteLine($"current segment length: {currentSegment.Length}");
                    currentSegment.close();
                    currentSegment = new LineSegment(theta, r, 50, minSegLength);
                }
                prevPoint = point;
            }

            if (currentSegment.LongEnough)
            {
                currentSegment.close();
                lineSegments.Add(currentSegment);
            }

            return lineSegments;
        }

        public bool validPoint(int X, int Y)
        {
            Debug.WriteLine($"-----------------------------------");
            Debug.WriteLine($"X: {X}");
            Debug.WriteLine($"Y: {Y}");
            Debug.WriteLine($"Startpoint X: {startPoint.X}");
            Debug.WriteLine($"StartPoint Y: {startPoint.Y}");
            Debug.WriteLine($"endPoint X: {endPoint.X}");
            Debug.WriteLine($"endPoint Y: {endPoint.Y}");
            Debug.WriteLine($"-----------------------------------");
            if (dX >= dY){
                Debug.WriteLine($"({X}, {Y}) falls within X range [{startPoint.X} ... {endPoint.X}]: {X >= startPoint.X && X <= endPoint.X}");
                return X >= startPoint.X && X <= endPoint.X;
            }
            else
            {
                Debug.WriteLine($"({X}, {Y}) falls within Y range [{startPoint.Y} ... {endPoint.Y}]: {Y >= startPoint.X && Y <= endPoint.X}");
                return Y >= startPoint.Y && Y <= endPoint.Y;
            }
        }
        private float distancePoints((int X, int Y) a, (int X, int Y) b)
        {
            int xGap = Math.Abs(a.X - b.X);
            int yGap = Math.Abs(a.Y - b.Y);
            return (float)Math.Sqrt(xGap * xGap + yGap * yGap);
        }

        public void drawToImage(Bitmap image, Color color, int width, int height, int thickness = 1)
        {
            if(thickness < 1)
                throw new ArgumentException($"Thickness can't be lower than 1, was: {thickness}");
            
            int xSpan = endPoint.X - startPoint.X;
            int ySpan = endPoint.Y - startPoint.Y;

            if (xSpan <= ySpan)
            {

                for(int y = 0; y <= ySpan; y++)
                {
                    int yTransform = (height / 2) - startPoint.Y - y;
                    float xTransform = (float)(yTransform * Math.Sin(theta) - r) / (float)(-Math.Cos(theta));
                    float x = xTransform + (width / 2);

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
