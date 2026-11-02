using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinal.Localization
{
    /// <summary>Parser CSV (RFC 4180) com suporte a campos entre aspas e aspas escapadas.</summary>
    public static class CsvParser
    {
        public static List<string[]> Parse(string text)
        {
            var rows = new List<string[]>();
            if (string.IsNullOrEmpty(text))
                return rows;

            var row = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;
            var start = text[0] == '﻿' ? 1 : 0;

            void EndRow()
            {
                row.Add(field.ToString());
                field.Clear();
                var isBlankLine = row.Count == 1 && row[0].Length == 0;
                if (!isBlankLine)
                    rows.Add(row.ToArray());
                row.Clear();
            }

            for (var i = start; i < text.Length; i++)
            {
                var c = text[i];

                if (inQuotes)
                {
                    if (c != '"')
                        field.Append(c);
                    else if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                        inQuotes = false;
                }
                else if (c == '"')
                    inQuotes = true;
                else if (c == ',')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;
                    EndRow();
                }
                else
                    field.Append(c);
            }

            if (inQuotes)
                throw new FormatException("CSV inválido: campo entre aspas não foi fechado.");

            if (field.Length > 0 || row.Count > 0)
                EndRow();

            return rows;
        }
    }
}
