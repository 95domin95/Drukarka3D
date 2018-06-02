using System;
using System.Collections.Generic;
using System.Text;

namespace Drukarka3D.Controllers
{
    public class ProjectPrivacy
    {
        public string Id { get; set; }
        public string IsPrivate { get; set; }
    }//Dawid
    public class CanvasScreenshot
    {
        public string File { get; set; }
        public string ProjectName { get; set; }
    }//Dawid
    public class Like
    {
        public int OrderId { get; set; }
    }//Kamil

    public class Rating
    {
        public double Rate { get; set; }

        public string Id { get; set; }
    }//Kamil

    public class FileToPrint
    {
        public string FilePath { get; set; }
    }//Dominik
}
