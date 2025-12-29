namespace KnightOwlBot
{
    internal class Piece
    {
        public int[] MoveDelta;
        public bool IsWhite;
        public bool IsSliding;
        public int Material;
        public byte Notation;
        public bool hasMoved = false;

        private static readonly int[] bishop = [-9, -7, 7, 9];
        private static readonly int[] rook = [-8, -1, 1, 8];
        private static readonly int[] queen = [-9, -8, -7, -1, 1, 7, 8, 9];
        private static readonly int[] knight = [-17, -15, -10, -6, 6, 10, 15, 17];

        public Piece(char pieceIn)
        {
            switch (pieceIn)
            {
                case 'P':
                    IsWhite = true;
                    Material = 100;
                    IsSliding = false;
                    Notation = 1;
                    return;
                case 'N':
                    IsWhite = true;
                    Material = 300;
                    IsSliding = false;
                    MoveDelta = knight;
                    Notation = 2;
                    return;
                case 'B':
                    IsWhite = true;
                    Material = 300;
                    IsSliding = true;
                    MoveDelta = bishop;
                    Notation = 3;
                    return;
                case 'R':
                    IsWhite = true;
                    Material = 500;
                    IsSliding = true;
                    MoveDelta = rook;
                    Notation = 4;
                    return;
                case 'Q':
                    IsWhite = true;
                    Material = 900;
                    IsSliding = true;
                    MoveDelta = queen;
                    Notation = 5;
                    return;
                case 'K':
                    IsWhite = true;
                    Material = 0;
                    IsSliding = false;
                    MoveDelta = queen;
                    Notation = 6;
                    return;

                case 'p':
                    IsWhite = false;
                    Material = -100;
                    IsSliding = false;
                    Notation = 7;
                    return;
                case 'n':
                    IsWhite = false;
                    Material = -300;
                    IsSliding = false;
                    MoveDelta = knight;
                    Notation = 8;
                    return;
                case 'b':
                    IsWhite = false;
                    Material = -300;
                    IsSliding = true;
                    MoveDelta = bishop;
                    Notation = 9;
                    return;
                case 'r':
                    IsWhite = false;
                    Material = -500;
                    IsSliding = true;
                    MoveDelta = rook;
                    Notation = 10;
                    return;
                case 'q':
                    IsWhite = false;
                    Material = -900;
                    IsSliding = true;
                    MoveDelta = queen;
                    Notation = 11;
                    return;
                case 'k':
                    IsWhite = false;
                    Material = 0;
                    IsSliding = false;
                    MoveDelta = queen;
                    Notation = 12;
                    return;
            }
        }

        public static char byteToChar(byte b)
        {
            return b switch
            {
                1 => 'P',
                2 => 'N',
                3 => 'B',
                4 => 'R',
                5 => 'Q',
                6 => 'K',
                7 => 'p',
                8 => 'n',
                9 => 'b',
                10 => 'r',
                11 => 'q',
                12 => 'k',
                _ => ' ',
            };
        }
    }
}