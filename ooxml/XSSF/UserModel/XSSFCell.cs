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

using NPOI.SS.UserModel;
using NPOI.XSSF.Model;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Util;
using System;
using NPOI.SS.Formula.PTG;
using NPOI.SS.Formula;
using NPOI.SS;
using NPOI.Util;
using NPOI.SS.Formula.Eval;
using System.Globalization;
using System.IO;

namespace NPOI.XSSF.UserModel
{

    /**
     * High level representation of a cell in a row of a spreadsheet.
     * <p>
     * Cells can be numeric, formula-based or string-based (text).  The cell type
     * specifies this.  String cells cannot conatin numbers and numeric cells cannot
     * contain strings (at least according to our model).  Client apps should do the
     * conversions themselves.  Formula cells have the formula string, as well as
     * the formula result, which can be numeric or string.
     * </p>
     * <p>
     * Cells should have their number (0 based) before being Added to a row.  Only
     * cells that have values should be Added.
     * </p>
     */
    public class XSSFCell : ICell
    {

        private static readonly String FALSE_AS_STRING = "0";
        private static readonly String TRUE_AS_STRING = "1";

        /**
         * the xml bean Containing information about the cell's location, value,
         * data type, formatting, and formula
         */
        private readonly CT_Cell _cell;

        /**
         * the XSSFRow this cell belongs to
         */
        private readonly XSSFRow _row;

        /**
         * 0-based column index
         */
        private int _cellNum;

        /**
         * Table of strings shared across this workbook.
         * If two cells contain the same string, then the cell value is the same index into SharedStringsTable
         */
        private readonly SharedStringsTable _sharedStringSource;

        /**
         * Table of cell styles shared across all cells in a workbook.
         */
        private readonly StylesTable _stylesSource;

        /**
         * Construct a XSSFCell.
         *
         * @param row the parent row.
         * @param cell the xml bean Containing information about the cell.
         */
        public XSSFCell(XSSFRow row, CT_Cell cell)
        {
            _cell = cell;
            _row = row;
            if (cell.r != null)
            {
                _cellNum = new CellReference(cell.r).Col;
            }
            else
            {
                int prevNum = row.LastCellNum;
                if (prevNum != -1)
                {
                    _cellNum = (row as XSSFRow).GetCell(prevNum - 1, MissingCellPolicy.RETURN_NULL_AND_BLANK).ColumnIndex + 1;
                }
            }
            _sharedStringSource = ((XSSFWorkbook)row.Sheet.Workbook).GetSharedStringSource();
            _stylesSource = ((XSSFWorkbook)row.Sheet.Workbook).GetStylesSource();
        }

        /// <summary>
        /// Copy cell value, formula and style, from srcCell per cell copy policy
        ///  If srcCell is null, clears the cell value and cell style per cell copy policy
        ///  
        /// This does not shift references in formulas. Use {@link XSSFRowShifter} to shift references in formulas.
        /// </summary>
        /// <param name="srcCell">The cell to take value, formula and style from</param>
        /// <param name="policy">The policy for copying the information, see {@link CellCopyPolicy}</param>
        /// <exception cref="ArgumentException">if copy cell style and srcCell is from a different workbook</exception>
        public void CopyCellFrom(ICell srcCell, CellCopyPolicy policy)
        {
            // Copy cell value (cell type is updated implicitly)
            if (policy.IsCopyCellValue)
            {
                if (srcCell != null)
                {
                    CellType copyCellType = srcCell.CellType;
                    if (copyCellType == CellType.Formula && !policy.IsCopyCellFormula)
                    {
                        // Copy formula result as value
                        // FIXME: Cached value may be stale
                        copyCellType = srcCell.CachedFormulaResultType;
                    }
                    switch (copyCellType)
                    {
                        case CellType.Boolean:
                            SetCellValue(srcCell.BooleanCellValue);
                            break;
                        case CellType.Error:
                            SetCellErrorValue(srcCell.ErrorCellValue);
                            break;
                        case CellType.Formula:
                            SetCellFormula(srcCell.CellFormula);
                            break;
                        case CellType.Numeric:
                            // DataFormat is not copied unless policy.isCopyCellStyle is true
                            if (DateUtil.IsCellDateFormatted(srcCell))
                            {
                                SetCellValue(srcCell.DateCellValue);
                            }
                            else
                            {
                                SetCellValue(srcCell.NumericCellValue);
                            }
                            break;
                        case CellType.String:
                            SetCellValue(srcCell.StringCellValue);
                            break;
                        case CellType.Blank:
                            SetBlankInternal();
                            break;
                        default:
                            throw new ArgumentException("Invalid cell type " + srcCell.CellType);
                    }
                }
                else
                { //srcCell is null
                    SetBlankInternal();
                }
            }

            // Copy CellStyle
            if (policy.IsCopyCellStyle)
            {
                if (srcCell != null)
                {
                    CellStyle = (srcCell.CellStyle);
                }
                else
                {
                    // clear cell style
                    CellStyle = (null);
                }
            }


            if (policy.IsMergeHyperlink)
            {
                // if srcCell doesn't have a hyperlink and destCell has a hyperlink, don't clear destCell's hyperlink
                IHyperlink srcHyperlink = srcCell.Hyperlink;
                if (srcHyperlink != null)
                {
                    Hyperlink = new XSSFHyperlink(srcHyperlink);
                }
            }
            else if (policy.IsCopyHyperlink)
            {
                // overwrite the hyperlink at dest cell with srcCell's hyperlink
                // if srcCell doesn't have a hyperlink, clear the hyperlink (if one exists) at destCell
                if (srcCell == null || srcCell.Hyperlink == null)
                {
                    Hyperlink = (null);
                }
                else
                {
                    Hyperlink = new XSSFHyperlink(srcCell.Hyperlink);
                }
            }
        }


