
/* ====================================================================
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


/*
 * TestCellStyle.java
 *
 * Created on December 11, 2001, 5:51 PM
 */
namespace TestCases.HSSF.UserModel
{
    using System;
    using System.IO;
    using NPOI.Util;
    using NPOI.HSSF.UserModel;


    using NUnit.Framework;using NUnit.Framework.Legacy;
    using TestCases.HSSF;
    using NPOI.SS.UserModel;
    using NPOI.HSSF.Util;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /**
     * Class to Test cell styling functionality
     *
     * @author Andrew C. Oliver
     */
    [TestFixture]
    public class TestCellStyle
    {

        private static HSSFWorkbook OpenSample(String sampleFileName)
        {
            return HSSFTestDataSamples.OpenSampleWorkbook(sampleFileName);
        }

        /** Creates a new instance of TestCellStyle */

        public TestCellStyle()
        {

        }

        /**
         * TEST NAME:  Test Write Sheet Font <P>
         * OBJECTIVE:  Test that HSSF can Create a simple spreadsheet with numeric and string values and styled with fonts.<P>
         * SUCCESS:    HSSF Creates a sheet.  Filesize Matches a known good.  NPOI.SS.UserModel.Sheet objects
         *             Last row, first row is Tested against the correct values (99,0).<P>
         * FAILURE:    HSSF does not Create a sheet or excepts.  Filesize does not Match the known good.
         *             NPOI.SS.UserModel.Sheet last row or first row is incorrect.             <P>
         *
         */
        [Test]
        public void TestWriteSheetFont()
        {
            string filepath = TempFile.GetTempFilePath("TestWriteSheetFont",
                                                        ".xls");
            FileStream out1 = new FileStream(filepath, FileMode.OpenOrCreate);
            HSSFWorkbook wb = new HSSFWorkbook();
            NPOI.SS.UserModel.ISheet s = wb.CreateSheet();
            IRow r = null;
            ICell c = null;
            IFont fnt = wb.CreateFont();
            NPOI.SS.UserModel.ICellStyle cs = wb.CreateCellStyle();

            fnt.Color = (HSSFColor.Red.Index);
            fnt.IsBold = true;
            cs.SetFont(fnt);
            for (short rownum = (short)0; rownum < 100; rownum++)
            {
                r = s.CreateRow(rownum);

                // r.SetRowNum(( short ) rownum);
                for (short cellnum = (short)0; cellnum < 50; cellnum += 2)
                {
                    c = r.CreateCell(cellnum);
                    c.SetCellValue(rownum * 10000 + cellnum
                                   + (((double)rownum / 1000)
                                      + ((double)cellnum / 10000)));
                    c = r.CreateCell(cellnum + 1);
                    c.SetCellValue("TEST");
                    c.CellStyle = (cs);
                }
            }
            wb.Write(out1);
            out1.Close();
            SanityChecker sanityChecker = new SanityChecker();
            sanityChecker.CheckHSSFWorkbook(wb);
            ClassicAssert.AreEqual(99, s.LastRowNum, "LAST ROW == 99");
            ClassicAssert.AreEqual(0, s.FirstRowNum, "FIRST ROW == 0");

            // assert((s.LastRowNum == 99));
        }

        /**
         * Tests that is creating a file with a date or an calendar works correctly.
         */
        [Test]
        public void TestDataStyle()

