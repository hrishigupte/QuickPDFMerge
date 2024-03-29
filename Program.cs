﻿// See https://aka.ms/new-console-template for more information
using System.Text;
using iText.Kernel.Exceptions;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;


if ((args == null) || (args.Length == 0))
{
    Console.WriteLine("Arguments cannot be null");
    Console.WriteLine("Exiting");
    return;
}

if ((args.Length <= 2))
{
    Console.WriteLine("Provide atleast two input files and an output file to add Documents to");
    Console.WriteLine("Exiting");
    return;
}

string Outfile = "";
int inputparamlength = args.Length - 2;

Outfile = args[inputparamlength + 1];

if (File.Exists(Outfile))
{
    Console.WriteLine("Output file already exists...Please specify a valid output file name.. Overwrite (yes/no):");
    string? choice = Console.ReadLine();
    if ((String.IsNullOrEmpty(choice)) || (choice.Trim().ToLower() == "no"))
    {
        Console.WriteLine("Exiting");
        return;
    }
}
MergeFiles(Outfile, args);

static void MergeFiles(string Outfile, string[] args)
{
    Console.WriteLine("Would you like to protect the output PDF file with a password? (yes/no):");
    string? pwdchoice = Console.ReadLine();
    string finalpwd = "";
    WriterProperties props = new WriterProperties();
    bool ispropused = false;
    if ((String.IsNullOrEmpty(pwdchoice)) || (pwdchoice.Trim().ToLower() == "no") || (pwdchoice.Trim().ToLower() != "yes"))
    {
        Console.WriteLine("File will not be password protected");
    }
    else
    {
        string? pwd, pwdmatch;
        bool dopwdsmatch = false;

        while (!dopwdsmatch)
        {
            Console.WriteLine("Enter you password ");
            pwd = Console.ReadLine();
            Console.WriteLine("Enter your password again");
            pwdmatch = Console.ReadLine();
            if ((String.IsNullOrEmpty(pwd)) || (String.IsNullOrEmpty(pwdmatch)))
                continue;
            else if (pwd.Trim() == pwdmatch.Trim())
            {
                dopwdsmatch = true;
                finalpwd = pwd.ToString();
            }
        }
        try
        {
            byte[] finalpwdBytes = Encoding.UTF8.GetBytes(finalpwd);
            props.SetStandardEncryption(finalpwdBytes, finalpwdBytes, EncryptionConstants.ALLOW_PRINTING, EncryptionConstants.ENCRYPTION_AES_256);
            ispropused = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Encountered Exception while trying to set password for file " + ex.ToString());
        }
    }
    try
    {
        using (FileStream fs = File.Open(Outfile, FileMode.Create))
        {
            PdfDocument pdfoutdoc;
            if (!ispropused)
            {
                pdfoutdoc = new PdfDocument(new PdfWriter(fs));
            }
            else
            {
                pdfoutdoc = new PdfDocument(new PdfWriter(fs, props));
            }
            PdfMerger pdfMerger = new PdfMerger(pdfoutdoc);
            PdfDocument pdfindoc;
            PdfReader pdfinreader;
            string? inputpwd;
            for (int i = 0; i <= args.Length - 2; i++)
            {
                try
                {
                    using (FileStream fsinput = File.Open(args[i], FileMode.Open, FileAccess.Read))
                    {
                        ReaderProperties rdprops = new ReaderProperties();
                        pdfinreader = new PdfReader(fsinput, rdprops);
                        inputpwd = "";
                        try
                        {
                            pdfindoc = new PdfDocument(pdfinreader);
                        }
                        catch (BadPasswordException)
                        {
                            Console.WriteLine("The input document to be merged is encrypted, please enter the password for input file : " + args[i]);
                            inputpwd = Console.ReadLine();
                            rdprops.SetPassword(Encoding.UTF8.GetBytes(inputpwd));
                            if (pdfinreader.IsCloseStream())
                            {
                                pdfinreader.SetCloseStream(false);
                            }
                            pdfinreader.Close();
                            fsinput.Position = 0;
                            pdfinreader = new PdfReader(fsinput, rdprops);
                            pdfindoc = new PdfDocument(pdfinreader);
                        }
                        pdfMerger.Merge(pdfindoc, 1, pdfindoc.GetNumberOfPages());
                        pdfindoc.Close();
                        pdfinreader.Close();
                    }
                }
                catch (FileNotFoundException fnex)
                {
                    Console.WriteLine(" Specified input file could not be found " + fnex.ToString());
                }
                catch (Exception gex)
                {
                    Console.WriteLine("Skippping file due to exception " + args[i] + " Exception : " + gex.ToString());
                }
            }
            pdfoutdoc.Close();
            Console.WriteLine("Successfully merged documents ");
            Console.WriteLine("Output file : " + Outfile.ToString());
        }
    }
    catch (FieldAccessException faex)
    {
        Console.WriteLine("Unable to access Output File " + faex.ToString());
    }

}

