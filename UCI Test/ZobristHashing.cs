using System;
using System.Runtime.CompilerServices;

namespace KnightOwlBot
{
    internal class ZobristHashing
    {
        private static readonly ulong[][] TABLE; // 64 squares * 12 pieces
        //pawns cant be on first or last rank so we save 16 entries
        // [0-7][0] enpassent files
        // [0][6] is white to move
        // [1-4][6] castling rights (KQkq)

        private static readonly Random rand = new(696131231); //Number from random.org

        public ulong hashValue;

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
            hashValue = 0;
            for (int i = 0; i < 64; i++)
            {
                Piece piece = board.board[i];
                if (piece != null)
                {
                    UpdateHash(piece.notation, i);
                }
            }
            if (board.isWhiteToMove)
            {
                UpdateHashSideToMove();
            }
            UpdateHashCastling(board.castlingRights);
            UpdateHashEnPassent(board.enPassentIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateHash(int pieceNotation, int squareIndex)
        {
            hashValue ^= TABLE[squareIndex][pieceNotation];
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
            hashValue ^= TABLE[0][6];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateHashCastling(bool[] castlingRights)
        {
            for (int i = 0; i < 4; i++)
            {
                if (castlingRights[i])
                {
                    hashValue ^= TABLE[i + 1][6];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateHashEnPassent(int enPassentIndex)
        {
            if (enPassentIndex < 64)
            {
                int file = enPassentIndex % 8;
                hashValue ^= TABLE[file][0];
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
