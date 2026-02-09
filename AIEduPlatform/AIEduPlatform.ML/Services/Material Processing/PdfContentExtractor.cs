using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.Interfaces.Services;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AIEduPlatform.ML.MaterialProcessing
{
    public class PdfContentExtractor : IDisposable, IPdfContentExtractor
    {
        private PdfDocument _pdfDocument;
        private string _pdfPath;
        private IVisionService _visionService;
        private int _globalSectionCounter = 0;

        public PdfContentExtractor(string pdfPath, IVisionService visionService = null)
        {
            if (!File.Exists(pdfPath))
            {
                throw new FileNotFoundException($"PDF file not found: {pdfPath}");
            }

            _pdfPath = pdfPath;
            _pdfDocument = PdfDocument.Open(pdfPath);
            _visionService = visionService;
        }

        public int PageCount => _pdfDocument.NumberOfPages;
        public string FilePath => _pdfPath;
        public string FileName => Path.GetFileName(_pdfPath);

        /// <summary>
        /// Resets the global section counter (call when starting a new document)
        /// </summary>
        public void ResetSectionCounter()
        {
            _globalSectionCounter = 0;
        }

        /// <summary>
        /// Extracts all pages with clean, structured content
        /// </summary>
        public async Task<List<PageContent>> ExtractAllPagesAsync(
            CancellationToken cancellationToken = default)
        {
            var pages = new List<PageContent>();

            for (int i = 1; i <= PageCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pageContent = await ExtractPageWithStructureAsync(i, cancellationToken);
                pages.Add(pageContent);
            }

            return pages;
        }

        /// <summary>
        /// Extracts a single page with section detection and clean formatting
        /// </summary>
        public async Task<PageContent> ExtractPageWithStructureAsync(
            int pageNumber,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidatePageNumber(pageNumber);

            var page = _pdfDocument.GetPage(pageNumber);
            var words = page.GetWords().ToList();

            // Detect sections/headings
            var sections = DetectSections(page, words);

            // Build clean content
            var cleanContent = await BuildCleanContentAsync(page, words, pageNumber, cancellationToken);

            // Determine primary section for this page
            var primarySection = sections.FirstOrDefault()?.Title ?? "Content";

            return new PageContent
            {
                PageNumber = pageNumber,
                Content = cleanContent,
                Sections = sections,
                PrimarySection = primarySection,
                SourceFile = FileName,
                WordCount = CountWords(cleanContent)
            };
        }

        /// <summary>
        /// Builds clean, properly formatted content from page
        /// </summary>
        private async Task<string> BuildCleanContentAsync(
            Page page,
            List<Word> words,
            int pageNumber,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = new StringBuilder();
            var elements = new List<ContentElement>();

            // Add text elements
            foreach (var word in words)
            {
                elements.Add(new ContentElement
                {
                    Type = ContentType.Text,
                    Content = word.Text,
                    Position = new Position
                    {
                        X = word.BoundingBox.Left,
                        Y = word.BoundingBox.Bottom,
                        Width = word.BoundingBox.Width,
                        Height = word.BoundingBox.Height
                    },
                    FontSize = word.BoundingBox.Height
                });
            }

            // Add images if vision service available
            if (_visionService != null)
            {
                var images = page.GetImages().ToList();
                int imageIndex = 0;

                foreach (var image in images)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var imageBytes = image.RawBytes.ToArray();
                    using var imageStream = new MemoryStream(imageBytes);
                    var imgInterpretation = await _visionService.ExtractInfoFromImageAsync(
                        imageStream,
                        cancellationToken);

                    if (!string.IsNullOrWhiteSpace(imgInterpretation.DetailedCaption))
                    {
                        elements.Add(new ContentElement
                        {
                            Type = ContentType.Image,
                            Content = imgInterpretation.DetailedCaption,
                            ImageIndex = imageIndex++,
                            Position = new Position
                            {
                                X = image.Bounds.Left,
                                Y = image.Bounds.Bottom,
                                Width = image.Bounds.Width,
                                Height = image.Bounds.Height
                            }
                        });
                    }
                }
            }

            // Sort by reading order
            var sortedElements = elements
                .OrderByDescending(e => e.Position.Y)
                .ThenBy(e => e.Position.X)
                .ToList();

            // Build text with proper spacing
            var currentLine = new StringBuilder();
            double? lastY = null;
            double lineHeightThreshold = 5;

            foreach (var element in sortedElements)
            {
                if (element.Type == ContentType.Image)
                {
                    // Add image content as a separate paragraph
                    if (currentLine.Length > 0)
                    {
                        content.AppendLine(currentLine.ToString().Trim());
                        currentLine.Clear();
                    }
                    content.AppendLine();
                    content.AppendLine(element.Content.Trim());
                    content.AppendLine();
                    lastY = null;
                }
                else // Text
                {
                    // Check if new line
                    if (lastY.HasValue && Math.Abs(element.Position.Y - lastY.Value) > lineHeightThreshold)
                    {
                        // New line detected
                        if (currentLine.Length > 0)
                        {
                            content.AppendLine(currentLine.ToString().Trim());
                            currentLine.Clear();
                        }
                    }

                    currentLine.Append(element.Content);
                    currentLine.Append(" ");
                    lastY = element.Position.Y;
                }
            }

            // Add remaining line
            if (currentLine.Length > 0)
            {
                content.AppendLine(currentLine.ToString().Trim());
            }

            return CleanText(content.ToString());
        }

        /// <summary>
        /// Detects sections and headings in the page with global numbering
        /// </summary>
        private List<PageSection> DetectSections(Page page, List<Word> allWords)
        {
            var sections = new List<PageSection>();

            // Group words into lines
            var lines = GroupWordsIntoLines(allWords);

            // Estimate normal font size
            var avgFontSize = allWords.Any() ? allWords.Average(w => w.BoundingBox.Height) : 12;
            var headingThreshold = avgFontSize * 1.2; // 20% larger than average

            foreach (var line in lines)
            {
                var lineText = string.Join(" ", line.Select(w => w.Text)).Trim();
                var lineFontSize = line.Average(w => w.BoundingBox.Height);

                // Check if this is a heading
                if (IsHeading(lineText, lineFontSize, headingThreshold))
                {
                    var level = DetermineHeadingLevel(lineFontSize, avgFontSize);
                    var cleanTitle = CleanSectionTitle(lineText);

                    // Only increment for main headings (level 1-2)
                    if (level <= 2)
                    {
                        _globalSectionCounter++;
                    }

                    sections.Add(new PageSection
                    {
                        Title = $"{_globalSectionCounter}. {cleanTitle}",
                        Level = level,
                        StartY = line.First().BoundingBox.Bottom
                    });

                    // Only take first main heading per page
                    break;
                }
            }

            // If no sections found, try to extract from content
            if (sections.Count == 0)
            {
                var contentBasedSection = ExtractSectionFromContent(allWords);
                sections.Add(contentBasedSection ?? new PageSection
                {
                    Title = "Content",
                    Level = 1,
                    StartY = page.Height
                });
            }

            return sections;
        }

        /// <summary>
        /// Attempts to extract section name from page content
        /// </summary>
        private PageSection ExtractSectionFromContent(List<Word> words)
        {
            // Look for potential section titles in first few lines
            var lines = GroupWordsIntoLines(words).Take(3).ToList();

            foreach (var line in lines)
            {
                var lineText = string.Join(" ", line.Select(w => w.Text)).Trim();

                // Skip very long lines (likely paragraphs)
                if (lineText.Length > 100)
                    continue;

                // Look for title-like patterns
                if (Regex.IsMatch(lineText, @"^[A-Z][a-zA-Z\s]{3,50}$"))
                {
                    _globalSectionCounter++;
                    return new PageSection
                    {
                        Title = $"{_globalSectionCounter}. {CleanSectionTitle(lineText)}",
                        Level = 2,
                        StartY = line.First().BoundingBox.Bottom
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Groups words into lines based on Y position
        /// </summary>
        private List<List<Word>> GroupWordsIntoLines(List<Word> words)
        {
            var lines = new List<List<Word>>();
            var currentLine = new List<Word>();

            var sortedWords = words
                .OrderByDescending(w => w.BoundingBox.Bottom)
                .ThenBy(w => w.BoundingBox.Left)
                .ToList();

            double? lastY = null;
            double lineThreshold = 5;

            foreach (var word in sortedWords)
            {
                if (lastY.HasValue && Math.Abs(word.BoundingBox.Bottom - lastY.Value) > lineThreshold)
                {
                    // New line
                    if (currentLine.Count > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = new List<Word>();
                    }
                }

                currentLine.Add(word);
                lastY = word.BoundingBox.Bottom;
            }

            if (currentLine.Count > 0)
            {
                lines.Add(currentLine);
            }

            return lines;
        }

        /// <summary>
        /// Determines if a line is a heading
        /// </summary>
        private bool IsHeading(string text, double fontSize, double threshold)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Font size check
            if (fontSize > threshold)
                return true;

            // Pattern matching for common heading formats
            var headingPatterns = new[]
            {
                @"^\d+\.?\s+[A-Z]",                    // "1. Introduction"
                @"^Chapter\s+\d+",                     // "Chapter 1"
                @"^Section\s+\d+",                     // "Section 1"
                @"^\d+\.\d+\s+[A-Z]",                 // "1.1 Overview"
                @"^[A-Z][A-Z\s]{5,}$"                 // "ALL CAPS HEADING"
            };

            foreach (var pattern in headingPatterns)
            {
                if (Regex.IsMatch(text, pattern))
                    return true;
            }

            // All caps check (for short headings)
            if (text.Length > 5 && text.Length < 50)
            {
                var upperCount = text.Count(char.IsUpper);
                var letterCount = text.Count(char.IsLetter);

                if (letterCount > 0 && (upperCount / (double)letterCount) > 0.7)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines heading level based on font size
        /// </summary>
        private int DetermineHeadingLevel(double fontSize, double avgFontSize)
        {
            var ratio = fontSize / avgFontSize;

            if (ratio >= 1.5) return 1;
            if (ratio >= 1.3) return 2;
            if (ratio >= 1.2) return 3;
            return 4;
        }

        /// <summary>
        /// Enhanced section title cleaning - removes Unicode bullets and cleans formatting
        /// </summary>
        private string CleanSectionTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // Remove Unicode bullet points
            title = title.Replace("\uF0B7", "").Replace("•", "").Replace("\u2022", "");

            // Remove common prefixes
            title = Regex.Replace(title, @"^(Chapter|Section|Part)\s+\d+:?\s*", "", RegexOptions.IgnoreCase);

            // Remove existing numbering
            title = Regex.Replace(title, @"^\d+\.?\d*\s+", "");

            // Clean up whitespace
            title = Regex.Replace(title, @"\s+", " ").Trim();

            // Remove leading special characters
            title = title.TrimStart('-', '•', '*', '.', ':', ';');

            // Capitalize first letter if lowercase
            if (title.Length > 0 && char.IsLower(title[0]))
            {
                title = char.ToUpper(title[0]) + title.Substring(1);
            }

            return title.Trim();
        }

        /// <summary>
        /// Cleans extracted text
        /// </summary>
        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Fix bullet points
            text = text.Replace("\uF0B7", "•");
            text = text.Replace("\u2022", "•");

            // Remove page markers
            text = Regex.Replace(text, @"\[PAGE \d+\]", "");

            // Normalize whitespace
            text = Regex.Replace(text, @"[ \t]+", " ");

            // Fix multiple blank lines (keep max 2)
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            // Remove leading/trailing whitespace per line
            var lines = text.Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line));

            return string.Join("\n", lines).Trim();
        }

        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private void ValidatePageNumber(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > PageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber),
                    $"Page number must be between 1 and {PageCount}");
            }
        }

        public void Dispose()
        {
            _pdfDocument?.Dispose();
        }
    }
}