using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;

namespace KnightOwlBot
{
    internal class Board
    {
        public Piece[] board { get; set; }
        public List<int> ThreeFold { get; set; }
        public bool IsWhiteToMove { get; set; }
        public int EnPassentIndex {  get; set; }
        public ulong[] bitboards = new ulong[21];
        /*
        0 = P, 1 = N, 2 = B, 3 = R, 4 = Q, 5 = K,
        6 = p, 7 = n, 8 = b, 9 = r, 10 = q, 11 = k,
        12 = attacks
        */

        public static Board BuildFromFenString(string fenString)
        {
            Board boardOut = new()
            {
                board = new Piece[64],
                ThreeFold = []
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

        public static Move[] GetLegalMoves(Board board)
        {
            ulong bitboardAttacked = 0UL;
            ulong[] bitboardPinned = new ulong[64];
            List<Move> moves = [];
            Move move;
            int moveDelta;
            int lastJ;
            byte lastCap;
            string pos1;
            string pos2;
            int pieceCount = -1; //start with index 0
            int index = 0;

            for (int i = 0; i < 64; i++)
            {
                index++;

                if (board.board[i] == null)
                {
                    continue;
                }
                pieceCount++; //TODO: if there are more than 20 pieces: continue

                if (board.board[i].IsWhite != board.IsWhiteToMove)
                {
                    continue;
                }
                pos1 = Board.IndexToPos(i);

                if (board.board[i].Notation is 0 or 6) //p or P
                {
                    string promPieces = "qrbn";
                    int fw = 8;
                    int fw2 = 16;
                    int cap1 = 7;
                    int cap2 = 9;
                    int start = 1; // start row
                    int prom = 7;
                    if (board.IsWhiteToMove)
                    {
                        promPieces = "QRBN";
                        fw = -8;
                        fw2 = -16;
                        cap1 = -9;
                        cap2 = -7;
                        start = 6;
                        prom = 0;
                    }
                    if ((i + fw) / 8 == prom) //Promotion
                    {
                        if (board.board[i + fw] == null)
                        {
                           for (int j = 0; j < 4; j++)
                           {
                               move = moveHelper(pos1, Board.IndexToPos(i + fw) + char.ToLower(promPieces[j]), false, (byte)69, 9);
                               move.PromPiece = promPieces[j];
                               moves.Add(move);
                           }
                        }

                        if (i % 8 != 0 && (board.board[i + cap1] != null && board.board[i + cap1].IsWhite != board.IsWhiteToMove)) //capture
                        {
                            lastCap = board.board[i + cap1] != null ? board.board[i + cap1].Notation : (byte) 69;
                            for (int j = 0; j < 4; j++)
                            {
                                move = moveHelper(pos1, Board.IndexToPos(i + cap1) + char.ToLower(promPieces[j]), true, lastCap, 10);
                                move.PromPiece = promPieces[j];
                                moves.Add(move);
                            }

                        }

                        if (i % 8 != 7 && (board.board[i + cap2] != null && board.board[i + cap2].IsWhite != board.IsWhiteToMove))
                        {
                            lastCap = board.board[i + cap2] != null ? board.board[i + cap2].Notation : (byte)69;
                            for (int j = 0; j < 4; j++)
                            {
                                move = moveHelper(pos1, Board.IndexToPos(i + cap2) + char.ToLower(promPieces[j]), true, lastCap, 10);
                                move.PromPiece = promPieces[j];
                                moves.Add(move);
                            }
                        }
                        continue;
                    }
                    else if (board.board[i + fw] == null) //move one forward
                    {
                        moves.Add(moveHelper(pos1, Board.IndexToPos(i + fw), false, (byte)69, 1));

                        if (i / 8 == start && board.board[i + fw2] == null) 
                        {
                            move = moveHelper(pos1, Board.IndexToPos(i + fw2), false, (byte)69, 1);
                            move.EnPassentIndex = i + fw;
                            moves.Add(move);
                        }
                    }

                    if (i % 8 != 0) 
                    {
                        bitboardAttacked |= 1UL << (i + cap1);
                        if ((board.board[i + cap1] != null && board.board[i + cap1].IsWhite != board.IsWhiteToMove || (i + cap1 == board.EnPassentIndex && i / 8 != start)))//capture
                        {
                            lastCap = board.board[i + cap1] != null ? board.board[i + cap1].Notation : (byte)69;
                            moves.Add(moveHelper(pos1, Board.IndexToPos(i + cap1), true, lastCap, 6));
                        }
                    }

                    if (i % 8 != 7)
                    {
                        bitboardAttacked |= 1UL << (i + cap2);
                        if ((board.board[i + cap2] != null && board.board[i + cap2].IsWhite != board.IsWhiteToMove || (i + cap2 == board.EnPassentIndex && i / 8 != start)))
                        {
                            lastCap = board.board[i + cap2] != null ? board.board[i + cap2].Notation : (byte)69;
                            moves.Add(moveHelper(pos1, Board.IndexToPos(i + cap2), true, lastCap, 6));
                        }
                    }


                    continue;
                }

                for (int k = 0; k < board.board[i].MoveDelta.Length; k++)
                {
                    lastJ = i;
                    moveDelta = board.board[i].MoveDelta[k];
                    for (int j = i + moveDelta; j < 64 && j >= 0; j += moveDelta)
                    {
                        if (j % 8 == 0 && lastJ % 8 == 7 || j % 8 == 7 && lastJ % 8 == 0)
                        {
                            break;
                        }

                        if (board.board[i].Notation is 7 or 1 && i % 8 < 2 && (moveDelta == -10 || moveDelta == 6) || i % 8 > 5 && (moveDelta == 10 || moveDelta == -6)) //n or N
                        {
                            break;
                        }

                        pos2 = Board.IndexToPos(j);

                        if (board.board[j] == null)
                        {
                            bitboardAttacked |= 1UL << j;
                            moves.Add(moveHelper(pos1, pos2, false, (byte)69, 1));
                        }
                        else if (board.board[j].IsWhite == board.IsWhiteToMove)
                        {
                            bitboardAttacked |= 1UL << j;
                            bitboardPinned[pieceCount] = pinnedPieces(board, j, lastJ, moveDelta);
                            break;
                        }
                        else
                        {
                            bitboardAttacked |= 1UL << j;
                            bitboardPinned[pieceCount] = pinnedPieces(board, j, lastJ, moveDelta);
                            lastCap = board.board[j].Notation;
                            moves.Add(moveHelper(pos1, pos2, true, lastCap, 5));
                            break;
                        }

                        if (!board.board[i].IsSliding)
                        {
                            break;
                        }
                        lastJ = j;
                    }
                }
            }
            List<Move> legalMoves = [];
            for (int i = 0; i < moves.Count; i++)
            {
                board = DoMove(moves[i], board);
                if (isLegal(board, moves[i], bitboardAttacked, bitboardPinned))
                {
                    legalMoves.Add(moves[i]);
                }
                board = UndoMove(moves[i], board);
            }
            board.bitboards[20] = bitboardAttacked;
            //Console.Write(".");
            return [.. legalMoves];
        }

        private static ulong pinnedPieces(Board board, int index, int lastJ, int moveDelta)
        {
            ulong bitboardPinned = 0UL;
            for (int j = index + moveDelta; j < 64 && j >= 0; j += moveDelta)
            {
                if (j % 8 == 0 && lastJ % 8 == 7 || j % 8 == 7 && lastJ % 8 == 0)
                {
                    break;
                }
                if (board.board[j] != null)
                {
                    if (board.board[j].Material == 0 && board.IsWhiteToMove != board.board[j].IsWhite)
                    {
                        return bitboardPinned;
                    }
                    break;
                }
                bitboardPinned |= 1UL << j;
            }
            return 0; //return 0 if we are not pinning the King
        }

        public static Board DoMove(Move move, Board board) //BUG NULL REFRENCE IF ENPASSANT
        {
            if (move == null)
            {
                Console.WriteLine("ERROR AT Board.DoMove -> NullException");
                Console.WriteLine(BoardToString(board.board));
                Board.PrintBoard(board);
                return null;
            }
            int index1 = move.Notation[0] - 96 + 64 - 8 * Convert.ToInt32(new string(move.Notation[1], 1)) - 1; //index of lower case letter in alphabet (a = 1, b = 2, ...)
            int index2 = move.Notation[2] - 96 + 64 - 8 * Convert.ToInt32(new string(move.Notation[3], 1)) - 1;

            board.board[index2] = board.board[index1];
            board.board[index1] = null;

            if (board.board[index2].Material == 0)
            {
                if (index1 == 60 || index1 == 4) //Castling
                {
                    int rookMove = 0;
                    char rook;
                    if (board.IsWhiteToMove)
                    {
                        rook = 'R';
                        if (index2 == 62)
                        {
                            board.board[63] = null;
                            rookMove = 61;
                        }
                        else if (index2 == 58)
                        {
                            board.board[56] = null;
                            rookMove = 59;
                        }
                    }
                    else
                    {
                        rook = 'r';
                        if(index2 == 6)
                        {
                            board.board[7] = null;
                            rookMove = 5;
                        }
                        else if (index2 == 2)
                        {
                            board.board[0] = null;
                            rookMove = 3;
                        }
                    }
                    if (rookMove != 0)
                    {
                        board.board[rookMove] = Piece.CreatePiece(rook);
                    }
                }
            }

            if (move.PromPiece != '\0')
            {
                board.board[index2] = Piece.CreatePiece(move.PromPiece);
            }

            board.EnPassentIndex = move.EnPassentIndex;

            if (move.IsCapture && move.LastCapture == 69)
            {
                if(board.IsWhiteToMove)
                {
                    board.board[index2 + 8] = null;
                }
                else
                {
                    board.board[index2 - 8] = null;
                }
            }

            board.IsWhiteToMove = !board.IsWhiteToMove;
            board.ThreeFold.Add(BoardToString(board.board).GetHashCode());

            return board;
        }

        public static Board UndoMove(Move move, Board board)
        {
            Move newMove = new()
            {
                Notation = Convert.ToString("" + move.Notation[2] + move.Notation[3] + move.Notation[0] + move.Notation[1])  //e2e4
            };

            if (move.PromPiece != '\0')
            {
                char pawn = board.IsWhiteToMove ? 'p' : 'P';
                board.board[newMove.Notation[0] - 96 + 64 - 8 * Convert.ToInt32(new string(newMove.Notation[1], 1)) - 1] = Piece.CreatePiece(pawn);
            }
            //Do Move
            int index1 = newMove.Notation[0] - 96 + 64 - 8 * Convert.ToInt32(new string(newMove.Notation[1], 1)) - 1;
            int index2 = newMove.Notation[2] - 96 + 64 - 8 * Convert.ToInt32(new string(newMove.Notation[3], 1)) - 1;

            board.board[index2] = board.board[index1];
            board.board[index1] = null;

            board.IsWhiteToMove = !board.IsWhiteToMove;
            //Do Move

            if (move.IsCapture)
            {
                if (move.LastCapture == 69) //En Passent
                {
                    int offset = board.IsWhiteToMove ? 8 : -8;
                    char pawn = board.IsWhiteToMove ? 'p' : 'P';
                    int pos1 = index1 + offset;
                    board.board[pos1] = Piece.CreatePiece(pawn);
                }
                else
                {
                    int[] pos1 =
                    [
                        newMove.Notation[0] - 96,   //index of lower case letter in alphabet (a = 1, b = 2, ...)
                        64 - 8 * Convert.ToInt32(new string(newMove.Notation[1], 1)) - 1,
                    ];
                    board.board[pos1[0] + pos1[1]] = Piece.CreatePiece(Piece.byteToChar(move.LastCapture));
                }
            }
            board.EnPassentIndex = move.EnPassentIndex;
            board.ThreeFold.RemoveAt(board.ThreeFold.Count - 1);

            return board;
        }

        public static bool IsDraw(Board board)
        {
            if (board.ThreeFold.Count < 3)
            {
                return false;
            }
            bool count = false; // Three Fold at just two moves
            int compare = board.ThreeFold[board.ThreeFold.Count - 1];
            for (int i = 0; i < board.ThreeFold.Count - 1; i++)
            {
                if (compare == board.ThreeFold[i])
                {
                    if (count)
                    {
                        return true;
                    }
                    count = true;
                }
            }
            return false;
        }

        private static string BoardToString(Piece[] pieces)
        {
            char[] chars = [.. pieces.Select(p => p == null ? ' ' : (char)p.Notation)];
            return new string(chars);
        }

        private static Move moveHelper(string pos1, string pos2, bool isCapture, byte lastCapture, byte moveValue)
        {
            Move move = new()
            {
                Notation = pos1 + pos2,
                IsCapture = isCapture,
                LastCapture = lastCapture,
                MoveValue = moveValue
            };

            return move;
        }

        private static bool isLegal(Board board, Move move, ulong bitboard, ulong[] bitboardPinns)
        {
            char king = board.IsWhiteToMove ? 'K' : 'k';
            //if move Piece != K && "to square" AND bitboard == bitboard [If piece goes in between King and Attacker its legal even if King is in bitboard]
            for (int i = 0; i < 64; i++)
            {
                if (board.board[i] != null && board.board[i].Notation == king)
                {
                    ulong kingBitboard = 1UL << i;

                    if ((kingBitboard & bitboard) == bitboard)
                    {
                        return false;
                    }
                    break;
                }
            }
            int index1 = move.Notation[0] - 96 + 64 - 8 * Convert.ToInt32(new string(move.Notation[1], 1)) - 1;
            int index2 = move.Notation[2] - 96 + 64 - 8 * Convert.ToInt32(new string(move.Notation[3], 1)) - 1;
            ulong movePos1 = 1UL << index1;
            ulong movePos2 = 1UL << index2;
            for (int i = 0; i < bitboardPinns.Length; i++)
            {
                if (bitboardPinns[i] == 0)
                {
                    continue;
                }
                if ((movePos1 & bitboardPinns[i]) == bitboardPinns[i])
                {
                    if ((movePos2 & bitboardPinns[i]) == bitboardPinns[i])
                    {
                        return true;
                    }
                }
            }
            //Console.WriteLine("ERROR NO KING WAS FOUND!!!");
            //Board.PrintBoard(board);
            return true;
        }

        private static string IndexToPos(int index)
        {
            return "" + (char)(index % 8 + 97) + Convert.ToString(8 - index / 8); //char 97 = a
        }

        public static void PrintBoard(Board board)
        {
            for (int i = 0; i < board.board.Length; i++)
            {
                if (i % 8 == 0)
                {
                    Console.WriteLine();
                }

                if (board.board[i] == null)
                {
                    Console.Write("- ");
                }
                else
                {
                    Console.Write(Piece.byteToChar(board.board[i].Notation) + " ");
                }
                
            }
            Console.WriteLine(" \n");
        }

    }
}