        {
            string filepath = TempFile.GetTempFilePath("TestWriteSheetStyleDate",
                                                        ".xls");
            FileStream out1 = new FileStream(filepath, FileMode.OpenOrCreate);
            HSSFWorkbook wb = new HSSFWorkbook();
            NPOI.SS.UserModel.ISheet s = wb.CreateSheet();
            NPOI.SS.UserModel.ICellStyle cs = wb.CreateCellStyle();
            IRow row = s.CreateRow(0);

            // with Date:
            ICell cell = row.CreateCell(1);
            cs.DataFormat = (HSSFDataFormat.GetBuiltinFormat("m/d/yy"));
            cell.CellStyle = (cs);
            cell.SetCellValue(DateTime.Now);

            // with Calendar:
            cell = row.CreateCell(2);
            cs.DataFormat = (HSSFDataFormat.GetBuiltinFormat("m/d/yy"));
            cell.CellStyle = (cs);
            cell.SetCellValue(DateTime.Now);

            wb.Write(out1);
            out1.Close();
            SanityChecker sanityChecker = new SanityChecker();
            sanityChecker.CheckHSSFWorkbook(wb);

            ClassicAssert.AreEqual(0, s.LastRowNum, "LAST ROW ");
            ClassicAssert.AreEqual(0, s.FirstRowNum, "FIRST ROW ");

        }
        [Test]
        public void TestHashEquals()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            NPOI.SS.UserModel.ISheet s = wb.CreateSheet();
            NPOI.SS.UserModel.ICellStyle cs1 = wb.CreateCellStyle();
            NPOI.SS.UserModel.ICellStyle cs2 = wb.CreateCellStyle();
            IRow row = s.CreateRow(0);
            ICell cell1 = row.CreateCell(1);
            ICell cell2 = row.CreateCell(2);

            cs1.DataFormat = (HSSFDataFormat.GetBuiltinFormat("m/d/yy"));
            cs2.DataFormat = (HSSFDataFormat.GetBuiltinFormat("m/dd/yy"));

            cell1.CellStyle = (cs1);
            cell1.SetCellValue(DateTime.Now);

            cell2.CellStyle = (cs2);
            cell2.SetCellValue(DateTime.Now);

            ClassicAssert.AreEqual(cs1.GetHashCode(), cs1.GetHashCode());
            ClassicAssert.AreEqual(cs2.GetHashCode(), cs2.GetHashCode());
            ClassicAssert.IsTrue(cs1.Equals(cs1));
            ClassicAssert.IsTrue(cs2.Equals(cs2));

            // Change cs1, hash will alter
            int hash1 = cs1.GetHashCode();
            cs1.DataFormat = (HSSFDataFormat.GetBuiltinFormat("m/dd/yy"));
            ClassicAssert.IsFalse(hash1 == cs1.GetHashCode());

            wb.Close();
        }

        /**
         * TEST NAME:  Test Write Sheet Style <P>
         * OBJECTIVE:  Test that HSSF can Create a simple spreadsheet with numeric and string values and styled with colors
         *             and borders.<P>
         * SUCCESS:    HSSF Creates a sheet.  Filesize Matches a known good.  NPOI.SS.UserModel.Sheet objects
         *             Last row, first row is Tested against the correct values (99,0).<P>
         * FAILURE:    HSSF does not Create a sheet or excepts.  Filesize does not Match the known good.
         *             NPOI.SS.UserModel.Sheet last row or first row is incorrect.             <P>
         *
         */
        [Test]
        public void TestWriteSheetStyle()
        {
            string filepath = TempFile.GetTempFilePath("TestWriteSheetStyle",
                                                        ".xls");
            FileStream out1 = new FileStream(filepath, FileMode.OpenOrCreate);
            HSSFWorkbook wb = new HSSFWorkbook();
            NPOI.SS.UserModel.ISheet s = wb.CreateSheet();
            IRow r = null;
            ICell c = null;
            IFont fnt = wb.CreateFont();
            NPOI.SS.UserModel.ICellStyle cs = wb.CreateCellStyle();
            NPOI.SS.UserModel.ICellStyle cs2 = wb.CreateCellStyle();

            cs.BorderBottom = (BorderStyle.Thin);
            cs.BorderLeft = (BorderStyle.Thin);
            cs.BorderRight = (BorderStyle.Thin);
            cs.BorderTop = (BorderStyle.Thin);
            cs.FillForegroundColor = (short)0xA;
            cs.FillPattern = FillPattern.SolidForeground;
            fnt.Color = (short)0xf;
            fnt.IsItalic = (true);
            cs2.FillForegroundColor = (short)0x0;
            cs2.FillPattern = FillPattern.SolidForeground;
            cs2.SetFont(fnt);
            for (short rownum = (short)0; rownum < 100; rownum++)
            {
                r = s.CreateRow(rownum);

                // r.SetRowNum(( short ) rownum);
                for (short cellnum = (short)0; cellnum < 50; cellnum += 2)
                {
                    c = r.CreateCell(cellnum);
                    c.SetCellValue(rownum * 10000 + cellnum
                                   + (((double)rownum / 1000)
                                      + ((double)cellnum / 10000)));
                    c.CellStyle = (cs);
                    c = r.CreateCell(cellnum + 1);
                    c.SetCellValue("TEST");
                    c.CellStyle = (cs2);
                }
            }
            wb.Write(out1);
            out1.Close();
            SanityChecker sanityChecker = new SanityChecker();
            sanityChecker.CheckHSSFWorkbook(wb);
            ClassicAssert.AreEqual(99, s.LastRowNum, "LAST ROW == 99");
            ClassicAssert.AreEqual(0, s.FirstRowNum, "FIRST ROW == 0");

            // assert((s.LastRowNum == 99));
        }

