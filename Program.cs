

namespace Dungeon
{
    using System;
    using System.Collections.Generic;

    class Program
    {
        static void displayAdjList(Dictionary<int,List<int>> adj)
        {
            // foreach (var j in adj.Keys)
            // {
            //     foreach (var i in adj[j])
            //         Console.Write("{" + j + ", " + i + "} ");
            // }
            // Console.WriteLine();
        }
        static void displayTable(Dictionary<int,(int,int)> table)
        {
            // foreach (int j in table.Keys)
            // {
            //     (int a, int b) = table[j];
            //     Console.Write(j + ": {" + a + ", " + b + "} ");
            // }
            // Console.WriteLine();
        }
        
        static (int n, int m, int s, List<(int from, int to)> corridors, int[] coinRooms) ParseInput()
        {
            // Read first line with n, m, s
            string[] firstLine = Console.ReadLine().Split(' ');
            int n = int.Parse(firstLine[0]); // number of rooms
            int m = int.Parse(firstLine[1]); // number of corridors
            int s = int.Parse(firstLine[2]); // number of silver coins
            // Read m lines of corridors
            var corridors = new List<(int from, int to)>();

            for (int i = 0; i < m; i++)
            {
                string[] corridor = Console.ReadLine().Split(' ');
                int from = int.Parse(corridor[0]);
                int to = int.Parse(corridor[1]);
                corridors.Add((from, to));
            }
            // Read last line with coin locations
            string[] coinLine = Console.ReadLine().Split(' ');
            List<int> parsedCoinLine = coinLine.Select(int.Parse).ToList();
            int[] coinRooms = new int[n + 1];

            foreach (int i in parsedCoinLine)
            {
                coinRooms[i] = 1;
            }


            return (n, m, s, corridors, coinRooms);
        }

        // adjacency list, n entries for each room. Each entry has a list of possible destinations and a 0 or 1 for a coin
        static (Dictionary<int, List<int>>, Dictionary<int, List<int>>) setupAdj(int n, int m, List<(int from, int to)> corridors)
        {
            Dictionary<int, List<int>> adj = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> reverseadj = new Dictionary<int, List<int>>();
            List<int>[] edges = new List<int>[n + 1];
            List<int>[] revedges = new List<int>[n + 1];
            //adds all destinations to the start indices gotten from the corridor list
            for (int i = 0; i < m; i++)
            {
                (int s, int e) = corridors[i];
                if (s != e)
                {
                    if (edges[s] == null) edges[s] = new List<int>();
                    edges[s].Add(e);
                    if (revedges[e] == null) revedges[e] = new List<int>();
                    revedges[e].Add(s);
                }
            }
            for (int i = 1; i < n + 1; i++)
            {
                adj[i] = edges[i] ?? new List<int>();
                reverseadj[i] = revedges[i] ?? new List<int>();
            }
            return (adj, reverseadj);
        }

        static void Main(string[] args)
        {
            //parse input as:
            // n = exit and number of rooms
            // m = number of corridors
            // s = number of silver coins
            // corridors = list of corridors (from, to)
            // coinRooms = set of rooms with coins
            var (n, m, s, corridors, coinRooms) = ParseInput();
            var (adj,revadj) = setupAdj(n, m, corridors);
            displayAdjList(adj);
            solve(n,m,corridors,coinRooms,adj,revadj);
            
        }



