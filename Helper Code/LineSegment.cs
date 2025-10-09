using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.Helper_Code
{
    public class LineSegment
    {
        public (int X, int Y) startPoint { get; private set; }
        public (int X, int Y) endPoint { get; private set; }

        public HashSet<(int X, int Y)> points { get; private set; } = new HashSet<(int X, int Y)>();

        private float theta, r;

        private int maxGap, minSegLength;

        public LineSegment(float theta, float r, int maxGap, int minSegLength)
        {
            this.theta = theta;
            this.r = r;

            this.maxGap = maxGap;
            this.minSegLength = minSegLength;

        }

        private bool fallsOnSlope(int x, int y)
        {
            float yCalc = (float)(x * Math.Cos(theta) - r) / (float)(-Math.Sin(theta));

            int roundYCalc = (int)Math.Round(yCalc);

            return roundYCalc == y;
        }

        public void addPoint(int x, int y)
        {
            if (fallsOnSlope(x, y))
            {
                if (x < startPoint.X)
                    startPoint = (x, y);
                else if (x > endPoint.X)
                    endPoint = (x, y);

                points.Add((x, y));
            }
            else
                throw new Exception("Tried adding a point that doesn't fall on the line");
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
        public void mergeLine(LineSegment seg)
        {
            if (sameLine(seg))
            {
                if (seg.startPoint.X < startPoint.X)
                    this.startPoint = seg.startPoint;
                else if (seg.endPoint.X > endPoint.X)
                    this.endPoint = seg.endPoint;

                foreach ((int, int) point in seg.points)
                    this.points.Add(point);
            }
        }

        public bool longEnoughSegment()
        {
            return (this.points.Count >= minSegLength);
        }

        public List<LineSegment> getSegments()
        {
            List<LineSegment> lineSegments = new List<LineSegment>();

            List<(int X, int Y)> pointsList = points.ToList<(int X, int Y)>();
            pointsList.Sort(((int X, int _) a, (int X, int _) b) => a.X - b.X); //sort based on X-direction

            (int X, int Y) segStart = pointsList[0];
            (int X, int Y) segEnd = pointsList[0];

            return lineSegments;
        }
    }
}
