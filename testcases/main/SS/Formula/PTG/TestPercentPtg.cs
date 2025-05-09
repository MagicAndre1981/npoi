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

namespace TestCases.SS.Formula.PTG
{

    using NUnit.Framework;using NUnit.Framework.Legacy;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;

    /**
     * Tests for {@link PercentPtg}.
     *
     * @author Daniel Noll (daniel at nuix dot com dot au)
     */
    [TestFixture]
    public class TestPercentPtg : AbstractPtgTestCase
    {
        /**
         * Tests Reading a file Containing this ptg.
         */
        [Test]
        public void TestReading()
        {
            HSSFWorkbook workbook = LoadWorkbook("PercentPtg.xls");
            ISheet sheet = workbook.GetSheetAt(0);

            ClassicAssert.AreEqual(53000.0, sheet.GetRow(0).GetCell(0).NumericCellValue, 0.0, "Wrong numeric value for original number");
            ClassicAssert.AreEqual(5300.0, sheet.GetRow(1).GetCell(0).NumericCellValue, 0.0, "Wrong numeric value for percent formula result");
            ClassicAssert.AreEqual("A1*10%", sheet.GetRow(1).GetCell(0).CellFormula, "Wrong formula string for percent formula");
        }
    }

}