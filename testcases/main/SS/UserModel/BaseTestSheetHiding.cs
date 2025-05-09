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

namespace TestCases.SS.UserModel
{
    using System;

    using NUnit.Framework;using NUnit.Framework.Legacy;

    using NPOI.SS;
    using TestCases.SS;
    using NPOI.SS.UserModel;

    /**
     */
    [TestFixture]
    public class BaseTestSheetHiding
    {

        protected ITestDataProvider _testDataProvider;
        protected IWorkbook wbH;
        protected IWorkbook wbU;

        private String _file1, _file2;
        public BaseTestSheetHiding()
            : this(TestCases.HSSF.HSSFITestDataProvider.Instance, "TwoSheetsOneHidden.xls", "TwoSheetsNoneHidden.xls")
        {
        }
        /**
         * @param TestDataProvider an object that provides Test data in HSSF /  specific way
         */
        protected BaseTestSheetHiding(ITestDataProvider TestDataProvider,
                                      String file1, String file2)
        {
            _testDataProvider = TestDataProvider;
            _file1 = file1;
            _file2 = file2;
        }
        [SetUp]
        public void SetUp()
        {
            wbH = _testDataProvider.OpenSampleWorkbook(_file1);
            wbU = _testDataProvider.OpenSampleWorkbook(_file2);
        }

        [Test]
        [Obsolete]
        public void TestSheetHiddenOld()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            wb.CreateSheet("MySheet");

            ClassicAssert.IsFalse(wb.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb.IsSheetVeryHidden(0));

            wb.SetSheetHidden(0, SheetVisibility.Hidden);
            ClassicAssert.IsTrue(wb.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb.IsSheetVeryHidden(0));

            wb.SetSheetHidden(0, SheetVisibility.VeryHidden);
            ClassicAssert.IsFalse(wb.IsSheetHidden(0));
            ClassicAssert.IsTrue(wb.IsSheetVeryHidden(0));

            wb.SetSheetHidden(0, SheetVisibility.Visible);
            ClassicAssert.IsFalse(wb.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb.IsSheetVeryHidden(0));

            try
            {
                wb.SetSheetHidden(0, -1);
                Assert.Fail("expectd exception");
            }
            catch (ArgumentException)
            {
                // ok
            }
            try
            {
                wb.SetSheetHidden(0, 3);
                Assert.Fail("expectd exception");
            }
            catch (ArgumentException)
            {
                // ok
            }

            wb.Close();
        }

        [Test]
        public void TestSheetVisibility()
        {
            IWorkbook wb = _testDataProvider.CreateWorkbook();
            wb.CreateSheet("MySheet");

            ClassicAssert.IsFalse(wb.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb.IsSheetVeryHidden(0));
            ClassicAssert.AreEqual(SheetVisibility.Visible, wb.GetSheetVisibility(0));

            wb.SetSheetVisibility(0, SheetVisibility.Hidden);
            ClassicAssert.IsTrue(wb.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb.IsSheetVeryHidden(0));
            ClassicAssert.AreEqual(SheetVisibility.Hidden, wb.GetSheetVisibility(0));

            wb.SetSheetVisibility(0, SheetVisibility.VeryHidden);
            ClassicAssert.IsFalse(wb.IsSheetHidden(0));
            ClassicAssert.IsTrue(wb.IsSheetVeryHidden(0));
            ClassicAssert.AreEqual(SheetVisibility.VeryHidden, wb.GetSheetVisibility(0));

            wb.SetSheetVisibility(0, SheetVisibility.Visible);
            ClassicAssert.IsFalse(wb.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb.IsSheetVeryHidden(0));
            ClassicAssert.AreEqual(SheetVisibility.Visible, wb.GetSheetVisibility(0));

            wb.Close();
        }

        /**
         * Test that we Get the right number of sheets,
         *  with the right text on them, no matter what
         *  the hidden flags are
         */
        [Test]
        public void TestTextSheets()
        {
            // Both should have two sheets
            ClassicAssert.AreEqual(2, wbH.NumberOfSheets);
            ClassicAssert.AreEqual(2, wbU.NumberOfSheets);

            // All sheets should have one row
            ClassicAssert.AreEqual(0, wbH.GetSheetAt(0).LastRowNum);
            ClassicAssert.AreEqual(0, wbH.GetSheetAt(1).LastRowNum);
            ClassicAssert.AreEqual(0, wbU.GetSheetAt(0).LastRowNum);
            ClassicAssert.AreEqual(0, wbU.GetSheetAt(1).LastRowNum);

            // All rows should have one column
            ClassicAssert.AreEqual(1, wbH.GetSheetAt(0).GetRow(0).LastCellNum);
            ClassicAssert.AreEqual(1, wbH.GetSheetAt(1).GetRow(0).LastCellNum);
            ClassicAssert.AreEqual(1, wbU.GetSheetAt(0).GetRow(0).LastCellNum);
            ClassicAssert.AreEqual(1, wbU.GetSheetAt(1).GetRow(0).LastCellNum);

            // Text should be sheet based
            ClassicAssert.AreEqual("Sheet1A1", wbH.GetSheetAt(0).GetRow(0).GetCell(0).RichStringCellValue.String);
            ClassicAssert.AreEqual("Sheet2A1", wbH.GetSheetAt(1).GetRow(0).GetCell(0).RichStringCellValue.String);
            ClassicAssert.AreEqual("Sheet1A1", wbU.GetSheetAt(0).GetRow(0).GetCell(0).RichStringCellValue.String);
            ClassicAssert.AreEqual("Sheet2A1", wbU.GetSheetAt(1).GetRow(0).GetCell(0).RichStringCellValue.String);
        }

        /**
         * Check that we can Get and Set the hidden flags
         *  as expected
         */
        [Test]
        public void TestHideUnHideFlags()
        {
            ClassicAssert.IsTrue(wbH.IsSheetHidden(0));
            ClassicAssert.IsFalse(wbH.IsSheetHidden(1));
            ClassicAssert.IsFalse(wbU.IsSheetHidden(0));
            ClassicAssert.IsFalse(wbU.IsSheetHidden(1));
        }

        /**
         * Turn the sheet with none hidden into the one with
         *  one hidden
         */
        [Test]
        public void TestHide()
        {
            wbU.SetSheetHidden(0,SheetVisibility.Hidden);
            ClassicAssert.IsTrue(wbU.IsSheetHidden(0));
            ClassicAssert.IsFalse(wbU.IsSheetHidden(1));
            IWorkbook wb2 = _testDataProvider.WriteOutAndReadBack(wbU);
            ClassicAssert.IsTrue(wb2.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb2.IsSheetHidden(1));

            wb2.Close();
        }

        /**
         * Turn the sheet with one hidden into the one with
         *  none hidden
         */
        [Test]
        public void TestUnHide()
        {
            wbH.SetSheetHidden(0,SheetVisibility.Visible);
            ClassicAssert.IsFalse(wbH.IsSheetHidden(0));
            ClassicAssert.IsFalse(wbH.IsSheetHidden(1));
            IWorkbook wb2 = _testDataProvider.WriteOutAndReadBack(wbH);
            ClassicAssert.IsFalse(wb2.IsSheetHidden(0));
            ClassicAssert.IsFalse(wb2.IsSheetHidden(1));

            wb2.Close();
        }
    }
}