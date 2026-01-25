using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Word
{
    public string WordText { get; set; }
    public double Start { get; set; }
    public double End { get; set; }
}

public class Verse
{
    public int Number { get; set; }
    public string Html { get; set; }
    public double AudioStart { get; set; }
    public double AudioEnd { get; set; }
}

public class VerseTimingCalculator
{
    /// <summary>
    /// Calculates the precise audio start and end times for each verse based on transcription data.
    /// </summary>
    /// <param name="verses">Array of verses with HTML text and approximate timings.</param>
    /// <param name="transcription">Array of words with their exact timings.</param>
    /// <returns>Array of tuples containing (audioStart, audioEnd) for each verse.</returns>
    public (double, double)[] GetVerseTimings(Verse[] verses, Word[] transcription)
    {
        // Extract words from transcription
        string[] transWords = transcription.Select(w => w.WordText).ToArray();
        
        // Start after prelude (e.g., "Psalm 91."), assuming it’s the first 3 words
        int transIdx = 3; // Adjust this based on actual prelude length if known
        
        List<(double, double)> results = new List<(double, double)>();

        foreach (var verse in verses)
        {
            // Split verse HTML into words, keeping punctuation with words
            string[] verseWords = SplitHtmlToWords(verse.Html);
            
            // Find the best starting index in transcription with maximum matches
            int bestStartIdx = -1;
            int maxMatches = -1;
            
            // Search from current transIdx onwards
            for (int i = transIdx; i <= transWords.Length - verseWords.Length; i++)
            {
                int matches = 0;
                for (int j = 0; j < verseWords.Length; j++)
                {
                    if (transWords[i + j].Equals(verseWords[j], StringComparison.OrdinalIgnoreCase))
                    {
                        matches++;
                    }
                }
                if (matches > maxMatches)
                {
                    maxMatches = matches;
                    bestStartIdx = i;
                }
            }

            if (bestStartIdx != -1)
            {
                // Assuming the number of words matches, calculate end index
                int endIdx = bestStartIdx + verseWords.Length - 1;
                
                // Ensure indices are within bounds
                if (endIdx < transcription.Length)
                {
                    double audioStart = transcription[bestStartIdx].Start;
                    double audioEnd = transcription[endIdx].End;
                    results.Add((audioStart, audioEnd));
                    transIdx = endIdx + 1; // Move to next position for the next verse
                }
                else
                {
                    // Handle case where transcription is too short
                    results.Add((0, 0));
                }
            }
            else
            {
                // No match found
                results.Add((0, 0));
            }
        }

        return results.ToArray();
    }

    /// <summary>
    /// Splits HTML text into words, keeping punctuation attached to words.
    /// </summary>
    /// <param name="html">The HTML text of the verse.</param>
    /// <returns>Array of words.</returns>
    private string[] SplitHtmlToWords(string html)
    {
        // Remove HTML tags if present (basic stripping)
        string text = Regex.Replace(html, "<[^>]+>", string.Empty);
        
        // Split on whitespace, preserving words with punctuation
        return Regex.Split(text, @"\s+").Where(w => !string.IsNullOrEmpty(w)).ToArray();
    }
}