        /**
         * @return table of strings shared across this workbook
         */
        protected SharedStringsTable GetSharedStringSource()
        {
            return _sharedStringSource;
        }

        /**
         * @return table of cell styles shared across this workbook
         */
        protected StylesTable GetStylesSource()
        {
            return _stylesSource;
        }

        /**
         * Returns the sheet this cell belongs to
         *
         * @return the sheet this cell belongs to
         */
        public ISheet Sheet
        {
            get
            {
                return _row.Sheet;
            }
        }

        /**
         * Returns the row this cell belongs to
         *
         * @return the row this cell belongs to
         */
        public IRow Row
        {
            get
            {
                return _row;
            }
        }

        /**
         * Get the value of the cell as a bool.
         * <p>
         * For strings, numbers, and errors, we throw an exception. For blank cells we return a false.
         * </p>
         * @return the value of the cell as a bool
         * @throws InvalidOperationException if the cell type returned by {@link #CellType}
         *   is not CellType.Boolean, CellType.Blank or CellType.Formula
         */
        public bool BooleanCellValue
        {
            get
            {
                CellType cellType = CellType;
                switch (cellType)
                {
                    case CellType.Blank:
                        return false;
                    case CellType.Boolean:
                        return _cell.IsSetV() && TRUE_AS_STRING.Equals(_cell.v);
                    case CellType.Formula:
                        //YK: should throw an exception if requesting bool value from a non-bool formula
                        return _cell.IsSetV() && TRUE_AS_STRING.Equals(_cell.v);
                    default:
                        throw TypeMismatch(CellType.Boolean, cellType, false);
                }
            }
        }

        /**
         * Set a bool value for the cell
         *
         * @param value the bool value to Set this cell to.  For formulas we'll Set the
         *        precalculated value, for bools we'll Set its value. For other types we
         *        will change the cell to a bool cell and Set its value.
         */
        public ICell SetCellValue(bool value)
        {
            _cell.t = (ST_CellType.b);
            _cell.v = (value ? TRUE_AS_STRING : FALSE_AS_STRING);
            return this;
        }

        /**
         * Get the value of the cell as a number.
         * <p>
         * For strings we throw an exception. For blank cells we return a 0.
         * For formulas or error cells we return the precalculated value;
         * </p>
         * @return the value of the cell as a number
         * @throws InvalidOperationException if the cell type returned by {@link #CellType} is CellType.String
         * @exception NumberFormatException if the cell value isn't a parsable <code>double</code>.
         * @see DataFormatter for turning this number into a string similar to that which Excel would render this number as.
         */
        public double NumericCellValue
        {
            get
            {
                CellType cellType = CellType;
                switch (cellType)
                {
                    case CellType.Blank:
                        return 0.0;
                    case CellType.Formula:
                    case CellType.Numeric:
                        if (_cell.IsSetV())
                        {
                            if (string.IsNullOrEmpty(_cell.v))
                                return 0.0;
                            try
                            {
                                return Double.Parse(_cell.v, CultureInfo.InvariantCulture);
                            }
                            catch (FormatException)
                            {
                                throw TypeMismatch(CellType.Numeric, CellType.String, false);
                            }
                        }
                        else
                        {
                            return 0.0;
                        }
                    default:
                        throw TypeMismatch(CellType.Numeric, cellType, false);
                }
            }
        }


        /**
         * Set a numeric value for the cell
         *
         * @param value  the numeric value to Set this cell to.  For formulas we'll Set the
         *        precalculated value, for numerics we'll Set its value. For other types we
         *        will change the cell to a numeric cell and Set its value.
         */
        public ICell SetCellValue(double value)
        {
            if (Double.IsInfinity(value))
            {
                // Excel does not support positive/negative infInities,
                // rather, it gives a #DIV/0! error in these cases.
                _cell.t = (ST_CellType.e);
                _cell.v = (FormulaError.DIV0.String);
            }
            else if (Double.IsNaN(value))
            {
                // Excel does not support Not-a-Number (NaN),
                // instead it immediately generates an #NUM! error.
                _cell.t = (ST_CellType.e);
                _cell.v = (FormulaError.NUM.String);
            }
            else
            {
                _cell.t = (ST_CellType.n);
                _cell.v = (value.ToString(CultureInfo.InvariantCulture));
            }

            return this;
        }

        /**
         * Get the value of the cell as a string
         * <p>
         * For numeric cells we throw an exception. For blank cells we return an empty string.
         * For formulaCells that are not string Formulas, we throw an exception
         * </p>
         * @return the value of the cell as a string
         */
        public String StringCellValue
        {
            get
            {
                return this.RichStringCellValue.String;
            }
        }

