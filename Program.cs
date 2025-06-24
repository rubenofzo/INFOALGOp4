// See https://aka.ms/new-console-template for more information
/*
 specification:
De invoer begint met een regel met drie getallen n ≤
   1.000.000, m ≤ 10.000.000 en s ≤ n. Hierbij is n het aantal
   kamers k1, . . . , kn, m het aantal gangen en s het totale aantal
   zilveren munten in de dungeon.
   Daarna volgen m regels met ieder 2 getallen. Een regel met de getallen (a, b) geeft aan
   dat er een eenrichtingsgang van kamer ka naar kamer kb is (het kan zijn dat er zowel een
   gang van a naar b als een gang van b naar a is).
   Tot slot volgt een regel met s verschillende getallen c1, . . . , cs, waarbij het getal ci
   aangeeft dat er een zilveren munt te vinden is in kamer kci (en in iedere kamer is hoogstens
   ´e´en munt te vinden).
   De ingang van de dungeon is kamer 1; de uitgang kamer n.
   
   Op de uitvoer komt het getal Z, het maximale aantal munten dat Pones op een wan-
   deling van de ingang naar de uitgang kan verzamelen. Daarna volgt een regel met Z
   verschillende getallen X1, . . . XZ , waarbij Xi zodanig is dat er een zilveren munt te vinden
   is in kamer kXi , en dat als i < j er een pad bestaat van kamer KXi naar kamer KXj . Ook
   moet er een pad bestaan van K1 (de ingang) naar KX1 en een pad van KXZ naar Kn (de
   uitgang). Als er geen pad van K1 naar Kn bestaat, geef dan als uitvoer het getal Z = 0
   gevolgd door een lege regel.
   Merk op dat een wandeling niet simpel hoeft te zijn: je kunt een bepaalde kamer meer
   dan eens bezoeken (maar een zilveren munt kun je slechts ´e´en keer meenemen).
   
   Algoritmische Aanwijzingen
   Kijk naar de relaties tussen het al dan niet oppakken van bepaalde munten en andere
   munten. In het voorbeeld (Figuur 1) is het bijvoorbeeld zo dat als je munt 9 pakt je nooit
   munt 6 kan pakken, maar als je munt 2 pakt kun je ook altijd munt 3 pakken en als je
   munt 10 pakt, kun je ook altijd munt 9 pakken.
   Je algoritme moet lineair zijn in de lengte van de invoer.
   De uitvoer is niet noodzakelijkerwijs uniek bepaald. DomJudge accepteert iedere geldige
   oplossing. In het voorbeeld zou onder meer 2 3 4 5 9 10 8 ook geldig zijn.
 */

