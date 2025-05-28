/*
 * Licensed to the Apache Software Foundation (ASF) Under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for Additional information regarding copyright ownership.
 * The ASF licenses this file to You Under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed Under the License is distributed on an "AS Is" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations Under the License.
 */

using System;
using System.Collections.Generic;

using NPOI.Util;
using NPOI.SS.Formula.Eval;

namespace NPOI.SS.Formula.Functions
{
    /**
     * @author Amol S. Deshmukh &lt; amolweb at ya hoo dot com &gt;
     *
     */
    public class Mode : Function
    {
        /**
         * if v is zero length or contains no duplicates, return value is
         * Double.NaN. Else returns the value that occurs most times and if there is
         * a tie, returns the first such value.
         *
         * @param v
         */
        public static double Evaluate(double[] v)
        {
            if (v.Length < 2)
            {
                throw new EvaluationException(ErrorEval.NA);
            }

            // very naive impl, may need to be optimized
            int[] counts = new int[v.Length];
            Arrays.Fill(counts, 1);
            for (int i = 0, iSize = v.Length; i < iSize; i++)
            {
                for (int j = i + 1, jSize = v.Length; j < jSize; j++)
                {
                    if (v[i] == v[j])
                        counts[i]++;
                }
            }
            double maxv = 0;
            int maxc = 0;
            for (int i = 0, iSize = counts.Length; i < iSize; i++)
            {
                if (counts[i] > maxc)
                {
                    maxv = v[i];
                    maxc = counts[i];
                }
            }
            if (maxc > 1)
            {
                return maxv;
            }
            throw new EvaluationException(ErrorEval.NA);

        }

        public ValueEval Evaluate(ValueEval[] args, int srcCellRow, int srcCellCol)
        {
            double result;
            try
            {
                List<double> temp = [];
                for (int i = 0; i < args.Length; i++)
                {
                    CollectValues(args[i], temp);
                }
                double[] values = temp.ToArray();
                result = Evaluate(values);
            }
            catch (EvaluationException e)
            {
                return e.GetErrorEval();
            }
            return new NumberEval(result);
        }

        private static void CollectValues(ValueEval arg, List<double> temp)
        {
            if (arg is TwoDEval ae)
            {
                int width = ae.Width;
                int height = ae.Height;
                for (int rrIx = 0; rrIx < height; rrIx++)
                {
                    for (int rcIx = 0; rcIx < width; rcIx++)
                    {
                        ValueEval ve1 = ae.GetValue(rrIx, rcIx);
                        CollectValue(ve1, temp, false);
                    }
                }
                return;
            }
            if (arg is RefEval re)
            {
                int firstSheetIndex = re.FirstSheetIndex;
                int lastSheetIndex = re.LastSheetIndex;
                for (int sIx = firstSheetIndex; sIx <= lastSheetIndex; sIx++)
                {
                    CollectValue(re.GetInnerValueEval(sIx), temp, true);
                }
                return;
            }
            CollectValue(arg, temp, true);

        }

        private static void CollectValue(ValueEval arg, List<double> temp, bool mustBeNumber)
        {
            if (arg is ErrorEval eval)
            {
                throw new EvaluationException(eval);
            }
            if (arg == BlankEval.instance || arg is BoolEval || arg is StringEval)
            {
                if (mustBeNumber)
                {
                    throw EvaluationException.InvalidValue();
                }
                return;
            }
            if (arg is NumberEval numberEval)
            {
                temp.Add(numberEval.NumberValue);
                return;
            }
            throw new InvalidOperationException("Unexpected value type (" + arg.GetType().Name + ")");
        }
    }
}