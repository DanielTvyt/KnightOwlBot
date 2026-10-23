namespace KnightOwlBot
{
    internal class Piece
    {
        //public int[] moveDelta;
        public bool isWhite;
        public bool isSliding;
        public int material;
        public byte notation;
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

        public static readonly int[][] moveDelta =
        [
            new int[] { 0 }, // Pawn
            knight,
            bishop,
            rook,
            queen,
            queen,
            new int[] { 0 }, // Pawn
            knight,
            bishop,
            rook,
            queen,
            queen
        ];

        public Piece(char pieceIn)
        {
            switch (pieceIn)
            {
                case 'P':
                    isWhite = true;
                    material = 100;
                    isSliding = false;
                    notation = P;
                    return;
                case 'N':
                    isWhite = true;
                    material = 300;
                    isSliding = false;
                    notation = N;
                    return;
                case 'B':
                    isWhite = true;
                    material = 350;
                    isSliding = true;
                    notation = B;
                    return;
                case 'R':
                    isWhite = true;
                    material = 500;
                    isSliding = true;
                    notation = R;
                    return;
                case 'Q':
                    isWhite = true;
                    material = 900;
                    isSliding = true;
                    notation = Q;
                    return;
                case 'K':
                    isWhite = true;
                    material = 0;
                    isSliding = false;
                    notation = K;
                    return;

                case 'p':
                    isWhite = false;
                    material = -100;
                    isSliding = false;
                    notation = p;
                    return;
                case 'n':
                    isWhite = false;
                    material = -300;
                    isSliding = false;
                    notation = n;
                    return;
                case 'b':
                    isWhite = false;
                    material = -350;
                    isSliding = true;
                    notation = b;
                    return;
                case 'r':
                    isWhite = false;
                    material = -500;
                    isSliding = true;
                    notation = r;
                    return;
                case 'q':
                    isWhite = false;
                    material = -900;
                    isSliding = true;
                    notation = q;
                    return;
                case 'k':
                    isWhite = false;
                    material = 0;
                    isSliding = false;
                    notation = k;
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