        static void solve(int amountOfRooms, int amountOfCorridors, List<(int from, int to)> corridors, int[] coinRooms, Dictionary<int, List<int>> adj, Dictionary<int, List<int>> revadj)
        {
            // Solve problem
            // reachability: filter all nodes that are not on a path
            (Dictionary<int, List<int>> nadj, Dictionary<int, List<int>> nrevadj) = bfs(amountOfRooms, corridors, adj, revadj);
            // if end and start not connected, return
            if (!(nadj.ContainsKey(1) && nrevadj.ContainsKey(amountOfRooms))){
               Console.WriteLine("0");
               Console.WriteLine();
               return; }

            // Find SCC's (turns problem into a DAG)
            (int[] starttimesG, int[]endtimesG,List<int> ts) = findSCC(amountOfRooms, corridors, nadj, nrevadj);
            (Dictionary<int, List<int>> adjSCC,Dictionary<int, List<int>> revadjSCC,int[]coinrooms,var sccToRoom) = processSCC(corridors,starttimesG,endtimesG,amountOfRooms,amountOfCorridors,coinRooms);
            //dynprog through sccs
            Dictionary<int, (int, int)>  distanceTable = SSSP(ts,adjSCC,revadjSCC,amountOfRooms,coinrooms);
            PrintSolution(distanceTable,amountOfRooms,sccToRoom);
        }
        static void PrintSolution(Dictionary<int, (int, int)> table, int n, Dictionary<int, List<int>> sccToRoom)
        {
            //Console.WriteLine("solution");
            displayAdjList(sccToRoom);
            if (!table.ContainsKey(n) || table[n].Item1 == int.MinValue)
            {
                Console.WriteLine("0");
                Console.WriteLine();
                return;
            }
            (int totalCoin, int pred) = table[n];
            Console.WriteLine(totalCoin);
            List<int> sccPath = new List<int>(n);
            int current = n;
            while (current != 0 && table.ContainsKey(current))
            {
                //we store the predecesor already
                (int coin, int predecessor) = table[current];
                //we get the actual path from the SCC reverse table
               //List<int> sccPath = sccToRoom[current];
                if (coin > 0 && sccToRoom.ContainsKey(current)) // Only add SCCs with coins
                {
                    sccPath.Add(current);
                }
                current = predecessor;
                if (current == 0) { break; }
            }
            sccPath.Reverse();
            // Print solution

            List<int> path = new List<int>();
            foreach (int sccRep in sccPath)
            {
                if (sccToRoom.ContainsKey(sccRep))
                {
                    // Add all original rooms with coins from this SCC
                    // Sort them to ensure consistent output
                    var roomsInSCC = sccToRoom[sccRep].OrderBy(x => x).ToList();
                    path.AddRange(roomsInSCC);
                }
            }

            if (sccPath.Count > 0)
            {
                Console.WriteLine(string.Join(" ", path));
            }
            else
            {
                Console.WriteLine();
            }
            // everyting works but i now need to get the actual path instead of the SCC placeholder paths.
        }

        static Dictionary<int, (int, int)> SSSP(List<int> ts, Dictionary<int, List<int>> adjSCC, Dictionary<int, List<int>> revadjSCC, int n, int[] coinrooms)
        {
            
            ts.Reverse(); //topological sort
            // foreach (int i in ts)
            // {
            //     Console.WriteLine(i);
            // }
            //dyn prog
            //for all incoming edges (u,v)
            //if d(v) > d (u) + w(u,v)
            //then d(v) = d(u) + w(u,v) pred(v) = u

            //we keep a dictionary per node of the cheapest distance to all neighbours
            Dictionary<int, (int, int)> distanceTable = new Dictionary<int, (int, int)>();
            //an entry is amount of coins + predecesor, for one this is 0 to signal the end
            //distanceTable[1] = (coinrooms[1], 0);
            foreach (int room in ts)
            {
                var entry = (int.MinValue, -1);
                distanceTable[room] = entry;
            }
            if (distanceTable.ContainsKey(1))
            {
                distanceTable[1] = (coinrooms[1], 0); // 0 is start node
            }
            foreach (int room in ts)
            {
                if (!distanceTable.ContainsKey(room)) continue;

                (int currentCoins, int pred) = distanceTable[room];

                if (currentCoins == int.MinValue && room != 1) continue; //unreachable

                // Update through all outgoing edges, the incoming ones for the neighbours
                if (adjSCC.ContainsKey(room))
                {
                    foreach (int neighbor in adjSCC[room])
                    {
                        if (!distanceTable.ContainsKey(neighbor)) continue;
                        int newCoinCount = currentCoins + coinrooms[neighbor];
                        (int neighborCoins, int neighborPred) = distanceTable[neighbor];

                        // Update if we found a better path
                        if (newCoinCount > neighborCoins)
                        {
                            distanceTable[neighbor] = (newCoinCount, room);
                        }
                    }
                }
            }
            displayAdjList(adjSCC);
            displayTable(distanceTable);

            return distanceTable;
            }

