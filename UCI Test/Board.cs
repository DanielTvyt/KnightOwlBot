using System;
using System.Collections.Generic;

namespace KnightOwlBot
{
    internal class Board
    {
        public Piece[] board { get; set; }
        public List<int> ThreeFold { get; set; }
        public bool IsWhiteToMove { get; set; }
        public int EnPassentIndex {  get; set; }
        public bool IsInCheck { get; set; }
        public bool DoubleCheck { get; set; }
        public int IndexOfCheckingPiece { get; set; }
        public UInt64 BitboardAttacked { get; set; }
        public UInt64 BitboardCheck { get; set; }
        public List<UInt64> BitboardPinned {  get; set; }

        public static Board BuildFromFenString(string fenString)
        {
            Board boardOut = new()
            {
                board = new Piece[64],
                ThreeFold = [],
                BitboardPinned = []
            };
            int y = 0;

            for (int i = 0; i < 64; i++)
            {
                if (fenString[y] == ' ')
                {
                    break;
                }
                if (fenString[y] == '/')
                {
                    y++;
                }
                if (char.IsDigit(fenString[y]))
                {
                    i += Convert.ToInt32(new string (fenString[y], 1)) - 1;
                    y++;
                    continue;
                }
                boardOut.board[i] = Piece.CreatePiece(fenString[y]);
                y++;
            }
            boardOut.IsWhiteToMove = fenString[y + 1] == 'w';

            for (int i = y + 3; i < fenString.Length; i++) //search for first space after w/b (skip castling rights)
            {
                if (fenString[i] == ' ')
                {
                    if (fenString[i + 1] == '-')
                    {
                        break;
                    }
                    boardOut.EnPassentIndex = fenString[i + 1] - 96 + 64 - 8 * Convert.ToInt32(new string(fenString[i + 2], 1)) - 1;
                    break;
                }
            }
            return boardOut;
        }

        private UInt64 GetBiboard()
        {
            IsWhiteToMove = !IsWhiteToMove;
            BitboardAttacked = 0;
            BitboardPinned = [];
            BitboardCheck = 0;
            IsInCheck = false;
            DoubleCheck = false;

            int cap1 = IsWhiteToMove ? -9 : 7;
            int cap2 = IsWhiteToMove ? -7 : 9;

            byte opponentKing = IsWhiteToMove ? (byte)12 : (byte)6;

            for (int i = 0; i < 64; i++)
            {
                if (board[i] == null || board[i].IsWhite != IsWhiteToMove)
                {
                    continue;
                }

                if (board[i].Notation is 1 or 7) //P or p
                {
                    if (i % 8 != 0) //capture
                    {
                        BitboardAttacked |= 1UL << (i + cap1);
                        if (board[i + cap1] != null && board[i + cap1].Notation == opponentKing)
                        {
                            if (IsInCheck) //double check
                            {
                                DoubleCheck = true;
                                IndexOfCheckingPiece = -1;
                            }
                            IndexOfCheckingPiece = i;
                            IsInCheck = true;
                        }
                    }
                    if (i % 8 != 7)
                    {
                        BitboardAttacked |= 1UL << (i + cap2);
                        if (board[i + cap2] != null && board[i + cap2].Notation == opponentKing)
                        {
                            if (IsInCheck) //double check
                            {
                                DoubleCheck = true;
                                IndexOfCheckingPiece = -1;
                            }
                            IndexOfCheckingPiece = i;
                            IsInCheck = true;
                        }
                    }
                    continue;
                }

                for (int k = 0; k < board[i].MoveDelta.Length; k++)
                {
                    int lastJ = i;
                    int moveDelta = board[i].MoveDelta[k];
                    for (int j = i + moveDelta; j < 64 && j >= 0; j += moveDelta)
                    {
                        if (j % 8 == 0 && lastJ % 8 == 7 || j % 8 == 7 && lastJ % 8 == 0 || i % 8 < 2 && moveDelta is -10 or 6 || i % 8 > 5 && moveDelta is 10 or -6) //detect board edge
                        {
                            break;
                        }

                        BitboardAttacked |= 1UL << j;
                        if (board[j] == null)
                        {
                            if (board[i].IsSliding)
                            {
                                lastJ = j;
                                continue;
                            }
                            break;
                        }
                        if (board[j].IsWhite == IsWhiteToMove) //same color piece
                        {
                            break;
                        }
                        if (!board[i].IsSliding && board[j].IsWhite != IsWhiteToMove)
                        {
                            if (board[j].Notation == opponentKing) //Knight check
                            {
                                if (IsInCheck) //double check
                                {
                                    DoubleCheck = true;
                                    IndexOfCheckingPiece = -1;
                                    break;
                                }
                                IsInCheck = true;
                                IndexOfCheckingPiece = i;
                            }
                            break;
                        }

                        //opponent piece (can be pinned)
                        if (board[j].Notation == opponentKing) //Cant pin the king
                        {
                            if (IsInCheck) //double check
                            {
                                DoubleCheck = true;
                                IndexOfCheckingPiece = -1;
                            }
                            else
                            {
                                IndexOfCheckingPiece = i;
                                IsInCheck = true;
                                isPinned(this, i, lastJ, i, moveDelta, true);
                            }
                            lastJ = j;
                            continue;
                        }

                        isPinned(this, i, lastJ, i, moveDelta, false);
                        break;
                    }
                }
            }
            IsWhiteToMove = !IsWhiteToMove;
            return BitboardAttacked;
        }

