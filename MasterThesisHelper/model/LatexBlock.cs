using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MasterThesisHelper.model
{
    [JsonDerivedType(typeof(LatexTextBlock))]
    [JsonDerivedType(typeof(LatexSectionBlock))]

    public class LatexBlock : IEnumerable<LatexBlock>, INotifyPropertyChanged
    {
        public string Section { get; set; }

        public List<LatexBlock> Children { get; set; } = new();
        public string FilePath { get; set; }
        public List<LatexBlock> GetChecked()
        {
            List<LatexBlock> all = new();
            GetChecked(this, all);
            return all;
        }

        private static void GetChecked(LatexBlock curr, List<LatexBlock> all)
        {
            if(curr.IsChecked)
            {
                all.Add(curr);
            }
            else
            {
                foreach(var c in curr.Children)
                {
                    GetChecked(c, all);
                }
            }
        }

        public IEnumerator<LatexBlock> GetEnumerator()
        {
            return Children.GetEnumerator();
        }
        private bool _isChecked = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        public virtual string GetCompleteString()
        {
            return string.Join("\n", Children.Select((x) => x.GetCompleteString()));
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                foreach (var c in Children)
                {
                    c.IsChecked = value;
                }
                OnPropertyChanged();
            }
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
        public override string GetCompleteString()
        {
            return Text;
        }
    }
    public class LatexSectionBlock : LatexBlock
    {
        string[] sectionNames = { "chapter","section","subsection","subsubsection"

        };
       
        [JsonPropertyOrder(0)]
        public string Title { get; set; }
        public override string ToString()
        {
            return GetPadding() + Title;
        }
        public override string GetCompleteString()
        {
            return "\\" + sectionNames[Level] + "{" + Title + "}\n" + base.GetCompleteString();
        }
    }

}
