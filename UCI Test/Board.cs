using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace KnightOwlBot
{
    internal class Board
    {
        public Piece[] board;
        public bool isWhiteToMove;
        public int enPassentIndex;
        public bool isInCheck;
        public bool doubleCheck;
        public int indexOfCheckingPiece;
        public UInt64 bitboardAttacked;
        public UInt64 bitboardCheck;
        public List<UInt64> bitboardPinned;
        public bool[] castlingRights;
        public int pieceCount;

        public readonly ZobristHashing zobrist = new();

        private const int MAXLEGALMOVES = 213;
        private readonly Move[] moves = new Move[MAXLEGALMOVES];
        private readonly List<ulong> repetitionHistory;

        public Board(string fenString)
        {
            board = new Piece[64];
            bitboardPinned = [];
            castlingRights = [false, false, false, false]; //KQkq
            enPassentIndex = 100;
            repetitionHistory = [];
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
                    i += Convert.ToInt32(new string(fenString[y], 1)) - 1;
                    y++;
                    continue;
                }
                board[i] = new Piece(fenString[y]);
                pieceCount++;
                y++;
            }
            isWhiteToMove = fenString[y + 1] == 'w';

            for (int i = y + 3; fenString[i] != ' '; i++)
            {
                y = i;
                switch (fenString[i])
                {
                    case 'K':
                        castlingRights[0] = true;
                        break;
                    case 'Q':
                        castlingRights[1] = true;
                        break;
                    case 'k':
                        castlingRights[2] = true;
                        break;
                    case 'q':
                        castlingRights[3] = true;
                        break;
                }
            }
            if (fenString[y + 2] != '-')
            {
                Console.WriteLine(fenString[y + 2] + " " + fenString[y + 3]);
                enPassentIndex = fenString[y + 2] - 96 + 64 - 8 * Convert.ToInt32(new string(fenString[y + 3], 1)) - 1;
            }
            zobrist.InitializeHash(this);
            repetitionHistory.Add(zobrist.hashValue);
        }

        private UInt64 GetBiboard()
        {
            isWhiteToMove = !isWhiteToMove;
            bitboardAttacked = 0;
            bitboardPinned = [];
            bitboardCheck = 0;
            isInCheck = false;
            doubleCheck = false;

            int cap1 = isWhiteToMove ? -9 : 7;
            int cap2 = isWhiteToMove ? -7 : 9;

            byte opponentKing = isWhiteToMove ? Piece.k : Piece.K;

            for (int i = 0; i < 64; i++)
            {
                if (board[i] == null || board[i].isWhite != isWhiteToMove)
                {
                    continue;
                }

                if (board[i].notation is Piece.P or Piece.p) //P or p
                {
                    if (i % 8 != 0) //capture
                    {
                        bitboardAttacked |= 1UL << (i + cap1);
                        if (board[i + cap1] != null && board[i + cap1].notation == opponentKing)
                        {
                            if (isInCheck) //double check
                            {
                                doubleCheck = true;
                                indexOfCheckingPiece = -1;
                            }
                            indexOfCheckingPiece = i;
                            isInCheck = true;
                        }
                    }
                    if (i % 8 != 7)
                    {
                        bitboardAttacked |= 1UL << (i + cap2);
                        if (board[i + cap2] != null && board[i + cap2].notation == opponentKing)
                        {
                            if (isInCheck) //double check
                            {
                                doubleCheck = true;
                                indexOfCheckingPiece = -1;
                            }
                            indexOfCheckingPiece = i;
                            isInCheck = true;
                        }
                    }
                    continue;
                }

                for (int k = 0; k < board[i].moveDelta.Length; k++)
                {
                    int lastJ = i;
                    int moveDelta = board[i].moveDelta[k];
                    for (int j = i + moveDelta; j < 64 && j >= 0; j += moveDelta)
                    {
                        if (j % 8 == 0 && lastJ % 8 == 7 || j % 8 == 7 && lastJ % 8 == 0 || i % 8 < 2 && moveDelta is -10 or 6 || i % 8 > 5 && moveDelta is 10 or -6) //detect board edge
                        {
                            break;
                        }

                        bitboardAttacked |= 1UL << j;
                        if (board[j] == null)
                        {
                            if (board[i].isSliding)
                            {
                                lastJ = j;
                                continue;
                            }
                            break;
                        }
                        if (board[j].isWhite == isWhiteToMove) //same color piece
                        {
                            break;
                        }
                        if (!board[i].isSliding && board[j].isWhite != isWhiteToMove)
                        {
                            if (board[j].notation == opponentKing) //Knight check
                            {
                                if (isInCheck) //double check
                                {
                                    doubleCheck = true;
                                    indexOfCheckingPiece = -1;
                                    break;
                                }
                                isInCheck = true;
                                indexOfCheckingPiece = i;
                            }
                            break;
                        }

                        //opponent piece (can be pinned)
                        if (board[j].notation == opponentKing) //Cant pin the king
                        {
                            if (isInCheck) //double check
                            {
                                doubleCheck = true;
                                indexOfCheckingPiece = -1;
                            }
                            else
                            {
                                indexOfCheckingPiece = i;
                                isInCheck = true;
                                IsPinned(this, i, lastJ, i, moveDelta, true);
                            }
                            lastJ = j;
                            continue;
                        }

                        IsPinned(this, i, lastJ, i, moveDelta, false);
                        break;
                    }
                }
            }
            isWhiteToMove = !isWhiteToMove;
            return bitboardAttacked;
        }

        private static void IsPinned(Board board, int posPinningPiece, int lastJ, int i, int moveDelta, bool isInCheck)
        {
            UInt64 pinMask = 0;
            bool encounteredFirstPiece = false;
            byte opponentKing = board.isWhiteToMove ? Piece.k : Piece.K;

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
                    if (board.board[j] != null && board.board[j].notation == opponentKing)
                    {
                        return;
                    }
                    board.bitboardCheck |= 1UL << j;
                    continue;
                }

                if (board.board[j] == null)
                {
                    pinMask |= 1UL << j;
                    lastJ = j;
                    continue;
                }
                if (board.board[j].notation == opponentKing)
                {
                    board.bitboardPinned.Add(pinMask);
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

            int castleK = isWhiteToMove ? 0 : 2;
            int castleQ = isWhiteToMove ? 1 : 3;

            int fw = isWhiteToMove ? -8 : 8;
            int cap1 = isWhiteToMove ? -9 : 7;
            int cap2 = isWhiteToMove ? -7 : 9;
            int start = isWhiteToMove ? 6 : 1; // start row
            int prom = isWhiteToMove ? 0 : 7;

            for (int i = 0; i < 64; i++)
            {
                if (board[i] == null || board[i].isWhite != isWhiteToMove)
                {
                    continue;
                }

                if (board[i].notation is Piece.P or Piece.p) //P or p
                {
                    if ((i + fw) / 8 == prom) //Promotion
                    {
                        char[] promPieces = isWhiteToMove ? ['Q', 'R', 'B', 'N'] : ['q', 'r', 'b', 'n'];
                        if (board[i + fw] == null)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                moves[moveCount++] = new Move(i, i + fw, false, null, 100, promPieces[j]);
                            }
                        }

                        if (i % 8 != 0 && (board[i + cap1] != null && board[i + cap1].isWhite != isWhiteToMove)) //capture
                        {
                            lastCap = board[i + cap1];
                            for (int j = 0; j < 4; j++)
                            {
                                moves[moveCount++] = new Move(i, i + cap1, true, lastCap, 100, promPieces[j]);
                            }

                        }

                        if (i % 8 != 7 && (board[i + cap2] != null && board[i + cap2].isWhite != isWhiteToMove))
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

                    if (i % 8 != 0 && (board[i + cap1] != null && board[i + cap1].isWhite != isWhiteToMove || (i + cap1 == enPassentIndex && i / 8 != start))) //capture
                    {
                        lastCap = board[i + cap1];
                        moves[moveCount++] = new Move(i, i + cap1, true, lastCap);
                    }

                    if (i % 8 != 7 && (board[i + cap2] != null && board[i + cap2].isWhite != isWhiteToMove || (i + cap2 == enPassentIndex && i / 8 != start)))
                    {
                        lastCap = board[i + cap2];
                        moves[moveCount++] = new Move(i, i + cap2, true, lastCap);
                    }
                    continue;
                }

                if (board[i].notation is Piece.K or Piece.k) //K or k
                {
                    //Castling
                    if (castlingRights[castleK] && board[i + 1] == null && board[i + 2] == null)
                    {
                        Move m = new(i, i + 2) { isCastleMove = true };
                        moves[moveCount++] = m;
                    }
                    if (castlingRights[castleQ] && board[i - 1] == null && board[i - 2] == null && board[i - 3] == null)
                    {
                        Move m = new(i, i - 2) { isCastleMove = true };
                        moves[moveCount++] = m;
                    }
                }

                for (int k = 0; k < board[i].moveDelta.Length; k++)
                {
                    int lastJ = i;
                    int moveDelta = board[i].moveDelta[k];
                    for (int j = i + moveDelta; j < 64 && j >= 0; j += moveDelta)
                    {
                        if (j % 8 == 0 && lastJ % 8 == 7 || j % 8 == 7 && lastJ % 8 == 0 || i % 8 < 2 && moveDelta is -10 or 6 || i % 8 > 5 && moveDelta is 10 or -6) //detect board edge
                        {
                            break;
                        }

                        if (board[j] == null)
                        {
                            moves[moveCount++] = new Move(i, j);

                            if (board[i].isSliding)
                            {
                                lastJ = j;
                                continue;
                            }
                            break;
                        }

                        if (board[j].isWhite != isWhiteToMove)
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
            int indexOfCheckingPiece = board.indexOfCheckingPiece;
            UInt64 bitboardCheck = board.bitboardCheck;
            List<UInt64> pinMasks = board.bitboardPinned;
            UInt64 curKingMask = 0;
            bool isInCheck = board.isInCheck;
            bool doubleCheck = board.doubleCheck;
            byte kingNotation = board.isWhiteToMove ? Piece.K : Piece.k;

            for (int i = 0; i < 64; i++)
            {
                if (board.board[i] != null && board.board[i].notation == kingNotation) //K or k
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
                if (doubleCheck && board.board[m.index1].notation is not Piece.K and not Piece.k) //only king moves allowed
                {
                    continue;
                }

                if (m.isCastleMove)
                {
                    if (isInCheck)
                    {
                        continue;
                    }
                    UInt64 castleMask;
                    if (m.index2 > m.index1) //king side
                    {
                        castleMask = (1UL << (m.index1 + 1)) | (1UL << (m.index1 + 2));
                    }
                    else //queen side
                    {
                        castleMask = (1UL << (m.index1 - 1)) | (1UL << (m.index1 - 2));
                    }
                    if ((castleMask & bitboard) == 0)
                    {
                        legalMoves.Add(m);
                    }
                    continue;
                }

                foreach (var u in pinMasks)
                {
                    if ((1UL << m.index1 & u) != 0 && (1UL << m.index2 & u) == 0) //pinned piece moved outside pin line
                    {
                        goto endLoop;
                    }
                }

                if (isInCheck) //take checking piece or block check
                {
                    if ((m.index2 == indexOfCheckingPiece || (bitboardCheck & 1UL << m.index2) != 0) && board.board[m.index1].notation != kingNotation)
                    {
                        legalMoves.Add(m);
                        continue;
                    }
                }

                if (m.isCapture && m.lastCapture == null) //en passent capture
                {
                    int indexOfCapturedPawn = !board.isWhiteToMove ? m.index2 + 8 : m.index2 - 8;
                    foreach (var u in pinMasks) //chek if captured pawn was pinned
                    {
                        if ((1UL << indexOfCapturedPawn & u) != 0)
                        {
                            break;
                        }
                    }
                    board.DoMove(m);
                    board.isWhiteToMove = !board.isWhiteToMove;
                    UInt64 bitboardEp = board.GetBiboard(); //check if king is check after en passent capture
                    board.isWhiteToMove = !board.isWhiteToMove;

                    if ((kingMask & bitboardEp) == 0)
                    {
                        legalMoves.Add(m);
                    }
                    board.UndoMove(m);
                    continue;
                }

                board.DoMove(m);

                if (board.board[m.index2].notation == kingNotation)
                {
                    kingMask = 1UL << m.index2;
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
            int index1 = move.index1;
            int index2 = move.index2;

            board[index2] = board[index1];
            board[index1] = null;

            byte pieceNotation = board[index2].notation;

            if (board[index2].material == 0)
            {
                if (index1 == 60 || index1 == 4)
                {
                    int rookMove = 0;
                    int rookPos = 0;
                    if (isWhiteToMove)
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
                        if (index2 == 6)
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
                        zobrist.UpdateHash(board[rookMove].notation, rookMove);
                        zobrist.UpdateHash(board[rookMove].notation, rookPos);
                        move.isCastleMove = true;
                    }
                }
            }

            if (move.promPiece != 0)
            {
                board[index2] = new Piece(move.promPiece);
            }

            // store previous en-passant index so it can be restored on undo
            move.prevEnPassentIndex = enPassentIndex;
            enPassentIndex = move.enPassentIndex;

            if (move.isCapture)
            {
                pieceCount--;
                if (move.lastCapture == null)
                {
                    int pawnIndex = isWhiteToMove ? index2 + 8 : index2 - 8;
                    zobrist.UpdateHash(board[pawnIndex].notation, pawnIndex);
                    board[pawnIndex] = null;
                }
                else
                {
                    zobrist.UpdateHash(move.lastCapture.notation, move.index2);
                    if (move.lastCapture.notation is Piece.R or Piece.r)
                        CalculateCastlingRights();
                }
            }
            if (!board[index2].hasMoved)
            {
                move.isFirstMove = true;
                board[index2].hasMoved = true;
                if (board[index2].notation is Piece.K or Piece.k or Piece.R or Piece.r)
                    CalculateCastlingRights();
            }
            isWhiteToMove = !isWhiteToMove;

            zobrist.UpdateHashMove(pieceNotation, index1, board[index2].notation, index2, enPassentIndex, move.prevEnPassentIndex);

            repetitionHistory.Add(zobrist.hashValue);
        }

        public void UndoMove(Move move)
        {
            int index2 = move.index1;
            int index1 = move.index2;

            board[index2] = board[index1];
            board[index1] = null;

            byte pieceNotation = board[index2].notation;

            if (move.isCapture)
            {
                pieceCount++;
                if (move.lastCapture != null)
                {
                    board[index1] = move.lastCapture;
                    zobrist.UpdateHash(board[index1].notation, index1);
                    if (board[index1].notation is Piece.R or Piece.r)
                        CalculateCastlingRights();
                }
                else
                {
                    int pawnIndex = isWhiteToMove ? index1 - 8 : index1 + 8;
                    board[pawnIndex] = new Piece(board[index2].isWhite ? 'p' : 'P');
                    zobrist.UpdateHash(board[pawnIndex].notation, pawnIndex);
                }
            }

            if (move.promPiece != '\0')
            {
                board[index2] = new Piece(isWhiteToMove ? 'p' : 'P');
            }

            if (move.isFirstMove)
            {
                board[index2].hasMoved = false;
                if (board[index2].notation is Piece.K or Piece.k or Piece.R or Piece.r)
                    CalculateCastlingRights();
            }

            if (move.isCastleMove)
            {
                int rookStart, rookEnd;
                if (index1 > index2) //king side
                {
                    rookStart = !isWhiteToMove ? 63 : 7;
                    rookEnd = index1 - 1;
                }
                else //queen side
                {
                    rookStart = !isWhiteToMove ? 56 : 0;
                    rookEnd = index1 + 1;
                }
                board[rookStart] = board[rookEnd];
                board[rookEnd] = null;
                board[rookStart].hasMoved = false;
                zobrist.UpdateHash(board[rookStart].notation, rookStart);
                zobrist.UpdateHash(board[rookStart].notation, rookEnd);
            }

            // restore previous en-passant index
            enPassentIndex = move.prevEnPassentIndex;
            isWhiteToMove = !isWhiteToMove;

            zobrist.UpdateHashMove(pieceNotation, index1, board[index2].notation, index2, enPassentIndex, move.enPassentIndex);

            repetitionHistory.RemoveAt(repetitionHistory.Count - 1);
        }

        public void CalculateCastlingRights()
        {
            bool[] curCastlingRights = [true, true, true, true];

            if (board[60] != null && board[60].notation == Piece.K && !board[60].hasMoved)
            {
                curCastlingRights[0] = board[63] != null && board[63].notation == Piece.R && !board[63].hasMoved; //white king side

                curCastlingRights[1] = board[56] != null && board[56].notation == Piece.R && !board[56].hasMoved; //white queen side
            }
            else
            {
                curCastlingRights[0] = false;
                curCastlingRights[1] = false;
            }
            if (board[4] != null && board[4].notation == Piece.k && !board[4].hasMoved)
            {
                curCastlingRights[2] = board[7] != null && board[7].notation == Piece.r && !board[7].hasMoved; //black king side

                curCastlingRights[3] = board[0] != null && board[0].notation == Piece.r && !board[0].hasMoved; //black queen side
            }
            else
            {
                curCastlingRights[2] = false;
                curCastlingRights[3] = false;
            }

            zobrist.UpdateHashCastling(curCastlingRights, castlingRights);
            castlingRights = curCastlingRights;
        }

        public bool IsThreefoldRepetition()
        {
            ulong current = repetitionHistory[repetitionHistory.Count - 1];

            // TODO Check for irreversible moves
            for (int i = repetitionHistory.Count - 2; i >= 0; i--)
            {
                if (repetitionHistory[i] == current)
                {
                    return true;
                }
            }
            return false;
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
                sb.Append(Piece.byteToChar(piece.notation));
            }
            foreach (var right in castlingRights)
            {
                sb.Append(right ? "1" : "0");
            }
            sb.Append(isWhiteToMove ? "w" : "b");
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
                Console.Write(Piece.byteToChar(board[i].notation) + " ");
            }
            CalculateCastlingRights();
            Console.Write("\nKQkq ");
            foreach (var right in castlingRights)
            {
                Console.Write(right + " ");
            }
            Console.WriteLine(" \n" + "Hash: " + zobrist.hashValue + "\n");
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
