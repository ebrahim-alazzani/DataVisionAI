using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Services;

public class ExcelReaderService
{
    private static readonly XNamespace NsMain = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace NsRel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public async Task<DataTableProfile> LoadAndProfileAsync(string filePath, string? targetSheetName = null)
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var fileInfo = new FileInfo(filePath);
            var extension = fileInfo.Extension.ToLowerInvariant();

            DataTable table;
            var sheetNames = new List<string>();

            if (extension == ".csv")
            {
                table = ReadCsv(filePath);
                sheetNames.Add("Sheet1");
            }
            else if (extension == ".xlsx")
            {
                (table, sheetNames) = ReadXlsx(filePath, targetSheetName);
            }
            else
            {
                throw new NotSupportedException($"صيغة الملف غير مدعومة: {extension}. يرجى استخدام .xlsx أو .csv");
            }

            var profile = new DataTableProfile
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                SheetName = table.TableName,
                FileSizeBytes = fileInfo.Length,
                TotalRows = table.Rows.Count,
                TotalColumns = table.Columns.Count,
                SheetNames = sheetNames,
                RawTable = table
            };

            foreach (DataColumn col in table.Columns)
            {
                profile.Columns.Add(ProfileColumn(table, col));
            }

            return profile;
        });
    }

    private static (DataTable table, List<string> sheetNames) ReadXlsx(string filePath, string? targetSheet)
    {
        using var zip = ZipFile.OpenRead(filePath);

        // 1. Load shared strings
        var sharedStrings = new List<string>();
        var sharedStringsEntry = zip.GetEntry("xl/sharedStrings.xml");
        if (sharedStringsEntry != null)
        {
            using var stream = sharedStringsEntry.Open();
            var doc = XDocument.Load(stream);
            foreach (var si in doc.Descendants(NsMain + "si"))
            {
                // Can contain multiple <t> or formatting
                var text = string.Concat(si.Descendants(NsMain + "t").Select(t => t.Value));
                sharedStrings.Add(text);
            }
        }

        // 2. Read workbook to get sheet names
        var sheetNames = new List<string>();
        var sheetMap = new Dictionary<string, string>(); // name -> filename
        var wbEntry = zip.GetEntry("xl/workbook.xml");
        if (wbEntry != null)
        {
            using var stream = wbEntry.Open();
            var doc = XDocument.Load(stream);
            int sheetIndex = 1;
            foreach (var sheet in doc.Descendants(NsMain + "sheet"))
            {
                var name = sheet.Attribute("name")?.Value ?? $"Sheet{sheetIndex}";
                sheetNames.Add(name);
                sheetMap[name] = $"xl/worksheets/sheet{sheetIndex}.xml";
                sheetIndex++;
            }
        }

        if (sheetNames.Count == 0)
        {
            sheetNames.Add("Sheet1");
            sheetMap["Sheet1"] = "xl/worksheets/sheet1.xml";
        }

        var activeSheetName = !string.IsNullOrEmpty(targetSheet) && sheetMap.ContainsKey(targetSheet)
            ? targetSheet
            : sheetNames[0];

        var sheetPath = sheetMap[activeSheetName];
        var sheetEntry = zip.GetEntry(sheetPath) ?? zip.Entries.FirstOrDefault(e => e.FullName.StartsWith("xl/worksheets/sheet") && e.FullName.EndsWith(".xml"));

        if (sheetEntry == null)
            throw new InvalidOperationException("لم يتم العثور على أوراق عمل داخل ملف الإكسل.");

        var dataTable = new DataTable(activeSheetName);

        using (var sheetStream = sheetEntry.Open())
        {
            var sheetDoc = XDocument.Load(sheetStream);
            var rows = sheetDoc.Descendants(NsMain + "row").ToList();

            if (rows.Count > 0)
            {
                // Row 1 is headers
                var headerRow = rows[0];
                var headers = new List<string>();
                var headerCols = new HashSet<string>();

                foreach (var cell in headerRow.Elements(NsMain + "c"))
                {
                    var val = GetCellValue(cell, sharedStrings);
                    var colName = string.IsNullOrWhiteSpace(val) ? $"Column_{headers.Count + 1}" : val.Trim();

                    // Avoid duplicate column names in DataTable
                    var uniqueColName = colName;
                    int dup = 2;
                    while (headerCols.Contains(uniqueColName))
                    {
                        uniqueColName = $"{colName}_{dup++}";
                    }
                    headerCols.Add(uniqueColName);
                    headers.Add(uniqueColName);
                    dataTable.Columns.Add(uniqueColName, typeof(string));
                }

                // Data Rows
                for (int i = 1; i < rows.Count; i++)
                {
                    var rowEl = rows[i];
                    var dr = dataTable.NewRow();

                    foreach (var cell in rowEl.Elements(NsMain + "c"))
                    {
                        var cellRef = cell.Attribute("r")?.Value ?? "";
                        var colLetters = Regex.Match(cellRef, @"^[A-Z]+").Value;
                        int colIdx = ColumnLettersToIndex(colLetters);

                        if (colIdx >= 0 && colIdx < dataTable.Columns.Count)
                        {
                            var val = GetCellValue(cell, sharedStrings);
                            dr[colIdx] = val;
                        }
                    }

                    dataTable.Rows.Add(dr);
                }
            }
        }

        return (dataTable, sheetNames);
    }

    private static string GetCellValue(XElement cell, List<string> sharedStrings)
    {
        var type = cell.Attribute("t")?.Value;
        var valEl = cell.Element(NsMain + "v");
        var val = valEl?.Value ?? "";

        if (type == "s" && int.TryParse(val, out int strIdx) && strIdx >= 0 && strIdx < sharedStrings.Count)
        {
            return sharedStrings[strIdx];
        }
        if (type == "inlineStr")
        {
            return cell.Element(NsMain + "is")?.Element(NsMain + "t")?.Value ?? "";
        }

        return val;
    }

    private static int ColumnLettersToIndex(string columnLetters)
    {
        if (string.IsNullOrEmpty(columnLetters)) return -1;
        int sum = 0;
        foreach (char c in columnLetters)
        {
            sum = (sum * 26) + (c - 'A' + 1);
        }
        return sum - 1;
    }

    private static DataTable ReadCsv(string filePath)
    {
        var dt = new DataTable("Sheet1");
        using var reader = new StreamReader(filePath, Encoding.UTF8);

        string? headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine)) return dt;

        char delimiter = headerLine.Contains(';') ? ';' : (headerLine.Contains('\t') ? '\t' : ',');
        var headers = ParseCsvLine(headerLine, delimiter);
        var existingNames = new HashSet<string>();

        foreach (var h in headers)
        {
            var colName = string.IsNullOrWhiteSpace(h) ? $"Column_{dt.Columns.Count + 1}" : h.Trim();
            var unique = colName;
            int counter = 2;
            while (existingNames.Contains(unique))
            {
                unique = $"{colName}_{counter++}";
            }
            existingNames.Add(unique);
            dt.Columns.Add(unique, typeof(string));
        }

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = ParseCsvLine(line, delimiter);
            var row = dt.NewRow();
            for (int i = 0; i < Math.Min(parts.Count, dt.Columns.Count); i++)
            {
                row[i] = parts[i];
            }
            dt.Rows.Add(row);
        }

        return dt;
    }

    private static List<string> ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString().Trim());
        return result;
    }

    private static DataColumnProfile ProfileColumn(DataTable table, DataColumn col)
    {
        var colName = col.ColumnName;
        int nonNullCount = 0;
        int nullCount = 0;
        var distinctValues = new HashSet<string>();
        var sampleValues = new List<string>();

        var numericValues = new List<double>();
        int detectedNumberCount = 0;
        int detectedDateCount = 0;

        foreach (DataRow row in table.Rows)
        {
            var rawVal = row[col];
            if (rawVal == null || rawVal == DBNull.Value || string.IsNullOrWhiteSpace(rawVal.ToString()))
            {
                nullCount++;
                continue;
            }

            nonNullCount++;
            var strVal = rawVal.ToString()!.Trim();
            if (distinctValues.Count < 500)
            {
                distinctValues.Add(strVal);
            }

            if (sampleValues.Count < 6)
            {
                sampleValues.Add(strVal);
            }

            // Check if numeric
            if (double.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out double numVal) ||
                double.TryParse(strVal, NumberStyles.Any, CultureInfo.CurrentCulture, out numVal))
            {
                detectedNumberCount++;
                numericValues.Add(numVal);
            }
            else if (DateTime.TryParse(strVal, out _))
            {
                detectedDateCount++;
            }
        }

        string detectedType = "string";
        if (nonNullCount > 0)
        {
            if ((double)detectedNumberCount / nonNullCount > 0.8)
                detectedType = "number";
            else if ((double)detectedDateCount / nonNullCount > 0.8)
                detectedType = "datetime";
        }

        var profile = new DataColumnProfile
        {
            Name = colName,
            DataType = detectedType,
            NonNullCount = nonNullCount,
            NullCount = nullCount,
            UniqueCount = distinctValues.Count,
            SampleValues = sampleValues
        };

        if (profile.IsNumeric && numericValues.Count > 0)
        {
            profile.MinValue = numericValues.Min();
            profile.MaxValue = numericValues.Max();
            profile.MeanValue = Math.Round(numericValues.Average(), 2);
            profile.SumValue = Math.Round(numericValues.Sum(), 2);
        }

        return profile;
    }
}
