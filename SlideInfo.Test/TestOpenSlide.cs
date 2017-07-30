using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlideInfo.Core;

namespace SlideInfo.Test
{
	[TestClass]
	public class TestSlideWithoutOpening
	{
		[TestMethod()]
		public void TestDetectFormat()
		{
			var osr = new OpenSlide();
			Assert.IsTrue(osr.DetectFormat("nonExisting") == null);
			Assert.IsTrue(osr.DetectFormat("setup.py") == null);
			Assert.IsTrue(osr.DetectFormat("tests/boxes.tiff") == "generic-tiff");
		}

		[TestMethod()]
		public void TestOpen()
		{
			MyAssert.Throws<OpenSlideException>(() => new OpenSlide("does_not_exist"));
			MyAssert.Throws<OpenSlideException>(() => new OpenSlide("setup.py"));
			MyAssert.Throws<OpenSlideException>(() => new OpenSlide("tests/unopenable.tiff"));
		}

		[TestMethod()]
		[ExpectedException(typeof(AccessViolationException))]
		public void TestReadRegionOnClosedHandle()
		{
			var osr = new OpenSlide("tests/boxes.tiff");
			osr.Close();

			osr.ReadRegion(new SizeL(0, 0), 0, new SizeL(100, 100));
		}

		[TestMethod()]
		public void TestContextManager()
		{
			var osr = new OpenSlide("tests/boxes.tiff");
			Assert.AreEqual(4, osr.LevelCount);
		}
	}

	[TestClass]
	public class TestSlide
	{
		private OpenSlide osr;
		private readonly string fileName = "tests/boxes.tiff";

		[TestInitialize]
		public void SetUp()
		{
			osr = new OpenSlide(fileName);
		}

		[TestCleanup]
		public void TearDown()
		{
			osr.Close();
		}

		[TestMethod]
		public void TestToString()
		{
			Assert.AreEqual("OpenSlide(tests/boxes.tiff)", osr.ToString());
		}

		[TestMethod]
		public void TestBasicMetadata()
		{
			Assert.AreEqual(4, osr.LevelCount);

			var testLevelDimensions = new List<SizeL>(4)
			{
				new SizeL(300, 250),
				new SizeL(150, 125),
				new SizeL(75, 62),
				new SizeL(37, 31)
			};

			CollectionAssert.AreEqual(testLevelDimensions, (ICollection)osr.LevelDimensions);
			Assert.AreEqual(new SizeL(300, 250), osr.LevelDimensions[0]);
			Assert.AreEqual(osr.LevelCount, osr.LevelDownsamples.Count);
			Assert.AreEqual(1, (int)osr.LevelDownsamples[0]);
			Assert.AreEqual(2, (int)osr.LevelDownsamples[1]);
			Assert.AreEqual(4, (int)osr.LevelDownsamples[2]);
			Assert.AreEqual(8, (int)osr.LevelDownsamples[3]);

			Assert.AreEqual(0, osr.GetBestLevelForDownsample(0.5));
			Assert.AreEqual(1, osr.GetBestLevelForDownsample(3));
			Assert.AreEqual(3, osr.GetBestLevelForDownsample(37));
		}


		[TestMethod]
		[ExpectedException(typeof(OpenSlideException))]
		public void TestProperties()
		{
			Assert.AreEqual("generic-tiff", osr.Properties["openslide.vendor"]);
			var nonexisting = osr.Properties["nonExisting"];
		}

		[TestMethod]
		public void TestReadRegion()
		{
			Assert.AreEqual(new Size(400, 400), osr.ReadRegion(new SizeL(-10, -10), 1, new SizeL(400, 400)).Size);
		}

		[TestMethod]
		public void TestReadRegionSizeDimensionZero()
		{
			MyAssert.Throws<ArgumentException>(() => osr.ReadRegion(new SizeL(0, 0), 0, new SizeL(400, 0)));
		}

		[TestMethod]
		public void TestReadRegionBadLevel()
		{
			Assert.AreEqual(new Size(100, 100), osr.ReadRegion(new SizeL(0, 0), 4, new SizeL(100, 100)).Size);
		}

		[TestMethod]
		public void TestReadRegionBadSize()
		{
			MyAssert.Throws<OpenSlideException>(() => osr.ReadRegion(new SizeL(0, 0), 1, new SizeL(400, -5)));
		}

		[TestMethod]
		public void TestThumbnail()
		{
			Assert.AreEqual(new Size(100, 83), osr.GetThumbnail(new SizeL(100, 100)).Size);
		}

	}

	[TestClass]
	public class TestAperioSlide
	{
		private OpenSlide osr;
		private readonly string filename = "tests/small.svs";

		[TestInitialize]
		public void SetUp()
		{
			osr = new OpenSlide(filename);
		}

		[TestCleanup]
		public void TearDown()
		{
			osr.Close();
		}

		[TestMethod]
		[ExpectedException(typeof(OpenSlideException))]
		public void TestAssociatedImages()
		{
			Assert.AreEqual(new Size(16, 16), osr.ReadAssociatedImage("thumbnail").Size);
			var nonexisting = osr.AssociatedImages["nonExisting"];
		}
	}

	[TestClass]
	public class TestUnreadableSlide
	{
		private OpenSlide osr;
		private readonly string filename = "tests/unreadable.svs";

		[TestInitialize]
		public void SetUp()
		{
			osr = new OpenSlide(filename);
		}

		[TestCleanup]
		public void TearDown()
		{
			osr.Close();
		}

		[TestMethod]
		public void TestReadBadRegion()
		{
			Assert.AreEqual(osr.Properties["openslide.vendor"], "aperio");
			MyAssert.Throws<OpenSlideException>(() => osr.ReadRegion(new SizeL(0, 0), 0, new SizeL(16, 16)));
		}

		[TestMethod]
		[ExpectedException(typeof(OpenSlideException))]
		public void TestReadBadAssociatedImage()
		{
			Assert.AreEqual(osr.Properties["openslide.vendor"], "aperio");
			var nonexisting = osr.AssociatedImages["macro"];
		}
	}
}


