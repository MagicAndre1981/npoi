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

using NPOI.OpenXmlFormats.Dml;
using NPOI.OpenXmlFormats.Dml.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.Util;
using System;
namespace NPOI.XSSF.UserModel
{

    /**
     * Represents a shape in a SpreadsheetML Drawing.
     *
     * @author Yegor Kozlov
     */
    public abstract class XSSFShape: IShape
    {
        [Obsolete]
        public static int EMU_PER_PIXEL = 9525;
        [Obsolete]
        public static int EMU_PER_POINT = 12700;
        [Obsolete]
        public static int POINT_DPI = 72;
        [Obsolete]
        public static int PIXEL_DPI = 96;

        /**
         * Parent Drawing
         */
        protected XSSFDrawing drawing;

        protected IEG_Anchor IEGanchor;
        /**
         * The parent shape, always not-null for shapes in groups
         */
        public XSSFShapeGroup parent;

        /**
         * anchor that is used by this shape
         */
        internal XSSFAnchor anchor;

        public virtual string ShapeName => throw new System.NotImplementedException();

        public virtual IChildAnchor Anchor => throw new System.NotImplementedException();
        /**
         * Return the Drawing that owns this shape
         *
         * @return the parent Drawing that owns this shape
         */
        public XSSFDrawing GetDrawing()
        {
            return drawing;
        }

        /**
         * Gets the parent shape.
         */
        public IShape Parent
        {
            get
            {
                return parent;
            }
        }

        /**
         * @return  the anchor that is used by this shape.
         */
        public XSSFAnchor GetAnchor()
        {
            return anchor;
        }

        public IEG_Anchor cellanchor
        {
            get
            {
                if(IEGanchor == null) {
                    return parent.cellanchor;
                }
                return IEGanchor;
            }
            set
            {
                if(parent == null)
                {
                    IEGanchor = value;
                }
                else
                {
                    parent.cellanchor = value;
                }
            }
        }
        /**
         * Returns xml bean with shape properties.
         *
         * @return xml bean with shape properties.
         */
        protected internal abstract NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties GetShapeProperties();

        public abstract uint ID
        {
            get;
        }

        public abstract string Name
        {
            get;
            set;
        }

        /**
         * Whether this shape is not Filled with a color
         *
         * @return true if this shape is not Filled with a color.
         */
        public bool IsNoFill
        {
            get
            {
                return GetShapeProperties().noFill != null;
            }
            set 
            {
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
                //unset solid and pattern Fills if they are Set
                if (props.IsSetPattFill()) props.unsetPattFill();
                if (props.IsSetSolidFill()) props.unsetSolidFill();

                props.noFill = new CT_NoFillProperties();
            }
        }


        /**
         * Sets the color used to fill this shape using the solid fill pattern.
         */
        public void SetFillColor(int red, int green, int blue)
        {
            NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
            CT_SolidColorFillProperties fill = props.IsSetSolidFill() ? props.solidFill : props.AddNewSolidFill();
            CT_SRgbColor rgb = new CT_SRgbColor();
            rgb.val = (new byte[] { (byte)red, (byte)green, (byte)blue });
            fill.srgbClr = (rgb);
        }

        /**
         * The color applied to the lines of this shape.
         */
        public void SetLineStyleColor(int red, int green, int blue)
        {
            NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
            CT_LineProperties ln = props.IsSetLn() ? props.ln : props.AddNewLn();
            CT_SolidColorFillProperties fill = ln.IsSetSolidFill() ? ln.solidFill : ln.AddNewSolidFill();
            CT_SRgbColor rgb = new CT_SRgbColor();
            rgb.val = (new byte[] { (byte)red, (byte)green, (byte)blue });
            fill.srgbClr = (rgb);
        }



        public int CountOfAllChildren
        {
            get { throw new System.NotImplementedException(); }
        }

        public int FillColor
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                SetFillColor(value >> 16 & 0xff
                           , value >>  8 & 0xff
                           , value       & 0xff);
            }
        }

        public virtual LineStyle LineStyle
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();

                if(value == LineStyle.None) {
                    props.ln = null;
                } else {

                    CT_LineProperties ln = props.IsSetLn() ? props.ln : props.AddNewLn();
                    CT_PresetLineDashProperties dashStyle = new CT_PresetLineDashProperties();
                    dashStyle.val = (ST_PresetLineDashVal)value;
                    ln.prstDash = dashStyle;
                    props.ln = ln;
                }
            }
        }

        public virtual int LineStyleColor
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                SetLineStyleColor(value >> 16 & 0xff
                                , value >>  8 & 0xff
                                , value       & 0xff);
            }
        }

        public virtual double LineWidth
        {
            get
            {
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
                if (props.IsSetLn())
                {
                    return props.ln.w*1.0 / Units.EMU_PER_POINT;
                }
                else
                {
                    return 0.0;
                }
            }
            set
            {
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
                CT_LineProperties ln = props.IsSetLn() ? props.ln : props.AddNewLn();
                ln.w = (int)(value * Units.EMU_PER_POINT);
            }
        }

        public virtual LineEndingCapType LineEndingCapType
        {
            get
            {
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
                return props.IsSetLn() ? (LineEndingCapType)props.ln.cap : LineEndingCapType.None;
            }
            set
            {
                if(value == LineEndingCapType.None)
                {
                    throw new System.ArgumentException();
                }
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
                CT_LineProperties ln = props.IsSetLn() ? props.ln : props.AddNewLn();
                ln.cap = (ST_LineCap) value;
            }
        }

        public virtual CompoundLineType CompoundLineType
        {
            get
            {
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
                return props.IsSetLn() ? (CompoundLineType)props.ln.cmpd : CompoundLineType.None;
            }
            set
            {
                if(value == CompoundLineType.None)
                {
                    throw new System.ArgumentException();
                }
                NPOI.OpenXmlFormats.Dml.Spreadsheet.CT_ShapeProperties props = GetShapeProperties();
                CT_LineProperties ln = props.IsSetLn() ? props.ln : props.AddNewLn();
                ln.cmpd = (ST_CompoundLine)value;
            }
        }
    }
}



