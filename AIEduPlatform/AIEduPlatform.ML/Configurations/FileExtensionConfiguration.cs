using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Configurations
{
    public enum SupportedImageFormats { png, jpg, jpeg, gif, bmp, webp, tiff, tif }
    public enum SupportedVideoFormats { mp4, avi, mov, mkv, webm, flv, wmv, m4v }
    public enum SupportedDocumentFormats { pdf }
    public enum SupportedAudioFormats { wav, mp3 }
    
    public static class FileExtensionConfiguration
    {
        private static readonly HashSet<string> _supportedImageExtensions;
        private static readonly HashSet<string> _supportedVideoExtensions;
        private static readonly HashSet<string> _supportedDocumentExtensions;
        private static readonly HashSet<string> _supportedAudioExtensions;
        private static readonly HashSet<string> _allSupportedExtensions;
        private static readonly Dictionary<string, MaterialType> _extensionToMaterialTypeMap;

        static FileExtensionConfiguration()
        {
            // Build extension sets from enums
            _supportedImageExtensions = BuildExtensionSet<SupportedImageFormats>();
            _supportedVideoExtensions = BuildExtensionSet<SupportedVideoFormats>();
            _supportedDocumentExtensions = BuildExtensionSet<SupportedDocumentFormats>();
            _supportedAudioExtensions = BuildExtensionSet<SupportedAudioFormats>();

            // Combine all supported extensions
            _allSupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _allSupportedExtensions.UnionWith(_supportedImageExtensions);
            _allSupportedExtensions.UnionWith(_supportedVideoExtensions);
            _allSupportedExtensions.UnionWith(_supportedDocumentExtensions);
            _allSupportedExtensions.UnionWith(_supportedAudioExtensions);

            // Build the mapping dictionary
            _extensionToMaterialTypeMap = new Dictionary<string, MaterialType>(StringComparer.OrdinalIgnoreCase);

            foreach (var ext in _supportedImageExtensions)
                _extensionToMaterialTypeMap[ext] = MaterialType.Image;

            foreach (var ext in _supportedVideoExtensions)
                _extensionToMaterialTypeMap[ext] = MaterialType.Video;

            foreach (var ext in _supportedDocumentExtensions)
                _extensionToMaterialTypeMap[ext] = MaterialType.Document;

            foreach (var ext in _supportedAudioExtensions)
                _extensionToMaterialTypeMap[ext] = MaterialType.Audio;
        }

        /// <summary>
        /// Gets all supported file extensions (with leading dots)
        /// </summary>
        public static IReadOnlyCollection<string> AllSupportedExtensions =>
            _allSupportedExtensions.Select(e => $".{e}").ToList();

        /// <summary>
        /// Gets all supported image extensions (with leading dots)
        /// </summary>
        public static IReadOnlyCollection<string> SupportedImageExtensions =>
            _supportedImageExtensions.Select(e => $".{e}").ToList();

        /// <summary>
        /// Gets all supported video extensions (with leading dots)
        /// </summary>
        public static IReadOnlyCollection<string> SupportedVideoExtensions =>
            _supportedVideoExtensions.Select(e => $".{e}").ToList();

        /// <summary>
        /// Gets all supported document extensions (with leading dots)
        /// </summary>
        public static IReadOnlyCollection<string> SupportedDocumentExtensions =>
            _supportedDocumentExtensions.Select(e => $".{e}").ToList();

        /// <summary>
        /// Gets all supported audio extensions (with leading dots)
        /// </summary>
        public static IReadOnlyCollection<string> SupportedAudioExtensions =>
            _supportedAudioExtensions.Select(e => $".{e}").ToList();

        /// <summary>
        /// Determines the MaterialType based on file extension
        /// </summary>
        /// <param name="fileName">The file name or extension</param>
        /// <param name="defaultType">Default type if extension is not recognized</param>
        /// <returns>The corresponding MaterialType</returns>
        public static MaterialType GetMaterialType(string fileName, MaterialType defaultType = MaterialType.Document)
        {
            var extension = Path.GetExtension(fileName).TrimStart('.');
            return _extensionToMaterialTypeMap.GetValueOrDefault(extension, defaultType);
        }

        /// <summary>
        /// Checks if a file extension is supported
        /// </summary>
        /// <param name="fileName">The file name or extension</param>
        /// <returns>True if the extension is supported</returns>
        public static bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName).TrimStart('.');
            return _allSupportedExtensions.Contains(extension);
        }

        /// <summary>
        /// Gets a user-friendly string of all supported extensions
        /// </summary>
        public static string GetSupportedExtensionsString() =>
            string.Join(", ", AllSupportedExtensions);

        private static HashSet<string> BuildExtensionSet<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetNames(typeof(TEnum))
                .Select(name => name.ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
