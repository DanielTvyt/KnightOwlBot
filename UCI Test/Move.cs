namespace KnightOwlBot
{
    internal class Move
    {
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public bool IsCapture { get; set; }
        public char PromPiece { get; set; }
        public byte LastCapture { get; set; }
        public int EnPassentIndex { get; set; }
        public int PrevEnPassentIndex { get; set; }
        public int MoveValue { get; set; }

        private string Notation;

        public Move(int index1, int index2, bool isCapture = false, byte lastCapture = 0, int enPassentIndex = -1, char promPiece = '\0')
        {
            this.Index1 = index1;
            this.Index2 = index2;
            this.IsCapture = isCapture;
            this.PromPiece = promPiece;
            this.LastCapture = lastCapture;
            this.EnPassentIndex = enPassentIndex;
            this.PrevEnPassentIndex = -1;
            this.MoveValue = 0;
            this.Notation = null;
        }

        public string GetNotation()
        {
            if (this.Notation != null)
            {
                return this.Notation;
            }
            string moveNotation = LookupTables.indexToPos[Index1] + LookupTables.indexToPos[Index2];
            if (PromPiece != '\0')
            {
                moveNotation += char.ToLower(PromPiece);
            }
            this.Notation = moveNotation;
            return this.Notation;
        }
    }
}
