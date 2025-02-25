using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UCI_Test
{
    internal class Engine
    {
        private static int nodes = 0;
        private static Random rand = new();
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
            string pvString;
            if (time == 0)
            {
                time = 500 * 100; //if no time is given search for 500ms
            }
            uint maxTime = (time+inc) / 100;
            var watch = new Stopwatch();
            watch.Start();
            for (uint depth = 1; watch.ElapsedMilliseconds < maxTime; depth++)
            {
                List<string> bestPv = [];
                List<string> pv;      
                nodes = 0;
                int cp;
                int score;
                int maxScore = int.MinValue;
                int minScore = int.MaxValue;
                int alpha = int.MinValue;
                int beta = int.MaxValue;

                foreach (Move move in Board.GetLegalMoves(board))
                {
                    board = Board.DoMove(move, board);

                    (score, pv) = Engine.Search(board, depth, alpha, beta);

                    board = Board.UndoMove(move, board);

                    if (board.IsWhiteToMove)
                    {
                        if (score > maxScore)
                        {
                            maxScore = score;
                            alpha = Math.Max(maxScore, alpha);
                            pv.Add(move.Notation);
                            bestPv = pv;
                            bestmove = move;
                            //Console.WriteLine("Max " + maxScore + " move " + bestmove.Notation);
                        }
                    }
                    else
                    {
                        if (score < minScore)
                        {
                            minScore = score;
                            beta = Math.Min(minScore, beta);
                            pv.Add(move.Notation);
                            bestPv = pv;
                            bestmove = move;
                            //Console.WriteLine("Min " + minScore + " move " + bestmove.Notation);
                        }
                    }

                }

                if (board.IsWhiteToMove)
                {
                    cp = maxScore;
                }
                else
                {
                    cp = minScore * -1;
                }

                bestPv.Reverse();
                pvString = string.Join(" ", bestPv);

                Console.WriteLine("info depth " + depth + " time " + watch.ElapsedMilliseconds + " nodes " + nodes + " pv " + pvString + " score cp " + cp + " nps " + (1000 * nodes / (watch.ElapsedMilliseconds + 1)));
            }
        

            if (bestmove.PromPiece != '\0')
            {
                return bestmove.Notation + char.ToLower(bestmove.PromPiece);
            }

            return bestmove.Notation;
        }

        private static (int, List<string>) Search(Board board, uint depth, int alpha, int beta)
        {
            depth--;
            string bestMove = null;
            List<string> pv = [];
            List<string> bestPv = [];

            if (depth <= 0)
            {
                return (Eval(board), pv);
            }

            Move[] moves = Board.GetLegalMoves(board);

            if (moves.Length == 0)
            {
                return (board.IsWhiteToMove ? Convert.ToInt32(-10000 * depth) : Convert.ToInt32(10000 * depth), pv);
            }

            int score;
            int bestScore = board.IsWhiteToMove ? int.MinValue : int.MaxValue;

            foreach (Move move in moves)
            {
                board = Board.DoMove(move, board);

                (score, pv) = Search(board, depth, alpha, beta);

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
            int Material = 0;

            foreach (Piece piece in board.board)
            {
                if (piece == null)
                {
                    continue;
                }
                Material += piece.Material;
            }
            //Material += rand.Next(10);
            return Material;
        }
    }
}