namespace Dungeon
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;

    class Program
    {
        // Function for graph representation
        static void addEdge(List<List<KeyValuePair<int, int>>> adj, int u, int v, int w)
        {
            adj[u].Add(new KeyValuePair<int, int>(v, w));
        }

        static void displayAdjList(Dictionary<int,List<int>> adj)
        {
            
            foreach (var j in adj.Keys)
            {
                foreach (var i in adj[j])
                    Console.Write("{" + j + ", " + i + "} ");
            }
            Console.WriteLine();
        }

        // adjacency list, n entries for each room. Each entry has a list of possible destinations and a 0 or 1 for a coin
        static (Dictionary<int,List<int>>,Dictionary<int,List<int>>) setupAdj(int n, int m, List<(int from, int to)> corridors)
        {
            Dictionary<int,List<int>> adj = new Dictionary<int,List<int>>();
            Dictionary<int,List<int>> reverseadj = new Dictionary<int,List<int>>();
            List<int>[] edges = new List<int>[n+1];
            List<int>[] revedges = new List<int>[n+1];
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
            for (int i = 1; i < n+1; i++)
            {
                adj[i] = edges[i] ?? new List<int>();
                reverseadj[i] = revedges[i] ?? new List<int>();
            }
            return (adj,reverseadj);
        }

        static void Main(string[] args)
        {
            //parse input as:
            // n = exit and number of rooms
            // m = number of corridors
            // s = number of silver coins
            // corridors = list of corridors (from, to)
            // coinRooms = set of rooms with coins
            var (n, m, s, corridors, coinRooms, hasChoin) = ParseInput();
            var (adj,revadj) = setupAdj(n, m, corridors);
            displayAdjList(adj); //TODO remove
            
            solve(n,m,corridors,coinRooms,adj,revadj);

            
        }

        static (int n, int m, int s, List<(int from, int to)> corridors, HashSet<int> coinRooms, bool[] hasCoin) ParseInput()
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
            bool[] hasChoin = new bool[n];

            HashSet<int> coinRooms = new HashSet<int>(coinLine.Select(int.Parse).ToList());
            return (n, m, s, corridors, coinRooms, hasChoin);
        }

        static void solve(int amountOfRooms, int amountOfCorridors, List<(int from, int to)> corridors, HashSet<int> coinRooms, Dictionary<int, List<int>> adj, Dictionary<int, List<int>> revadj)
        {
            // Solve problem
            // reachability: filter all nodes that are not on a path
            (Dictionary<int, List<int>> nadj, Dictionary<int, List<int>> nrevadj) = bfs(amountOfRooms, corridors, adj, revadj);
            // if end and start not connected, return
            if (!(nadj.ContainsKey(1) && revadj.ContainsKey(amountOfRooms))){
                Console.WriteLine("0");
                return; }

            // Find SCC's (turns problem into a DAG)
            (int[] starttimesG, int[]endtimesG,List<int> ts) = findSCC(amountOfRooms, corridors, nadj, nrevadj);
            (Dictionary<int, List<int>> adjSCC,int[]coinsSCC) = processSCC(corridors,starttimesG,endtimesG,amountOfRooms,amountOfCorridors,coinRooms);
            //dynprog through sccs
            int path = SSSP(ts,adjSCC);
            PrintSolution();
        }

        static int SSSP(List<int> ts,Dictionary<int, List<int>> adjSCC)
        {
            //topological sort //note that its bacckward from the slides, so indice n is the first one on which we edit :)
            foreach (int i in ts)
            {
                Console.WriteLine(i);
            }
            //dyn prog
            //for all incoming edges (u,v)
            //if d(v) > d (u) + w(u,v)
            //then d(v) = d(u) + w(u,v) pred(v) = u
            int sccStart = //This is still 1?
            int sccEnd = //TODO make sure end is kept


            var dp = new Dictionary<int, int>();
            foreach (var scc in adjSCC.Keys)
                dp[scc] = int.MinValue;
            dp[sccStart] = coinsInSCC.ContainsKey(sccStart) ? coinsInSCC[sccStart] : 0;

            foreach (var scc in topo)
            {
                if (!adjSCC.ContainsKey(scc)) continue;
                foreach (var neighbor in adjSCC[scc])
                {
                    int coins = coinsInSCC.ContainsKey(neighbor) ? coinsInSCC[neighbor] : 0;
                    if (dp[scc] + coins > dp[neighbor])
                        dp[neighbor] = dp[scc] + coins;
                }
            }

            int min = dp[sccEnd] == int.MinValue ? 0 : dp[sccEnd];

            return 0;
        }
        static (Dictionary<int, List<int>>,HashSet<int>) processSCC(List<(int from, int to)> corridors, int[] gS, int[] gE, int n, int m,HashSet<int>coinrooms)
        {
            List<List<int>> sccs = formatSCC(gS, gE, n);
            //now turn the identified nodes into actual nodes 
            foreach (List<int> scc in sccs)
            {
                if (scc.Count > 1)
                {
                    int newroom = scc[0];
                    foreach (int room in scc)
                    {
                        corridors = corridors.Select(corridor =>
                            (
                                corridor.from == room ? newroom : corridor.from,
                                corridor.to == room ? newroom : corridor.to
                            )
                        ).ToList();
                    }
                }
            }
            foreach ((int i, int j) in corridors)
            { Console.Write("{" + i + " " + j + "} "); }

            var (newAdj, _) = setupAdj(n, m, corridors);
            Console.WriteLine();
            displayAdjList(newAdj);

            //TODO coinrooms
            

            return newAdj;
        }

        static List<List<int>> formatSCC(int[] gS, int[] gE, int n)
        {
            int i = 0;
            List<List<int>> sccs = new List<List<int>>();

            Console.WriteLine("lenght of input" + gS.Length);
            //zip with indices
            var zipS = gS.Select((value, index) => (index, value)).ToList();
            var sortS = zipS.OrderBy(pair => pair.value).ToList();
            var zipE = gE.Select((value, index) => (index, value)).ToList();
            var sortE = zipE.OrderBy(pair => pair.value).ToList();

            while (i < n)
            {
                i += 1;
                Console.WriteLine("newLoop, i = " + i);
                if (sortS[i].index == sortE[i].index)
                {
                    Console.WriteLine("singleton case:");
                    //not a loop, singleton
                    //add to SCC
                    Console.WriteLine("adding singleton:" + sortS[i].index);
                    sccs.Add(new List<int> { sortS[i].index });
                }
                else
                {
                    Console.WriteLine("loop case:");
                    //is a loop, find out how big it is
                    int startLoop = i;
                    List<int> loopNodes = new List<int>();
                    while (sortS[startLoop].index != sortE[i].index)
                    {

                        loopNodes.Add(sortS[i].index);
                        i += 1;
                        Console.WriteLine("loopin:" + i);
                    }
                    loopNodes.Add(sortS[i].index);
                    //now i points to the mid point of the loop, with sortS[i].index being the final node added to the loop.
                    sccs.Add(loopNodes);
                }
            }
            Console.WriteLine("scc:" + sccs.Count);
            foreach (List<int> scc in sccs)
            {
                foreach (int j in scc)
                { Console.Write(j + " "); }
                Console.WriteLine("\n");
            }
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
                Console.WriteLine("key" + i);
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
            Console.WriteLine("finished dfs 1");
            foreach (int i in starttimes)
            { Console.WriteLine(i); }
            Console.WriteLine("end start, begin end");
            foreach (int i in endtimes)
            { Console.WriteLine(i); }
            Console.WriteLine("finished dfs 2");
            foreach (int i in revstarttimes)
            { Console.WriteLine(i); }
            Console.WriteLine("end start, begin end");
            foreach (int i in revendtimes)
            { Console.WriteLine(i); }
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
                Console.WriteLine("room" + room);

                canReachFromStart[room] = true;
                if (adj.ContainsKey(room) && adj[room] != null && adj[room].Count > 0)
                {

                    foreach (int destination in adj[room])
                    {
                        Console.WriteLine("adj" + destination);
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

            //testing:
            Console.WriteLine("finished bfs 1");
            foreach (bool b in canReachFromStart)
            { Console.WriteLine(b); }
            Console.WriteLine("finished bfs 2");
            foreach (bool b in canReachEnd)
            { Console.WriteLine(b); }
            Console.WriteLine("relevant nodes:");
            foreach (bool b in relevant)
            { Console.WriteLine(b); }
            return (adj, revadj); //TODO check if these are actually filtered, answer is no
            //

        }

        static void PrintSolution()
        {
            // Print solution
            Console.WriteLine("end program");
        }

        // static void testSolution()
        // {

        //     for (int i = 0; i < 10; i++)
        //     {
        //         string filepath = $"{ i}.in";
        //         string[] lines = File.ReadAllLines(filepath);
        //         var (n, m, s, edges, coins) = ParseInput();
        //         foreach l in lines)
        //         {
        //             Console.WriteLine(l);
        //         }
                
        //     }

        //     string[] lines = File.ReadAllLines(filepath);

        //     var (n, m, s, edges, coins, hasCoin) = Parser.ParseInput(lines);
        //     var result = solve(n, m, s, edges, coins, hasCoin);
        //     Assert.Equal(4, n);
        //     Assert.Equal(4, m);
        //     Assert.Equal(2, s);
        //     Assert.Equal((1, 2), edges[0]);
        //     Assert.Contains(4, coins);

        //     Console.WriteLine("start test");
        // }
    }
 
}