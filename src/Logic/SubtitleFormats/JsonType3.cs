﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class JsonType3 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".json"; }
        }

        public override string Name
        {
            get { return "JSON Type 3"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                    sb.Append(",");
                sb.Append("{\"duration\":");
                sb.Append(p.Duration.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"content\":\"");
                sb.Append(p.Text.Replace("\\", string.Empty).Replace("{", string.Empty).Replace("{", string.Empty).Replace("\"", "\\\"").Replace(Environment.NewLine, "\\n") + "\"");
                sb.Append(",\"startOfParagraph\":false");
                sb.Append(",\"startTime\":");
                sb.Append(p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append("}");
                count++;
            }
            sb.Append("]");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);
            int startIndex = sb.ToString().IndexOf("[{\"duration");
            if (startIndex == -1)
                return;

            string text = sb.ToString().Substring(startIndex);
            foreach (string line in text.Replace("},{", Environment.NewLine).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "startTime");
                string duration = Json.ReadTag(s, "duration");
                string content = Json.ReadTag(s, "content");
                if (start != null && duration != null && content != null)
                {
                    double startSeconds;
                    double durationSeconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startSeconds) &&
                        double.TryParse(duration, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out durationSeconds) &&
                        content != null)
                    {
                        content = content.Replace("<br />", Environment.NewLine);
                        content = content.Replace("<br>", Environment.NewLine);
                        content = content.Replace("<br/>", Environment.NewLine);
                        content = content.Replace("\\n", Environment.NewLine);
                        subtitle.Paragraphs.Add(new Paragraph(content, startSeconds, startSeconds + durationSeconds));
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

    }
}