        /**
         * Get the value of the cell as a XSSFRichTextString
         * <p>
         * For numeric cells we throw an exception. For blank cells we return an empty string.
         * For formula cells we return the pre-calculated value if a string, otherwise an exception
         * </p>
         * @return the value of the cell as a XSSFRichTextString
         */
        public IRichTextString RichStringCellValue
        {
            get
            {
                CellType cellType = CellType;
                XSSFRichTextString rt;
                switch (cellType)
                {
                    case CellType.Blank:
                        rt = new XSSFRichTextString("");
                        break;
                    case CellType.String:
                        if (_cell.t == ST_CellType.inlineStr)
                        {
                            if (_cell.IsSetIs())
                            {
                                //string is expressed directly in the cell defInition instead of implementing the shared string table.
                                rt = new XSSFRichTextString(_cell.@is);
                            }
                            else if (_cell.IsSetV())
                            {
                                //cached result of a formula
                                rt = new XSSFRichTextString(_cell.v);
                            }
                            else
                            {
                                rt = new XSSFRichTextString("");
                            }
                        }
                        else if (_cell.t == ST_CellType.str)
                        {
                            //cached formula value
                            rt = new XSSFRichTextString(_cell.IsSetV() ? _cell.v : "");
                        }
                        else
                        {
                            if (_cell.IsSetV())
                            {
                                int idx = Int32.Parse(_cell.v);
                                rt = new XSSFRichTextString(_sharedStringSource.GetEntryAt(idx));
                            }
                            else
                            {
                                rt = new XSSFRichTextString("");
                            }
                        }
                        break;
                    case CellType.Formula:
                        CheckFormulaCachedValueType(CellType.String, GetBaseCellType(false));
                        rt = new XSSFRichTextString(_cell.IsSetV() ? _cell.v : "");
                        break;
                    default:
                        throw TypeMismatch(CellType.String, cellType, false);
                }
                rt.SetStylesTableReference(_stylesSource);
                return rt;
            }
        }

        private static void CheckFormulaCachedValueType(CellType expectedTypeCode, CellType cachedValueType)
        {
            if (cachedValueType != expectedTypeCode)
            {
                throw TypeMismatch(expectedTypeCode, cachedValueType, true);
            }
        }

        /**
         * Set a string value for the cell.
         *
         * @param str value to Set the cell to.  For formulas we'll Set the formula
         * cached string result, for String cells we'll Set its value. For other types we will
         * change the cell to a string cell and Set its value.
         * If value is null then we will change the cell to a Blank cell.
         */
        public ICell SetCellValue(String str)
        {
            return SetCellValue(str == null ? null : new XSSFRichTextString(str));
        }

        /**
         * Set a string value for the cell.
         *
         * @param str  value to Set the cell to.  For formulas we'll Set the 'pre-Evaluated result string,
         * for String cells we'll Set its value.  For other types we will
         * change the cell to a string cell and Set its value.
         * If value is null then we will change the cell to a Blank cell.
         */
        public ICell SetCellValue(IRichTextString str)
        {
            if (str == null || str.String == null)
            {
                SetCellType(CellType.Blank);
                return this;
            }

            if (str.Length > SpreadsheetVersion.EXCEL2007.MaxTextLength)
            {
                throw new ArgumentException("The maximum length of cell contents (text) is 32,767 characters");
            }
            CellType cellType = CellType;
            switch (cellType)
            {
                case CellType.Formula:
                    _cell.v = (str.String);
                    _cell.t= (ST_CellType.str);
                    break;
                default:
                    if (_cell.t == ST_CellType.inlineStr)
                    {
                        //set the 'pre-Evaluated result
                        _cell.v = str.String;
                    }
                    else
                    {
                        _cell.t = ST_CellType.s;
                        XSSFRichTextString rt = (XSSFRichTextString)str;
                        rt.SetStylesTableReference(_stylesSource);
                        int sRef = _sharedStringSource.AddEntry(rt.GetCTRst());
                        _cell.v=sRef.ToString();
                    }
                    break;
            }

            return this;
        }

        /// <summary>
        /// Return a formula for the cell,  for example, <code>SUM(C4:E4)</code>
        /// </summary>
        public String CellFormula
        {
            get
            {
                // existing behavior - create a new XSSFEvaluationWorkbook for every call
                return GetCellFormula(null);
            }
            set
            {
                SetCellFormula(value);
            }
        }

        public void RemoveFormula()
        {
            if (CellType == CellType.Blank)
                return;

            if (IsPartOfArrayFormulaGroup)
            {
                TryToDeleteArrayFormula(null);
                return;
            }
            ((XSSFWorkbook)_row.Sheet.Workbook).OnDeleteFormula(this);
            if (_cell.IsSetF())
            {
                ((XSSFSheet)_row.Sheet).OnDeleteFormula(this, null);
                _cell.unsetF();
            }
        }