        static (Dictionary<int, List<int>>,Dictionary<int, List<int>>, int[],Dictionary<int, List<int>>) processSCC(List<(int from, int to)> corridors, int[] gS, int[] gE, int n, int m, int[] coinrooms)
        {
            List<List<int>> sccs = formatSCC(gS, gE, n);
            Dictionary<int, int> roomToSCC = new Dictionary<int, int>();
            Dictionary<int, List<int>> SCCtorooms = new Dictionary<int, List<int>>(); //for printing the cycle later
            int[] newCoinrooms = new int[n + 1];
            
            foreach (List<int> scc in sccs)
            {
                
                //ensures room 1 stays room 1 and the final room remains the final one
                int representative;
                if (scc.Contains(1))
                    representative = 1;
                else if (scc.Contains(n))
                    representative = n;
                else
                    representative = scc.Min();

                SCCtorooms[representative] = new List<int>();
                
                // Sum all coins in this SCC
                int totalCoins = scc.Sum(room => coinrooms[room]);
                newCoinrooms[representative] = totalCoins;

                // Map all rooms in SCC to representative
                foreach (int room in scc)
                {
                    roomToSCC[room] = representative;
                    if (coinrooms[room] == 1 ){
                        SCCtorooms[representative].Add(room);
                    } }
            }
            
            // Update corridors
            var newCorridors = corridors
                .Select(corridor => (
                    roomToSCC.GetValueOrDefault(corridor.from, corridor.from),
                    roomToSCC.GetValueOrDefault(corridor.to, corridor.to)
                ))
                .Where(corridor => corridor.Item1 != corridor.Item2).Distinct() // Remove duplicates
                .ToList();
            
            // Build new adjacency lists
            var (newAdj, newRevAdj) = setupAdj(n, newCorridors.Count, newCorridors);
            
            return (newAdj, newRevAdj, newCoinrooms, SCCtorooms);
        }

        static List<List<int>> formatSCC(int[] gS, int[] gE, int n)
        {
            
            int i = 0;
            List<List<int>> sccs = new List<List<int>>();

            //Console.WriteLine("lenght of input" + gS.Length);
            //zip with indices
            var zipS = gS.Select((value, index) => (index, value)).ToList();
            var sortS = zipS.OrderBy(pair => pair.value).ToList();
            var zipE = gE.Select((value, index) => (index, value)).ToList();
            var sortE = zipE.OrderBy(pair => pair.value).ToList();

            while (i < n)
            {
                i += 1;
                //Console.WriteLine("newLoop, i = " + i);
                if (sortS[i].index == sortE[i].index)
                {
                    //Console.WriteLine("singleton case:");
                    //not a loop, singleton
                    //add to SCC
                    //Console.WriteLine("adding singleton:" + sortS[i].index);
                    sccs.Add(new List<int> { sortS[i].index });
                }
                else
                {
                    //Console.WriteLine("loop case:");
                    //is a loop, find out how big it is
                    int startLoop = i;
                    List<int> loopNodes = new List<int>();
                    while (sortS[startLoop].index != sortE[i].index)
                    {

                        loopNodes.Add(sortS[i].index);
                        i += 1;
                        //Console.WriteLine("loopin:" + i);
                    }
                    loopNodes.Add(sortS[i].index);
                    //now i points to the mid point of the loop, with sortS[i].index being the final node added to the loop.
                    sccs.Add(loopNodes);
                }
            }
            // Console.WriteLine("scc:" + sccs.Count);
            // foreach (List<int> scc in sccs)
            // {
            //     foreach (int j in scc)
            //     { Console.Write(j + " "); }
            //     Console.WriteLine("\n");
            // }
            return sccs;
        }

        static (int[], int[],List<int>) findSCC(int n, List<(int from, int to)> corridors, Dictionary<int, List<int>> adj, Dictionary<int, List<int>> revadj)
        {
            bool[] discovered = new bool[n + 1];
            int[] starttimes = new int[n + 1];
            int[] endtimes = new int[n + 1];
            int entrance = 1; // room 1 is the entrance
            int exit = n; // room n is the exit
            Stack<int> sortedEndtimes = new Stack<int>();

            //we do the topological sort here as were already doing dfs anyway
            List<int> topologicalSort = new List<int>();
            //do dfs on G to get f[u]
            int time = 0;
            for (int i = 1; i < n + 1; i++)
            {
                //Console.WriteLine("key" + i);
                if (!discovered[i])
                {
                    (time, starttimes, endtimes, sortedEndtimes, discovered,topologicalSort) = singledfs(adj, i, time, discovered, starttimes, endtimes, sortedEndtimes,topologicalSort);
                }
            }
            //now what?
            time = 0;
            bool[] revdiscovered = new bool[n + 1];
            int[] revstarttimes = new int[n + 1];
            int[] revendtimes = new int[n + 1];
            while (sortedEndtimes.Count() > 0)
            {
                int room = sortedEndtimes.Pop();
                if (!(revdiscovered[room] == true))
                {
                    (time, revstarttimes, revendtimes, _, revdiscovered,_) = singledfs(revadj, room, time, revdiscovered, revstarttimes, revendtimes, new Stack<int>(),new List<int>());
                }
            }
            


            //testing:
            // Console.WriteLine("finished dfs 1");
            // foreach (int i in starttimes)
            // { Console.WriteLine(i); }
            // Console.WriteLine("end start, begin end");
            // foreach (int i in endtimes)
            // { Console.WriteLine(i); }
            // Console.WriteLine("finished dfs 2");
            // foreach (int i in revstarttimes)
            // { Console.WriteLine(i); }
            // Console.WriteLine("end start, begin end");
            // foreach (int i in revendtimes)
            // { Console.WriteLine(i); }
            //
            return (revstarttimes, revendtimes,topologicalSort);
        }

