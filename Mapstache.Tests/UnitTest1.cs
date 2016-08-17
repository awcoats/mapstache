using System;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebStache;

namespace WebMap.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethodWithLargeRangeOfIds()
        {
             for(int i=1;i<65536;i=i+1000)
             {
                 using (var utfGrid = new Utf8Grid(1,1,1,1))
                 using ( var graphics = utfGrid.CreateGraphics())
                 using ( var brush = Utf8Grid.CreateBrush(i))
                 {
                     graphics.FillRectangle(brush, 0, 0, 256, 256);
                     var json = utfGrid.CreateUtfGridJson();
                     Assert.AreEqual(1, utfGrid.Keys.Count, "With i=" + i.ToString());
                     Assert.AreEqual(i.ToString(), utfGrid.Keys[0], "With i=" + i.ToString());   
                 }
                
             }
        }

        [TestMethod]
        public void TestMethod2()
        {
            
            for (int i = 255; i < 300; i = i + 1)
            {
                using (var utfGrid = new Utf8Grid(1,1,1,1))
                using (var graphics = utfGrid.CreateGraphics())
                using (var brush = Utf8Grid.CreateBrush(i))
                {
                    graphics.FillRectangle(brush, 0, 0, 256, 256);
                    var json = utfGrid.CreateUtfGridJson();
                    Assert.AreEqual(1, utfGrid.Keys.Count, "With i=" + i.ToString());
                    Assert.AreEqual(i.ToString(), utfGrid.Keys[0], "With i=" + i.ToString());
                }
            }
        }

        [TestMethod]
        public void TestMethodWithPartiallyFillTileReturnsEmptyStringId()
        {
            using (var utfGrid = new Utf8Grid(1,1,1,1))
            {
                var graphics = utfGrid.CreateGraphics();
                var brush = Utf8Grid.CreateBrush(5);
                graphics.FillRectangle(brush, 0, 0, 10, 10);
                var json = utfGrid.CreateUtfGridJson();
                Assert.AreEqual(2, utfGrid.Keys.Count);
                Assert.AreEqual("", utfGrid.Keys[0]);
                Assert.AreEqual("5", utfGrid.Keys[1]);
            }
        }
    }
}
