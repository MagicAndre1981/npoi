﻿/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */
namespace TestCases.XWPF
{
    using ICSharpCode.SharpZipLib.Zip;
    using NPOI;
    using NPOI.HSSF.Record.Crypto;
    using NPOI.OpenXmlFormats.Wordprocessing;
    using NPOI.POIFS.FileSystem;
    using NPOI.Util;
    using NPOI.XWPF;
    using NPOI.XWPF.UserModel;
    using NUnit.Framework;using NUnit.Framework.Legacy;
    using System;
    using System.IO;
    using System.Xml;
    using TestCases;

    [TestFixture]
    public class TestXWPFBugs
    {
        [Test]
        public void Bug55802()
        {
            String blabla =
                "Bir, iki, \u00fc\u00e7, d\u00f6rt, be\u015f,\n" +
                "\nalt\u0131, yedi, sekiz, dokuz, on.\n" +
                "\nK\u0131rm\u0131z\u0131 don,\n" +
                "\ngel bizim bah\u00e7eye kon,\n" +
                "\nsar\u0131 limon";
            XWPFDocument doc = new XWPFDocument();
            XWPFRun run = doc.CreateParagraph().CreateRun();

            foreach (String str in blabla.Split("\n".ToCharArray()))
            {
                run.SetText(str);
                run.AddBreak();
            }

            run.FontFamily = (/*setter*/"Times New Roman");
            run.FontSize = (/*setter*/20);
            ClassicAssert.AreEqual(run.FontFamily, "Times New Roman");
            ClassicAssert.AreEqual(run.GetFontFamily(FontCharRange.CS), "Times New Roman");
            ClassicAssert.AreEqual(run.GetFontFamily(FontCharRange.EastAsia), "Times New Roman");
            ClassicAssert.AreEqual(run.GetFontFamily(FontCharRange.HAnsi), "Times New Roman");
            run.SetFontFamily("Arial", FontCharRange.HAnsi);
            ClassicAssert.AreEqual(run.GetFontFamily(FontCharRange.HAnsi), "Arial");

            doc.Close();
        }
        /**
         * A word document that's encrypted with non-standard
         *  Encryption options, and no cspname section. See bug 53475
         */
        [Ignore("encryption function need re port from poi")]
        [Test]
        public void Test53475()
        {
            try
            {
                Biff8EncryptionKey.CurrentUserPassword = (/*setter*/"solrcell");
                FileStream file = POIDataSamples.GetDocumentInstance().GetFile("bug53475-password-is-solrcell.docx");
                NPOIFSFileSystem filesystem = new NPOIFSFileSystem(file,null, true, true);
/*
                // Check the encryption details
                EncryptionInfo info = new EncryptionInfo(filesystem);
                ClassicAssert.AreEqual(128, info.Header.KeySize);
                ClassicAssert.AreEqual(EncryptionHeader.ALGORITHM_AES_128, info.Header.Algorithm);
                ClassicAssert.AreEqual(EncryptionHeader.HASH_SHA1, info.Header.HashAlgorithm);

                // Check it can be decoded
                Decryptor d = Decryptor.GetInstance(info);
                ClassicAssert.IsTrue("Unable to Process: document is encrypted", d.VerifyPassword("solrcell"));

                // Check we can read the word document in that
                InputStream dataStream = d.GetDataStream(filesystem);
                OPCPackage opc = OPCPackage.Open(dataStream);
                XWPFDocument doc = new XWPFDocument(opc);
                XWPFWordExtractor ex = new XWPFWordExtractor(doc);
                String text = ex.Text;
                ClassicAssert.IsNotNull(text);
                ClassicAssert.AreEqual("This is password protected Word document.", text.Trim());
                ex.Close();
 */
                filesystem.Close();
            }
            finally
            {
                Biff8EncryptionKey.CurrentUserPassword = (/*setter*/null);
            }
        }
        [Test]
        public void Bug57495_getTableArrayInDoc()
        {
            XWPFDocument doc = new XWPFDocument();
            //let's create a few tables for the test
            for (int i = 0; i < 3; i++)
            {
                doc.CreateTable(2, 2);
            }
            XWPFTable table = doc.GetTableArray(0);
            ClassicAssert.IsNotNull(table);
            //let's check also that returns the correct table
            XWPFTable same = doc.Tables[0];
            ClassicAssert.AreEqual(table, same);
        }
        [Test]
        public void Bug57495_getParagraphArrayInTableCell()
        {
            XWPFDocument doc = new XWPFDocument();
            //let's create a table for the test
            XWPFTable table = doc.CreateTable(2, 2);
            ClassicAssert.IsNotNull(table);
            XWPFParagraph p = table.GetRow(0).GetCell(0).GetParagraphArray(0);
            ClassicAssert.IsNotNull(p);
            //let's check also that returns the correct paragraph
            XWPFParagraph same = table.GetRow(0).GetCell(0).Paragraphs[0];
            ClassicAssert.AreEqual(p, same);
        }

