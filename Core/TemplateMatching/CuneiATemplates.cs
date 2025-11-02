using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using INFOIBV.Core.Main;

namespace INFOIBV.Core.TemplateMatching
{
    public enum LetterPart
    {
        P,
        Q
    }
    
    public abstract class CuneiATemplate
    {
        public TemplateMatchingImage templateImage;

        public CuneiATemplate()
        {
            String baseDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            templateImage = ProcessingImage.fromBitmap(
                new Bitmap(
                    Path.Combine(baseDirectory, "images", "templates", getTemplatePath())
                )
            ).toTemplateMatchingImage();
        }

        protected abstract string getTemplatePath();
        public abstract Dictionary<Point, LetterPart> extractLetters(Region region);
    }

    public class q : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "q.bmp";
        }

        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, region.minY), LetterPart.Q }
            };
        }
    }
    
    
    public class p : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "p.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, region.minY), LetterPart.P }
            };
        }
    }
    
    public class pp : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "pp.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int x1 = region.minX, x2 = region.minX + region.width / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(x1, region.minY + 2), LetterPart.P },
                { new Point(x2, region.minY + 0), LetterPart.P }
            };
        }
    }
    
    public class ppp : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "ppp.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int x1 = region.minX, x2 = region.minX + region.width / 3, x3 = region.minX + 2 * region.width / 3;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(x1, region.minY + 3), LetterPart.P },
                { new Point(x2, region.minY + 2), LetterPart.P },
                { new Point(x3, region.minY + 0), LetterPart.P }
            };
        }
    }
    
    public class qp_I : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "qp_I.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int y1 = region.minY, y2 = region.minY + region.height / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, y1), LetterPart.Q },
                { new Point(region.minX, y2 + 1), LetterPart.P }
            };
        }
    }
    
    public class qp_II : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "qp_II.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int y1 = region.minY, y2 = region.minY + region.height / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, y1), LetterPart.Q },
                { new Point(region.minX + region.width / 3, y2), LetterPart.P }
            };
        }
    }
    
    public class qp_III : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "qp_III.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int y1 = region.minY, y2 = region.minY + region.height / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, y1), LetterPart.Q },
                { new Point(region.minX + 2 * region.width / 3, y2 - 1), LetterPart.P }
            };
        }
    }
    
    public class qpp_I : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "qpp_I.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int y1 = region.minY, y2 = region.minY + region.height / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, y1), LetterPart.Q },
                { new Point(region.minX, y2 + 1), LetterPart.P },
                { new Point(region.minX + region.width / 3, y2), LetterPart.P }
            };
        }
    }
    
    public class qpp_II : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "qpp_II.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int y1 = region.minY, y2 = region.minY + region.height / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, y1), LetterPart.Q },
                { new Point(region.minX, y2 + 1), LetterPart.P },
                { new Point(region.minX + 2 * region.width / 3, y2 - 1), LetterPart.P }
            };
        }
    }
    
    public class qpp_III : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "qpp_III.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int y1 = region.minY, y2 = region.minY + region.height / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, y1), LetterPart.Q },
                { new Point(region.minX + 1 * region.width / 3, y2), LetterPart.P },
                { new Point(region.minX + 2 * region.width / 3, y2 - 1), LetterPart.P }
            };
        }
    }
    
    public class all : CuneiATemplate
    {
        protected override string getTemplatePath()
        {
            return "all.bmp";
        }
        
        public override Dictionary<Point, LetterPart> extractLetters(Region region)
        {
            int y1 = region.minY, y2 = region.minY + region.height / 2;
            return new Dictionary<Point, LetterPart>
            {
                { new Point(region.minX, y1), LetterPart.Q },
                { new Point(region.minX, y2), LetterPart.P },
                { new Point(region.minX + 1 * region.width / 3, y2), LetterPart.P },
                { new Point(region.minX + 2 * region.width / 3, y2), LetterPart.P }
            };
        }
    }
}