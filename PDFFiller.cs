using System;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Collections.Generic;


    /// <summary> 
    /// Summary description for PDFFiller 
    /// </summary> 
    public class PDFFiller
    {
        private ArrayList PFDlocs;

        /// <summary> 
        /// Fill PDF form & Merge PDF 
        /// </summary> 
        public PDFFiller()
        {

            PFDlocs = new ArrayList();
        }

        private void GetFormFields(string source)
        {
            GetFormFields(File.OpenRead(source));
        }

        private void GetFormFields(Stream source) 
        { 
            PdfReader reader = null; 
            try { 
                reader = new PdfReader(source); 
                PRAcroForm form = reader.AcroForm; 
                if (form == null) { 
                    //ac.debugText("This document has no fields."); 
                    return; 
                } 
                //PdfLister list = new PdfLister(System.out); 
                Hashtable refToField = new Hashtable(); 
                ArrayList fields = form.Fields; 
                foreach (PRAcroForm.FieldInformation field in fields) { 
                    refToField.Add(field.Ref.Number, field); 
                } 
                for (int page = 1; page <= reader.NumberOfPages; page++) { 
                    
                    PdfDictionary dPage = reader.GetPageN(page); 
                    PdfArray annots = (PdfArray)PdfReader.GetPdfObject((PdfObject)dPage.Get(PdfName.ANNOTS)); 
                    if (annots == null) {
                        break;
                    } 
                    
                    ArrayList ali = annots.ArrayList; 
                    PRIndirectReference iRef = null; 
                    foreach (PdfObject refObj in ali) { 
                        PdfDictionary an = (PdfDictionary)PdfReader.GetPdfObject(refObj); 
                        PdfName name = (PdfName)an.Get(PdfName.SUBTYPE); 
                        if (name == null || !name.Equals(PdfName.WIDGET)) { 
                            break;
                        } 
                        
                        
                        PdfArray rect = (PdfArray)PdfReader.GetPdfObject(an.Get(PdfName.RECT)); 
                        
                        string fName = ""; 
                        
                        PRAcroForm.FieldInformation field = null; 
                        
                        while ((an != null)) { 
                            PdfString tName = (PdfString)an.Get(PdfName.T); 
                            if ((tName != null)) { 
                                fName = tName.ToString() + "." + fName; 
                            } 
                            if (refObj.IsIndirect() && field == null) { 
                                iRef = (PRIndirectReference)refObj; 
                                field = (PRAcroForm.FieldInformation)refToField[iRef.Number]; 
                            } 
                            //refObj = (PdfObject)an.Get(PdfName.PARENT); 
                            an = (PdfDictionary)PdfReader.GetPdfObject((PdfObject)an.Get(PdfName.PARENT)); 
                        } 
                        if (fName.EndsWith(".")) { 
                            fName = fName.Substring(0, fName.Length - 1); 
                        } 
                        
                        PDFFieldLocation tempLoc = new PDFFieldLocation(); 
                        ArrayList arr = rect.ArrayList; 
                        
                        tempLoc.fieldName = fName; 
                        tempLoc.page = page; 
                        tempLoc.x1 = ((PdfNumber)PdfReader.GetPdfObject((PdfObject)arr[0])).FloatValue; 
                        tempLoc.y1 = ((PdfNumber)PdfReader.GetPdfObject((PdfObject)arr[1])).FloatValue; 
                        tempLoc.x2 = ((PdfNumber)PdfReader.GetPdfObject((PdfObject)arr[2])).FloatValue; 
                        tempLoc.y2 = ((PdfNumber)PdfReader.GetPdfObject((PdfObject)arr[3])).FloatValue; 
                        
                        this.PFDlocs.Add(tempLoc); 
                    } 
                } 
            } 
            catch (Exception e) { 
                throw new Exception("Critical Exception in GetFormFields", e); 
            } 
            finally { 
                if ((reader != null)) { 
                    reader.Close(); 
                } 
            } 
        }


        private ArrayList FindFieldLocs(string fieldname)
        {
            //ac.debugText("LOOKING FOR FIELDNAME:" + fieldname); 
            ArrayList tempVec = new ArrayList();
            //ac.debugText("\nTHIS MANY ITEMS TO CHECK:" + this.PFDlocs.size()); 
            foreach (PDFFieldLocation field in this.PFDlocs)
            {
                if (field.fieldName.Equals(fieldname))
                {
                    //ac.debugText("\nFOUND:" + fieldname); 
                    tempVec.Add(field);
                }
            }
            return tempVec;
        }

        private bool ReplaceImage(PDFField field, PdfStamper stamp)
        {
            //ac.debugText("\nreplacin the image:" + field.getValue()); 
            ArrayList fields = this.FindFieldLocs(field.Name);
            if (fields.Count > 0)
            {
                //ac.debugText("\n\nfields:" + fields.size()); 
                try
                {
                    //File fImg = new File(field.Value); 
                    PdfContentByte cb = null;
                    for (int x = 0; x <= fields.Count - 1; x++)
                    {
                        // = stamp.GetOverContent(1); 
                        PDFFieldLocation fieldLoc = (PDFFieldLocation)fields[x];
                        //ac.debugText("\n\n(" + fieldLoc.x1 + "," + fieldLoc.y1 + ") \n\t\tWidth:" + (fieldLoc.x2 - fieldLoc.x1) + "\n\t\tHeight:" + (fieldLoc.y2 - fieldLoc.y1)); 
                        cb = stamp.GetOverContent(fieldLoc.page);
                        Image img = Image.GetInstance(field.ImageValue);
                        // (fImg.toURL()); 
                        img.SetAbsolutePosition(fieldLoc.x1, fieldLoc.y1);
                        img.ScaleAbsolute((fieldLoc.x2 - fieldLoc.x1), (fieldLoc.y2 - fieldLoc.y1));

                        cb.AddImage(img);
                    }

                    return true;
                }
                // e.printStackTrace(); 
                catch (Exception e)
                {
                    throw new Exception("Critical Exception thrown within ReplaceImage", e);
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary> 
        /// Fill Pdf 
        /// </summary> 
        /// <param name="TemplatePath">pdf form template path</param> 
        /// <param name="Fields">ArrayList of PDFFiled instance</param> 
        /// <returns>Output Pdf byte()</returns> 
        public byte[] FillPDF(string TemplatePath, List<PDFField> Fields)
        {

            return FillPDF(File.OpenRead(TemplatePath), Fields);
        }


        /// <summary> 
        /// Fill Pdf 
        /// </summary> 
        /// <param name="TempStream">pdf form template byte()</param> 
        /// <param name="fields">ArrayList of PDFFiled instance</param> 
        /// <returns>Output Pdf byte()</returns> 
        public byte[] FillPDF(Stream TempStream, List<PDFField> fields)
        {

            MemoryStream OutputStream = new MemoryStream();
            PdfReader reader = null;
            try
            {
                reader = new PdfReader(TempStream);

                TempStream.Position = 0;
                GetFormFields(TempStream);
                TempStream.Close();

                PdfStamper stamp = new PdfStamper(reader, OutputStream);
                stamp.Writer.CloseStream = false;

                AcroFields form = stamp.AcroFields;

                //Hashtable formFields = form.Fields; 

                //set the field values in the pdf form 
                foreach (PDFField aField in fields)
                {
                    //if (aField.Type.Equals("TEXT")) 
                    // form.SetField(aField.Name, aField.Value); 
                    //else 
                    if (aField.Type.Equals(PDFFieldType.IMAGE))
                    {
                        this.ReplaceImage(aField, stamp);
                    }
                    else
                    {
                        form.SetField(aField.Name, aField.Value);
                        
                    }
                }

                stamp.FormFlattening = true;
                stamp.Close();

                //OutputStream.Position = 0; 
                return OutputStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Critical Exception thrown in PDFFIller", ex);
            }
            finally
            {
                if ((reader != null))
                {
                    reader.Close();
                }
            }
        }

        /// <summary> 
        /// Merge multi pdfs into one pdf 
        /// </summary> 
        /// <param name="Pieces">ArrayList of Pdf byte()</param> 
        /// <returns>Output Pdf byte()</returns> 
        public byte[] MergePdf(List<byte[]> Pieces)
        {
            MemoryStream OutputStream = new MemoryStream();
            if (Pieces.Count < 2)
            {
                //System.err.println("2 or more files are required"); 
                throw new Exception("2 or more files are required.");
            }
            else
            {
                try
                {
                    int pageOffset = 0;
                    ArrayList master = new ArrayList();
                    int f = 0;
                    //String outFile = this.rootPath + outputName; 
                    iTextSharp.text.Document document = null;
                    PdfCopy writer = null;
                    while (f < Pieces.Count)
                    {
                        // we create a reader for a certain document 
                        //ac.debugText("\nFile " + f + ": " + this.changeSlashType(pathType, (String)pieces.get(f))); 
                        //Dim reader As New PdfReader(TryCast(Pieces(f), byte())) 
                        PdfReader reader;
                        if ((Pieces[f] != null))
                        {
                            if (((!object.ReferenceEquals(Pieces[f].GetType(), typeof(int)))))
                            {
                                reader = new PdfReader((byte[])Pieces[f]);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }


                        reader.ConsolidateNamedDestinations();
                        // we retrieve the total number of pages 
                        int n = reader.NumberOfPages;
                        ArrayList bookmarks = SimpleBookmark.GetBookmark(reader);
                        if ((bookmarks != null))
                        {
                            if (pageOffset != 0)
                            {
                                SimpleBookmark.ShiftPageNumbers(bookmarks, pageOffset, null);
                            }
                            master.AddRange(bookmarks);
                        }
                        pageOffset += n;
                        //ac.debugText("\n\nThere are " + n + " pages in " + this.changeSlashType(pathType, (String)pieces.get(f)) + "\n\n"); 

                        if (f == 0)
                        {
                            // step 1: creation of a document-object 
                            document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
                            // step 2: we create a writer that listens to the document 
                            writer = new PdfCopy(document, OutputStream);
                            writer.CloseStream = false;
                            // step 3: we open the document 
                            document.Open();
                        }
                        // step 4: we add content 
                        int i = 0;
                        while (i < n)
                        {
                            PdfImportedPage page;
                            i += 1;
                            page = writer.GetImportedPage(reader, i);
                            //ac.debugText("Processed page " + i); 
                            writer.AddPage(page);
                        }
                        PRAcroForm form = reader.AcroForm;
                        if ((form != null))
                        {
                            writer.CopyAcroForm(reader);
                        }
                        f += 1;
                    }
                    if (master.Count > 0)
                    {
                        writer.Outlines = master;
                    }
                    // step 5: we close the document 
                    document.Close();
                }
                catch (Exception e)
                {
                    //e.printStackTrace(); 
                    //System.Diagnostics.Debug.WriteLine(e); 
                    throw new Exception("Crticial Exception in MergePdf", e);
                }
            }
            //OutputStream.Position = 0 
            return OutputStream.ToArray();
        }


    }