        /**
         * Cloning one NPOI.SS.UserModel.CellType onto Another, same
         *  HSSFWorkbook
         */
        [Test]
        public void TestCloneStyleSameWB()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            IFont fnt = wb.CreateFont();
            fnt.FontName = ("TestingFont");
            ClassicAssert.AreEqual(5, wb.NumberOfFonts);

            NPOI.SS.UserModel.ICellStyle orig = wb.CreateCellStyle();
            orig.Alignment = (HorizontalAlignment.Right);
            orig.SetFont(fnt);
            orig.DataFormat = ((short)18);

            ClassicAssert.AreEqual(HorizontalAlignment.Right, orig.Alignment);
            ClassicAssert.AreEqual(fnt, orig.GetFont(wb));
            ClassicAssert.AreEqual(18, orig.DataFormat);

            NPOI.SS.UserModel.ICellStyle clone = wb.CreateCellStyle();
            ClassicAssert.AreNotEqual(HorizontalAlignment.Right, clone.Alignment);
            ClassicAssert.AreNotEqual(fnt, clone.GetFont(wb));
            ClassicAssert.AreNotEqual(18, clone.DataFormat);

            clone.CloneStyleFrom(orig);
            ClassicAssert.AreEqual(HorizontalAlignment.Right, orig.Alignment);
            ClassicAssert.AreEqual(fnt, clone.GetFont(wb));
            ClassicAssert.AreEqual(18, clone.DataFormat);
            ClassicAssert.AreEqual(5, wb.NumberOfFonts);

