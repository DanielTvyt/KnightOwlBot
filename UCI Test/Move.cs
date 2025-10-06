namespace KnightOwlBot
{
    internal class Move
    {
        public string Notation {  get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public bool IsCapture { get; set; }
        public char PromPiece { get; set; }
        public byte LastCapture { get; set; }
        public int EnPassentIndex { get; set; }
        public int MoveValue { get; set; }
    }
}
