using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace KnightOwlBot
{
    internal class Uci
    {
        public static Task Listen()
        {
            string bestmove;
            string position = "position startpos";
            Board board = GetPos(position);

            while (true)
            {
                uint time = 0;
                uint inc = 0;
                string sTime = "";
                string sInc = "";
                string UciIn = Console.ReadLine();

                if (UciIn.Contains("position"))
                {
                    board = GetPos(UciIn);
                }
                else if (UciIn.Contains("go"))
                {
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
                else if (UciIn == "quit")
                {
                    Environment.Exit(0);
                }
                else if (UciIn == "print")
                {
                    board.PrintBoard();
                }
                else if (UciIn.Contains("perft"))
                {
                    int depth;
                    try
                    {
                        string inDepth = UciIn.Remove(0, 6);
                        depth = Convert.ToInt32(inDepth.ToString()) - 1;
                        Console.WriteLine(depth + 1);
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
                        if (i == 0)
                        {
                            foreach (var move in moves)
                            {
                                nodes++;
                                Console.WriteLine(move.GetNotation() + ": 1");
                            }
                            timer1.Stop();
                            Console.WriteLine("ply: " + (i + 1) + " Time " + timer1.ElapsedMilliseconds + " Nodes " + nodes + " knps " + nodes / ((ulong)timer1.ElapsedMilliseconds + 1));
                            continue;
                        }
                        for (int j = 0; j < moves.Length; j++)
                        {
                            board.DoMove(moves[j]);
                            Console.Write(moves[j].GetNotation() + ": ");
                            curNodes = Engine.Perft(board, i);
                            Console.WriteLine(curNodes);
                            nodes += curNodes;
                            board.UndoMove(moves[j]);
                        }
                        timer1.Stop();
                        Console.WriteLine("ply: " + (i + 1) + " Time " + timer1.ElapsedMilliseconds + " Nodes " + nodes + " knps " + nodes / ((ulong)timer1.ElapsedMilliseconds + 1));
                    }
                }
                else if (UciIn == "speedtest")
                {
                    speedtest();
                }
            }
        }


        public static Board GetPos(string uciIn)
        {
            Board board = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            if (uciIn == "position startpos")
            {
                return board;
            }
            string fenString;
            if (uciIn.Contains("fen"))   //positon fen <fenstring>
            {
                fenString = uciIn.Remove(0, 13);
                if (uciIn.Contains("moves"))
                {
                    fenString = fenString.Remove(fenString.IndexOf("moves") - 1, fenString.Length - fenString.IndexOf("moves") + 1);
                }
                board = new Board(fenString);
            }
            if (uciIn.Contains("moves"))
            {
                uciIn = uciIn.Remove(0, (uciIn.IndexOf("moves") + 6));
                string[] moves = uciIn.Split(' ');

                for (int i = 0; i < moves.Length; i++)
                {
                    Move move = new((byte)((moves[i][0] - 'a') + (8 - (moves[i][1] - '0')) * 8), (byte)((moves[i][2] - 'a') + (8 - (moves[i][3] - '0')) * 8));

                    if (moves[i].Length == 5)
                    {
                        move.PromPiece = moves[i][4];
                        if (board.IsWhiteToMove)
                        {
                            move.PromPiece = char.ToUpper(move.PromPiece);
                        }
                    }
                    if (board.board[move.Index1].Notation is 1 or 7 && (move.Index1 - move.Index2) % 8 != 0 && board.board[move.Index2] == null)
                    {
                        move.IsCapture = true; //en passent capture
                        move.LastCapture = null;
                    }
                    if (board.board[move.Index2] != null)
                    {
                        move.IsCapture = true;
                        move.LastCapture = board.board[move.Index2];
                    }

                    board.DoMove(move);
                }
            }
            return board;
        }

        private static async Task speedtest()
        {
            long totalTime = 0;
            long totalNodes = 0;
            int instances = 6;
            Console.WriteLine("Starting speedtest with " + instances + " Threads");
            string exePath = AppDomain.CurrentDomain.FriendlyName;
            string args = "speedtest ";

            var processes = new List<Process>();

            for (int i = 0; i < instances; i++)
            {
                Console.WriteLine("Starting Thread " + i);
                var p = new Process();
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = args + i;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.WriteLine(e.Data);
                        string[] strings = e.Data.Split(' ');
                        try
                        {
                            totalTime += Convert.ToInt64(strings[3]);
                            totalNodes += Convert.ToInt64(strings[5]);
                        }
                        catch { }
                    }
                };

                p.Start();
                p.BeginOutputReadLine();
                processes.Add(p);
            }

            foreach (var p in processes)
            {
                p.WaitForExit();
            }
            Console.WriteLine($"Nodes {totalNodes} time {totalTime} nps {Math.Round(totalNodes / (decimal)totalTime * 1000)}");
        }
    }
}