        /**
         * package/hierarchy use only - reuse an existing evaluation workbook if available for caching
         *
         * @param fpb evaluation workbook for reuse, if available, or null to create a new one as needed
         * @return a formula for the cell
         * @throws InvalidOperationException if the cell type returned by {@link #getCellType()} is not CELL_TYPE_FORMULA
         */
        protected internal String GetCellFormula(XSSFEvaluationWorkbook fpb)
        {
            CellType cellType = CellType;
            if (cellType != CellType.Formula) 
                throw TypeMismatch(CellType.Formula, cellType, false);

            CT_CellFormula f = _cell.f;
            if (IsPartOfArrayFormulaGroup && f == null)
            {
                XSSFCell cell = ((XSSFSheet)Sheet).GetFirstCellInArrayFormula(this);
                return cell.GetCellFormula(fpb);
            }
            if (f.t == ST_CellFormulaType.shared)
            {
                //return ConvertSharedFormula((int)f.si);
                return ConvertSharedFormula((int)f.si, fpb == null ? XSSFEvaluationWorkbook.Create(Sheet.Workbook) : fpb);
            }
            return f.Value;
        }

        /// <summary>
        /// Creates a non shared formula from the shared formula counterpart
        /// </summary>
        /// <param name="si">Shared Group Index</param>
        /// <param name="fpb"></param>
        /// <returns>non shared formula created for the given shared formula and this cell</returns>
        private String ConvertSharedFormula(int si, XSSFEvaluationWorkbook fpb)
        {
            XSSFSheet sheet = (XSSFSheet)Sheet;

            CT_CellFormula f = sheet.GetSharedFormula(si);
            if (f == null) throw new InvalidOperationException(
                     "Master cell of a shared formula with sid=" + si + " was not found");

            String sharedFormula = f.Value;
            //Range of cells which the shared formula applies to
            String sharedFormulaRange = f.@ref;

            CellRangeAddress ref1 = CellRangeAddress.ValueOf(sharedFormulaRange);

            int sheetIndex = sheet.Workbook.GetSheetIndex(sheet);
            SharedFormula sf = new SharedFormula(SpreadsheetVersion.EXCEL2007);

            Ptg[] ptgs = FormulaParser.Parse(sharedFormula, fpb, FormulaType.Cell, sheetIndex, RowIndex);
            Ptg[] fmla = sf.ConvertSharedFormulas(ptgs,
                    RowIndex - ref1.FirstRow, ColumnIndex - ref1.FirstColumn);
            return FormulaRenderer.ToFormulaString(fpb, fmla);
        }

        /**
         * Sets formula for this cell.
         * <p>
         * Note, this method only Sets the formula string and does not calculate the formula value.
         * To Set the precalculated value use {@link #setCellValue(double)} or {@link #setCellValue(String)}
         * </p>
         *
         * @param formula the formula to Set, e.g. <code>"SUM(C4:E4)"</code>.
         *  If the argument is <code>null</code> then the current formula is Removed.
         * @throws NPOI.ss.formula.FormulaParseException if the formula has incorrect syntax or is otherwise invalid
         * @throws InvalidOperationException if the operation is not allowed, for example,
         *  when the cell is a part of a multi-cell array formula
         */
        public ICell SetCellFormula(String formula)
        {
            if (IsPartOfArrayFormulaGroup)
            {
                NotifyArrayFormulaChanging();
            }
            return SetFormula(formula, FormulaType.Cell);
        }

        internal void SetCellArrayFormula(String formula, CellRangeAddress range)
        {
            SetFormula(formula, FormulaType.Array);
            CT_CellFormula cellFormula = _cell.f;
            cellFormula.t = (ST_CellFormulaType.array);
            cellFormula.@ref = (range.FormatAsString());
        }
        /// <summary>
        /// Called when this an array formula in this cell is deleted.
        /// </summary>
        /// <param name="message">a customized exception message for the case if deletion of the cell is impossible. If null, a default message will be generated</param>
        internal void TryToDeleteArrayFormula(String message)
        {
            if (!IsPartOfArrayFormulaGroup)
                return;

            CellRangeAddress arrayFormulaRange = ArrayFormulaRange;
            if (arrayFormulaRange.NumberOfCells > 1)
            {
                if (message == null)
                {
                    message = "Cell " + new CellReference(this).FormatAsString() + " is part of a multi-cell array formula. " +
                            "You cannot change part of an array.";
                }
                throw new InvalidOperationException(message);
            }
            //un-register the single-cell array formula from the parent sheet through public interface
            Row.Sheet.RemoveArrayFormula(this);
        }

        private XSSFCell SetFormula(String formula, FormulaType formulaType)
        {
            XSSFWorkbook wb = (XSSFWorkbook)_row.Sheet.Workbook;
            if (formula == null)
            {
                RemoveFormula();
                return this;
            }

            if (wb.CellFormulaValidation)
            {
                IFormulaParsingWorkbook fpb = XSSFEvaluationWorkbook.Create(wb);
                //validate through the FormulaParser
                FormulaParser.Parse(formula, fpb, formulaType, wb.GetSheetIndex(this.Sheet), RowIndex);
            }
            CT_CellFormula f = new CT_CellFormula { Value = formula };
            _cell.f= f;
            if (_cell.IsSetV()) _cell.unsetV();
            return this;
        }

        /// <summary>
        /// Returns zero-based column index of this cell
        /// </summary>
        public int ColumnIndex
        {
            get
            {
                return _cellNum;
            }

            internal set
            {
                _cellNum = value;
            }
        }

