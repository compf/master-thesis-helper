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
            private readonly LatexBlock  head;
        private Stack<int> indexStack = new Stack<int>();
        public string Current => current.ToString();
        private bool hasGoneUp = false;
        object IEnumerator.Current =>current;

        public void Dispose()
        {
            
        }
        public LatexEnumerator(LatexBlock head)
        {
            this.head = head;

        }
        public bool MoveNext()
        {
          if(current==null)
            {
                current = head;
                indexStack.Push(0);
                return true;
            }
            else
            {

                int index=indexStack.Pop();
                if(index<current.Children.Count)
                {
                    var next = current.Children[index];
           
                    indexStack.Push(index + 1);
                    if (!hasGoneUp)
                    {
                        indexStack.Push(0);
                        current = next;

                    }
                    hasGoneUp = false;
                    return true;
                }
                else
                {
                    current = current.Parent;
                    hasGoneUp = true;
                    return current!=null &&  MoveNext();
                }
            }
        }

        public void Reset()
        {
            current = null;
        }
    }
}
