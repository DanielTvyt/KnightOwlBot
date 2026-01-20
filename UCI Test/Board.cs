using System;
using System.Collections.Generic;
using System.Text;

namespace KnightOwlBot
{
    internal class Board
    {
        private const int MAXLEGALMOVES = 213;
        private Move[] moves = new Move[MAXLEGALMOVES];

        public Piece[] board;
        public bool IsWhiteToMove;
        public int EnPassentIndex;
        public bool IsInCheck;
        public bool DoubleCheck;
        public int IndexOfCheckingPiece;
        public UInt64 BitboardAttacked;
        public UInt64 BitboardCheck;
        public List<UInt64> BitboardPinned;
        public bool[] CastlingRights;
        public int pieceCount;

        public ZobristHashing zobrist = new();

        public Board(string fenString)
        {
            board = new Piece[64];
            BitboardPinned = [];
            CastlingRights = [false, false, false, false]; //KQkq
            EnPassentIndex = 100;
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
                board[i] = new Piece(fenString[y]);
                pieceCount++;
                y++;
            }
            IsWhiteToMove = fenString[y + 1] == 'w';

            for (int i = y + 3; fenString[i] != ' '; i++)
            {
                y = i;
                switch (fenString[i])
                {
                    case 'K':
                        CastlingRights[0] = true;
                        break;
                    case 'Q':
                        CastlingRights[1] = true;
                        break;
                    case 'k':
                        CastlingRights[2] = true;
                        break;
                    case 'q':
                        CastlingRights[3] = true;
                        break;
                }
            }
            if (fenString[y + 2] != '-')
            {
                Console.WriteLine(fenString[y + 2] + " " + fenString[y + 3]);
                EnPassentIndex = fenString[y + 2] - 96 + 64 - 8 * Convert.ToInt32(new string(fenString[y + 3], 1)) - 1;
            }
            zobrist.InitializeHash(this);
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

            byte opponentKing = IsWhiteToMove ? Piece.k : Piece.K;

            for (int i = 0; i < 64; i++)
            {
                if (board[i] == null || board[i].IsWhite != IsWhiteToMove)
                {
                    continue;
                }

                if (board[i].Notation is Piece.P or Piece.p) //P or p
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
            byte opponentKing = board.IsWhiteToMove ? Piece.k : Piece.K;

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

        private void GetPseudoLegalMoves() 
        {
            int moveCount = 0;
            Piece lastCap;

            int castleK = IsWhiteToMove ? 0 : 2;
            int castleQ = IsWhiteToMove ? 1 : 3;
            CalculateCastlingRights();

            int fw = IsWhiteToMove ? -8 : 8;
            int cap1 = IsWhiteToMove ? -9 : 7;
            int cap2 = IsWhiteToMove ? -7 : 9;
            int start = IsWhiteToMove ? 6 : 1; // start row
            int prom = IsWhiteToMove ? 0 : 7;

            for (int i = 0; i < 64; i++)
            {
                if (board[i] == null || board[i].IsWhite != IsWhiteToMove)
                {
                    continue;
                }

                if (board[i].Notation is Piece.P or Piece.p) //P or p
                {
                    if ((i + fw) / 8 == prom) //Promotion
                    {
                        char[] promPieces = IsWhiteToMove ? ['Q', 'R', 'B', 'N'] : ['q', 'r', 'b', 'n'];
                        if (board[i + fw] == null)
                        {
                           for (int j = 0; j < 4; j++)
                           {
                               moves[moveCount++] = new Move(i, i + fw, false, null, 100, promPieces[j]);
                           }
                        }

                        if (i % 8 != 0 && (board[i + cap1] != null && board[i + cap1].IsWhite != IsWhiteToMove)) //capture
                        {
                            lastCap = board[i + cap1];
                            for (int j = 0; j < 4; j++)
                            {
                                moves[moveCount++] = new Move(i, i + cap1, true, lastCap, 100, promPieces[j]);
                            }

                        }

                        if (i % 8 != 7 && (board[i + cap2] != null && board[i + cap2].IsWhite != IsWhiteToMove))
                        {
                            lastCap = board[i + cap2];
                            for (int j = 0; j < 4; j++)
                            {
                                moves[moveCount++] = new Move(i, i + cap2, true, lastCap, 100, promPieces[j]);
                            }
                        }
                        continue;
                    }

                    if (board[i + fw] == null) //move one forward
                    {
                        moves[moveCount++] = new Move(i, i + fw);

                        if (i / 8 == start && board[i + fw * 2] == null) 
                        {
                            moves[moveCount++] = new Move(i, (byte)(i + fw * 2), false, null, i + fw);
                        }
                    }

                    if (i % 8 != 0 && (board[i+cap1] != null && board[i + cap1].IsWhite != IsWhiteToMove || (i + cap1 == EnPassentIndex && i / 8 != start))) //capture
                    {
                        lastCap = board[i + cap1];
                        moves[moveCount++] = new Move(i, i + cap1, true, lastCap);
                    }

                    if (i % 8 != 7 && (board[i + cap2] != null && board[i + cap2].IsWhite != IsWhiteToMove || (i + cap2 == EnPassentIndex && i / 8 != start)))
                    {
                        lastCap = board[i + cap2];
                        moves[moveCount++] = new Move(i, i + cap2, true, lastCap);
                    }
                    continue;
                }

                if (board[i].Notation is Piece.K or Piece.k) //K or k
                {
                    //Castling
                    if (CastlingRights[castleK] && board[i + 1] == null && board[i + 2] == null)
                    {
                        Move m = new(i, i + 2) { IsCastleMove = true };
                        moves[moveCount++] = m;
                    }
                    if (CastlingRights[castleQ] && board[i - 1] == null && board[i - 2] == null && board[i - 3] == null)
                    {
                        Move m = new(i, i - 2) { IsCastleMove = true };
                        moves[moveCount++] = m;
                    }
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

                        if (board[j] == null)
                        {
                            moves[moveCount++] = new Move(i, j);

                            if (board[i].IsSliding)
                            {
                                lastJ = j;
                                continue;
                            }
                            break;
                        }

                        if (board[j].IsWhite != IsWhiteToMove)
                        {
                            lastCap = board[j];
                            moves[moveCount++] = new Move(i, j, true, lastCap);
                        }
                        break;
                    }
                }
            }
            moves[moveCount] = null; //end marker
        }

