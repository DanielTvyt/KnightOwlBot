using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UCI_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Uci.Listen();
            }
            Board board = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Board board1 = new Board();
            board1 = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            
            for (int i = 1; i <= 7; i++)
            {
                var timer1 = System.Diagnostics.Stopwatch.StartNew();
                ulong nodes = 0;
                timer1.Start();
                nodes = Engine.Perft(board1, i);
                timer1.Stop();
                Console.WriteLine("ply: " + i + " Time: " + timer1.ElapsedMilliseconds + " Nodes: " + nodes);
            }
            
            Move move = new Move();
            move.Notation = "e2e4";
            
            var timer = System.Diagnostics.Stopwatch.StartNew();
            timer.Start();
            for (int i = 1; i <= 5000000; i++)
            {
                //Board.GetCaptures(board1);
            }
            timer.Stop();
            Console.WriteLine("Time: " + timer.ElapsedMilliseconds);
            
            //Board.PrintBoard(board1);
            Console.ReadLine();
        }
    }
}