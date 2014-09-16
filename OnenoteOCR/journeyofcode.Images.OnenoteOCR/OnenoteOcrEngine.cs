using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Office.Interop.OneNote;

namespace journeyofcode.Images.OnenoteOCR
{
    public sealed class OnenoteOcrEngine
        : IOcrEngine, IDisposable
    {
        private const int PollInterval = 250;
        private const int PollAttempts = 2;

        private readonly OnenotePage _page;

        public OnenoteOcrEngine()
        {
            var app = this.Try(() => new Application(), e => new OcrException("Error initializing OneNote", e));
            this._page = this.Try(() => new OnenotePage(app), e => new OcrException("Error initializing page.", e));
        }

        public string Recognize(Image image)
        {
            return this.Try(() => this.RecognizeIntern(image), e => new OcrException("Error during recognition", e));
        }

        private string RecognizeIntern(Image image)
        {
            this._page.Reload();

            this._page.Clear();
            this._page.AddImage(image);

            this._page.Save();

            int total = 0;
            do
            {
                Thread.Sleep(PollInterval);

                this._page.Reload();

                string result = this._page.ReadOcrText();
                if (result != null)
                    return result;
            } while (total++ < PollAttempts);

            return null;
        }

        private T Try<T>(Func<T> action, Func<Exception, Exception> excecption)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                throw excecption(ex);
            }
        }

        public void Dispose()
        {
            if (this._page == null)
                return;

            this._page.Delete();
        }
    }

    internal sealed class OnenotePage
    {
        private const String OneNoteNamespace = "http://schemas.microsoft.com/office/onenote/2013/onenote";
        private const String DefaultOutline = "<one:Outline xmlns:one=\"" + OneNoteNamespace + "\"><one:OEChildren><one:OE><one:T><![CDATA[A]]></one:T></one:OE></one:OEChildren></one:Outline>";

        private readonly Application _app;
        private XDocument _document;
        private string _pageId;

        private XElement Oe
        {
            get
            {
                var outline = this._document.Root.Element(XName.Get("Outline", OneNoteNamespace));
                if (outline == null)
                {
                    outline = XElement.Parse(DefaultOutline);
                    this._document.Root.Add(outline);
                }

                var children = outline.Element(XName.Get("OEChildren", OneNoteNamespace));
                return children.Element(XName.Get("OE", OneNoteNamespace));
            }
        }

        public OnenotePage(Application app)
        {
            this._app = app;
            
            this.LoadOrCreatePage();
        }

        public void Reload()
        {
            string strXml;
            this._app.GetPageContent(this._pageId, out strXml, PageInfo.piBinaryData);

            this._document = XDocument.Parse(strXml);
        }

        public void Clear()
        {
            this.Oe.Elements().Remove();
        }

        public void Delete()
        {
            this._app.DeleteHierarchy(this._pageId);
        }

        public void AddImage(Image image)
        {
            var img = this.CreateImageTag(image);
            this.Oe.Add(img);
        }
        
        public string ReadOcrText()
        {
            var img = this.Oe.Element(XName.Get("Image", OneNoteNamespace));
            var ocrData = img.Element(XName.Get("OCRData", OneNoteNamespace));

            if (ocrData == null)
                return null;

            var ocrText = ocrData.Element(XName.Get("OCRText", OneNoteNamespace)).Value;
            return ocrText;
        }


        public void Save()
        {
            var xml = this._document.ToString();
            this._app.UpdatePageContent(xml);

            this._app.NavigateTo(this._pageId);
        }

        private XElement CreateImageTag(Image image)
        {
            var img = new XElement(XName.Get("Image", OneNoteNamespace));
            
            var data = new XElement(XName.Get("Data", OneNoteNamespace));
            data.Value = this.ToBase64(image);
            img.Add(data);

            return img;
        }

        private string ToBase64(Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Png);

                var binary = memoryStream.ToArray();
                return Convert.ToBase64String(binary);
            }
        }
        
        private void LoadOrCreatePage()
        {
            string hierarchy;
            this._app.GetHierarchy(String.Empty, HierarchyScope.hsPages, out hierarchy);

            var doc = XDocument.Parse(hierarchy);
            var section = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Section"));
            if (section == null)
                throw new OcrException("No section found");

            var sectionId = section.Attribute("ID").Value;
            this._app.CreateNewPage(sectionId, out this._pageId);

            this.Reload();
        }
    }
}
