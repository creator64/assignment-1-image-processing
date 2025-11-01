namespace INFOIBV.Core.TemplateMatching
{
    public abstract class CuneiATemplate
    {
        public string templatePath;
    }

    public class q : CuneiATemplate
    {
        public q()
        {
            templatePath = "q.bmp";
        }
    }
    
    public class p : CuneiATemplate
    {
        public p()
        {
            templatePath = "p.bmp";
        }
    }
    
    public class pp : CuneiATemplate
    {
        public pp()
        {
            templatePath = "pp.bmp";
        }
    }
    
    public class ppp : CuneiATemplate
    {
        public ppp()
        {
            templatePath = "ppp.bmp";
        }
    }
    
    public class qp_I : CuneiATemplate
    {
        public qp_I()
        {
            templatePath = "qp_I.bmp";
        }
    }
    
    public class qp_II : CuneiATemplate
    {
        public qp_II()
        {
            templatePath = "qp_II.bmp";
        }
    }
    
    public class qp_III : CuneiATemplate
    {
        public qp_III()
        {
            templatePath = "qp_III.bmp";
        }
    }
    
    public class qpp_I : CuneiATemplate
    {
        public qpp_I()
        {
            templatePath = "qpp_I.bmp";
        }
    }
    
    public class qpp_II : CuneiATemplate
    {
        public qpp_II()
        {
            templatePath = "qpp_II.bmp";
        }
    }
    
    public class qpp_III : CuneiATemplate
    {
        public qpp_III()
        {
            templatePath = "qpp_III.bmp";
        }
    }
    
    public class all : CuneiATemplate
    {
        public all()
        {
            templatePath = "all.bmp";
        }
    }
}