namespace KnightOwlBot
{
    internal class Move
    {
        public string Notation {  get; set; }
        public bool IsCapture { get; set; }
        public char PromPiece { get; set; }
        public byte LastCapture { get; set; }
        public int EnPassentIndex { get; set; }
        public byte MoveValue { get; set; }
    }
}
