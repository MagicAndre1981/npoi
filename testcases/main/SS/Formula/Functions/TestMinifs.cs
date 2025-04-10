﻿/*
 *  ====================================================================
 *    Licensed to the Apache Software Foundation (ASF) under one or more
 *    contributor license agreements.  See the NOTICE file distributed with
 *    this work for additional information regarding copyright ownership.
 *    The ASF licenses this file to You under the Apache License, Version 2.0
 *    (the "License"); you may not use this file except in compliance with
 *    the License.  You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 * ====================================================================
 */

namespace TestCases.SS.Formula.Functions
{
    using NPOI.HSSF.UserModel;
    using NPOI.SS.Formula;
    using NPOI.SS.Formula.Eval;
    using NPOI.SS.Formula.Functions;
    using NUnit.Framework;using NUnit.Framework.Legacy;

    /**
     * Test cases for MINIFS()
     */
    [TestFixture]
    public class TestMinifs
    {
        private static OperationEvaluationContext EC = new OperationEvaluationContext(null, null, 0, 1, 0, null);

        private static ValueEval InvokeMinifs(ValueEval[] args, OperationEvaluationContext ec)
        {
            return new Minifs().Evaluate(args, EC);
        }

        private static void ConfirmDouble(double expected, ValueEval actualEval)
        {
            ClassicAssert.IsTrue(actualEval is NumericValueEval, "Expected numeric result");
            NumericValueEval nve = (NumericValueEval)actualEval;
            ClassicAssert.AreEqual(expected, nve.NumberValue, 0);
        }

        private static void Confirm(double expectedResult, ValueEval[] args)
        {
            ConfirmDouble(expectedResult, InvokeMinifs(args, EC));
        }

        /**
         *  Example 1 from
         *  https://support.microsoft.com/en-us/office/minifs-function-6ca1ddaa-079b-4e74-80cc-72eef32e6599
         */
        [Test]
        public void TestExample1()
        {
            // mimic test sample from https://support.microsoft.com/en-us/office/minifs-function-6ca1ddaa-079b-4e74-80cc-72eef32e6599
            ValueEval[] a2a7 = new ValueEval[]
            {
                new NumberEval(89),
                new NumberEval(93),
                new NumberEval(96),
                new NumberEval(85),
                new NumberEval(91),
                new NumberEval(88)
            };

            ValueEval[] b2b7 = new ValueEval[]
            {
                new NumberEval(1),
                new NumberEval(2),
                new NumberEval(2),
                new NumberEval(3),
                new NumberEval(1),
                new NumberEval(1)
            };

            // "=MinIFS(A2:A9, B2:B9, "=A*", C2:C9, 1)"
            ValueEval[] args = new ValueEval[]
            {
                EvalFactory.CreateAreaEval("A2:A7", a2a7),
                EvalFactory.CreateAreaEval("B2:B7", b2b7),
                new NumberEval(1)
            };
            Confirm(88.0, args);
        }

        /**
         *  Example 2 from
         *  https://support.microsoft.com/en-us/office/minifs-function-6ca1ddaa-079b-4e74-80cc-72eef32e6599
         */
        [Test]
        public void TestExample2()
        {
            ValueEval[] a2a5 = new ValueEval[] {
                new NumberEval(10),
                new NumberEval(11),
                new NumberEval(100),
                new NumberEval(111)
            };

            ValueEval[] b3b6 = new ValueEval[] {
                new StringEval("a"),
                new StringEval("a"),
                new StringEval("b"),
                new StringEval("a")
            };

            ValueEval[] args;

            // "=MINIFS(A2:A5,B3:B6,"a")"
            args = new ValueEval[]{
                EvalFactory.CreateAreaEval("A2:A5", a2a5),
                EvalFactory.CreateAreaEval("B3:B6", b3b6),
                new StringEval("a")
            };
            Confirm(10.0, args);
        }
    }
}