using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesisHelper.model
{
    internal interface IChatModel
    {
        string SendAndWait(string message, IEnumerable<LatexBlock> context);
    }
    public class StubChatModel : IChatModel
    {
        public string SendAndWait(string message, IEnumerable<LatexBlock> context)
        {
           return System.IO.File.ReadAllText("answer.txt");
        }
    }
}
