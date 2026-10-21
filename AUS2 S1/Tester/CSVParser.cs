using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace AUS2_S1.Tester
{
    // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt 35
    public class CSVParser
    {
        public LinkedList<string> ColumnData { get; } = new LinkedList<string>();
        public LinkedList<string> ColumnA { get; } = new LinkedList<string>();
        public LinkedList<string> ColumnB { get; } = new LinkedList<string>();

        public LinkedList<string> ColumnId { get; } = new LinkedList<string>();
        public LinkedList<string> ColumnMinA { get; } = new LinkedList<string>();
        public LinkedList<string> ColumnMinB { get; } = new LinkedList<string>();
        public LinkedList<string> ColumnMaxA { get; } = new LinkedList<string>();
        public LinkedList<string> ColumnMaxB { get; } = new LinkedList<string>();

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt 35
        public void LoadPoints(string filePath, int dataColumnIndex = 0, int aColumnIndex = 1, int bColumnIndex = 2, char delimiter = ';')
        {
            ColumnData.Clear();
            ColumnA.Clear();
            ColumnB.Clear();

            using var reader = new StreamReader(filePath);
            reader.ReadLine(); // preskoc hlavicku

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(delimiter);
                ColumnData.AddLast(parts[dataColumnIndex]);
                ColumnA.AddLast(parts[aColumnIndex]);
                ColumnB.AddLast(parts[bColumnIndex]);
            }
        }

        // Vygenerovane / upravene pomocou AI (Model: Claude Sonnet 4.6) - Prompt 35
        public void LoadRanges(string filePath, int idColumnIndex = 0, int minAColumnIndex = 1, int minBColumnIndex = 2, int maxAColumnIndex = 3, int maxBColumnIndex = 4, char delimiter = ';')
        {
            ColumnId.Clear();
            ColumnMinA.Clear();
            ColumnMinB.Clear();
            ColumnMaxA.Clear();
            ColumnMaxB.Clear();

            using var reader = new StreamReader(filePath);
            reader.ReadLine(); // preskoc hlavicku

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(delimiter);
                ColumnId.AddLast(parts[idColumnIndex]);
                ColumnMinA.AddLast(parts[minAColumnIndex]);
                ColumnMinB.AddLast(parts[minBColumnIndex]);
                ColumnMaxA.AddLast(parts[maxAColumnIndex]);
                ColumnMaxB.AddLast(parts[maxBColumnIndex]);
            }
        }
    }
}
