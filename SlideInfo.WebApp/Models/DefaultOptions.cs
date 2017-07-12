using System.Drawing.Imaging;
using System.Linq;

namespace SlideInfo.WebApp.Models
{
    public class DefaultOptions
    {
	    public static string DEEPZOOM_FORMAT = "jpeg";
	    public static int DEEPZOOM_TILE_SIZE = 254;
	    public static int DEEPZOOM_OVERLAP = 1;
	    public static bool DEEPZOOM_LIMIT_BOUNDS = true;
	    public static int DEEPZOOM_TILE_QUALITY = 75;
	    public static string SLIDE_NAME = "slide";

	    public static EncoderParameters GetQualityEncoderParameter()
	    {
		    var qualityEncoder = Encoder.Quality;
		    var qualityParameter =
			    new EncoderParameters(1)
			    {
				    Param = { [0] = new EncoderParameter(qualityEncoder, DEEPZOOM_TILE_QUALITY) }
			    };
		    return qualityParameter;
	    }

	    public static ImageCodecInfo GetEncoder(ImageFormat format)
	    {
		    var codecs = ImageCodecInfo.GetImageDecoders();
		    return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
	    }
	}
}
