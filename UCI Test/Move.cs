namespace KnightOwlBot
{
    internal class Move
    {
        public int Index1;
        public int Index2;
        public bool IsCapture;
        public char PromPiece;
        public Piece LastCapture;
        public int EnPassentIndex;
        public int PrevEnPassentIndex = 100;
        public int MoveValue = 0;
        public bool IsFirstMove = false;
        public bool IsCastleMove = false;

        private string Notation = null;

        public Move(int index1, int index2, bool isCapture = false, Piece lastCapture = null, int enPassentIndex = 100, char promPiece = '\0')
        {
            this.Index1 = index1;
            this.Index2 = index2;
            this.IsCapture = isCapture;
            this.PromPiece = promPiece;
            this.LastCapture = lastCapture;
            this.EnPassentIndex = enPassentIndex;
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
