using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;

namespace PDFMerge
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new string[] { @"s:\hrishi-shams\temp\visa\hrishi\i797\1797_Dec_2010.pdf", @"s:\hrishi-shams\temp\visa\hrishi\i797\i797_Nov_2008.pdf", @"c:\users\hrishi\documents\combined.pdf" };
            //args = new string[] { @"s:\hrishi-shams\temp\visa\C.pdf", @"s:\hrishi-shams\temp\S.pdf",
            //    @"c:\users\hrishi\documents\combined.pdf" };
            if ((args ==null)||(args.Length==0))
            {
                Console.WriteLine("Arguments cannot be null");
                Console.ReadLine();
                return;
            }
            string Outfile = "";
            int inputparamlength = args.Length - 2;
            if ((args[0]=="-e") && (args.Length>1))
            {
                //inputparamlength = args.Length - 1;
                string inputfile = args[1];
                new ImageExtractor().ExtractImages(inputfile);
                return;
            }
            Outfile = args[inputparamlength];
            //string Outfile = args[args.Length-1];

            //List<string> inputFiles = new List<string>();

            


            using (FileStream stream = new FileStream(Outfile, FileMode.Create))
            {
                Document doc = new Document();
                PdfCopy pdf = new PdfCopy(doc, stream);
                doc.Open();
                

                PdfReader reader = null;
                PdfImportedPage page = null;

                for (int i = 0; i <= inputparamlength; i++)
                {
                    if (args[i].Contains("-")) 
                        continue;
                    reader = new PdfReader(args[i]);

                    for (int j=0; j< reader.NumberOfPages;j++)                
                    {
                        page = pdf.GetImportedPage(reader, j+1);
                        pdf.AddPage(page);
                        
                    }
                    pdf.FreeReader(reader);
                    reader.Close();                    
                }
                pdf.Close();
                doc.Close();
            }
        }
    }
}
