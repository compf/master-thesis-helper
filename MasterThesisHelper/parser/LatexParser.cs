using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using  System.IO;
using System.Diagnostics;
using System.Text.Json.Serialization;
namespace MasterThesisHelper.parser
{
    [JsonDerivedType(typeof(LatexTextBlock))]
    [JsonDerivedType(typeof(LatexSectionBlock))]

    public class LatexBlock
    {
        public List<LatexBlock> Children { get; set; } = new();
        public string FilePath { get; set; }
        public List<int> Position { get; set; } = new();

        public int Level { get; set; }
        public int Line { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public LatexBlock Parent { get; set; }
        public void AddChild(LatexBlock child)
        {
            Children.Add(child);
            child.Parent = this;
        }
        protected string GetPadding()
        {
            if(Level<0)
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
            return "";
        }

    }

    public class LatexTextBlock : LatexBlock
    {
        public string Text { get; set; }
        public override string ToString()
        {
            return GetPadding() + Text.Substring(0,Math.Min(20,Text.Length));
        }
    }
    public class LatexSectionBlock : LatexBlock
    {
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
            block.Position.Add(1);
            block.Level = -1;
            int j = 0;
            var result = Parse( block, null, ref j);
            return result;

        }
        private LatexBlock Parse( LatexBlock parent, string[] lines, ref int i)
        {
            Trace.WriteLine(parent);
            string buffer = "";
            if(lines==null)
            {
                lines = System.IO.File.ReadAllLines(parent.FilePath);
            }
            for( ;i<lines.Length;i++)
            {
                string line = lines[i];
                int level = 0;
                if(line.TrimStart().StartsWith("%"))
                {
                    continue;
                }
                else if(CheckIsSection(line,ref level))
                {
                    if(!String.IsNullOrWhiteSpace(buffer))
                    {
                        LatexTextBlock latexTextBlock = new LatexTextBlock();
                        latexTextBlock.Text = buffer;
                        latexTextBlock.FilePath = parent.FilePath;
                        latexTextBlock.Line = i;
                        latexTextBlock.Position = new List<int>(parent.Position);
                        parent.AddChild(latexTextBlock);
                        Trace.WriteLine(latexTextBlock);
                        buffer = "";
                    }
                    LatexSectionBlock block = new LatexSectionBlock();
                    block.Title = GetBetweenCurlyBrackets(line);
                    block.FilePath = parent.FilePath;
                    block.Line = i;
                    block.Position = new List<int>(parent.Position);

                    if (level> parent.Level)
                    {
                        block.Position.Add(1);
                        block.Level = parent.Level + 1;
                        parent.AddChild(block);
                        i++;
                        Parse( block, lines, ref i);
                    }

                    else
                    {
                        block.Position[level]++;
                        for(int j=level +1;j<block.Position.Count;j++)
                        {
                            block.Position[j] = 1;
                        }
                        while(parent.Level+1 !=level)
                        {
                            parent=parent.Parent;
                        }
                        block.Level = parent.Level+1;
                        parent.AddChild(block);
                        parent = block;
                    }
                }
                else if(line.Contains("\\input{"))
                {
                    string path = GetBetweenCurlyBrackets(line);
                    path = System.IO.Path.Combine(Path.GetDirectoryName(mainPath), path)+".tex";
                    LatexBlock block = new LatexBlock();
                    block.FilePath = path;
                   
                    block.Position = new List<int>(parent.Position);
                    block.Line = i;
                    block.Level = parent.Level;
                    parent.AddChild(block);
                    int j = 0;
                    Parse(block, null,  ref j);
                    
                }
                else
                {
                    buffer += line + "\n";
                }
            }
            if (!String.IsNullOrWhiteSpace(buffer))
            {
                LatexTextBlock latexTextBlock = new LatexTextBlock();
                latexTextBlock.Text = buffer;
                latexTextBlock.FilePath = parent.FilePath;
                latexTextBlock.Line = i;
                latexTextBlock.Position = new List<int>(parent.Position);
                parent.AddChild(latexTextBlock);
                Trace.WriteLine(latexTextBlock);
                buffer = "";
            }
            return parent;

        }
        private string GetBetweenCurlyBrackets(string line)
        {
            int open = line.IndexOf("{");
            int length=  line.IndexOf("}") - open;
            return line.Substring(open+1, length-1);
        }
        private bool CheckIsSection(string line, ref int level)
        {
            if (line.Contains("\\chapter{"))
            {
                level =0;
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
