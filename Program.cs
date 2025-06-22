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
    
    class Program
    {
        // Function for graph representation
        static void addEdge(List<List<KeyValuePair<int, int>>> adj, int u, int v, int w)
        {
            adj[u].Add(new KeyValuePair<int, int>(v, w));
        }

        static void displayAdjList(List<List<KeyValuePair<int, int>>> adj)
        {
            for (int i = 0; i < adj.Count; i++)
            {
                Console.Write(i + ": "); 
                foreach (var j in adj[i])
                {
                    Console.Write("{" + j.Key + ", " + j.Value + "} "); 
                }
                Console.WriteLine();
            }
        }

        static List<List<KeyValuePair<int, int>>> setupAdj(int n, int m, List<(int from, int to)> corridors,HashSet<int> coinRooms)
        {
            List<List<KeyValuePair<int, int>>> adj = new List<List<KeyValuePair<int, int>>>();
            for (int i = 0; i < n; i++)
            {
                adj.Add(new List<KeyValuePair<int, int>>());
            }
            for (int i = 0; i < m; i++)
            {
                (int,int) edge = corridors[i];
                int choin = coinRooms.Contains(edge.Item1) ? 1 : 0; // 1 if coin in room 1, 0 otherwise
                addEdge(adj, edge.Item1, edge.Item2,choin);
            }
            return adj;
        }
        
        static void Main(string[] args)
        {
            var (n, m, s, corridors, coinRooms,hasChoin) = ParseInput();
           
            //entrance in room corridors[0] exit in room corridors[n]
            // var entrance = ? this is room with number 1
            //var exit = ? this is room with numberk
            //Solve(entrance,exit,corridors,coinRooms);
            List<List<KeyValuePair<int, int>>> adj = setupAdj(n,m,corridors,coinRooms);
            Console.WriteLine("Adjacency List Representation:");
            displayAdjList(adj);
            
            PrintSolution();
        }   

        static (int n, int m, int s, List<(int from, int to)> corridors, HashSet<int> coinRooms, bool[]) ParseInput()
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

        static void Solve((int from, int to) entrance, (int from, int to) exit, List<(int from, int to)> corridors, HashSet<int> coinRooms)
        {
            // Solve problem
            bfs(entrance,exit,corridors,coinRooms);
        }

        static void bfs((int from, int to) entrance, (int from, int to) exit, List<(int from, int to)> corridors, HashSet<int> coinRooms)
        {
            //bfs:
            // Dictionary<int, int> parent = new Dictionary<int, int>();
            // Queue<int> queue = new Queue<int>();
            // queue.Enqueue(entrance.from);
            // parent[entrance.from] = -1;
            //
            // while (queue.Count > 0 && !parent.ContainsKey(exit.to))
            // {
            //     int state = queue.Dequeue();
            //     if (graph.ContainsKey(state))
            //     {
            //         foreach (var child in graph[state])
            //         {
            //             if (!parent.ContainsKey(child) && capacities.GetValueOrDefault((state, child), 0) > 0)
            //             {
            //                 parent[child] = state;
            //                 queue.Enqueue(child);
            //             }
            //         }
            //     }
            // }
            // Print shortest path
            
        }

        static void PrintSolution()
        {
            // Print solution
            Console.WriteLine("end program");
        }
    }
 
}