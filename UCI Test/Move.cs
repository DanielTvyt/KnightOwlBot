using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCI_Test
{
    internal class Move
    {
        public string Notation {  get; set; }
        public bool IsCapture { get; set; }
        public char PromPiece { get; set; }
        public char LastCapture { get; set; }
        public int EnPassentIndex { get; set; }
    }
}
