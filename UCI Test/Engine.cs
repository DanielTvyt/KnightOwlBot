using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace UCI_Test
{
    internal class Engine
    {
        private static int nodes = 0;
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
            Move bestmove = null;
            if (time == 0)
            {
                time = 500 * 100; //if no time is given search for 500ms
            }
            uint maxTime = time / 100;
            var watch = new Stopwatch();
            watch.Start();
            for (int depth = 1; watch.ElapsedMilliseconds < maxTime; depth++)
            {
                nodes = 0;
                int score;
                int maxScore = int.MinValue;
                int minScore = int.MaxValue;
                int alpha = int.MinValue;
                int beta = int.MaxValue;

                foreach (Move move in Board.GetLegalMoves(board))
                {
                    board = Board.DoMove(move, board);

                    score = Engine.Search(board, depth, alpha, beta);

                    board = Board.UndoMove(move, board);

                    if (board.IsWhiteToMove)
                    {
                        if (score > maxScore)
                        {
                            bestmove = move;
                            maxScore = score;
                            Console.WriteLine("Max " + maxScore + " move " + bestmove.Notation);
                            alpha = Math.Max(maxScore, alpha);
                        }
                    }
                    else
                    {
                        if (score < minScore)
                        {
                            bestmove = move;
                            minScore = score;
                            Console.WriteLine("Min " + minScore + " move " + bestmove.Notation);
                            beta = Math.Min(minScore, beta);
                        }
                    }

                }

                if (board.IsWhiteToMove)
                {
                    Console.WriteLine("info depth " + depth + " time " + watch.ElapsedMilliseconds + " nodes " + nodes + " pv " + bestmove.Notation + " score cp " + (maxScore * 100) + " nps " + (1000 * nodes / (watch.ElapsedMilliseconds + 1)));
                }
                else
                {
                    Console.WriteLine("info depth " + depth + " time " + watch.ElapsedMilliseconds + " nodes " + nodes + " pv " + bestmove.Notation + " score cp " + (minScore * -100) + " nps " + (1000 * nodes / (watch.ElapsedMilliseconds + 1)));
                }
            }
        

            if (bestmove.PromPiece != '\0')
            {
                return bestmove.Notation + char.ToLower(bestmove.PromPiece);
            }

            return bestmove.Notation;
        }

        private static int Search(Board board, int depth, int alpha, int beta)
        {
            depth--;

            if (depth <= 0)
            {
                return Eval(board);
            }

            Move[] moves = Board.GetLegalMoves(board);

            if (moves.Length == 0)
            {
                if (board.IsWhiteToMove)
                {
                    return -1000 * depth;
                }
                return 1000 * depth;
            }

            Move bestmove;
            int score;
            int maxScore = int.MinValue;
            int minScore = int.MaxValue;

            foreach (Move move in moves)
            {
                board = Board.DoMove(move, board);
                score = Search(board, depth, alpha, beta);
                board = Board.UndoMove(move, board);

                if (board.IsWhiteToMove)
                {
                    if (beta <= score)
                    {
                        return beta;
                    }
                    if (score > maxScore)
                    {
                        bestmove = move;
                        maxScore = score;
                        alpha = Math.Max(maxScore, alpha);
                    }
                }
                else
                {
                    if (alpha >= score)
                    {
                        return alpha;
                    }
                    if (score < minScore)
                    {
                        bestmove = move;
                        minScore = score;
                        beta = Math.Min(minScore, beta);
                    }
                }

            }
            if (board.IsWhiteToMove)
            {
                return maxScore;
            }
            else
            {
                return minScore;
            }
        }


        private static int Eval(Board board)
        {
            nodes++;
            int Material = 0;

            foreach (Piece piece in board.board)
            {
                if (piece == null)
                {
                    continue;
                }
                Material += piece.Material;
            }

            return Material;
        }
    }
}
