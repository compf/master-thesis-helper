using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Collections;
namespace MasterThesisHelper.parser
{
    [JsonDerivedType(typeof(LatexTextBlock))]
    [JsonDerivedType(typeof(LatexSectionBlock))]

    public class LatexBlock :IEnumerable<LatexBlock>
    {
        public string Section { get; set; }

        public List<LatexBlock> Children { get; set; } = new();
        public string FilePath { get; set; }


        public IEnumerator<LatexBlock> GetEnumerator()
        {
            return Children.GetEnumerator();
        }
         IEnumerator IEnumerable.GetEnumerator()
        {
            return Children.GetEnumerator();
        }
        public string GetLevelDownSection()
        {
            return Section + ".1";
        }
        public string IncrementSectionAt(int at)
        {
            int[] splitted = Section.Split(".").Select((b) => int.Parse(b)).ToArray();
            splitted[at]++;
            for (int i = at + 1; i < splitted.Length; i++)
            {
                splitted[i] = 1;
            }
            return String.Join(".", splitted);
        }
        public int Level { get; set; }
        public int Line { get; set; }

        [JsonIgnore]
        public LatexBlock Parent { get; set; }
        public void AddChild(LatexBlock child)
        {
            Children.Add(child);
            child.Parent = this;
        }
        protected string GetPadding()
        {
            if (Level < 0)
            {
                return "";
            }
            else
            {
                return new string('\t', Level);
            }
        }
        public override string ToString()
        {
            string all = String.Join('\n', Children);
            return all.Substring(0, Math.Min(20, all.Length));
        }

    }

    public class LatexTextBlock : LatexBlock
    {
        [JsonPropertyOrder(0)]
        public string Text { get; set; }
        public override string ToString()
        {
            return GetPadding() + Text.Substring(0, Math.Min(20, Text.Length));
        }
    }
    public class LatexSectionBlock : LatexBlock
    {
        [JsonPropertyOrder(0)]
        public string Title { get; set; }
        public override string ToString()
        {
            return GetPadding() + Title;
        }
    }

    public class LatexParser
    {
        private string mainPath;
        public LatexParser(string mainPath)
        {
            this.mainPath = mainPath;
        }

        public LatexBlock Parse()
        {
            LatexBlock block = new LatexBlock();
            block.FilePath = mainPath;
            block.Line = 0;
            block.Section = "1";
            block.Level = -1;
            int j = 0;
            var result = Parse(block, null, ref j);
            return result;

        }
        private void HandleNonEmptyBuffer(string buffer, LatexBlock parent ,int line)
        {
            if (  buffer.Trim().StartsWith("\\"))
            {
                return;
            }
            LatexTextBlock latexTextBlock = new LatexTextBlock();
            latexTextBlock.Text = buffer;
            latexTextBlock.FilePath = parent.FilePath;
            latexTextBlock.Line = line;
            latexTextBlock.Section = (parent.Section);
            parent.AddChild(latexTextBlock);
        }
        private LatexBlock Parse(LatexBlock parent, string[] lines, ref int i)
        {
            string buffer = "";
            if (lines == null)
            {
                lines = System.IO.File.ReadAllLines(parent.FilePath);
            }
            for (; i < lines.Length; i++)
            {
                string line = lines[i];
                int level = 0;
                if (line.TrimStart().StartsWith("%"))
                {
                    continue;
                }
                else if(line =="")
                {
                    if (!String.IsNullOrWhiteSpace(buffer))
                    {
                        HandleNonEmptyBuffer(buffer, parent, i);
                        buffer = "";
                    }
                }
                else if (line.Contains("\\begin{comment}"))
                {
                    while (!line.Contains("\\end{comment}"))
                    {
                        i++;
                        line = lines[i];
                    }
                }
                else if (CheckIsSection(line, ref level))
                {
                    if (!String.IsNullOrWhiteSpace(buffer))
                    {
                        HandleNonEmptyBuffer(buffer, parent, i);
                        buffer = "";
                    }
                    LatexSectionBlock block = new LatexSectionBlock();
                    block.Title = GetBetweenCurlyBrackets(line);
                    block.FilePath = parent.FilePath;
                    block.Line = i;
                    block.Section = (parent.Section);

                    if (level > parent.Level)
                    {
                        block.Section = parent.GetLevelDownSection();
                        block.Level = parent.Level + 1;
                        parent.AddChild(block);
                        i++;
                        Parse(block, lines, ref i);
                    }

                    else
                    {
                        block.Section = parent.IncrementSectionAt(level);

                        while (parent.Level + 1 != level)
                        {
                            parent = parent.Parent;
                        }
                        block.Level = parent.Level + 1;
                        parent.AddChild(block);
                        parent = block;
                    }
                }
                else if (line.Contains("\\input{"))
                {
                    string path = GetBetweenCurlyBrackets(line);
                    path = System.IO.Path.Combine(Path.GetDirectoryName(mainPath), path) + ".tex";
                    LatexBlock block = new LatexBlock();
                    block.FilePath = path;

                    block.Section = (parent.Section);
                    block.Line = i;
                    block.Level = parent.Level;
                    parent.AddChild(block);
                    int j = 0;
                    Parse(block, null, ref j);

                }
                else
                {
                    buffer += line + "\n";
                }
            }
            if (!String.IsNullOrWhiteSpace(buffer))
            {
                HandleNonEmptyBuffer(buffer, parent, i);
                buffer = "";
            }
            return parent;

        }
        private string GetBetweenCurlyBrackets(string line)
        {
            int open = line.IndexOf("{");
            int length = line.IndexOf("}") - open;
            return line.Substring(open + 1, length - 1);
        }
        private bool CheckIsSection(string line, ref int level)
        {
            if (line.Contains("\\chapter{"))
            {
                level = 0;
            }
            else if (line.Contains("\\section{"))
            {
                level = 1;
            }
            else if (line.Contains("\\subsection{"))
            {
                level = 2;
            }
            else if (line.Contains("\\subsubsection{"))
            {
                level = 3;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