        /// <summary>
        /// Returns zero-based row index of a row in the sheet that contains this cell
        /// </summary>
        public int RowIndex
        {
            get
            {
                return _row.RowNum;
            }
        }
        /// <summary>
        /// Returns an A1 style reference to the location of this cell
        /// </summary>
        /// <returns>A1 style reference to the location of this cell</returns>
        public String GetReference()
        {
            String ref1 = _cell.r;
            if (ref1 == null)
            {
                return new CellAddress(this).FormatAsString();
            }
            return ref1;
        }
        public CellAddress Address
        {
            get
            {
                return new CellAddress(this);
            }
        }
        /// <summary>
        /// Return the cell's style.
        /// </summary>
        public ICellStyle CellStyle
        {
            get
            {
                XSSFCellStyle style = null;
                if ((null != _stylesSource) && (_stylesSource.NumCellStyles > 0))
                {
                    long idx = _cell.IsSetS() ? _cell.s : 0;
                    style = _stylesSource.GetStyleAt((int)idx);
                }
                return style;
            }
            set 
            {
                if (value == null)
                {
                    if (_cell.IsSetS()) _cell.unsetS();
                }
                else
                {
                    XSSFCellStyle xStyle = (XSSFCellStyle)value;
                    xStyle.VerifyBelongsToStylesSource(_stylesSource);

                    long idx = _stylesSource.PutStyle(xStyle);
                    _cell.s = (uint)idx;
                }
            }
        }
        /// <summary>
        /// POI currently supports these formula types:
        /// <list type="bullet">
        /// <item><description> <see cref="ST_CellFormulaType.normal" /></description></item>
        /// <item><description> <see cref="ST_CellFormulaType.shared" /></description></item>
        /// <item><description> <see cref="ST_CellFormulaType.array" /></description></item>
        /// </list>
        /// POI does not support <see cref="ST_CellFormulaType.dataTable" /> formulas.
        /// </summary>
        /// <return>true if the cell is of a formula type POI can handle
        /// </return>
        private bool IsFormulaCell
        {
            get
            {
                if ((_cell.f != null && _cell.f.t != ST_CellFormulaType.dataTable) || ((XSSFSheet)Sheet).IsCellInArrayFormulaContext(this))
                {
                    return true;
                }
                return false;
            }
            
        }
        /// <summary>
        /// Return the cell type.  Tables in an array formula return
        /// <see cref="CellType.FORMULA" /> for all cells, even though the formula is only defined
        /// in the OOXML file for the top left cell of the array.
        /// <para>
        /// NOTE: POI does not support data table formulas.
        /// Cells in a data table appear to POI as plain cells typed from their cached value.</para>
        /// </summary>
        public CellType CellType
        {
            get
            {

                if (IsFormulaCell)
                {
                    return CellType.Formula;
                }

                return GetBaseCellType(true);
            }
        }
        /// <summary>
        /// Only valid for formula cells
        /// </summary>
        public CellType CachedFormulaResultType
        {
            get
            {
                if (!IsFormulaCell)
                {
                    throw new InvalidOperationException("Only formula cells have cached results");
                }

                return GetBaseCellType(false);
            }
        }

        /// <summary>
        /// Detect cell type based on the "t" attribute of the CT_Cell bean
        /// </summary>
        /// <param name="blankCells"></param>
        /// <returns></returns>
        private CellType GetBaseCellType(bool blankCells)
        {
            switch (_cell.t)
            {
                case ST_CellType.b:
                    return CellType.Boolean;
                case ST_CellType.n:
                    if (!_cell.IsSetV() && blankCells)
                    {
                        // ooxml does have a separate cell type of 'blank'.  A blank cell Gets encoded as
                        // (either not present or) a numeric cell with no value Set.
                        // The formula Evaluator (and perhaps other clients of this interface) needs to
                        // distinguish blank values which sometimes Get translated into zero and sometimes
                        // empty string, depending on context
                        return CellType.Blank;
                    }
                    return CellType.Numeric;
                case ST_CellType.e:
                    return CellType.Error;
                case ST_CellType.s: // String is in shared strings
                case ST_CellType.inlineStr: // String is inline in cell
                case ST_CellType.str:
                    return CellType.String;
                default:
                    throw new InvalidOperationException("Illegal cell type: " + this._cell.t);
            }
        }

        /// <summary>
        /// Get the value of the cell as a date.
        /// </summary>
        public DateTime? DateCellValue
        {
            get
            {
                if (CellType != CellType.Numeric && CellType != CellType.Formula)
                {
                    return null;
                }

                double value = NumericCellValue;
                bool date1904 = Sheet.Workbook.IsDate1904();
                return DateUtil.GetJavaDate(value, date1904);
            }
        }
#if NET6_0_OR_GREATER
        public DateOnly? DateOnlyCellValue 
        { 
            get{
                if (CellType != CellType.Numeric && CellType != CellType.Formula)
                {
                    return null;
                }
                double value = NumericCellValue;
                bool date1904 = Sheet.Workbook.IsDate1904();
                return DateOnly.FromDateTime(DateUtil.GetJavaDate(value, date1904));
            }
        }

