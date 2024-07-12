using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MasterThesisHelper.parser;
namespace MasterThesisHelper.model
{
    public class LatexEnumerator : IEnumerator<string>
    {
        private LatexBlock current;
        private List<LatexBlock> blocks;
        private int index = -1;
        
        public string Current => blocks[index].ToString();

        object IEnumerator.Current => blocks[index];

        public void Dispose()
        {
            
        }
        public LatexEnumerator(LatexBlock head)
        {
            Queue<LatexBlock> blocks = new();
            this.blocks = new();
            blocks.Enqueue(head);
            while(blocks.Any())
            {
                var block = blocks.Dequeue();
                this.blocks.Add(block);
                foreach(var c in block.Children)
                {
                    blocks.Enqueue(c);
                }
            }

        }
        public bool MoveNext()
        {
            index++;
            return index < blocks.Count;
        }

        public void Reset()
        {
            index = -1;
        }
    }
}
