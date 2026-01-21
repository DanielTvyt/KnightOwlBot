using System;
using System.Collections.Generic;

namespace KnightOwlBot.Engine
{
    internal class Search
    {
        public static (int, List<Move>) MiniMax(Board board, uint depth, uint ply, int alpha, int beta)
        {
            List<Move> pv = [];

            if (depth == 0)
            {
                return (Evaluation.Eval(board), pv);
            }

            Move[] moves = Board.GetLegalMoves(board);

            if (moves.Length == 0)
            {
                if (board.isInCheck)
                {
                    return (Convert.ToInt32(-Start.INF + ply), pv);
                }
                return (0, pv);
            }

            Move bestMove = null;
            List<Move> bestPv = [];
            moves = SortMoves(moves, board, true);
            int score;

            foreach (Move move in moves)
            {
                board.DoMove(move);

                if (move.isCapture && depth == 1) //quiescence search
                {
                    (score, pv) = QuiescenceSearch(board, ply + 1, -beta, -alpha);
                    score *= -1;
                }
                else
                {
                    (score, pv) = MiniMax(board, depth - 1, ply + 1, -beta, -alpha);
                    score *= -1;
                }

                if (score >= beta)
                {
                    board.UndoMove(move);
                    return (beta, pv);
                }
                if (score > alpha)
                {
                    bestMove = move;
                    bestPv = pv;
                    alpha = score;
                }
                board.UndoMove(move);
            }
            bestPv.Add(bestMove);
            return (alpha, bestPv);
        }

        private static (int, List<Move>) QuiescenceSearch(Board board, uint ply, int alpha, int beta)
        {
            List<Move> pv = [];
            int standPat = Evaluation.Eval(board);

            if (standPat >= beta)
            {
                return (beta, pv);
            }
            if (standPat > alpha)
            {
                alpha = standPat;
            }
            Move[] moves = Board.GetLegalMoves(board);

            if (moves.Length == 0)
            {
                if (board.isInCheck)
                {
                    return (Convert.ToInt32(-Start.INF + ply), pv);
                }
                return (0, pv);
            }

            moves = Array.FindAll(moves, m => m.isCapture);
            moves = SortMoves(moves, board, true);
            int score;
            Move bestMove = null;
            foreach (Move move in moves)
            {
                board.DoMove(move);
                (score, pv) = QuiescenceSearch(board, ply + 1, -beta, -alpha);
                score *= -1;
                if (score >= beta)
                {
                    board.UndoMove(move);
                    return (score, pv);
                }
                if (score > alpha)
                {
                    alpha = score;
                    bestMove = move;
                }
                board.UndoMove(move);
            }
            pv.Add(bestMove);
            return (alpha, pv);
        }

        public static Move[] SortMoves(Move[] moves, Board board, bool isSearch)
        {
            if (isSearch)
            {
                foreach (Move move in moves)
                {
                    if (move.isCapture)
                    {
                        Piece capturedPiece = board.board[move.index2];
                        if (capturedPiece != null)
                        {
                            move.moveValue = Math.Abs(capturedPiece.material * 10) - Math.Abs(board.board[move.index1].material);
                        }
                        else
                        {
                            move.moveValue = 900; //en passent capture value
                        }
                    }
                    else if (move.promPiece != '\0')
                    {
                        switch (move.promPiece)
                        {
                            case 'Q' or 'q':
                                move.moveValue = 8000;
                                break;
                            case 'R' or 'r':
                                move.moveValue = 4000;
                                break;
                            case 'B' or 'b':
                                move.moveValue = 3000;
                                break;
                            case 'N' or 'n':
                                move.moveValue = 2000;
                                break;
                        }
                    }
                    else
                    {
                        move.moveValue = 0;
                    }
                }
            }

            Array.Sort(moves, delegate (Move x, Move y) { return y.moveValue.CompareTo(x.moveValue); });
            return moves;
        }
    }
}