            orig.Alignment = HorizontalAlignment.Left;
            ClassicAssert.AreEqual(HorizontalAlignment.Right, clone.Alignment);
        }

        /**
         * Cloning one NPOI.SS.UserModel.CellType onto Another, across
         *  two different HSSFWorkbooks
         */
        [Test]
        public void TestCloneStyleDiffWB()
        {
            HSSFWorkbook wbOrig = new HSSFWorkbook();

            IFont fnt = wbOrig.CreateFont();
            fnt.FontName = ("TestingFont");
            ClassicAssert.AreEqual(5, wbOrig.NumberOfFonts);

            IDataFormat fmt = wbOrig.CreateDataFormat();
            fmt.GetFormat("MadeUpOne");
            fmt.GetFormat("MadeUpTwo");

            NPOI.SS.UserModel.ICellStyle orig = wbOrig.CreateCellStyle();
            orig.Alignment = (HorizontalAlignment.Right);
            orig.SetFont(fnt);
            orig.DataFormat = (fmt.GetFormat("Test##"));

            ClassicAssert.AreEqual(HorizontalAlignment.Right, orig.Alignment);
            ClassicAssert.AreEqual(fnt, orig.GetFont(wbOrig));
            ClassicAssert.AreEqual(fmt.GetFormat("Test##"), orig.DataFormat);

            // Now a style on another workbook
            HSSFWorkbook wbClone = new HSSFWorkbook();
            ClassicAssert.AreEqual(4, wbClone.NumberOfFonts);
            IDataFormat fmtClone = wbClone.CreateDataFormat();

            NPOI.SS.UserModel.ICellStyle clone = wbClone.CreateCellStyle();
            ClassicAssert.AreEqual(4, wbClone.NumberOfFonts);

            ClassicAssert.AreNotEqual(HorizontalAlignment.Right, clone.Alignment);
            ClassicAssert.AreNotEqual("TestingFont", clone.GetFont(wbClone).FontName);

            clone.CloneStyleFrom(orig);
            ClassicAssert.AreEqual(HorizontalAlignment.Right, clone.Alignment);
            ClassicAssert.AreEqual("TestingFont", clone.GetFont(wbClone).FontName);
            ClassicAssert.AreEqual(fmtClone.GetFormat("Test##"), clone.DataFormat);
            ClassicAssert.AreNotEqual(fmtClone.GetFormat("Test##"), fmt.GetFormat("Test##"));
            ClassicAssert.AreEqual(5, wbClone.NumberOfFonts);
        }
        [Test]
        public void TestStyleNames()
        {
            HSSFWorkbook wb = OpenSample("WithExtendedStyles.xls");
            NPOI.SS.UserModel.ISheet s = wb.GetSheetAt(0);
            ICell c1 = s.GetRow(0).GetCell(0);
            ICell c2 = s.GetRow(1).GetCell(0);
            ICell c3 = s.GetRow(2).GetCell(0);

            HSSFCellStyle cs1 = (HSSFCellStyle)c1.CellStyle;
            HSSFCellStyle cs2 = (HSSFCellStyle)c2.CellStyle;
            HSSFCellStyle cs3 = (HSSFCellStyle)c3.CellStyle;

            ClassicAssert.IsNotNull(cs1);
            ClassicAssert.IsNotNull(cs2);
            ClassicAssert.IsNotNull(cs3);

            // Check we got the styles we'd expect
            ClassicAssert.AreEqual(10, cs1.GetFont(wb).FontHeightInPoints);
            ClassicAssert.AreEqual(9, cs2.GetFont(wb).FontHeightInPoints);
            ClassicAssert.AreEqual(12, cs3.GetFont(wb).FontHeightInPoints);

            ClassicAssert.AreEqual(15, cs1.Index);
            ClassicAssert.AreEqual(23, cs2.Index);
            ClassicAssert.AreEqual(24, cs3.Index);

            ClassicAssert.IsNull(cs1.ParentStyle);
            ClassicAssert.IsNotNull(cs2.ParentStyle);
            ClassicAssert.IsNotNull(cs3.ParentStyle);

            ClassicAssert.AreEqual(21, cs2.ParentStyle.Index);
            ClassicAssert.AreEqual(22, cs3.ParentStyle.Index);

            // Now Check we can get style records for 
            //  the parent ones
            ClassicAssert.IsNull(wb.Workbook.GetStyleRecord(15));
            ClassicAssert.IsNull(wb.Workbook.GetStyleRecord(23));
            ClassicAssert.IsNull(wb.Workbook.GetStyleRecord(24));

            ClassicAssert.IsNotNull(wb.Workbook.GetStyleRecord(21));
            ClassicAssert.IsNotNull(wb.Workbook.GetStyleRecord(22));

            // Now Check the style names
            ClassicAssert.AreEqual(null, cs1.UserStyleName);
            ClassicAssert.AreEqual(null, cs2.UserStyleName);
            ClassicAssert.AreEqual(null, cs3.UserStyleName);
            ClassicAssert.AreEqual("style1", cs2.ParentStyle.UserStyleName);
            ClassicAssert.AreEqual("style2", cs3.ParentStyle.UserStyleName);

            // now apply a named style to a new cell
            ICell c4 = s.GetRow(0).CreateCell(1);
            c4.CellStyle = (cs2);
            ClassicAssert.AreEqual("style1", ((HSSFCellStyle)c4.CellStyle).ParentStyle.UserStyleName);
        }

        [Test]
        public void TestGetSetBorderHair()
        {
            HSSFWorkbook wb = OpenSample("55341_CellStyleBorder.xls");
            ISheet s = wb.GetSheetAt(0);
            ICellStyle cs;

            cs = s.GetRow(0).GetCell(0).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Hair, cs.BorderRight);

            cs = s.GetRow(1).GetCell(1).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Dotted, cs.BorderRight);

            cs = s.GetRow(2).GetCell(2).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.DashDotDot, cs.BorderRight);

            cs = s.GetRow(3).GetCell(3).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Dashed, cs.BorderRight);

            cs = s.GetRow(4).GetCell(4).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Thin, cs.BorderRight);

            cs = s.GetRow(5).GetCell(5).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.MediumDashDotDot, cs.BorderRight);

            cs = s.GetRow(6).GetCell(6).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.SlantedDashDot, cs.BorderRight);

            cs = s.GetRow(7).GetCell(7).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.MediumDashDot, cs.BorderRight);

            cs = s.GetRow(8).GetCell(8).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.MediumDashed, cs.BorderRight);

            cs = s.GetRow(9).GetCell(9).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Medium, cs.BorderRight);

            cs = s.GetRow(10).GetCell(10).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Thick, cs.BorderRight);

            cs = s.GetRow(11).GetCell(11).CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Double, cs.BorderRight);
        }

        [Test]
        public void TestShrinkToFit()
        {
            // Existing file
            IWorkbook wb = OpenSample("ShrinkToFit.xls");
            ISheet s = wb.GetSheetAt(0);
            IRow r = s.GetRow(0);
            ICellStyle cs = r.GetCell(0).CellStyle;

            ClassicAssert.AreEqual(true, cs.ShrinkToFit);

            // New file
            IWorkbook wbOrig = new HSSFWorkbook();
            s = wbOrig.CreateSheet();
            r = s.CreateRow(0);

            cs = wbOrig.CreateCellStyle();
            cs.ShrinkToFit = (/*setter*/false);
            r.CreateCell(0).CellStyle = (/*setter*/cs);

            cs = wbOrig.CreateCellStyle();
            cs.ShrinkToFit = (/*setter*/true);
            r.CreateCell(1).CellStyle = (/*setter*/cs);

            // Write out1, Read, and check
            wb = HSSFTestDataSamples.WriteOutAndReadBack(wbOrig as HSSFWorkbook);
            s = wb.GetSheetAt(0);
            r = s.GetRow(0);
            ClassicAssert.AreEqual(false, r.GetCell(0).CellStyle.ShrinkToFit);
            ClassicAssert.AreEqual(true, r.GetCell(1).CellStyle.ShrinkToFit);
        }

        [Test]
        public void Test56959()
        {
            IWorkbook wb = new HSSFWorkbook();
            ISheet sheet = wb.CreateSheet("somesheet");

            IRow row = sheet.CreateRow(0);

            // Create a new font and alter it.
            IFont font = wb.CreateFont();
            font.FontHeightInPoints = ((short)24);
            font.FontName = ("Courier New");
            font.IsItalic = (true);
            font.IsStrikeout = (true);
            font.Color = (HSSFColor.Red.Index);

            ICellStyle style = wb.CreateCellStyle();
            style.BorderBottom = BorderStyle.Dotted;
            style.SetFont(font);

            ICell cell = row.CreateCell(0);
            cell.CellStyle = (style);
            cell.SetCellValue("testtext");

            ICell newCell = row.CreateCell(1);

            newCell.CellStyle = (style);
            newCell.SetCellValue("2testtext2");
            ICellStyle newStyle = newCell.CellStyle;
            ClassicAssert.AreEqual(BorderStyle.Dotted, newStyle.BorderBottom);
            ClassicAssert.AreEqual(HSSFColor.Red.Index, ((HSSFCellStyle)newStyle).GetFont(wb).Color);

            //        OutputStream out = new FileOutputStream("/tmp/56959.xls");
            //        try {
            //            wb.write(out);
            //        } finally {
            //            out.close();
            //        }
        }

        [Test]
        public void Test58043()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFCellStyle cellStyle = wb.CreateCellStyle() as HSSFCellStyle;
            ClassicAssert.AreEqual(0, cellStyle.Rotation);
            cellStyle.Rotation = ((short)89);
            ClassicAssert.AreEqual(89, cellStyle.Rotation);

            cellStyle.Rotation = ((short)90);
            ClassicAssert.AreEqual(90, cellStyle.Rotation);

            cellStyle.Rotation = ((short)-1);
            ClassicAssert.AreEqual(-1, cellStyle.Rotation);

            cellStyle.Rotation = ((short)-89);
            ClassicAssert.AreEqual(-89, cellStyle.Rotation);

            cellStyle.Rotation = ((short)-90);
            ClassicAssert.AreEqual(-90, cellStyle.Rotation);

            cellStyle.Rotation = ((short)-89);
            ClassicAssert.AreEqual(-89, cellStyle.Rotation);
            // values above 90 are mapped to the correct values for compatibility between HSSF and XSSF
            cellStyle.Rotation = ((short)179);
            ClassicAssert.AreEqual(-89, cellStyle.Rotation);

            cellStyle.Rotation = ((short)180);
            ClassicAssert.AreEqual(-90, cellStyle.Rotation);

            wb.Close();
        }

        [Test, Parallelizable(ParallelScope.None)]
        public async Task TestNPOI1469()
        {
            const string dateFormat = "yyyy/MM/dd";
            const string timeFormat = "HH:mm:ss";
            const int rows = 1_000;
            const int dop = 2;

            var time = DateTime.UtcNow.AddYears(-1);

            Console.WriteLine($"Start time: {time:yyyy/MM/dd} {time:HH:mm:ss}");

            using var wb = new HSSFWorkbook();
            var expected = Write(wb);
            var actual = await Read(wb); // single threaded
            Compare(expected, actual);

            Console.WriteLine("Single-threaded passed");

            var tasks = new List<Task<List<string[]>>>(dop);
            for(var i = 0; i<dop; i++)
                tasks.Add(Read(wb));
            await Task.WhenAll(tasks);

            for(var i = 0; i<dop; i++)
                Compare(expected, tasks[i].Result);

            List<string[]> Write(HSSFWorkbook wb)
            {
                List<string[]> results = new(rows);

                IDataFormat df = wb.CreateDataFormat();
                var dateStyle = wb.CreateCellStyle();
                dateStyle.DataFormat = df.GetFormat(dateFormat);
                var timeStyle = wb.CreateCellStyle();
                timeStyle.DataFormat = df.GetFormat(timeFormat);

                ISheet sheet = wb.CreateSheet();

                for(var i = 0; i<rows; i++)
                {
                    IRow row = sheet.CreateRow(i);

                    ICell cellA = row.CreateCell(0);
                    cellA.SetCellValue(time);
                    cellA.CellStyle = dateStyle;

                    ICell cellB = row.CreateCell(1);
                    cellB.SetCellValue(time);
                    cellB.CellStyle = timeStyle;

                    results.Add([time.ToString(dateFormat), time.ToString(timeFormat)]);
                    time = time.AddHours(1);
                }

                return results;
            }

            async Task<List<string[]>> Read(HSSFWorkbook wb)
            {
                await Task.Yield();
                List<string[]> results = new(rows);
                DataFormatter df = new DataFormatter();
                ISheet sheet = wb.GetSheetAt(0);
                var numRows = sheet.LastRowNum;
                for(var r = 0; r<=numRows; r++)
                {
                    IRow row = sheet.GetRow(r);
                    var numCells = row.LastCellNum;
                    var readRow = new string[numCells];
                    for(var c = 0; c<numCells; c++)
                    {
                        ICell cell = row.GetCell(c);
                        if(cell is null) continue;
                        readRow[c] = df.FormatCellValue(cell);
                    }
                    results.Add(readRow);
                }
                return results;
            }

            void Compare(List<string[]> expected, List<string[]> actual)
            {
                Assert.That(actual.Count, Is.EqualTo(expected.Count), "Row count mismatch");
                for(var r = 0; r<expected.Count; r++)
                    Assert.That(actual[r], Is.EqualTo(expected[r]), $"Row mismatch ({r})");
            }
        }
    }
}
