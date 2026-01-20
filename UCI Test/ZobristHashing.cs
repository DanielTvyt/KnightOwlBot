using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace KnightOwlBot
{
    internal class ZobristHashing
    {
        public static readonly ulong[][] TABLE; // 64 squares * 12 pieces
        //pawns cant be on first or last rank so we save 16 entries
        // [0-7][0] enpassent files
        // [0][6] is white to move
        // [1-4][6] castling rights (KQkq)

        private static Random rand = new Random(696131231); //Number from random.org

        public ulong HashValue;

        static ZobristHashing()
        {
            var zobristTable = new ulong[64][];

            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                zobristTable[squareIndex] = new ulong[12];
                for (int pieceIndex = 0; pieceIndex < 12; pieceIndex++)
                {
                    zobristTable[squareIndex][pieceIndex] = NextUInt64();
                }
            }
            TABLE = zobristTable;
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
                UpdateHashSideToMove();
            }
            UpdateHashCastling(board.CastlingRights);
            UpdateHashEnPassent(board.EnPassentIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateHash(int pieceNotation, int squareIndex)
        {
            HashValue ^= TABLE[squareIndex][pieceNotation];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateHashMove(int fromPieceNotation, int fromIndex, int toPieceNotation, int toIndex, int curEnpassant, int lastEnpassant)
        {
            UpdateHash(fromPieceNotation, fromIndex);
            UpdateHash(toPieceNotation, toIndex);
            UpdateHashEnPassent(curEnpassant);
            UpdateHashEnPassent(lastEnpassant);
            UpdateHashSideToMove();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateHashSideToMove()
        {
            HashValue ^= TABLE[0][6];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateHashCastling(bool[] castlingRights)
        {
            for (int i = 0; i < 4; i++)
            {
                if (castlingRights[i])
                {
                    HashValue ^= TABLE[i + 1][6];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateHashEnPassent(int enPassentIndex)
        {
            if (enPassentIndex < 64)
            {
                int file = enPassentIndex % 8;
                HashValue ^= TABLE[file][0];
            }
        }

        private static UInt64 NextUInt64()
        {
            byte[] buffer = new byte[8];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
