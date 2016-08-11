using Microsoft.VisualStudio.TestTools.UnitTesting;
using TriWinDirMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace TriWinDirMover.Tests
{
    [TestClass()]
    public class DirectoryTests
    {
        private Directory dir;

        public DirectoryTests()
        {
            dir = new Directory(new System.IO.DirectoryInfo(@"C:\Games\Guild Wars 2"));
        }

        [TestMethod()]
        public void CalculateSize()
        {
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            long size = 0;
            int timeout = 60 * 1000;
            dir.PropertyChanged += delegate (object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                size = ((Directory)dir).Size;
                Debug.WriteLine("Size: " + size);
                waitHandle.Set();
            };
            /*
            dir.CalculateSize();
            dir.StopCalculateSize();

            if (waitHandle.WaitOne(timeout))
            {
                Assert.AreEqual(size, Directory.SizeValue.NotCalculated);
            }
            waitHandle.Reset();
            */
            dir.CalculateSize();
            if (waitHandle.WaitOne(timeout))
            {
                Assert.AreEqual(size > 0, true);
            }
        }
        [TestMethod()]
        public void DirectoryTest()
        {
            Debug.WriteLine("Error: " + dir.Error);
            Assert.AreEqual(dir.HasError, false);
        }

        [TestMethod()]
        public void GetDirectoriesTest()
        {
            var dirs = dir.GetDirectories();
            Assert.AreEqual(dirs.Count(), 2);
        }
    }
}