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
                outputFileName = String.Format("{0:00}.jpg",i);
                outputFileName = Path.Combine(dir2,outputFileName);
                var res = PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES)) as PdfDictionary;
                var xobj = PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)) as PdfDictionary;
                if (xobj==null) continue;
                var keys = xobj.Keys;
                if (keys.Count==0) continue;
                //foreach (var k in keys)
                //    Console.WriteLine(k);
                string width ="",height="",bitspc="",filter="";
                foreach (var nm in keys)
                {
                //PdfName nm = keys.OfType<PdfName>().SingleOrDefault();
                    var obj = xobj.Get((PdfName)nm);
                    if (!obj.IsIndirect()) continue;
                    var tg = PdfReader.GetPdfObject(obj) as PdfDictionary;
                    var type = tg.Get(PdfName.SUBTYPE) as PdfName;
                    
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
                    
                    if (filter=="/DCTDecode")
                    {
                        File.WriteAllBytes(outputFileName,data);
                    }
                    else if (filter=="/JBIG2Decode")
                    {
                      
                        int wd = int.Parse(width);
                        int ht = int.Parse(height);
                        int bpp = int.Parse(bitspc);
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
                            using (MemoryStream bmpstream = new MemoryStream())
                            {
                                bmp.Save(bmpstream,ImageFormat.Tiff);
                                File.WriteAllBytes(outputFileName.Replace("jpg","tiff"),bmpstream.GetBuffer());
                                bmpstream.Close();
                            }
                        }
                      
                     
                       //File.WriteAllBytes(outputFileName.Replace("jpg","jbig2"),ldata);
                       
                    }
                    else 
                    {
                        var bytedata = PdfReader.FlateDecode(data);
                        File.WriteAllBytes(outputFileName,bytedata);
                    }
                    
                    /* MemoryStream stream = new MemoryStream(data);
                    //var image = new MagickImage(stream);
                    //image.Write(outputFileName);
                    Bitmap bmp = new Bitmap(stream);
                    //bmp.Save(outputFileName);
                    using (MemoryStream mspng = new MemoryStream())
                    {
                        bmp.Save(mspng,System.Drawing.Imaging.ImageFormat.Png);
                        int l = (int)mspng.Length;
                        byte[] pngdata = new byte[l];
                        mspng.Read(pngdata,0,l);
                        File.WriteAllBytes(outputFileName,pngdata);
                        mspng.Close();
                        stream.Close();
                    }
                    //File.WriteAllBytes(outputFileName,data); */
                }
            }
            
           // return null;

        }
    }
}