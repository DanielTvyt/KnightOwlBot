using System;
using System.Collections.Generic;

namespace KnightOwlBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Uci.Listen();
            }

            //startpos rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1

            Board[] boards = new Board[10];

            boards[0] = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Board postion = boards[0]; 

            Board.PrintBoard(boards[0]);
            Move[] tests = Board.GetLegalMoves(boards[0]);
            foreach (Move test in tests)
            {
                boards[1] = Board.DoMove(test, boards[0]);
            
                Console.WriteLine(test.Notation);
                Board.PrintBoard(boards[1]);
            }
            
            Console.ReadLine();
            for (int i = 1; i <= 7; i++)
            {
                var timer1 = System.Diagnostics.Stopwatch.StartNew();
                ulong nodes = 0;
                ulong curNodes;
                timer1.Start();
                int ply = 1;
                boards = new Board[i + 1];
                boards[0] = postion;
                Move[] moves = Board.GetLegalMoves(boards[ply - 1]);
                Console.WriteLine(moves.Length);
                for (int j = 0; j < moves.Length; j++)
                {
                    boards[ply] = Board.DoMove(moves[j], boards[ply - 1]);
                    Console.Write(moves[j].Notation + ": ");
                    curNodes = Engine.Perft(boards, i, (ply + 1));
                    Console.WriteLine(curNodes);
                    nodes += curNodes;
                }
                
                timer1.Stop();
                Console.WriteLine("ply: " + i + " Time: " + timer1.ElapsedMilliseconds + " Nodes: " + nodes);
            }
            Move move = new Move();
            move.Notation = "e2e4";
            
            var timer = System.Diagnostics.Stopwatch.StartNew();
            timer.Start();
            for (int i = 1; i <= 5000000; i++)
            {
                Board.DoMove(move, boards[0]);
            }
            timer.Stop();
            Console.WriteLine("Time: " + timer.ElapsedMilliseconds);
            
            //Board.PrintBoard(board1);
            Console.ReadLine();
        }
    }
}