        public TimeOnly? TimeOnlyCellValue 
        { 
            get{
                if (CellType != CellType.Numeric && CellType != CellType.Formula)
                {
                    return null;
                }
                double value = NumericCellValue;
                bool date1904 = Sheet.Workbook.IsDate1904();
                return TimeOnly.FromDateTime(DateUtil.GetJavaDate(value, date1904));
            }
        }
#endif
        public void SetCellValue(DateTime? value)
        {
            if (value == null)
            {
                SetCellType(CellType.Blank);
                return;
            }
            SetCellValue(value.Value);
        }
        /// <summary>
        ///  Set a date value for the cell. Excel treats dates as numeric so you will need to format the cell as a date.
        /// </summary>
        /// <param name="value">the date value to Set this cell to.  For formulas we'll set the precalculated value, 
        /// for numerics we'll Set its value. For other types we will change the cell to a numeric cell and Set its value. </param>
        public ICell SetCellValue(DateTime value)
        {
            bool date1904 = Sheet.Workbook.IsDate1904();
            return SetCellValue(DateUtil.GetExcelDate(value, date1904));
        }

#if NET6_0_OR_GREATER
        public ICell SetCellValue(DateOnly value)
        {
            bool date1904 = Sheet.Workbook.IsDate1904();
            return SetCellValue(DateUtil.GetExcelDate(value, date1904));
        }
        
        public ICell SetCellValue(DateOnly? value)
        {
            if (value == null)
            {
                SetCellType(CellType.Blank);
                return this;
            }
            
            return SetCellValue(value.Value);
        }
#endif
        
        /// <summary>
        /// Returns the error message, such as #VALUE!
        /// </summary>
        public String ErrorCellString
        {
            get
            {
                CellType cellType = GetBaseCellType(true);
                if (cellType != CellType.Error) throw TypeMismatch(CellType.Error, cellType, false);

                return _cell.v;
            }
        }

        /// <summary>
        /// Get the value of the cell as an error code.
        /// For strings, numbers, and bools, we throw an exception.
        /// For blank cells we return a 0.
        /// </summary>
        public byte ErrorCellValue
        {
            get
            {
                String code = this.ErrorCellString;
                if (code == null)
                {
                    return 0;
                }

                return FormulaError.ForString(code).Code;
            }
        }
        public ICell SetCellErrorValue(byte errorCode)
        {
            FormulaError error = FormulaError.ForInt(errorCode);
            return SetCellErrorValue(error);
        }
        /// <summary>
        /// Set a error value for the cell
        /// </summary>
        /// <param name="error">the error value to Set this cell to. 
        /// For formulas we'll Set the precalculated value , for errors we'll set
        /// its value. For other types we will change the cell to an error cell and Set its value.
        /// </param>
        public ICell SetCellErrorValue(FormulaError error)
        {
            _cell.t = ST_CellType.e;
            _cell.v = error.String;
            return this;
        }

        /// <summary>
        /// Sets this cell as the active cell for the worksheet.
        /// </summary>
        public void SetAsActiveCell()
        {
            Sheet.ActiveCell = Address;
        }

        /// <summary>
        /// Blanks this cell. Blank cells have no formula or value but may have styling.
        /// This method erases all the data previously associated with this cell.
        /// </summary>
        private void SetBlankInternal()
        {
            CT_Cell blank = new CT_Cell();
            blank.r = (_cell.r);
            if (_cell.IsSetS()) blank.s=(_cell.s);
            _cell.Set(blank);
        }
        public ICell SetBlank()
        {
            return SetCellType(CellType.Blank);
        }

        /// <summary>
        /// Sets column index of this cell
        /// </summary>
        /// <param name="num"></param>
        internal void SetCellNum(int num)
        {
            CheckBounds(num);
            _cellNum = num;
            String ref1 = new CellReference(RowIndex, ColumnIndex).FormatAsString();
            _cell.r = (ref1);
        }
        /// <summary>
        /// Set the cells type (numeric, formula or string)
        /// </summary>
        /// <param name="cellType"></param>
        public ICell SetCellType(CellType cellType)
        {
            CellType prevType = CellType;

            if (IsPartOfArrayFormulaGroup)
            {
                NotifyArrayFormulaChanging();
            }
            if (prevType == CellType.Formula && cellType != CellType.Formula)
            {
                ((XSSFWorkbook)Sheet.Workbook).OnDeleteFormula(this);
            }

            switch (cellType)
            {
                case CellType.Blank:
                    SetBlankInternal();
                    break;
                case CellType.Boolean:
                    String newVal = ConvertCellValueToBoolean() ? TRUE_AS_STRING : FALSE_AS_STRING;
                    _cell.t= (ST_CellType.b);
                    _cell.v= (newVal);
                    break;
                case CellType.Numeric:
                    _cell.t = (ST_CellType.n);
                    break;
                case CellType.Error:
                    _cell.t = (ST_CellType.e);
                    break;
                case CellType.String:
                    if (prevType != CellType.String)
                    {
                        String str = ConvertCellValueToString();
                        XSSFRichTextString rt = new XSSFRichTextString(str);
                        rt.SetStylesTableReference(_stylesSource);
                        int sRef = _sharedStringSource.AddEntry(rt.GetCTRst());
                        _cell.v= sRef.ToString();
                    }
                    _cell.t= (ST_CellType.s);
                    break;
                case CellType.Formula:
                    if (!_cell.IsSetF())
                    {
                        CT_CellFormula f = new CT_CellFormula();
                        f.Value = "0";
                        _cell.f = (f);
                        if (_cell.IsSetT()) _cell.unsetT();
                    }
                    break;
                default:
                    throw new ArgumentException("Illegal cell type: " + cellType);
            }
            if (cellType != CellType.Formula && _cell.IsSetF())
            {
                _cell.unsetF();
            }

            return this;
        }
        /// <summary>
        /// Returns a string representation of the cell
        /// </summary>
        /// <returns>Formula cells return the formula string, rather than the formula result.
        /// Dates are displayed in dd-MMM-yyyy format
        /// Errors are displayed as #ERR&lt;errIdx&gt;
        /// </returns>
        public override String ToString()
        {
            switch (CellType)
            {
                case CellType.Blank:
                    return "";
                case CellType.Boolean:
                    return BooleanCellValue ? "TRUE" : "FALSE";
                case CellType.Error:
                    return ErrorEval.GetText(ErrorCellValue);
                case CellType.Formula:
                    return CellFormula;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(this))
                    {
                        SimpleDateFormat sdf = new SimpleDateFormat("dd-MMM-yyyy");
                        return sdf.Format(DateCellValue, CultureInfo.CurrentCulture);
                    }
                    return NumericCellValue.ToString();
                case CellType.String:
                    return RichStringCellValue.ToString();
                default:
                    return "Unknown Cell Type: " + CellType;
            }
        }

