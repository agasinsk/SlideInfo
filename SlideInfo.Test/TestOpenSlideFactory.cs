using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlideInfo.Core;

namespace SlideInfo.Test
{
    [TestClass]
    public class TestOpenSlideFactory
    {
        [TestMethod]
        public void ShouldReturnOpenSlideObject()
        {
            var osr = AbstractSlideFactory.GetSlide("tests/boxes.tiff");

            Assert.IsTrue(osr is OpenSlide);
        }

        [TestMethod]
        public void ShouldReturnImageSlideObject()
        {
            var osr = AbstractSlideFactory.GetSlide("tests/boxes.png");

            Assert.IsTrue(osr is ImageSlide);
        }
    }
}
