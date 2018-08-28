using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using System.Drawing;

namespace PDFMerge
{
    public class ImageExtractor 
    {

        public void ExtractImages(string inputFile)
        {
            string outputFileName = "";
            var dir1 = Path.GetDirectoryName(inputFile);
            var filename = Path.GetFileNameWithoutExtension(inputFile);
            var dir2 = Path.Combine(dir1,filename);
            if (!Directory.Exists(dir2))
            {
                Directory.CreateDirectory(dir2);
            }

            var pdfreader = new PdfReader(inputFile);

            int n = pdfreader.NumberOfPages;

            for (int i =1; i<=n;i++)
            {
                var pg = pdfreader.GetPageN(i);
                outputFileName = String.Format("{0:00}.jpg",i);
                outputFileName = Path.Combine(dir2,outputFileName);
                var res = PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES)) as PdfDictionary;
                var xobj = PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)) as PdfDictionary;
                if (xobj==null) continue;
                var keys = xobj.Keys;
                if (keys.Count==0) continue;
                foreach (var k in keys)
                    Console.WriteLine(k);
                PdfName nm = keys.OfType<PdfName>().SingleOrDefault();
                var obj = xobj.Get(nm);
                if (!obj.IsIndirect()) continue;

                var tg = PdfReader.GetPdfObject(obj) as PdfDictionary;
                var type = PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE)) as PdfName;
                var filter = PdfReader.GetPdfObject(tg.Get(PdfName.FILTER)).ToString();
                Console.WriteLine(filter);
                
                if (!PdfName.IMAGE.Equals(type)) continue;
                Console.WriteLine(filter);
                if (!(filter=="/DCTDecode"||filter=="/FlatDecode"))
                    continue;

                int xrefIndex = (obj as PRIndirectReference).Number;
                var pdfStream = pdfreader.GetPdfObject(xrefIndex) as PRStream;
                var data = PdfReader.GetStreamBytesRaw(pdfStream);
                File.WriteAllBytes(outputFileName,data);

            }
            
           // return null;

        }
    }
}