        /**
         * Returns the raw, underlying ooxml value for the cell
         * <p>
         * If the cell Contains a string, then this value is an index into
         * the shared string table, pointing to the actual string value. Otherwise,
         * the value of the cell is expressed directly in this element. Cells Containing formulas express
         * the last calculated result of the formula in this element.
         * </p>
         *
         * @return the raw cell value as Contained in the underlying CT_Cell bean,
         *     <code>null</code> for blank cells.
         */
        public String GetRawValue()
        {
            return _cell.v;
        }

        /// <summary>
        /// Used to help format error messages
        /// </summary>
        /// <param name="cellTypeCode"></param>
        /// <returns></returns>
        private static String GetCellTypeName(CellType cellTypeCode)
        {
            switch (cellTypeCode)
            {
                case CellType.Blank: return "blank";
                case CellType.String: return "text";
                case CellType.Boolean: return "bool";
                case CellType.Error: return "error";
                case CellType.Numeric: return "numeric";
                case CellType.Formula: return "formula";
            }
            return "#unknown cell type (" + cellTypeCode + ")#";
        }

        /**
         * Used to help format error messages
         */
        private static InvalidOperationException TypeMismatch(CellType expectedTypeCode, CellType actualTypeCode, bool isFormulaCell)
        {
            String msg = "Cannot get a "
                + GetCellTypeName(expectedTypeCode) + " value from a "
                + GetCellTypeName(actualTypeCode) + " " + (isFormulaCell ? "formula " : "") + "cell";
            return new InvalidOperationException(msg);
        }

        /**
         * @throws RuntimeException if the bounds are exceeded.
         */
        private static void CheckBounds(int cellIndex)
        {
            SpreadsheetVersion v = SpreadsheetVersion.EXCEL2007;
            int maxcol = SpreadsheetVersion.EXCEL2007.LastColumnIndex;
            if (cellIndex < 0 || cellIndex > maxcol)
            {
                throw new ArgumentException("Invalid column index (" + cellIndex
                        + ").  Allowable column range for " + v.ToString() + " is (0.."
                        + maxcol + ") or ('A'..'" + v.LastColumnName + "')");
            }
        }

        /// <summary>
        ///  Returns cell comment associated with this cell
        /// </summary>
        public IComment CellComment
        {
            get
            {
                return Sheet.GetCellComment(new CellAddress(this));
            }
            set 
            {
                if (value == null)
                {
                    RemoveCellComment();
                    return;
                }
                value.SetAddress(RowIndex, ColumnIndex);
            }
        }

        /// <summary>
        /// Removes the comment for this cell, if there is one.
        /// </summary>
        public void RemoveCellComment() {
            IComment comment = this.CellComment;
            if (comment != null)
            {
                CellAddress ref1 = new CellAddress(GetReference());
                XSSFSheet sh = (XSSFSheet)Sheet;
                sh.GetCommentsTable(false).RemoveComment(ref1);
                sh.GetVMLDrawing(false).RemoveCommentShape(RowIndex, ColumnIndex);
            }
        }

        /// <summary>
        /// Get or set hyperlink associated with this cell
        /// If the supplied hyperlink is null on setting, the hyperlink for this cell will be removed.
        /// </summary>
        public IHyperlink Hyperlink
        {
            get
            {
                return ((XSSFSheet)Sheet).GetHyperlink(_row.RowNum, _cellNum);
            }
            set 
            {
                if (value == null)
                {
                    RemoveHyperlink();
                    return;
                }
                XSSFHyperlink link = (XSSFHyperlink)value;

                // Assign to us
                link.SetCellReference(new CellReference(_row.RowNum, _cellNum).FormatAsString());

                // Add to the lists
                ((XSSFSheet)Sheet).AddHyperlink(link);
            }
        }

        /**
         * Removes the hyperlink for this cell, if there is one.
         */
        public void RemoveHyperlink()
        {
            ((XSSFSheet)Sheet).RemoveHyperlink(_row.RowNum, _cellNum);
        }
        /**
         * Returns the xml bean containing information about the cell's location (reference), value,
         * data type, formatting, and formula
         *
         * @return the xml bean containing information about this cell
         */
        internal CT_Cell GetCTCell()
        {
            return _cell;
        }