        private static void isPinned(Board board, int posPinningPiece, int lastJ, int i, int moveDelta, bool isInCheck)
        {
            UInt64 pinMask = 0;
            bool encounteredFirstPiece = false;
            byte opponentKing = board.IsWhiteToMove ? (byte)12 : (byte)6;

            if (!isInCheck)
            {
                pinMask |= 1UL << posPinningPiece; //Pinned piece can take pinning piece
            }

            for (int j = i + moveDelta; j < 64 && j >= 0; j += moveDelta)
            {
                if (j % 8 == 0 && lastJ % 8 == 7 || j % 8 == 7 && lastJ % 8 == 0)
                {
                    break;
                }
                if (isInCheck)
                {
                    if (board.board[j] != null && board.board[j].Notation == opponentKing)
                    {
                        return;
                    }
                    board.BitboardCheck |= 1UL << j;
                    continue;
                }

                if (board.board[j] == null)
                {
                    pinMask |= 1UL << j;
                    lastJ = j;
                    continue;
                }
                if (board.board[j].Notation == opponentKing)
                {
                    board.BitboardPinned.Add(pinMask);
                    break;
                }
                if (!encounteredFirstPiece) //only one piece between attacker and king
                {
                    pinMask |= 1UL << j;
                    lastJ = j;
                    encounteredFirstPiece = true;
                    continue;
                }
                return;
            }
            return;
        }

