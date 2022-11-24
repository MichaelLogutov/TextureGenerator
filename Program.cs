using System.Text.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using SkiaSharp;
using TextureGenerator;

var logger = InitializeLogging();
var configuration = LoadConfiguration();

var tile_faces = LoadTileFaces(configuration);

var pixels_in_one_mm = configuration.OutputDPI / 25.4f;
var gap_width = configuration.GapWidth * pixels_in_one_mm;
var tile_width = configuration.TileWidth * pixels_in_one_mm;
var tile_length = configuration.TileLength * pixels_in_one_mm;

var output_x_tiles = Math.Ceiling((float) configuration.OutputWidth / (configuration.TileLength + configuration.GapWidth));
var output_y_tiles = Math.Ceiling((float) configuration.OutputHeight / (configuration.TileWidth + configuration.GapWidth));
var output_width = (int) Math.Round(output_x_tiles * (tile_length + gap_width));
var output_height = (int) Math.Round(output_y_tiles * (tile_width + gap_width));
var output_width_mm = (int) Math.Round(output_width / pixels_in_one_mm);
var output_height_mm = (int) Math.Round(output_height / pixels_in_one_mm);

logger.Info($"Maximum tiles: {output_x_tiles}x{output_y_tiles}");
logger.Info($"Result image size: {output_width}x{output_width} px ({output_width_mm}x{output_height_mm} mm)");

using var output_bitmap = new SKBitmap(output_width, output_height);
using var output_canvas = new SKCanvas(output_bitmap);
using var output_bumpmap_bitmap = new SKBitmap(output_width, output_height);
using var output_bumpmap_canvas = new SKCanvas(output_bumpmap_bitmap);

logger.Info("Rendering ...");

output_canvas.DrawColor(SKColor.Parse(configuration.GapColor));
output_bumpmap_canvas.DrawColor(SKColors.Black);

var y = 0.0f;
var start_x = 0.0f;

var bumpmap_paint = new SKPaint { Color = SKColors.White };

while (y < output_height)
{
    var x = start_x;
    while (x < output_width)
    {
        var rect = SKRect.Create(x, y, tile_length, tile_width);
        var face = tile_faces[Random.Shared.Next(tile_faces.Count)];
        output_canvas.DrawBitmap(face, rect);
        output_bumpmap_canvas.DrawRect(rect, bumpmap_paint);

        x += tile_length + gap_width;
    }

    y += tile_width + gap_width;

    var offset = configuration.OffsetMin;
    if (configuration.OffsetMax - configuration.OffsetMin > 0.01)
        offset = configuration.OffsetMin + (float) Random.Shared.NextDouble() * (configuration.OffsetMax - configuration.OffsetMin);

    start_x += tile_length * offset / 100.0f;
    if (start_x > 0)
        start_x -= tile_length;
}

if (string.IsNullOrWhiteSpace(configuration.OutputFilename))
    throw new InvalidOperationException($"Configuration field {nameof(configuration.OutputFilename)} is required.");

var output_filename = AddFilenameSuffix(Path.GetFullPath(configuration.OutputFilename),
    $"_{output_width}x{output_height}px_{output_width_mm}x{output_height_mm}mm");
logger.Info("Saving output to {0}", output_filename);
using (var output_file = new SKFileWStream(output_filename))
    output_bitmap.Encode(output_file, SKEncodedImageFormat.Jpeg, 95);

var output_bumpmap_filename = AddFilenameSuffix(output_filename, "_bump");
logger.Info("Saving bumpmap to {0}", output_bumpmap_filename);
using (var output_file = new SKFileWStream(output_bumpmap_filename))
    output_bumpmap_bitmap.Encode(output_file, SKEncodedImageFormat.Jpeg, 95);

logger.Info("All done");

List<SKBitmap> LoadTileFaces(Configuration config)
{
    if (string.IsNullOrWhiteSpace(config.TileFacesDirectory))
        throw new InvalidOperationException($"Configuration field {nameof(config.TileFacesDirectory)} is required.");

    var tileFaces = new List<SKBitmap>();
    foreach (var filename in Directory.EnumerateFiles(config.TileFacesDirectory))
    {
        using var codec = SKCodec.Create(filename);
        var face = SKBitmap.Decode(codec);

        if (face.Width < face.Height)
        {
            var rotated = new SKBitmap(face.Height, face.Width);

            using var canvas = new SKCanvas(rotated);
            canvas.Translate(face.Height, 0);
            canvas.RotateDegrees(90);
            canvas.DrawBitmap(face, 0, 0);

            face.Dispose();
            face = rotated;
        }

        tileFaces.Add(face);

        if (config.AllowFaceFlip)
        {
            var flipped = new SKBitmap(face.Width, face.Height);

            using var canvas = new SKCanvas(flipped);
            canvas.Translate(face.Width, face.Height);
            canvas.RotateDegrees(180);
            canvas.DrawBitmap(face, 0, 0);

            tileFaces.Add(flipped);
        }
    }

    return tileFaces;
}

string AddFilenameSuffix(string path, string suffix)
{
    return Path.Combine(Path.GetDirectoryName(path) ?? string.Empty,
        Path.GetFileNameWithoutExtension(path) + suffix + Path.GetExtension(path));
}

Logger InitializeLogging()
{
    LogManager.Configuration = new LoggingConfiguration
    {
        LoggingRules =
        {
            new LoggingRule("*", LogLevel.Info, new ConsoleTarget
            {
                Layout = @"${date:format=dd.MM.yyyy HH\:mm\:ss} ${message}${onexception:${newline}${exception:format=tostring}${newline}${exception:format=data}}"
            })
        }
    };
    LogManager.ResumeLogging();

    return LogManager.GetLogger("Application");
}

Configuration LoadConfiguration()
{
    var config_filename = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
    if (string.IsNullOrWhiteSpace(config_filename))
        throw new InvalidOperationException("Configuration filename must be provided");

    config_filename = Path.GetFullPath(config_filename);

    using var file = File.OpenRead(config_filename);
    var result = JsonSerializer.Deserialize<Configuration>(file, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (result is null)
        throw new InvalidOperationException("Unable to parse configuration file");

    logger.Info("Using configuration:\n{0}", JsonSerializer.Serialize(result, new JsonSerializerOptions
    {
        WriteIndented = true
    }));

    var config_directory = Path.GetDirectoryName(config_filename);
    if (!string.IsNullOrWhiteSpace(config_directory))
        Directory.SetCurrentDirectory(config_directory);

    return result;
}