        public static Move[] GetLegalMoves(Board board)
        {
            board.GetPseudoLegalMoves();
            List<Move> legalMoves = [];
            UInt64 bitboard = board.GetBiboard();
            int indexOfCheckingPiece = board.IndexOfCheckingPiece;
            UInt64 bitboardCheck = board.BitboardCheck;
            List<UInt64> pinMasks = board.BitboardPinned;
            UInt64 curKingMask = 0;
            bool isInCheck = board.IsInCheck;
            bool doubleCheck = board.DoubleCheck;
            byte kingNotation = board.IsWhiteToMove ? Piece.K : Piece.k;

            for (int i = 0; i < 64; i++)
            {
                if (board.board[i] != null && board.board[i].Notation == kingNotation) //K or k
                {
                    curKingMask = 1UL << i;
                    break;
                }
            }

            foreach (Move m in board.moves)
            {
                if (m == null)
                {
                    break;
                }
                UInt64 kingMask = curKingMask;
                if (doubleCheck && board.board[m.Index1].Notation is not Piece.K and not Piece.k) //only king moves allowed
                {
                    continue;
                }

                if (m.IsCastleMove)
                {
                    if (isInCheck)
                    {
                        continue;
                    }
                    UInt64 castleMask;
                    if (m.Index2 > m.Index1) //king side
                    {
                        castleMask = (1UL << (m.Index1 + 1)) | (1UL << (m.Index1 + 2));
                    }
                    else //queen side
                    {
                        castleMask = (1UL << (m.Index1 - 1)) | (1UL << (m.Index1 - 2));
                    }
                    if ((castleMask & bitboard) == 0)
                    {
                        legalMoves.Add(m);
                    }
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

                if (m.IsCapture && m.LastCapture == null) //en passent capture
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

            byte pieceNotation = board[index2].Notation;

            if (board[index2].Material == 0)
            {
                if (index1 == 60 || index1 == 4)
                {
                    int rookMove = 0;
                    int rookPos = 0;
                    if (IsWhiteToMove)
                    {
                        if (index2 == 62)
                        {
                            rookMove = 61;
                            rookPos = 63;
                        }
                        else if (index2 == 58)
                        {
                            rookMove = 59;
                            rookPos = 56;
                        }
                    }
                    else
                    {
                        if(index2 == 6)
                        {
                            rookMove = 5;
                            rookPos = 7;
                        }
                        else if (index2 == 2)
                        {
                            rookMove = 3;
                            rookPos = 0;
                        }
                    }
                    if (rookMove != 0)
                    {
                        board[rookMove] = board[rookPos];
                        board[rookPos] = null;
                        zobrist.UpdateHash(board[rookMove].Notation, rookMove);
                        zobrist.UpdateHash(board[rookMove].Notation, rookPos);
                        move.IsCastleMove = true;
                    }
                }
            }

            if (move.PromPiece != 0)
            {
                board[index2] = new Piece(move.PromPiece);
            }

            // store previous en-passant index so it can be restored on undo
            move.PrevEnPassentIndex = EnPassentIndex;
            EnPassentIndex = move.EnPassentIndex;

            if (move.IsCapture)
            {
                pieceCount--;
                if (move.LastCapture == null)
                {
                    int pawnIndex = IsWhiteToMove ? index2 + 8 : index2 - 8;
                    zobrist.UpdateHash(board[pawnIndex].Notation, pawnIndex);
                    board[pawnIndex] = null;
                }
                else
                {
                    zobrist.UpdateHash(move.LastCapture.Notation, move.Index2);
                }
            }
            if (!board[index2].hasMoved)
            {
                move.IsFirstMove = true;
                board[index2].hasMoved = true;
            }
            IsWhiteToMove = !IsWhiteToMove;

            zobrist.UpdateHashMove(pieceNotation, index1, board[index2].Notation, index2, EnPassentIndex, move.PrevEnPassentIndex);
        }

        public void UndoMove(Move move)
        {
            int index2 = move.Index1;
            int index1 = move.Index2;

            board[index2] = board[index1];
            board[index1] = null;

            byte pieceNotation = board[index2].Notation;

            if (move.IsCapture)
            {
                pieceCount++;
                if (move.LastCapture != null)
                {
                    board[index1] = move.LastCapture;
                    zobrist.UpdateHash(board[index1].Notation, index1);
                }
                else
                {
                    int pawnIndex = IsWhiteToMove ? index1 - 8 : index1 + 8;
                    board[pawnIndex] = new Piece(board[index2].IsWhite ? 'p' : 'P');
                    zobrist.UpdateHash(board[pawnIndex].Notation, pawnIndex);
                }
            }

            if (move.PromPiece != '\0')
            {
                board[index2] = new Piece(IsWhiteToMove ? 'p' : 'P');
            }

            if (move.IsFirstMove)
            {
                board[index2].hasMoved = false;
            }

            if (move.IsCastleMove)
            {
                int rookStart, rookEnd;
                if (index1 > index2) //king side
                {
                    rookStart = !IsWhiteToMove ? 63 : 7;
                    rookEnd = index1 - 1;
                }
                else //queen side
                {
                    rookStart = !IsWhiteToMove ? 56 : 0;
                    rookEnd = index1 + 1;
                }
                board[rookStart] = board[rookEnd];
                board[rookEnd] = null;
                board[rookStart].hasMoved = false;
                zobrist.UpdateHash(board[rookStart].Notation, rookStart);
                zobrist.UpdateHash(board[rookStart].Notation, rookEnd);
            }

            // restore previous en-passant index
            EnPassentIndex = move.PrevEnPassentIndex;
            IsWhiteToMove = !IsWhiteToMove;

            zobrist.UpdateHashMove(pieceNotation, index1, board[index2].Notation, index2, EnPassentIndex, move.EnPassentIndex);
        }

        public void CalculateCastlingRights()
        {
            zobrist.UpdateHashCastling(CastlingRights);

            CastlingRights = [true, true, true, true];

            if (board[60] != null && board[60].Notation == Piece.K && !board[60].hasMoved)
            {
                CastlingRights[0] = board[63] != null && board[63].Notation == Piece.R && !board[63].hasMoved; //white king side

                CastlingRights[1] = board[56] != null && board[56].Notation == Piece.R && !board[56].hasMoved; //white queen side
            }
            else
            {
                CastlingRights[0] = false;
                CastlingRights[1] = false;
            }
            if (board[4] != null && board[4].Notation == Piece.k && !board[4].hasMoved)
            {
                CastlingRights[2] = board[7] != null && board[7].Notation == Piece.r && !board[7].hasMoved; //black king side

                CastlingRights[3] = board[0] != null && board[0].Notation == Piece.r && !board[0].hasMoved; //black queen side
            }
            else
            {
                CastlingRights[2] = false;
                CastlingRights[3] = false;
            }

            zobrist.UpdateHashCastling(CastlingRights);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var piece in board)
            {
                if (piece == null)
                {
                    sb.Append("-");
                    continue;
                }
                sb.Append(Piece.byteToChar(piece.Notation));
            }
            foreach (var right in CastlingRights)
            {
                sb.Append(right ? "1" : "0");
            }
            sb.Append(IsWhiteToMove ? "w" : "b");
            return sb.ToString();
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
            CalculateCastlingRights();
            Console.Write("\nKQkq ");
            foreach (var right in CastlingRights)
            {
                Console.Write(right + " ");
            }
            Console.WriteLine(" \n" + "Hash: " + zobrist.HashValue + "\n");
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
