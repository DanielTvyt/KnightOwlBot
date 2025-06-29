﻿using System;

namespace KnightOwlBot
{
    internal class Uci
    {
        public static void Listen()
        { 
            string bestmove;
            string position = "";
            Board board;

            while (true)
            {
                uint time = 0;
                uint inc = 0;
                string sTime = "";
                string sInc = "";
                string UciIn = Console.ReadLine();

                if (UciIn.Contains("position"))
                {
                    position = UciIn;
                }
                else if (UciIn.Contains("go"))
                {
                    board = Uci.GetPos(position);
                    if (board.IsWhiteToMove)
                    {
                        if (UciIn.Contains("wtime"))
                        {
                            int x = UciIn.IndexOf("wtime");
                            for (int i = x + 6; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sTime += UciIn[i];
                            }
                            time = Convert.ToUInt32(sTime);
                        }
                        if (UciIn.Contains("winc"))
                        {
                            int x = UciIn.IndexOf("winc");
                            for (int i = x + 5; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sInc += UciIn[i];
                            }
                            inc = Convert.ToUInt32(sTime);
                        }
                    }
                    else
                    {
                        if (UciIn.Contains("btime"))
                        {
                            int x = UciIn.IndexOf("btime");
                            for (int i = x + 6; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sTime += UciIn[i];
                            }
                            time = Convert.ToUInt32(sTime);
                        }
                        if (UciIn.Contains("binc"))
                        {
                            int x = UciIn.IndexOf("binc");
                            for (int i = x + 5; i < UciIn.Length; i++)
                            {
                                if (UciIn[i] == ' ')
                                {
                                    break;
                                }
                                sInc += UciIn[i];
                            }
                            inc = Convert.ToUInt32(sTime);
                        }
                    }
                    if (UciIn.Contains("infinite"))
                    {
                        time = uint.MaxValue;
                    }
                    bestmove = Engine.Run(board, time, inc);
                    Console.WriteLine("bestmove " + bestmove);
                }
                else if (UciIn == "uci")
                {
                    Console.WriteLine("id name UciTest");
                    Console.WriteLine("uciok");
                }
                else if (UciIn.Contains("isready"))
                {
                    Console.WriteLine("readyok");
                }
                else if (UciIn == "print")
                {
                    board = Uci.GetPos(position);
                    Board.PrintBoard(board);
                }
                else if (UciIn.Contains("perft"))
                {
                    int depth;
                    board = Uci.GetPos(position);
                    try
                    {
                        depth = Convert.ToInt32(UciIn[6].ToString());
                        Console.WriteLine(depth);
                    }
                    catch
                    {
                        Console.WriteLine("No Depth given so were searching for a depth of 4");
                        depth = 4;
                    }
                    for (int i = 0; i <= depth; i++)
                    {
                        var timer1 = System.Diagnostics.Stopwatch.StartNew();
                        ulong nodes = 0;
                        ulong curNodes;
                        timer1.Start();
                        Move[] moves = Board.GetLegalMoves(board);
                        int ply = 1;
                        Board[] boards = new Board[i + 1];
                        boards[0] = board;
                        if (i == 0)
                        {
                            foreach (var move in moves)
                            {
                                nodes++;
                                Console.WriteLine(move.Notation + ": 1");
                            }
                            timer1.Stop();
                            Console.WriteLine("ply: " + i + " Time " + timer1.ElapsedMilliseconds + " Nodes " + nodes + " knps " + nodes / ((ulong)timer1.ElapsedMilliseconds + 1));
                            continue;
                        }
                        for (int j = 0; j < moves.Length; j++)
                        {
                            boards[ply] = Board.DoMove(moves[j], boards[ply - 1]);
                            Console.Write(moves[j].Notation + ": ");
                            curNodes = Engine.Perft(boards, i, ply + 1);
                            Console.WriteLine(curNodes);
                            nodes += curNodes;
                        }
                        timer1.Stop();
                        Console.WriteLine("ply: " + i + " Time " + timer1.ElapsedMilliseconds + " Nodes " + nodes + " knps " + nodes / ((ulong)timer1.ElapsedMilliseconds + 1));
                    }
                }
                else if (UciIn == "quit")
                {
                    Environment.Exit(0);
                }
            }
        }


        public static Board GetPos(string uciIn)
        {
            string fenString;
            if (uciIn.Contains("fen"))   //positon fen <fenstring>
            {
                fenString = uciIn.Remove(0, 13);
                return Board.BuildFromFenString(fenString);
            }
            else
            {
                Board board = Board.BuildFromFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                if (uciIn == "position startpos")
                {
                    return board;
                }
                else                     //position startpos moves <move1 move2 ...>
                {
                    string[] moves = uciIn.Split(' ');

                    for (int i = 3; i < moves.Length; i++) //skip pos, start, moves
                    {
                        Move move = new()
                        {
                            Notation = moves[i]
                        };
                        if (move.Notation.Length == 5)
                        {
                            move.PromPiece = move.Notation[4];
                            if (board.IsWhiteToMove)
                            {
                                move.PromPiece = char.ToUpper(move.PromPiece);
                            }
                        }
                        board = Board.DoMove(move, board);
                    }
                    return board;
                }
            }
        }
    }
}
