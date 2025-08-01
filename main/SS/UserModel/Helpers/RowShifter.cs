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

using NPOI.SS.Formula;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using static ICSharpCode.SharpZipLib.Zip.FastZip;

namespace NPOI.SS.UserModel.Helpers
{
    /// <summary>
    /// Helper for Shifting rows up or down
    /// This abstract class exists to consolidate duplicated code between XSSFRowShifter 
    /// and HSSFRowShifter(currently methods sprinkled throughout HSSFSheet)
    /// </summary>
    public abstract class RowShifter
    {
        protected ISheet sheet;

        public RowShifter(ISheet sh)
        {
            sheet = sh;
        }

        /// <summary>
        /// Shifts, grows, or shrinks the merged regions due to a row Shift.
        /// Merged regions that are completely overlaid by Shifting will be deleted.
        /// </summary>
        /// <param name="startRow">the row to start Shifting</param>
        /// <param name="endRow">the row to end Shifting</param>
        /// <param name="n">the number of rows to shift</param>
        /// <returns>an array of affected merged regions, doesn't contain deleted ones</returns>
        public List<CellRangeAddress> ShiftMergedRegions(int startRow, int endRow, int n)
        {
            List<CellRangeAddress> shiftedRegions = [];
            HashSet<int> removedIndices = [];
            var size = sheet.NumMergedRegions;

            for (var i = 0; i < size; i++)
            {
                var merged = sheet.GetMergedRegion(i);

                //Shift if the merged region inside the Shifting rows
                if (RemovalNeeded(merged, startRow, endRow, n))
                {
                    removedIndices.Add(i);
                    continue;
                }

                bool inStart = (merged.FirstRow >= startRow || merged.LastRow >= startRow);
                bool inEnd = (merged.FirstRow <= endRow || merged.LastRow <= endRow);

                //don't check if it's not within the shifted area
                if (!inStart || !inEnd) {
                    continue;
                }

                //only shift if the region outside the shifted rows is not merged too
                if (!merged.ContainsRow(startRow - 1) && !merged.ContainsRow(endRow + 1)) {
                    merged.FirstRow = merged.FirstRow + n;
                    merged.LastRow = merged.LastRow + n;
                    //have to remove/add it back
                    shiftedRegions.Add(merged);
                    removedIndices.Add(i);
                }
            }

            if (removedIndices.Count != 0)
            {
                sheet.RemoveMergedRegions(removedIndices.ToList());
            }

            //add it which is within the shifted area back
            foreach (var region in shiftedRegions)
            {
                sheet.AddMergedRegion(region);
            }

            return shiftedRegions;
        }
        
        private static bool RemovalNeeded(CellRangeAddress merged, int startRow, int endRow, int n)
        {
            int movedRows = endRow - startRow + 1;

            // build a range of the rows that are overwritten, i.e. the target-area, but without
            // rows that are moved along
            CellRangeAddress overwrite;
            if(n > 0) {
                // area is moved down => overwritten area is [endRow + n - movedRows, endRow + n]
                overwrite = new CellRangeAddress(Math.Max(endRow + 1, endRow + n - movedRows), endRow + n, 0, 0);
            } else {
                // area is moved up => overwritten area is [startRow + n, startRow + n + movedRows]
                overwrite = new CellRangeAddress(startRow + n, Math.Min(startRow - 1, startRow + n + movedRows), 0, 0);
            }

            // if the merged-region and the overwritten area intersect, we need to remove it
            return merged.Intersects(overwrite);
        }
        /// <summary>
        /// Verify that the given column indices and step denote a valid range of columns to shift
        /// </summary>
        /// <param name="firstShiftColumnIndex">the column to start shifting</param>
        /// <param name="lastShiftColumnIndex">the column to end shifting</param>
        /// <param name="step">length of the shifting step</param>
        /// <exception cref="ArgumentException"></exception>
        public static void ValidateShiftParameters(int firstShiftColumnIndex, int lastShiftColumnIndex, int step)
        {
            if (step < 0)
            {
                throw new ArgumentException("Shifting step may not be negative, but had " + step);
            }

            if (firstShiftColumnIndex > lastShiftColumnIndex)
            {
                throw new ArgumentException(string.Format("Incorrect shifting range : %d-%d", firstShiftColumnIndex, lastShiftColumnIndex));
            }
        }
        
        /// <summary>
        /// Verify that the given column indices and step denote a valid range of columns to shift to the left
        /// </summary>
        /// <param name="firstShiftColumnIndex">the column to start shifting</param>
        /// <param name="lastShiftColumnIndex">the column to end shifting</param>
        /// <param name="step">length of the shifting step</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ValidateShiftLeftParameters(int firstShiftColumnIndex, int lastShiftColumnIndex, int step)
        {
            ValidateShiftParameters(firstShiftColumnIndex, lastShiftColumnIndex, step);

            if (firstShiftColumnIndex - step < 0)
            {
                throw new InvalidOperationException("Column index less than zero: " + (firstShiftColumnIndex + step));
            }
        }
        
        /// <summary>
        /// Updated named ranges
        /// </summary>
        /// <param name="Shifter"></param>
        public abstract void UpdateNamedRanges(FormulaShifter Shifter);
        
        /// <summary>
        /// Update formulas.
        /// </summary>
        /// <param name="Shifter"></param>
        public abstract void UpdateFormulas(FormulaShifter Shifter);

        /// <summary>
        /// Update the formulas in specified row using the formula Shifting policy specified by Shifter
        /// </summary>
        /// <param name="row">the row to update the formulas on</param>
        /// <param name="Shifter">the formula Shifting policy</param>
        public abstract void UpdateRowFormulas(IRow row, FormulaShifter Shifter);

        public abstract void UpdateConditionalFormatting(FormulaShifter Shifter);
        
        /// <summary>
        /// Shift the Hyperlink anchors (not the hyperlink text, even if the hyperlink
        /// is of type LINK_DOCUMENT and refers to a cell that was Shifted). Hyperlinks
        /// do not track the content they point to.
        /// </summary>
        /// <param name="Shifter">the formula Shifting policy</param>
        public abstract void UpdateHyperlinks(FormulaShifter Shifter);
    }
}