        //note to self, ref exists, sadly i found out too late about this so now the code is ugly :(
        static (int, int[], int[], Stack<int>, bool[], List<int>) singledfs(Dictionary<int, List<int>> adj, int room, int time, bool[] discovered, int[] starttimes, int[] endtimes, Stack<int> sortedEndtimes, List<int> ts)
        {
            time += 1;
            discovered[room] = true;
            starttimes[room] = time;

            if (adj.ContainsKey(room) && adj[room].Count > 0)
            {
                foreach (int destination in adj[room])
                {
                    if (!(discovered[destination] == true))
                    {
                        //evt predecesor recorden
                        (time, starttimes, endtimes, sortedEndtimes, discovered, ts) = singledfs(adj, destination, time, discovered, starttimes, endtimes, sortedEndtimes,ts);
                    }
                }
            }
            time += 1;
            endtimes[room] = time;
            sortedEndtimes.Push(room);
            ts.Add(room);
            return (time, starttimes, endtimes, sortedEndtimes, discovered, ts);
        }

        static (Dictionary<int, List<int>>, Dictionary<int, List<int>>) bfs(int n, List<(int from, int to)> corridors, Dictionary<int, List<int>> adj, Dictionary<int, List<int>> revadj)
        {
            bool[] canReachFromStart = new bool[n + 1];
            bool[] canReachEnd = new bool[n + 1];
            int entrance = 1; // room 1 is the entrance
            int exit = n; // room n is the exit

            // BFS from start
            //bool[] seen = new bool[n];
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(entrance);
            //seen[0] = true;
            canReachFromStart[1] = true;
            while (queue.Count > 0)
            {
                int room = queue.Dequeue();
                //Console.WriteLine("room" + room);

                canReachFromStart[room] = true;
                if (adj.ContainsKey(room) && adj[room] != null && adj[room].Count > 0)
                {

                    foreach (int destination in adj[room])
                    {
                        //Console.WriteLine("adj" + destination);
                        if (!(canReachFromStart[destination] == true))
                        {
                            queue.Enqueue(destination);
                        }
                    }
                }
            }


            // BFS from end
            queue.Clear();
            queue.Enqueue(exit);
            //seen[0] = true;
            canReachEnd[n] = true;
            while (queue.Count > 0)
            {
                int room = queue.Dequeue();
                if (revadj.ContainsKey(room) && revadj[room] != null && revadj[room].Count > 0)
                {
                    foreach (int destination in revadj[room])
                    {
                        if (!(canReachEnd[destination] == true))
                        {
                            canReachEnd[destination] = true;
                            queue.Enqueue(destination);
                        }
                    }
                }
            }

            bool[] relevant = new bool[n + 1];  // n is the array length
            for (int i = 0; i < n + 1; i++)
            {
                relevant[i] = canReachFromStart[i] && canReachEnd[i];
                if (!relevant[i])
                {
                    if (adj.ContainsKey(i))
                    {
                        adj.Remove(i);
                    }
                    if (revadj.ContainsKey(i))
                    {
                        revadj.Remove(i);
                    } //TODO also remove the values
                }
            }
            foreach (int i in adj.Keys.ToList())
            {
                if (adj[i] != null && adj[i].Count > 0)
                {
                    adj[i] = adj[i].Where(target => relevant[target]).ToList();
                }
            }

            foreach (int i in revadj.Keys.ToList())
            {
                if (revadj[i] != null && revadj[i].Count > 0)
                {
                    revadj[i] = revadj[i].Where(source => relevant[source]).ToList();
                }
            }

            // //testing:
            // Console.WriteLine("finished bfs 1");
            // foreach (bool b in canReachFromStart)
            // { Console.WriteLine(b); }
            // Console.WriteLine("finished bfs 2");
            // foreach (bool b in canReachEnd)
            // { Console.WriteLine(b); }
            // Console.WriteLine("relevant nodes:");
            // foreach (bool b in relevant)
            // { Console.WriteLine(b); }
            return (adj, revadj); //TODO check if these are actually filtered, answer is no
            // //

        }
    }
 
}