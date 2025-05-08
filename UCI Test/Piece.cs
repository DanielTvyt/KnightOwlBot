namespace KnightOwlBot
{
    internal class Piece
    {
        public int[] MoveDelta;
        public bool IsWhite;
        public bool IsSliding;
        public int Material;
        public byte Notation;

        public static Piece CreatePiece(char pieceIn)
        { 
            int[] bishop = [-9, -7, 7, 9];
            int[] rook = [-8, -1, 1, 8];
            int[] queen = [-9, -8, -7, -1, 1, 7, 8, 9];
            int[] knight = [-17, -15, -10, -6, 6, 10, 15, 17];
            switch (pieceIn)
            {
                case 'P':
                    Piece pawnW = new Piece
                    {
                        IsWhite = true,
                        Material = 100,
                        IsSliding = false,
                        Notation = 0,
                    };
                    return pawnW;
                case 'N':
                    Piece knightW = new Piece
                    {
                        IsWhite = true,
                        Material = 300,
                        IsSliding = false,
                        MoveDelta = knight,
                        Notation = 1,
                    };
                    return knightW;
                case 'B':
                    Piece bishopW = new Piece
                    {
                        IsWhite = true,
                        Material = 300,
                        IsSliding = true,
                        MoveDelta = bishop,
                        Notation = 2,
                    };
                    return bishopW;
                case 'R':
                    Piece rookW = new Piece
                    {
                        IsWhite = true,
                        Material = 500,
                        IsSliding = true,
                        MoveDelta = rook,
                        Notation = 3,
                    };
                    return rookW;
                case 'Q':
                    Piece queenW = new Piece
                    {
                        IsWhite = true,
                        Material = 900,
                        IsSliding = true,
                        MoveDelta = queen,
                        Notation = 4,
                    };
                    return queenW;
                case 'K':
                    Piece kingW = new Piece
                    {
                        IsWhite = true,
                        Material = 0,
                        IsSliding = false,
                        MoveDelta = queen,
                        Notation = 5,
                    };
                    return kingW;

                case 'p':
                    Piece pawnB = new Piece
                    {
                        IsWhite = false,
                        Material = -100,
                        IsSliding = false,
                        Notation = 6,
                    };
                    return pawnB;
                case 'n':
                    Piece knightB = new Piece
                    {
                        IsWhite = false,
                        Material = -300,
                        IsSliding = false,
                        MoveDelta = knight,
                        Notation = 7,
                    };
                    return knightB;
                case 'b':
                    Piece bishopB = new Piece
                    {
                        IsWhite = false,
                        Material = -300,
                        IsSliding = true,
                        MoveDelta = bishop,
                        Notation = 8,
                    };
                    return bishopB;
                case 'r':
                    Piece rookB = new Piece
                    {
                        IsWhite = false,
                        Material = -500,
                        IsSliding = true,
                        MoveDelta = rook,
                        Notation = 9,
                    };
                    return rookB;
                case 'q':
                    Piece queenB = new Piece
                    {
                        IsWhite = false,
                        Material = -900,
                        IsSliding = true,
                        MoveDelta = queen,
                        Notation = 10,
                    };
                    return queenB;
                case 'k':
                    Piece kingB = new Piece
                    {
                        IsWhite = false,
                        Material = 0,
                        IsSliding = false,
                        MoveDelta = queen,
                        Notation = 11,
                    };
                    return kingB;

                default:
                    return null;
            }
        }

        public static char byteToChar( byte b )
        {
            switch ( b )
            {
                case 0:
                    return 'P';
                case 1:
                    return 'N';
                case 2:
                    return 'B';
                case 3:
                    return 'R';
                case 4:
                    return 'Q';
                case 5:
                    return 'K';
                case 6:
                    return 'p';
                case 7:
                    return 'n';
                case 8:
                    return 'b';
                case 9:
                    return 'r';
                case 10:
                    return 'q';
                case 11:
                    return 'k';
                default:
                    return ' ';
                    
            }
        }
    }
}
