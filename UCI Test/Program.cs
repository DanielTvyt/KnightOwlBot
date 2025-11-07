using System;
using System.Diagnostics;

namespace KnightOwlBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Warm up
            Board board = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Board.GetLegalMoves(board);
            board.GetHashCode();
            board = null;

            if (args.Length > 0 && args[0] == "speedtest")
            {
                speedtest(args[1]);
            }

            Uci.Listen();
        }

        public static void speedtest(string id)
        {
            // Each thread builds its own independent boards
            var b1 = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            var b2 = new Board("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
            var b3 = new Board("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10");
            var b4 = new Board("5k2/1B6/8/2p5/8/2K1n2p/5p2/8 b - - 1 57");

            ulong nodes = 0;
            var timer = Stopwatch.StartNew();

            ulong curNodes;
            bool first = true;

            while (first)
            {
                first = false;
                curNodes = Engine.Perft(b1, 6); //~15s
                if (curNodes != 119060324)
                    Console.WriteLine($"[Thread {id}] Error in perft startpos");
                nodes += curNodes;

                curNodes = Engine.Perft(b2, 7); //~25s
                if (curNodes != 178633661)
                    Console.WriteLine($"[Thread {id}] Error in perft b2");
                nodes += curNodes;

                curNodes = Engine.Perft(b3, 5); //~20s
                if (curNodes != 164075551)
                    Console.WriteLine($"[Thread {id}] Error in perft b3");
                nodes += curNodes;

                curNodes = Engine.Perft(b4, 7); //~30s
                if (curNodes != 302573451)
                    Console.WriteLine($"[Thread {id}] Error in perft b4");
                nodes += curNodes;
            }

            timer.Stop();
            long elapsed = timer.ElapsedMilliseconds;
            double knps = nodes / (double)(elapsed);

            // Print individual result
            Console.WriteLine($"Thread {id} Time {elapsed} Nodes {nodes} knps {knps:N0}");
            Environment.Exit(0);
        }
    }
}