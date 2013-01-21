using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Kiss.Utils;
using Kiss.Web;

namespace Kiss.Web.Optimization
{
    /// <summary>
    /// Automates the creation of sprites and base64 inlining for CSS
    /// </summary>
    static class ImageOptimizations
    {
        private static readonly string[] _extensions = { "*.jpg", "*.gif", "*.png", "*.bmp", "*.jpeg" };
        private static readonly object _lockObj = new object();
        public const string TimestampFileName = "timeStamp.dat";
        public const string SettingsFileName = "sprite.config";

        /// <summary>
        /// Rebuilds the cache / dependancies for all subfolders below the specified directory
        /// </summary>
        /// <param name="path">The root directory for the cache rebuild</param>
        /// <param name="rebuildImages">Indicate whether the directories should be rebuilt as well</param>
        public static void AddCacheDependencies(string path, bool rebuildImages)
        {
            IHost host = ServiceLocator.Instance.Resolve<IHost>();

            foreach (ISite site in host.AllSites)
            {
                string themes_path = ServerUtil.MapPath(StringUtil.CombinUrl(site.VirtualPath, site.ThemeRoot));

                if (!Directory.Exists(themes_path))
                    continue;

                foreach (var theme in Directory.GetDirectories(themes_path, "*", SearchOption.TopDirectoryOnly))
                {
                    foreach (var sprites_path in Directory.GetDirectories(theme, "sprites", SearchOption.AllDirectories))
                    {
                        foreach (var dir in Directory.GetDirectories(sprites_path, "*", SearchOption.TopDirectoryOnly))
                        {
                            if (!File.Exists(Path.Combine(dir, SettingsFileName)))
                                continue;

                            if (rebuildImages)
                                ProcessDirectory(theme, dir, true);

                            InsertItemIntoCache(theme, dir);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when the cache dependancy of a subdirectory of the root image folder is modified, created, or removed
        /// </summary>
        private static void RebuildFromCacheHit(string key, object value, CacheItemRemovedReason reason)
        {
            var data = (KeyValuePair<string, string>)value;
            string cssPath = data.Key;
            string path = data.Value;

            switch (reason)
            {
                case CacheItemRemovedReason.DependencyChanged:
                    if (ProcessDirectory(cssPath, path, true))
                    {
                        // Add the current directory back into the cache
                        InsertItemIntoCache(cssPath, path);
                    }
                    break;
                // Cache items will only be manually removed if they have to be rebuilt due to changes in a folder that they inherit settings from
                case CacheItemRemovedReason.Removed:
                    if (ProcessDirectory(cssPath, path, false))
                    {
                        InsertItemIntoCache(cssPath, path);
                    }
                    break;

                case CacheItemRemovedReason.Expired:
                case CacheItemRemovedReason.Underused:
                    // Don't need to reprocess parameters, just re-insert the item into the cache
                    HttpRuntime.Cache.Insert(key, value, new CacheDependency(path), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, RebuildFromCacheHit);
                    break;

                default:
                    break;
            }
            return;
        }

        private static void InsertItemIntoCache(string cssPath, string path)
        {
            string key = Guid.NewGuid().ToString();
            var value = new KeyValuePair<string, string>(cssPath, path);
            HttpRuntime.Cache.Insert(key, value, new CacheDependency(new string[]{ path, GetCssFileName(cssPath, path)}), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, RebuildFromCacheHit);
        }

        /// <summary>
        /// Executes the image optimizer on a specific subdirectory of the root image folder (non-recursive)
        /// </summary>
        /// <param name="cssPath">The path to the directory of generated css file</param>
        /// <param name="path">The path to the directory to be rebuilt</param>
        /// <returns>False if the directory does not exist</returns>
        /// <param name="checkIfFilesWereModified">Indicate whether the directory should only be rebuilt if files were modified</param>
        private static bool ProcessDirectory(string cssPath, string path, bool checkIfFilesWereModified)
        {
            // Check if directory was deleted
            if (!Directory.Exists(path))
                return false;

            string cssFileName = GetCssFileName(cssPath, path);

            try
            {
                if (checkIfFilesWereModified && !HaveFilesBeenModified(path) && File.Exists(cssFileName))
                    return true;

                // Make a list of the disk locations of each image
                List<string> imageLocations = new List<string>();

                foreach (string extension in _extensions)
                {
                    imageLocations.AddRange(Directory.GetFiles(path, extension));
                }

                // Make sure to delete any existing sprites (or other images with the filename sprite###.imageExtension)
                imageLocations.RemoveAll(delegate(string p)
                {
                    if (IsOutputSprite(p))
                    {
                        File.Delete(p);
                        return true;
                    }
                    return false;
                });

                // Import settings from settings file
                ImageSettings settings = ReadSettings(path);
                if (settings == null)
                    return false;

                // Create pointer to the CSS output file
                lock (_lockObj)
                {
                    using (TextWriter cssOutput = new StreamWriter(cssFileName, false))
                    {
                        PerformOptimizations(cssPath, path, settings, cssOutput, imageLocations);
                    }
                }

                SaveFileModificationData(path);
                return true;
            }
            catch (Exception)
            {
                if (!Directory.Exists(path))
                {
                    return false;
                }
                throw;
            }
        }

        private static string GetCssFileName(string cssPath, string path)
        {
            return Path.Combine(cssPath, Path.GetFileName(path)) + ".css";
        }

        private static void SaveFileModificationData(string path)
        {
            using (TextWriter timeStamp = new StreamWriter(GetTimeStampFile(path)))
            {
                timeStamp.Write(GetCurrentTimeStampInfo(path));
            }
        }

        /// <summary>
        /// Reads the timestamps of all of the files within a directory, and outputs them in a single sorted string. Used to determine if changes have occured to a directory upon application start.
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <returns>A sorted string containing all filenames and last modified timestamps</returns>
        private static string GetCurrentTimeStampInfo(string path)
        {
            List<string> fileLocations = Directory.GetFiles(path).ToList();

            // Remove the timestamp file, since it can't be included in the comparison
            string timeStampFile = GetTimeStampFile(path);
            fileLocations.Remove(timeStampFile);
            fileLocations.Sort();

            StringBuilder timeStampBuilder = new StringBuilder();

            foreach (string file in fileLocations)
            {
                string name = Path.GetFileName(file);
                DateTime lastModificationTime = File.GetLastWriteTimeUtc(file);

                timeStampBuilder.Append(name).Append(lastModificationTime);
            }

            return timeStampBuilder.ToString();
        }

        private static string GetSavedTimeStampInfo(string path)
        {
            try
            {
                using (TextReader timeStamp = new StreamReader(GetTimeStampFile(path)))
                {
                    return timeStamp.ReadToEnd();
                }
            }
            // In the case of an exception, regenerate all sprites
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private static string GetTimeStampFile(string path)
        {
            return Path.Combine(path, TimestampFileName);
        }

        private static bool HaveFilesBeenModified(string path)
        {
            return GetCurrentTimeStampInfo(path) != GetSavedTimeStampInfo(path);
        }

        /// <summary>
        /// Checks if an image (passed by path or image name) is a sprite image or CSS file created by the framework
        /// </summary>
        /// <param name="path">The path or filename of the image in question</param>
        /// <returns>True if the image is a sprite, false if it is not</returns>
        public static bool IsOutputSprite(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path).TrimStart('.');
            List<string> imageExtensions = new List<string>(_extensions);

            return ((Regex.Match(name, @"^sprite[0-9]*$").Success && imageExtensions.Contains("*." + extension)) || extension == "css");
        }

        private static void PerformOptimizations(string cssPath, string path, ImageSettings settings, TextWriter cssOutput, List<string> imageLocations)
        {
            // Create a list containing each image (in Bitmap format), and calculate the total size (in pixels) of final image        
            int x = 0;
            int y = 0;
            int imageIndex = 0;
            long size = 0;
            int spriteNumber = 0;
            List<Bitmap> images = new List<Bitmap>();

            string relativeUrl = path.Replace(cssPath, string.Empty).Replace(@"\", "/").TrimStart('/');
            if (!string.IsNullOrEmpty(relativeUrl))
                relativeUrl += "/";

            try
            {
                foreach (string imagePath in imageLocations)
                {
                    // If the image is growing above the specified max file size, make the sprite with the existing images
                    // and add the new image to the next sprite list

                    if ((imageIndex > 0) && IsSpriteOversized(settings.MaxSize, size, imagePath))
                    {
                        GenerateSprite(relativeUrl, path, settings, x, y, spriteNumber, images, cssOutput);

                        // Clear the existing images
                        foreach (Bitmap image in images)
                        {
                            image.Dispose();
                        }

                        // Reset variables to initial values, and increment the spriteNumber
                        images.Clear();
                        x = 0;
                        y = 0;
                        imageIndex = 0;
                        size = 0;
                        spriteNumber++;
                    }

                    // Add the current image to the list of images that are to be processed
                    images.Add(new Bitmap(imagePath));

                    // Use the image tag to store its name
                    images[imageIndex].Tag = MakeCssClassName(imagePath);

                    // Find the total pixel size of the sprite based on the tiling direction
                    if (settings.TileInYAxis)
                    {
                        y += images[imageIndex].Height;
                        if (x < images[imageIndex].Width)
                        {
                            x = images[imageIndex].Width;
                        }
                    }
                    else
                    {
                        x += images[imageIndex].Width;
                        if (y < images[imageIndex].Height)
                        {
                            y = images[imageIndex].Height;
                        }
                    }

                    // Update the filesize size of the bitmap list
                    size += (new FileInfo(imagePath)).Length;

                    imageIndex++;
                }

                // Merge the final list of bitmaps into a sprite
                if (imageIndex != 0)
                    GenerateSprite(relativeUrl, path, settings, x, y, spriteNumber, images, cssOutput);
            }
            finally // Close the CSS file and clear the images list
            {
                foreach (Bitmap image in images)
                {
                    image.Dispose();
                }
                images.Clear();
            }
        }

        private static bool IsSpriteOversized(int maxSize, long spriteSize, string imagePath)
        {
            // Estimate the size of the sprite after adding the current image
            long nextSize = spriteSize + new FileInfo(imagePath).Length;

            // Determine of the size is too large
            return nextSize > (1024 * maxSize);
        }

        /// <summary>
        /// Make the appropriate CSS ID name for the sprite to be used
        /// </summary>
        /// <param name="pathToImage">The path to the image</param>
        /// <param name="pathToSpriteFolder">The path to the folder used to store sprites, used if the path to the image was not relative to the sprites folder</param>
        /// <returns>The CSS class used to reference the optimized image</returns>
        private static string MakeCssClassName(string pathToImage)
        {
            string dirname = Path.GetFileName(Path.GetDirectoryName(pathToImage));
            string filename = Path.GetFileNameWithoutExtension(pathToImage);

            return string.Concat(dirname, "-", filename);
        }

        private static void GenerateSprite(string relativeUrl, string path, ImageSettings settings, int x, int y, int spriteNumber, List<Bitmap> images, TextWriter cssOutput)
        {
            // Create a drawing surface and add the images to it
            using (Bitmap sprite = new Bitmap(x, y))
            {
                using (Graphics drawingSurface = Graphics.FromImage(sprite))
                {

                    // Set the background to the specs from the settings file
                    drawingSurface.Clear(settings.BackgroundColor);

                    // Make the final sprite and save it
                    int xOffset = 0;
                    int yOffset = 0;
                    foreach (Bitmap image in images)
                    {
                        drawingSurface.DrawImage(image, new Rectangle(xOffset, yOffset, image.Width, image.Height));

                        // Add the CSS data
                        GenerateCss(relativeUrl, xOffset, yOffset, spriteNumber, settings.Format, image, cssOutput);

                        if (settings.TileInYAxis)
                        {
                            yOffset += image.Height;
                        }
                        else
                        {
                            xOffset += image.Width;
                        }
                    }

                    // Set the encoder parameters and make the image
                    try
                    {
                        using (EncoderParameters spriteEncoderParameters = new EncoderParameters(1))
                        {
                            spriteEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, settings.Quality);

                            // Attempt to save the image to disk with the specified encoder
                            sprite.Save(Path.Combine(path, "sprite" + spriteNumber + "." + settings.Format), GetEncoderInfo(settings.Format), spriteEncoderParameters);
                        }
                    }
                    catch (Exception)
                    {
                        // If errors occur, get the CLI to auto-choose an encoder. Unfortunately this means that the quality settings will be not used.
                        try
                        {
                            sprite.Save(Path.Combine(path, "sprite" + spriteNumber + "." + settings.Format));
                        }
                        catch (Exception)
                        {
                            // If errors occur again, try to save as a png
                            sprite.Save(Path.Combine(path, "sprite" + spriteNumber + ".png"));
                        }
                    }
                }
            }
        }

        private static void GenerateCss(string relativeUrl, int xOffset, int yOffset, int spriteNumber, string fileExtension, Bitmap image, TextWriter cssOutput)
        {
            cssOutput.WriteLine("." + (string)image.Tag);
            cssOutput.WriteLine("{");
            cssOutput.WriteLine("    width:" + image.Width + "px;");
            cssOutput.WriteLine("    height:" + image.Height + "px;");

            cssOutput.WriteLine("    background-image:url({0}sprite{1}.{2});", relativeUrl, spriteNumber, fileExtension);
            cssOutput.WriteLine("    background-repeat:no-repeat;");
            cssOutput.WriteLine("    background-position:-" + xOffset + "px -" + yOffset + "px;");

            cssOutput.WriteLine("}");
        }

        private static string ConvertImageToBase64(Bitmap image, ImageFormat format)
        {
            string base64;
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, format);
                base64 = Convert.ToBase64String(memory.ToArray());
            }
            return base64;
        }

        private static ImageFormat GetImageFormat(string fileExtension)
        {
            switch (fileExtension.ToUpper())
            {
                case "JPG":
                case "JPEG":
                    return ImageFormat.Jpeg;

                case "GIF":
                    return ImageFormat.Gif;

                case "PNG":
                    return ImageFormat.Png;

                case "BMP":
                    return ImageFormat.Bmp;

                default:
                    return ImageFormat.Png;
            }
        }

        private static ImageSettings ReadSettings(string path)
        {
            ImageSettings settings = new ImageSettings();
            XmlTextReader settingsData;

            // Open the settings file. If it cannot be opened, or one cannot be found, use defaults
            try
            {
                using (settingsData = new XmlTextReader(Path.Combine(path, SettingsFileName)))
                {
                    while (settingsData.Read())
                    {
                        if (settingsData.NodeType == XmlNodeType.Element)
                        {
                            string nodeName = settingsData.Name;

                            if (nodeName.Equals("FileFormat", StringComparison.OrdinalIgnoreCase))
                            {
                                settings.Format = settingsData.ReadElementContentAsString().Trim('.');
                            }
                            else if (nodeName.Equals("Quality", StringComparison.OrdinalIgnoreCase))
                            {
                                settings.Quality = settingsData.ReadElementContentAsInt();
                            }
                            else if (nodeName.Equals("MaxSize", StringComparison.OrdinalIgnoreCase))
                            {
                                settings.MaxSize = settingsData.ReadElementContentAsInt();
                            }
                            else if (nodeName.Equals("BackgroundColor", StringComparison.OrdinalIgnoreCase))
                            {
                                string output = settingsData.ReadElementContentAsString();
                                int temp = Int32.Parse(output, System.Globalization.NumberStyles.HexNumber);
                                settings.BackgroundColor = Color.FromArgb(temp);
                            }
                            else if (nodeName.Equals("TileInYAxis", StringComparison.OrdinalIgnoreCase))
                            {
                                settings.TileInYAxis = settingsData.ReadElementContentAsBoolean();
                            }
                        }
                    }
                }
                return settings;

            }
            // If any other exceptions occur, use the default values
            catch (Exception)
            {
                return null;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string format)
        {
            // Find the appropriate codec for the specified file extension
            if (format == "jpg")
                format = "jpeg";
            format = "image/" + format.ToLower();
            // Get a list of all the available encoders
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            // Search the list for the proper encoder
            foreach (ImageCodecInfo encoder in encoders)
            {
                if (encoder.MimeType == format)
                    return encoder;
            }

            // If a format cannot be found, throw an exception
            throw new FormatException("Encoder not found! The CLI will attempt to automatically choose an encoder, however image quality settings will be ignored");
        }
    }
}