using Microsoft.VisualStudio.TestTools.UnitTesting;
using TriWinDirMover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriWinDirMover.Tests
{
    [TestClass()]
    public class DirectorySetTests
    {
        [TestMethod()]
        public void AccessTest()
        {
            DirectorySet dirSet = new DirectorySet(@"C:\Games\Foo", @"Z:\Foo");
            Assert.AreEqual(dirSet.Source.HasError, false);
            Assert.AreEqual(dirSet.Target.HasError, true);
        }
        [TestMethod()]
        public void DirectorySetTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetHashCodeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void EqualsTest()
        {
            Assert.Fail();
        }
    }
}