        /**
         * Chooses a new bool value for the cell when its type is changing.<p/>
         *
         * Usually the caller is calling SetCellType() with the intention of calling
         * SetCellValue(bool) straight afterwards.  This method only exists to give
         * the cell a somewhat reasonable value until the SetCellValue() call (if at all).
         * TODO - perhaps a method like SetCellTypeAndValue(int, Object) should be introduced to avoid this
         */
        private bool ConvertCellValueToBoolean()
        {
            CellType cellType = CellType;

            if (cellType == CellType.Formula)
            {
                cellType = GetBaseCellType(false);
            }

            switch (cellType)
            {
                case CellType.Boolean:
                    return TRUE_AS_STRING.Equals(_cell.v);
                case CellType.String:
                    int sstIndex = Int32.Parse(_cell.v);
                    XSSFRichTextString rt = new XSSFRichTextString(_sharedStringSource.GetEntryAt(sstIndex));
                    String text = rt.String;
                    return Boolean.Parse(text);
                case CellType.Numeric:
                    return Double.Parse(_cell.v, CultureInfo.InvariantCulture) != 0;

                case CellType.Error:
                case CellType.Blank:
                    return false;
            }
            throw new RuntimeException("Unexpected cell type (" + cellType + ")");
        }

        private String ConvertCellValueToString()
        {
            CellType cellType = CellType;

            switch (cellType)
            {
                case CellType.Blank:
                    return "";
                case CellType.Boolean:
                    return TRUE_AS_STRING.Equals(_cell.v) ? "TRUE" : "FALSE";
                case CellType.String:
                    int sstIndex = Int32.Parse(_cell.v);
                    XSSFRichTextString rt = new XSSFRichTextString(_sharedStringSource.GetEntryAt(sstIndex));
                    return rt.String;
                case CellType.Numeric:
                case CellType.Error:
                    return _cell.v;
                case CellType.Formula:
                    // should really Evaluate, but HSSFCell can't call HSSFFormulaEvaluator
                    // just use cached formula result instead
                    break;
                default:
                    throw new InvalidOperationException("Unexpected cell type (" + cellType + ")");
            }
            cellType = GetBaseCellType(false);
            String textValue = _cell.v;
            switch (cellType)
            {
                case CellType.Boolean:
                    if (TRUE_AS_STRING.Equals(textValue))
                    {
                        return "TRUE";
                    }
                    if (FALSE_AS_STRING.Equals(textValue))
                    {
                        return "FALSE";
                    }
                    throw new InvalidOperationException("Unexpected bool cached formula value '"
                        + textValue + "'.");
                case CellType.String:
                case CellType.Numeric:
                case CellType.Error:
                    return textValue;
            }
            throw new InvalidOperationException("Unexpected formula result type (" + cellType + ")");
        }

        public CellRangeAddress ArrayFormulaRange
        {
            get
            {
                XSSFCell cell = ((XSSFSheet)Sheet).GetFirstCellInArrayFormula(this);
                if (cell == null)
                {
                    throw new InvalidOperationException("Cell " + GetReference()
                            + " is not part of an array formula.");
                }
                String formulaRef = cell._cell.f.@ref;
                return CellRangeAddress.ValueOf(formulaRef);
            }
        }

        public bool IsPartOfArrayFormulaGroup
        {
            get
            {
                return ((XSSFSheet)Sheet).IsCellInArrayFormulaContext(this);
            }
        }

        /**
         * The purpose of this method is to validate the cell state prior to modification
         *
         * @see #NotifyArrayFormulaChanging()
         */
        internal void NotifyArrayFormulaChanging(String msg)
        {
            if (IsPartOfArrayFormulaGroup)
            {
                CellRangeAddress cra = this.ArrayFormulaRange;
                if (cra.NumberOfCells > 1)
                {
                    throw new InvalidOperationException(msg);
                }
                //un-register the Single-cell array formula from the parent XSSFSheet
                Row.Sheet.RemoveArrayFormula(this);
            }
        }

        /// <summary>
        /// Called when this cell is modified.The purpose of this method is to validate the cell state prior to modification.
        /// </summary>
        /// <exception cref="InvalidOperationException">if modification is not allowed</exception>
        internal void NotifyArrayFormulaChanging()
        {
            CellReference ref1 = new CellReference(this);
            String msg = "Cell " + ref1.FormatAsString() + " is part of a multi-cell array formula. " +
                    "You cannot change part of an array.";
            NotifyArrayFormulaChanging(msg);
        }

        #region ICell Members


        public bool IsMergedCell
        {
            get {
                return this.Sheet.IsMergedRegion(new CellRangeAddress(this.RowIndex, this.RowIndex, this.ColumnIndex, this.ColumnIndex));
            }
            
        }

        #endregion


        public ICell CopyCellTo(int targetIndex)
        {
            return CellUtil.CopyCell(this.Row, this.ColumnIndex, targetIndex);
        }

        [Obsolete("Will be removed at NPOI 2.8, Use CachedFormulaResultType instead.")]
        public CellType GetCachedFormulaResultTypeEnum()
        {
            if(!IsFormulaCell)
            {
                throw new InvalidOperationException("Only formula cells have cached results");
            }

            return GetBaseCellType(false);
        }
    }
}