        private static Move[] GetPseudoLegalMoves(Board board) 
        {
            List<Move> moves = [];
            byte lastCap;

            string promPieces = board.IsWhiteToMove ? "QRBN" : "qrbn";
            int fw = board.IsWhiteToMove ? -8 : 8;
            int cap1 = board.IsWhiteToMove ? -9 : 7;
            int cap2 = board.IsWhiteToMove ? -7 : 9;
            int start = board.IsWhiteToMove ? 6 : 1; // start row
            int prom = board.IsWhiteToMove ? 0 : 7;

            for (int i = 0; i < 64; i++)
            {
                if (board.board[i] == null || board.board[i].IsWhite != board.IsWhiteToMove)
                {
                    continue;
                }

                if (board.board[i].Notation is 1 or 7) //P or p
                {
                    if ((i + fw) / 8 == prom) //Promotion
                    {
                        if (board.board[i + fw] == null)
                        {
                           for (int j = 0; j < 4; j++)
                           {
                               moves.Add(moveHelper(i, i + fw, false, (byte)0, promPieces[j]));
                           }
                        }

                        if (i % 8 != 0 && (board.board[i + cap1] != null && board.board[i + cap1].IsWhite != board.IsWhiteToMove)) //capture
                        {
                            lastCap = board.board[i + cap1].Notation;
                            for (int j = 0; j < 4; j++)
                            {
                                moves.Add(moveHelper(i, i + cap1, true, lastCap, promPieces[j]));
                            }

                        }

                        if (i % 8 != 7 && (board.board[i + cap2] != null && board.board[i + cap2].IsWhite != board.IsWhiteToMove))
                        {
                            lastCap = board.board[i + cap2].Notation;
                            for (int j = 0; j < 4; j++)
                            {
                                moves.Add(moveHelper(i, i + cap2, true, lastCap, promPieces[j]));
                            }
                        }
                        continue;
                    }

                    if (board.board[i + fw] == null) //move one forward
                    {
                        moves.Add(moveHelper(i, i + fw, false, (byte)0, '\0'));

                        if (i / 8 == start && board.board[i + fw * 2] == null) 
                        {
                            Move move = moveHelper(i, i + fw * 2, false, (byte)0, '\0');
                            move.EnPassentIndex = i + fw;
                            moves.Add(move);
                        }
                    }

                    if (i % 8 != 0 && (board.board[i+cap1] != null && board.board[i + cap1].IsWhite != board.IsWhiteToMove || (i + cap1 == board.EnPassentIndex && i / 8 != start))) //capture
                    {
                        lastCap = board.board[i + cap1] != null ? board.board[i + cap1].Notation : (byte)0;
                        moves.Add(moveHelper(i, i + cap1, true, lastCap, '\0'));
                    }

                    if (i % 8 != 7 && (board.board[i + cap2] != null && board.board[i + cap2].IsWhite != board.IsWhiteToMove || (i + cap2 == board.EnPassentIndex && i / 8 != start)))
                    {
                        lastCap = board.board[i + cap2] != null ? board.board[i + cap2].Notation : (byte)0;
                        moves.Add(moveHelper(i, i + cap2, true, lastCap, '\0'));
                    }
                    continue;
                }

                for (int k = 0; k < board.board[i].MoveDelta.Length; k++)
                {
                    int lastJ = i;
                    int moveDelta = board.board[i].MoveDelta[k];
                    for (int j = i + moveDelta; j < 64 && j >= 0; j += moveDelta)
                    {
                        if (j % 8 == 0 && lastJ % 8 == 7 || j % 8 == 7 && lastJ % 8 == 0 || i % 8 < 2 && moveDelta is -10 or 6 || i % 8 > 5 && moveDelta is 10 or -6) //detect board edge
                        {
                            break;
                        }

                        if (board.board[j] == null)
                        {
                            moves.Add(moveHelper(i, j, false, (byte)0, '\0'));

                            if (board.board[i].IsSliding)
                            {
                                lastJ = j;
                                continue;
                            }
                            break;
                        }

                        if (board.board[j].IsWhite != board.IsWhiteToMove)
                        {
                            lastCap = board.board[j].Notation;
                            moves.Add(moveHelper(i, j, true, lastCap, '\0'));
                        }
                        break;
                    }
                }
            }
            return [.. moves];
        }

        public static Move[] GetLegalMoves(Board board)
        {
            Move[] moves = GetPseudoLegalMoves(board);
            List<Move> legalMoves = [];
            UInt64 bitboard = board.GetBiboard();
            int indexOfCheckingPiece = board.IndexOfCheckingPiece;
            UInt64 bitboardCheck = board.BitboardCheck;
            List<UInt64> pinMasks = board.BitboardPinned;
            UInt64 curKingMask = 0;
            bool isInCheck = board.IsInCheck;
            bool doubleCheck = board.DoubleCheck;
            byte kingNotation = board.IsWhiteToMove ? (byte)6 : (byte)12;

            for (int i = 0; i < 64; i++)
            {
                if (board.board[i] != null && board.board[i].Notation == kingNotation) //K or k
                {
                    curKingMask = 1UL << i;
                    break;
                }
            }

            foreach (Move m in moves)
            {
                UInt64 kingMask = curKingMask;
                if (doubleCheck && board.board[m.Index1].Notation is not 6 and not 12) //only king moves allowed
                {
                    continue;
                }

                foreach (var u in pinMasks)
                {
                    if ((1UL << m.Index1 & u) != 0 && (1UL << m.Index2 & u) == 0) //pinned piece moved outside pin line
                    {
                        goto endLoop;
                    }
                }

                if (isInCheck) //take checking piece or block check
                {
                    if ((m.Index2 == indexOfCheckingPiece || (bitboardCheck & 1UL << m.Index2) != 0) && board.board[m.Index1].Notation != kingNotation)
                    {
                        legalMoves.Add(m);
                        continue;
                    }
                }

                if (m.IsCapture && m.LastCapture == 0) //en passent capture
                {
                    int indexOfCapturedPawn = !board.IsWhiteToMove ? m.Index2 + 8 : m.Index2 - 8;
                    foreach (var u in pinMasks) //chek if captured pawn was pinned
                    {
                        if ((1UL << indexOfCapturedPawn & u) != 0)
                        {
                            break;
                        }
                    }
                    board.DoMove(m);
                    board.IsWhiteToMove = !board.IsWhiteToMove;
                    UInt64 bitboardEp = board.GetBiboard(); //check if king is check after en passent capture
                    board.IsWhiteToMove = !board.IsWhiteToMove;

                    if ((kingMask & bitboardEp) == 0)
                    {
                        legalMoves.Add(m);
                    }
                    board.UndoMove(m);
                    continue;
                }

                board.DoMove(m);

                if (board.board[m.Index2].Notation == kingNotation)
                {
                    kingMask = 1UL << m.Index2;
                }

                if ((kingMask & bitboard) == 0)
                {
                    legalMoves.Add(m);
                }

                board.UndoMove(m);
                endLoop:;
            }
            return [.. legalMoves];
        }

