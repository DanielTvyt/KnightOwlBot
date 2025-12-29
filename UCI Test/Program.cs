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
            var b1 = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            var b2 = new Board("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
            var b3 = new Board("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");
            var b4 = new Board("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ");

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

                curNodes = Engine.Perft(b3, 6); //~30s
                if (curNodes != 706045033)
                    Console.WriteLine($"[Thread {id}] Error in perft b3");
                nodes += curNodes;

                curNodes = Engine.Perft(b4, 5); //~8s
                if (curNodes != 193690690)
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