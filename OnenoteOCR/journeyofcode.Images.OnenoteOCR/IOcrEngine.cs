using System;
using System.Drawing;
using System.Linq;

namespace journeyofcode.Images.OnenoteOCR
{
    public interface IOcrEngine
    {
        string Recognize(Image image);
    }
}
