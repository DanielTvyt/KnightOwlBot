using System;
using System.Collections.Generic;
using System.Linq;

namespace KnightOwlBot
{
    internal class Board
    {
        public Piece[] board { get; set; }
        public List<string> ThreeFold { get; set; }
        public bool IsWhiteToMove { get; set; }
        public int EnPassentIndex {  get; set; }

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

        private static Move[] GetCaptures(Board board) //No Enpassant -> cant capture King
        {
            List<Move> moves = [];
            int moveDelta;
            int lastJ;
            char lastCap;
            string pos1;
            string pos2;
            int index = 0;

            for (int i = 0; i < 64; i++)
            {
                index++;

                if (board.board[i] == null || board.board[i].IsWhite != board.IsWhiteToMove)
                {
                    continue;
                }
                pos1 = Board.IndexToPos(i);

                if (board.board[i].Notation is 'P' or 'p')
                {
                    int cap1 = 7;
                    int cap2 = 9;
                    if (board.IsWhiteToMove)
                    {
                        cap1 = -9;
                        cap2 = -7;
                    }

                    if (i % 8 != 0 && (board.board[i + cap1] != null && board.board[i + cap1].IsWhite != board.IsWhiteToMove)) //capture
                    {
                        lastCap = board.board[i + cap1] != null ? board.board[i + cap1].Notation : '\0';
                        moves.Add(moveHelper(pos1, Board.IndexToPos(i + cap1), true, lastCap, 0));
                    }

                    if (i % 8 != 7 && (board.board[i + cap2] != null && board.board[i + cap2].IsWhite != board.IsWhiteToMove))
                    {
                        lastCap = board.board[i + cap2] != null ? board.board[i + cap2].Notation : '\0';
                        moves.Add(moveHelper(pos1, Board.IndexToPos(i + cap2), true, lastCap, 0));
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

                        if (board.board[i].Notation is 'n' or 'N' && i % 8 < 2 && (moveDelta == -10 || moveDelta == 6) || i % 8 > 5 && (moveDelta == 10 || moveDelta == -6))
                        {
                            break;
                        }

                        pos2 = Board.IndexToPos(j);

                        if (board.board[j] != null && board.board[j].IsWhite == board.IsWhiteToMove)
                        {
                            break;
                        }
                        else if (board.board[j] != null)
                        {
                            lastCap = board.board[j].Notation;
                            moves.Add(moveHelper(pos1, pos2, true, lastCap, 0));

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

            return [.. moves];
        }

        private static Move[] GetPseudoLegalMoves(Board board)
        {
            List<Move> moves = [];
            Move move;
            int moveDelta;
            int lastJ;
            char lastCap;
            string pos1;
            string pos2;
            int index = 0;

            for (int i = 0; i < 64; i++)
            {
                index++;

                if (board.board[i] == null || board.board[i].IsWhite != board.IsWhiteToMove)
                {
                    continue;
                }
                pos1 = Board.IndexToPos(i);

                if (board.board[i].Notation is 'P' or 'p')
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
                               move = moveHelper(pos1, Board.IndexToPos(i + fw) + char.ToLower(promPieces[j]), false, '\0', 9);
                               move.PromPiece = promPieces[j];
                               moves.Add(move);
                           }
                        }

                        if (i % 8 != 0 && (board.board[i + cap1] != null && board.board[i + cap1].IsWhite != board.IsWhiteToMove)) //capture
                        {
                            lastCap = board.board[i + cap1] != null ? board.board[i + cap1].Notation : '\0';
                            for (int j = 0; j < 4; j++)
                            {
                                move = moveHelper(pos1, Board.IndexToPos(i + cap1) + char.ToLower(promPieces[j]), true, lastCap, 10);
                                move.PromPiece = promPieces[j];
                                moves.Add(move);
                            }

                        }

                        if (i % 8 != 7 && (board.board[i + cap2] != null && board.board[i + cap2].IsWhite != board.IsWhiteToMove))
                        {
                            lastCap = board.board[i + cap2] != null ? board.board[i + cap2].Notation : '\0';
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
                        moves.Add(moveHelper(pos1, Board.IndexToPos(i + fw), false, '\0', 1));

                        if (i / 8 == start && board.board[i + fw2] == null) 
                        {
                            move = moveHelper(pos1, Board.IndexToPos(i + fw2), false, '\0', 1);
                            move.EnPassentIndex = i + fw;
                            moves.Add(move);
                        }
                    }

                    if (i % 8 != 0 && (board.board[i+cap1] != null && board.board[i + cap1].IsWhite != board.IsWhiteToMove || (i + cap1 == board.EnPassentIndex && i / 8 != start))) //capture
                    {
                        lastCap = board.board[i + cap1] != null ? board.board[i + cap1].Notation : '\0';
                        moves.Add(moveHelper(pos1, Board.IndexToPos(i + cap1), true, lastCap, 6));
                    }

                    if (i % 8 != 7 && (board.board[i + cap2] != null && board.board[i + cap2].IsWhite != board.IsWhiteToMove || (i + cap2 == board.EnPassentIndex && i / 8 != start)))
                    {
                        lastCap = board.board[i + cap2] != null ? board.board[i + cap2].Notation : '\0';
                        moves.Add(moveHelper(pos1, Board.IndexToPos(i + cap2), true, lastCap, 6));
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

                        if (board.board[i].Notation is 'n' or 'N' && i % 8 < 2 && (moveDelta == -10 || moveDelta == 6) || i % 8 > 5 && (moveDelta == 10 || moveDelta == -6))
                        {
                            break;
                        }

                        pos2 = Board.IndexToPos(j);

                        if (board.board[j] == null)
                        {
                            moves.Add(moveHelper(pos1, pos2, false, '\0', 1));
                        }
                        else if (board.board[j].IsWhite == board.IsWhiteToMove)
                        {
                            break;
                        }
                        else
                        {
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
            
            return [.. moves];
        }

        public static Move[] GetLegalMoves(Board board)
        {
            Move[] moves = Board.GetPseudoLegalMoves(board);
            List<Move> legalMoves = [];
            foreach (Move m in moves)
            {
                if (Board.isLegal(board, m))
                {
                    legalMoves.Add(m);
                }
            }
            return [.. legalMoves];
        }

        public static Board DoMove(Move move, Board board)
        {
            int index1 = move.Notation[0] - 96 + 64 - 8 * Convert.ToInt32(new string(move.Notation[1], 1)) - 1; //index of lower case letter in alphabet (a = 1, b = 2, ...)
            int index2 = move.Notation[2] - 96 + 64 - 8 * Convert.ToInt32(new string(move.Notation[3], 1)) - 1;

            board.board[index2] = board.board[index1];
            board.board[index1] = null;

            if (board.board[index2].Material == 0)
            {
                if (index1 == 60 || index1 == 4)
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

            if (move.IsCapture && move.LastCapture == '\0')
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
            board.ThreeFold.Add(BoardToString(board.board));

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
                if (move.LastCapture == '\0') //En Passent
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
                    board.board[pos1[0] + pos1[1]] = Piece.CreatePiece(move.LastCapture);
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
            int count = 0;
            string compare = BoardToString(board.board);
            for (int i = 0; i < board.ThreeFold.Count; i++)
            {
                if (compare == board.ThreeFold[i])
                {
                    count++;
                    if (count == 3)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string BoardToString(Piece[] pieces)
        {
            char[] chars = [.. pieces.Select(p => p == null ? ' ' : p.Notation)];
            return new string(chars);
        }

        private static Move moveHelper(string pos1, string pos2, bool isCapture, char lastCapture, byte moveValue)
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

        private static bool isLegal(Board board, Move move)
        {
            board = DoMove(move, board);
            Move[] moves = GetCaptures(board);
                         
            if (board.IsWhiteToMove)
            {
                foreach (Move LegalMove in moves)
                {
                    Board.DoMove(LegalMove, board);
                    if (!board.board.Any(Piece => Piece != null && Piece.Notation == 'k'))
                    {
                        Board.UndoMove(LegalMove, board);
                        Board.UndoMove(move, board);
                        return false;
                    }
                    Board.UndoMove(LegalMove, board);
                }
            }
            else
            {
                foreach (Move LegalMove in moves)
                {
                    Board.DoMove(LegalMove, board);
                    if (!board.board.Any(Piece => Piece != null && Piece.Notation == 'K'))
                    {
                        Board.UndoMove(LegalMove, board);
                        Board.UndoMove(move, board);
                        return false;
                    }
                    Board.UndoMove(LegalMove, board);
                }
            }
            Board.UndoMove(move, board);
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
                    Console.WriteLine(" ");
                }

                if (board.board[i] == null)
                {
                    Console.Write("- ");
                }
                else
                {
                    Console.Write(board.board[i].Notation + " ");
                }
                
            }
            Console.WriteLine(" \n");
        }

    }
}
