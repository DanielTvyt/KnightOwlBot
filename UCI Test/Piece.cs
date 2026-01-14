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

        public const byte P = 0;
        public const byte N = 1;
        public const byte B = 2;
        public const byte R = 3;
        public const byte Q = 4;
        public const byte K = 5;
        public const byte p = 6;
        public const byte n = 7;
        public const byte b = 8;
        public const byte r = 9;
        public const byte q = 10;
        public const byte k = 11;

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
                    Notation = P;
                    return;
                case 'N':
                    IsWhite = true;
                    Material = 300;
                    IsSliding = false;
                    MoveDelta = knight;
                    Notation = N;
                    return;
                case 'B':
                    IsWhite = true;
                    Material = 300;
                    IsSliding = true;
                    MoveDelta = bishop;
                    Notation = B;
                    return;
                case 'R':
                    IsWhite = true;
                    Material = 500;
                    IsSliding = true;
                    MoveDelta = rook;
                    Notation = R;
                    return;
                case 'Q':
                    IsWhite = true;
                    Material = 900;
                    IsSliding = true;
                    MoveDelta = queen;
                    Notation = Q;
                    return;
                case 'K':
                    IsWhite = true;
                    Material = 0;
                    IsSliding = false;
                    MoveDelta = queen;
                    Notation = K;
                    return;

                case 'p':
                    IsWhite = false;
                    Material = -100;
                    IsSliding = false;
                    Notation = p;
                    return;
                case 'n':
                    IsWhite = false;
                    Material = -300;
                    IsSliding = false;
                    MoveDelta = knight;
                    Notation = n;
                    return;
                case 'b':
                    IsWhite = false;
                    Material = -300;
                    IsSliding = true;
                    MoveDelta = bishop;
                    Notation = b;
                    return;
                case 'r':
                    IsWhite = false;
                    Material = -500;
                    IsSliding = true;
                    MoveDelta = rook;
                    Notation = r;
                    return;
                case 'q':
                    IsWhite = false;
                    Material = -900;
                    IsSliding = true;
                    MoveDelta = queen;
                    Notation = q;
                    return;
                case 'k':
                    IsWhite = false;
                    Material = 0;
                    IsSliding = false;
                    MoveDelta = queen;
                    Notation = k;
                    return;
            }
        }

        public static char byteToChar(byte input)
        {
            return input switch
            {
                P => 'P',
                N => 'N',
                B => 'B',
                R => 'R',
                Q => 'Q',
                K => 'K',
                p => 'p',
                n => 'n',
                b => 'b',
                r => 'r',
                q => 'q',
                k => 'k',
                _ => ' ',
            };
        }
    }
}