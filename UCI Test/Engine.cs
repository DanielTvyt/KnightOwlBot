using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KnightOwlBot
{
    internal class Engine
    {
        private static uint nodes = 0;
        public static ulong Perft(Board board, int depth)
        {
            Move[] legalMoves = Board.GetLegalMoves(board);
            ulong leaves = 0;
            depth--;
            if (depth < 1)
            {
                return (ulong)legalMoves.Length;
            }
            foreach (Move move in legalMoves)
            {
                board = Board.DoMove(move, board);
                leaves += Perft(board, depth);
                board = Board.UndoMove(move, board);
            }
            return leaves;
        }
        public static string Run(Board board, uint time, uint inc)
        {
            string pvString;
            if (time == 0)
            {
                time = 500 * 100; //if no time is given search for 500ms
            }
            uint maxTime = (time + inc) / 100;
            var watch = new Stopwatch();
            watch.Start();
            List<string> pv = [];
            nodes = 0;
            for (uint depth = 1; watch.ElapsedMilliseconds < maxTime || depth <= 1; depth++)
            {
                int score;
                int alpha = int.MinValue;
                int beta = int.MaxValue;

                (score, pv) = Engine.Search(board, depth, alpha, beta);

                if (!board.IsWhiteToMove)
                {
                    score *= -1;
                }

                long searchTime = watch.ElapsedMilliseconds == 0 ? 1 : watch.ElapsedMilliseconds;

                pv.Reverse();
                pvString = string.Join(" ", pv);

                Console.WriteLine("info depth " + depth + " time " + searchTime + " nodes " + nodes + " pv " + pvString + " score cp " + score + " nps " + Convert.ToUInt32(nodes / (decimal) searchTime * 1000));
            }
            return pv[0];
        }

        private static (int, List<string>) Search(Board board, uint depth, int alpha, int beta)
        {
            List<string> pv = [];

            if (depth == 0)
            {
                return (Eval(board), pv);
            }

            Move[] moves = Board.GetLegalMoves(board);

            if (moves.Length == 0)
            {
                return (board.IsWhiteToMove ? Convert.ToInt32(-10000 * depth) : Convert.ToInt32(10000 * depth), pv);
            }

            string bestMove = null;
            List<string> bestPv = [];
            moves = SortMoves(moves);
            int score;
            int bestScore = board.IsWhiteToMove ? int.MinValue : int.MaxValue;

            foreach (Move move in moves)
            {
                board = Board.DoMove(move, board);

                (score, pv) = Search(board, depth - 1, alpha, beta);

                board = Board.UndoMove(move, board);

                if (board.IsWhiteToMove)
                {
                    if (beta <= score)
                    {
                        pv.Add(move.Notation);
                        return (beta, pv);
                    }
                    if (score > bestScore)
                    {
                        bestMove = move.Notation;
                        bestPv = pv;
                        bestScore = score;
                        alpha = Math.Max(bestScore, alpha);
                    }
                }
                else
                {
                    if (alpha >= score)
                    {
                        pv.Add(move.Notation);
                        return (alpha, pv);
                    }
                    if (score < bestScore)
                    {
                        bestMove = move.Notation;
                        bestPv = pv;
                        bestScore = score;
                        beta = Math.Min(bestScore, beta);
                    }
                }
            }
            bestPv.Add(bestMove);
            return (bestScore, bestPv);
        }


        private static int Eval(Board board)
        {
            nodes++;
            if (Board.IsDraw(board))
            {
                return 0;
            }

            int Material = 0;

            for (int i = 0; i < board.board.Length; i++)
            {
                Piece piece = board.board[i];
                if (piece == null)
                {
                    continue;
                }
                if (piece.Notation == 'P')
                {
                    Material += 8 - i / 8;
                }
                else if (piece.Notation == 'p')
                {
                    Material -= i / 8;
                }
                Material += piece.Material;
            }
            return Material;
        }

        private static Move[] SortMoves(Move[] moves)
        {
            Array.Sort(moves, delegate (Move x, Move y) { return x.MoveValue.CompareTo(y.MoveValue); });
            return moves;
        }
    }
}
