using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnightOwlBot.Engine
{
    internal class Start
    {
        public const int INF = int.MaxValue - 1;
        public static uint nodes = 0;
        public static long maxTime;
        public static Stopwatch watch = new();

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
                board.DoMove(move);
                leaves += Perft(board, depth);
                board.UndoMove(move);
            }
            return leaves;
        }
        public static string Run(Board board, uint time, uint inc)
        {
            if (time == 0)
            {
                time = 500 * 100; //if no time is given search for 500ms
            }
            uint searchTime = (time + inc) / 100;
            maxTime = (time + inc) / 10;
            watch.Start();
            nodes = 0;
            int score = 0;
            List<Move> pv;
            List<Move> bestPv = [];
            Move bestMove = null;
            Move[] moves = Board.GetLegalMoves(board);

            if (moves.Length == 0) return "0000";

            for (uint depth = 1; watch.ElapsedMilliseconds < searchTime || depth <= 1; depth++)
            {
                uint ply = 0;
                int alpha = -INF;
                int beta = INF;

                moves = Search.SortMoves(moves, board, false);

                foreach (Move move in moves)
                {
                    board.DoMove(move);

                    (score, pv) = Search.MiniMax(board, depth - 1, ply + 1, -beta, -alpha);
                    score *= -1;

                    move.MoveValue = score;

                    if (score > alpha)
                    {
                        bestMove = move;
                        bestPv = pv;
                        alpha = score;
                        move.MoveValue = 1000 + Math.Abs(alpha * 1000); //Search best move first next iteration
                    }
                    board.UndoMove(move);

                    if (watch.ElapsedMilliseconds >= maxTime && depth != 1)
                    {
                        Console.WriteLine("Time is up");
                        break;
                    }
                }

                bestPv.Add(bestMove);
                while (bestPv.Remove(null)) { }
                string pvString = "";
                bestPv.Reverse();
                foreach (Move move in bestPv)
                {
                    pvString += move.GetNotation() + " ";
                }
                int selDepth = bestPv.Count;

                long takenTime = watch.ElapsedMilliseconds == 0 ? 1 : watch.ElapsedMilliseconds;
                string bestScore;

                if (Math.Abs(alpha) > INF - 1000) //checkmate
                {
                    string perspective = score > 0 ? "" : "-";
                    bestScore = "mate " + perspective + (INF - Math.Abs(alpha)) / 2;
                }
                else
                {
                    bestScore = "cp " + alpha.ToString();
                }

                Console.WriteLine("info depth " + depth + " seldepth " + selDepth + " score " + bestScore + " nodes " + nodes + " nps " + Convert.ToUInt32(nodes / (decimal)takenTime * 1000) + " time " + takenTime + " pv " + pvString);
            }
            watch = new Stopwatch();
            return bestPv[0].GetNotation();
        }
    }
}
