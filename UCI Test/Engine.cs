using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KnightOwlBot
{
    internal class Engine
    {
        private static uint nodes = 0;
        private static uint maxDepth;
        private static long maxTime;
        private static Stopwatch watch = new();

        public static ulong Perft(Board[] boards, int depth, int ply)
        {
            Move[] legalMoves = Board.GetLegalMoves(boards[ply - 1]);
            ulong leaves = 0;
            depth--;
            if (depth < 1)
            {
                return (ulong)legalMoves.Length;
            }
            foreach (Move move in legalMoves)
            {
                boards[ply] = Board.DoMove(move, boards[ply - 1]);
                leaves += Perft(boards, depth, ply + 1);
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
            uint searchTime = (time + inc) / 100;
            maxTime = (time + inc) / 10;
            watch.Start();
            List<string> pv = [];
            nodes = 0;
            List<Board> boards = new();
            boards.Add(board);
            for (uint depth = 1; watch.ElapsedMilliseconds < searchTime || depth <= 1; depth++)
            {
                int ply = 0;
                maxDepth = depth;
                int score;
                int alpha = int.MinValue;
                int beta = int.MaxValue;
                try
                {
                    (score, pv) = Search(boards, depth, ply + 1, alpha, beta);
                }
                catch
                {
                    continue;
                }

                if (!board.IsWhiteToMove)
                {
                    score *= -1;
                }

                long takenTime = watch.ElapsedMilliseconds == 0 ? 1 : watch.ElapsedMilliseconds;

                pv.Reverse();
                pvString = string.Join(" ", pv);

                Console.WriteLine("info depth " + depth + " seldepth " + pv.Count + " score cp " + score + " nodes " + nodes + " nps " + Convert.ToUInt32(nodes / (decimal)takenTime * 1000) + " time " + takenTime + " pv " + pvString);
            }
            watch = new Stopwatch();
            return pv[0];
        }

        private static (int, List<string>) Search(List<Board> board, uint depth, int ply, int alpha, int beta)
        {
            List<string> pv = [];

            if (depth == 0)
            {
                return (Eval(board[ply - 1]), pv);
            }

            if (watch.ElapsedMilliseconds > maxTime && maxDepth > 1)
            {
                throw new Exception("0");
            }

            Move[] moves = Board.GetLegalMoves(board[ply - 1]);

            if (moves.Length == 0)
            {
                if (board[ply - 1].IsInCheck)
                {
                    return (board[ply - 1].IsWhiteToMove ? Convert.ToInt32(-10000 * depth) : Convert.ToInt32(10000 * depth), pv);
                }
                return (0, pv);
            }

            string bestMove = null;
            List<string> bestPv = [];
            moves = SortMoves(moves);
            int score;
            int bestScore = board[ply - 1].IsWhiteToMove ? int.MinValue : int.MaxValue;

            if (board.Count <= ply)
            {
                board.Add(null);
            }

            foreach (Move move in moves)
            {
                board[ply] = Board.DoMove(move, board[ply - 1]);

                if (move.IsCapture && depth == 1) //quiescence search
                {
                    (score, pv) = Search(board, depth, ply + 1, alpha, beta);
                }
                else
                {
                    (score, pv) = Search(board, depth - 1, ply + 1, alpha, beta);
                }

                if (board[ply - 1].IsWhiteToMove)
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

            for (int i = 0; i < board.board.Length; i++)
            {
                Piece piece = board.board[i];
                if (piece == null)
                {
                    continue;
                }

                switch (piece.Notation)
                {
                    case 1:
                        Material += PST.pawn[i];
                        break;
                    case 2:
                        Material += PST.knight[i];
                        break;
                    case 3:
                        Material += PST.bishop[i];
                        break;
                    case 4:
                        Material += PST.rook[i];
                        break;
                    case 5:
                        Material += PST.queen[i];
                        break;
                    case 6:
                        Material += PST.king[i];
                        break;

                    case 7:
                        Material -= PST.pawn[63 - i];
                        break;
                    case 8:
                        Material -= PST.knight[63 - i];
                        break;
                    case 9:
                        Material -= PST.bishop[63 - i];
                        break;
                    case 10:
                        Material -= PST.rook[63 - i];
                        break;
                    case 11:
                        Material -= PST.queen[63 - i];
                        break;
                    case 12:
                        Material -= PST.king[63 - i];
                        break;
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
