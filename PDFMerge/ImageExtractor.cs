using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
                iTextSharp.text.Rectangle page = pdfreader.GetCropBox(i);
                float pageHeight =  page.Height;
                float pageWidth = page.Width;

                outputFileName = String.Format("{0:00}.jpg",i);
                outputFileName = Path.Combine(dir2,outputFileName);
                var res = PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES)) as PdfDictionary;
                var xobj = PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)) as PdfDictionary;
                if (xobj==null) continue;
                var keys = xobj.Keys;
                if (keys.Count==0) continue;
                string width ="",height="",bitspc="",filter="",colorspace="",length="";
                int iteration = 0;
                List<string> dtcbmplist=new List<string>();
                foreach (var nm in keys)
                {
                //PdfName nm = keys.OfType<PdfName>().SingleOrDefault();
                    var obj = xobj.Get((PdfName)nm);
                    if (!obj.IsIndirect()) continue;
                    var tg = PdfReader.GetPdfObject(obj) as PdfDictionary;
                    var type = tg.Get(PdfName.SUBTYPE) as PdfName;
                    outputFileName = String.Format("{0:00}.jpg", i);
                    outputFileName = Path.Combine(dir2, outputFileName);
                    foreach (PdfName t in tg.Keys)
                    {
                       // Console.WriteLine(t);
                       switch (t.ToString().ToUpper())
                       {
                            case "/WIDTH":
                                width = tg.Get(t).ToString();
                                break;
                            case "/HEIGHT":
                                height = tg.Get(t).ToString();
                                break;
                            case "/FILTER":
                                filter = tg.Get(t).ToString();
                                break;
                            case "/BITSPERCOMPONENT":
                                bitspc = tg.Get(t).ToString();
                                break;
                            case "/COLORSPACE":
                                colorspace = tg.Get(t).ToString();
                                break;
                            case "/LENGTH":
                                length = tg.Get(t).ToString();
                                break;
                            default:
                                break;
                       }

                    }
                    
                    
                    if (!PdfName.IMAGE.Equals(type)) continue;
                    //if (!(filter=="/DCTDecode"||filter=="/FlatDecode" ||filter.Contains("Obj")))
                    //    continue;

                    int xrefIndex = (obj as PRIndirectReference).Number;
                    var pdfStream = pdfreader.GetPdfObject(xrefIndex) as PRStream;
                    var data = PdfReader.GetStreamBytesRaw(pdfStream);
                    //System.Drawing.Image srcimg;
                    if (filter == "/DCTDecode")
                    {
                        //File.WriteAllBytes(outputFileName,data);
                        string ot = String.Format("{0:00}{1}.jpg", i,iteration);
                        ot = Path.Combine(dir2, ot);
                        File.WriteAllBytes(ot, data);
                        dtcbmplist.Add(ot);
                        iteration++;

                    }
                    else if ((filter=="/JBIG2Decode"))
                    {
                      
                        int wd = int.Parse(width);
                        int ht = int.Parse(height);
                        int bpp = int.Parse(bitspc);
                        Bitmap src;
                        PixelFormat format = PixelFormat.Format1bppIndexed;
                        switch (bpp)
                        {
                            case 1: 
                                format = PixelFormat.Format1bppIndexed;
                                break;
                            case 8:
                                format = PixelFormat.Format8bppIndexed;
                                break;
                            case 24:
                                format = PixelFormat.Format24bppRgb;
                                break;
                            default:
                                break;
                        }
                        
                        using (Bitmap bmp = new Bitmap(wd,ht,format))
                        {
                            BitmapData bmd = bmp.LockBits(new System.Drawing.Rectangle(0,0,wd,ht),ImageLockMode.WriteOnly,format);
                            Marshal.Copy(data,0,bmd.Scan0,data.Length);
                            bmp.UnlockBits(bmd);
                            bmp.Save(outputFileName, ImageFormat.Jpeg);
                        }
                       
                    }
                    else 
                    {
                        var bytedata = PdfReader.FlateDecode(data);
                        File.WriteAllBytes(outputFileName.Replace("jpg","png"),bytedata);
                    }
                }
                if (dtcbmplist.Count>0)
                {
                    int imageWidth = 0, imageHeigth = 0;
                    Bitmap b = new Bitmap((int)pageWidth,(int) pageHeight);
                    Graphics g = Graphics.FromImage(b);
                    int localWidth = 0;
                    //MemoryStream s = new MemoryStream();
                    foreach (string fl in dtcbmplist)
                    {
                        Bitmap btc = (Bitmap)Bitmap.FromFile(fl);
                        g.DrawImage(btc,new Point(localWidth,0));
                        localWidth += btc.Width;
                    }
                    g.Flush();
                    b.Save(outputFileName);
                }
            }
            
           // return null;

        }
    }
}