﻿using NPOI.Util;
using NUnit.Framework;using NUnit.Framework.Legacy;
using System.IO;
using System.Threading;

namespace TestCases.Util
{
    /// <summary>
    /// Tests of creating temp files
    /// </summary>
    [TestFixture]
    internal class TestTempFile
    {
        [Test]
        public void TestCreateTempFile()
        {
            FileInfo fileInfo = null;
            Assert.DoesNotThrow(() => fileInfo = TempFile.CreateTempFile("test", ".xls"));

            ClassicAssert.IsTrue(fileInfo!=null && fileInfo.Exists);

            string tempDirPath = Path.GetDirectoryName(fileInfo.FullName);

            while(Directory.Exists(tempDirPath))
            {
                try
                {
                    Directory.Delete(tempDirPath, true);
                }
                catch 
                {
                    Thread.Sleep(5);
                }
            }

            ClassicAssert.IsFalse(Directory.Exists(tempDirPath));

            if(fileInfo!=null)
            {
                fileInfo.Refresh();
                ClassicAssert.IsFalse(fileInfo.Exists);
            }

            FileInfo file = null;
            Assert.DoesNotThrow(() => file = TempFile.CreateTempFile("test2", ".xls"));
            ClassicAssert.IsTrue(Directory.Exists(tempDirPath));

            if(file !=null && file.Exists)
                file.Delete();
        }

        [Test]
        public void TestGetTempFilePath()
        {
            string path = "";
            Assert.DoesNotThrow(() => path = TempFile.GetTempFilePath("test", ".xls"));

            ClassicAssert.IsTrue(!string.IsNullOrWhiteSpace(path));

            string tempDirPath = Path.GetDirectoryName(path);

            while(Directory.Exists(tempDirPath))
            {
                try
                {
                    Directory.Delete(tempDirPath, true);
                }
                catch 
                {
                    Thread.Sleep(10);
                }
            }

            ClassicAssert.IsFalse(Directory.Exists(tempDirPath));

            Assert.DoesNotThrow(() => TempFile.GetTempFilePath("test", ".xls"));
            ClassicAssert.IsTrue(Directory.Exists(tempDirPath));
        }
    }
}
