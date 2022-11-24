namespace TextureGenerator;

public class Configuration
{
    /// <summary>
    ///     Path to directory with pictures of the tile faces.
    ///     The tile faces will be randomly selected from those pictures during the rendering.
    ///     The relative path will be based on configuration file location.
    ///     This parameter is required.
    /// </summary>
    public string? TileFacesDirectory { get; set; }

    /// <summary>
    ///     Tiles physical width in mm.
    ///     Default is 1200.
    /// </summary>
    public int TileWidth { get; set; } = 1200;

    /// <summary>
    ///     Tiles physical length in mm.
    ///     Default is 200.
    /// </summary>
    public int TileLength { get; set; } = 200;

    /// <summary>
    ///     Tiles minimum offset (in percent of the tiles length) when starting new row.
    ///     Default is 30%.
    /// </summary>
    public float OffsetMin { get; set; } = 30;

    /// <summary>
    ///     Tiles minimum offset (in percent of the tiles length) when starting new row.
    ///     Default is 30%.
    /// </summary>
    public float OffsetMax { get; set; } = 30;

    /// <summary>
    ///     Physical gap width between tiles (mortar line) in mm.
    ///     Default is 4.
    /// </summary>
    public int GapWidth { get; set; }

    /// <summary>
    ///     Color of the gap (mortar) between tiles.
    ///     Default is #3e3e3e
    /// </summary>
    public string GapColor { get; set; } = "#3e3e3e";

    /// <summary>
    ///     If true then all tiles faces allowed to be flipped during rendering to randomize the appearance.
    ///     Default is true.
    /// </summary>
    public bool AllowFaceFlip { get; set; } = true;

    /// <summary>
    ///     DPI of the output image.
    ///     Default is 96.
    /// </summary>
    public int OutputDPI { get; set; } = 96;

    /// <summary>
    ///     Desired output image width in mm.
    ///     The actual output image width will be calculated that it will be no less than this value
    ///     but contains whole number of tiles in the first row.
    ///     Default is 7000.
    /// </summary>
    public int OutputWidth { get; set; } = 7000;

    /// <summary>
    ///     Desired output image height in mm.
    ///     The actual output image height will be calculated that it will be no less than this value
    ///     but contains whole number tiles horizontally.
    ///     Default is 4000.
    /// </summary>
    public int OutputHeight { get; set; } = 4000;

    /// <summary>
    ///     Output filename.
    ///     The actual filename will be prefixed with the resulting image sizes. Also the bumpmap
    ///     image will be places with the same name but with suffix "_bump".
    ///     Only ".jpg" files currently supported.
    ///     The relative path will be based on configuration file location.
    ///     This parameter is required.
    /// </summary>
    public string? OutputFilename { get; set; }
}