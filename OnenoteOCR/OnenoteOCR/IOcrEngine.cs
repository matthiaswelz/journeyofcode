using System;
using System.Linq;
using System.Drawing;

namespace OnenoteOCR
{
    public interface IOcrEngine
    {
        string Recognize(Image image);
    }
}
