namespace KnightOwlBot
{
    internal class Move
    {
        public int index1;
        public int index2;
        public bool isCapture;
        public char promPiece;
        public Piece lastCapture;
        public int enPassentIndex;
        public int prevEnPassentIndex = 100;
        public int moveValue = 0;
        public bool isFirstMove = false;
        public bool isCastleMove = false;

        public Move(int index1, int index2, bool isCapture = false, Piece lastCapture = null, int enPassentIndex = 100, char promPiece = '\0')
        {
            this.index1 = index1;
            this.index2 = index2;
            this.isCapture = isCapture;
            this.promPiece = promPiece;
            this.lastCapture = lastCapture;
            this.enPassentIndex = enPassentIndex;
        }

        public string GetNotation()
        {
            string moveNotation = LookupTables.indexToPos[index1] + LookupTables.indexToPos[index2];
            if (promPiece != '\0')
            {
                moveNotation += char.ToLower(promPiece);
            }
            return moveNotation;
        }
    }
}