        public void DoMove(Move move)
        { 
            int index1 = move.Index1;
            int index2 = move.Index2;

            board[index2] = board[index1];
            board[index1] = null;

            if (board[index2].Material == 0)
            {
                if (index1 == 60 || index1 == 4)
                {
                    int rookMove = 0;
                    char rook;
                    if (IsWhiteToMove)
                    {
                        rook = 'R';
                        if (index2 == 62)
                        {
                            board[63] = null;
                            rookMove = 61;
                        }
                        else if (index2 == 58)
                        {
                            board[56] = null;
                            rookMove = 59;
                        }
                    }
                    else
                    {
                        rook = 'r';
                        if(index2 == 6)
                        {
                            board[7] = null;
                            rookMove = 5;
                        }
                        else if (index2 == 2)
                        {
                            board[0] = null;
                            rookMove = 3;
                        }
                    }
                    if (rookMove != 0)
                    {
                        board[rookMove] = Piece.CreatePiece(rook);
                    }
                }
            }

            if (move.PromPiece != 0)
            {
                board[index2] = Piece.CreatePiece(move.PromPiece);
            }

            EnPassentIndex = move.EnPassentIndex;

            if (move.IsCapture && move.LastCapture == 0)
            {
                if(IsWhiteToMove)
                {
                    board[index2 + 8] = null;
                }
                else
                {
                    board[index2 - 8] = null;
                }
            }
            IsWhiteToMove = !IsWhiteToMove;
        }

        public void UndoMove(Move move)
        {
            int index2 = move.Index1;
            int index1 = move.Index2;

            board[index2] = board[index1];
            board[index1] = null;
            if (move.IsCapture)
            {
                if (move.LastCapture != 0)
                {
                    board[index1] = Piece.CreatePiece(Piece.byteToChar(move.LastCapture));
                }
                else
                {
                    int pawnIndex = IsWhiteToMove ? index1 - 8 : index1 + 8;
                    board[pawnIndex] = Piece.CreatePiece(board[index2].IsWhite ? 'p' : 'P');
                }
            }

            if (move.PromPiece != 0)
            {
                board[index2] = Piece.CreatePiece(IsWhiteToMove ? 'p' : 'P');
            }

            move.EnPassentIndex = EnPassentIndex;

            IsWhiteToMove = !IsWhiteToMove;
        }

        private static Move moveHelper(int index1, int index2, bool isCapture, byte lastCapture, char promPiece)
        {
            Move move = new()
            {
                Notation = LookupTables.indexToPos[index1] + LookupTables.indexToPos[index2],
                Index1 = index1,
                Index2 = index2,
                IsCapture = isCapture,
                LastCapture = lastCapture,
            };
            if (promPiece != '\0')
            {
                move.Notation += char.ToLower(promPiece);
                move.PromPiece = promPiece;
            }

            return move;
        }

        public void PrintBoard()
        {
            for (int i = 0; i < board.Length; i++)
            {
                if (i % 8 == 0)
                {
                    Console.WriteLine(" ");
                }

                if (board[i] == null)
                {
                    Console.Write("- ");
                    continue;
                }
                Console.Write(Piece.byteToChar(board[i].Notation) + " ");
            }
            Console.WriteLine(" \n");
        }

        public static void PrintBitboard(UInt64 bitboard)
        {
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    Console.WriteLine(" ");
                }

                UInt64 x = 1UL << i;
                x &= bitboard;

                if (x == 0)
                {
                    Console.Write("- ");
                    continue;
                }
                Console.Write("# ");
            }
            Console.WriteLine(" \n");
        }

    }
}