        [Test]
        public void Bug57495_convertPixelsToEMUs()
        {
            int pixels = 100;
            int expectedEMU = 952500;
            int result = Units.PixelToEMU(pixels);
            ClassicAssert.AreEqual(expectedEMU, result);
        }
        [Test]
        public void Bug57312_NullPointException()
        {
            XWPFDocument doc = XWPFTestDataSamples.OpenSampleDocument("57312.docx");
            ClassicAssert.IsNotNull(doc);

            foreach (IBodyElement bodyElement in doc.BodyElements)
            {
                BodyElementType elementType = bodyElement.ElementType;

                if (elementType == BodyElementType.PARAGRAPH)
                {
                    XWPFParagraph paragraph = (XWPFParagraph)bodyElement;

                    foreach (IRunElement iRunElem in paragraph.IRuns)
                    {

                        if (iRunElem is XWPFRun)
                        {
                            XWPFRun RunElement = (XWPFRun)iRunElem;

                            UnderlinePatterns underline = RunElement.Underline;
                            ClassicAssert.IsNotNull(underline);

                            //System.out.Println("Found: " + underline + ": " + RunElement.GetText(0));
                        }
                    }
                }
            }
        }

        [Test]
        public void Test56392()
        {
            XWPFDocument doc = XWPFTestDataSamples.OpenSampleDocument("56392.docx");
            ClassicAssert.IsNotNull(doc);
        }

        /**
         * Removing a run needs to remove it from both Runs and IRuns
         */
        [Test]
        public void Test57829()
        {
            XWPFDocument doc = XWPFTestDataSamples.OpenSampleDocument("sample.docx");
            ClassicAssert.IsNotNull(doc);
            ClassicAssert.AreEqual(3, doc.Paragraphs.Count);

            foreach (XWPFParagraph paragraph in doc.Paragraphs)
            {
                paragraph.RemoveRun(0);
                ClassicAssert.IsNotNull(paragraph.Text);
            }
        }
        /**
         * Removing a run needs to take into account position of run if paragraph contains hyperlink runs
         */
        [Test]
        public void Test58618()
        {
            XWPFDocument doc = XWPFTestDataSamples.OpenSampleDocument("58618.docx");
            XWPFParagraph para = (XWPFParagraph)doc.BodyElements[0];
            ClassicAssert.IsNotNull(para);
            ClassicAssert.AreEqual("Some text  some hyper links link link and some text.....", para.Text);
            XWPFRun run = para.InsertNewRun(para.Runs.Count);
            run.SetText("New Text");
            ClassicAssert.AreEqual("Some text  some hyper links link link and some text.....New Text", para.Text);
            para.RemoveRun(para.Runs.Count - 2);
            ClassicAssert.AreEqual("Some text  some hyper links link linkNew Text", para.Text);
        }
        [Test]
        public void Bug59058()
        {
            String[] files = { "bug57031.docx", "bug59058.docx" };
            foreach (String f in files)
            {
                ZipFile zf = new ZipFile(POIDataSamples.GetDocumentInstance().GetFile(f));
                ZipEntry entry = zf.GetEntry("word/document.xml");
                XmlDocument xml = POIXMLDocumentPart.ConvertStreamToXml(zf.GetInputStream(entry));
                DocumentDocument document = DocumentDocument.Parse(xml, POIXMLDocumentPart.NamespaceManager);
                ClassicAssert.IsNotNull(document);
                zf.Close();
            }
        }
        [Test]
        public void Test59378()
        {
            XWPFDocument doc = XWPFTestDataSamples.OpenSampleDocument("59378.docx");
            ByteArrayOutputStream out1 = new ByteArrayOutputStream();
            doc.Write(out1);
            out1.Close();
            XWPFDocument doc2 = new XWPFDocument(new ByteArrayInputStream(out1.ToByteArray()));
            doc2.Close();
            XWPFDocument docBack = XWPFTestDataSamples.WriteOutAndReadBack(doc);
            docBack.Close();
        }

        [Test]
        public void Test63788() {
            using (XWPFDocument doc = new XWPFDocument())
            {

                XWPFNumbering numbering = doc.CreateNumbering();

                for (int i = 10; i >= 0; i--) {
                    addNumberingWithAbstractId(numbering, i);        //add numbers in reverse order
                }

                for (int i = 0; i <= 10; i++) {
                    ClassicAssert.AreEqual(i, int.Parse(numbering.GetAbstractNum(i.ToString()).GetAbstractNum().abstractNumId));
                }

                //attempt to remove item with numId 2
                ClassicAssert.IsTrue(numbering.RemoveAbstractNum("2"));

                for (int i = 0; i <= 10; i++) {
                    XWPFAbstractNum abstractNum = numbering.GetAbstractNum(i.ToString());

                    // we removed id "2", so this one should be empty, all others not
                    if (i == 2) {
                        ClassicAssert.IsNull(abstractNum, "Failed for " + i);
                    } else {
                        ClassicAssert.IsNotNull(abstractNum, "Failed for " + i);
                        ClassicAssert.AreEqual(i, int.Parse(abstractNum.GetAbstractNum().abstractNumId));
                    }
                }

                // removing the same again fails
                ClassicAssert.IsFalse(numbering.RemoveAbstractNum("2"));

                // removing another one works
                ClassicAssert.IsTrue(numbering.RemoveAbstractNum("4"));
            }
        }

        private static void addNumberingWithAbstractId(XWPFNumbering documentNumbering, int id)
        {
            // create a numbering scheme
            CT_AbstractNum cTAbstractNum = new CT_AbstractNum();
            // give the scheme an ID
            cTAbstractNum.abstractNumId = id.ToString();

            XWPFAbstractNum abstractNum = new XWPFAbstractNum(cTAbstractNum);
            string abstractNumID = documentNumbering.AddAbstractNum(abstractNum);

            documentNumbering.AddNum(abstractNumID);
        }
    }
}

