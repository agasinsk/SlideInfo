using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlideInfo.Core;
using SlideInfo.Helpers;

namespace SlideInfo.Test
{
	[TestClass]
	public class TestImageSlide
	{
		[TestMethod]
		public void TestDetectFormat()
		{
			var osr = new ImageSlide();
			Assert.IsTrue(osr.DetectFormat("nonExisting") == null);
			Assert.IsTrue(osr.DetectFormat("setup.py") == null);
			Assert.IsTrue(osr.DetectFormat("tests/boxes.png").Contains("Png", StringComparison.OrdinalIgnoreCase));
		}

		[TestMethod]
		public void TestOpen()
		{
			MyAssert.Throws<IOException>(() => new ImageSlide("does_not_exist"));
			MyAssert.Throws<IOException>(() => new ImageSlide("setup.py"));
		}

		[TestMethod]
		public void TestOpenImage()
		{
			var img = Image.FromFile("tests/boxes.png");
			var osr = new ImageSlide(img);
			Assert.AreEqual(osr.Dimensions, new SizeL(300, 250));
		}

		[TestMethod]
		public void TestOperationsOnClosedHandle()
		{
			var img = Image.FromFile("tests/boxes.png");
			var osr = new ImageSlide(img);
			osr.Close();
			MyAssert.Throws<Exception>(() => osr.ReadRegion(new SizeL(0, 0), 0, new SizeL(100, 100)));
			//# If an Image is passed to the constructor, ImageSlide.close() shouldn't close it
			var bitmap = img as Bitmap;
			Assert.AreEqual(Color.White.ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
		}

	}

	[TestClass]
	public class TestImage
	{
		private ImageSlide osr;

		[TestInitialize]
		public void SetUp()
		{
			osr = new ImageSlide("tests/boxes.png");
		}

		[TestCleanup]
		public void TearDown()
		{
			osr.Close();
		}

		[TestMethod]
		public void TestMetadata()
		{
			Assert.AreEqual(1, osr.LevelCount);
			Assert.AreEqual(new SizeL(300, 250), osr.LevelDimensions[0]);
			Assert.AreEqual(new SizeL(300, 250), osr.Dimensions);
			Assert.AreEqual(1.0, osr.LevelDownsamples[0]);

			Assert.AreEqual(0, osr.GetBestLevelForDownsample(0.5));
			Assert.AreEqual(0, osr.GetBestLevelForDownsample(3));

			Assert.AreEqual(null, osr.Properties);
			Assert.AreEqual(null, osr.AssociatedImages);
		}

		[TestMethod]
		public void TestReadRegion()
		{
			Assert.AreEqual(new Size(400, 400), osr.ReadRegion(new SizeL(-10, -10), 0, new SizeL(400, 400)).Size);
		}

		[TestMethod]
		public void TestReadRegionSizeDimensionZero()
		{
			MyAssert.Throws<ArgumentException>(() => osr.ReadRegion(new SizeL(0, 0), 0, new SizeL(400, 0)));
		}

		[TestMethod]
		public void TestReadRegionBadLevel()
		{
			MyAssert.Throws<OpenSlideException>(() => osr.ReadRegion(new SizeL(0, 0), 1, new SizeL(100, 100)));
		}

		[TestMethod]
		public void TestReadRegionBadSize()
		{
			MyAssert.Throws<OpenSlideException>(() => osr.ReadRegion(new SizeL(0, 0), 1, new SizeL(400, -5)));
		}

		[TestMethod]
		public void TestThumbnail()
		{
			Assert.AreEqual(new Size(100, 83), osr.GetThumbnail(new SizeL(100, 83)).Size);
		}

		[TestMethod]
		public void TestToString()
		{
			Assert.AreEqual("ImageSlide(tests/boxes.png)", osr.ToString());
		}


	}
}
