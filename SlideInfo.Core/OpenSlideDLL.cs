using System;
using System.Runtime.InteropServices;

namespace SlideInfo.Core
{
	internal unsafe class OpenSlideDll
	{
		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr openslide_detect_vendor([MarshalAs(UnmanagedType.LPStr)] string filename);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int* openslide_open([MarshalAs(UnmanagedType.LPStr)] string filename);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int openslide_get_level_count(int* osr);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void openslide_get_level0_dimensions(int* osr, out long w, out long h);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void openslide_get_level_dimensions(int* osr, int level, out long w, out long h);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern double openslide_get_level_downsample(int* osr, int level);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int openslide_get_best_level_for_downsample(int* osr, double downsample);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void openslide_read_region(int* osr,
			void* dest,
			long x, long y,
			int level,
			long w, long h);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void openslide_close(int* osr);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr openslide_get_error(int* osr);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern byte** openslide_get_property_names(int* osr);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr openslide_get_property_value(int* osr, [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern byte** openslide_get_associated_image_names(int* osr);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void openslide_get_associated_image_dimensions(int* osr,
			[MarshalAs(UnmanagedType.LPStr)] string name, out long w, out long h);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void openslide_read_associated_image(int* osr, [MarshalAs(UnmanagedType.LPStr)] string name,
			void* dest);

		[DllImport(@"libopenslide-0.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr openslide_get_version();
	}
}