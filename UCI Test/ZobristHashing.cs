using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnightOwlBot
{
    internal class ZobristHashing
    {
        public static readonly ulong[] TABLE = new ulong[768]; // 12 pieces * 64 squares
        //pawns cant be on first or last rank so we save 16 entries
        public const int IsWhiteIndex       = 000; // 1 entry
        public const int CastlingIndex      = 012; // 4 entries (x * 12)
        public const int EnPassentFileIndex = 008; // 8 entries (x * 12)

        public ulong HashValue = 0;

        public ZobristHashing()
        {
            Random rand = new Random(696131231); //Number from random.org
            for (int i = 0; i < TABLE.Length; i++)
            {
                TABLE[i] = NextUInt64(rand);
            }
        }

        public void InitializeHash(Board board)
        {
            HashValue = 0;
            for (int i = 0; i < 64; i++)
            {
                Piece piece = board.board[i];
                if (piece != null)
                {
                    UpdateHash(piece.Notation, i);
                }
            }
            if (board.IsWhiteToMove)
            {
                HashValue ^= TABLE[IsWhiteIndex];
            }
            for (int i = 0; i < board.CastlingRights.Length; i++)
            {
                if (board.CastlingRights[i])
                {
                    HashValue ^= TABLE[CastlingIndex + i * 12];
                }
            }
            if (board.EnPassentIndex != 100)
            {
                int file = board.EnPassentIndex % 8;
                HashValue ^= TABLE[EnPassentFileIndex + file * 12];
            }
        }

        public void UpdateHash(int pieceNotation, int squareIndex)
        {
            HashValue ^= TABLE[(pieceNotation - 1) + squareIndex * 12];
        }

        public static UInt64 NextUInt64(Random rng)
        {
            byte[] buffer = new byte[8];
            rng.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
