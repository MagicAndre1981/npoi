
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


namespace NPOI.Util
{
    using System;

    /**
     * A common exception thrown by our binary format Parsers
     *  (especially HSSF and DDF), when they hit invalid
     *  format or data when Processing a record.
     */
    [Serializable]
    public class RecordFormatException
        : RuntimeException
    {
        public RecordFormatException(String exception):
            base(exception)
        {
        }

        public RecordFormatException(String exception, Exception ex)
            : base(exception, ex)
        {

        }

        public RecordFormatException(Exception ex):
            base(ex)
        {
        }

        /**
         * Syntactic sugar to check whether a RecordFormatException should
         * be thrown.  If assertTrue is <code>false</code>, this will throw this
         * exception with the message.
         *
         * @param assertTrue
         * @param message
         */
        public static void Check(bool assertTrue, String message)
        {
            if (! assertTrue)
            {
                throw new RecordFormatException(message);
            }
        }
    }

}