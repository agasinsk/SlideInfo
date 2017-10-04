using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlideInfo.Core;
using SlideInfo.Helpers;

namespace SlideInfo.Test
{
	[TestClass]
	public class BoxesDeepZoomTest
	{
		private OpenSlide osr;
		private DeepZoomGenerator dz;

		[TestInitialize]
		public void SetUp()
		{	
			osr = new OpenSlide("tests/boxes.tiff");
			dz = new DeepZoomGenerator(osr);
		}

		[TestCleanup]
		public void TearDown()
		{
			osr.Close();
		}

		[TestMethod]
		public void TestMetadata()
		{
			Assert.AreEqual(10, dz.LevelCount);
			Assert.AreEqual(11, dz.TilesCount);
			SizeL[] testTileDimensions = {  new SizeL(1, 1), new SizeL(1, 1), new SizeL(1, 1),
												new SizeL(1, 1), new SizeL(1, 1), new SizeL(1, 1),
												new SizeL(1, 1), new SizeL(1, 1), new SizeL(1, 1),
												new SizeL(2, 1) };
			CollectionAssert.AreEqual(testTileDimensions, dz.TileDimensions);

			var testLevelDimensions = new List<SizeL>
			{
				new SizeL(1,1), new SizeL(2,1), new SizeL(3,2), new SizeL(5,4),
				new SizeL(10,8), new SizeL(19,16), new SizeL(38,32), new SizeL(75,63),
				new SizeL(150,125), new SizeL(300,250)
			};
			CollectionAssert.AreEqual(testLevelDimensions, dz.DeepZoomLevelDimensions as ICollection);
		}

		[TestMethod]
		public void TestGetTile()
		{
			Assert.AreEqual(new Size(47, 250), dz.GetTile(9, new SizeL(1, 0)).Size);
		}

		[TestMethod]
		public void TestGetTileBadLevel()
		{
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(-1, new SizeL(0, 0)));
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(10, new SizeL(0, 0)));
		}

		[TestMethod]
		public void TestGetTileBadAdress()
		{
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(0, new SizeL(-1, 0)));
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(0, new SizeL(1, 0)));
		}

		[TestMethod]
		public void TestGetDziMetadataString()
		{
			Assert.IsTrue(dz.GetDziMetadataString("jpeg").Contains("http://schemas.microsoft.com/deepzoom/2008"));
			Assert.IsTrue(dz.GetDziMetadataString("JPEG").Contains("jpeg"));
			Assert.IsTrue(dz.GetDziMetadataString("jpeg").Contains("254"));
			Assert.IsTrue(dz.GetDziMetadataString("jpeg").Contains("1"));
			Assert.IsTrue(dz.GetDziMetadataString("PNG").Contains("png"));
			MyAssert.Throws<ArgumentException>(() => dz.GetDziMetadataString("gif"));
			MyAssert.Throws<ArgumentException>(() => dz.GetDziMetadataString("gif"));
		}

		[TestMethod]
		public void TestToString()
		{
			Assert.AreEqual($"DeepZoomGenerator({osr}, tile_size = 254, overlap = 1, limit_bounds = True)", dz.ToString());
		}
	}

	[TestClass]
	public class BoxesImageDeepZoomTest
	{
		private ImageSlide osr;
		private DeepZoomGenerator dz;

		[TestInitialize]
		public void SetUp()
		{
			osr = new ImageSlide("tests/boxes.png");
			dz = new DeepZoomGenerator(osr);
		}

		[TestCleanup]
		public void TearDown()
		{
			osr.Close();
		}

		[TestMethod]
		public void TestMetadata()
		{
			Assert.AreEqual(10, dz.LevelCount);
			Assert.AreEqual(11, dz.TilesCount);
			SizeL[] testTileDimensions = {  new SizeL(1, 1), new SizeL(1, 1), new SizeL(1, 1),
				new SizeL(1, 1), new SizeL(1, 1), new SizeL(1, 1),
				new SizeL(1, 1), new SizeL(1, 1), new SizeL(1, 1),
				new SizeL(2, 1) };
			CollectionAssert.AreEqual(dz.TileDimensions, testTileDimensions);

			var testLevelDimensions = new List<SizeL>
			{
				new SizeL(1,1), new SizeL(2,1), new SizeL(3,2), new SizeL(5,4),
				new SizeL(10,8), new SizeL(19,16), new SizeL(38,32), new SizeL(75,63),
				new SizeL(150,125), new SizeL(300,250)
			};
			var dzLevelDimensions = dz.DeepZoomLevelDimensions;
			CollectionAssert.AreEqual(dzLevelDimensions as ICollection, testLevelDimensions);
		}

		[TestMethod]
		public void TestGetTile()
		{
			Assert.AreEqual(new Size(47, 250), dz.GetTile(9, new SizeL(1, 0)).Size);
		}

		[TestMethod]
		public void TestGetTileBadLevel()
		{
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(-1, new SizeL(0, 0)));
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(10, new SizeL(0, 0)));
		}

		[TestMethod]
		public void TestGetTileBadAdress()
		{
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(0, new SizeL(-1, 0)));
			MyAssert.Throws<ArgumentException>(() => dz.GetTile(0, new SizeL(1, 0)));
		}

		[TestMethod]
		public void TestGetDziMetadataString()
		{
			Assert.IsTrue(dz.GetDziMetadataString("jpeg").Contains("http://schemas.microsoft.com/deepzoom/2008"));
			Assert.IsTrue(dz.GetDziMetadataString("JPEG").Contains("jpeg"));
			Assert.IsTrue(dz.GetDziMetadataString("jpeg").Contains("254"));
			Assert.IsTrue(dz.GetDziMetadataString("jpeg").Contains("1"));
			Assert.IsTrue(dz.GetDziMetadataString("PNG").Contains("png"));
			MyAssert.Throws<ArgumentException>(() => dz.GetDziMetadataString("gif"));
			MyAssert.Throws<ArgumentException>(() => dz.GetDziMetadataString("gif"));
		}

		[TestMethod]
		public void TestToString()
		{
			Assert.AreEqual($"DeepZoomGenerator({osr}, tile_size = 254, overlap = 1, limit_bounds = True)", dz.ToString());
		}
	}
}
